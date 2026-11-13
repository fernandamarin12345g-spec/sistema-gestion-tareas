import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:movil/main.dart';
import 'package:movil/models/catalogos.dart';
import 'package:movil/models/sesion.dart';
import 'package:movil/models/tarea.dart';
import 'package:movil/services/tarea_service.dart';

/// Sesión de prueba (no se llama a la API real).
Sesion sesionDePrueba({bool gestionaTareas = true, bool esAdmin = false}) => Sesion(
      token: 'token-de-prueba',
      expira: DateTime.now().add(const Duration(hours: 1)),
      idUsuario: 2,
      nombre: 'Fernanda Marin',
      rol: esAdmin ? 'Administrador' : (gestionaTareas ? 'Líder de Proyecto' : 'Desarrollador'),
      esAdmin: esAdmin,
      gestionaTareas: gestionaTareas || esAdmin,
    );

/// Servicio falso con sesión iniciada y tareas fijas.
class ServicioConTareas extends TareaService {
  final bool gestiona;
  final bool admin;

  ServicioConTareas({this.gestiona = true, this.admin = false});

  @override
  Future<Sesion?> cargarSesionGuardada() async {
    sesion = sesionDePrueba(gestionaTareas: gestiona, esAdmin: admin);
    return sesion;
  }

  @override
  Future<List<Estado>> obtenerEstados() async {
    return const [Estado(id: 1, nombre: 'Pendiente'), Estado(id: 2, nombre: 'Terminada', esFinal: true)];
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

  testWidgets('El Administrador ve todas las secciones en el menú lateral', (WidgetTester tester) async {
    await tester.pumpWidget(AppGestionTareas(servicio: ServicioConTareas(admin: true)));
    await tester.pumpAndSettle();

    tester.state<ScaffoldState>(find.byType(Scaffold)).openDrawer();
    await tester.pumpAndSettle();

    for (final seccion in ['Inicio', 'Tareas', 'Proyectos', 'Estados', 'Roles', 'Usuarios']) {
      expect(find.text(seccion), findsOneWidget);
    }
  });

  testWidgets('Un Desarrollador no ve Estados, Roles ni Usuarios en el menú', (WidgetTester tester) async {
    await tester.pumpWidget(AppGestionTareas(servicio: ServicioConTareas(gestiona: false)));
    await tester.pumpAndSettle();

    tester.state<ScaffoldState>(find.byType(Scaffold)).openDrawer();
    await tester.pumpAndSettle();

    expect(find.text('Inicio'), findsOneWidget);
    expect(find.text('Proyectos'), findsOneWidget);
    expect(find.text('Estados'), findsNothing);
    expect(find.text('Roles'), findsNothing);
    expect(find.text('Usuarios'), findsNothing);
  });

  testWidgets('Desde el menú se abre Inicio con el dashboard', (WidgetTester tester) async {
    await tester.pumpWidget(AppGestionTareas(servicio: ServicioConTareas()));
    await tester.pumpAndSettle();

    tester.state<ScaffoldState>(find.byType(Scaffold)).openDrawer();
    await tester.pumpAndSettle();
    await tester.tap(find.text('Inicio'));
    await tester.pumpAndSettle();

    expect(find.text('Sistema de Gestión y Seguimiento de Tareas'), findsOneWidget);
    expect(find.text('Tareas por estado'), findsOneWidget);
  });
}
