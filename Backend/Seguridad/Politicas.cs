using System.Security.Claims;

namespace Backend.Seguridad
{
    // Nombres de las políticas de autorización (se configuran en Program.cs)
    public static class Politicas
    {
        // Solo usuarios con un rol marcado como administrador (Roles.is_admin = 1)
        public const string Administrador = "Administrador";

        // Administradores y roles que gestionan tareas (Roles.gestiona_tareas = 1), por ejemplo Líder de Proyecto
        public const string GestionTareas = "GestionTareas";
    }

    // Atajos para leer los datos del usuario que inició sesión (vienen dentro del token JWT)
    public static class UsuarioActual
    {
        public static int IdUsuario(this ClaimsPrincipal usuario)
        {
            return int.TryParse(usuario.FindFirst("id_usuario")?.Value, out int id) ? id : 0;
        }

        public static bool EsAdmin(this ClaimsPrincipal usuario)
        {
            return usuario.HasClaim("es_admin", "true");
        }

        public static bool GestionaTareas(this ClaimsPrincipal usuario)
        {
            return usuario.EsAdmin() || usuario.HasClaim("gestiona_tareas", "true");
        }
    }
}
