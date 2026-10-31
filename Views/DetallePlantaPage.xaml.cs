using Microsoft.Extensions.DependencyInjection;
using PlantGuard_proyecto.Services;
using PlantGuard_proyecto.ViewModels;

namespace PlantGuard_proyecto.Views;

[QueryProperty(nameof(IdPlanta), "idPlanta")]
public partial class DetallePlantaPage : ContentPage
{
    private readonly DetallePlantaViewModel _viewModel;

    public string IdPlanta { get; set; } = string.Empty;

    public DetallePlantaPage()
    {
        InitializeComponent();

        var services = App.Current?.Handler?.MauiContext?.Services ?? throw new InvalidOperationException("No se pudo acceder a la colección de servicios.");
        _viewModel = new DetallePlantaViewModel(
            services.GetRequiredService<PlantGuard_proyecto.Data.DatabaseService>(),
            services.GetRequiredService<PlantaService>(),
            services.GetRequiredService<EspecieService>(),
            services.GetRequiredService<FotoService>(),
            services.GetRequiredService<AnalisisService>(),
            services.GetRequiredService<RiegoService>());
        BindingContext = _viewModel;

    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (int.TryParse(IdPlanta, out var idPlanta) && idPlanta > 0)
        {
            await _viewModel.CargarAsync(idPlanta);
        }
    }

    private async void RiegoButtonClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//riego");

    private async void AnalisisButtonClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("analisis-salud");
}
