using PlantGuard_proyecto.Data;
using PlantGuard_proyecto.Models;

namespace PlantGuard_proyecto.Services;

public class PlantaService
{
    private readonly DatabaseService _databaseService;
    private readonly AuthService _authService;

    public PlantaService(DatabaseService databaseService, AuthService authService)
    {
        _databaseService = databaseService;
        _authService = authService;
    }

    public Task<List<Planta>> GetPlantasAsync()
    {
        var idUsuario = _authService.IdUsuarioActual;
        return _databaseService.Connection.Table<Planta>()
            .Where(x => x.IdUsuario == idUsuario)
            .OrderBy(x => x.Nombre)
            .ToListAsync();
    }

    public Task<Planta?> GetPlantaAsync(int id)
    {
        var idUsuario = _authService.IdUsuarioActual;
        return _databaseService.Connection.Table<Planta>()
            .Where(x => x.IdPlanta == id && x.IdUsuario == idUsuario)
            .FirstOrDefaultAsync();
    }

    public Task<int> InsertPlantaAsync(Planta planta)
    {
        planta.IdUsuario = _authService.IdUsuarioActual;
        return _databaseService.Connection.InsertAsync(planta);
    }

    public Task<int> UpdatePlantaAsync(Planta planta)
    {
        planta.IdUsuario = _authService.IdUsuarioActual;
        return _databaseService.Connection.UpdateAsync(planta);
    }

    public Task<int> DeletePlantaAsync(Planta planta) =>
        _databaseService.Connection.DeleteAsync(planta);
}
