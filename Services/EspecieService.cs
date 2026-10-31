using PlantGuard_proyecto.Data;
using PlantGuard_proyecto.Models;

namespace PlantGuard_proyecto.Services;

public class EspecieService
{
    private readonly DatabaseService _databaseService;

    public EspecieService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public Task<List<Especie>> GetEspeciesAsync() =>
        _databaseService.Connection.Table<Especie>().OrderBy(x => x.NombreComun).ToListAsync();

    public Task<Especie?> GetEspecieAsync(int id) =>
        _databaseService.Connection.Table<Especie>().Where(x => x.IdEspecie == id).FirstOrDefaultAsync();
}
