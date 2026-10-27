import 'package:flutter/material.dart';

import 'models/sesion.dart';
import 'screens/lista_tareas_screen.dart';
import 'screens/login_screen.dart';
import 'services/tarea_service.dart';

void main() {
  runApp(AppGestionTareas(servicio: TareaService()));
}

class AppGestionTareas extends StatelessWidget {
  /// El servicio que habla con la API. En las pruebas se reemplaza por uno falso.
  final TareaService servicio;

  const AppGestionTareas({super.key, required this.servicio});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      debugShowCheckedModeBanner: false,
      title: 'Gestión de Tareas',
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(seedColor: Colors.teal),
        useMaterial3: true,
      ),
      home: PantallaInicio(servicio: servicio),
    );
  }
}

/// Decide la primera pantalla: si hay una sesión guardada y vigente, la lista de tareas;
/// si no, el inicio de sesión.
class PantallaInicio extends StatefulWidget {
  final TareaService servicio;

  const PantallaInicio({super.key, required this.servicio});

  @override
  State<PantallaInicio> createState() => _PantallaInicioState();
}

class _PantallaInicioState extends State<PantallaInicio> {
  late final Future<Sesion?> _sesionGuardada = widget.servicio.cargarSesionGuardada();

  @override
  Widget build(BuildContext context) {
    return FutureBuilder<Sesion?>(
      future: _sesionGuardada,
      builder: (context, resultado) {
        if (resultado.connectionState != ConnectionState.done) {
          return const Scaffold(body: Center(child: CircularProgressIndicator()));
        }
        if (resultado.data != null) {
          return ListaTareasScreen(servicio: widget.servicio);
        }
        return LoginScreen(servicio: widget.servicio);
      },
    );
  }
}
