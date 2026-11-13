namespace Escritorio;

/// <summary>Sección Proyectos: el mismo data grid de la web (en el escritorio es de consulta).</summary>
public class VistaProyectos : Pagina
{
    private readonly FormPrincipal _app;
    private readonly Grid<Proyecto> _grid;

    public VistaProyectos(FormPrincipal app)
    {
        _app = app;
        Agregar(new TituloSeccion("Proyectos",
            "Proyectos o contextos a los que pertenecen las tareas. La creación y edición de proyectos se hace desde el dashboard web."), 25);

        // ID | Proyecto | Estado | Tareas | Acciones
        var columnas = new List<ColumnaGrid<Proyecto>>
        {
            new("ID", 0, 60, (g, r, p) => Celdas.Texto(g, r, $"#{p.Id}", fuente: Estilo.Negrita, color: Estilo.Titulo)),
            new("Proyecto", 2.5f, 0, (g, r, p) => Celdas.Texto(g, r, p.Nombre, p.Descripcion)),
            new("Estado", 1f, 0, (g, r, p) =>
            {
                (Color fondo, Color texto) = Estilo.ColoresEstadoProyecto(p.Estado);
                Celdas.Insignia(g, r, p.Estado, fondo, texto);
            }),
            new("Tareas", 1.3f, 0, (g, r, p) => Celdas.Texto(g, r, $"{p.TareasAbiertas} abierta(s) de {p.TotalTareas}"))
        };
        _grid = new Grid<Proyecto>(columnas,
            _ => new[] { new AccionGrid<Proyecto>("Ver tareas", Estilo.Gris, Estilo.GrisHover, p => _app.VerTareasDeProyecto(p.Id)) },
            150, "No hay proyectos registrados.");
        Agregar(_grid, 10);
    }

    public void Refrescar()
    {
        _grid.MostrarDatos(_app.Proyectos);
    }
}
