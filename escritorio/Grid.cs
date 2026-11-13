using System.Drawing.Drawing2D;

namespace Escritorio;

/// <summary>Una columna del grid. Fraccion funciona como "fr" en CSS Grid; AnchoFijo como "60px".</summary>
public class ColumnaGrid<T>
{
    public ColumnaGrid(string titulo, float fraccion, int anchoFijo, Action<Graphics, Rectangle, T> dibujar)
    {
        Titulo = titulo;
        Fraccion = fraccion;
        AnchoFijo = anchoFijo;
        Dibujar = dibujar;
    }

    public string Titulo { get; }
    public float Fraccion { get; }
    public int AnchoFijo { get; }
    public Action<Graphics, Rectangle, T> Dibujar { get; }
}

/// <summary>Un botón de la columna "Acciones" (Ver, Editar, Eliminar...).</summary>
public sealed record AccionGrid<T>(string Texto, Color Fondo, Color Hover, Action<T> Ejecutar, Color? ColorTexto = null);

/// <summary>
/// Data grid igual al de la web (.contenedor-datos-grid): tarjeta blanca, encabezado gris claro en mayúsculas,
/// filas separadas por una línea, y la columna Acciones alineada a la derecha.
/// </summary>
public class Grid<T> : ControlDibujado
{
    private readonly List<ColumnaGrid<T>> _columnas;
    private readonly Func<T, IEnumerable<AccionGrid<T>>> _acciones;
    private readonly int _anchoAcciones;
    private readonly string _mensajeVacio;
    private readonly Func<T, bool>? _resaltar;
    private readonly Func<T, int>? _altoFila;
    private List<T> _filas = new();

    /// <param name="anchoAcciones">Ancho de la columna Acciones (px de diseño), como "230px" en el CSS.</param>
    /// <param name="resaltar">Filas que se pintan de rojo claro (tareas atrasadas).</param>
    /// <param name="altoFila">Alto de cada fila en px de diseño, si no todas miden lo mismo.</param>
    public Grid(IEnumerable<ColumnaGrid<T>> columnas, Func<T, IEnumerable<AccionGrid<T>>> acciones, int anchoAcciones,
                string mensajeVacio, Func<T, bool>? resaltar = null, Func<T, int>? altoFila = null)
    {
        _altoFila = altoFila;
        _columnas = columnas.ToList();
        _acciones = acciones;
        _anchoAcciones = anchoAcciones;
        _mensajeVacio = mensajeVacio;
        _resaltar = resaltar;
    }

    private static int AltoEncabezado => Estilo.E(46);
    private static int AltoFila => Estilo.E(68);
    private static int AltoVacio => Estilo.E(64);
    private static int Relleno => Estilo.E(20);   // padding: 12px 20px
    private static int Separacion => Estilo.E(10); // gap: 10px

    public void MostrarDatos(IEnumerable<T> filas)
    {
        _filas = filas.ToList();
        Actualizar();
    }

    private int AltoDe(T dato) => _altoFila == null ? AltoFila : Math.Max(AltoFila, Estilo.E(_altoFila(dato)));

    public override int AltoPara(int ancho) => AltoEncabezado + (_filas.Count == 0 ? AltoVacio : _filas.Sum(AltoDe)) + 1;

    /// <summary>Calcula la posición y el ancho de cada columna, igual que grid-template-columns.</summary>
    private void CalcularColumnas(out int[] posiciones, out int[] anchos)
    {
        int cantidad = _columnas.Count;
        int fijos = _columnas.Sum(c => c.AnchoFijo > 0 ? Estilo.E(c.AnchoFijo) : 0);
        float fracciones = _columnas.Sum(c => c.AnchoFijo > 0 ? 0f : c.Fraccion);
        int disponible = Width - Relleno * 2 - Separacion * cantidad - Estilo.E(_anchoAcciones);
        int flexible = Math.Max(0, disponible - fijos);

        posiciones = new int[cantidad];
        anchos = new int[cantidad];
        int x = Relleno;
        for (int i = 0; i < cantidad; i++)
        {
            ColumnaGrid<T> columna = _columnas[i];
            anchos[i] = columna.AnchoFijo > 0
                ? Estilo.E(columna.AnchoFijo)
                : fracciones > 0 ? (int)(flexible * columna.Fraccion / fracciones) : 0;
            posiciones[i] = x;
            x += anchos[i] + Separacion;
        }
    }

    protected override void Dibujar(Graphics g)
    {
        var tarjeta = new Rectangle(0, 0, Width - 1, Height - 1);
        using GraphicsPath forma = Estilo.Redondeado(tarjeta, Estilo.E(8));
        using (var blanco = new SolidBrush(Color.White))
        {
            g.FillPath(blanco, forma);
        }

        // Todo lo de adentro se recorta a la tarjeta, así las esquinas quedan redondeadas
        g.SetClip(forma);

        // Encabezado
        using (var fondoEncabezado = new SolidBrush(Estilo.FondoEncabezadoGrid))
        {
            g.FillRectangle(fondoEncabezado, 0, 0, Width, AltoEncabezado);
        }
        using (var lineaEncabezado = new Pen(Estilo.Borde, Estilo.E(2)))
        {
            g.DrawLine(lineaEncabezado, 0, AltoEncabezado - Estilo.E(1), Width, AltoEncabezado - Estilo.E(1));
        }

        CalcularColumnas(out int[] posiciones, out int[] anchos);
        for (int i = 0; i < _columnas.Count; i++)
        {
            Estilo.Escribir(g, _columnas[i].Titulo.ToUpperInvariant(), Estilo.PequenaNegrita, Estilo.TextoSuave,
                new Rectangle(posiciones[i], 0, anchos[i], AltoEncabezado));
        }
        Estilo.Escribir(g, "ACCIONES", Estilo.PequenaNegrita, Estilo.TextoSuave,
            new Rectangle(Width - Relleno - Estilo.E(_anchoAcciones), 0, Estilo.E(_anchoAcciones), AltoEncabezado), Estilo.Derecha);

        if (_filas.Count == 0)
        {
            Estilo.Escribir(g, _mensajeVacio, Estilo.Normal, Estilo.Texto,
                new Rectangle(Relleno, AltoEncabezado, Width - Relleno * 2, AltoVacio));
        }

        int y = AltoEncabezado;
        for (int fila = 0; fila < _filas.Count; fila++)
        {
            T dato = _filas[fila];
            int altoFila = AltoDe(dato);

            // Fila atrasada: fondo rojo muy claro (.fila-atrasada)
            if (_resaltar?.Invoke(dato) == true)
            {
                using var fondoFila = new SolidBrush(Estilo.FilaAtrasada);
                g.FillRectangle(fondoFila, 0, y, Width, altoFila);
            }

            // Línea entre filas (la última no lleva)
            if (fila < _filas.Count - 1)
            {
                using var linea = new Pen(Estilo.LineaFila);
                g.DrawLine(linea, 0, y + altoFila - 1, Width, y + altoFila - 1);
            }

            for (int i = 0; i < _columnas.Count; i++)
            {
                _columnas[i].Dibujar(g, new Rectangle(posiciones[i], y, anchos[i], altoFila), dato);
            }

            // Botones de acciones, de derecha a izquierda
            List<AccionGrid<T>> acciones = _acciones(dato).ToList();
            int xBoton = Width - Relleno;
            for (int k = acciones.Count - 1; k >= 0; k--)
            {
                AccionGrid<T> accion = acciones[k];
                int anchoBoton = AnchoBoton(accion.Texto);
                xBoton -= anchoBoton;
                var zona = new Rectangle(xBoton, y + (altoFila - AltoBoton) / 2, anchoBoton, AltoBoton);
                DibujarBoton(g, zona, accion.Texto, accion.Fondo, accion.Hover, accion.ColorTexto ?? Color.White, () => accion.Ejecutar(dato));
                xBoton -= Separacion;
            }
            y += altoFila;
        }

        g.ResetClip();
        using var borde = new Pen(Estilo.Borde);
        g.DrawPath(borde, forma);
    }
}

/// <summary>Formas de dibujar el contenido de una celda.</summary>
public static class Celdas
{
    /// <summary>Texto de una o dos líneas (por ejemplo, el título con el proyecto debajo en gris).</summary>
    public static void Texto(Graphics g, Rectangle celda, string principal, string? secundario = null,
                             Font? fuente = null, Color? color = null, Font? fuenteSecundaria = null, Color? colorSecundario = null)
    {
        Font f1 = fuente ?? Estilo.Normal;
        Color c1 = color ?? Estilo.Texto;
        if (string.IsNullOrEmpty(secundario))
        {
            Estilo.Escribir(g, principal, f1, c1, celda);
            return;
        }

        Font f2 = fuenteSecundaria ?? Estilo.Pequena;
        Color c2 = colorSecundario ?? Estilo.TextoTenue;
        int alto1 = Estilo.AltoLinea(f1);
        int alto2 = Estilo.AltoLinea(f2);
        int separacion = Estilo.E(3);
        int y = celda.Y + (celda.Height - alto1 - alto2 - separacion) / 2;

        Estilo.Escribir(g, principal, f1, c1, new Rectangle(celda.X, y, celda.Width, alto1));
        Estilo.Escribir(g, secundario, f2, c2, new Rectangle(celda.X, y + alto1 + separacion, celda.Width, alto2));
    }

    /// <summary>Texto seguido de etiquetas pequeñas (por ejemplo "Terminada [Cierra la tarea]").</summary>
    public static void TextoConEtiquetas(Graphics g, Rectangle celda, string texto,
                                         IEnumerable<(string texto, (Color fondo, Color texto, Color borde) colores)>? etiquetas)
    {
        int anchoTexto = Math.Min(Estilo.Medir(texto, Estilo.Normal).Width + Estilo.E(2), celda.Width);
        Estilo.Escribir(g, texto, Estilo.Normal, Estilo.Texto, new Rectangle(celda.X, celda.Y, anchoTexto, celda.Height));

        int x = celda.X + anchoTexto + Estilo.E(10);
        foreach ((string textoEtiqueta, (Color fondo, Color colorTexto, Color borde)) in etiquetas ?? Enumerable.Empty<(string, (Color, Color, Color))>())
        {
            x += Estilo.Etiqueta(g, textoEtiqueta, fondo, colorTexto, borde, x, celda.Y + celda.Height / 2) + Estilo.E(8);
        }
    }

    /// <summary>Badge redondeado centrado verticalmente en la celda.</summary>
    public static void Insignia(Graphics g, Rectangle celda, string texto, Color fondo, Color colorTexto, Color? borde = null)
    {
        Estilo.Insignia(g, texto, fondo, colorTexto, borde, celda.X, celda.Y + celda.Height / 2);
    }
}

/// <summary>Botones redondeados para filtrar por estado (.btn-filtro de la web). "Todas" va en azul oscuro.</summary>
public class ChipsEstado : ControlDibujado
{
    private List<string> _nombres = new() { "Todas" };
    private string _seleccionado = "Todas";

    /// <summary>Se dispara cuando el usuario elige otro estado.</summary>
    public event Action<string>? Cambio;

    public string Seleccionado => _seleccionado;

    public void EstablecerEstados(IEnumerable<string> nombres)
    {
        _nombres = new List<string> { "Todas" };
        _nombres.AddRange(nombres);
        if (!_nombres.Contains(_seleccionado)) _seleccionado = "Todas";
        Actualizar();
    }

    public void Seleccionar(string nombre)
    {
        _seleccionado = _nombres.Contains(nombre) ? nombre : "Todas";
        Invalidate();
    }

    /// <summary>Ubica los botones uno al lado del otro y los pasa a la siguiente línea si no caben (flex-wrap).</summary>
    private List<Rectangle> Ubicar(int ancho)
    {
        var zonas = new List<Rectangle>();
        int margen = Estilo.E(4);
        int alto = Estilo.AltoLinea(Estilo.PequenaNegrita) + Estilo.E(18);
        int x = margen;
        int y = margen;
        foreach (string nombre in _nombres)
        {
            int anchoBoton = Estilo.Medir(nombre, Estilo.PequenaNegrita).Width + Estilo.E(30);
            if (x + anchoBoton > ancho - margen && x > margen)
            {
                x = margen;
                y += alto + Estilo.E(10);
            }
            zonas.Add(new Rectangle(x, y, anchoBoton, alto));
            x += anchoBoton + Estilo.E(10);
        }
        return zonas;
    }

    public override int AltoPara(int ancho)
    {
        List<Rectangle> zonas = Ubicar(ancho);
        return zonas.Count == 0 ? 0 : zonas[^1].Bottom + Estilo.E(4);
    }

    protected override void Dibujar(Graphics g)
    {
        List<Rectangle> zonas = Ubicar(Width);
        for (int i = 0; i < _nombres.Count; i++)
        {
            string nombre = _nombres[i];
            Rectangle zona = zonas[i];
            bool esTodas = i == 0;

            bool encima = RegistrarZona(zona, () =>
            {
                _seleccionado = nombre;
                Invalidate();
                Cambio?.Invoke(nombre);
            });

            // El filtro activo lleva un contorno azul separado (outline: 2px solid #0ea5e9; outline-offset: 2px)
            if (nombre == _seleccionado)
            {
                Rectangle contorno = Rectangle.Inflate(zona, Estilo.E(3), Estilo.E(3));
                Estilo.Rellenar(g, contorno, contorno.Height / 2, BackColor, Estilo.Azul, Estilo.E(2));
            }

            Color fondo = esTodas ? (encima ? Estilo.LateralHover : Estilo.Lateral) : (encima ? Estilo.EstadoBorde : Estilo.EstadoFondo);
            Color texto = esTodas ? Color.White : Estilo.EstadoTexto;
            Estilo.Rellenar(g, zona, zona.Height / 2, fondo);
            Estilo.Escribir(g, nombre, Estilo.PequenaNegrita, texto, zona, Estilo.Centro);
        }
    }
}
