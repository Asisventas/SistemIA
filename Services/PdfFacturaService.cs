using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QRCoder;
using SistemIA.Models;

namespace SistemIA.Services;

/// <summary>
/// Servicio para generar PDF de facturas en formato KuDE (Representación gráfica de Factura Electrónica).
/// El diseño sigue el estándar SIFEN de Paraguay.
/// </summary>
public class PdfFacturaService
{
    private readonly IServiceProvider _serviceProvider;

    public PdfFacturaService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private string Pad7(string? n)
        => int.TryParse(n, out var x) ? x.ToString("D7") : (n ?? "0000001");

    private string NumeroKude(Venta venta)
        => $"{venta.Establecimiento ?? "001"}-{venta.PuntoExpedicion ?? "001"}-{Pad7(venta.NumeroFactura)}";

    public async Task<byte[]> GenerarPdfFactura(int idVenta)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Cargar venta con todas las relaciones necesarias
        var venta = await context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Sucursal)
            .Include(v => v.Usuario)
            .Include(v => v.TipoPago)
            .Include(v => v.Moneda)
            .FirstOrDefaultAsync(v => v.IdVenta == idVenta);

        if (venta == null)
            throw new ArgumentException($"Venta con ID {idVenta} no encontrada");

        var detalles = await context.Set<VentaDetalle>()
            .Include(d => d.Producto)
            .Where(d => d.IdVenta == idVenta)
            .OrderBy(d => d.IdVentaDetalle)
            .ToListAsync();

        // Cargar configuración de caja para determinar tipo de facturación
        Caja? cajaConfig = null;
        if (venta.IdCaja.HasValue)
        {
            cajaConfig = await context.Cajas.AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdCaja == venta.IdCaja.Value);
        }
        cajaConfig ??= await context.Cajas.AsNoTracking()
            .FirstOrDefaultAsync(c => c.CajaActual == 1);

        var tipoFacturacion = cajaConfig?.TipoFacturacion ?? "AUTOIMPRESOR";
        bool esFacturaElectronica = tipoFacturacion?.ToUpper() == "ELECTRONICA"
                                 || tipoFacturacion?.ToUpper() == "FACTURA ELECTRONICA";

        Console.WriteLine($"[PDF KuDE] Venta {idVenta}, Caja: {venta.IdCaja}, TipoFacturacion: {tipoFacturacion}, EsFE: {esFacturaElectronica}");

        // Generar QR solo si es factura electrónica y tiene CDC
        byte[]? qrPng = null;
        if (esFacturaElectronica && !string.IsNullOrWhiteSpace(venta.CDC))
        {
            var urlQr = $"https://ekuatia.set.gov.py/consultas/gestionarDoc/qr?CDC={venta.CDC}";
            using var qr = new QRCodeGenerator();
            var qrData = qr.CreateQrCode(urlQr, QRCodeGenerator.ECCLevel.M);
            using var qrCode = new PngByteQRCode(qrData);
            qrPng = qrCode.GetGraphic(pixelsPerModule: 20);
        }

        // Datos de la empresa/sucursal
        var sucursal = venta.Sucursal;
        var cliente = venta.Cliente;
        var rucEmpresa = $"{sucursal?.RUC}-{sucursal?.DV}";
        var timbrado = venta.Timbrado ?? "";
        var vigenciaTimbrado = cajaConfig?.VigenciaDel?.ToString("dd/MM/yyyy") ?? "";

        // Calcular totales
        var totalExenta = detalles.Sum(x => x.Exenta);
        var totalGrav5 = detalles.Sum(x => x.Grabado5);
        var totalGrav10 = detalles.Sum(x => x.Grabado10);
        var iva5 = detalles.Sum(x => x.IVA5);
        var iva10 = detalles.Sum(x => x.IVA10);
        var totalIva = iva5 + iva10;

        // RUC/CI del cliente
        var rucCiCliente = string.IsNullOrWhiteSpace(cliente?.RUC)
            ? cliente?.NumeroDocumentoIdentidad ?? "—"
            : $"{cliente?.RUC}-{cliente?.DV}";

        // Condición de venta
        var condicionVenta = string.IsNullOrWhiteSpace(venta.FormaPago)
            ? (venta.TipoPago?.EsCredito == true ? "CRÉDITO" : "CONTADO")
            : venta.FormaPago;

        QuestPDF.Settings.License = LicenseType.Community;

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(10, Unit.Millimetre);
                page.DefaultTextStyle(t => t.FontSize(9).FontFamily(Fonts.Arial));

                page.Content().Column(column =>
                {
                    // ========== ENCABEZADO KuDE ==========
                    column.Item().Border(1).BorderColor(Colors.Grey.Medium).Row(encabezado =>
                    {
                        // COLUMNA IZQUIERDA: Logo + Datos Empresa
                        encabezado.RelativeItem(3).Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(6).Column(izq =>
                        {
                            // Logo de la empresa (si existe)
                            if (sucursal?.Logo != null && sucursal.Logo.Length > 0)
                            {
                                izq.Item().Height(40).Width(120).Image(sucursal.Logo).FitArea();
                            }
                            else
                            {
                                // Placeholder si no hay logo
                                izq.Item().Height(40).Width(120)
                                    .Background(Colors.Grey.Lighten3)
                                    .AlignCenter().AlignMiddle()
                                    .Text("LOGO").FontSize(12).Bold().FontColor(Colors.Grey.Medium);
                            }

                            izq.Item().PaddingTop(8).Text(sucursal?.NombreEmpresa ?? "EMPRESA").FontSize(11).Bold();
                            izq.Item().Text(sucursal?.Direccion ?? "").FontSize(8);
                            if (!string.IsNullOrWhiteSpace(sucursal?.Telefono))
                                izq.Item().Text($"Teléfono: {sucursal.Telefono}").FontSize(8);
                            if (!string.IsNullOrWhiteSpace(sucursal?.Correo))
                                izq.Item().Text(sucursal.Correo).FontSize(8);
                            if (!string.IsNullOrWhiteSpace(sucursal?.RubroEmpresa))
                                izq.Item().PaddingTop(4).Text($"Actividad Económica: {sucursal.RubroEmpresa}").FontSize(8);
                        });

                        // COLUMNA DERECHA: Datos Fiscales
                        encabezado.RelativeItem(2).Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(6).Column(der =>
                        {
                            der.Item().AlignRight().Text($"R.U.C.: {rucEmpresa}").FontSize(10).Bold();
                            if (!string.IsNullOrWhiteSpace(timbrado))
                                der.Item().AlignRight().Text($"Timbrado N°: {timbrado}").FontSize(10).Bold();
                            if (!string.IsNullOrWhiteSpace(vigenciaTimbrado))
                                der.Item().AlignRight().Text($"Fecha de Inicio de Vigencia: {vigenciaTimbrado}").FontSize(8);

                            der.Item().PaddingTop(12).AlignRight()
                                .Text(esFacturaElectronica ? "FACTURA ELECTRÓNICA" : "FACTURA")
                                .FontSize(14).Bold().FontColor(Colors.Blue.Darken2);

                            der.Item().AlignRight().Text(NumeroKude(venta)).FontSize(14).Bold();
                        });
                    });

                    // ========== BANDA DE INFORMACIÓN (2 columnas) ==========
                    column.Item().PaddingTop(8).Border(1).BorderColor(Colors.Grey.Medium).Row(banda =>
                    {
                        // Columna izquierda - Datos de la transacción
                        banda.RelativeItem().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(izq =>
                        {
                            izq.Item().Text(t => { t.Span("Fecha y Hora de Emisión: ").Bold(); t.Span($"{venta.Fecha:dd/MM/yyyy HH:mm:ss}"); });
                            izq.Item().Text(t => { t.Span("Condición de Venta: ").Bold(); t.Span(condicionVenta); });
                            izq.Item().Text(t => { t.Span("Cuotas: ").Bold(); t.Span($"{(venta.TipoPago?.EsCredito == true ? (venta.Plazo ?? 0) : 1)}"); });
                            izq.Item().Text(t => { t.Span("Moneda: ").Bold(); t.Span(venta.Moneda?.CodigoISO ?? "PYG"); });
                            izq.Item().Text(t => { t.Span("Tipo de Cambio: ").Bold(); t.Span($"{venta.CambioDelDia ?? 1}"); });
                            izq.Item().Text(t => { t.Span("N° Venta: ").Bold(); t.Span($"{venta.IdVenta}"); });
                            izq.Item().Text(t => { t.Span("N° Pedido: ").Bold(); t.Span(venta.NroPedido ?? "—"); });
                        });

                        // Columna derecha - Datos del cliente
                        banda.RelativeItem().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(der =>
                        {
                            der.Item().Text(t => { t.Span("Tipo Doc.: ").Bold(); t.Span(venta.TipoDocumento ?? "FACTURA"); });
                            der.Item().Text(t => { t.Span("R.U.C./C.I.: ").Bold(); t.Span(rucCiCliente); });
                            der.Item().Text(t => { t.Span("Razón Social: ").Bold(); t.Span(cliente?.RazonSocial ?? "SIN NOMBRE"); });
                            der.Item().Text(t => { t.Span("Dirección: ").Bold(); t.Span(cliente?.Direccion ?? "—"); });
                            if (!string.IsNullOrWhiteSpace(cliente?.Telefono))
                                der.Item().Text(t => { t.Span("Teléfono: ").Bold(); t.Span(cliente.Telefono); });
                            if (!string.IsNullOrWhiteSpace(cliente?.Email))
                                der.Item().Text(t => { t.Span("Correo Electrónico: ").Bold(); t.Span(cliente.Email); });
                            der.Item().Text(t => { t.Span("Plazo Crédito (días): ").Bold(); t.Span($"{(venta.TipoPago?.EsCredito == true ? (venta.Plazo ?? 0) : 0)}"); });
                            der.Item().Text(t => { t.Span("Tipo de Transacción: ").Bold(); t.Span(venta.TipoPago?.EsCredito == true ? "Crédito" : "Contado"); });
                        });
                    });

                    // ========== TABLA DE ITEMS ==========
                    column.Item().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(50);   // Código
                            columns.RelativeColumn(4);    // Descripción
                            columns.ConstantColumn(40);   // Unidad
                            columns.ConstantColumn(35);   // Cajas
                            columns.ConstantColumn(40);   // Cantidad
                            columns.ConstantColumn(55);   // Precio Unit.
                            columns.ConstantColumn(45);   // Descuento
                            columns.ConstantColumn(55);   // Exentas
                            columns.ConstantColumn(45);   // 5%
                            columns.ConstantColumn(55);   // 10%
                        });

                        // Encabezado de tabla
                        table.Header(header =>
                        {
                            void HeaderCell(IContainer container, string texto)
                            {
                                container.Border(0.5f).BorderColor(Colors.Grey.Medium)
                                    .Background(Colors.Grey.Lighten3)
                                    .Padding(3).AlignCenter().AlignMiddle()
                                    .Text(texto).FontSize(7).Bold();
                            }

                            HeaderCell(header.Cell(), "Código");
                            HeaderCell(header.Cell(), "Descripción");
                            HeaderCell(header.Cell(), "Unidad Medida");
                            HeaderCell(header.Cell(), "Cajas");
                            HeaderCell(header.Cell(), "Cantidad");
                            HeaderCell(header.Cell(), "Precio Unitario");
                            HeaderCell(header.Cell(), "Descuento");
                            HeaderCell(header.Cell(), "Exentas");
                            HeaderCell(header.Cell(), "5%");
                            HeaderCell(header.Cell(), "10%");
                        });

                        // Filas de datos
                        foreach (var d in detalles)
                        {
                            var codigo = d.Producto?.CodigoInterno ?? d.Producto?.CodigoBarras ?? d.IdProducto.ToString();
                            
                            // Calcular valores de venta CON IVA incluido (Grabado + IVA)
                            var valorExenta = d.Exenta;
                            var valorVenta5 = d.Grabado5 + d.IVA5;   // Precio con IVA 5% incluido
                            var valorVenta10 = d.Grabado10 + d.IVA10; // Precio con IVA 10% incluido

                            void DataCell(IContainer container, string texto, bool alignRight = false)
                            {
                                var cell = container.Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(2);
                                if (alignRight)
                                    cell.AlignRight().Text(texto).FontSize(8);
                                else
                                    cell.Text(texto).FontSize(8);
                            }

                            DataCell(table.Cell(), codigo);
                            DataCell(table.Cell(), d.Producto?.Descripcion ?? "");
                            table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(2).AlignCenter().Text(d.Producto?.UndMedida ?? "UNI").FontSize(8);
                            table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(2).AlignCenter().Text("1,00").FontSize(8);
                            DataCell(table.Cell(), d.Cantidad.ToString("N2"), true);
                            DataCell(table.Cell(), d.PrecioUnitario.ToString("N0"), true);
                            DataCell(table.Cell(), (d.PorcentajeDescuento ?? 0).ToString("N0"), true);
                            // Mostrar valor de venta CON IVA incluido en cada columna
                            DataCell(table.Cell(), valorExenta > 0 ? valorExenta.ToString("N0") : "", true);
                            DataCell(table.Cell(), valorVenta5 > 0 ? valorVenta5.ToString("N0") : "", true);
                            DataCell(table.Cell(), valorVenta10 > 0 ? valorVenta10.ToString("N0") : "", true);
                        }
                    });

                    // ========== RESUMEN / TOTALES ==========
                    column.Item().PaddingTop(8).Row(resumen =>
                    {
                        // Espacio izquierdo (para el texto en letras)
                        resumen.RelativeItem(2).Column(letras =>
                        {
                            letras.Item().Text("Subtotal:").FontSize(9).Bold();
                            letras.Item().PaddingTop(4).Text("Total de la Operación (en guaraníes):").FontSize(9).Bold();
                            if (!string.IsNullOrWhiteSpace(venta.TotalEnLetras))
                                letras.Item().PaddingTop(4).Text(venta.TotalEnLetras).FontSize(8).Italic();
                        });

                        // Totales numéricos
                        resumen.RelativeItem(1).Column(nums =>
                        {
                            nums.Item().AlignRight().Text(venta.Total.ToString("N0")).FontSize(9).Bold();
                            nums.Item().PaddingTop(4).AlignRight().Text(venta.Total.ToString("N0")).FontSize(11).Bold();
                        });
                    });

                    // ========== LIQUIDACIÓN DEL IVA ==========
                    column.Item().PaddingTop(8).Border(1).BorderColor(Colors.Grey.Medium).Column(liq =>
                    {
                        liq.Item().Background(Colors.Blue.Lighten4).Padding(4).AlignCenter()
                            .Text("Liquidación del I.V.A.").FontSize(10).Bold();

                        // Calcular totales de venta CON IVA
                        var totalVenta5 = totalGrav5 + iva5;   // Total venta 5% (con IVA)
                        var totalVenta10 = totalGrav10 + iva10; // Total venta 10% (con IVA)

                        liq.Item().Table(tablaIva =>
                        {
                            tablaIva.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(2); // Concepto
                                cols.RelativeColumn();  // 5%
                                cols.RelativeColumn();  // 10%
                                cols.RelativeColumn();  // Total
                            });

                            // Encabezado
                            void HeaderIva(IContainer c, string txt)
                            {
                                c.Border(0.5f).BorderColor(Colors.Grey.Lighten1)
                                    .Background(Colors.Blue.Lighten5)
                                    .Padding(3).AlignCenter()
                                    .Text(txt).FontSize(8).Bold();
                            }

                            HeaderIva(tablaIva.Cell(), "Concepto");
                            HeaderIva(tablaIva.Cell(), "5%");
                            HeaderIva(tablaIva.Cell(), "10%");
                            HeaderIva(tablaIva.Cell(), "Total");

                            // Fila: Total Gravado (base imponible)
                            void CeldaIva(IContainer c, string txt)
                            {
                                c.Border(0.5f).BorderColor(Colors.Grey.Lighten1)
                                    .Padding(3).AlignRight()
                                    .Text(txt).FontSize(8);
                            }
                            void CeldaIvaLabel(IContainer c, string txt)
                            {
                                c.Border(0.5f).BorderColor(Colors.Grey.Lighten1)
                                    .Padding(3)
                                    .Text(txt).FontSize(8);
                            }

                            CeldaIvaLabel(tablaIva.Cell(), "Total Gravado");
                            CeldaIva(tablaIva.Cell(), totalGrav5.ToString("N0"));
                            CeldaIva(tablaIva.Cell(), totalGrav10.ToString("N0"));
                            CeldaIva(tablaIva.Cell(), (totalGrav5 + totalGrav10).ToString("N0"));

                            // Fila: Liquidación IVA
                            CeldaIvaLabel(tablaIva.Cell(), "Liquidación IVA");
                            CeldaIva(tablaIva.Cell(), iva5.ToString("N0"));
                            CeldaIva(tablaIva.Cell(), iva10.ToString("N0"));
                            CeldaIva(tablaIva.Cell(), totalIva.ToString("N0"));

                            // Fila: Total (gravado + IVA = valor venta)
                            tablaIva.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1)
                                .Background(Colors.Grey.Lighten4).Padding(3)
                                .Text("Total c/IVA").FontSize(8).Bold();
                            tablaIva.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1)
                                .Background(Colors.Grey.Lighten4).Padding(3).AlignRight()
                                .Text(totalVenta5.ToString("N0")).FontSize(8).Bold();
                            tablaIva.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1)
                                .Background(Colors.Grey.Lighten4).Padding(3).AlignRight()
                                .Text(totalVenta10.ToString("N0")).FontSize(8).Bold();
                            tablaIva.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten1)
                                .Background(Colors.Grey.Lighten4).Padding(3).AlignRight()
                                .Text((totalVenta5 + totalVenta10).ToString("N0")).FontSize(8).Bold();

                            // Fila: Exentas (si hay)
                            if (totalExenta > 0)
                            {
                                CeldaIvaLabel(tablaIva.Cell(), "Total Exentas");
                                CeldaIva(tablaIva.Cell(), "—");
                                CeldaIva(tablaIva.Cell(), "—");
                                CeldaIva(tablaIva.Cell(), totalExenta.ToString("N0"));
                            }
                        });
                    });

                    // ========== SECCIÓN QR y CDC (solo Factura Electrónica) ==========
                    if (esFacturaElectronica)
                    {
                        column.Item().PaddingTop(10).Border(1).BorderColor(Colors.Grey.Medium).Row(validacion =>
                        {
                            // QR a la izquierda
                            validacion.ConstantItem(100).Padding(6).Column(qrCol =>
                            {
                                if (qrPng != null)
                                {
                                    qrCol.Item().Width(80).Height(80).Image(qrPng).FitArea();
                                }
                                else
                                {
                                    qrCol.Item().Width(80).Height(80)
                                        .Border(1).BorderColor(Colors.Grey.Lighten2)
                                        .Background(Colors.Grey.Lighten4)
                                        .AlignCenter().AlignMiddle()
                                        .Text("QR").FontSize(14).Bold().FontColor(Colors.Grey.Medium);
                                }
                            });

                            // Textos de validación
                            validacion.RelativeItem().Padding(6).Column(textos =>
                            {
                                textos.Item().Text("Consulte la validez de esta Factura Electrónica con el número de CDC impreso abajo en:")
                                    .FontSize(8).Bold();
                                textos.Item().Text("https://ekuatia.set.gov.py/consultas/")
                                    .FontSize(9).FontColor(Colors.Blue.Darken2).Bold();

                                textos.Item().PaddingTop(6).Border(1).BorderColor(Colors.Grey.Medium)
                                    .Background(Colors.Grey.Lighten4).Padding(4)
                                    .Text($"CDC: {venta.CDC ?? "(Pendiente)"}")
                                    .FontSize(9).Bold();

                                textos.Item().PaddingTop(6)
                                    .Text("ESTE DOCUMENTO ES UNA REPRESENTACIÓN GRÁFICA DE UN DOCUMENTO ELECTRÓNICO (XML)")
                                    .FontSize(7).Bold().FontColor(Colors.Red.Darken1);

                                textos.Item().PaddingTop(4).Text("Información de interés del receptor emisor:").FontSize(7);
                                textos.Item().Text("Si su documento electrónico presenta algún error, podrá solicitar la modificación dentro de las 48 horas siguientes de la emisión de este comprobante.")
                                    .FontSize(7);
                            });
                        });
                    }

                    // ========== LEYENDA FINAL ==========
                    column.Item().PaddingTop(10).Text(
                        "La falta de pago a su vencimiento constituirá en mora automática al deudor, sin necesidad de interpelación judicial o extrajudicial previa, devengándose un interés moratorio del 3% mensual para operaciones en guaraníes, más un interés punitorio del 30% del interés moratorio, el que se adicionará, por todo el tiempo de mora y que será calculado sobre los saldos deudores hasta el efectivo pago. A todos los efectos legales y procesales emergentes de este documento las partes aceptan la jurisdicción y competencia de los Jueces y Tribunales de la ciudad de Asunción, renunciando a cualquier otro fuero o jurisdicción que pudiera corresponder."
                    ).FontSize(6).FontColor(Colors.Grey.Darken1);
                });
            });
        });

        return documento.GeneratePdf();
    }

    /// <summary>
    /// Formatea el CDC en grupos de 4 caracteres para mejor legibilidad
    /// </summary>
    private static string FormatearCdc(string cdc)
    {
        if (string.IsNullOrWhiteSpace(cdc)) return "";
        var chars = cdc.Replace(" ", "").Replace("-", "");
        var grupos = new List<string>();
        for (int i = 0; i < chars.Length; i += 4)
        {
            grupos.Add(chars.Substring(i, Math.Min(4, chars.Length - i)));
        }
        return string.Join(" ", grupos);
    }
}
