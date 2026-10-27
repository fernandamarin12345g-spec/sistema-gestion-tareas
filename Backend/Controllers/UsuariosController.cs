using System.Net.Mail;
using Backend.Seguridad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

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
            public string? contrasena { get; set; }   // Obligatoria al crear; al editar, solo si se quiere cambiar
        }

        public class UsuarioResponse
        {
            public int id_usuario { get; set; }
            public string nombre { get; set; } = string.Empty;
            public string correo { get; set; } = string.Empty;
            public int? id_rol { get; set; }                      // Puede ser null si el usuario no tiene rol
            public string nombre_rol { get; set; } = "Sin rol";
            public List<object> tareas { get; set; } = new List<object>();
        }

        // Valida los datos antes de tocar la base de datos.
        // Devuelve el mensaje de error, o null si todo está bien.
        private static string? ValidarUsuario(UsuarioDto usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario.nombre))
                return "El nombre es obligatorio.";

            if (usuario.nombre.Trim().Length > 100)
                return "El nombre no puede tener más de 100 caracteres.";

            if (string.IsNullOrWhiteSpace(usuario.correo))
                return "El correo electrónico es obligatorio.";

            string correo = usuario.correo.Trim();
            // MailAddress revisa que el correo tenga un formato válido (usuario@dominio)
            if (correo.Length > 100 || !MailAddress.TryCreate(correo, out var direccion) || direccion.Address != correo)
                return "El correo electrónico no tiene un formato válido.";

            if (usuario.id_rol <= 0)
                return "Debes seleccionar un rol válido.";

            if (!string.IsNullOrEmpty(usuario.contrasena) && usuario.contrasena.Length < 8)
                return "La contraseña debe tener al menos 8 caracteres.";

            return null;
        }

        // 1. LISTAR USUARIOS CON SUS TAREAS (GET)
        [HttpGet]
        public IActionResult ObtenerUsuarios()
        {
            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection") ?? string.Empty;
            var diccionarioUsuarios = new Dictionary<int, UsuarioResponse>();

            using (var conexion = new SqliteConnection(cadenaConexion))
            {
                conexion.Open();
                var comando = conexion.CreateCommand();

                // LEFT JOIN en Roles: así no desaparece un usuario que no tenga rol
                comando.CommandText = @"
                    SELECT u.id_usuario, u.nombre, u.correo, u.id_rol, r.nombre_rol,
                           t.id_tarea, t.titulo, e.nombre_estado
                    FROM Usuarios u
                    LEFT JOIN Roles r ON u.id_rol = r.id_rol
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
                                id_rol = lector.IsDBNull(3) ? null : lector.GetInt32(3),
                                nombre_rol = lector.IsDBNull(4) ? "Sin rol" : lector.GetString(4)
                            };
                        }

                        if (!lector.IsDBNull(5))
                        {
                            diccionarioUsuarios[idUsuario].tareas.Add(new
                            {
                                id_tarea = lector.GetInt32(5),
                                titulo = lector.GetString(6),
                                estado = lector.IsDBNull(7) ? "Sin estado" : lector.GetString(7)
                            });
                        }
                    }
                }
            }
            return Ok(diccionarioUsuarios.Values.ToList());
        }

        // 2. CREAR USUARIO (POST)
        [HttpPost]
        [Authorize(Policy = Politicas.Administrador)]
        public IActionResult CrearUsuario([FromBody] UsuarioDto nuevoUsuario)
        {
            string? error = ValidarUsuario(nuevoUsuario);
            if (error == null && string.IsNullOrEmpty(nuevoUsuario.contrasena))
            {
                error = "La contraseña es obligatoria para crear un usuario.";
            }
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
                    comando.CommandText = @"
                        INSERT INTO Usuarios (nombre, correo, id_rol, contrasena_hash)
                        VALUES (@nombre, @correo, @idRol, @hash);
                        SELECT last_insert_rowid();";
                    comando.Parameters.AddWithValue("@nombre", nuevoUsuario.nombre.Trim());
                    comando.Parameters.AddWithValue("@correo", nuevoUsuario.correo.Trim());
                    comando.Parameters.AddWithValue("@idRol", nuevoUsuario.id_rol);
                    // Se guarda el hash, nunca la contraseña
                    comando.Parameters.AddWithValue("@hash", ServicioContrasenas.GenerarHash(nuevoUsuario.contrasena!));

                    nuevoId = (long)(comando.ExecuteScalar() ?? 0L);
                }
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                // 2067 = se repitió un valor UNIQUE (el correo). Cualquier otro = llave foránea (el rol no existe).
                string mensaje = ex.SqliteExtendedErrorCode == 2067
                    ? "El correo electrónico ya se encuentra registrado."
                    : "El rol seleccionado no existe.";
                return BadRequest(new { mensaje });
            }

            return Created($"/api/Usuarios/{nuevoId}", new { mensaje = "Usuario creado exitosamente", id = nuevoId });
        }

        // 3. ACTUALIZAR USUARIO (PUT)
        [HttpPut("{id}")]
        [Authorize(Policy = Politicas.Administrador)]
        public IActionResult ActualizarUsuario(int id, [FromBody] UsuarioDto usuarioActualizado)
        {
            string? error = ValidarUsuario(usuarioActualizado);
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

                    // Comando 1: verificar si el correo pertenece a otro usuario diferente
                    using (var comandoVerificar = conexion.CreateCommand())
                    {
                        comandoVerificar.CommandText = "SELECT COUNT(1) FROM Usuarios WHERE correo = @correo AND id_usuario != @id";
                        comandoVerificar.Parameters.AddWithValue("@correo", usuarioActualizado.correo.Trim());
                        comandoVerificar.Parameters.AddWithValue("@id", id);

                        long existeCorreo = (long)(comandoVerificar.ExecuteScalar() ?? 0L);
                        if (existeCorreo > 0)
                        {
                            return BadRequest(new { mensaje = "El correo electrónico ya está en uso por otro usuario." });
                        }
                    }

                    // Comando 2: actualizar los datos del usuario
                    using (var comandoUpdate = conexion.CreateCommand())
                    {
                        // Si no se envía contraseña, se conserva la que ya tenía (COALESCE)
                        comandoUpdate.CommandText = "UPDATE Usuarios SET nombre = @nombre, correo = @correo, id_rol = @idRol, contrasena_hash = COALESCE(@hash, contrasena_hash) WHERE id_usuario = @id";
                        comandoUpdate.Parameters.AddWithValue("@nombre", usuarioActualizado.nombre.Trim());
                        comandoUpdate.Parameters.AddWithValue("@correo", usuarioActualizado.correo.Trim());
                        comandoUpdate.Parameters.AddWithValue("@idRol", usuarioActualizado.id_rol);
                        comandoUpdate.Parameters.AddWithValue("@hash", string.IsNullOrEmpty(usuarioActualizado.contrasena)
                            ? DBNull.Value
                            : (object)ServicioContrasenas.GenerarHash(usuarioActualizado.contrasena));
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
                return BadRequest(new { mensaje = "El rol seleccionado no existe." });
            }
        }

        // 4. ELIMINAR USUARIO (DELETE)
        [HttpDelete("{id}")]
        [Authorize(Policy = Politicas.Administrador)]
        public IActionResult EliminarUsuario(int id)
        {
            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection") ?? string.Empty;

            using (var conexion = new SqliteConnection(cadenaConexion))
            {
                conexion.Open();
                var comando = conexion.CreateCommand();
                // Sus tareas no se borran: quedan "Sin asignar" (ON DELETE SET NULL)
                comando.CommandText = "DELETE FROM Usuarios WHERE id_usuario = @id";
                comando.Parameters.AddWithValue("@id", id);

                int filasAfectadas = comando.ExecuteNonQuery();
                if (filasAfectadas == 0)
                {
                    return NotFound(new { mensaje = "Usuario no encontrado" });
                }
            }
            return Ok(new { mensaje = "Usuario eliminado exitosamente" });
        }
    }
}