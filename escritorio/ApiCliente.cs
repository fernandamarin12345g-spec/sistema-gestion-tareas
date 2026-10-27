using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Escritorio;

/// <summary>Error con un mensaje listo para mostrarle al usuario.</summary>
public class ApiException : Exception
{
    public ApiException(string mensaje) : base(mensaje) { }
}

/// <summary>La API respondió 401: el token venció. Hay que volver a iniciar sesión.</summary>
public class SesionExpiradaException : ApiException
{
    public SesionExpiradaException() : base("Tu sesión expiró. Vuelve a iniciar sesión.") { }
}

/// <summary>
/// Cliente HTTP que consume la MISMA API que usan el dashboard web y la app móvil.
/// </summary>
public class ApiCliente
{
    // Dirección de la API. Se puede cambiar con la variable de entorno GESTION_TAREAS_API
    // (por ejemplo, cuando el backend esté en un servidor remoto).
    private static readonly string UrlBase =
        Environment.GetEnvironmentVariable("GESTION_TAREAS_API") ?? "http://localhost:5293/api/";

    private readonly HttpClient _http = new()
    {
        BaseAddress = new Uri(UrlBase.EndsWith('/') ? UrlBase : UrlBase + "/"),
        Timeout = TimeSpan.FromSeconds(8)
    };

    /// <summary>Sesión actual. Mientras sea null, la API rechaza las peticiones (401).</summary>
    public Sesion? Sesion { get; private set; }

    // ---------------------------------------------------------------------
    // Utilidades internas
    // ---------------------------------------------------------------------

    /// <summary>Envía la petición con el token y convierte cualquier falla en un ApiException con un mensaje claro.</summary>
    private async Task<HttpResponseMessage> EnviarAsync(HttpRequestMessage peticion)
    {
        if (Sesion != null)
        {
            peticion.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Sesion.Token);
        }

        HttpResponseMessage respuesta;
        try
        {
            respuesta = await _http.SendAsync(peticion);
        }
        catch (TaskCanceledException)
        {
            throw new ApiException("El servidor tardó demasiado en responder.");
        }
        catch (HttpRequestException)
        {
            throw new ApiException("No se pudo conectar con el servidor. Revisa que el backend esté corriendo.");
        }

        if (respuesta.IsSuccessStatusCode)
        {
            return respuesta;
        }

        // Con sesión iniciada, un 401 significa que el token venció.
        // Sin sesión (en el login), el 401 trae el mensaje "Correo o contraseña incorrectos."
        if (respuesta.StatusCode == HttpStatusCode.Unauthorized && Sesion != null)
        {
            throw new SesionExpiradaException();
        }

        string porDefecto = respuesta.StatusCode == HttpStatusCode.Forbidden
            ? "No tienes permiso para realizar esta acción."
            : $"Error del servidor (código {(int)respuesta.StatusCode}).";

        throw new ApiException(await LeerMensajeAsync(respuesta, porDefecto));
    }

    /// <summary>Lee el campo "mensaje" que devuelve la API en los errores (400, 403, 404...).</summary>
    private static async Task<string> LeerMensajeAsync(HttpResponseMessage respuesta, string porDefecto)
    {
        try
        {
            using JsonDocument documento = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
            if (documento.RootElement.ValueKind == JsonValueKind.Object &&
                documento.RootElement.TryGetProperty("mensaje", out JsonElement mensaje) &&
                mensaje.ValueKind == JsonValueKind.String)
            {
                return mensaje.GetString() ?? porDefecto;
            }
        }
        catch (JsonException)
        {
            // La respuesta no era JSON: se usa el mensaje genérico
        }
        return porDefecto;
    }

    private async Task<T> ObtenerAsync<T>(string ruta) where T : new()
    {
        using var peticion = new HttpRequestMessage(HttpMethod.Get, ruta);
        using HttpResponseMessage respuesta = await EnviarAsync(peticion);
        return await respuesta.Content.ReadFromJsonAsync<T>() ?? new T();
    }

    // ---------------------------------------------------------------------
    // Sesión
    // ---------------------------------------------------------------------

    /// <summary>POST /api/Auth/login. Si los datos son correctos, guarda el token en memoria.</summary>
    public async Task<Sesion> IniciarSesionAsync(string correo, string contrasena)
    {
        Sesion = null;

        using var peticion = new HttpRequestMessage(HttpMethod.Post, "Auth/login")
        {
            Content = JsonContent.Create(new { correo, contrasena })
        };
        using HttpResponseMessage respuesta = await EnviarAsync(peticion);

        Sesion? nueva = await respuesta.Content.ReadFromJsonAsync<Sesion>();
        if (nueva == null || string.IsNullOrEmpty(nueva.Token))
        {
            throw new ApiException("La respuesta del servidor no es válida.");
        }

        Sesion = nueva;
        return nueva;
    }

    /// <summary>Olvida el token. La próxima petición tendrá que iniciar sesión otra vez.</summary>
    public void CerrarSesion()
    {
        Sesion = null;
    }

    // ---------------------------------------------------------------------
    // Tareas
    // ---------------------------------------------------------------------

    /// <summary>GET /api/Tareas con los filtros opcionales que ya tiene la API.</summary>
    public Task<List<Tarea>> ObtenerTareasAsync(int? idEstado, int? idProyecto, int? idUsuario, bool soloAtrasadas)
    {
        var filtros = new List<string>();
        if (idEstado.HasValue) filtros.Add($"id_estado={idEstado.Value}");
        if (idProyecto.HasValue) filtros.Add($"id_proyecto={idProyecto.Value}");
        if (idUsuario.HasValue) filtros.Add($"id_usuario={idUsuario.Value}");
        if (soloAtrasadas) filtros.Add("atrasadas=true");

        string ruta = filtros.Count > 0 ? "Tareas?" + string.Join("&", filtros) : "Tareas";
        return ObtenerAsync<List<Tarea>>(ruta);
    }

    /// <summary>POST /api/Tareas (solo Administrador y Líder de Proyecto)</summary>
    public async Task CrearTareaAsync(NuevaTarea tarea)
    {
        using var peticion = new HttpRequestMessage(HttpMethod.Post, "Tareas")
        {
            Content = JsonContent.Create(tarea)
        };
        using HttpResponseMessage respuesta = await EnviarAsync(peticion);
    }

    /// <summary>PATCH /api/Tareas/{id}/estado. Un Desarrollador puede usarlo con sus propias tareas.</summary>
    public async Task CambiarEstadoAsync(int idTarea, int idEstadoNuevo)
    {
        using var peticion = new HttpRequestMessage(HttpMethod.Patch, $"Tareas/{idTarea}/estado")
        {
            Content = JsonContent.Create(new { id_estado = idEstadoNuevo })
        };
        using HttpResponseMessage respuesta = await EnviarAsync(peticion);
    }

    /// <summary>DELETE /api/Tareas/{id} (solo Administrador)</summary>
    public async Task EliminarTareaAsync(int idTarea)
    {
        using var peticion = new HttpRequestMessage(HttpMethod.Delete, $"Tareas/{idTarea}");
        using HttpResponseMessage respuesta = await EnviarAsync(peticion);
    }

    // ---------------------------------------------------------------------
    // Catálogos
    // ---------------------------------------------------------------------

    public Task<List<Estado>> ObtenerEstadosAsync() => ObtenerAsync<List<Estado>>("Estados");

    public Task<List<Proyecto>> ObtenerProyectosAsync() => ObtenerAsync<List<Proyecto>>("Proyectos");

    public Task<List<Usuario>> ObtenerUsuariosAsync() => ObtenerAsync<List<Usuario>>("Usuarios");
}
