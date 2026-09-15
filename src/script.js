const formulario = document.querySelector('form');
const inputCorreo = document.getElementById('correo');
const contenedorTareas = document.getElementById('contenedor-tareas');

formulario.addEventListener('submit', function(evento) {
    evento.preventDefault();

    let correo = inputCorreo.value;

    if (correo === '') {
        alert('Atención: El campo de correo no puede estar vacío.');
    } else {
        alert('¡Éxito! El correo: ' + correo + ' fue registrado en el sistema.');
        formulario.reset();
    }
});


const misTareas = [
    { titulo: "Investigar sobre bases de datos", estado: "Pendiente" },
    { titulo: "Aprender JavaScript", estado: "En progreso" },
    { titulo: "Maquetar sitio web", estado: "Terminada" }
];


function cargarTareas() {
    misTareas.forEach(function(tarea) {
        let elementoTarea = document.createElement('p'); // Corregida la 'e' minúscula
        elementoTarea.textContent = tarea.titulo + ' - Estado: ' + tarea.estado;
        contenedorTareas.appendChild(elementoTarea);
    });
}


cargarTareas();

