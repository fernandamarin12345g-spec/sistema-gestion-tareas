import 'package:flutter/material.dart';

import '../config/tema.dart';
import '../screens/estados_screen.dart';
import '../screens/inicio_screen.dart';
import '../screens/lista_tareas_screen.dart';
import '../screens/proyectos_screen.dart';
import '../screens/roles_screen.dart';
import '../screens/usuarios_screen.dart';
import '../services/tarea_service.dart';
import 'comunes.dart';

/// Secciones de la app: las mismas del menú lateral de la web y del escritorio.
enum Seccion { inicio, tareas, proyectos, estados, roles, usuarios }

/// Datos de cada opción del menú.
class _OpcionMenu {
  final Seccion seccion;
  final String texto;
  final IconData icono;
  final bool soloAdmin;

  const _OpcionMenu(this.seccion, this.texto, this.icono, {this.soloAdmin = false});
}

const _opciones = [
  _OpcionMenu(Seccion.inicio, 'Inicio', Icons.dashboard_outlined),
  _OpcionMenu(Seccion.tareas, 'Tareas', Icons.checklist_rounded),
  _OpcionMenu(Seccion.proyectos, 'Proyectos', Icons.folder_outlined),
  // Los maestros solo los ve el Administrador (igual que en la web y el escritorio)
  _OpcionMenu(Seccion.estados, 'Estados', Icons.flag_outlined, soloAdmin: true),
  _OpcionMenu(Seccion.roles, 'Roles', Icons.badge_outlined, soloAdmin: true),
  _OpcionMenu(Seccion.usuarios, 'Usuarios', Icons.people_outline, soloAdmin: true),
];

/// Menú lateral oscuro, igual a la barra lateral de la web y del escritorio:
/// logo "ESA SAS", las secciones (la actual resaltada) y, abajo, el usuario con "Cerrar sesión".
class MenuLateral extends StatelessWidget {
  final TareaService servicio;
  final Seccion actual;

  const MenuLateral({super.key, required this.servicio, required this.actual});

  /// Pantalla que corresponde a cada sección.
  static Widget pantallaDe(Seccion seccion, TareaService servicio) {
    switch (seccion) {
      case Seccion.inicio:
        return InicioScreen(servicio: servicio);
      case Seccion.tareas:
        return ListaTareasScreen(servicio: servicio);
      case Seccion.proyectos:
        return ProyectosScreen(servicio: servicio);
      case Seccion.estados:
        return EstadosScreen(servicio: servicio);
      case Seccion.roles:
        return RolesScreen(servicio: servicio);
      case Seccion.usuarios:
        return UsuariosScreen(servicio: servicio);
    }
  }

  /// Cambia de sección reemplazando la pantalla actual (sin animación, como al cambiar de pestaña).
  static void irA(BuildContext context, Seccion seccion, TareaService servicio) {
    Navigator.of(context).pushAndRemoveUntil(
      PageRouteBuilder(
        pageBuilder: (_, _, _) => pantallaDe(seccion, servicio),
        transitionDuration: Duration.zero,
        reverseTransitionDuration: Duration.zero,
      ),
      (_) => false,
    );
  }

  @override
  Widget build(BuildContext context) {
    final sesion = servicio.sesion;
    final esAdmin = sesion?.esAdmin ?? false;
    final visibles = _opciones.where((o) => !o.soloAdmin || esAdmin).toList();

    return Drawer(
      backgroundColor: Colores.lateral,
      shape: const RoundedRectangleBorder(),
      child: SafeArea(
        child: Padding(
          padding: const EdgeInsets.fromLTRB(20, 28, 20, 20),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              const Text(
                'ESA SAS',
                textAlign: TextAlign.center,
                style: TextStyle(color: Colores.celeste, fontSize: 24, fontWeight: FontWeight.bold, letterSpacing: 2),
              ),
              const SizedBox(height: 32),
              Expanded(
                child: ListView(
                  padding: EdgeInsets.zero,
                  children: [
                    for (final opcion in visibles)
                      Padding(
                        padding: const EdgeInsets.only(bottom: 6),
                        child: _ItemMenu(
                          opcion: opcion,
                          activo: opcion.seccion == actual,
                          onTap: () {
                            Navigator.pop(context); // Cierra el menú
                            if (opcion.seccion != actual) {
                              irA(context, opcion.seccion, servicio);
                            }
                          },
                        ),
                      ),
                  ],
                ),
              ),
              const Divider(color: Colores.lateralHover, height: 1),
              const SizedBox(height: 16),
              Text(
                sesion?.nombre ?? '',
                style: const TextStyle(color: Colores.textoClaro, fontWeight: FontWeight.bold, fontSize: 15),
              ),
              const SizedBox(height: 2),
              Text(sesion?.rol ?? '', style: const TextStyle(color: Colores.textoMenu, fontSize: 13)),
              const SizedBox(height: 14),
              FilledButton(
                style: FilledButton.styleFrom(
                  backgroundColor: Colores.lateralHover,
                  foregroundColor: Colores.textoClaro,
                  minimumSize: const Size(0, 42),
                ),
                onPressed: () {
                  Navigator.pop(context);
                  cerrarSesionYSalir(context, servicio);
                },
                child: const Text('Cerrar sesión'),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

/// Un enlace del menú. El activo lleva fondo más claro y texto celeste (nav a.activo de la web).
class _ItemMenu extends StatelessWidget {
  final _OpcionMenu opcion;
  final bool activo;
  final VoidCallback onTap;

  const _ItemMenu({required this.opcion, required this.activo, required this.onTap});

  @override
  Widget build(BuildContext context) {
    final color = activo ? Colores.celeste : Colores.textoMenu;
    return Material(
      color: activo ? Colores.lateralHover : Colors.transparent,
      borderRadius: BorderRadius.circular(8),
      child: ListTile(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
        leading: Icon(opcion.icono, color: color),
        title: Text(
          opcion.texto,
          style: TextStyle(color: color, fontWeight: activo ? FontWeight.w600 : FontWeight.w500, fontSize: 16),
        ),
        onTap: onTap,
      ),
    );
  }
}
