using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlantGuard_proyecto.Models;
using PlantGuard_proyecto.Services;

namespace PlantGuard_proyecto.ViewModels;

public partial class AnalisisViewModel : ObservableObject
{
    private readonly PlantaService _plantaService;
    private readonly AnalisisService _analisisService;

    [ObservableProperty]
    private ObservableCollection<Planta> plantas = new();

    [ObservableProperty]
    private Planta? plantaSeleccionada;

    [ObservableProperty]
    private string rutaImagen = string.Empty;

    [ObservableProperty]
    private string resultado = "Sin análisis";

    [ObservableProperty]
    private double nivelConfianza;

    [ObservableProperty]
    private string observacion = string.Empty;

    public IAsyncRelayCommand CargarAsyncCommand { get; }
    public IAsyncRelayCommand GuardarAsyncCommand { get; }

    public AnalisisViewModel(PlantaService plantaService, AnalisisService analisisService)
    {
        _plantaService = plantaService;
        _analisisService = analisisService;
        CargarAsyncCommand = new AsyncRelayCommand(CargarAsync);
        GuardarAsyncCommand = new AsyncRelayCommand(GuardarAsync);
    }

    public async Task CargarAsync()
    {
        Plantas = new ObservableCollection<Planta>(await _plantaService.GetPlantasAsync());
    }

    public async Task GuardarAsync()
    {
        if (PlantaSeleccionada == null)
        {
            return;
        }

        var analisis = new Analisis
        {
            IdPlanta = PlantaSeleccionada.IdPlanta,
            Fecha = DateTime.Now.ToString("yyyy-MM-dd"),
            Imagen = RutaImagen,
            Resultado = Resultado,
            NivelConfianza = NivelConfianza,
            Observacion = Observacion
        };

        await _analisisService.GuardarAnalisisAsync(analisis);
    }
}
