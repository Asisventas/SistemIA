using Microsoft.EntityFrameworkCore;
using SistemIA.Data;
using SistemIA.Models;

namespace SistemIA.Services
{
    /// <summary>
    /// Servicio para gestión de lotes de productos con metodología FEFO (First Expired, First Out).
    /// Maneja asignación automática de lotes en ventas, creación en compras, y alertas de vencimiento.
    /// </summary>
    public interface ILoteService
    {
        /// <summary>
        /// Obtiene el stock total disponible en todos los lotes activos de un producto.
        /// Suma el stock de TODOS los lotes para validar si hay suficiente para vender.
        /// </summary>
        Task<decimal> ObtenerStockTotalFEFOAsync(int idProducto, int idDeposito);
        
        /// <summary>
        /// Obtiene todos los lotes activos ordenados por FEFO para distribuir una venta.
        /// Devuelve lista de lotes desde el más próximo a vencer hasta el más lejano.
        /// </summary>
        Task<List<ProductoLote>> ObtenerLotesFEFOParaVentaAsync(int idProducto, int idDeposito);
        
        /// <summary>
        /// Obtiene el primer lote FEFO con stock disponible (para preview/información).
        /// </summary>
        Task<ProductoLote?> ObtenerLoteFEFOAsync(int idProducto, int idDeposito, decimal cantidadRequerida);
        
        /// <summary>
        /// Descuenta stock de un lote y registra el movimiento.
        /// </summary>
        Task<bool> DescontarStockLoteAsync(int idProductoLote, decimal cantidad, string tipoDocumento, int? idDocumento, int? idDocumentoDetalle, string? referencia, string? usuario);
        
        /// <summary>
        /// Incrementa stock de un lote y registra el movimiento (para compras o devoluciones).
        /// </summary>
        Task<bool> IncrementarStockLoteAsync(int idProductoLote, decimal cantidad, string tipoDocumento, int? idDocumento, int? idDocumentoDetalle, string? referencia, string? usuario);
        
        /// <summary>
        /// Crea un nuevo lote para un producto (usado en compras).
        /// </summary>
        Task<ProductoLote> CrearLoteAsync(int idProducto, int idDeposito, string numeroLote, DateTime? fechaVencimiento, decimal stockInicial, decimal? costoUnitario, int? idCompra, int? idCompraDetalle, string? usuario);
        
        /// <summary>
        /// Busca un lote existente por número de lote.
        /// </summary>
        Task<ProductoLote?> BuscarLotePorNumeroAsync(int idProducto, int idDeposito, string numeroLote);
        
        /// <summary>
        /// Obtiene todos los lotes activos de un producto ordenados por FEFO.
        /// </summary>
        Task<List<ProductoLote>> ObtenerLotesProductoAsync(int idProducto, int? idDeposito = null, bool soloConStock = false);
        
        /// <summary>
        /// Obtiene productos con lotes próximos a vencer.
        /// </summary>
        Task<List<ProductoLoteAlertaDto>> ObtenerAlertasVencimientoAsync(int? idDeposito = null, int? diasAnticipacion = null);
        
        /// <summary>
        /// Obtiene lotes SIN fecha de vencimiento que tienen stock (bloqueados para venta).
        /// </summary>
        Task<List<LoteSinFechaDto>> ObtenerLotesSinFechaVencimientoAsync(int? idDeposito = null);
        
        /// <summary>
        /// Actualiza el estado de lotes vencidos o agotados.
        /// </summary>
        Task<int> ActualizarEstadosLotesAsync();
        
        /// <summary>
        /// Obtiene el resumen de stock por lotes de un producto.
        /// </summary>
        Task<ResumenStockLoteDto> ObtenerResumenStockLoteAsync(int idProducto, int? idDeposito = null);
        
        /// <summary>
        /// Revierte un movimiento de lote (para anulaciones).
        /// </summary>
        Task<bool> RevertirMovimientoAsync(string tipoDocumento, int idDocumento, string? usuario);
    }

    public class LoteService : ILoteService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        
        public LoteService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Obtiene el stock total disponible en lotes activos NO VENCIDOS y CON fecha de vencimiento.
        /// IMPORTANTE: Solo incluye lotes que tienen FechaVencimiento definida y no está vencida.
        /// Lotes sin fecha de vencimiento NO se incluyen (el usuario debe cargar la fecha primero).
        /// </summary>
        public async Task<decimal> ObtenerStockTotalFEFOAsync(int idProducto, int idDeposito)
        {
            await using var ctx = await _contextFactory.CreateDbContextAsync();
            var hoy = DateTime.Today;
            
            // FIX 2026-01-19: Solo incluir lotes que TIENEN fecha de vencimiento y NO están vencidos
            // Lotes sin fecha de vencimiento se excluyen porque el usuario debe cargarla primero
            return await ctx.ProductosLotes
                .Where(l => l.IdProducto == idProducto 
                         && l.IdDeposito == idDeposito 
                         && l.Estado == "Activo"
                         && l.Stock > 0
                         && l.FechaVencimiento.HasValue          // DEBE tener fecha de vencimiento
                         && l.FechaVencimiento.Value >= hoy)     // Y no estar vencido
                .SumAsync(l => l.Stock);
        }

        /// <summary>
        /// Obtiene lotes activos CON fecha de vencimiento válida ordenados por FEFO para ventas.
        /// IMPORTANTE: NO incluye lotes sin fecha de vencimiento (deben ser cargados primero).
        /// </summary>
        public async Task<List<ProductoLote>> ObtenerLotesFEFOParaVentaAsync(int idProducto, int idDeposito)
        {
            await using var ctx = await _contextFactory.CreateDbContextAsync();
            var hoy = DateTime.Today;
            
            // FIX 2026-01-19: Solo devolver lotes que TIENEN fecha de vencimiento válida
            // Lotes sin fecha NO se pueden vender hasta que el usuario la cargue
            return await ctx.ProductosLotes
                .Where(l => l.IdProducto == idProducto 
                         && l.IdDeposito == idDeposito 
                         && l.Estado == "Activo"
                         && l.Stock > 0
                         && l.FechaVencimiento.HasValue          // DEBE tener fecha
                         && l.FechaVencimiento.Value >= hoy)     // Y no estar vencido
                .OrderBy(l => l.FechaVencimiento)  // El más próximo a vencer primero (FEFO)
                .ThenBy(l => l.FechaIngreso)       // Si mismo vencimiento, el más antiguo
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene el primer lote FEFO CON fecha de vencimiento válida (para preview/información).
        /// IMPORTANTE: NO devuelve lotes sin fecha de vencimiento.
        /// </summary>
        public async Task<ProductoLote?> ObtenerLoteFEFOAsync(int idProducto, int idDeposito, decimal cantidadRequerida)
        {
            await using var ctx = await _contextFactory.CreateDbContextAsync();
            var hoy = DateTime.Today;
            
            // FIX 2026-01-19: Solo devolver lotes con fecha de vencimiento válida
            // Lotes sin fecha NO se pueden vender hasta que se cargue la fecha
            return await ctx.ProductosLotes
                .Where(l => l.IdProducto == idProducto 
                         && l.IdDeposito == idDeposito 
                         && l.Estado == "Activo"
                         && l.Stock > 0
                         && l.FechaVencimiento.HasValue          // DEBE tener fecha
                         && l.FechaVencimiento.Value >= hoy)     // Y no estar vencido
                .OrderBy(l => l.FechaVencimiento)  // El más próximo a vencer primero (FEFO)
                .ThenBy(l => l.FechaIngreso)       // Si mismo vencimiento, el más antiguo
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Descuenta stock de un lote específico y registra el movimiento de salida.
        /// </summary>
        public async Task<bool> DescontarStockLoteAsync(int idProductoLote, decimal cantidad, string tipoDocumento, int? idDocumento, int? idDocumentoDetalle, string? referencia, string? usuario)
        {
            if (cantidad <= 0) return false;
            
            await using var ctx = await _contextFactory.CreateDbContextAsync();
            
            var lote = await ctx.ProductosLotes.FindAsync(idProductoLote);
            if (lote == null || lote.Stock < cantidad)
                return false;
            
            var stockAnterior = lote.Stock;
            lote.Stock -= cantidad;
            
            // Actualizar estado si se agotó
            if (lote.Stock <= 0)
            {
                lote.Stock = 0;
                lote.Estado = "Agotado";
            }
            
            lote.FechaModificacion = DateTime.Now;
            lote.UsuarioModificacion = usuario;
            
            // Registrar movimiento
            var movimiento = new MovimientoLote
            {
                IdProductoLote = idProductoLote,
                TipoMovimiento = "Salida",
                Cantidad = -cantidad,  // Negativo para salidas
                StockAnterior = stockAnterior,
                StockPosterior = lote.Stock,
                TipoDocumento = tipoDocumento,
                IdDocumento = idDocumento,
                IdDocumentoDetalle = idDocumentoDetalle,
                ReferenciaDocumento = referencia,
                Motivo = $"Venta - {tipoDocumento}",
                FechaMovimiento = DateTime.Now,
                Usuario = usuario
            };
            
            ctx.MovimientosLotes.Add(movimiento);
            await ctx.SaveChangesAsync();
            
            return true;
        }

        /// <summary>
        /// Incrementa stock de un lote existente y registra el movimiento de entrada.
        /// </summary>
        public async Task<bool> IncrementarStockLoteAsync(int idProductoLote, decimal cantidad, string tipoDocumento, int? idDocumento, int? idDocumentoDetalle, string? referencia, string? usuario)
        {
            if (cantidad <= 0) return false;
            
            await using var ctx = await _contextFactory.CreateDbContextAsync();
            
            var lote = await ctx.ProductosLotes.FindAsync(idProductoLote);
            if (lote == null)
                return false;
            
            var stockAnterior = lote.Stock;
            lote.Stock += cantidad;
            
            // Si estaba agotado y ahora tiene stock, reactivar
            if (lote.Estado == "Agotado" && lote.Stock > 0)
            {
                // Verificar si no está vencido
                if (!lote.FechaVencimiento.HasValue || lote.FechaVencimiento.Value > DateTime.Today)
                    lote.Estado = "Activo";
            }
            
            lote.FechaModificacion = DateTime.Now;
            lote.UsuarioModificacion = usuario;
            
            // Registrar movimiento
            var movimiento = new MovimientoLote
            {
                IdProductoLote = idProductoLote,
                TipoMovimiento = "Entrada",
                Cantidad = cantidad,  // Positivo para entradas
                StockAnterior = stockAnterior,
                StockPosterior = lote.Stock,
                TipoDocumento = tipoDocumento,
                IdDocumento = idDocumento,
                IdDocumentoDetalle = idDocumentoDetalle,
                ReferenciaDocumento = referencia,
                Motivo = $"Entrada - {tipoDocumento}",
                FechaMovimiento = DateTime.Now,
                Usuario = usuario
            };
            
            ctx.MovimientosLotes.Add(movimiento);
            await ctx.SaveChangesAsync();
            
            return true;
        }

        /// <summary>
        /// Crea un nuevo lote para un producto.
        /// </summary>
        public async Task<ProductoLote> CrearLoteAsync(int idProducto, int idDeposito, string numeroLote, DateTime? fechaVencimiento, decimal stockInicial, decimal? costoUnitario, int? idCompra, int? idCompraDetalle, string? usuario)
        {
            await using var ctx = await _contextFactory.CreateDbContextAsync();
            
            var lote = new ProductoLote
            {
                IdProducto = idProducto,
                IdDeposito = idDeposito,
                NumeroLote = numeroLote,
                FechaVencimiento = fechaVencimiento,
                Stock = stockInicial,
                StockInicial = stockInicial,
                CostoUnitario = costoUnitario,
                IdCompra = idCompra,
                IdCompraDetalle = idCompraDetalle,
                FechaIngreso = DateTime.Now,
                Estado = "Activo",
                FechaCreacion = DateTime.Now,
                UsuarioCreacion = usuario
            };
            
            ctx.ProductosLotes.Add(lote);
            
            // Registrar movimiento de entrada inicial
            await ctx.SaveChangesAsync();  // Guardar primero para obtener el ID
            
            var movimiento = new MovimientoLote
            {
                IdProductoLote = lote.IdProductoLote,
                TipoMovimiento = "Entrada",
                Cantidad = stockInicial,
                StockAnterior = 0,
                StockPosterior = stockInicial,
                TipoDocumento = idCompra.HasValue ? "Compra" : "Ajuste",
                IdDocumento = idCompra,
                IdDocumentoDetalle = idCompraDetalle,
                ReferenciaDocumento = idCompra.HasValue ? $"Compra #{idCompra}" : "Stock inicial",
                Motivo = idCompra.HasValue ? "Ingreso por compra" : "Creación de lote inicial",
                FechaMovimiento = DateTime.Now,
                Usuario = usuario
            };
            
            ctx.MovimientosLotes.Add(movimiento);
            await ctx.SaveChangesAsync();
            
            return lote;
        }

        /// <summary>
        /// Busca un lote existente por número de lote para un producto y depósito.
        /// </summary>
        public async Task<ProductoLote?> BuscarLotePorNumeroAsync(int idProducto, int idDeposito, string numeroLote)
        {
            if (string.IsNullOrWhiteSpace(numeroLote)) return null;
            
            await using var ctx = await _contextFactory.CreateDbContextAsync();
            
            return await ctx.ProductosLotes
                .FirstOrDefaultAsync(l => l.IdProducto == idProducto 
                                       && l.IdDeposito == idDeposito 
                                       && l.NumeroLote == numeroLote);
        }

        /// <summary>
        /// Obtiene todos los lotes de un producto ordenados por FEFO.
        /// </summary>
        public async Task<List<ProductoLote>> ObtenerLotesProductoAsync(int idProducto, int? idDeposito = null, bool soloConStock = false)
        {
            await using var ctx = await _contextFactory.CreateDbContextAsync();
            
            var query = ctx.ProductosLotes
                .Include(l => l.Deposito)
                .Where(l => l.IdProducto == idProducto);
            
            if (idDeposito.HasValue)
                query = query.Where(l => l.IdDeposito == idDeposito.Value);
            
            if (soloConStock)
                query = query.Where(l => l.Stock > 0 && l.Estado == "Activo");
            
            return await query
                .OrderBy(l => l.FechaVencimiento ?? DateTime.MaxValue)  // Sin vencimiento al final
                .ThenBy(l => l.FechaIngreso)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene alertas de productos con lotes próximos a vencer.
        /// </summary>
        public async Task<List<ProductoLoteAlertaDto>> ObtenerAlertasVencimientoAsync(int? idDeposito = null, int? diasAnticipacion = null)
        {
            await using var ctx = await _contextFactory.CreateDbContextAsync();
            
            var hoy = DateTime.Today;
            
            var query = from lote in ctx.ProductosLotes
                        join producto in ctx.Productos on lote.IdProducto equals producto.IdProducto
                        join deposito in ctx.Depositos on lote.IdDeposito equals deposito.IdDeposito
                        where lote.Stock > 0 
                           && lote.Estado == "Activo"
                           && lote.FechaVencimiento.HasValue
                        select new { lote, producto, deposito };
            
            if (idDeposito.HasValue)
                query = query.Where(x => x.lote.IdDeposito == idDeposito.Value);
            
            var alertas = new List<ProductoLoteAlertaDto>();
            var datos = await query.ToListAsync();
            
            foreach (var item in datos)
            {
                var diasParaVencer = (item.lote.FechaVencimiento!.Value - hoy).Days;
                var diasAlerta = diasAnticipacion ?? item.producto.DiasAlertaVencimiento;
                
                // Solo incluir si está dentro del rango de alerta o ya venció
                if (diasParaVencer <= diasAlerta)
                {
                    alertas.Add(new ProductoLoteAlertaDto
                    {
                        IdProductoLote = item.lote.IdProductoLote,
                        IdProducto = item.producto.IdProducto,
                        IdDeposito = item.deposito.IdDeposito,
                        CodigoProducto = item.producto.CodigoInterno,
                        DescripcionProducto = item.producto.Descripcion ?? "",
                        NumeroLote = item.lote.NumeroLote,
                        FechaVencimiento = item.lote.FechaVencimiento!.Value,
                        DiasParaVencer = diasParaVencer,
                        Stock = item.lote.Stock,
                        CostoUnitario = item.lote.CostoUnitario ?? 0m,
                        NombreDeposito = item.deposito.Nombre ?? "",
                        Estado = diasParaVencer < 0 ? "Vencido" : 
                                 diasParaVencer == 0 ? "Vence Hoy" :
                                 diasParaVencer <= 7 ? "Crítico" :
                                 diasParaVencer <= 15 ? "Urgente" : "Alerta",
                        PermiteVentaVencido = item.producto.PermiteVentaVencido
                    });
                }
            }
            
            return alertas.OrderBy(a => a.DiasParaVencer).ToList();
        }

        /// <summary>
        /// Actualiza el estado de lotes vencidos o agotados automáticamente.
        /// </summary>
        public async Task<int> ActualizarEstadosLotesAsync()
        {
            await using var ctx = await _contextFactory.CreateDbContextAsync();
            
            var hoy = DateTime.Today;
            var actualizados = 0;
            
            // Marcar lotes vencidos
            var lotesVencidos = await ctx.ProductosLotes
                .Where(l => l.Estado == "Activo" 
                         && l.FechaVencimiento.HasValue 
                         && l.FechaVencimiento.Value < hoy)
                .ToListAsync();
            
            foreach (var lote in lotesVencidos)
            {
                lote.Estado = "Vencido";
                lote.FechaModificacion = DateTime.Now;
                actualizados++;
            }
            
            // Marcar lotes agotados
            var lotesAgotados = await ctx.ProductosLotes
                .Where(l => l.Estado == "Activo" && l.Stock <= 0)
                .ToListAsync();
            
            foreach (var lote in lotesAgotados)
            {
                lote.Estado = "Agotado";
                lote.FechaModificacion = DateTime.Now;
                actualizados++;
            }
            
            if (actualizados > 0)
                await ctx.SaveChangesAsync();
            
            return actualizados;
        }

        /// <summary>
        /// Obtiene resumen de stock por lotes de un producto.
        /// </summary>
        public async Task<ResumenStockLoteDto> ObtenerResumenStockLoteAsync(int idProducto, int? idDeposito = null)
        {
            await using var ctx = await _contextFactory.CreateDbContextAsync();
            
            var query = ctx.ProductosLotes.Where(l => l.IdProducto == idProducto);
            
            if (idDeposito.HasValue)
                query = query.Where(l => l.IdDeposito == idDeposito.Value);
            
            var lotes = await query.ToListAsync();
            var hoy = DateTime.Today;
            
            return new ResumenStockLoteDto
            {
                IdProducto = idProducto,
                TotalLotes = lotes.Count,
                LotesActivos = lotes.Count(l => l.Estado == "Activo"),
                LotesVencidos = lotes.Count(l => l.Estado == "Vencido"),
                LotesAgotados = lotes.Count(l => l.Estado == "Agotado"),
                StockTotal = lotes.Where(l => l.Estado == "Activo").Sum(l => l.Stock),
                StockVencido = lotes.Where(l => l.Estado == "Vencido").Sum(l => l.Stock),
                ProximoVencimiento = lotes
                    .Where(l => l.Estado == "Activo" && l.FechaVencimiento.HasValue && l.FechaVencimiento.Value > hoy)
                    .OrderBy(l => l.FechaVencimiento)
                    .Select(l => l.FechaVencimiento)
                    .FirstOrDefault()
            };
        }

        /// <summary>
        /// Revierte movimientos de lote asociados a un documento (para anulaciones).
        /// </summary>
        public async Task<bool> RevertirMovimientoAsync(string tipoDocumento, int idDocumento, string? usuario)
        {
            await using var ctx = await _contextFactory.CreateDbContextAsync();
            
            // Buscar movimientos del documento
            var movimientos = await ctx.MovimientosLotes
                .Include(m => m.ProductoLote)
                .Where(m => m.TipoDocumento == tipoDocumento && m.IdDocumento == idDocumento)
                .ToListAsync();
            
            if (!movimientos.Any())
                return true;  // No hay nada que revertir
            
            foreach (var mov in movimientos)
            {
                var lote = mov.ProductoLote;
                if (lote == null) continue;
                
                var stockAnterior = lote.Stock;
                
                // Revertir: si fue salida (negativo), sumamos; si fue entrada (positivo), restamos
                lote.Stock -= mov.Cantidad;
                
                // Asegurar que no quede negativo
                if (lote.Stock < 0) lote.Stock = 0;
                
                // Actualizar estado
                if (lote.Stock > 0 && lote.Estado == "Agotado")
                {
                    if (!lote.FechaVencimiento.HasValue || lote.FechaVencimiento.Value > DateTime.Today)
                        lote.Estado = "Activo";
                }
                else if (lote.Stock <= 0)
                {
                    lote.Estado = "Agotado";
                }
                
                lote.FechaModificacion = DateTime.Now;
                lote.UsuarioModificacion = usuario;
                
                // Registrar movimiento de reversión
                var movReversion = new MovimientoLote
                {
                    IdProductoLote = lote.IdProductoLote,
                    TipoMovimiento = mov.Cantidad < 0 ? "Entrada" : "Salida",  // Inverso al original
                    Cantidad = -mov.Cantidad,  // Inverso
                    StockAnterior = stockAnterior,
                    StockPosterior = lote.Stock,
                    TipoDocumento = "Anulacion",
                    IdDocumento = idDocumento,
                    ReferenciaDocumento = $"Anulación de {tipoDocumento} #{idDocumento}",
                    Motivo = $"Reversión por anulación",
                    FechaMovimiento = DateTime.Now,
                    Usuario = usuario
                };
                
                ctx.MovimientosLotes.Add(movReversion);
            }
            
            await ctx.SaveChangesAsync();
            return true;
        }
        
        /// <summary>
        /// Obtiene lotes SIN fecha de vencimiento que tienen stock (bloqueados para venta).
        /// Estos lotes requieren que se ingrese la fecha de vencimiento para poder venderse.
        /// </summary>
        public async Task<List<LoteSinFechaDto>> ObtenerLotesSinFechaVencimientoAsync(int? idDeposito = null)
        {
            await using var ctx = await _contextFactory.CreateDbContextAsync();
            
            var query = from lote in ctx.ProductosLotes
                        join producto in ctx.Productos on lote.IdProducto equals producto.IdProducto
                        join deposito in ctx.Depositos on lote.IdDeposito equals deposito.IdDeposito
                        where lote.Stock > 0 
                           && lote.Estado == "Activo"
                           && !lote.FechaVencimiento.HasValue  // SIN fecha de vencimiento
                        select new LoteSinFechaDto
                        {
                            IdProductoLote = lote.IdProductoLote,
                            IdProducto = producto.IdProducto,
                            IdDeposito = deposito.IdDeposito,
                            CodigoProducto = producto.CodigoInterno,
                            DescripcionProducto = producto.Descripcion ?? "",
                            NumeroLote = lote.NumeroLote,
                            FechaIngreso = lote.FechaIngreso,
                            Stock = lote.Stock,
                            CostoUnitario = lote.CostoUnitario ?? 0m,
                            NombreDeposito = deposito.Nombre ?? "",
                            ControlaVencimiento = producto.ControlarVencimiento,
                            DiasAlertaVencimiento = producto.DiasAlertaVencimiento
                        };
            
            if (idDeposito.HasValue)
                query = query.Where(x => x.IdDeposito == idDeposito.Value);
            
            return await query.OrderBy(l => l.DescripcionProducto)
                              .ThenBy(l => l.NumeroLote)
                              .ToListAsync();
        }
    }

    // ========== DTOs ==========

    /// <summary>
    /// DTO para alertas de vencimiento de lotes.
    /// </summary>
    public class ProductoLoteAlertaDto
    {
        public int IdProductoLote { get; set; }
        public int IdProducto { get; set; }
        public int IdDeposito { get; set; }
        public string? CodigoProducto { get; set; }
        public string DescripcionProducto { get; set; } = "";
        public string NumeroLote { get; set; } = "";
        public DateTime FechaVencimiento { get; set; }
        public int DiasParaVencer { get; set; }
        public decimal Stock { get; set; }
        public decimal CostoUnitario { get; set; }
        public decimal ValorStock => Stock * CostoUnitario;
        public string NombreDeposito { get; set; } = "";
        public string Estado { get; set; } = "";  // Vencido, Vence Hoy, Crítico, Urgente, Alerta
        public bool PermiteVentaVencido { get; set; }
    }

    /// <summary>
    /// DTO para lotes SIN fecha de vencimiento (bloqueados para venta).
    /// </summary>
    public class LoteSinFechaDto
    {
        public int IdProductoLote { get; set; }
        public int IdProducto { get; set; }
        public int IdDeposito { get; set; }
        public string? CodigoProducto { get; set; }
        public string DescripcionProducto { get; set; } = "";
        public string NumeroLote { get; set; } = "";
        public DateTime FechaIngreso { get; set; }
        public decimal Stock { get; set; }
        public decimal CostoUnitario { get; set; }
        public decimal ValorStock => Stock * CostoUnitario;
        public string NombreDeposito { get; set; } = "";
        public bool ControlaVencimiento { get; set; }
        public int DiasAlertaVencimiento { get; set; }
    }

    /// <summary>
    /// DTO para resumen de stock por lotes.
    /// </summary>
    public class ResumenStockLoteDto
    {
        public int IdProducto { get; set; }
        public int TotalLotes { get; set; }
        public int LotesActivos { get; set; }
        public int LotesVencidos { get; set; }
        public int LotesAgotados { get; set; }
        public decimal StockTotal { get; set; }
        public decimal StockVencido { get; set; }
        public DateTime? ProximoVencimiento { get; set; }
    }
}
