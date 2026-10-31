using System.Security.Cryptography;
using System.Text.RegularExpressions;
using PlantGuard_proyecto.Data;
using PlantGuard_proyecto.Models;

namespace PlantGuard_proyecto.Services;

public record ResultadoAuth(bool Exito, string? Error = null);

/// <summary>
/// Registro, inicio de sesión y sesión actual. Todo funciona offline sobre SQLite.
/// Las contraseñas nunca se guardan en texto plano: se almacena un hash PBKDF2 con sal.
/// </summary>
public class AuthService
{
    private const string KeyIdUsuario = "sesion_id_usuario";
    private const string KeyNombreUsuario = "sesion_nombre_usuario";
    private const int Iteraciones = 100_000;
    private const int LongitudMinimaPassword = 6;

    private static readonly Regex CorreoRegex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    private static readonly Regex UsuarioRegex =
        new(@"^[a-zA-Z0-9._-]{3,20}$", RegexOptions.Compiled);

    private readonly DatabaseService _databaseService;

    public AuthService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    /// <summary>Id del usuario con sesión iniciada (0 si no hay sesión).</summary>
    public int IdUsuarioActual => Preferences.Default.Get(KeyIdUsuario, 0);

    public string NombreUsuarioActual => Preferences.Default.Get(KeyNombreUsuario, string.Empty);

    public bool HaySesion => IdUsuarioActual > 0;

    public async Task<ResultadoAuth> RegistrarAsync(string? nombreUsuario, string? correo, string? password, string? confirmacion)
    {
        nombreUsuario = nombreUsuario?.Trim() ?? string.Empty;
        correo = correo?.Trim() ?? string.Empty;
        password ??= string.Empty;

        if (!UsuarioRegex.IsMatch(nombreUsuario))
        {
            return new(false, "El usuario debe tener entre 3 y 20 caracteres (letras, números, punto, guion o guion bajo).");
        }

        if (!CorreoRegex.IsMatch(correo))
        {
            return new(false, "Escribe un correo válido.");
        }

        if (password.Length < LongitudMinimaPassword)
        {
            return new(false, $"La contraseña debe tener al menos {LongitudMinimaPassword} caracteres.");
        }

        if (password != confirmacion)
        {
            return new(false, "Las contraseñas no coinciden.");
        }

        await _databaseService.InicializarAsync();
        var db = _databaseService.Connection;

        var usuarioNorm = nombreUsuario.ToLowerInvariant();
        var correoNorm = correo.ToLowerInvariant();

        if (await db.Table<Usuario>().Where(x => x.NombreUsuario == usuarioNorm).CountAsync() > 0)
        {
            return new(false, "Ese nombre de usuario ya está en uso.");
        }

        if (await db.Table<Usuario>().Where(x => x.Correo == correoNorm).CountAsync() > 0)
        {
            return new(false, "Ya existe una cuenta con ese correo.");
        }

        var esPrimerUsuario = await db.Table<Usuario>().CountAsync() == 0;

        var salt = RandomNumberGenerator.GetBytes(16);
        var usuario = new Usuario
        {
            NombreUsuario = usuarioNorm,
            Correo = correoNorm,
            Salt = Convert.ToBase64String(salt),
            PasswordHash = Hashear(password, salt),
            FechaRegistro = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        await db.InsertAsync(usuario);

        // Datos creados antes de que existiera el login (IdUsuario = 0): los hereda la primera cuenta.
        if (esPrimerUsuario)
        {
            await db.ExecuteAsync("UPDATE Plantas SET IdUsuario = ? WHERE IdUsuario = 0", usuario.IdUsuario);
            await db.ExecuteAsync("UPDATE Riegos SET IdUsuario = ? WHERE IdUsuario = 0", usuario.IdUsuario);
            await db.ExecuteAsync("UPDATE Analisis SET IdUsuario = ? WHERE IdUsuario = 0", usuario.IdUsuario);
        }

        IniciarSesionLocal(usuario);
        return new(true);
    }

    public async Task<ResultadoAuth> IniciarSesionAsync(string? usuarioOCorreo, string? password)
    {
        var identificador = usuarioOCorreo?.Trim().ToLowerInvariant() ?? string.Empty;
        if (identificador.Length == 0 || string.IsNullOrEmpty(password))
        {
            return new(false, "Escribe tu usuario (o correo) y tu contraseña.");
        }

        await _databaseService.InicializarAsync();

        var usuario = await _databaseService.Connection.Table<Usuario>()
            .Where(x => x.NombreUsuario == identificador || x.Correo == identificador)
            .FirstOrDefaultAsync();

        // Mismo mensaje para "no existe" y "contraseña incorrecta".
        if (usuario is null || !Verificar(password, usuario))
        {
            return new(false, "Usuario o contraseña incorrectos.");
        }

        IniciarSesionLocal(usuario);
        return new(true);
    }

    public void CerrarSesion()
    {
        Preferences.Default.Remove(KeyIdUsuario);
        Preferences.Default.Remove(KeyNombreUsuario);
    }

    private static void IniciarSesionLocal(Usuario usuario)
    {
        Preferences.Default.Set(KeyIdUsuario, usuario.IdUsuario);
        Preferences.Default.Set(KeyNombreUsuario, usuario.NombreUsuario);
    }

    private static string Hashear(string password, byte[] salt) =>
        Convert.ToBase64String(Rfc2898DeriveBytes.Pbkdf2(password, salt, Iteraciones, HashAlgorithmName.SHA256, 32));

    private static bool Verificar(string password, Usuario usuario)
    {
        var salt = Convert.FromBase64String(usuario.Salt);
        var esperado = Convert.FromBase64String(usuario.PasswordHash);
        var calculado = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iteraciones, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(esperado, calculado);
    }
}
