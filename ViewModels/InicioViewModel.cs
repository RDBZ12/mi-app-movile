using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlantGuard_proyecto.Data;
using PlantGuard_proyecto.Models;
using PlantGuard_proyecto.Services;

namespace PlantGuard_proyecto.ViewModels;

public partial class InicioViewModel : ObservableObject
{
    private readonly PlantaService _plantaService;
    private readonly RiegoService _riegoService;
    private readonly EspecieService _especieService;
    private readonly DatabaseService _databaseService;

    [ObservableProperty]
    private int plantasCount;

    [ObservableProperty]
    private string proximoRiego = "Sin riegos programados";

    [ObservableProperty]
    private string estadoGeneral = "Sin datos";

    [ObservableProperty]
    private ObservableCollection<RiegoPendiente> proximosRiegos = new();

    public IAsyncRelayCommand CargarAsyncCommand { get; }

    public InicioViewModel(PlantaService plantaService, RiegoService riegoService, EspecieService especieService, DatabaseService databaseService)
    {
        _plantaService = plantaService;
        _riegoService = riegoService;
        _especieService = especieService;
        _databaseService = databaseService;
        CargarAsyncCommand = new AsyncRelayCommand(CargarAsync);
    }

    public async Task CargarAsync()
    {
        await _databaseService.InicializarAsync();
        var plantas = await _plantaService.GetPlantasAsync();
        var especies = await _especieService.GetEspeciesAsync();
        foreach (var planta in plantas)
        {
            planta.Especie = especies.FirstOrDefault(x => x.IdEspecie == planta.IdEspecie);
        }

        PlantasCount = plantas.Count;
        var riegos = await _riegoService.GetRiegosAsync();
        var detalles = await _riegoService.GetUltimosRiegosAsync();
        var fechaPorRiego = riegos.ToDictionary(x => x.IdRiego, x =>
            DateTime.TryParse(x.Fecha, out var fecha) ? fecha.Date : DateTime.MinValue);
        var ultimoRiegoPorPlanta = detalles
            .Where(x => fechaPorRiego.ContainsKey(x.IdRiego))
            .GroupBy(x => x.IdPlanta)
            .ToDictionary(x => x.Key, x => x.OrderByDescending(d => fechaPorRiego[d.IdRiego]).First());

        var proximos = plantas.Select(planta =>
        {
            var frecuencia = Math.Max(1, planta.Especie?.RiegoFrecuenciaDias ?? 7);
            var ultimaFecha = ultimoRiegoPorPlanta.TryGetValue(planta.IdPlanta, out var detalle)
                ? fechaPorRiego[detalle.IdRiego]
                : (DateTime.TryParse(planta.FechaRegistro, out var registro) ? registro.Date : DateTime.Today);
            var fechaRiego = ultimaFecha.AddDays(frecuencia);
            return new RiegoPendiente(planta.Nombre, fechaRiego, frecuencia);
        }).OrderBy(x => x.Fecha).Take(5).ToList();

        ProximoRiego = proximos.Count == 0
            ? "Agrega una planta para planificar sus riegos"
            : proximos[0].TextoFecha;
        ProximosRiegos = new ObservableCollection<RiegoPendiente>(proximos);

        var saludables = plantas.Count(x => x.Estado == "Saludable");
        var atencion = plantas.Count(x => x.Estado == "Necesita atención");
        var problemas = plantas.Count(x => x.Estado == "Con problemas");
        EstadoGeneral = $"{saludables} saludables · {atencion} necesitan atención · {problemas} con problemas";
    }
}

public sealed record RiegoPendiente(string Nombre, DateTime Fecha, int FrecuenciaDias)
{
    public string TextoFecha => Fecha.Date == DateTime.Today
        ? "Hoy"
        : Fecha.Date == DateTime.Today.AddDays(1)
            ? "Mañana"
            : Fecha.ToString("ddd, d MMM");

    public string Descripcion => $"Riego cada {FrecuenciaDias} días";
}
