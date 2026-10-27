import 'package:flutter/material.dart';

import '../services/tarea_service.dart';
import 'lista_tareas_screen.dart';

class LoginScreen extends StatefulWidget {
  final TareaService servicio;

  /// Mensaje opcional para mostrar al llegar (por ejemplo, "Tu sesión expiró").
  final String? mensaje;

  const LoginScreen({super.key, required this.servicio, this.mensaje});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _formKey = GlobalKey<FormState>();
  final _correoController = TextEditingController();
  final _contrasenaController = TextEditingController();

  bool _cargando = false;
  bool _ocultarContrasena = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _error = widget.mensaje;
  }

  @override
  void dispose() {
    _correoController.dispose();
    _contrasenaController.dispose();
    super.dispose();
  }

  /// POST /api/Auth/login. Si sale bien, abre la lista de tareas.
  Future<void> _ingresar() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() {
      _cargando = true;
      _error = null;
    });

    try {
      await widget.servicio.iniciarSesion(_correoController.text.trim(), _contrasenaController.text);
      if (!mounted) return;
      Navigator.pushReplacement(
        context,
        MaterialPageRoute(builder: (_) => ListaTareasScreen(servicio: widget.servicio)),
      );
    } on ApiException catch (e) {
      if (!mounted) return;
      setState(() {
        _error = e.mensaje;
        _cargando = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFF0F172A),
      body: Center(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(24),
          child: Card(
            child: Padding(
              padding: const EdgeInsets.all(24),
              child: Form(
                key: _formKey,
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    const Text(
                      'ESA SAS',
                      textAlign: TextAlign.center,
                      style: TextStyle(fontSize: 28, fontWeight: FontWeight.bold, color: Color(0xFF0EA5E9), letterSpacing: 2),
                    ),
                    const SizedBox(height: 4),
                    const Text('Sistema de Gestión de Tareas', textAlign: TextAlign.center),
                    const SizedBox(height: 24),
                    TextFormField(
                      controller: _correoController,
                      keyboardType: TextInputType.emailAddress,
                      autofillHints: const [AutofillHints.email],
                      decoration: const InputDecoration(labelText: 'Correo electrónico', border: OutlineInputBorder()),
                      validator: (valor) => (valor == null || valor.trim().isEmpty) ? 'Escribe tu correo' : null,
                    ),
                    const SizedBox(height: 12),
                    TextFormField(
                      controller: _contrasenaController,
                      obscureText: _ocultarContrasena,
                      autofillHints: const [AutofillHints.password],
                      decoration: InputDecoration(
                        labelText: 'Contraseña',
                        border: const OutlineInputBorder(),
                        suffixIcon: IconButton(
                          icon: Icon(_ocultarContrasena ? Icons.visibility : Icons.visibility_off),
                          tooltip: _ocultarContrasena ? 'Mostrar contraseña' : 'Ocultar contraseña',
                          onPressed: () => setState(() => _ocultarContrasena = !_ocultarContrasena),
                        ),
                      ),
                      validator: (valor) => (valor == null || valor.isEmpty) ? 'Escribe tu contraseña' : null,
                      onFieldSubmitted: (_) => _ingresar(),
                    ),
                    if (_error != null) ...[
                      const SizedBox(height: 12),
                      Text(
                        _error!,
                        textAlign: TextAlign.center,
                        style: const TextStyle(color: Color(0xFFDC2626), fontWeight: FontWeight.bold),
                      ),
                    ],
                    const SizedBox(height: 20),
                    FilledButton(
                      onPressed: _cargando ? null : _ingresar,
                      child: _cargando
                          ? const SizedBox(width: 20, height: 20, child: CircularProgressIndicator(strokeWidth: 2))
                          : const Text('Ingresar'),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}
