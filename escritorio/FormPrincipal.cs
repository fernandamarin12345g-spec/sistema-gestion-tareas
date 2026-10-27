using System.Text;

namespace Escritorio;

/// <summary>
/// Ventana principal: lista de tareas con filtros, y acciones según el rol del usuario.
/// </summary>
public class FormPrincipal : Form
{
    private readonly ApiCliente _api;
    private readonly UsuarioSesion _usuario;

    // Filtros
    private readonly ComboBox _cmbEstado = Estilo.Lista();
    private readonly ComboBox _cmbProyecto = Estilo.Lista();
    private readonly CheckBox _chkAtrasadas = new() { Text = "Solo atrasadas", AutoSize = true, Margin = new Padding(12, 8, 3, 3) };
    private readonly CheckBox _chkMias = new() { Text = "Solo mis tareas", AutoSize = true, Margin = new Padding(12, 8, 3, 3) };
    private readonly Button _btnActualizar = Estilo.Boton("Actualizar");

    // Tabla y acciones
    private readonly DataGridView _tabla = new();
    private readonly Button _btnNueva = Estilo.BotonPrincipal("+ Nueva tarea");
    private readonly Button _btnDetalle = Estilo.Boton("Ver detalle");
    private readonly Button _btnCambiarEstado = Estilo.Boton("Cambiar estado");
    private readonly Button _btnEliminar = Estilo.Boton("Eliminar");
    private readonly Button _btnCerrarSesion = Estilo.Boton("Cerrar sesión");

    // Barra de estado inferior
    private readonly ToolStripStatusLabel _lblUsuario = new();
    private readonly ToolStripStatusLabel _lblResumen = new() { Spring = true, TextAlign = ContentAlignment.MiddleRight };

    private List<Estado> _estados = new();
    private bool _cargandoFiltros;
    private bool _ocupado;

    public FormPrincipal(ApiCliente api)
    {
        _api = api;
        _usuario = api.Sesion?.Usuario ?? throw new InvalidOperationException("No hay una sesión iniciada.");

        Text = "Sistema de Gestión de Tareas · ESA SAS";
        Font = Estilo.Fuente();
        StartPosition = FormStartPosition.CenterScreen;
        Size = LogicalToDeviceUnits(new Size(1150, 650));
        MinimumSize = LogicalToDeviceUnits(new Size(900, 500));

        ConstruirFiltros(out FlowLayoutPanel panelFiltros);
        ConstruirTabla();
        ConstruirAcciones(out FlowLayoutPanel panelAcciones);

        var barra = new StatusStrip();
        barra.Items.Add(_lblUsuario);
        barra.Items.Add(_lblResumen);
        _lblUsuario.Text = $"{_usuario.Nombre} · {_usuario.Rol}";

        // Orden de acoplamiento: la barra queda abajo del todo, luego las acciones,
        // los filtros arriba y la tabla ocupa el espacio que sobra.
        Controls.Add(_tabla);
        Controls.Add(panelFiltros);
        Controls.Add(panelAcciones);
        Controls.Add(barra);
        _tabla.BringToFront();

        // Permisos: los botones que el rol no puede usar ni se muestran
        _btnNueva.Visible = _usuario.GestionaTareas;   // Administrador y Líder de Proyecto
        _btnEliminar.Visible = _usuario.EsAdmin;       // Solo Administrador

        Load += async (_, _) => await EjecutarAsync(CargarTodoAsync);
    }

    // ---------------------------------------------------------------------
    // Construcción de la interfaz
    // ---------------------------------------------------------------------

    private void ConstruirFiltros(out FlowLayoutPanel panel)
    {
        _cmbEstado.Width = LogicalToDeviceUnits(170);
        _cmbProyecto.Width = LogicalToDeviceUnits(240);

        panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(LogicalToDeviceUnits(8))
        };
        panel.Controls.Add(Estilo.Etiqueta("Estado:"));
        panel.Controls.Add(_cmbEstado);
        panel.Controls.Add(Estilo.Etiqueta("Proyecto:"));
        panel.Controls.Add(_cmbProyecto);
        panel.Controls.Add(_chkAtrasadas);
        panel.Controls.Add(_chkMias);
        panel.Controls.Add(_btnActualizar);

        // Cada vez que cambia un filtro se vuelve a pedir la lista a la API
        _cmbEstado.SelectedIndexChanged += async (_, _) => await FiltrosCambiaronAsync();
        _cmbProyecto.SelectedIndexChanged += async (_, _) => await FiltrosCambiaronAsync();
        _chkAtrasadas.CheckedChanged += async (_, _) => await FiltrosCambiaronAsync();
        _chkMias.CheckedChanged += async (_, _) => await FiltrosCambiaronAsync();
        _btnActualizar.Click += async (_, _) => await EjecutarAsync(CargarTareasAsync);
    }

    private void ConstruirTabla()
    {
        _tabla.Dock = DockStyle.Fill;
        _tabla.ReadOnly = true;
        _tabla.AllowUserToAddRows = false;
        _tabla.AllowUserToDeleteRows = false;
        _tabla.AllowUserToResizeRows = false;
        _tabla.MultiSelect = false;
        _tabla.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _tabla.RowHeadersVisible = false;
        _tabla.AutoGenerateColumns = false;
        _tabla.BackgroundColor = Color.White;
        _tabla.BorderStyle = BorderStyle.None;
        _tabla.RowTemplate.Height = LogicalToDeviceUnits(30);

        _tabla.EnableHeadersVisualStyles = false;
        _tabla.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        _tabla.ColumnHeadersHeight = LogicalToDeviceUnits(34);
        _tabla.ColumnHeadersDefaultCellStyle.BackColor = Estilo.Principal;
        _tabla.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        _tabla.ColumnHeadersDefaultCellStyle.Font = Estilo.Fuente(10F, FontStyle.Bold);
        _tabla.DefaultCellStyle.SelectionBackColor = Estilo.FondoSeleccion;
        _tabla.DefaultCellStyle.SelectionForeColor = Color.Black;
        _tabla.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);

        AgregarColumna(nameof(Tarea.Id), "#", 50);
        AgregarColumna(nameof(Tarea.Titulo), "Título", 0);
        AgregarColumna(nameof(Tarea.Responsable), "Responsable", 160);
        AgregarColumna(nameof(Tarea.Estado), "Estado", 120);
        AgregarColumna(nameof(Tarea.Proyecto), "Proyecto", 190);
        AgregarColumna(nameof(Tarea.Prioridad), "Prioridad", 90);
        AgregarColumna(nameof(Tarea.FechaLimiteTexto), "Fecha límite", 110);
        AgregarColumna(nameof(Tarea.AtrasadaTexto), "Atrasada", 80);

        _tabla.DataBindingComplete += (_, _) => PintarAtrasadas();
        _tabla.SelectionChanged += (_, _) => ActualizarBotones();
        _tabla.CellDoubleClick += (_, e) =>
        {
            if (e.RowIndex >= 0) MostrarDetalle();
        };
    }

    /// <param name="ancho">Ancho en píxeles; 0 = la columna ocupa el espacio que sobra.</param>
    private void AgregarColumna(string propiedad, string titulo, int ancho)
    {
        var columna = new DataGridViewTextBoxColumn
        {
            DataPropertyName = propiedad,
            HeaderText = titulo,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        if (ancho == 0)
        {
            columna.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            columna.MinimumWidth = LogicalToDeviceUnits(180);
        }
        else
        {
            columna.Width = LogicalToDeviceUnits(ancho);
        }
        _tabla.Columns.Add(columna);
    }

    private void ConstruirAcciones(out FlowLayoutPanel panel)
    {
        panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            AutoSize = true,
            Padding = new Padding(LogicalToDeviceUnits(8))
        };
        panel.Controls.Add(_btnNueva);
        panel.Controls.Add(_btnDetalle);
        panel.Controls.Add(_btnCambiarEstado);
        panel.Controls.Add(_btnEliminar);
        panel.Controls.Add(_btnCerrarSesion);
        _btnCerrarSesion.Margin = new Padding(LogicalToDeviceUnits(30), 3, 3, 3);

        _btnNueva.Click += async (_, _) => await EjecutarAsync(NuevaTareaAsync);
        _btnDetalle.Click += (_, _) => MostrarDetalle();
        _btnCambiarEstado.Click += async (_, _) => await EjecutarAsync(CambiarEstadoAsync);
        _btnEliminar.Click += async (_, _) => await EjecutarAsync(EliminarAsync);
        _btnCerrarSesion.Click += (_, _) => CerrarSesion();
    }

    // ---------------------------------------------------------------------
    // Carga de datos
    // ---------------------------------------------------------------------

    /// <summary>
    /// Ejecuta una acción que llama a la API mostrando el cursor de espera.
    /// Si la sesión expiró, cierra la ventana y vuelve al inicio de sesión.
    /// </summary>
    private async Task EjecutarAsync(Func<Task> accion)
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

    private async Task CargarTodoAsync()
    {
        // Estados y proyectos se piden al mismo tiempo
        Task<List<Estado>> pedirEstados = _api.ObtenerEstadosAsync();
        Task<List<Proyecto>> pedirProyectos = _api.ObtenerProyectosAsync();
        _estados = await pedirEstados;
        List<Proyecto> proyectos = await pedirProyectos;

        _cargandoFiltros = true; // Evita recargar la tabla mientras se llenan las listas
        _cmbEstado.Items.Clear();
        _cmbEstado.Items.Add(new Opcion(null, "Todos"));
        foreach (Estado e in _estados)
        {
            _cmbEstado.Items.Add(new Opcion(e.Id, e.Nombre));
        }
        _cmbEstado.SelectedIndex = 0;

        _cmbProyecto.Items.Clear();
        _cmbProyecto.Items.Add(new Opcion(null, "Todos"));
        foreach (Proyecto p in proyectos)
        {
            _cmbProyecto.Items.Add(new Opcion(p.Id, p.Nombre));
        }
        _cmbProyecto.SelectedIndex = 0;
        _cargandoFiltros = false;

        await CargarTareasAsync();
    }

    private async Task FiltrosCambiaronAsync()
    {
        if (_cargandoFiltros) return;
        await EjecutarAsync(CargarTareasAsync);
    }

    /// <summary>GET /api/Tareas con los filtros elegidos.</summary>
    private async Task CargarTareasAsync()
    {
        int? idEstado = (_cmbEstado.SelectedItem as Opcion)?.Id;
        int? idProyecto = (_cmbProyecto.SelectedItem as Opcion)?.Id;
        int? idUsuario = _chkMias.Checked ? _usuario.IdUsuario : null;

        List<Tarea> tareas = await _api.ObtenerTareasAsync(idEstado, idProyecto, idUsuario, _chkAtrasadas.Checked);
        _tabla.DataSource = tareas;

        int atrasadas = tareas.Count(t => t.Atrasada);
        _lblResumen.Text = $"{tareas.Count} tarea(s) · {atrasadas} atrasada(s) · actualizado a las {DateTime.Now:HH:mm}";
        ActualizarBotones();
    }

    /// <summary>Las tareas atrasadas se pintan en rojo claro.</summary>
    private void PintarAtrasadas()
    {
        foreach (DataGridViewRow fila in _tabla.Rows)
        {
            if (fila.DataBoundItem is Tarea { Atrasada: true })
            {
                fila.DefaultCellStyle.BackColor = Estilo.FondoAtrasada;
                fila.DefaultCellStyle.ForeColor = Estilo.TextoAtrasada;
            }
        }
    }

    private Tarea? TareaSeleccionada => _tabla.CurrentRow?.DataBoundItem as Tarea;

    /// <summary>Activa los botones según la tarea seleccionada y el rol.</summary>
    private void ActualizarBotones()
    {
        Tarea? tarea = TareaSeleccionada;
        _btnDetalle.Enabled = tarea != null;
        _btnEliminar.Enabled = tarea != null;
        // Cambiar estado: quien gestiona tareas, o el responsable de la tarea
        _btnCambiarEstado.Enabled = tarea != null && (_usuario.GestionaTareas || tarea.IdUsuario == _usuario.IdUsuario);
    }

    private void SeleccionarTarea(int idTarea)
    {
        foreach (DataGridViewRow fila in _tabla.Rows)
        {
            if (fila.DataBoundItem is Tarea tarea && tarea.Id == idTarea)
            {
                _tabla.CurrentCell = fila.Cells[0];
                return;
            }
        }
    }

    // ---------------------------------------------------------------------
    // Acciones
    // ---------------------------------------------------------------------

    private void MostrarDetalle()
    {
        Tarea? t = TareaSeleccionada;
        if (t == null) return;

        var texto = new StringBuilder();
        texto.AppendLine(t.Titulo);
        texto.AppendLine();
        texto.AppendLine(string.IsNullOrWhiteSpace(t.Descripcion) ? "Sin descripción" : t.Descripcion);
        texto.AppendLine();
        texto.AppendLine($"Responsable: {t.Responsable}");
        texto.AppendLine($"Estado: {t.Estado}");
        texto.AppendLine($"Prioridad: {t.Prioridad}");
        texto.AppendLine($"Proyecto: {t.Proyecto}");
        texto.AppendLine($"Fecha de creación: {Formato.Fecha(t.FechaCreacion)}");
        texto.AppendLine($"Fecha límite: {Formato.Fecha(t.FechaLimite)}{(t.Atrasada ? "  (ATRASADA)" : "")}");
        texto.AppendLine($"Fecha de finalización: {Formato.Fecha(t.FechaFinalizacion)}");
        texto.AppendLine($"Última actualización: {Formato.Fecha(t.FechaActualizacion)}");

        MessageBox.Show(this, texto.ToString(), $"Tarea #{t.Id}", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async Task NuevaTareaAsync()
    {
        List<Usuario> usuarios = await _api.ObtenerUsuariosAsync();
        List<Proyecto> proyectos = await _api.ObtenerProyectosAsync();

        using var formulario = new FormTarea(_api, usuarios, _estados, proyectos);
        DialogResult resultado = formulario.ShowDialog(this);

        if (resultado == DialogResult.Abort) throw new SesionExpiradaException();
        if (resultado == DialogResult.OK) await CargarTareasAsync();
    }

    /// <summary>PATCH /api/Tareas/{id}/estado</summary>
    private async Task CambiarEstadoAsync()
    {
        Tarea? tarea = TareaSeleccionada;
        if (tarea == null) return;

        using var dialogo = new FormCambiarEstado(tarea, _estados);
        if (dialogo.ShowDialog(this) != DialogResult.OK) return;

        Estado? nuevo = dialogo.EstadoElegido;
        if (nuevo == null || nuevo.Id == tarea.IdEstado) return;

        await _api.CambiarEstadoAsync(tarea.Id, nuevo.Id);
        await CargarTareasAsync();
        SeleccionarTarea(tarea.Id);
    }

    /// <summary>DELETE /api/Tareas/{id} (solo Administrador)</summary>
    private async Task EliminarAsync()
    {
        Tarea? tarea = TareaSeleccionada;
        if (tarea == null) return;

        DialogResult confirmar = MessageBox.Show(this,
            $"¿Eliminar la tarea #{tarea.Id} \"{tarea.Titulo}\"?\nEsta acción no se puede deshacer.",
            "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
        if (confirmar != DialogResult.Yes) return;

        await _api.EliminarTareaAsync(tarea.Id);
        await CargarTareasAsync();
    }

    private void CerrarSesion()
    {
        _api.CerrarSesion();
        DialogResult = DialogResult.Retry; // Program.cs vuelve a mostrar el inicio de sesión
    }
}
