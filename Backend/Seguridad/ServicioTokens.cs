using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Seguridad
{
    // Datos del usuario que se guardan dentro del token
    public class UsuarioAutenticado
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string NombreRol { get; set; } = "Sin rol";
        public bool EsAdmin { get; set; }
        public bool GestionaTareas { get; set; }
    }

    // Crea los tokens JWT que la web y la app móvil envían en cada petición
    // (encabezado  Authorization: Bearer <token>).
    public class ServicioTokens
    {
        private readonly IConfiguration _configuracion;

        public ServicioTokens(IConfiguration configuracion)
        {
            _configuracion = configuracion;
        }

        public (string token, DateTime expira) CrearToken(UsuarioAutenticado usuario)
        {
            string clave = _configuracion["Jwt:Clave"]
                ?? throw new InvalidOperationException("Falta la configuración Jwt:Clave.");
            int minutos = _configuracion.GetValue<int?>("Jwt:MinutosExpiracion") ?? 480;
            DateTime expira = DateTime.UtcNow.AddMinutes(minutos);

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = _configuracion["Jwt:Emisor"],
                Audience = _configuracion["Jwt:Audiencia"],
                Expires = expira,
                // "Claims": los datos del usuario que viajan firmados dentro del token
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("id_usuario", usuario.Id.ToString()),
                    new Claim("nombre", usuario.Nombre),
                    new Claim("correo", usuario.Correo),
                    new Claim("rol", usuario.NombreRol),
                    new Claim("es_admin", usuario.EsAdmin ? "true" : "false"),
                    new Claim("gestiona_tareas", usuario.GestionaTareas ? "true" : "false"),
                }),
                // Firma con la clave secreta: si alguien modifica el token, la firma deja de coincidir
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clave)),
                    SecurityAlgorithms.HmacSha256)
            };

            string token = new JsonWebTokenHandler().CreateToken(descriptor);
            return (token, expira);
        }
    }
}
