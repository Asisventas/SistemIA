using Microsoft.EntityFrameworkCore;
using SistemIA.Models;

namespace SistemIA.Services
{
    public interface IInventarioService
    {
        Task<int> CrearDepositoAsync(string nombre, int idSucursal, string? descripcion = null);
        Task AjustarStockAsync(int idProducto, int idDeposito, decimal cantidad, int tipo, string? motivo, string? usuario);
        Task<decimal> ObtenerStockAsync(int idProducto, int idDeposito);
        Task<IReadOnlyList<ProductoDeposito>> ObtenerStocksPorProductoAsync(int idProducto);
    }

    public class InventarioService : IInventarioService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        private readonly ILogger<InventarioService> _logger;

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

        public async Task AjustarStockAsync(int idProducto, int idDeposito, decimal cantidad, int tipo, string? motivo, string? usuario)
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

            await using var ctx = await _dbFactory.CreateDbContextAsync();
            var prod = await ctx.Productos.FirstOrDefaultAsync(p => p.IdProducto == idProducto);
            if (prod == null) throw new InvalidOperationException("Producto no encontrado");
            var dep = await ctx.Depositos.FirstOrDefaultAsync(d => d.IdDeposito == idDeposito);
            if (dep == null) throw new InvalidOperationException("Depósito no encontrado");

            ProductoDeposito? pd = null;
            var ajustarStockDirecto = !(tipo == 2 && prod.EsCombo); // si es salida de combo, no descuenta stock del combo
            if (ajustarStockDirecto)
            {
                pd = await ctx.ProductosDepositos
                    .FirstOrDefaultAsync(x => x.IdProducto == idProducto && x.IdDeposito == idDeposito);
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
                }

                var delta = tipo == 1 ? cantidad : -cantidad;
                // Validar que no quede saldo negativo
                if (pd.Stock + delta < 0)
                    throw new InvalidOperationException("Stock insuficiente en el depósito");

                pd.Stock += delta;
                pd.FechaModificacion = DateTime.Now;
                pd.UsuarioModificacion = usuario ?? "Sistema";
            }

            // Registrar movimiento
            ctx.MovimientosInventario.Add(new MovimientoInventario
            {
                IdProducto = idProducto,
                IdDeposito = idDeposito,
                Tipo = tipo,
                Cantidad = cantidad,
                Motivo = motivo,
                Usuario = usuario ?? "Sistema",
                Fecha = DateTime.Now
            });

            await ctx.SaveChangesAsync();

            // Si es una salida de un combo, descontar los componentes también
            if (tipo == 2 && prod.EsCombo)
            {
                var componentes = await ctx.ProductosComponentes
                    .AsNoTracking()
                    .Where(pc => pc.IdProducto == idProducto && pc.Activo)
                    .ToListAsync();
                if (componentes.Count > 0)
                {
                    _logger.LogInformation($"Producto {idProducto} es combo con {componentes.Count} componentes. Procesando recursión.");
                    foreach (var comp in componentes)
                    {
                        var cantDesc = comp.Cantidad * cantidad; // proporcional a la cantidad vendida del combo
                        _logger.LogInformation($"Descontando componente {comp.IdComponente}, cantidad: {cantDesc}");
                        
                        if (cantDesc <= 0)
                        {
                            _logger.LogWarning($"ADVERTENCIA: Cantidad de componente es {cantDesc}, saltando...");
                            continue;
                        }
                        
                        // Recursión simple: descontar componente como salida
                        await AjustarStockAsync(comp.IdComponente, idDeposito, cantDesc, 2, $"Desc. combo #{idProducto}", usuario);
                    }
                }
            }

            // Recalcular y persistir el stock total del producto como suma de todos los depósitos
            var total = await ctx.ProductosDepositos
                .Where(x => x.IdProducto == idProducto)
                .SumAsync(x => (decimal?)x.Stock) ?? 0m;
            prod.Stock = total;
            prod.FechaModificacion = DateTime.Now;
            prod.UsuarioModificacion = usuario ?? "Sistema";
            await ctx.SaveChangesAsync();
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
