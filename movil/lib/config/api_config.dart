import 'package:flutter/foundation.dart';

/// Dirección base de la API del backend (ASP.NET Core).
///
/// - En el emulador de Android, "localhost" es el propio emulador, no el computador.
///   Por eso se usa 10.0.2.2, que es la dirección del computador vista desde el emulador.
/// - En web o en Windows (la app corre en el mismo computador) se usa localhost.
///
/// Se usa el puerto HTTP 5293 (no el HTTPS 7019) porque el emulador no confía
/// en el certificado de desarrollo de HTTPS de .NET.
String get apiBaseUrl {
  if (!kIsWeb && defaultTargetPlatform == TargetPlatform.android) {
    return 'http://10.0.2.2:5293/api';
  }
  return 'http://localhost:5293/api';
}
