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

        // 1. LECTURA Y RENDERIZADO (GET)
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

        // 2. CREAR ESTADO (POST) - 
        [HttpPost]
        public IActionResult CrearEstado([FromBody] EstadoNuevo nuevoEstado)
        {
            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection") ?? string.Empty;

            try
            {
                using (var conexion = new SqliteConnection(cadenaConexion))
                {
                    conexion.Open();
                    var comando = conexion.CreateCommand();
                    comando.CommandText = "INSERT INTO Estados (nombre_estado) VALUES (@nombre)";
                    comando.Parameters.AddWithValue("@nombre", nuevoEstado.nombre_estado);
                    comando.ExecuteNonQuery();
                }
                return Ok(new { mensaje = "Estado creado exitosamente" });
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                return BadRequest(new { mensaje = "El estado ya existe o entra en conflicto con un registro anterior." });
            }
        }

        // 3. ACTUALIZAR ESTADO (PUT) 
        [HttpPut("{id}")]
        public IActionResult ActualizarEstado(int id, [FromBody] EstadoNuevo estadoActualizado)
        {
            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection") ?? string.Empty;

            try
            {
                using (var conexion = new SqliteConnection(cadenaConexion))
                {
                    conexion.Open();
                    var comando = conexion.CreateCommand();
                    comando.CommandText = "UPDATE Estados SET nombre_estado = @nombre WHERE id_estado = @id";
                    comando.Parameters.AddWithValue("@nombre", estadoActualizado.nombre_estado);
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
                return BadRequest(new { mensaje = "El nombre que intentas asignar ya está registrado." });
            }
        }

        // 4. ELIMINAR ESTADO (DELETE) - 
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
                    comando.ExecuteNonQuery();
                }
                return Ok(new { mensaje = "Estado eliminado exitosamente" });
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                // devolvemos un mensaje 
                return BadRequest(new { mensaje = "No se puede eliminar este estado porque hay tareas que actualmente lo están usando. Cambia el estado de esas tareas primero." });
            }
        } 
    } 
} 