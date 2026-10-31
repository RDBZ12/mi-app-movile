using PlantGuard_proyecto.Data;
using PlantGuard_proyecto.Models;

namespace PlantGuard_proyecto.Services;

public class RiegoService
{
    private readonly DatabaseService _databaseService;

    public RiegoService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task<int> RegistrarRiegoAsync(Riego riego, List<RiegoDetalle> detalles)
    {
        var riegoId = await _databaseService.Connection.InsertAsync(riego);

        foreach (var detalle in detalles)
        {
            detalle.IdRiego = riegoId;
            await _databaseService.Connection.InsertAsync(detalle);
        }

        return riegoId;
    }

    public Task<List<Riego>> GetRiegosAsync() =>
        _databaseService.Connection.Table<Riego>().OrderByDescending(x => x.Fecha).ToListAsync();

    public Task<List<RiegoDetalle>> GetDetallesPorRiegoAsync(int idRiego) =>
        _databaseService.Connection.Table<RiegoDetalle>().Where(x => x.IdRiego == idRiego).ToListAsync();

    public Task<List<RiegoDetalle>> GetUltimosRiegosAsync() =>
        _databaseService.Connection.Table<RiegoDetalle>().OrderByDescending(x => x.IdDetalle).ToListAsync();
}
