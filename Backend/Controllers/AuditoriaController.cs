using Backend.Seguridad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace Backend.Controllers
{
    // Consulta del registro de auditoría. Solo el Administrador puede verlo.
    // Es de solo lectura: los registros los crea la API sola (FiltroAuditoria y AuthController).
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Politicas.Administrador)]
    public class AuditoriaController : ControllerBase
    {
        private readonly IConfiguration _configuracion;

        public AuditoriaController(IConfiguration configuracion)
        {
            _configuracion = configuracion;
        }

        // GET /api/Auditoria?limite=200&modulo=Tareas
        // Devuelve los registros más recientes primero.
        [HttpGet]
        public IActionResult ObtenerRegistros([FromQuery] int limite = 200, [FromQuery] string? modulo = null)
        {
            limite = Math.Clamp(limite, 1, 1000);
            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection") ?? string.Empty;
            var registros = new List<object>();

            using (var conexion = new SqliteConnection(cadenaConexion))
            {
                conexion.Open();
                var comando = conexion.CreateCommand();
                comando.CommandText = @"
                    SELECT a.id_log, a.fecha_hora, a.accion, a.modulo, a.id_registro, a.detalle,
                           a.id_usuario, u.nombre
                    FROM Auditoria_Log a
                    LEFT JOIN Usuarios u ON a.id_usuario = u.id_usuario
                    WHERE (@modulo IS NULL OR a.modulo = @modulo)
                    ORDER BY a.id_log DESC
                    LIMIT @limite";
                comando.Parameters.AddWithValue("@modulo", string.IsNullOrWhiteSpace(modulo) ? (object)DBNull.Value : modulo.Trim());
                comando.Parameters.AddWithValue("@limite", limite);

                using (var lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        registros.Add(new
                        {
                            id_log = lector.GetInt32(0),
                            fecha_hora = lector.IsDBNull(1) ? null : Convert.ToString(lector.GetValue(1)),
                            accion = lector.GetString(2),
                            modulo = lector.IsDBNull(3) ? null : lector.GetString(3),
                            id_registro = lector.IsDBNull(4) ? (int?)null : lector.GetInt32(4),
                            detalle = lector.IsDBNull(5) ? null : lector.GetString(5),
                            id_usuario = lector.IsDBNull(6) ? (int?)null : lector.GetInt32(6),
                            usuario = lector.IsDBNull(7) ? "Sistema / desconocido" : lector.GetString(7)
                        });
                    }
                }
            }

            return Ok(registros);
        }
    }
}
