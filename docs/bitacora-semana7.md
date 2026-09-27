# Bitácora Semana 7

## ¿Qué aprendí?

*   **Arquitectura de Backend y Creación de una API REST:** Comprendí el salto conceptual de los datos estáticos en memoria hacia una arquitectura robusta de servicios web utilizando ASP.NET Core en Visual Studio 2022, implementando controladores dedicados (`TareasController.cs`) para actuar como un puente estructurado entre el cliente y la base de datos.
*   **Configuración de Conexiones Locales con SQLite (`Microsoft.Data.Sqlite`):** Aprendí a integrar el paquete NuGet de SQLite en un entorno moderno de .NET, configurando la cadena de conexión (`ConnectionStrings`) dentro del archivo `appsettings.json` para enlazar de forma limpia y segura el archivo binario `gestion_tareas.sqlite` ubicado en el directorio del Backend.
*   **Implementación de Endpoints y Métodos HTTP:** Dominé la asignación de verbos HTTP estándar mediante atributos específicos (`[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]`) y rutas personalizadas (`[Route("api/[controller]")]`), permitiendo que la API reciba, procese y devuelva respuestas estructuradas en formato JSON.
*   **Ejecución de Comandos SQL desde Código C#:** Aprendí a instanciar conexiones dinámicas (`SqliteConnection`), abrir comandos transaccionales y ejecutar consultas parametrizadas (`Parameters.AddWithValue`) para realizar de manera segura las operaciones del ciclo CRUD (Lectura, Inserción, Actualización de estados por ID y Borrado de registros).
*   **Pruebas Profesionales de APIs con Postman:** Comprendí el flujo de trabajo de la industria utilizando Postman para simular peticiones HTTP hacia el entorno local (`https://localhost:7019/api/Tareas`), configurando cuerpos de petición en formato `raw JSON` y validando códigos de estado HTTP exitosos (`200 OK`).
*   **Estandarización y Documentación en Markdown:** Consolidé el uso de Markdown estructurado con encabezados, listas de viñetas técnicas y bloques de código para documentar formalmente este proceso dentro de las entregas y el control de versiones del proyecto.

## ¿Qué problemas encontré?

*   **Ausencia de interfaz visual y errores 404 en el navegador:** Al ejecutar el proyecto por primera vez en .NET, el navegador web por defecto arrojaba un error 404 al intentar cargar la ruta raíz, debido a que las APIs puras no poseen páginas HTML estáticas de bienvenida.
*   **Incompatibilidad con interfaces gráficas predeterminadas recientes:** Al no venir habilitada por defecto la interfaz de pruebas visuales (Swagger) en las versiones más recientes de .NET utilizadas, el entorno inicial no mostraba tableros gráficos para interactuar con los endpoints.
*   **Gestión de parámetros dinámicos por ID:** Durante la configuración de las rutas de actualización y eliminación, existía el riesgo de modificar o eliminar registros incorrectos si el mapeo de los parámetros de la URL (`{id}`) no coincidía exactamente con los argumentos esperados en el controlador de C#.

## ¿Cómo los resolví?

*   **Interpretación correcta del comportamiento de una API:** Comprendí que un servidor Backend no requiere interfaces visuales de navegación web local, sino que su propósito es responder a solicitudes mediante endpoints específicos (como `api/Tareas`), los cuales se comprueban directamente mediante la herramienta Postman.
*   **Migración a pruebas profesionales con Postman:** Reorienté la validación técnica utilizando Postman para enviar peticiones personalizadas (`POST`, `PUT`, `DELETE`) con cuerpos en formato JSON, logrando verificar la persistencia de los datos de manera limpia y profesional.
*   **Estructuración precisa de rutas y parámetros:** Ajusté las anotaciones de los métodos en el controlador utilizando los decoradores de ruta con identificadores dinámicos (`[HttpPut("{id}")]` y `[HttpDelete("{id}")]`), asegurando que las operaciones de actualización y borrado impactaran de manera exacta y controlada únicamente el registro seleccionado.

## ¿Qué conceptos aún no domino?

Tengo total claridad sobre la configuración del `appsettings.json`, la inyección de dependencias del contexto de configuración, el diseño de controladores REST y la validación de operaciones CRUD mediante Postman conectadas a SQLite. Mi siguiente desafío, de acuerdo con el plan de trabajo de la práctica en Strategic Software Alliance SAS para las próximas fases, será integrar estos servicios backend con la capa de interfaz de usuario o automatizar flujos avanzados de gestión de datos dentro del sistema.
