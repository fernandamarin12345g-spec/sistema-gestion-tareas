namespace Escritorio;

/// <summary>Colores y botones compartidos por todas las ventanas (los mismos tonos de la web y el móvil).</summary>
public static class Estilo
{
    public static readonly Color Principal = Color.FromArgb(15, 118, 110);   // Verde azulado
    public static readonly Color Marca = Color.FromArgb(14, 165, 233);       // Azul "ESA SAS"
    public static readonly Color Error = Color.FromArgb(220, 38, 38);
    public static readonly Color FondoAtrasada = Color.FromArgb(254, 226, 226);
    public static readonly Color TextoAtrasada = Color.FromArgb(153, 27, 27);
    public static readonly Color FondoSeleccion = Color.FromArgb(204, 251, 241);

    public static Font Fuente(float tamano = 10F, FontStyle estilo = FontStyle.Regular) => new("Segoe UI", tamano, estilo);

    /// <summary>Botón principal (fondo de color).</summary>
    public static Button BotonPrincipal(string texto)
    {
        var boton = new Button
        {
            Text = texto,
            AutoSize = true,
            BackColor = Principal,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Padding = new Padding(10, 4, 10, 4),
            Cursor = Cursors.Hand
        };
        boton.FlatAppearance.BorderSize = 0;
        return boton;
    }

    /// <summary>Botón secundario (estilo normal de Windows).</summary>
    public static Button Boton(string texto) => new()
    {
        Text = texto,
        AutoSize = true,
        Padding = new Padding(10, 4, 10, 4),
        Cursor = Cursors.Hand
    };

    /// <summary>Etiqueta alineada con los cuadros de texto y listas de una fila.</summary>
    public static Label Etiqueta(string texto) => new()
    {
        Text = texto,
        AutoSize = true,
        Anchor = AnchorStyles.Left,
        Margin = new Padding(3, 9, 3, 3)
    };

    public static ComboBox Lista() => new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList
    };
}
