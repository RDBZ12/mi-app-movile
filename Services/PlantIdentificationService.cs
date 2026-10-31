namespace PlantGuard_proyecto.Services;

public class PlantIdentificationService : IPlantIdentificationService
{
    public Task<(string Nombre, double Confianza, string Descripcion, string Luz, string Riego, string Temperatura, string Humedad, string Cuidados)> IdentificarAsync(string? imagePath)
    {
        throw new NotSupportedException("La identificación automática requiere integrar un modelo de visión. Usa el catálogo local para elegir la especie que coincide con la foto.");
    }
}
