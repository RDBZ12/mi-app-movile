using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlantGuard_proyecto.Models;
using PlantGuard_proyecto.Services;

namespace PlantGuard_proyecto.ViewModels;

public partial class RecordatoriosViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;

    [ObservableProperty]
    private ObservableCollection<Recordatorio> recordatorios = new();

    public IAsyncRelayCommand CargarAsyncCommand { get; }

    public RecordatoriosViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        CargarAsyncCommand = new AsyncRelayCommand(CargarAsync);
    }

    public async Task CargarAsync()
    {
        var lista = await _databaseService.Connection.Table<Recordatorio>().ToListAsync();
        Recordatorios = new ObservableCollection<Recordatorio>(lista);
    }
}
