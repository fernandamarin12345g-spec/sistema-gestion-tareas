/// Una tarea tal como la devuelve la API (GET /api/Tareas).
class Tarea {
  final int id;
  final String titulo;
  final String? descripcion;
  final int? idUsuario;
  final String responsable;
  final int? idEstado;
  final String estado;
  final bool esFinal;
  final int? idProyecto;
  final String proyecto;
  final String prioridad;
  final String? fechaCreacion;
  final String? fechaLimite;
  final String? fechaFinalizacion;
  final String? fechaActualizacion;
  final bool atrasada;

  const Tarea({
    required this.id,
    required this.titulo,
    this.descripcion,
    this.idUsuario,
    this.responsable = 'Sin asignar',
    this.idEstado,
    this.estado = 'Sin estado',
    this.esFinal = false,
    this.idProyecto,
    this.proyecto = 'Sin proyecto',
    this.prioridad = 'Media',
    this.fechaCreacion,
    this.fechaLimite,
    this.fechaFinalizacion,
    this.fechaActualizacion,
    this.atrasada = false,
  });

  /// Convierte el JSON de la API en un objeto Tarea.
  /// Los nombres de las llaves son los mismos que usa el backend.
  factory Tarea.fromJson(Map<String, dynamic> json) {
    return Tarea(
      id: json['id'] as int,
      titulo: json['titulo'] as String,
      descripcion: json['descripcion'] as String?,
      idUsuario: json['id_usuario'] as int?,
      responsable: (json['usuario_asignado'] as String?) ?? 'Sin asignar',
      idEstado: json['id_estado'] as int?,
      estado: (json['estado'] as String?) ?? 'Sin estado',
      esFinal: (json['es_final'] as bool?) ?? false,
      idProyecto: json['id_proyecto'] as int?,
      proyecto: (json['proyecto'] as String?) ?? 'Sin proyecto',
      prioridad: (json['prioridad'] as String?) ?? 'Media',
      fechaCreacion: json['fecha_creacion'] as String?,
      fechaLimite: json['fecha_limite'] as String?,
      fechaFinalizacion: json['fecha_finalizacion'] as String?,
      fechaActualizacion: json['fecha_actualizacion'] as String?,
      atrasada: (json['atrasada'] as bool?) ?? false,
    );
  }
}
