import 'package:flutter/material.dart';

import '../models/catalogos.dart';
import '../models/tarea.dart';
import '../services/tarea_service.dart';
import '../config/tema.dart';
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
      backgroundColor: Colors.white,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(16))),
      builder: (contextoHoja) => SafeArea(
        child: ListView(
          shrinkWrap: true,
          children: [
            const ListTile(
              title: Text('Cambiar estado', style: TextStyle(fontWeight: FontWeight.bold, color: Colores.titulo)),
            ),
            ...estados.map((estado) => ListTile(
                  title: Text(estado.nombre, style: const TextStyle(color: Colores.texto)),
                  subtitle: estado.esFinal
                      ? const Text('Cierra la tarea', style: TextStyle(color: Color(0xFF15803D)))
                      : null,
                  trailing: estado.id == _tarea.idEstado ? const Icon(Icons.check, color: Colores.azul) : null,
                  onTap: () => Navigator.pop(contextoHoja, estado),
                )),
          ],
        ),
      ),
    );

    if (elegido == null || elegido.id == _tarea.idEstado) return;

    setState(() => _trabajando = true);
    try {
      await widget.servicio.cambiarEstado(_tarea.id, elegido.id);
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
          FilledButton(
            style: FilledButton.styleFrom(backgroundColor: Colores.rojo),
            onPressed: () => Navigator.pop(contextoDialogo, true),
            child: const Text('Eliminar'),
          ),
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
    final sesion = widget.servicio.sesion;

    // Permisos (la API también los valida): eliminar solo Administrador;
    // cambiar estado Administrador, Líder o el responsable de la tarea.
    final puedeEliminar = sesion?.esAdmin ?? false;
    final puedeCambiarEstado =
        (sesion?.gestionaTareas ?? false) || (sesion != null && _tarea.idUsuario == sesion.idUsuario);
    final sinDescripcion = _tarea.descripcion == null || _tarea.descripcion!.trim().isEmpty;

    return Scaffold(
      appBar: AppBar(title: Text('Tarea #${_tarea.id}')),
      body: RefreshIndicator(
        onRefresh: _recargar,
        child: ListView(
          padding: const EdgeInsets.all(16),
          children: [
            // Misma tarjeta de detalle de la web (.tarjeta-detalle)
            Card(
              child: Padding(
                padding: const EdgeInsets.all(20),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    if (_tarea.atrasada)
                      Container(
                        margin: const EdgeInsets.only(bottom: 16),
                        padding: const EdgeInsets.all(12),
                        decoration: BoxDecoration(
                          color: Colores.avisoFondo,
                          borderRadius: BorderRadius.circular(6),
                          border: Border.all(color: Colores.avisoBorde),
                        ),
                        child: const Text(
                          'Esta tarea está atrasada: su fecha límite ya pasó y todavía no está cerrada.',
                          style: TextStyle(color: Colores.avisoTexto, fontWeight: FontWeight.bold),
                        ),
                      ),
                    Text(
                      '#${_tarea.id} · ${_tarea.titulo}',
                      style: const TextStyle(fontSize: 20, fontWeight: FontWeight.bold, color: Colores.titulo),
                    ),
                    const SizedBox(height: 8),
                    Text(
                      sinDescripcion ? 'Sin descripción' : _tarea.descripcion!,
                      style: TextStyle(
                        color: Colores.textoSuave,
                        fontStyle: sinDescripcion ? FontStyle.italic : FontStyle.normal,
                      ),
                    ),
                    const Divider(height: 32),
                    // Datos en dos columnas (.detalle-datos)
                    _FilaDatos(
                      izquierda: _Dato(etiqueta: 'Estado', valor: Etiqueta.estado(_tarea.estado)),
                      derecha: _Dato(etiqueta: 'Prioridad', valor: Etiqueta.prioridad(_tarea.prioridad)),
                    ),
                    _FilaDatos(
                      izquierda: _Dato.texto('Responsable', _tarea.responsable),
                      derecha: _Dato.texto('Proyecto', _tarea.proyecto),
                    ),
                    _FilaDatos(
                      izquierda: _Dato.texto('Fecha de creación', formatearFecha(_tarea.fechaCreacion)),
                      derecha: _Dato.texto(
                        'Fecha límite',
                        formatearFecha(_tarea.fechaLimite),
                        color: _tarea.atrasada ? Colores.textoAtrasada : Colores.titulo,
                      ),
                    ),
                    _FilaDatos(
                      izquierda: _Dato.texto('Fecha de finalización', formatearFecha(_tarea.fechaFinalizacion)),
                      derecha: _Dato.texto('Última actualización', formatearFecha(_tarea.fechaActualizacion)),
                    ),
                    if (puedeCambiarEstado) ...[
                      const SizedBox(height: 8),
                      FilledButton.icon(
                        onPressed: _trabajando ? null : _cambiarEstado,
                        icon: const Icon(Icons.swap_horiz),
                        label: const Text('Cambiar estado'),
                      ),
                    ],
                    if (puedeEliminar) ...[
                      const SizedBox(height: 10),
                      FilledButton.icon(
                        style: FilledButton.styleFrom(backgroundColor: Colores.rojo),
                        onPressed: _trabajando ? null : _eliminar,
                        icon: const Icon(Icons.delete_outline),
                        label: const Text('Eliminar'),
                      ),
                    ],
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

/// Dos datos lado a lado.
class _FilaDatos extends StatelessWidget {
  final Widget izquierda;
  final Widget derecha;

  const _FilaDatos({required this.izquierda, required this.derecha});

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 18),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Expanded(child: izquierda),
          const SizedBox(width: 16),
          Expanded(child: derecha),
        ],
      ),
    );
  }
}

/// Un dato: etiqueta gris en mayúsculas y el valor debajo.
class _Dato extends StatelessWidget {
  final String etiqueta;
  final Widget valor;

  const _Dato({required this.etiqueta, required this.valor});

  /// Dato de texto en negrita.
  factory _Dato.texto(String etiqueta, String texto, {Color color = Colores.titulo}) {
    return _Dato(
      etiqueta: etiqueta,
      valor: Text(texto, style: TextStyle(fontWeight: FontWeight.bold, color: color, fontSize: 14)),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          etiqueta.toUpperCase(),
          style: const TextStyle(fontSize: 11, fontWeight: FontWeight.bold, color: Colores.textoTenue),
        ),
        const SizedBox(height: 6),
        Align(alignment: Alignment.centerLeft, child: valor),
      ],
    );
  }
}
