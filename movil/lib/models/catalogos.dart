// Modelos pequeños para llenar las listas desplegables del formulario.

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
  final String estado;

  const Proyecto({required this.id, required this.nombre, this.estado = 'Activo'});

  factory Proyecto.fromJson(Map<String, dynamic> json) {
    return Proyecto(
      id: json['id_proyecto'] as int,
      nombre: json['nombre'] as String,
      estado: (json['estado'] as String?) ?? 'Activo',
    );
  }
}

class Usuario {
  final int id;
  final String nombre;

  const Usuario({required this.id, required this.nombre});

  factory Usuario.fromJson(Map<String, dynamic> json) {
    return Usuario(
      id: json['id_usuario'] as int,
      nombre: json['nombre'] as String,
    );
  }
}
