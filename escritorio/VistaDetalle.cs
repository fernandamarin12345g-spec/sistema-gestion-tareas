namespace Escritorio;

/// <summary>Vista de detalle de una tarea (section#tarea-detalle de la web).</summary>
public class VistaDetalle : Pagina
{
    private readonly TarjetaDetalle _tarjeta;

    public VistaDetalle(FormPrincipal app)
    {
        var volver = BotonWeb.Secundario("← Volver a tareas", Estilo.FondoContenido);
        volver.Click += (_, _) => app.MostrarSeccion(Seccion.Tareas);
        Agregar(volver, 20, anchoPropio: true);

        _tarjeta = new TarjetaDetalle(app);
        Agregar(_tarjeta, 10, anchoMaximo: 800);
    }

    public void Mostrar(Tarea tarea)
    {
        _tarjeta.Mostrar(tarea);
        IrArriba();
        PerformLayout();
    }
}

/// <summary>Tarjeta con todos los datos de la tarea, el cambio de estado y los botones Editar / Eliminar.</summary>
public class TarjetaDetalle : Tarjeta, IAltoSegunAncho
{
    private readonly FormPrincipal _app;
    private readonly ComboBox _nuevoEstado = Estilo.Lista();
    private readonly BotonWeb _btnGuardarEstado = BotonWeb.Azul("Guardar estado", Color.White);
    private readonly BotonWeb _btnEditar = BotonWeb.Editar("Editar", Color.White);
    private readonly BotonWeb _btnEliminar = BotonWeb.Eliminar("Eliminar", Color.White);

    private Tarea? _tarea;
    private bool _puedeCambiarEstado;

    // Posiciones calculadas en el último acomodo (se usan al dibujar)
    private Rectangle _zonaAviso, _zonaTitulo, _zonaDescripcion;
    private readonly List<Rectangle> _celdas = new();
    private int _yCambioEstado;

    private const string TextoAviso = "Esta tarea está atrasada: su fecha límite ya pasó y todavía no está cerrada.";

    public TarjetaDetalle(FormPrincipal app) : base(Estilo.FondoContenido)
    {
        _app = app;
        _nuevoEstado.Width = Estilo.E(260);
        Controls.AddRange(new Control[] { _nuevoEstado, _btnGuardarEstado, _btnEditar, _btnEliminar });

        _btnGuardarEstado.Click += (_, _) =>
        {
            if (_tarea != null && _nuevoEstado.SelectedItem is Opcion { Id: int idEstado })
                _app.CambiarEstado(_tarea.Id, idEstado);
        };
        _btnEditar.Click += (_, _) =>
        {
            if (_tarea != null) _app.EditarTarea(_tarea);
        };
        _btnEliminar.Click += (_, _) =>
        {
            if (_tarea != null) _app.EliminarTarea(_tarea);
        };
    }

    public void Mostrar(Tarea tarea)
    {
        _tarea = tarea;
        UsuarioSesion usuario = _app.Usuario;

        // Pueden cambiar el estado: Administrador, Líder, o el responsable de la tarea
        _puedeCambiarEstado = usuario.PuedeGestionar || tarea.IdUsuario == usuario.IdUsuario;
        Estilo.LlenarLista(_nuevoEstado, _app.Estados.Select(e => new Opcion(e.Id, e.Nombre)));
        Estilo.SeleccionarOpcion(_nuevoEstado, tarea.IdEstado);

        _nuevoEstado.Visible = _puedeCambiarEstado;
        _btnGuardarEstado.Visible = _puedeCambiarEstado;
        _btnEditar.Visible = usuario.PuedeGestionar;
        _btnEliminar.Visible = usuario.EsAdmin;

        PerformLayout();
        Invalidate();
    }

    private static int Relleno => Estilo.E(30);

    /// <summary>Calcula el alto (y, si aplicar es true, la posición de cada parte).</summary>
    private int Calcular(int ancho, bool aplicar)
    {
        int p = Relleno;
        int interior = Math.Max(Estilo.E(200), ancho - p * 2);
        int y = p;
        if (_tarea == null) return p * 2 + Estilo.E(30);

        if (aplicar) _celdas.Clear();

        // Aviso rojo si está atrasada
        if (_tarea.Atrasada)
        {
            int altoAviso = Estilo.AltoParrafo(TextoAviso, Estilo.Negrita, interior - Estilo.E(30)) + Estilo.E(24);
            if (aplicar) _zonaAviso = new Rectangle(p, y, interior, altoAviso);
            y += altoAviso + Estilo.E(20);
        }

        // Título: "#8 · diseñar crud"
        string titulo = $"#{_tarea.Id} · {_tarea.Titulo}";
        int altoTitulo = Estilo.AltoParrafo(titulo, Estilo.FuenteSubtitulo, interior);
        if (aplicar) _zonaTitulo = new Rectangle(p, y, interior, altoTitulo);
        y += altoTitulo + Estilo.E(10);

        // Descripción
        int altoDescripcion = Estilo.AltoParrafo(TextoDescripcion, FuenteDescripcion, interior);
        if (aplicar) _zonaDescripcion = new Rectangle(p, y, interior, altoDescripcion);
        y += altoDescripcion + Estilo.E(28);

        // Datos en 2 columnas (.detalle-datos: gap 18px 30px)
        int anchoColumna = (interior - Estilo.E(30)) / 2;
        int altoDato = Estilo.E(54);
        for (int i = 0; i < 8; i++)
        {
            int fila = i / 2;
            int columna = i % 2;
            if (aplicar)
                _celdas.Add(new Rectangle(p + columna * (anchoColumna + Estilo.E(30)), y + fila * (altoDato + Estilo.E(18)), anchoColumna, altoDato));
        }
        y += altoDato * 4 + Estilo.E(18) * 3 + Estilo.E(25);

        // Cambiar estado
        if (_puedeCambiarEstado)
        {
            if (aplicar)
            {
                _yCambioEstado = y;
                int yControles = y + Estilo.E(24);
                _nuevoEstado.Location = new Point(p, yControles);
                _btnGuardarEstado.Location = new Point(_nuevoEstado.Right + Estilo.E(10),
                    yControles + (_nuevoEstado.Height - _btnGuardarEstado.Height) / 2);
            }
            y += Estilo.E(24) + Math.Max(_nuevoEstado.Height, _btnGuardarEstado.Height) + Estilo.E(20);
        }

        // Editar / Eliminar
        bool hayBotones = _app.Usuario.PuedeGestionar || _app.Usuario.EsAdmin;
        if (hayBotones)
        {
            if (aplicar)
            {
                int x = p;
                if (_app.Usuario.PuedeGestionar)
                {
                    _btnEditar.Location = new Point(x, y);
                    x = _btnEditar.Right + Estilo.E(10);
                }
                if (_app.Usuario.EsAdmin) _btnEliminar.Location = new Point(x, y);
            }
            y += _btnEditar.Height;
        }
        else
        {
            y -= Estilo.E(20);
        }

        return y + p;
    }

    private string TextoDescripcion => string.IsNullOrWhiteSpace(_tarea?.Descripcion) ? "Sin descripción" : _tarea!.Descripcion!;
    private Font FuenteDescripcion => string.IsNullOrWhiteSpace(_tarea?.Descripcion) ? Estilo.Cursiva : Estilo.Normal;

    public int AltoPara(int ancho) => Calcular(ancho, false);

    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);
        Calcular(Width, true);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (_tarea is not Tarea t) return;
        Graphics g = e.Graphics;

        if (t.Atrasada)
        {
            Estilo.Rellenar(g, _zonaAviso, Estilo.E(6), Estilo.AvisoFondo, Estilo.AvisoBorde);
            Rectangle texto = Rectangle.Inflate(_zonaAviso, -Estilo.E(15), -Estilo.E(12));
            Estilo.Escribir(g, TextoAviso, Estilo.Negrita, Estilo.AvisoTexto, texto, Estilo.Parrafo);
        }

        Estilo.Escribir(g, $"#{t.Id} · {t.Titulo}", Estilo.FuenteSubtitulo, Estilo.Titulo, _zonaTitulo, Estilo.Parrafo);
        Estilo.Escribir(g, TextoDescripcion, FuenteDescripcion, Estilo.TextoSuave, _zonaDescripcion, Estilo.Parrafo);

        // Los 8 datos: etiqueta gris en mayúsculas y el valor debajo
        (string etiqueta, Action<Rectangle> valor)[] datos =
        {
            ("Estado", r => Celdas.Insignia(g, r, t.Estado, Estilo.EstadoFondo, Estilo.EstadoTexto, Estilo.EstadoBorde)),
            ("Prioridad", r =>
            {
                (Color fondo, Color texto) = Estilo.ColoresPrioridad(t.Prioridad);
                Celdas.Insignia(g, r, t.Prioridad, fondo, texto);
            }),
            ("Responsable", r => Celdas.Texto(g, r, t.Responsable, fuente: Estilo.Negrita, color: Estilo.Titulo)),
            ("Proyecto", r => Celdas.Texto(g, r, t.Proyecto, fuente: Estilo.Negrita, color: Estilo.Titulo)),
            ("Fecha de creación", r => Celdas.Texto(g, r, Formato.Fecha(t.FechaCreacion), fuente: Estilo.Negrita, color: Estilo.Titulo)),
            ("Fecha límite", r => Celdas.Texto(g, r, Formato.Fecha(t.FechaLimite), fuente: Estilo.Negrita,
                color: t.Atrasada ? Estilo.TextoAtrasada : Estilo.Titulo)),
            ("Fecha de finalización", r => Celdas.Texto(g, r, Formato.Fecha(t.FechaFinalizacion), fuente: Estilo.Negrita, color: Estilo.Titulo)),
            ("Última actualización", r => Celdas.Texto(g, r, Formato.Fecha(t.FechaActualizacion), fuente: Estilo.Negrita, color: Estilo.Titulo))
        };

        for (int i = 0; i < datos.Length && i < _celdas.Count; i++)
        {
            Rectangle celda = _celdas[i];
            int altoEtiqueta = Estilo.E(18);
            Estilo.Escribir(g, datos[i].etiqueta.ToUpperInvariant(), Estilo.PequenaNegrita, Estilo.TextoTenue,
                new Rectangle(celda.X, celda.Y, celda.Width, altoEtiqueta));
            datos[i].valor(new Rectangle(celda.X, celda.Y + altoEtiqueta + Estilo.E(4), celda.Width, celda.Height - altoEtiqueta - Estilo.E(4)));
        }

        if (_puedeCambiarEstado)
        {
            Estilo.Escribir(g, "Cambiar estado", Estilo.PequenaNegrita, Estilo.TextoEtiqueta,
                new Rectangle(Relleno, _yCambioEstado, Estilo.E(300), Estilo.E(18)));
        }
    }
}
