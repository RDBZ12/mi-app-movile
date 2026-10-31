using Microsoft.Extensions.DependencyInjection;
using PlantGuard_proyecto.Models;
using PlantGuard_proyecto.Services;

namespace PlantGuard_proyecto.Views;

public partial class AnalisisSaludPage : ContentPage
{
    private readonly PlantaService _plantaService;
    private readonly AnalisisService _analisisService;
    private string? _photoPath;

    public AnalisisSaludPage()
    {
        InitializeComponent();

        var services = App.Current?.Handler?.MauiContext?.Services ?? throw new InvalidOperationException("No se pudo acceder a la colección de servicios.");
        _plantaService = services.GetRequiredService<PlantaService>();
        _analisisService = services.GetRequiredService<AnalisisService>();

        Loaded += async (_, _) =>
        {
            var plantas = await _plantaService.GetPlantasAsync();
            PlantaPicker.ItemsSource = plantas.Select(x => x.Nombre).ToList();
        };

        PhotoButton.Clicked += async (_, _) =>
        {
            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo != null)
            {
                _photoPath = photo.FullPath;
                PreviewImage.Source = ImageSource.FromFile(_photoPath);
                PreviewImage.IsVisible = true;
            }
        };

        AnalyzeButton.Clicked += async (_, _) =>
        {
            ResultadoLabel.Text = "Estado: Posible problema";
            ConfianzaLabel.Text = "Confianza: 87%";
            ObservacionLabel.Text = "Se detectan síntomas visuales que podrían estar relacionados con estrés de la planta.";
        };

        GuardarAnalisisButton.Clicked += async (_, _) =>
        {
            var plantas = await _plantaService.GetPlantasAsync();
            var planta = plantas.FirstOrDefault(x => x.Nombre == PlantaPicker.SelectedItem?.ToString());
            if (planta == null)
            {
                await DisplayAlert("Validación", "Debes seleccionar una planta.", "Aceptar");
                return;
            }

            var analisis = new Analisis
            {
                IdPlanta = planta.IdPlanta,
                Fecha = DateTime.Now.ToString("yyyy-MM-dd"),
                Imagen = _photoPath,
                Resultado = "Posible problema",
                NivelConfianza = 87,
                Observacion = ObservacionLabel.Text
            };

            await _analisisService.GuardarAnalisisAsync(analisis);
            await DisplayAlert("Éxito", "Análisis guardado correctamente.", "Aceptar");
        };
    }
}
