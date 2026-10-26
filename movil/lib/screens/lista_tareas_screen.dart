import 'package:flutter/material.dart';
import '../models/tarea.dart';
import '../services/tarea_service.dart';
import 'detalle_tarea_screen.dart';
import 'formulario_tarea_screen.dart';

class ListaTareasScreen extends StatefulWidget {
  const ListaTareasScreen({super.key});

  @override
  State<ListaTareasScreen> createState() => _ListaTareasScreenState();
}

class _ListaTareasScreenState extends State<ListaTareasScreen> {
  final TareaService _servicio = TareaService();

  Future<void> _abrirFormulario() async {
    // Abre el formulario y espera lo que devuelva al cerrarse.
    final resultado = await Navigator.push<Map<String, String>>(
      context,
      MaterialPageRoute(builder: (_) => const FormularioTareaScreen()),
    );

    if (resultado != null) {
      // setState le avisa a Flutter que redibuje la lista.
      setState(() {
        _servicio.agregarTarea(resultado['titulo']!, resultado['responsable']!);
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    final tareas = _servicio.obtenerTareas();

    return Scaffold(
      appBar: AppBar(
        title: const Text('Mis tareas'),
        backgroundColor: Theme.of(context).colorScheme.inversePrimary,
      ),
      body: tareas.isEmpty
          ? const Center(child: Text('No hay tareas registradas.'))
          : ListView.builder(
              itemCount: tareas.length,
              itemBuilder: (context, index) {
                final Tarea tarea = tareas[index];
                return Card(
                  margin: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                  child: ListTile(
                    leading: CircleAvatar(child: Text('${tarea.id}')),
                    title: Text(tarea.titulo),
                    subtitle: Text('${tarea.responsable} · ${tarea.estado}'),
                    trailing: const Icon(Icons.chevron_right),
                    onTap: () {
                      Navigator.push(
                        context,
                        MaterialPageRoute(builder: (_) => DetalleTareaScreen(tarea: tarea)),
                      );
                    },
                  ),
                );
              },
            ),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: _abrirFormulario,
        icon: const Icon(Icons.add),
        label: const Text('Nueva tarea'),
      ),
    );
  }
}