/* Captura de elementos clave del DOM para su manipulación */
const formulario = document.querySelector('form');
const inputCorreo = document.getElementById('correo');
const contenedorTareas = document.getElementById('contenedor-tareas');

/* Intercepción del evento 'submit' del formulario de contacto */
formulario.addEventListener('submit', function(evento) {
    evento.preventDefault(); // Bloquea la recarga automática de la página

    let correo = inputCorreo.value;

    /* Validación lógica condicional para verificar campos vacíos */
    if (correo === '') {
        alert('Atención: El campo de correo no puede estar vacío.');
    } else {
        alert('¡Éxito! El correo: ' + correo + ' fue registrado en el sistema.');
        formulario.reset(); // Limpia los campos del formulario tras el éxito
    }
});

/* Estructura de datos simulada (Arreglo de objetos) con las tareas iniciales */
const misTareas = [
    { titulo: "Investigar sobre bases de datos", estado: "Pendiente" },
    { titulo: "Aprender JavaScript", estado: "En progreso" },
    { titulo: "Maquetar sitio web", estado: "Terminada" }
];

/* Función encargada de iterar el arreglo e inyectar componentes dinámicos en el DOM */
function cargarTareas() {
    misTareas.forEach(function(tarea) {
        let elementoTarea = document.createElement('p'); // Creación de etiqueta en memoria
        elementoTarea.textContent = tarea.titulo + ' - Estado: ' + tarea.estado; // Inyección de texto descriptivo
        contenedorTareas.appendChild(elementoTarea); // Ensamblaje visual final en el contenedor
    });
}

/* Ejecución de la función para renderizar las tareas al cargar el script */
cargarTareas();