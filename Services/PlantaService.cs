using PlantGuard_proyecto.Data;
using PlantGuard_proyecto.Models;

namespace PlantGuard_proyecto.Services;

public class PlantaService
{
    private readonly DatabaseService _databaseService;

    public PlantaService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public Task<List<Planta>> GetPlantasAsync() =>
        _databaseService.Connection.Table<Planta>().OrderBy(x => x.Nombre).ToListAsync();

    public Task<Planta?> GetPlantaAsync(int id) =>
        _databaseService.Connection.Table<Planta>().Where(x => x.IdPlanta == id).FirstOrDefaultAsync();

    public Task<int> InsertPlantaAsync(Planta planta) =>
        _databaseService.Connection.InsertAsync(planta);

    public Task<int> UpdatePlantaAsync(Planta planta) =>
        _databaseService.Connection.UpdateAsync(planta);

    public Task<int> DeletePlantaAsync(Planta planta) =>
        _databaseService.Connection.DeleteAsync(planta);
}
