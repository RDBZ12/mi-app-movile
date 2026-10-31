using SQLite;

namespace PlantGuard_proyecto.Models;

[Table("Usuarios")]
public class Usuario
{
    [PrimaryKey, AutoIncrement]
    public int IdUsuario { get; set; }

    [Unique, NotNull]
    public string NombreUsuario { get; set; } = string.Empty;

    [Unique, NotNull]
    public string Correo { get; set; } = string.Empty;

    [NotNull]
    public string PasswordHash { get; set; } = string.Empty;

    [NotNull]
    public string Salt { get; set; } = string.Empty;

    public string? FechaRegistro { get; set; }
}
