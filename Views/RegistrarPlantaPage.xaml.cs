using Microsoft.Extensions.DependencyInjection;
using PlantGuard_proyecto.Data;
using PlantGuard_proyecto.Models;
using PlantGuard_proyecto.Services;

namespace PlantGuard_proyecto.Views;

public partial class RegistrarPlantaPage : ContentPage
{
    private readonly PlantaService _plantaService;
    private readonly EspecieService _especieService;
    private readonly DatabaseService _databaseService;
    private string? _selectedPhotoPath;

    public RegistrarPlantaPage()
    {
        InitializeComponent();

        var services = App.Current?.Handler?.MauiContext?.Services ?? throw new InvalidOperationException("No se pudo acceder a la colección de servicios.");
        _plantaService = services.GetRequiredService<PlantaService>();
        _especieService = services.GetRequiredService<EspecieService>();
        _databaseService = services.GetRequiredService<DatabaseService>();

        Loaded += async (_, _) =>
        {
            await _databaseService.InicializarAsync();
            var especies = await _especieService.GetEspeciesAsync();
            EspeciePicker.ItemsSource = especies.Select(x => x.NombreComun).ToList();
        };

        PhotoButton.Clicked += async (_, _) => await ElegirFotoAsync();
        GuardarButton.Clicked += async (_, _) => await GuardarPlantaAsync();
    }

    private async Task ElegirFotoAsync()
    {
        var accion = await DisplayActionSheet("Añadir fotografía", "Cancelar", null, "Tomar una foto", "Elegir de la galería");
        if (accion == "Cancelar" || accion is null)
        {
            return;
        }

        try
        {
            if (accion == "Tomar una foto")
            {
                var permiso = await Permissions.RequestAsync<Permissions.Camera>();
                if (permiso != PermissionStatus.Granted)
                {
                    await DisplayAlert("Permiso de cámara", "Activa el permiso de cámara en los ajustes del teléfono.", "Aceptar");
                    return;
                }
            }

            var result = accion == "Tomar una foto"
                ? await MediaPicker.Default.CapturePhotoAsync()
                : await MediaPicker.Default.PickPhotoAsync();
            if (result is null)
            {
                return;
            }

            _selectedPhotoPath = await GuardarFotoLocalAsync(result);
            PreviewImage.Source = ImageSource.FromFile(_selectedPhotoPath);
            PreviewImage.IsVisible = true;
        }
        catch (Exception ex)
        {
            await DisplayAlert("No se pudo cargar la foto", ex.Message, "Aceptar");
        }
    }

    private async Task<string> GuardarFotoLocalAsync(FileResult source)
    {
        var directory = Path.Combine(FileSystem.AppDataDirectory, "Photos");
        Directory.CreateDirectory(directory);
        var extension = Path.GetExtension(source.FileName);
        if (string.IsNullOrWhiteSpace(extension) || extension.Length > 8)
        {
            extension = ".jpg";
        }

        var filename = $"planta_{Guid.NewGuid():N}{extension}";
        var destination = Path.Combine(directory, filename);
        await using var input = await source.OpenReadAsync();
        await using var output = File.Create(destination);
        await input.CopyToAsync(output);
        return destination;
    }

    private async Task GuardarPlantaAsync()
    {
        if (string.IsNullOrWhiteSpace(NombreEntry.Text))
        {
            await DisplayAlert("Validación", "El nombre de la planta es obligatorio.", "Aceptar");
            return;
        }

        if (EspeciePicker.SelectedItem is null)
        {
            await DisplayAlert("Validación", "Debes seleccionar una especie.", "Aceptar");
            return;
        }

        var especies = await _especieService.GetEspeciesAsync();
        var especieSeleccionada = especies.FirstOrDefault(x => x.NombreComun == EspeciePicker.SelectedItem.ToString());
        if (especieSeleccionada == null)
        {
            await DisplayAlert("Validación", "La especie elegida no es válida.", "Aceptar");
            return;
        }

        var planta = new Planta
        {
            Nombre = NombreEntry.Text,
            IdEspecie = especieSeleccionada.IdEspecie,
            FechaRegistro = DateTime.Now.ToString("yyyy-MM-dd"),
            Foto = _selectedPhotoPath,
            Ubicacion = UbicacionEntry.Text,
            Estado = "Saludable",
            Notas = NotasEditor.Text
        };

        await _plantaService.InsertPlantaAsync(planta);
        await DisplayAlert("Éxito", "Planta registrada correctamente.", "Aceptar");
        await Shell.Current.GoToAsync("//plantas");
    }
}
