import 'dart:async';
import 'dart:convert';

import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';

import '../config/api_config.dart';
import '../models/catalogos.dart';
import '../models/tarea.dart';

/// Error con un mensaje listo para mostrarle al usuario.
/// Cuando la API responde con { "mensaje": "..." }, ese es el mensaje que se guarda aquí.
class ApiException implements Exception {
  final String mensaje;

  ApiException(this.mensaje);

  @override
  String toString() => mensaje;
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
  // Llaves con las que se guarda la última lista de tareas en el teléfono (persistencia local)
  static const String _claveCache = 'tareas_cache';
  static const String _claveFechaCache = 'tareas_cache_fecha';

  static const Duration _tiempoMaximo = Duration(seconds: 8);

  final http.Client _cliente;

  TareaService({http.Client? cliente}) : _cliente = cliente ?? http.Client();

  // ---------------------------------------------------------------------
  // Utilidades internas
  // ---------------------------------------------------------------------

  Uri _url(String ruta) => Uri.parse('$apiBaseUrl$ruta');

  static const Map<String, String> _encabezadosJson = {'Content-Type': 'application/json'};

  /// Ejecuta una petición y convierte cualquier falla en un ApiException con un mensaje claro.
  Future<http.Response> _enviar(Future<http.Response> Function() peticion) async {
    try {
      final respuesta = await peticion().timeout(_tiempoMaximo);
      if (respuesta.statusCode >= 200 && respuesta.statusCode < 300) {
        return respuesta;
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
  String _leerMensaje(http.Response respuesta) {
    try {
      final datos = jsonDecode(utf8.decode(respuesta.bodyBytes));
      if (datos is Map<String, dynamic> && datos['mensaje'] is String) {
        return datos['mensaje'] as String;
      }
    } catch (_) {
      // La respuesta no era JSON: se usa el mensaje genérico
    }
    return 'Error del servidor (código ${respuesta.statusCode}).';
  }

  /// Decodifica el cuerpo en UTF-8 para que las tildes y la ñ se vean bien.
  dynamic _decodificar(http.Response respuesta) => jsonDecode(utf8.decode(respuesta.bodyBytes));

  // ---------------------------------------------------------------------
  // Tareas
  // ---------------------------------------------------------------------

  /// GET /api/Tareas. Si no hay conexión, devuelve la última lista guardada en el teléfono.
  Future<ResultadoTareas> obtenerTareas() async {
    try {
      final respuesta = await _enviar(() => _cliente.get(_url('/Tareas')));
      final cuerpo = utf8.decode(respuesta.bodyBytes);
      final lista = (jsonDecode(cuerpo) as List).map((e) => Tarea.fromJson(e as Map<String, dynamic>)).toList();

      await _guardarCache(cuerpo);
      return ResultadoTareas(tareas: lista);
    } on ApiException {
      final copia = await _leerCache();
      if (copia != null) return copia;
      rethrow;
    }
  }

  /// GET /api/Tareas/{id}: el detalle actualizado de una tarea.
  Future<Tarea> obtenerTarea(int id) async {
    final respuesta = await _enviar(() => _cliente.get(_url('/Tareas/$id')));
    return Tarea.fromJson(_decodificar(respuesta) as Map<String, dynamic>);
  }

  /// POST /api/Tareas
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
    await _enviar(() => _cliente.post(_url('/Tareas'), headers: _encabezadosJson, body: cuerpo));
  }

  /// PUT /api/Tareas/{id} cambiando solo el estado.
  /// El PUT de la API reemplaza la tarea completa, por eso se envían también los demás datos.
  Future<void> cambiarEstado(Tarea tarea, int idEstadoNuevo) async {
    final cuerpo = jsonEncode({
      'titulo': tarea.titulo,
      'descripcion': tarea.descripcion,
      'IdUsuarioResponsable': tarea.idUsuario,
      'id_estado': idEstadoNuevo,
      'id_proyecto': tarea.idProyecto,
      'prioridad': tarea.prioridad,
      'fecha_limite': tarea.fechaLimite,
    });
    await _enviar(() => _cliente.put(_url('/Tareas/${tarea.id}'), headers: _encabezadosJson, body: cuerpo));
  }

  /// DELETE /api/Tareas/{id}
  Future<void> eliminarTarea(int id) async {
    await _enviar(() => _cliente.delete(_url('/Tareas/$id')));
  }

  // ---------------------------------------------------------------------
  // Catálogos para el formulario
  // ---------------------------------------------------------------------

  Future<List<Estado>> obtenerEstados() async {
    final respuesta = await _enviar(() => _cliente.get(_url('/Estados')));
    return (_decodificar(respuesta) as List).map((e) => Estado.fromJson(e as Map<String, dynamic>)).toList();
  }

  Future<List<Proyecto>> obtenerProyectos() async {
    final respuesta = await _enviar(() => _cliente.get(_url('/Proyectos')));
    return (_decodificar(respuesta) as List).map((e) => Proyecto.fromJson(e as Map<String, dynamic>)).toList();
  }

  Future<List<Usuario>> obtenerUsuarios() async {
    final respuesta = await _enviar(() => _cliente.get(_url('/Usuarios')));
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
