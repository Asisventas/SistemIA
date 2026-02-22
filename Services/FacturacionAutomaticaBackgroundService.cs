using Microsoft.EntityFrameworkCore;
using SistemIA.Models;

namespace SistemIA.Services
{
    /// <summary>
    /// Servicio en segundo plano que procesa las suscripciones pendientes
    /// y genera facturas automáticamente según la hora configurada.
    /// Se ejecuta cada 5 minutos verificando si hay suscripciones para procesar.
    /// </summary>
    public class FacturacionAutomaticaBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FacturacionAutomaticaBackgroundService> _logger;
        private readonly TimeSpan _intervaloVerificacion = TimeSpan.FromMinutes(5);

        public FacturacionAutomaticaBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<FacturacionAutomaticaBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🔄 Servicio de facturación automática iniciado");

            // Esperar 30 segundos antes de la primera ejecución para que la app arranque
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcesarSuscripcionesPendientesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error en el ciclo de facturación automática");
                }

                // Esperar antes de la próxima verificación
                await Task.Delay(_intervaloVerificacion, stoppingToken);
            }

            _logger.LogInformation("⏹️ Servicio de facturación automática detenido");
        }

        private async Task ProcesarSuscripcionesPendientesAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            
            await using var db = await dbFactory.CreateDbContextAsync(stoppingToken);
            
            var ahora = DateTime.Now;
            var hoy = ahora.Date;
            var horaActual = ahora.TimeOfDay;

            // Buscar suscripciones que deben procesarse:
            // - Estado = Activa
            // - FacturacionActiva = true
            // - FechaProximaFactura <= hoy
            // - HoraFacturacion <= hora actual (o null para procesar inmediatamente)
            // - No tienen FacturaAutomatica pendiente para este período
            var suscripcionesPendientes = await db.SuscripcionesClientes
                .Include(s => s.Cliente)
                .Include(s => s.Producto)
                .Include(s => s.Sucursal)
                .Include(s => s.Caja)
                .Where(s => s.Estado == "Activa"
                         && s.FacturacionActiva
                         && s.FechaProximaFactura != null
                         && s.FechaProximaFactura.Value.Date <= hoy
                         && (s.HoraFacturacion == null || s.HoraFacturacion.Value <= horaActual))
                .ToListAsync(stoppingToken);

            if (!suscripcionesPendientes.Any())
            {
                return; // Nada que procesar
            }

            _logger.LogInformation("📋 Encontradas {Count} suscripciones pendientes de facturar", suscripcionesPendientes.Count);

            var facturacionService = scope.ServiceProvider.GetRequiredService<IFacturacionAutomaticaService>();

            foreach (var suscripcion in suscripcionesPendientes)
            {
                if (stoppingToken.IsCancellationRequested) break;

                try
                {
                    // Verificar que no se haya generado ya una factura para este período
                    var yaGenerada = await db.FacturasAutomaticas
                        .AnyAsync(f => f.IdSuscripcion == suscripcion.IdSuscripcion
                                    && f.PeriodoFacturado == ObtenerPeriodoActual(suscripcion),
                                    stoppingToken);

                    if (yaGenerada)
                    {
                        _logger.LogDebug("⏭️ Suscripción {Id} ya tiene factura para este período", suscripcion.IdSuscripcion);
                        continue;
                    }

                    _logger.LogInformation("📄 Generando factura para suscripción {Id} - Cliente: {Cliente}",
                        suscripcion.IdSuscripcion,
                        suscripcion.Cliente?.RazonSocial ?? "N/A");

                    var resultado = await facturacionService.GenerarFacturaAsync(suscripcion.IdSuscripcion);

                    if (resultado.Exito)
                    {
                        _logger.LogInformation("✅ Factura generada exitosamente para suscripción {Id} - Venta #{IdVenta}",
                            suscripcion.IdSuscripcion,
                            resultado.IdVenta);
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ Error al generar factura para suscripción {Id}: {Mensaje}",
                            suscripcion.IdSuscripcion,
                            resultado.Mensaje);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error procesando suscripción {Id}", suscripcion.IdSuscripcion);
                }

                // Pequeña pausa entre facturas para no sobrecargar
                await Task.Delay(500, stoppingToken);
            }
        }

        private string ObtenerPeriodoActual(Models.Suscripciones.SuscripcionCliente suscripcion)
        {
            var fecha = suscripcion.FechaProximaFactura ?? DateTime.Today;
            return suscripcion.TipoPeriodo switch
            {
                "Mensual" => fecha.ToString("yyyy-MM"),
                "Bimestral" => $"{fecha.Year}-B{((fecha.Month - 1) / 2) + 1}",
                "Trimestral" => $"{fecha.Year}-T{((fecha.Month - 1) / 3) + 1}",
                "Semestral" => $"{fecha.Year}-S{(fecha.Month <= 6 ? 1 : 2)}",
                "Anual" => fecha.Year.ToString(),
                _ => fecha.ToString("yyyy-MM")
            };
        }
    }
}
