using System.Globalization;

namespace Escritorio;

/// <summary>
/// Sección Tareas (igual que en la web): formulario para crear o editar, filtros por estado,
/// barra de filtros y el data grid de tareas.
/// </summary>
public class VistaTareas : Pagina
{
    private const int SinValor = -1; // Opción "Sin responsable" / "Sin proyecto" en los filtros

    private readonly FormPrincipal _app;
    private readonly FormularioTarea _formulario;
    private readonly ChipsEstado _chips = new();
    private readonly FlowLayoutPanel _barraFiltros;
    private readonly ComboBox _filtroResponsable = Estilo.Lista(38);
    private readonly ComboBox _filtroProyecto = Estilo.Lista(38);
    private readonly ComboBox _filtroPrioridad = Estilo.Lista(38);
    private readonly CheckBox _soloAtrasadas = new()
    {
        Text = "Solo atrasadas",
        AutoSize = true,
        Font = Estilo.PequenaNegrita,
        ForeColor = Estilo.TextoEtiqueta,
        BackColor = Estilo.FondoContenido,
        Cursor = Cursors.Hand
    };
    private readonly Grid<Tarea> _grid;
    private bool _llenandoFiltros;

    public VistaTareas(FormPrincipal app)
    {
        _app = app;

        // 1. Título
        Agregar(new TituloSeccion("Tareas"), 15);

        // 2. Formulario (solo Administrador y Líder de Proyecto, como .solo-gestion)
        _formulario = new FormularioTarea(app);
        Agregar(_formulario, 30, anchoMaximo: 700);
        MostrarBloque(_formulario, app.Usuario.PuedeGestionar);

        // 3. Botones de filtro por estado
        Agregar(_chips, 18);

        // 4. Barra de filtros
        int anchoFiltro = Estilo.E(220);
        foreach (ComboBox filtro in new[] { _filtroResponsable, _filtroProyecto, _filtroPrioridad })
        {
            filtro.Width = anchoFiltro;
            filtro.Margin = new Padding(0, 0, Estilo.E(10), Estilo.E(10));
        }
        _filtroPrioridad.Items.AddRange(new object[] { "Todas las prioridades", "Baja", "Media", "Alta", "Crítica" });
        _filtroPrioridad.SelectedIndex = 0;
        _soloAtrasadas.Margin = new Padding(Estilo.E(4), Estilo.E(9), 0, Estilo.E(10));

        _barraFiltros = new FlowLayoutPanel { BackColor = Estilo.FondoContenido, WrapContents = true };
        _barraFiltros.Controls.AddRange(new Control[] { _filtroResponsable, _filtroProyecto, _filtroPrioridad, _soloAtrasadas });
        Agregar(_barraFiltros, 4);

        // 5. Data grid: ID | Título | Responsable | Prioridad | Fecha límite | Estado | Acciones
        var columnas = new List<ColumnaGrid<Tarea>>
        {
            new("ID", 0, 60, (g, r, t) => Celdas.Texto(g, r, $"#{t.Id}", fuente: Estilo.Negrita, color: Estilo.Titulo)),
            new("Título", 2.2f, 0, (g, r, t) => Celdas.Texto(g, r, t.Titulo, t.Proyecto)),
            new("Responsable", 1.3f, 0, (g, r, t) => Celdas.Texto(g, r, t.Responsable)),
            new("Prioridad", 0.9f, 0, (g, r, t) =>
            {
                (Color fondo, Color texto) = Estilo.ColoresPrioridad(t.Prioridad);
                Celdas.Insignia(g, r, t.Prioridad, fondo, texto);
            }),
            new("Fecha límite", 1.1f, 0, (g, r, t) => Celdas.Texto(g, r, Formato.Fecha(t.FechaLimite),
                t.Atrasada ? "Atrasada" : null, fuenteSecundaria: Estilo.PequenaNegrita, colorSecundario: Estilo.TextoAtrasada)),
            new("Estado", 1f, 0, (g, r, t) => Celdas.Insignia(g, r, t.Estado, Estilo.EstadoFondo, Estilo.EstadoTexto, Estilo.EstadoBorde))
        };
        _grid = new Grid<Tarea>(columnas, AccionesDe, 230, "No hay tareas para mostrar con estos filtros.", t => t.Atrasada);
        Agregar(_grid, 10);

        // Cada filtro vuelve a aplicar todos los filtros juntos
        _chips.Cambio += _ => AplicarFiltros();
        _filtroResponsable.SelectedIndexChanged += (_, _) => AplicarFiltros();
        _filtroProyecto.SelectedIndexChanged += (_, _) => AplicarFiltros();
        _filtroPrioridad.SelectedIndexChanged += (_, _) => AplicarFiltros();
        _soloAtrasadas.CheckedChanged += (_, _) => AplicarFiltros();
    }

    /// <summary>Botones de cada fila según el rol (igual que .solo-gestion y .solo-admin en la web).</summary>
    private IEnumerable<AccionGrid<Tarea>> AccionesDe(Tarea tarea)
    {
        yield return new AccionGrid<Tarea>("Ver", Estilo.Gris, Estilo.GrisHover, t => _app.VerDetalle(t.Id));
        if (_app.Usuario.PuedeGestionar)
            yield return new AccionGrid<Tarea>("Editar", Estilo.Naranja, Estilo.NaranjaHover, PrepararEdicion);
        if (_app.Usuario.EsAdmin)
            yield return new AccionGrid<Tarea>("Eliminar", Estilo.Rojo, Estilo.RojoHover, t => _app.EliminarTarea(t));
    }

    /// <summary>Vuelve a llenar listas y filtros con los datos que tiene la ventana principal.</summary>
    public void Refrescar()
    {
        _llenandoFiltros = true;
        _chips.EstablecerEstados(_app.Estados.Select(e => e.Nombre));

        var responsables = new List<Opcion> { new(null, "Todos los responsables"), new(SinValor, "Sin responsable") };
        responsables.AddRange(_app.Usuarios.Select(u => new Opcion(u.Id, u.Nombre)));
        Estilo.LlenarLista(_filtroResponsable, responsables);

        var proyectos = new List<Opcion> { new(null, "Todos los proyectos"), new(SinValor, "Sin proyecto") };
        proyectos.AddRange(_app.Proyectos.Select(p => new Opcion(p.Id, p.Nombre)));
        Estilo.LlenarLista(_filtroProyecto, proyectos);
        _llenandoFiltros = false;

        _formulario.CargarListas(_app.Usuarios, _app.Proyectos, _app.Estados);
        AplicarFiltros();
    }

    /// <summary>Aplica TODOS los filtros a la vez: estado + responsable + proyecto + prioridad + atrasadas.</summary>
    private void AplicarFiltros()
    {
        if (_llenandoFiltros) return;

        string estado = _chips.Seleccionado;
        int? responsable = (_filtroResponsable.SelectedItem as Opcion)?.Id;
        int? proyecto = (_filtroProyecto.SelectedItem as Opcion)?.Id;
        string? prioridad = _filtroPrioridad.SelectedIndex > 0 ? _filtroPrioridad.SelectedItem as string : null;
        bool soloAtrasadas = _soloAtrasadas.Checked;

        IEnumerable<Tarea> filtradas = _app.Tareas.Where(t =>
            (estado == "Todas" || t.Estado == estado) &&
            (responsable == null || (responsable == SinValor ? t.IdUsuario == null : t.IdUsuario == responsable)) &&
            (proyecto == null || (proyecto == SinValor ? t.IdProyecto == null : t.IdProyecto == proyecto)) &&
            (prioridad == null || t.Prioridad == prioridad) &&
            (!soloAtrasadas || t.Atrasada));

        _grid.MostrarDatos(filtradas);
    }

    /// <summary>"Ver tareas" desde Proyectos: deja solo el filtro de ese proyecto.</summary>
    public void FiltrarPorProyecto(int idProyecto)
    {
        _llenandoFiltros = true;
        _chips.Seleccionar("Todas");
        Estilo.SeleccionarOpcion(_filtroResponsable, null);
        Estilo.SeleccionarOpcion(_filtroProyecto, idProyecto);
        _filtroPrioridad.SelectedIndex = 0;
        _soloAtrasadas.Checked = false;
        _llenandoFiltros = false;
        AplicarFiltros();
        IrArriba();
    }

    /// <summary>Botón Editar: carga la tarea en el formulario de arriba.</summary>
    public void PrepararEdicion(Tarea tarea)
    {
        _app.MostrarSeccion(Seccion.Tareas);
        IrArriba();
        _formulario.PrepararEdicion(tarea);
    }
}

/// <summary>Formulario para crear o editar una tarea (form#form-crear-tarea de la web).</summary>
public class FormularioTarea : Tarjeta, IAltoSegunAncho
{
    private static readonly string[] Prioridades = { "Baja", "Media", "Alta", "Crítica" };

    private readonly FormPrincipal _app;
    private readonly CajaTexto _titulo = new("Escribe el título de la tarea...", Color.White);
    private readonly CajaTexto _descripcion = new("Descripción de la tarea (opcional)...", Color.White, multilinea: true, altoLogico: 90);
    private readonly ComboBox _responsable = Estilo.Lista();
    private readonly ComboBox _proyecto = Estilo.Lista();
    private readonly ComboBox _estado = Estilo.Lista();
    private readonly ComboBox _prioridad = Estilo.Lista();
    private readonly CampoFecha _fechaLimite = new(Color.White);
    private readonly BotonWeb _btnGuardar = BotonWeb.Azul("Crear Tarea", Color.White);
    private readonly BotonWeb _btnCancelar = BotonWeb.Cancelar("Cancelar", Color.White);
    private readonly List<(string texto, Point posicion)> _etiquetas = new();
    private int? _idEnEdicion;
    private string _error = "";

    public FormularioTarea(FormPrincipal app) : base(Estilo.FondoContenido)
    {
        _app = app;
        _titulo.Caja.MaxLength = 200;
        _descripcion.Caja.MaxLength = 2000;
        _prioridad.Items.AddRange(Prioridades);
        _prioridad.SelectedItem = "Media";
        _btnCancelar.Visible = false;

        Controls.AddRange(new Control[]
        {
            _titulo, _descripcion, _responsable, _proyecto, _estado, _prioridad, _fechaLimite, _btnGuardar, _btnCancelar
        });

        _btnGuardar.Click += async (_, _) => await GuardarAsync();
        _btnCancelar.Click += (_, _) => CancelarEdicion();
    }

    private static int Relleno => Estilo.E(25);       // padding: 25px
    private static int Espacio => Estilo.E(15);       // gap: 15px
    private static int AltoCampo => Estilo.E(44);
    private static int AltoEtiqueta => Estilo.E(24);

    public int AltoPara(int ancho) =>
        Relleno + AltoCampo + Espacio + _descripcion.Height + Espacio
        + (AltoEtiqueta + AltoCampo + Espacio) * 2
        + _btnGuardar.Height + Relleno;

    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);
        int x = Relleno;
        int ancho = Width - Relleno * 2;
        int y = Relleno;
        _etiquetas.Clear();

        _titulo.SetBounds(x, y, ancho, AltoCampo);
        y += AltoCampo + Espacio;
        _descripcion.SetBounds(x, y, ancho, _descripcion.Height);
        y += _descripcion.Height + Espacio;

        // Fila de 2 columnas: Responsable | Proyecto
        int columna2 = (ancho - Espacio) / 2;
        Ubicar("Responsable", _responsable, x, y, columna2);
        Ubicar("Proyecto", _proyecto, x + columna2 + Espacio, y, columna2);
        y += AltoEtiqueta + AltoCampo + Espacio;

        // Fila de 3 columnas: Estado | Prioridad | Fecha límite
        int columna3 = (ancho - Espacio * 2) / 3;
        Ubicar("Estado", _estado, x, y, columna3);
        Ubicar("Prioridad", _prioridad, x + columna3 + Espacio, y, columna3);
        Ubicar("Fecha límite", _fechaLimite, x + (columna3 + Espacio) * 2, y, columna3);
        y += AltoEtiqueta + AltoCampo + Espacio;

        _btnGuardar.Location = new Point(x, y);
        _btnCancelar.Location = new Point(_btnGuardar.Right + Estilo.E(10), y);
        Invalidate();
    }

    private void Ubicar(string etiqueta, Control campo, int x, int y, int ancho)
    {
        _etiquetas.Add((etiqueta, new Point(x, y)));
        int altoCampo = campo is ComboBox ? campo.Height : AltoCampo;
        campo.SetBounds(x, y + AltoEtiqueta, ancho, altoCampo);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;

        // Etiquetas de los campos (.campo-formulario label)
        foreach ((string texto, Point posicion) in _etiquetas)
        {
            Estilo.Escribir(g, texto, Estilo.PequenaNegrita, Estilo.TextoEtiqueta,
                new Rectangle(posicion.X, posicion.Y, Estilo.E(300), AltoEtiqueta - Estilo.E(6)));
        }

        // Mensaje de validación al lado de los botones
        if (_error.Length > 0)
        {
            int x = (_btnCancelar.Visible ? _btnCancelar.Right : _btnGuardar.Right) + Estilo.E(15);
            Estilo.Escribir(g, _error, Estilo.PequenaNegrita, Estilo.TextoAtrasada,
                new Rectangle(x, _btnGuardar.Top, Math.Max(0, Width - x - Relleno), _btnGuardar.Height));
        }
    }

    /// <summary>Llena las listas desplegables con los datos de la API.</summary>
    public void CargarListas(IReadOnlyList<Usuario> usuarios, IReadOnlyList<Proyecto> proyectos, IReadOnlyList<Estado> estados)
    {
        var opcionesResponsable = new List<Opcion> { new(null, "Sin responsable") };
        opcionesResponsable.AddRange(usuarios.Select(u => new Opcion(u.Id, u.Nombre)));
        Estilo.LlenarLista(_responsable, opcionesResponsable);

        var opcionesProyecto = new List<Opcion> { new(null, "Sin proyecto") };
        opcionesProyecto.AddRange(proyectos.Select(p => new Opcion(p.Id, p.Nombre)));
        Estilo.LlenarLista(_proyecto, opcionesProyecto);

        var opcionesEstado = new List<Opcion> { new(null, "Selecciona el estado...") };
        opcionesEstado.AddRange(estados.Select(e => new Opcion(e.Id, e.Nombre)));
        Estilo.LlenarLista(_estado, opcionesEstado);
    }

    public void PrepararEdicion(Tarea tarea)
    {
        _idEnEdicion = tarea.Id;
        _titulo.Caja.Text = tarea.Titulo;
        _descripcion.Caja.Text = tarea.Descripcion ?? "";
        Estilo.SeleccionarOpcion(_responsable, tarea.IdUsuario);
        Estilo.SeleccionarOpcion(_proyecto, tarea.IdProyecto);
        Estilo.SeleccionarOpcion(_estado, tarea.IdEstado);
        _prioridad.SelectedItem = Prioridades.Contains(tarea.Prioridad) ? tarea.Prioridad : "Media";
        _fechaLimite.Establecer(
            DateTime.TryParseExact(tarea.FechaLimite, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha)
                ? fecha : null);

        _btnGuardar.Text = "Actualizar Tarea";
        _btnGuardar.AjustarTamano();
        _btnCancelar.Visible = true;
        _error = "";
        PerformLayout();
        _titulo.Caja.Focus();
    }

    public void CancelarEdicion()
    {
        _idEnEdicion = null;
        _titulo.Caja.Text = "";
        _descripcion.Caja.Text = "";
        Estilo.SeleccionarOpcion(_responsable, null);
        Estilo.SeleccionarOpcion(_proyecto, null);
        Estilo.SeleccionarOpcion(_estado, null);
        _prioridad.SelectedItem = "Media";
        _fechaLimite.Establecer(null);

        _btnGuardar.Text = "Crear Tarea";
        _btnGuardar.AjustarTamano();
        _btnCancelar.Visible = false;
        _error = "";
        PerformLayout();
    }

    private void MostrarError(string mensaje)
    {
        _error = mensaje;
        Invalidate();
    }

    /// <summary>POST /api/Tareas (crear) o PUT /api/Tareas/{id} (editar).</summary>
    private async Task GuardarAsync()
    {
        _error = "";
        string titulo = _titulo.Caja.Text.Trim();
        int? idEstado = (_estado.SelectedItem as Opcion)?.Id;

        if (titulo.Length == 0)
        {
            MostrarError("Escribe el título de la tarea.");
            _titulo.Caja.Focus();
            return;
        }
        if (idEstado == null)
        {
            MostrarError("Selecciona el estado de la tarea.");
            return;
        }

        var datos = new NuevaTarea
        {
            Titulo = titulo,
            Descripcion = string.IsNullOrWhiteSpace(_descripcion.Caja.Text) ? null : _descripcion.Caja.Text.Trim(),
            IdUsuarioResponsable = (_responsable.SelectedItem as Opcion)?.Id,
            IdEstado = idEstado.Value,
            IdProyecto = (_proyecto.SelectedItem as Opcion)?.Id,
            Prioridad = _prioridad.SelectedItem as string ?? "Media",
            FechaLimite = _fechaLimite.Fecha?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        };
        int? idEnEdicion = _idEnEdicion;

        _btnGuardar.Enabled = false;
        Invalidate();
        try
        {
            await _app.EjecutarAsync(async () =>
            {
                if (idEnEdicion == null) await _app.Api.CrearTareaAsync(datos);
                else await _app.Api.ActualizarTareaAsync(idEnEdicion.Value, datos);

                CancelarEdicion();
                await _app.RecargarDatosAsync();
            });
        }
        finally
        {
            _btnGuardar.Enabled = true;
        }
    }
}
