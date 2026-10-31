namespace PlantGuard_proyecto.Views;

public partial class ResultadoIdentificacionPage : ContentPage
{
    public ResultadoIdentificacionPage()
    {
        InitializeComponent();
    }

    private async void VolverAIdentificarClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//identificar");
    }
}
