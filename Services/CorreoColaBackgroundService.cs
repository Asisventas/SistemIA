using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace SistemIA.Services
{
    /// <summary>
    /// Servicio de background que procesa la cola de correos pendientes periódicamente.
    /// Reintenta enviar correos que fallaron por falta de conexión.
    /// </summary>
    public class CorreoColaBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CorreoColaBackgroundService> _logger;
        
        // Intervalo entre procesamiento de cola (2 minutos)
        private readonly TimeSpan _intervalo = TimeSpan.FromMinutes(2);
        
        // Máximo de correos a procesar por ciclo
        private const int MaxCorreosPorCiclo = 10;

        public CorreoColaBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<CorreoColaBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[CorreoColaBackground] Servicio iniciado. Intervalo: {Min} minutos", _intervalo.TotalMinutes);

            // Esperar un poco al inicio para que el sistema termine de cargar
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcesarColaAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[CorreoColaBackground] Error en ciclo de procesamiento");
                }

                await Task.Delay(_intervalo, stoppingToken);
            }

            _logger.LogInformation("[CorreoColaBackground] Servicio detenido");
        }

        private async Task ProcesarColaAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var colaService = scope.ServiceProvider.GetRequiredService<ICorreoColaService>();

            // Primero verificar si hay correos pendientes
            var pendientes = await colaService.ObtenerCantidadPendientesAsync();
            
            if (pendientes == 0)
            {
                // No hay nada que procesar
                return;
            }

            _logger.LogInformation("[CorreoColaBackground] {Count} correos pendientes en cola", pendientes);

            // Procesar la cola
            var (procesados, exitosos, fallidos) = await colaService.ProcesarColaPendienteAsync(MaxCorreosPorCiclo);

            if (procesados > 0)
            {
                _logger.LogInformation("[CorreoColaBackground] Procesados: {Proc}, Exitosos: {Ok}, Fallidos: {Fail}",
                    procesados, exitosos, fallidos);
            }
        }
    }
}
