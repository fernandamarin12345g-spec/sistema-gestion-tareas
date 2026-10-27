import 'package:flutter_test/flutter_test.dart';
import 'package:movil/main.dart';
import 'package:movil/models/sesion.dart';
import 'package:movil/models/tarea.dart';
import 'package:movil/services/tarea_service.dart';

/// Sesión de prueba (no se llama a la API real).
Sesion sesionDePrueba({bool gestionaTareas = true}) => Sesion(
      token: 'token-de-prueba',
      expira: DateTime.now().add(const Duration(hours: 1)),
      idUsuario: 2,
      nombre: 'Fernanda Marin',
      rol: gestionaTareas ? 'Líder de Proyecto' : 'Desarrollador',
      gestionaTareas: gestionaTareas,
    );

/// Servicio falso con sesión iniciada y tareas fijas.
class ServicioConTareas extends TareaService {
  final bool gestiona;

  ServicioConTareas({this.gestiona = true});

  @override
  Future<Sesion?> cargarSesionGuardada() async {
    sesion = sesionDePrueba(gestionaTareas: gestiona);
    return sesion;
  }

  @override
  Future<ResultadoTareas> obtenerTareas() async {
    return const ResultadoTareas(tareas: [
      Tarea(id: 1, titulo: 'Tarea de prueba', responsable: 'Fernanda Marin', estado: 'Pendiente', prioridad: 'Alta'),
      Tarea(id: 2, titulo: 'Tarea vencida', estado: 'Pendiente', fechaLimite: '2026-10-20', atrasada: true),
    ]);
  }
}

/// Servicio falso con sesión iniciada pero con el backend apagado.
class ServicioSinConexion extends TareaService {
  @override
  Future<Sesion?> cargarSesionGuardada() async {
    sesion = sesionDePrueba();
    return sesion;
  }

  @override
  Future<ResultadoTareas> obtenerTareas() async {
    throw ApiException('No se pudo conectar con el servidor. Revisa que el backend esté corriendo.');
  }
}

/// Servicio falso sin sesión guardada.
class ServicioSinSesion extends TareaService {
  @override
  Future<Sesion?> cargarSesionGuardada() async => null;
}

void main() {
  testWidgets('Sin sesión guardada, la app abre en el inicio de sesión', (WidgetTester tester) async {
    await tester.pumpWidget(AppGestionTareas(servicio: ServicioSinSesion()));
    await tester.pumpAndSettle();

    expect(find.text('Ingresar'), findsOneWidget);
    expect(find.text('Mis tareas'), findsNothing);
  });

  testWidgets('Con sesión, la app muestra las tareas que entrega la API', (WidgetTester tester) async {
    await tester.pumpWidget(AppGestionTareas(servicio: ServicioConTareas()));
    await tester.pumpAndSettle();

    expect(find.text('Mis tareas'), findsOneWidget);
    expect(find.text('Tarea de prueba'), findsOneWidget);
    expect(find.text('Alta'), findsOneWidget);
    expect(find.text('Atrasada'), findsOneWidget);
    expect(find.text('Nueva tarea'), findsOneWidget);
  });

  testWidgets('Un Desarrollador no ve el botón Nueva tarea', (WidgetTester tester) async {
    await tester.pumpWidget(AppGestionTareas(servicio: ServicioConTareas(gestiona: false)));
    await tester.pumpAndSettle();

    expect(find.text('Tarea de prueba'), findsOneWidget);
    expect(find.text('Nueva tarea'), findsNothing);
  });

  testWidgets('Sin conexión muestra el error y el botón Reintentar', (WidgetTester tester) async {
    await tester.pumpWidget(AppGestionTareas(servicio: ServicioSinConexion()));
    await tester.pumpAndSettle();

    expect(find.textContaining('No se pudo conectar'), findsOneWidget);
    expect(find.text('Reintentar'), findsOneWidget);
  });
}
