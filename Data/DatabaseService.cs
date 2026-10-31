using PlantGuard_proyecto.Models;
using SQLite;

namespace PlantGuard_proyecto.Data;

public class DatabaseService
{
    private SQLiteAsyncConnection? _database;
    private readonly string _databasePath;
    private readonly object _initializationLock = new();
    private Task? _initializationTask;

    public DatabaseService()
    {
        _databasePath = Path.Combine(FileSystem.AppDataDirectory, "PlantGuard.db3");
    }

    public SQLiteAsyncConnection Connection =>
        _database ??= new SQLiteAsyncConnection(_databasePath);

    public Task InicializarAsync()
    {
        lock (_initializationLock)
        {
            return _initializationTask ??= InicializarBaseAsync();
        }
    }

    private async Task InicializarBaseAsync()
    {
        await Connection.CreateTableAsync<Usuario>();
        await Connection.CreateTableAsync<Especie>();
        await Connection.CreateTableAsync<Planta>();
        await Connection.CreateTableAsync<PlantaFoto>();
        await Connection.CreateTableAsync<Riego>();
        await Connection.CreateTableAsync<RiegoDetalle>();
        await Connection.CreateTableAsync<Analisis>();
        await Connection.CreateTableAsync<Recordatorio>();
        await Connection.CreateTableAsync<Cuidado>();
        await Connection.CreateTableAsync<EspecieCuidado>();
        await DatabaseInitializer.SeedAsync(Connection);
    }
}
