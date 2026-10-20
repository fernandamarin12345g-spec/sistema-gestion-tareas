using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TareasController : ControllerBase
    {
        private readonly IConfiguration _configuracion;

        public TareasController(IConfiguration configuracion)
        {
            _configuracion = configuracion;
        }

        public class TareaDto
        {
            public required string titulo { get; set; }
            public int IdUsuarioResponsable { get; set; }
            public int id_estado { get; set; } // Recibe el ID del estado seleccionado en el formulario
        }

        [HttpGet]
        public IActionResult ObtenerTareas()
        {
            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection") ?? string.Empty;
            var listaTareas = new List<object>();

            using (var conexion = new SqliteConnection(cadenaConexion))
            {
                conexion.Open();
                var comando = conexion.CreateCommand();
                comando.CommandText = @"
                    SELECT t.id_tarea, t.titulo, t.id_usuario_responsable, u.nombre, t.id_estado, e.nombre_estado 
                    FROM Tareas t
                    LEFT JOIN Usuarios u ON t.id_usuario_responsable = u.id_usuario
                    LEFT JOIN Estados e ON t.id_estado = e.id_estado";

                using (var lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        listaTareas.Add(new
                        {
                            id = lector.GetInt32(0),
                            titulo = lector.GetString(1),
                            id_usuario = lector.IsDBNull(2) ? (int?)null : lector.GetInt32(2),
                            usuario_asignado = lector.IsDBNull(3) ? "Sin asignar" : lector.GetString(3),
                            id_estado = lector.GetInt32(4),
                            estado = lector.GetString(5)
                        });
                    }
                }
            }
            return Ok(listaTareas);
        }

        [HttpPost]
        public IActionResult CrearTarea([FromBody] TareaDto nuevaTarea)
        {
            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection") ?? string.Empty;

            try
            {
                using (var conexion = new SqliteConnection(cadenaConexion))
                {
                    conexion.Open();
                    var comando = conexion.CreateCommand();
                    comando.CommandText = "INSERT INTO Tareas (titulo, id_usuario_responsable, id_estado) VALUES (@titulo, @usuario, @estado)";
                    comando.Parameters.AddWithValue("@titulo", nuevaTarea.titulo);
                    comando.Parameters.AddWithValue("@usuario", nuevaTarea.IdUsuarioResponsable);
                    comando.Parameters.AddWithValue("@estado", nuevaTarea.id_estado);
                    comando.ExecuteNonQuery();
                }
                return Ok(new { mensaje = "Tarea creada exitosamente" });
            }
            catch (SqliteException ex)
            {
                return BadRequest(new { mensaje = "Error al crear la tarea: " + ex.Message });
            }
        }

        [HttpPut("{id}")]
        public IActionResult ActualizarTarea(int id, [FromBody] TareaDto tareaActualizada)
        {
            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection") ?? string.Empty;

            try
            {
                using (var conexion = new SqliteConnection(cadenaConexion))
                {
                    conexion.Open();
                    var comando = conexion.CreateCommand();
                    comando.CommandText = "UPDATE Tareas SET titulo = @titulo, id_usuario_responsable = @usuario, id_estado = @estado WHERE id_tarea = @id";
                    comando.Parameters.AddWithValue("@titulo", tareaActualizada.titulo);
                    comando.Parameters.AddWithValue("@usuario", tareaActualizada.IdUsuarioResponsable);
                    comando.Parameters.AddWithValue("@estado", tareaActualizada.id_estado);
                    comando.Parameters.AddWithValue("@id", id);

                    int filasAfectadas = comando.ExecuteNonQuery();
                    if (filasAfectadas == 0)
                    {
                        return NotFound(new { mensaje = "Tarea no encontrada" });
                    }
                }
                return Ok(new { mensaje = "Tarea actualizada exitosamente" });
            }
            catch (SqliteException ex)
            {
                return BadRequest(new { mensaje = "Error al actualizar la tarea: " + ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult EliminarTarea(int id)
        {
            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection") ?? string.Empty;

            using (var conexion = new SqliteConnection(cadenaConexion))
            {
                conexion.Open();
                var comando = conexion.CreateCommand();
                comando.CommandText = "DELETE FROM Tareas WHERE id_tarea = @id";
                comando.Parameters.AddWithValue("@id", id);
                comando.ExecuteNonQuery();
            }
            return Ok(new { mensaje = "Tarea eliminada exitosamente" });
        }
    }
}