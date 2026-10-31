using CommunityToolkit.Mvvm.ComponentModel;
using PlantGuard_proyecto.Data;
using PlantGuard_proyecto.Models;
using PlantGuard_proyecto.Services;

namespace PlantGuard_proyecto.ViewModels;

public partial class IdentificarViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;
    private readonly EspecieService _especieService;
    private readonly PlantaService _plantaService;
    private readonly FotoService _fotoService;

    [ObservableProperty]
    private ObservableCollection<Especie> especiesFiltradas = new();

    [ObservableProperty]
    private Especie? especieSeleccionada;

    [ObservableProperty]
    private string textoBusqueda = string.Empty;

    [ObservableProperty]
    private string? rutaFoto;

    [ObservableProperty]
    private ImageSource? fotoSeleccionada;

    [ObservableProperty]
    private string nombrePlanta = string.Empty;

    [ObservableProperty]
    private string mensaje = "La búsqueda usa el catálogo guardado en el teléfono. Compara la foto y elige la especie que mejor coincide.";

    [ObservableProperty]
    private bool estaCargando;

    [ObservableProperty]
    private bool puedeGuardar;

    [ObservableProperty]
    private string textoCuidados = string.Empty;

    public bool TieneFoto => FotoSeleccionada is not null;
    public bool TieneEspecieSeleccionada => EspecieSeleccionada is not null;

    public IdentificarViewModel(
        DatabaseService databaseService,
        EspecieService especieService,
        PlantaService plantaService,
        FotoService fotoService)
    {
        _databaseService = databaseService;
        _especieService = especieService;
        _plantaService = plantaService;
        _fotoService = fotoService;
    }

    public async Task CargarAsync()
    {
        if (EstaCargando)
        {
            return;
        }

        EstaCargando = true;
        try
        {
            await _databaseService.InicializarAsync();
            FiltrarEspecies(await _especieService.GetEspeciesAsync());
            if (EspeciesFiltradas.Count == 0)
            {
                Mensaje = "El catálogo local no contiene especies.";
            }
        }
        catch (Exception ex)
        {
            Mensaje = $"No se pudo cargar el catálogo: {ex.Message}";
        }
        finally
        {
            EstaCargando = false;
        }
    }

    public void AsignarFoto(string path)
    {
        RutaFoto = path;
        FotoSeleccionada = ImageSource.FromFile(path);
        OnPropertyChanged(nameof(TieneFoto));
        ActualizarPuedeGuardar();
        Mensaje = "Foto lista. Busca la especie en el catálogo y selecciónala para ver sus cuidados.";
    }

    public async Task<bool> GuardarPlantaAsync()
    {
        if (string.IsNullOrWhiteSpace(RutaFoto) || EspecieSeleccionada is null)
        {
            Mensaje = "Toma o elige una foto y selecciona una especie antes de guardar.";
            return false;
        }

        EstaCargando = true;
        try
        {
            await _databaseService.InicializarAsync();
            var planta = new Planta
            {
                Nombre = string.IsNullOrWhiteSpace(NombrePlanta) ? EspecieSeleccionada.NombreComun : NombrePlanta.Trim(),
                IdEspecie = EspecieSeleccionada.IdEspecie,
                FechaRegistro = DateTime.Now.ToString("yyyy-MM-dd"),
                Foto = RutaFoto,
                Estado = "Saludable"
            };

            await _plantaService.InsertPlantaAsync(planta);
            await _fotoService.GuardarFotoAsync(new PlantaFoto
            {
                IdPlanta = planta.IdPlanta,
                RutaFoto = RutaFoto,
                Fecha = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Tipo = "Identificación",
                Descripcion = EspecieSeleccionada.NombreComun
            });

            PuedeGuardar = false;
            Mensaje = "Planta guardada correctamente.";
            return true;
        }
        finally
        {
            EstaCargando = false;
        }
    }

    partial void OnTextoBusquedaChanged(string value)
    {
        _ = ActualizarCatalogoAsync();
    }

    partial void OnEspecieSeleccionadaChanged(Especie? value)
    {
        OnPropertyChanged(nameof(TieneEspecieSeleccionada));
        TextoCuidados = value is null
            ? string.Empty
            : $"Luz: {value.Luz ?? "Sin datos"}\nRiego: cada {value.RiegoFrecuenciaDias} días\nTemperatura: {value.Temperatura ?? "Sin datos"}\nHumedad: {value.Humedad ?? "Sin datos"}\nCuidados: {value.Cuidados ?? "Sin datos"}";
        ActualizarPuedeGuardar();
    }

    private async Task ActualizarCatalogoAsync()
    {
        try
        {
            FiltrarEspecies(await _especieService.GetEspeciesAsync());
        }
        catch (Exception ex)
        {
            Mensaje = $"No se pudo buscar en el catálogo: {ex.Message}";
        }
    }

    private void FiltrarEspecies(IEnumerable<Especie> especies)
    {
        var filtro = TextoBusqueda.Trim();
        var coincidencias = string.IsNullOrEmpty(filtro)
            ? especies
            : especies.Where(especie =>
                especie.NombreComun.Contains(filtro, StringComparison.CurrentCultureIgnoreCase)
                || (especie.NombreCientifico?.Contains(filtro, StringComparison.CurrentCultureIgnoreCase) ?? false));
        EspeciesFiltradas = new ObservableCollection<Especie>(coincidencias.OrderBy(x => x.NombreComun));
    }

    private void ActualizarPuedeGuardar()
    {
        PuedeGuardar = !string.IsNullOrWhiteSpace(RutaFoto) && EspecieSeleccionada is not null;
    }
}
