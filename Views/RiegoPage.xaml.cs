using Microsoft.Extensions.DependencyInjection;
using PlantGuard_proyecto.Data;
using PlantGuard_proyecto.Services;

namespace PlantGuard_proyecto.Views;

public partial class RiegoPage : ContentPage
{
    private readonly PlantaService _plantaService;
    private readonly RiegoService _riegoService;
    private readonly DatabaseService _databaseService;

    public RiegoPage()
    {
        InitializeComponent();

        var services = App.Current?.Handler?.MauiContext?.Services ?? throw new InvalidOperationException("No se pudo acceder a la colección de servicios.");
        _plantaService = services.GetRequiredService<PlantaService>();
        _riegoService = services.GetRequiredService<RiegoService>();
        _databaseService = services.GetRequiredService<DatabaseService>();
        DateLabel.Text = DateTime.Now.ToString("dddd, d 'de' MMMM", new System.Globalization.CultureInfo("es"));

        PlantList.ItemsSource = new List<Models.Planta>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _databaseService.InicializarAsync();
        var plantas = await _plantaService.GetPlantasAsync();
        PlantList.ItemsSource = plantas;
        UpdateSelectionSummary();
    }

    private void SelectionChanged(object? sender, CheckedChangedEventArgs e) => UpdateSelectionSummary();

    private void UpdateSelectionSummary()
    {
        var plantas = PlantList.ItemsSource as IEnumerable<Models.Planta> ?? Enumerable.Empty<Models.Planta>();
        var count = plantas.Count(x => x.SeleccionadaParaRiego);
        SelectionLabel.Text = $"{count} seleccionadas";
        SummaryLabel.Text = count == 1 ? "1 planta seleccionada" : $"{count} plantas seleccionadas";
    }

    private async void AddPlantClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("registrar-planta");

    private async void GuardarClicked(object? sender, EventArgs e)
    {
        var plantas = PlantList.ItemsSource as IEnumerable<Models.Planta> ?? Enumerable.Empty<Models.Planta>();
        var seleccionadas = plantas.Where(x => x.SeleccionadaParaRiego).ToList();
        if (seleccionadas.Count == 0)
        {
            await DisplayAlert("Selecciona plantas", "Marca al menos una planta para registrar el riego.", "Aceptar");
            return;
        }

        if (!double.TryParse(CantidadEntry.Text, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.CurrentCulture, out var cantidad) || cantidad <= 0)
        {
            await DisplayAlert("Cantidad no válida", "Indica una cantidad de agua mayor que cero.", "Aceptar");
            return;
        }

        GuardarButton.IsEnabled = false;
        try
        {
            var riego = new Models.Riego
            {
                Fecha = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Observacion = ObservacionEditor.Text?.Trim()
            };
            var detalles = seleccionadas.Select(planta => new Models.RiegoDetalle
            {
                IdPlanta = planta.IdPlanta,
                CantidadAgua = cantidad,
                Observacion = ObservacionEditor.Text?.Trim()
            }).ToList();

            await _riegoService.RegistrarRiegoAsync(riego, detalles);
            await DisplayAlert("Riego registrado", $"Se registró el cuidado de {seleccionadas.Count} planta(s).", "Aceptar");
            CantidadEntry.Text = string.Empty;
            ObservacionEditor.Text = string.Empty;
            foreach (var planta in seleccionadas)
            {
                planta.SeleccionadaParaRiego = false;
            }

            PlantList.ItemsSource = await _plantaService.GetPlantasAsync();
            UpdateSelectionSummary();
        }
        catch (Exception ex)
        {
            await DisplayAlert("No se pudo registrar", ex.Message, "Aceptar");
        }
        finally
        {
            GuardarButton.IsEnabled = true;
        }
    }
}
