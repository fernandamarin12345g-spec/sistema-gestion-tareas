
// MÓDULO: MAESTRO DE TAREAS
const API_TAREAS_URL = 'https://localhost:7019/api/Tareas';
const contenedorTareas = document.getElementById('contenedor-tareas');
const formCrearTarea = document.getElementById('form-crear-tarea');
const inputTitulo = document.getElementById('titulo-tarea');
const selectUsuarioTarea = document.getElementById('usuario-asignado-tarea');
const selectEstadoTarea = document.getElementById('estado-tarea');

let miListaDeTareas = [];
let idTareaEnEdicion = null;

async function cargarUsuariosYEstadosParaTareas() {
    if (!selectUsuarioTarea || !selectEstadoTarea) return;
    
    selectUsuarioTarea.innerHTML = '<option value="">Selecciona un usuario responsable...</option>';
    miListaDeUsuarios.forEach(u => { selectUsuarioTarea.innerHTML += `<option value="${u.id_usuario}">${u.nombre}</option>`; });

    selectEstadoTarea.innerHTML = '<option value="">Selecciona el estado...</option>';
    miListaDeEstados.forEach(e => { selectEstadoTarea.innerHTML += `<option value="${e.id_estado}">${e.nombre_estado}</option>`; });
}

async function cargarTareas() {
    try {
        contenedorTareas.innerHTML = '<p>Conectando con la base de datos...</p>';
        const respuesta = await fetch(API_TAREAS_URL);
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
    contenedorTareas.className = 'contenedor-datos-grid'; // Aplicamos el Data Grid

    if (tareas.length === 0) {
        contenedorTareas.innerHTML = '<div style="padding: 20px;">No hay tareas para mostrar en este filtro.</div>';
        return;
    }

    // Dibujamos el encabezado de las columnas
    const header = document.createElement('div');
    header.className = 'fila-grid-tareas encabezado-grid';
    header.innerHTML = `<div>ID</div><div>Título</div><div>Responsable</div><div>Estado</div><div style="text-align:right;">Acciones</div>`;
    contenedorTareas.appendChild(header);

    // Dibujamos las filas de datos
    tareas.forEach(tarea => {
        const fila = document.createElement('div');
        fila.className = 'fila-grid-tareas'; 
        const responsable = tarea.usuario_asignado ? tarea.usuario_asignado : 'Sin asignar';

        fila.innerHTML = `
            <div><strong>#${tarea.id}</strong></div>
            <div>${tarea.titulo}</div>
            <div>${responsable}</div>
            <div><span class="badge-estado">${tarea.estado}</span></div>
            <div class="acciones-grid">
                <button class="btn-editar" onclick="prepararEdicionTarea(${tarea.id}, '${tarea.titulo}', ${tarea.id_estado}, ${tarea.id_usuario})">Editar</button>
                <button class="btn-eliminar" onclick="eliminarTarea(${tarea.id})">Eliminar</button>
            </div>
        `;
        contenedorTareas.appendChild(fila);
    });
}

if (formCrearTarea) {
    formCrearTarea.addEventListener('submit', async function(evento) {
        evento.preventDefault(); 
        const tituloTexto = inputTitulo.value.trim();
        const idUsuario = parseInt(selectUsuarioTarea.value);
        const idEstado = parseInt(selectEstadoTarea.value);

        if (tituloTexto === '' || isNaN(idUsuario) || isNaN(idEstado)) return;

        try {
            let respuesta;
            if (idTareaEnEdicion === null) {
                respuesta = await fetch(API_TAREAS_URL, {
                    method: 'POST', headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ titulo: tituloTexto, IdUsuarioResponsable: idUsuario, id_estado: idEstado })
                });
            } else {
                respuesta = await fetch(`${API_TAREAS_URL}/${idTareaEnEdicion}`, {
                    method: 'PUT', headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ titulo: tituloTexto, IdUsuarioResponsable: idUsuario, id_estado: idEstado })
                });
            }

            if (respuesta.ok) {
                cancelarEdicionTarea();
                cargarTareas(); 
                if (typeof cargarUsuarios === 'function') cargarUsuarios(); 
            }
        } catch (error) { console.error('Error al enviar los datos:', error); }
    });
}

function prepararEdicionTarea(id, tituloActual, idEstadoActual, idUsuarioActual) {
    window.scrollTo({ top: 0, behavior: 'smooth' });
    idTareaEnEdicion = id; 
    inputTitulo.value = tituloActual; selectUsuarioTarea.value = idUsuarioActual; selectEstadoTarea.value = idEstadoActual;
    document.getElementById('btn-submit-tarea').textContent = 'Actualizar Tarea';
    document.getElementById('btn-cancelar-tarea').classList.add('visible');
}

function cancelarEdicionTarea() {
    idTareaEnEdicion = null; 
    inputTitulo.value = ''; selectUsuarioTarea.value = ''; selectEstadoTarea.value = '';
    document.getElementById('btn-submit-tarea').textContent = 'Crear Tarea';
    document.getElementById('btn-cancelar-tarea').classList.remove('visible');
}

async function eliminarTarea(id) {
    if (!confirm('¿Segura que deseas eliminar esta tarea de la base de datos?')) return;
    try {
        const respuesta = await fetch(`${API_TAREAS_URL}/${id}`, { method: 'DELETE' });
        if (respuesta.ok) { cargarTareas(); if (typeof cargarUsuarios === 'function') cargarUsuarios(); }
    } catch (error) { console.error('Error al eliminar:', error); }
}

function filtrarVista(criterio) {
    if (criterio === 'Todas') renderizarTareas(miListaDeTareas); 
    else renderizarTareas(miListaDeTareas.filter(t => t.estado === criterio));
}

function renderizarFiltrosTareas() {
    const contenedorFiltros = document.getElementById('controles-filtro');
    if (!contenedorFiltros) return;
    contenedorFiltros.innerHTML = '<button onclick="filtrarVista(\'Todas\')" class="btn-filtro btn-filtro-todas">Todas</button>';
    miListaDeEstados.forEach(estado => {
        const btn = document.createElement('button');
        btn.className = 'btn-filtro'; 
        btn.textContent = estado.nombre_estado; 
        btn.onclick = () => filtrarVista(estado.nombre_estado); 
        contenedorFiltros.appendChild(btn);
    });
}


// MÓDULO: MAESTRO DE ESTADOS
const API_ESTADOS_URL = 'https://localhost:7019/api/Estados';
const contenedorEstados = document.getElementById('contenedor-estados');
const formCrearEstado = document.getElementById('form-crear-estado');
const inputNombreEstado = document.getElementById('nombre-estado');
let miListaDeEstados = [];
let idEstadoEnEdicion = null; 

async function cargarEstados() {
    try {
        const respuesta = await fetch(API_ESTADOS_URL);
        if (!respuesta.ok) throw new Error('Falló la respuesta del servidor al cargar estados.');
        miListaDeEstados = await respuesta.json();
        renderizarEstados(miListaDeEstados);
        if (typeof renderizarFiltrosTareas === 'function') renderizarFiltrosTareas();
    } catch (error) { console.error('Error:', error); }
}

function renderizarEstados(estados) {
    contenedorEstados.innerHTML = '';
    contenedorEstados.className = 'contenedor-datos-grid';
    
    if (estados.length === 0) {
        contenedorEstados.innerHTML = '<div style="padding: 20px;">No hay estados registrados.</div>';
        return;
    }

    const header = document.createElement('div');
    header.className = 'fila-grid-basica encabezado-grid';
    header.innerHTML = `<div>ID</div><div>Nombre del Estado</div><div style="text-align:right;">Acciones</div>`;
    contenedorEstados.appendChild(header);

    estados.forEach(estado => {
        const fila = document.createElement('div');
        fila.className = 'fila-grid-basica'; 
        fila.innerHTML = `
            <div><strong>#${estado.id_estado}</strong></div>
            <div>${estado.nombre_estado}</div>
            <div class="acciones-grid">
                <button class="btn-editar" onclick="prepararEdicionEstado(${estado.id_estado}, '${estado.nombre_estado}')">Editar</button>
                <button class="btn-eliminar" onclick="eliminarEstado(${estado.id_estado})">Eliminar</button>
            </div>
        `;
        contenedorEstados.appendChild(fila);
    });
}

if (formCrearEstado) {
    formCrearEstado.addEventListener('submit', async function(evento) {
        evento.preventDefault();
        const nombreTexto = inputNombreEstado.value.trim();
        if (nombreTexto === '') return;

        try {
            let respuesta;
            if (idEstadoEnEdicion === null) {
                respuesta = await fetch(API_ESTADOS_URL, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ nombre_estado: nombreTexto }) });
            } else {
                respuesta = await fetch(`${API_ESTADOS_URL}/${idEstadoEnEdicion}`, { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ nombre_estado: nombreTexto }) });
            }

            if (respuesta.ok) { cancelarEdicionEstado(); inicializarSistema(); } 
            else { const err = await respuesta.json(); alert(err.mensaje); cargarEstados(); }
        } catch (error) { console.error('Error:', error); }
    });
}

function prepararEdicionEstado(id, nombreActual) {
    window.scrollTo({ top: 0, behavior: 'smooth' });
    idEstadoEnEdicion = id; inputNombreEstado.value = nombreActual;
    document.getElementById('btn-submit-estado').textContent = 'Actualizar Estado';
    document.getElementById('btn-cancelar-estado').classList.add('visible');
}

function cancelarEdicionEstado() {
    idEstadoEnEdicion = null; inputNombreEstado.value = '';
    document.getElementById('btn-submit-estado').textContent = 'Agregar Estado';
    document.getElementById('btn-cancelar-estado').classList.remove('visible');
}

async function eliminarEstado(id) {
    if (!confirm('¿Segura que deseas eliminar este estado?')) return;
    try {
        const respuesta = await fetch(`${API_ESTADOS_URL}/${id}`, { method: 'DELETE' });
        if (respuesta.ok) { inicializarSistema(); } 
        else { const err = await respuesta.json(); alert(err.mensaje); }
    } catch (error) { console.error('Error:', error); }
}


// MÓDULO: MAESTRO DE ROLES
const API_ROLES_URL = 'https://localhost:7019/api/Roles';
const contenedorRoles = document.getElementById('contenedor-roles');
const formCrearRol = document.getElementById('form-crear-rol');
const inputNombreRol = document.getElementById('nombre-rol');
const checkboxEsAdmin = document.getElementById('es-admin');
let miListaDeRoles = [];
let idRolEnEdicion = null; 

async function cargarRoles() {
    try {
        const respuesta = await fetch(API_ROLES_URL);
        if (!respuesta.ok) throw new Error('Falló servidor en roles.');
        miListaDeRoles = await respuesta.json();
        renderizarRoles(miListaDeRoles);
    } catch (error) { console.error('Error:', error); }
}

function renderizarRoles(roles) {
    if(!contenedorRoles) return;
    contenedorRoles.innerHTML = '';
    contenedorRoles.className = 'contenedor-datos-grid';
    
    if (roles.length === 0) {
        contenedorRoles.innerHTML = '<div style="padding: 20px;">No hay roles registrados.</div>';
        return;
    }

    const header = document.createElement('div');
    header.className = 'fila-grid-basica encabezado-grid';
    header.innerHTML = `<div>ID</div><div>Rol y Permisos</div><div style="text-align:right;">Acciones</div>`;
    contenedorRoles.appendChild(header);

    roles.forEach(rol => {
        const fila = document.createElement('div');
        fila.className = 'fila-grid-basica'; 
        const etiquetaAdmin = rol.is_admin ? '<span class="etiqueta-admin">Administrador</span>' : '';

        fila.innerHTML = `
            <div><strong>#${rol.id_rol}</strong></div>
            <div>${rol.nombre_rol} ${etiquetaAdmin}</div>
            <div class="acciones-grid">
                <button class="btn-editar" onclick="prepararEdicionRol(${rol.id_rol}, '${rol.nombre_rol}', ${rol.is_admin})">Editar</button>
                <button class="btn-eliminar" onclick="eliminarRol(${rol.id_rol})">Eliminar</button>
            </div>
        `;
        contenedorRoles.appendChild(fila);
    });
}

if(formCrearRol) {
    formCrearRol.addEventListener('submit', async function(evento) {
        evento.preventDefault();
        const nombreTexto = inputNombreRol.value.trim();
        const esAdmin = checkboxEsAdmin.checked; 
        if (nombreTexto === '') return;

        try {
            let respuesta;
            if (idRolEnEdicion === null) {
                respuesta = await fetch(API_ROLES_URL, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ nombre_rol: nombreTexto, is_admin: esAdmin }) });
            } else {
                respuesta = await fetch(`${API_ROLES_URL}/${idRolEnEdicion}`, { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ nombre_rol: nombreTexto, is_admin: esAdmin }) });
            }

            if (respuesta.ok) { cancelarEdicionRol(); inicializarSistema(); if (typeof cargarRolesParaSelect === 'function') cargarRolesParaSelect(); } 
            else { const err = await respuesta.json(); alert(err.mensaje); cargarRoles(); }
        } catch (error) { console.error('Error al procesar rol:', error); }
    });
}

function prepararEdicionRol(id, nombreActual, isAdminActual) {
    window.scrollTo({ top: 0, behavior: 'smooth' });
    idRolEnEdicion = id; inputNombreRol.value = nombreActual; checkboxEsAdmin.checked = isAdminActual;
    document.getElementById('btn-submit-rol').textContent = 'Actualizar Rol';
    document.getElementById('btn-cancelar-rol').classList.add('visible');
}

function cancelarEdicionRol() {
    idRolEnEdicion = null; inputNombreRol.value = ''; checkboxEsAdmin.checked = false;
    document.getElementById('btn-submit-rol').textContent = 'Agregar Rol';
    document.getElementById('btn-cancelar-rol').classList.remove('visible');
}

async function eliminarRol(id) {
    if (!confirm('¿Segura que deseas eliminar este rol?')) return;
    try {
        const respuesta = await fetch(`${API_ROLES_URL}/${id}`, { method: 'DELETE' });
        if (respuesta.ok) { inicializarSistema(); if (typeof cargarRolesParaSelect === 'function') cargarRolesParaSelect(); } 
        else { const err = await respuesta.json(); alert(err.mensaje); }
    } catch (error) { console.error('Error al eliminar rol:', error); }
}


// MÓDULO: MAESTRO DE USUARIOS
const API_USUARIOS_URL = 'https://localhost:7019/api/Usuarios';
const contenedorUsuarios = document.getElementById('contenedor-usuarios');
const formCrearUsuario = document.getElementById('form-crear-usuario');
const selectRolUsuario = document.getElementById('rol-usuario');

let miListaDeUsuarios = [];
let idUsuarioEnEdicion = null; 

async function cargarRolesParaSelect() {
    if (!selectRolUsuario) return;
    try {
        const respuesta = await fetch(API_ROLES_URL);
        if (respuesta.ok) {
            const roles = await respuesta.json();
            selectRolUsuario.innerHTML = '<option value="">Selecciona un rol para este usuario...</option>';
            roles.forEach(rol => { selectRolUsuario.innerHTML += `<option value="${rol.id_rol}">${rol.nombre_rol}</option>`; });
        }
    } catch (error) { console.error('Error:', error); }
}

async function cargarUsuarios() {
    try {
        const respuesta = await fetch(API_USUARIOS_URL);
        if (!respuesta.ok) throw new Error('Falló servidor.');
        miListaDeUsuarios = await respuesta.json();
        renderizarCarpetasRoles();
    } catch (error) { console.error('Error:', error); }
}

function renderizarCarpetasRoles() {
    const contenedorCarpetas = document.getElementById('contenedor-carpetas-roles');
    if (!contenedorCarpetas) return;
    contenedorCarpetas.innerHTML = '';
    contenedorCarpetas.className = 'contenedor-datos-grid';

    if (miListaDeRoles.length === 0) {
        contenedorCarpetas.innerHTML = '<div style="padding: 20px;">No hay roles creados. Ve a Roles para crear uno.</div>';
        return;
    }

    const header = document.createElement('div');
    header.className = 'fila-grid-basica encabezado-grid';
    header.innerHTML = `<div>Rol</div><div>Personal Registrado</div><div style="text-align:right;">Acciones</div>`;
    contenedorCarpetas.appendChild(header);

    miListaDeRoles.forEach(rol => {
        const usuariosDelRol = miListaDeUsuarios.filter(u => u.id_rol === rol.id_rol);
        const fila = document.createElement('div');
        fila.className = 'fila-grid-basica';

        fila.innerHTML = `
            <div><strong>${rol.nombre_rol}</strong></div>
            <div>${usuariosDelRol.length} usuario(s)</div>
            <div class="acciones-grid">
                <button class="btn-editar" style="background-color:#38bdf8; color:#0f172a;" onclick="abrirDetalleRol(${rol.id_rol}, '${rol.nombre_rol}')">Ver usuarios &rarr;</button>
            </div>
        `;
        contenedorCarpetas.appendChild(fila);
    });
}

function abrirDetalleRol(idRol, nombreRol) {
    document.getElementById('titulo-rol-seleccionado').textContent = `Usuarios del Rol: ${nombreRol}`;
    const usuariosFiltrados = miListaDeUsuarios.filter(u => u.id_rol === idRol);
    renderizarUsuarios(usuariosFiltrados);
    mostrarSeccion('usuarios-detalle');
}

function renderizarUsuarios(usuarios) {
    if (!contenedorUsuarios) return;
    contenedorUsuarios.innerHTML = '';
    contenedorUsuarios.className = 'contenedor-datos-grid';
    
    if (usuarios.length === 0) {
        contenedorUsuarios.innerHTML = '<div style="padding: 20px;">No hay usuarios registrados con este rol.</div>';
        return;
    }

    const header = document.createElement('div');
    header.className = 'fila-grid-usuarios encabezado-grid';
    header.innerHTML = `<div>Nombre</div><div>Información</div><div>Tareas Asignadas</div><div style="text-align:right;">Acciones</div>`;
    contenedorUsuarios.appendChild(header);

    usuarios.forEach(usuario => {
        let htmlTareas = '';
        if (usuario.tareas && usuario.tareas.length > 0) {
            htmlTareas = '<ul style="margin:0; padding-left:15px; font-size:0.85rem; color:#475569;">';
            usuario.tareas.forEach(tarea => { htmlTareas += `<li>#${tarea.id_tarea} - ${tarea.titulo} <strong>(${tarea.estado})</strong></li>`; });
            htmlTareas += '</ul>';
        } else {
            htmlTareas = '<span style="color:#94a3b8; font-size:0.85rem;">Sin tareas asignadas</span>';
        }

        const fila = document.createElement('div');
        fila.className = 'fila-grid-usuarios';
        
        fila.innerHTML = `
            <div><strong>${usuario.nombre}</strong></div>
            <div>
                ${usuario.correo}<br>
                <span class="etiqueta-rol" style="margin-top:5px;">${usuario.nombre_rol}</span>
            </div>
            <div>${htmlTareas}</div>
            <div class="acciones-grid">
                <button class="btn-editar" onclick="prepararEdicionUsuario(${usuario.id_usuario}, '${usuario.nombre}', '${usuario.correo}', ${usuario.id_rol})">Editar</button>
                <button class="btn-eliminar" onclick="eliminarUsuario(${usuario.id_usuario})">Eliminar</button>
            </div>
        `;
        contenedorUsuarios.appendChild(fila);
    });
}

if (formCrearUsuario) {
    formCrearUsuario.addEventListener('submit', async function(evento) {
        evento.preventDefault();
        const nombreTexto = document.getElementById('nombre-usuario').value.trim();
        const correoTexto = document.getElementById('correo-usuario').value.trim();
        const idRolSeleccionado = parseInt(selectRolUsuario.value);

        if (nombreTexto === '' || correoTexto === '' || isNaN(idRolSeleccionado)) return;

        try {
            let respuesta;
            if (idUsuarioEnEdicion === null) {
                respuesta = await fetch(API_USUARIOS_URL, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ nombre: nombreTexto, correo: correoTexto, id_rol: idRolSeleccionado }) });
            } else {
                respuesta = await fetch(`${API_USUARIOS_URL}/${idUsuarioEnEdicion}`, { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ nombre: nombreTexto, correo: correoTexto, id_rol: idRolSeleccionado }) });
            }

            if (respuesta.ok) { cancelarEdicionUsuario(); cargarUsuarios(); } 
            else { const err = await respuesta.json(); alert(err.mensaje); cargarUsuarios(); }
        } catch (error) { console.error('Error:', error); }
    });
}

function prepararEdicionUsuario(id, nombreActual, correoActual, idRolActual) {
    mostrarSeccion('usuarios'); window.scrollTo({ top: 0, behavior: 'smooth' });
    idUsuarioEnEdicion = id; document.getElementById('nombre-usuario').value = nombreActual;
    document.getElementById('correo-usuario').value = correoActual; selectRolUsuario.value = idRolActual;
    document.getElementById('btn-submit-usuario').textContent = 'Actualizar Usuario';
    document.getElementById('btn-cancelar-usuario').classList.add('visible');
}

function cancelarEdicionUsuario() {
    idUsuarioEnEdicion = null; document.getElementById('nombre-usuario').value = '';
    document.getElementById('correo-usuario').value = ''; selectRolUsuario.value = '';
    document.getElementById('btn-submit-usuario').textContent = 'Agregar Usuario';
    document.getElementById('btn-cancelar-usuario').classList.remove('visible');
}

async function eliminarUsuario(id) {
    if (!confirm('¿Segura que deseas eliminar este usuario?')) return;
    try {
        const respuesta = await fetch(`${API_USUARIOS_URL}/${id}`, { method: 'DELETE' });
        if (respuesta.ok) { mostrarSeccion('usuarios'); cargarUsuarios(); }
    } catch (error) { console.error('Error:', error); }
}


// CONTROLADOR DE NAVEGACIÓN SPA
function mostrarSeccion(idSeccion) {
    window.location.hash = idSeccion;
    document.querySelectorAll('.seccion-vista').forEach(seccion => seccion.classList.remove('activa'));

    const seccionHome = document.getElementById('home');
    if (idSeccion === 'home') {
        if (seccionHome) seccionHome.style.display = 'block';
    } else {
        if (seccionHome) seccionHome.style.display = 'none';
        const seccionAMostrar = document.getElementById(idSeccion);
        if (seccionAMostrar) seccionAMostrar.classList.add('activa');
    }

    // Marca el botón activo en el sidebar del Dashboard
    document.querySelectorAll('nav a').forEach(link => {
        if(link.getAttribute('href') === '#' + idSeccion) link.classList.add('activo');
        else link.classList.remove('activo');
    });
}

document.addEventListener("DOMContentLoaded", () => {
    let seccionGuardada = window.location.hash.substring(1);
    if (!seccionGuardada) seccionGuardada = 'home';
    mostrarSeccion(seccionGuardada);
});

// GESTIÓN DE CARGAS ESTRICTAMENTE ORDENADAS
async function inicializarSistema() {
    await cargarEstados();
    await cargarRoles();
    cargarRolesParaSelect();
    await cargarUsuarios();
    await cargarUsuariosYEstadosParaTareas();
    cargarTareas();
}
inicializarSistema();