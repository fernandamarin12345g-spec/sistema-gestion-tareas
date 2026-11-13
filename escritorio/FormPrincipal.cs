namespace Escritorio;

/// <summary>Secciones de la app (las mismas secciones de la SPA web).</summary>
public enum Seccion
{
    Inicio,
    Tareas,
    Detalle,
    Proyectos,
    Estados,
    Roles,
    Usuarios,
    UsuariosRol
}

/// <summary>
/// Ventana principal con la misma estructura de la web: barra lateral oscura a la izquierda
/// y el contenido a la derecha (grid-template-columns: 250px 1fr).
/// Guarda los datos que vienen de la API y los comparte con todas las secciones.
/// </summary>
public class FormPrincipal : Form
{
    private readonly BarraLateral _lateral;
    private readonly Panel _contenido = new() { Dock = DockStyle.Fill, BackColor = Estilo.FondoContenido };
    private readonly DesplazamientoConRueda _rueda = new();

    private readonly VistaInicio _vistaInicio;
    private readonly VistaTareas _vistaTareas;
    private readonly VistaDetalle _vistaDetalle;
    private readonly VistaProyectos _vistaProyectos;
    private readonly VistaEstados _vistaEstados;
    private readonly VistaRoles _vistaRoles;
    private readonly VistaUsuarios _vistaUsuarios;
    private readonly VistaUsuariosRol _vistaUsuariosRol;
    private readonly Dictionary<Seccion, Pagina> _paginas;

    private List<Tarea> _tareas = new();
    private List<Estado> _estados = new();
    private List<Proyecto> _proyectos = new();
    private List<Usuario> _usuarios = new();
    private List<Rol> _roles = new();
    private Seccion _seccionActual;
    private bool _ocupado;

    public FormPrincipal(ApiCliente api)
    {
        Api = api;
        Usuario = api.Sesion?.Usuario ?? throw new InvalidOperationException("No hay una sesión iniciada.");

        Text = "Sistema de Gestión de Tareas · ESA SAS";
        Font = Estilo.Normal;
        BackColor = Estilo.FondoContenido;
        StartPosition = FormStartPosition.CenterScreen;
        Rectangle pantalla = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1366, 768);
        Size = new Size(Math.Min(Estilo.E(1280), pantalla.Width), Math.Min(Estilo.E(800), pantalla.Height));
        MinimumSize = new Size(Math.Min(Estilo.E(980), pantalla.Width), Math.Min(Estilo.E(600), pantalla.Height));
        WindowState = FormWindowState.Maximized; // Se abre ocupando toda la pantalla, como el navegador

        _lateral = new BarraLateral(Usuario);
        _vistaInicio = new VistaInicio(this);
        _vistaTareas = new VistaTareas(this);
        _vistaDetalle = new VistaDetalle(this);
        _vistaProyectos = new VistaProyectos(this);
        _vistaEstados = new VistaEstados(this);
        _vistaRoles = new VistaRoles(this);
        _vistaUsuarios = new VistaUsuarios(this);
        _vistaUsuariosRol = new VistaUsuariosRol(this);
        _paginas = new Dictionary<Seccion, Pagina>
        {
            [Seccion.Inicio] = _vistaInicio,
            [Seccion.Tareas] = _vistaTareas,
            [Seccion.Detalle] = _vistaDetalle,
            [Seccion.Proyectos] = _vistaProyectos,
            [Seccion.Estados] = _vistaEstados,
            [Seccion.Roles] = _vistaRoles,
            [Seccion.Usuarios] = _vistaUsuarios,
            [Seccion.UsuariosRol] = _vistaUsuariosRol
        };
        foreach (Pagina pagina in _paginas.Values)
        {
            pagina.Visible = false;
            _contenido.Controls.Add(pagina);
        }

        // Primero el contenido (relleno) y después la barra lateral (izquierda)
        Controls.Add(_contenido);
        Controls.Add(_lateral);

        // Menú: cada enlace abre su sección
        foreach ((Seccion seccion, ItemMenu item) in _lateral.Items)
        {
            item.Click += (_, _) => MostrarSeccion(seccion);
        }
        _lateral.CerrarSesion.Click += (_, _) =>
        {
            Api.CerrarSesion();
            DialogResult = DialogResult.Retry; // Program.cs vuelve a mostrar el inicio de sesión
        };

        // La rueda del mouse desplaza la sección que está debajo del puntero
        Application.AddMessageFilter(_rueda);
        FormClosed += (_, _) => Application.RemoveMessageFilter(_rueda);

        MostrarSeccion(Seccion.Inicio);
        Load += async (_, _) => await EjecutarAsync(RecargarTodoAsync);
    }

    // ---------------------------------------------------------------------
    // Datos compartidos por las secciones
    // ---------------------------------------------------------------------

    public ApiCliente Api { get; }
    public UsuarioSesion Usuario { get; }
    public IReadOnlyList<Tarea> Tareas => _tareas;
    public IReadOnlyList<Estado> Estados => _estados;
    public IReadOnlyList<Proyecto> Proyectos => _proyectos;
    public IReadOnlyList<Usuario> Usuarios => _usuarios;
    public IReadOnlyList<Rol> Roles => _roles;

    /// <summary>Carga todo desde la API (las peticiones van al mismo tiempo).</summary>
    public async Task RecargarTodoAsync()
    {
        Task<List<Tarea>> tareas = Api.ObtenerTareasAsync();
        Task<List<Estado>> estados = Api.ObtenerEstadosAsync();
        Task<List<Proyecto>> proyectos = Api.ObtenerProyectosAsync();
        Task<List<Usuario>> usuarios = Api.ObtenerUsuariosAsync();
        Task<List<Rol>> roles = Usuario.EsAdmin ? Api.ObtenerRolesAsync() : Task.FromResult(new List<Rol>());
        await Task.WhenAll(tareas, estados, proyectos, usuarios, roles);

        _tareas = tareas.Result;
        _estados = estados.Result;
        _proyectos = proyectos.Result;
        _usuarios = usuarios.Result;
        _roles = roles.Result;
        RefrescarVistas();
    }

    /// <summary>Después de crear, editar o eliminar una tarea: tareas, proyectos (cuentan tareas) y usuarios (listan tareas).</summary>
    public async Task RecargarDatosAsync()
    {
        Task<List<Tarea>> tareas = Api.ObtenerTareasAsync();
        Task<List<Proyecto>> proyectos = Api.ObtenerProyectosAsync();
        Task<List<Usuario>> usuarios = Api.ObtenerUsuariosAsync();
        await Task.WhenAll(tareas, proyectos, usuarios);

        _tareas = tareas.Result;
        _proyectos = proyectos.Result;
        _usuarios = usuarios.Result;
        RefrescarVistas();
    }

    private void RefrescarVistas()
    {
        _vistaInicio.Refrescar();
        _vistaTareas.Refrescar();
        _vistaProyectos.Refrescar();
        if (!Usuario.EsAdmin) return;
        _vistaEstados.Refrescar();
        _vistaRoles.Refrescar();
        _vistaUsuarios.Refrescar();
        _vistaUsuariosRol.Refrescar();
    }

    /// <summary>
    /// Ejecuta una acción que llama a la API mostrando el cursor de espera.
    /// Si la sesión expiró, vuelve al inicio de sesión; si la API rechaza algo, muestra su mensaje.
    /// </summary>
    public async Task EjecutarAsync(Func<Task> accion)
    {
        if (_ocupado) return;
        _ocupado = true;
        UseWaitCursor = true;
        try
        {
            await accion();
        }
        catch (SesionExpiradaException)
        {
            DialogResult = DialogResult.Abort;
        }
        catch (ApiException ex)
        {
            MessageBox.Show(this, ex.Message, "No se pudo completar la acción", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            _ocupado = false;
            UseWaitCursor = false;
        }
    }

    /// <summary>Pregunta Sí / No antes de eliminar (como confirm() en la web).</summary>
    public bool Confirmar(string pregunta) =>
        MessageBox.Show(this, pregunta, "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2) == DialogResult.Yes;

    // ---------------------------------------------------------------------
    // Navegación
    // ---------------------------------------------------------------------

    public void MostrarSeccion(Seccion seccion)
    {
        // Las secciones de administración solo se abren para administradores
        if (seccion is Seccion.Estados or Seccion.Roles or Seccion.Usuarios or Seccion.UsuariosRol && !Usuario.EsAdmin)
        {
            seccion = Seccion.Inicio;
        }

        _seccionActual = seccion;
        Pagina mostrar = _paginas[seccion];
        foreach (Pagina pagina in _paginas.Values)
        {
            pagina.Visible = pagina == mostrar;
        }
        mostrar.PerformLayout();

        // El enlace activo del menú (Detalle cuenta como Tareas; Usuarios del rol como Usuarios)
        Seccion enlace = seccion switch
        {
            Seccion.Detalle => Seccion.Tareas,
            Seccion.UsuariosRol => Seccion.Usuarios,
            _ => seccion
        };
        foreach ((Seccion s, ItemMenu item) in _lateral.Items)
        {
            item.MarcarActivo(s == enlace);
        }
    }

    // ---------------------------------------------------------------------
    // Acciones de tareas
    // ---------------------------------------------------------------------

    /// <summary>GET /api/Tareas/{id} y abre la vista de detalle.</summary>
    public void VerDetalle(int idTarea)
    {
        _ = EjecutarAsync(async () =>
        {
            Tarea tarea = await Api.ObtenerTareaAsync(idTarea);
            _vistaDetalle.Mostrar(tarea);
            MostrarSeccion(Seccion.Detalle);
        });
    }

    /// <summary>PATCH /api/Tareas/{id}/estado y vuelve a mostrar el detalle actualizado.</summary>
    public void CambiarEstado(int idTarea, int idEstado)
    {
        _ = EjecutarAsync(async () =>
        {
            await Api.CambiarEstadoAsync(idTarea, idEstado);
            await RecargarDatosAsync();
            Tarea tarea = await Api.ObtenerTareaAsync(idTarea);
            _vistaDetalle.Mostrar(tarea);
        });
    }

    public void EditarTarea(Tarea tarea) => _vistaTareas.PrepararEdicion(tarea);

    /// <summary>DELETE /api/Tareas/{id} (solo Administrador), con confirmación.</summary>
    public void EliminarTarea(Tarea tarea)
    {
        if (!Confirmar($"¿Segura que deseas eliminar la tarea #{tarea.Id} \"{tarea.Titulo}\" de la base de datos?")) return;

        _ = EjecutarAsync(async () =>
        {
            await Api.EliminarTareaAsync(tarea.Id);
            if (_seccionActual == Seccion.Detalle) MostrarSeccion(Seccion.Tareas);
            await RecargarDatosAsync();
        });
    }

    /// <summary>Botón "Ver tareas" de un proyecto.</summary>
    public void VerTareasDeProyecto(int idProyecto)
    {
        MostrarSeccion(Seccion.Tareas);
        _vistaTareas.FiltrarPorProyecto(idProyecto);
    }

    // ---------------------------------------------------------------------
    // Acciones de usuarios
    // ---------------------------------------------------------------------

    public void VerUsuariosDeRol(Rol rol)
    {
        _vistaUsuariosRol.Mostrar(rol);
        MostrarSeccion(Seccion.UsuariosRol);
    }

    public void EditarUsuario(Usuario usuario)
    {
        MostrarSeccion(Seccion.Usuarios);
        _vistaUsuarios.PrepararEdicion(usuario);
    }

    public void EliminarUsuario(Usuario usuario)
    {
        if (!Confirmar($"¿Segura que deseas eliminar al usuario \"{usuario.Nombre}\"?")) return;

        _ = EjecutarAsync(async () =>
        {
            await Api.EliminarUsuarioAsync(usuario.Id);
            await RecargarTodoAsync();
        });
    }
}

/// <summary>Barra lateral oscura (header de la web): logo, menú y, abajo, el usuario con Cerrar sesión.</summary>
public class BarraLateral : Panel
{
    private readonly UsuarioSesion _usuario;
    private readonly List<(Seccion seccion, ItemMenu item)> _items = new();
    private int _yUsuario;

    public BarraLateral(UsuarioSesion usuario)
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
                 | ControlStyles.ResizeRedraw, true);
        _usuario = usuario;
        BackColor = Estilo.Lateral;
        CerrarSesion.Font = Estilo.PequenaNegrita;

        AgregarItem(Seccion.Inicio, "Inicio");
        AgregarItem(Seccion.Tareas, "Tareas");
        AgregarItem(Seccion.Proyectos, "Proyectos");
        if (usuario.EsAdmin) // .solo-admin
        {
            AgregarItem(Seccion.Estados, "Estados");
            AgregarItem(Seccion.Roles, "Roles");
            AgregarItem(Seccion.Usuarios, "Usuarios");
        }

        Controls.Add(CerrarSesion);

        // El ancho se pone al final: al cambiarlo se acomodan los controles, y para eso ya deben existir todos
        Dock = DockStyle.Left;
        Width = Estilo.E(250);
    }

    public IReadOnlyList<(Seccion seccion, ItemMenu item)> Items => _items;
    public BotonWeb CerrarSesion { get; } =
        new("Cerrar sesión", Estilo.LateralHover, Estilo.LateralBotonHover, Estilo.TextoClaro, Estilo.Lateral);

    private void AgregarItem(Seccion seccion, string texto)
    {
        var item = new ItemMenu(texto);
        _items.Add((seccion, item));
        Controls.Add(item);
    }

    // header { padding: 2rem 1.5rem }
    private static int RellenoX => Estilo.E(24);
    private static int RellenoY => Estilo.E(32);
    private static int AltoLogo => Estilo.E(32);

    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);
        int x = RellenoX;
        int ancho = Width - RellenoX * 2;

        // Logo + margin-bottom: 2rem, y luego los enlaces con gap: 15px
        int y = RellenoY + AltoLogo + Estilo.E(32);
        foreach ((Seccion _, ItemMenu item) in _items)
        {
            item.SetBounds(x, y, ancho, Estilo.E(44));
            y += Estilo.E(44) + Estilo.E(15);
        }

        // Usuario y Cerrar sesión pegados abajo (margin-top: auto)
        CerrarSesion.SetBounds(x, Height - RellenoY - Estilo.E(34), ancho, Estilo.E(34));
        _yUsuario = CerrarSesion.Top - Estilo.E(10) - Estilo.E(44);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.Clear(BackColor);

        // "ESA SAS" en celeste con letter-spacing: 2px
        Estilo.EscribirEspaciado(g, "ESA SAS", Estilo.Logo, Estilo.Celeste, new Rectangle(0, RellenoY, Width, AltoLogo), Estilo.E(2));

        // .usuario-sesion: línea arriba (border-top), nombre y rol
        int x = RellenoX;
        int ancho = Width - RellenoX * 2;
        using (var linea = new Pen(Estilo.LateralHover))
        {
            g.DrawLine(linea, x, _yUsuario - Estilo.E(20), x + ancho, _yUsuario - Estilo.E(20));
        }
        Estilo.Escribir(g, _usuario.Nombre, Estilo.Negrita, Estilo.TextoClaro, new Rectangle(x, _yUsuario, ancho, Estilo.E(22)));
        Estilo.Escribir(g, _usuario.Rol, Estilo.Pequena, Estilo.TextoMenu, new Rectangle(x, _yUsuario + Estilo.E(24), ancho, Estilo.E(18)));
    }
}
