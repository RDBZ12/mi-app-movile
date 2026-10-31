using SQLite;

namespace PlantGuard_proyecto.Models;

[Table("EspecieCuidado")]
public class EspecieCuidado
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int IdEspecie { get; set; }
    public int IdCuidado { get; set; }
    public string? Nivel { get; set; }
    public string? Observacion { get; set; }
}
