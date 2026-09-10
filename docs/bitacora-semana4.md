¿Qué aprendí?

Aislamiento estratégico: Implementé el comando git checkout -b feature-web-informativa para crear y saltar inmediatamente a una nueva rama. Esto garantiza la protección del código estable en la rama main mientras construyo el sitio web informativo inicial.

Gestión simultánea de archivos en terminal: Eliminé el archivo de prueba usando rm src/app.txt y logré crear múltiples archivos de código en una sola ejecución separando las rutas por comas: ni src/index.html, src/styles.css -Force.

Eficiencia con atajos de teclado: Oculté el panel de directorios con Ctrl + B para despejar el área de trabajo. Abrí archivos sin tocar el ratón mediante el buscador integrado Ctrl + P. Navegué a líneas exactas de código (como la línea 6) con Ctrl + G y generé saltos de línea limpios desde cualquier punto usando Ctrl + Enter.

Generación de código (Boilerplate): Utilicé la abreviatura ! de Emmet para autogenerar toda la estructura indispensable de HTML5. Analicé cada una de sus etiquetas base: <html lang="es"> (La raíz del proyecto), <head> (El cerebro oculto de configuraciones), <meta name="viewport"...> (Etiqueta fundamental para habilitar el responsive design) y <body> (El espacio donde se redactará todo el contenido visual).

Configuración inicial: Ajusté la etiqueta <title> al nombre oficial ("Sistema de Gestión de Tareas") y empleé la abreviatura link:css para inyectar automáticamente el enlace a la hoja de estilos.

Estructuración semántica del HTML: Utilicé Emmet (header>nav>ul>li*4>a) para crear el menú superior. Comprendí que <nav> aloja los enlaces principales y <ul>/<li> los agrupan. Estructuré el contenido con <main> (contenedor principal) y <section id="home">. Usé <h1> para el título principal (solo debe haber uno por documento) y armé un formulario de contacto conectando las etiquetas <form>, <label> (texto descriptivo), <input> (caja interactiva) y <button>.

Reseteo y Estilos Base (CSS): Apliqué el selector universal * para eliminar márgenes (margin: 0) y rellenos (padding: 0) por defecto, junto con box-sizing: border-box para evitar que las cajas se deformen. Configuré el body con tipografía moderna, altura de línea de 1.6 y colores de alto contraste para mejorar la lectura.

Layouts con Flexbox: Transformé el menú vertical en horizontal usando display: flex, centrándolo con justify-content: center y separándolo con gap: 2rem. Para el formulario, usé flex-direction: column para apilar los elementos verticalmente, align-items: center para centrarlos y la técnica de margin: 2rem auto para posicionar la caja perfectamente en el medio de la pantalla.

Interactividad visual: Quité los subrayados de los enlaces con text-decoration: none y apliqué la pseudo-clase :hover para que los botones y enlaces cambien de color al pasar el cursor (retroalimentación visual).

Responsive Design (Diseño Adaptable): Implementé la regla condicional @media (max-width: 768px) para detectar dispositivos móviles. En este tamaño, sobrescribí Flexbox para que el menú vuelva a apilarse verticalmente. Validé este comportamiento abriendo las herramientas de desarrollador (Ctrl + Shift + I) y el emulador móvil (Ctrl + Shift + M).

Fusión de Ramas (Git Merge): Tras guardar los avances, utilicé git checkout main para regresar a la rama oficial. Ejecuté git merge feature-web-informativa para trasladar todo mi trabajo finalizado sin conflictos (Fast-forward) y finalmente sincronicé la nube con git push.

¿Qué problemas encontré?

Al intentar subir mis archivos iniciales con git push, la consola bloqueó la acción con un error fatal, indicando que la nueva rama (feature-web-informativa) no tenía un origen remoto asignado en la nube (no upstream branch).

¿Cómo los resolví?

Identifiqué que era una advertencia de sincronización esperada al crear ramas nuevas. Apliqué la instrucción sugerida por la consola: git push -u origin feature-web-informativa, lo cual enlazó permanentemente mi computadora con el repositorio en GitHub y subió los avances de manera exitosa.

¿Qué conceptos aún no domino?

La preparación del entorno y la estructura base del código HTML me quedaron claras gracias a los atajos, pero estaba a la expectativa de enfrentar la implementación de etiquetas semánticas avanzadas, menús de navegación y reglas de diseño visual responsivo. (Actualización: Tras finalizar esta sesión, logré comprender la lógica de Flexbox y Media Queries, por lo que el próximo desafío será la manipulación interactiva con JavaScript o la conexión con bases de datos).