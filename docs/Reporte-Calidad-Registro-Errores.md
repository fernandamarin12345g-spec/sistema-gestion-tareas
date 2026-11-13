# Reporte de Aseguramiento de Calidad y Registro de Errores

**Proyecto:** Sistema de Gestión y Seguimiento de Tareas
**Empresa:** Strategic Software Alliance S.A.S. (ESA SAS)
**Elaborado por:** Fernanda Marín Tobón
**Periodo:** 11 de agosto al 13 de noviembre de 2026 (semanas 1 a 14)

Este reporte acompaña a la [Matriz de Pruebas Funcionales y de API](Matriz-Pruebas-Funcionales-y-API.md). La matriz dice **qué se probó y qué resultado dio**. Este documento registra **los errores (bugs) que aparecieron**, cómo se diagnosticaron, cómo se corrigieron y cómo se comprobó que la corrección funcionaba.

## 1. Metodología

### 1.1 Tipos de prueba

| Tipo | Herramienta | Qué cubre |
| --- | --- | --- |
| Pruebas de API | `Backend/Backend.http` (Visual Studio) y Postman | 47 casos: respuestas correctas, validaciones (400), recursos inexistentes (404), sin sesión (401) y sin permiso (403) |
| Pruebas funcionales manuales | Navegador, app de escritorio y emulador de Android | Flujos completos de cada pantalla con los tres roles |
| Pruebas automáticas | `flutter test` | 7 pruebas de widgets de la app móvil |
| Análisis estático | `flutter analyze` y `dotnet build` | Errores de compilación y avisos del analizador |
| Pruebas de integración | Las tres aplicaciones contra la misma API | Que un cambio hecho en un cliente se vea en los demás |
| Pruebas de despliegue | Docker Compose | Construcción de imágenes, puertos, volumen y proxy |

### 1.2 Proceso para cada error

1. **Detectar:** el error aparece en una prueba (mensaje en pantalla, código HTTP inesperado o la aplicación se cierra).
2. **Reproducir:** repetir los pasos exactos hasta que el error ocurra siempre.
3. **Diagnosticar:** encontrar la causa con la consola del navegador (F12), el depurador de Visual Studio, los logs de la API (`docker compose logs api`) o el archivo `errores.log` de la app de escritorio.
4. **Corregir:** cambiar el código en una rama de Git (`feature-...`), nunca directamente en `main`.
5. **Validar:** repetir la prueba que falló y las pruebas relacionadas, para comprobar que la corrección no dañó otra cosa.
6. **Registrar:** anotar el error en la bitácora de la semana y en este reporte.

### 1.3 Severidad

| Nivel | Significado |
| --- | --- |
| **Crítica** | La aplicación se cierra o no se puede usar |
| **Alta** | Una función importante no sirve o da datos incorrectos |
| **Media** | La función sirve, pero con un comportamiento incorrecto o confuso |
| **Baja** | Detalle visual o de comodidad |

## 2. Registro de errores

| ID | Sem. | Módulo | Síntoma | Causa raíz | Corrección | Validación | Severidad | Estado |
| --- | :---: | --- | --- | --- | --- | --- | :---: | :---: |
| **BUG-01** | 5 | Web (JavaScript) | La consola mostraba `formulario is not defined` y el formulario dejó de funcionar | Al editar el bloque de validación se borraron las declaraciones `const` del inicio del archivo | Se restauraron las constantes al inicio de `script.js`, antes de los `addEventListener` | El formulario volvió a validar y la consola quedó sin errores | Alta | Cerrado |
| **BUG-02** | 5 | Web (JavaScript) | El texto de la tarea no aparecía en la lista | La variable se declaró como `ElementoTarea` y se usó como `elementoTarea` (JavaScript distingue mayúsculas) | Se unificó el nombre `elementoTarea` en toda la función | La tarea se mostró en la lista | Media | Cerrado |
| **BUG-03** | 6 | Base de datos | El script SQL fallaba al crear las llaves primarias | Se usó `SERIAL`, que es de PostgreSQL y no existe en SQLite | Se cambió por `INTEGER PRIMARY KEY AUTOINCREMENT` y los booleanos por `0/1` | El script creó todas las tablas | Alta | Cerrado |
| **BUG-04** | 8 | API Usuarios | Cada usuario salía repetido una vez por cada tarea que tenía | El `LEFT JOIN` entre Usuarios y Tareas devuelve una fila por cada tarea | Se agrupan las filas en un `Dictionary<int, UsuarioResponse>` y las tareas se anidan en una lista | `GET /api/Usuarios` devuelve cada usuario una sola vez con sus tareas (CP-USU-02) | Alta | Cerrado |
| **BUG-05** | 8 | Web (formularios) | Al editar un registro se creaba uno nuevo | El mismo formulario tenía un solo evento `submit` que siempre hacía `POST` | Variables de edición (`idEstadoEnEdicion`...) que cambian el envío a `PUT`, y botón Cancelar | Editar actualiza el registro sin duplicarlo (CP-ROL-02, CP-TAR-03) | Alta | Cerrado |
| **BUG-06** | 8 | Web (navegación) | Después de guardar, la página volvía siempre a Inicio | La recarga de datos no recordaba la sección activa | La sección activa se guarda en el `#hash` de la URL y se restaura al recargar | Guardar en Estados deja al usuario en Estados (CP-NAV-01) | Media | Cerrado |
| **BUG-07** | 9 | API (PUT) | Error 500: `Must add values for the following parameters` | Se reutilizaba el mismo `SqliteCommand` para el `SELECT` de validación y el `UPDATE`, y conservaba los parámetros anteriores | Comandos independientes (`comandoVerificar` y `comandoUpdate`), cada uno en su bloque `using` | Las ediciones de usuarios y tareas responden 200 (CP-USU-01) | Crítica | Cerrado |
| **BUG-08** | 9 | API Estados y Roles | Al eliminar un estado en uso, la API se caía con una excepción no controlada | SQLite rechaza el borrado por la llave foránea (`SQLite Error 19`) | `try-catch` con `when (ex.SqliteErrorCode == 19)` que responde 400 con un mensaje claro | El borrado se bloquea con el mensaje en web y móvil (CP-EST-02, CP-MOV-10) | Alta | Cerrado |
| **BUG-09** | 9 | Web (CSS) | Los textos largos se montaban sobre la columna siguiente | Anchos de columna demasiado pequeños en el grid | Se ajustó `grid-template-columns` | Las filas se ven alineadas | Baja | Cerrado |
| **BUG-10** | 10 | Móvil (pruebas) | `flutter test` fallaba | La prueba seguía buscando la clase `MyApp` del proyecto de ejemplo | Se reescribió la prueba con `AppGestionTareas` | `flutter test` pasa | Media | Cerrado |
| **BUG-11** | 11 | Base de datos / Git | La API respondía `no such table: Tareas` | Al unir una rama, Git borró del disco la base de datos que estaba versionada | Se restauró desde una copia; desde entonces hay respaldo antes de cada migración y la base no se sube a Git | La API volvió a responder; `.gitignore` excluye `*.sqlite` | Crítica | Cerrado |
| **BUG-12** | 11 | Móvil / API | La app en el emulador no se conectaba a la API | La API redirigía HTTP a HTTPS y el emulador no confía en el certificado de desarrollo | La redirección a HTTPS solo se activa fuera de desarrollo; el emulador usa `http://10.0.2.2:5293` | La lista de tareas carga en el emulador | Alta | Cerrado |
| **BUG-13** | 11 | API Proyectos | Se podían crear "GESTIÓN" y "gestión" como proyectos distintos | `LOWER()` de SQLite no convierte letras con tilde | La comparación se hace en C# con `StringComparison.OrdinalIgnoreCase` | El nombre repetido con tilde se rechaza (CP-PRO-03) | Media | Cerrado |
| **BUG-14** | 12 | Clientes (sesión) | Una contraseña mal escrita mostraba "Tu sesión expiró" | El código 401 se trataba igual en el login y en las demás pantallas | El 401 solo cuenta como sesión vencida cuando ya hay una sesión iniciada | El login muestra "Correo o contraseña incorrectos" (CP-AUT-02, CP-ESC-01) | Media | Cerrado |
| **BUG-15** | 12 | Escritorio | La aplicación se cerraba sola al iniciar sesión, sin ningún mensaje | Dos fallas: (1) en la barra lateral se cambiaba el ancho antes de crear el botón "Cerrar sesión" (`NullReferenceException` en `OnLayout`); (2) en los formularios de Estados y Roles, una casilla con tamaño automático provocaba un ciclo de acomodo infinito | Se agregó un registro global de errores (`errores.log`) que mostró el archivo y la línea. El botón se crea antes del cambio de ancho y a las casillas solo se les cambia la posición | La app abre todas las secciones sin cerrarse (CP-ESC-02 a CP-ESC-06) | Crítica | Cerrado |
| **BUG-16** | 12 | Escritorio | No se podía bajar hasta las últimas tareas después de filtrar | La barra de desplazamiento de Windows no se actualizaba al crecer el contenido | Barra de desplazamiento propia, recalculada en cada cambio de contenido | Se llega hasta la última fila (CP-ESC-07) | Alta | Cerrado |
| **BUG-17** | 13 | Web en Docker | "No se pudo conectar con el servidor" | `script.js` seguía apuntando a la API de Visual Studio, y el navegador usaba la versión guardada en caché | La dirección de la API depende del puerto (Live Server o `/api` por Nginx) y se recargó con Ctrl+Shift+R | El dashboard carga en `localhost:8080` (CP-DOC-03) | Alta | Cerrado |
| **BUG-18** | 13 | Escritorio | Una tarea creada desde la web no aparecía en el escritorio | El escritorio solo pedía los datos al iniciar sesión | Los datos se vuelven a pedir cada vez que se cambia de sección | La tarea #24 aparece al entrar a Tareas (CP-INT-03) | Media | Cerrado |
| **BUG-19** | 14 | API en Docker | Las fechas quedaban 5 horas adelantadas (la #24 decía 15:36 en lugar de 10:36) | El contenedor usaba la hora UTC | Variable `TZ=America/Bogota` en `docker-compose.yml` | Los registros de auditoría muestran la hora de Colombia | Media | Cerrado |
| **BUG-20** | 14 | Web (caché) | La sección Auditoría se veía en una sola columna y sin colores | El navegador seguía usando el `styles.css` anterior | `index.html` pide `styles.css?v=5` y `script.js?v=5`, así el navegador descarga la versión nueva | La tabla de auditoría se ve con sus columnas y colores | Baja | Cerrado |

## 3. Problemas del entorno de desarrollo

No son errores del código del sistema, pero frenaron el trabajo y se resolvieron así:

| ID | Sem. | Problema | Solución |
| --- | :---: | --- | --- |
| **ENT-01** | 10 | El repositorio tenía 127 archivos generados (`bin/`, `obj/`, `.vs/`) | `.gitignore` en la raíz y `git rm -r --cached` |
| **ENT-02** | 11 | El emulador se quedaba en `Installing...` (`adb ... timed out`) | `adb kill-server`, `adb start-server` y arranque en frío del emulador |
| **ENT-03** | 11 | Windows bloqueaba los plugins de Flutter (*symlink support*) | Se activó el Modo de desarrollador de Windows |
| **ENT-04** | 13 | Microsoft Store no instalaba Docker Desktop (error `0x00000001`) | Instalador oficial de docker.com con WSL 2 |
| **ENT-05** | 14 | Un archivo editado fuera de VS Code mostraba "The content of the file is newer" | `File: Revert File` para cargar la versión del disco sin sobrescribirla |

## 4. Resumen

| Severidad | Errores | Cerrados |
| --- | :---: | :---: |
| Crítica | 3 | 3 |
| Alta | 9 | 9 |
| Media | 6 | 6 |
| Baja | 2 | 2 |
| **Total** | **20** | **20** |

- **Errores por capa:** API y base de datos 8, web 6, escritorio 3, móvil 2, Docker 1.
- **Resultado de las pruebas:** 63 de 63 casos de la matriz aprobados, 47 casos de API en `Backend.http`, 7 de 7 pruebas automáticas del móvil, y `dotnet build` y `flutter analyze` sin errores.
- **Errores abiertos:** ninguno.

## 5. Lecciones aprendidas

- **Registrar los errores es lo que permite corregirlos.** El cierre silencioso de la app de escritorio (BUG-15) solo se pudo resolver después de agregar `errores.log`, que mostró el archivo y la línea exactos.
- **Validar en la API y no solo en la pantalla.** Las reglas (nombres repetidos, estados en uso, permisos) están en el backend, así que valen igual para la web, el escritorio y el móvil.
- **Respaldar antes de tocar la base de datos.** Después de BUG-11, cada migración empieza con una copia de seguridad.
- **Probar en el mismo entorno donde va a correr.** Los errores BUG-17, BUG-19 y BUG-20 solo aparecieron en Docker o en el navegador, no en Visual Studio.
- **Cada corrección se vuelve a probar con su caso de la matriz**, para comprobar que el error no vuelve y que no se dañó otra parte.
