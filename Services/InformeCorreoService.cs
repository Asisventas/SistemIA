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
        private readonly ILogger<InformeCorreoService> _logger;

        public InformeCorreoService(
            IDbContextFactory<AppDbContext> dbFactory,
            ICorreoService correoService,
            ILogger<InformeCorreoService> logger)
        {
            _dbFactory = dbFactory;
            _correoService = correoService;
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

        public async Task<(bool Exito, string Mensaje, int InformesEnviados)> EnviarInformesCierreAsync(int sucursalId)
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

                var informesEnviados = 0;
                var errores = new List<string>();
                var fecha = DateTime.Today;

                // Lista de informes que pueden enviarse al cierre
                var informesCierre = new[]
                {
                    (TipoInformeEnum.VentasDiarias, "RecibeVentasDiarias"),
                    (TipoInformeEnum.VentasDetallado, "RecibeVentasDetallado"),
                    (TipoInformeEnum.VentasAgrupado, "RecibeVentasAgrupado"),
                    (TipoInformeEnum.ComprasGeneral, "RecibeInformeCompras"),
                    (TipoInformeEnum.ComprasDetallado, "RecibeComprasDetallado"),
                    (TipoInformeEnum.ResumenCaja, "RecibeResumenCaja"),
                    (TipoInformeEnum.CuentasPorCobrar, "RecibeCuentasPorCobrar"),
                    (TipoInformeEnum.CuentasPorPagar, "RecibeCuentasPorPagar"),
                    (TipoInformeEnum.AlertaStockBajo, "RecibeAlertaStock"),
                    (TipoInformeEnum.ResumenSifen, "RecibeResumenSifen"),
                    (TipoInformeEnum.ResumenCierreSistema, "RecibeResumenCierre")
                };

                foreach (var (tipo, campo) in informesCierre)
                {
                    var destinatarios = await ObtenerDestinatariosAsync(tipo, sucursalId);
                    if (destinatarios.Any())
                    {
                        var resultado = await EnviarInformeAsync(tipo, sucursalId, fecha, fecha);
                        if (resultado.Exito)
                        {
                            informesEnviados++;
                        }
                        else
                        {
                            errores.Add($"{ObtenerNombreInforme(tipo)}: {resultado.Mensaje}");
                        }
                    }
                }

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
            var ventas = await ctx.Ventas
                .AsNoTracking()
                .Where(v => v.IdSucursal == sucursalId && v.Fecha >= desde && v.Fecha <= hasta.AddDays(1) && v.Estado != "Anulada")
                .Include(v => v.Cliente)
                .OrderBy(v => v.Fecha)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine(GenerarEncabezadoHtml($"Informe de Ventas Diarias", empresa, sucursal, desde, hasta));
            
            sb.AppendLine("<h3>Resumen del Día</h3>");
            sb.AppendLine("<table class='summary'>");
            sb.AppendLine($"<tr><td>Total de Ventas:</td><td><strong>{ventas.Count}</strong></td></tr>");
            sb.AppendLine($"<tr><td>Monto Total:</td><td><strong>Gs. {ventas.Sum(v => v.Total):N0}</strong></td></tr>");
            sb.AppendLine("</table>");

            if (ventas.Any())
            {
                sb.AppendLine("<h3>Detalle de Ventas</h3>");
                sb.AppendLine("<table class='data'>");
                sb.AppendLine("<thead><tr><th>Nro. Factura</th><th>Cliente</th><th>Fecha/Hora</th><th class='right'>Total</th></tr></thead>");
                sb.AppendLine("<tbody>");
                foreach (var v in ventas)
                {
                    sb.AppendLine($"<tr>");
                    sb.AppendLine($"<td>{v.NumeroFactura ?? "-"}</td>");
                    sb.AppendLine($"<td>{v.Cliente?.RazonSocial ?? "Sin cliente"}</td>");
                    sb.AppendLine($"<td>{v.Fecha:dd/MM/yyyy HH:mm}</td>");
                    sb.AppendLine($"<td class='right'>Gs. {v.Total:N0}</td>");
                    sb.AppendLine($"</tr>");
                }
                sb.AppendLine("</tbody></table>");
            }

            sb.AppendLine(GenerarPieHtml());
            return sb.ToString();
        }

        private async Task<string> GenerarHtmlVentasDetalladoAsync(AppDbContext ctx, int sucursalId, DateTime desde, DateTime hasta, string empresa, string sucursal)
        {
            var ventas = await ctx.Ventas
                .AsNoTracking()
                .Where(v => v.IdSucursal == sucursalId && v.Fecha >= desde && v.Fecha <= hasta.AddDays(1) && v.Estado != "Anulada")
                .Include(v => v.Cliente)
                .OrderBy(v => v.Fecha)
                .ToListAsync();

            // Cargar detalles por separado
            var ventaIds = ventas.Select(v => v.IdVenta).ToList();
            var detalles = await ctx.VentasDetalles
                .AsNoTracking()
                .Where(d => ventaIds.Contains(d.IdVenta))
                .Include(d => d.Producto)
                .ToListAsync();
            var detallesPorVenta = detalles.GroupBy(d => d.IdVenta).ToDictionary(g => g.Key, g => g.ToList());

            var sb = new StringBuilder();
            sb.AppendLine(GenerarEncabezadoHtml($"Informe de Ventas Detallado", empresa, sucursal, desde, hasta));

            sb.AppendLine("<h3>Resumen</h3>");
            sb.AppendLine("<table class='summary'>");
            sb.AppendLine($"<tr><td>Total de Ventas:</td><td><strong>{ventas.Count}</strong></td></tr>");
            sb.AppendLine($"<tr><td>Monto Total:</td><td><strong>Gs. {ventas.Sum(v => v.Total):N0}</strong></td></tr>");
            sb.AppendLine("</table>");

            foreach (var v in ventas)
            {
                sb.AppendLine($"<div class='venta-card'>");
                sb.AppendLine($"<h4>Factura: {v.NumeroFactura ?? "-"} - {v.Cliente?.RazonSocial ?? "Sin cliente"}</h4>");
                sb.AppendLine($"<p>Fecha: {v.Fecha:dd/MM/yyyy HH:mm}</p>");
                
                if (detallesPorVenta.TryGetValue(v.IdVenta, out var ventaDetalles) && ventaDetalles.Any())
                {
                    sb.AppendLine("<table class='data small'>");
                    sb.AppendLine("<thead><tr><th>Producto</th><th class='right'>Cant.</th><th class='right'>P.Unit.</th><th class='right'>Subtotal</th></tr></thead>");
                    sb.AppendLine("<tbody>");
                    foreach (var d in ventaDetalles)
                    {
                        sb.AppendLine($"<tr>");
                        sb.AppendLine($"<td>{d.Producto?.Descripcion ?? "Producto"}</td>");
                        sb.AppendLine($"<td class='right'>{d.Cantidad:N2}</td>");
                        sb.AppendLine($"<td class='right'>Gs. {d.PrecioUnitario:N0}</td>");
                        sb.AppendLine($"<td class='right'>Gs. {d.Importe:N0}</td>");
                        sb.AppendLine($"</tr>");
                    }
                    sb.AppendLine("</tbody></table>");
                }
                sb.AppendLine($"<p class='total'>Total: <strong>Gs. {v.Total:N0}</strong></p>");
                sb.AppendLine("</div>");
            }

            sb.AppendLine(GenerarPieHtml());
            return sb.ToString();
        }

        private async Task<string> GenerarHtmlVentasAgrupadoAsync(AppDbContext ctx, int sucursalId, DateTime desde, DateTime hasta, string empresa, string sucursal)
        {
            var ventas = await ctx.Ventas
                .AsNoTracking()
                .Where(v => v.IdSucursal == sucursalId && v.Fecha >= desde && v.Fecha <= hasta.AddDays(1) && v.Estado != "Anulada")
                .GroupBy(v => v.Fecha.Date)
                .Select(g => new { Fecha = g.Key, Cantidad = g.Count(), Total = g.Sum(v => v.Total) })
                .OrderBy(x => x.Fecha)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine(GenerarEncabezadoHtml($"Informe de Ventas Agrupado", empresa, sucursal, desde, hasta));

            sb.AppendLine("<table class='data'>");
            sb.AppendLine("<thead><tr><th>Fecha</th><th class='right'>Cantidad</th><th class='right'>Total</th></tr></thead>");
            sb.AppendLine("<tbody>");
            foreach (var item in ventas)
            {
                sb.AppendLine($"<tr><td>{item.Fecha:dd/MM/yyyy}</td><td class='right'>{item.Cantidad}</td><td class='right'>Gs. {item.Total:N0}</td></tr>");
            }
            sb.AppendLine($"<tr class='total-row'><td><strong>TOTAL</strong></td><td class='right'><strong>{ventas.Sum(v => v.Cantidad)}</strong></td><td class='right'><strong>Gs. {ventas.Sum(v => v.Total):N0}</strong></td></tr>");
            sb.AppendLine("</tbody></table>");

            sb.AppendLine(GenerarPieHtml());
            return sb.ToString();
        }

        private async Task<string> GenerarHtmlComprasGeneralAsync(AppDbContext ctx, int sucursalId, DateTime desde, DateTime hasta, string empresa, string sucursal)
        {
            var compras = await ctx.Compras
                .AsNoTracking()
                .Where(c => c.IdSucursal == sucursalId && c.Fecha >= desde && c.Fecha <= hasta.AddDays(1) && c.Estado != "Anulada")
                .Include(c => c.Proveedor)
                .OrderBy(c => c.Fecha)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine(GenerarEncabezadoHtml($"Informe de Compras", empresa, sucursal, desde, hasta));

            sb.AppendLine("<h3>Resumen</h3>");
            sb.AppendLine("<table class='summary'>");
            sb.AppendLine($"<tr><td>Total de Compras:</td><td><strong>{compras.Count}</strong></td></tr>");
            sb.AppendLine($"<tr><td>Monto Total:</td><td><strong>Gs. {compras.Sum(c => c.Total):N0}</strong></td></tr>");
            sb.AppendLine("</table>");

            if (compras.Any())
            {
                sb.AppendLine("<h3>Detalle de Compras</h3>");
                sb.AppendLine("<table class='data'>");
                sb.AppendLine("<thead><tr><th>Nro. Factura</th><th>Proveedor</th><th>Fecha</th><th class='right'>Total</th></tr></thead>");
                sb.AppendLine("<tbody>");
                foreach (var c in compras)
                {
                    sb.AppendLine($"<tr>");
                    sb.AppendLine($"<td>{c.NumeroFactura ?? "-"}</td>");
                    sb.AppendLine($"<td>{c.Proveedor?.RazonSocial ?? "Sin proveedor"}</td>");
                    sb.AppendLine($"<td>{c.Fecha:dd/MM/yyyy}</td>");
                    sb.AppendLine($"<td class='right'>Gs. {c.Total:N0}</td>");
                    sb.AppendLine($"</tr>");
                }
                sb.AppendLine("</tbody></table>");
            }

            sb.AppendLine(GenerarPieHtml());
            return sb.ToString();
        }

        private async Task<string> GenerarHtmlComprasDetalladoAsync(AppDbContext ctx, int sucursalId, DateTime desde, DateTime hasta, string empresa, string sucursal)
        {
            // Similar a compras general pero con detalles de productos
            return await GenerarHtmlComprasGeneralAsync(ctx, sucursalId, desde, hasta, empresa, sucursal);
        }

        private async Task<string> GenerarHtmlResumenCajaAsync(AppDbContext ctx, int sucursalId, DateTime desde, DateTime hasta, string empresa, string sucursal)
        {
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
            sb.AppendLine(GenerarEncabezadoHtml($"Resumen de Caja", empresa, sucursal, desde, hasta));

            sb.AppendLine("<h3>Resumen del Período</h3>");
            sb.AppendLine("<table class='summary'>");
            sb.AppendLine($"<tr><td>Total Ventas:</td><td><strong>Gs. {ventas.Sum(v => v.Total):N0}</strong></td></tr>");
            sb.AppendLine($"<tr><td>Cantidad de Ventas:</td><td>{ventas.Count}</td></tr>");
            sb.AppendLine($"<tr><td>Cierres de Caja:</td><td>{cierres.Count}</td></tr>");
            if (cierres.Any())
            {
                sb.AppendLine($"<tr><td>Total Entregado:</td><td>Gs. {cierres.Sum(c => c.TotalEntregado):N0}</td></tr>");
                sb.AppendLine($"<tr><td>Diferencia Total:</td><td style='{(cierres.Sum(c => c.Diferencia) < 0 ? "color:red;" : "")}'>Gs. {cierres.Sum(c => c.Diferencia):N0}</td></tr>");
            }
            sb.AppendLine("</table>");

            // Resumen de operaciones desde los cierres
            if (cierres.Any())
            {
                sb.AppendLine("<h3>Operaciones en Cierres</h3>");
                sb.AppendLine("<table class='summary'>");
                sb.AppendLine($"<tr><td>Ventas Contado:</td><td style='color:green;'>Gs. {cierres.Sum(c => c.TotalVentasContado):N0}</td></tr>");
                sb.AppendLine($"<tr><td>Ventas Crédito:</td><td>Gs. {cierres.Sum(c => c.TotalVentasCredito):N0}</td></tr>");
                sb.AppendLine($"<tr><td>Cobros Crédito:</td><td style='color:green;'>Gs. {cierres.Sum(c => c.TotalCobrosCredito):N0}</td></tr>");
                sb.AppendLine($"<tr><td>Anulaciones:</td><td style='color:red;'>Gs. {cierres.Sum(c => c.TotalAnulaciones):N0}</td></tr>");
                
                var totalNCVentas = cierres.Sum(c => c.TotalNotasCredito);
                var cantNCVentas = cierres.Sum(c => c.CantNotasCredito);
                if (cantNCVentas > 0)
                    sb.AppendLine($"<tr><td>NC Ventas ({cantNCVentas}):</td><td style='color:#ffc107;'>-Gs. {totalNCVentas:N0}</td></tr>");
                
                var totalComprasEfect = cierres.Sum(c => c.TotalComprasEfectivo);
                var cantComprasEfect = cierres.Sum(c => c.CantComprasEfectivo);
                if (cantComprasEfect > 0)
                    sb.AppendLine($"<tr><td>Compras Efectivo ({cantComprasEfect}):</td><td style='color:red;'>-Gs. {totalComprasEfect:N0}</td></tr>");
                
                var totalNCCompras = cierres.Sum(c => c.TotalNotasCreditoCompras);
                var cantNCCompras = cierres.Sum(c => c.CantNotasCreditoCompras);
                if (cantNCCompras > 0)
                    sb.AppendLine($"<tr><td>NC Compras ({cantNCCompras}):</td><td style='color:green;'>+Gs. {totalNCCompras:N0}</td></tr>");
                
                // Neto en Caja
                var netoEnCaja = cierres.Sum(c => c.TotalVentasContado + c.TotalCobrosCredito - c.TotalNotasCredito - c.TotalComprasEfectivo + c.TotalNotasCreditoCompras);
                sb.AppendLine($"<tr style='border-top:2px solid #333;'><td><strong>NETO EN CAJA:</strong></td><td><strong>Gs. {netoEnCaja:N0}</strong></td></tr>");
                sb.AppendLine("</table>");
            }

            sb.AppendLine(GenerarPieHtml());
            return sb.ToString();
        }

        private async Task<string> GenerarHtmlCuentasPorCobrarAsync(AppDbContext ctx, int sucursalId, string empresa, string sucursal)
        {
            var cuentas = await ctx.CuentasPorCobrar
                .AsNoTracking()
                .Where(c => c.IdSucursal == sucursalId && c.Estado == "PENDIENTE")
                .Include(c => c.Cliente)
                .OrderBy(c => c.FechaVencimiento)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine(GenerarEncabezadoHtml($"Cuentas por Cobrar", empresa, sucursal, null, null));

            sb.AppendLine("<h3>Resumen</h3>");
            sb.AppendLine("<table class='summary'>");
            sb.AppendLine($"<tr><td>Total Pendiente:</td><td><strong>Gs. {cuentas.Sum(c => c.SaldoPendiente):N0}</strong></td></tr>");
            sb.AppendLine($"<tr><td>Cantidad de Cuentas:</td><td>{cuentas.Count}</td></tr>");
            var vencidas = cuentas.Where(c => c.FechaVencimiento < DateTime.Today).ToList();
            if (vencidas.Any())
            {
                sb.AppendLine($"<tr><td style='color:red;'>Vencidas:</td><td style='color:red;'><strong>Gs. {vencidas.Sum(c => c.SaldoPendiente):N0} ({vencidas.Count})</strong></td></tr>");
            }
            sb.AppendLine("</table>");

            if (cuentas.Any())
            {
                sb.AppendLine("<h3>Detalle</h3>");
                sb.AppendLine("<table class='data'>");
                sb.AppendLine("<thead><tr><th>Cliente</th><th>Vencimiento</th><th class='right'>Saldo</th><th>Estado</th></tr></thead>");
                sb.AppendLine("<tbody>");
                foreach (var c in cuentas.Take(50))
                {
                    var vencida = c.FechaVencimiento < DateTime.Today;
                    sb.AppendLine($"<tr style='{(vencida ? "color:red;" : "")}'>");
                    sb.AppendLine($"<td>{c.Cliente?.RazonSocial ?? "-"}</td>");
                    sb.AppendLine($"<td>{c.FechaVencimiento:dd/MM/yyyy}</td>");
                    sb.AppendLine($"<td class='right'>Gs. {c.SaldoPendiente:N0}</td>");
                    sb.AppendLine($"<td>{(vencida ? "VENCIDA" : "Vigente")}</td>");
                    sb.AppendLine($"</tr>");
                }
                sb.AppendLine("</tbody></table>");
            }

            sb.AppendLine(GenerarPieHtml());
            return sb.ToString();
        }

        private async Task<string> GenerarHtmlCuentasPorPagarAsync(AppDbContext ctx, int sucursalId, string empresa, string sucursal)
        {
            var cuentas = await ctx.CuentasPorPagar
                .AsNoTracking()
                .Where(c => c.IdSucursal == sucursalId && c.Estado == "PENDIENTE")
                .Include(c => c.Proveedor)
                .OrderBy(c => c.FechaVencimiento)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine(GenerarEncabezadoHtml($"Cuentas por Pagar", empresa, sucursal, null, null));

            sb.AppendLine("<h3>Resumen</h3>");
            sb.AppendLine("<table class='summary'>");
            sb.AppendLine($"<tr><td>Total Pendiente:</td><td><strong>Gs. {cuentas.Sum(c => c.SaldoPendiente):N0}</strong></td></tr>");
            sb.AppendLine($"<tr><td>Cantidad de Cuentas:</td><td>{cuentas.Count}</td></tr>");
            var vencidas = cuentas.Where(c => c.FechaVencimiento < DateTime.Today).ToList();
            if (vencidas.Any())
            {
                sb.AppendLine($"<tr><td style='color:red;'>Vencidas:</td><td style='color:red;'><strong>Gs. {vencidas.Sum(c => c.SaldoPendiente):N0} ({vencidas.Count})</strong></td></tr>");
            }
            sb.AppendLine("</table>");

            if (cuentas.Any())
            {
                sb.AppendLine("<h3>Detalle</h3>");
                sb.AppendLine("<table class='data'>");
                sb.AppendLine("<thead><tr><th>Proveedor</th><th>Vencimiento</th><th class='right'>Saldo</th><th>Estado</th></tr></thead>");
                sb.AppendLine("<tbody>");
                foreach (var c in cuentas.Take(50))
                {
                    var vencida = c.FechaVencimiento < DateTime.Today;
                    sb.AppendLine($"<tr style='{(vencida ? "color:red;" : "")}'>");
                    sb.AppendLine($"<td>{c.Proveedor?.RazonSocial ?? "-"}</td>");
                    sb.AppendLine($"<td>{c.FechaVencimiento:dd/MM/yyyy}</td>");
                    sb.AppendLine($"<td class='right'>Gs. {c.SaldoPendiente:N0}</td>");
                    sb.AppendLine($"<td>{(vencida ? "VENCIDA" : "Vigente")}</td>");
                    sb.AppendLine($"</tr>");
                }
                sb.AppendLine("</tbody></table>");
            }

            sb.AppendLine(GenerarPieHtml());
            return sb.ToString();
        }

        private async Task<string> GenerarHtmlStockBajoAsync(AppDbContext ctx, int sucursalId, string empresa, string sucursal)
        {
            var productos = await ctx.Productos
                .AsNoTracking()
                .Where(p => p.Activo && p.Stock < p.StockMinimo)
                .OrderBy(p => p.Stock - p.StockMinimo)
                .Take(100)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine(GenerarEncabezadoHtml($"Alerta de Stock Bajo", empresa, sucursal, null, null));

            sb.AppendLine($"<h3 style='color:orange;'>⚠️ {productos.Count} productos con stock bajo el mínimo</h3>");

            if (productos.Any())
            {
                sb.AppendLine("<table class='data'>");
                sb.AppendLine("<thead><tr><th>Código</th><th>Producto</th><th class='right'>Stock</th><th class='right'>Mínimo</th><th class='right'>Faltante</th></tr></thead>");
                sb.AppendLine("<tbody>");
                foreach (var p in productos)
                {
                    var faltante = p.StockMinimo - p.Stock;
                    sb.AppendLine($"<tr>");
                    sb.AppendLine($"<td>{p.CodigoBarras ?? "-"}</td>");
                    sb.AppendLine($"<td>{p.Descripcion}</td>");
                    sb.AppendLine($"<td class='right' style='color:{(p.Stock <= 0 ? "red" : "orange")};'>{p.Stock:N0}</td>");
                    sb.AppendLine($"<td class='right'>{p.StockMinimo:N0}</td>");
                    sb.AppendLine($"<td class='right' style='color:red;'>{faltante:N0}</td>");
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

            var sb = new StringBuilder();
            sb.AppendLine(GenerarEncabezadoHtml($"Resumen SIFEN", empresa, sucursal, desde, hasta));

            sb.AppendLine("<h3>Estado de Documentos Electrónicos</h3>");
            sb.AppendLine("<table class='summary'>");
            sb.AppendLine($"<tr><td>Total Documentos:</td><td><strong>{ventas.Count}</strong></td></tr>");
            sb.AppendLine($"<tr style='color:green;'><td>✅ Aprobados:</td><td><strong>{aprobadas.Count}</strong></td></tr>");
            sb.AppendLine($"<tr style='color:orange;'><td>⏳ Pendientes:</td><td><strong>{pendientes.Count}</strong></td></tr>");
            if (rechazadas.Any())
            {
                sb.AppendLine($"<tr style='color:red;'><td>❌ Rechazados:</td><td><strong>{rechazadas.Count}</strong></td></tr>");
            }
            sb.AppendLine("</table>");

            if (rechazadas.Any())
            {
                sb.AppendLine("<h3 style='color:red;'>Documentos Rechazados</h3>");
                sb.AppendLine("<table class='data'>");
                sb.AppendLine("<thead><tr><th>Factura</th><th>Fecha</th><th>Mensaje</th></tr></thead>");
                sb.AppendLine("<tbody>");
                foreach (var v in rechazadas.Take(20))
                {
                    sb.AppendLine($"<tr>");
                    sb.AppendLine($"<td>{v.NumeroFactura}</td>");
                    sb.AppendLine($"<td>{v.Fecha:dd/MM/yyyy}</td>");
                    sb.AppendLine($"<td>{v.MensajeSifen ?? "-"}</td>");
                    sb.AppendLine($"</tr>");
                }
                sb.AppendLine("</tbody></table>");
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

            var sb = new StringBuilder();
            sb.AppendLine(GenerarEncabezadoHtml($"Resumen al Cierre del Sistema", empresa, sucursal, fecha, fecha));

            sb.AppendLine("<h3>📊 Resumen del Día</h3>");
            sb.AppendLine("<table class='summary'>");
            sb.AppendLine("<tr><th colspan='2' style='background:#007bff;color:white;'>VENTAS</th></tr>");
            sb.AppendLine($"<tr><td>Cantidad:</td><td><strong>{ventas.Count}</strong></td></tr>");
            sb.AppendLine($"<tr><td>Total Ventas:</td><td><strong style='color:green;'>Gs. {ventas.Sum(v => v.Total):N0}</strong></td></tr>");
            sb.AppendLine("<tr><th colspan='2' style='background:#17a2b8;color:white;'>COMPRAS</th></tr>");
            sb.AppendLine($"<tr><td>Cantidad:</td><td><strong>{compras.Count}</strong></td></tr>");
            sb.AppendLine($"<tr><td>Total Compras:</td><td><strong style='color:red;'>Gs. {compras.Sum(c => c.Total):N0}</strong></td></tr>");
            sb.AppendLine("<tr><th colspan='2' style='background:#6c757d;color:white;'>SALDOS PENDIENTES</th></tr>");
            sb.AppendLine($"<tr><td>Cuentas por Cobrar:</td><td>Gs. {cxc:N0}</td></tr>");
            sb.AppendLine($"<tr><td>Cuentas por Pagar:</td><td>Gs. {cxp:N0}</td></tr>");
            sb.AppendLine("</table>");

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

        // ========== HELPERS HTML ==========

        private string GenerarEncabezadoHtml(string titulo, string empresa, string sucursal, DateTime? desde, DateTime? hasta)
        {
            var fechaStr = desde.HasValue
                ? (hasta.HasValue && hasta != desde ? $"{desde:dd/MM/yyyy} - {hasta:dd/MM/yyyy}" : $"{desde:dd/MM/yyyy}")
                : DateTime.Today.ToString("dd/MM/yyyy");

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; margin: 20px; background: #f5f5f5; }}
        .container {{ max-width: 800px; margin: 0 auto; background: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; border-bottom: 2px solid #007bff; padding-bottom: 20px; margin-bottom: 20px; }}
        .header h1 {{ color: #333; margin: 0 0 5px 0; font-size: 24px; }}
        .header h2 {{ color: #007bff; margin: 0 0 10px 0; font-size: 18px; }}
        .header .fecha {{ color: #666; font-size: 14px; }}
        h3 {{ color: #333; border-bottom: 1px solid #ddd; padding-bottom: 10px; margin-top: 25px; }}
        table.summary {{ width: 100%; border-collapse: collapse; margin: 15px 0; }}
        table.summary td {{ padding: 8px 15px; border-bottom: 1px solid #eee; }}
        table.summary td:first-child {{ color: #666; width: 60%; }}
        table.data {{ width: 100%; border-collapse: collapse; margin: 15px 0; font-size: 13px; }}
        table.data th {{ background: #007bff; color: white; padding: 10px; text-align: left; }}
        table.data td {{ padding: 8px 10px; border-bottom: 1px solid #eee; }}
        table.data tr:hover {{ background: #f8f9fa; }}
        .right {{ text-align: right; }}
        .total-row {{ background: #f8f9fa; font-weight: bold; }}
        .venta-card {{ background: #f8f9fa; padding: 15px; margin: 15px 0; border-radius: 5px; border-left: 4px solid #007bff; }}
        .venta-card h4 {{ margin: 0 0 10px 0; color: #333; }}
        .venta-card .total {{ text-align: right; margin-top: 10px; font-size: 16px; }}
        table.small {{ font-size: 12px; }}
        .footer {{ text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; color: #999; font-size: 12px; }}
    </style>
</head>
<body>
<div class='container'>
    <div class='header'>
        <h1>{empresa}</h1>
        <h2>{titulo}</h2>
        <div class='fecha'>Sucursal: {sucursal} | Fecha: {fechaStr}</div>
    </div>
";
        }

        private string GenerarPieHtml()
        {
            return $@"
    <div class='footer'>
        <p>Generado automáticamente por SistemIA - {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>
    </div>
</div>
</body>
</html>";
        }
    }
}
