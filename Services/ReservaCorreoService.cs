using Microsoft.EntityFrameworkCore;
using SistemIA.Models;

namespace SistemIA.Services
{
    /// <summary>
    /// Servicio para envío de correos relacionados con reservas
    /// </summary>
    public interface IReservaCorreoService
    {
        /// <summary>
        /// Envía correo de confirmación de reserva al cliente
        /// </summary>
        Task<(bool Exito, string Mensaje)> EnviarConfirmacionAsync(int idReserva);
        
        /// <summary>
        /// Envía correo de confirmación de reserva con opción de encolar si falla
        /// </summary>
        Task<(bool Exito, string Mensaje, bool Encolado)> EnviarConfirmacionConColaAsync(int idReserva);
    }

    public class ReservaCorreoService : IReservaCorreoService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        private readonly ICorreoService _correoService;
        private readonly ICorreoColaService _colaService;
        private readonly ILogger<ReservaCorreoService> _logger;

        public ReservaCorreoService(
            IDbContextFactory<AppDbContext> dbFactory,
            ICorreoService correoService,
            ICorreoColaService colaService,
            ILogger<ReservaCorreoService> logger)
        {
            _dbFactory = dbFactory;
            _correoService = correoService;
            _colaService = colaService;
            _logger = logger;
        }

        public async Task<(bool Exito, string Mensaje)> EnviarConfirmacionAsync(int idReserva)
        {
            var resultado = await EnviarConfirmacionConColaAsync(idReserva);
            return (resultado.Exito || resultado.Encolado, resultado.Mensaje);
        }
        
        public async Task<(bool Exito, string Mensaje, bool Encolado)> EnviarConfirmacionConColaAsync(int idReserva)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync();

                var reserva = await db.Reservas
                    .Include(r => r.Mesa)
                    .Include(r => r.Sucursal)
                    .FirstOrDefaultAsync(r => r.IdReserva == idReserva);

                if (reserva == null)
                    return (false, "Reserva no encontrada", false);

                if (string.IsNullOrWhiteSpace(reserva.Email))
                    return (false, "La reserva no tiene email configurado", false);

                var sucursal = reserva.Sucursal ?? await db.Sucursal.FindAsync(reserva.IdSucursal);
                if (sucursal == null)
                    return (false, "Sucursal no encontrada", false);

                var mesa = reserva.Mesa ?? await db.Mesas.FindAsync(reserva.IdMesa);

                // Generar HTML del correo
                var htmlCorreo = GenerarHtmlConfirmacion(reserva, sucursal, mesa);
                var asunto = $"✅ Confirmación de Reserva #{reserva.NumeroReserva} - {sucursal.NombreEmpresa}";

                // Obtener configuración de correo de la sucursal
                var configCorreo = await db.ConfiguracionesCorreo
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Activo && c.IdSucursal == reserva.IdSucursal);

                if (configCorreo == null || !configCorreo.ConfiguracionCompleta)
                {
                    _logger.LogWarning("No hay configuración de correo activa para sucursal {IdSucursal}", reserva.IdSucursal);
                    return (false, "No hay configuración de correo activa para esta sucursal", false);
                }

                // Intentar enviar correo (con timeout corto para no bloquear)
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                
                try
                {
                    var resultado = await _correoService.EnviarCorreoAsync(
                        configCorreo,
                        reserva.Email,
                        asunto,
                        htmlCorreo,
                        null
                    );

                    if (resultado.Exito)
                    {
                        _logger.LogInformation("Correo de confirmación enviado para reserva {IdReserva} a {Email}",
                            idReserva, reserva.Email);
                        return (true, "Correo enviado exitosamente", false);
                    }
                    else
                    {
                        // Falló el envío - encolar para reintento
                        _logger.LogWarning("Fallo al enviar correo de reserva {IdReserva}: {Mensaje}. Encolando...", 
                            idReserva, resultado.Mensaje);
                        
                        var idEncolado = await _colaService.EncolarCorreoAsync(
                            reserva.Email,
                            asunto,
                            htmlCorreo,
                            reserva.IdSucursal,
                            "Reserva",
                            idReserva
                        );
                        
                        if (idEncolado > 0)
                        {
                            return (false, $"Correo encolado para envío posterior (sin conexión). ID: {idEncolado}", true);
                        }
                        
                        return (false, resultado.Mensaje, false);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Timeout - encolar para reintento
                    _logger.LogWarning("Timeout al enviar correo de reserva {IdReserva}. Encolando...", idReserva);
                    
                    var idEncolado = await _colaService.EncolarCorreoAsync(
                        reserva.Email,
                        asunto,
                        htmlCorreo,
                        reserva.IdSucursal,
                        "Reserva",
                        idReserva
                    );
                    
                    return (false, "Correo encolado (timeout de conexión)", idEncolado > 0);
                }
                catch (Exception ex) when (EsErrorDeConexion(ex))
                {
                    // Error de conexión - encolar para reintento
                    _logger.LogWarning("Sin conexión al enviar correo de reserva {IdReserva}. Encolando...", idReserva);
                    
                    var idEncolado = await _colaService.EncolarCorreoAsync(
                        reserva.Email,
                        asunto,
                        htmlCorreo,
                        reserva.IdSucursal,
                        "Reserva",
                        idReserva
                    );
                    
                    return (false, "Correo encolado (sin conexión a internet)", idEncolado > 0);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo de confirmación de reserva {IdReserva}", idReserva);
                return (false, $"Error: {ex.Message}", false);
            }
        }
        
        /// <summary>
        /// Detecta si el error es por falta de conexión a internet
        /// </summary>
        private bool EsErrorDeConexion(Exception ex)
        {
            var mensaje = ex.Message.ToLower();
            var innerMensaje = ex.InnerException?.Message?.ToLower() ?? "";

            return mensaje.Contains("unable to connect") ||
                   mensaje.Contains("no such host") ||
                   mensaje.Contains("network") ||
                   mensaje.Contains("timeout") ||
                   mensaje.Contains("connection") ||
                   mensaje.Contains("socket") ||
                   innerMensaje.Contains("unable to connect") ||
                   innerMensaje.Contains("network") ||
                   innerMensaje.Contains("socket");
        }

        /// <summary>
        /// Genera el HTML del correo de confirmación con logo y datos de la empresa
        /// </summary>
        private string GenerarHtmlConfirmacion(Reserva reserva, Sucursal sucursal, Mesa? mesa)
        {
            // Convertir logo a base64 si existe
            string logoHtml = "";
            if (sucursal.Logo != null && sucursal.Logo.Length > 0)
            {
                var logoBase64 = Convert.ToBase64String(sucursal.Logo);
                logoHtml = $"<img src=\"data:image/png;base64,{logoBase64}\" alt=\"Logo\" style=\"max-height: 80px; max-width: 200px;\" />";
            }

            var fechaReserva = reserva.FechaReserva.ToString("dddd, dd 'de' MMMM 'de' yyyy", 
                new System.Globalization.CultureInfo("es-PY"));
            var horaInicio = reserva.HoraInicio.ToString("hh\\:mm");
            var horaFin = reserva.HoraFin?.ToString("hh\\:mm") ?? "-";
            var nombreMesa = mesa?.TextoMostrar ?? $"Mesa {reserva.IdMesa}";

            // Filas opcionales
            var filaPersonas = reserva.CantidadPersonas > 0
                ? $@"<tr>
                        <td style=""color: #6c757d; font-size: 14px;"">👥 Personas:</td>
                        <td style=""color: #333; font-size: 14px; font-weight: 600;"">{reserva.CantidadPersonas}</td>
                    </tr>"
                : "";

            var filaObservaciones = !string.IsNullOrWhiteSpace(reserva.Observaciones)
                ? $@"<tr>
                        <td style=""color: #6c757d; font-size: 14px; vertical-align: top;"">📝 Notas:</td>
                        <td style=""color: #333; font-size: 14px;"">{System.Net.WebUtility.HtmlEncode(reserva.Observaciones)}</td>
                    </tr>"
                : "";

            var telefonoEmpresa = !string.IsNullOrEmpty(sucursal.Telefono)
                ? $"<p style=\"color: rgba(255,255,255,0.9); margin: 3px 0 0 0; font-size: 14px;\">📞 {sucursal.Telefono}</p>"
                : "";

            var telefonoPie = !string.IsNullOrEmpty(sucursal.Telefono)
                ? $" | Tel: {sucursal.Telefono}"
                : "";

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body style=""margin: 0; padding: 0; font-family: 'Segoe UI', Arial, sans-serif; background-color: #f4f4f4;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #f4f4f4; padding: 20px 0;"">
        <tr>
            <td align=""center"">
                <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1);"">
                    
                    <!-- Cabecera con logo y datos de la empresa -->
                    <tr>
                        <td style=""background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; border-radius: 8px 8px 0 0;"">
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
                                <tr>
                                    <td align=""center"">
                                        {logoHtml}
                                    </td>
                                </tr>
                                <tr>
                                    <td align=""center"" style=""padding-top: 15px;"">
                                        <h1 style=""color: #ffffff; margin: 0; font-size: 24px; font-weight: 600;"">{System.Net.WebUtility.HtmlEncode(sucursal.NombreEmpresa)}</h1>
                                        <p style=""color: rgba(255,255,255,0.9); margin: 5px 0 0 0; font-size: 14px;"">{System.Net.WebUtility.HtmlEncode(sucursal.Direccion)}</p>
                                        {telefonoEmpresa}
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    
                    <!-- Mensaje de confirmación -->
                    <tr>
                        <td style=""padding: 30px 40px 20px 40px;"">
                            <div style=""text-align: center; margin-bottom: 25px;"">
                                <span style=""font-size: 48px;"">✅</span>
                                <h2 style=""color: #28a745; margin: 10px 0 5px 0; font-size: 22px;"">¡Reserva Confirmada!</h2>
                                <p style=""color: #666; margin: 0; font-size: 14px;"">Reserva #{reserva.NumeroReserva}</p>
                            </div>
                            
                            <p style=""color: #333; font-size: 16px; line-height: 1.5; margin-bottom: 25px;"">
                                Estimado/a <strong>{System.Net.WebUtility.HtmlEncode(reserva.NombreCliente)}</strong>,
                            </p>
                            <p style=""color: #555; font-size: 15px; line-height: 1.6;"">
                                Nos complace informarle que su reserva ha sido <strong style=""color: #28a745;"">confirmada</strong>. 
                                A continuación encontrará los detalles:
                            </p>
                        </td>
                    </tr>
                    
                    <!-- Detalles de la reserva -->
                    <tr>
                        <td style=""padding: 0 40px 30px 40px;"">
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #f8f9fa; border-radius: 8px; border: 1px solid #e9ecef;"">
                                <tr>
                                    <td style=""padding: 20px;"">
                                        <table width=""100%"" cellpadding=""8"" cellspacing=""0"">
                                            <tr>
                                                <td style=""color: #6c757d; font-size: 14px; width: 40%;"">📅 Fecha:</td>
                                                <td style=""color: #333; font-size: 14px; font-weight: 600;"">{fechaReserva}</td>
                                            </tr>
                                            <tr>
                                                <td style=""color: #6c757d; font-size: 14px;"">🕐 Hora:</td>
                                                <td style=""color: #333; font-size: 14px; font-weight: 600;"">{horaInicio} - {horaFin}</td>
                                            </tr>
                                            <tr>
                                                <td style=""color: #6c757d; font-size: 14px;"">🪑 Mesa:</td>
                                                <td style=""color: #333; font-size: 14px; font-weight: 600;"">{nombreMesa}</td>
                                            </tr>
                                            {filaPersonas}
                                            {filaObservaciones}
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    
                    <!-- Mensaje de agradecimiento -->
                    <tr>
                        <td style=""padding: 0 40px 30px 40px;"">
                            <div style=""background: linear-gradient(135deg, #ffecd2 0%, #fcb69f 100%); border-radius: 8px; padding: 20px; text-align: center;"">
                                <p style=""color: #7c4a03; font-size: 16px; margin: 0; line-height: 1.5;"">
                                    🙏 <strong>¡Gracias por elegirnos!</strong><br/>
                                    <span style=""font-size: 14px;"">Le esperamos con gusto. Si necesita modificar o cancelar su reserva, por favor contáctenos.</span>
                                </p>
                            </div>
                        </td>
                    </tr>
                    
                    <!-- Pie de página -->
                    <tr>
                        <td style=""background-color: #f8f9fa; padding: 20px 40px; border-radius: 0 0 8px 8px; border-top: 1px solid #e9ecef;"">
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
                                <tr>
                                    <td align=""center"">
                                        <p style=""color: #6c757d; font-size: 12px; margin: 0;"">
                                            {System.Net.WebUtility.HtmlEncode(sucursal.NombreEmpresa)} | {System.Net.WebUtility.HtmlEncode(sucursal.Direccion)}{telefonoPie}
                                        </p>
                                        <p style=""color: #adb5bd; font-size: 11px; margin: 10px 0 0 0;"">
                                            Este es un correo automático generado por SistemIA. Por favor no responda a este mensaje.
                                        </p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        }
    }
}
