// Modelos de los maestros (Estados, Proyectos, Usuarios y Roles) tal como los devuelve la API.

class Estado {
  final int id;
  final String nombre;
  final bool esFinal;

  const Estado({required this.id, required this.nombre, this.esFinal = false});

  factory Estado.fromJson(Map<String, dynamic> json) {
    return Estado(
      id: json['id_estado'] as int,
      nombre: json['nombre_estado'] as String,
      esFinal: (json['es_final'] as bool?) ?? false,
    );
  }
}

class Proyecto {
  final int id;
  final String nombre;
  final String? descripcion;
  final String estado;
  final int totalTareas;
  final int tareasAbiertas;

  const Proyecto({
    required this.id,
    required this.nombre,
    this.descripcion,
    this.estado = 'Activo',
    this.totalTareas = 0,
    this.tareasAbiertas = 0,
  });

  factory Proyecto.fromJson(Map<String, dynamic> json) {
    return Proyecto(
      id: json['id_proyecto'] as int,
      nombre: json['nombre'] as String,
      descripcion: json['descripcion'] as String?,
      estado: (json['estado'] as String?) ?? 'Activo',
      totalTareas: (json['total_tareas'] as int?) ?? 0,
      tareasAbiertas: (json['tareas_abiertas'] as int?) ?? 0,
    );
  }
}

/// Tarea asignada a un usuario (viene dentro de GET /api/Usuarios).
class TareaDeUsuario {
  final int id;
  final String titulo;
  final String estado;

  const TareaDeUsuario({required this.id, required this.titulo, this.estado = 'Sin estado'});

  factory TareaDeUsuario.fromJson(Map<String, dynamic> json) {
    return TareaDeUsuario(
      id: json['id_tarea'] as int,
      titulo: json['titulo'] as String,
      estado: (json['estado'] as String?) ?? 'Sin estado',
    );
  }
}

class Usuario {
  final int id;
  final String nombre;
  final String correo;
  final int? idRol;
  final String rol;
  final List<TareaDeUsuario> tareas;

  const Usuario({
    required this.id,
    required this.nombre,
    this.correo = '',
    this.idRol,
    this.rol = 'Sin rol',
    this.tareas = const [],
  });

  factory Usuario.fromJson(Map<String, dynamic> json) {
    return Usuario(
      id: json['id_usuario'] as int,
      nombre: json['nombre'] as String,
      correo: (json['correo'] as String?) ?? '',
      idRol: json['id_rol'] as int?,
      rol: (json['nombre_rol'] as String?) ?? 'Sin rol',
      tareas: ((json['tareas'] as List?) ?? const [])
          .map((t) => TareaDeUsuario.fromJson(t as Map<String, dynamic>))
          .toList(),
    );
  }
}

class Rol {
  final int id;
  final String nombre;
  final bool esAdmin;
  final bool gestionaTareas;

  const Rol({required this.id, required this.nombre, this.esAdmin = false, this.gestionaTareas = false});

  factory Rol.fromJson(Map<String, dynamic> json) {
    return Rol(
      id: json['id_rol'] as int,
      nombre: json['nombre_rol'] as String,
      esAdmin: (json['is_admin'] as bool?) ?? false,
      gestionaTareas: (json['gestiona_tareas'] as bool?) ?? false,
    );
  }
}
