import 'package:flutter/material.dart';

import '../config/tema.dart';
import '../models/catalogos.dart';
import '../services/tarea_service.dart';
import '../utils/formato.dart';
import '../widgets/comunes.dart';
import '../widgets/menu_lateral.dart';

/// Maestro de Estados (solo Administrador): crear, editar y eliminar estados de las tareas.
class EstadosScreen extends StatefulWidget {
  final TareaService servicio;

  const EstadosScreen({super.key, required this.servicio});

  @override
  State<EstadosScreen> createState() => _EstadosScreenState();
}

class _EstadosScreenState extends State<EstadosScreen> {
  bool _cargando = true;
  String? _error;
  List<Estado> _estados = [];

  @override
  void initState() {
    super.initState();
    _cargar();
  }

  Future<void> _cargar() async {
    setState(() {
      _cargando = _estados.isEmpty;
      _error = null;
    });
    try {
      final estados = await widget.servicio.obtenerEstados();
      if (!mounted) return;
      setState(() {
        _estados = estados;
        _cargando = false;
      });
    } on SesionExpiradaException catch (e) {
      if (mounted) await cerrarSesionYSalir(context, widget.servicio, mensaje: e.mensaje);
    } on ApiException catch (e) {
      if (!mounted) return;
      setState(() {
        _error = e.mensaje;
        _cargando = false;
      });
    }
  }

  Future<void> _abrirFormulario([Estado? estado]) async {
    final guardado = await abrirHojaFormulario(context, _FormularioEstado(servicio: widget.servicio, estado: estado));
    if (!guardado || !mounted) return;
    mostrarMensaje(context, estado == null ? 'Estado creado exitosamente' : 'Estado actualizado exitosamente');
    _cargar();
  }

  Future<void> _eliminar(Estado estado) async {
    final confirmado = await confirmarEliminacion(context, '¿Seguro que quieres eliminar el estado "${estado.nombre}"?');
    if (!confirmado || !mounted) return;
    try {
      await widget.servicio.eliminarEstado(estado.id);
      if (!mounted) return;
      mostrarMensaje(context, 'Estado eliminado exitosamente');
      _cargar();
    } on ApiException catch (e) {
      // Por ejemplo: "No se puede eliminar este estado porque hay tareas que actualmente lo están usando..."
      if (mounted) mostrarMensaje(context, e.mensaje, error: true);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Estados'),
        actions: [IconButton(icon: const Icon(Icons.refresh), tooltip: 'Actualizar', onPressed: _cargar)],
      ),
      drawer: MenuLateral(servicio: widget.servicio, actual: Seccion.estados),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () => _abrirFormulario(),
        icon: const Icon(Icons.add),
        label: const Text('Nuevo estado', style: TextStyle(fontWeight: FontWeight.bold)),
      ),
      body: CuerpoCarga(
        cargando: _cargando,
        error: _error,
        onReintentar: _cargar,
        contenido: () => ListView(
          physics: const AlwaysScrollableScrollPhysics(),
          padding: const EdgeInsets.fromLTRB(16, 16, 16, 96),
          children: [
            const TituloSeccion(
              titulo: 'Estados',
              descripcion: 'Estados por los que pasa una tarea. Los marcados como "Cierra la tarea" la dan por terminada.',
            ),
            if (_estados.isEmpty) const TarjetaVacia('No hay estados registrados.'),
            for (final estado in _estados)
              TarjetaRegistro(
                id: estado.id,
                titulo: estado.nombre,
                etiquetas: [
                  Etiqueta.estado(estado.nombre),
                  if (estado.esFinal)
                    const Etiqueta(texto: 'Cierra la tarea', fondo: Colores.verdeFondo, colorTexto: Colores.verdeTexto),
                ],
                acciones: [
                  BotonAccion(texto: 'Editar', color: Colores.naranja, onPressed: () => _abrirFormulario(estado)),
                  BotonAccion(texto: 'Eliminar', color: Colores.rojo, onPressed: () => _eliminar(estado)),
                ],
              ),
          ],
        ),
      ),
    );
  }
}

/// Formulario para crear (estado null) o editar un estado.
class _FormularioEstado extends StatefulWidget {
  final TareaService servicio;
  final Estado? estado;

  const _FormularioEstado({required this.servicio, this.estado});

  @override
  State<_FormularioEstado> createState() => _FormularioEstadoState();
}

class _FormularioEstadoState extends State<_FormularioEstado> {
  final _formKey = GlobalKey<FormState>();
  late final _nombreController = TextEditingController(text: widget.estado?.nombre ?? '');
  late bool _esFinal = widget.estado?.esFinal ?? false;
  bool _guardando = false;
  String? _error;

  @override
  void dispose() {
    _nombreController.dispose();
    super.dispose();
  }

  Future<void> _guardar() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() {
      _guardando = true;
      _error = null;
    });
    try {
      await widget.servicio.guardarEstado(
        id: widget.estado?.id,
        nombre: _nombreController.text.trim(),
        esFinal: _esFinal,
      );
      if (mounted) Navigator.pop(context, true);
    } on ApiException catch (e) {
      // El mensaje viene de la API, por ejemplo "Ya existe un estado con ese nombre."
      if (!mounted) return;
      setState(() {
        _error = e.mensaje;
        _guardando = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Form(
      key: _formKey,
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          EncabezadoFormulario(widget.estado == null ? 'Nuevo estado' : 'Editar estado'),
          ErrorFormulario(_error),
          TextFormField(
            controller: _nombreController,
            decoration: const InputDecoration(labelText: 'Nombre del estado'),
            textCapitalization: TextCapitalization.sentences,
            maxLength: 50,
            validator: (valor) => (valor == null || valor.trim().isEmpty) ? 'El nombre es obligatorio.' : null,
          ),
          CheckboxListTile(
            contentPadding: EdgeInsets.zero,
            controlAffinity: ListTileControlAffinity.leading,
            value: _esFinal,
            onChanged: (valor) => setState(() => _esFinal = valor ?? false),
            title: const Text('Cierra la tarea (estado final)'),
            subtitle: const Text('Por ejemplo "Terminada" o "Cancelada".'),
          ),
          const SizedBox(height: 12),
          FilledButton(
            onPressed: _guardando ? null : _guardar,
            child: _guardando
                ? const SizedBox(width: 22, height: 22, child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white))
                : Text(widget.estado == null ? 'Crear estado' : 'Guardar cambios'),
          ),
        ],
      ),
    );
  }
}
