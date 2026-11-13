import 'package:flutter/material.dart';

/// Colores del sistema: los mismos códigos de src/styles.css (web) y de Estilo.cs (escritorio).
class Colores {
  Colores._();

  // Barra lateral / encabezado oscuro
  static const lateral = Color(0xFF0F172A);
  static const lateralHover = Color(0xFF1E293B);
  static const celeste = Color(0xFF38BDF8);
  static const textoMenu = Color(0xFF94A3B8);
  static const textoClaro = Color(0xFFE2E8F0);

  // Contenido
  static const fondo = Color(0xFFF1F5F9);
  static const borde = Color(0xFFE2E8F0);
  static const bordeCampo = Color(0xFFCBD5E1);
  static const titulo = Color(0xFF0F172A);
  static const texto = Color(0xFF334155);
  static const textoSuave = Color(0xFF64748B);
  static const textoEtiqueta = Color(0xFF475569);
  static const textoTenue = Color(0xFF94A3B8);

  // Botones
  static const azul = Color(0xFF0EA5E9);
  static const rojo = Color(0xFFEF4444);

  // Badges y avisos
  static const estadoFondo = Color(0xFFE0F2FE);
  static const estadoTexto = Color(0xFF0369A1);
  static const estadoBorde = Color(0xFFBAE6FD);
  static const filaAtrasada = Color(0xFFFFF7F7);
  static const textoAtrasada = Color(0xFFDC2626);
  static const avisoFondo = Color(0xFFFEE2E2);
  static const avisoTexto = Color(0xFFB91C1C);
  static const avisoBorde = Color(0xFFFCA5A5);
}

/// Tema de la app: fondo gris claro, encabezado azul oscuro, tarjetas blancas con borde,
/// campos con borde gris (celeste al enfocarlos) y botones azules, igual que la web.
ThemeData temaApp() {
  final bordeCampo = OutlineInputBorder(
    borderRadius: BorderRadius.circular(6),
    borderSide: const BorderSide(color: Colores.bordeCampo),
  );

  return ThemeData(
    useMaterial3: true,
    colorScheme: ColorScheme.fromSeed(seedColor: Colores.azul).copyWith(
      primary: Colores.azul,
      onPrimary: Colors.white,
      secondary: Colores.celeste,
      surface: Colors.white,
      error: Colores.textoAtrasada,
    ),
    scaffoldBackgroundColor: Colores.fondo,
    appBarTheme: const AppBarTheme(
      backgroundColor: Colores.lateral,
      foregroundColor: Colors.white,
      elevation: 0,
      scrolledUnderElevation: 0,
      titleTextStyle: TextStyle(color: Colors.white, fontSize: 20, fontWeight: FontWeight.bold),
    ),
    cardTheme: CardThemeData(
      color: Colors.white,
      elevation: 0,
      margin: EdgeInsets.zero,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(8),
        side: const BorderSide(color: Colores.borde),
      ),
    ),
    inputDecorationTheme: InputDecorationTheme(
      filled: true,
      fillColor: Colors.white,
      contentPadding: const EdgeInsets.symmetric(horizontal: 12, vertical: 14),
      border: bordeCampo,
      enabledBorder: bordeCampo,
      focusedBorder: bordeCampo.copyWith(borderSide: const BorderSide(color: Colores.celeste, width: 2)),
      labelStyle: const TextStyle(color: Colores.textoEtiqueta),
      floatingLabelStyle: const TextStyle(color: Colores.estadoTexto, fontWeight: FontWeight.w600),
    ),
    filledButtonTheme: FilledButtonThemeData(
      style: FilledButton.styleFrom(
        backgroundColor: Colores.azul,
        foregroundColor: Colors.white,
        minimumSize: const Size(0, 48),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(6)),
        textStyle: const TextStyle(fontSize: 15, fontWeight: FontWeight.bold),
      ),
    ),
    floatingActionButtonTheme: const FloatingActionButtonThemeData(
      backgroundColor: Colores.azul,
      foregroundColor: Colors.white,
    ),
    snackBarTheme: const SnackBarThemeData(
      backgroundColor: Colores.lateral,
      behavior: SnackBarBehavior.floating,
    ),
    progressIndicatorTheme: const ProgressIndicatorThemeData(color: Colores.azul),
    dividerTheme: const DividerThemeData(color: Colores.borde),
  );
}
