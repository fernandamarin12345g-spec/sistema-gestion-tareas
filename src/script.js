
// CONFIGURACIÓN GENERAL
// La dirección del backend se escribe una sola vez. Si cambia (por ejemplo en Docker), solo se cambia aquí.
const API_BASE_URL = 'https://localhost:7019/api';

// Convierte los caracteres especiales (< > & " ') en texto seguro antes de meterlos en innerHTML.
// Así, un título como  L'Oréal  o  <b>hola</b>  se muestra tal cual y no rompe la página (evita XSS).
function escaparHTML(texto) {
    return String(texto ?? '')
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

// Muestra el mensaje de error que devuelve la API ({ mensaje: "..." }).
// Si la respuesta no trae un mensaje legible, muestra uno genérico.
async function mostrarErrorDeRespuesta(respuesta, mensajePorDefecto) {
    let mensaje = mensajePorDefecto;
    try {
        const datos = await respuesta.json();
        if (datos && datos.mensaje) mensaje = datos.mensaje;
    } catch (e) {
        // La respuesta no era JSON: se queda el mensaje por defecto
    }
    alert(mensaje);
}


// MÓDULO: MAESTRO DE TAREAS
const API_TAREAS_URL = `${API_BASE_URL}/Tareas`;
const API_PROYECTOS_URL = `${API_BASE_URL}/Proyectos`;
const contenedorTareas = document.getElementById('contenedor-tareas');
const formCrearTarea = document.getElementById('form-crear-tarea');
const inputTitulo = document.getElementById('titulo-tarea');
const inputDescripcion = document.getElementById('descripcion-tarea');
const selectUsuarioTarea = document.getElementById('usuario-asignado-tarea');
const selectProyectoTarea = document.getElementById('proyecto-tarea');
const selectEstadoTarea = document.getElementById('estado-tarea');
const selectPrioridadTarea = document.getElementById('prioridad-tarea');
const inputFechaLimite = document.getElementById('fecha-limite-tarea');

let miListaDeTareas = [];
let miListaDeProyectos = [];
let idTareaEnEdicion = null;
let filtroEstadoActual = 'Todas';

// Convierte "2026-10-26" en "26/10/2026" y "2026-10-26 17:40:06" en "26/10/2026 17:40"
function formatearFecha(texto) {
    if (!texto) return '—';
    const [fecha, hora] = texto.split(' ');
    const [anio, mes, dia] = fecha.split('-');
    const fechaBonita = `${dia}/${mes}/${anio}`;
    return hora ? `${fechaBonita} ${hora.substring(0, 5)}` : fechaBonita;
}

// Devuelve la clase CSS del color de cada prioridad (Crítica -> prioridad-critica)
function clasePrioridad(prioridad) {
    const nombre = (prioridad || 'Media').toLowerCase().normalize('NFD').replace(/[̀-ͯ]/g, '');
    return `badge-prioridad prioridad-${nombre}`;
}

async function cargarProyectos() {
    try {
        const respuesta = await fetch(API_PROYECTOS_URL);
        if (!respuesta.ok) throw new Error('Falló servidor en proyectos.');
        miListaDeProyectos = await respuesta.json();
    } catch (error) { console.error('Error:', error); }
}

async function cargarUsuariosYEstadosParaTareas() {
    if (!selectUsuarioTarea || !selectEstadoTarea) return;

    // Selects del formulario
    selectUsuarioTarea.innerHTML = '<option value="">Sin responsable</option>';
    miListaDeUsuarios.forEach(u => { selectUsuarioTarea.innerHTML += `<option value="${u.id_usuario}">${escaparHTML(u.nombre)}</option>`; });

    selectProyectoTarea.innerHTML = '<option value="">Sin proyecto</option>';
    miListaDeProyectos.forEach(p => { selectProyectoTarea.innerHTML += `<option value="${p.id_proyecto}">${escaparHTML(p.nombre)}</option>`; });

    selectEstadoTarea.innerHTML = '<option value="">Selecciona el estado...</option>';
    miListaDeEstados.forEach(e => { selectEstadoTarea.innerHTML += `<option value="${e.id_estado}">${escaparHTML(e.nombre_estado)}</option>`; });

    // Selects de la barra de filtros (se conserva lo que estaba seleccionado)
    const filtroResponsable = document.getElementById('filtro-responsable');
    const valorResponsable = filtroResponsable.value;
    filtroResponsable.innerHTML = '<option value="">Todos los responsables</option><option value="sin">Sin responsable</option>';
    miListaDeUsuarios.forEach(u => { filtroResponsable.innerHTML += `<option value="${u.id_usuario}">${escaparHTML(u.nombre)}</option>`; });
    filtroResponsable.value = valorResponsable;

    const filtroProyecto = document.getElementById('filtro-proyecto');
    const valorProyecto = filtroProyecto.value;
    filtroProyecto.innerHTML = '<option value="">Todos los proyectos</option><option value="sin">Sin proyecto</option>';
    miListaDeProyectos.forEach(p => { filtroProyecto.innerHTML += `<option value="${p.id_proyecto}">${escaparHTML(p.nombre)}</option>`; });
    filtroProyecto.value = valorProyecto;
}

async function cargarTareas() {
    try {
        contenedorTareas.innerHTML = '<p>Conectando con la base de datos...</p>';
        const respuesta = await fetch(API_TAREAS_URL);
        if (!respuesta.ok) throw new Error('Falló la respuesta del servidor.');

        miListaDeTareas = await respuesta.json();
        aplicarFiltros();
    } catch (error) {
        console.error('Error de conexión:', error);
        contenedorTareas.innerHTML = '<p style="color: red;">Error: No pude conectar con el Backend.</p>';
    }
}

// Aplica TODOS los filtros a la vez: estado (botones) + responsable + proyecto + prioridad + atrasadas
function aplicarFiltros() {
    const responsable = document.getElementById('filtro-responsable').value;
    const proyecto = document.getElementById('filtro-proyecto').value;
    const prioridad = document.getElementById('filtro-prioridad').value;
    const soloAtrasadas = document.getElementById('filtro-atrasadas').checked;

    const tareasFiltradas = miListaDeTareas.filter(t => {
        if (filtroEstadoActual !== 'Todas' && t.estado !== filtroEstadoActual) return false;
        if (responsable === 'sin' && t.id_usuario !== null) return false;
        if (responsable !== '' && responsable !== 'sin' && t.id_usuario !== parseInt(responsable)) return false;
        if (proyecto === 'sin' && t.id_proyecto !== null) return false;
        if (proyecto !== '' && proyecto !== 'sin' && t.id_proyecto !== parseInt(proyecto)) return false;
        if (prioridad !== '' && t.prioridad !== prioridad) return false;
        if (soloAtrasadas && !t.atrasada) return false;
        return true;
    });

    renderizarTareas(tareasFiltradas);
}

function renderizarTareas(tareas) {
    contenedorTareas.innerHTML = '';
    contenedorTareas.className = 'contenedor-datos-grid'; // Aplicamos el Data Grid

    if (tareas.length === 0) {
        contenedorTareas.innerHTML = '<div style="padding: 20px;">No hay tareas para mostrar con estos filtros.</div>';
        return;
    }

    // Dibujamos el encabezado de las columnas
    const header = document.createElement('div');
    header.className = 'fila-grid-tareas encabezado-grid';
    header.innerHTML = `<div>ID</div><div>Título</div><div>Responsable</div><div>Prioridad</div><div>Fecha límite</div><div>Estado</div><div style="text-align:right;">Acciones</div>`;
    contenedorTareas.appendChild(header);

    // Dibujamos las filas de datos
    tareas.forEach(tarea => {
        const fila = document.createElement('div');
        fila.className = 'fila-grid-tareas' + (tarea.atrasada ? ' fila-atrasada' : '');
        const responsable = tarea.usuario_asignado ? tarea.usuario_asignado : 'Sin asignar';
        const avisoAtrasada = tarea.atrasada ? '<br><span class="texto-atrasada">Atrasada</span>' : '';

        // Los botones solo envían el ID; los demás datos se buscan en la lista (así un apóstrofe no rompe el botón)
        fila.innerHTML = `
            <div><strong>#${tarea.id}</strong></div>
            <div>${escaparHTML(tarea.titulo)}<br><small class="texto-secundario">${escaparHTML(tarea.proyecto)}</small></div>
            <div>${escaparHTML(responsable)}</div>
            <div><span class="${clasePrioridad(tarea.prioridad)}">${escaparHTML(tarea.prioridad)}</span></div>
            <div>${formatearFecha(tarea.fecha_limite)}${avisoAtrasada}</div>
            <div><span class="badge-estado">${escaparHTML(tarea.estado)}</span></div>
            <div class="acciones-grid">
                <button class="btn-ver" onclick="verDetalleTarea(${tarea.id})">Ver</button>
                <button class="btn-editar" onclick="prepararEdicionTarea(${tarea.id})">Editar</button>
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
        const idEstado = parseInt(selectEstadoTarea.value);

        if (tituloTexto === '' || isNaN(idEstado)) return;

        // Los campos opcionales que vienen vacíos se envían como null
        const datosTarea = {
            titulo: tituloTexto,
            descripcion: inputDescripcion.value.trim() || null,
            IdUsuarioResponsable: selectUsuarioTarea.value ? parseInt(selectUsuarioTarea.value) : null,
            id_estado: idEstado,
            id_proyecto: selectProyectoTarea.value ? parseInt(selectProyectoTarea.value) : null,
            prioridad: selectPrioridadTarea.value,
            fecha_limite: inputFechaLimite.value || null   // el input type="date" ya entrega AAAA-MM-DD
        };

        try {
            let respuesta;
            if (idTareaEnEdicion === null) {
                respuesta = await fetch(API_TAREAS_URL, {
                    method: 'POST', headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(datosTarea)
                });
            } else {
                respuesta = await fetch(`${API_TAREAS_URL}/${idTareaEnEdicion}`, {
                    method: 'PUT', headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(datosTarea)
                });
            }

            if (respuesta.ok) {
                cancelarEdicionTarea();
                cargarTareas();
                if (typeof cargarUsuarios === 'function') cargarUsuarios();
            } else {
                await mostrarErrorDeRespuesta(respuesta, 'No se pudo guardar la tarea.');
            }
        } catch (error) {
            console.error('Error al enviar los datos:', error);
            alert('No se pudo conectar con el servidor.');
        }
    });
}

function prepararEdicionTarea(id) {
    const tarea = miListaDeTareas.find(t => t.id === id);
    if (!tarea) return;

    mostrarSeccion('tareas');
    window.scrollTo({ top: 0, behavior: 'smooth' });
    document.querySelector('main').scrollTo({ top: 0, behavior: 'smooth' });

    idTareaEnEdicion = id;
    inputTitulo.value = tarea.titulo;
    inputDescripcion.value = tarea.descripcion ?? '';
    selectUsuarioTarea.value = tarea.id_usuario ?? '';
    selectProyectoTarea.value = tarea.id_proyecto ?? '';
    selectEstadoTarea.value = tarea.id_estado ?? '';
    selectPrioridadTarea.value = tarea.prioridad || 'Media';
    inputFechaLimite.value = tarea.fecha_limite ?? '';
    document.getElementById('btn-submit-tarea').textContent = 'Actualizar Tarea';
    document.getElementById('btn-cancelar-tarea').classList.add('visible');
}

function cancelarEdicionTarea() {
    idTareaEnEdicion = null;
    inputTitulo.value = ''; inputDescripcion.value = '';
    selectUsuarioTarea.value = ''; selectProyectoTarea.value = ''; selectEstadoTarea.value = '';
    selectPrioridadTarea.value = 'Media'; inputFechaLimite.value = '';
    document.getElementById('btn-submit-tarea').textContent = 'Crear Tarea';
    document.getElementById('btn-cancelar-tarea').classList.remove('visible');
}

// Consulta el detalle de una tarea a la API (GET /api/Tareas/{id}) y lo muestra en su propia vista
async function verDetalleTarea(id) {
    const contenedor = document.getElementById('contenedor-detalle-tarea');
    contenedor.innerHTML = '<p>Cargando tarea...</p>';
    mostrarSeccion('tarea-detalle');

    try {
        const respuesta = await fetch(`${API_TAREAS_URL}/${id}`);
        if (!respuesta.ok) {
            await mostrarErrorDeRespuesta(respuesta, 'No se pudo cargar la tarea.');
            mostrarSeccion('tareas');
            return;
        }
        const tarea = await respuesta.json();
        const avisoAtrasada = tarea.atrasada
            ? '<div class="aviso-atrasada">Esta tarea está atrasada: su fecha límite ya pasó y todavía no está cerrada.</div>'
            : '';

        contenedor.innerHTML = `
            ${avisoAtrasada}
            <h2>#${tarea.id} · ${escaparHTML(tarea.titulo)}</h2>
            <p class="detalle-descripcion">${tarea.descripcion ? escaparHTML(tarea.descripcion) : '<em>Sin descripción</em>'}</p>

            <div class="detalle-datos">
                <div><span>Estado</span><strong><span class="badge-estado">${escaparHTML(tarea.estado)}</span></strong></div>
                <div><span>Prioridad</span><strong><span class="${clasePrioridad(tarea.prioridad)}">${escaparHTML(tarea.prioridad)}</span></strong></div>
                <div><span>Responsable</span><strong>${escaparHTML(tarea.usuario_asignado)}</strong></div>
                <div><span>Proyecto</span><strong>${escaparHTML(tarea.proyecto)}</strong></div>
                <div><span>Fecha de creación</span><strong>${formatearFecha(tarea.fecha_creacion)}</strong></div>
                <div><span>Fecha límite</span><strong>${formatearFecha(tarea.fecha_limite)}</strong></div>
                <div><span>Fecha de finalización</span><strong>${formatearFecha(tarea.fecha_finalizacion)}</strong></div>
                <div><span>Última actualización</span><strong>${formatearFecha(tarea.fecha_actualizacion)}</strong></div>
            </div>

            <div class="contenedor-botones-formulario">
                <button class="btn-editar" onclick="prepararEdicionTarea(${tarea.id})">Editar</button>
                <button class="btn-eliminar" onclick="eliminarTarea(${tarea.id})">Eliminar</button>
            </div>
        `;
    } catch (error) {
        console.error('Error al cargar el detalle:', error);
        alert('No se pudo conectar con el servidor.');
        mostrarSeccion('tareas');
    }
}

async function eliminarTarea(id) {
    if (!confirm('¿Segura que deseas eliminar esta tarea de la base de datos?')) return;
    try {
        const respuesta = await fetch(`${API_TAREAS_URL}/${id}`, { method: 'DELETE' });
        if (respuesta.ok) {
            if (window.location.hash === '#tarea-detalle') mostrarSeccion('tareas');
            cargarTareas();
            if (typeof cargarUsuarios === 'function') cargarUsuarios();
        } else {
            await mostrarErrorDeRespuesta(respuesta, 'No se pudo eliminar la tarea.');
        }
    } catch (error) {
        console.error('Error al eliminar:', error);
        alert('No se pudo conectar con el servidor.');
    }
}

// Filtro por estado (botones): guarda el estado elegido y vuelve a aplicar todos los filtros
function filtrarVista(criterio) {
    filtroEstadoActual = criterio;
    document.querySelectorAll('.btn-filtro').forEach(btn => {
        btn.classList.toggle('btn-filtro-activo', btn.dataset.estado === criterio);
    });
    aplicarFiltros();
}

function renderizarFiltrosTareas() {
    const contenedorFiltros = document.getElementById('controles-filtro');
    if (!contenedorFiltros) return;
    contenedorFiltros.innerHTML = '';

    const nombres = ['Todas', ...miListaDeEstados.map(e => e.nombre_estado)];
    nombres.forEach(nombre => {
        const btn = document.createElement('button');
        btn.className = 'btn-filtro' + (nombre === 'Todas' ? ' btn-filtro-todas' : '');
        if (nombre === filtroEstadoActual) btn.classList.add('btn-filtro-activo');
        btn.dataset.estado = nombre;
        btn.textContent = nombre;
        btn.onclick = () => filtrarVista(nombre);
        contenedorFiltros.appendChild(btn);
    });
}


// MÓDULO: MAESTRO DE ESTADOS
const API_ESTADOS_URL = `${API_BASE_URL}/Estados`;
const contenedorEstados = document.getElementById('contenedor-estados');
const formCrearEstado = document.getElementById('form-crear-estado');
const inputNombreEstado = document.getElementById('nombre-estado');
const checkboxEsFinal = document.getElementById('es-final-estado');
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
            <div>${escaparHTML(estado.nombre_estado)} ${estado.es_final ? '<span class="etiqueta-final">Cierra la tarea</span>' : ''}</div>
            <div class="acciones-grid">
                <button class="btn-editar" onclick="prepararEdicionEstado(${estado.id_estado})">Editar</button>
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
                respuesta = await fetch(API_ESTADOS_URL, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ nombre_estado: nombreTexto, es_final: checkboxEsFinal.checked }) });
            } else {
                respuesta = await fetch(`${API_ESTADOS_URL}/${idEstadoEnEdicion}`, { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ nombre_estado: nombreTexto, es_final: checkboxEsFinal.checked }) });
            }

            if (respuesta.ok) { cancelarEdicionEstado(); inicializarSistema(); }
            else { await mostrarErrorDeRespuesta(respuesta, 'No se pudo guardar el estado.'); cargarEstados(); }
        } catch (error) {
            console.error('Error:', error);
            alert('No se pudo conectar con el servidor.');
        }
    });
}

function prepararEdicionEstado(id) {
    const estado = miListaDeEstados.find(e => e.id_estado === id);
    if (!estado) return;

    window.scrollTo({ top: 0, behavior: 'smooth' });
    idEstadoEnEdicion = id; inputNombreEstado.value = estado.nombre_estado; checkboxEsFinal.checked = estado.es_final;
    document.getElementById('btn-submit-estado').textContent = 'Actualizar Estado';
    document.getElementById('btn-cancelar-estado').classList.add('visible');
}

function cancelarEdicionEstado() {
    idEstadoEnEdicion = null; inputNombreEstado.value = ''; checkboxEsFinal.checked = false;
    document.getElementById('btn-submit-estado').textContent = 'Agregar Estado';
    document.getElementById('btn-cancelar-estado').classList.remove('visible');
}

async function eliminarEstado(id) {
    if (!confirm('¿Segura que deseas eliminar este estado?')) return;
    try {
        const respuesta = await fetch(`${API_ESTADOS_URL}/${id}`, { method: 'DELETE' });
        if (respuesta.ok) { inicializarSistema(); }
        else { await mostrarErrorDeRespuesta(respuesta, 'No se pudo eliminar el estado.'); }
    } catch (error) {
        console.error('Error:', error);
        alert('No se pudo conectar con el servidor.');
    }
}


// MÓDULO: MAESTRO DE ROLES
const API_ROLES_URL = `${API_BASE_URL}/Roles`;
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
            <div>${escaparHTML(rol.nombre_rol)} ${etiquetaAdmin}</div>
            <div class="acciones-grid">
                <button class="btn-editar" onclick="prepararEdicionRol(${rol.id_rol})">Editar</button>
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
            else { await mostrarErrorDeRespuesta(respuesta, 'No se pudo guardar el rol.'); cargarRoles(); }
        } catch (error) {
            console.error('Error al procesar rol:', error);
            alert('No se pudo conectar con el servidor.');
        }
    });
}

function prepararEdicionRol(id) {
    const rol = miListaDeRoles.find(r => r.id_rol === id);
    if (!rol) return;

    window.scrollTo({ top: 0, behavior: 'smooth' });
    idRolEnEdicion = id; inputNombreRol.value = rol.nombre_rol; checkboxEsAdmin.checked = rol.is_admin;
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
        else { await mostrarErrorDeRespuesta(respuesta, 'No se pudo eliminar el rol.'); }
    } catch (error) {
        console.error('Error al eliminar rol:', error);
        alert('No se pudo conectar con el servidor.');
    }
}


// MÓDULO: MAESTRO DE USUARIOS
const API_USUARIOS_URL = `${API_BASE_URL}/Usuarios`;
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
            roles.forEach(rol => { selectRolUsuario.innerHTML += `<option value="${rol.id_rol}">${escaparHTML(rol.nombre_rol)}</option>`; });
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
            <div><strong>${escaparHTML(rol.nombre_rol)}</strong></div>
            <div>${usuariosDelRol.length} usuario(s)</div>
            <div class="acciones-grid">
                <button class="btn-editar" style="background-color:#38bdf8; color:#0f172a;" onclick="abrirDetalleRol(${rol.id_rol})">Ver usuarios &rarr;</button>
            </div>
        `;
        contenedorCarpetas.appendChild(fila);
    });
}

function abrirDetalleRol(idRol) {
    const rol = miListaDeRoles.find(r => r.id_rol === idRol);
    const nombreRol = rol ? rol.nombre_rol : '';
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
            usuario.tareas.forEach(tarea => { htmlTareas += `<li>#${tarea.id_tarea} - ${escaparHTML(tarea.titulo)} <strong>(${escaparHTML(tarea.estado)})</strong></li>`; });
            htmlTareas += '</ul>';
        } else {
            htmlTareas = '<span style="color:#94a3b8; font-size:0.85rem;">Sin tareas asignadas</span>';
        }

        const fila = document.createElement('div');
        fila.className = 'fila-grid-usuarios';

        fila.innerHTML = `
            <div><strong>${escaparHTML(usuario.nombre)}</strong></div>
            <div>
                ${escaparHTML(usuario.correo)}<br>
                <span class="etiqueta-rol" style="margin-top:5px;">${escaparHTML(usuario.nombre_rol)}</span>
            </div>
            <div>${htmlTareas}</div>
            <div class="acciones-grid">
                <button class="btn-editar" onclick="prepararEdicionUsuario(${usuario.id_usuario})">Editar</button>
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
            else { await mostrarErrorDeRespuesta(respuesta, 'No se pudo guardar el usuario.'); cargarUsuarios(); }
        } catch (error) {
            console.error('Error:', error);
            alert('No se pudo conectar con el servidor.');
        }
    });
}

function prepararEdicionUsuario(id) {
    const usuario = miListaDeUsuarios.find(u => u.id_usuario === id);
    if (!usuario) return;

    mostrarSeccion('usuarios'); window.scrollTo({ top: 0, behavior: 'smooth' });
    idUsuarioEnEdicion = id; document.getElementById('nombre-usuario').value = usuario.nombre;
    document.getElementById('correo-usuario').value = usuario.correo; selectRolUsuario.value = usuario.id_rol ?? '';
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
        if (respuesta.ok) { mostrarSeccion('usuarios'); cargarUsuarios(); cargarTareas(); }
        else { await mostrarErrorDeRespuesta(respuesta, 'No se pudo eliminar el usuario.'); }
    } catch (error) {
        console.error('Error:', error);
        alert('No se pudo conectar con el servidor.');
    }
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
    // Las vistas de detalle necesitan saber qué mostrar; al recargar la página se vuelve a la lista
    if (seccionGuardada === 'tarea-detalle') seccionGuardada = 'tareas';
    if (seccionGuardada === 'usuarios-detalle') seccionGuardada = 'usuarios';
    mostrarSeccion(seccionGuardada);
});

// GESTIÓN DE CARGAS ESTRICTAMENTE ORDENADAS
async function inicializarSistema() {
    await cargarEstados();
    await cargarRoles();
    cargarRolesParaSelect();
    await cargarUsuarios();
    await cargarProyectos();
    await cargarUsuariosYEstadosParaTareas();
    cargarTareas();
}
inicializarSistema();
