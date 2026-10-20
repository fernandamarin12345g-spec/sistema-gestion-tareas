## 2. Documentación de Debugging y Corrección de Errores

Durante la fase de integración y pruebas de la Semana 9, se diagnosticaron y resolvieron los siguientes errores críticos utilizando el depurador de Visual Studio y las herramientas de desarrollador del navegador.

### Bug 1: Conflicto de parámetros en actualizaciones (PUT)
* **Excepción detectada:** `System.InvalidOperationException: 'Must add values for the following parameters:'`
* **Módulos afectados:** `UsuariosController.cs` y `TareasController.cs`
* **Diagnóstico:** Al ejecutar el método `PUT`, el sistema realizaba primero una consulta `SELECT` (para verificar correos duplicados o existencias) y luego reutilizaba el mismo objeto `SqliteCommand` para ejecutar el `UPDATE`. Al cambiar el `CommandText`, SQLite y C# conservaban el estado anterior de los parámetros, exigiendo valores que ya no correspondían a la nueva consulta.
* **Solución implementada:** Se refactorizó el código aislando las transacciones. Se implementaron bloques `using` independientes para crear comandos separados (`comandoVerificar` y `comandoUpdate`). De esta manera, cada consulta maneja su propio ciclo de vida y sus propios parámetros (`Parameters.AddWithValue`), garantizando una ejecución limpia.

### Bug 2: Violación de Integridad Referencial al Eliminar
* **Excepción detectada:** `Microsoft.Data.Sqlite.SqliteException: 'SQLite Error 19: FOREIGN KEY constraint failed.'`
* **Módulo afectado:** `EstadosController.cs` (Método `DELETE`)
* **Diagnóstico:** El sistema colapsaba con una excepción no controlada en el backend cuando un usuario intentaba eliminar un "Estado" (ej. "En proceso") que actualmente estaba siendo referenciado (asignado) por una o más tareas en la tabla `Tareas`. SQLite bloqueó la transacción por seguridad para evitar dejar registros huérfanos.
* **Solución implementada:** Se implementó control de excepciones defensivo. Se envolvió el método `ExecuteNonQuery()` en un bloque `try-catch` capturando explícitamente la condición `when (ex.SqliteErrorCode == 19)`. En lugar de romper el servidor, ahora el backend retorna un código HTTP `400 BadRequest` con un mensaje descriptivo controlado: *"No se puede eliminar este estado porque hay tareas que actualmente lo están usando"*, el cual es interpretado e impreso por el frontend mediante un `alert()`.