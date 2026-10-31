using SQLite;

namespace PlantGuard_proyecto.Models;

[Table("PlantaFotos")]
public class PlantaFoto
{
    [PrimaryKey, AutoIncrement]
    public int IdFoto { get; set; }

    public int IdPlanta { get; set; }
    public string RutaFoto { get; set; } = string.Empty;
    public string? Fecha { get; set; }
    public string? Tipo { get; set; }
    public string? Descripcion { get; set; }
}
