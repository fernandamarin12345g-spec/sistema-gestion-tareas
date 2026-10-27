import 'package:flutter_test/flutter_test.dart';
import 'package:movil/main.dart';
import 'package:movil/models/tarea.dart';
import 'package:movil/services/tarea_service.dart';

/// Servicio falso: entrega datos fijos sin llamar a la API real.
class ServicioConTareas extends TareaService {
  @override
  Future<ResultadoTareas> obtenerTareas() async {
    return const ResultadoTareas(tareas: [
      Tarea(id: 1, titulo: 'Tarea de prueba', responsable: 'Fernanda Marin', estado: 'Pendiente', prioridad: 'Alta'),
      Tarea(id: 2, titulo: 'Tarea vencida', estado: 'Pendiente', fechaLimite: '2026-10-20', atrasada: true),
    ]);
  }
}

/// Servicio falso que simula que el backend está apagado.
class ServicioSinConexion extends TareaService {
  @override
  Future<ResultadoTareas> obtenerTareas() async {
    throw ApiException('No se pudo conectar con el servidor. Revisa que el backend esté corriendo.');
  }
}

void main() {
  testWidgets('La app muestra las tareas que entrega la API', (WidgetTester tester) async {
    await tester.pumpWidget(AppGestionTareas(servicio: ServicioConTareas()));
    await tester.pumpAndSettle();

    expect(find.text('Mis tareas'), findsOneWidget);
    expect(find.text('Tarea de prueba'), findsOneWidget);
    expect(find.text('Alta'), findsOneWidget);
    expect(find.text('Atrasada'), findsOneWidget);
  });

  testWidgets('Sin conexión muestra el error y el botón Reintentar', (WidgetTester tester) async {
    await tester.pumpWidget(AppGestionTareas(servicio: ServicioSinConexion()));
    await tester.pumpAndSettle();

    expect(find.textContaining('No se pudo conectar'), findsOneWidget);
    expect(find.text('Reintentar'), findsOneWidget);
  });
}
