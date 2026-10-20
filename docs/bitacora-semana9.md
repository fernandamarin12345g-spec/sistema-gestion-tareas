# Bitácora Semana 9

## ¿Qué aprendí?

- **Refactorización UI/UX con CSS Grid:** Aprendí a transformar un diseño web lineal basado en tarjetas simples en un Dashboard empresarial moderno, implementando la propiedad `display: grid` y `repeat(auto-fit, minmax(...))` para adaptar la interfaz visual.
- **Diseño de Tablas de Datos Dinámicas (Data Grids):** Sustituí las tarjetas individuales por filas de datos centralizadas, organizando la visualización de IDs, títulos, estados y botones de acción en columnas alineadas.
- **Auditoría de Calidad (Pruebas y Validación):** Comprendí la importancia de estructurar una Matriz de Pruebas para evaluar tanto la interfaz web como las respuestas directas de la API.
- **Manejo de Errores de Base de Datos:** Aprendí a identificar excepciones críticas en el backend, específicamente cómo SQLite bloquea las transacciones por llaves foráneas (`SqliteErrorCode == 19`) al intentar borrar registros asociados.
- **Aislamiento de Comandos en C#:** Entendí que reutilizar un mismo objeto de comandos SQL para consultas consecutivas (`SELECT` y luego `UPDATE`) genera conflictos de parámetros, obligando a separar la lógica en bloques independientes.

## ¿Qué problemas encontré?

- **Colapso del servidor por parámetros sucios en el PUT:** Al editar registros, el servidor arrojaba errores 500 porque el comando SQL retenía los parámetros de consultas anteriores, rompiendo la ejecución.
- **Bloqueos por integridad referencial:** Al intentar eliminar estados o roles que ya tenían tareas o usuarios asignados, la base de datos arrojaba excepciones no controladas que dejaban la aplicación sin respuesta clara para el usuario.
- **Desbordamiento visual en las celdas del Grid:** Tras aplicar el nuevo diseño de tablas, los nombres largos de los elementos chocaban entre columnas, desalineando la estructura visual en la interfaz.

## ¿Cómo los resolví?

- **Separación de transacciones:** Refactoricé el código de los controladores en C# para instanciar comandos totalmente independientes para las validaciones y para las actualizaciones, limpiando la memoria de parámetros.
- **Implementación de bloques `try-catch` para excepciones SQL:** Envolví las operaciones de borrado en bloques protegidos para interceptar el error 19 de SQLite y retornar un código `400 BadRequest` con un mensaje descriptivo que el frontend pudiera mostrar en un `alert()`.
- **Ajuste de anchos de columna en CSS:** Modifiqué los valores de `grid-template-columns` en las hojas de estilo para otorgar un espacio fijo adecuado a los textos largos, evitando que se encimaran.

## ¿Qué conceptos aún no domino?

Aunque el sistema es funcional y he logrado integrar con éxito el backend, el frontend y la base de datos, reconozco que todavía me encuentro en un proceso de consolidación de mis bases técnicas. Me cuesta asimilar de memoria toda la sintaxis y la estructura lógica al programar desde cero, pero aún así logro resolverlo de manera correcta. Asimismo, el manejo del flujo asíncrono (`async/await`) en JavaScript y la sincronización del DOM siguen siendo áreas donde necesito ganar mayor fluidez y seguridad operativa para evitar fallos de tiempo de ejecución. Mi enfoque para las próximas fases será practicar de manera más eficiente estos patrones de desarrollo, preparándome de la mejor manera para el reto de la arquitectura móvil.
