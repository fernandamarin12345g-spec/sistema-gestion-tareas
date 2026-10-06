// Defino la ruta de mi API en ASP.NET Core
const API_URL = 'https://localhost:7019/api/Tareas';

// Capturo los elementos de mi interfaz que voy a manipular
const contenedorTareas = document.getElementById('contenedor-tareas');
const formCrearTarea = document.getElementById('form-crear-tarea');
const inputTitulo = document.getElementById('titulo-tarea');

// Guardo la lista de tareas en memoria para poder filtrarlas rápidamente sin recargar la base de datos
let miListaDeTareas = [];

/* 1. LECTURA Y RENDERIZADO (GET) */
async function cargarTareas() {
    try {
        contenedorTareas.innerHTML = '<p>Conectando con la base de datos...</p>';
        
        const respuesta = await fetch(API_URL);
        if (!respuesta.ok) throw new Error('Falló la respuesta del servidor.');

        miListaDeTareas = await respuesta.json();
        renderizarTareas(miListaDeTareas);

    } catch (error) {
        console.error('Error de conexión:', error);
        contenedorTareas.innerHTML = '<p style="color: red;">Error: No pude conectar con el Backend.</p>';
    }
}

function renderizarTareas(tareas) {
    contenedorTareas.innerHTML = ''; 

    if (tareas.length === 0) {
        contenedorTareas.innerHTML = '<p>No hay tareas para mostrar en este filtro.</p>';
        return;
    }

    tareas.forEach(tarea => {
        const divTarea = document.createElement('div');
        divTarea.style.cssText = 'background-color: #ffffff; padding: 15px 30px; border-radius: 8px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1); width: 80%; max-width: 600px; display: flex; justify-content: space-between; align-items: center; border-left: 5px solid #7BC8E2; margin: 0 auto 15px auto;';

        divTarea.innerHTML = `
            <div style="text-align: left; flex-grow: 1;">
                <strong>ID: ${tarea.id}</strong> - ${tarea.titulo}
                <br>
                <small>Estado actual: 
                    <select onchange="actualizarEstado(${tarea.id}, this.value)" style="padding: 3px; margin-top: 5px; border-radius: 4px;">
                        <option value="Pendiente" ${tarea.estado === 'Pendiente' ? 'selected' : ''}>Pendiente</option>
                        <option value="En progreso" ${tarea.estado === 'En progreso' ? 'selected' : ''}>En progreso</option>
                        <option value="Terminada" ${tarea.estado === 'Terminada' ? 'selected' : ''}>Terminada</option>
                    </select>
                </small>
            </div>
            <button onclick="eliminarTarea(${tarea.id})" style="background-color: #ff6b6b; color: white; border: none; padding: 6px 12px; border-radius: 4px; cursor: pointer; margin-left: 15px; font-size: 0.9rem;">Eliminar</button>
        `;
        contenedorTareas.appendChild(divTarea);
    });
}

/* 2. CREAR TAREA (POST) */
formCrearTarea.addEventListener('submit', async function(evento) {
    evento.preventDefault(); 
    const tituloTexto = inputTitulo.value.trim();

    if (tituloTexto === '') {
        alert('El título no puede estar vacío.');
        return;
    }

    try {
        const respuesta = await fetch(API_URL, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ titulo: tituloTexto })
        });

        if (respuesta.ok) {
            inputTitulo.value = ''; 
            cargarTareas(); 
        }
    } catch (error) {
        console.error('Error al enviar los datos:', error);
    }
});

/* 3. ACTUALIZAR ESTADO (PUT) */
async function actualizarEstado(id, nuevoEstado) {
    try {
        const respuesta = await fetch(`${API_URL}/${id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ estado: nuevoEstado })
        });

        if (respuesta.ok) {
            cargarTareas(); 
        } else {
            alert('No pude actualizar el estado de la tarea en el servidor.');
        }
    } catch (error) {
        console.error('Error al actualizar:', error);
    }
}

/* 4. ELIMINAR TAREA (DELETE) */
async function eliminarTarea(id) {
    if (!confirm('¿Estoy segura de que deseo eliminar esta tarea de la base de datos?')) return;

    try {
        const respuesta = await fetch(`${API_URL}/${id}`, { method: 'DELETE' });
        if (respuesta.ok) {
            cargarTareas(); 
        }
    } catch (error) {
        console.error('Error al eliminar:', error);
    }
}

/* 5. FILTROS VISUALES */
function filtrarVista(criterio) {
    if (criterio === 'Todas') {
        renderizarTareas(miListaDeTareas); 
    } else {
        const tareasFiltradas = miListaDeTareas.filter(tarea => tarea.estado === criterio);
        renderizarTareas(tareasFiltradas);
    }
}

cargarTareas();

/* MÓDULO: MAESTRO DE ESTADOS */

const API_ESTADOS_URL = 'https://localhost:7019/api/Estados';
const contenedorEstados = document.getElementById('contenedor-estados');
const formCrearEstado = document.getElementById('form-crear-estado');
const inputNombreEstado = document.getElementById('nombre-estado');
let miListaDeEstados = [];

async function cargarEstados() {
    try {
        const respuesta = await fetch(API_ESTADOS_URL);
        if (!respuesta.ok) throw new Error('Falló la respuesta del servidor al cargar estados.');
        
        miListaDeEstados = await respuesta.json();
        renderizarEstados(miListaDeEstados);
    } catch (error) {
        console.error('Error:', error);
        contenedorEstados.innerHTML = '<p style="color: red;">No se pudieron cargar los estados.</p>';
    }
}

function renderizarEstados(estados) {
    contenedorEstados.innerHTML = '';
    
    if (estados.length === 0) {
        contenedorEstados.innerHTML = '<p>No hay estados registrados.</p>';
        return;
    }

    estados.forEach(estado => {
        const divEstado = document.createElement('div');
        divEstado.className = 'tarjeta-tarea-dinamica'; 
        
        divEstado.innerHTML = `
            <div class="contenido-tarea">
                <strong>ID: ${estado.id_estado}</strong> - ${estado.nombre_estado}
            </div>
            <button class="btn-eliminar" onclick="eliminarEstado(${estado.id_estado})">Eliminar</button>
        `;
        contenedorEstados.appendChild(divEstado);
    });
}

formCrearEstado.addEventListener('submit', async function(evento) {
    evento.preventDefault();
    const nombreTexto = inputNombreEstado.value.trim();

    if (nombreTexto === '') return;

    try {
        const respuesta = await fetch(API_ESTADOS_URL, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ nombre_estado: nombreTexto })
        });

        if (respuesta.ok) {
            inputNombreEstado.value = '';
            cargarEstados(); 
        }
    } catch (error) {
        console.error('Error al crear estado:', error);
    }
});

async function eliminarEstado(id) {
    if (!confirm('¿Segura que deseas eliminar este estado?')) return;

    try {
        const respuesta = await fetch(`${API_ESTADOS_URL}/${id}`, { method: 'DELETE' });
        if (respuesta.ok) {
            cargarEstados();
        }
    } catch (error) {
        console.error('Error al eliminar estado:', error);
    }
}

cargarEstados();

/*MÓDULO: MAESTRO DE ROLES */

const API_ROLES_URL = 'https://localhost:7019/api/Roles';
const contenedorRoles = document.getElementById('contenedor-roles');
const formCrearRol = document.getElementById('form-crear-rol');
const inputNombreRol = document.getElementById('nombre-rol');
const checkboxEsAdmin = document.getElementById('es-admin');
let miListaDeRoles = [];

async function cargarRoles() {
    try {
        const respuesta = await fetch(API_ROLES_URL);
        if (!respuesta.ok) throw new Error('Falló la respuesta del servidor al cargar roles.');
        
        miListaDeRoles = await respuesta.json();
        renderizarRoles(miListaDeRoles);
    } catch (error) {
        console.error('Error:', error);
        if(contenedorRoles) contenedorRoles.innerHTML = '<p style="color: red;">No se pudieron cargar los roles.</p>';
    }
}

function renderizarRoles(roles) {
    if(!contenedorRoles) return;
    contenedorRoles.innerHTML = '';
    
    if (roles.length === 0) {
        contenedorRoles.innerHTML = '<p>No hay roles registrados.</p>';
        return;
    }

    roles.forEach(rol => {
        const divRol = document.createElement('div');
        divRol.className = 'tarjeta-tarea-dinamica'; 
        
        const etiquetaAdmin = rol.is_admin ? '<span class="etiqueta-admin">Administrador</span>' : '';

        divRol.innerHTML = `
            <div class="contenido-tarea">
                <strong>ID: ${rol.id_rol}</strong> - ${rol.nombre_rol} ${etiquetaAdmin}
            </div>
            <button class="btn-eliminar" onclick="eliminarRol(${rol.id_rol})">Eliminar</button>
        `;
        
        contenedorRoles.appendChild(divRol);
    });
}

if(formCrearRol) {
    formCrearRol.addEventListener('submit', async function(evento) {
        evento.preventDefault();
        const nombreTexto = inputNombreRol.value.trim();
        const esAdmin = checkboxEsAdmin.checked; 

        if (nombreTexto === '') return;

        try {
            const respuesta = await fetch(API_ROLES_URL, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ nombre_rol: nombreTexto, is_admin: esAdmin })
            });

            if (respuesta.ok) {
                inputNombreRol.value = '';
                checkboxEsAdmin.checked = false; 
                cargarRoles(); 
            }
        } catch (error) {
            console.error('Error al crear rol:', error);
        }
    });
}

async function eliminarRol(id) {
    if (!confirm('¿Segura que deseas eliminar este rol?')) return;

    try {
        const respuesta = await fetch(`${API_ROLES_URL}/${id}`, { method: 'DELETE' });
        if (respuesta.ok) {
            cargarRoles();
        }
    } catch (error) {
        console.error('Error al eliminar rol:', error);
    }
}

cargarRoles();

/*MÓDULO: MAESTRO DE USUARIOS */
const API_USUARIOS_URL = 'https://localhost:7019/api/Usuarios';
const contenedorUsuarios = document.getElementById('contenedor-usuarios');
const formCrearUsuario = document.getElementById('form-crear-usuario');
const selectRolUsuario = document.getElementById('rol-usuario');

async function cargarRolesParaSelect() {
    if (!selectRolUsuario) return;
    try {
        const respuesta = await fetch(API_ROLES_URL);
        if (respuesta.ok) {
            const roles = await respuesta.json();
            selectRolUsuario.innerHTML = '<option value="">Selecciona un rol para este usuario...</option>';
            roles.forEach(rol => {
                selectRolUsuario.innerHTML += `<option value="${rol.id_rol}">${rol.nombre_rol}</option>`;
            });
        }
    } catch (error) {
        console.error('Error al cargar roles para el select:', error);
    }
}

async function cargarUsuarios() {
    try {
        const respuesta = await fetch(API_USUARIOS_URL);
        if (!respuesta.ok) throw new Error('Falló la respuesta del servidor al cargar usuarios.');
        
        const usuarios = await respuesta.json();
        renderizarUsuarios(usuarios);
    } catch (error) {
        console.error('Error:', error);
        if(contenedorUsuarios) contenedorUsuarios.innerHTML = '<p style="color: red;">No se pudieron cargar los usuarios.</p>';
    }
}

function renderizarUsuarios(usuarios) {
    if (!contenedorUsuarios) return;
    contenedorUsuarios.innerHTML = '';
    
    if (usuarios.length === 0) {
        contenedorUsuarios.innerHTML = '<p>No hay usuarios registrados.</p>';
        return;
    }

    usuarios.forEach(usuario => {
        let htmlTareas = '';
        if (usuario.tareas && usuario.tareas.length > 0) {
            htmlTareas = '<ul class="lista-tareas-usuario">';
            usuario.tareas.forEach(tarea => {
                htmlTareas += `
                    <li class="item-tarea-usuario">
                        <span><strong>#${tarea.id_tarea}</strong> - ${tarea.titulo}</span>
                        <span class="badge-estado">${tarea.estado}</span>
                    </li>
                `;
            });
            htmlTareas += '</ul>';
        } else {
            htmlTareas = '<p class="texto-sin-tareas">Este usuario no tiene tareas asignadas actualmente.</p>';
        }

        const divUsuario = document.createElement('div');
        divUsuario.className = 'tarjeta-usuario';
        
        divUsuario.innerHTML = `
            <div class="cabecera-usuario">
                <div>
                    <strong>${usuario.nombre}</strong> <br>
                    <small>${usuario.correo}</small> <br>
                    <span class="etiqueta-rol">${usuario.nombre_rol}</span>
                </div>
                <div>
                    <button class="btn-editar" onclick="prepararEdicionUsuario(${usuario.id_usuario}, '${usuario.nombre}', '${usuario.correo}', ${usuario.id_rol})">Editar</button>
                    <button class="btn-eliminar" onclick="eliminarUsuario(${usuario.id_usuario})">Eliminar</button>
                </div>
            </div>
            <div>
                <h4 class="titulo-tareas">Tareas Asignadas:</h4>
                ${htmlTareas}
            </div>
        `;
        
        contenedorUsuarios.appendChild(divUsuario);
    });
}

// Evento por defecto para CREAR (POST)
if (formCrearUsuario) {
    formCrearUsuario.addEventListener('submit', async function(evento) {
        evento.preventDefault();
        
        const nombreTexto = document.getElementById('nombre-usuario').value.trim();
        const correoTexto = document.getElementById('correo-usuario').value.trim();
        const idRolSeleccionado = parseInt(selectRolUsuario.value);

        if (nombreTexto === '' || correoTexto === '' || isNaN(idRolSeleccionado)) {
            alert("Por favor completa todos los campos y selecciona un rol.");
            return;
        }

        try {
            const respuesta = await fetch(API_USUARIOS_URL, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ 
                    nombre: nombreTexto, 
                    correo: correoTexto, 
                    id_rol: idRolSeleccionado 
                })
            });

            if (respuesta.ok) {
                document.getElementById('nombre-usuario').value = '';
                document.getElementById('correo-usuario').value = '';
                selectRolUsuario.value = '';
                cargarUsuarios(); 
            }
        } catch (error) {
            console.error('Error al crear usuario:', error);
        }
    });
}

// Función para actualizar (PUT) - Carga los datos arriba y cambia el comportamiento del formulario
function prepararEdicionUsuario(id, nombreActual, correoActual, idRolActual) {
    document.getElementById('nombre-usuario').value = nombreActual;
    document.getElementById('correo-usuario').value = correoActual;
    selectRolUsuario.value = idRolActual;

    document.getElementById('btn-submit-usuario').textContent = 'Actualizar Usuario';
    document.getElementById('btn-cancelar').style.display = 'inline-block';

    formCrearUsuario.onsubmit = async function(evento) {
        evento.preventDefault();

        const nuevoNombre = document.getElementById('nombre-usuario').value.trim();
        const nuevoCorreo = document.getElementById('correo-usuario').value.trim();
        const nuevoRol = parseInt(selectRolUsuario.value);

        try {
            const respuesta = await fetch(`${API_USUARIOS_URL}/${id}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ nombre: nuevoNombre, correo: nuevoCorreo, id_rol: nuevoRol })
            });

            if (respuesta.ok) {
                alert('Usuario actualizado exitosamente');
                document.getElementById('nombre-usuario').value = '';
                document.getElementById('correo-usuario').value = '';
                selectRolUsuario.value = '';
                
                cargarUsuarios();
                cancelarEdicion();
            }
        } catch (error) {
            console.error('Error al actualizar usuario:', error);
        }
    };
}

function cancelarEdicion() {
    document.getElementById('nombre-usuario').value = '';
    document.getElementById('correo-usuario').value = '';
    selectRolUsuario.value = '';
    document.getElementById('btn-submit-usuario').textContent = 'Agregar Usuario';
    document.getElementById('btn-cancelar').style.display = 'none';
    location.reload();
}

async function eliminarUsuario(id) {
    if (!confirm('¿Segura que deseas eliminar este usuario?')) return;

    try {
        const respuesta = await fetch(`${API_USUARIOS_URL}/${id}`, { method: 'DELETE' });
        if (respuesta.ok) {
            cargarUsuarios();
        }
    } catch (error) {
        console.error('Error al eliminar usuario:', error);
    }
}

cargarRolesParaSelect(); 
cargarUsuarios(); 

/*CONTROLADOR DE NAVEGACIÓN POR VISTAS (SPA) CON HASH */
function mostrarSeccion(idSeccion) {
    window.location.hash = idSeccion;

    const secciones = document.querySelectorAll('.seccion-vista');
    secciones.forEach(seccion => {
        seccion.classList.remove('activa');
    });

    const seccionHome = document.getElementById('home');

    if (idSeccion === 'home') {
        if (seccionHome) seccionHome.style.display = 'block';
    } else {
        if (seccionHome) seccionHome.style.display = 'none';
        const seccionAMostrar = document.getElementById(idSeccion);
        if (seccionAMostrar) {
            seccionAMostrar.classList.add('activa');
        }
    }
}

document.addEventListener("DOMContentLoaded", () => {
    let seccionGuardada = window.location.hash.substring(1);
    
    if (!seccionGuardada) {
        seccionGuardada = 'home';
    }

    mostrarSeccion(seccionGuardada);
});