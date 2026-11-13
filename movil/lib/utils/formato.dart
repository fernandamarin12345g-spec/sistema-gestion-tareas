import 'package:flutter/material.dart';

import '../config/tema.dart';

/// Convierte "2026-10-26" en "26/10/2026" y "2026-10-26 17:40:06" en "26/10/2026 17:40".
String formatearFecha(String? texto) {
  if (texto == null || texto.isEmpty) return '—';
  final partes = texto.split(' ');
  final fecha = partes[0].split('-');
  if (fecha.length != 3) return texto;
  final fechaBonita = '${fecha[2]}/${fecha[1]}/${fecha[0]}';
  if (partes.length < 2) return fechaBonita;
  final hora = partes[1].length >= 5 ? partes[1].substring(0, 5) : partes[1];
  return '$fechaBonita $hora';
}

/// Convierte un DateTime en el formato que espera la API: AAAA-MM-DD.
String fechaParaApi(DateTime fecha) {
  final mes = fecha.month.toString().padLeft(2, '0');
  final dia = fecha.day.toString().padLeft(2, '0');
  return '${fecha.year}-$mes-$dia';
}

/// Colores de fondo y texto para cada prioridad (los mismos de la web).
({Color fondo, Color texto}) coloresPrioridad(String prioridad) {
  switch (prioridad) {
    case 'Baja':
      return (fondo: const Color(0xFFF1F5F9), texto: const Color(0xFF475569));
    case 'Alta':
      return (fondo: const Color(0xFFFFEDD5), texto: const Color(0xFFC2410C));
    case 'Crítica':
      return (fondo: const Color(0xFFFEE2E2), texto: const Color(0xFFB91C1C));
    default: // Media
      return (fondo: const Color(0xFFFEF9C3), texto: const Color(0xFF854D0E));
  }
}

/// Píldora de color (.badge-prioridad, "Atrasada", etc.).
class Etiqueta extends StatelessWidget {
  final String texto;
  final Color fondo;
  final Color colorTexto;
  final Color? borde;

  const Etiqueta({super.key, required this.texto, required this.fondo, required this.colorTexto, this.borde});

  /// Píldora azul del estado (.badge-estado).
  const Etiqueta.estado(this.texto, {super.key})
      : fondo = Colores.estadoFondo,
        colorTexto = Colores.estadoTexto,
        borde = Colores.estadoBorde;

  /// Píldora con los colores de la prioridad.
  factory Etiqueta.prioridad(String prioridad, {Key? key}) {
    final colores = coloresPrioridad(prioridad);
    return Etiqueta(key: key, texto: prioridad, fondo: colores.fondo, colorTexto: colores.texto);
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
      decoration: BoxDecoration(
        color: fondo,
        borderRadius: BorderRadius.circular(20),
        border: borde == null ? null : Border.all(color: borde!),
      ),
      child: Text(
        texto,
        style: TextStyle(color: colorTexto, fontSize: 12, fontWeight: FontWeight.bold),
      ),
    );
  }
}
