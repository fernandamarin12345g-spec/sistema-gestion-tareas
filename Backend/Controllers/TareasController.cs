using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TareasController : ControllerBase
    {
        // Prioridades permitidas (la base de datos también las restringe con un CHECK)
        private static readonly string[] PrioridadesValidas = { "Baja", "Media", "Alta", "Crítica" };

        // Formatos en que se guardan las fechas en SQLite
        private const string FormatoFechaHora = "yyyy-MM-dd HH:mm:ss";
        private const string FormatoFecha = "yyyy-MM-dd";

        private readonly IConfiguration _configuracion;

        public TareasController(IConfiguration configuracion)
        {
            _configuracion = configuracion;
        }

        public class TareaDto
        {
            public required string titulo { get; set; }
            public string? descripcion { get; set; }          // Opcional
            public int? IdUsuarioResponsable { get; set; }    // Opcional: tarea sin responsable
            public int id_estado { get; set; }
            public int? id_proyecto { get; set; }             // Opcional
            public string? prioridad { get; set; }            // Baja, Media, Alta o Crítica (por defecto Media)
            public string? fecha_limite { get; set; }         // Opcional, formato AAAA-MM-DD
        }

        private string CadenaConexion => _configuracion.GetConnectionString("DefaultConnection") ?? string.Empty;

        // Devuelve la prioridad escrita como en la lista oficial (sin importar mayúsculas),
        // "Media" si viene vacía, o null si no es una prioridad válida.
        private static string? NormalizarPrioridad(string? prioridad)
        {
            if (string.IsNullOrWhiteSpace(prioridad)) return "Media";
            return PrioridadesValidas.FirstOrDefault(p => string.Equals(p, prioridad.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        // Convierte un texto vacío en null, y quita espacios sobrantes
        private static string? TextoONull(string? texto)
        {
            return string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
        }

        // Valida los datos antes de tocar la base de datos.
        // Devuelve el mensaje de error, o null si todo está bien.
        private static string? ValidarTarea(TareaDto tarea)
        {
            if (string.IsNullOrWhiteSpace(tarea.titulo))
                return "El título de la tarea es obligatorio.";

            if (tarea.titulo.Trim().Length > 200)
                return "El título no puede tener más de 200 caracteres.";

            if (tarea.descripcion != null && tarea.descripcion.Trim().Length > 2000)
                return "La descripción no puede tener más de 2000 caracteres.";

            if (tarea.id_estado <= 0)
                return "Debes seleccionar un estado válido.";

            if (tarea.IdUsuarioResponsable is <= 0)
                return "El responsable seleccionado no es válido.";

            if (tarea.id_proyecto is <= 0)
                return "El proyecto seleccionado no es válido.";

            if (NormalizarPrioridad(tarea.prioridad) == null)
                return "La prioridad debe ser Baja, Media, Alta o Crítica.";

            if (!string.IsNullOrWhiteSpace(tarea.fecha_limite) &&
                !DateOnly.TryParseExact(tarea.fecha_limite.Trim(), FormatoFecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                return "La fecha límite debe tener el formato AAAA-MM-DD (por ejemplo 2026-11-15).";

            return null;
        }

        // Consulta si un estado es final (true/false). Devuelve null si el estado no existe.
        private static bool? ConsultarEsFinal(SqliteConnection conexion, int idEstado)
        {
            using (var comando = conexion.CreateCommand())
            {
                comando.CommandText = "SELECT es_final FROM Estados WHERE id_estado = @id";
                comando.Parameters.AddWithValue("@id", idEstado);
                object? resultado = comando.ExecuteScalar();

                if (resultado == null || resultado == DBNull.Value) return null;
                return Convert.ToInt64(resultado) == 1;
            }
        }

        // Consulta base: la tarea con los nombres de su responsable, estado y proyecto
        private const string ConsultaBase = @"
            SELECT t.id_tarea, t.titulo, t.descripcion,
                   t.id_usuario_responsable, u.nombre,
                   t.id_estado, e.nombre_estado, e.es_final,
                   t.id_proyecto, p.nombre,
                   t.prioridad, t.fecha_creacion, t.fecha_limite, t.fecha_finalizacion, t.fecha_actualizacion
            FROM Tareas t
            LEFT JOIN Usuarios u ON t.id_usuario_responsable = u.id_usuario
            LEFT JOIN Estados e ON t.id_estado = e.id_estado
            LEFT JOIN Proyectos p ON t.id_proyecto = p.id_proyecto";

        // Convierte una fila de la consulta en el objeto JSON que devuelve la API
        private static object LeerTarea(SqliteDataReader lector)
        {
            string? fechaLimite = lector.IsDBNull(12) ? null : lector.GetString(12);
            bool esFinal = !lector.IsDBNull(7) && lector.GetInt64(7) == 1;

            // Regla de negocio: una tarea está atrasada si su fecha límite ya pasó
            // y su estado NO es final (no está Terminada ni Cancelada).
            bool atrasada = fechaLimite != null
                && DateOnly.TryParseExact(fechaLimite, FormatoFecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out var limite)
                && limite < DateOnly.FromDateTime(DateTime.Now)
                && !esFinal;

            return new
            {
                id = lector.GetInt32(0),
                titulo = lector.GetString(1),
                descripcion = lector.IsDBNull(2) ? null : lector.GetString(2),
                id_usuario = lector.IsDBNull(3) ? (int?)null : lector.GetInt32(3),
                usuario_asignado = lector.IsDBNull(4) ? "Sin asignar" : lector.GetString(4),
                id_estado = lector.IsDBNull(5) ? (int?)null : lector.GetInt32(5),
                estado = lector.IsDBNull(6) ? "Sin estado" : lector.GetString(6),
                es_final = esFinal,
                id_proyecto = lector.IsDBNull(8) ? (int?)null : lector.GetInt32(8),
                proyecto = lector.IsDBNull(9) ? "Sin proyecto" : lector.GetString(9),
                prioridad = lector.IsDBNull(10) ? "Media" : lector.GetString(10),
                fecha_creacion = lector.IsDBNull(11) ? null : lector.GetString(11),
                fecha_limite = fechaLimite,
                fecha_finalizacion = lector.IsDBNull(13) ? null : lector.GetString(13),
                fecha_actualizacion = lector.IsDBNull(14) ? null : lector.GetString(14),
                atrasada
            };
        }

        // 1. LISTAR TAREAS (GET) con filtros opcionales
        // Ejemplos: /api/Tareas?id_estado=9   /api/Tareas?prioridad=Alta   /api/Tareas?atrasadas=true
        [HttpGet]
        public IActionResult ObtenerTareas(
            [FromQuery] int? id_estado,
            [FromQuery] int? id_usuario,
            [FromQuery] int? id_proyecto,
            [FromQuery] string? prioridad,
            [FromQuery] bool? atrasadas)
        {
            var listaTareas = new List<object>();
            var condiciones = new List<string>();

            using (var conexion = new SqliteConnection(CadenaConexion))
            {
                conexion.Open();
                var comando = conexion.CreateCommand();

                // Cada filtro que llega se agrega como condición del WHERE, siempre con parámetros
                if (id_estado.HasValue)
                {
                    condiciones.Add("t.id_estado = @filtroEstado");
                    comando.Parameters.AddWithValue("@filtroEstado", id_estado.Value);
                }
                if (id_usuario.HasValue)
                {
                    condiciones.Add("t.id_usuario_responsable = @filtroUsuario");
                    comando.Parameters.AddWithValue("@filtroUsuario", id_usuario.Value);
                }
                if (id_proyecto.HasValue)
                {
                    condiciones.Add("t.id_proyecto = @filtroProyecto");
                    comando.Parameters.AddWithValue("@filtroProyecto", id_proyecto.Value);
                }
                if (!string.IsNullOrWhiteSpace(prioridad))
                {
                    condiciones.Add("t.prioridad = @filtroPrioridad");
                    comando.Parameters.AddWithValue("@filtroPrioridad", NormalizarPrioridad(prioridad) ?? prioridad.Trim());
                }
                if (atrasadas == true)
                {
                    // Misma regla que en LeerTarea: fecha límite vencida y estado no final
                    condiciones.Add("t.fecha_limite < date('now', 'localtime') AND COALESCE(e.es_final, 0) = 0");
                }

                string consulta = ConsultaBase;
                if (condiciones.Count > 0)
                {
                    consulta += " WHERE " + string.Join(" AND ", condiciones);
                }
                comando.CommandText = consulta + " ORDER BY t.id_tarea";

                using (var lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        listaTareas.Add(LeerTarea(lector));
                    }
                }
            }
            return Ok(listaTareas);
        }

        // 2. CONSULTAR EL DETALLE DE UNA TAREA (GET por id)
        [HttpGet("{id}")]
        public IActionResult ObtenerTarea(int id)
        {
            using (var conexion = new SqliteConnection(CadenaConexion))
            {
                conexion.Open();
                var comando = conexion.CreateCommand();
                comando.CommandText = ConsultaBase + " WHERE t.id_tarea = @id";
                comando.Parameters.AddWithValue("@id", id);

                using (var lector = comando.ExecuteReader())
                {
                    if (lector.Read())
                    {
                        return Ok(LeerTarea(lector));
                    }
                }
            }
            return NotFound(new { mensaje = "Tarea no encontrada" });
        }

        // 3. CREAR TAREA (POST)
        [HttpPost]
        public IActionResult CrearTarea([FromBody] TareaDto nuevaTarea)
        {
            string? error = ValidarTarea(nuevaTarea);
            if (error != null)
            {
                return BadRequest(new { mensaje = error });
            }

            string ahora = DateTime.Now.ToString(FormatoFechaHora, CultureInfo.InvariantCulture);
            long nuevoId;

            try
            {
                using (var conexion = new SqliteConnection(CadenaConexion))
                {
                    conexion.Open();

                    bool? esFinal = ConsultarEsFinal(conexion, nuevaTarea.id_estado);
                    if (esFinal == null)
                    {
                        return BadRequest(new { mensaje = "El estado seleccionado no existe." });
                    }

                    var comando = conexion.CreateCommand();
                    comando.CommandText = @"
                        INSERT INTO Tareas (titulo, descripcion, id_usuario_responsable, id_estado, id_proyecto,
                                            prioridad, fecha_creacion, fecha_limite, fecha_finalizacion, fecha_actualizacion)
                        VALUES (@titulo, @descripcion, @usuario, @estado, @proyecto,
                                @prioridad, @ahora, @limite, @finalizacion, @ahora);
                        SELECT last_insert_rowid();";
                    comando.Parameters.AddWithValue("@titulo", nuevaTarea.titulo.Trim());
                    comando.Parameters.AddWithValue("@descripcion", (object?)TextoONull(nuevaTarea.descripcion) ?? DBNull.Value);
                    comando.Parameters.AddWithValue("@usuario", (object?)nuevaTarea.IdUsuarioResponsable ?? DBNull.Value);
                    comando.Parameters.AddWithValue("@estado", nuevaTarea.id_estado);
                    comando.Parameters.AddWithValue("@proyecto", (object?)nuevaTarea.id_proyecto ?? DBNull.Value);
                    comando.Parameters.AddWithValue("@prioridad", NormalizarPrioridad(nuevaTarea.prioridad)!);
                    comando.Parameters.AddWithValue("@ahora", ahora);
                    comando.Parameters.AddWithValue("@limite", (object?)TextoONull(nuevaTarea.fecha_limite) ?? DBNull.Value);
                    // Si la tarea se crea directamente en un estado final, queda con fecha de finalización
                    comando.Parameters.AddWithValue("@finalizacion", esFinal == true ? (object)ahora : DBNull.Value);

                    nuevoId = (long)(comando.ExecuteScalar() ?? 0L);
                }
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                return BadRequest(new { mensaje = "El responsable o el proyecto seleccionado no existe." });
            }

            // 201 Created: el estándar REST cuando se crea un recurso nuevo
            return Created($"/api/Tareas/{nuevoId}", new { mensaje = "Tarea creada exitosamente", id = nuevoId });
        }

        // 4. ACTUALIZAR TAREA (PUT)
        [HttpPut("{id}")]
        public IActionResult ActualizarTarea(int id, [FromBody] TareaDto tareaActualizada)
        {
            string? error = ValidarTarea(tareaActualizada);
            if (error != null)
            {
                return BadRequest(new { mensaje = error });
            }

            string ahora = DateTime.Now.ToString(FormatoFechaHora, CultureInfo.InvariantCulture);

            try
            {
                using (var conexion = new SqliteConnection(CadenaConexion))
                {
                    conexion.Open();

                    bool? esFinal = ConsultarEsFinal(conexion, tareaActualizada.id_estado);
                    if (esFinal == null)
                    {
                        return BadRequest(new { mensaje = "El estado seleccionado no existe." });
                    }

                    var comando = conexion.CreateCommand();
                    // fecha_finalizacion: se conserva si ya estaba finalizada, se llena al pasar a un estado final,
                    // y se borra si la tarea vuelve a un estado no final (por ejemplo, de Terminada a En progreso).
                    comando.CommandText = @"
                        UPDATE Tareas
                        SET titulo = @titulo,
                            descripcion = @descripcion,
                            id_usuario_responsable = @usuario,
                            id_estado = @estado,
                            id_proyecto = @proyecto,
                            prioridad = @prioridad,
                            fecha_limite = @limite,
                            fecha_finalizacion = CASE WHEN @esFinal = 1 THEN COALESCE(fecha_finalizacion, @ahora) ELSE NULL END,
                            fecha_actualizacion = @ahora
                        WHERE id_tarea = @id";
                    comando.Parameters.AddWithValue("@titulo", tareaActualizada.titulo.Trim());
                    comando.Parameters.AddWithValue("@descripcion", (object?)TextoONull(tareaActualizada.descripcion) ?? DBNull.Value);
                    comando.Parameters.AddWithValue("@usuario", (object?)tareaActualizada.IdUsuarioResponsable ?? DBNull.Value);
                    comando.Parameters.AddWithValue("@estado", tareaActualizada.id_estado);
                    comando.Parameters.AddWithValue("@proyecto", (object?)tareaActualizada.id_proyecto ?? DBNull.Value);
                    comando.Parameters.AddWithValue("@prioridad", NormalizarPrioridad(tareaActualizada.prioridad)!);
                    comando.Parameters.AddWithValue("@limite", (object?)TextoONull(tareaActualizada.fecha_limite) ?? DBNull.Value);
                    comando.Parameters.AddWithValue("@esFinal", esFinal == true ? 1 : 0);
                    comando.Parameters.AddWithValue("@ahora", ahora);
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
                return BadRequest(new { mensaje = "El responsable o el proyecto seleccionado no existe." });
            }
        }

        // 5. ELIMINAR TAREA (DELETE)
        [HttpDelete("{id}")]
        public IActionResult EliminarTarea(int id)
        {
            using (var conexion = new SqliteConnection(CadenaConexion))
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
