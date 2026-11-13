import 'package:flutter/material.dart';

import '../config/tema.dart';
import '../models/catalogos.dart';
import '../services/tarea_service.dart';
import '../utils/formato.dart';
import '../widgets/comunes.dart';
import '../widgets/menu_lateral.dart';
import 'lista_tareas_screen.dart';

/// Sección Proyectos: los proyectos con sus tareas abiertas y totales.
/// En el celular es de consulta; los proyectos se crean y editan desde el dashboard web.
class ProyectosScreen extends StatefulWidget {
  final TareaService servicio;

  const ProyectosScreen({super.key, required this.servicio});

  @override
  State<ProyectosScreen> createState() => _ProyectosScreenState();
}

class _ProyectosScreenState extends State<ProyectosScreen> {
  bool _cargando = true;
  String? _error;
  List<Proyecto> _proyectos = [];

  @override
  void initState() {
    super.initState();
    _cargar();
  }

  Future<void> _cargar() async {
    setState(() {
      _cargando = _proyectos.isEmpty;
      _error = null;
    });
    try {
      final proyectos = await widget.servicio.obtenerProyectos();
      if (!mounted) return;
      setState(() {
        _proyectos = proyectos;
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

  /// "Ver tareas": abre la sección Tareas filtrada por el proyecto.
  void _verTareas(Proyecto proyecto) {
    Navigator.of(context).pushAndRemoveUntil(
      PageRouteBuilder(
        pageBuilder: (_, _, _) => ListaTareasScreen(
          servicio: widget.servicio,
          idProyecto: proyecto.id,
          nombreProyecto: proyecto.nombre,
        ),
        transitionDuration: Duration.zero,
      ),
      (_) => false,
    );
  }

  /// Colores de la píldora del estado del proyecto.
  static Etiqueta _etiquetaEstado(String estado) {
    switch (estado) {
      case 'Activo':
        return Etiqueta(texto: estado, fondo: Colores.verdeFondo, colorTexto: Colores.verdeTexto);
      case 'Finalizado':
        return Etiqueta(texto: estado, fondo: Colores.fondo, colorTexto: Colores.textoEtiqueta);
      default:
        return Etiqueta(texto: estado, fondo: const Color(0xFFFEF9C3), colorTexto: const Color(0xFF854D0E));
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Proyectos'),
        actions: [IconButton(icon: const Icon(Icons.refresh), tooltip: 'Actualizar', onPressed: _cargar)],
      ),
      drawer: MenuLateral(servicio: widget.servicio, actual: Seccion.proyectos),
      body: CuerpoCarga(
        cargando: _cargando,
        error: _error,
        onReintentar: _cargar,
        contenido: () => ListView(
          physics: const AlwaysScrollableScrollPhysics(),
          padding: const EdgeInsets.all(16),
          children: [
            const TituloSeccion(
              titulo: 'Proyectos',
              descripcion: 'Proyectos o contextos a los que pertenecen las tareas.',
            ),
            if (_proyectos.isEmpty) const TarjetaVacia('No hay proyectos registrados.'),
            for (final proyecto in _proyectos)
              TarjetaRegistro(
                id: proyecto.id,
                titulo: proyecto.nombre,
                subtitulo: proyecto.descripcion,
                etiquetas: [_etiquetaEstado(proyecto.estado)],
                detalle: Row(
                  children: [
                    const Icon(Icons.checklist_rounded, size: 16, color: Colores.textoSuave),
                    const SizedBox(width: 4),
                    Text(
                      '${proyecto.tareasAbiertas} abierta(s) de ${proyecto.totalTareas}',
                      style: const TextStyle(fontSize: 13, color: Colores.texto),
                    ),
                  ],
                ),
                acciones: [
                  BotonAccion(texto: 'Ver tareas', color: Colores.gris, onPressed: () => _verTareas(proyecto)),
                ],
              ),
          ],
        ),
      ),
    );
  }
}
