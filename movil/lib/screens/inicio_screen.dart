import 'package:flutter/material.dart';

import '../config/tema.dart';
import '../models/catalogos.dart';
import '../models/tarea.dart';
import '../services/tarea_service.dart';
import '../utils/formato.dart';
import '../widgets/comunes.dart';
import '../widgets/menu_lateral.dart';
import 'detalle_tarea_screen.dart';

/// Sección Inicio: el dashboard de la web (indicadores y paneles), acomodado para celular.
class InicioScreen extends StatefulWidget {
  final TareaService servicio;

  const InicioScreen({super.key, required this.servicio});

  @override
  State<InicioScreen> createState() => _InicioScreenState();
}

class _InicioScreenState extends State<InicioScreen> {
  bool _cargando = true;
  String? _error;
  List<Tarea> _tareas = [];
  List<Estado> _estados = [];

  @override
  void initState() {
    super.initState();
    _cargar();
  }

  Future<void> _cargar() async {
    setState(() {
      _cargando = _tareas.isEmpty;
      _error = null;
    });
    try {
      // Las dos peticiones salen al mismo tiempo
      final resultados = await Future.wait([widget.servicio.obtenerTareas(), widget.servicio.obtenerEstados()]);
      if (!mounted) return;
      setState(() {
        _tareas = (resultados[0] as ResultadoTareas).tareas;
        _estados = resultados[1] as List<Estado>;
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

  Future<void> _verDetalle(Tarea tarea) async {
    await Navigator.push(
      context,
      MaterialPageRoute(builder: (_) => DetalleTareaScreen(tarea: tarea, servicio: widget.servicio)),
    );
    _cargar(); // Por si la tarea cambió
  }

  /// Cuenta cuántas tareas hay por cada nombre y las ordena de mayor a menor.
  List<(String, int)> _contarPor(Iterable<Tarea> tareas, String Function(Tarea) nombreDe) {
    final conteo = <String, int>{};
    for (final tarea in tareas) {
      conteo.update(nombreDe(tarea), (valor) => valor + 1, ifAbsent: () => 1);
    }
    final lista = conteo.entries.map((e) => (e.key, e.value)).toList();
    lista.sort((a, b) => b.$2.compareTo(a.$2));
    return lista;
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Inicio'),
        actions: [IconButton(icon: const Icon(Icons.refresh), tooltip: 'Actualizar', onPressed: _cargar)],
      ),
      drawer: MenuLateral(servicio: widget.servicio, actual: Seccion.inicio),
      body: CuerpoCarga(
        cargando: _cargando,
        error: _error,
        onReintentar: _cargar,
        contenido: _construirDashboard,
      ),
    );
  }

  Widget _construirDashboard() {
    // Por estado: todos los estados, también los que tienen 0 tareas
    final porEstado = _estados.map((e) => (e.nombre, _tareas.where((t) => t.idEstado == e.id).length)).toList()
      ..sort((a, b) => b.$2.compareTo(a.$2));
    // Por responsable: solo tareas abiertas (carga de trabajo actual)
    final porResponsable = _contarPor(_tareas.where((t) => !t.esFinal), (t) => t.responsable);
    final porProyecto = _contarPor(_tareas, (t) => t.proyecto);
    final atrasadas = _tareas.where((t) => t.atrasada).toList()
      ..sort((a, b) => (a.fechaLimite ?? '').compareTo(b.fechaLimite ?? ''));

    return ListView(
      physics: const AlwaysScrollableScrollPhysics(),
      padding: const EdgeInsets.all(16),
      children: [
        const TituloSeccion(
          titulo: 'Sistema de Gestión y Seguimiento de Tareas',
          descripcion: 'Plataforma centralizada para organizar responsabilidades, prioridades y estados de equipos de desarrollo.',
        ),
        ResumenKpis(tareas: _tareas),
        const SizedBox(height: 16),
        _PanelBarras(titulo: 'Tareas por estado', datos: porEstado),
        _PanelBarras(titulo: 'Tareas abiertas por responsable', datos: porResponsable),
        _PanelBarras(titulo: 'Tareas por proyecto', datos: porProyecto),
        _PanelAtrasadas(tareas: atrasadas, onVer: _verDetalle),
      ],
    );
  }
}

/// Fondo blanco con borde y título, como los paneles del dashboard web.
class _Panel extends StatelessWidget {
  final String titulo;
  final Widget contenido;

  const _Panel({required this.titulo, required this.contenido});

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: Card(
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(titulo, style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: Colores.titulo)),
              const SizedBox(height: 12),
              contenido,
            ],
          ),
        ),
      ),
    );
  }
}

/// Barras horizontales: nombre | barra | cantidad (.fila-barra de la web).
class _PanelBarras extends StatelessWidget {
  final String titulo;
  final List<(String, int)> datos;

  const _PanelBarras({required this.titulo, required this.datos});

  /// Largo de la barra (entre 0 y 1). Si hay al menos una tarea se ve un pedacito.
  static double _proporcion(int valor, int maximo) {
    if (maximo == 0 || valor == 0) return 0;
    final proporcion = valor / maximo;
    return proporcion < 0.03 ? 0.03 : proporcion;
  }

  @override
  Widget build(BuildContext context) {
    if (datos.isEmpty) {
      return _Panel(
        titulo: titulo,
        contenido: const Text('Sin datos para mostrar.', style: TextStyle(color: Colores.textoTenue)),
      );
    }

    final maximo = datos.map((d) => d.$2).reduce((a, b) => a > b ? a : b);
    return _Panel(
      titulo: titulo,
      contenido: Column(
        children: [
          for (final (nombre, valor) in datos)
            Padding(
              padding: const EdgeInsets.symmetric(vertical: 5),
              child: Row(
                children: [
                  SizedBox(
                    width: 110,
                    child: Text(
                      nombre,
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                      style: const TextStyle(fontSize: 13, color: Colores.texto),
                    ),
                  ),
                  const SizedBox(width: 8),
                  Expanded(
                    child: ClipRRect(
                      borderRadius: BorderRadius.circular(4),
                      child: LinearProgressIndicator(
                        value: _proporcion(valor, maximo),
                        minHeight: 10,
                        backgroundColor: Colores.fondo,
                        color: Colores.azul,
                      ),
                    ),
                  ),
                  const SizedBox(width: 8),
                  SizedBox(
                    width: 28,
                    child: Text(
                      '$valor',
                      textAlign: TextAlign.right,
                      style: const TextStyle(fontWeight: FontWeight.bold, color: Colores.titulo),
                    ),
                  ),
                ],
              ),
            ),
        ],
      ),
    );
  }
}

/// Lista de tareas atrasadas con su botón Ver.
class _PanelAtrasadas extends StatelessWidget {
  final List<Tarea> tareas;
  final ValueChanged<Tarea> onVer;

  const _PanelAtrasadas({required this.tareas, required this.onVer});

  @override
  Widget build(BuildContext context) {
    if (tareas.isEmpty) {
      return const _Panel(
        titulo: 'Tareas atrasadas',
        contenido: Text('No hay tareas atrasadas.', style: TextStyle(color: Colores.textoTenue)),
      );
    }

    return _Panel(
      titulo: 'Tareas atrasadas',
      contenido: Column(
        children: [
          for (var i = 0; i < tareas.length; i++) ...[
            Row(
              children: [
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        '#${tareas[i].id} ${tareas[i].titulo}',
                        style: const TextStyle(fontWeight: FontWeight.bold, color: Colores.titulo),
                      ),
                      const SizedBox(height: 2),
                      Text(
                        '${tareas[i].responsable} · venció el ${formatearFecha(tareas[i].fechaLimite)}',
                        style: const TextStyle(fontSize: 12, color: Colores.textoAtrasada),
                      ),
                    ],
                  ),
                ),
                const SizedBox(width: 8),
                BotonAccion(texto: 'Ver', color: Colores.gris, onPressed: () => onVer(tareas[i])),
              ],
            ),
            if (i < tareas.length - 1) const Divider(height: 20),
          ],
        ],
      ),
    );
  }
}
