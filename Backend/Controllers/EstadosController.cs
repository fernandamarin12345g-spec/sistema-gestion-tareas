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

        [HttpPost]
        public IActionResult CrearEstado([FromBody] EstadoNuevo nuevoEstado)
        {
            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection") ?? string.Empty;

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

        [HttpDelete("{id}")]
        public IActionResult EliminarEstado(int id)
        {
            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection") ?? string.Empty;

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
    }
}