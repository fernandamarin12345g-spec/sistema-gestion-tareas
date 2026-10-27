// Sesión del usuario que inició sesión: el token JWT y sus datos básicos.
class Sesion {
  final String token;
  final DateTime expira;
  final int idUsuario;
  final String nombre;
  final String rol;
  final bool esAdmin;
  final bool gestionaTareas;

  const Sesion({
    required this.token,
    required this.expira,
    required this.idUsuario,
    required this.nombre,
    this.rol = 'Sin rol',
    this.esAdmin = false,
    this.gestionaTareas = false,
  });

  /// El token sirve solo hasta su fecha de vencimiento.
  bool get estaVigente => DateTime.now().toUtc().isBefore(expira.toUtc());

  /// Convierte la respuesta de POST /api/Auth/login en una Sesion.
  factory Sesion.desdeLogin(Map<String, dynamic> json) {
    final usuario = json['usuario'] as Map<String, dynamic>;
    return Sesion(
      token: json['token'] as String,
      expira: DateTime.parse(json['expira'] as String),
      idUsuario: usuario['id_usuario'] as int,
      nombre: usuario['nombre'] as String,
      rol: (usuario['rol'] as String?) ?? 'Sin rol',
      esAdmin: (usuario['es_admin'] as bool?) ?? false,
      gestionaTareas: (usuario['gestiona_tareas'] as bool?) ?? false,
    );
  }

  /// Para guardarla en el teléfono y recuperarla al volver a abrir la app.
  Map<String, dynamic> toJson() => {
        'token': token,
        'expira': expira.toIso8601String(),
        'usuario': {
          'id_usuario': idUsuario,
          'nombre': nombre,
          'rol': rol,
          'es_admin': esAdmin,
          'gestiona_tareas': gestionaTareas,
        },
      };
}
