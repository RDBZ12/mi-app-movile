using PlantGuard_proyecto.Services;

namespace PlantGuard_proyecto.Views;

public partial class RegistroPage : ContentPage
{
    private readonly AuthService _authService;

    public RegistroPage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    private void UsuarioCompleted(object? sender, EventArgs e) => CorreoEntry.Focus();

    private void CorreoCompleted(object? sender, EventArgs e) => PasswordEntry.Focus();

    private void PasswordCompleted(object? sender, EventArgs e) => ConfirmarEntry.Focus();

    private async void ConfirmarCompleted(object? sender, EventArgs e) => await RegistrarAsync();

    private async void RegistrarClicked(object? sender, EventArgs e) => await RegistrarAsync();

    private async void VolverClicked(object? sender, EventArgs e) => await Navigation.PopAsync();

    private void MostrarClicked(object? sender, EventArgs e)
    {
        PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
        ConfirmarEntry.IsPassword = PasswordEntry.IsPassword;
        MostrarButton.Text = PasswordEntry.IsPassword ? "Mostrar" : "Ocultar";
    }

    private async Task RegistrarAsync()
    {
        if (!RegistrarButton.IsEnabled)
        {
            return;
        }

        MostrarError(null);
        RegistrarButton.IsEnabled = false;
        RegistrarButton.Text = "Creando cuenta...";
        try
        {
            var resultado = await _authService.RegistrarAsync(
                UsuarioEntry.Text, CorreoEntry.Text, PasswordEntry.Text, ConfirmarEntry.Text);

            if (!resultado.Exito)
            {
                MostrarError(resultado.Error);
                return;
            }

            // El registro deja la sesión iniciada: entra directo a la app.
            ((App)Application.Current!).MostrarApp();
        }
        catch (Exception ex)
        {
            MostrarError($"No se pudo crear la cuenta: {ex.Message}");
        }
        finally
        {
            RegistrarButton.IsEnabled = true;
            RegistrarButton.Text = "Crear cuenta";
        }
    }

    private void MostrarError(string? mensaje)
    {
        ErrorLabel.Text = mensaje ?? string.Empty;
        ErrorLabel.IsVisible = !string.IsNullOrEmpty(mensaje);
    }
}
