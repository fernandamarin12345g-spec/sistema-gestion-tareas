import 'package:flutter/material.dart';

import 'screens/lista_tareas_screen.dart';
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
      home: ListaTareasScreen(servicio: servicio),
    );
  }
}
