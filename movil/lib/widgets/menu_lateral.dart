import 'package:flutter/material.dart';

import '../config/tema.dart';
import '../models/sesion.dart';

/// Menú lateral oscuro, igual a la barra lateral de la web y del escritorio:
/// logo "ESA SAS", la sección actual y, abajo, el usuario con "Cerrar sesión".
class MenuLateral extends StatelessWidget {
  final Sesion? sesion;
  final VoidCallback onCerrarSesion;

  const MenuLateral({super.key, required this.sesion, required this.onCerrarSesion});

  @override
  Widget build(BuildContext context) {
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
              // Enlace activo: fondo más claro y texto celeste (nav a.activo)
              Container(
                decoration: BoxDecoration(color: Colores.lateralHover, borderRadius: BorderRadius.circular(8)),
                child: ListTile(
                  leading: const Icon(Icons.checklist_rounded, color: Colores.celeste),
                  title: const Text(
                    'Tareas',
                    style: TextStyle(color: Colores.celeste, fontWeight: FontWeight.w600, fontSize: 16),
                  ),
                  onTap: () => Navigator.pop(context),
                ),
              ),
              const Spacer(),
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
                  onCerrarSesion();
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
