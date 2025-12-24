using Microsoft.EntityFrameworkCore;
using SistemIA.Models;

namespace SistemIA.Services;

public interface ICajaService
{
    Task<Caja?> ObtenerCajaActualAsync();
    void LimpiarCache();
}

public class CajaService : ICajaService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private static Caja? _cajaActualCache;
    private static DateTime _ultimaActualizacion = DateTime.MinValue;
    private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromMinutes(5); // Cache por 5 minutos

    public CajaService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<Caja?> ObtenerCajaActualAsync()
    {
        // Verificar si el cache es válido
        if (_cajaActualCache != null && (DateTime.Now - _ultimaActualizacion) < CACHE_DURATION)
        {
            return _cajaActualCache;
        }

        // Cargar desde base de datos
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        
        _cajaActualCache = await ctx.Cajas
            .AsNoTracking()
            .Where(c => c.CajaActual == 1)
            .FirstOrDefaultAsync() ??
            await ctx.Cajas
            .AsNoTracking()
            .OrderBy(c => c.IdCaja)
            .FirstOrDefaultAsync();

        _ultimaActualizacion = DateTime.Now;
        return _cajaActualCache;
    }

    public void LimpiarCache()
    {
        _cajaActualCache = null;
        _ultimaActualizacion = DateTime.MinValue;
    }
}