import 'package:flutter/material.dart';
import '../models/tarea.dart';

class DetalleTareaScreen extends StatelessWidget {
  final Tarea tarea;

  const DetalleTareaScreen({super.key, required this.tarea});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text('Tarea #${tarea.id}')),
      body: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(tarea.titulo, style: Theme.of(context).textTheme.headlineSmall),
            const SizedBox(height: 16),
            _Dato(etiqueta: 'Responsable', valor: tarea.responsable),
            _Dato(etiqueta: 'Estado', valor: tarea.estado),
          ],
        ),
      ),
    );
  }
}

// Widget pequeño para mostrar "Etiqueta: valor" en una fila.
class _Dato extends StatelessWidget {
  final String etiqueta;
  final String valor;

  const _Dato({required this.etiqueta, required this.valor});

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 8),
      child: Row(
        children: [
          Text('$etiqueta: ', style: const TextStyle(fontWeight: FontWeight.bold)),
          Expanded(child: Text(valor)),
        ],
      ),
    );
  }
}