using Microsoft.Extensions.DependencyInjection;
using PlantGuard_proyecto.Models;
using PlantGuard_proyecto.Services;
using PlantGuard_proyecto.ViewModels;

namespace PlantGuard_proyecto.Views;

public partial class PlantasPage : ContentPage
{
    public PlantasPage()
    {
        InitializeComponent();

        var services = App.Current?.Handler?.MauiContext?.Services ?? throw new InvalidOperationException("No se pudo acceder a la colección de servicios.");
        BindingContext = new PlantasViewModel(
            services.GetRequiredService<PlantaService>(),
            services.GetRequiredService<EspecieService>(),
            services.GetRequiredService<PlantGuard_proyecto.Data.DatabaseService>());

        searchBar.TextChanged += async (_, e) =>
        {
            if (BindingContext is PlantasViewModel viewModel)
            {
                viewModel.TextoBusqueda = e.NewTextValue ?? string.Empty;
                await viewModel.AplicarFiltroAsync(viewModel.FiltroActual);
                PlantCollection.ItemsSource = viewModel.Plantas;
            }
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PlantasViewModel viewModel)
        {
            await viewModel.CargarAsync();
            PlantCollection.ItemsSource = viewModel.Plantas;
        }
    }

    private async void OnFilterClicked(object sender, EventArgs e)
    {
        if (BindingContext is PlantasViewModel viewModel && sender is Button button)
        {
            await viewModel.AplicarFiltroAsync(button.Text ?? "Todas");
            PlantCollection.ItemsSource = viewModel.Plantas;
        }
    }

    private async void AddPlantClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("registrar-planta");

    private async void PlantTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Border border && border.BindingContext is Planta planta)
        {
            var parameters = new ShellNavigationQueryParameters { ["idPlanta"] = planta.IdPlanta.ToString() };
            await Shell.Current.GoToAsync("detalle-planta", parameters);
        }
    }
}
