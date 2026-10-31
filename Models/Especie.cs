using SQLite;

namespace PlantGuard_proyecto.Models;

[Table("Especies")]
public class Especie
{
    [PrimaryKey, AutoIncrement]
    public int IdEspecie { get; set; }

    [NotNull]
    public string NombreComun { get; set; } = string.Empty;

    public string? NombreCientifico { get; set; }
    public string? Descripcion { get; set; }
    public string? Luz { get; set; }
    public int RiegoFrecuenciaDias { get; set; }
    public string? Temperatura { get; set; }
    public string? Humedad { get; set; }
    public string? Cuidados { get; set; }
    public string? Imagen { get; set; }
}
