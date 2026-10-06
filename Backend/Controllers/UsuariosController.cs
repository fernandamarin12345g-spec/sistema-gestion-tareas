using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Linq;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IConfiguration _configuracion;

        public UsuariosController(IConfiguration configuracion)
        {
            _configuracion = configuracion;
        }

        // Clase para recibir datos al crear/editar
        public class UsuarioDto
        {
            public required string nombre { get; set; }
            public required string correo { get; set; }
            public int id_rol { get; set; }
        }

        // Nueva clase para estructurar la respuesta con su lista de tareas
        public class UsuarioResponse
        {
            public int id_usuario { get; set; }
            public string nombre { get; set; } = string.Empty;
            public string correo { get; set; } = string.Empty;
            public int id_rol { get; set; }
            public string nombre_rol { get; set; } = string.Empty;

            // Aquí guardaremos todas las tareas asociadas a esta persona
            public List<object> tareas { get; set; } = new List<object>();
        }

        // 1. LEER (GET) - Ahora agrupando tareas por usuario
        [HttpGet]
        public IActionResult ObtenerUsuarios()
        {
            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection") ?? string.Empty;

            // Usamos un diccionario para agrupar las filas de SQLite en objetos de usuario
            var diccionarioUsuarios = new Dictionary<int, UsuarioResponse>();

            using (var conexion = new SqliteConnection(cadenaConexion))
            {
                conexion.Open();
                var comando = conexion.CreateCommand();

                // Usamos LEFT JOIN en Tareas y Estados para que los usuarios sin tareas también aparezcan
                comando.CommandText = @"
                    SELECT u.id_usuario, u.nombre, u.correo, u.id_rol, r.nombre_rol, 
                           t.id_tarea, t.titulo, e.nombre_estado 
                    FROM Usuarios u 
                    JOIN Roles r ON u.id_rol = r.id_rol
                    LEFT JOIN Tareas t ON u.id_usuario = t.id_usuario_responsable
                    LEFT JOIN Estados e ON t.id_estado = e.id_estado";

                using (var lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        int idUsuario = lector.GetInt32(0);

                        // Si es la primera vez que leemos a este usuario en el ciclo, lo creamos
                        if (!diccionarioUsuarios.ContainsKey(idUsuario))
                        {
                            diccionarioUsuarios[idUsuario] = new UsuarioResponse
                            {
                                id_usuario = idUsuario,
                                nombre = lector.GetString(1),
                                correo = lector.GetString(2),
                                id_rol = lector.GetInt32(3),
                                nombre_rol = lector.GetString(4)
                            };
                        }

                        // Verificamos si la fila trae una tarea (columna 5 no es nula)
                        if (!lector.IsDBNull(5))
                        {
                            diccionarioUsuarios[idUsuario].tareas.Add(new
                            {
                                id_tarea = lector.GetInt32(5),
                                titulo = lector.GetString(6),
                                estado = lector.GetString(7)
                            });
                        }
                    }
                }
            }
            // Retornamos solo la lista de usuarios ya empaquetada con sus tareas adentro
            return Ok(diccionarioUsuarios.Values.ToList());
        }

        // 2. CREAR (POST)
        [HttpPost]
        public IActionResult CrearUsuario([FromBody] UsuarioDto nuevoUsuario)
        {
            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection") ?? string.Empty;

            using (var conexion = new SqliteConnection(cadenaConexion))
            {
                conexion.Open();
                var comando = conexion.CreateCommand();
                comando.CommandText = "INSERT INTO Usuarios (nombre, correo, id_rol) VALUES (@nombre, @correo, @idRol)";
                comando.Parameters.AddWithValue("@nombre", nuevoUsuario.nombre);
                comando.Parameters.AddWithValue("@correo", nuevoUsuario.correo);
                comando.Parameters.AddWithValue("@idRol", nuevoUsuario.id_rol);
                comando.ExecuteNonQuery();
            }
            return Ok(new { mensaje = "Usuario creado exitosamente" });
        }

        // 3. ACTUALIZAR (PUT)
        [HttpPut("{id}")]
        public IActionResult ActualizarUsuario(int id, [FromBody] UsuarioDto usuarioActualizado)
        {
            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection") ?? string.Empty;

            using (var conexion = new SqliteConnection(cadenaConexion))
            {
                conexion.Open();
                var comando = conexion.CreateCommand();
                comando.CommandText = "UPDATE Usuarios SET nombre = @nombre, correo = @correo, id_rol = @idRol WHERE id_usuario = @id";
                comando.Parameters.AddWithValue("@nombre", usuarioActualizado.nombre);
                comando.Parameters.AddWithValue("@correo", usuarioActualizado.correo);
                comando.Parameters.AddWithValue("@idRol", usuarioActualizado.id_rol);
                comando.Parameters.AddWithValue("@id", id);

                int filasAfectadas = comando.ExecuteNonQuery();
                if (filasAfectadas == 0)
                {
                    return NotFound(new { mensaje = "Usuario no encontrado" });
                }
            }
            return Ok(new { mensaje = "Usuario actualizado exitosamente" });
        }

        // 4. ELIMINAR (DELETE)
        [HttpDelete("{id}")]
        public IActionResult EliminarUsuario(int id)
        {
            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection") ?? string.Empty;

            using (var conexion = new SqliteConnection(cadenaConexion))
            {
                conexion.Open();
                var comando = conexion.CreateCommand();
                comando.CommandText = "DELETE FROM Usuarios WHERE id_usuario = @id";
                comando.Parameters.AddWithValue("@id", id);
                comando.ExecuteNonQuery();
            }
            return Ok(new { mensaje = "Usuario eliminado exitosamente" });
        }
    }
}