class Tarea {
  final int id;
  final String titulo;
  final String responsable;
  final String estado;

  const Tarea({
    required this.id,
    required this.titulo,
    required this.responsable,
    required this.estado,
  });

  // Convierte el JSON de GET /api/Tareas en un objeto Tarea.
  factory Tarea.fromJson(Map<String, dynamic> json) {
    return Tarea(
      id: json['id'] as int,
      titulo: json['titulo'] as String,
      responsable: (json['usuario_asignado'] as String?) ?? 'Sin asignar',
      estado: (json['estado'] as String?) ?? 'Sin estado',
    );
  }
}