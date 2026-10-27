import 'dart:async';
import 'dart:convert';

import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';

import '../config/api_config.dart';
import '../models/catalogos.dart';
import '../models/sesion.dart';
import '../models/tarea.dart';

/// Error con un mensaje listo para mostrarle al usuario.
/// Cuando la API responde con { "mensaje": "..." }, ese es el mensaje que se guarda aquí.
class ApiException implements Exception {
  final String mensaje;

  ApiException(this.mensaje);

  @override
  String toString() => mensaje;
}

/// La API respondió 401: el token venció o no es válido. Hay que volver a iniciar sesión.
class SesionExpiradaException extends ApiException {
  SesionExpiradaException() : super('Tu sesión expiró. Vuelve a iniciar sesión.');
}

/// Resultado de pedir la lista de tareas: las tareas y si vienen del servidor o de la copia guardada.
class ResultadoTareas {
  final List<Tarea> tareas;
  final bool desdeCache;
  final String? fechaCache;

  const ResultadoTareas({required this.tareas, this.desdeCache = false, this.fechaCache});
}

/// Servicio que consume la MISMA API que usa el dashboard web.
class TareaService {
  // Llaves con las que se guardan datos en el teléfono (persistencia local)
  static const String _claveCache = 'tareas_cache';
  static const String _claveFechaCache = 'tareas_cache_fecha';
  static const String _claveSesion = 'sesion';

  static const Duration _tiempoMaximo = Duration(seconds: 8);

  final http.Client _cliente;

  /// Sesión actual. Mientras sea null, la API rechaza las peticiones (401).
  Sesion? sesion;

  TareaService({http.Client? cliente}) : _cliente = cliente ?? http.Client();

  // ---------------------------------------------------------------------
  // Utilidades internas
  // ---------------------------------------------------------------------

  Uri _url(String ruta) => Uri.parse('$apiBaseUrl$ruta');

  /// Encabezados de cada petición: JSON y el token JWT (Authorization: Bearer ...)
  Map<String, String> get _encabezados => {
        'Content-Type': 'application/json',
        if (sesion != null) 'Authorization': 'Bearer ${sesion!.token}',
      };

  /// Ejecuta una petición y convierte cualquier falla en un ApiException con un mensaje claro.
  Future<http.Response> _enviar(Future<http.Response> Function() peticion) async {
    try {
      final respuesta = await peticion().timeout(_tiempoMaximo);
      if (respuesta.statusCode >= 200 && respuesta.statusCode < 300) {
        return respuesta;
      }
      if (respuesta.statusCode == 401) {
        throw SesionExpiradaException();
      }
      if (respuesta.statusCode == 403) {
        throw ApiException(_leerMensaje(respuesta, porDefecto: 'No tienes permiso para realizar esta acción.'));
      }
      throw ApiException(_leerMensaje(respuesta));
    } on ApiException {
      rethrow;
    } on TimeoutException {
      throw ApiException('El servidor tardó demasiado en responder.');
    } on Exception {
      throw ApiException('No se pudo conectar con el servidor. Revisa que el backend esté corriendo.');
    }
  }

  /// Lee el campo "mensaje" que devuelve la API en los errores (400, 404...).
  String _leerMensaje(http.Response respuesta, {String? porDefecto}) {
    try {
      final datos = jsonDecode(utf8.decode(respuesta.bodyBytes));
      if (datos is Map<String, dynamic> && datos['mensaje'] is String) {
        return datos['mensaje'] as String;
      }
    } catch (_) {
      // La respuesta no era JSON: se usa el mensaje genérico
    }
    return porDefecto ?? 'Error del servidor (código ${respuesta.statusCode}).';
  }

  /// Decodifica el cuerpo en UTF-8 para que las tildes y la ñ se vean bien.
  dynamic _decodificar(http.Response respuesta) => jsonDecode(utf8.decode(respuesta.bodyBytes));

  // ---------------------------------------------------------------------
  // Sesión (inicio de sesión con JWT)
  // ---------------------------------------------------------------------

  /// POST /api/Auth/login. Si los datos son correctos, guarda la sesión en el teléfono.
  Future<Sesion> iniciarSesion(String correo, String contrasena) async {
    final http.Response respuesta;
    try {
      respuesta = await _cliente
          .post(
            _url('/Auth/login'),
            headers: const {'Content-Type': 'application/json'},
            body: jsonEncode({'correo': correo, 'contrasena': contrasena}),
          )
          .timeout(_tiempoMaximo);
    } on TimeoutException {
      throw ApiException('El servidor tardó demasiado en responder.');
    } on Exception {
      throw ApiException('No se pudo conectar con el servidor. Revisa que el backend esté corriendo.');
    }

    // En el login, un 401 significa correo o contraseña incorrectos (no sesión vencida)
    if (respuesta.statusCode != 200) {
      throw ApiException(_leerMensaje(respuesta, porDefecto: 'No se pudo iniciar sesión.'));
    }

    final nuevaSesion = Sesion.desdeLogin(_decodificar(respuesta) as Map<String, dynamic>);
    sesion = nuevaSesion;

    try {
      final preferencias = await SharedPreferences.getInstance();
      await preferencias.setString(_claveSesion, jsonEncode(nuevaSesion.toJson()));
    } catch (_) {
      // Si no se puede guardar, la sesión dura mientras la app esté abierta
    }
    return nuevaSesion;
  }

  /// Recupera la sesión guardada al abrir la app. Devuelve null si no hay o si el token ya venció.
  Future<Sesion?> cargarSesionGuardada() async {
    try {
      final preferencias = await SharedPreferences.getInstance();
      final texto = preferencias.getString(_claveSesion);
      if (texto == null) return null;

      final guardada = Sesion.desdeLogin(jsonDecode(texto) as Map<String, dynamic>);
      if (!guardada.estaVigente) return null;

      sesion = guardada;
      return guardada;
    } catch (_) {
      return null;
    }
  }

  /// Borra la sesión y la copia de tareas (para que otro usuario no vea datos ajenos).
  Future<void> cerrarSesion() async {
    sesion = null;
    try {
      final preferencias = await SharedPreferences.getInstance();
      await preferencias.remove(_claveSesion);
      await preferencias.remove(_claveCache);
      await preferencias.remove(_claveFechaCache);
    } catch (_) {
      // Nada que borrar
    }
  }

  // ---------------------------------------------------------------------
  // Tareas
  // ---------------------------------------------------------------------

  /// GET /api/Tareas. Si no hay conexión, devuelve la última lista guardada en el teléfono.
  Future<ResultadoTareas> obtenerTareas() async {
    try {
      final respuesta = await _enviar(() => _cliente.get(_url('/Tareas'), headers: _encabezados));
      final cuerpo = utf8.decode(respuesta.bodyBytes);
      final lista = (jsonDecode(cuerpo) as List).map((e) => Tarea.fromJson(e as Map<String, dynamic>)).toList();

      await _guardarCache(cuerpo);
      return ResultadoTareas(tareas: lista);
    } on SesionExpiradaException {
      rethrow; // Sin sesión válida no se muestran datos guardados
    } on ApiException {
      final copia = await _leerCache();
      if (copia != null) return copia;
      rethrow;
    }
  }

  /// GET /api/Tareas/{id}: el detalle actualizado de una tarea.
  Future<Tarea> obtenerTarea(int id) async {
    final respuesta = await _enviar(() => _cliente.get(_url('/Tareas/$id'), headers: _encabezados));
    return Tarea.fromJson(_decodificar(respuesta) as Map<String, dynamic>);
  }

  /// POST /api/Tareas (solo Administrador y Líder de Proyecto)
  Future<void> crearTarea({
    required String titulo,
    String? descripcion,
    int? idUsuario,
    required int idEstado,
    int? idProyecto,
    String prioridad = 'Media',
    String? fechaLimite,
  }) async {
    final cuerpo = jsonEncode({
      'titulo': titulo,
      'descripcion': descripcion,
      'IdUsuarioResponsable': idUsuario,
      'id_estado': idEstado,
      'id_proyecto': idProyecto,
      'prioridad': prioridad,
      'fecha_limite': fechaLimite,
    });
    await _enviar(() => _cliente.post(_url('/Tareas'), headers: _encabezados, body: cuerpo));
  }

  /// PATCH /api/Tareas/{id}/estado: cambia solo el estado.
  /// Un Desarrollador puede usarlo con las tareas asignadas a él.
  Future<void> cambiarEstado(int idTarea, int idEstadoNuevo) async {
    final cuerpo = jsonEncode({'id_estado': idEstadoNuevo});
    await _enviar(() => _cliente.patch(_url('/Tareas/$idTarea/estado'), headers: _encabezados, body: cuerpo));
  }

  /// DELETE /api/Tareas/{id} (solo Administrador)
  Future<void> eliminarTarea(int id) async {
    await _enviar(() => _cliente.delete(_url('/Tareas/$id'), headers: _encabezados));
  }

  // ---------------------------------------------------------------------
  // Catálogos para el formulario
  // ---------------------------------------------------------------------

  Future<List<Estado>> obtenerEstados() async {
    final respuesta = await _enviar(() => _cliente.get(_url('/Estados'), headers: _encabezados));
    return (_decodificar(respuesta) as List).map((e) => Estado.fromJson(e as Map<String, dynamic>)).toList();
  }

  Future<List<Proyecto>> obtenerProyectos() async {
    final respuesta = await _enviar(() => _cliente.get(_url('/Proyectos'), headers: _encabezados));
    return (_decodificar(respuesta) as List).map((e) => Proyecto.fromJson(e as Map<String, dynamic>)).toList();
  }

  Future<List<Usuario>> obtenerUsuarios() async {
    final respuesta = await _enviar(() => _cliente.get(_url('/Usuarios'), headers: _encabezados));
    return (_decodificar(respuesta) as List).map((e) => Usuario.fromJson(e as Map<String, dynamic>)).toList();
  }

  // ---------------------------------------------------------------------
  // Persistencia local básica (shared_preferences)
  // ---------------------------------------------------------------------

  Future<void> _guardarCache(String cuerpoJson) async {
    try {
      final preferencias = await SharedPreferences.getInstance();
      await preferencias.setString(_claveCache, cuerpoJson);
      await preferencias.setString(_claveFechaCache, DateTime.now().toString().substring(0, 16));
    } catch (_) {
      // Si no se puede guardar la copia, la app sigue funcionando igual
    }
  }

  Future<ResultadoTareas?> _leerCache() async {
    try {
      final preferencias = await SharedPreferences.getInstance();
      final cuerpo = preferencias.getString(_claveCache);
      if (cuerpo == null) return null;

      final lista = (jsonDecode(cuerpo) as List).map((e) => Tarea.fromJson(e as Map<String, dynamic>)).toList();
      return ResultadoTareas(
        tareas: lista,
        desdeCache: true,
        fechaCache: preferencias.getString(_claveFechaCache),
      );
    } catch (_) {
      return null;
    }
  }
}
