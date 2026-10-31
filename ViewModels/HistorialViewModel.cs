using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlantGuard_proyecto.Data;
using PlantGuard_proyecto.Services;

namespace PlantGuard_proyecto.ViewModels;

public partial class HistorialViewModel : ObservableObject
{
    private readonly AnalisisService _analisisService;
    private readonly RiegoService _riegoService;
    private readonly PlantaService _plantaService;
    private readonly FotoService _fotoService;
    private readonly DatabaseService _databaseService;
    private List<EventoHistorial> _todosLosEventos = new();

    [ObservableProperty]
    private ObservableCollection<EventoHistorial> eventos = new();

    [ObservableProperty]
    private string filtroActual = "Todos";

    public IAsyncRelayCommand CargarAsyncCommand { get; }

    public HistorialViewModel(DatabaseService databaseService, AnalisisService analisisService, RiegoService riegoService, PlantaService plantaService, FotoService fotoService)
    {
        _databaseService = databaseService;
        _analisisService = analisisService;
        _riegoService = riegoService;
        _plantaService = plantaService;
        _fotoService = fotoService;
        CargarAsyncCommand = new AsyncRelayCommand(CargarAsync);
    }

    public async Task CargarAsync()
    {
        await _databaseService.InicializarAsync();
        var plantas = await _plantaService.GetPlantasAsync();
        var nombresPorPlanta = plantas.ToDictionary(x => x.IdPlanta, x => x.Nombre);
        var eventosLista = new List<EventoHistorial>();
        var riegos = await _riegoService.GetRiegosAsync();
        foreach (var riego in riegos)
        {
            var detalles = await _riegoService.GetDetallesPorRiegoAsync(riego.IdRiego);
            var nombres = detalles.Select(x => nombresPorPlanta.GetValueOrDefault(x.IdPlanta, "Planta"));
            eventosLista.Add(new EventoHistorial("Riego", "Riego registrado",
                string.Join(", ", nombres.DefaultIfEmpty(riego.Observacion ?? "Sin observación")),
                Fecha(riego.Fecha)));
        }

        var analisis = await _analisisService.GetTodosAsync();
        foreach (var item in analisis)
        {
            var planta = nombresPorPlanta.GetValueOrDefault(item.IdPlanta, "Planta");
            eventosLista.Add(new EventoHistorial("Análisis", "Revisión de salud",
                $"{planta} · {item.Resultado ?? "Sin resultado"}", Fecha(item.Fecha)));
        }

        foreach (var planta in plantas)
        {
            var fotos = await _fotoService.GetFotosPorPlantaAsync(planta.IdPlanta);
            foreach (var foto in fotos)
            {
                eventosLista.Add(new EventoHistorial("Fotos", "Fotografía guardada",
                    $"{planta.Nombre} · {foto.Descripcion ?? foto.Tipo ?? "Foto de planta"}", Fecha(foto.Fecha)));
            }
        }

        _todosLosEventos = eventosLista.OrderByDescending(x => x.Fecha).ToList();
        AplicarFiltro(FiltroActual);
    }

    public void AplicarFiltro(string filtro)
    {
        FiltroActual = filtro;
        var filtrados = filtro == "Todos"
            ? _todosLosEventos
            : _todosLosEventos.Where(x => x.Tipo == filtro).ToList();
        Eventos = new ObservableCollection<EventoHistorial>(filtrados);
    }

    private static DateTime Fecha(string? valor) =>
        DateTime.TryParse(valor, out var fecha) ? fecha : DateTime.MinValue;
}

public sealed record EventoHistorial(string Tipo, string Titulo, string Descripcion, DateTime Fecha)
{
    public string FechaTexto => Fecha == DateTime.MinValue ? "Fecha sin registrar" : Fecha.ToString("d 'de' MMM · HH:mm");
}
