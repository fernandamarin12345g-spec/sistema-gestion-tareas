import 'package:flutter/material.dart';

import '../config/tema.dart';
import '../models/tarea.dart';
import '../services/tarea_service.dart';
import '../utils/formato.dart';
import '../widgets/menu_lateral.dart';
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

  // Filtro por estado (los mismos botones de la web: "Todas", "Pendiente"...)
  String _filtroEstado = 'Todas';

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
        actions: [
          IconButton(icon: const Icon(Icons.refresh), tooltip: 'Actualizar', onPressed: _cargar),
        ],
      ),
      drawer: MenuLateral(sesion: widget.servicio.sesion, onCerrarSesion: _cerrarSesion),
      body: _construirCuerpo(),
      // Solo Administrador y Líder de Proyecto pueden crear tareas
      floatingActionButton: (widget.servicio.sesion?.gestionaTareas ?? false)
          ? FloatingActionButton.extended(
              onPressed: _abrirFormulario,
              icon: const Icon(Icons.add),
              label: const Text('Nueva tarea', style: TextStyle(fontWeight: FontWeight.bold)),
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
              const Icon(Icons.cloud_off, size: 64, color: Colores.textoTenue),
              const SizedBox(height: 16),
              Text(
                _error!,
                textAlign: TextAlign.center,
                style: const TextStyle(fontSize: 16, color: Colores.texto),
              ),
              const SizedBox(height: 16),
              FilledButton.icon(onPressed: _cargar, icon: const Icon(Icons.refresh), label: const Text('Reintentar')),
            ],
          ),
        ),
      );
    }

    // Estados que aparecen en las tareas, para los botones de filtro
    final estados = <String>{for (final t in _tareas) t.estado}.toList();
    final visibles =
        _filtroEstado == 'Todas' ? _tareas : _tareas.where((t) => t.estado == _filtroEstado).toList();

    // 3. Resumen, filtros y lista (deslizar hacia abajo para actualizar)
    return RefreshIndicator(
      onRefresh: _cargar,
      child: ListView(
        padding: const EdgeInsets.fromLTRB(16, 16, 16, 96),
        children: [
          if (_desdeCache)
            Container(
              margin: const EdgeInsets.only(bottom: 12),
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: const Color(0xFFFEF9C3),
                borderRadius: BorderRadius.circular(8),
                border: Border.all(color: const Color(0xFFFDE68A)),
              ),
              child: Text(
                'Sin conexión con el servidor. Mostrando las tareas guardadas el ${_fechaCache ?? 'último ingreso'}.',
                style: const TextStyle(color: Color(0xFF854D0E)),
              ),
            ),
          _Resumen(tareas: _tareas),
          const SizedBox(height: 16),
          SizedBox(
            height: 40,
            child: ListView(
              scrollDirection: Axis.horizontal,
              children: [
                for (final nombre in ['Todas', ...estados])
                  Padding(
                    padding: const EdgeInsets.only(right: 8),
                    child: _BotonFiltro(
                      texto: nombre,
                      oscuro: nombre == 'Todas',
                      seleccionado: nombre == _filtroEstado,
                      onTap: () => setState(() => _filtroEstado = nombre),
                    ),
                  ),
              ],
            ),
          ),
          const SizedBox(height: 12),
          if (visibles.isEmpty)
            const Card(
              child: Padding(
                padding: EdgeInsets.all(24),
                child: Center(
                  child: Text('No hay tareas para mostrar.', style: TextStyle(color: Colores.texto)),
                ),
              ),
            ),
          for (final tarea in visibles)
            Padding(
              padding: const EdgeInsets.only(bottom: 10),
              child: _TarjetaTarea(tarea: tarea, onTap: () => _abrirDetalle(tarea)),
            ),
        ],
      ),
    );
  }
}

/// Indicadores del dashboard: total, abiertas, atrasadas (en rojo si hay) y cerradas.
class _Resumen extends StatelessWidget {
  final List<Tarea> tareas;

  const _Resumen({required this.tareas});

  @override
  Widget build(BuildContext context) {
    final cerradas = tareas.where((t) => t.esFinal).length;
    final atrasadas = tareas.where((t) => t.atrasada).length;

    return Row(
      children: [
        _Kpi(titulo: 'Total', valor: tareas.length),
        const SizedBox(width: 8),
        _Kpi(titulo: 'Abiertas', valor: tareas.length - cerradas),
        const SizedBox(width: 8),
        _Kpi(titulo: 'Atrasadas', valor: atrasadas, alerta: atrasadas > 0),
        const SizedBox(width: 8),
        _Kpi(titulo: 'Cerradas', valor: cerradas),
      ],
    );
  }
}

class _Kpi extends StatelessWidget {
  final String titulo;
  final int valor;
  final bool alerta;

  const _Kpi({required this.titulo, required this.valor, this.alerta = false});

  @override
  Widget build(BuildContext context) {
    return Expanded(
      child: Container(
        padding: const EdgeInsets.fromLTRB(10, 10, 6, 10),
        decoration: BoxDecoration(
          color: alerta ? Colores.filaAtrasada : Colors.white,
          borderRadius: BorderRadius.circular(8),
          border: Border.all(color: alerta ? Colores.avisoBorde : Colores.borde),
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              titulo.toUpperCase(),
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
              style: TextStyle(
                fontSize: 10,
                fontWeight: FontWeight.bold,
                color: alerta ? Colores.avisoTexto : Colores.textoSuave,
              ),
            ),
            const SizedBox(height: 4),
            Text(
              '$valor',
              style: const TextStyle(fontSize: 22, fontWeight: FontWeight.bold, color: Colores.titulo),
            ),
          ],
        ),
      ),
    );
  }
}

/// Botón redondeado de filtro (.btn-filtro). "Todas" va en azul oscuro; el activo lleva contorno azul.
class _BotonFiltro extends StatelessWidget {
  final String texto;
  final bool oscuro;
  final bool seleccionado;
  final VoidCallback onTap;

  const _BotonFiltro({required this.texto, required this.oscuro, required this.seleccionado, required this.onTap});

  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(20),
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 16),
        alignment: Alignment.center,
        decoration: BoxDecoration(
          color: oscuro ? Colores.lateral : Colores.estadoFondo,
          borderRadius: BorderRadius.circular(20),
          border: Border.all(color: seleccionado ? Colores.azul : Colors.transparent, width: 2),
        ),
        child: Text(
          texto,
          style: TextStyle(
            color: oscuro ? Colors.white : Colores.estadoTexto,
            fontWeight: FontWeight.bold,
            fontSize: 13,
          ),
        ),
      ),
    );
  }
}

/// Tarjeta de una tarea: la misma información de una fila del grid de la web, acomodada para celular.
class _TarjetaTarea extends StatelessWidget {
  final Tarea tarea;
  final VoidCallback onTap;

  const _TarjetaTarea({required this.tarea, required this.onTap});

  @override
  Widget build(BuildContext context) {
    return Card(
      color: tarea.atrasada ? Colores.filaAtrasada : Colors.white,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(8),
        side: BorderSide(color: tarea.atrasada ? Colores.avisoBorde : Colores.borde),
      ),
      clipBehavior: Clip.antiAlias,
      child: InkWell(
        onTap: onTap,
        child: Padding(
          padding: const EdgeInsets.all(14),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // #ID y título, con el proyecto debajo en gris
              Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    '#${tarea.id}',
                    style: const TextStyle(fontWeight: FontWeight.bold, color: Colores.titulo, fontSize: 15),
                  ),
                  const SizedBox(width: 10),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          tarea.titulo,
                          maxLines: 2,
                          overflow: TextOverflow.ellipsis,
                          style: const TextStyle(fontSize: 15, fontWeight: FontWeight.w600, color: Colores.titulo),
                        ),
                        const SizedBox(height: 2),
                        Text(
                          tarea.proyecto,
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                          style: const TextStyle(fontSize: 12, color: Colores.textoTenue),
                        ),
                      ],
                    ),
                  ),
                  const Icon(Icons.chevron_right, color: Colores.textoTenue),
                ],
              ),
              const SizedBox(height: 10),
              // Estado, prioridad y si está atrasada
              Wrap(
                spacing: 6,
                runSpacing: 6,
                children: [
                  Etiqueta.estado(tarea.estado),
                  Etiqueta.prioridad(tarea.prioridad),
                  if (tarea.atrasada)
                    const Etiqueta(
                      texto: 'Atrasada',
                      fondo: Colores.avisoFondo,
                      colorTexto: Colores.avisoTexto,
                      borde: Colores.avisoBorde,
                    ),
                ],
              ),
              const SizedBox(height: 10),
              // Responsable y fecha límite
              Row(
                children: [
                  const Icon(Icons.person_outline, size: 16, color: Colores.textoSuave),
                  const SizedBox(width: 4),
                  Expanded(
                    child: Text(
                      tarea.responsable,
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                      style: const TextStyle(fontSize: 13, color: Colores.texto),
                    ),
                  ),
                  if (tarea.fechaLimite != null) ...[
                    Icon(
                      Icons.event_outlined,
                      size: 16,
                      color: tarea.atrasada ? Colores.textoAtrasada : Colores.textoSuave,
                    ),
                    const SizedBox(width: 4),
                    Text(
                      formatearFecha(tarea.fechaLimite),
                      style: TextStyle(
                        fontSize: 13,
                        color: tarea.atrasada ? Colores.textoAtrasada : Colores.texto,
                        fontWeight: tarea.atrasada ? FontWeight.bold : FontWeight.normal,
                      ),
                    ),
                  ],
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }
}
