import '../models/tarea.dart';

// Por ahora los datos viven en memoria.
// luego este archivo pedirá las tareas a la API con el paquete http.
class TareaService {
  final List<Tarea> _tareas = [
    const Tarea(id: 1, titulo: 'Diseñar esquema de base de datos', responsable: 'Fernanda Marin', estado: 'En revisión'),
    const Tarea(id: 2, titulo: 'Diseñar backend', responsable: 'Luis Abreu Acevedo', estado: 'Terminada'),
    const Tarea(id: 3, titulo: 'Crear app móvil', responsable: 'Fernanda Marin', estado: 'En progreso'),
  ];

  List<Tarea> obtenerTareas() => List.unmodifiable(_tareas);

  void agregarTarea(String titulo, String responsable) {
    final nuevoId = _tareas.isEmpty ? 1 : _tareas.last.id + 1;
    _tareas.add(Tarea(id: nuevoId, titulo: titulo, responsable: responsable, estado: 'Pendiente'));
  }
}