¿Qué aprendí?

Gestión de archivos desde consola: Utilicé PowerShell para entrar a la carpeta contenedora y renombrar el archivo de lógica ejecutando ren scripts.js script.js, ajustándolo a los estándares.

Conexión del "Cerebro" (JS a HTML): Implementé la etiqueta <script src="script.js"></script> como puente de conexión. Aprendí la regla de oro de colocarla justo antes del cierre del body para garantizar que el navegador primero renderice el DOM antes de que JavaScript intente interactuar con él.

Herramientas de diagnóstico (Debugging): Realicé la primera prueba lógica mediante console.log("¡El sistema de tareas está conectado!");. Inspeccionando el navegador (Ctrl + Shift + I > Pestaña Console), verifiqué la correcta ejecución del código oculto al usuario final.

Captura del DOM: Aprendí a atrapar elementos de la interfaz usando variables inmutables (const) y los métodos document.querySelector('form') y document.getElementById('correo').

Intercepción de Eventos (Event Listeners): Apliqué addEventListener('submit') al formulario para escuchar el clic del usuario. Implementé la instrucción crítica evento.preventDefault() para bloquear la recarga automática del navegador y permitir que JavaScript asumiera el control.

Decisiones Lógicas: Desarrollé una validación condicional (if/else) combinada con variables mutables (let) y el comparador estricto (===) para verificar si el campo de correo estaba vacío. Proporcioné retroalimentación visual al usuario usando la función emergente nativa alert() y limpié el formulario con .reset().

Consumo de datos y Componentes Dinámicos: Preparé el esqueleto creando un contenedor vacío div id="contenedor-tareas". Simulé la recepción de datos mediante la creación de un arreglo de objetos (const misTareas = [...]) con propiedades específicas (titulo, estado). Construí la función cargarTareas() y utilicé un ciclo .forEach() para iterar sobre la lista. Implementé la creación de etiquetas en memoria con document.createElement('p'), inyección de datos con .textContent y el ensamblaje visual final con contenedorTareas.appendChild().

Estilización e Identidad Corporativa: Actualicé el código CSS (Hexadecimal #7BC8E2) para adaptar la página a los colores principales de la empresa. Además, di diseño de "tarjeta moderna" a las tareas inyectadas usando Flexbox vertical (flex-direction: column), box-shadow (profundidad 3D) y un borde decorativo corporativo (border-left).

Accesibilidad y Contraste: Cambié el color del texto del menú y los botones a un tono oscuro (#333333) para garantizar su legibilidad sobre el nuevo color de fondo claro corporativo.

¿Qué problemas encontré?

Interferencia del autocompletado: El editor de código generó automáticamente una estructura de formulario e IDs (como form id="form-tarea" y id="lista-tareas") al presionar Tabulador, lo cual desconectaba mi archivo script.js e iba en contra de mi aprendizaje manual de los fundamentos del DOM.

Pérdida de referencias (ReferenceError): Al actualizar el bloque de código de validación, borré accidentalmente las declaraciones iniciales (const). La consola arrojó el error formulario is not defined porque JavaScript quedó ciego al perder la conexión con las etiquetas.

Sensibilidad a mayúsculas/minúsculas (Case-sensitive): JavaScript arrojó un error al definir una variable con mayúscula inicial (let ElementoTarea) e intentar usarla en la línea siguiente con minúscula (elementoTarea.textContent), asumiendo que eran entidades distintas.

¿Cómo los resolví?

Gestión manual de código: Borré el bloque completo sugerido por la herramienta de autocompletado e ingresé manualmente el section id="tareas" y el contenedor específico (id="contenedor-tareas") requerido por mi script, priorizando la comprensión lógica sin depender de IA.

Reubicación de variables: Comprendí que el orden de declaración importa en programación estructurada. Restauré las constantes en la parte superior del archivo script.js para asegurar que las variables estuvieran definidas antes de aplicar los Event Listeners.

Auditoría de sintaxis: Apliqué un debugging rápido unificando el nombre de la variable (elementoTarea) en todo el bloque de la función constructora para permitir la inyección correcta del texto.

¿Qué conceptos aún no domino?

El manejo del DOM, las validaciones lógicas con if/else y la creación de componentes dinámicos con iteradores (forEach) me resultan claros ahora y el código es totalmente funcional. Mi siguiente desafío, según mi plan de práctica, será abandonar los arreglos de datos simulados (Hardcoding) y comprender cómo conectarme a bases de datos remotas mediante llamadas asíncronas (APIs / Fetch), lo cual abordaré en las próximas fases del proyecto.