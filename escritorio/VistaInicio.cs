namespace Escritorio;

/// <summary>Sección Inicio: bienvenida y dashboard (section#home de la web).</summary>
public class VistaInicio : Pagina
{
    private readonly FormPrincipal _app;
    private readonly Dashboard _dashboard;

    public VistaInicio(FormPrincipal app)
    {
        _app = app;
        Agregar(new TituloSeccion("Sistema de Gestión y Seguimiento de Tareas",
            "Plataforma centralizada para organizar responsabilidades, prioridades y estados de equipos de desarrollo."), 30);
        _dashboard = new Dashboard(app);
        Agregar(_dashboard, 10);
    }

    public void Refrescar()
    {
        _dashboard.Mostrar(_app.Tareas, _app.Estados);
    }
}

/// <summary>
/// Indicadores (tarjetas KPI) y los cuatro paneles del dashboard: por estado, abiertas por responsable,
/// por proyecto y la lista de tareas atrasadas. Mismos cálculos que renderizarDashboard() de la web.
/// </summary>
public class Dashboard : ControlDibujado
{
    private readonly FormPrincipal _app;
    private int _total, _abiertas, _cerradas;
    private List<Tarea> _atrasadas = new();
    private List<(string nombre, int valor)> _porEstado = new();
    private List<(string nombre, int valor)> _porResponsable = new();
    private List<(string nombre, int valor)> _porProyecto = new();

    public Dashboard(FormPrincipal app)
    {
        _app = app;
    }

    private static int Espacio => Estilo.E(20);
    private static int AltoKpi => Estilo.E(104);
    private static int AltoFilaBarra => Estilo.E(32);
    private static int AltoFilaAtrasada => Estilo.E(60);
    private static int EncabezadoPanel => Estilo.E(56); // padding + título h3

    public void Mostrar(IReadOnlyList<Tarea> tareas, IReadOnlyList<Estado> estados)
    {
        _total = tareas.Count;
        _cerradas = tareas.Count(t => t.EsFinal);
        _abiertas = _total - _cerradas;
        _atrasadas = tareas.Where(t => t.Atrasada).OrderBy(t => t.FechaLimite).ToList();

        // Por estado: se muestran todos los estados, también los que tienen 0 tareas
        _porEstado = estados
            .Select(e => (e.Nombre, tareas.Count(t => t.IdEstado == e.Id)))
            .OrderByDescending(x => x.Item2)
            .ToList();

        // Por responsable: solo tareas abiertas (carga de trabajo actual)
        _porResponsable = ContarPor(tareas.Where(t => !t.EsFinal), t => t.Responsable);
        _porProyecto = ContarPor(tareas, t => t.Proyecto);

        Actualizar();
    }

    private static List<(string nombre, int valor)> ContarPor(IEnumerable<Tarea> tareas, Func<Tarea, string> obtenerNombre) =>
        tareas.GroupBy(obtenerNombre)
              .Select(grupo => (grupo.Key, grupo.Count()))
              .OrderByDescending(x => x.Item2)
              .ToList();

    // Columnas como "repeat(auto-fit, minmax(170px, 1fr))" y "minmax(340px, 1fr)"
    private static int Columnas(int ancho, int minimo, int maximo) =>
        Math.Clamp((ancho + Espacio) / (Estilo.E(minimo) + Espacio), 1, maximo);

    private int AltoPanel(int indice)
    {
        if (indice == 3) return EncabezadoPanel + Math.Max(1, _atrasadas.Count) * AltoFilaAtrasada + Estilo.E(14);
        int filas = indice switch { 0 => _porEstado.Count, 1 => _porResponsable.Count, _ => _porProyecto.Count };
        return EncabezadoPanel + Math.Max(1, filas) * AltoFilaBarra + Estilo.E(20);
    }

    public override int AltoPara(int ancho)
    {
        int columnasKpi = Columnas(ancho, 170, 4);
        int filasKpi = (4 + columnasKpi - 1) / columnasKpi;
        int alto = filasKpi * (AltoKpi + Espacio) + Estilo.E(5);

        int columnasPanel = Columnas(ancho, 340, 4);
        for (int inicio = 0; inicio < 4; inicio += columnasPanel)
        {
            int altoFila = 0;
            for (int i = inicio; i < Math.Min(4, inicio + columnasPanel); i++) altoFila = Math.Max(altoFila, AltoPanel(i));
            alto += altoFila + Espacio;
        }
        return alto;
    }

    protected override void Dibujar(Graphics g)
    {
        // ---- Tarjetas KPI ----
        (string titulo, int valor, bool alerta)[] kpis =
        {
            ("Total de tareas", _total, false),
            ("Abiertas", _abiertas, false),
            ("Atrasadas", _atrasadas.Count, _atrasadas.Count > 0),
            ("Cerradas", _cerradas, false)
        };
        int columnasKpi = Columnas(Width, 170, 4);
        int anchoKpi = (Width - Espacio * (columnasKpi - 1)) / columnasKpi;
        int y = 0;
        for (int i = 0; i < kpis.Length; i++)
        {
            int columna = i % columnasKpi;
            int fila = i / columnasKpi;
            var zona = new Rectangle(columna * (anchoKpi + Espacio), fila * (AltoKpi + Espacio), anchoKpi, AltoKpi);
            DibujarKpi(g, zona, kpis[i].titulo, kpis[i].valor, kpis[i].alerta);
            y = zona.Bottom + Espacio;
        }
        y += Estilo.E(5);

        // ---- Paneles ----
        int columnasPanel = Columnas(Width, 340, 4);
        int anchoPanel = (Width - Espacio * (columnasPanel - 1)) / columnasPanel;
        for (int inicio = 0; inicio < 4; inicio += columnasPanel)
        {
            int altoFila = 0;
            for (int i = inicio; i < Math.Min(4, inicio + columnasPanel); i++) altoFila = Math.Max(altoFila, AltoPanel(i));

            for (int i = inicio; i < Math.Min(4, inicio + columnasPanel); i++)
            {
                var zona = new Rectangle((i - inicio) * (anchoPanel + Espacio), y, anchoPanel, altoFila);
                switch (i)
                {
                    case 0: DibujarPanelBarras(g, zona, "Tareas por estado", _porEstado); break;
                    case 1: DibujarPanelBarras(g, zona, "Tareas abiertas por responsable", _porResponsable); break;
                    case 2: DibujarPanelBarras(g, zona, "Tareas por proyecto", _porProyecto); break;
                    default: DibujarPanelAtrasadas(g, zona); break;
                }
            }
            y += altoFila + Espacio;
        }
    }

    /// <summary>.tarjeta-kpi (y .kpi-alerta en rojo si hay tareas atrasadas).</summary>
    private static void DibujarKpi(Graphics g, Rectangle zona, string titulo, int valor, bool alerta)
    {
        Estilo.Rellenar(g, zona, Estilo.E(8), alerta ? Estilo.FilaAtrasada : Color.White, alerta ? Estilo.AvisoBorde : Estilo.Borde);
        int x = zona.X + Estilo.E(20);
        int ancho = zona.Width - Estilo.E(40);
        Estilo.Escribir(g, titulo.ToUpperInvariant(), Estilo.PequenaNegrita, alerta ? Estilo.AvisoTexto : Estilo.TextoSuave,
            new Rectangle(x, zona.Y + Estilo.E(18), ancho, Estilo.E(20)));
        Estilo.Escribir(g, valor.ToString(), Estilo.Kpi, Estilo.Titulo,
            new Rectangle(x, zona.Y + Estilo.E(42), ancho, Estilo.E(48)));
    }

    private static void DibujarFondoPanel(Graphics g, Rectangle zona, string titulo)
    {
        Estilo.Rellenar(g, zona, Estilo.E(8), Color.White, Estilo.Borde);
        Estilo.Escribir(g, titulo, Estilo.TituloPanel, Estilo.Titulo,
            new Rectangle(zona.X + Estilo.E(20), zona.Y + Estilo.E(18), zona.Width - Estilo.E(40), Estilo.E(24)));
    }

    /// <summary>Barras horizontales: nombre | barra | cantidad (.fila-barra).</summary>
    private static void DibujarPanelBarras(Graphics g, Rectangle zona, string titulo, List<(string nombre, int valor)> datos)
    {
        DibujarFondoPanel(g, zona, titulo);
        int x = zona.X + Estilo.E(20);
        int ancho = zona.Width - Estilo.E(40);
        int y = zona.Y + EncabezadoPanel;

        if (datos.Count == 0)
        {
            Estilo.Escribir(g, "Sin datos para mostrar.", Estilo.Normal, Estilo.TextoTenue, new Rectangle(x, y, ancho, AltoFilaBarra));
            return;
        }

        int maximo = datos.Max(d => d.valor);
        int anchoNombre = Estilo.E(150);
        int anchoValor = Estilo.E(36);
        int separacion = Estilo.E(10);
        int anchoPista = Math.Max(Estilo.E(20), ancho - anchoNombre - anchoValor - separacion * 2);

        foreach ((string nombre, int valor) in datos)
        {
            Estilo.Escribir(g, nombre, Estilo.Normal, Estilo.Texto, new Rectangle(x, y, anchoNombre, AltoFilaBarra));

            var pista = new Rectangle(x + anchoNombre + separacion, y + (AltoFilaBarra - Estilo.E(10)) / 2, anchoPista, Estilo.E(10));
            Estilo.Rellenar(g, pista, Estilo.E(4), Estilo.LineaFila);
            if (maximo > 0 && valor > 0)
            {
                double porcentaje = Math.Max((double)valor / maximo, 0.03);
                var relleno = new Rectangle(pista.X, pista.Y, Math.Max(Estilo.E(4), (int)(pista.Width * porcentaje)), pista.Height);
                Estilo.Rellenar(g, relleno, Estilo.E(4), Estilo.Azul);
            }

            Estilo.Escribir(g, valor.ToString(), Estilo.Negrita, Estilo.Titulo,
                new Rectangle(pista.Right + separacion, y, anchoValor, AltoFilaBarra), Estilo.Derecha);
            y += AltoFilaBarra;
        }
    }

    /// <summary>Lista de tareas atrasadas con su botón Ver (.fila-atrasada-dashboard).</summary>
    private void DibujarPanelAtrasadas(Graphics g, Rectangle zona)
    {
        DibujarFondoPanel(g, zona, "Tareas atrasadas");
        int x = zona.X + Estilo.E(20);
        int ancho = zona.Width - Estilo.E(40);
        int y = zona.Y + EncabezadoPanel;

        if (_atrasadas.Count == 0)
        {
            Estilo.Escribir(g, "No hay tareas atrasadas.", Estilo.Normal, Estilo.TextoTenue, new Rectangle(x, y, ancho, AltoFilaBarra));
            return;
        }

        for (int i = 0; i < _atrasadas.Count; i++)
        {
            Tarea tarea = _atrasadas[i];
            var fila = new Rectangle(x, y, ancho, AltoFilaAtrasada);

            int anchoVer = AnchoBoton("Ver");
            var zonaVer = new Rectangle(fila.Right - anchoVer, fila.Y + (fila.Height - AltoBoton) / 2, anchoVer, AltoBoton);
            DibujarBoton(g, zonaVer, "Ver", Estilo.Gris, Estilo.GrisHover, Color.White, () => _app.VerDetalle(tarea.Id));

            var texto = new Rectangle(fila.X, fila.Y, fila.Width - anchoVer - Estilo.E(10), fila.Height);
            Celdas.Texto(g, texto, $"#{tarea.Id} {tarea.Titulo}",
                $"{tarea.Responsable} · venció el {Formato.Fecha(tarea.FechaLimite)}", fuente: Estilo.Negrita, color: Estilo.Titulo);

            if (i < _atrasadas.Count - 1)
            {
                using var linea = new Pen(Estilo.LineaFila);
                g.DrawLine(linea, fila.X, fila.Bottom - 1, fila.Right, fila.Bottom - 1);
            }
            y += AltoFilaAtrasada;
        }
    }
}
