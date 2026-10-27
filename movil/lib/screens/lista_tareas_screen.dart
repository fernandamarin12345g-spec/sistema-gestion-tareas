import 'package:flutter/material.dart';

import '../models/tarea.dart';
import '../services/tarea_service.dart';
import '../utils/formato.dart';
import 'detalle_tarea_screen.dart';
import 'formulario_tarea_screen.dart';
import 'login_screen.dart';

class ListaTareasScreen extends StatefulWidget {
  final TareaService servicio;

  const ListaTareasScreen({super.key, required this.servicio});

  @override
  State<ListaTareasScreen> createState() => _ListaTareasScreenState();
}

class _ListaTareasScreenState extends State<ListaTareasScreen> {
  // Estado de la pantalla: cargando, error, o la lista de tareas
  bool _cargando = true;
  String? _error;
  List<Tarea> _tareas = [];
  bool _desdeCache = false;
  String? _fechaCache;

  @override
  void initState() {
    super.initState();
    _cargar();
  }

  /// Pide las tareas a la API y actualiza la pantalla según el resultado.
  Future<void> _cargar() async {
    setState(() {
      _cargando = true;
      _error = null;
    });

    try {
      final resultado = await widget.servicio.obtenerTareas();
      if (!mounted) return;
      setState(() {
        _tareas = resultado.tareas;
        _desdeCache = resultado.desdeCache;
        _fechaCache = resultado.fechaCache;
        _cargando = false;
      });
    } on SesionExpiradaException catch (e) {
      // El token venció: se vuelve al inicio de sesión
      await _cerrarSesion(mensaje: e.mensaje);
    } on ApiException catch (e) {
      if (!mounted) return;
      setState(() {
        _error = e.mensaje;
        _cargando = false;
      });
    }
  }

  /// Borra la sesión del teléfono y vuelve a la pantalla de inicio de sesión.
  Future<void> _cerrarSesion({String? mensaje}) async {
    await widget.servicio.cerrarSesion();
    if (!mounted) return;
    Navigator.of(context).pushAndRemoveUntil(
      MaterialPageRoute(builder: (_) => LoginScreen(servicio: widget.servicio, mensaje: mensaje)),
      (_) => false,
    );
  }

  Future<void> _abrirFormulario() async {
    final creada = await Navigator.push<bool>(
      context,
      MaterialPageRoute(builder: (_) => FormularioTareaScreen(servicio: widget.servicio)),
    );

    if (creada == true) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Tarea creada exitosamente')));
      _cargar();
    }
  }

  Future<void> _abrirDetalle(Tarea tarea) async {
    await Navigator.push(
      context,
      MaterialPageRoute(builder: (_) => DetalleTareaScreen(tarea: tarea, servicio: widget.servicio)),
    );
    // Al volver se recarga la lista, por si la tarea cambió de estado o se eliminó
    _cargar();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Mis tareas'),
        backgroundColor: Theme.of(context).colorScheme.inversePrimary,
        actions: [
          IconButton(icon: const Icon(Icons.refresh), tooltip: 'Actualizar', onPressed: _cargar),
          PopupMenuButton<String>(
            icon: const Icon(Icons.account_circle),
            tooltip: 'Mi cuenta',
            onSelected: (opcion) {
              if (opcion == 'salir') _cerrarSesion();
            },
            itemBuilder: (_) => [
              PopupMenuItem<String>(
                enabled: false,
                child: Text('${widget.servicio.sesion?.nombre ?? ''}\n${widget.servicio.sesion?.rol ?? ''}'),
              ),
              const PopupMenuItem<String>(value: 'salir', child: Text('Cerrar sesión')),
            ],
          ),
        ],
      ),
      body: _construirCuerpo(),
      // Solo Administrador y Líder de Proyecto pueden crear tareas
      floatingActionButton: (widget.servicio.sesion?.gestionaTareas ?? false)
          ? FloatingActionButton.extended(
              onPressed: _abrirFormulario,
              icon: const Icon(Icons.add),
              label: const Text('Nueva tarea'),
            )
          : null,
    );
  }

  Widget _construirCuerpo() {
    // 1. Cargando por primera vez
    if (_cargando && _tareas.isEmpty) {
      return const Center(child: CircularProgressIndicator());
    }

    // 2. Error y sin datos guardados para mostrar
    if (_error != null && _tareas.isEmpty) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Icon(Icons.cloud_off, size: 64, color: Colors.grey),
              const SizedBox(height: 16),
              Text(_error!, textAlign: TextAlign.center, style: const TextStyle(fontSize: 16)),
              const SizedBox(height: 16),
              FilledButton.icon(onPressed: _cargar, icon: const Icon(Icons.refresh), label: const Text('Reintentar')),
            ],
          ),
        ),
      );
    }

    // 3. Lista (deslizar hacia abajo para actualizar)
    return RefreshIndicator(
      onRefresh: _cargar,
      child: ListView(
        padding: const EdgeInsets.only(bottom: 88),
        children: [
          if (_desdeCache)
            Container(
              width: double.infinity,
              color: const Color(0xFFFEF9C3),
              padding: const EdgeInsets.all(12),
              child: Text(
                'Sin conexión con el servidor. Mostrando las tareas guardadas el ${_fechaCache ?? 'último ingreso'}.',
                style: const TextStyle(color: Color(0xFF854D0E)),
              ),
            ),
          if (_tareas.isEmpty)
            const Padding(
              padding: EdgeInsets.all(32),
              child: Center(child: Text('No hay tareas registradas.')),
            ),
          ..._tareas.map(_construirTarjeta),
        ],
      ),
    );
  }

  Widget _construirTarjeta(Tarea tarea) {
    final colores = coloresPrioridad(tarea.prioridad);

    return Card(
      margin: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
      color: tarea.atrasada ? const Color(0xFFFFF7F7) : null,
      child: ListTile(
        leading: CircleAvatar(child: Text('${tarea.id}')),
        title: Text(tarea.titulo),
        subtitle: Padding(
          padding: const EdgeInsets.only(top: 4),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text('${tarea.responsable} · ${tarea.estado}'),
              const SizedBox(height: 6),
              Wrap(
                spacing: 6,
                runSpacing: 4,
                children: [
                  Etiqueta(texto: tarea.prioridad, fondo: colores.fondo, colorTexto: colores.texto),
                  if (tarea.fechaLimite != null)
                    Etiqueta(
                      texto: 'Límite ${formatearFecha(tarea.fechaLimite)}',
                      fondo: const Color(0xFFF1F5F9),
                      colorTexto: const Color(0xFF475569),
                    ),
                  if (tarea.atrasada)
                    const Etiqueta(texto: 'Atrasada', fondo: Color(0xFFFEE2E2), colorTexto: Color(0xFFB91C1C)),
                ],
              ),
            ],
          ),
        ),
        trailing: const Icon(Icons.chevron_right),
        onTap: () => _abrirDetalle(tarea),
      ),
    );
  }
}
