using Microsoft.EntityFrameworkCore;
using SistemIA.Models;
using System.Net.Mail;
using System.Text;

namespace SistemIA.Services
{
    /// <summary>
    /// Interface para el servicio de generación y envío de informes por correo.
    /// </summary>
    public interface IInformeCorreoService
    {
        /// <summary>
        /// Envía un informe específico a los destinatarios configurados.
        /// </summary>
        Task<(bool Exito, string Mensaje)> EnviarInformeAsync(TipoInformeEnum tipoInforme, int sucursalId, DateTime? fechaDesde = null, DateTime? fechaHasta = null);

        /// <summary>
        /// Envía todos los informes configurados para envío al cierre del sistema.
        /// </summary>
        Task<(bool Exito, string Mensaje, int InformesEnviados)> EnviarInformesCierreAsync(int sucursalId);

        /// <summary>
        /// Envía todos los informes configurados al cierre, usando datos del cierre de caja (fecha, turno, caja).
        /// </summary>
        Task<(bool Exito, string Mensaje, int InformesEnviados)> EnviarInformesCierreAsync(
            int sucursalId, DateTime fechaCaja, int turno, int idCaja);

        /// <summary>
        /// Envía resumen diario a los destinatarios configurados.
        /// </summary>
        Task<(bool Exito, string Mensaje)> EnviarResumenDiarioAsync(int sucursalId, DateTime fecha);

        /// <summary>
        /// Envía resumen semanal a los destinatarios configurados.
        /// </summary>
        Task<(bool Exito, string Mensaje)> EnviarResumenSemanalAsync(int sucursalId, DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Envía resumen mensual a los destinatarios configurados.
        /// </summary>
        Task<(bool Exito, string Mensaje)> EnviarResumenMensualAsync(int sucursalId, int año, int mes);

        /// <summary>
        /// Genera el contenido HTML de un informe específico.
        /// </summary>
        Task<string> GenerarHtmlInformeAsync(TipoInformeEnum tipoInforme, int sucursalId, DateTime? fechaDesde = null, DateTime? fechaHasta = null);

        /// <summary>
        /// Obtiene la lista de destinatarios para un tipo de informe.
        /// </summary>
        Task<List<DestinatarioInforme>> ObtenerDestinatariosAsync(TipoInformeEnum tipoInforme, int sucursalId);
    }

    /// <summary>
    /// Servicio para generación y envío de informes por correo electrónico.
    /// </summary>
    public class InformeCorreoService : IInformeCorreoService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        private readonly ICorreoService _correoService;
        private readonly IHtmlToPdfService _htmlToPdfService;
        private readonly IInformePdfService _informePdfService;
        private readonly ILogger<InformeCorreoService> _logger;

        public InformeCorreoService(
            IDbContextFactory<AppDbContext> dbFactory,
            ICorreoService correoService,
            IHtmlToPdfService htmlToPdfService,
            IInformePdfService informePdfService,
            ILogger<InformeCorreoService> logger)
        {
            _dbFactory = dbFactory;
            _correoService = correoService;
            _htmlToPdfService = htmlToPdfService;
            _informePdfService = informePdfService;
            _logger = logger;
        }

        public async Task<(bool Exito, string Mensaje)> EnviarInformeAsync(
            TipoInformeEnum tipoInforme, 
            int sucursalId, 
            DateTime? fechaDesde = null, 
            DateTime? fechaHasta = null)
        {
            try
            {
                // Verificar configuración de correo
                if (!await _correoService.EstaConfiguradoAsync(sucursalId))
                {
                    return (false, "Servicio de correo no configurado");
                }

                // Obtener destinatarios
                var destinatarios = await ObtenerDestinatariosAsync(tipoInforme, sucursalId);
                if (!destinatarios.Any())
                {
                    _logger.LogWarning("No hay destinatarios configurados para {TipoInforme}", tipoInforme);
                    return (false, $"No hay destinatarios configurados para {tipoInforme}");
                }

                // Generar contenido del informe
                var html = await GenerarHtmlInformeAsync(tipoInforme, sucursalId, fechaDesde, fechaHasta);
                if (string.IsNullOrEmpty(html))
                {
                    return (false, "No se pudo generar el contenido del informe");
                }

                // Obtener información de sucursal
                await using var ctx = await _dbFactory.CreateDbContextAsync();
                var sucursal = await ctx.Sucursal.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sucursalId);
                var nombreSucursal = sucursal?.NombreSucursal ?? "Sucursal";

                // Construir asunto
                var fechaStr = fechaDesde.HasValue 
                    ? $"{fechaDesde:dd/MM/yyyy}" + (fechaHasta.HasValue && fechaHasta != fechaDesde ? $" - {fechaHasta:dd/MM/yyyy}" : "")
                    : DateTime.Today.ToString("dd/MM/yyyy");
                var asunto = $"Informe {ObtenerNombreInforme(tipoInforme)} - {nombreSucursal} - {fechaStr}";

                // Enviar a cada destinatario
                var enviados = 0;
                var errores = new List<string>();

                foreach (var dest in destinatarios)
                {
                    var resultado = await _correoService.EnviarCorreoAsync(dest.Correo, asunto, html);
                    if (resultado.Exito)
                    {
                        enviados++;
                        _logger.LogInformation("Informe {TipoInforme} enviado a {Correo}", tipoInforme, dest.Correo);
                    }
                    else
                    {
                        errores.Add($"{dest.Nombre}: {resultado.Mensaje}");
                        _logger.LogWarning("Error enviando informe a {Correo}: {Error}", dest.Correo, resultado.Mensaje);
                    }
                }

                if (enviados > 0)
                {
                    return (true, $"Informe enviado a {enviados} de {destinatarios.Count} destinatarios");
                }

                return (false, $"No se pudo enviar el informe. Errores: {string.Join("; ", errores)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar informe {TipoInforme}", tipoInforme);
                return (false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Versión legacy que usa fecha actual - redirige a la nueva versión con parámetros del cierre.
        /// </summary>
        public async Task<(bool Exito, string Mensaje, int InformesEnviados)> EnviarInformesCierreAsync(int sucursalId)
        {
            // Si no se proporcionan datos del cierre, usar valores por defecto
            return await EnviarInformesCierreAsync(sucursalId, DateTime.Today, 1, 0);
        }

        /// <summary>
        /// Envía todos los informes configurados al cierre usando datos específicos del cierre de caja.
        /// Los informes se generan como PDF profesionales usando QuestPDF y se envían como adjuntos.
        /// </summary>
        public async Task<(bool Exito, string Mensaje, int InformesEnviados)> EnviarInformesCierreAsync(
            int sucursalId, DateTime fechaCaja, int turno, int idCaja)
        {
            try
            {
                await using var ctx = await _dbFactory.CreateDbContextAsync();

                // Verificar configuración
                var config = await ctx.ConfiguracionesCorreo
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Activo && c.IdSucursal == sucursalId);

                if (config == null || !config.EnviarAlCierreSistema)
                {
                    return (false, "Envío al cierre no está habilitado", 0);
                }

                // Obtener información de sucursal
                var sucursal = await ctx.Sucursal.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sucursalId);
                var nombreEmpresa = sucursal?.NombreEmpresa ?? "Empresa";
                var nombreSucursal = sucursal?.NombreSucursal ?? "Sucursal";

                var informesEnviados = 0;
                var errores = new List<string>();

                // Lista de informes que pueden enviarse al cierre
                var informesCierre = new[]
                {
                    TipoInformeEnum.VentasDetallado,
                    TipoInformeEnum.VentasAgrupado,
                    TipoInformeEnum.ComprasDetallado,
                    TipoInformeEnum.NotasCreditoAgrupado,
                    TipoInformeEnum.NCComprasAgrupado,
                    TipoInformeEnum.ProductosValorizado,
                    TipoInformeEnum.CuentasPorCobrar,
                    TipoInformeEnum.CuentasPorPagar,
                    TipoInformeEnum.ResumenCaja
                };

                _logger.LogInformation("Iniciando envío de informes al cierre para sucursal {SucursalId}, FechaCaja: {FechaCaja}, Turno: {Turno}, Caja: {IdCaja}", 
                    sucursalId, fechaCaja, turno, idCaja);

                foreach (var tipo in informesCierre)
                {
                    try
                    {
                        var destinatarios = await ObtenerDestinatariosAsync(tipo, sucursalId);
                        _logger.LogInformation("Informe {Tipo}: {Cantidad} destinatarios encontrados", tipo, destinatarios.Count);
                        
                        if (!destinatarios.Any())
                        {
                            _logger.LogInformation("Sin destinatarios para {Tipo}, omitiendo", tipo);
                            continue;
                        }

                        // Generar PDF usando el servicio profesional QuestPDF
                        var nombreInforme = ObtenerNombreInforme(tipo);
                        byte[]? pdfBytes = null;
                        
                        try
                        {
                            pdfBytes = tipo switch
                            {
                                TipoInformeEnum.VentasDetallado => await _informePdfService.GenerarPdfVentasDetalladoAsync(sucursalId, fechaCaja, turno, idCaja),
                                TipoInformeEnum.VentasAgrupado => await _informePdfService.GenerarPdfVentasAgrupadoAsync(sucursalId, fechaCaja, turno, idCaja),
                                TipoInformeEnum.ComprasDetallado => await _informePdfService.GenerarPdfComprasDetalladoAsync(sucursalId, fechaCaja, turno, idCaja),
                                TipoInformeEnum.NotasCreditoAgrupado => await _informePdfService.GenerarPdfNCVentasAsync(sucursalId, fechaCaja, turno, idCaja),
                                TipoInformeEnum.NCComprasAgrupado => await _informePdfService.GenerarPdfNCComprasAsync(sucursalId, fechaCaja, turno, idCaja),
                                TipoInformeEnum.ProductosValorizado => await _informePdfService.GenerarPdfProductosValorizadoAsync(sucursalId),
                                TipoInformeEnum.ResumenCaja => await _informePdfService.GenerarPdfResumenCajaAsync(sucursalId, fechaCaja, turno, idCaja),
                                // Para informes sin generador QuestPDF, usar el método HTML legacy
                                _ => await GenerarPdfLegacyAsync(tipo, sucursalId, fechaCaja, nombreEmpresa, nombreSucursal, turno, idCaja)
                            };
                        }
                        catch (Exception exPdf)
                        {
                            _logger.LogError(exPdf, "Error al generar PDF profesional para {Tipo}, intentando fallback HTML", tipo);
                            // Intentar fallback a HTML -> PDF
                            try
                            {
                                pdfBytes = await GenerarPdfLegacyAsync(tipo, sucursalId, fechaCaja, nombreEmpresa, nombreSucursal, turno, idCaja);
                            }
                            catch (Exception exFallback)
                            {
                                _logger.LogError(exFallback, "Fallback HTML también falló para {Tipo}", tipo);
                                errores.Add($"{nombreInforme}: Error al generar PDF");
                                continue;
                            }
                        }

                        if (pdfBytes == null || pdfBytes.Length == 0)
                        {
                            _logger.LogWarning("PDF vacío para {Tipo}, omitiendo", tipo);
                            continue;
                        }

                        // Preparar nombre del archivo
                        var nombreArchivo = $"{nombreInforme.Replace(" ", "_")}_{fechaCaja:yyyyMMdd}_T{turno}.pdf";
                        
                        // Preparar asunto y cuerpo del correo
                        var asunto = $"{nombreInforme} - {nombreSucursal} - {fechaCaja:dd/MM/yyyy}";
                        var cuerpoHtml = GenerarCuerpoCorreoInforme(nombreInforme, nombreEmpresa, nombreSucursal, fechaCaja, turno, idCaja);

                        // Enviar a cada destinatario con PDF adjunto
                        foreach (var dest in destinatarios)
                        {
                            try
                            {
                                var attachment = new System.Net.Mail.Attachment(
                                    new MemoryStream(pdfBytes), nombreArchivo, "application/pdf");
                                
                                var resultado = await _correoService.EnviarCorreoAsync(
                                    dest.Correo, asunto, cuerpoHtml, new List<System.Net.Mail.Attachment> { attachment });
                                
                                attachment.Dispose();
                                
                                if (resultado.Exito)
                                {
                                    _logger.LogInformation("Informe {Tipo} enviado a {Correo}", tipo, dest.Correo);
                                }
                                else
                                {
                                    _logger.LogWarning("Error enviando {Tipo} a {Correo}: {Error}", tipo, dest.Correo, resultado.Mensaje);
                                }
                            }
                            catch (Exception exEnvio)
                            {
                                _logger.LogError(exEnvio, "Excepción enviando {Tipo} a {Correo}", tipo, dest.Correo);
                            }
                        }

                        informesEnviados++;
                        _logger.LogInformation("Informe {Tipo} procesado correctamente", tipo);
                    }
                    catch (Exception exInforme)
                    {
                        _logger.LogError(exInforme, "Excepción al procesar informe {Tipo}", tipo);
                        errores.Add($"{ObtenerNombreInforme(tipo)}: Error interno - {exInforme.Message}");
                    }
                }

                _logger.LogInformation("Envío al cierre completado: {Enviados} informes enviados, {Errores} errores", 
                    informesEnviados, errores.Count);

                if (informesEnviados > 0)
                {
                    var mensaje = $"Se enviaron {informesEnviados} informes al cierre";
                    if (errores.Any())
                    {
                        mensaje += $". Errores: {string.Join("; ", errores)}";
                    }
                    return (true, mensaje, informesEnviados);
                }

                return (false, "No se enviaron informes. " + string.Join("; ", errores), 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar informes de cierre");
                return (false, $"Error: {ex.Message}", 0);
            }
        }

        /// <summary>
        /// Genera el cuerpo HTML del correo que acompaña al PDF adjunto
        /// </summary>
        private string GenerarCuerpoCorreoInforme(string nombreInforme, string nombreEmpresa, string nombreSucursal, 
            DateTime fechaCaja, int turno, int idCaja)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html><html><head><meta charset='utf-8'></head><body style='font-family: Arial, sans-serif;'>");
            sb.AppendLine($"<div style='max-width: 600px; margin: 0 auto; padding: 20px;'>");
            
            // Encabezado
            sb.AppendLine($"<h2 style='color: #333; border-bottom: 2px solid #007bff; padding-bottom: 10px;'>{nombreEmpresa}</h2>");
            sb.AppendLine($"<h3 style='color: #007bff; margin-top: 0;'>{nombreInforme}</h3>");
            
            // Información del cierre
            sb.AppendLine("<table style='width: 100%; border-collapse: collapse; margin: 20px 0;'>");
            sb.AppendLine($"<tr><td style='padding: 8px; background: #f8f9fa;'><strong>Sucursal:</strong></td><td style='padding: 8px;'>{nombreSucursal}</td></tr>");
            sb.AppendLine($"<tr><td style='padding: 8px; background: #f8f9fa;'><strong>Fecha Caja:</strong></td><td style='padding: 8px;'>{fechaCaja:dd/MM/yyyy}</td></tr>");
            sb.AppendLine($"<tr><td style='padding: 8px; background: #f8f9fa;'><strong>Turno:</strong></td><td style='padding: 8px;'>{turno}</td></tr>");
            if (idCaja > 0)
                sb.AppendLine($"<tr><td style='padding: 8px; background: #f8f9fa;'><strong>Caja:</strong></td><td style='padding: 8px;'>{idCaja}</td></tr>");
            sb.AppendLine("</table>");
            
            // Mensaje
            sb.AppendLine("<div style='background: #d4edda; border: 1px solid #c3e6cb; border-radius: 4px; padding: 15px; margin: 20px 0;'>");
            sb.AppendLine("<p style='margin: 0; color: #155724;'><strong>📎 Informe PDF Adjunto</strong></p>");
            sb.AppendLine("<p style='margin: 5px 0 0 0; color: #155724;'>El informe completo se encuentra en el archivo PDF adjunto a este correo.</p>");
            sb.AppendLine("</div>");
            
            // Pie
            sb.AppendLine($"<p style='color: #666; font-size: 12px; margin-top: 30px; border-top: 1px solid #ddd; padding-top: 10px;'>");
            sb.AppendLine($"Generado automáticamente por SistemIA el {DateTime.Now:dd/MM/yyyy HH:mm}");
            sb.AppendLine("</p>");
            
            sb.AppendLine("</div></body></html>");
            return sb.ToString();
        }

        /// <summary>
        /// Genera PDF usando el método legacy (HTML -> iTextSharp) para informes sin generador QuestPDF.
        /// </summary>
        private async Task<byte[]> GenerarPdfLegacyAsync(TipoInformeEnum tipo, int sucursalId, DateTime fechaCaja, 
            string nombreEmpresa, string nombreSucursal, int turno, int idCaja)
        {
            var html = await GenerarHtmlInformeAsync(tipo, sucursalId, fechaCaja, fechaCaja);
            if (string.IsNullOrEmpty(html))
            {
                return Array.Empty<byte>();
            }

            var nombreInforme = ObtenerNombreInforme(tipo);
            var filtros = $"Fecha: {fechaCaja:dd/MM/yyyy} | Turno: {turno}" + (idCaja > 0 ? $" | Caja: {idCaja}" : "");
            return _htmlToPdfService.ConvertirHtmlAPdf(html, nombreInforme, filtros, nombreEmpresa, nombreSucursal);
        }

        public async Task<(bool Exito, string Mensaje)> EnviarResumenDiarioAsync(int sucursalId, DateTime fecha)
        {
            return await EnviarInformeAsync(TipoInformeEnum.VentasDiarias, sucursalId, fecha, fecha);
        }

        public async Task<(bool Exito, string Mensaje)> EnviarResumenSemanalAsync(int sucursalId, DateTime fechaInicio, DateTime fechaFin)
        {
            return await EnviarInformeAsync(TipoInformeEnum.ResumenSemanal, sucursalId, fechaInicio, fechaFin);
        }

        public async Task<(bool Exito, string Mensaje)> EnviarResumenMensualAsync(int sucursalId, int año, int mes)
        {
            var fechaInicio = new DateTime(año, mes, 1);
            var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);
            return await EnviarInformeAsync(TipoInformeEnum.ResumenMensual, sucursalId, fechaInicio, fechaFin);
        }

        public async Task<List<DestinatarioInforme>> ObtenerDestinatariosAsync(TipoInformeEnum tipoInforme, int sucursalId)
        {
            await using var ctx = await _dbFactory.CreateDbContextAsync();

            var todos = await ctx.DestinatariosInforme
                .AsNoTracking()
                .Where(d => d.Activo && (d.IdSucursal == null || d.IdSucursal == sucursalId))
                .ToListAsync();

            // Filtrar por tipo de informe
            return todos.Where(d => TieneHabilitadoInforme(d, tipoInforme)).ToList();
        }

        private bool TieneHabilitadoInforme(DestinatarioInforme dest, TipoInformeEnum tipo)
        {
            // Si tiene RecibeTodosLosInformes = true, automáticamente recibe cualquier informe
            if (dest.RecibeTodosLosInformes)
                return true;

            return tipo switch
            {
                TipoInformeEnum.VentasDiarias => dest.RecibeVentasDiarias,
                TipoInformeEnum.VentasDetallado => dest.RecibeVentasDetallado,
                TipoInformeEnum.VentasAgrupado => dest.RecibeVentasAgrupado,
                TipoInformeEnum.VentasClasificacion => dest.RecibeVentasClasificacion,
                TipoInformeEnum.ComprasGeneral => dest.RecibeInformeCompras,
                TipoInformeEnum.ComprasDetallado => dest.RecibeComprasDetallado,
                TipoInformeEnum.NotasCreditoAgrupado => dest.RecibeNotasCredito,
                TipoInformeEnum.NotasCreditoDetallado => dest.RecibeNCDetallado,
                TipoInformeEnum.NCComprasAgrupado => dest.RecibeNCCompras,
                TipoInformeEnum.NCComprasDetallado => dest.RecibeNCCompras,
                TipoInformeEnum.ProductosValorizado => dest.RecibeProductosValorizado,
                TipoInformeEnum.ProductosDetallado => dest.RecibeProductosDetallado,
                TipoInformeEnum.MovimientosProductos => dest.RecibeMovimientosStock,
                TipoInformeEnum.AjustesStock => dest.RecibeAjustesStock,
                TipoInformeEnum.AlertaStockBajo => dest.RecibeAlertaStock,
                TipoInformeEnum.CierreCaja => dest.RecibeCierreCaja,
                TipoInformeEnum.ResumenCaja => dest.RecibeResumenCaja,
                TipoInformeEnum.CuentasPorCobrar => dest.RecibeCuentasPorCobrar,
                TipoInformeEnum.CuentasPorPagar => dest.RecibeCuentasPorPagar,
                TipoInformeEnum.Asistencia => dest.RecibeAsistencia,
                TipoInformeEnum.ResumenSifen => dest.RecibeResumenSifen,
                TipoInformeEnum.ResumenSemanal => dest.RecibeResumenSemanal,
                TipoInformeEnum.ResumenMensual => dest.RecibeResumenMensual,
                TipoInformeEnum.ResumenCierreSistema => dest.RecibeResumenCierre,
                TipoInformeEnum.AlertaVencimientos => dest.RecibeAlertaVencimientos,
                _ => false
            };
        }

        public async Task<string> GenerarHtmlInformeAsync(
            TipoInformeEnum tipoInforme, 
            int sucursalId, 
            DateTime? fechaDesde = null, 
            DateTime? fechaHasta = null)
        {
            await using var ctx = await _dbFactory.CreateDbContextAsync();
            
            var sucursal = await ctx.Sucursal.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sucursalId);
            var nombreEmpresa = sucursal?.NombreEmpresa ?? "Mi Empresa";
            var nombreSucursal = sucursal?.NombreSucursal ?? "Sucursal";

            var desde = fechaDesde ?? DateTime.Today;
            var hasta = fechaHasta ?? DateTime.Today;

            return tipoInforme switch
            {
                TipoInformeEnum.VentasDiarias => await GenerarHtmlVentasDiariasAsync(ctx, sucursalId, desde, hasta, nombreEmpresa, nombreSucursal),
                TipoInformeEnum.VentasDetallado => await GenerarHtmlVentasDetalladoAsync(ctx, sucursalId, desde, hasta, nombreEmpresa, nombreSucursal),
                TipoInformeEnum.VentasAgrupado => await GenerarHtmlVentasAgrupadoAsync(ctx, sucursalId, desde, hasta, nombreEmpresa, nombreSucursal),
                TipoInformeEnum.ComprasGeneral => await GenerarHtmlComprasGeneralAsync(ctx, sucursalId, desde, hasta, nombreEmpresa, nombreSucursal),
                TipoInformeEnum.ComprasDetallado => await GenerarHtmlComprasDetalladoAsync(ctx, sucursalId, desde, hasta, nombreEmpresa, nombreSucursal),
                TipoInformeEnum.NotasCreditoAgrupado => await GenerarHtmlNotasCreditoVentasAsync(ctx, sucursalId, desde, hasta, nombreEmpresa, nombreSucursal),
                TipoInformeEnum.NotasCreditoDetallado => await GenerarHtmlNotasCreditoVentasAsync(ctx, sucursalId, desde, hasta, nombreEmpresa, nombreSucursal),
                TipoInformeEnum.NCComprasAgrupado => await GenerarHtmlNotasCreditoComprasAsync(ctx, sucursalId, desde, hasta, nombreEmpresa, nombreSucursal),
                TipoInformeEnum.NCComprasDetallado => await GenerarHtmlNotasCreditoComprasAsync(ctx, sucursalId, desde, hasta, nombreEmpresa, nombreSucursal),
                TipoInformeEnum.ProductosValorizado => await GenerarHtmlProductosValorizadoAsync(ctx, sucursalId, nombreEmpresa, nombreSucursal),
                TipoInformeEnum.ResumenCaja => await GenerarHtmlResumenCajaAsync(ctx, sucursalId, desde, hasta, nombreEmpresa, nombreSucursal),
                TipoInformeEnum.CuentasPorCobrar => await GenerarHtmlCuentasPorCobrarAsync(ctx, sucursalId, nombreEmpresa, nombreSucursal),
                TipoInformeEnum.CuentasPorPagar => await GenerarHtmlCuentasPorPagarAsync(ctx, sucursalId, nombreEmpresa, nombreSucursal),
                TipoInformeEnum.AlertaStockBajo => await GenerarHtmlStockBajoAsync(ctx, sucursalId, nombreEmpresa, nombreSucursal),
                TipoInformeEnum.ResumenSifen => await GenerarHtmlResumenSifenAsync(ctx, sucursalId, desde, hasta, nombreEmpresa, nombreSucursal),
                TipoInformeEnum.ResumenCierreSistema => await GenerarHtmlResumenCierreAsync(ctx, sucursalId, desde, nombreEmpresa, nombreSucursal),
                _ => GenerarHtmlGenerico(tipoInforme, nombreEmpresa, nombreSucursal, desde, hasta)
            };
        }

        private string ObtenerNombreInforme(TipoInformeEnum tipo)
        {
            return TipoInformeHelper.ObtenerTodosLosInformes()
                .FirstOrDefault(i => i.Tipo == tipo)?.Nombre ?? tipo.ToString();
        }

        // ========== GENERADORES DE HTML PARA CADA TIPO DE INFORME ==========

        private async Task<string> GenerarHtmlVentasDiariasAsync(AppDbContext ctx, int sucursalId, DateTime desde, DateTime hasta, string empresa, string sucursal)
        {
            var sucursalInfo = await ctx.Sucursal.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sucursalId);
            var ruc = sucursalInfo != null ? $"{sucursalInfo.RUC}-{sucursalInfo.DV}" : null;
            var logoBase64 = sucursalInfo?.Logo != null ? Convert.ToBase64String(sucursalInfo.Logo) : null;

            var ventas = await ctx.Ventas
                .AsNoTracking()
                .Where(v => v.IdSucursal == sucursalId && v.Fecha >= desde && v.Fecha <= hasta.AddDays(1) && v.Estado != "Anulada")
                .Include(v => v.Cliente)
                .OrderBy(v => v.Fecha)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine(GenerarEncabezadoHtml("Informe de Ventas Diarias", empresa, sucursal, desde, hasta, ruc, logoBase64));
            
            sb.AppendLine(GenerarResumenBox("Resumen del Día",
                ("Total de Ventas:", ventas.Count.ToString(), false),
                ("Monto Total:", $"Gs. {ventas.Sum(v => v.Total):N0}", true)
            ));

            if (ventas.Any())
            {
                sb.AppendLine("<table class=\"tabla\">");
                sb.AppendLine("<thead><tr><th>Nro. Factura</th><th>Cliente</th><th>Fecha/Hora</th><th class=\"right\">Total</th></tr></thead>");
                sb.AppendLine("<tbody>");
                foreach (var v in ventas)
                {
                    sb.AppendLine($"<tr>");
                    sb.AppendLine($"<td>{v.NumeroFactura ?? "-"}</td>");
                    sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(v.Cliente?.RazonSocial ?? "Sin cliente")}</td>");
                    sb.AppendLine($"<td>{v.Fecha:dd/MM/yyyy HH:mm}</td>");
                    sb.AppendLine($"<td class=\"right\">{v.Total:N0}</td>");
                    sb.AppendLine($"</tr>");
                }
                sb.AppendLine("</tbody>");
                sb.AppendLine($"<tfoot><tr><th colspan=\"3\" class=\"right\">TOTAL</th><th class=\"right\">{ventas.Sum(v => v.Total):N0}</th></tr></tfoot>");
                sb.AppendLine("</table>");
            }
            else
            {
                sb.AppendLine("<p>No hay ventas en el período seleccionado.</p>");
            }

            sb.AppendLine(GenerarPieHtml());
            return sb.ToString();
        }

        private async Task<string> GenerarHtmlVentasDetalladoAsync(AppDbContext ctx, int sucursalId, DateTime desde, DateTime hasta, string empresa, string sucursal)
        {
            // Obtener info de sucursal para RUC y logo
            var sucursalInfo = await ctx.Sucursal.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sucursalId);
            var ruc = sucursalInfo != null ? $"{sucursalInfo.RUC}-{sucursalInfo.DV}" : null;
            var logoBase64 = sucursalInfo?.Logo != null ? Convert.ToBase64String(sucursalInfo.Logo) : null;

            var ventas = await ctx.Ventas
                .AsNoTracking()
                .Where(v => v.IdSucursal == sucursalId && v.Fecha >= desde && v.Fecha <= hasta.AddDays(1) && v.Estado != "Anulada")
                .Include(v => v.Cliente)
                .Include(v => v.Moneda)
                .OrderBy(v => v.Fecha)
                .ToListAsync();

            // Cargar detalles con producto
            var ventaIds = ventas.Select(v => v.IdVenta).ToList();
            var detalles = await ctx.VentasDetalles
                .AsNoTracking()
                .Where(d => ventaIds.Contains(d.IdVenta))
                .Include(d => d.Producto)
                .ToListAsync();
            var detallesPorVenta = detalles.GroupBy(d => d.IdVenta).ToDictionary(g => g.Key, g => g.ToList());

            // Crear filas planas (como el informe del sistema)
            var filas = new List<(int IdVenta, DateTime Fecha, string Numero, string Cliente, string Producto, 
                decimal Cantidad, decimal Costo, decimal Precio, string Moneda, decimal Cambio, 
                decimal Importe, decimal ImporteGs, decimal ImporteUsd, int? IdCaja, int? Turno)>();

            foreach (var v in ventas)
            {
                if (detallesPorVenta.TryGetValue(v.IdVenta, out var dets))
                {
                    foreach (var d in dets)
                    {
                        var cambio = v.CambioDelDia ?? 1m;
                        var importeGs = v.Moneda?.CodigoISO == "PYG" ? d.Importe : d.Importe * cambio;
                        var importeUsd = v.Moneda?.CodigoISO == "USD" ? d.Importe : (cambio > 0 ? d.Importe / cambio : 0m);
                        
                        filas.Add((
                            v.IdVenta,
                            v.Fecha,
                            v.NumeroFactura ?? "-",
                            v.Cliente?.RazonSocial ?? "Sin cliente",
                            d.Producto?.Descripcion ?? "Producto",
                            d.Cantidad,
                            d.Producto?.CostoUnitarioGs ?? 0m,
                            d.PrecioUnitario,
                            v.Moneda?.CodigoISO ?? "PYG",
                            cambio,
                            d.Importe,
                            importeGs,
                            importeUsd,
                            v.IdCaja,
                            v.Turno
                        ));
                    }
                }
            }

            var sb = new StringBuilder();
            sb.AppendLine(GenerarEncabezadoHtml("Listado Ventas Detallado", empresa, sucursal, desde, hasta, ruc, logoBase64));

            // Tabla con formato idéntico al sistema
            sb.AppendLine("<table class=\"tabla\">");
            sb.AppendLine("<thead><tr>");
            sb.AppendLine("<th>ID</th><th>Fecha</th><th>Doc.</th><th>Cliente</th><th>Producto</th>");
            sb.AppendLine("<th class=\"right\">Cant.</th><th class=\"right\">Costo</th><th class=\"right\">Precio</th>");
            sb.AppendLine("<th>Mon</th><th class=\"right\">Cambio</th><th class=\"right\">Importe</th>");
            sb.AppendLine("<th class=\"right\">Importe Gs</th><th class=\"right\">Importe $</th>");
            sb.AppendLine("<th>Caja</th><th>Turno</th>");
            sb.AppendLine("</tr></thead>");
            sb.AppendLine("<tbody>");

            foreach (var r in filas)
            {
                sb.AppendLine($"<tr>");
                sb.AppendLine($"<td>{r.IdVenta}</td>");
                sb.AppendLine($"<td>{r.Fecha:dd/MM/yyyy}</td>");
                sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(r.Numero)}</td>");
                sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(r.Cliente)}</td>");
                sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(r.Producto)}</td>");
                sb.AppendLine($"<td class=\"right\">{r.Cantidad:N2}</td>");
                sb.AppendLine($"<td class=\"right\">{r.Costo:N0}</td>");
                sb.AppendLine($"<td class=\"right\">{r.Precio:N0}</td>");
                sb.AppendLine($"<td>{r.Moneda}</td>");
                sb.AppendLine($"<td class=\"right\">{r.Cambio:N2}</td>");
                sb.AppendLine($"<td class=\"right\">{r.Importe:N0}</td>");
                sb.AppendLine($"<td class=\"right\">{r.ImporteGs:N0}</td>");
                sb.AppendLine($"<td class=\"right\">{r.ImporteUsd:N2}</td>");
                sb.AppendLine($"<td>{r.IdCaja?.ToString() ?? "-"}</td>");
                sb.AppendLine($"<td>{r.Turno?.ToString() ?? "-"}</td>");
                sb.AppendLine($"</tr>");
            }

            sb.AppendLine("</tbody>");
            sb.AppendLine($"<tfoot><tr>");
            sb.AppendLine($"<th colspan=\"11\" class=\"right\">Totales</th>");
            sb.AppendLine($"<th class=\"right\">{filas.Sum(f => f.ImporteGs):N0}</th>");
            sb.AppendLine($"<th class=\"right\">{filas.Sum(f => f.ImporteUsd):N2}</th>");
            sb.AppendLine($"<th></th><th></th>");
            sb.AppendLine($"</tr></tfoot>");
            sb.AppendLine("</table>");

            sb.AppendLine(GenerarPieHtml());
            return sb.ToString();
        }

        private async Task<string> GenerarHtmlVentasAgrupadoAsync(AppDbContext ctx, int sucursalId, DateTime desde, DateTime hasta, string empresa, string sucursal)
        {
            var sucursalInfo = await ctx.Sucursal.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sucursalId);
            var ruc = sucursalInfo != null ? $"{sucursalInfo.RUC}-{sucursalInfo.DV}" : null;
            var logoBase64 = sucursalInfo?.Logo != null ? Convert.ToBase64String(sucursalInfo.Logo) : null;

            var ventas = await ctx.Ventas
                .AsNoTracking()
                .Where(v => v.IdSucursal == sucursalId && v.Fecha >= desde && v.Fecha <= hasta.AddDays(1) && v.Estado != "Anulada")
                .GroupBy(v => v.Fecha.Date)
                .Select(g => new { Fecha = g.Key, Cantidad = g.Count(), Total = g.Sum(v => v.Total) })
                .OrderBy(x => x.Fecha)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine(GenerarEncabezadoHtml("Listado Ventas Agrupado", empresa, sucursal, desde, hasta, ruc, logoBase64));

            sb.AppendLine("<table class=\"tabla\">");
            sb.AppendLine("<thead><tr><th>Fecha</th><th class=\"right\">Cantidad</th><th class=\"right\">Total Gs.</th></tr></thead>");
            sb.AppendLine("<tbody>");
            foreach (var item in ventas)
            {
                sb.AppendLine($"<tr><td>{item.Fecha:dd/MM/yyyy}</td><td class=\"right\">{item.Cantidad}</td><td class=\"right\">{item.Total:N0}</td></tr>");
            }
            sb.AppendLine("</tbody>");
            sb.AppendLine($"<tfoot><tr><th>TOTAL</th><th class=\"right\">{ventas.Sum(v => v.Cantidad)}</th><th class=\"right\">{ventas.Sum(v => v.Total):N0}</th></tr></tfoot>");
            sb.AppendLine("</table>");

            sb.AppendLine(GenerarPieHtml());
            return sb.ToString();
        }

        private async Task<string> GenerarHtmlComprasGeneralAsync(AppDbContext ctx, int sucursalId, DateTime desde, DateTime hasta, string empresa, string sucursal)
        {
            var sucursalInfo = await ctx.Sucursal.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sucursalId);
            var ruc = sucursalInfo != null ? $"{sucursalInfo.RUC}-{sucursalInfo.DV}" : null;
            var logoBase64 = sucursalInfo?.Logo != null ? Convert.ToBase64String(sucursalInfo.Logo) : null;

            var compras = await ctx.Compras
                .AsNoTracking()
                .Where(c => c.IdSucursal == sucursalId && c.Fecha >= desde && c.Fecha <= hasta.AddDays(1) && c.Estado != "Anulada")
                .Include(c => c.Proveedor)
                .OrderBy(c => c.Fecha)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine(GenerarEncabezadoHtml("Listado de Compras", empresa, sucursal, desde, hasta, ruc, logoBase64));

            // Resumen
            sb.AppendLine(GenerarResumenBox("Resumen",
                ("Total de Compras:", compras.Count.ToString(), false),
                ("Monto Total:", $"Gs. {compras.Sum(c => c.Total):N0}", true)
            ));

            if (compras.Any())
            {
                sb.AppendLine("<table class=\"tabla\">");
                sb.AppendLine("<thead><tr><th>ID</th><th>Nro. Factura</th><th>Proveedor</th><th>RUC</th><th>Fecha</th><th class=\"right\">Total Gs.</th></tr></thead>");
                sb.AppendLine("<tbody>");
                foreach (var c in compras)
                {
                    sb.AppendLine($"<tr>");
                    sb.AppendLine($"<td>{c.IdCompra}</td>");
                    sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(c.NumeroFactura ?? "-")}</td>");
                    sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(c.Proveedor?.RazonSocial ?? "Sin proveedor")}</td>");
                    sb.AppendLine($"<td>{c.Proveedor?.RUC ?? "-"}</td>");
                    sb.AppendLine($"<td>{c.Fecha:dd/MM/yyyy}</td>");
                    sb.AppendLine($"<td class=\"right\">{c.Total:N0}</td>");
                    sb.AppendLine($"</tr>");
                }
                sb.AppendLine("</tbody>");
                sb.AppendLine($"<tfoot><tr><th colspan=\"5\" class=\"right\">TOTAL</th><th class=\"right\">{compras.Sum(c => c.Total):N0}</th></tr></tfoot>");
                sb.AppendLine("</table>");
            }

            sb.AppendLine(GenerarPieHtml());
            return sb.ToString();
        }

        private async Task<string> GenerarHtmlComprasDetalladoAsync(AppDbContext ctx, int sucursalId, DateTime desde, DateTime hasta, string empresa, string sucursal)
        {
            var sucursalInfo = await ctx.Sucursal.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sucursalId);
            var ruc = sucursalInfo != null ? $"{sucursalInfo.RUC}-{sucursalInfo.DV}" : null;
            var logoBase64 = sucursalInfo?.Logo != null ? Convert.ToBase64String(sucursalInfo.Logo) : null;

            var compras = await ctx.Compras
                .AsNoTracking()
                .Where(c => c.IdSucursal == sucursalId && c.Fecha >= desde && c.Fecha <= hasta.AddDays(1) && c.Estado != "Anulada")
                .Include(c => c.Proveedor)
                .OrderBy(c => c.Fecha)
                .ToListAsync();

            var compraIds = compras.Select(c => c.IdCompra).ToList();
            var detalles = await ctx.ComprasDetalles
                .AsNoTracking()
                .Where(d => compraIds.Contains(d.IdCompra))
                .Include(d => d.Producto)
                .ToListAsync();

            // Filas planas
            var filas = new List<(int IdCompra, DateTime Fecha, string NumFact, string Proveedor, 
                string Producto, decimal Cantidad, decimal PrecioUnit, decimal Importe)>();

            foreach (var c in compras)
            {
                var dets = detalles.Where(d => d.IdCompra == c.IdCompra);
                foreach (var d in dets)
                {
                    filas.Add((c.IdCompra, c.Fecha, c.NumeroFactura ?? "-", 
                        c.Proveedor?.RazonSocial ?? "Sin proveedor",
                        d.Producto?.Descripcion ?? "Producto",
                        d.Cantidad, d.PrecioUnitario, d.Importe));
                }
            }

            var sb = new StringBuilder();
            sb.AppendLine(GenerarEncabezadoHtml("Listado Compras Detallado", empresa, sucursal, desde, hasta, ruc, logoBase64));

            sb.AppendLine("<table class=\"tabla\">");
            sb.AppendLine("<thead><tr><th>ID</th><th>Fecha</th><th>Nro. Factura</th><th>Proveedor</th><th>Producto</th><th class=\"right\">Cant.</th><th class=\"right\">P.Unit.</th><th class=\"right\">Importe</th></tr></thead>");
            sb.AppendLine("<tbody>");
            foreach (var r in filas)
            {
                sb.AppendLine($"<tr>");
                sb.AppendLine($"<td>{r.IdCompra}</td>");
                sb.AppendLine($"<td>{r.Fecha:dd/MM/yyyy}</td>");
                sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(r.NumFact)}</td>");
                sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(r.Proveedor)}</td>");
                sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(r.Producto)}</td>");
                sb.AppendLine($"<td class=\"right\">{r.Cantidad:N2}</td>");
                sb.AppendLine($"<td class=\"right\">{r.PrecioUnit:N0}</td>");
                sb.AppendLine($"<td class=\"right\">{r.Importe:N0}</td>");
                sb.AppendLine($"</tr>");
            }
            sb.AppendLine("</tbody>");
            sb.AppendLine($"<tfoot><tr><th colspan=\"7\" class=\"right\">TOTAL</th><th class=\"right\">{filas.Sum(f => f.Importe):N0}</th></tr></tfoot>");
            sb.AppendLine("</table>");

            sb.AppendLine(GenerarPieHtml());
            return sb.ToString();
        }

        private async Task<string> GenerarHtmlResumenCajaAsync(AppDbContext ctx, int sucursalId, DateTime desde, DateTime hasta, string empresa, string sucursal)
        {
            var sucursalInfo = await ctx.Sucursal.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sucursalId);
            var ruc = sucursalInfo != null ? $"{sucursalInfo.RUC}-{sucursalInfo.DV}" : null;
            var logoBase64 = sucursalInfo?.Logo != null ? Convert.ToBase64String(sucursalInfo.Logo) : null;

            var cierres = await ctx.CierresCaja
                .AsNoTracking()
                .Where(c => c.IdSucursal == sucursalId && c.FechaCaja >= desde && c.FechaCaja <= hasta)
                .OrderBy(c => c.FechaCaja)
                .ToListAsync();

            var ventas = await ctx.Ventas
                .AsNoTracking()
                .Where(v => v.IdSucursal == sucursalId && v.Fecha >= desde && v.Fecha <= hasta.AddDays(1) && v.Estado != "Anulada")
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine(GenerarEncabezadoHtml("Resumen de Caja", empresa, sucursal, desde, hasta, ruc, logoBase64));

            // Resumen del período
            sb.AppendLine(GenerarResumenBox("Resumen del Período",
                ("Total Ventas:", $"Gs. {ventas.Sum(v => v.Total):N0}", false),
                ("Cantidad de Ventas:", ventas.Count.ToString(), false),
                ("Cierres de Caja:", cierres.Count.ToString(), false)
            ));

            if (cierres.Any())
            {
                // Tabla de operaciones
                sb.AppendLine("<table class=\"tabla\">");
                sb.AppendLine("<thead><tr><th>Concepto</th><th class=\"right\">Ingresos</th><th class=\"right\">Egresos</th></tr></thead>");
                sb.AppendLine("<tbody>");
                
                var ventasContado = cierres.Sum(c => c.TotalVentasContado);
                var cobrosCredito = cierres.Sum(c => c.TotalCobrosCredito);
                var ncVentas = cierres.Sum(c => c.TotalNotasCredito);
                var comprasEfect = cierres.Sum(c => c.TotalComprasEfectivo);
                var ncCompras = cierres.Sum(c => c.TotalNotasCreditoCompras);
                var anulaciones = cierres.Sum(c => c.TotalAnulaciones);

                sb.AppendLine($"<tr><td>Ventas Contado</td><td class=\"right\">{ventasContado:N0}</td><td class=\"right\">-</td></tr>");
                sb.AppendLine($"<tr><td>Cobros Crédito</td><td class=\"right\">{cobrosCredito:N0}</td><td class=\"right\">-</td></tr>");
                if (ncCompras > 0)
                    sb.AppendLine($"<tr><td>NC Compras (devolución)</td><td class=\"right\">{ncCompras:N0}</td><td class=\"right\">-</td></tr>");
                if (ncVentas > 0)
                    sb.AppendLine($"<tr><td>NC Ventas (devolución cliente)</td><td class=\"right\">-</td><td class=\"right\">{ncVentas:N0}</td></tr>");
                if (comprasEfect > 0)
                    sb.AppendLine($"<tr><td>Compras Efectivo</td><td class=\"right\">-</td><td class=\"right\">{comprasEfect:N0}</td></tr>");
                if (anulaciones > 0)
                    sb.AppendLine($"<tr><td>Anulaciones</td><td class=\"right\">-</td><td class=\"right\">{anulaciones:N0}</td></tr>");
                
                var totalIngresos = ventasContado + cobrosCredito + ncCompras;
                var totalEgresos = ncVentas + comprasEfect + anulaciones;
                var netoEnCaja = totalIngresos - totalEgresos;

                sb.AppendLine("</tbody>");
                sb.AppendLine($"<tfoot>");
                sb.AppendLine($"<tr><th>TOTALES</th><th class=\"right\">{totalIngresos:N0}</th><th class=\"right\">{totalEgresos:N0}</th></tr>");
                sb.AppendLine($"<tr><th colspan=\"2\">NETO EN CAJA</th><th class=\"right\">{netoEnCaja:N0}</th></tr>");
                sb.AppendLine($"</tfoot>");
                sb.AppendLine("</table>");

                // Detalle de entrega
                sb.AppendLine(GenerarResumenBox("Entrega",
                    ("Total Entregado:", $"Gs. {cierres.Sum(c => c.TotalEntregado):N0}", false),
                    ("Diferencia:", $"Gs. {cierres.Sum(c => c.Diferencia):N0}", cierres.Sum(c => c.Diferencia) != 0)
                ));
            }

            sb.AppendLine(GenerarPieHtml());
            return sb.ToString();
        }

        private async Task<string> GenerarHtmlCuentasPorCobrarAsync(AppDbContext ctx, int sucursalId, string empresa, string sucursal)
        {
            var sucursalInfo = await ctx.Sucursal.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sucursalId);
            var ruc = sucursalInfo != null ? $"{sucursalInfo.RUC}-{sucursalInfo.DV}" : null;
            var logoBase64 = sucursalInfo?.Logo != null ? Convert.ToBase64String(sucursalInfo.Logo) : null;

            var cuentas = await ctx.CuentasPorCobrar
                .AsNoTracking()
                .Where(c => c.IdSucursal == sucursalId && c.Estado == "PENDIENTE")
                .Include(c => c.Cliente)
                .OrderBy(c => c.FechaVencimiento)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine(GenerarEncabezadoHtml("Cuentas por Cobrar", empresa, sucursal, null, null, ruc, logoBase64));

            var vencidas = cuentas.Where(c => c.FechaVencimiento < DateTime.Today).ToList();
            sb.AppendLine(GenerarResumenBox("Resumen",
                ("Total Pendiente:", $"Gs. {cuentas.Sum(c => c.SaldoPendiente):N0}", false),
                ("Cantidad de Cuentas:", cuentas.Count.ToString(), false),
                ("Vencidas:", vencidas.Any() ? $"Gs. {vencidas.Sum(c => c.SaldoPendiente):N0} ({vencidas.Count})" : "0", vencidas.Any())
            ));

            if (cuentas.Any())
            {
                sb.AppendLine("<table class=\"tabla\">");
                sb.AppendLine("<thead><tr><th>Cliente</th><th>RUC</th><th>Vencimiento</th><th class=\"right\">Saldo Gs.</th><th>Estado</th></tr></thead>");
                sb.AppendLine("<tbody>");
                foreach (var c in cuentas.Take(100))
                {
                    var vencida = c.FechaVencimiento < DateTime.Today;
                    var estilo = vencida ? " style=\"color:red;\"" : "";
                    sb.AppendLine($"<tr{estilo}>");
                    sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(c.Cliente?.RazonSocial ?? "-")}</td>");
                    sb.AppendLine($"<td>{c.Cliente?.RUC ?? "-"}</td>");
                    sb.AppendLine($"<td>{c.FechaVencimiento:dd/MM/yyyy}</td>");
                    sb.AppendLine($"<td class=\"right\">{c.SaldoPendiente:N0}</td>");
                    sb.AppendLine($"<td>{(vencida ? "VENCIDA" : "Vigente")}</td>");
                    sb.AppendLine($"</tr>");
                }
                sb.AppendLine("</tbody>");
                sb.AppendLine($"<tfoot><tr><th colspan=\"3\">TOTAL</th><th class=\"right\">{cuentas.Sum(c => c.SaldoPendiente):N0}</th><th></th></tr></tfoot>");
                sb.AppendLine("</table>");
            }

            sb.AppendLine(GenerarPieHtml());
            return sb.ToString();
        }

        private async Task<string> GenerarHtmlCuentasPorPagarAsync(AppDbContext ctx, int sucursalId, string empresa, string sucursal)
        {
            var sucursalInfo = await ctx.Sucursal.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sucursalId);
            var ruc = sucursalInfo != null ? $"{sucursalInfo.RUC}-{sucursalInfo.DV}" : null;
            var logoBase64 = sucursalInfo?.Logo != null ? Convert.ToBase64String(sucursalInfo.Logo) : null;

            var cuentas = await ctx.CuentasPorPagar
                .AsNoTracking()
                .Where(c => c.IdSucursal == sucursalId && c.Estado == "PENDIENTE")
                .Include(c => c.Proveedor)
                .OrderBy(c => c.FechaVencimiento)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine(GenerarEncabezadoHtml("Cuentas por Pagar", empresa, sucursal, null, null, ruc, logoBase64));

            var vencidas = cuentas.Where(c => c.FechaVencimiento < DateTime.Today).ToList();
            sb.AppendLine(GenerarResumenBox("Resumen",
                ("Total Pendiente:", $"Gs. {cuentas.Sum(c => c.SaldoPendiente):N0}", false),
                ("Cantidad de Cuentas:", cuentas.Count.ToString(), false),
                ("Vencidas:", vencidas.Any() ? $"Gs. {vencidas.Sum(c => c.SaldoPendiente):N0} ({vencidas.Count})" : "0", vencidas.Any())
            ));

            if (cuentas.Any())
            {
                sb.AppendLine("<table class=\"tabla\">");
                sb.AppendLine("<thead><tr><th>Proveedor</th><th>RUC</th><th>Vencimiento</th><th class=\"right\">Saldo Gs.</th><th>Estado</th></tr></thead>");
                sb.AppendLine("<tbody>");
                foreach (var c in cuentas.Take(100))
                {
                    var vencida = c.FechaVencimiento < DateTime.Today;
                    var estilo = vencida ? " style=\"color:red;\"" : "";
                    sb.AppendLine($"<tr{estilo}>");
                    sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(c.Proveedor?.RazonSocial ?? "-")}</td>");
                    sb.AppendLine($"<td>{c.Proveedor?.RUC ?? "-"}</td>");
                    sb.AppendLine($"<td>{c.FechaVencimiento:dd/MM/yyyy}</td>");
                    sb.AppendLine($"<td class=\"right\">{c.SaldoPendiente:N0}</td>");
                    sb.AppendLine($"<td>{(vencida ? "VENCIDA" : "Vigente")}</td>");
                    sb.AppendLine($"</tr>");
                }
                sb.AppendLine("</tbody>");
                sb.AppendLine($"<tfoot><tr><th colspan=\"3\">TOTAL</th><th class=\"right\">{cuentas.Sum(c => c.SaldoPendiente):N0}</th><th></th></tr></tfoot>");
                sb.AppendLine("</table>");
            }

            sb.AppendLine(GenerarPieHtml());
            return sb.ToString();
        }

        private async Task<string> GenerarHtmlStockBajoAsync(AppDbContext ctx, int sucursalId, string empresa, string sucursal)
        {
            var sucursalInfo = await ctx.Sucursal.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sucursalId);
            var ruc = sucursalInfo != null ? $"{sucursalInfo.RUC}-{sucursalInfo.DV}" : null;
            var logoBase64 = sucursalInfo?.Logo != null ? Convert.ToBase64String(sucursalInfo.Logo) : null;

            var productos = await ctx.Productos
                .AsNoTracking()
                .Where(p => p.Activo && p.Stock < p.StockMinimo)
                .OrderBy(p => p.Stock - p.StockMinimo)
                .Take(100)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine(GenerarEncabezadoHtml("Alerta de Stock Bajo", empresa, sucursal, null, null, ruc, logoBase64));

            sb.AppendLine(GenerarResumenBox("⚠️ Productos con Stock Bajo",
                ("Cantidad de Productos:", productos.Count.ToString(), false)
            ));

            if (productos.Any())
            {
                sb.AppendLine("<table class=\"tabla\">");
                sb.AppendLine("<thead><tr><th>Código</th><th>Producto</th><th class=\"right\">Stock</th><th class=\"right\">Mínimo</th><th class=\"right\">Faltante</th></tr></thead>");
                sb.AppendLine("<tbody>");
                foreach (var p in productos)
                {
                    var faltante = p.StockMinimo - p.Stock;
                    var estilo = p.Stock <= 0 ? " style=\"color:red;\"" : " style=\"color:orange;\"";
                    sb.AppendLine($"<tr{estilo}>");
                    sb.AppendLine($"<td>{p.CodigoBarras ?? p.CodigoInterno ?? "-"}</td>");
                    sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(p.Descripcion)}</td>");
                    sb.AppendLine($"<td class=\"right\">{p.Stock:N0}</td>");
                    sb.AppendLine($"<td class=\"right\">{p.StockMinimo:N0}</td>");
                    sb.AppendLine($"<td class=\"right\">{faltante:N0}</td>");
                    sb.AppendLine($"</tr>");
                }
                sb.AppendLine("</tbody></table>");
            }

            sb.AppendLine(GenerarPieHtml());
            return sb.ToString();
        }

        private async Task<string> GenerarHtmlResumenSifenAsync(AppDbContext ctx, int sucursalId, DateTime desde, DateTime hasta, string empresa, string sucursal)
        {
            var ventas = await ctx.Ventas
                .AsNoTracking()
                .Where(v => v.IdSucursal == sucursalId && v.Fecha >= desde && v.Fecha <= hasta.AddDays(1))
                .ToListAsync();

            var aprobadas = ventas.Where(v => v.EstadoSifen == "Aprobado").ToList();
            var pendientes = ventas.Where(v => string.IsNullOrEmpty(v.EstadoSifen) || v.EstadoSifen == "Pendiente").ToList();
            var rechazadas = ventas.Where(v => v.EstadoSifen == "Rechazado").ToList();

            var sucursalInfo = await ctx.Sucursal.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sucursalId);
            var ruc = sucursalInfo != null ? $"{sucursalInfo.RUC}-{sucursalInfo.DV}" : null;
            var logoBase64 = sucursalInfo?.Logo != null ? Convert.ToBase64String(sucursalInfo.Logo) : null;

            var sb = new StringBuilder();
            sb.AppendLine(GenerarEncabezadoHtml("Resumen SIFEN", empresa, sucursal, desde, hasta, ruc, logoBase64));

            // Resumen con colores
            sb.AppendLine("<div class=\"resumen-box\">");
            sb.AppendLine("<h4>Estado de Documentos Electrónicos</h4>");
            sb.AppendLine("<table style=\"width:100%;border-collapse:collapse;\">");
            sb.AppendLine($"<tr><td style=\"padding:4px 8px;\">Total Documentos:</td><td style=\"padding:4px 8px;\"><strong>{ventas.Count}</strong></td></tr>");
            sb.AppendLine($"<tr style=\"color:#28a745;\"><td style=\"padding:4px 8px;\">✅ Aprobados:</td><td style=\"padding:4px 8px;\"><strong>{aprobadas.Count}</strong></td></tr>");
            sb.AppendLine($"<tr style=\"color:#ffc107;\"><td style=\"padding:4px 8px;\">⏳ Pendientes:</td><td style=\"padding:4px 8px;\"><strong>{pendientes.Count}</strong></td></tr>");
            if (rechazadas.Any())
            {
                sb.AppendLine($"<tr style=\"color:#dc3545;\"><td style=\"padding:4px 8px;\">❌ Rechazados:</td><td style=\"padding:4px 8px;\"><strong>{rechazadas.Count}</strong></td></tr>");
            }
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");

            if (rechazadas.Any())
            {
                sb.AppendLine("<h4 style=\"color:#dc3545;margin-top:20px;\">Documentos Rechazados</h4>");
                sb.AppendLine("<table class=\"tabla\">");
                sb.AppendLine("<thead><tr><th>Factura</th><th>Fecha</th><th>Mensaje de Error</th></tr></thead>");
                sb.AppendLine("<tbody>");
                foreach (var v in rechazadas.Take(20))
                {
                    sb.AppendLine($"<tr>");
                    sb.AppendLine($"<td>{v.NumeroFactura}</td>");
                    sb.AppendLine($"<td>{v.Fecha:dd/MM/yyyy}</td>");
                    sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(v.MensajeSifen ?? "-")}</td>");
                    sb.AppendLine($"</tr>");
                }
                sb.AppendLine("</tbody></table>");
                if (rechazadas.Count > 20)
                {
                    sb.AppendLine($"<p class=\"muted\">Mostrando 20 de {rechazadas.Count} documentos rechazados.</p>");
                }
            }

            sb.AppendLine(GenerarPieHtml());
            return sb.ToString();
        }

        private async Task<string> GenerarHtmlResumenCierreAsync(AppDbContext ctx, int sucursalId, DateTime fecha, string empresa, string sucursal)
        {
            var ventas = await ctx.Ventas
                .AsNoTracking()
                .Where(v => v.IdSucursal == sucursalId && v.Fecha.Date == fecha.Date && v.Estado != "Anulada")
                .ToListAsync();

            var compras = await ctx.Compras
                .AsNoTracking()
                .Where(c => c.IdSucursal == sucursalId && c.Fecha.Date == fecha.Date && c.Estado != "Anulada")
                .ToListAsync();

            var cxc = await ctx.CuentasPorCobrar
                .AsNoTracking()
                .Where(c => c.IdSucursal == sucursalId && c.Estado == "PENDIENTE")
                .SumAsync(c => c.SaldoPendiente);

            var cxp = await ctx.CuentasPorPagar
                .AsNoTracking()
                .Where(c => c.IdSucursal == sucursalId && c.Estado == "PENDIENTE")
                .SumAsync(c => c.SaldoPendiente);

            var sucursalInfo = await ctx.Sucursal.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sucursalId);
            var ruc = sucursalInfo != null ? $"{sucursalInfo.RUC}-{sucursalInfo.DV}" : null;
            var logoBase64 = sucursalInfo?.Logo != null ? Convert.ToBase64String(sucursalInfo.Logo) : null;

            var sb = new StringBuilder();
            sb.AppendLine(GenerarEncabezadoHtml("Resumen al Cierre del Sistema", empresa, sucursal, fecha, fecha, ruc, logoBase64));

            // Sección VENTAS
            sb.AppendLine("<div class=\"resumen-box\" style=\"border-left:4px solid #007bff;\">");
            sb.AppendLine("<h4 style=\"color:#007bff;margin:0 0 10px 0;\">📈 VENTAS</h4>");
            sb.AppendLine("<table style=\"width:100%;border-collapse:collapse;\">");
            sb.AppendLine($"<tr><td style=\"padding:4px 8px;\">Cantidad:</td><td style=\"padding:4px 8px;\"><strong>{ventas.Count}</strong></td></tr>");
            sb.AppendLine($"<tr><td style=\"padding:4px 8px;\">Total Ventas:</td><td style=\"padding:4px 8px;\"><strong style=\"color:#28a745;\">Gs. {ventas.Sum(v => v.Total):N0}</strong></td></tr>");
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");

            // Sección COMPRAS
            sb.AppendLine("<div class=\"resumen-box\" style=\"border-left:4px solid #17a2b8;\">");
            sb.AppendLine("<h4 style=\"color:#17a2b8;margin:0 0 10px 0;\">📉 COMPRAS</h4>");
            sb.AppendLine("<table style=\"width:100%;border-collapse:collapse;\">");
            sb.AppendLine($"<tr><td style=\"padding:4px 8px;\">Cantidad:</td><td style=\"padding:4px 8px;\"><strong>{compras.Count}</strong></td></tr>");
            sb.AppendLine($"<tr><td style=\"padding:4px 8px;\">Total Compras:</td><td style=\"padding:4px 8px;\"><strong style=\"color:#dc3545;\">Gs. {compras.Sum(c => c.Total):N0}</strong></td></tr>");
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");

            // Sección SALDOS
            sb.AppendLine("<div class=\"resumen-box\" style=\"border-left:4px solid #6c757d;\">");
            sb.AppendLine("<h4 style=\"color:#6c757d;margin:0 0 10px 0;\">💰 SALDOS PENDIENTES</h4>");
            sb.AppendLine("<table style=\"width:100%;border-collapse:collapse;\">");
            sb.AppendLine($"<tr><td style=\"padding:4px 8px;\">Cuentas por Cobrar:</td><td style=\"padding:4px 8px;\"><strong>Gs. {cxc:N0}</strong></td></tr>");
            sb.AppendLine($"<tr><td style=\"padding:4px 8px;\">Cuentas por Pagar:</td><td style=\"padding:4px 8px;\"><strong>Gs. {cxp:N0}</strong></td></tr>");
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");

            sb.AppendLine(GenerarPieHtml());
            return sb.ToString();
        }

        private async Task<string> GenerarHtmlNotasCreditoVentasAsync(AppDbContext ctx, int sucursalId, DateTime desde, DateTime hasta, string empresa, string sucursal)
        {
            var sucursalInfo = await ctx.Sucursal.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sucursalId);
            var ruc = sucursalInfo != null ? $"{sucursalInfo.RUC}-{sucursalInfo.DV}" : null;
            var logoBase64 = sucursalInfo?.Logo != null ? Convert.ToBase64String(sucursalInfo.Logo) : null;

            var ncs = await ctx.NotasCreditoVentas
                .AsNoTracking()
                .Where(nc => nc.IdSucursal == sucursalId && nc.Fecha.Date >= desde.Date && nc.Fecha.Date <= hasta.Date && nc.Estado != "Anulada")
                .Include(nc => nc.Cliente)
                .OrderBy(nc => nc.Fecha)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine(GenerarEncabezadoHtml("Notas de Crédito - Ventas", empresa, sucursal, desde, hasta, ruc, logoBase64));
            
            sb.AppendLine(GenerarResumenBox("Resumen",
                ("Total NC:", ncs.Count.ToString(), false),
                ("Monto Total:", $"Gs. {ncs.Sum(nc => nc.Total):N0}", true)
            ));

            if (ncs.Any())
            {
                sb.AppendLine("<table class=\"tabla\">");
                sb.AppendLine("<thead><tr><th>ID</th><th>Número</th><th>Cliente</th><th>RUC</th><th>Motivo</th><th>Fecha</th><th class=\"right\">Total Gs.</th></tr></thead>");
                sb.AppendLine("<tbody>");
                foreach (var nc in ncs)
                {
                    var numNC = $"{nc.Establecimiento}-{nc.PuntoExpedicion}-{nc.NumeroNota:D7}";
                    sb.AppendLine($"<tr>");
                    sb.AppendLine($"<td>{nc.IdNotaCreditoVenta}</td>");
                    sb.AppendLine($"<td>{numNC}</td>");
                    sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(nc.Cliente?.RazonSocial ?? nc.NombreCliente ?? "-")}</td>");
                    sb.AppendLine($"<td>{nc.Cliente?.RUC ?? "-"}</td>");
                    sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(nc.Motivo ?? "-")}</td>");
                    sb.AppendLine($"<td>{nc.Fecha:dd/MM/yyyy}</td>");
                    sb.AppendLine($"<td class=\"right\">{nc.Total:N0}</td>");
                    sb.AppendLine($"</tr>");
                }
                sb.AppendLine("</tbody>");
                sb.AppendLine($"<tfoot><tr><th colspan=\"6\" class=\"right\">TOTAL</th><th class=\"right\">{ncs.Sum(nc => nc.Total):N0}</th></tr></tfoot>");
                sb.AppendLine("</table>");
            }
            else
            {
                sb.AppendLine("<p>No hay notas de crédito en el período seleccionado.</p>");
            }

            sb.AppendLine(GenerarPieHtml());
            return sb.ToString();
        }

        private async Task<string> GenerarHtmlNotasCreditoComprasAsync(AppDbContext ctx, int sucursalId, DateTime desde, DateTime hasta, string empresa, string sucursal)
        {
            var sucursalInfo = await ctx.Sucursal.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sucursalId);
            var ruc = sucursalInfo != null ? $"{sucursalInfo.RUC}-{sucursalInfo.DV}" : null;
            var logoBase64 = sucursalInfo?.Logo != null ? Convert.ToBase64String(sucursalInfo.Logo) : null;

            var ncs = await ctx.NotasCreditoCompras
                .AsNoTracking()
                .Where(nc => nc.IdSucursal == sucursalId && nc.Fecha.Date >= desde.Date && nc.Fecha.Date <= hasta.Date && nc.Estado != "Anulada")
                .Include(nc => nc.Proveedor)
                .OrderBy(nc => nc.Fecha)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine(GenerarEncabezadoHtml("Notas de Crédito - Compras", empresa, sucursal, desde, hasta, ruc, logoBase64));
            
            sb.AppendLine(GenerarResumenBox("Resumen",
                ("Total NC:", ncs.Count.ToString(), false),
                ("Monto Total (crédito):", $"Gs. {ncs.Sum(nc => nc.Total):N0}", true)
            ));

            if (ncs.Any())
            {
                sb.AppendLine("<table class=\"tabla\">");
                sb.AppendLine("<thead><tr><th>ID</th><th>Número NC</th><th>Proveedor</th><th>RUC</th><th>Motivo</th><th>Fecha</th><th class=\"right\">Total Gs.</th></tr></thead>");
                sb.AppendLine("<tbody>");
                foreach (var nc in ncs)
                {
                    sb.AppendLine($"<tr>");
                    sb.AppendLine($"<td>{nc.IdNotaCreditoCompra}</td>");
                    sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(nc.NumeroNota ?? "-")}</td>");
                    sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(nc.Proveedor?.RazonSocial ?? "-")}</td>");
                    sb.AppendLine($"<td>{nc.Proveedor?.RUC ?? "-"}</td>");
                    sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(nc.Motivo ?? "-")}</td>");
                    sb.AppendLine($"<td>{nc.Fecha:dd/MM/yyyy}</td>");
                    sb.AppendLine($"<td class=\"right\">{nc.Total:N0}</td>");
                    sb.AppendLine($"</tr>");
                }
                sb.AppendLine("</tbody>");
                sb.AppendLine($"<tfoot><tr><th colspan=\"6\" class=\"right\">TOTAL</th><th class=\"right\">{ncs.Sum(nc => nc.Total):N0}</th></tr></tfoot>");
                sb.AppendLine("</table>");
            }
            else
            {
                sb.AppendLine("<p>No hay notas de crédito de compras en el período seleccionado.</p>");
            }

            sb.AppendLine(GenerarPieHtml());
            return sb.ToString();
        }

        private async Task<string> GenerarHtmlProductosValorizadoAsync(AppDbContext ctx, int sucursalId, string empresa, string sucursal)
        {
            var sucursalInfo = await ctx.Sucursal.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sucursalId);
            var ruc = sucursalInfo != null ? $"{sucursalInfo.RUC}-{sucursalInfo.DV}" : null;
            var logoBase64 = sucursalInfo?.Logo != null ? Convert.ToBase64String(sucursalInfo.Logo) : null;

            var productos = await ctx.Productos
                .AsNoTracking()
                .Where(p => p.IdSucursal == sucursalId && p.Activo)
                .Include(p => p.Clasificacion)
                .OrderBy(p => p.Clasificacion != null ? p.Clasificacion.Nombre : "")
                .ThenBy(p => p.Descripcion)
                .ToListAsync();

            var productosConStock = productos.Where(p => p.Stock > 0).ToList();
            var totalValorizado = productosConStock.Sum(p => p.Stock * (p.CostoUnitarioGs ?? 0));

            var sb = new StringBuilder();
            sb.AppendLine(GenerarEncabezadoHtml("Stock Valorizado", empresa, sucursal, DateTime.Today, DateTime.Today, ruc, logoBase64));
            
            sb.AppendLine(GenerarResumenBox("Resumen",
                ("Total Productos:", productos.Count.ToString(), false),
                ("Con Stock:", productosConStock.Count.ToString(), false),
                ("Valor Total Stock:", $"Gs. {totalValorizado:N0}", true)
            ));

            if (productosConStock.Any())
            {
                sb.AppendLine("<table class=\"tabla\">");
                sb.AppendLine("<thead><tr><th>Código</th><th>Producto</th><th>Clasificación</th><th class=\"right\">Stock</th><th class=\"right\">P. Compra</th><th class=\"right\">Valorizado</th></tr></thead>");
                sb.AppendLine("<tbody>");
                foreach (var p in productosConStock)
                {
                    var valorizado = p.Stock * (p.CostoUnitarioGs ?? 0);
                    sb.AppendLine($"<tr>");
                    sb.AppendLine($"<td>{p.CodigoBarras ?? p.CodigoInterno ?? "-"}</td>");
                    sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(p.Descripcion)}</td>");
                    sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(p.Clasificacion?.Nombre ?? "-")}</td>");
                    sb.AppendLine($"<td class=\"right\">{p.Stock:N2}</td>");
                    sb.AppendLine($"<td class=\"right\">{p.CostoUnitarioGs:N0}</td>");
                    sb.AppendLine($"<td class=\"right\">{valorizado:N0}</td>");
                    sb.AppendLine($"</tr>");
                }
                sb.AppendLine("</tbody>");
                sb.AppendLine($"<tfoot><tr><th colspan=\"5\" class=\"right\">TOTAL VALORIZADO</th><th class=\"right\">{totalValorizado:N0}</th></tr></tfoot>");
                sb.AppendLine("</table>");
            }

            sb.AppendLine(GenerarPieHtml());
            return sb.ToString();
        }

        private string GenerarHtmlGenerico(TipoInformeEnum tipo, string empresa, string sucursal, DateTime desde, DateTime hasta)
        {
            var sb = new StringBuilder();
            sb.AppendLine(GenerarEncabezadoHtml(ObtenerNombreInforme(tipo), empresa, sucursal, desde, hasta));
            sb.AppendLine($"<p>Este informe ({tipo}) está en desarrollo.</p>");
            sb.AppendLine(GenerarPieHtml());
            return sb.ToString();
        }

        // ========== HELPERS HTML - FORMATO IDÉNTICO AL SISTEMA ==========

        /// <summary>
        /// Genera el encabezado HTML con el formato EXACTO del sistema (el mismo que usa el botón Imprimir)
        /// </summary>
        private string GenerarEncabezadoHtml(string titulo, string empresa, string sucursal, DateTime? desde, DateTime? hasta, string? ruc = null, string? logoBase64 = null, string? filtrosExtra = null)
        {
            var fechaStr = desde.HasValue
                ? (hasta.HasValue && hasta != desde ? $"Desde: {desde:dd/MM/yyyy} &nbsp; Hasta: {hasta:dd/MM/yyyy}" : $"Fecha: {desde:dd/MM/yyyy}")
                : $"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}";

            var logoHtml = !string.IsNullOrEmpty(logoBase64)
                ? $"<img class=\"logo\" src=\"data:image/png;base64,{logoBase64}\" alt=\"Logo\"/>"
                : "<div class=\"logo d-flex align-items-center justify-content-center\" style=\"font-size:10px;color:#999;min-height:40px;min-width:100px;\">LOGO</div>";

            var rucHtml = !string.IsNullOrEmpty(ruc) ? $"<div class=\"small\">RUC: {ruc}</div>" : "";
            var filtrosHtml = !string.IsNullOrEmpty(filtrosExtra) 
                ? $"<div class=\"small muted\">{System.Net.WebUtility.HtmlEncode(filtrosExtra)}</div>" 
                : "";

            // CSS EXACTO del sistema - copiado de InformeVentasDetallado.razor y InformeProductosValorizado.razor
            return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8""/>
    <title>{titulo}</title>
    <style>
        /* ========== ESTILOS EXACTOS DEL SISTEMA (copiar de Imprimir()) ========== */
        @media print {{ @page {{ size: A4 landscape; margin: 10mm 10mm; }} }}
        
        body {{ font-family: 'Segoe UI', Arial, sans-serif; font-size: 10px; color: #111; margin: 0; padding: 15px; }}
        
        /* Encabezado - EXACTO del sistema */
        .factura-header {{
            display: flex;
            align-items: center;
            gap: 16px;
            border-bottom: 2px solid #222;
            padding-bottom: 8px;
            margin-bottom: 12px;
        }}
        .factura-header .logo {{
            max-width: 120px;
            max-height: 60px;
            object-fit: contain;
            display: block;
            border-radius: 6px;
            border: 1px solid #e5e7eb;
            background: #fff;
            padding: 6px;
        }}
        .factura-header .logo-wrap {{
            width: 140px;
            display: flex;
            align-items: center;
            justify-content: center;
        }}
        .factura-header .empresa h2 {{ margin: 0; font-size: 18px; }}
        .factura-header .empresa .small {{ color: #555; font-size: 10px; }}
        
        /* Resumen - EXACTO del sistema (estilo InformeProductosValorizado) */
        .resumen {{
            display: flex;
            gap: 20px;
            margin-bottom: 15px;
            padding: 10px;
            background: #f8f9fa;
            border-radius: 8px;
        }}
        .resumen-item {{
            flex: 1;
            text-align: center;
            padding: 8px;
            background: white;
            border-radius: 6px;
            border: 1px solid #dee2e6;
        }}
        .resumen-item .label {{
            font-size: 9px;
            color: #666;
            text-transform: uppercase;
            display: block;
        }}
        .resumen-item .value {{
            font-size: 14px;
            font-weight: bold;
            display: block;
        }}
        
        /* Tablas - EXACTO del sistema */
        .table {{ width: 100%; border-collapse: collapse; margin-bottom: 1rem; }}
        .table-sm th, .table-sm td {{ padding: .25rem; }}
        .tabla th, .tabla td {{
            padding: .25rem;
            border-bottom: 1px solid #e5e7eb;
            vertical-align: top;
        }}
        .tabla thead th {{ background: #f8f9fa; font-weight: 600; }}
        .tabla tfoot {{ font-weight: bold; }}
        .tabla tfoot th {{ border-top: 2px solid #dee2e6; }}
        
        /* Utilidades Bootstrap */
        .right {{ text-align: right; }}
        .text-end {{ text-align: right; }}
        .muted {{ color: #666; }}
        .fw-bold {{ font-weight: 700; }}
        .ms-auto {{ margin-left: auto; }}
        .d-flex {{ display: flex; }}
        .align-items-center {{ align-items: center; }}
        .justify-content-center {{ justify-content: center; }}
        .container-fluid {{ width: 100%; padding-right: 15px; padding-left: 15px; }}
        .text-success {{ color: #198754; }}
        .text-danger {{ color: #dc3545; }}
        .text-muted {{ color: #6c757d; }}
        .small {{ font-size: 85%; }}
        
        /* Pie de página */
        .footer {{
            text-align: center;
            margin-top: 20px;
            padding-top: 10px;
            border-top: 1px solid #ddd;
            color: #888;
            font-size: 9px;
        }}
    </style>
</head>
<body>
<div class=""container-fluid"">
    <!-- Encabezado EXACTO del sistema -->
    <div class=""factura-header"">
        <div class=""logo-wrap"">
            {logoHtml}
        </div>
        <div class=""empresa"">
            <h2>{System.Net.WebUtility.HtmlEncode(empresa)}</h2>
            {rucHtml}
            <div class=""small"">{System.Net.WebUtility.HtmlEncode(sucursal)}</div>
        </div>
        <div class=""ms-auto text-end"">
            <div class=""fw-bold"" style=""font-size:18px"">{System.Net.WebUtility.HtmlEncode(titulo)}</div>
            <div>{fechaStr}</div>
            {filtrosHtml}
        </div>
    </div>
";
        }

        private string GenerarPieHtml()
        {
            return $@"
    <!-- Pie de página -->
    <div class=""footer"">
        <p>Generado: {DateTime.Now:dd/MM/yyyy HH:mm}</p>
    </div>
</div>
</body>
</html>";
        }

        /// <summary>
        /// Genera resumen con formato EXACTO del sistema (estilo flex como InformeProductosValorizado)
        /// </summary>
        private string GenerarResumen(params (string Label, string Valor, string? Clase)[] items)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<div class=\"resumen\">");
            foreach (var (label, valor, clase) in items)
            {
                var claseValor = !string.IsNullOrEmpty(clase) ? $" {clase}" : "";
                sb.AppendLine($"<div class=\"resumen-item\"><div class=\"label\">{System.Net.WebUtility.HtmlEncode(label)}</div><div class=\"value{claseValor}\">{System.Net.WebUtility.HtmlEncode(valor)}</div></div>");
            }
            sb.AppendLine("</div>");
            return sb.ToString();
        }

        /// <summary>
        /// Genera un cuadro de resumen con formato alternativo (para compatibilidad)
        /// </summary>
        private string GenerarResumenBox(string titulo, params (string Label, string Valor, bool EsTotal)[] items)
        {
            // Usar el nuevo formato flex del sistema
            var itemsList = items.Select(i => (i.Label.TrimEnd(':'), i.Valor, i.EsTotal ? "text-success" : (string?)null)).ToArray();
            return GenerarResumen(itemsList);
        }
    }
}
