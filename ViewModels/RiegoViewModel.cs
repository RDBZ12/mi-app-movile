using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlantGuard_proyecto.Models;
using PlantGuard_proyecto.Services;

namespace PlantGuard_proyecto.ViewModels;

public partial class RiegoViewModel : ObservableObject
{
    private readonly PlantaService _plantaService;
    private readonly RiegoService _riegoService;

    [ObservableProperty]
    private ObservableCollection<Planta> plantas = new();

    [ObservableProperty]
    private ObservableCollection<RiegoDetalle> detalles = new();

    [ObservableProperty]
    private string observacion = string.Empty;

    public IAsyncRelayCommand CargarAsyncCommand { get; }
    public IAsyncRelayCommand GuardarAsyncCommand { get; }

    public RiegoViewModel(PlantaService plantaService, RiegoService riegoService)
    {
        _plantaService = plantaService;
        _riegoService = riegoService;
        CargarAsyncCommand = new AsyncRelayCommand(CargarAsync);
        GuardarAsyncCommand = new AsyncRelayCommand(GuardarAsync);
    }

    public async Task CargarAsync()
    {
        Plantas = new ObservableCollection<Planta>(await _plantaService.GetPlantasAsync());
    }

    public async Task GuardarAsync()
    {
        var seleccionadas = Plantas.Where(x => x.Estado == "Saludable" || x.Estado == "Necesita atención").ToList();
        if (seleccionadas.Count == 0)
        {
            return;
        }

        var riego = new Riego
        {
            Fecha = DateTime.Now.ToString("yyyy-MM-dd"),
            Observacion = Observacion
        };

        var detalles = seleccionadas.Select(planta => new RiegoDetalle
        {
            IdPlanta = planta.IdPlanta,
            CantidadAgua = 250,
            Observacion = $"Riego registrado para {planta.Nombre}"
        }).ToList();

        await _riegoService.RegistrarRiegoAsync(riego, detalles);
    }
}
