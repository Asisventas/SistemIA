using Microsoft.EntityFrameworkCore;
using SistemIA.Models;

namespace SistemIA.Services
{
    /// <summary>
    /// Servicio en segundo plano que envía alertas diarias por correo
    /// de productos con stock bajo el mínimo configurado.
    /// Se ejecuta una vez al día a las 7:00 AM.
    /// </summary>
    public class StockMinimoAlertaBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<StockMinimoAlertaBackgroundService> _logger;
        private readonly TimeSpan _horaEnvio = new TimeSpan(8, 0, 0); // 8:00 AM
        private DateTime _ultimoEnvio = DateTime.MinValue;

        public StockMinimoAlertaBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<StockMinimoAlertaBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("📦 Servicio de alerta de stock mínimo iniciado");

            // Esperar 60 segundos antes de la primera verificación
            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var ahora = DateTime.Now;
                    
                    // Verificar si debemos enviar (una vez al día, después de la hora configurada)
                    if (ahora.TimeOfDay >= _horaEnvio && 
                        _ultimoEnvio.Date < ahora.Date)
                    {
                        await EnviarAlertaStockMinimoAsync(stoppingToken);
                        _ultimoEnvio = ahora;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error en el ciclo de alerta de stock mínimo");
                }

                // Verificar cada 30 minutos
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }

            _logger.LogInformation("⏹️ Servicio de alerta de stock mínimo detenido");
        }

        private async Task EnviarAlertaStockMinimoAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            var informeService = scope.ServiceProvider.GetRequiredService<IInformeCorreoService>();
            
            await using var db = await dbFactory.CreateDbContextAsync(stoppingToken);

            // Obtener todas las sucursales
            var sucursales = await db.Sucursal
                .ToListAsync(stoppingToken);

            foreach (var sucursal in sucursales)
            {
                try
                {
                    // Verificar si hay productos con stock bajo en esta sucursal
                    var productosConStockBajo = await db.Productos
                        .Where(p => p.Activo && 
                                    p.StockMinimo > 0 && 
                                    p.Stock <= p.StockMinimo &&
                                    p.IdSucursal == sucursal.Id)
                        .CountAsync(stoppingToken);

                    if (productosConStockBajo > 0)
                    {
                        _logger.LogInformation("📧 Enviando alerta de stock bajo para sucursal {Sucursal} ({Cantidad} productos)", 
                            sucursal.NombreSucursal, productosConStockBajo);

                        var resultado = await informeService.EnviarInformeAsync(
                            TipoInformeEnum.AlertaStockBajo,
                            sucursal.Id);

                        if (resultado.Exito)
                        {
                            _logger.LogInformation("✅ Alerta de stock bajo enviada para sucursal {Sucursal}", 
                                sucursal.NombreSucursal);
                        }
                        else
                        {
                            _logger.LogWarning("⚠️ No se pudo enviar alerta para sucursal {Sucursal}: {Mensaje}", 
                                sucursal.NombreSucursal, resultado.Mensaje);
                        }
                    }
                    else
                    {
                        _logger.LogDebug("ℹ️ No hay productos con stock bajo en sucursal {Sucursal}", 
                            sucursal.NombreSucursal);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error al enviar alerta para sucursal {Sucursal}", 
                        sucursal.NombreSucursal);
                }
            }
        }
    }
}
