using Microsoft.Data.Sqlite;

namespace Backend.Auditoria
{
    // Registro de auditoría: guarda en la tabla Auditoria_Log quién hizo qué, sobre qué registro y cuándo.
    // Lo usan el FiltroAuditoria (para todos los cambios de la API) y el AuthController (inicios de sesión).
    public class ServicioAuditoria
    {
        private const string FormatoFechaHora = "yyyy-MM-dd HH:mm:ss";

        // Tabla, llave primaria y columna con el nombre de cada módulo (controlador) que se audita.
        // Los nombres vienen de esta lista fija, nunca del usuario, así que se pueden poner en el SQL.
        private static readonly Dictionary<string, (string tabla, string llave, string nombre)> Modulos = new()
        {
            ["Tareas"] = ("Tareas", "id_tarea", "titulo"),
            ["Estados"] = ("Estados", "id_estado", "nombre_estado"),
            ["Roles"] = ("Roles", "id_rol", "nombre_rol"),
            ["Usuarios"] = ("Usuarios", "id_usuario", "nombre"),
            ["Proyectos"] = ("Proyectos", "id_proyecto", "nombre")
        };

        private readonly string _cadenaConexion;
        private readonly ILogger<ServicioAuditoria> _logger;

        public ServicioAuditoria(IConfiguration configuracion, ILogger<ServicioAuditoria> logger)
        {
            _cadenaConexion = configuracion.GetConnectionString("DefaultConnection") ?? string.Empty;
            _logger = logger;
        }

        // Se llama una vez al arrancar la API: crea la tabla si no existe y le agrega
        // las columnas de la versión 5 (modulo, id_registro, detalle) si todavía no las tiene.
        // Así la misma API funciona con la base de Visual Studio y con la de Docker sin correr scripts a mano.
        public void PrepararTabla()
        {
            using var conexion = new SqliteConnection(_cadenaConexion);
            conexion.Open();

            Ejecutar(conexion, @"
                CREATE TABLE IF NOT EXISTS Auditoria_Log (
                    id_log INTEGER PRIMARY KEY AUTOINCREMENT,
                    accion VARCHAR(50) NOT NULL,
                    fecha_hora DATETIME DEFAULT CURRENT_TIMESTAMP,
                    id_usuario INTEGER REFERENCES Usuarios(id_usuario) ON DELETE SET NULL
                )");

            var columnas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var comando = conexion.CreateCommand())
            {
                comando.CommandText = "PRAGMA table_info(Auditoria_Log)";
                using var lector = comando.ExecuteReader();
                while (lector.Read()) columnas.Add(lector.GetString(1));
            }

            if (!columnas.Contains("modulo")) Ejecutar(conexion, "ALTER TABLE Auditoria_Log ADD COLUMN modulo VARCHAR(50)");
            if (!columnas.Contains("id_registro")) Ejecutar(conexion, "ALTER TABLE Auditoria_Log ADD COLUMN id_registro INTEGER");
            if (!columnas.Contains("detalle")) Ejecutar(conexion, "ALTER TABLE Auditoria_Log ADD COLUMN detalle TEXT");
        }

        // Guarda un registro. Si algo falla, solo se escribe en el log de la API:
        // la auditoría nunca debe impedir que el usuario complete su acción.
        public void Registrar(int? idUsuario, string accion, string modulo, int? idRegistro, string? detalle)
        {
            try
            {
                using var conexion = new SqliteConnection(_cadenaConexion);
                conexion.Open();
                using var comando = conexion.CreateCommand();
                comando.CommandText = @"
                    INSERT INTO Auditoria_Log (accion, fecha_hora, id_usuario, modulo, id_registro, detalle)
                    VALUES (@accion, @fecha, @usuario, @modulo, @registro, @detalle)";
                comando.Parameters.AddWithValue("@accion", accion);
                comando.Parameters.AddWithValue("@fecha", DateTime.Now.ToString(FormatoFechaHora));
                comando.Parameters.AddWithValue("@usuario", (object?)idUsuario ?? DBNull.Value);
                comando.Parameters.AddWithValue("@modulo", modulo);
                comando.Parameters.AddWithValue("@registro", (object?)idRegistro ?? DBNull.Value);
                comando.Parameters.AddWithValue("@detalle", (object?)Recortar(detalle) ?? DBNull.Value);
                comando.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo guardar el registro de auditoría ({Accion} en {Modulo}).", accion, modulo);
            }
        }

        // Devuelve el nombre o título de un registro (por ejemplo, el título de la tarea 8),
        // o null si el módulo no se audita o el registro no existe.
        public string? ObtenerNombre(string modulo, int id)
        {
            if (!Modulos.TryGetValue(modulo, out var datos)) return null;
            try
            {
                using var conexion = new SqliteConnection(_cadenaConexion);
                conexion.Open();
                using var comando = conexion.CreateCommand();
                comando.CommandText = $"SELECT {datos.nombre} FROM {datos.tabla} WHERE {datos.llave} = @id";
                comando.Parameters.AddWithValue("@id", id);
                return comando.ExecuteScalar() as string;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo leer el nombre del registro {Id} de {Modulo}.", id, modulo);
                return null;
            }
        }

        private static void Ejecutar(SqliteConnection conexion, string sql)
        {
            using var comando = conexion.CreateCommand();
            comando.CommandText = sql;
            comando.ExecuteNonQuery();
        }

        private static string? Recortar(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return null;
            texto = texto.Trim();
            return texto.Length > 300 ? texto[..300] : texto;
        }
    }
}
