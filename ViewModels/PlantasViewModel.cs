using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlantGuard_proyecto.Data;
using PlantGuard_proyecto.Models;
using PlantGuard_proyecto.Services;

namespace PlantGuard_proyecto.ViewModels;

public partial class PlantasViewModel : ObservableObject
{
    private readonly PlantaService _plantaService;
    private readonly EspecieService _especieService;
    private readonly DatabaseService _databaseService;

    [ObservableProperty]
    private ObservableCollection<Planta> plantas = new();

    [ObservableProperty]
    private string textoBusqueda = string.Empty;

    [ObservableProperty]
    private string filtroActual = "Todas";

    public IAsyncRelayCommand CargarAsyncCommand { get; }

    public PlantasViewModel(PlantaService plantaService, EspecieService especieService, DatabaseService databaseService)
    {
        _plantaService = plantaService;
        _especieService = especieService;
        _databaseService = databaseService;
        CargarAsyncCommand = new AsyncRelayCommand(CargarAsync);
    }

    public async Task CargarAsync()
    {
        await _databaseService.InicializarAsync();
        await AplicarFiltroAsync(FiltroActual);
    }

    public async Task AplicarFiltroAsync(string filtro)
    {
        FiltroActual = filtro;
        var plantasDb = await _plantaService.GetPlantasAsync();
        var especies = await _especieService.GetEspeciesAsync();
        foreach (var planta in plantasDb)
        {
            planta.Especie = especies.FirstOrDefault(x => x.IdEspecie == planta.IdEspecie);
        }

        if (string.Equals(filtro, "Saludables", StringComparison.OrdinalIgnoreCase))
        {
            plantasDb = plantasDb.Where(x => x.Estado == "Saludable").ToList();
        }
        else if (string.Equals(filtro, "Necesitan atención", StringComparison.OrdinalIgnoreCase))
        {
            plantasDb = plantasDb.Where(x => x.Estado == "Necesita atención").ToList();
        }
        else if (string.Equals(filtro, "Con problemas", StringComparison.OrdinalIgnoreCase))
        {
            plantasDb = plantasDb.Where(x => x.Estado == "Con problemas").ToList();
        }

        if (!string.IsNullOrWhiteSpace(TextoBusqueda))
        {
            plantasDb = plantasDb.Where(x => x.Nombre.Contains(TextoBusqueda, StringComparison.OrdinalIgnoreCase)
                || (!string.IsNullOrEmpty(x.Especie?.NombreComun) && x.Especie.NombreComun.Contains(TextoBusqueda, StringComparison.OrdinalIgnoreCase))).ToList();
        }

        Plantas = new ObservableCollection<Planta>(plantasDb);
    }
}
