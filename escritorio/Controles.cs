using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace Escritorio;

// Controles visuales propios que imitan los elementos de la web:
// botones redondeados, campos de texto, tarjetas, enlaces del menú y páginas con scroll.

/// <summary>Un control que sabe qué alto necesita según el ancho disponible (para apilarlo en una Pagina).</summary>
public interface IAltoSegunAncho
{
    int AltoPara(int ancho);
}

/// <summary>Botón redondeado con color al pasar el mouse (como button, .btn-ver, .btn-editar... de la web).</summary>
public class BotonWeb : Control
{
    private readonly Color _fondo;
    private readonly Color _hover;
    private readonly Color _colorTexto;
    private bool _encima;

    public BotonWeb(string texto, Color fondo, Color hover, Color colorTexto, Color fondoContenedor)
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
                 | ControlStyles.ResizeRedraw, true);
        _fondo = fondo;
        _hover = hover;
        _colorTexto = colorTexto;
        BackColor = fondoContenedor;
        Font = Estilo.Negrita;
        Text = texto;
        Cursor = Cursors.Hand;
        AjustarTamano();
    }

    // Los mismos tipos de botón de styles.css
    public static BotonWeb Azul(string texto, Color contenedor) => new(texto, Estilo.Azul, Estilo.AzulHover, Color.White, contenedor);
    public static BotonWeb Editar(string texto, Color contenedor) => new(texto, Estilo.Naranja, Estilo.NaranjaHover, Color.White, contenedor);
    public static BotonWeb Eliminar(string texto, Color contenedor) => new(texto, Estilo.Rojo, Estilo.RojoHover, Color.White, contenedor);
    public static BotonWeb Cancelar(string texto, Color contenedor) => new(texto, Estilo.Cancelar, Estilo.CancelarHover, Estilo.Texto, contenedor);
    public static BotonWeb Secundario(string texto, Color contenedor) => new(texto, Estilo.Secundario, Estilo.SecundarioHover, Estilo.TextoEtiqueta, contenedor);

    /// <summary>Ajusta el tamaño al texto (padding como en el CSS).</summary>
    public void AjustarTamano()
    {
        Size medida = Estilo.Medir(Text, Font);
        Size = new Size(medida.Width + Estilo.E(40), medida.Height + Estilo.E(22));
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _encima = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _encima = false;
        Invalidate();
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);
        Invalidate();
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.Clear(BackColor);
        Color fondo = !Enabled ? Estilo.Mezclar(_fondo, BackColor, 0.4f) : _encima ? _hover : _fondo;
        Estilo.Rellenar(g, ClientRectangle, Estilo.E(6), fondo);
        Estilo.Escribir(g, Text, Font, _colorTexto, ClientRectangle, Estilo.Centro);
    }
}

/// <summary>Campo de texto con borde redondeado y borde celeste al enfocarlo (como input y textarea de la web).</summary>
public class CajaTexto : Control
{
    private readonly bool _multilinea;

    /// <summary>El TextBox real que está adentro.</summary>
    public TextBox Caja { get; } = new()
    {
        BorderStyle = BorderStyle.None,
        Font = Estilo.Normal,
        BackColor = Color.White,
        ForeColor = Estilo.Texto
    };

    public CajaTexto(string marcador, Color fondoContenedor, bool multilinea = false, int altoLogico = 44)
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
                 | ControlStyles.ResizeRedraw, true);
        BackColor = fondoContenedor;
        Cursor = Cursors.IBeam;
        _multilinea = multilinea;

        Caja.PlaceholderText = marcador;
        Caja.Multiline = multilinea;
        if (multilinea) Caja.ScrollBars = ScrollBars.Vertical;
        Caja.GotFocus += (_, _) => Invalidate();
        Caja.LostFocus += (_, _) => Invalidate();
        Controls.Add(Caja);

        Height = Estilo.E(altoLogico);
    }

    /// <summary>Espacio reservado a la derecha (para un ícono).</summary>
    protected virtual int EspacioDerecho => 0;

    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);
        int x = Estilo.E(12);
        int ancho = Math.Max(10, Width - x * 2 - EspacioDerecho);
        if (_multilinea)
        {
            Caja.SetBounds(x, Estilo.E(10), ancho, Math.Max(10, Height - Estilo.E(20)));
        }
        else
        {
            Caja.SetBounds(x, (Height - Caja.Height) / 2, ancho, Caja.Height);
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        Caja.Focus();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(BackColor);
        bool enfocado = Caja.Focused;
        Estilo.Rellenar(e.Graphics, ClientRectangle, Estilo.E(6), Color.White,
            enfocado ? Estilo.Celeste : Estilo.BordeCampo, enfocado ? Estilo.E(2) : 1f);
    }
}

/// <summary>Campo de fecha: muestra dd/mm/aaaa y abre un calendario al hacer clic (como input type="date").</summary>
public class CampoFecha : CajaTexto
{
    private readonly MonthCalendar _calendario = new() { MaxSelectionCount = 1 };
    private readonly ToolStripDropDown _desplegable = new() { Padding = Padding.Empty };
    private DateTime? _fecha;

    public CampoFecha(Color fondoContenedor) : base("dd/mm/aaaa", fondoContenedor)
    {
        Caja.ReadOnly = true;
        Caja.BackColor = Color.White;
        Caja.Cursor = Cursors.Hand;
        Cursor = Cursors.Hand;

        _desplegable.Items.Add(new ToolStripControlHost(_calendario) { Margin = Padding.Empty, Padding = Padding.Empty });
        _calendario.DateSelected += (_, e) =>
        {
            Establecer(e.Start);
            _desplegable.Close();
        };
        Caja.Click += (_, _) => Abrir();
    }

    /// <summary>La fecha elegida, o null si el campo está vacío.</summary>
    public DateTime? Fecha => _fecha;

    public void Establecer(DateTime? fecha)
    {
        _fecha = fecha?.Date;
        Caja.Text = _fecha?.ToString("dd/MM/yyyy") ?? "";
        Invalidate();
    }

    protected override int EspacioDerecho => Estilo.E(28);

    private Rectangle ZonaIcono => new(Width - Estilo.E(34), 0, Estilo.E(30), Height);

    private void Abrir()
    {
        if (_fecha.HasValue) _calendario.SetDate(_fecha.Value);
        _desplegable.Show(this, new Point(0, Height));
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        // Con una fecha puesta, la "×" la borra; en cualquier otra parte se abre el calendario
        if (_fecha.HasValue && ZonaIcono.Contains(e.Location))
        {
            Establecer(null);
            return;
        }
        Abrir();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        string icono = _fecha.HasValue ? "×" : "▾";
        Estilo.Escribir(e.Graphics, icono, Estilo.Negrita, Estilo.TextoSuave, ZonaIcono, Estilo.Centro);
    }
}

/// <summary>Tarjeta blanca con esquinas redondeadas y borde gris (como form y .tarjeta-detalle).</summary>
public class Tarjeta : Panel
{
    public Tarjeta(Color fondoContenedor)
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
                 | ControlStyles.ResizeRedraw, true);
        BackColor = fondoContenedor;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(BackColor);
        Estilo.Rellenar(e.Graphics, ClientRectangle, Estilo.E(8), Color.White, Estilo.Borde);

        // Borde gris alrededor de las listas desplegables (como los <select> de la web)
        foreach (Control control in Controls)
        {
            if (control is ComboBox && control.Visible)
            {
                Rectangle marco = Rectangle.Inflate(control.Bounds, Estilo.E(1), Estilo.E(1));
                Estilo.Rellenar(e.Graphics, marco, Estilo.E(6), Color.White, Estilo.BordeCampo);
            }
        }
        base.OnPaint(e);
    }
}

/// <summary>Enlace del menú lateral (nav a de la web): gris, y celeste con fondo al pasar el mouse o si está activo.</summary>
public class ItemMenu : Control
{
    private bool _encima;
    private bool _activo;

    public ItemMenu(string texto)
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
                 | ControlStyles.ResizeRedraw, true);
        Text = texto;
        Font = Estilo.Menu;
        BackColor = Estilo.Lateral;
        Cursor = Cursors.Hand;
        Height = Estilo.E(46);
    }

    public void MarcarActivo(bool activo)
    {
        _activo = activo;
        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _encima = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _encima = false;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.Clear(BackColor);
        bool resaltado = _activo || _encima;
        int corrimiento = resaltado ? Estilo.E(5) : 0; // transform: translateX(5px)

        if (resaltado)
        {
            Estilo.Rellenar(g, new Rectangle(corrimiento, 0, Width - corrimiento, Height), Estilo.E(8), Estilo.LateralHover);
        }
        var zonaTexto = new Rectangle(corrimiento + Estilo.E(15), 0, Width - corrimiento - Estilo.E(15), Height);
        Estilo.Escribir(g, Text, Font, resaltado ? Estilo.Celeste : Estilo.TextoMenu, zonaTexto);
    }
}

/// <summary>
/// Control que se dibuja a sí mismo (encabezados, grids, gráficos).
/// Permite registrar "zonas" clicables, por ejemplo botones dibujados dentro de una fila.
/// </summary>
public abstract class ControlDibujado : Control, IAltoSegunAncho
{
    private readonly List<(Rectangle zona, Action accion)> _zonas = new();
    private Point _mouse = new(-1, -1);
    private int _zonaEncima = -1;

    protected ControlDibujado()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
                 | ControlStyles.ResizeRedraw, true);
        // No toma el foco al hacer clic: así la página no "salta" al tocar el grid
        SetStyle(ControlStyles.Selectable, false);
        BackColor = Estilo.FondoContenido;
        Font = Estilo.Normal;
    }

    public abstract int AltoPara(int ancho);

    protected abstract void Dibujar(Graphics g);

    protected override void OnPaint(PaintEventArgs e)
    {
        _zonas.Clear();
        e.Graphics.Clear(BackColor);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        Dibujar(e.Graphics);
    }

    /// <summary>Registra una zona clicable. Devuelve true si el mouse está encima (para pintarla con el color hover).</summary>
    protected bool RegistrarZona(Rectangle zona, Action accion)
    {
        _zonas.Add((zona, accion));
        return zona.Contains(_mouse);
    }

    /// <summary>Dibuja un botón redondeado que ejecuta la acción al hacer clic.</summary>
    protected void DibujarBoton(Graphics g, Rectangle zona, string texto, Color fondo, Color hover, Color colorTexto, Action accion)
    {
        bool encima = RegistrarZona(zona, accion);
        Estilo.Rellenar(g, zona, Estilo.E(6), encima ? hover : fondo);
        Estilo.Escribir(g, texto, Estilo.PequenaNegrita, colorTexto, zona, Estilo.Centro);
    }

    /// <summary>Ancho de un botón dibujado (padding: 8px 15px).</summary>
    protected static int AnchoBoton(string texto) => Estilo.Medir(texto, Estilo.PequenaNegrita).Width + Estilo.E(30);

    protected static int AltoBoton => Estilo.E(34);

    /// <summary>Cuando cambian los datos: la página recalcula el alto y se vuelve a dibujar.</summary>
    protected void Actualizar()
    {
        // Se le avisa a la Pagina que contiene este control (puede estar dentro del lienzo)
        Control? contenedor = Parent;
        while (contenedor != null && contenedor is not Pagina) contenedor = contenedor.Parent;
        (contenedor ?? Parent)?.PerformLayout();
        Invalidate();
    }

    private int ZonaEn(Point punto)
    {
        for (int i = _zonas.Count - 1; i >= 0; i--)
        {
            if (_zonas[i].zona.Contains(punto)) return i;
        }
        return -1;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        _mouse = e.Location;
        int zona = ZonaEn(e.Location);
        Cursor = zona >= 0 ? Cursors.Hand : Cursors.Default;
        if (zona != _zonaEncima)
        {
            _zonaEncima = zona;
            Invalidate();
        }
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _mouse = new Point(-1, -1);
        Cursor = Cursors.Default;
        if (_zonaEncima != -1)
        {
            _zonaEncima = -1;
            Invalidate();
        }
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        if (e.Button != MouseButtons.Left) return;
        int zona = ZonaEn(e.Location);
        if (zona >= 0) _zonas[zona].accion();
    }
}

/// <summary>Título y subtítulo de una sección (h2 y p de la web).</summary>
public class TituloSeccion : ControlDibujado
{
    private string _titulo;
    private readonly string? _subtitulo;
    private readonly Font _fuente;

    public TituloSeccion(string titulo, string? subtitulo = null, Font? fuente = null)
    {
        _titulo = titulo;
        _subtitulo = subtitulo;
        _fuente = fuente ?? Estilo.FuenteTitulo;
    }

    public void CambiarTitulo(string titulo)
    {
        _titulo = titulo;
        Actualizar();
    }

    public override int AltoPara(int ancho)
    {
        int alto = Estilo.AltoParrafo(_titulo, _fuente, ancho);
        if (_subtitulo != null) alto += Estilo.E(8) + Estilo.AltoParrafo(_subtitulo, Estilo.Normal, ancho);
        return alto;
    }

    protected override void Dibujar(Graphics g)
    {
        int altoTitulo = Estilo.AltoParrafo(_titulo, _fuente, Width);
        Estilo.Escribir(g, _titulo, _fuente, Estilo.Titulo, new Rectangle(0, 0, Width, altoTitulo), Estilo.Parrafo);
        if (_subtitulo != null)
        {
            int y = altoTitulo + Estilo.E(8);
            Estilo.Escribir(g, _subtitulo, Estilo.Normal, Estilo.TextoSuave, new Rectangle(0, y, Width, Height - y), Estilo.Parrafo);
        }
    }
}

/// <summary>
/// Área de contenido con scroll (main de la web). Apila sus bloques de arriba hacia abajo,
/// cada uno con el ancho disponible (o un ancho máximo, como max-width en CSS).
/// El contenido va en un "lienzo" que se mueve hacia arriba o abajo con una barra de desplazamiento propia.
/// </summary>
public class Pagina : Panel
{
    private sealed class Bloque
    {
        public required Control Control { get; init; }
        public int Espacio { get; init; }
        public int AnchoMaximo { get; init; }
        public bool AnchoPropio { get; init; }
        public bool Visible { get; set; } = true;
    }

    private readonly List<Bloque> _bloques = new();
    private readonly Panel _lienzo = new() { BackColor = Estilo.FondoContenido };
    private readonly VScrollBar _barra = new() { Dock = DockStyle.Right, Visible = false };
    private int _altoContenido;
    private bool _acomodando;

    public Pagina()
    {
        BackColor = Estilo.FondoContenido;
        Dock = DockStyle.Fill;
        DoubleBuffered = true;
        Controls.Add(_lienzo);
        Controls.Add(_barra);
        _barra.ValueChanged += (_, _) => _lienzo.Top = -_barra.Value;
    }

    /// <param name="espacioDespues">Separación con el siguiente bloque (px de diseño).</param>
    /// <param name="anchoMaximo">Ancho máximo (px de diseño); 0 = todo el ancho.</param>
    /// <param name="anchoPropio">true = el bloque conserva su propio tamaño (por ejemplo un botón).</param>
    public T Agregar<T>(T control, int espacioDespues = 20, int anchoMaximo = 0, bool anchoPropio = false) where T : Control
    {
        _bloques.Add(new Bloque
        {
            Control = control,
            Espacio = Estilo.E(espacioDespues),
            AnchoMaximo = anchoMaximo > 0 ? Estilo.E(anchoMaximo) : 0,
            AnchoPropio = anchoPropio
        });
        _lienzo.Controls.Add(control);
        return control;
    }

    /// <summary>Muestra u oculta un bloque (por ejemplo, el formulario solo para quien gestiona tareas).</summary>
    public void MostrarBloque(Control control, bool visible)
    {
        Bloque? bloque = _bloques.Find(b => b.Control == control);
        if (bloque == null) return;
        bloque.Visible = visible;
        control.Visible = visible;
        PerformLayout();
    }

    public void IrArriba() => MoverA(0);

    /// <summary>Desplaza la página con la rueda del mouse (delta = 120 por cada "clic" de la rueda).</summary>
    public void Desplazar(int delta)
    {
        int lineas = Math.Max(1, SystemInformation.MouseWheelScrollLines);
        int pixeles = (int)(delta / 120.0 * lineas * Estilo.E(22));
        MoverA(_barra.Value - pixeles);
    }

    private int DesplazamientoMaximo => Math.Max(0, _altoContenido - ClientSize.Height);

    private void MoverA(int posicion)
    {
        int valor = Math.Clamp(posicion, 0, DesplazamientoMaximo);
        if (_barra.Value != valor) _barra.Value = valor;
        _lienzo.Top = -valor;
    }

    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);
        if (_acomodando) return;
        _acomodando = true;
        try
        {
            // Se acomoda una vez; si aparece o desaparece la barra cambia el ancho, y se acomoda otra vez
            for (int intento = 0; intento < 2; intento++)
            {
                int anchoVisible = ClientSize.Width - (_barra.Visible ? _barra.Width : 0);
                _altoContenido = Acomodar(anchoVisible);
                bool necesitaBarra = _altoContenido > ClientSize.Height;
                if (necesitaBarra == _barra.Visible) break;
                _barra.Visible = necesitaBarra;
            }

            int anchoLienzo = ClientSize.Width - (_barra.Visible ? _barra.Width : 0);
            _lienzo.SetBounds(0, _lienzo.Top, Math.Max(1, anchoLienzo), Math.Max(_altoContenido, ClientSize.Height));

            // Barra de desplazamiento: el valor máximo alcanzable es Maximum - LargeChange + 1
            _barra.Minimum = 0;
            _barra.SmallChange = Estilo.E(40);
            _barra.LargeChange = Math.Max(1, ClientSize.Height);
            _barra.Maximum = Math.Max(0, _altoContenido - 1);
            MoverA(_barra.Value);
        }
        finally
        {
            _acomodando = false;
        }
    }

    /// <summary>Ubica los bloques uno debajo del otro y devuelve el alto total.</summary>
    private int Acomodar(int anchoVisible)
    {
        int margenX = Estilo.E(56);   // main { padding: 3rem 4rem }
        int margenY = Estilo.E(44);
        int anchoDisponible = Math.Max(Estilo.E(320), anchoVisible - margenX * 2);
        int y = margenY;

        foreach (Bloque bloque in _bloques)
        {
            if (!bloque.Visible) continue;
            Control control = bloque.Control;

            int ancho = bloque.AnchoPropio ? control.Width
                : bloque.AnchoMaximo > 0 ? Math.Min(anchoDisponible, bloque.AnchoMaximo)
                : anchoDisponible;

            int alto = control is IAltoSegunAncho medible ? medible.AltoPara(ancho)
                : bloque.AnchoPropio ? control.Height
                : control.GetPreferredSize(new Size(ancho, 0)).Height;

            control.SetBounds(margenX, y, ancho, alto);
            y += alto + bloque.Espacio;
        }
        return y + margenY;
    }
}

/// <summary>
/// Formulario sencillo en tarjeta (como los form de Estados, Roles y Usuarios en la web):
/// un campo debajo del otro y abajo los botones Agregar / Cancelar.
/// </summary>
public class FormularioSimple : Tarjeta, IAltoSegunAncho
{
    private readonly List<Control> _campos;
    private readonly string _textoCrear;
    private string _error = "";

    public FormularioSimple(string textoCrear, params Control[] campos) : base(Estilo.FondoContenido)
    {
        _textoCrear = textoCrear;
        _campos = campos.ToList();
        BtnGuardar = BotonWeb.Azul(textoCrear, Color.White);
        BtnCancelar = BotonWeb.Cancelar("Cancelar", Color.White);
        BtnCancelar.Visible = false;

        Controls.AddRange(campos);
        Controls.Add(BtnGuardar);
        Controls.Add(BtnCancelar);
    }

    public BotonWeb BtnGuardar { get; }
    public BotonWeb BtnCancelar { get; }

    private static int Relleno => Estilo.E(25);
    private static int Espacio => Estilo.E(15);

    private static int AltoDe(Control campo) => campo is CheckBox ? campo.GetPreferredSize(Size.Empty).Height : campo.Height;

    public int AltoPara(int ancho) => Relleno + _campos.Sum(c => AltoDe(c) + Espacio) + BtnGuardar.Height + Relleno;

    private bool _acomodando;

    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);
        if (_acomodando) return;
        _acomodando = true;
        try
        {
            int y = Relleno;
            int ancho = Width - Relleno * 2;
            foreach (Control campo in _campos)
            {
                int alto = AltoDe(campo);
                if (campo is CheckBox)
                {
                    // Una casilla con AutoSize decide su propio tamaño: solo se mueve.
                    // (Si se le fuerza un ancho, ella lo cambia de vuelta y el acomodo se repite sin fin.)
                    campo.Location = new Point(Relleno, y);
                }
                else
                {
                    campo.SetBounds(Relleno, y, ancho, alto);
                }
                y += alto + Espacio;
            }
            BtnGuardar.Location = new Point(Relleno, y);
            BtnCancelar.Location = new Point(BtnGuardar.Right + Estilo.E(10), y);
            Invalidate();
        }
        finally
        {
            _acomodando = false;
        }
    }

    /// <summary>Cambia entre "Agregar ..." y "Actualizar ..." (con el botón Cancelar visible).</summary>
    public void ModoEdicion(bool editando, string textoEditar)
    {
        BtnGuardar.Text = editando ? textoEditar : _textoCrear;
        BtnGuardar.AjustarTamano();
        BtnCancelar.Visible = editando;
        _error = "";
        PerformLayout();
    }

    public void MostrarError(string mensaje)
    {
        _error = mensaje;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (_error.Length == 0) return;
        int x = (BtnCancelar.Visible ? BtnCancelar.Right : BtnGuardar.Right) + Estilo.E(15);
        Estilo.Escribir(e.Graphics, _error, Estilo.PequenaNegrita, Estilo.TextoAtrasada,
            new Rectangle(x, BtnGuardar.Top, Math.Max(0, Width - x - Relleno), BtnGuardar.Height));
    }
}

/// <summary>
/// Hace que la rueda del mouse desplace la página que está debajo del puntero,
/// aunque el foco esté en una lista desplegable o en otro control.
/// </summary>
public sealed class DesplazamientoConRueda : IMessageFilter
{
    private const int WM_MOUSEWHEEL = 0x020A;

    [DllImport("user32.dll")]
    private static extern IntPtr WindowFromPoint(Point punto);

    public bool PreFilterMessage(ref Message m)
    {
        if (m.Msg != WM_MOUSEWHEEL) return false;

        Control? debajo = Control.FromChildHandle(WindowFromPoint(Cursor.Position));
        if (debajo == null) return false;                                       // Por ejemplo, una lista desplegable abierta
        if (debajo is ComboBox { DroppedDown: true }) return false;
        if (debajo is TextBox { Multiline: true } && debajo.Focused) return false; // La descripción tiene su propio scroll

        Control? actual = debajo;
        while (actual != null && actual is not Pagina) actual = actual.Parent;
        if (actual is not Pagina pagina) return false;

        int delta = (short)(((long)m.WParam >> 16) & 0xFFFF);
        pagina.Desplazar(delta);
        return true; // El mensaje ya se usó: no llega al control (así una lista no cambia de opción por accidente)
    }
}
