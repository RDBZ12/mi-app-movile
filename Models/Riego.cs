using SQLite;

namespace PlantGuard_proyecto.Models;

[Table("Riegos")]
public class Riego
{
    [PrimaryKey, AutoIncrement]
    public int IdRiego { get; set; }

    public string? Fecha { get; set; }
    public string? Observacion { get; set; }
}
