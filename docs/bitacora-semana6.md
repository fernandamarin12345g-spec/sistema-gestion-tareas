# Bitácora Semana 6

## ¿Qué aprendí?

*   **Arquitectura de Bases de Datos en Tercera Forma Normal (3NF):** Comprendí cómo descomponer la información del sistema de gestión de tareas en tablas maestras y relacionales (`Roles`, `Proyectos`, `Etiqueta`, `Usuarios`, `Tareas`, `Tarea_Etiqueta`, y `Auditoria_Log`) para eliminar la redundancia y garantizar la integridad referencial.
*   **Lenguaje de Definición de Datos (DDL):** Aprendí a estructurar la creación física de tablas adaptadas al dialecto de SQLite, utilizando tipos de datos precisos (`VARCHAR`, `DATETIME`, `BOOLEAN` simulado con enteros `0/1`) y restricciones avanzadas como llaves primarias autoincrementales (`INTEGER PRIMARY KEY AUTOINCREMENT`) y llaves foráneas con reglas de cascada (`ON DELETE CASCADE` y `ON DELETE SET NULL`).
*   **Lenguaje de Manipulación y Consulta (DML y DQL):** Ejecuté inserciones múltiples (Bulk Inserts) de datos de prueba e implementé consultas multitabla avanzadas utilizando el comando `SELECT` en conjunto con múltiples operaciones `JOIN` y `ON` para cruzar la información de IDs numéricos hacia nombres legibles de usuarios, proyectos y etiquetas.
*   **Implementación Completa del Ciclo CRUD:** Comprendí la aplicación práctica de las cuatro operaciones fundamentales de los datos: **C**rear (`INSERT`), **R**eer (`SELECT`), **U**p actualizar (`UPDATE` con cláusula de seguridad `WHERE`) y **D**efinir/Borrar (`DELETE` controlado), asegurando que el sistema pueda modificar o depurar registros sin comprometer la estructura general.
*   **Implementación y Dominio del Formato Markdown:** Aprendí a estructurar formalmente toda la documentación técnica oficial del proyecto utilizando **Markdown**, empleando encabezados jerárquicos (`##`, `###`), listas organizadas, bloques de código SQL especializados y referencias cruzadas limpias. Comprendí que este formato estructurado es el estándar indiscutible de la industria para redactar archivos fundamentales como el `README.md` y las bitácoras dentro de los repositorios de GitHub, facilitando una lectura profesional, limpia y jerarquizada para las revisiones periódicas del tutor, Luis Abreu Acevedo.
*   **Entornos Locales para Bases de Datos (SQLite en VS Code):** Instalé y configuré la extensión de SQLite en Visual Studio Code, creando el archivo binario compilado (`gestion_tareas.sqlite`) dentro de la carpeta `docs` para validar de forma directa la ejecución de scripts y consultas sin requerir servidores externos complejos.

## ¿Qué problemas encontré?

*   **Conflicto de duplicidad en la ejecución de scripts (`Table already exists`):** Al intentar reejecutar todo el archivo `esquema_base_datos.sql` completo usando los atajos de ejecución en el editor, el motor de SQLite se detenía abruptamente y lanzaba un error en la consola porque las tablas maestras ya habían sido creadas en una prueba previa.
*   **Incompatibilidad de dialectos SQL previos:** Al intentar aplicar instrucciones nativas de otros motores de bases de datos como `SERIAL` (propio de PostgreSQL), el intérprete de SQLite arrojaba errores de sintaxis, impidiendo la correcta creación de las llaves primarias autoincrementales.
*   **Interpretación binaria del archivo de base de datos:** Al hacer clic por error en el archivo físico `gestion_tareas.sqlite`, Visual Studio Code intentó renderizarlo como un documento de texto plano, mostrando caracteres ilegibles, símbolos de interrogación y bloques de bytes binarios.

## ¿Cómo los resolví?

*   **Selección de bloques específicos:** Aprendí a aislar la ejecución seleccionando únicamente el bloque de la consulta `SELECT` o las operaciones específicas con los atajos del editor, evitando reintentar la creación de tablas existentes y obteniendo de inmediato la visualización limpia de los resultados en el panel derecho.
*   **Adaptación de sintaxis al estándar SQLite:** Reemplacé el modificador `SERIAL` por la estructura exacta de tres palabras exigida por SQLite (`INTEGER PRIMARY KEY AUTOINCREMENT`), y ajusté los valores booleanos a formato numérico (`1/0`), logrando una compatibilidad perfecta en el entorno local.
*   **Gestión correcta de archivos compilados:** Identifiqué que los archivos con extensión `.sqlite` son contenedores binarios optimizados. Comprendí que nunca deben editarse de forma manual como texto plano, sino que la manipulación lógica debe realizarse siempre a través del script estructurado en `.sql`.

## ¿Qué conceptos aún no domino?

Tengo total claridad sobre la modelación relacional en 3NF, la sintaxis DDL/DML/DQL, el ciclo CRUD completo, la documentación profesional estructurada en Markdown y la gestión de archivos SQLite locales. Mi siguiente desafío, de acuerdo con el plan de trabajo de la práctica para la **Semana 7**, será dar el salto hacia el desarrollo del **Backend y la creación de una API REST**, conectando esta base de datos mediante endpoints, métodos HTTP y el manejo estructurado de respuestas en formato JSON.
