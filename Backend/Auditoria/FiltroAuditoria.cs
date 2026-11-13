using Backend.Seguridad;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Backend.Auditoria
{
    // Filtro que se ejecuta alrededor de TODAS las acciones de los controladores.
    // Si la petición cambia datos (POST, PUT, PATCH o DELETE) y la API respondió bien (2xx),
    // guarda en Auditoria_Log quién la hizo, qué hizo, en qué módulo y sobre qué registro.
    // Así no hay que escribir código de auditoría en cada controlador.
    public class FiltroAuditoria : IAsyncActionFilter
    {
        // Campos del cuerpo de la petición que sirven para describir el registro
        private static readonly string[] CamposNombre = { "titulo", "nombre", "nombre_estado", "nombre_rol" };

        private readonly ServicioAuditoria _auditoria;

        public FiltroAuditoria(ServicioAuditoria auditoria)
        {
            _auditoria = auditoria;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext contexto, ActionExecutionDelegate siguiente)
        {
            string metodo = contexto.HttpContext.Request.Method;
            string modulo = contexto.RouteData.Values["controller"]?.ToString() ?? string.Empty;

            // Las consultas (GET) no cambian datos. El inicio de sesión se audita en el AuthController.
            bool cambiaDatos = HttpMethods.IsPost(metodo) || HttpMethods.IsPut(metodo)
                               || HttpMethods.IsPatch(metodo) || HttpMethods.IsDelete(metodo);
            if (!cambiaDatos || modulo == "Auth" || modulo == "Auditoria")
            {
                await siguiente();
                return;
            }

            int? idRegistro = LeerIdDeLaRuta(contexto);

            // El nombre se lee ANTES de ejecutar la acción: después de eliminar ya no existe,
            // y al editar sirve para ver el nombre anterior.
            string? nombreAntes = idRegistro.HasValue ? _auditoria.ObtenerNombre(modulo, idRegistro.Value) : null;

            ActionExecutedContext resultado = await siguiente();

            // Solo se registra lo que de verdad se hizo: nada si hubo error o la API rechazó la petición
            if (resultado.Exception != null && !resultado.ExceptionHandled) return;
            int codigo = (resultado.Result as IStatusCodeActionResult)?.StatusCode ?? StatusCodes.Status200OK;
            if (codigo < 200 || codigo >= 300) return;

            // Al crear, el id nuevo viene en la respuesta: Created(..., new { mensaje, id })
            idRegistro ??= LeerIdDeLaRespuesta(resultado.Result);

            string accion = metodo.ToUpperInvariant() switch
            {
                "POST" => "Crear",
                "PUT" => "Editar",
                "PATCH" => "Cambiar estado",
                "DELETE" => "Eliminar",
                _ => metodo
            };

            string? detalle = ArmarDetalle(accion, contexto, nombreAntes);

            int idUsuario = contexto.HttpContext.User.IdUsuario();
            _auditoria.Registrar(idUsuario > 0 ? idUsuario : null, accion, modulo, idRegistro, detalle);
        }

        // Texto corto que explica el cambio, por ejemplo:
        //   Crear:          "Prueba Docker"
        //   Editar:         "Pendiente" o "Diseñar backend → Diseñar API" si cambió el nombre
        //   Cambiar estado: "Prueba Docker → En progreso"
        //   Eliminar:       "Prueba móvil"
        private string? ArmarDetalle(string accion, ActionExecutingContext contexto, string? nombreAntes)
        {
            string? nombreNuevo = LeerNombreDelCuerpo(contexto);

            switch (accion)
            {
                case "Crear":
                    return nombreNuevo;

                case "Editar":
                    string? nombre = nombreAntes != null && nombreNuevo != null && nombreAntes != nombreNuevo
                        ? $"{nombreAntes} → {nombreNuevo}"
                        : nombreNuevo ?? nombreAntes;
                    // En una tarea también se anota el estado con que quedó (la web cambia el estado desde Editar)
                    int? idEstadoEditado = LeerEntero(contexto, "id_estado");
                    string? estadoEditado = idEstadoEditado.HasValue ? _auditoria.ObtenerNombre("Estados", idEstadoEditado.Value) : null;
                    return estadoEditado == null ? nombre : $"{nombre} · estado: {estadoEditado}";

                case "Cambiar estado":
                    int? idEstado = LeerEntero(contexto, "id_estado");
                    string? estado = idEstado.HasValue ? _auditoria.ObtenerNombre("Estados", idEstado.Value) : null;
                    return estado == null ? nombreAntes : $"{nombreAntes} → {estado}";

                default:
                    return nombreAntes;
            }
        }

        private static int? LeerIdDeLaRuta(ActionExecutingContext contexto)
        {
            object? valor = contexto.RouteData.Values["id"];
            return int.TryParse(valor?.ToString(), out int id) ? id : null;
        }

        private static int? LeerIdDeLaRespuesta(IActionResult? resultado)
        {
            if (resultado is not ObjectResult { Value: not null } objeto) return null;
            object? id = objeto.Value.GetType().GetProperty("id")?.GetValue(objeto.Value);
            try
            {
                return id == null ? null : Convert.ToInt32(id);
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Busca en el objeto recibido (el DTO del cuerpo) el primer campo que sirva como nombre
        private static string? LeerNombreDelCuerpo(ActionExecutingContext contexto)
        {
            foreach (object? argumento in contexto.ActionArguments.Values)
            {
                if (argumento == null || argumento is string || argumento.GetType().IsPrimitive) continue;
                foreach (string campo in CamposNombre)
                {
                    if (argumento.GetType().GetProperty(campo)?.GetValue(argumento) is string texto && !string.IsNullOrWhiteSpace(texto))
                        return texto.Trim();
                }
            }
            return null;
        }

        private static int? LeerEntero(ActionExecutingContext contexto, string campo)
        {
            foreach (object? argumento in contexto.ActionArguments.Values)
            {
                if (argumento == null || argumento.GetType().IsPrimitive) continue;
                object? valor = argumento.GetType().GetProperty(campo)?.GetValue(argumento);
                if (valor is int entero) return entero;
            }
            return null;
        }
    }
}
