using System;
using System.Linq;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QRCoder;
using SistemIA.Models;

namespace SistemIA.Services;

public class PdfFacturaService
{
    private readonly IServiceProvider _serviceProvider;

    public PdfFacturaService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private string ObtenerSimboloMoneda(Moneda? moneda)
    {
        return moneda?.Simbolo ?? "Gs.";
    }

    private string FormatearPrecio(decimal valor, Moneda? moneda)
    {
        var simbolo = ObtenerSimboloMoneda(moneda);
        
        // Si es dólar (símbolo $), formatea con 2 decimales
        if (simbolo == "$")
            return valor.ToString("N2");
        
        // Para moneda local (Gs.), formatea sin decimales
        return valor.ToString("N0");
    }

    public async Task<byte[]> GenerarPdfFactura(int idVenta)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Cargar venta (async) con las relaciones necesarias
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
        var cajaConfig = await context.Cajas.AsNoTracking()
            .FirstOrDefaultAsync(c => c.CajaActual == 1);
        var tipoFacturacion = cajaConfig?.TipoFacturacion ?? "ELECTRONICA";
        bool esFacturaElectronica = tipoFacturacion == "ELECTRONICA";

        // DEBUG: Log de configuración
        Console.WriteLine($"[DEBUG PDF] CajaConfig encontrada: {cajaConfig != null}");
        Console.WriteLine($"[DEBUG PDF] TipoFacturacion: {tipoFacturacion}");
        Console.WriteLine($"[DEBUG PDF] esFacturaElectronica: {esFacturaElectronica}");

        // Generar QR solo si es factura electrónica
        byte[]? qrPng = null;
        if (esFacturaElectronica && !string.IsNullOrWhiteSpace(venta.CDC))
        {
            using var qr = new QRCodeGenerator();
            var qrData = qr.CreateQrCode(venta.CDC, QRCodeGenerator.ECCLevel.M);
            using var qrCode = new PngByteQRCode(qrData);
            qrPng = qrCode.GetGraphic(pixelsPerModule: 20);
        }

        QuestPDF.Settings.License = LicenseType.Community;

        var documento = Document.Create(container =>
        {
            // Cargar plantilla XML
            var layoutPath = Path.Combine(AppContext.BaseDirectory, "FacturaLayout.xml");
            if (!File.Exists(layoutPath))
            {
                // Path alternativo (cuando se ejecuta desde raíz del proyecto con watch)
                var alt = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "FacturaLayout.xml");
                if (File.Exists(alt)) layoutPath = alt;        
            }
            XDocument? xmlLayout = null;
            try { if (File.Exists(layoutPath)) xmlLayout = XDocument.Load(layoutPath); } catch { /* ignorar */ }

            // Derivar algunos campos desde la plantilla si existe
            string razonSocial = xmlLayout?.Root?.Element("Empresa")?.Element("RazonSocial")?.Value
                ?? "FERRER MIRANDA NESTOR FABIAN";
            string rucEmpresa = xmlLayout?.Root?.Element("Empresa")?.Element("Ruc")?.Value ?? "1462998-1";
            string timbrado = venta.Timbrado ?? xmlLayout?.Root?.Element("Factura")?.Element("TimbradoNumero")?.Value ?? "";
            string vigencia = xmlLayout?.Root?.Element("Factura")?.Element("TimbradoVigencia")?.Value ?? "";


            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(12, Unit.Millimetre);
                page.DefaultTextStyle(t => t.FontSize(9).FontFamily(Fonts.Arial));

                page.Content().Column(column =>
                {
                    // ENCABEZADO EXACTO COMO LA IMAGEN
                    column.Item().Row(row =>
                    {
                        // Izquierda: Logo y empresa
                        row.RelativeItem(2).Column(left =>
                        {
                            // Logo ASISVENTAS (rectángulo negro con texto cyan)
                            left.Item().Width(70, Unit.Millimetre).Height(20, Unit.Millimetre)
                                .Border(1).BorderColor(Colors.Black)
                                .Background(Colors.Black)
                                .AlignCenter().AlignMiddle()
                                .Text("¡ASISVENTAS!").FontColor(Colors.Cyan.Medium).FontSize(14).Bold();

                            // Información de la empresa
                            left.Item().PaddingTop(10).Column(empresa =>
                            {
                                empresa.Item().Text(razonSocial).FontSize(11).Bold();
                                empresa.Item().Text($"R.U.C.: {rucEmpresa}").FontSize(10);
                                empresa.Item().Text("AVDA. AVELINO MARTINEZ Y DEL MAESTRO").FontSize(9);
                                empresa.Item().Text("(021) 573291").FontSize(9);
                                empresa.Item().Text("CONSULTORAWEMAG@HOTMAIL.COM").FontSize(9);
                                empresa.Item().PaddingTop(4).Text("DESARROLLO DE SOFTWARE").FontSize(10).Bold();
                            });
                        });

                        // Derecha: Datos fiscales
                        row.RelativeItem(1).Column(right =>
                        {
                            right.Item().AlignRight().Text($"R.U.C.: {rucEmpresa}").FontSize(11).Bold();
                            if (!string.IsNullOrWhiteSpace(timbrado))
                                right.Item().AlignRight().Text($"Timbrado Nº: {timbrado}").FontSize(11).Bold();
                            if (!string.IsNullOrWhiteSpace(vigencia))
                                right.Item().AlignRight().Text($"Vigencia: {vigencia}").FontSize(9);
                        });
                    });

                    // TÍTULO CENTRADO
                    column.Item().PaddingTop(15).AlignCenter().Column(titulo =>
                    {
                        titulo.Item().Text(esFacturaElectronica ? "FACTURA ELECTRÓNICA" : "FACTURA")
                            .FontSize(18).Bold();
                        titulo.Item().Text($"001-002-{venta.IdVenta.ToString().PadLeft(7, '0')}")
                            .FontSize(16).Bold();
                    });

                    // INFORMACIÓN DE LA VENTA (DOS COLUMNAS)
                    column.Item().PaddingTop(12).Row(info =>
                    {
                        // Izquierda
                        info.RelativeItem().Column(izq =>
                        {
                            izq.Item().Text($"Fecha y Hora de Emisión: {venta.Fecha:dd-MM-yyyy HH:mm:ss}").FontSize(9);
                            izq.Item().Text("Condición de Venta: CONTADO").FontSize(9);
                            izq.Item().Text($"Moneda: {(venta.Moneda?.CodigoISO ?? "PYG")}").FontSize(9);
                            if (venta.CambioDelDia.HasValue && venta.EsMonedaExtranjera == true)
                                izq.Item().Text($"Tipo de Cambio: {venta.CambioDelDia:0.0000}").FontSize(9);
                            izq.Item().Text($"Nº Venta: {venta.IdVenta}").FontSize(9);
                            // Mostrar el Número de Pedido si existe
                            if (!string.IsNullOrWhiteSpace(venta.NroPedido))
                                izq.Item().Text($"Nº Pedido: {venta.NroPedido}").FontSize(9);
                            else
                                izq.Item().Text("Nº Pedido:").FontSize(9);
                        });

                        // Derecha
                        info.RelativeItem().Column(der =>
                        {
                            // Mostrar el Tipo de Documento en negritas
                            var tipoDoc = venta.TipoDocumento ?? "FACTURA";
                            der.Item().Text($"Tipo Doc.: {tipoDoc}").FontSize(9).Bold();
                            
                            var rucCi = string.IsNullOrWhiteSpace(venta.Cliente?.RUC) 
                                ? venta.Cliente?.NumeroDocumento ?? "X-4"
                                : $"{venta.Cliente?.RUC}-{venta.Cliente?.DV}";
                            
                            der.Item().Text($"R.U.C./C.I.: {rucCi}").FontSize(9);
                            der.Item().Text($"Razón Social: {venta.Cliente?.RazonSocial ?? "SIN NOMBRE-CONSUMIDOR FINAL"}")
                                .FontSize(9);
                            der.Item().Text("Teléfono:").FontSize(9);
                            der.Item().Text("Correo Electrónico:").FontSize(9);
                            der.Item().Text($"Plazo Crédito (días): {venta.Plazo ?? 0}").FontSize(9);
                            der.Item().Text("Tipo de Transacción: Mixto").FontSize(9);
                        });
                    });

                    // TABLA DE PRODUCTOS EXACTA COMO EN LA IMAGEN
                    column.Item().PaddingTop(15).Table(table =>
                    {
                        // Definir todas las columnas como en la imagen
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40); // Código
                            columns.RelativeColumn(3); // Descripción
                            columns.ConstantColumn(35); // Unidad Medida
                            columns.ConstantColumn(30); // Cajas
                            columns.ConstantColumn(35); // Cantidad
                            columns.ConstantColumn(45); // Precio Unitario
                            columns.ConstantColumn(40); // Descuento
                            columns.ConstantColumn(45); // Exentas
                            columns.ConstantColumn(35); // 5%
                            columns.ConstantColumn(35); // 10%
                        });

                        // ENCABEZADO COMPLETO
                        table.Header(header =>
                        {
                            // Primera fila - headers principales
                            header.Cell().Border(1).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3)
                                .Padding(2).Text("Código").FontSize(8).Bold().AlignCenter();
                            header.Cell().Border(1).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3)
                                .Padding(2).Text("Descripción del Producto o Servicio").FontSize(8).Bold().AlignCenter();
                            header.Cell().Border(1).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3)
                                .Padding(2).Text("Unidad Medida").FontSize(8).Bold().AlignCenter();
                            header.Cell().Border(1).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3)
                                .Padding(2).Text("Cajas").FontSize(8).Bold().AlignCenter();
                            header.Cell().Border(1).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3)
                                .Padding(2).Text("Cantidad").FontSize(8).Bold().AlignCenter();
                            header.Cell().Border(1).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3)
                                .Padding(2).Text("Precio Unitario").FontSize(8).Bold().AlignCenter();
                            header.Cell().Border(1).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3)
                                .Padding(2).Text("Descuento").FontSize(8).Bold().AlignCenter();

                            // Agrupación "Valor de Venta" - 3 columnas
                            header.Cell().ColumnSpan(3).Border(1).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3)
                                .Padding(2).Text("Valor de Venta").FontSize(8).Bold().AlignCenter();

                            // Segunda fila - sub-headers de "Valor de Venta"
                            for (int i = 0; i < 7; i++)
                                header.Cell().Border(1).BorderColor(Colors.Black).Background(Colors.White)
                                    .Padding(1).Text("").FontSize(6);
                            
                            header.Cell().Border(1).BorderColor(Colors.Black).Background(Colors.Grey.Lighten4)
                                .Padding(1).Text("Exentas").FontSize(7).Bold().AlignCenter();
                            header.Cell().Border(1).BorderColor(Colors.Black).Background(Colors.Grey.Lighten4)
                                .Padding(1).Text("5%").FontSize(7).Bold().AlignCenter();
                            header.Cell().Border(1).BorderColor(Colors.Black).Background(Colors.Grey.Lighten4)
                                .Padding(1).Text("10%").FontSize(7).Bold().AlignCenter();
                        });

                        // FILAS DE DATOS
                        foreach (var d in detalles)
                        {
                            table.Cell().Border(1).BorderColor(Colors.Black).Padding(2)
                                .Text(d.Producto?.CodigoInterno ?? "").FontSize(8).AlignCenter();
                            table.Cell().Border(1).BorderColor(Colors.Black).Padding(2)
                                .Text(d.Producto?.Descripcion ?? "").FontSize(8);
                            table.Cell().Border(1).BorderColor(Colors.Black).Padding(2)
                                .Text("UNI").FontSize(8).AlignCenter();
                            table.Cell().Border(1).BorderColor(Colors.Black).Padding(2)
                                .Text("").FontSize(8).AlignCenter();
                            table.Cell().Border(1).BorderColor(Colors.Black).Padding(2)
                                .Text($"{d.Cantidad:N0}").FontSize(8).AlignCenter();
                            table.Cell().Border(1).BorderColor(Colors.Black).Padding(2)
                                .Text($"{d.PrecioUnitario:N0}").FontSize(8).AlignRight();
                            table.Cell().Border(1).BorderColor(Colors.Black).Padding(2)
                                .Text("").FontSize(8).AlignRight();
                            table.Cell().Border(1).BorderColor(Colors.Black).Padding(2)
                                .Text(d.Exenta > 0 ? $"{d.Exenta:N0}" : "").FontSize(8).AlignRight();
                            table.Cell().Border(1).BorderColor(Colors.Black).Padding(2)
                                .Text(d.Grabado5 > 0 ? $"{d.Grabado5:N0}" : "").FontSize(8).AlignRight();
                            table.Cell().Border(1).BorderColor(Colors.Black).Padding(2)
                                .Text(d.Grabado10 > 0 ? $"{d.Grabado10:N0}" : "").FontSize(8).AlignRight();
                        }
                    });

                    // TOTALES COMO EN LA IMAGEN (DERECHA)
                    var totalExenta = detalles.Sum(x => x.Exenta);
                    var totalGrav5 = detalles.Sum(x => x.Grabado5);
                    var totalGrav10 = detalles.Sum(x => x.Grabado10);
                    var iva5 = detalles.Sum(x => x.IVA5);
                    var iva10 = detalles.Sum(x => x.IVA10);
                    var subtotal = totalExenta + totalGrav5 + totalGrav10;
                    var totalIva = iva5 + iva10;

                    column.Item().PaddingTop(8).Row(row =>
                    {
                        row.RelativeItem(2); // Espacio izquierdo

                        // Totales derecha
                        row.RelativeItem(1).Column(totales =>
                        {
                            totales.Item().Border(1).BorderColor(Colors.Black).Padding(4).Row(r =>
                            {
                                r.RelativeItem().Text($"Sub Total {ObtenerSimboloMoneda(venta.Moneda)}").FontSize(10).Bold();
                                r.ConstantItem(80).Text($"{FormatearPrecio(subtotal, venta.Moneda)}").FontSize(10).Bold().AlignRight();
                            });

                            totales.Item().Border(1).BorderColor(Colors.Black).Padding(4).Row(r =>
                            {
                                r.RelativeItem().Text($"Total I.V.A. {ObtenerSimboloMoneda(venta.Moneda)}").FontSize(10).Bold();
                                r.ConstantItem(80).Text($"{FormatearPrecio(totalIva, venta.Moneda)}").FontSize(10).Bold().AlignRight();
                            });

                            totales.Item().Border(2).BorderColor(Colors.Black).Background(Colors.Red.Lighten4)
                                .Padding(4).Row(r =>
                                {
                                    r.RelativeItem().Text($"TOTAL GENERAL {ObtenerSimboloMoneda(venta.Moneda)}").FontSize(12).Bold();
                                    r.ConstantItem(80).Text($"{FormatearPrecio(venta.Total, venta.Moneda)}").FontSize(12).Bold().AlignRight();
                                });
                        });
                    });

                    // LIQUIDACIÓN DEL IVA - TABLA AZUL COMO EN LA IMAGEN
                    column.Item().PaddingTop(12).Border(1).BorderColor(Colors.Black).Column(liq =>
                    {
                        liq.Item().Background(Colors.Blue.Lighten3).Padding(4)
                            .AlignCenter().Text("LIQUIDACIÓN DEL I.V.A.").FontSize(11).Bold();

                        liq.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.RelativeColumn();
                                c.RelativeColumn();
                            });

                            t.Header(h =>
                            {
                                h.Cell().Border(1).BorderColor(Colors.Black).Background(Colors.Blue.Lighten4)
                                    .Padding(4).AlignCenter().Text("0%").FontSize(10).Bold();
                                h.Cell().Border(1).BorderColor(Colors.Black).Background(Colors.Blue.Lighten4)
                                    .Padding(4).AlignCenter().Text("5%").FontSize(10).Bold();
                                h.Cell().Border(1).BorderColor(Colors.Black).Background(Colors.Blue.Lighten4)
                                    .Padding(4).AlignCenter().Text("10%").FontSize(10).Bold();
                            });

                            t.Cell().Border(1).BorderColor(Colors.Black).Padding(4)
                                .AlignCenter().Text("0").FontSize(10);
                            t.Cell().Border(1).BorderColor(Colors.Black).Padding(4)
                                .AlignCenter().Text("0").FontSize(10);
                            t.Cell().Border(1).BorderColor(Colors.Black).Padding(4)
                                .AlignCenter().Text($"{iva10:N0}").FontSize(10);
                        });
                    });

                    // QR Y CDC PROMINENTE COMO EN LA IMAGEN (solo para facturas electrónicas)
                    if (esFacturaElectronica && !string.IsNullOrWhiteSpace(venta.CDC) && qrPng != null)
                    {
                        column.Item().PaddingTop(15).Row(row =>
                        {
                            // QR grande a la izquierda
                            row.ConstantItem(50, Unit.Millimetre).Column(qrCol =>
                            {
                                qrCol.Item().Width(45, Unit.Millimetre).Height(45, Unit.Millimetre)
                                    .Border(1).BorderColor(Colors.Black)
                                    .Image(qrPng).FitArea();
                            });

                            // Información CDC
                            row.RelativeItem().PaddingLeft(8).Column(infoCol =>
                            {
                                infoCol.Item().Text("Consulte la validez de esta Factura Electrónica con el número de CDC impreso abajo en:")
                                    .FontSize(9).Bold();
                                infoCol.Item().PaddingTop(4).Text("https://ekuatia.set.gov.py/consultas/")
                                    .FontSize(10).Bold().FontColor(Colors.Blue.Darken2);

                                // CDC destacado
                                infoCol.Item().PaddingTop(8).Border(2).BorderColor(Colors.Black)
                                    .Background(Colors.Grey.Lighten4).Padding(6)
                                    .Text($"CDC: {FormatearCdc(venta.CDC!)}")
                                    .FontSize(12).Bold().FontColor(Colors.Black);

                                infoCol.Item().PaddingTop(8).Text("ESTE DOCUMENTO ES UNA REPRESENTACIÓN GRÁFICA DE UN DOCUMENTO ELECTRÓNICO (XML)")
                                    .FontSize(9).Bold().FontColor(Colors.Red.Darken1);

                                infoCol.Item().PaddingTop(4).Text("Información de interés del facturador electrónico emisor:")
                                    .FontSize(8);
                                infoCol.Item().Text("Si su documento electrónico presenta algún error, podrá solicitar la modificación dentro de las 72 horas siguientes de la emisión de este comprobante.")
                                    .FontSize(8);
                            });
                        });
                    }
                });
            });
        });

        return documento.GeneratePdf();
    }

    private static string FormatearCdc(string cdc)
    {
        if (string.IsNullOrWhiteSpace(cdc) || cdc.Length < 16)
            return cdc;

        return string.Join(" ", Enumerable.Range(0, (cdc.Length + 3) / 4)
            .Select(i => cdc.Substring(i * 4, Math.Min(4, cdc.Length - i * 4))));
    }
}