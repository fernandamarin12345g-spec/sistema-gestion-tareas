import 'package:flutter/material.dart';
import 'screens/lista_tareas_screen.dart';

void main() {
  runApp(const AppGestionTareas());
}

class AppGestionTareas extends StatelessWidget {
  const AppGestionTareas({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      debugShowCheckedModeBanner: false,
      title: 'Gestión de Tareas',
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(seedColor: Colors.teal),
        useMaterial3: true,
      ),
      home: const ListaTareasScreen(),
    );
  }
}