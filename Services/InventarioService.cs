using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using SistemIA.Models;

namespace SistemIA.Services
{
    public interface IInventarioService
    {
        Task<int> CrearDepositoAsync(string nombre, int idSucursal, string? descripcion = null);
        Task AjustarStockAsync(int idProducto, int idDeposito, decimal cantidad, int tipo, string? motivo, string? usuario);
        Task AjustarStockAsync(int idProducto, int idDeposito, decimal cantidad, int tipo, string? motivo, string? usuario, 
            int? idSucursal = null, int? idCaja = null, DateTime? fechaCaja = null, int? turno = null);
        Task<decimal> ObtenerStockAsync(int idProducto, int idDeposito);
        Task<IReadOnlyList<ProductoDeposito>> ObtenerStocksPorProductoAsync(int idProducto);
    }

    public class InventarioService : IInventarioService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        private readonly ILogger<InventarioService> _logger;
        private static readonly SemaphoreSlim _stockLock = new(1, 1);  // Semáforo para concurrencia

        public InventarioService(IDbContextFactory<AppDbContext> dbFactory, ILogger<InventarioService> logger)
        {
            _dbFactory = dbFactory;
            _logger = logger;
        }

        public async Task<int> CrearDepositoAsync(string nombre, int idSucursal, string? descripcion = null)
        {
            await using var ctx = await _dbFactory.CreateDbContextAsync();
            var existe = await ctx.Depositos.AnyAsync(d => d.IdSucursal == idSucursal && d.Nombre == nombre);
            if (existe) throw new InvalidOperationException("Ya existe un depósito con ese nombre en la sucursal.");
            var dep = new Deposito
            {
                Nombre = nombre.Trim(),
                IdSucursal = idSucursal,
                Descripcion = descripcion,
                Activo = true,
                FechaCreacion = DateTime.Now,
                UsuarioCreacion = "Sistema"
            };
            ctx.Depositos.Add(dep);
            await ctx.SaveChangesAsync();
            return dep.IdDeposito;
        }

        // Sobrecarga simple para compatibilidad hacia atrás
        public Task AjustarStockAsync(int idProducto, int idDeposito, decimal cantidad, int tipo, string? motivo, string? usuario)
            => AjustarStockAsync(idProducto, idDeposito, cantidad, tipo, motivo, usuario, null, null, null, null);

        public async Task AjustarStockAsync(int idProducto, int idDeposito, decimal cantidad, int tipo, string? motivo, string? usuario,
            int? idSucursal = null, int? idCaja = null, DateTime? fechaCaja = null, int? turno = null)
        {
            _logger.LogInformation($"AjustarStockAsync llamado - Producto: {idProducto}, Deposito: {idDeposito}, Cantidad: {cantidad}, Tipo: {tipo}");
            
            if (cantidad <= 0) 
            {
                _logger.LogError($"Cantidad inválida: {cantidad}");
                throw new ArgumentOutOfRangeException(nameof(cantidad), cantidad, "La cantidad debe ser mayor a 0");
            }
            
            if (tipo != 1 && tipo != 2) 
            {
                _logger.LogError($"Tipo inválido: {tipo}");
                throw new ArgumentOutOfRangeException(nameof(tipo), tipo, "El tipo debe ser 1 (entrada) o 2 (salida)");
            }

            // Usamos semáforo para evitar conflictos de concurrencia en el mismo servidor
            await _stockLock.WaitAsync();
            try
            {
                await using var ctx = await _dbFactory.CreateDbContextAsync();
                using var trx = await ctx.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
                
                try
                {
                    var prod = await ctx.Productos.FirstOrDefaultAsync(p => p.IdProducto == idProducto);
                    if (prod == null) throw new InvalidOperationException("Producto no encontrado");
                    var dep = await ctx.Depositos.FirstOrDefaultAsync(d => d.IdDeposito == idDeposito);
                    if (dep == null) throw new InvalidOperationException("Depósito no encontrado");

                    // Obtener sucursal del depósito si no se especificó
                    var sucursalId = idSucursal ?? dep.IdSucursal;

                    ProductoDeposito? pd = null;
                    decimal cantidadAnterior = 0;
                    decimal saldoPosterior = 0;
                    var ajustarStockDirecto = !(tipo == 2 && prod.EsCombo);
                    
                    if (ajustarStockDirecto)
                    {
                        // Usar bloqueo con UPDLOCK para evitar conflictos de concurrencia entre servidores
                        pd = await ctx.ProductosDepositos
                            .FromSqlRaw(@"SELECT * FROM ProductosDepositos WITH (UPDLOCK) 
                                         WHERE IdProducto = @p0 AND IdDeposito = @p1", idProducto, idDeposito)
                            .FirstOrDefaultAsync();
                            
                        if (pd == null)
                        {
                            pd = new ProductoDeposito
                            {
                                IdProducto = idProducto,
                                IdDeposito = idDeposito,
                                Stock = 0,
                                StockMinimo = 0,
                                FechaCreacion = DateTime.Now,
                                UsuarioCreacion = usuario ?? "Sistema"
                            };
                            ctx.ProductosDepositos.Add(pd);
                            await ctx.SaveChangesAsync();
                        }

                        cantidadAnterior = pd.Stock;
                        var delta = tipo == 1 ? cantidad : -cantidad;
                        if (pd.Stock + delta < 0)
                            throw new InvalidOperationException($"Stock insuficiente en el depósito. Stock actual: {pd.Stock}, intentando descontar: {cantidad}");

                        pd.Stock += delta;
                        saldoPosterior = pd.Stock;
                        pd.FechaModificacion = DateTime.Now;
                        pd.UsuarioModificacion = usuario ?? "Sistema";
                    }

                    // Obtener precios y moneda del producto para valorización
                    decimal? precioCosto = prod.CostoUnitarioGs;
                    decimal? precioVenta = prod.PrecioUnitarioGs;
                    int? idMoneda = prod.IdMonedaPrecio ?? 1; // 1 = Guaraníes por defecto
                    decimal tipoCambio = 1m;
                    decimal? precioCostoGs = precioCosto;
                    decimal? precioVentaGs = precioVenta;

                    // Si no es Guaraníes (IdMoneda != 1), obtener tipo de cambio
                    if (idMoneda.HasValue && idMoneda.Value != 1)
                    {
                        var cambio = await ctx.TiposCambioHistorico
                            .Where(tc => tc.IdMonedaOrigen == idMoneda.Value 
                                      && tc.IdMonedaDestino == 1 
                                      && tc.FechaRegistro.Date <= DateTime.Now.Date)
                            .OrderByDescending(tc => tc.FechaRegistro)
                            .FirstOrDefaultAsync();
                        
                        if (cambio != null && cambio.TasaCambio > 0)
                        {
                            tipoCambio = cambio.TasaCambio;
                            precioCostoGs = precioCosto * tipoCambio;
                            precioVentaGs = precioVenta * tipoCambio;
                        }
                    }

                    // Registrar movimiento con datos de trazabilidad completos
                    ctx.MovimientosInventario.Add(new MovimientoInventario
                    {
                        IdProducto = idProducto,
                        IdDeposito = idDeposito,
                        Tipo = tipo,
                        Cantidad = cantidad,
                        CantidadAnterior = cantidadAnterior,
                        SaldoPosterior = saldoPosterior,
                        Motivo = motivo,
                        Usuario = usuario ?? "Sistema",
                        Fecha = DateTime.Now,
                        FechaCaja = fechaCaja,
                        Turno = turno,
                        IdSucursal = sucursalId,
                        IdCaja = idCaja,
                        PrecioCosto = precioCosto,
                        PrecioVenta = precioVenta,
                        IdMoneda = idMoneda,
                        TipoCambio = tipoCambio,
                        PrecioCostoGs = precioCostoGs,
                        PrecioVentaGs = precioVentaGs
                    });

                    await ctx.SaveChangesAsync();
                    await trx.CommitAsync();

                    // Si es una salida de un combo, descontar los componentes también (fuera de la transacción principal)
                    if (tipo == 2 && prod.EsCombo)
                    {
                        await ProcesarComponentesComboAsync(idProducto, idDeposito, cantidad, usuario, sucursalId, idCaja, fechaCaja, turno);
                    }

                    // Recalcular stock total del producto
                    await RecalcularStockTotalAsync(idProducto, usuario);
                }
                catch
                {
                    await trx.RollbackAsync();
                    throw;
                }
            }
            finally
            {
                _stockLock.Release();
            }
        }
        
        private async Task ProcesarComponentesComboAsync(int idProducto, int idDeposito, decimal cantidad, string? usuario,
            int? idSucursal, int? idCaja, DateTime? fechaCaja, int? turno)
        {
            await using var ctx = await _dbFactory.CreateDbContextAsync();
            var componentes = await ctx.ProductosComponentes
                .AsNoTracking()
                .Where(pc => pc.IdProducto == idProducto && pc.Activo)
                .ToListAsync();
                
            if (componentes.Count > 0)
            {
                _logger.LogInformation($"Producto {idProducto} es combo con {componentes.Count} componentes.");
                foreach (var comp in componentes)
                {
                    var cantDesc = comp.Cantidad * cantidad;
                    if (cantDesc <= 0) continue;
                    
                    _logger.LogInformation($"Descontando componente {comp.IdComponente}, cantidad: {cantDesc}");
                    await AjustarStockAsync(comp.IdComponente, idDeposito, cantDesc, 2, $"Desc. combo #{idProducto}", usuario,
                        idSucursal, idCaja, fechaCaja, turno);
                }
            }
        }
        
        private async Task RecalcularStockTotalAsync(int idProducto, string? usuario)
        {
            await using var ctx = await _dbFactory.CreateDbContextAsync();
            var prod = await ctx.Productos.FirstOrDefaultAsync(p => p.IdProducto == idProducto);
            if (prod != null)
            {
                var total = await ctx.ProductosDepositos
                    .Where(x => x.IdProducto == idProducto)
                    .SumAsync(x => (decimal?)x.Stock) ?? 0m;
                prod.Stock = total;
                prod.FechaModificacion = DateTime.Now;
                prod.UsuarioModificacion = usuario ?? "Sistema";
                await ctx.SaveChangesAsync();
            }
        }

        public async Task<decimal> ObtenerStockAsync(int idProducto, int idDeposito)
        {
            await using var ctx = await _dbFactory.CreateDbContextAsync();
            var pd = await ctx.ProductosDepositos.AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdProducto == idProducto && x.IdDeposito == idDeposito);
            return pd?.Stock ?? 0m;
        }

        public async Task<IReadOnlyList<ProductoDeposito>> ObtenerStocksPorProductoAsync(int idProducto)
        {
            await using var ctx = await _dbFactory.CreateDbContextAsync();
            return await ctx.ProductosDepositos
                .Include(x => x.Deposito)
                .Where(x => x.IdProducto == idProducto)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
