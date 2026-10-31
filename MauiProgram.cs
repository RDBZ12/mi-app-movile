using Microsoft.Extensions.Logging;
using PlantGuard_proyecto.Data;
using PlantGuard_proyecto.Services;
using PlantGuard_proyecto.ViewModels;

namespace PlantGuard_proyecto;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<EspecieService>();
        builder.Services.AddSingleton<PlantaService>();
        builder.Services.AddSingleton<RiegoService>();
        builder.Services.AddSingleton<FotoService>();
        builder.Services.AddSingleton<AnalisisService>();
        builder.Services.AddSingleton<NotificacionService>();
        builder.Services.AddSingleton<IPlantIdentificationService, PlantIdentificationService>();

        builder.Services.AddTransient<InicioViewModel>();
        builder.Services.AddTransient<PlantasViewModel>();
        builder.Services.AddTransient<DetallePlantaViewModel>();
        builder.Services.AddTransient<IdentificarViewModel>();
        builder.Services.AddTransient<AnalisisViewModel>();
        builder.Services.AddTransient<RiegoViewModel>();
        builder.Services.AddTransient<RecordatoriosViewModel>();
        builder.Services.AddTransient<HistorialViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
