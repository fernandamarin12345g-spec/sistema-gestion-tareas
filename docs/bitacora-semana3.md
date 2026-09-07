¿Qué aprendí?
Durante esta tercera semana llevé el control de versiones, la arquitectura de directorios y la documentación de mi proyecto a un nivel profesional, simulando un flujo de trabajo real de desarrollo de software para el sistema de gestión de tareas.

Extensiones estratégicas en VS Code:

Instalé GitLens (usando Ctrl + Shift + X), una herramienta fundamental que muestra un texto tenue al lado de cada línea de código indicando el autor exacto y el mensaje del commit donde fue modificada, facilitando la auditoría visual.

Instalé Prettier - Code formatter, el cual limpia y organiza la estructura visual del código de forma automática, unificando estándares profesionales en múltiples lenguajes.

Aprendí a usar Ctrl + B para ocultar o mostrar el panel lateral y optimizar mi espacio de trabajo.

Aislamiento y ramas (Branches): Comprendí la importancia de no programar directamente en la línea base (main). Aprendí a verificar el estado de la ruta con git status, a crear entornos seguros con git branch feature-arquitectura y a moverme físicamente a ellos mediante git checkout feature-arquitectura.

Estructura de directorios avanzada: Utilicé la terminal (PowerShell) con el comando ni -Force (ej. ni src/app.txt -Force y ni docs/bitacora-semana3.md -Force) para crear carpetas y archivos anidados de un solo golpe, separando el código fuente de la documentación.

Reorganización de archivos: Trasladé mis bitácoras anteriores y el mapa conceptual de la semana pasada a la carpeta docs usando el comando de movimiento mv (ej. mv bitacora-semana1.md docs/), manteniendo el repositorio limpio y ordenado.

Documentación técnica y Commits Semánticos: Redacté el archivo README.md estructurándolo con sintaxis Markdown (#, ##, viñetas) para detallar la descripción del sistema multiplataforma enfocado en la gestión y seguimiento de tareas. Además, implementé Conventional Commits usando prefijos profesionales:

feat: para nuevas estructuras o características.

docs: para actualizaciones de documentación y el README.

chore: para tareas de mantenimiento y reorganización de carpetas.

Fusión de código (Merge) y Sincronización: Aprendí a regresar a la rama principal con git checkout main, a integrar el trabajo seguro de mi rama paralela usando git merge feature-arquitectura, a vincular por primera vez la rama con la nube usando git push -u origin feature-arquitectura, y a limpiar el repositorio local borrando la rama de forma segura con git branch -D.

Auditoría gráfica del historial: Utilicé el comando avanzado git log --graph --oneline --all para generar un mapa visual en la terminal que ilustra cómo se ramificó y unió el código, guardando la captura como imagen (git-graph-semana3.png) dentro de la carpeta docs para respaldarla en GitHub.

¿Qué problemas encontré?
Al intentar cambiar de rama con git checkout, escribí mal el nombre (feauture-arquitectura con una "u" extra) y la terminal me arrojó un error indicando que el archivo o ruta no existía en los registros de Git.

En un commit anterior olvidé incluir la bandera -m con el mensaje, lo que provocó que la terminal se bloqueara abriendo un editor de texto desconocido (Vim) del cual no podía salir con el ratón.

Al ejecutar git checkout main antes de hacer el merge, noté que todas las carpetas nuevas (src, docs) desaparecieron repentinamente de mi panel izquierdo en VS Code, lo cual me causó confusión.

Al intentar hacer el primer git push en mi nueva rama, la terminal se detuvo mostrando un error fatal indicando que la rama no tenía un repositorio upstream asignado en la nube (fatal: The current branch feature-arquitectura has no upstream branch).

Al intentar borrar la rama local con git branch -d antes de completar la fusión correctamente, la terminal emitió una advertencia de seguridad bloqueando la acción para evitar pérdida de datos.

¿Cómo los resolví?
Revisé detalladamente el error en la consola, corregí el tipeo escribiendo el nombre exacto de la rama (feature-arquitectura) y entendí que Git es sumamente estricto con los caracteres.

Para salir del bloqueo de la terminal con el editor Vim, presioné la tecla Esc, escribí :q! y presioné Enter para abortar el proceso de manera segura y volver a escribir el comando correctamente.

Comprendí que la desaparición temporal de los archivos es parte del aislamiento de entornos en Git; los archivos no se borraron, simplemente estaban guardados en la otra rama y reaparecieron de inmediato al ejecutar el git merge.

Solucioné el problema del push ejecutando el comando exacto que la misma terminal me sugirió: git push --set-upstream origin feature-arquitectura (o su atajo git push -u), logrando sincronizar mi entorno local con GitHub.

Atendí la pista de la consola asegurándome de posicionarme en main, realizar primero el git merge y, una vez integrado todo el código, utilicé la bandera en mayúscula (git branch -D feature-arquitectura) para forzar su borrado local de forma limpia.

¿Qué conceptos aún no domino?
Tengo claro el flujo completo de creación de ramas, commits semánticos y fusiones locales/remotas, pero me gustaría practicar más adelante cómo resolver un "conflicto de fusión" (merge conflict) en caso de que dos líneas de código entren en contradicción al trabajar en equipos de desarrollo más grandes.