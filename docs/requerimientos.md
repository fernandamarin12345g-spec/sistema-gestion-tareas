# Documento de Requerimientos y Casos de Uso

**Proyecto:** Sistema de Gestión y Seguimiento de Tareas
**Empresa:** Strategic Software Alliance S.A.S. (ESA SAS)
**Versión del documento:** 2.0 (actualizada al cierre de la semana 13)

## 1. Descripción general

El sistema centraliza las actividades de un equipo de desarrollo de software. Permite registrar tareas, asignarles un responsable, un proyecto, una prioridad y una fecha límite, y seguir su avance por estados. Un dashboard muestra la carga de trabajo del equipo y las tareas atrasadas.

El sistema está formado por una API REST (ASP.NET Core), una base de datos SQLite y tres clientes que usan la misma API: un dashboard web, una aplicación de escritorio para Windows y una aplicación móvil en Flutter.

## 2. Alcance

**Incluye:** inicio de sesión, permisos por rol, gestión de tareas, maestros de Estados, Proyectos, Roles y Usuarios, dashboard de seguimiento, cálculo de tareas atrasadas, copia local de tareas en el móvil y ejecución con Docker.

**No incluye en esta versión:** etiquetas, registro de auditoría, notificaciones, adjuntos ni control de horas trabajadas (ver la sección 9, trabajo futuro).

## 3. Actores

| Actor | Descripción | Qué puede hacer |
| --- | --- | --- |
| **Administrador** | Responsable del sistema (rol con `is_admin = 1`) | Todo: tareas, eliminar tareas, maestros de Estados, Proyectos, Roles y Usuarios |
| **Líder de Proyecto** | Coordina al equipo (rol con `gestiona_tareas = 1`) | Crear, editar y asignar tareas, y cambiar el estado de cualquier tarea |
| **Desarrollador** | Integrante del equipo (rol sin permisos especiales) | Consultar tareas y proyectos, y cambiar el estado de las tareas asignadas a él |

Los permisos no dependen del nombre del rol: se leen de las columnas `is_admin` y `gestiona_tareas` de la tabla `Roles`. Así el Administrador puede crear un rol nuevo y decidir qué puede hacer sin cambiar el código.

## 4. Requerimientos funcionales

| ID | Requerimiento | Prioridad | Estado |
| --- | --- | :---: | :---: |
| **RF-01** | El sistema debe permitir iniciar sesión con correo y contraseña, y entregar un token JWT con el usuario, su rol y sus permisos. | Alta | Implementado |
| **RF-02** | El sistema debe rechazar las peticiones sin token o con el token vencido (401), y las acciones sin permiso (403). | Alta | Implementado |
| **RF-03** | El sistema debe permitir crear tareas con título, descripción, responsable, estado, proyecto, prioridad (Baja, Media, Alta o Crítica) y fecha límite. | Alta | Implementado |
| **RF-04** | El sistema debe permitir consultar el listado de tareas y el detalle de cada una, con sus fechas de creación, actualización y finalización. | Alta | Implementado |
| **RF-05** | El sistema debe permitir editar una tarea (Administrador y Líder de Proyecto). | Alta | Implementado |
| **RF-06** | El sistema debe permitir cambiar solo el estado de una tarea. Un Desarrollador solo puede hacerlo en sus propias tareas. | Alta | Implementado |
| **RF-07** | El sistema debe permitir eliminar tareas (solo Administrador), pidiendo confirmación antes. | Media | Implementado |
| **RF-08** | El sistema debe marcar como **atrasada** una tarea cuya fecha límite ya pasó y cuyo estado no es final. | Alta | Implementado |
| **RF-09** | El sistema debe permitir filtrar las tareas por estado, responsable, proyecto, prioridad y "solo atrasadas". | Media | Implementado |
| **RF-10** | El sistema debe mostrar un dashboard con el total de tareas, abiertas, atrasadas y cerradas, y las tareas por estado, por responsable y por proyecto. | Alta | Implementado |
| **RF-11** | El sistema debe tener un maestro de **Estados** editable, donde cada estado indique si cierra la tarea. | Alta | Implementado |
| **RF-12** | El sistema debe tener un maestro de **Proyectos** con nombre, descripción y estado (Planificación, Activo, Pausado o Finalizado), que muestre cuántas tareas tiene cada uno. | Media | Implementado |
| **RF-13** | El sistema debe tener un maestro de **Roles** con sus permisos (administrador y gestiona tareas). | Alta | Implementado |
| **RF-14** | El sistema debe tener un maestro de **Usuarios** con nombre, correo, rol y contraseña, que muestre las tareas asignadas a cada uno. | Alta | Implementado |
| **RF-15** | El sistema no debe permitir eliminar un estado, rol o proyecto que esté en uso, y debe explicar el motivo. | Media | Implementado |
| **RF-16** | El sistema no debe permitir nombres repetidos en estados, roles y proyectos, ni correos repetidos en usuarios. | Media | Implementado |
| **RF-17** | La aplicación móvil debe mostrar la última lista de tareas guardada cuando no haya conexión con el servidor. | Media | Implementado |
| **RF-18** | Las tres aplicaciones (web, escritorio y móvil) deben mostrar las mismas secciones según el rol del usuario. | Media | Implementado |

## 5. Requerimientos no funcionales

| ID | Tipo | Requerimiento |
| --- | --- | --- |
| **RNF-01** | Seguridad | Las contraseñas se guardan como hash PBKDF2-SHA256 con sal y 100.000 iteraciones; nunca en texto plano. |
| **RNF-02** | Seguridad | Todas las consultas a la base de datos usan parámetros (`@nombre`) para evitar inyección SQL. |
| **RNF-03** | Seguridad | La clave para firmar los tokens JWT no se guarda en el repositorio; se pasa como variable de entorno. |
| **RNF-04** | Integridad | Los datos se validan en la API antes de guardarlos (campos obligatorios, longitudes, formato de correo y fecha, valores permitidos). |
| **RNF-05** | Usabilidad | Las tres aplicaciones usan los mismos colores, la misma barra lateral y los mismos nombres de secciones. |
| **RNF-06** | Usabilidad | Los errores se muestran con mensajes claros en español, tomados de la respuesta de la API. |
| **RNF-07** | Portabilidad | La API y la web se pueden ejecutar en cualquier equipo con Docker, con un solo comando (`docker compose up`). |
| **RNF-08** | Mantenibilidad | El código está organizado por capas (controladores, seguridad, servicios, pantallas) y comentado en español. |
| **RNF-09** | Compatibilidad | La web funciona en navegadores modernos; el escritorio en Windows 10/11; el móvil en Android. |
| **RNF-10** | Disponibilidad de datos | En Docker, la base de datos vive en un volumen, por lo que no se pierde al reconstruir el contenedor. |

## 6. Reglas de negocio

- **RN-01:** Una tarea está atrasada si `fecha_limite` es anterior a hoy y su estado tiene `es_final = 0`.
- **RN-02:** Al pasar una tarea a un estado final se guarda la `fecha_finalizacion`; si vuelve a un estado no final, esa fecha se borra.
- **RN-03:** Cada edición de una tarea actualiza su `fecha_actualizacion`.
- **RN-04:** Al crear un usuario la contraseña es obligatoria (mínimo 8 caracteres). Al editarlo, si se deja vacía, no se cambia.
- **RN-05:** No se puede eliminar un proyecto que tenga tareas, para no perderlas.
- **RN-06:** Un Desarrollador no puede reabrir una tarea cerrada (pasarla de un estado final a uno no final); eso lo hace un Líder de Proyecto o el Administrador.

## 7. Casos de uso

### CU-01 Iniciar sesión
- **Actor:** cualquier usuario.
- **Flujo principal:** el usuario escribe su correo y contraseña → la API valida los datos → devuelve un token JWT → la aplicación muestra las secciones permitidas para su rol.
- **Flujo alterno:** si los datos son incorrectos, se muestra "Correo o contraseña incorrectos" y no se entra.

### CU-02 Crear una tarea
- **Actor:** Administrador o Líder de Proyecto.
- **Precondición:** haber iniciado sesión.
- **Flujo principal:** abre el formulario → escribe el título y elige estado, prioridad, responsable, proyecto y fecha límite → la API valida y guarda la tarea con su fecha de creación → aparece en el listado y en el dashboard.
- **Flujo alterno:** si falta el título o un dato no es válido, la API responde 400 y se muestra el mensaje.

### CU-03 Cambiar el estado de una tarea
- **Actor:** Administrador, Líder de Proyecto o el Desarrollador responsable.
- **Flujo principal:** abre el detalle de la tarea → elige el nuevo estado → la API actualiza el estado y las fechas.
- **Flujo alterno:** si un Desarrollador intenta cambiar una tarea ajena, la API responde 403 y la aplicación no muestra esa opción.

### CU-04 Consultar el dashboard
- **Actor:** cualquier usuario.
- **Flujo principal:** entra a Inicio → ve los indicadores y los paneles → desde "Tareas atrasadas" abre el detalle de una tarea con el botón Ver.

### CU-05 Gestionar los estados
- **Actor:** Administrador.
- **Flujo principal:** entra a Estados → crea "En pruebas" sin marcar "Cierra la tarea" → el nuevo estado aparece en los filtros y en los formularios de las tres aplicaciones.
- **Flujo alterno:** si intenta eliminar un estado que usan algunas tareas, la API lo impide y explica el motivo.

### CU-06 Crear un rol y un usuario
- **Actor:** Administrador.
- **Flujo principal:** crea el rol "Analista QA" marcando solo "Gestiona tareas" → crea un usuario con ese rol y una contraseña → el usuario inicia sesión y puede crear tareas, pero no ve los maestros.

### CU-07 Ver las tareas de un proyecto
- **Actor:** cualquier usuario.
- **Flujo principal:** entra a Proyectos → ve las tareas abiertas y totales de cada uno → pulsa "Ver tareas" → se abre el listado filtrado por ese proyecto.

### CU-08 Consultar tareas sin conexión (móvil)
- **Actor:** cualquier usuario en el celular.
- **Flujo principal:** abre la app sin conexión con el servidor → la app muestra la última lista guardada, con un aviso y la fecha en que se guardó.

## 8. Trazabilidad

| Requerimiento | Dónde se implementa | Cómo se prueba |
| --- | --- | --- |
| RF-01, RF-02 | `AuthController`, `Seguridad/`, `Program.cs` | `Backend.http` casos 0 y 37 a 42 |
| RF-03 a RF-09 | `TareasController` y las pantallas de tareas de los tres clientes | `Backend.http` casos 1 a 6 y 21 a 28; pruebas de widgets del móvil |
| RF-10 | Sección Inicio (web, escritorio y móvil) | Matriz de pruebas, casos de dashboard |
| RF-11 a RF-16 | `EstadosController`, `ProyectosController`, `RolesController`, `UsuariosController` | `Backend.http` casos 7 a 20 y 29 a 36 |
| RF-17 | `movil/lib/services/tarea_service.dart` | Prueba "Sin conexión" del móvil |
| RF-18 | Menú lateral de los tres clientes | Pruebas del menú del móvil; evidencias de las bitácoras 12 y 13 |

## 9. Trabajo futuro

Estas ideas se plantearon al inicio del proyecto y quedan para una siguiente versión. La base de datos ya tiene las tablas reservadas para las dos primeras:

- **Etiquetas** para agrupar tareas por tecnología o área (`Etiqueta` y `Tarea_Etiqueta`).
- **Registro de auditoría** con el usuario, la acción y la fecha de cada cambio (`Auditoria_Log`).
- Permisos más detallados por acción (por ejemplo, `crear_tarea` o `borrar_proyecto`) en lugar de los dos permisos actuales.
- Porcentaje de avance que cambie el estado automáticamente, y horas estimadas frente a horas reales.
- Notificaciones cuando una tarea esté por vencer.
