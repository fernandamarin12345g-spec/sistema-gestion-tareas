using System.Globalization;
using System.Text.Json.Serialization;

namespace Escritorio;

// Clases que representan el JSON que devuelve la API.
// [JsonPropertyName] indica el nombre exacto de cada campo en el backend.

/// <summary>Una tarea tal como la devuelve GET /api/Tareas.</summary>
public class Tarea
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("titulo")] public string Titulo { get; set; } = "";
    [JsonPropertyName("descripcion")] public string? Descripcion { get; set; }
    [JsonPropertyName("id_usuario")] public int? IdUsuario { get; set; }
    [JsonPropertyName("usuario_asignado")] public string Responsable { get; set; } = "Sin asignar";
    [JsonPropertyName("id_estado")] public int? IdEstado { get; set; }
    [JsonPropertyName("estado")] public string Estado { get; set; } = "Sin estado";
    [JsonPropertyName("es_final")] public bool EsFinal { get; set; }
    [JsonPropertyName("id_proyecto")] public int? IdProyecto { get; set; }
    [JsonPropertyName("proyecto")] public string Proyecto { get; set; } = "Sin proyecto";
    [JsonPropertyName("prioridad")] public string Prioridad { get; set; } = "Media";
    [JsonPropertyName("fecha_creacion")] public string? FechaCreacion { get; set; }
    [JsonPropertyName("fecha_limite")] public string? FechaLimite { get; set; }
    [JsonPropertyName("fecha_finalizacion")] public string? FechaFinalizacion { get; set; }
    [JsonPropertyName("fecha_actualizacion")] public string? FechaActualizacion { get; set; }
    [JsonPropertyName("atrasada")] public bool Atrasada { get; set; }
}

/// <summary>Datos para crear una tarea (POST /api/Tareas).</summary>
public class NuevaTarea
{
    [JsonPropertyName("titulo")] public string Titulo { get; set; } = "";
    [JsonPropertyName("descripcion")] public string? Descripcion { get; set; }
    [JsonPropertyName("IdUsuarioResponsable")] public int? IdUsuarioResponsable { get; set; }
    [JsonPropertyName("id_estado")] public int IdEstado { get; set; }
    [JsonPropertyName("id_proyecto")] public int? IdProyecto { get; set; }
    [JsonPropertyName("prioridad")] public string Prioridad { get; set; } = "Media";
    [JsonPropertyName("fecha_limite")] public string? FechaLimite { get; set; }
}

public class Estado
{
    [JsonPropertyName("id_estado")] public int Id { get; set; }
    [JsonPropertyName("nombre_estado")] public string Nombre { get; set; } = "";
    [JsonPropertyName("es_final")] public bool EsFinal { get; set; }

    public override string ToString() => Nombre; // Lo que se ve en las listas desplegables
}

public class Proyecto
{
    [JsonPropertyName("id_proyecto")] public int Id { get; set; }
    [JsonPropertyName("nombre")] public string Nombre { get; set; } = "";
    [JsonPropertyName("descripcion")] public string? Descripcion { get; set; }
    [JsonPropertyName("estado")] public string Estado { get; set; } = "Activo";
    [JsonPropertyName("total_tareas")] public int TotalTareas { get; set; }
    [JsonPropertyName("tareas_abiertas")] public int TareasAbiertas { get; set; }

    public override string ToString() => Nombre;
}

public class Usuario
{
    [JsonPropertyName("id_usuario")] public int Id { get; set; }
    [JsonPropertyName("nombre")] public string Nombre { get; set; } = "";
    [JsonPropertyName("correo")] public string Correo { get; set; } = "";
    [JsonPropertyName("id_rol")] public int? IdRol { get; set; }
    [JsonPropertyName("nombre_rol")] public string Rol { get; set; } = "Sin rol";
    [JsonPropertyName("tareas")] public List<TareaResumen> Tareas { get; set; } = new();

    public override string ToString() => Nombre;
}

/// <summary>Tarea asignada a un usuario (viene dentro de GET /api/Usuarios).</summary>
public class TareaResumen
{
    [JsonPropertyName("id_tarea")] public int Id { get; set; }
    [JsonPropertyName("titulo")] public string Titulo { get; set; } = "";
    [JsonPropertyName("estado")] public string Estado { get; set; } = "";
}

public class Rol
{
    [JsonPropertyName("id_rol")] public int Id { get; set; }
    [JsonPropertyName("nombre_rol")] public string Nombre { get; set; } = "";
    [JsonPropertyName("is_admin")] public bool EsAdmin { get; set; }
    [JsonPropertyName("gestiona_tareas")] public bool GestionaTareas { get; set; }

    public override string ToString() => Nombre;
}

/// <summary>Respuesta de POST /api/Auth/login: el token JWT y los datos del usuario.</summary>
public class Sesion
{
    [JsonPropertyName("token")] public string Token { get; set; } = "";
    [JsonPropertyName("expira")] public DateTime Expira { get; set; }
    [JsonPropertyName("usuario")] public UsuarioSesion Usuario { get; set; } = new();
}

public class UsuarioSesion
{
    [JsonPropertyName("id_usuario")] public int IdUsuario { get; set; }
    [JsonPropertyName("nombre")] public string Nombre { get; set; } = "";
    [JsonPropertyName("correo")] public string Correo { get; set; } = "";
    [JsonPropertyName("rol")] public string Rol { get; set; } = "";
    [JsonPropertyName("es_admin")] public bool EsAdmin { get; set; }
    [JsonPropertyName("gestiona_tareas")] public bool GestionaTareas { get; set; }

    /// <summary>Puede crear y editar tareas: Administrador o un rol que gestiona tareas (Líder de Proyecto).</summary>
    [JsonIgnore] public bool PuedeGestionar => EsAdmin || GestionaTareas;
}

/// <summary>Una opción de lista desplegable con un id que puede ser null ("Todos", "Sin asignar").</summary>
public class Opcion
{
    public int? Id { get; }
    public string Texto { get; }

    public Opcion(int? id, string texto)
    {
        Id = id;
        Texto = texto;
    }

    public override string ToString() => Texto;
}

public static class Formato
{
    private static readonly string[] FormatosApi = { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd" };

    /// <summary>Convierte "2026-10-26" en "26/10/2026" y "2026-10-26 17:40:00" en "26/10/2026 17:40".</summary>
    public static string Fecha(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return "—";

        if (DateTime.TryParseExact(texto, FormatosApi, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha))
        {
            return texto.Length > 10 ? fecha.ToString("dd/MM/yyyy HH:mm") : fecha.ToString("dd/MM/yyyy");
        }
        return texto;
    }
}
