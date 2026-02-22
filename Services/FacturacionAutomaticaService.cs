using Microsoft.EntityFrameworkCore;
using SistemIA.Models;
using SistemIA.Models.Suscripciones;
using System.Globalization;

namespace SistemIA.Services
{
    public interface IFacturacionAutomaticaService
    {
        /// <summary>
        /// Procesa las suscripciones pendientes y genera las facturas correspondientes
        /// </summary>
        Task<ResultadoProcesoFacturacion> ProcesarSuscripcionesPendientesAsync(int? idSucursal = null);

        /// <summary>
        /// Genera factura para una suscripción específica
        /// </summary>
        Task<ResultadoGeneracionFactura> GenerarFacturaAsync(int idSuscripcion);

        /// <summary>
        /// Reintenta generar facturas que fallaron previamente
        /// </summary>
        Task<ResultadoProcesoFacturacion> ReintentarFacturasConErrorAsync(int? idSucursal = null, int maxIntentos = 3);

        /// <summary>
        /// Envía correos pendientes de facturas generadas
        /// </summary>
        Task<int> EnviarCorreosPendientesAsync();

        /// <summary>
        /// Actualiza el estado de cobro cuando se detecta un pago
        /// </summary>
        Task ActualizarEstadoCobroAsync(int idVenta, decimal montoCobrado);

        /// <summary>
        /// Obtiene estadísticas de facturación para el dashboard
        /// </summary>
        Task<EstadisticasFacturacion> ObtenerEstadisticasAsync(int? idSucursal = null, DateTime? desde = null, DateTime? hasta = null);

        /// <summary>
        /// Obtiene suscripciones próximas a facturar
        /// </summary>
        Task<List<SuscripcionCliente>> ObtenerSuscripcionesProximasAsync(int? idSucursal = null, int dias = 7);
    }

    public class FacturacionAutomaticaService : IFacturacionAutomaticaService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ICorreoService? _correoService;
        private readonly ILogger<FacturacionAutomaticaService> _logger;

        public FacturacionAutomaticaService(
            IDbContextFactory<AppDbContext> contextFactory,
            ICorreoService? correoService,
            ILogger<FacturacionAutomaticaService> logger)
        {
            _contextFactory = contextFactory;
            _correoService = correoService;
            _logger = logger;
        }

        public async Task<ResultadoProcesoFacturacion> ProcesarSuscripcionesPendientesAsync(int? idSucursal = null)
        {
            var resultado = new ResultadoProcesoFacturacion
            {
                FechaProceso = DateTime.Now
            };

            try
            {
                await using var db = await _contextFactory.CreateDbContextAsync();
                var hoy = DateTime.Today;

                // Obtener suscripciones activas cuya próxima factura es hoy o antes
                var suscripcionesPendientes = await db.SuscripcionesClientes
                    .Include(s => s.Cliente)
                    .Include(s => s.Producto)
                    .Include(s => s.Sucursal)
                    .Include(s => s.Caja)
                    .Where(s => s.Estado == "Activa" 
                             && s.FacturacionActiva 
                             && s.FechaProximaFactura != null 
                             && s.FechaProximaFactura.Value.Date <= hoy
                             && (idSucursal == null || s.IdSucursal == idSucursal))
                    .ToListAsync();

                resultado.TotalProcesadas = suscripcionesPendientes.Count;

                foreach (var suscripcion in suscripcionesPendientes)
                {
                    try
                    {
                        var resultadoFactura = await GenerarFacturaInternoAsync(db, suscripcion);
                        if (resultadoFactura.Exito)
                        {
                            resultado.Exitosas++;
                        }
                        else
                        {
                            resultado.ConErrores++;
                            resultado.Errores.Add($"Suscripción {suscripcion.IdSuscripcion}: {resultadoFactura.Mensaje}");
                        }
                    }
                    catch (Exception ex)
                    {
                        resultado.ConErrores++;
                        resultado.Errores.Add($"Suscripción {suscripcion.IdSuscripcion}: {ex.Message}");
                        _logger.LogError(ex, "Error procesando suscripción {IdSuscripcion}", suscripcion.IdSuscripcion);
                    }
                }

                await db.SaveChangesAsync();
                resultado.Exitoso = true;
            }
            catch (Exception ex)
            {
                resultado.Exitoso = false;
                resultado.MensajeGeneral = ex.Message;
                _logger.LogError(ex, "Error en proceso de facturación automática");
            }

            return resultado;
        }

        public async Task<ResultadoGeneracionFactura> GenerarFacturaAsync(int idSuscripcion)
        {
            await using var db = await _contextFactory.CreateDbContextAsync();

            var suscripcion = await db.SuscripcionesClientes
                .Include(s => s.Cliente)
                .Include(s => s.Producto)
                .Include(s => s.Sucursal)
                .Include(s => s.Caja)
                .FirstOrDefaultAsync(s => s.IdSuscripcion == idSuscripcion);

            if (suscripcion == null)
            {
                return new ResultadoGeneracionFactura
                {
                    Exito = false,
                    Mensaje = "Suscripción no encontrada"
                };
            }

            var resultado = await GenerarFacturaInternoAsync(db, suscripcion);
            await db.SaveChangesAsync();
            return resultado;
        }

        private async Task<ResultadoGeneracionFactura> GenerarFacturaInternoAsync(AppDbContext db, SuscripcionCliente suscripcion)
        {
            var resultado = new ResultadoGeneracionFactura();

            try
            {
                // Validaciones
                if (suscripcion.Cliente == null)
                {
                    return new ResultadoGeneracionFactura { Exito = false, Mensaje = "Cliente no encontrado" };
                }

                // Verificar si hay venta de referencia o producto
                bool usarVentaReferencia = suscripcion.IdVentaReferencia.HasValue;
                Venta? ventaRef = null;
                List<VentaDetalle>? detallesVentaRef = null;
                
                if (usarVentaReferencia)
                {
                    ventaRef = await db.Ventas
                        .FirstOrDefaultAsync(v => v.IdVenta == suscripcion.IdVentaReferencia);
                    
                    if (ventaRef != null)
                    {
                        // Cargar detalles por separado para evitar problemas con EF Core
                        detallesVentaRef = await db.VentasDetalles
                            .Include(d => d.Producto)
                            .Where(d => d.IdVenta == ventaRef.IdVenta)
                            .ToListAsync();
                    }
                    
                    if (ventaRef == null || detallesVentaRef == null || !detallesVentaRef.Any())
                    {
                        return new ResultadoGeneracionFactura { Exito = false, Mensaje = "Venta de referencia no encontrada o sin detalles" };
                    }
                }
                else if (suscripcion.Producto == null)
                {
                    return new ResultadoGeneracionFactura { Exito = false, Mensaje = "Producto no encontrado" };
                }

                // Calcular período
                var fechaInicioPeriodo = suscripcion.FechaUltimaFactura?.AddDays(1) ?? suscripcion.FechaInicio;
                var fechaFinPeriodo = suscripcion.CalcularProximaFechaFactura().AddDays(-1);
                var periodoTexto = ObtenerTextoPeriodo(fechaInicioPeriodo, suscripcion.TipoPeriodo);

                // Calcular monto total
                decimal montoTotal = usarVentaReferencia 
                    ? ventaRef!.Total 
                    : (suscripcion.MontoFacturar * suscripcion.Cantidad);
                
                // Crear registro de factura automática
                var facturaAuto = new FacturaAutomatica
                {
                    IdSuscripcion = suscripcion.IdSuscripcion,
                    IdSucursal = suscripcion.IdSucursal,
                    IdCliente = suscripcion.IdCliente,
                    PeriodoFacturado = periodoTexto,
                    FechaInicioPeriodo = fechaInicioPeriodo,
                    FechaFinPeriodo = fechaFinPeriodo,
                    MontoFacturado = montoTotal,
                    FechaProgramada = suscripcion.FechaProximaFactura ?? DateTime.Today,
                    EstadoFactura = "Pendiente",
                    EstadoCobro = "Pendiente",
                    EstadoCorreoFactura = suscripcion.EnviarCorreoFactura ? "Pendiente" : "NoAplica",
                    EstadoCorreoRecibo = suscripcion.EnviarCorreoRecibo ? "Pendiente" : "NoAplica",
                    FechaCreacion = DateTime.Now
                };

                db.FacturasAutomaticas.Add(facturaAuto);

                // Obtener numeración de factura - usar caja de suscripción o primera de la sucursal
                var caja = suscripcion.Caja ?? await db.Cajas.FirstOrDefaultAsync(c => c.IdSucursal == suscripcion.IdSucursal);
                if (caja == null)
                {
                    facturaAuto.EstadoFactura = "ErrorGeneracion";
                    facturaAuto.MensajeError = "No se encontró caja para facturar";
                    facturaAuto.IntentosGeneracion++;
                    return new ResultadoGeneracionFactura { Exito = false, Mensaje = facturaAuto.MensajeError };
                }

                // Obtener siguiente número de factura
                // Nivel1 = Establecimiento, Nivel2 = Punto de Expedición
                var establecimiento = caja.Nivel1 ?? "001";
                var puntoExpedicion = caja.Nivel2 ?? "001";
                
                var ultimoNumero = await db.Ventas
                    .Where(v => v.Establecimiento == establecimiento && v.PuntoExpedicion == puntoExpedicion)
                    .Select(v => v.NumeroFactura)
                    .OrderByDescending(n => n)
                    .FirstOrDefaultAsync();
                
                int nuevoNumero = 1;
                if (!string.IsNullOrEmpty(ultimoNumero) && int.TryParse(ultimoNumero, out var ultimo))
                {
                    nuevoNumero = ultimo + 1;
                }

                // Determinar condición de pago
                var condicionPago = usarVentaReferencia 
                    ? (suscripcion.CondicionPago ?? ventaRef!.FormaPago ?? "Contado")
                    : "Crédito";
                var esCredito = condicionPago.Contains("Crédito", StringComparison.OrdinalIgnoreCase);

                // Crear la venta base
                var venta = new Venta
                {
                    IdSucursal = suscripcion.IdSucursal,
                    IdCaja = caja.IdCaja,
                    Establecimiento = establecimiento,
                    PuntoExpedicion = puntoExpedicion,
                    NumeroFactura = nuevoNumero.ToString("0000000"),
                    Serie = caja.Serie,
                    Timbrado = caja.Timbrado,
                    IdCliente = suscripcion.IdCliente,
                    Fecha = DateTime.Now,
                    Turno = caja.TurnoActual ?? 1,
                    IdMoneda = usarVentaReferencia ? (ventaRef!.IdMoneda ?? 1) : 1,
                    CambioDelDia = usarVentaReferencia ? (ventaRef!.CambioDelDia ?? 1) : 1,
                    FormaPago = condicionPago,
                    CodigoCondicion = esCredito ? "CREDITO" : "CONTADO",
                    MedioPago = usarVentaReferencia ? (ventaRef!.MedioPago ?? "EFECTIVO") : "EFECTIVO",
                    Estado = "Confirmada",
                    TipoDocumento = "Factura",
                    TipoIngreso = "VENTAS",
                    Comentario = $"Factura automática - Suscripción #{suscripcion.IdSuscripcion} - {periodoTexto}"
                };

                // Si hay venta de referencia, copiar datos adicionales
                if (usarVentaReferencia && ventaRef != null)
                {
                    venta.Total = ventaRef.Total;
                    venta.IdTipoPago = ventaRef.IdTipoPago;
                    venta.Plazo = ventaRef.Plazo;
                }
                else
                {
                    // Cálculo original con producto único
                    venta.Total = suscripcion.MontoFacturar * suscripcion.Cantidad;
                }

                db.Ventas.Add(venta);
                await db.SaveChangesAsync(); // Guardar para obtener el ID

                // Crear detalles de la venta
                if (usarVentaReferencia && ventaRef != null)
                {
                    // Cargar los detalles de la venta de referencia
                    var detallesRef = await db.VentasDetalles
                        .Where(d => d.IdVenta == ventaRef.IdVenta)
                        .ToListAsync();
                    
                    // Copiar todos los detalles de la venta de referencia
                    foreach (var detRef in detallesRef)
                    {
                        var detalle = new VentaDetalle
                        {
                            IdVenta = venta.IdVenta,
                            IdProducto = detRef.IdProducto,
                            Cantidad = detRef.Cantidad,
                            PrecioUnitario = detRef.PrecioUnitario,
                            Importe = detRef.Importe,
                            Exenta = detRef.Exenta,
                            Grabado5 = detRef.Grabado5,
                            Grabado10 = detRef.Grabado10,
                            IVA5 = detRef.IVA5,
                            IVA10 = detRef.IVA10,
                            IdTipoIva = detRef.IdTipoIva,
                            PorcentajeDescuento = detRef.PorcentajeDescuento
                        };
                        db.VentasDetalles.Add(detalle);
                    }
                }
                else
                {
                    // Crear detalle único con el producto de la suscripción
                    var montoLinea = suscripcion.MontoFacturar * suscripcion.Cantidad;
                    decimal gravado10 = 0, gravado5 = 0, exenta = 0, iva10 = 0, iva5 = 0;
                    
                    switch (suscripcion.Producto?.IdTipoIva)
                    {
                        case 1: iva10 = Math.Round(montoLinea / 11m, 0); gravado10 = montoLinea - iva10; break;
                        case 2: iva5 = Math.Round(montoLinea / 21m, 0); gravado5 = montoLinea - iva5; break;
                        default: exenta = montoLinea; break;
                    }
                    
                    var detalle = new VentaDetalle
                    {
                        IdVenta = venta.IdVenta,
                        IdProducto = suscripcion.IdProducto ?? 0,
                        Cantidad = suscripcion.Cantidad,
                        PrecioUnitario = suscripcion.MontoFacturar,
                        Importe = montoLinea,
                        Exenta = exenta,
                        Grabado5 = gravado5,
                        Grabado10 = gravado10,
                        IVA5 = iva5,
                        IVA10 = iva10,
                        IdTipoIva = suscripcion.Producto?.IdTipoIva
                    };
                    db.VentasDetalles.Add(detalle);
                }

                await db.SaveChangesAsync();

                // Actualizar factura automática
                facturaAuto.IdVenta = venta.IdVenta;
                facturaAuto.NumeroFactura = $"{establecimiento}-{puntoExpedicion}-{nuevoNumero:0000000}";
                facturaAuto.EstadoFactura = "Generada";
                facturaAuto.FechaGeneracion = DateTime.Now;
                facturaAuto.EstadoCorreoFactura = suscripcion.EnviarCorreoFactura ? "Pendiente" : "NoAplica";
                facturaAuto.MontoFacturado = venta.Total;

                // Si es CONTADO, marcar como cobrado automáticamente
                if (!esCredito)
                {
                    facturaAuto.EstadoCobro = "Cobrado";
                    facturaAuto.FechaCobro = DateTime.Now;
                    facturaAuto.MontoCobrado = venta.Total;
                }
                else
                {
                    facturaAuto.EstadoCobro = "Pendiente";
                }

                // Actualizar suscripción
                suscripcion.FechaUltimaFactura = DateTime.Now;
                suscripcion.FechaProximaFactura = suscripcion.CalcularProximaFechaFactura();
                suscripcion.TotalFacturasGeneradas++;
                suscripcion.FechaModificacion = DateTime.Now;

                resultado.Exito = true;
                resultado.IdVenta = venta.IdVenta;
                resultado.IdFacturaAutomatica = facturaAuto.IdFacturaAutomatica;
                resultado.NumeroFactura = facturaAuto.NumeroFactura;
                resultado.Mensaje = $"Factura {facturaAuto.NumeroFactura} generada correctamente";

                _logger.LogInformation("Factura automática generada: {NumeroFactura} para cliente {IdCliente}", 
                    facturaAuto.NumeroFactura, suscripcion.IdCliente);
            }
            catch (Exception ex)
            {
                resultado.Exito = false;
                resultado.Mensaje = ex.Message;
                _logger.LogError(ex, "Error generando factura automática para suscripción {IdSuscripcion}", suscripcion.IdSuscripcion);
            }

            return resultado;
        }

        public async Task<ResultadoProcesoFacturacion> ReintentarFacturasConErrorAsync(int? idSucursal = null, int maxIntentos = 3)
        {
            var resultado = new ResultadoProcesoFacturacion { FechaProceso = DateTime.Now };

            try
            {
                await using var db = await _contextFactory.CreateDbContextAsync();

                var facturasConError = await db.FacturasAutomaticas
                    .Include(f => f.Suscripcion)
                        .ThenInclude(s => s!.Cliente)
                    .Include(f => f.Suscripcion)
                        .ThenInclude(s => s!.Producto)
                    .Include(f => f.Suscripcion)
                        .ThenInclude(s => s!.Caja)
                    .Where(f => f.EstadoFactura == "ErrorGeneracion" 
                             && f.IntentosGeneracion < maxIntentos
                             && (idSucursal == null || f.IdSucursal == idSucursal))
                    .ToListAsync();

                resultado.TotalProcesadas = facturasConError.Count;

                foreach (var factura in facturasConError)
                {
                    if (factura.Suscripcion == null) continue;

                    try
                    {
                        factura.IntentosGeneracion++;
                        var resultadoGeneracion = await GenerarFacturaInternoAsync(db, factura.Suscripcion);

                        if (resultadoGeneracion.Exito)
                        {
                            factura.EstadoFactura = "Generada";
                            factura.FechaGeneracion = DateTime.Now;
                            factura.IdVenta = resultadoGeneracion.IdVenta;
                            factura.NumeroFactura = resultadoGeneracion.NumeroFactura;
                            factura.MensajeError = null;
                            resultado.Exitosas++;
                        }
                        else
                        {
                            factura.MensajeError = resultadoGeneracion.Mensaje;
                            resultado.ConErrores++;
                        }
                    }
                    catch (Exception ex)
                    {
                        factura.MensajeError = ex.Message;
                        resultado.ConErrores++;
                        _logger.LogError(ex, "Error reintentando factura {IdFactura}", factura.IdFacturaAutomatica);
                    }
                }

                await db.SaveChangesAsync();
                resultado.Exitoso = true;
            }
            catch (Exception ex)
            {
                resultado.Exitoso = false;
                resultado.MensajeGeneral = ex.Message;
                _logger.LogError(ex, "Error en reintento de facturas con error");
            }

            return resultado;
        }

        public async Task<int> EnviarCorreosPendientesAsync()
        {
            if (_correoService == null) return 0;

            var enviados = 0;
            await using var db = await _contextFactory.CreateDbContextAsync();

            var facturasPendientesCorreo = await db.FacturasAutomaticas
                .Include(f => f.Suscripcion)
                    .ThenInclude(s => s!.Cliente)
                .Include(f => f.Venta)
                .Where(f => f.EstadoCorreoFactura == "Pendiente" && f.IdVenta != null)
                .Take(50)
                .ToListAsync();

            foreach (var factura in facturasPendientesCorreo)
            {
                try
                {
                    var email = factura.Suscripcion?.Cliente?.Email;
                    if (string.IsNullOrWhiteSpace(email))
                    {
                        factura.EstadoCorreoFactura = "SinCorreo";
                        continue;
                    }

                    // Aquí se enviaría el correo usando _correoService
                    // Por ahora marcamos como enviado
                    factura.EstadoCorreoFactura = "Enviado";
                    factura.FechaEnvioCorreoFactura = DateTime.Now;
                    enviados++;
                }
                catch (Exception ex)
                {
                    factura.EstadoCorreoFactura = "Error";
                    _logger.LogError(ex, "Error enviando correo de factura {IdFactura}", factura.IdFacturaAutomatica);
                }
            }

            await db.SaveChangesAsync();
            return enviados;
        }

        public async Task ActualizarEstadoCobroAsync(int idVenta, decimal montoCobrado)
        {
            await using var db = await _contextFactory.CreateDbContextAsync();

            var factura = await db.FacturasAutomaticas
                .FirstOrDefaultAsync(f => f.IdVenta == idVenta);

            if (factura == null) return;

            factura.MontoCobrado = montoCobrado;

            if (montoCobrado >= factura.MontoFacturado)
            {
                factura.EstadoCobro = "Cobrado";
                factura.FechaCobro = DateTime.Now;
            }
            else if (montoCobrado > 0)
            {
                factura.EstadoCobro = "Parcial";
            }
            else
            {
                factura.EstadoCobro = "Pendiente";
            }

            await db.SaveChangesAsync();
        }

        public async Task<EstadisticasFacturacion> ObtenerEstadisticasAsync(int? idSucursal = null, DateTime? desde = null, DateTime? hasta = null)
        {
            await using var db = await _contextFactory.CreateDbContextAsync();

            var desdeDate = desde ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var hastaDate = hasta ?? DateTime.Today;

            var stats = new EstadisticasFacturacion();

            // Suscripciones activas
            stats.TotalSuscripcionesActivas = await db.SuscripcionesClientes
                .CountAsync(s => s.Estado == "Activa" && s.FacturacionActiva && (idSucursal == null || s.IdSucursal == idSucursal));

            // Facturas pendientes hoy
            var hoy = DateTime.Today;
            stats.SuscripcionesPendientesHoy = await db.SuscripcionesClientes
                .CountAsync(s => s.Estado == "Activa" && s.FacturacionActiva 
                              && s.FechaProximaFactura != null && s.FechaProximaFactura.Value.Date <= hoy
                              && (idSucursal == null || s.IdSucursal == idSucursal));

            // Facturas del período
            stats.TotalFacturasGeneradas = await db.FacturasAutomaticas
                .CountAsync(f => f.EstadoFactura == "Generada" 
                              && f.FechaCreacion >= desdeDate && f.FechaCreacion <= hastaDate
                              && (idSucursal == null || f.IdSucursal == idSucursal));

            stats.TotalFacturasConError = await db.FacturasAutomaticas
                .CountAsync(f => f.EstadoFactura == "ErrorGeneracion"
                              && (idSucursal == null || f.IdSucursal == idSucursal));

            stats.TotalMonto = await db.FacturasAutomaticas
                .Where(f => f.EstadoFactura == "Generada" 
                         && f.FechaCreacion >= desdeDate && f.FechaCreacion <= hastaDate
                         && (idSucursal == null || f.IdSucursal == idSucursal))
                .SumAsync(f => (decimal?)f.MontoFacturado) ?? 0;

            stats.TotalCobrado = await db.FacturasAutomaticas
                .Where(f => f.EstadoCobro == "Cobrado"
                         && f.FechaCreacion >= desdeDate && f.FechaCreacion <= hastaDate
                         && (idSucursal == null || f.IdSucursal == idSucursal))
                .SumAsync(f => (decimal?)f.MontoFacturado) ?? 0;

            return stats;
        }

        public async Task<List<SuscripcionCliente>> ObtenerSuscripcionesProximasAsync(int? idSucursal = null, int dias = 7)
        {
            await using var db = await _contextFactory.CreateDbContextAsync();

            var fechaLimite = DateTime.Today.AddDays(dias);

            return await db.SuscripcionesClientes
                .Include(s => s.Cliente)
                .Include(s => s.Producto)
                .Where(s => s.Estado == "Activa" 
                         && s.FacturacionActiva 
                         && s.FechaProximaFactura != null 
                         && s.FechaProximaFactura.Value.Date <= fechaLimite
                         && (idSucursal == null || s.IdSucursal == idSucursal))
                .OrderBy(s => s.FechaProximaFactura)
                .Take(20)
                .ToListAsync();
        }

        private static string ObtenerTextoPeriodo(DateTime fecha, string tipoPeriodo)
        {
            var cultura = new CultureInfo("es-PY");
            return tipoPeriodo switch
            {
                "Anual" => fecha.Year.ToString(),
                "Semestral" => fecha.Month <= 6 ? $"1er Sem. {fecha.Year}" : $"2do Sem. {fecha.Year}",
                "Trimestral" => $"Q{((fecha.Month - 1) / 3) + 1} {fecha.Year}",
                "Bimestral" => $"Bim. {((fecha.Month - 1) / 2) + 1} {fecha.Year}",
                _ => cultura.DateTimeFormat.GetMonthName(fecha.Month) + " " + fecha.Year
            };
        }
    }

    // ========== DTOs ==========

    public class ResultadoProcesoFacturacion
    {
        public bool Exitoso { get; set; }
        public DateTime FechaProceso { get; set; }
        public int TotalProcesadas { get; set; }
        public int Exitosas { get; set; }
        public int ConErrores { get; set; }
        public string? MensajeGeneral { get; set; }
        public List<string> Errores { get; set; } = new();
    }

    public class ResultadoGeneracionFactura
    {
        public bool Exito { get; set; }
        public int? IdVenta { get; set; }
        public int? IdFacturaAutomatica { get; set; }
        public string? NumeroFactura { get; set; }
        public string? Mensaje { get; set; }
    }

    public class EstadisticasFacturacion
    {
        public int TotalSuscripcionesActivas { get; set; }
        public int SuscripcionesPendientesHoy { get; set; }
        public int TotalFacturasGeneradas { get; set; }
        public int TotalFacturasConError { get; set; }
        public decimal TotalMonto { get; set; }
        public decimal TotalCobrado { get; set; }
    }
}
