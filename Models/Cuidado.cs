using SQLite;

namespace PlantGuard_proyecto.Models;

[Table("Cuidados")]
public class Cuidado
{
    [PrimaryKey, AutoIncrement]
    public int IdCuidado { get; set; }

    [NotNull]
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}
