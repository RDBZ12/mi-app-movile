using PlantGuard_proyecto.Services;

namespace PlantGuard_proyecto.Views;

public partial class LoginPage : ContentPage
{
    private readonly AuthService _authService;

    public LoginPage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    private void UsuarioCompleted(object? sender, EventArgs e) => PasswordEntry.Focus();

    private async void PasswordCompleted(object? sender, EventArgs e) => await IniciarSesionAsync();

    private async void LoginClicked(object? sender, EventArgs e) => await IniciarSesionAsync();

    private void MostrarClicked(object? sender, EventArgs e)
    {
        PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
        MostrarButton.Text = PasswordEntry.IsPassword ? "Mostrar" : "Ocultar";
    }

    private async void CrearCuentaClicked(object? sender, EventArgs e)
    {
        MostrarError(null);
        await Navigation.PushAsync(new RegistroPage(_authService));
    }

    private async Task IniciarSesionAsync()
    {
        if (!LoginButton.IsEnabled)
        {
            return;
        }

        MostrarError(null);
        LoginButton.IsEnabled = false;
        LoginButton.Text = "Entrando...";
        try
        {
            var resultado = await _authService.IniciarSesionAsync(UsuarioEntry.Text, PasswordEntry.Text);
            if (!resultado.Exito)
            {
                MostrarError(resultado.Error);
                return;
            }

            PasswordEntry.Text = string.Empty;
            ((App)Application.Current!).MostrarApp();
        }
        catch (Exception ex)
        {
            MostrarError($"No se pudo iniciar sesión: {ex.Message}");
        }
        finally
        {
            LoginButton.IsEnabled = true;
            LoginButton.Text = "Iniciar sesión";
        }
    }

    private void MostrarError(string? mensaje)
    {
        ErrorLabel.Text = mensaje ?? string.Empty;
        ErrorLabel.IsVisible = !string.IsNullOrEmpty(mensaje);
    }
}
