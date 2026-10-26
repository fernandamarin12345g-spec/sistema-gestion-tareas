using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EstadosController : ControllerBase
    {
        private readonly IConfiguration _configuracion;

        public EstadosController(IConfiguration configuracion)
        {
            _configuracion = configuracion;
        }

        public class EstadoNuevo
        {
            public required string nombre_estado { get; set; }
        }

        // Valida el nombre antes de tocar la base de datos. Devuelve el mensaje de error o null.
        private static string? ValidarNombre(string? nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return "El nombre del estado es obligatorio.";

            if (nombre.Trim().Length > 50)
                return "El nombre del estado no puede tener más de 50 caracteres.";

            return null;
        }

        // Revisa si ya existe OTRO estado con el mismo nombre, sin importar mayúsculas/minúsculas.
        private static bool ExisteNombre(SqliteConnection conexion, string nombre, int idExcluir)
        {
            using (var comando = conexion.CreateCommand())
            {
                comando.CommandText = "SELECT COUNT(1) FROM Estados WHERE LOWER(nombre_estado) = LOWER(@nombre) AND id_estado != @id";
                comando.Parameters.AddWithValue("@nombre", nombre);
                comando.Parameters.AddWithValue("@id", idExcluir);
                return (long)(comando.ExecuteScalar() ?? 0L) > 0;
            }
        }

        // 1. LISTAR ESTADOS (GET)
        [HttpGet]
        public IActionResult ObtenerEstados()
        {
            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection") ?? string.Empty;
            var listaEstados = new List<object>();

            using (var conexion = new SqliteConnection(cadenaConexion))
            {
                conexion.Open();
                var comando = conexion.CreateCommand();
                comando.CommandText = "SELECT id_estado, nombre_estado FROM Estados";

                using (var lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        listaEstados.Add(new
                        {
                            id_estado = lector.GetInt32(0),
                            nombre_estado = lector.GetString(1)
                        });
                    }
                }
            }
            return Ok(listaEstados);
        }

        // 2. CREAR ESTADO (POST)
        [HttpPost]
        public IActionResult CrearEstado([FromBody] EstadoNuevo nuevoEstado)
        {
            string? error = ValidarNombre(nuevoEstado.nombre_estado);
            if (error != null)
            {
                return BadRequest(new { mensaje = error });
            }

            string nombre = nuevoEstado.nombre_estado.Trim();
            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection") ?? string.Empty;
            long nuevoId;

            try
            {
                using (var conexion = new SqliteConnection(cadenaConexion))
                {
                    conexion.Open();

                    if (ExisteNombre(conexion, nombre, 0))
                    {
                        return BadRequest(new { mensaje = "Ya existe un estado con ese nombre." });
                    }

                    var comando = conexion.CreateCommand();
                    comando.CommandText = @"
                        INSERT INTO Estados (nombre_estado) VALUES (@nombre);
                        SELECT last_insert_rowid();";
                    comando.Parameters.AddWithValue("@nombre", nombre);
                    nuevoId = (long)(comando.ExecuteScalar() ?? 0L);
                }
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                return BadRequest(new { mensaje = "Ya existe un estado con ese nombre." });
            }

            return Created($"/api/Estados/{nuevoId}", new { mensaje = "Estado creado exitosamente", id = nuevoId });
        }

        // 3. ACTUALIZAR ESTADO (PUT)
        [HttpPut("{id}")]
        public IActionResult ActualizarEstado(int id, [FromBody] EstadoNuevo estadoActualizado)
        {
            string? error = ValidarNombre(estadoActualizado.nombre_estado);
            if (error != null)
            {
                return BadRequest(new { mensaje = error });
            }

            string nombre = estadoActualizado.nombre_estado.Trim();
            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection") ?? string.Empty;

            try
            {
                using (var conexion = new SqliteConnection(cadenaConexion))
                {
                    conexion.Open();

                    if (ExisteNombre(conexion, nombre, id))
                    {
                        return BadRequest(new { mensaje = "Ya existe otro estado con ese nombre." });
                    }

                    var comando = conexion.CreateCommand();
                    comando.CommandText = "UPDATE Estados SET nombre_estado = @nombre WHERE id_estado = @id";
                    comando.Parameters.AddWithValue("@nombre", nombre);
                    comando.Parameters.AddWithValue("@id", id);

                    int filasAfectadas = comando.ExecuteNonQuery();
                    if (filasAfectadas == 0)
                    {
                        return NotFound(new { mensaje = "Estado no encontrado" });
                    }
                }
                return Ok(new { mensaje = "Estado actualizado exitosamente" });
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                return BadRequest(new { mensaje = "Ya existe otro estado con ese nombre." });
            }
        }

        // 4. ELIMINAR ESTADO (DELETE)
        [HttpDelete("{id}")]
        public IActionResult EliminarEstado(int id)
        {
            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection") ?? string.Empty;

            try
            {
                using (var conexion = new SqliteConnection(cadenaConexion))
                {
                    conexion.Open();
                    var comando = conexion.CreateCommand();
                    comando.CommandText = "DELETE FROM Estados WHERE id_estado = @id";
                    comando.Parameters.AddWithValue("@id", id);

                    int filasAfectadas = comando.ExecuteNonQuery();
                    if (filasAfectadas == 0)
                    {
                        return NotFound(new { mensaje = "Estado no encontrado" });
                    }
                }
                return Ok(new { mensaje = "Estado eliminado exitosamente" });
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                // La llave foránea impide borrar un estado que todavía usan algunas tareas
                return BadRequest(new { mensaje = "No se puede eliminar este estado porque hay tareas que actualmente lo están usando. Cambia el estado de esas tareas primero." });
            }
        }
    }
}