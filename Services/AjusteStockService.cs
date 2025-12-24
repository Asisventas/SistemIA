using Microsoft.EntityFrameworkCore;
using SistemIA.Models;

namespace SistemIA.Services
{
    public interface IAjusteStockService
    {
        Task<int> CrearAjusteAsync(int idSucursal, int? idCaja, int? turno, string usuario, string? comentario,
            int idDeposito, IEnumerable<LineaAjusteInput> lineas);
    }

    public record LineaAjusteInput(int IdProducto, decimal StockSistema, decimal StockAjuste, decimal PrecioCostoGs);

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
            int idDeposito, IEnumerable<LineaAjusteInput> lineas)
        {
            await using var ctx = await _dbFactory.CreateDbContextAsync();
            using var trx = await ctx.Database.BeginTransactionAsync();

            // Normalizar y truncar según restricciones de la BD
            var usr = string.IsNullOrWhiteSpace(usuario) ? "Sistema" : (usuario.Length > 50 ? usuario.Substring(0, 50) : usuario);
            var comm = string.IsNullOrWhiteSpace(comentario) ? null : (comentario!.Length > 280 ? comentario.Substring(0, 280) : comentario);

            var cab = new AjusteStock
            {
                IdSucursal = idSucursal,
                IdCaja = idCaja,
                Turno = turno,
                FechaAjuste = DateTime.Now,
                Usuario = usr,
                Comentario = comm,
                UsuarioCreacion = usr
            };
            ctx.AjustesStock.Add(cab);
            await ctx.SaveChangesAsync();

            decimal totalMonto = 0;
            foreach (var l in lineas)
            {
                var dif = l.StockAjuste - l.StockSistema; // positivo = entrada, negativo = salida
                var monto = Math.Abs(dif) * (l.PrecioCostoGs <= 0 ? 0 : l.PrecioCostoGs);
                totalMonto += monto;

                var det = new AjusteStockDetalle
                {
                    IdAjusteStock = cab.IdAjusteStock,
                    IdProducto = l.IdProducto,
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
                await ctx.SaveChangesAsync();

                if (dif != 0)
                {
                    var tipo = dif > 0 ? 1 : 2; // 1 entrada, 2 salida
                    var cantidad = Math.Abs(dif);
                    await _inventario.AjustarStockAsync(l.IdProducto, idDeposito, cantidad, tipo, $"Ajuste de stock #{cab.IdAjusteStock}", usr);
                }
            }

            cab.TotalMonto = totalMonto;
            ctx.AjustesStock.Update(cab);
            await ctx.SaveChangesAsync();
            await trx.CommitAsync();
            return cab.IdAjusteStock;
        }
    }
}
