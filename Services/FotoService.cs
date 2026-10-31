using PlantGuard_proyecto.Data;
using PlantGuard_proyecto.Models;

namespace PlantGuard_proyecto.Services;

public class FotoService
{
    private readonly DatabaseService _databaseService;

    public FotoService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public Task<int> GuardarFotoAsync(PlantaFoto foto) =>
        _databaseService.Connection.InsertAsync(foto);

    public Task<List<PlantaFoto>> GetFotosPorPlantaAsync(int idPlanta) =>
        _databaseService.Connection.Table<PlantaFoto>().Where(x => x.IdPlanta == idPlanta).OrderByDescending(x => x.Fecha).ToListAsync();
}
