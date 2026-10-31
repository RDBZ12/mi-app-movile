using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlantGuard_proyecto.Models;
using PlantGuard_proyecto.Services;

namespace PlantGuard_proyecto.ViewModels;

public partial class RecordatoriosViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;
    private readonly PlantaService _plantaService;

    [ObservableProperty]
    private ObservableCollection<Recordatorio> recordatorios = new();

    public IAsyncRelayCommand CargarAsyncCommand { get; }

    public RecordatoriosViewModel(DatabaseService databaseService, PlantaService plantaService)
    {
        _databaseService = databaseService;
        _plantaService = plantaService;
        CargarAsyncCommand = new AsyncRelayCommand(CargarAsync);
    }

    public async Task CargarAsync()
    {
        var idsPlantas = (await _plantaService.GetPlantasAsync()).Select(x => x.IdPlanta).ToHashSet();
        var lista = await _databaseService.Connection.Table<Recordatorio>().ToListAsync();
        Recordatorios = new ObservableCollection<Recordatorio>(lista.Where(x => idsPlantas.Contains(x.IdPlanta)));
    }
}
