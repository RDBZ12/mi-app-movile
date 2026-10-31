using PlantGuard_proyecto.Data;
using PlantGuard_proyecto.Models;

namespace PlantGuard_proyecto.Services;

public class AnalisisService
{
    private readonly DatabaseService _databaseService;
    private readonly AuthService _authService;

    public AnalisisService(DatabaseService databaseService, AuthService authService)
    {
        _databaseService = databaseService;
        _authService = authService;
    }

    public Task<int> GuardarAnalisisAsync(Analisis analisis)
    {
        analisis.IdUsuario = _authService.IdUsuarioActual;
        return _databaseService.Connection.InsertAsync(analisis);
    }

    public Task<List<Analisis>> GetAnalisisPorPlantaAsync(int idPlanta)
    {
        var idUsuario = _authService.IdUsuarioActual;
        return _databaseService.Connection.Table<Analisis>()
            .Where(x => x.IdPlanta == idPlanta && x.IdUsuario == idUsuario)
            .OrderByDescending(x => x.Fecha)
            .ToListAsync();
    }

    public Task<List<Analisis>> GetTodosAsync()
    {
        var idUsuario = _authService.IdUsuarioActual;
        return _databaseService.Connection.Table<Analisis>()
            .Where(x => x.IdUsuario == idUsuario)
            .OrderByDescending(x => x.Fecha)
            .ToListAsync();
    }
}
