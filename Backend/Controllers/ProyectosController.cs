using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace Backend.Controllers
{
    // Maestro de Proyectos: un proyecto agrupa tareas (por ejemplo "Sistema de Gestión de Tareas").
    [Route("api/[controller]")]
    [ApiController]
    public class ProyectosController : ControllerBase
    {
        // Estados permitidos para un proyecto
        private static readonly string[] EstadosValidos = { "Planificación", "Activo", "Pausado", "Finalizado" };

        private readonly IConfiguration _configuracion;

        public ProyectosController(IConfiguration configuracion)
        {
            _configuracion = configuracion;
        }

        public class ProyectoDto
        {
            public required string nombre { get; set; }
            public string? descripcion { get; set; }   // Opcional
            public string? estado { get; set; }        // Planificación, Activo, Pausado o Finalizado (por defecto Activo)
        }

        private string CadenaConexion => _configuracion.GetConnectionString("DefaultConnection") ?? string.Empty;

        // Devuelve el estado escrito como en la lista oficial (sin importar mayúsculas),
        // "Activo" si viene vacío, o null si no es un estado válido.
        private static string? NormalizarEstado(string? estado)
        {
            if (string.IsNullOrWhiteSpace(estado)) return "Activo";
            return EstadosValidos.FirstOrDefault(e => string.Equals(e, estado.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        // Valida los datos antes de tocar la base de datos. Devuelve el mensaje de error o null.
        private static string? ValidarProyecto(ProyectoDto proyecto)
        {
            if (string.IsNullOrWhiteSpace(proyecto.nombre))
                return "El nombre del proyecto es obligatorio.";

            if (proyecto.nombre.Trim().Length > 100)
                return "El nombre del proyecto no puede tener más de 100 caracteres.";

            if (proyecto.descripcion != null && proyecto.descripcion.Trim().Length > 1000)
                return "La descripción no puede tener más de 1000 caracteres.";

            if (NormalizarEstado(proyecto.estado) == null)
                return "El estado del proyecto debe ser Planificación, Activo, Pausado o Finalizado.";

            return null;
        }

        // Revisa si ya existe OTRO proyecto con el mismo nombre, sin importar mayúsculas/minúsculas.
        // La comparación se hace en C# y no con LOWER() de SQLite, porque LOWER() de SQLite
        // no convierte letras con tilde: "GESTIÓN" y "gestión" las vería como distintas.
        private static bool ExisteNombre(SqliteConnection conexion, string nombre, int idExcluir)
        {
            using (var comando = conexion.CreateCommand())
            {
                comando.CommandText = "SELECT nombre FROM Proyectos WHERE id_proyecto != @id";
                comando.Parameters.AddWithValue("@id", idExcluir);

                using (var lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        if (string.Equals(lector.GetString(0).Trim(), nombre, StringComparison.OrdinalIgnoreCase))
                            return true;
                    }
                }
            }
            return false;
        }

        // Consulta base: cada proyecto con cuántas tareas tiene en total y cuántas siguen abiertas
        private const string ConsultaBase = @"
            SELECT p.id_proyecto, p.nombre, p.descripcion, p.estado,
                   COUNT(t.id_tarea) AS total_tareas,
                   SUM(CASE WHEN t.id_tarea IS NOT NULL AND COALESCE(e.es_final, 0) = 0 THEN 1 ELSE 0 END) AS tareas_abiertas
            FROM Proyectos p
            LEFT JOIN Tareas t ON t.id_proyecto = p.id_proyecto
            LEFT JOIN Estados e ON t.id_estado = e.id_estado";

        private static object LeerProyecto(SqliteDataReader lector)
        {
            return new
            {
                id_proyecto = lector.GetInt32(0),
                nombre = lector.GetString(1),
                descripcion = lector.IsDBNull(2) ? null : lector.GetString(2),
                estado = lector.IsDBNull(3) ? "Activo" : lector.GetString(3),
                total_tareas = lector.GetInt64(4),
                tareas_abiertas = lector.IsDBNull(5) ? 0 : lector.GetInt64(5)
            };
        }

        // 1. LISTAR PROYECTOS (GET)
        [HttpGet]
        public IActionResult ObtenerProyectos()
        {
            var listaProyectos = new List<object>();

            using (var conexion = new SqliteConnection(CadenaConexion))
            {
                conexion.Open();
                var comando = conexion.CreateCommand();
                comando.CommandText = ConsultaBase + " GROUP BY p.id_proyecto ORDER BY p.nombre";

                using (var lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        listaProyectos.Add(LeerProyecto(lector));
                    }
                }
            }
            return Ok(listaProyectos);
        }

        // 2. CONSULTAR UN PROYECTO (GET por id)
        [HttpGet("{id}")]
        public IActionResult ObtenerProyecto(int id)
        {
            using (var conexion = new SqliteConnection(CadenaConexion))
            {
                conexion.Open();
                var comando = conexion.CreateCommand();
                comando.CommandText = ConsultaBase + " WHERE p.id_proyecto = @id GROUP BY p.id_proyecto";
                comando.Parameters.AddWithValue("@id", id);

                using (var lector = comando.ExecuteReader())
                {
                    if (lector.Read())
                    {
                        return Ok(LeerProyecto(lector));
                    }
                }
            }
            return NotFound(new { mensaje = "Proyecto no encontrado" });
        }

        // 3. CREAR PROYECTO (POST)
        [HttpPost]
        public IActionResult CrearProyecto([FromBody] ProyectoDto nuevoProyecto)
        {
            string? error = ValidarProyecto(nuevoProyecto);
            if (error != null)
            {
                return BadRequest(new { mensaje = error });
            }

            string nombre = nuevoProyecto.nombre.Trim();
            long nuevoId;

            using (var conexion = new SqliteConnection(CadenaConexion))
            {
                conexion.Open();

                if (ExisteNombre(conexion, nombre, 0))
                {
                    return BadRequest(new { mensaje = "Ya existe un proyecto con ese nombre." });
                }

                var comando = conexion.CreateCommand();
                comando.CommandText = @"
                    INSERT INTO Proyectos (nombre, descripcion, estado) VALUES (@nombre, @descripcion, @estado);
                    SELECT last_insert_rowid();";
                comando.Parameters.AddWithValue("@nombre", nombre);
                comando.Parameters.AddWithValue("@descripcion", string.IsNullOrWhiteSpace(nuevoProyecto.descripcion) ? DBNull.Value : (object)nuevoProyecto.descripcion.Trim());
                comando.Parameters.AddWithValue("@estado", NormalizarEstado(nuevoProyecto.estado)!);
                nuevoId = (long)(comando.ExecuteScalar() ?? 0L);
            }

            return Created($"/api/Proyectos/{nuevoId}", new { mensaje = "Proyecto creado exitosamente", id = nuevoId });
        }

        // 4. ACTUALIZAR PROYECTO (PUT)
        [HttpPut("{id}")]
        public IActionResult ActualizarProyecto(int id, [FromBody] ProyectoDto proyectoActualizado)
        {
            string? error = ValidarProyecto(proyectoActualizado);
            if (error != null)
            {
                return BadRequest(new { mensaje = error });
            }

            string nombre = proyectoActualizado.nombre.Trim();

            using (var conexion = new SqliteConnection(CadenaConexion))
            {
                conexion.Open();

                if (ExisteNombre(conexion, nombre, id))
                {
                    return BadRequest(new { mensaje = "Ya existe otro proyecto con ese nombre." });
                }

                var comando = conexion.CreateCommand();
                comando.CommandText = "UPDATE Proyectos SET nombre = @nombre, descripcion = @descripcion, estado = @estado WHERE id_proyecto = @id";
                comando.Parameters.AddWithValue("@nombre", nombre);
                comando.Parameters.AddWithValue("@descripcion", string.IsNullOrWhiteSpace(proyectoActualizado.descripcion) ? DBNull.Value : (object)proyectoActualizado.descripcion.Trim());
                comando.Parameters.AddWithValue("@estado", NormalizarEstado(proyectoActualizado.estado)!);
                comando.Parameters.AddWithValue("@id", id);

                int filasAfectadas = comando.ExecuteNonQuery();
                if (filasAfectadas == 0)
                {
                    return NotFound(new { mensaje = "Proyecto no encontrado" });
                }
            }
            return Ok(new { mensaje = "Proyecto actualizado exitosamente" });
        }

        // 5. ELIMINAR PROYECTO (DELETE)
        [HttpDelete("{id}")]
        public IActionResult EliminarProyecto(int id)
        {
            using (var conexion = new SqliteConnection(CadenaConexion))
            {
                conexion.Open();

                // Regla de negocio: no se elimina un proyecto que todavía tiene tareas.
                // (La base tiene ON DELETE CASCADE y borraría esas tareas sin avisar; aquí lo impedimos.)
                using (var comandoContar = conexion.CreateCommand())
                {
                    comandoContar.CommandText = "SELECT COUNT(1) FROM Tareas WHERE id_proyecto = @id";
                    comandoContar.Parameters.AddWithValue("@id", id);
                    long cantidadTareas = (long)(comandoContar.ExecuteScalar() ?? 0L);

                    if (cantidadTareas > 0)
                    {
                        return BadRequest(new { mensaje = $"No se puede eliminar este proyecto porque tiene {cantidadTareas} tarea(s). Muévelas a otro proyecto o elimínalas primero." });
                    }
                }

                var comando = conexion.CreateCommand();
                comando.CommandText = "DELETE FROM Proyectos WHERE id_proyecto = @id";
                comando.Parameters.AddWithValue("@id", id);

                int filasAfectadas = comando.ExecuteNonQuery();
                if (filasAfectadas == 0)
                {
                    return NotFound(new { mensaje = "Proyecto no encontrado" });
                }
            }
            return Ok(new { mensaje = "Proyecto eliminado exitosamente" });
        }
    }
}
