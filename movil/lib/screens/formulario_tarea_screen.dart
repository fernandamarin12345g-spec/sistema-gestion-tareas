import 'package:flutter/material.dart';

import '../models/catalogos.dart';
import '../services/tarea_service.dart';
import '../utils/formato.dart';

class FormularioTareaScreen extends StatefulWidget {
  final TareaService servicio;

  const FormularioTareaScreen({super.key, required this.servicio});

  @override
  State<FormularioTareaScreen> createState() => _FormularioTareaScreenState();
}

class _FormularioTareaScreenState extends State<FormularioTareaScreen> {
  static const List<String> _prioridades = ['Baja', 'Media', 'Alta', 'Crítica'];

  final _formKey = GlobalKey<FormState>();
  final _tituloController = TextEditingController();
  final _descripcionController = TextEditingController();

  // Listas que llegan de la API para llenar los desplegables
  List<Estado> _estados = [];
  List<Usuario> _usuarios = [];
  List<Proyecto> _proyectos = [];

  // Valores seleccionados
  int? _idEstado;
  int? _idUsuario;
  int? _idProyecto;
  String _prioridad = 'Media';
  DateTime? _fechaLimite;

  bool _cargando = true;
  bool _guardando = false;
  String? _error;

  @override
  void initState() {
    super.initState();
    _cargarCatalogos();
  }

  @override
  void dispose() {
    // Libera la memoria de los campos de texto al cerrar la pantalla.
    _tituloController.dispose();
    _descripcionController.dispose();
    super.dispose();
  }

  /// Pide a la API los estados, usuarios y proyectos al mismo tiempo.
  Future<void> _cargarCatalogos() async {
    setState(() {
      _cargando = true;
      _error = null;
    });

    try {
      final resultados = await Future.wait([
        widget.servicio.obtenerEstados(),
        widget.servicio.obtenerUsuarios(),
        widget.servicio.obtenerProyectos(),
      ]);
      if (!mounted) return;

      setState(() {
        _estados = resultados[0] as List<Estado>;
        _usuarios = resultados[1] as List<Usuario>;
        _proyectos = resultados[2] as List<Proyecto>;

        // Estado inicial: "Pendiente" si existe; si no, el primer estado que no cierre la tarea
        final pendiente = _estados.where((e) => e.nombre.toLowerCase() == 'pendiente');
        final abiertos = _estados.where((e) => !e.esFinal);
        _idEstado = pendiente.isNotEmpty
            ? pendiente.first.id
            : (abiertos.isNotEmpty ? abiertos.first.id : (_estados.isNotEmpty ? _estados.first.id : null));

        _cargando = false;
      });
    } on ApiException catch (e) {
      if (!mounted) return;
      setState(() {
        _error = e.mensaje;
        _cargando = false;
      });
    }
  }

  Future<void> _elegirFecha() async {
    final hoy = DateTime.now();
    final elegida = await showDatePicker(
      context: context,
      initialDate: _fechaLimite ?? hoy,
      firstDate: DateTime(hoy.year - 1),
      lastDate: DateTime(hoy.year + 5),
      helpText: 'Fecha límite',
    );
    if (!mounted) return;
    if (elegida != null) setState(() => _fechaLimite = elegida);
  }

  /// POST /api/Tareas. Si la API rechaza los datos, se muestra su mensaje.
  Future<void> _guardar() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() => _guardando = true);
    try {
      await widget.servicio.crearTarea(
        titulo: _tituloController.text.trim(),
        descripcion: _descripcionController.text.trim().isEmpty ? null : _descripcionController.text.trim(),
        idUsuario: _idUsuario,
        idEstado: _idEstado!,
        idProyecto: _idProyecto,
        prioridad: _prioridad,
        fechaLimite: _fechaLimite == null ? null : fechaParaApi(_fechaLimite!),
      );
      if (!mounted) return;
      Navigator.pop(context, true);
    } on ApiException catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.mensaje)));
      setState(() => _guardando = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Nueva tarea')),
      body: _construirCuerpo(),
    );
  }

  Widget _construirCuerpo() {
    if (_cargando) {
      return const Center(child: CircularProgressIndicator());
    }

    if (_error != null) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Icon(Icons.cloud_off, size: 64, color: Colors.grey),
              const SizedBox(height: 16),
              Text(_error!, textAlign: TextAlign.center),
              const SizedBox(height: 16),
              FilledButton.icon(onPressed: _cargarCatalogos, icon: const Icon(Icons.refresh), label: const Text('Reintentar')),
            ],
          ),
        ),
      );
    }

    const separacion = SizedBox(height: 12);

    return Form(
      key: _formKey,
      child: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          TextFormField(
            controller: _tituloController,
            maxLength: 200,
            decoration: const InputDecoration(labelText: 'Título', border: OutlineInputBorder()),
            validator: (valor) => (valor == null || valor.trim().isEmpty) ? 'Escribe un título' : null,
          ),
          separacion,
          TextFormField(
            controller: _descripcionController,
            maxLines: 3,
            decoration: const InputDecoration(labelText: 'Descripción (opcional)', border: OutlineInputBorder()),
          ),
          separacion,
          DropdownButtonFormField<int>(
            initialValue: _idEstado,
            decoration: const InputDecoration(labelText: 'Estado', border: OutlineInputBorder()),
            items: _estados.map((e) => DropdownMenuItem(value: e.id, child: Text(e.nombre))).toList(),
            onChanged: (valor) => setState(() => _idEstado = valor),
            validator: (valor) => valor == null ? 'Selecciona un estado' : null,
          ),
          separacion,
          DropdownButtonFormField<String>(
            initialValue: _prioridad,
            decoration: const InputDecoration(labelText: 'Prioridad', border: OutlineInputBorder()),
            items: _prioridades.map((p) => DropdownMenuItem(value: p, child: Text(p))).toList(),
            onChanged: (valor) => setState(() => _prioridad = valor ?? 'Media'),
          ),
          separacion,
          DropdownButtonFormField<int?>(
            initialValue: _idUsuario,
            decoration: const InputDecoration(labelText: 'Responsable', border: OutlineInputBorder()),
            items: [
              const DropdownMenuItem<int?>(value: null, child: Text('Sin responsable')),
              ..._usuarios.map((u) => DropdownMenuItem<int?>(value: u.id, child: Text(u.nombre))),
            ],
            onChanged: (valor) => setState(() => _idUsuario = valor),
          ),
          separacion,
          DropdownButtonFormField<int?>(
            initialValue: _idProyecto,
            isExpanded: true,
            decoration: const InputDecoration(labelText: 'Proyecto', border: OutlineInputBorder()),
            items: [
              const DropdownMenuItem<int?>(value: null, child: Text('Sin proyecto')),
              ..._proyectos.map((p) => DropdownMenuItem<int?>(
                    value: p.id,
                    child: Text(p.nombre, overflow: TextOverflow.ellipsis),
                  )),
            ],
            onChanged: (valor) => setState(() => _idProyecto = valor),
          ),
          separacion,
          InputDecorator(
            decoration: const InputDecoration(labelText: 'Fecha límite (opcional)', border: OutlineInputBorder()),
            child: Row(
              children: [
                Expanded(
                  child: Text(_fechaLimite == null ? 'Sin fecha límite' : formatearFecha(fechaParaApi(_fechaLimite!))),
                ),
                if (_fechaLimite != null)
                  IconButton(
                    icon: const Icon(Icons.clear),
                    tooltip: 'Quitar fecha',
                    onPressed: () => setState(() => _fechaLimite = null),
                  ),
                IconButton(
                  icon: const Icon(Icons.calendar_month),
                  tooltip: 'Elegir fecha',
                  onPressed: _elegirFecha,
                ),
              ],
            ),
          ),
          const SizedBox(height: 20),
          SizedBox(
            width: double.infinity,
            child: FilledButton(
              onPressed: _guardando ? null : _guardar,
              child: _guardando
                  ? const SizedBox(width: 20, height: 20, child: CircularProgressIndicator(strokeWidth: 2))
                  : const Text('Guardar'),
            ),
          ),
        ],
      ),
    );
  }
}
