using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TareasController : ControllerBase
    {
        private readonly IConfiguration _configuracion;

        public TareasController(IConfiguration configuracion)
        {
            _configuracion = configuracion;
        }

        // 1. LEER (GET) 
        [HttpGet]
        public IActionResult ObtenerTareas()
        {
            var listaTareas = new List<object>();
            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection");

            using (var conexion = new SqliteConnection(cadenaConexion))
            {
                conexion.Open();
                var comando = conexion.CreateCommand();
                comando.CommandText = "SELECT * FROM Tareas";

                using (var lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        listaTareas.Add(new
                        {
                            Id = lector.GetInt32(0),
                            Titulo = lector.GetString(1),
                            Estado = lector.GetString(2)
                        });
                    }
                }
            }
            return Ok(listaTareas);
        }

        // 2. CREAR (POST)
        [HttpPost]
        public IActionResult CrearTarea([FromBody] TareaNueva tarea)
        {
            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection");
            using (var conexion = new SqliteConnection(cadenaConexion))
            {
                conexion.Open();
                var comando = conexion.CreateCommand();
                // Asignamos proyecto 1 y usuario 2 por defecto basándonos en tu SQL
                comando.CommandText = "INSERT INTO Tareas (titulo, estado, id_proyecto, id_usuario_responsable) VALUES (@titulo, 'Pendiente', 1, 2)";
                comando.Parameters.AddWithValue("@titulo", tarea.Titulo);
                comando.ExecuteNonQuery();
            }
            return Ok(new { mensaje = "Tarea creada exitosamente" });
        }

        // 3. ACTUALIZAR (PUT)
        [HttpPut("{id}")]
        public IActionResult ActualizarTarea(int id, [FromBody] TareaActualizada tarea)
        {
            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection");
            using (var conexion = new SqliteConnection(cadenaConexion))
            {
                conexion.Open();
                var comando = conexion.CreateCommand();
                comando.CommandText = "UPDATE Tareas SET estado = @estado WHERE id_tarea = @id";
                comando.Parameters.AddWithValue("@estado", tarea.Estado);
                comando.Parameters.AddWithValue("@id", id);
                comando.ExecuteNonQuery();
            }
            return Ok(new { mensaje = "Tarea actualizada exitosamente" });
        }

        // 4. ELIMINAR (DELETE)
        [HttpDelete("{id}")]
        public IActionResult EliminarTarea(int id)
        {
            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection");
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

    // Clases auxiliares para recibir la información que envímos al probar
    public class TareaNueva
    {
        public string Titulo { get; set; }
    }

    public class TareaActualizada
    {
        public string Estado { get; set; }
    }
}