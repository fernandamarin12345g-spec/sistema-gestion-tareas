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
            public int? IdUsuarioResponsable { get; set; } // Puede venir vacío: tarea sin responsable
            public int id_estado { get; set; }             // ID del estado seleccionado en el formulario
        }

        // Valida los datos antes de tocar la base de datos.
        // Devuelve el mensaje de error, o null si todo está bien.
        private static string? ValidarTarea(TareaDto tarea)
        {
            if (string.IsNullOrWhiteSpace(tarea.titulo))
                return "El título de la tarea es obligatorio.";

            if (tarea.titulo.Trim().Length > 200)
                return "El título no puede tener más de 200 caracteres.";

            if (tarea.id_estado <= 0)
                return "Debes seleccionar un estado válido.";

            if (tarea.IdUsuarioResponsable is <= 0)
                return "El responsable seleccionado no es válido.";

            return null;
        }

        // 1. LISTAR TAREAS (GET)
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
                            // Con LEFT JOIN estos valores pueden venir vacíos (NULL): los revisamos antes de leerlos
                            id_estado = lector.IsDBNull(4) ? (int?)null : lector.GetInt32(4),
                            estado = lector.IsDBNull(5) ? "Sin estado" : lector.GetString(5)
                        });
                    }
                }
            }
            return Ok(listaTareas);
        }

        // 2. CREAR TAREA (POST)
        [HttpPost]
        public IActionResult CrearTarea([FromBody] TareaDto nuevaTarea)
        {
            string? error = ValidarTarea(nuevaTarea);
            if (error != null)
            {
                return BadRequest(new { mensaje = error });
            }

            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection") ?? string.Empty;
            long nuevoId;

            try
            {
                using (var conexion = new SqliteConnection(cadenaConexion))
                {
                    conexion.Open();
                    var comando = conexion.CreateCommand();
                    // Inserta la tarea y en la misma consulta pide el ID que SQLite le asignó
                    comando.CommandText = @"
                        INSERT INTO Tareas (titulo, id_usuario_responsable, id_estado)
                        VALUES (@titulo, @usuario, @estado);
                        SELECT last_insert_rowid();";
                    comando.Parameters.AddWithValue("@titulo", nuevaTarea.titulo.Trim());
                    // Si no hay responsable, se guarda NULL en la base de datos
                    comando.Parameters.AddWithValue("@usuario", (object?)nuevaTarea.IdUsuarioResponsable ?? DBNull.Value);
                    comando.Parameters.AddWithValue("@estado", nuevaTarea.id_estado);

                    nuevoId = (long)(comando.ExecuteScalar() ?? 0L);
                }
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                return BadRequest(new { mensaje = "El estado o el responsable seleccionado no existe." });
            }

            // 201 Created: el estándar REST cuando se crea un recurso nuevo
            return Created($"/api/Tareas/{nuevoId}", new { mensaje = "Tarea creada exitosamente", id = nuevoId });
        }

        // 3. ACTUALIZAR TAREA (PUT)
        [HttpPut("{id}")]
        public IActionResult ActualizarTarea(int id, [FromBody] TareaDto tareaActualizada)
        {
            string? error = ValidarTarea(tareaActualizada);
            if (error != null)
            {
                return BadRequest(new { mensaje = error });
            }

            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection") ?? string.Empty;

            try
            {
                using (var conexion = new SqliteConnection(cadenaConexion))
                {
                    conexion.Open();
                    var comando = conexion.CreateCommand();
                    comando.CommandText = @"
                        UPDATE Tareas
                        SET titulo = @titulo, id_usuario_responsable = @usuario, id_estado = @estado
                        WHERE id_tarea = @id";
                    comando.Parameters.AddWithValue("@titulo", tareaActualizada.titulo.Trim());
                    comando.Parameters.AddWithValue("@usuario", (object?)tareaActualizada.IdUsuarioResponsable ?? DBNull.Value);
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
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                return BadRequest(new { mensaje = "El estado o el responsable seleccionado no existe." });
            }
        }

        // 4. ELIMINAR TAREA (DELETE)
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

                int filasAfectadas = comando.ExecuteNonQuery();
                if (filasAfectadas == 0)
                {
                    return NotFound(new { mensaje = "Tarea no encontrada" });
                }
            }
            return Ok(new { mensaje = "Tarea eliminada exitosamente" });
        }
    }
}