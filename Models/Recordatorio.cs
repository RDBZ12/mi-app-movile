using SQLite;

namespace PlantGuard_proyecto.Models;

[Table("Recordatorios")]
public class Recordatorio
{
    [PrimaryKey, AutoIncrement]
    public int IdRecordatorio { get; set; }

    public int IdPlanta { get; set; }
    public string? Tipo { get; set; }
    public string? Fecha { get; set; }
    public string? Hora { get; set; }
    public int FrecuenciaDias { get; set; }
    public int Activo { get; set; } = 1;
}
