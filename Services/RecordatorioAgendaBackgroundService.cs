using SistemIA.Models;
using Microsoft.EntityFrameworkCore;

namespace SistemIA.Services;

/// <summary>
/// Servicio en segundo plano que procesa los recordatorios de citas de la agenda
/// y envía notificaciones por correo electrónico cuando corresponde.
/// </summary>
public class RecordatorioAgendaBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RecordatorioAgendaBackgroundService> _logger;
    private readonly TimeSpan _intervaloVerificacion = TimeSpan.FromMinutes(1); // Verificar cada minuto

    public RecordatorioAgendaBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<RecordatorioAgendaBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RecordatorioAgendaBackgroundService iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcesarRecordatoriosPendientes(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando recordatorios de agenda");
            }

            await Task.Delay(_intervaloVerificacion, stoppingToken);
        }

        _logger.LogInformation("RecordatorioAgendaBackgroundService detenido");
    }

    private async Task ProcesarRecordatoriosPendientes(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var correoService = scope.ServiceProvider.GetService<ICorreoService>();

        var ahora = DateTime.Now;

        // Obtener recordatorios pendientes que ya deberían haberse enviado
        var recordatoriosPendientes = await db.RecordatoriosCitas
            .Include(r => r.Cita)
            .Where(r => !r.Enviado 
                     && r.FechaHoraProgramada <= ahora
                     && r.Cita != null 
                     && r.Cita.Estado != "Cancelada"
                     && r.Cita.Estado != "Completada")
            .ToListAsync(stoppingToken);

        if (recordatoriosPendientes.Count == 0)
            return;

        _logger.LogInformation($"Procesando {recordatoriosPendientes.Count} recordatorios de agenda");

        foreach (var recordatorio in recordatoriosPendientes)
        {
            try
            {
                var cita = recordatorio.Cita!;
                var minutosAntes = recordatorio.MinutosAntes;

                // Verificar si la cita ya pasó (no tiene sentido recordar)
                if (cita.FechaHoraInicio < ahora)
                {
                    recordatorio.Enviado = true;
                    recordatorio.FechaEnvio = ahora;
                    recordatorio.Resultado = "Omitido (cita ya pasó)";
                    continue;
                }

                // Enviar notificación por correo si está configurado
                if (recordatorio.Tipo == "Correo" && !string.IsNullOrEmpty(cita.Email) && correoService != null)
                {
                    var enviado = await EnviarCorreoRecordatorio(correoService, db, cita, minutosAntes);
                    recordatorio.Enviado = true;
                    recordatorio.FechaEnvio = ahora;
                    recordatorio.Resultado = enviado ? "Enviado" : "Error al enviar correo";
                }
                else if (recordatorio.Tipo == "Sistema")
                {
                    // Solo marcar como procesado (la notificación en sistema se mostraría en la UI)
                    recordatorio.Enviado = true;
                    recordatorio.FechaEnvio = ahora;
                    recordatorio.Resultado = "Notificación del sistema registrada";
                }
                else
                {
                    recordatorio.Enviado = true;
                    recordatorio.FechaEnvio = ahora;
                    recordatorio.Resultado = "Sin destino de envío";
                }

                _logger.LogInformation($"Recordatorio procesado para cita {cita.IdCita}: {cita.Titulo}");
            }
            catch (Exception ex)
            {
                recordatorio.Resultado = $"Error: {ex.Message}";
                _logger.LogError(ex, $"Error procesando recordatorio {recordatorio.IdRecordatorio}");
            }
        }

        await db.SaveChangesAsync(stoppingToken);
    }

    private async Task<bool> EnviarCorreoRecordatorio(
        ICorreoService correoService, 
        AppDbContext db,
        CitaAgenda cita, 
        int minutosAntes)
    {
        try
        {
            // Obtener configuración de correo de la sucursal
            var configCorreo = await db.Set<ConfiguracionCorreo>()
                .FirstOrDefaultAsync(c => c.IdSucursal == cita.IdSucursal && c.Activo);

            if (configCorreo == null)
            {
                _logger.LogWarning($"No hay configuración de correo activa para sucursal {cita.IdSucursal}");
                return false;
            }

            var asunto = $"Recordatorio: {cita.Titulo}";
            var cuerpo = GenerarCuerpoCorreoRecordatorio(cita, minutosAntes);

            // Usar la sobrecarga correcta con ConfiguracionCorreo
            var (exito, mensaje) = await correoService.EnviarCorreoAsync(
                configCorreo,
                cita.Email!,
                asunto,
                cuerpo,
                null);

            return exito;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error enviando correo de recordatorio para cita {cita.IdCita}");
            return false;
        }
    }

    private string GenerarCuerpoCorreoRecordatorio(CitaAgenda cita, int minutosAntes)
    {
        var tiempoTexto = minutosAntes switch
        {
            < 60 => $"{minutosAntes} minutos",
            60 => "1 hora",
            120 => "2 horas",
            1440 => "1 día",
            _ => $"{minutosAntes} minutos"
        };

        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; padding: 20px; background: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 0 auto; background: white; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }}
        .header {{ background: {cita.ColorFondo}; color: {cita.ColorTexto}; padding: 20px; text-align: center; }}
        .header h1 {{ margin: 0; font-size: 24px; }}
        .content {{ padding: 30px; }}
        .info-row {{ display: flex; align-items: flex-start; margin-bottom: 15px; }}
        .info-icon {{ width: 24px; margin-right: 12px; color: #666; }}
        .info-text {{ flex: 1; }}
        .info-label {{ font-weight: bold; color: #333; }}
        .info-value {{ color: #666; margin-top: 4px; }}
        .reminder {{ background: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin-bottom: 20px; border-radius: 0 4px 4px 0; }}
        .reminder strong {{ color: #856404; }}
        .btn {{ display: inline-block; background: {cita.ColorFondo}; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; margin-top: 20px; }}
        .footer {{ background: #f8f9fa; padding: 15px; text-align: center; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔔 Recordatorio de Cita</h1>
        </div>
        <div class='content'>
            <div class='reminder'>
                <strong>Tienes una cita programada en {tiempoTexto}</strong>
            </div>

            <div class='info-row'>
                <span class='info-icon'>📋</span>
                <div class='info-text'>
                    <div class='info-label'>Título</div>
                    <div class='info-value'>{cita.Titulo}</div>
                </div>
            </div>

            <div class='info-row'>
                <span class='info-icon'>📅</span>
                <div class='info-text'>
                    <div class='info-label'>Fecha y Hora</div>
                    <div class='info-value'>{cita.FechaHoraInicio:dddd, dd 'de' MMMM 'de' yyyy 'a las' HH:mm}</div>
                </div>
            </div>

            <div class='info-row'>
                <span class='info-icon'>⏱️</span>
                <div class='info-text'>
                    <div class='info-label'>Duración</div>
                    <div class='info-value'>{cita.DuracionMinutos} minutos</div>
                </div>
            </div>";

        if (!string.IsNullOrEmpty(cita.Direccion))
        {
            html += $@"
            <div class='info-row'>
                <span class='info-icon'>📍</span>
                <div class='info-text'>
                    <div class='info-label'>Ubicación</div>
                    <div class='info-value'>{cita.Direccion}</div>
                </div>
            </div>";
        }

        if (!string.IsNullOrEmpty(cita.Descripcion))
        {
            html += $@"
            <div class='info-row'>
                <span class='info-icon'>📝</span>
                <div class='info-text'>
                    <div class='info-label'>Descripción</div>
                    <div class='info-value'>{cita.Descripcion}</div>
                </div>
            </div>";
        }

        if (!string.IsNullOrEmpty(cita.UrlMaps))
        {
            html += $@"
            <a href='{cita.UrlMaps}' class='btn'>📍 Ver ubicación en Google Maps</a>";
        }

        html += $@"
        </div>
        <div class='footer'>
            Este es un recordatorio automático de SistemIA<br>
            {DateTime.Now:yyyy}
        </div>
    </div>
</body>
</html>";

        return html;
    }
}
