import 'package:flutter/material.dart';

import '../models/catalogos.dart';
import '../models/tarea.dart';
import '../services/tarea_service.dart';
import '../utils/formato.dart';

class DetalleTareaScreen extends StatefulWidget {
  final Tarea tarea;
  final TareaService servicio;

  const DetalleTareaScreen({super.key, required this.tarea, required this.servicio});

  @override
  State<DetalleTareaScreen> createState() => _DetalleTareaScreenState();
}

class _DetalleTareaScreenState extends State<DetalleTareaScreen> {
  late Tarea _tarea;
  bool _trabajando = false;

  @override
  void initState() {
    super.initState();
    // Se muestra de inmediato la tarea que llegó de la lista y luego se pide el detalle actualizado
    _tarea = widget.tarea;
    _recargar();
  }

  /// GET /api/Tareas/{id}
  Future<void> _recargar() async {
    try {
      final actualizada = await widget.servicio.obtenerTarea(_tarea.id);
      if (!mounted) return;
      setState(() => _tarea = actualizada);
    } on ApiException {
      // Sin conexión: se queda con los datos que ya tenía
    }
  }

  void _mostrarMensaje(String texto) {
    ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(texto)));
  }

  /// Muestra la lista de estados y cambia el estado de la tarea (PUT /api/Tareas/{id}).
  Future<void> _cambiarEstado() async {
    List<Estado> estados;
    try {
      estados = await widget.servicio.obtenerEstados();
    } on ApiException catch (e) {
      if (mounted) _mostrarMensaje(e.mensaje);
      return;
    }
    if (!mounted) return;

    final elegido = await showModalBottomSheet<Estado>(
      context: context,
      builder: (contextoHoja) => SafeArea(
        child: ListView(
          shrinkWrap: true,
          children: [
            const ListTile(title: Text('Cambiar estado', style: TextStyle(fontWeight: FontWeight.bold))),
            ...estados.map((estado) => ListTile(
                  title: Text(estado.nombre),
                  subtitle: estado.esFinal ? const Text('Cierra la tarea') : null,
                  trailing: estado.id == _tarea.idEstado ? const Icon(Icons.check) : null,
                  onTap: () => Navigator.pop(contextoHoja, estado),
                )),
          ],
        ),
      ),
    );

    if (elegido == null || elegido.id == _tarea.idEstado) return;

    setState(() => _trabajando = true);
    try {
      await widget.servicio.cambiarEstado(_tarea, elegido.id);
      await _recargar();
      if (mounted) _mostrarMensaje('Estado cambiado a ${elegido.nombre}');
    } on ApiException catch (e) {
      if (mounted) _mostrarMensaje(e.mensaje);
    } finally {
      if (mounted) setState(() => _trabajando = false);
    }
  }

  /// DELETE /api/Tareas/{id}, con confirmación.
  Future<void> _eliminar() async {
    final confirmado = await showDialog<bool>(
      context: context,
      builder: (contextoDialogo) => AlertDialog(
        title: const Text('Eliminar tarea'),
        content: Text('¿Segura que deseas eliminar "${_tarea.titulo}"?'),
        actions: [
          TextButton(onPressed: () => Navigator.pop(contextoDialogo, false), child: const Text('Cancelar')),
          FilledButton(onPressed: () => Navigator.pop(contextoDialogo, true), child: const Text('Eliminar')),
        ],
      ),
    );
    if (confirmado != true) return;

    setState(() => _trabajando = true);
    try {
      await widget.servicio.eliminarTarea(_tarea.id);
      if (!mounted) return;
      _mostrarMensaje('Tarea eliminada');
      Navigator.pop(context);
    } on ApiException catch (e) {
      if (!mounted) return;
      _mostrarMensaje(e.mensaje);
      setState(() => _trabajando = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final colores = coloresPrioridad(_tarea.prioridad);

    return Scaffold(
      appBar: AppBar(
        title: Text('Tarea #${_tarea.id}'),
        actions: [
          IconButton(
            icon: const Icon(Icons.delete_outline),
            tooltip: 'Eliminar',
            onPressed: _trabajando ? null : _eliminar,
          ),
        ],
      ),
      body: RefreshIndicator(
        onRefresh: _recargar,
        child: ListView(
          padding: const EdgeInsets.all(16),
          children: [
            if (_tarea.atrasada)
              Container(
                margin: const EdgeInsets.only(bottom: 16),
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(
                  color: const Color(0xFFFEE2E2),
                  borderRadius: BorderRadius.circular(8),
                ),
                child: const Text(
                  'Esta tarea está atrasada: su fecha límite ya pasó y todavía no está cerrada.',
                  style: TextStyle(color: Color(0xFFB91C1C), fontWeight: FontWeight.bold),
                ),
              ),
            Text(_tarea.titulo, style: Theme.of(context).textTheme.headlineSmall),
            const SizedBox(height: 8),
            Text(
              _tarea.descripcion ?? 'Sin descripción',
              style: TextStyle(color: Colors.grey.shade700, fontStyle: _tarea.descripcion == null ? FontStyle.italic : null),
            ),
            const SizedBox(height: 16),
            Wrap(
              spacing: 8,
              children: [
                Etiqueta(texto: _tarea.estado, fondo: const Color(0xFFE0F2FE), colorTexto: const Color(0xFF0369A1)),
                Etiqueta(texto: _tarea.prioridad, fondo: colores.fondo, colorTexto: colores.texto),
              ],
            ),
            const Divider(height: 32),
            _Dato(etiqueta: 'Responsable', valor: _tarea.responsable),
            _Dato(etiqueta: 'Proyecto', valor: _tarea.proyecto),
            _Dato(etiqueta: 'Fecha de creación', valor: formatearFecha(_tarea.fechaCreacion)),
            _Dato(etiqueta: 'Fecha límite', valor: formatearFecha(_tarea.fechaLimite)),
            _Dato(etiqueta: 'Fecha de finalización', valor: formatearFecha(_tarea.fechaFinalizacion)),
            _Dato(etiqueta: 'Última actualización', valor: formatearFecha(_tarea.fechaActualizacion)),
            const SizedBox(height: 24),
            FilledButton.icon(
              onPressed: _trabajando ? null : _cambiarEstado,
              icon: const Icon(Icons.swap_horiz),
              label: const Text('Cambiar estado'),
            ),
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
      padding: const EdgeInsets.only(bottom: 10),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SizedBox(
            width: 150,
            child: Text(etiqueta, style: const TextStyle(fontWeight: FontWeight.bold)),
          ),
          Expanded(child: Text(valor)),
        ],
      ),
    );
  }
}
