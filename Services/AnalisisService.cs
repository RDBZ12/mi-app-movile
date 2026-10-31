using PlantGuard_proyecto.Data;
using PlantGuard_proyecto.Models;

namespace PlantGuard_proyecto.Services;

public class AnalisisService
{
    private readonly DatabaseService _databaseService;

    public AnalisisService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public Task<int> GuardarAnalisisAsync(Analisis analisis) =>
        _databaseService.Connection.InsertAsync(analisis);

    public Task<List<Analisis>> GetAnalisisPorPlantaAsync(int idPlanta) =>
        _databaseService.Connection.Table<Analisis>().Where(x => x.IdPlanta == idPlanta).OrderByDescending(x => x.Fecha).ToListAsync();

    public Task<List<Analisis>> GetTodosAsync() =>
        _databaseService.Connection.Table<Analisis>().OrderByDescending(x => x.Fecha).ToListAsync();
}
