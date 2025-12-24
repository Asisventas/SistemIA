using Microsoft.EntityFrameworkCore;
using SistemIA.Models;

namespace SistemIA.Services
{
    public class PagoProveedorService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;

        public PagoProveedorService(IDbContextFactory<AppDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        /// <summary>
        /// Obtiene todas las compras pendientes de pago de un proveedor (solo a crédito)
        /// </summary>
        public async Task<List<Compra>> ObtenerComprasPendientesAsync(int idProveedor)
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            return await db.Compras
                .Where(c => c.IdProveedor == idProveedor 
                    && c.Estado == "Confirmado"
                    && c.IdTipoPago.HasValue 
                    && c.TipoPago!.EsCredito == true)
                .Include(c => c.Proveedor)
                .Include(c => c.Moneda)
                .Include(c => c.TipoPago)
                .OrderByDescending(c => c.Fecha)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene el cambio del día actual
        /// </summary>
        public async Task<decimal> ObtenerCambioDelDiaAsync()
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            var hoy = DateTime.Now.Date;
            var cambio = await db.TiposCambio
                .Where(tc => tc.FechaTipoCambio.Date == hoy)
                .OrderByDescending(tc => tc.FechaTipoCambio)
                .FirstOrDefaultAsync();
            
            return cambio?.TasaCompra ?? cambio?.TasaCambio ?? 1m;
        }

        /// <summary>
        /// Registra un pago a proveedor
        /// </summary>
        public async Task<int> RegistrarPagoAsync(PagoProveedor pago, List<PagoProveedorDetalle> detalles)
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                // Agregar pago
                db.PagosProveedores.Add(pago);
                await db.SaveChangesAsync();

                // Agregar detalles
                foreach (var detalle in detalles)
                {
                    detalle.IdPagoProveedor = pago.IdPagoProveedor;
                    db.PagosProveedoresDetalles.Add(detalle);
                }
                await db.SaveChangesAsync();

                // NOTA: El estado de la compra (Pagada/Confirmado) se maneja en el componente
                // según el cálculo del saldo pendiente real

                await transaction.CommitAsync();
                return pago.IdPagoProveedor;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Obtiene el histórico de pagos a un proveedor
        /// </summary>
        public async Task<List<PagoProveedor>> ObtenerHistoricoPagosAsync(int idProveedor, int limite = 100)
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            return await db.PagosProveedores
                .Where(pp => pp.IdProveedor == idProveedor)
                .Include(pp => pp.Compra)
                .Include(pp => pp.Proveedor)
                .Include(pp => pp.Moneda)
                .Include(pp => pp.Usuario)
                .Include(pp => pp.Detalles)
                .OrderByDescending(pp => pp.FechaPago)
                .Take(limite)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene un pago específico con todos sus detalles
        /// </summary>
        public async Task<PagoProveedor?> ObtenerPagoAsync(int idPago)
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            return await db.PagosProveedores
                .Where(pp => pp.IdPagoProveedor == idPago)
                .Include(pp => pp.Compra)
                .Include(pp => pp.Proveedor)
                .Include(pp => pp.Moneda)
                .Include(pp => pp.Usuario)
                .Include(pp => pp.Detalles)
                .FirstOrDefaultAsync();
        }
    }
}
