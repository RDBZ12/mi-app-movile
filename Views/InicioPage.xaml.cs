using Microsoft.Extensions.DependencyInjection;
using PlantGuard_proyecto.Services;
using PlantGuard_proyecto.ViewModels;

namespace PlantGuard_proyecto.Views;

public partial class InicioPage : ContentPage
{
    public InicioPage()
    {
        InitializeComponent();

        var services = App.Current?.Handler?.MauiContext?.Services ?? throw new InvalidOperationException("No se pudo acceder a la colección de servicios.");
        BindingContext = new InicioViewModel(
            services.GetRequiredService<PlantaService>(),
            services.GetRequiredService<RiegoService>(),
            services.GetRequiredService<EspecieService>(),
            services.GetRequiredService<PlantGuard_proyecto.Data.DatabaseService>());
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is InicioViewModel viewModel)
        {
            await viewModel.CargarAsync();
        }
    }

    private async void IdentificarClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//identificar");

    private async void MisPlantasClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//plantas");

    private async void AgregarPlantaClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("registrar-planta");

    private async void RiegoClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//riego");
}
