import 'package:flutter/material.dart';

import '../config/tema.dart';
import '../models/catalogos.dart';
import '../services/tarea_service.dart';
import '../utils/formato.dart';
import '../widgets/comunes.dart';
import '../widgets/menu_lateral.dart';

/// Gestión de Usuarios (solo Administrador): usuarios por rol, con sus tareas asignadas.
class UsuariosScreen extends StatefulWidget {
  final TareaService servicio;

  const UsuariosScreen({super.key, required this.servicio});

  @override
  State<UsuariosScreen> createState() => _UsuariosScreenState();
}

class _UsuariosScreenState extends State<UsuariosScreen> {
  bool _cargando = true;
  String? _error;
  List<Usuario> _usuarios = [];
  List<Rol> _roles = [];

  // Filtro por rol (como "Usuarios por rol" de la web)
  String _filtroRol = 'Todos';

  @override
  void initState() {
    super.initState();
    _cargar();
  }

  Future<void> _cargar() async {
    setState(() {
      _cargando = _usuarios.isEmpty;
      _error = null;
    });
    try {
      final resultados = await Future.wait([widget.servicio.obtenerUsuarios(), widget.servicio.obtenerRoles()]);
      if (!mounted) return;
      setState(() {
        _usuarios = resultados[0] as List<Usuario>;
        _roles = resultados[1] as List<Rol>;
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

  Future<void> _abrirFormulario([Usuario? usuario]) async {
    final guardado = await abrirHojaFormulario(
      context,
      _FormularioUsuario(servicio: widget.servicio, roles: _roles, usuario: usuario),
    );
    if (!guardado || !mounted) return;
    mostrarMensaje(context, usuario == null ? 'Usuario creado exitosamente' : 'Usuario actualizado exitosamente');
    _cargar();
  }

  Future<void> _eliminar(Usuario usuario) async {
    final confirmado =
        await confirmarEliminacion(context, '¿Seguro que quieres eliminar al usuario "${usuario.nombre}"?');
    if (!confirmado || !mounted) return;
    try {
      await widget.servicio.eliminarUsuario(usuario.id);
      if (!mounted) return;
      mostrarMensaje(context, 'Usuario eliminado exitosamente');
      _cargar();
    } on ApiException catch (e) {
      if (mounted) mostrarMensaje(context, e.mensaje, error: true);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Usuarios'),
        actions: [IconButton(icon: const Icon(Icons.refresh), tooltip: 'Actualizar', onPressed: _cargar)],
      ),
      drawer: MenuLateral(servicio: widget.servicio, actual: Seccion.usuarios),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () => _abrirFormulario(),
        icon: const Icon(Icons.person_add_alt),
        label: const Text('Nuevo usuario', style: TextStyle(fontWeight: FontWeight.bold)),
      ),
      body: CuerpoCarga(
        cargando: _cargando,
        error: _error,
        onReintentar: _cargar,
        contenido: _construirLista,
      ),
    );
  }

  Widget _construirLista() {
    final visibles = _filtroRol == 'Todos' ? _usuarios : _usuarios.where((u) => u.rol == _filtroRol).toList();

    return ListView(
      physics: const AlwaysScrollableScrollPhysics(),
      padding: const EdgeInsets.fromLTRB(16, 16, 16, 96),
      children: [
        const TituloSeccion(
          titulo: 'Usuarios',
          descripcion: 'Integrantes del equipo, su rol y las tareas que tienen asignadas.',
        ),
        FilaFiltros(
          opciones: _roles.map((r) => r.nombre).toList(),
          seleccionado: _filtroRol,
          textoTodos: 'Todos',
          onCambio: (nombre) => setState(() => _filtroRol = nombre),
        ),
        const SizedBox(height: 12),
        if (visibles.isEmpty) const TarjetaVacia('No hay usuarios con este rol.'),
        for (final usuario in visibles)
          TarjetaRegistro(
            id: usuario.id,
            titulo: usuario.nombre,
            subtitulo: usuario.correo,
            etiquetas: [
              Etiqueta.estado(usuario.rol),
              Etiqueta(
                texto: '${usuario.tareas.length} tarea(s)',
                fondo: Colores.fondo,
                colorTexto: Colores.textoEtiqueta,
              ),
            ],
            detalle: usuario.tareas.isEmpty ? null : _TareasDelUsuario(tareas: usuario.tareas),
            acciones: [
              BotonAccion(texto: 'Editar', color: Colores.naranja, onPressed: () => _abrirFormulario(usuario)),
              BotonAccion(texto: 'Eliminar', color: Colores.rojo, onPressed: () => _eliminar(usuario)),
            ],
          ),
      ],
    );
  }
}

/// Tareas asignadas al usuario: número, título y estado.
class _TareasDelUsuario extends StatelessWidget {
  final List<TareaDeUsuario> tareas;

  const _TareasDelUsuario({required this.tareas});

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(10),
      decoration: BoxDecoration(color: Colores.fondo, borderRadius: BorderRadius.circular(6)),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          for (final tarea in tareas)
            Padding(
              padding: const EdgeInsets.symmetric(vertical: 3),
              child: Row(
                children: [
                  Expanded(
                    child: Text(
                      '#${tarea.id} ${tarea.titulo}',
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                      style: const TextStyle(fontSize: 13, color: Colores.texto),
                    ),
                  ),
                  const SizedBox(width: 6),
                  Text(
                    tarea.estado,
                    style: const TextStyle(fontSize: 12, color: Colores.estadoTexto, fontWeight: FontWeight.bold),
                  ),
                ],
              ),
            ),
        ],
      ),
    );
  }
}

/// Formulario para crear (usuario null) o editar un usuario.
class _FormularioUsuario extends StatefulWidget {
  final TareaService servicio;
  final List<Rol> roles;
  final Usuario? usuario;

  const _FormularioUsuario({required this.servicio, required this.roles, this.usuario});

  @override
  State<_FormularioUsuario> createState() => _FormularioUsuarioState();
}

class _FormularioUsuarioState extends State<_FormularioUsuario> {
  final _formKey = GlobalKey<FormState>();
  late final _nombreController = TextEditingController(text: widget.usuario?.nombre ?? '');
  late final _correoController = TextEditingController(text: widget.usuario?.correo ?? '');
  final _contrasenaController = TextEditingController();
  late int? _idRol = widget.roles.any((r) => r.id == widget.usuario?.idRol) ? widget.usuario?.idRol : null;
  bool _verContrasena = false;
  bool _guardando = false;
  String? _error;

  bool get _esNuevo => widget.usuario == null;

  @override
  void dispose() {
    _nombreController.dispose();
    _correoController.dispose();
    _contrasenaController.dispose();
    super.dispose();
  }

  Future<void> _guardar() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() {
      _guardando = true;
      _error = null;
    });
    try {
      await widget.servicio.guardarUsuario(
        id: widget.usuario?.id,
        nombre: _nombreController.text.trim(),
        correo: _correoController.text.trim(),
        idRol: _idRol!,
        contrasena: _contrasenaController.text,
      );
      if (mounted) Navigator.pop(context, true);
    } on ApiException catch (e) {
      // Por ejemplo: "Ya existe un usuario con ese correo." o "El correo electrónico no tiene un formato válido."
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
          EncabezadoFormulario(_esNuevo ? 'Nuevo usuario' : 'Editar usuario'),
          ErrorFormulario(_error),
          TextFormField(
            controller: _nombreController,
            decoration: const InputDecoration(labelText: 'Nombre completo'),
            textCapitalization: TextCapitalization.words,
            maxLength: 100,
            validator: (valor) => (valor == null || valor.trim().isEmpty) ? 'El nombre es obligatorio.' : null,
          ),
          TextFormField(
            controller: _correoController,
            decoration: const InputDecoration(labelText: 'Correo electrónico'),
            keyboardType: TextInputType.emailAddress,
            maxLength: 100,
            validator: (valor) {
              final correo = valor?.trim() ?? '';
              if (correo.isEmpty) return 'El correo es obligatorio.';
              if (!RegExp(r'^[^@\s]+@[^@\s]+\.[^@\s]+$').hasMatch(correo)) return 'Escribe un correo válido.';
              return null;
            },
          ),
          DropdownButtonFormField<int>(
            initialValue: _idRol,
            decoration: const InputDecoration(labelText: 'Rol'),
            items: widget.roles.map((r) => DropdownMenuItem(value: r.id, child: Text(r.nombre))).toList(),
            onChanged: (valor) => setState(() => _idRol = valor),
            validator: (valor) => valor == null ? 'Selecciona un rol.' : null,
          ),
          const SizedBox(height: 16),
          TextFormField(
            controller: _contrasenaController,
            obscureText: !_verContrasena,
            decoration: InputDecoration(
              labelText: _esNuevo ? 'Contraseña' : 'Nueva contraseña (opcional)',
              helperText: _esNuevo ? 'Mínimo 8 caracteres.' : 'Déjala vacía para no cambiarla.',
              suffixIcon: IconButton(
                icon: Icon(_verContrasena ? Icons.visibility_off : Icons.visibility),
                onPressed: () => setState(() => _verContrasena = !_verContrasena),
              ),
            ),
            validator: (valor) {
              final contrasena = valor ?? '';
              if (_esNuevo && contrasena.isEmpty) return 'La contraseña es obligatoria.';
              if (contrasena.isNotEmpty && contrasena.length < 8) return 'Debe tener al menos 8 caracteres.';
              return null;
            },
          ),
          const SizedBox(height: 20),
          FilledButton(
            onPressed: _guardando ? null : _guardar,
            child: _guardando
                ? const SizedBox(width: 22, height: 22, child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white))
                : Text(_esNuevo ? 'Crear usuario' : 'Guardar cambios'),
          ),
        ],
      ),
    );
  }
}
