# Comparativa: desarrollo manual frente a desarrollo asistido por IA

**Proyecto:** Sistema de Gestión y Seguimiento de Tareas
**Actividad del plan de formación:** Semana 15. Comparación entre desarrollo manual y desarrollo asistido por IA; generación de código, explicación, refactorización, depuración, pruebas, documentación y revisión crítica del código generado.
**Elaborado por:** Fernanda Marín Tobón

## 1. Objetivo

Comparar dos funcionalidades del mismo sistema, de tamaño parecido:

- **Hecha a mano:** el **Maestro de Estados** (semana 8).
- **Acelerada con IA:** el **Registro de auditoría** (semana 14).

La comparación mira el tiempo, la calidad, los errores, la comprensión del código y el papel de cada parte. La idea no es decidir cuál "es mejor", sino entender **para qué sirve la IA, qué riesgos tiene y qué sigue dependiendo de la desarrolladora**.

## 2. Contexto de uso de la IA en la práctica

- **Semanas 1 a 13:** casi todo el sistema lo desarrollé yo, dentro de las horas de cada semana del plan. Usé Gemini y Claude solo para cosas puntuales y sencillas, por ejemplo entender un mensaje de error o recordar la sintaxis de un comando.
- **Semana 14:** la auditoría se construyó con Claude como asistente de programación, a propósito, para esta comparación.

## 3. Las dos funcionalidades

| | Maestro de Estados (manual) | Registro de auditoría (con IA) |
| --- | --- | --- |
| **Qué hace** | Crear, consultar, editar y eliminar los estados de las tareas | Guardar automáticamente quién hizo cada cambio, sobre qué registro y cuándo, más los inicios de sesión, y mostrarlo al Administrador |
| **Backend** | `EstadosController.cs` (197 líneas): GET, POST, PUT y DELETE con SQL parametrizado | `FiltroAuditoria.cs` (148), `ServicioAuditoria.cs` (121) y `AuditoriaController.cs` (68): 337 líneas nuevas de C#, más cambios en `Program.cs` y `AuthController.cs` |
| **Frontend** | Formulario y tabla de Estados en `index.html` y `script.js` | Sección Auditoría con filtro por módulo y acciones en colores |
| **Base de datos** | Tabla `Estados` | Columnas nuevas en `Auditoria_Log`, que se agregan solas al arrancar la API |
| **Conceptos nuevos** | CRUD, verbos HTTP, `fetch`, formulario de doble uso (crear y editar) | Filtros de ASP.NET Core (`IAsyncActionFilter`), inyección de dependencias, reflexión, migración automática con `PRAGMA table_info` |

## 4. Proceso manual: Maestro de Estados (semana 8)

1. Escribí el controlador a partir del de Tareas de la semana 7, adaptando las consultas SQL a la tabla `Estados`.
2. Probé cada ruta con Postman antes de tocar la web.
3. Hice el formulario y la tabla en la web, y conecté cada botón con `fetch`.
4. Corregí los errores a medida que aparecían, investigando en la documentación y con el depurador:
   - El formulario creaba un registro nuevo cuando quería editar (BUG-05).
   - La página volvía a Inicio después de guardar (BUG-06).
   - En la semana 9 aparecieron dos más: el error 500 por reutilizar el comando SQL (BUG-07) y la caída al eliminar un estado en uso (BUG-08).

**Tiempo:** formó parte de las 20 horas de la semana 8, que también incluyeron los maestros de Roles y Usuarios y la conexión de la web con la API. Las correcciones de la semana 9 tomaron horas adicionales.

## 5. Proceso con IA: Registro de auditoría (semana 14)

### 5.1 Registro de prompts y decisiones

| Hora | Quién | Mensaje o acción | Decisión |
| --- | --- | --- | --- |
| 12:19 | Yo | Compartí el plan de formación y pedí revisar qué faltaba para evidenciar los entregables | — |
| 12:22 | IA | Detectó que el entregable 1 pide "lógica de auditoría" y que la tabla `Auditoria_Log` existía pero nunca se usaba. Propuso registrar cada cambio y los inicios de sesión, con una sección solo para el Administrador | — |
| 12:25 | Yo | *"si hagamosla"* | Se aprueba el alcance |
| 12:25 – 12:45 | IA | Revisó `Program.cs`, los controladores y `script.js`, y escribió el código | Ver decisiones de diseño (5.2) |
| 13:26 | Yo | `dotnet build`: **compiló a la primera** | — |
| 13:27 | Yo | `docker compose up -d --build` y prueba en la web: un login fallido, un login correcto, y crear, editar y eliminar una tarea | — |
| 13:31 | Yo | Envié capturas: los registros se guardan con la hora correcta, pero la tabla se ve en una sola columna y sin colores | — |
| 13:32 | IA | Diagnóstico: el navegador usaba el `styles.css` viejo de la caché | Se agrega `?v=5` a los enlaces del CSS y del JS |
| 13:32 | IA | Notó que en la web el estado se cambia desde "Editar", así que el detalle no decía el estado nuevo | El detalle de "Editar" incluye el estado |
| 13:47 | Yo | La tabla ya se veía bien, pero la compilación salió toda en caché | — |
| 13:48 | IA | Comprobó que la mejora del detalle no había llegado a mi carpeta y la volvió a copiar | — |
| 13:50 | Yo | Nueva prueba: *"#24 Prueba Docker · estado: En progreso"* | Funcionalidad aceptada |
| 13:52 | Yo | `git commit`, `push` y `merge` a `main` (commit `d06bceb`) | — |

### 5.2 Decisiones de diseño y por qué

| Decisión | Alternativa descartada | Motivo |
| --- | --- | --- |
| Un **filtro global** (`FiltroAuditoria`) que se ejecuta en todas las acciones | Escribir código de auditoría en cada controlador | Un solo lugar: cualquier controlador nuevo queda auditado sin hacer nada más |
| Registrar **solo si la respuesta fue 2xx** | Registrar todos los intentos | La auditoría debe mostrar lo que de verdad pasó, no lo que se rechazó |
| Leer el nombre del registro **antes** de ejecutar la acción | Leerlo después | Después de eliminar, el registro ya no existe |
| **Migración automática** al arrancar (`PRAGMA table_info` + `ALTER TABLE`) | Pedir que se ejecute un script SQL a mano | La misma API funciona con la base de Visual Studio y con la de Docker |
| Auditar el login fallido **con el correo, nunca con la contraseña** | No auditar los intentos fallidos | Permite ver intentos sospechosos sin guardar datos sensibles |
| Si la auditoría falla, **la acción del usuario sigue** (solo se escribe en el log) | Cancelar la acción | La auditoría no debe impedir trabajar |
| `TZ=America/Bogota` en Docker | Dejar la hora UTC | La hora de la auditoría tiene que ser la real |

## 6. Comparación

| Criterio | Manual (Estados) | Con IA (Auditoría) |
| --- | --- | --- |
| **Tiempo** | Parte de las 20 h de la semana 8, más correcciones en la semana 9 | **1 h 25 min** desde la aprobación hasta tenerla probada y en `main` (12:25 a 13:50) |
| **Compilación** | Se corrigió durante la semana; no quedó registrado cuántos intentos tomó | Compiló a la primera |
| **Errores encontrados** | 4 (BUG-05 a BUG-08), dos de ellos de severidad alta o crítica | 3, todos menores y de entorno: caché del navegador, un archivo que no se copió, un detalle incompleto |
| **Quién encontró los errores** | Yo, probando | **Yo, probando**: la IA no podía ejecutar el sistema y dependía de mis capturas |
| **Diseño** | Directo, aprendiendo cada paso | Usó conceptos que todavía no dominaba (filtros, reflexión) |
| **Documentación** | La escribí después, en la bitácora | Comentarios en el código y migración documentada desde el principio |
| **Comprensión** | Total: cada línea la escribí yo | Hubo que **revisar y estudiar** el código para poder explicarlo (sección 7) |
| **Aprendizaje** | Alto: CRUD, HTTP y depuración desde cero | Medio: aprendí patrones nuevos leyendo código que no escribí |

## 7. Revisión crítica del código generado

Antes de aceptar el código lo revisé con las preguntas del tutor:

- **¿Qué problema resuelve?** Cumplir la "lógica de auditoría" del entregable 1: saber quién cambió qué y cuándo.
- **¿Dónde se ejecuta?** En la API (contenedor `gestion-tareas-api`). El filtro corre después de cada acción de un controlador, y los datos quedan en la tabla `Auditoria_Log` de SQLite, dentro del volumen `./datos`.
- **¿Cómo se comunican las partes?** La web llama a `GET /api/Auditoria`. El controlador exige la política `Administrador`, lee la tabla con un `LEFT JOIN` a `Usuarios` y devuelve JSON.
- **¿Qué ocurre si falla?** `ServicioAuditoria.Registrar` captura la excepción y la escribe en el log de la API. El usuario no se entera y su acción se completa.
- **¿Es seguro?** Las consultas usan parámetros (`@accion`, `@usuario`...). Los nombres de tablas que se ponen en el SQL salen de una lista fija dentro del código y nunca de lo que escribe el usuario. La contraseña nunca se guarda. Un Líder de Proyecto recibe `403` al pedir la auditoría (caso 47 de `Backend.http`).
- **¿Cómo pruebo que funciona?** Con el login fallido y correcto, crear, editar y eliminar una tarea, y revisar la sección Auditoría (`evidencia-auditoria-web.png`), más los casos 45 a 47 de `Backend.http`.

**Qué tuve que estudiar para poder explicarlo:**
- Qué es un `IAsyncActionFilter` y qué hace `await siguiente()`: ejecuta la acción del controlador y devuelve su resultado.
- Cómo se registra un servicio con inyección de dependencias (`AddSingleton`, `AddScoped`).
- Qué es la reflexión (`GetType().GetProperty("titulo")`): leer un campo de un objeto sin conocer su clase de antemano.

## 8. Conclusiones

1. **La IA acelera, pero no reemplaza las pruebas.** El código compiló a la primera, pero los tres problemas solo aparecieron cuando lo ejecuté y lo revisé en el navegador.
2. **Sin bases propias, la IA no sirve igual.** Pude revisar el código y encontrar los errores porque ya había construido a mano el mismo tipo de funcionalidad (controlador, SQL, `fetch`, Docker).
3. **La IA propone diseños que yo no habría pensado** (el filtro global, la migración automática). Eso es útil, pero me obliga a estudiar lo que no conozco antes de aceptarlo.
4. **La responsabilidad sigue siendo de quien desarrolla:** decidir el alcance, probar, revisar la seguridad, hacer el commit y poder defender el código.
5. **Uso recomendado:** escribir a mano lo que todavía estoy aprendiendo, y usar la IA para tareas repetitivas o de diseño más avanzado, revisando siempre lo que genera.
