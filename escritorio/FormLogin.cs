namespace Escritorio;

/// <summary>Ventana de inicio de sesión: POST /api/Auth/login.</summary>
public class FormLogin : Form
{
    private readonly ApiCliente _api;
    private readonly TextBox _txtCorreo = new();
    private readonly TextBox _txtContrasena = new() { UseSystemPasswordChar = true };
    private readonly CheckBox _chkMostrar = new() { Text = "Mostrar contraseña", AutoSize = true };
    private readonly Label _lblError = new() { AutoSize = true, ForeColor = Estilo.Error };
    private readonly Button _btnIngresar = Estilo.BotonPrincipal("Ingresar");

    public FormLogin(ApiCliente api, string? mensaje)
    {
        _api = api;

        Text = "Iniciar sesión · Gestión de Tareas";
        Font = Estilo.Fuente();
        BackColor = Color.White;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;

        int ancho = LogicalToDeviceUnits(300);
        _txtCorreo.Width = ancho;
        _txtContrasena.Width = ancho;
        _btnIngresar.AutoSize = false;
        _btnIngresar.Size = new Size(ancho, LogicalToDeviceUnits(38));
        _btnIngresar.Margin = new Padding(3, 12, 3, 3);
        _lblError.MaximumSize = new Size(ancho, 0);
        _lblError.Text = mensaje ?? "";

        var marca = new Label
        {
            Text = "ESA SAS",
            AutoSize = false,
            Size = new Size(ancho, LogicalToDeviceUnits(44)),
            TextAlign = ContentAlignment.MiddleCenter,
            Font = Estilo.Fuente(20F, FontStyle.Bold),
            ForeColor = Estilo.Marca
        };
        var subtitulo = new Label
        {
            Text = "Sistema de Gestión de Tareas",
            AutoSize = false,
            Size = new Size(ancho, LogicalToDeviceUnits(26)),
            TextAlign = ContentAlignment.MiddleCenter,
            Margin = new Padding(3, 0, 3, 14)
        };

        // Los controles se apilan de arriba hacia abajo
        var panel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(LogicalToDeviceUnits(24))
        };
        panel.Controls.Add(marca);
        panel.Controls.Add(subtitulo);
        panel.Controls.Add(new Label { Text = "Correo electrónico", AutoSize = true });
        panel.Controls.Add(_txtCorreo);
        panel.Controls.Add(new Label { Text = "Contraseña", AutoSize = true, Margin = new Padding(3, 10, 3, 3) });
        panel.Controls.Add(_txtContrasena);
        panel.Controls.Add(_chkMostrar);
        panel.Controls.Add(_lblError);
        panel.Controls.Add(_btnIngresar);
        Controls.Add(panel);

        // Eventos
        AcceptButton = _btnIngresar; // Enter = Ingresar
        _chkMostrar.CheckedChanged += (_, _) => _txtContrasena.UseSystemPasswordChar = !_chkMostrar.Checked;
        _btnIngresar.Click += async (_, _) => await IngresarAsync();
    }

    private async Task IngresarAsync()
    {
        _lblError.Text = "";

        if (string.IsNullOrWhiteSpace(_txtCorreo.Text) || _txtContrasena.Text.Length == 0)
        {
            _lblError.Text = "Escribe tu correo y tu contraseña.";
            return;
        }

        _btnIngresar.Enabled = false;
        _btnIngresar.Text = "Ingresando...";
        try
        {
            await _api.IniciarSesionAsync(_txtCorreo.Text.Trim(), _txtContrasena.Text);
            DialogResult = DialogResult.OK; // Cierra esta ventana y abre la principal
        }
        catch (ApiException ex)
        {
            _lblError.Text = ex.Message;
            _txtContrasena.SelectAll();
            _txtContrasena.Focus();
        }
        finally
        {
            _btnIngresar.Enabled = true;
            _btnIngresar.Text = "Ingresar";
        }
    }
}
