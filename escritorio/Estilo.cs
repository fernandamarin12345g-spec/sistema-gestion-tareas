using System.Drawing.Drawing2D;

namespace Escritorio;

/// <summary>
/// Colores, fuentes y funciones de dibujo compartidas por toda la app.
/// Los colores son los mismos códigos de src/styles.css, para que el escritorio se vea igual que la web.
/// </summary>
public static class Estilo
{
    // ---------------------------------------------------------------------
    // Colores (mismos códigos hexadecimales de styles.css)
    // ---------------------------------------------------------------------

    // Barra lateral (header de la web)
    public static readonly Color Lateral = Hex("#0f172a");
    public static readonly Color LateralHover = Hex("#1e293b");
    public static readonly Color LateralBotonHover = Hex("#334155");
    public static readonly Color Celeste = Hex("#38bdf8");
    public static readonly Color TextoMenu = Hex("#94a3b8");
    public static readonly Color TextoClaro = Hex("#e2e8f0");

    // Contenido
    public static readonly Color FondoContenido = Hex("#f1f5f9");
    public static readonly Color Borde = Hex("#e2e8f0");
    public static readonly Color BordeCampo = Hex("#cbd5e1");
    public static readonly Color Titulo = Hex("#0f172a");
    public static readonly Color Texto = Hex("#334155");
    public static readonly Color TextoSuave = Hex("#64748b");
    public static readonly Color TextoEtiqueta = Hex("#475569");
    public static readonly Color TextoTenue = Hex("#94a3b8");
    public static readonly Color FondoEncabezadoGrid = Hex("#f8fafc");
    public static readonly Color LineaFila = Hex("#f1f5f9");
    public static readonly Color FilaAtrasada = Hex("#fff7f7");
    public static readonly Color TextoAtrasada = Hex("#dc2626");

    // Botones
    public static readonly Color Azul = Hex("#0ea5e9");
    public static readonly Color AzulHover = Hex("#0284c7");
    public static readonly Color Gris = Hex("#64748b");
    public static readonly Color GrisHover = Hex("#475569");
    public static readonly Color Naranja = Hex("#f59e0b");
    public static readonly Color NaranjaHover = Hex("#d97706");
    public static readonly Color Rojo = Hex("#ef4444");
    public static readonly Color RojoHover = Hex("#dc2626");
    public static readonly Color Secundario = Hex("#e2e8f0");
    public static readonly Color SecundarioHover = Hex("#cbd5e1");
    public static readonly Color Cancelar = Hex("#cbd5e1");
    public static readonly Color CancelarHover = Hex("#94a3b8");

    // Badges y avisos
    public static readonly Color EstadoFondo = Hex("#e0f2fe");
    public static readonly Color EstadoTexto = Hex("#0369a1");
    public static readonly Color EstadoBorde = Hex("#bae6fd");
    public static readonly Color AvisoFondo = Hex("#fee2e2");
    public static readonly Color AvisoTexto = Hex("#b91c1c");
    public static readonly Color AvisoBorde = Hex("#fca5a5");

    public static Color Hex(string codigo) => ColorTranslator.FromHtml(codigo);

    /// <summary>Colores de .prioridad-baja, -media, -alta y -critica.</summary>
    public static (Color fondo, Color texto) ColoresPrioridad(string prioridad) => prioridad switch
    {
        "Baja" => (Hex("#f1f5f9"), Hex("#475569")),
        "Alta" => (Hex("#ffedd5"), Hex("#c2410c")),
        "Crítica" => (Hex("#fee2e2"), Hex("#b91c1c")),
        _ => (Hex("#fef9c3"), Hex("#854d0e")) // Media
    };

    /// <summary>Colores de .estado-proyecto-planificacion, -activo, -pausado y -finalizado.</summary>
    public static (Color fondo, Color texto) ColoresEstadoProyecto(string estado) => estado switch
    {
        "Planificación" => (Hex("#ede9fe"), Hex("#6d28d9")),
        "Pausado" => (Hex("#fef9c3"), Hex("#854d0e")),
        "Finalizado" => (Hex("#f1f5f9"), Hex("#475569")),
        _ => (Hex("#dcfce7"), Hex("#15803d")) // Activo
    };

    /// <summary>Mezcla dos colores (para dibujar un botón desactivado más claro).</summary>
    public static Color Mezclar(Color a, Color b, float cantidadDeB) => Color.FromArgb(
        (int)(a.R + (b.R - a.R) * cantidadDeB),
        (int)(a.G + (b.G - a.G) * cantidadDeB),
        (int)(a.B + (b.B - a.B) * cantidadDeB));

    // ---------------------------------------------------------------------
    // Fuentes (Segoe UI, igual que la web)
    // ---------------------------------------------------------------------

    private const string Familia = "Segoe UI";
    public static readonly Font Normal = new(Familia, 10.5F);
    public static readonly Font Negrita = new(Familia, 10.5F, FontStyle.Bold);
    public static readonly Font Cursiva = new(Familia, 10.5F, FontStyle.Italic);
    public static readonly Font Pequena = new(Familia, 9F);
    public static readonly Font PequenaNegrita = new(Familia, 9F, FontStyle.Bold);
    public static readonly Font Menu = new("Segoe UI Semibold", 12F);           // nav a: 1.05rem, peso 600
    public static readonly Font Logo = new(Familia, 18F, FontStyle.Bold);           // 1.5rem bold
    public static readonly Font FuenteH3 = new(Familia, 12F, FontStyle.Bold);
    public static readonly Font FuenteTitulo = new(Familia, 18F, FontStyle.Bold);
    public static readonly Font FuenteSubtitulo = new(Familia, 15F, FontStyle.Bold);
    public static readonly Font TituloPanel = new(Familia, 11F, FontStyle.Bold);
    public static readonly Font Kpi = new(Familia, 26F, FontStyle.Bold);

    // ---------------------------------------------------------------------
    // Escala de la pantalla (125 %, 150 %...)
    // ---------------------------------------------------------------------

    private static float? _escala;

    /// <summary>1 con la pantalla al 100 %, 1.5 al 150 %, etc.</summary>
    public static float Escala
    {
        get
        {
            if (_escala == null)
            {
                using Graphics g = Graphics.FromHwnd(IntPtr.Zero);
                _escala = g.DpiX / 96f;
            }
            return _escala.Value;
        }
    }

    /// <summary>Convierte píxeles "de diseño" (como en el CSS) a píxeles reales de la pantalla.</summary>
    public static int E(int pixeles) => (int)Math.Round(pixeles * Escala);

    // ---------------------------------------------------------------------
    // Texto
    // ---------------------------------------------------------------------

    public const TextFormatFlags Izquierda = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine
                                            | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding;
    public const TextFormatFlags Derecha = TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine
                                          | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding;
    public const TextFormatFlags Centro = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine
                                         | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding;
    public const TextFormatFlags Parrafo = TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak
                                          | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.TextBoxControl;

    public static void Escribir(Graphics g, string texto, Font fuente, Color color, Rectangle zona, TextFormatFlags formato = Izquierda)
        => TextRenderer.DrawText(g, texto, fuente, zona, color, formato);

    public static Size Medir(string texto, Font fuente)
        => TextRenderer.MeasureText(texto, fuente, Size.Empty, TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);

    /// <summary>Alto de una línea de texto con esa fuente.</summary>
    public static int AltoLinea(Font fuente) => Medir("Ag", fuente).Height;

    /// <summary>Alto que ocupa un párrafo que se parte en varias líneas dentro de un ancho.</summary>
    public static int AltoParrafo(string texto, Font fuente, int ancho)
        => TextRenderer.MeasureText(texto, fuente, new Size(Math.Max(ancho, 1), int.MaxValue), Parrafo).Height;

    // ---------------------------------------------------------------------
    // Figuras
    // ---------------------------------------------------------------------

    /// <summary>Rectángulo con esquinas redondeadas (como border-radius).</summary>
    public static GraphicsPath Redondeado(Rectangle r, int radio)
    {
        var camino = new GraphicsPath();
        int diametro = Math.Min(radio * 2, Math.Min(r.Width, r.Height));
        if (diametro <= 0)
        {
            camino.AddRectangle(r);
            return camino;
        }
        camino.AddArc(r.X, r.Y, diametro, diametro, 180, 90);
        camino.AddArc(r.Right - diametro, r.Y, diametro, diametro, 270, 90);
        camino.AddArc(r.Right - diametro, r.Bottom - diametro, diametro, diametro, 0, 90);
        camino.AddArc(r.X, r.Bottom - diametro, diametro, diametro, 90, 90);
        camino.CloseFigure();
        return camino;
    }

    /// <summary>Rellena un rectángulo redondeado y, si se indica, le dibuja el borde.</summary>
    public static void Rellenar(Graphics g, Rectangle r, int radio, Color fondo, Color? borde = null, float grosorBorde = 1f)
    {
        if (r.Width <= 0 || r.Height <= 0) return;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using (GraphicsPath camino = Redondeado(r, radio))
        using (var brocha = new SolidBrush(fondo))
        {
            g.FillPath(brocha, camino);
        }

        if (borde.HasValue)
        {
            // El borde se dibuja un poco hacia adentro para que no se corte en los lados
            int adentro = (int)(grosorBorde / 2);
            var zonaBorde = new Rectangle(r.X + adentro, r.Y + adentro, r.Width - 1 - adentro * 2, r.Height - 1 - adentro * 2);
            using GraphicsPath caminoBorde = Redondeado(zonaBorde, radio);
            using var lapiz = new Pen(borde.Value, grosorBorde);
            g.DrawPath(lapiz, caminoBorde);
        }
    }

    /// <summary>Dibuja una "píldora" como .badge-estado o .badge-prioridad. Devuelve su ancho.</summary>
    public static int Insignia(Graphics g, string texto, Color fondo, Color colorTexto, Color? borde, int x, int centroY)
    {
        Size medida = Medir(texto, PequenaNegrita);
        var zona = new Rectangle(x, centroY - (medida.Height + E(10)) / 2, medida.Width + E(22), medida.Height + E(10));
        Rellenar(g, zona, zona.Height / 2, fondo, borde);
        Escribir(g, texto, PequenaNegrita, colorTexto, zona, Centro);
        return zona.Width;
    }

    /// <summary>Etiqueta pequeña con esquinas poco redondeadas (.etiqueta-final, .etiqueta-admin, .etiqueta-rol). Devuelve su ancho.</summary>
    public static int Etiqueta(Graphics g, string texto, Color fondo, Color colorTexto, Color borde, int x, int centroY)
    {
        Size medida = Medir(texto, Pequena);
        var zona = new Rectangle(x, centroY - (medida.Height + E(6)) / 2, medida.Width + E(14), medida.Height + E(6));
        Rellenar(g, zona, E(4), fondo, borde);
        Escribir(g, texto, Pequena, colorTexto, zona, Centro);
        return zona.Width;
    }

    // Colores de las etiquetas de la web
    public static readonly (Color fondo, Color texto, Color borde) EtiquetaFinal = (Hex("#dcfce7"), Hex("#15803d"), Hex("#86efac"));
    public static readonly (Color fondo, Color texto, Color borde) EtiquetaAdmin = (Hex("#fef2f2"), Hex("#ef4444"), Hex("#fca5a5"));
    public static readonly (Color fondo, Color texto, Color borde) EtiquetaRol = (Hex("#f1f5f9"), Hex("#475569"), Hex("#cbd5e1"));

    /// <summary>Texto con separación entre letras (letter-spacing), centrado en la zona.</summary>
    public static void EscribirEspaciado(Graphics g, string texto, Font fuente, Color color, Rectangle zona, int espacioLetras)
    {
        int[] anchos = texto.Select(letra => Medir(letra.ToString(), fuente).Width).ToArray();
        int total = anchos.Sum() + espacioLetras * (texto.Length - 1);
        int x = zona.X + (zona.Width - total) / 2;
        for (int i = 0; i < texto.Length; i++)
        {
            Escribir(g, texto[i].ToString(), fuente, color, new Rectangle(x, zona.Y, anchos[i] + E(4), zona.Height));
            x += anchos[i] + espacioLetras;
        }
    }

    /// <summary>Casilla de verificación con el estilo de la web (texto en negrita).</summary>
    public static CheckBox Casilla(string texto) => new()
    {
        Text = texto,
        AutoSize = true,
        Font = Negrita,
        ForeColor = Texto,
        BackColor = Color.White,
        Cursor = Cursors.Hand
    };

    // ---------------------------------------------------------------------
    // Listas desplegables con el estilo de los <select> de la web
    // ---------------------------------------------------------------------

    public static ComboBox Lista(int altoLogico = 42)
    {
        var lista = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle = FlatStyle.Flat,
            DrawMode = DrawMode.OwnerDrawFixed,
            Font = Normal,
            BackColor = Color.White,
            ForeColor = Texto,
            ItemHeight = Math.Max(E(altoLogico) - 6, 16),
            MaxDropDownItems = 10,
            Cursor = Cursors.Hand
        };
        lista.DrawItem += DibujarOpcion;
        return lista;
    }

    private static void DibujarOpcion(object? sender, DrawItemEventArgs e)
    {
        if (sender is not ComboBox lista) return;

        bool resaltada = (e.State & DrawItemState.Selected) != 0 && (e.State & DrawItemState.ComboBoxEdit) == 0;
        using (var fondo = new SolidBrush(resaltada ? EstadoFondo : Color.White))
        {
            e.Graphics.FillRectangle(fondo, e.Bounds);
        }

        if (e.Index >= 0)
        {
            var zona = new Rectangle(e.Bounds.X + E(10), e.Bounds.Y, e.Bounds.Width - E(14), e.Bounds.Height);
            Escribir(e.Graphics, lista.GetItemText(lista.Items[e.Index]) ?? "", lista.Font, resaltada ? EstadoTexto : Texto, zona);
        }
    }

    /// <summary>Selecciona en la lista la opción con ese id (o la primera si no existe).</summary>
    public static void SeleccionarOpcion(ComboBox lista, int? id)
    {
        for (int i = 0; i < lista.Items.Count; i++)
        {
            if (lista.Items[i] is Opcion opcion && opcion.Id == id)
            {
                lista.SelectedIndex = i;
                return;
            }
        }
        if (lista.Items.Count > 0) lista.SelectedIndex = 0;
    }

    /// <summary>Llena una lista con una primera opción fija ("Sin proyecto", "Todos"...) y las demás.</summary>
    public static void LlenarLista(ComboBox lista, IEnumerable<Opcion> opciones)
    {
        int? anterior = (lista.SelectedItem as Opcion)?.Id;
        lista.BeginUpdate();
        lista.Items.Clear();
        foreach (Opcion opcion in opciones)
        {
            lista.Items.Add(opcion);
        }
        lista.EndUpdate();
        SeleccionarOpcion(lista, anterior);
    }
}
