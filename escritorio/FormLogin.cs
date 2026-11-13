namespace Escritorio;

/// <summary>Inicio de sesión con el mismo diseño de la web: fondo oscuro y una tarjeta blanca al centro.</summary>
public class FormLogin : Form
{
    private readonly ApiCliente _api;
    private readonly Tarjeta _tarjeta = new(Estilo.Lateral);
    private readonly Label _titulo = new()
    {
        Text = "ESA SAS",
        Font = new Font("Segoe UI", 22F, FontStyle.Bold),
        ForeColor = Estilo.Azul,
        BackColor = Color.White,
        TextAlign = ContentAlignment.MiddleCenter
    };
    private readonly Label _subtitulo = new()
    {
        Text = "Sistema de Gestión y Seguimiento de Tareas",
        Font = Estilo.Normal,
        ForeColor = Estilo.TextoSuave,
        BackColor = Color.White,
        TextAlign = ContentAlignment.MiddleCenter
    };
    private readonly CajaTexto _correo = new("Correo electrónico", Color.White);
    private readonly CajaTexto _contrasena = new("Contraseña", Color.White);
    private readonly Label _error = new()
    {
        Font = Estilo.Negrita,
        ForeColor = Estilo.TextoAtrasada,
        BackColor = Color.White,
        TextAlign = ContentAlignment.MiddleLeft
    };
    private readonly BotonWeb _btnIngresar = BotonWeb.Azul("Ingresar", Color.White);

    public FormLogin(ApiCliente api, string? mensaje)
    {
        _api = api;

        Text = "Iniciar sesión · Gestión de Tareas";
        Font = Estilo.Normal;
        BackColor = Estilo.Lateral;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(Estilo.E(560), Estilo.E(540));

        _contrasena.Caja.UseSystemPasswordChar = true;
        _error.Text = mensaje ?? "";

        _tarjeta.Controls.AddRange(new Control[] { _titulo, _subtitulo, _correo, _contrasena, _error, _btnIngresar });
        Controls.Add(_tarjeta);
        AcomodarTarjeta();

        // Enter en cualquiera de los dos campos = Ingresar
        _correo.Caja.KeyDown += TeclaPresionada;
        _contrasena.Caja.KeyDown += TeclaPresionada;
        _btnIngresar.Click += async (_, _) => await IngresarAsync();
        Shown += (_, _) => _correo.Caja.Focus();
    }

    /// <summary>Tarjeta de 380 px de ancho (max-width: 380px) centrada en la ventana.</summary>
    private void AcomodarTarjeta()
    {
        int relleno = Estilo.E(25);
        int espacio = Estilo.E(14);
        int ancho = Estilo.E(380);
        int interior = ancho - relleno * 2;
        int y = relleno;

        _titulo.SetBounds(relleno, y, interior, Estilo.E(44));
        y += Estilo.E(44);
        _subtitulo.SetBounds(relleno, y, interior, Estilo.E(26));
        y += Estilo.E(26) + espacio + Estilo.E(6);
        _correo.SetBounds(relleno, y, interior, Estilo.E(46));
        y += Estilo.E(46) + espacio;
        _contrasena.SetBounds(relleno, y, interior, Estilo.E(46));
        y += Estilo.E(46) + espacio;
        _error.SetBounds(relleno, y, interior, Estilo.E(22));
        y += Estilo.E(22) + espacio;
        _btnIngresar.SetBounds(relleno, y, interior, Estilo.E(46));
        y += Estilo.E(46) + relleno;

        _tarjeta.Size = new Size(ancho, y);
        _tarjeta.Location = new Point((ClientSize.Width - ancho) / 2, (ClientSize.Height - y) / 2);
    }

    private async void TeclaPresionada(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter) return;
        e.SuppressKeyPress = true; // Evita el sonido de Windows
        await IngresarAsync();
    }

    /// <summary>POST /api/Auth/login</summary>
    private async Task IngresarAsync()
    {
        if (!_btnIngresar.Enabled) return;
        _error.Text = "";

        if (string.IsNullOrWhiteSpace(_correo.Caja.Text) || _contrasena.Caja.Text.Length == 0)
        {
            _error.Text = "Escribe tu correo y tu contraseña.";
            return;
        }

        _btnIngresar.Enabled = false;
        _btnIngresar.Text = "Ingresando...";
        try
        {
            await _api.IniciarSesionAsync(_correo.Caja.Text.Trim(), _contrasena.Caja.Text);
            DialogResult = DialogResult.OK; // Cierra esta ventana y abre la principal
        }
        catch (ApiException ex)
        {
            _error.Text = ex.Message;
            _contrasena.Caja.SelectAll();
            _contrasena.Caja.Focus();
        }
        finally
        {
            _btnIngresar.Enabled = true;
            _btnIngresar.Text = "Ingresar";
        }
    }
}
