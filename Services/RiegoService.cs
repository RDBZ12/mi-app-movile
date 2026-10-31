using PlantGuard_proyecto.Data;
using PlantGuard_proyecto.Models;

namespace PlantGuard_proyecto.Services;

public class RiegoService
{
    private readonly DatabaseService _databaseService;
    private readonly AuthService _authService;

    public RiegoService(DatabaseService databaseService, AuthService authService)
    {
        _databaseService = databaseService;
        _authService = authService;
    }

    public async Task<int> RegistrarRiegoAsync(Riego riego, List<RiegoDetalle> detalles)
    {
        riego.IdUsuario = _authService.IdUsuarioActual;
        await _databaseService.Connection.InsertAsync(riego);
        var riegoId = riego.IdRiego;

        foreach (var detalle in detalles)
        {
            detalle.IdRiego = riegoId;
            await _databaseService.Connection.InsertAsync(detalle);
        }

        return riegoId;
    }

    public Task<List<Riego>> GetRiegosAsync()
    {
        var idUsuario = _authService.IdUsuarioActual;
        return _databaseService.Connection.Table<Riego>()
            .Where(x => x.IdUsuario == idUsuario)
            .OrderByDescending(x => x.Fecha)
            .ToListAsync();
    }

    public Task<List<RiegoDetalle>> GetDetallesPorRiegoAsync(int idRiego) =>
        _databaseService.Connection.Table<RiegoDetalle>().Where(x => x.IdRiego == idRiego).ToListAsync();

    public async Task<List<RiegoDetalle>> GetUltimosRiegosAsync()
    {
        var idsRiegosUsuario = (await GetRiegosAsync()).Select(x => x.IdRiego).ToHashSet();
        var detalles = await _databaseService.Connection.Table<RiegoDetalle>()
            .OrderByDescending(x => x.IdDetalle)
            .ToListAsync();
        return detalles.Where(x => idsRiegosUsuario.Contains(x.IdRiego)).ToList();
    }
}
