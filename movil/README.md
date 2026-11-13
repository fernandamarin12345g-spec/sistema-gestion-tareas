# Aplicación móvil (Flutter)

Cliente móvil del **Sistema de Gestión y Seguimiento de Tareas**. Usa la misma API y la misma base de datos que la web y el escritorio.

## Secciones

Igual que en la web, desde el menú lateral:

- **Inicio:** indicadores y paneles del dashboard, con la lista de tareas atrasadas.
- **Tareas:** listado con filtros por estado, detalle, cambio de estado, creación y eliminación según el rol.
- **Proyectos:** tareas abiertas y totales de cada proyecto, con "Ver tareas".
- **Estados, Roles y Usuarios:** solo para el Administrador; crear, editar y eliminar.

## Estructura

```
lib/
├── config/     Dirección de la API (api_config.dart) y colores del tema (tema.dart)
├── models/     Tarea, Sesion y los maestros (catalogos.dart)
├── services/   TareaService: llamadas a la API, sesión JWT y copia local de tareas
├── screens/    Una pantalla por sección, más login, detalle y formulario de tarea
├── utils/      Formato de fechas y etiquetas de colores
└── widgets/    Menú lateral y piezas comunes (tarjetas, indicadores, filtros)
test/
└── widget_test.dart   7 pruebas de widgets
```

## Ejecutar

Con la API encendida (Visual Studio o Docker, puerto 5293) y un emulador de Android abierto:

```powershell
flutter pub get
flutter run
```

Desde el emulador, la API se encuentra en `http://10.0.2.2:5293/api` (ver `lib/config/api_config.dart`).

## Pruebas

```powershell
flutter analyze
flutter test
```
