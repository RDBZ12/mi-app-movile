using PlantGuard_proyecto.ViewModels;

namespace PlantGuard_proyecto.Views;

public partial class IdentificarPage : ContentPage
{
    private readonly IdentificarViewModel _viewModel;
    private bool _busy;

    public IdentificarPage()
    {
        InitializeComponent();

        var services = App.Current?.Handler?.MauiContext?.Services
            ?? throw new InvalidOperationException("No se pudo acceder a los servicios de la aplicación.");
        _viewModel = ActivatorUtilities.CreateInstance<IdentificarViewModel>(services);
        BindingContext = _viewModel;

        TakePhotoButton.Clicked += async (_, _) => await SeleccionarFotoAsync(capturar: true);
        SelectPhotoButton.Clicked += async (_, _) => await SeleccionarFotoAsync(capturar: false);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CargarAsync();
    }

    private async Task SeleccionarFotoAsync(bool capturar)
    {
        if (_busy)
        {
            return;
        }

        _busy = true;
        TakePhotoButton.IsEnabled = false;
        SelectPhotoButton.IsEnabled = false;
        try
        {
            if (capturar)
            {
                var permission = await Permissions.RequestAsync<Permissions.Camera>();
                if (permission != PermissionStatus.Granted)
                {
                    await DisplayAlert("Permiso de cámara", "Permite el acceso a la cámara en los ajustes del teléfono para tomar una foto.", "Aceptar");
                    return;
                }
            }

            var photo = capturar
                ? await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions { Title = "Fotografía de la planta" })
                : await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions { Title = "Seleccionar una planta" });
            if (photo is null)
            {
                return;
            }

            var localPath = await CopiarFotoLocalAsync(photo);
            _viewModel.AsignarFoto(localPath);
        }
        catch (FeatureNotSupportedException)
        {
            await DisplayAlert("Función no disponible", capturar ? "Este dispositivo no permite tomar fotografías." : "Este dispositivo no permite seleccionar fotografías.", "Aceptar");
        }
        catch (PermissionException)
        {
            await DisplayAlert("Permiso requerido", "Revisa los permisos de cámara y fotografías en los ajustes del teléfono.", "Aceptar");
        }
        catch (Exception ex)
        {
            await DisplayAlert("No se pudo cargar la foto", ex.Message, "Aceptar");
        }
        finally
        {
            _busy = false;
            TakePhotoButton.IsEnabled = true;
            SelectPhotoButton.IsEnabled = true;
        }
    }

    private static async Task<string> CopiarFotoLocalAsync(FileResult photo)
    {
        var directory = Path.Combine(FileSystem.AppDataDirectory, "Photos");
        Directory.CreateDirectory(directory);
        var extension = Path.GetExtension(photo.FileName);
        if (string.IsNullOrWhiteSpace(extension) || extension.Length > 8)
        {
            extension = ".jpg";
        }

        var destination = Path.Combine(directory, $"identificacion_{Guid.NewGuid():N}{extension}");
        await using var source = await photo.OpenReadAsync();
        await using var target = File.Create(destination);
        await source.CopyToAsync(target);
        return destination;
    }

    private async void GuardarPlantaClicked(object? sender, EventArgs e)
    {
        if (_busy)
        {
            return;
        }

        _busy = true;
        SavePlantButton.IsEnabled = false;
        try
        {
            var saved = await _viewModel.GuardarPlantaAsync();
            if (saved)
            {
                await DisplayAlert("Planta guardada", "La planta y su fotografía se agregaron a tu colección.", "Aceptar");
                await Shell.Current.GoToAsync("//plantas");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("No se pudo guardar", ex.Message, "Aceptar");
        }
        finally
        {
            _busy = false;
            SavePlantButton.IsEnabled = _viewModel.PuedeGuardar;
        }
    }
}
