namespace Escritorio;

// Secciones solo para Administrador: Estados, Roles y Usuarios (igual que en la web).

/// <summary>Maestro de Estados: formulario + grid (ID | Nombre del Estado | Acciones).</summary>
public class VistaEstados : Pagina
{
    private readonly FormPrincipal _app;
    private readonly CajaTexto _nombre = new("Nombre del nuevo estado (ej. En Revisión)...", Color.White);
    private readonly CheckBox _esFinal = Estilo.Casilla("¿Este estado cierra la tarea? (por ejemplo Terminada o Cancelada)");
    private readonly FormularioSimple _formulario;
    private readonly Grid<Estado> _grid;
    private int? _idEnEdicion;

    public VistaEstados(FormPrincipal app)
    {
        _app = app;
        Agregar(new TituloSeccion("Estados", "Administración de los estados disponibles para las tareas."), 25);

        _formulario = new FormularioSimple("Agregar Estado", _nombre, _esFinal);
        _formulario.BtnGuardar.Click += async (_, _) => await GuardarAsync();
        _formulario.BtnCancelar.Click += (_, _) => Cancelar();
        Agregar(_formulario, 30, anchoMaximo: 700);

        var columnas = new List<ColumnaGrid<Estado>>
        {
            new("ID", 0, 180, (g, r, e) => Celdas.Texto(g, r, $"#{e.Id}", fuente: Estilo.Negrita, color: Estilo.Titulo)),
            new("Nombre del Estado", 2f, 0, (g, r, e) =>
                Celdas.TextoConEtiquetas(g, r, e.Nombre, e.EsFinal ? new[] { ("Cierra la tarea", Estilo.EtiquetaFinal) } : null))
        };
        _grid = new Grid<Estado>(columnas, _ => new[]
        {
            new AccionGrid<Estado>("Editar", Estilo.Naranja, Estilo.NaranjaHover, PrepararEdicion),
            new AccionGrid<Estado>("Eliminar", Estilo.Rojo, Estilo.RojoHover, Eliminar)
        }, 200, "No hay estados registrados.");
        Agregar(_grid, 10);
    }

    public void Refrescar() => _grid.MostrarDatos(_app.Estados);

    private void PrepararEdicion(Estado estado)
    {
        _idEnEdicion = estado.Id;
        _nombre.Caja.Text = estado.Nombre;
        _esFinal.Checked = estado.EsFinal;
        _formulario.ModoEdicion(true, "Actualizar Estado");
        IrArriba();
        _nombre.Caja.Focus();
    }

    private void Cancelar()
    {
        _idEnEdicion = null;
        _nombre.Caja.Text = "";
        _esFinal.Checked = false;
        _formulario.ModoEdicion(false, "");
    }

    private async Task GuardarAsync()
    {
        string nombre = _nombre.Caja.Text.Trim();
        if (nombre.Length == 0)
        {
            _formulario.MostrarError("Escribe el nombre del estado.");
            return;
        }
        int? id = _idEnEdicion;
        bool esFinal = _esFinal.Checked;
        await _app.EjecutarAsync(async () =>
        {
            await _app.Api.GuardarEstadoAsync(id, nombre, esFinal);
            Cancelar();
            await _app.RecargarTodoAsync();
        });
    }

    private void Eliminar(Estado estado)
    {
        if (!_app.Confirmar($"¿Segura que deseas eliminar el estado \"{estado.Nombre}\"?")) return;
        _ = _app.EjecutarAsync(async () =>
        {
            await _app.Api.EliminarEstadoAsync(estado.Id);
            await _app.RecargarTodoAsync();
        });
    }
}

/// <summary>Maestro de Roles: formulario + grid (ID | Rol y Permisos | Acciones).</summary>
public class VistaRoles : Pagina
{
    private readonly FormPrincipal _app;
    private readonly CajaTexto _nombre = new("Nombre del rol (ej. Auditor, Cliente)...", Color.White);
    private readonly CheckBox _esAdmin = Estilo.Casilla("¿Este rol tiene permisos de Administrador?");
    private readonly CheckBox _gestionaTareas = Estilo.Casilla("¿Este rol puede crear y editar tareas? (por ejemplo Líder de Proyecto)");
    private readonly FormularioSimple _formulario;
    private readonly Grid<Rol> _grid;
    private int? _idEnEdicion;

    public VistaRoles(FormPrincipal app)
    {
        _app = app;
        Agregar(new TituloSeccion("Roles", "Administración de niveles de acceso y permisos del sistema."), 25);

        _formulario = new FormularioSimple("Agregar Rol", _nombre, _esAdmin, _gestionaTareas);
        _formulario.BtnGuardar.Click += async (_, _) => await GuardarAsync();
        _formulario.BtnCancelar.Click += (_, _) => Cancelar();
        Agregar(_formulario, 30, anchoMaximo: 700);

        var columnas = new List<ColumnaGrid<Rol>>
        {
            new("ID", 0, 180, (g, r, rol) => Celdas.Texto(g, r, $"#{rol.Id}", fuente: Estilo.Negrita, color: Estilo.Titulo)),
            new("Rol y Permisos", 2f, 0, (g, r, rol) =>
            {
                var etiquetas = new List<(string, (Color, Color, Color))>();
                if (rol.EsAdmin) etiquetas.Add(("Administrador", Estilo.EtiquetaAdmin));
                if (rol.GestionaTareas) etiquetas.Add(("Gestiona tareas", Estilo.EtiquetaFinal));
                Celdas.TextoConEtiquetas(g, r, rol.Nombre, etiquetas);
            })
        };
        _grid = new Grid<Rol>(columnas, _ => new[]
        {
            new AccionGrid<Rol>("Editar", Estilo.Naranja, Estilo.NaranjaHover, PrepararEdicion),
            new AccionGrid<Rol>("Eliminar", Estilo.Rojo, Estilo.RojoHover, Eliminar)
        }, 200, "No hay roles registrados.");
        Agregar(_grid, 10);
    }

    public void Refrescar() => _grid.MostrarDatos(_app.Roles);

    private void PrepararEdicion(Rol rol)
    {
        _idEnEdicion = rol.Id;
        _nombre.Caja.Text = rol.Nombre;
        _esAdmin.Checked = rol.EsAdmin;
        _gestionaTareas.Checked = rol.GestionaTareas;
        _formulario.ModoEdicion(true, "Actualizar Rol");
        IrArriba();
        _nombre.Caja.Focus();
    }

    private void Cancelar()
    {
        _idEnEdicion = null;
        _nombre.Caja.Text = "";
        _esAdmin.Checked = false;
        _gestionaTareas.Checked = false;
        _formulario.ModoEdicion(false, "");
    }

    private async Task GuardarAsync()
    {
        string nombre = _nombre.Caja.Text.Trim();
        if (nombre.Length == 0)
        {
            _formulario.MostrarError("Escribe el nombre del rol.");
            return;
        }
        int? id = _idEnEdicion;
        bool esAdmin = _esAdmin.Checked;
        bool gestiona = _gestionaTareas.Checked;
        await _app.EjecutarAsync(async () =>
        {
            await _app.Api.GuardarRolAsync(id, nombre, esAdmin, gestiona);
            Cancelar();
            await _app.RecargarTodoAsync();
        });
    }

    private void Eliminar(Rol rol)
    {
        if (!_app.Confirmar($"¿Segura que deseas eliminar el rol \"{rol.Nombre}\"?")) return;
        _ = _app.EjecutarAsync(async () =>
        {
            await _app.Api.EliminarRolAsync(rol.Id);
            await _app.RecargarTodoAsync();
        });
    }
}

/// <summary>
/// Gestión de Usuarios: formulario + "carpetas" por rol (Rol | Personal Registrado | Ver usuarios →).
/// Al abrir un rol se ve la lista de sus usuarios con sus tareas (usuarios-detalle en la web).
/// </summary>
public class VistaUsuarios : Pagina
{
    private readonly FormPrincipal _app;
    private readonly CajaTexto _nombre = new("Nombre completo del usuario...", Color.White);
    private readonly CajaTexto _correo = new("Correo electrónico...", Color.White);
    private readonly CajaTexto _contrasena = new("Contraseña (mínimo 8 caracteres)...", Color.White);
    private readonly ComboBox _rol = Estilo.Lista();
    private readonly FormularioSimple _formulario;
    private readonly Grid<Rol> _carpetas;
    private int? _idEnEdicion;

    public VistaUsuarios(FormPrincipal app)
    {
        _app = app;
        _contrasena.Caja.UseSystemPasswordChar = true;

        Agregar(new TituloSeccion("Gestión de Usuarios", "Administración del personal, asignación de roles y seguimiento de tareas."), 25);

        _formulario = new FormularioSimple("Agregar Usuario", _nombre, _correo, _contrasena, _rol);
        _formulario.BtnGuardar.Click += async (_, _) => await GuardarAsync();
        _formulario.BtnCancelar.Click += (_, _) => Cancelar();
        Agregar(_formulario, 30, anchoMaximo: 700);

        Agregar(new TituloSeccion("Selecciona un rol para ver a su personal:", fuente: Estilo.FuenteH3), 12);

        var columnas = new List<ColumnaGrid<Rol>>
        {
            new("Rol", 0, 180, (g, r, rol) => Celdas.Texto(g, r, rol.Nombre, fuente: Estilo.Negrita, color: Estilo.Titulo)),
            new("Personal Registrado", 2f, 0, (g, r, rol) =>
                Celdas.Texto(g, r, $"{_app.Usuarios.Count(u => u.IdRol == rol.Id)} usuario(s)"))
        };
        _carpetas = new Grid<Rol>(columnas, _ => new[]
        {
            new AccionGrid<Rol>("Ver usuarios →", Estilo.Celeste, Estilo.Azul, rol => _app.VerUsuariosDeRol(rol), Estilo.Lateral)
        }, 200, "No hay roles creados. Ve a Roles para crear uno.");
        Agregar(_carpetas, 10);
    }

    public void Refrescar()
    {
        var opciones = new List<Opcion> { new(null, "Selecciona un rol para este usuario...") };
        opciones.AddRange(_app.Roles.Select(r => new Opcion(r.Id, r.Nombre)));
        Estilo.LlenarLista(_rol, opciones);
        _carpetas.MostrarDatos(_app.Roles);
    }

    public void PrepararEdicion(Usuario usuario)
    {
        _idEnEdicion = usuario.Id;
        _nombre.Caja.Text = usuario.Nombre;
        _correo.Caja.Text = usuario.Correo;
        _contrasena.Caja.Text = "";
        _contrasena.Caja.PlaceholderText = "Nueva contraseña (déjala vacía para no cambiarla)...";
        Estilo.SeleccionarOpcion(_rol, usuario.IdRol);
        _formulario.ModoEdicion(true, "Actualizar Usuario");
        IrArriba();
        _nombre.Caja.Focus();
    }

    private void Cancelar()
    {
        _idEnEdicion = null;
        _nombre.Caja.Text = "";
        _correo.Caja.Text = "";
        _contrasena.Caja.Text = "";
        _contrasena.Caja.PlaceholderText = "Contraseña (mínimo 8 caracteres)...";
        Estilo.SeleccionarOpcion(_rol, null);
        _formulario.ModoEdicion(false, "");
    }

    private async Task GuardarAsync()
    {
        string nombre = _nombre.Caja.Text.Trim();
        string correo = _correo.Caja.Text.Trim();
        string contrasena = _contrasena.Caja.Text;
        int? idRol = (_rol.SelectedItem as Opcion)?.Id;

        if (nombre.Length == 0 || correo.Length == 0 || idRol == null)
        {
            _formulario.MostrarError("Completa nombre, correo y rol.");
            return;
        }
        // Al crear, la contraseña es obligatoria; al editar, solo se envía si se quiere cambiar
        if (_idEnEdicion == null && contrasena.Length < 8)
        {
            _formulario.MostrarError("La contraseña debe tener al menos 8 caracteres.");
            return;
        }

        int? id = _idEnEdicion;
        await _app.EjecutarAsync(async () =>
        {
            await _app.Api.GuardarUsuarioAsync(id, nombre, correo, idRol.Value, contrasena.Length > 0 ? contrasena : null);
            Cancelar();
            await _app.RecargarTodoAsync();
        });
    }
}

/// <summary>Usuarios de un rol: Nombre | Información | Tareas Asignadas | Acciones.</summary>
public class VistaUsuariosRol : Pagina
{
    private readonly FormPrincipal _app;
    private readonly TituloSeccion _titulo = new("Usuarios del Rol");
    private readonly Grid<Usuario> _grid;
    private int? _idRol;

    public VistaUsuariosRol(FormPrincipal app)
    {
        _app = app;
        Agregar(_titulo, 15);

        var volver = BotonWeb.Secundario("← Volver a todos los roles", Estilo.FondoContenido);
        volver.Click += (_, _) => app.MostrarSeccion(Seccion.Usuarios);
        Agregar(volver, 20, anchoPropio: true);

        var columnas = new List<ColumnaGrid<Usuario>>
        {
            new("Nombre", 2f, 0, (g, r, u) => Celdas.Texto(g, r, u.Nombre, fuente: Estilo.Negrita, color: Estilo.Titulo)),
            new("Información", 2f, 0, DibujarInformacion),
            new("Tareas Asignadas", 2f, 0, DibujarTareas)
        };
        _grid = new Grid<Usuario>(columnas, _ => new[]
        {
            new AccionGrid<Usuario>("Editar", Estilo.Naranja, Estilo.NaranjaHover, u => _app.EditarUsuario(u)),
            new AccionGrid<Usuario>("Eliminar", Estilo.Rojo, Estilo.RojoHover, u => _app.EliminarUsuario(u))
        }, 200, "No hay usuarios registrados con este rol.",
        altoFila: u => 34 + Math.Max(1, u.Tareas.Count) * 22);
        Agregar(_grid, 10);
    }

    public void Mostrar(Rol rol)
    {
        _idRol = rol.Id;
        _titulo.CambiarTitulo($"Usuarios del Rol: {rol.Nombre}");
        Refrescar();
        IrArriba();
    }

    public void Refrescar()
    {
        if (_idRol == null) return;
        _grid.MostrarDatos(_app.Usuarios.Where(u => u.IdRol == _idRol));
    }

    /// <summary>Correo y, debajo, la etiqueta del rol.</summary>
    private static void DibujarInformacion(Graphics g, Rectangle celda, Usuario usuario)
    {
        int altoCorreo = Estilo.AltoLinea(Estilo.Normal);
        int altoEtiqueta = Estilo.AltoLinea(Estilo.Pequena) + Estilo.E(6);
        int y = celda.Y + (celda.Height - altoCorreo - Estilo.E(6) - altoEtiqueta) / 2;
        Estilo.Escribir(g, usuario.Correo, Estilo.Normal, Estilo.Texto, new Rectangle(celda.X, y, celda.Width, altoCorreo));
        (Color fondo, Color texto, Color borde) = Estilo.EtiquetaRol;
        Estilo.Etiqueta(g, usuario.Rol, fondo, texto, borde, celda.X, y + altoCorreo + Estilo.E(6) + altoEtiqueta / 2);
    }

    /// <summary>Lista de tareas: "• #5 - Título (Estado)".</summary>
    private static void DibujarTareas(Graphics g, Rectangle celda, Usuario usuario)
    {
        int altoLinea = Estilo.E(22);
        if (usuario.Tareas.Count == 0)
        {
            Estilo.Escribir(g, "Sin tareas asignadas", Estilo.Pequena, Estilo.TextoTenue, celda);
            return;
        }

        int y = celda.Y + (celda.Height - usuario.Tareas.Count * altoLinea) / 2;
        foreach (TareaResumen tarea in usuario.Tareas)
        {
            string estado = $"({tarea.Estado})";
            int anchoEstado = Estilo.Medir(estado, Estilo.PequenaNegrita).Width + Estilo.E(4);
            string texto = $"• #{tarea.Id} - {tarea.Titulo} ";
            int anchoTexto = Math.Min(Estilo.Medir(texto, Estilo.Pequena).Width, Math.Max(0, celda.Width - anchoEstado));

            Estilo.Escribir(g, texto, Estilo.Pequena, Estilo.TextoEtiqueta, new Rectangle(celda.X, y, anchoTexto, altoLinea));
            Estilo.Escribir(g, estado, Estilo.PequenaNegrita, Estilo.TextoEtiqueta, new Rectangle(celda.X + anchoTexto, y, anchoEstado, altoLinea));
            y += altoLinea;
        }
    }
}
