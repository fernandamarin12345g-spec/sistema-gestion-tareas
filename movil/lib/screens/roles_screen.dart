import 'package:flutter/material.dart';

import '../config/tema.dart';
import '../models/catalogos.dart';
import '../services/tarea_service.dart';
import '../utils/formato.dart';
import '../widgets/comunes.dart';
import '../widgets/menu_lateral.dart';

/// Maestro de Roles (solo Administrador): crear, editar y eliminar roles y sus permisos.
class RolesScreen extends StatefulWidget {
  final TareaService servicio;

  const RolesScreen({super.key, required this.servicio});

  @override
  State<RolesScreen> createState() => _RolesScreenState();
}

class _RolesScreenState extends State<RolesScreen> {
  bool _cargando = true;
  String? _error;
  List<Rol> _roles = [];

  @override
  void initState() {
    super.initState();
    _cargar();
  }

  Future<void> _cargar() async {
    setState(() {
      _cargando = _roles.isEmpty;
      _error = null;
    });
    try {
      final roles = await widget.servicio.obtenerRoles();
      if (!mounted) return;
      setState(() {
        _roles = roles;
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

  Future<void> _abrirFormulario([Rol? rol]) async {
    final guardado = await abrirHojaFormulario(context, _FormularioRol(servicio: widget.servicio, rol: rol));
    if (!guardado || !mounted) return;
    mostrarMensaje(context, rol == null ? 'Rol creado exitosamente' : 'Rol actualizado exitosamente');
    _cargar();
  }

  Future<void> _eliminar(Rol rol) async {
    final confirmado = await confirmarEliminacion(context, '¿Seguro que quieres eliminar el rol "${rol.nombre}"?');
    if (!confirmado || !mounted) return;
    try {
      await widget.servicio.eliminarRol(rol.id);
      if (!mounted) return;
      mostrarMensaje(context, 'Rol eliminado exitosamente');
      _cargar();
    } on ApiException catch (e) {
      // Por ejemplo: "No se puede eliminar este rol porque hay usuarios asignados a él."
      if (mounted) mostrarMensaje(context, e.mensaje, error: true);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Roles'),
        actions: [IconButton(icon: const Icon(Icons.refresh), tooltip: 'Actualizar', onPressed: _cargar)],
      ),
      drawer: MenuLateral(servicio: widget.servicio, actual: Seccion.roles),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () => _abrirFormulario(),
        icon: const Icon(Icons.add),
        label: const Text('Nuevo rol', style: TextStyle(fontWeight: FontWeight.bold)),
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
              titulo: 'Roles',
              descripcion: 'Roles del equipo y lo que cada uno puede hacer en el sistema.',
            ),
            if (_roles.isEmpty) const TarjetaVacia('No hay roles registrados.'),
            for (final rol in _roles)
              TarjetaRegistro(
                id: rol.id,
                titulo: rol.nombre,
                etiquetas: [
                  if (rol.esAdmin)
                    const Etiqueta(texto: 'Administrador', fondo: Colores.avisoFondo, colorTexto: Colores.avisoTexto),
                  if (rol.gestionaTareas)
                    const Etiqueta(texto: 'Gestiona tareas', fondo: Colores.verdeFondo, colorTexto: Colores.verdeTexto),
                  if (!rol.esAdmin && !rol.gestionaTareas)
                    const Etiqueta(
                      texto: 'Solo sus tareas',
                      fondo: Colores.fondo,
                      colorTexto: Colores.textoEtiqueta,
                    ),
                ],
                acciones: [
                  BotonAccion(texto: 'Editar', color: Colores.naranja, onPressed: () => _abrirFormulario(rol)),
                  BotonAccion(texto: 'Eliminar', color: Colores.rojo, onPressed: () => _eliminar(rol)),
                ],
              ),
          ],
        ),
      ),
    );
  }
}

/// Formulario para crear (rol null) o editar un rol.
class _FormularioRol extends StatefulWidget {
  final TareaService servicio;
  final Rol? rol;

  const _FormularioRol({required this.servicio, this.rol});

  @override
  State<_FormularioRol> createState() => _FormularioRolState();
}

class _FormularioRolState extends State<_FormularioRol> {
  final _formKey = GlobalKey<FormState>();
  late final _nombreController = TextEditingController(text: widget.rol?.nombre ?? '');
  late bool _esAdmin = widget.rol?.esAdmin ?? false;
  late bool _gestionaTareas = widget.rol?.gestionaTareas ?? false;
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
      await widget.servicio.guardarRol(
        id: widget.rol?.id,
        nombre: _nombreController.text.trim(),
        esAdmin: _esAdmin,
        gestionaTareas: _gestionaTareas,
      );
      if (mounted) Navigator.pop(context, true);
    } on ApiException catch (e) {
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
          EncabezadoFormulario(widget.rol == null ? 'Nuevo rol' : 'Editar rol'),
          ErrorFormulario(_error),
          TextFormField(
            controller: _nombreController,
            decoration: const InputDecoration(labelText: 'Nombre del rol'),
            textCapitalization: TextCapitalization.sentences,
            maxLength: 50,
            validator: (valor) => (valor == null || valor.trim().isEmpty) ? 'El nombre es obligatorio.' : null,
          ),
          CheckboxListTile(
            contentPadding: EdgeInsets.zero,
            controlAffinity: ListTileControlAffinity.leading,
            value: _esAdmin,
            onChanged: (valor) => setState(() => _esAdmin = valor ?? false),
            title: const Text('Es administrador'),
            subtitle: const Text('Acceso a todo, incluidos Estados, Roles y Usuarios.'),
          ),
          CheckboxListTile(
            contentPadding: EdgeInsets.zero,
            controlAffinity: ListTileControlAffinity.leading,
            value: _gestionaTareas,
            onChanged: (valor) => setState(() => _gestionaTareas = valor ?? false),
            title: const Text('Gestiona tareas'),
            subtitle: const Text('Puede crear y editar tareas (por ejemplo, Líder de Proyecto).'),
          ),
          const SizedBox(height: 12),
          FilledButton(
            onPressed: _guardando ? null : _guardar,
            child: _guardando
                ? const SizedBox(width: 22, height: 22, child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white))
                : Text(widget.rol == null ? 'Crear rol' : 'Guardar cambios'),
          ),
        ],
      ),
    );
  }
}
