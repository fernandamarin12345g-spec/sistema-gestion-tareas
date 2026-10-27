using System.Globalization;

namespace Escritorio;

/// <summary>Formulario para crear una tarea (POST /api/Tareas).</summary>
public class FormTarea : Form
{
    private static readonly string[] Prioridades = { "Baja", "Media", "Alta", "Crítica" };

    private readonly ApiCliente _api;
    private readonly TextBox _txtTitulo = new() { MaxLength = 200 };
    private readonly TextBox _txtDescripcion = new() { Multiline = true, ScrollBars = ScrollBars.Vertical, MaxLength = 2000 };
    private readonly ComboBox _cmbResponsable = Estilo.Lista();
    private readonly ComboBox _cmbEstado = Estilo.Lista();
    private readonly ComboBox _cmbProyecto = Estilo.Lista();
    private readonly ComboBox _cmbPrioridad = Estilo.Lista();
    private readonly DateTimePicker _dtpFechaLimite = new() { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = false };
    private readonly Label _lblError = new() { AutoSize = true, ForeColor = Estilo.Error };
    private readonly Button _btnGuardar = Estilo.BotonPrincipal("Guardar");

    public FormTarea(ApiCliente api, IReadOnlyList<Usuario> usuarios, IReadOnlyList<Estado> estados, IReadOnlyList<Proyecto> proyectos)
    {
        _api = api;

        Text = "Nueva tarea";
        Font = Estilo.Fuente();
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;

        // Listas desplegables
        _cmbResponsable.Items.Add(new Opcion(null, "Sin asignar"));
        foreach (Usuario u in usuarios)
        {
            _cmbResponsable.Items.Add(new Opcion(u.Id, $"{u.Nombre} ({u.Rol})"));
        }
        _cmbResponsable.SelectedIndex = 0;

        foreach (Estado e in estados)
        {
            _cmbEstado.Items.Add(e);
        }
        // Por defecto, el primer estado que no es final (por ejemplo, Pendiente)
        Estado? inicial = estados.FirstOrDefault(e => !e.EsFinal);
        if (inicial != null) _cmbEstado.SelectedItem = inicial;
        else if (_cmbEstado.Items.Count > 0) _cmbEstado.SelectedIndex = 0;

        _cmbProyecto.Items.Add(new Opcion(null, "Sin proyecto"));
        foreach (Proyecto p in proyectos.Where(p => p.Estado != "Finalizado"))
        {
            _cmbProyecto.Items.Add(new Opcion(p.Id, p.Nombre));
        }
        _cmbProyecto.SelectedIndex = 0;

        _cmbPrioridad.Items.AddRange(Prioridades);
        _cmbPrioridad.SelectedItem = "Media";

        // Tamaños según la escala de la pantalla
        int anchoCampo = LogicalToDeviceUnits(340);
        _cmbResponsable.Width = anchoCampo;
        _cmbEstado.Width = anchoCampo;
        _cmbProyecto.Width = anchoCampo;
        _cmbPrioridad.Width = LogicalToDeviceUnits(160);
        _dtpFechaLimite.Width = LogicalToDeviceUnits(160);
        _txtTitulo.Width = anchoCampo;
        _txtDescripcion.Width = anchoCampo;
        _txtDescripcion.Height = LogicalToDeviceUnits(80);
        _lblError.MaximumSize = new Size(LogicalToDeviceUnits(460), 0);

        // Tabla de dos columnas: etiqueta | campo
        var tabla = new TableLayoutPanel
        {
            ColumnCount = 2,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(LogicalToDeviceUnits(16))
        };
        tabla.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, anchoCampo + LogicalToDeviceUnits(8)));

        AgregarFila(tabla, "Título *", _txtTitulo);
        AgregarFila(tabla, "Descripción", _txtDescripcion);
        AgregarFila(tabla, "Responsable", _cmbResponsable);
        AgregarFila(tabla, "Estado *", _cmbEstado);
        AgregarFila(tabla, "Proyecto", _cmbProyecto);
        AgregarFila(tabla, "Prioridad", _cmbPrioridad);
        AgregarFila(tabla, "Fecha límite", _dtpFechaLimite);

        // Mensaje de error (ocupa las dos columnas)
        tabla.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tabla.Controls.Add(_lblError, 0, tabla.RowCount);
        tabla.SetColumnSpan(_lblError, 2);
        tabla.RowCount++;

        // Botones
        var btnCancelar = Estilo.Boton("Cancelar");
        btnCancelar.DialogResult = DialogResult.Cancel;
        var botones = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, AutoSize = true, Anchor = AnchorStyles.Right };
        botones.Controls.Add(_btnGuardar);
        botones.Controls.Add(btnCancelar);
        tabla.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tabla.Controls.Add(botones, 0, tabla.RowCount);
        tabla.SetColumnSpan(botones, 2);
        tabla.RowCount++;

        Controls.Add(tabla);

        CancelButton = btnCancelar;
        _btnGuardar.Click += async (_, _) => await GuardarAsync();

        var ayuda = new ToolTip();
        ayuda.SetToolTip(_dtpFechaLimite, "Marca la casilla para poner fecha límite");
    }

    private static void AgregarFila(TableLayoutPanel tabla, string etiqueta, Control campo)
    {
        tabla.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var lbl = Estilo.Etiqueta(etiqueta);
        lbl.Anchor = AnchorStyles.Left | AnchorStyles.Top;
        tabla.Controls.Add(lbl, 0, tabla.RowCount);
        tabla.Controls.Add(campo, 1, tabla.RowCount);
        tabla.RowCount++;
    }

    private async Task GuardarAsync()
    {
        _lblError.Text = "";

        // Validaciones rápidas antes de llamar a la API (la API también valida)
        if (string.IsNullOrWhiteSpace(_txtTitulo.Text))
        {
            _lblError.Text = "El título de la tarea es obligatorio.";
            _txtTitulo.Focus();
            return;
        }
        if (_cmbEstado.SelectedItem is not Estado estado)
        {
            _lblError.Text = "Debes seleccionar un estado.";
            return;
        }

        var nueva = new NuevaTarea
        {
            Titulo = _txtTitulo.Text.Trim(),
            Descripcion = string.IsNullOrWhiteSpace(_txtDescripcion.Text) ? null : _txtDescripcion.Text.Trim(),
            IdUsuarioResponsable = (_cmbResponsable.SelectedItem as Opcion)?.Id,
            IdEstado = estado.Id,
            IdProyecto = (_cmbProyecto.SelectedItem as Opcion)?.Id,
            Prioridad = _cmbPrioridad.SelectedItem as string ?? "Media",
            FechaLimite = _dtpFechaLimite.Checked
                ? _dtpFechaLimite.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                : null
        };

        _btnGuardar.Enabled = false;
        try
        {
            await _api.CrearTareaAsync(nueva);
            DialogResult = DialogResult.OK;
        }
        catch (SesionExpiradaException)
        {
            DialogResult = DialogResult.Abort;
        }
        catch (ApiException ex)
        {
            _lblError.Text = ex.Message; // Por ejemplo, el mensaje 400 de la API
        }
        finally
        {
            _btnGuardar.Enabled = true;
        }
    }
}
