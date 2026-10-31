using Microsoft.Extensions.DependencyInjection;
using PlantGuard_proyecto.Services;
using PlantGuard_proyecto.ViewModels;

namespace PlantGuard_proyecto.Views;

public partial class HistorialPage : ContentPage
{
    public HistorialPage()
    {
        InitializeComponent();

        var services = App.Current?.Handler?.MauiContext?.Services ?? throw new InvalidOperationException("No se pudo acceder a la colección de servicios.");
        BindingContext = new HistorialViewModel(
            services.GetRequiredService<PlantGuard_proyecto.Data.DatabaseService>(),
            services.GetRequiredService<AnalisisService>(),
            services.GetRequiredService<RiegoService>(),
            services.GetRequiredService<PlantaService>(),
            services.GetRequiredService<FotoService>());

    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is HistorialViewModel viewModel)
        {
            await viewModel.CargarAsync();
        }
    }

    private void FiltroClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && BindingContext is HistorialViewModel viewModel)
        {
            viewModel.AplicarFiltro(button.Text ?? "Todos");
        }
    }
}
