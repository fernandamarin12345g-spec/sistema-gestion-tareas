# Matriz de Pruebas Funcionales y de API

**Proyecto:** Sistema de Gestión y Seguimiento de Tareas
**Última actualización:** semana 14 (noviembre de 2026)

**Cómo se probó:**
- **API:** con el archivo `Backend/Backend.http` desde Visual Studio (44 casos, numerados del 0 al 44).
- **Móvil:** pruebas automáticas de widgets con `flutter test` y pruebas manuales en el emulador de Android.
- **Web y escritorio:** pruebas manuales con los usuarios de cada rol, con capturas en las bitácoras.
- **Docker:** levantando los contenedores con `docker compose` y usando las tres aplicaciones contra ellos.

## Resumen

| Módulo | Casos | Aprobados | Por ejecutar |
| --- | :---: | :---: | :---: |
| 1. Web (navegación y maestros) | 11 | 11 | 0 |
| 2. Seguridad e inicio de sesión (API) | 8 | 8 | 0 |
| 3. Tareas (API) | 9 | 9 | 0 |
| 4. Maestros (API) | 10 | 10 | 0 |
| 5. Escritorio | 7 | 7 | 0 |
| 6. Móvil | 11 | 9 | 2 |
| 7. Integración entre plataformas | 3 | 3 | 0 |
| 8. Docker | 4 | 4 | 0 |
| **Total** | **63** | **61** | **2** |

## 1. Web: navegación y maestros

| ID Caso | Módulo / Capa | Tipo de Prueba | Descripción del Escenario | Pasos de Ejecución | Resultado Esperado | Resultado Obtenido | Estado |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :---: |
| **CP-NAV-01** | UI / Frontend | Prueba Funcional | Navegación SPA (Single Page Application) por vistas | Hacer clic en los enlaces del menú de navegación lateral (Inicio, Tareas, Estados, etc.). | La URL cambia el `#hash` y muestra la sección activa sin recargar la página web. | Navegación fluida y sin recargas de página. | **Aprobado** |
| **CP-SYS-01** | Sistema / JS | Carga Secuencial | Inicialización de los maestros antes que las tareas | Recargar la aplicación web y observar el orden de peticiones HTTP. | Se cargan Estados, Roles y Usuarios, y al final Tareas. | Las listas desplegables se llenan antes de mostrar las tareas. | **Aprobado** |
| **CP-EST-01** | Estados (API) | Pruebas de API | Crear un nuevo estado con `POST` | Enviar `{"nombre_estado": "En Revisión"}` a `/api/Estados`. | Responde `201 Created` con el `id` del nuevo estado. | Registra correctamente el estado. | **Aprobado** |
| **CP-EST-02** | Estados (API/BD) | Restricción Llave Foránea | Impedir eliminar un estado asignado a tareas | Intentar eliminar desde la web un estado en uso. | Responde `400 Bad Request` con un mensaje claro. | Bloquea la eliminación y evita tareas sin estado. | **Aprobado** |
| **CP-ROL-01** | Roles (API/BD) | Validación de duplicados | Evitar roles con nombres repetidos | Crear un rol con un nombre que ya existe (con otras mayúsculas). | Responde `400 Bad Request`: "Ya existe un rol con ese nombre." | Evita roles duplicados. | **Aprobado** |
| **CP-ROL-02** | Roles (Frontend) | Prueba Funcional | Editar los permisos de un rol | Seleccionar "Editar", marcar "Administrador" y guardar. | Envía `PUT` y la tarjeta muestra la etiqueta roja "Administrador". | Actualiza el permiso y lo refleja en pantalla. | **Aprobado** |
| **CP-USU-01** | Usuarios (API/BD) | Integridad de Datos | Editar un usuario sin cambiar su correo | Cambiar el rol de un usuario dejando su correo igual. | La validación excluye al mismo usuario (`id_usuario != @id`) y permite el cambio. | Actualiza nombre y rol sin error de correo repetido. | **Aprobado** |
| **CP-USU-02** | Usuarios (UI) | Vista Relacional | Ver las tareas asignadas a un usuario | Abrir la tarjeta de un usuario con tareas. | Muestra la lista de tareas con número, título y estado. | Muestra correctamente las tareas de la persona. | **Aprobado** |
| **CP-TAR-01** | Tareas (UI/API) | Creación Completa | Elegir responsable y estado al crear una tarea | Llenar el título, elegir usuario y estado, y pulsar "Crear Tarea". | Se guarda con el `id_estado` y el responsable elegidos. | Tarea guardada con el estado inicial correcto. | **Aprobado** |
| **CP-TAR-02** | Tareas (Frontend) | Filtro Dinámico | Botones de filtro por estado | Hacer clic en un botón de estado (ej. "Pendiente"). | Solo se muestran las tareas con ese estado. | Filtra correctamente. | **Aprobado** |
| **CP-TAR-03** | Tareas (API) | Modificación (`PUT`) | Editar el título, responsable o estado de una tarea | Pulsar "Editar", cambiar datos y guardar. | Envía `PUT` y actualiza la tarea y su `fecha_actualizacion`. | Actualiza la tarea y refresca la tabla. | **Aprobado** |

## 2. Seguridad e inicio de sesión (API)

| ID Caso | Módulo / Capa | Tipo de Prueba | Descripción del Escenario | Pasos de Ejecución | Resultado Esperado | Resultado Obtenido | Estado |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :---: |
| **CP-AUT-01** | Auth (API) | Pruebas de API | Iniciar sesión como Administrador | `POST /api/Auth/login` con correo y contraseña correctos (caso 0). | `200 OK` con `token`, `expira` y los datos del usuario. | Devuelve el token y el rol "Administrador". | **Aprobado** |
| **CP-AUT-02** | Auth (API) | Prueba Negativa | Contraseña incorrecta | `POST /api/Auth/login` con una contraseña errada (caso 37). | `401 Unauthorized`: "Correo o contraseña incorrectos." | Rechaza el ingreso con el mensaje. | **Aprobado** |
| **CP-AUT-03** | Auth (API) | Prueba Negativa | Pedir datos sin token | `GET /api/Tareas` sin encabezado `Authorization` (caso 38). | `401 Unauthorized`. | No entrega datos. | **Aprobado** |
| **CP-AUT-04** | Auth (API) | Pruebas de API | Consultar el usuario de la sesión | `GET /api/Auth/yo` con el token (caso 39). | `200 OK` con el usuario y sus permisos. | Devuelve los datos del Administrador. | **Aprobado** |
| **CP-AUT-05** | Permisos (API) | Autorización | El Líder de Proyecto intenta crear un estado | `POST /api/Estados` con el token del Líder (caso 41). | `403 Forbidden` (solo Administrador). | Bloquea la acción. | **Aprobado** |
| **CP-AUT-06** | Permisos (API) | Autorización | El Líder de Proyecto intenta eliminar una tarea | `DELETE /api/Tareas/8` con el token del Líder (caso 42). | `403 Forbidden`. | Bloquea la eliminación. | **Aprobado** |
| **CP-AUT-07** | Permisos (API) | Autorización | El Líder de Proyecto cambia el estado de una tarea | `PATCH /api/Tareas/8/estado` (casos 43 y 44). | `200 OK` y la tarea cambia de estado. | La tarea pasa a "En progreso" y vuelve a "Pendiente". | **Aprobado** |
| **CP-AUT-08** | Seguridad (BD) | Revisión | Las contraseñas no se guardan en texto plano | Consultar la columna `contrasena_hash` en SQLite. | Solo aparece el hash `pbkdf2-sha256$100000$...`. | No se ve ninguna contraseña. | **Aprobado** |

## 3. Tareas (API)

| ID Caso | Módulo / Capa | Tipo de Prueba | Descripción del Escenario | Pasos de Ejecución | Resultado Esperado | Resultado Obtenido | Estado |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :---: |
| **CP-TAR-04** | Tareas (API) | Pruebas de API | Crear una tarea sin responsable | `POST /api/Tareas` sin `IdUsuarioResponsable` (caso 3). | `201 Created`; la tarea queda "Sin asignar". | Se crea correctamente. | **Aprobado** |
| **CP-TAR-05** | Tareas (API) | Validación | Título con solo espacios | `POST /api/Tareas` con `"titulo": "   "` (caso 4). | `400 Bad Request`. | Rechaza la tarea. | **Aprobado** |
| **CP-TAR-06** | Tareas (API) | Validación | Estado que no existe | `POST /api/Tareas` con un `id_estado` inválido (caso 5). | `400`: "El estado seleccionado no existe." | Rechaza la tarea con el mensaje. | **Aprobado** |
| **CP-TAR-07** | Tareas (API) | Prueba Negativa | Detalle o eliminación de una tarea inexistente | `GET` y `DELETE /api/Tareas/99999` (casos 6 y 23). | `404 Not Found`. | Responde 404. | **Aprobado** |
| **CP-TAR-08** | Tareas (API) | Regla de negocio | Tarea con fecha límite vencida | Crear una tarea con fecha límite pasada y estado "Pendiente" (caso 24). | La tarea llega con `"atrasada": true`. | Se marca como atrasada. | **Aprobado** |
| **CP-TAR-09** | Tareas (API) | Filtro | Solo tareas atrasadas | `GET /api/Tareas?atrasadas=true` (caso 25). | Solo devuelve las tareas atrasadas. | Filtra correctamente. | **Aprobado** |
| **CP-TAR-10** | Tareas (API) | Filtro | Filtrar por prioridad | `GET /api/Tareas?prioridad=Alta` (caso 26). | Solo tareas de prioridad Alta. | Filtra correctamente. | **Aprobado** |
| **CP-TAR-11** | Tareas (API) | Validación | Prioridad o fecha con formato inválido | `POST` con `"prioridad": "Urgente"` o fecha `31-12-2026` (casos 27 y 28). | `400 Bad Request` con el motivo. | Rechaza los datos. | **Aprobado** |
| **CP-TAR-12** | Tareas (API) | Regla de negocio | Fecha de finalización al cerrar una tarea | Cambiar una tarea a "Terminada" y luego a "Pendiente". | Se llena `fecha_finalizacion` al cerrarla y se borra al reabrirla. | Al pasar la #24 a Terminada se llenó la fecha de finalización (`evidencia-movil-fecha-finalizacion.png`); al volver a Pendiente quedó otra vez abierta. | **Aprobado** |

## 4. Maestros (API)

| ID Caso | Módulo / Capa | Tipo de Prueba | Descripción del Escenario | Pasos de Ejecución | Resultado Esperado | Resultado Obtenido | Estado |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :---: |
| **CP-USU-03** | Usuarios (API) | Validación | Correo con formato inválido | `POST /api/Usuarios` con `"correo": "correo-malo"` (caso 8). | `400`: "El correo electrónico no tiene un formato válido." | Rechaza el usuario. | **Aprobado** |
| **CP-USU-04** | Usuarios (API) | Validación | Nombre vacío o rol inexistente | `POST /api/Usuarios` (casos 9 y 10). | `400 Bad Request` con el motivo. | Rechaza los datos. | **Aprobado** |
| **CP-USU-05** | Usuarios (API) | Validación | Correo repetido | Crear un usuario con un correo ya registrado (caso 11). | `400 Bad Request`. | Evita correos duplicados. | **Aprobado** |
| **CP-EST-03** | Estados (API) | Validación | Nombre vacío o repetido con otras mayúsculas | `POST /api/Estados` (casos 13 y 14). | `400 Bad Request`. | Rechaza los datos. | **Aprobado** |
| **CP-EST-04** | Estados (API) | Pruebas de API | Campo `es_final` | `GET /api/Estados` (caso 30). | "Terminada" y "Cancelada" con `es_final: true`. | Correcto. | **Aprobado** |
| **CP-ROL-03** | Roles (API) | Restricción | Eliminar un rol que tienen usuarios | `DELETE /api/Roles/1` (caso 19). | `400`: "No se puede eliminar este rol porque hay usuarios asignados a él." | Bloquea la eliminación. | **Aprobado** |
| **CP-PRO-01** | Proyectos (API) | Pruebas de API | Listar proyectos con conteo de tareas | `GET /api/Proyectos` (caso 31). | Cada proyecto trae `total_tareas` y `tareas_abiertas`. | Correcto. | **Aprobado** |
| **CP-PRO-02** | Proyectos (API) | Pruebas de API | Crear un proyecto | `POST /api/Proyectos` (caso 32). | `201 Created`. | Se crea correctamente. | **Aprobado** |
| **CP-PRO-03** | Proyectos (API) | Validación | Nombre repetido o estado inválido | `POST /api/Proyectos` (casos 33 y 34). | `400 Bad Request` con el motivo. | Rechaza los datos. | **Aprobado** |
| **CP-PRO-04** | Proyectos (API) | Restricción | Eliminar un proyecto con tareas | `DELETE /api/Proyectos/1` (caso 35). | `400 Bad Request`; las tareas no se pierden. | Bloquea la eliminación. | **Aprobado** |

## 5. Aplicación de escritorio

| ID Caso | Módulo / Capa | Tipo de Prueba | Descripción del Escenario | Pasos de Ejecución | Resultado Esperado | Resultado Obtenido | Estado |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :---: |
| **CP-ESC-01** | Escritorio / Login | Prueba Negativa | Contraseña incorrecta | Escribir una contraseña errada y pulsar "Ingresar". | Muestra el mensaje de la API y no entra. | Correcto (`evidencia-escritorio-login-error.png`). | **Aprobado** |
| **CP-ESC-02** | Escritorio / Inicio | Prueba Funcional | Dashboard igual al de la web | Entrar como Administrador. | Indicadores y paneles con los mismos datos de la web. | Correcto (`evidencia-escritorio-inicio.png`). | **Aprobado** |
| **CP-ESC-03** | Escritorio / Tareas | Prueba Funcional | Tarea atrasada resaltada | Abrir Tareas con una tarea vencida. | La fila sale en rojo con la etiqueta "Atrasada". | Correcto (`evidencia-escritorio-admin-atrasada.png`). | **Aprobado** |
| **CP-ESC-04** | Escritorio / Permisos | Autorización | Menú y botones del Desarrollador | Entrar como Desarrollador. | Sin formulario, sin Estados, Roles ni Usuarios; solo el botón "Ver". | Correcto (`evidencia-escritorio-desarrollador.png`). | **Aprobado** |
| **CP-ESC-05** | Escritorio / Permisos | Autorización | El Desarrollador en su propia tarea | Abrir el detalle de la tarea #8, asignada a él. | Puede cambiar el estado, pero no editar ni eliminar. | Correcto (`evidencia-escritorio-detalle-propia.png`). | **Aprobado** |
| **CP-ESC-06** | Escritorio / Maestros | Prueba Funcional | Maestros de Estados, Roles y Usuarios | Entrar a cada sección como Administrador. | Se ven las etiquetas "Cierra la tarea" y los permisos de cada rol. | Correcto (capturas de estados, roles y usuarios). | **Aprobado** |
| **CP-ESC-07** | Escritorio / Desplazamiento | Prueba Funcional | Bajar hasta la última tarea después de filtrar | Cambiar de filtro y bajar con la rueda del mouse. | Se llega hasta el final de la lista. | Correcto después de la corrección de la semana 12. | **Aprobado** |

## 6. Aplicación móvil

| ID Caso | Módulo / Capa | Tipo de Prueba | Descripción del Escenario | Pasos de Ejecución | Resultado Esperado | Resultado Obtenido | Estado |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :---: |
| **CP-MOV-01** | Móvil / Login | Prueba automática | Sin sesión guardada abre el inicio de sesión | `flutter test`. | Se ve el botón "Ingresar". | Prueba pasada. | **Aprobado** |
| **CP-MOV-02** | Móvil / Tareas | Prueba automática | Con sesión muestra las tareas de la API | `flutter test`. | Se ven las tareas, la prioridad y la etiqueta "Atrasada". | Prueba pasada. | **Aprobado** |
| **CP-MOV-03** | Móvil / Permisos | Prueba automática | Un Desarrollador no ve "Nueva tarea" | `flutter test`. | El botón no aparece. | Prueba pasada. | **Aprobado** |
| **CP-MOV-04** | Móvil / Sin conexión | Prueba automática | Backend apagado | `flutter test`. | Mensaje "No se pudo conectar" y botón "Reintentar". | Prueba pasada. | **Aprobado** |
| **CP-MOV-05** | Móvil / Menú | Prueba automática | El Administrador ve todas las secciones | `flutter test`. | Inicio, Tareas, Proyectos, Estados, Roles y Usuarios. | Prueba pasada; en el emulador se ven las 6 secciones (`evidencia-movil-menu-completo.png`). | **Aprobado** |
| **CP-MOV-06** | Móvil / Menú | Prueba automática | El Desarrollador no ve los maestros | `flutter test`. | No aparecen Estados, Roles ni Usuarios. | Prueba pasada. | **Aprobado** |
| **CP-MOV-07** | Móvil / Inicio | Prueba automática | Navegar a Inicio desde el menú | `flutter test`. | Se ve el dashboard con "Tareas por estado". | Prueba pasada; en el emulador se ven los indicadores y los 4 paneles (`evidencia-movil-inicio-dashboard.png`). | **Aprobado** |
| **CP-MOV-08** | Móvil / Proyectos | Prueba manual | Ver las tareas de un proyecto | En el emulador: Proyectos → "Ver tareas". | Abre Tareas filtrada por ese proyecto. | | Por ejecutar |
| **CP-MOV-09** | Móvil / Estados | Prueba manual | Crear y eliminar un estado desde el celular | Estados → "Nuevo estado" → guardar → "Eliminar". | El estado aparece en la lista y luego se elimina. | | Por ejecutar |
| **CP-MOV-10** | Móvil / Estados | Prueba manual | Eliminar un estado en uso | Estados → "Eliminar" en "Pendiente". | Mensaje en rojo de la API y el estado no se borra. | Sale el mensaje "No se puede eliminar este estado porque hay tareas que actualmente lo están usando" (`evidencia-movil-estado-en-uso.png`). | **Aprobado** |
| **CP-MOV-11** | Móvil / Usuarios | Prueba manual | Filtrar usuarios por rol | Usuarios → botón "Desarrollador". | Solo se ven los usuarios de ese rol, con sus tareas. | Solo aparece Dev Prueba con sus 2 tareas (`evidencia-movil-usuarios-por-rol.png`). | **Aprobado** |

## 7. Integración entre plataformas

| ID Caso | Módulo / Capa | Tipo de Prueba | Descripción del Escenario | Pasos de Ejecución | Resultado Esperado | Resultado Obtenido | Estado |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :---: |
| **CP-INT-01** | Escritorio → Web | Integración | Tarea creada desde el escritorio | Crear una tarea en el escritorio y abrir la web. | La tarea aparece en la web, marcada igual. | Correcto (`evidencia-web-tarea-creada-desde-escritorio.png`). | **Aprobado** |
| **CP-INT-02** | Móvil → Web | Integración | Tarea creada desde el celular | Crear una tarea en el móvil y abrir la web. | La tarea aparece en la web. | Correcto (`evidencia-web-tarea-creada-desde-movil.png`). | **Aprobado** |
| **CP-INT-03** | Web → Escritorio | Integración | Tarea creada desde la web en Docker | Crear la tarea #24 en `localhost:8080` y entrar a Tareas en el escritorio. | La #24 aparece en el escritorio sin cerrar sesión. | Correcto (`evidencia-docker-escritorio.png`). | **Aprobado** |

## 8. Docker

| ID Caso | Módulo / Capa | Tipo de Prueba | Descripción del Escenario | Pasos de Ejecución | Resultado Esperado | Resultado Obtenido | Estado |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :---: |
| **CP-DOC-01** | Docker | Despliegue | Construir y levantar los contenedores | `docker compose up -d --build`. | Imágenes construidas y los dos contenedores iniciados. | Correcto (`evidencia-docker-terminal.png`). | **Aprobado** |
| **CP-DOC-02** | Docker | Despliegue | Estado de los contenedores | `docker compose ps` y Docker Desktop. | `gestion-tareas-api` (5293:8080) y `gestion-tareas-web` (8080:80) en *Up*. | Correcto (`evidencia-docker-contenedores.png`). | **Aprobado** |
| **CP-DOC-03** | Docker / Nginx | Integración | La web llega a la API por el proxy | Abrir `http://localhost:8080` e iniciar sesión. | El dashboard carga los datos de `/api`. | Correcto (`evidencia-docker-web.png`). | **Aprobado** |
| **CP-DOC-04** | Docker / Volumen | Persistencia | Los datos se conservan al recrear el contenedor | `docker compose down` y luego `docker compose up -d`. | Las tareas siguen en la base de `./datos`. | Después de recrear los contenedores siguen todas las tareas, incluida la #24 (`evidencia-docker-down-up.png`, `evidencia-docker-datos-persisten.png`). | **Aprobado** |
