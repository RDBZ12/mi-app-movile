using PlantGuard_proyecto.Models;
using SQLite;

namespace PlantGuard_proyecto.Data;

public static class DatabaseInitializer
{
    public static async Task SeedAsync(SQLiteAsyncConnection connection)
    {
        var existing = await connection.Table<Especie>().CountAsync();
        if (existing > 0)
        {
            return;
        }

        var especies = new List<Especie>
        {
            new() { NombreComun = "Rosa", NombreCientifico = "Rosa spp.", Descripcion = "Planta ornamental muy apreciada por sus flores aromáticas.", Luz = "Luz brillante e indirecta", RiegoFrecuenciaDias = 4, Temperatura = "18 - 28 °C", Humedad = "Media - alta", Cuidados = "Podar flores marchitas y mantener sustrato ligeramente húmedo.", Imagen = "rosa" },
            new() { NombreComun = "Girasol", NombreCientifico = "Helianthus annuus", Descripcion = "Planta de gran altura y flores solares.", Luz = "Mucha luz directa", RiegoFrecuenciaDias = 5, Temperatura = "18 - 30 °C", Humedad = "Media", Cuidados = "Requiere sol constante y riego moderado.", Imagen = "girasol" },
            new() { NombreComun = "Aloe Vera", NombreCientifico = "Aloe vera", Descripcion = "Suave y resistente, muy útil para interiores luminosos.", Luz = "Luz indirecta brillante", RiegoFrecuenciaDias = 14, Temperatura = "18 - 30 °C", Humedad = "Baja - media", Cuidados = "Dejar secar el sustrato entre riegos.", Imagen = "aloe" },
            new() { NombreComun = "Cactus", NombreCientifico = "Cactaceae", Descripcion = "Planta xerófila ideal para ambientes secos.", Luz = "Luz directa intensa", RiegoFrecuenciaDias = 21, Temperatura = "20 - 32 °C", Humedad = "Baja", Cuidados = "Riego espaciado y drenaje excelente.", Imagen = "cactus" },
            new() { NombreComun = "Monstera", NombreCientifico = "Monstera deliciosa", Descripcion = "Ficus tropical de hojas grandes y vistosas.", Luz = "Luz indirecta brillante", RiegoFrecuenciaDias = 7, Temperatura = "18 - 30 °C", Humedad = "Media - alta", Cuidados = "Mantener humedad y soporte para trepar.", Imagen = "monstera" },
            new() { NombreComun = "Pothos", NombreCientifico = "Epipremnum aureum", Descripcion = "Excelente planta colgante y muy fácil de cuidar.", Luz = "Luz indirecta", RiegoFrecuenciaDias = 7, Temperatura = "15 - 30 °C", Humedad = "Media", Cuidados = "Evitar exceso de agua y limpiar hojas.", Imagen = "pothos" },
            new() { NombreComun = "Sansevieria", NombreCientifico = "Dracaena trifasciata", Descripcion = "Muy resistente y adaptada a condiciones secas.", Luz = "Luz indirecta baja a media", RiegoFrecuenciaDias = 14, Temperatura = "18 - 32 °C", Humedad = "Baja - media", Cuidados = "Riego ocasional y poca humedad.", Imagen = "sansevieria" },
            new() { NombreComun = "Orquídea", NombreCientifico = "Orchidaceae", Descripcion = "Planta elegante que requiere humedad y luz suave.", Luz = "Luz indirecta brillante", RiegoFrecuenciaDias = 7, Temperatura = "18 - 26 °C", Humedad = "Alta", Cuidados = "Humedad ambiental y drenaje limpio.", Imagen = "orquidea" },
            new() { NombreComun = "Planta de café", NombreCientifico = "Coffea arabica", Descripcion = "Perfecta para un ambiente cálido y luminoso.", Luz = "Luz filtrada brillante", RiegoFrecuenciaDias = 5, Temperatura = "18 - 27 °C", Humedad = "Media - alta", Cuidados = "Mantener suelo húmedo pero no encharcado.", Imagen = "cafe" },
            new() { NombreComun = "Lavanda", NombreCientifico = "Lavandula angustifolia", Descripcion = "Aromática, muy apreciada por sus flores y fragancia.", Luz = "Sol directo", RiegoFrecuenciaDias = 6, Temperatura = "15 - 25 °C", Humedad = "Baja - media", Cuidados = "Requiere buen drenaje y suelo ligero.", Imagen = "lavanda" }
        };

        await connection.InsertAllAsync(especies);
    }
}
