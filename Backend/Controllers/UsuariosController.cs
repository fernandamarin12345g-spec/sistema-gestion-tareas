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

        public class UsuarioDto
        {
            public required string nombre { get; set; }
            public required string correo { get; set; }
            public int id_rol { get; set; }
        }

        public class UsuarioResponse
        {
            public int id_usuario { get; set; }
            public string nombre { get; set; } = string.Empty;
            public string correo { get; set; } = string.Empty;
            public int id_rol { get; set; }
            public string nombre_rol { get; set; } = string.Empty;
            public List<object> tareas { get; set; } = new List<object>();
        }

        [HttpGet]
        public IActionResult ObtenerUsuarios()
        {
            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection") ?? string.Empty;
            var diccionarioUsuarios = new Dictionary<int, UsuarioResponse>();

            using (var conexion = new SqliteConnection(cadenaConexion))
            {
                conexion.Open();
                var comando = conexion.CreateCommand();

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
            return Ok(diccionarioUsuarios.Values.ToList());
        }

        [HttpPost]
        public IActionResult CrearUsuario([FromBody] UsuarioDto nuevoUsuario)
        {
            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection") ?? string.Empty;

            try
            {
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
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                return BadRequest(new { mensaje = "El correo electrónico ya se encuentra registrado." });
            }
        }

        // 3. ACTUALIZAR (PUT) 
        [HttpPut("{id}")]
        public IActionResult ActualizarUsuario(int id, [FromBody] UsuarioDto usuarioActualizado)
        {
            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection") ?? string.Empty;

            try
            {
                using (var conexion = new SqliteConnection(cadenaConexion))
                {
                    conexion.Open();

                    // Comando 1: Verificar si el correo pertenece a otro usuario diferente
                    using (var comandoVerificar = conexion.CreateCommand())
                    {
                        comandoVerificar.CommandText = "SELECT COUNT(1) FROM Usuarios WHERE correo = @correo AND id_usuario != @id";
                        comandoVerificar.Parameters.AddWithValue("@correo", usuarioActualizado.correo);
                        comandoVerificar.Parameters.AddWithValue("@id", id);

                        long existeCorreo = (long)(comandoVerificar.ExecuteScalar() ?? 0);

                        if (existeCorreo > 0)
                        {
                            return BadRequest(new { mensaje = "El correo electrónico ya está en uso por otro usuario." });
                        }
                    }

                    // Comando 2: Actualizar datos de forma independiente sobre el mismo ID
                    using (var comandoUpdate = conexion.CreateCommand())
                    {
                        comandoUpdate.CommandText = "UPDATE Usuarios SET nombre = @nombre, correo = @correo, id_rol = @idRol WHERE id_usuario = @id";
                        comandoUpdate.Parameters.AddWithValue("@nombre", usuarioActualizado.nombre);
                        comandoUpdate.Parameters.AddWithValue("@correo", usuarioActualizado.correo);
                        comandoUpdate.Parameters.AddWithValue("@idRol", usuarioActualizado.id_rol);
                        comandoUpdate.Parameters.AddWithValue("@id", id);

                        int filasAfectadas = comandoUpdate.ExecuteNonQuery();
                        if (filasAfectadas == 0)
                        {
                            return NotFound(new { mensaje = "Usuario no encontrado" });
                        }
                    }
                }
                return Ok(new { mensaje = "Usuario actualizado exitosamente" });
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                return BadRequest(new { mensaje = "Error de integridad en la base de datos." });
            }
        }

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