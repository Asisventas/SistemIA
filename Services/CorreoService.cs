using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SistemIA.Models;

namespace SistemIA.Services
{
    /// <summary>
    /// Servicio centralizado para envío de correos electrónicos.
    /// Maneja envío de informes de cierre, facturas a clientes, alertas, etc.
    /// </summary>
    public interface ICorreoService
    {
        /// <summary>
        /// Envía correo de resumen de cierre de caja a los destinatarios configurados
        /// </summary>
        Task<(bool Exito, string Mensaje)> EnviarResumenCierreCajaAsync(CierreCaja cierre, List<EntregaCaja> entregas, int sucursalId);

        /// <summary>
        /// Envía factura PDF a cliente por correo
        /// </summary>
        Task<(bool Exito, string Mensaje)> EnviarFacturaClienteAsync(int idVenta, string correoDestino, byte[] pdfBytes, string nombreArchivo);

        /// <summary>
        /// Envía un correo genérico
        /// </summary>
        Task<(bool Exito, string Mensaje)> EnviarCorreoAsync(string destinatario, string asunto, string cuerpoHtml, List<Attachment>? adjuntos = null);

        /// <summary>
        /// Envía un correo usando una configuración específica con adjuntos como bytes
        /// </summary>
        Task<(bool Exito, string Mensaje)> EnviarCorreoAsync(ConfiguracionCorreo config, string destinatario, string asunto, string cuerpoHtml, List<(string nombre, byte[] contenido, string mimeType)>? adjuntosBytes = null);

        /// <summary>
        /// Verifica si el servicio de correo está configurado y activo
        /// </summary>
        Task<bool> EstaConfiguradoAsync(int sucursalId);
    }

    public class CorreoService : ICorreoService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        private readonly ILogger<CorreoService> _logger;

        public CorreoService(IDbContextFactory<AppDbContext> dbFactory, ILogger<CorreoService> logger)
        {
            _dbFactory = dbFactory;
            _logger = logger;
        }

        public async Task<bool> EstaConfiguradoAsync(int sucursalId)
        {
            await using var ctx = await _dbFactory.CreateDbContextAsync();
            var config = await ctx.ConfiguracionesCorreo
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Activo && c.IdSucursal == sucursalId);
            
            return config?.ConfiguracionCompleta ?? false;
        }

        public async Task<(bool Exito, string Mensaje)> EnviarResumenCierreCajaAsync(CierreCaja cierre, List<EntregaCaja> entregas, int sucursalId)
        {
            try
            {
                await using var ctx = await _dbFactory.CreateDbContextAsync();
                
                // Obtener configuración de correo
                var config = await ctx.ConfiguracionesCorreo
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Activo && c.IdSucursal == sucursalId);

                if (config == null || !config.ConfiguracionCompleta)
                {
                    _logger.LogWarning("No hay configuración de correo activa para sucursal {SucursalId}", sucursalId);
                    return (false, "No hay configuración de correo activa");
                }

                // Obtener destinatarios que tienen activado el envío al cierre de caja
                // Buscar por IdConfiguracionCorreo o por IdSucursal si no tienen config asignada
                var destinatarios = await ctx.DestinatariosInforme
                    .AsNoTracking()
                    .Where(d => d.Activo && d.RecibeCierreCaja 
                             && (d.IdConfiguracionCorreo == config.IdConfiguracionCorreo 
                                 || (d.IdConfiguracionCorreo == null && d.IdSucursal == sucursalId)))
                    .ToListAsync();

                if (!destinatarios.Any())
                {
                    _logger.LogInformation("No hay destinatarios con envío al cierre de caja activado");
                    return (false, "No hay destinatarios con envío al cierre activado");
                }

                // Obtener información de la sucursal
                var sucursal = await ctx.Sucursal.AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == sucursalId);

                // Obtener movimientos de inventario del turno/fecha/caja
                var movimientos = await ctx.MovimientosInventario
                    .AsNoTracking()
                    .Include(m => m.Producto)
                    .Where(m => m.FechaCaja == cierre.FechaCaja 
                             && m.Turno == cierre.Turno 
                             && m.IdCaja == cierre.IdCaja
                             && m.IdSucursal == sucursalId)
                    .OrderBy(m => m.Fecha)
                    .ToListAsync();

                // Obtener stocks actuales de los productos movidos
                var idsProductos = movimientos.Select(m => m.IdProducto).Distinct().ToList();
                var stocksPorProducto = await ctx.ProductosDepositos
                    .AsNoTracking()
                    .Include(pd => pd.Deposito)
                    .Where(pd => idsProductos.Contains(pd.IdProducto))
                    .ToListAsync();

                // Construir el correo con movimientos y stocks
                var asunto = $"Cierre de Caja #{cierre.IdCierreCaja} - {sucursal?.NombreSucursal ?? "Sucursal"} - {cierre.FechaCaja:dd/MM/yyyy}";
                var cuerpoHtml = GenerarHtmlCierreCaja(cierre, entregas, sucursal, movimientos, stocksPorProducto);

                // Enviar a cada destinatario
                var errores = new List<string>();
                var exitosos = 0;

                foreach (var dest in destinatarios)
                {
                    var resultado = await EnviarCorreoInternoAsync(config, dest.Correo, asunto, cuerpoHtml);
                    if (resultado.Exito)
                    {
                        exitosos++;
                        _logger.LogInformation("Cierre de caja enviado a {Correo}", dest.Correo);
                    }
                    else
                    {
                        errores.Add($"{dest.Nombre}: {resultado.Mensaje}");
                        _logger.LogWarning("Error enviando cierre a {Correo}: {Error}", dest.Correo, resultado.Mensaje);
                    }
                }

                // Registrar envío en auditoría (opcional)
                if (exitosos > 0)
                {
                    return (true, $"Cierre enviado a {exitosos} de {destinatarios.Count} destinatarios");
                }

                return (false, $"No se pudo enviar a ningún destinatario. Errores: {string.Join("; ", errores)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar resumen de cierre de caja");
                return (false, $"Error: {ex.Message}");
            }
        }

        public async Task<(bool Exito, string Mensaje)> EnviarFacturaClienteAsync(int idVenta, string correoDestino, byte[] pdfBytes, string nombreArchivo)
        {
            try
            {
                await using var ctx = await _dbFactory.CreateDbContextAsync();

                // Obtener la venta con sucursal
                var venta = await ctx.Ventas
                    .Include(v => v.Cliente)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(v => v.IdVenta == idVenta);

                if (venta == null)
                    return (false, "Venta no encontrada");

                var sucursalId = venta.IdSucursal;

                // Obtener configuración de correo
                var config = await ctx.ConfiguracionesCorreo
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Activo && c.IdSucursal == sucursalId);

                if (config == null || !config.ConfiguracionCompleta)
                    return (false, "No hay configuración de correo activa");

                // Obtener sucursal
                var sucursal = await ctx.Sucursal.AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == sucursalId);

                var numeroFactura = $"{venta.Establecimiento}-{venta.PuntoExpedicion}-{venta.NumeroFactura}";
                var asunto = $"Factura Electrónica {numeroFactura} - {sucursal?.NombreEmpresa ?? ""}";

                var cuerpoHtml = GenerarHtmlFacturaCliente(venta, sucursal, numeroFactura);

                // Crear adjunto
                var adjuntos = new List<Attachment>
                {
                    new Attachment(new MemoryStream(pdfBytes), nombreArchivo, "application/pdf")
                };

                var resultado = await EnviarCorreoInternoAsync(config, correoDestino, asunto, cuerpoHtml, adjuntos);
                
                // Limpiar adjuntos
                foreach (var adj in adjuntos)
                    adj.Dispose();

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar factura por correo");
                return (false, $"Error: {ex.Message}");
            }
        }

        public async Task<(bool Exito, string Mensaje)> EnviarCorreoAsync(string destinatario, string asunto, string cuerpoHtml, List<Attachment>? adjuntos = null)
        {
            try
            {
                await using var ctx = await _dbFactory.CreateDbContextAsync();

                // Buscar primera configuración activa
                var config = await ctx.ConfiguracionesCorreo
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Activo);

                if (config == null || !config.ConfiguracionCompleta)
                    return (false, "No hay configuración de correo activa");

                return await EnviarCorreoInternoAsync(config, destinatario, asunto, cuerpoHtml, adjuntos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo");
                return (false, $"Error: {ex.Message}");
            }
        }

        public async Task<(bool Exito, string Mensaje)> EnviarCorreoAsync(
            ConfiguracionCorreo config, 
            string destinatario, 
            string asunto, 
            string cuerpoHtml, 
            List<(string nombre, byte[] contenido, string mimeType)>? adjuntosBytes = null)
        {
            try
            {
                List<Attachment>? adjuntos = null;
                
                if (adjuntosBytes != null && adjuntosBytes.Count > 0)
                {
                    adjuntos = adjuntosBytes.Select(a => 
                        new Attachment(new MemoryStream(a.contenido), a.nombre, a.mimeType))
                        .ToList();
                }

                var resultado = await EnviarCorreoInternoAsync(config, destinatario, asunto, cuerpoHtml, adjuntos);

                // Limpiar recursos de adjuntos
                if (adjuntos != null)
                {
                    foreach (var adj in adjuntos)
                        adj.Dispose();
                }

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo con configuración específica");
                return (false, $"Error: {ex.Message}");
            }
        }

        private async Task<(bool Exito, string Mensaje)> EnviarCorreoInternoAsync(
            ConfiguracionCorreo config, 
            string destinatario, 
            string asunto, 
            string cuerpoHtml, 
            List<Attachment>? adjuntos = null)
        {
            try
            {
                using var client = new SmtpClient(config.ServidorSmtp, config.Puerto);
                client.EnableSsl = config.TipoSeguridad != "None";
                client.Timeout = (config.TimeoutSegundos > 0 ? config.TimeoutSegundos : 30) * 1000;

                // Configurar credenciales
                if (config.TipoProveedor == "SendGrid" && !string.IsNullOrEmpty(config.ApiKey))
                {
                    client.Credentials = new NetworkCredential("apikey", config.ApiKey);
                }
                else if (!string.IsNullOrEmpty(config.UsuarioSmtp))
                {
                    client.Credentials = new NetworkCredential(config.UsuarioSmtp, config.ContrasenaSmtp ?? "");
                }

                // Crear mensaje
                using var mail = new MailMessage();
                mail.From = new MailAddress(config.CorreoRemitenteEfectivo, config.NombreRemitenteEfectivo);
                mail.To.Add(destinatario);
                mail.Subject = asunto;
                mail.Body = cuerpoHtml;
                mail.IsBodyHtml = true;

                // Reply-To
                if (!string.IsNullOrEmpty(config.CorreoReplyTo))
                    mail.ReplyToList.Add(config.CorreoReplyTo);

                // BCC de auditoría
                if (!string.IsNullOrEmpty(config.CorreoBccAuditoria))
                    mail.Bcc.Add(config.CorreoBccAuditoria);

                // Agregar adjuntos
                if (adjuntos != null)
                {
                    foreach (var adj in adjuntos)
                        mail.Attachments.Add(adj);
                }

                // Agregar firma si existe
                if (!string.IsNullOrEmpty(config.FirmaHtml))
                {
                    mail.Body += $"<br/><br/>{config.FirmaHtml}";
                }

                await client.SendMailAsync(mail);
                
                _logger.LogInformation("Correo enviado exitosamente a {Destinatario}: {Asunto}", destinatario, asunto);
                return (true, "Correo enviado correctamente");
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, "Error SMTP al enviar correo a {Destinatario}", destinatario);
                return (false, $"Error SMTP: {smtpEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo a {Destinatario}", destinatario);
                return (false, $"Error: {ex.Message}");
            }
        }

        private string GenerarHtmlCierreCaja(CierreCaja cierre, List<EntregaCaja> entregas, Sucursal? sucursal, List<MovimientoInventario> movimientos, List<ProductoDeposito> stocksActuales)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head><meta charset='utf-8'/>");
            sb.AppendLine("<style>");
            sb.AppendLine("body{font-family:Arial,sans-serif;font-size:14px;color:#333;max-width:700px;margin:0 auto;padding:20px;}");
            sb.AppendLine(".header{background:#1a5f7a;color:white;padding:20px;border-radius:8px 8px 0 0;text-align:center;}");
            sb.AppendLine(".content{background:#f8f9fa;padding:20px;border:1px solid #ddd;border-radius:0 0 8px 8px;}");
            sb.AppendLine(".info-box{background:white;padding:15px;border-radius:5px;margin-bottom:15px;border:1px solid #e0e0e0;}");
            sb.AppendLine(".row{display:flex;justify-content:space-between;margin-bottom:8px;}");
            sb.AppendLine(".label{color:#666;} .value{font-weight:bold;}");
            sb.AppendLine("table{width:100%;border-collapse:collapse;margin-top:10px;}");
            sb.AppendLine("th,td{padding:10px;text-align:left;border-bottom:1px solid #ddd;}");
            sb.AppendLine("th{background:#e9ecef;font-weight:bold;}");
            sb.AppendLine(".right{text-align:right}");
            sb.AppendLine(".center{text-align:center}");
            sb.AppendLine(".total-row{background:#e9ecef;font-weight:bold;}");
            sb.AppendLine(".diferencia-neg{color:#dc3545;} .diferencia-pos{color:#28a745;}");
            sb.AppendLine(".entrada{color:#28a745;} .salida{color:#dc3545;}");
            sb.AppendLine(".badge{display:inline-block;padding:3px 8px;border-radius:4px;font-size:11px;font-weight:bold;}");
            sb.AppendLine(".badge-entrada{background:#d4edda;color:#155724;}");
            sb.AppendLine(".badge-salida{background:#f8d7da;color:#721c24;}");
            sb.AppendLine(".footer{text-align:center;margin-top:20px;color:#999;font-size:12px;}");
            sb.AppendLine("</style></head><body>");

            // Encabezado
            sb.AppendLine("<div class='header'>");
            sb.AppendLine($"<h2 style='margin:0;'>📊 Cierre de Caja #{cierre.IdCierreCaja}</h2>");
            sb.AppendLine($"<div style='margin-top:10px;opacity:0.9;'>{sucursal?.NombreEmpresa ?? "Empresa"}</div>");
            sb.AppendLine("</div>");

            sb.AppendLine("<div class='content'>");

            // Información del cierre
            sb.AppendLine("<div class='info-box'>");
            sb.AppendLine("<h4 style='margin-top:0;color:#1a5f7a;'>📋 Información del Cierre</h4>");
            sb.AppendLine($"<div class='row'><span class='label'>Sucursal:</span><span class='value'>{sucursal?.NombreSucursal ?? "N/A"}</span></div>");
            sb.AppendLine($"<div class='row'><span class='label'>Caja:</span><span class='value'>#{cierre.IdCaja}</span></div>");
            sb.AppendLine($"<div class='row'><span class='label'>Turno:</span><span class='value'>{cierre.Turno}</span></div>");
            sb.AppendLine($"<div class='row'><span class='label'>Fecha de Caja:</span><span class='value'>{cierre.FechaCaja:dd/MM/yyyy}</span></div>");
            sb.AppendLine($"<div class='row'><span class='label'>Fecha/Hora Cierre:</span><span class='value'>{cierre.FechaCierre:dd/MM/yyyy HH:mm}</span></div>");
            sb.AppendLine($"<div class='row'><span class='label'>Usuario:</span><span class='value'>{cierre.UsuarioCierre}</span></div>");
            sb.AppendLine("</div>");

            // Resumen de operaciones
            sb.AppendLine("<div class='info-box'>");
            sb.AppendLine("<h4 style='margin-top:0;color:#1a5f7a;'>💰 Resumen de Operaciones</h4>");
            sb.AppendLine("<table>");
            sb.AppendLine("<tr><td>Ventas Contado</td><td class='right'>" + cierre.TotalVentasContado.ToString("N0") + " ₲</td></tr>");
            sb.AppendLine("<tr><td>Ventas Crédito</td><td class='right'>" + cierre.TotalVentasCredito.ToString("N0") + " ₲</td></tr>");
            sb.AppendLine("<tr><td>Cobros Crédito</td><td class='right'>" + cierre.TotalCobrosCredito.ToString("N0") + " ₲</td></tr>");
            sb.AppendLine("<tr><td>Anulaciones</td><td class='right' style='color:#dc3545;'>" + cierre.TotalAnulaciones.ToString("N0") + " ₲</td></tr>");
            if (cierre.CantNotasCredito > 0)
                sb.AppendLine($"<tr><td>NC Ventas ({cierre.CantNotasCredito})</td><td class='right' style='color:#ffc107;'>-{cierre.TotalNotasCredito:N0} ₲</td></tr>");
            if (cierre.CantComprasEfectivo > 0)
                sb.AppendLine($"<tr><td>Compras Efectivo ({cierre.CantComprasEfectivo})</td><td class='right' style='color:#dc3545;'>-{cierre.TotalComprasEfectivo:N0} ₲</td></tr>");
            if (cierre.CantNotasCreditoCompras > 0)
                sb.AppendLine($"<tr><td>NC Compras ({cierre.CantNotasCreditoCompras})</td><td class='right' style='color:#198754;'>+{cierre.TotalNotasCreditoCompras:N0} ₲</td></tr>");
            // Neto en Caja (si hay operaciones que lo afectan)
            if (cierre.CantNotasCredito > 0 || cierre.CantComprasEfectivo > 0 || cierre.CantNotasCreditoCompras > 0)
            {
                var netoEnCaja = cierre.TotalVentasContado + cierre.TotalCobrosCredito - cierre.TotalNotasCredito - cierre.TotalComprasEfectivo + cierre.TotalNotasCreditoCompras;
                sb.AppendLine($"<tr style='border-top:2px solid #333;'><td><strong>NETO EN CAJA</strong></td><td class='right'><strong>{netoEnCaja:N0} ₲</strong></td></tr>");
            }
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");

            // Entregas por medio de pago
            sb.AppendLine("<div class='info-box'>");
            sb.AppendLine("<h4 style='margin-top:0;color:#1a5f7a;'>💳 Entregas por Medio de Pago</h4>");
            sb.AppendLine("<table>");
            sb.AppendLine("<thead><tr><th>Medio</th><th class='right'>Esperado</th><th class='right'>Entregado</th><th class='right'>Diferencia</th></tr></thead>");
            sb.AppendLine("<tbody>");
            foreach (var e in entregas)
            {
                var difClass = e.Diferencia < 0 ? "diferencia-neg" : e.Diferencia > 0 ? "diferencia-pos" : "";
                sb.AppendLine($"<tr><td>{e.Medio}</td><td class='right'>{e.MontoEsperado:N0}</td><td class='right'>{e.MontoEntregado:N0}</td><td class='right {difClass}'>{e.Diferencia:N0}</td></tr>");
            }
            sb.AppendLine("</tbody>");
            
            var difTotalClass = cierre.Diferencia < 0 ? "diferencia-neg" : cierre.Diferencia > 0 ? "diferencia-pos" : "";
            sb.AppendLine($"<tfoot><tr class='total-row'><td><strong>TOTAL</strong></td><td class='right'><strong>{cierre.TotalEsperado:N0}</strong></td><td class='right'><strong>{cierre.TotalEntregado:N0}</strong></td><td class='right {difTotalClass}'><strong>{cierre.Diferencia:N0}</strong></td></tr></tfoot>");
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");

            // ========== MOVIMIENTOS DE INVENTARIO ==========
            if (movimientos.Any())
            {
                sb.AppendLine("<div class='info-box'>");
                sb.AppendLine("<h4 style='margin-top:0;color:#1a5f7a;'>📦 Movimientos de Inventario</h4>");
                
                // Agrupar por categoría de motivo (no por motivo exacto)
                var gruposPorCategoria = movimientos
                    .GroupBy(m => ObtenerCategoriaMotivo(m.Motivo ?? "Otros"))
                    .OrderBy(g => g.Key)
                    .ToList();

                sb.AppendLine("<table style='font-size:13px;'>");
                sb.AppendLine("<thead><tr><th>Motivo</th><th class='center'>Entradas</th><th class='center'>Salidas</th><th class='right'>Costo Neto</th></tr></thead>");
                sb.AppendLine("<tbody>");

                decimal totalCostoNeto = 0;
                int totalEntradas = 0;
                int totalSalidas = 0;

                foreach (var grupo in gruposPorCategoria)
                {
                    var entradas = grupo.Where(m => m.Tipo == 1).Sum(m => m.Cantidad);
                    var salidas = grupo.Where(m => m.Tipo == 2).Sum(m => m.Cantidad);
                    var costoEntradas = grupo.Where(m => m.Tipo == 1).Sum(m => m.Cantidad * (m.PrecioCostoGs ?? 0));
                    var costoSalidas = grupo.Where(m => m.Tipo == 2).Sum(m => m.Cantidad * (m.PrecioCostoGs ?? 0));
                    var costoNeto = costoEntradas - costoSalidas;

                    totalEntradas += (int)entradas;
                    totalSalidas += (int)salidas;
                    totalCostoNeto += costoNeto;

                    var motivoIcono = ObtenerIconoMotivo(grupo.Key);
                    sb.AppendLine($"<tr>");
                    sb.AppendLine($"<td>{motivoIcono} {grupo.Key}</td>");
                    sb.AppendLine($"<td class='center entrada'>{(entradas > 0 ? $"+{entradas:N0}" : "-")}</td>");
                    sb.AppendLine($"<td class='center salida'>{(salidas > 0 ? $"-{salidas:N0}" : "-")}</td>");
                    sb.AppendLine($"<td class='right' style='color:{(costoNeto >= 0 ? "#28a745" : "#dc3545")};'>{costoNeto:N0} ₲</td>");
                    sb.AppendLine($"</tr>");
                }

                sb.AppendLine("</tbody>");
                sb.AppendLine($"<tfoot><tr class='total-row'>");
                sb.AppendLine($"<td><strong>TOTAL ({movimientos.Count} mov.)</strong></td>");
                sb.AppendLine($"<td class='center entrada'><strong>+{totalEntradas:N0}</strong></td>");
                sb.AppendLine($"<td class='center salida'><strong>-{totalSalidas:N0}</strong></td>");
                sb.AppendLine($"<td class='right' style='color:{(totalCostoNeto >= 0 ? "#28a745" : "#dc3545")};'><strong>{totalCostoNeto:N0} ₲</strong></td>");
                sb.AppendLine($"</tr></tfoot>");
                sb.AppendLine("</table>");

                // Detalle por producto (máximo 10 productos más movidos) con stock actual
                var productosMasMovidos = movimientos
                    .GroupBy(m => new { m.IdProducto, Descripcion = m.Producto?.Descripcion ?? $"Producto #{m.IdProducto}" })
                    .Select(g => new 
                    {
                        g.Key.IdProducto,
                        g.Key.Descripcion,
                        Entradas = g.Where(m => m.Tipo == 1).Sum(m => m.Cantidad),
                        Salidas = g.Where(m => m.Tipo == 2).Sum(m => m.Cantidad),
                        TotalMov = g.Count()
                    })
                    .OrderByDescending(x => x.TotalMov)
                    .Take(10)
                    .ToList();

                if (productosMasMovidos.Any())
                {
                    sb.AppendLine("<details style='margin-top:15px;' open>");
                    sb.AppendLine("<summary style='cursor:pointer;color:#1a5f7a;font-weight:bold;'>📊 Top 10 Productos con más movimientos (stock inicio vs actual)</summary>");
                    sb.AppendLine("<table style='font-size:12px;margin-top:10px;'>");
                    sb.AppendLine("<thead><tr><th>Producto</th><th class='center' style='background:#6c757d;color:white;'>Inicio</th><th class='center'>Entradas</th><th class='center'>Salidas</th><th class='center' style='background:#28a745;color:white;'>Actual</th><th class='center' style='background:#17a2b8;color:white;'>Almac.</th></tr></thead>");
                    sb.AppendLine("<tbody>");
                    foreach (var p in productosMasMovidos)
                    {
                        var nombreCorto = p.Descripcion.Length > 30 ? p.Descripcion.Substring(0, 27) + "..." : p.Descripcion;
                        
                        // Buscar stocks actuales: Principal (IdDeposito=1) y Almacén (IdDeposito=2)
                        var stockPrincipal = stocksActuales.FirstOrDefault(s => s.IdProducto == p.IdProducto && s.IdDeposito == 1)?.Stock ?? 0;
                        var stockAlmacen = stocksActuales.FirstOrDefault(s => s.IdProducto == p.IdProducto && s.IdDeposito == 2)?.Stock ?? 0;
                        
                        // Calcular stock inicial = actual + salidas - entradas (invertir movimientos)
                        var stockInicial = stockPrincipal + p.Salidas - p.Entradas;
                        
                        sb.AppendLine($"<tr><td title='{p.Descripcion}'>{nombreCorto}</td>");
                        sb.AppendLine($"<td class='center' style='color:#6c757d;font-weight:bold;'>{stockInicial:N0}</td>");
                        sb.AppendLine($"<td class='center entrada'>{(p.Entradas > 0 ? $"+{p.Entradas:N0}" : "-")}</td>");
                        sb.AppendLine($"<td class='center salida'>{(p.Salidas > 0 ? $"-{p.Salidas:N0}" : "-")}</td>");
                        sb.AppendLine($"<td class='center' style='font-weight:bold;color:{(stockPrincipal <= 0 ? "#dc3545" : "#28a745")};'>{stockPrincipal:N0}</td>");
                        sb.AppendLine($"<td class='center' style='color:#17a2b8;'>{stockAlmacen:N0}</td></tr>");
                    }
                    sb.AppendLine("</tbody></table>");
                    sb.AppendLine("<div style='font-size:10px;color:#666;margin-top:5px;'>Inicio = Stock al iniciar turno (Depósito Principal) | Actual = Stock actual (Principal) | Almac. = Almacén</div>");
                    sb.AppendLine("</details>");
                }

                sb.AppendLine("</div>");
            }
            else
            {
                // Sin movimientos
                sb.AppendLine("<div class='info-box' style='background:#f0f0f0;'>");
                sb.AppendLine("<p style='margin:0;text-align:center;color:#666;'>📦 No hubo movimientos de inventario en este turno</p>");
                sb.AppendLine("</div>");
            }

            // Observaciones
            if (!string.IsNullOrEmpty(cierre.Observaciones))
            {
                sb.AppendLine("<div class='info-box' style='background:#fff3cd;border-color:#ffc107;'>");
                sb.AppendLine("<h4 style='margin-top:0;color:#856404;'>📝 Observaciones</h4>");
                sb.AppendLine($"<p style='margin:0;'>{cierre.Observaciones}</p>");
                sb.AppendLine("</div>");
            }

            sb.AppendLine("</div>"); // content

            // Footer
            sb.AppendLine("<div class='footer'>");
            sb.AppendLine($"<p>Este correo fue generado automáticamente por SistemIA el {DateTime.Now:dd/MM/yyyy HH:mm}</p>");
            sb.AppendLine("</div>");

            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        /// <summary>
        /// Obtiene un icono emoji según el motivo del movimiento
        /// </summary>
        private string ObtenerIconoMotivo(string motivo)
        {
            var motivoLower = motivo.ToLower();
            
            // NC primero (antes de venta/compra) para evitar conflictos
            if (motivoLower.StartsWith("nc ") || motivoLower.Contains("nota de crédito") || motivoLower.Contains("devolución"))
            {
                if (motivoLower.Contains("compra")) return "↩️📥"; // NC Compra
                return "↩️🛒"; // NC Venta
            }
            
            if (motivoLower.Contains("venta")) return "🛒";
            if (motivoLower.Contains("compra")) return "📥";
            if (motivoLower.Contains("ajuste")) return "🔧";
            if (motivoLower.Contains("transfer") || motivoLower.Contains("traslado")) return "🔄";
            if (motivoLower.Contains("merma") || motivoLower.Contains("pérdida")) return "📉";
            if (motivoLower.Contains("inicial") || motivoLower.Contains("apertura")) return "🏁";
            return "📦";
        }

        /// <summary>
        /// Obtiene la categoría del motivo para agrupación con nombres completos y claros
        /// </summary>
        private string ObtenerCategoriaMotivo(string motivo)
        {
            var motivoLower = motivo.ToLower();
            
            // NC primero (antes de venta/compra) para evitar conflictos
            if (motivoLower.StartsWith("nc ") || motivoLower.Contains("nota de crédito") || motivoLower.Contains("devolución"))
            {
                if (motivoLower.Contains("compra")) return "Nota Crédito Compras (devolución a proveedor)";
                return "Nota Crédito Ventas (devolución cliente)";
            }
            
            if (motivoLower.Contains("venta")) return "Ventas (salidas por facturación)";
            if (motivoLower.Contains("compra")) return "Compras (ingresos de mercadería)";
            if (motivoLower.Contains("ajuste")) return "Ajustes manuales";
            if (motivoLower.Contains("transfer") || motivoLower.Contains("traslado")) return "Transferencias entre depósitos";
            if (motivoLower.Contains("merma") || motivoLower.Contains("pérdida")) return "Mermas/Pérdidas";
            if (motivoLower.Contains("inicial") || motivoLower.Contains("apertura")) return "Stock Inicial";
            return "Otros movimientos";
        }

        private string GenerarHtmlFacturaCliente(Venta venta, Sucursal? sucursal, string numeroFactura)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head><meta charset='utf-8'/>");
            sb.AppendLine("<style>");
            sb.AppendLine("body{font-family:Arial,sans-serif;font-size:14px;color:#333;max-width:600px;margin:0 auto;padding:20px;}");
            sb.AppendLine(".header{background:#2c5282;color:white;padding:20px;border-radius:8px 8px 0 0;text-align:center;}");
            sb.AppendLine(".content{background:#f8f9fa;padding:20px;border:1px solid #ddd;border-radius:0 0 8px 8px;}");
            sb.AppendLine(".info-box{background:white;padding:15px;border-radius:5px;margin-bottom:15px;border:1px solid #e0e0e0;}");
            sb.AppendLine(".highlight{background:#e8f4f8;padding:15px;border-radius:5px;border-left:4px solid #2c5282;margin-bottom:15px;}");
            sb.AppendLine(".footer{text-align:center;margin-top:20px;color:#999;font-size:12px;}");
            sb.AppendLine("</style></head><body>");

            // Encabezado
            sb.AppendLine("<div class='header'>");
            sb.AppendLine($"<h2 style='margin:0;'>📄 Factura Electrónica</h2>");
            sb.AppendLine($"<div style='margin-top:10px;font-size:18px;font-weight:bold;'>{numeroFactura}</div>");
            sb.AppendLine($"<div style='margin-top:5px;opacity:0.9;'>{sucursal?.NombreEmpresa ?? "Empresa"}</div>");
            sb.AppendLine("</div>");

            sb.AppendLine("<div class='content'>");

            sb.AppendLine($"<p>Estimado/a <strong>{venta.Cliente?.RazonSocial ?? "Cliente"}</strong>,</p>");
            sb.AppendLine("<p>Adjuntamos a este correo su Factura Electrónica en formato PDF.</p>");

            // Datos de la factura
            sb.AppendLine("<div class='highlight'>");
            sb.AppendLine($"<p style='margin:5px 0;'><strong>Número:</strong> {numeroFactura}</p>");
            sb.AppendLine($"<p style='margin:5px 0;'><strong>Fecha:</strong> {venta.Fecha:dd/MM/yyyy}</p>");
            sb.AppendLine($"<p style='margin:5px 0;'><strong>Total:</strong> {venta.Total:N0} {venta.Moneda?.Simbolo ?? "₲"}</p>");
            if (!string.IsNullOrEmpty(venta.CDC))
                sb.AppendLine($"<p style='margin:5px 0;font-size:11px;word-break:break-all;'><strong>CDC:</strong> {venta.CDC}</p>");
            sb.AppendLine("</div>");

            sb.AppendLine("<p>Puede verificar la autenticidad de este documento en el portal de SIFEN del Ministerio de Hacienda.</p>");
            sb.AppendLine("<p>Gracias por su preferencia.</p>");

            sb.AppendLine("</div>"); // content

            // Footer
            sb.AppendLine("<div class='footer'>");
            sb.AppendLine($"<p>{sucursal?.NombreEmpresa ?? ""} - RUC: {sucursal?.RUC ?? ""}-{sucursal?.DV}</p>");
            if (!string.IsNullOrEmpty(sucursal?.Direccion))
                sb.AppendLine($"<p>{sucursal.Direccion}</p>");
            if (!string.IsNullOrEmpty(sucursal?.Telefono))
                sb.AppendLine($"<p>Tel: {sucursal.Telefono}</p>");
            sb.AppendLine("</div>");

            sb.AppendLine("</body></html>");
            return sb.ToString();
        }
    }
}
