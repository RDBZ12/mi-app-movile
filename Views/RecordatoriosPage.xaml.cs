using Microsoft.Extensions.DependencyInjection;
using PlantGuard_proyecto.Data;
using PlantGuard_proyecto.ViewModels;

namespace PlantGuard_proyecto.Views;

public partial class RecordatoriosPage : ContentPage
{
    public RecordatoriosPage()
    {
        InitializeComponent();

        var services = App.Current?.Handler?.MauiContext?.Services ?? throw new InvalidOperationException("No se pudo acceder a la colección de servicios.");
        BindingContext = new RecordatoriosViewModel(services.GetRequiredService<DatabaseService>());
    }
}
