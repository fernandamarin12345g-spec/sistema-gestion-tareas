using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace Backend.Controllers
{
    // Por ahora solo consulta: la lista de proyectos se usa en el formulario de tareas,
    // en los filtros y en el dashboard. El CRUD completo de proyectos queda para una versión futura.
    [Route("api/[controller]")]
    [ApiController]
    public class ProyectosController : ControllerBase
    {
        private readonly IConfiguration _configuracion;

        public ProyectosController(IConfiguration configuracion)
        {
            _configuracion = configuracion;
        }

        // 1. LISTAR PROYECTOS (GET)
        [HttpGet]
        public IActionResult ObtenerProyectos()
        {
            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection") ?? string.Empty;
            var listaProyectos = new List<object>();

            using (var conexion = new SqliteConnection(cadenaConexion))
            {
                conexion.Open();
                var comando = conexion.CreateCommand();
                comando.CommandText = "SELECT id_proyecto, nombre, estado FROM Proyectos ORDER BY nombre";

                using (var lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        listaProyectos.Add(new
                        {
                            id_proyecto = lector.GetInt32(0),
                            nombre = lector.GetString(1),
                            estado = lector.IsDBNull(2) ? null : lector.GetString(2)
                        });
                    }
                }
            }
            return Ok(listaProyectos);
        }
    }
}
