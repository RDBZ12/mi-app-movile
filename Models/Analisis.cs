using SQLite;

namespace PlantGuard_proyecto.Models;

[Table("Analisis")]
public class Analisis
{
    [PrimaryKey, AutoIncrement]
    public int IdAnalisis { get; set; }

    public int IdPlanta { get; set; }
    public string? Fecha { get; set; }
    public string? Imagen { get; set; }
    public string? Resultado { get; set; }
    public double NivelConfianza { get; set; }
    public string? Observacion { get; set; }
}
