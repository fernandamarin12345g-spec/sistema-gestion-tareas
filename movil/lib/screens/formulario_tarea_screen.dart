import 'package:flutter/material.dart';

class FormularioTareaScreen extends StatefulWidget {
  const FormularioTareaScreen({super.key});

  @override
  State<FormularioTareaScreen> createState() => _FormularioTareaScreenState();
}

class _FormularioTareaScreenState extends State<FormularioTareaScreen> {
  final _formKey = GlobalKey<FormState>();
  final _tituloController = TextEditingController();
  final _responsableController = TextEditingController();

  @override
  void dispose() {
    // Libera la memoria de los campos de texto al cerrar la pantalla.
    _tituloController.dispose();
    _responsableController.dispose();
    super.dispose();
  }

  void _guardar() {
    if (_formKey.currentState!.validate()) {
      // Cierra el formulario y le devuelve los datos a la lista.
      Navigator.pop(context, {
        'titulo': _tituloController.text.trim(),
        'responsable': _responsableController.text.trim(),
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Nueva tarea')),
      body: Padding(
        padding: const EdgeInsets.all(16),
        child: Form(
          key: _formKey,
          child: Column(
            children: [
              TextFormField(
                controller: _tituloController,
                decoration: const InputDecoration(labelText: 'Título', border: OutlineInputBorder()),
                validator: (valor) =>
                    (valor == null || valor.trim().isEmpty) ? 'Escribe un título' : null,
              ),
              const SizedBox(height: 12),
              TextFormField(
                controller: _responsableController,
                decoration: const InputDecoration(labelText: 'Responsable', border: OutlineInputBorder()),
                validator: (valor) =>
                    (valor == null || valor.trim().isEmpty) ? 'Escribe el responsable' : null,
              ),
              const SizedBox(height: 20),
              SizedBox(
                width: double.infinity,
                child: FilledButton(onPressed: _guardar, child: const Text('Guardar')),
              ),
            ],
          ),
        ),
      ),
    );
  }
}