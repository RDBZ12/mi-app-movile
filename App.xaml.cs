using PlantGuard_proyecto.Data;
using PlantGuard_proyecto.Services;
using PlantGuard_proyecto.Views;

namespace PlantGuard_proyecto;

public partial class App : Application
{
    private readonly DatabaseService _databaseService;
    private readonly AuthService _authService;

    public App(DatabaseService databaseService, AuthService authService)
    {
        InitializeComponent();
        _databaseService = databaseService;
        _authService = authService;
        _ = _databaseService.InicializarAsync();

        // Con sesión guardada entra directo; si no, empieza en el login.
        MainPage = _authService.HaySesion ? new AppShell() : CrearLogin();
    }

    /// <summary>Muestra la app principal (se crea un Shell nuevo para cargar los datos del usuario actual).</summary>
    public void MostrarApp() => MainPage = new AppShell();

    /// <summary>Cierra la sesión y vuelve al login.</summary>
    public void CerrarSesion()
    {
        _authService.CerrarSesion();
        MainPage = CrearLogin();
    }

    private Page CrearLogin() => new NavigationPage(new LoginPage(_authService))
    {
        BarBackgroundColor = Colors.White,
        BarTextColor = Color.FromArgb("#397A57")
    };
}
