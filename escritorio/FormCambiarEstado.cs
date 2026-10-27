namespace Escritorio;

/// <summary>Ventana pequeña para elegir el nuevo estado de una tarea.</summary>
public class FormCambiarEstado : Form
{
    private readonly ComboBox _cmbEstado = Estilo.Lista();

    /// <summary>El estado que eligió el usuario (se lee después de ShowDialog).</summary>
    public Estado? EstadoElegido => _cmbEstado.SelectedItem as Estado;

    public FormCambiarEstado(Tarea tarea, IReadOnlyList<Estado> estados)
    {
        Text = $"Cambiar estado · Tarea #{tarea.Id}";
        Font = Estilo.Fuente();
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;

        int ancho = LogicalToDeviceUnits(320);
        _cmbEstado.Width = ancho;
        foreach (Estado estado in estados)
        {
            _cmbEstado.Items.Add(estado);
        }
        Estado? actual = estados.FirstOrDefault(e => e.Id == tarea.IdEstado);
        if (actual != null) _cmbEstado.SelectedItem = actual;
        else if (_cmbEstado.Items.Count > 0) _cmbEstado.SelectedIndex = 0;

        var btnAceptar = Estilo.BotonPrincipal("Guardar");
        btnAceptar.DialogResult = DialogResult.OK;
        var btnCancelar = Estilo.Boton("Cancelar");
        btnCancelar.DialogResult = DialogResult.Cancel;

        var botones = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            Anchor = AnchorStyles.Right, // Botones alineados a la derecha
            Margin = new Padding(0, 14, 0, 0)
        };
        botones.Controls.Add(btnAceptar);
        botones.Controls.Add(btnCancelar);

        var panel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(LogicalToDeviceUnits(16))
        };
        panel.Controls.Add(new Label
        {
            Text = tarea.Titulo,
            AutoSize = true,
            MaximumSize = new Size(ancho, 0),
            Font = Estilo.Fuente(11F, FontStyle.Bold)
        });
        panel.Controls.Add(new Label { Text = $"Estado actual: {tarea.Estado}", AutoSize = true, Margin = new Padding(3, 4, 3, 10) });
        panel.Controls.Add(new Label { Text = "Nuevo estado", AutoSize = true });
        panel.Controls.Add(_cmbEstado);
        panel.Controls.Add(botones);
        Controls.Add(panel);

        AcceptButton = btnAceptar;
        CancelButton = btnCancelar;
    }
}
