using PlantGuard_proyecto.Data;

namespace PlantGuard_proyecto;

public partial class App : Application
{
    private readonly DatabaseService _databaseService;

    public App(DatabaseService databaseService)
    {
        InitializeComponent();
        _databaseService = databaseService;
        _ = _databaseService.InicializarAsync();
        MainPage = new AppShell();
    }
}
