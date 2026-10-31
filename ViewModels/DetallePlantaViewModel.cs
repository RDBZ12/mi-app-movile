using CommunityToolkit.Mvvm.ComponentModel;
using PlantGuard_proyecto.Data;
using PlantGuard_proyecto.Models;
using PlantGuard_proyecto.Services;

namespace PlantGuard_proyecto.ViewModels;

public partial class DetallePlantaViewModel : ObservableObject
{
    private readonly PlantaService _plantaService;
    private readonly DatabaseService _databaseService;
    private readonly EspecieService _especieService;
    private readonly FotoService _fotoService;
    private readonly AnalisisService _analisisService;
    private readonly RiegoService _riegoService;

    [ObservableProperty]
    private Planta? plantaActual;

    [ObservableProperty]
    private Especie? especieActual;

    [ObservableProperty]
    private ObservableCollection<PlantaFoto> fotos = new();

    [ObservableProperty]
    private ObservableCollection<Analisis> analisis = new();

    [ObservableProperty]
    private ObservableCollection<RiegoDetalle> riegos = new();

    public DetallePlantaViewModel(DatabaseService databaseService, PlantaService plantaService, EspecieService especieService, FotoService fotoService, AnalisisService analisisService, RiegoService riegoService)
    {
        _databaseService = databaseService;
        _plantaService = plantaService;
        _especieService = especieService;
        _fotoService = fotoService;
        _analisisService = analisisService;
        _riegoService = riegoService;
    }

    public async Task CargarAsync(int idPlanta)
    {
        await _databaseService.InicializarAsync();
        PlantaActual = await _plantaService.GetPlantaAsync(idPlanta);
        if (PlantaActual == null)
        {
            return;
        }

        EspecieActual = PlantaActual.IdEspecie.HasValue ? await _especieService.GetEspecieAsync(PlantaActual.IdEspecie.Value) : null;
        Fotos = new ObservableCollection<PlantaFoto>(await _fotoService.GetFotosPorPlantaAsync(idPlanta));
        Analisis = new ObservableCollection<Analisis>(await _analisisService.GetAnalisisPorPlantaAsync(idPlanta));
        var riegoDetalles = await _riegoService.GetUltimosRiegosAsync();
        Riegos = new ObservableCollection<RiegoDetalle>(riegoDetalles.Where(x => x.IdPlanta == idPlanta).Take(10));
    }
}
