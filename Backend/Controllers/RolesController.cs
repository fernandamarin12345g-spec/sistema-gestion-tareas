using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IConfiguration _configuracion;

        public RolesController(IConfiguration configuracion)
        {
            _configuracion = configuracion;
        }

        public class RolNuevo
        {
            public required string nombre_rol { get; set; }
            public bool is_admin { get; set; }
        }

        // 1. LEER LOS ROLES (GET)
        [HttpGet]
        public IActionResult ObtenerRoles()
        {
            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection") ?? string.Empty;
            var listaRoles = new List<object>();

            using (var conexion = new SqliteConnection(cadenaConexion))
            {
                conexion.Open();
                var comando = conexion.CreateCommand();
                comando.CommandText = "SELECT id_rol, nombre_rol, is_admin FROM Roles";

                using (var lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        listaRoles.Add(new
                        {
                            id_rol = lector.GetInt32(0),
                            nombre_rol = lector.GetString(1),
                            // Convertimos el 1 o 0 de SQLite a true o false en C#
                            is_admin = lector.GetBoolean(2)
                        });
                    }
                }
            }
            return Ok(listaRoles);
        }

        // 2. CREAR UN ROL (POST)
        [HttpPost]
        public IActionResult CrearRol([FromBody] RolNuevo nuevoRol)
        {
            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection") ?? string.Empty;

            using (var conexion = new SqliteConnection(cadenaConexion))
            {
                conexion.Open();
                var comando = conexion.CreateCommand();
                comando.CommandText = "INSERT INTO Roles (nombre_rol, is_admin) VALUES (@nombre, @isAdmin)";
                comando.Parameters.AddWithValue("@nombre", nuevoRol.nombre_rol);
                // Si es admin guardamos 1, si no guardamos 0
                comando.Parameters.AddWithValue("@isAdmin", nuevoRol.is_admin ? 1 : 0);
                comando.ExecuteNonQuery();
            }
            return Ok(new { mensaje = "Rol creado exitosamente" });
        }

        // 3. ELIMINAR UN ROL (DELETE)
        [HttpDelete("{id}")]
        public IActionResult EliminarRol(int id)
        {
            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection") ?? string.Empty;

            using (var conexion = new SqliteConnection(cadenaConexion))
            {
                conexion.Open();
                var comando = conexion.CreateCommand();
                comando.CommandText = "DELETE FROM Roles WHERE id_rol = @id";
                comando.Parameters.AddWithValue("@id", id);
                comando.ExecuteNonQuery();
            }
            return Ok(new { mensaje = "Rol eliminado exitosamente" });
        }
        // 4. ACTUALIZAR UN ROL (PUT)
        [HttpPut("{id}")]
        public IActionResult ActualizarRol(int id, [FromBody] RolNuevo rolActualizado)
        {
            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection") ?? string.Empty;

            using (var conexion = new SqliteConnection(cadenaConexion))
            {
                conexion.Open();
                var comando = conexion.CreateCommand();
                comando.CommandText = "UPDATE Roles SET nombre_rol = @nombre, is_admin = @isAdmin WHERE id_rol = @id";
                comando.Parameters.AddWithValue("@nombre", rolActualizado.nombre_rol);
                comando.Parameters.AddWithValue("@isAdmin", rolActualizado.is_admin ? 1 : 0);
                comando.Parameters.AddWithValue("@id", id);

                int filasAfectadas = comando.ExecuteNonQuery();
                if (filasAfectadas == 0)
                {
                    return NotFound(new { mensaje = "Rol no encontrado" });
                }
            }
            return Ok(new { mensaje = "Rol actualizado exitosamente" });
        }
    }
}