using Backend.Seguridad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuracion;
        private readonly ServicioTokens _servicioTokens;

        public AuthController(IConfiguration configuracion, ServicioTokens servicioTokens)
        {
            _configuracion = configuracion;
            _servicioTokens = servicioTokens;
        }

        public class LoginDto
        {
            public required string correo { get; set; }
            public required string contrasena { get; set; }
        }

        // 1. INICIAR SESIÓN (POST /api/Auth/login) - es la única ruta que no pide token
        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult IniciarSesion([FromBody] LoginDto datos)
        {
            if (string.IsNullOrWhiteSpace(datos.correo) || string.IsNullOrWhiteSpace(datos.contrasena))
            {
                return BadRequest(new { mensaje = "Escribe tu correo y tu contraseña." });
            }

            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection") ?? string.Empty;
            UsuarioAutenticado? usuario = null;
            string? hashGuardado = null;

            using (var conexion = new SqliteConnection(cadenaConexion))
            {
                conexion.Open();
                var comando = conexion.CreateCommand();
                comando.CommandText = @"
                    SELECT u.id_usuario, u.nombre, u.correo, u.contrasena_hash,
                           r.nombre_rol, r.is_admin, r.gestiona_tareas
                    FROM Usuarios u
                    LEFT JOIN Roles r ON u.id_rol = r.id_rol
                    WHERE LOWER(u.correo) = LOWER(@correo)";
                comando.Parameters.AddWithValue("@correo", datos.correo.Trim());

                using (var lector = comando.ExecuteReader())
                {
                    if (lector.Read())
                    {
                        usuario = new UsuarioAutenticado
                        {
                            Id = lector.GetInt32(0),
                            Nombre = lector.GetString(1),
                            Correo = lector.GetString(2),
                            NombreRol = lector.IsDBNull(4) ? "Sin rol" : lector.GetString(4),
                            EsAdmin = !lector.IsDBNull(5) && lector.GetInt64(5) == 1,
                            GestionaTareas = !lector.IsDBNull(6) && lector.GetInt64(6) == 1
                        };
                        hashGuardado = lector.IsDBNull(3) ? null : lector.GetString(3);
                    }
                }
            }

            // El mismo mensaje si no existe el correo o si la contraseña está mal:
            // así no se revela qué correos están registrados.
            if (usuario == null || !ServicioContrasenas.Verificar(datos.contrasena, hashGuardado))
            {
                return Unauthorized(new { mensaje = "Correo o contraseña incorrectos." });
            }

            var (token, expira) = _servicioTokens.CrearToken(usuario);

            return Ok(new
            {
                token,
                expira,
                usuario = new
                {
                    id_usuario = usuario.Id,
                    nombre = usuario.Nombre,
                    correo = usuario.Correo,
                    rol = usuario.NombreRol,
                    es_admin = usuario.EsAdmin,
                    gestiona_tareas = usuario.GestionaTareas || usuario.EsAdmin
                }
            });
        }

        // 2. QUIÉN SOY (GET /api/Auth/yo) - devuelve los datos que vienen dentro del token
        [HttpGet("yo")]
        public IActionResult QuienSoy()
        {
            return Ok(new
            {
                id_usuario = User.IdUsuario(),
                nombre = User.FindFirst("nombre")?.Value,
                correo = User.FindFirst("correo")?.Value,
                rol = User.FindFirst("rol")?.Value,
                es_admin = User.EsAdmin(),
                gestiona_tareas = User.GestionaTareas()
            });
        }
    }
}
