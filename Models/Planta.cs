using SQLite;

namespace PlantGuard_proyecto.Models;

[Table("Plantas")]
public class Planta
{
    [PrimaryKey, AutoIncrement]
    public int IdPlanta { get; set; }

    [NotNull]
    public string Nombre { get; set; } = string.Empty;

    public int? IdEspecie { get; set; }
    public string? FechaRegistro { get; set; }
    public string? Foto { get; set; }
    public string? Ubicacion { get; set; }
    public string Estado { get; set; } = "Saludable";
    public string? Notas { get; set; }

    [Ignore]
    public bool SeleccionadaParaRiego { get; set; }

    [Ignore]
    public Especie? Especie { get; set; }
}
