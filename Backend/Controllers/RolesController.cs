using Backend.Seguridad;
using Microsoft.AspNetCore.Authorization;
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
            public bool? gestiona_tareas { get; set; }   // true = puede crear y editar tareas (ej. Líder de Proyecto)
        }

        // Valida el nombre antes de tocar la base de datos. Devuelve el mensaje de error o null.
        private static string? ValidarNombre(string? nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return "El nombre del rol es obligatorio.";

            if (nombre.Trim().Length > 50)
                return "El nombre del rol no puede tener más de 50 caracteres.";

            return null;
        }

        // La tabla Roles NO tiene UNIQUE en nombre_rol, así que los duplicados se revisan aquí,
        // sin importar mayúsculas/minúsculas.
        private static bool ExisteNombre(SqliteConnection conexion, string nombre, int idExcluir)
        {
            using (var comando = conexion.CreateCommand())
            {
                comando.CommandText = "SELECT COUNT(1) FROM Roles WHERE LOWER(nombre_rol) = LOWER(@nombre) AND id_rol != @id";
                comando.Parameters.AddWithValue("@nombre", nombre);
                comando.Parameters.AddWithValue("@id", idExcluir);
                return (long)(comando.ExecuteScalar() ?? 0L) > 0;
            }
        }

        // 1. LISTAR ROLES (GET)
        [HttpGet]
        public IActionResult ObtenerRoles()
        {
            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection") ?? string.Empty;
            var listaRoles = new List<object>();

            using (var conexion = new SqliteConnection(cadenaConexion))
            {
                conexion.Open();
                var comando = conexion.CreateCommand();
                comando.CommandText = "SELECT id_rol, nombre_rol, is_admin, gestiona_tareas FROM Roles";

                using (var lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        listaRoles.Add(new
                        {
                            id_rol = lector.GetInt32(0),
                            nombre_rol = lector.GetString(1),
                            is_admin = lector.GetBoolean(2),
                            gestiona_tareas = lector.GetBoolean(3)
                        });
                    }
                }
            }
            return Ok(listaRoles);
        }

        // 2. CREAR ROL (POST)
        [HttpPost]
        [Authorize(Policy = Politicas.Administrador)]
        public IActionResult CrearRol([FromBody] RolNuevo nuevoRol)
        {
            string? error = ValidarNombre(nuevoRol.nombre_rol);
            if (error != null)
            {
                return BadRequest(new { mensaje = error });
            }

            string nombre = nuevoRol.nombre_rol.Trim();
            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection") ?? string.Empty;
            long nuevoId;

            using (var conexion = new SqliteConnection(cadenaConexion))
            {
                conexion.Open();

                if (ExisteNombre(conexion, nombre, 0))
                {
                    return BadRequest(new { mensaje = "Ya existe un rol con ese nombre." });
                }

                var comando = conexion.CreateCommand();
                comando.CommandText = @"
                    INSERT INTO Roles (nombre_rol, is_admin, gestiona_tareas) VALUES (@nombre, @isAdmin, @gestionaTareas);
                    SELECT last_insert_rowid();";
                comando.Parameters.AddWithValue("@nombre", nombre);
                comando.Parameters.AddWithValue("@isAdmin", nuevoRol.is_admin ? 1 : 0);
                comando.Parameters.AddWithValue("@gestionaTareas", nuevoRol.gestiona_tareas == true ? 1 : 0);
                nuevoId = (long)(comando.ExecuteScalar() ?? 0L);
            }

            return Created($"/api/Roles/{nuevoId}", new { mensaje = "Rol creado exitosamente", id = nuevoId });
        }

        // 3. ACTUALIZAR ROL (PUT)
        [HttpPut("{id}")]
        [Authorize(Policy = Politicas.Administrador)]
        public IActionResult ActualizarRol(int id, [FromBody] RolNuevo rolActualizado)
        {
            string? error = ValidarNombre(rolActualizado.nombre_rol);
            if (error != null)
            {
                return BadRequest(new { mensaje = error });
            }

            string nombre = rolActualizado.nombre_rol.Trim();
            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection") ?? string.Empty;

            using (var conexion = new SqliteConnection(cadenaConexion))
            {
                conexion.Open();

                if (ExisteNombre(conexion, nombre, id))
                {
                    return BadRequest(new { mensaje = "Ya existe otro rol con ese nombre." });
                }

                var comando = conexion.CreateCommand();
                // Si no se envía gestiona_tareas, se conserva el valor que ya tenía (COALESCE)
                comando.CommandText = "UPDATE Roles SET nombre_rol = @nombre, is_admin = @isAdmin, gestiona_tareas = COALESCE(@gestionaTareas, gestiona_tareas) WHERE id_rol = @id";
                comando.Parameters.AddWithValue("@nombre", nombre);
                comando.Parameters.AddWithValue("@isAdmin", rolActualizado.is_admin ? 1 : 0);
                comando.Parameters.AddWithValue("@gestionaTareas", rolActualizado.gestiona_tareas.HasValue ? (object)(rolActualizado.gestiona_tareas.Value ? 1 : 0) : DBNull.Value);
                comando.Parameters.AddWithValue("@id", id);

                int filasAfectadas = comando.ExecuteNonQuery();
                if (filasAfectadas == 0)
                {
                    return NotFound(new { mensaje = "Rol no encontrado" });
                }
            }
            return Ok(new { mensaje = "Rol actualizado exitosamente" });
        }

        // 4. ELIMINAR ROL (DELETE)
        [HttpDelete("{id}")]
        [Authorize(Policy = Politicas.Administrador)]
        public IActionResult EliminarRol(int id)
        {
            string cadenaConexion = _configuracion.GetConnectionString("DefaultConnection") ?? string.Empty;

            try
            {
                using (var conexion = new SqliteConnection(cadenaConexion))
                {
                    conexion.Open();
                    var comando = conexion.CreateCommand();
                    comando.CommandText = "DELETE FROM Roles WHERE id_rol = @id";
                    comando.Parameters.AddWithValue("@id", id);

                    int filasAfectadas = comando.ExecuteNonQuery();
                    if (filasAfectadas == 0)
                    {
                        return NotFound(new { mensaje = "Rol no encontrado" });
                    }
                }
                return Ok(new { mensaje = "Rol eliminado exitosamente" });
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                // La llave foránea impide borrar un rol que todavía tienen algunos usuarios
                return BadRequest(new { mensaje = "No se puede eliminar este rol porque hay usuarios asignados a él." });
            }
        }
    }
}