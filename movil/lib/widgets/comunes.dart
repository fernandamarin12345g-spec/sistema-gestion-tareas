import 'package:flutter/material.dart';

import '../config/tema.dart';
import '../models/tarea.dart';
import '../screens/login_screen.dart';
import '../services/tarea_service.dart';

// Piezas que comparten todas las pantallas: encabezado de sección, carga y errores,
// indicadores (KPI), botones de filtro, tarjetas de los maestros y mensajes.

/// Borra la sesión del teléfono y vuelve a la pantalla de inicio de sesión.
Future<void> cerrarSesionYSalir(BuildContext context, TareaService servicio, {String? mensaje}) async {
  // Se toma el navegador antes de esperar: el menú lateral puede cerrarse mientras tanto
  final navegador = Navigator.of(context);
  await servicio.cerrarSesion();
  navegador.pushAndRemoveUntil(
    MaterialPageRoute(builder: (_) => LoginScreen(servicio: servicio, mensaje: mensaje)),
    (_) => false,
  );
}

/// Mensaje corto en la parte de abajo (rojo si es un error).
void mostrarMensaje(BuildContext context, String texto, {bool error = false}) {
  ScaffoldMessenger.of(context).showSnackBar(
    SnackBar(content: Text(texto), backgroundColor: error ? Colores.textoAtrasada : null),
  );
}

/// Ventana de confirmación antes de eliminar. Devuelve true si el usuario confirma.
Future<bool> confirmarEliminacion(BuildContext context, String mensaje) async {
  final confirmado = await showDialog<bool>(
    context: context,
    builder: (contextoDialogo) => AlertDialog(
      title: const Text('Confirmar eliminación'),
      content: Text(mensaje),
      actions: [
        TextButton(onPressed: () => Navigator.pop(contextoDialogo, false), child: const Text('Cancelar')),
        FilledButton(
          style: FilledButton.styleFrom(backgroundColor: Colores.rojo, minimumSize: const Size(0, 40)),
          onPressed: () => Navigator.pop(contextoDialogo, true),
          child: const Text('Eliminar'),
        ),
      ],
    ),
  );
  return confirmado ?? false;
}

/// Abre un formulario en una hoja que sube desde abajo y se acomoda al teclado.
/// Devuelve true si el formulario se guardó.
Future<bool> abrirHojaFormulario(BuildContext context, Widget formulario) async {
  final guardado = await showModalBottomSheet<bool>(
    context: context,
    isScrollControlled: true,
    backgroundColor: Colors.white,
    shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(16))),
    builder: (contextoHoja) => Padding(
      padding: EdgeInsets.only(bottom: MediaQuery.of(contextoHoja).viewInsets.bottom),
      child: SafeArea(child: SingleChildScrollView(padding: const EdgeInsets.all(20), child: formulario)),
    ),
  );
  return guardado ?? false;
}

/// Título y descripción de la sección (como el h2 y el párrafo de cada sección de la web).
class TituloSeccion extends StatelessWidget {
  final String titulo;
  final String? descripcion;

  const TituloSeccion({super.key, required this.titulo, this.descripcion});

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(titulo, style: const TextStyle(fontSize: 22, fontWeight: FontWeight.bold, color: Colores.titulo)),
          if (descripcion != null) ...[
            const SizedBox(height: 4),
            Text(descripcion!, style: const TextStyle(fontSize: 14, color: Colores.textoSuave)),
          ],
        ],
      ),
    );
  }
}

/// Muestra el indicador de carga, el error con "Reintentar" o el contenido.
class CuerpoCarga extends StatelessWidget {
  final bool cargando;
  final String? error;
  final Future<void> Function() onReintentar;
  final Widget Function() contenido;

  const CuerpoCarga({
    super.key,
    required this.cargando,
    required this.error,
    required this.onReintentar,
    required this.contenido,
  });

  @override
  Widget build(BuildContext context) {
    if (cargando) {
      return const Center(child: CircularProgressIndicator());
    }
    if (error != null) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Icon(Icons.cloud_off, size: 64, color: Colores.textoTenue),
              const SizedBox(height: 16),
              Text(error!, textAlign: TextAlign.center, style: const TextStyle(fontSize: 16, color: Colores.texto)),
              const SizedBox(height: 16),
              FilledButton.icon(onPressed: onReintentar, icon: const Icon(Icons.refresh), label: const Text('Reintentar')),
            ],
          ),
        ),
      );
    }
    return RefreshIndicator(onRefresh: onReintentar, child: contenido());
  }
}

/// Indicadores del dashboard: total, abiertas, atrasadas (en rojo si hay) y cerradas.
class ResumenKpis extends StatelessWidget {
  final List<Tarea> tareas;

  const ResumenKpis({super.key, required this.tareas});

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
class BotonFiltro extends StatelessWidget {
  final String texto;
  final bool oscuro;
  final bool seleccionado;
  final VoidCallback onTap;

  const BotonFiltro({
    super.key,
    required this.texto,
    required this.oscuro,
    required this.seleccionado,
    required this.onTap,
  });

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

/// Fila horizontal de botones de filtro ("Todas" + las opciones).
class FilaFiltros extends StatelessWidget {
  final List<String> opciones;
  final String seleccionado;
  final ValueChanged<String> onCambio;
  final String textoTodos;

  const FilaFiltros({
    super.key,
    required this.opciones,
    required this.seleccionado,
    required this.onCambio,
    this.textoTodos = 'Todas',
  });

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      height: 40,
      child: ListView(
        scrollDirection: Axis.horizontal,
        children: [
          for (final nombre in [textoTodos, ...opciones])
            Padding(
              padding: const EdgeInsets.only(right: 8),
              child: BotonFiltro(
                texto: nombre,
                oscuro: nombre == textoTodos,
                seleccionado: nombre == seleccionado,
                onTap: () => onCambio(nombre),
              ),
            ),
        ],
      ),
    );
  }
}

/// Botón pequeño de acción con los colores de la web (Ver gris, Editar naranja, Eliminar rojo).
class BotonAccion extends StatelessWidget {
  final String texto;
  final Color color;
  final VoidCallback onPressed;

  const BotonAccion({super.key, required this.texto, required this.color, required this.onPressed});

  @override
  Widget build(BuildContext context) {
    return FilledButton(
      style: FilledButton.styleFrom(
        backgroundColor: color,
        minimumSize: const Size(0, 36),
        padding: const EdgeInsets.symmetric(horizontal: 14),
        textStyle: const TextStyle(fontSize: 13, fontWeight: FontWeight.bold),
      ),
      onPressed: onPressed,
      child: Text(texto),
    );
  }
}

/// Tarjeta de un registro de los maestros: título, datos debajo, etiquetas y botones de acción.
/// Es la misma información de una fila del data grid de la web, acomodada para celular.
class TarjetaRegistro extends StatelessWidget {
  final int id;
  final String titulo;
  final String? subtitulo;
  final List<Widget> etiquetas;
  final Widget? detalle;
  final List<Widget> acciones;

  const TarjetaRegistro({
    super.key,
    required this.id,
    required this.titulo,
    this.subtitulo,
    this.etiquetas = const [],
    this.detalle,
    this.acciones = const [],
  });

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 10),
      child: Card(
        child: Padding(
          padding: const EdgeInsets.all(14),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text('#$id', style: const TextStyle(fontWeight: FontWeight.bold, color: Colores.titulo, fontSize: 15)),
                  const SizedBox(width: 10),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          titulo,
                          style: const TextStyle(fontSize: 15, fontWeight: FontWeight.w600, color: Colores.titulo),
                        ),
                        if (subtitulo != null && subtitulo!.isNotEmpty) ...[
                          const SizedBox(height: 2),
                          Text(subtitulo!, style: const TextStyle(fontSize: 13, color: Colores.textoSuave)),
                        ],
                      ],
                    ),
                  ),
                ],
              ),
              if (etiquetas.isNotEmpty) ...[
                const SizedBox(height: 10),
                Wrap(spacing: 6, runSpacing: 6, children: etiquetas),
              ],
              if (detalle != null) ...[
                const SizedBox(height: 10),
                detalle!,
              ],
              if (acciones.isNotEmpty) ...[
                const SizedBox(height: 12),
                Wrap(alignment: WrapAlignment.end, spacing: 8, runSpacing: 8, children: acciones),
              ],
            ],
          ),
        ),
      ),
    );
  }
}

/// Aviso de "no hay registros".
class TarjetaVacia extends StatelessWidget {
  final String texto;

  const TarjetaVacia(this.texto, {super.key});

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Center(child: Text(texto, style: const TextStyle(color: Colores.texto))),
      ),
    );
  }
}

/// Encabezado de los formularios en hoja (título y botón para cerrar).
class EncabezadoFormulario extends StatelessWidget {
  final String titulo;

  const EncabezadoFormulario(this.titulo, {super.key});

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 16),
      child: Row(
        children: [
          Expanded(
            child: Text(titulo, style: const TextStyle(fontSize: 20, fontWeight: FontWeight.bold, color: Colores.titulo)),
          ),
          IconButton(icon: const Icon(Icons.close), tooltip: 'Cerrar', onPressed: () => Navigator.pop(context, false)),
        ],
      ),
    );
  }
}

/// Mensaje de error dentro de un formulario (el que devuelve la API).
class ErrorFormulario extends StatelessWidget {
  final String? mensaje;

  const ErrorFormulario(this.mensaje, {super.key});

  @override
  Widget build(BuildContext context) {
    if (mensaje == null) return const SizedBox.shrink();
    return Container(
      width: double.infinity,
      margin: const EdgeInsets.only(bottom: 14),
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: Colores.avisoFondo,
        borderRadius: BorderRadius.circular(6),
        border: Border.all(color: Colores.avisoBorde),
      ),
      child: Text(mensaje!, style: const TextStyle(color: Colores.avisoTexto)),
    );
  }
}
