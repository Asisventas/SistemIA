using Microsoft.EntityFrameworkCore;
using SistemIA.Models;

namespace SistemIA.Services
{
    /// <summary>
    /// Servicio para manejar el historial de tipos de cambio
    /// </summary>
    public class TipoCambioHistoricoService : ITipoCambioHistoricoService
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
        private readonly ITipoCambioService _tipoCambioService;
        private readonly ILogger<TipoCambioHistoricoService> _logger;

        public TipoCambioHistoricoService(
            IDbContextFactory<AppDbContext> dbContextFactory,
            ITipoCambioService tipoCambioService,
            ILogger<TipoCambioHistoricoService> logger)
        {
            _dbContextFactory = dbContextFactory;
            _tipoCambioService = tipoCambioService;
            _logger = logger;
        }

        public async Task GuardarHistorialAsync()
        {
            try
            {
                _logger.LogInformation("[HISTORIAL] Iniciando guardado de historial");

                using var context = _dbContextFactory.CreateDbContext();

                // Obtener todos los tipos de cambio actuales
                var tiposCambio = await context.TiposCambio
                    .Include(tc => tc.MonedaOrigen)
                    .Include(tc => tc.MonedaDestino)
                    .Where(tc => tc.Estado == true)
                    .ToListAsync();

                var fechaActual = DateTime.Now;
                var registrosCreados = 0;

                foreach (var tipoCambio in tiposCambio)
                {
                    // Verificar si ya existe un registro para hoy
                    var existeHoy = await context.TiposCambioHistorico
                        .AnyAsync(h => h.IdMonedaOrigen == tipoCambio.IdMonedaOrigen
                                    && h.IdMonedaDestino == tipoCambio.IdMonedaDestino
                                    && h.FechaRegistro.Date == fechaActual.Date);

                    if (!existeHoy)
                    {
                        var historial = new TipoCambioHistorico
                        {
                            IdMonedaOrigen = tipoCambio.IdMonedaOrigen,
                            IdMonedaDestino = tipoCambio.IdMonedaDestino,
                            TasaCambio = tipoCambio.TasaCambio,
                            TasaCompra = tipoCambio.TasaCompra,
                            FechaRegistro = fechaActual,
                            Fuente = "Sistema",
                            UsuarioCreacion = "Sistema"
                        };

                        context.TiposCambioHistorico.Add(historial);
                        registrosCreados++;
                    }
                }

                await context.SaveChangesAsync();
                _logger.LogInformation($"[HISTORIAL] ✅ {registrosCreados} registros históricos guardados");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[HISTORIAL] ❌ Error al guardar historial");
                throw;
            }
        }

        public async Task<List<TipoCambioHistorico>> ObtenerHistorialAsync(string monedaOrigen, string monedaDestino, DateTime fechaDesde, DateTime fechaHasta)
        {
            using var context = _dbContextFactory.CreateDbContext();

            return await context.TiposCambioHistorico
                .Include(h => h.MonedaOrigen)
                .Include(h => h.MonedaDestino)
                .Where(h => h.MonedaOrigen!.CodigoISO == monedaOrigen
                         && h.MonedaDestino!.CodigoISO == monedaDestino
                         && h.FechaRegistro.Date >= fechaDesde.Date
                         && h.FechaRegistro.Date <= fechaHasta.Date)
                .OrderBy(h => h.FechaRegistro)
                .ToListAsync();
        }

        public async Task<List<TipoCambioHistorico>> ObtenerHistorialUltimoMesAsync()
        {
            using var context = _dbContextFactory.CreateDbContext();
            
            var fechaDesde = DateTime.Now.AddDays(-30);
            var fechaHasta = DateTime.Now;

            return await context.TiposCambioHistorico
                .Include(h => h.MonedaOrigen)
                .Include(h => h.MonedaDestino)
                .Where(h => h.FechaRegistro.Date >= fechaDesde.Date 
                         && h.FechaRegistro.Date <= fechaHasta.Date)
                .OrderBy(h => h.FechaRegistro)
                .ToListAsync();
        }

        public async Task<Dictionary<string, List<object>>> ObtenerDatosGraficoAsync(int diasAtras = 30)
        {
            using var context = _dbContextFactory.CreateDbContext();
            
            var fechaDesde = DateTime.Now.AddDays(-diasAtras);
            var fechaHasta = DateTime.Now;

            var datos = await context.TiposCambioHistorico
                .Include(h => h.MonedaOrigen)
                .Include(h => h.MonedaDestino)
                .Where(h => h.FechaRegistro.Date >= fechaDesde.Date 
                         && h.FechaRegistro.Date <= fechaHasta.Date)
                .OrderBy(h => h.FechaRegistro)
                .ToListAsync();

            var resultado = new Dictionary<string, List<object>>();

            // Agrupar por par de monedas
            var pares = datos.GroupBy(d => new { 
                Origen = d.MonedaOrigen!.CodigoISO, 
                Destino = d.MonedaDestino!.CodigoISO 
            });

            foreach (var par in pares)
            {
                var nombrePar = $"{par.Key.Origen}/{par.Key.Destino}";
                var puntos = new List<object>();

                foreach (var punto in par.OrderBy(p => p.FechaRegistro))
                {
                    puntos.Add(new
                    {
                        x = punto.FechaRegistro.ToString("yyyy-MM-ddTHH:mm:ss"),
                        y = punto.TasaCambio
                    });
                }

                if (puntos.Any())
                {
                    resultado[nombrePar] = puntos;
                }
            }

            return resultado;
        }

        public async Task<decimal> CalcularVariacionAsync(string monedaOrigen, string monedaDestino, int diasAtras = 7)
        {
            try
            {
                using var context = _dbContextFactory.CreateDbContext();

                var fechaDesde = DateTime.Now.AddDays(-diasAtras);

                var registros = await context.TiposCambioHistorico
                    .Include(h => h.MonedaOrigen)
                    .Include(h => h.MonedaDestino)
                    .Where(h => h.MonedaOrigen!.CodigoISO == monedaOrigen
                             && h.MonedaDestino!.CodigoISO == monedaDestino
                             && h.FechaRegistro.Date >= fechaDesde.Date)
                    .OrderBy(h => h.FechaRegistro)
                    .ToListAsync();

                if (registros.Count < 2)
                    return 0;

                var valorInicial = registros.First().TasaCambio;
                var valorFinal = registros.Last().TasaCambio;

                if (valorInicial == 0)
                    return 0;

                return ((valorFinal - valorInicial) / valorInicial) * 100;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[VARIACIÓN] Error calculando variación {monedaOrigen}/{monedaDestino}");
                return 0;
            }
        }
    }
}
