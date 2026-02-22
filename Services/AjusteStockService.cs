using Microsoft.EntityFrameworkCore;
using SistemIA.Models;

namespace SistemIA.Services
{
    public interface IAjusteStockService
    {
        Task<int> CrearAjusteAsync(int idSucursal, int? idCaja, int? turno, string usuario, string? comentario,
            IEnumerable<LineaAjusteInput> lineas, DateTime? fechaAjuste = null);
    }

    public record LineaAjusteInput(int IdProducto, int IdDeposito, decimal StockSistema, decimal StockAjuste, decimal PrecioCostoGs, int? IdLote = null);

    public class AjusteStockService : IAjusteStockService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        private readonly IInventarioService _inventario;
        private readonly ILogger<AjusteStockService> _logger;

        public AjusteStockService(IDbContextFactory<AppDbContext> dbFactory, IInventarioService inventario, ILogger<AjusteStockService> logger)
        {
            _dbFactory = dbFactory;
            _inventario = inventario;
            _logger = logger;
        }

       public async Task<int> CrearAjusteAsync(int idSucursal, int? idCaja, int? turno, string usuario, string? comentario,
    IEnumerable<LineaAjusteInput> lineas, DateTime? fechaAjuste = null)
{
    await using var ctx = await _dbFactory.CreateDbContextAsync();
    using var trx = await ctx.Database.BeginTransactionAsync();

    try
    {
        // 1. Normalización de datos
        var usr = string.IsNullOrWhiteSpace(usuario) ? "Sistema" : (usuario.Length > 50 ? usuario.Substring(0, 50) : usuario);
        var comm = string.IsNullOrWhiteSpace(comentario) ? null : (comentario!.Length > 280 ? comentario.Substring(0, 280) : comentario);
        var fecha = fechaAjuste ?? DateTime.Now;

        // 2. Crear Cabecera
        var cab = new AjusteStock
        {
            IdSucursal = idSucursal,
            IdCaja = idCaja,
            Turno = turno,
            FechaAjuste = fecha,
            Usuario = usr,
            Comentario = comm,
            UsuarioCreacion = usr
        };
        
        ctx.AjustesStock.Add(cab);
        await ctx.SaveChangesAsync();

        decimal totalMonto = 0;

        // 3. Procesar Líneas
        foreach (var l in lineas)
        {
            var dif = l.StockAjuste - l.StockSistema; // positivo = entrada, negativo = salida
            var monto = Math.Abs(dif) * (l.PrecioCostoGs <= 0 ? 0 : l.PrecioCostoGs);
            totalMonto += monto;

            var det = new AjusteStockDetalle
            {
                IdAjusteStock = cab.IdAjusteStock,
                IdProducto = l.IdProducto,
                IdDeposito = l.IdDeposito,
                IdProductoLote = l.IdLote,
                StockAjuste = l.StockAjuste,
                StockSistema = l.StockSistema,
                Diferencia = dif,
                Monto = monto,
                FechaAjuste = cab.FechaAjuste,
                IdSucursal = idSucursal,
                IdCaja = idCaja,
                Turno = turno,
                Usuario = usr,
                UsuarioCreacion = usr
            };

            ctx.AjustesStockDetalles.Add(det);
            
            // 4. Afectar Inventario si hay diferencia
            if (dif != 0)
            {
                var tipo = dif > 0 ? 1 : 2; // 1 entrada, 2 salida
                var cantidad = Math.Abs(dif);

                await _inventario.AjustarStockAsync(
                    l.IdProducto,
                    l.IdDeposito,
                    cantidad,
                    tipo,
                    $"Ajuste de stock #{cab.IdAjusteStock}",
                    usr,
                    l.IdLote
                );
            }
        }

        await ctx.SaveChangesAsync();
        await trx.CommitAsync();

        return cab.IdAjusteStock;
    }
    catch (Exception ex)
    {
        await trx.RollbackAsync();
        _logger.LogError(ex, "Error al crear ajuste de stock para la sucursal {IdSucursal}", idSucursal);
        throw;
    }
}
    }
}