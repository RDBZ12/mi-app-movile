namespace PlantGuard_proyecto.Services;

public interface IPlantIdentificationService
{
    Task<(string Nombre, double Confianza, string Descripcion, string Luz, string Riego, string Temperatura, string Humedad, string Cuidados)> IdentificarAsync(string? imagePath);
}
