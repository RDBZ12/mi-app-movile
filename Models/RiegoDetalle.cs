using SQLite;

namespace PlantGuard_proyecto.Models;

[Table("RiegoDetalle")]
public class RiegoDetalle
{
    [PrimaryKey, AutoIncrement]
    public int IdDetalle { get; set; }

    public int IdRiego { get; set; }
    public int IdPlanta { get; set; }
    public double CantidadAgua { get; set; }
    public string? Observacion { get; set; }
}
