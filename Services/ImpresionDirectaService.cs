using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Runtime.Versioning;

namespace SistemIA.Services;

/// <summary>
/// Servicio de impresión directa usando Windows PrintDocument.
/// Permite imprimir en cualquier impresora instalada en Windows sin mostrar diálogo.
/// </summary>
[SupportedOSPlatform("windows")]
public class ImpresionDirectaService
{
    private readonly ILogger<ImpresionDirectaService> _logger;

    public ImpresionDirectaService(ILogger<ImpresionDirectaService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Obtiene la lista de impresoras instaladas en el sistema.
    /// </summary>
    public List<string> ObtenerImpresoras()
    {
        var impresoras = new List<string>();
        foreach (string printer in PrinterSettings.InstalledPrinters)
        {
            impresoras.Add(printer);
        }
        return impresoras;
    }

    /// <summary>
    /// Obtiene la impresora predeterminada del sistema.
    /// </summary>
    public string? ObtenerImpresoraPredeterminada()
    {
        var settings = new PrinterSettings();
        return settings.PrinterName;
    }

    /// <summary>
    /// Imprime un ticket de venta en formato de 80mm.
    /// </summary>
    public async Task<ResultadoImpresion> ImprimirTicket80mm(DatosTicket ticket, string? nombreImpresora = null)
    {
        return await Task.Run(() =>
        {
            try
            {
                var printDoc = new PrintDocument();
                
                // Configurar impresora
                if (!string.IsNullOrEmpty(nombreImpresora))
                {
                    printDoc.PrinterSettings.PrinterName = nombreImpresora;
                    if (!printDoc.PrinterSettings.IsValid)
                    {
                        return new ResultadoImpresion
                        {
                            Exitoso = false,
                            Mensaje = $"Impresora '{nombreImpresora}' no encontrada"
                        };
                    }
                }

                printDoc.DocumentName = $"Ticket-{ticket.NumeroFactura}";
                
                // Configurar página para 80mm (aproximadamente 283 puntos a 90 DPI)
                printDoc.DefaultPageSettings.PaperSize = new PaperSize("Ticket80mm", 283, 0);
                printDoc.DefaultPageSettings.Margins = new Margins(5, 5, 5, 5);

                printDoc.PrintPage += (sender, e) => DibujarTicket80mm(e, ticket);
                
                printDoc.Print();
                
                _logger.LogInformation("Ticket {NumeroFactura} impreso correctamente en {Impresora}", 
                    ticket.NumeroFactura, printDoc.PrinterSettings.PrinterName);
                
                return new ResultadoImpresion
                {
                    Exitoso = true,
                    Mensaje = $"Impreso en {printDoc.PrinterSettings.PrinterName}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al imprimir ticket {NumeroFactura}", ticket.NumeroFactura);
                return new ResultadoImpresion
                {
                    Exitoso = false,
                    Mensaje = ex.Message
                };
            }
        });
    }

    /// <summary>
    /// Imprime una factura en formato A4.
    /// </summary>
    public async Task<ResultadoImpresion> ImprimirFacturaA4(DatosTicket ticket, string? nombreImpresora = null)
    {
        return await Task.Run(() =>
        {
            try
            {
                var printDoc = new PrintDocument();
                
                if (!string.IsNullOrEmpty(nombreImpresora))
                {
                    printDoc.PrinterSettings.PrinterName = nombreImpresora;
                    if (!printDoc.PrinterSettings.IsValid)
                    {
                        return new ResultadoImpresion
                        {
                            Exitoso = false,
                            Mensaje = $"Impresora '{nombreImpresora}' no encontrada"
                        };
                    }
                }

                printDoc.DocumentName = $"Factura-{ticket.NumeroFactura}";
                printDoc.DefaultPageSettings.PaperSize = new PaperSize("A4", 827, 1169); // A4 en centésimas de pulgada
                printDoc.DefaultPageSettings.Margins = new Margins(50, 50, 50, 50);

                printDoc.PrintPage += (sender, e) => DibujarFacturaA4(e, ticket);
                
                printDoc.Print();
                
                _logger.LogInformation("Factura A4 {NumeroFactura} impresa en {Impresora}", 
                    ticket.NumeroFactura, printDoc.PrinterSettings.PrinterName);
                
                return new ResultadoImpresion
                {
                    Exitoso = true,
                    Mensaje = $"Impreso en {printDoc.PrinterSettings.PrinterName}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al imprimir factura A4 {NumeroFactura}", ticket.NumeroFactura);
                return new ResultadoImpresion
                {
                    Exitoso = false,
                    Mensaje = ex.Message
                };
            }
        });
    }

    private void DibujarTicket80mm(PrintPageEventArgs e, DatosTicket ticket)
    {
        var g = e.Graphics!;
        
        // Fuentes optimizadas para ticket 80mm (tamaños aumentados para mejor legibilidad)
        var fuenteTitulo = new Font("Arial", 10, FontStyle.Bold);
        var fuenteSubtitulo = new Font("Arial", 8, FontStyle.Bold);
        var fuenteNormal = new Font("Arial", 7.5f);
        var fuentePequeña = new Font("Arial", 6.5f);
        var fuenteTotal = new Font("Arial", 10, FontStyle.Bold);
        var fuenteEncabezado = new Font("Arial", 6f, FontStyle.Bold);
        
        float y = 5;
        float anchoTicket = e.MarginBounds.Width;
        var formatoCentro = new StringFormat { Alignment = StringAlignment.Center };
        var formatoDerecha = new StringFormat { Alignment = StringAlignment.Far };
        
        // Posiciones de columnas para el detalle (proporcional al ancho)
        float colCant = 0;
        float colDesc = anchoTicket * 0.10f;
        float colPrecio = anchoTicket * 0.42f;
        float colExenta = anchoTicket * 0.58f;
        float col5 = anchoTicket * 0.74f;
        float col10 = anchoTicket * 0.87f;
        
        // === LOGO (si existe) ===
        if (ticket.LogoBytes != null && ticket.LogoBytes.Length > 0)
        {
            try
            {
                using var ms = new MemoryStream(ticket.LogoBytes);
                using var logo = Image.FromStream(ms);
                
                // Calcular tamaño proporcional (máximo 160px ancho, 70px alto - ampliado)
                float maxAncho = 160;
                float maxAlto = 70;
                float escala = Math.Min(maxAncho / logo.Width, maxAlto / logo.Height);
                float logoAncho = logo.Width * escala;
                float logoAlto = logo.Height * escala;
                
                // Centrar el logo
                float logoX = (anchoTicket - logoAncho) / 2;
                g.DrawImage(logo, logoX, y, logoAncho, logoAlto);
                y += logoAlto + 10;
            }
            catch
            {
                // Si falla el logo, continuar sin él
            }
        }
        
        // === ENCABEZADO EMPRESA ===
        g.DrawString(ticket.NombreEmpresa, fuenteTitulo, Brushes.Black, anchoTicket / 2, y, formatoCentro);
        y += 16;
        
        g.DrawString($"RUC: {ticket.RucEmpresa}", fuenteSubtitulo, Brushes.Black, anchoTicket / 2, y, formatoCentro);
        y += 14;
        
        if (!string.IsNullOrEmpty(ticket.DireccionEmpresa))
        {
            g.DrawString(ticket.DireccionEmpresa, fuentePequeña, Brushes.Black, anchoTicket / 2, y, formatoCentro);
            y += 12;
        }
        
        if (!string.IsNullOrEmpty(ticket.TelefonoEmpresa))
        {
            g.DrawString($"Tel: {ticket.TelefonoEmpresa}", fuentePequeña, Brushes.Black, anchoTicket / 2, y, formatoCentro);
            y += 12;
        }
        
        if (!string.IsNullOrEmpty(ticket.ActividadEconomica))
        {
            g.DrawString(ticket.ActividadEconomica, fuentePequeña, Brushes.Black, anchoTicket / 2, y, formatoCentro);
            y += 12;
        }
        
        // === LÍNEA DOBLE ===
        y += 4;
        g.DrawLine(Pens.Black, 0, y, anchoTicket, y);
        g.DrawLine(Pens.Black, 0, y + 2, anchoTicket, y + 2);
        y += 8;
        
        // === TIPO DOCUMENTO (recuadro) ===
        // Mostrar el tipo de documento real: FACTURA, PRESUPUESTO, etc.
        var tipoDoc = ticket.TipoDocumento?.ToUpper() ?? "FACTURA";
        if (ticket.EsElectronica && tipoDoc == "FACTURA")
        {
            tipoDoc = "FACTURA ELECTRÓNICA";
        }
        var rectTipo = new RectangleF(10, y, anchoTicket - 20, 18);
        g.DrawRectangle(Pens.Black, rectTipo.X, rectTipo.Y, rectTipo.Width, rectTipo.Height);
        g.DrawString(tipoDoc, fuenteSubtitulo, Brushes.Black, rectTipo.X + rectTipo.Width / 2, y + 3, formatoCentro);
        y += 24;
        
        // === LÍNEA DOBLE ===
        g.DrawLine(Pens.Black, 0, y, anchoTicket, y);
        g.DrawLine(Pens.Black, 0, y + 2, anchoTicket, y + 2);
        y += 10;
        
        // === DATOS DEL TIMBRADO ===
        DibujarFilaDatos(g, "Timbrado:", ticket.Timbrado, fuenteNormal, 0, anchoTicket * 0.55f, y);
        y += 14;
        
        if (ticket.VigenciaDel.HasValue)
        {
            DibujarFilaDatos(g, "Fecha Inicio Vigencia:", ticket.VigenciaDel.Value.ToString("dd/MM/yyyy"), fuentePequeña, 0, anchoTicket * 0.55f, y);
            y += 12;
        }
        
        if (ticket.VigenciaAl.HasValue)
        {
            DibujarFilaDatos(g, "Válido Hasta:", ticket.VigenciaAl.Value.ToString("dd/MM/yyyy"), fuentePequeña, 0, anchoTicket * 0.55f, y);
            y += 12;
        }
        
        // Línea simple
        y += 2;
        g.DrawLine(Pens.Black, 0, y, anchoTicket, y);
        y += 8;
        
        // === NÚMERO DE FACTURA Y FECHA ===
        DibujarFilaDatos(g, "Factura Nro:", ticket.NumeroFactura, fuenteSubtitulo, 0, anchoTicket * 0.45f, y);
        y += 14;
        
        // Fecha de la venta + hora actual del equipo
        var fechaEmision = ticket.Fecha.ToString("dd/MM/yyyy");
        var horaActual = DateTime.Now.ToString("HH:mm");
        DibujarFilaDatos(g, "Fecha de Emisión:", $"{fechaEmision} {horaActual}", fuenteNormal, 0, anchoTicket * 0.45f, y);
        y += 16;
        
        // === LÍNEA DOBLE ===
        g.DrawLine(Pens.Black, 0, y, anchoTicket, y);
        g.DrawLine(Pens.Black, 0, y + 2, anchoTicket, y + 2);
        y += 10;
        
        // === DATOS CLIENTE ===
        g.DrawString("Cliente:", fuenteNormal, Brushes.Black, 0, y);
        y += 12;
        
        // Nombre del cliente - adaptable a múltiples líneas si es largo
        var nombreCliente = ticket.NombreCliente ?? "SIN NOMBRE";
        var anchoDisponible = anchoTicket - 10;
        var lineasNombre = DividirTextoEnLineas(g, nombreCliente, fuenteNormal, anchoDisponible);
        foreach (var linea in lineasNombre)
        {
            g.DrawString(linea, fuenteNormal, Brushes.Black, 5, y);
            y += 12;
        }
        y += 2;
        
        // RUC/CI con DV si existe
        var rucCompleto = !string.IsNullOrEmpty(ticket.DvCliente) 
            ? $"{ticket.RucCliente}-{ticket.DvCliente}" 
            : ticket.RucCliente;
        DibujarFilaDatos(g, "RUC/CI:", rucCompleto, fuenteNormal, 0, anchoTicket * 0.25f, y);
        y += 14;
        
        // Línea simple
        y += 2;
        g.DrawLine(Pens.Black, 0, y, anchoTicket, y);
        y += 8;
        
        // === CONDICIÓN DE VENTA ===
        DibujarFilaDatos(g, "Condición de Venta:", ticket.CondicionVenta?.ToUpper() ?? "CONTADO", fuenteNormal, 0, anchoTicket * 0.50f, y);
        y += 16;
        
        // === LÍNEA DOBLE ===
        g.DrawLine(Pens.Black, 0, y, anchoTicket, y);
        g.DrawLine(Pens.Black, 0, y + 2, anchoTicket, y + 2);
        y += 10;
        
        // === ENCABEZADO DE DETALLE ===
        g.DrawString("CANT.", fuenteEncabezado, Brushes.Black, colCant, y);
        g.DrawString("DESCRIPCIÓN", fuenteEncabezado, Brushes.Black, colDesc, y);
        g.DrawString("P.UNIT.", fuenteEncabezado, Brushes.Black, colPrecio, y);
        g.DrawString("EXENTA", fuenteEncabezado, Brushes.Black, colExenta, y);
        g.DrawString("5%", fuenteEncabezado, Brushes.Black, col5, y);
        g.DrawString("10%", fuenteEncabezado, Brushes.Black, col10, y);
        y += 12;
        
        // Línea bajo encabezado
        g.DrawLine(Pens.Black, 0, y, anchoTicket, y);
        y += 5;
        
        // === DETALLE DE PRODUCTOS ===
        foreach (var item in ticket.Items)
        {
            // Cantidad: sin decimales si es entero, sino con 2 decimales
            var cantidadStr = item.Cantidad == Math.Truncate(item.Cantidad) 
                ? item.Cantidad.ToString("N0") 
                : item.Cantidad.ToString("N2");
            g.DrawString(cantidadStr, fuentePequeña, Brushes.Black, colCant, y);
            g.DrawString(TruncarTexto(item.Descripcion, 14), fuentePequeña, Brushes.Black, colDesc, y);
            g.DrawString(FormatearNumero(item.PrecioUnitario), fuentePequeña, Brushes.Black, colPrecio, y);
            g.DrawString(item.Exenta > 0 ? FormatearNumero(item.Exenta) : "-", fuentePequeña, Brushes.Black, colExenta, y);
            g.DrawString(item.Gravado5 > 0 ? FormatearNumero(item.Gravado5) : "-", fuentePequeña, Brushes.Black, col5, y);
            g.DrawString(item.Gravado10 > 0 ? FormatearNumero(item.Gravado10) : "-", fuentePequeña, Brushes.Black, col10, y);
            y += 12;
        }
        
        // === LÍNEA DOBLE ===
        y += 4;
        g.DrawLine(Pens.Black, 0, y, anchoTicket, y);
        g.DrawLine(Pens.Black, 0, y + 2, anchoTicket, y + 2);
        y += 10;
        
        // === SUB-TOTALES ===
        g.DrawString("SUB-TOTALES:", fuenteNormal, Brushes.Black, 0, y);
        g.DrawString(ticket.TotalExentas > 0 ? FormatearNumero(ticket.TotalExentas) : "-", fuentePequeña, Brushes.Black, colExenta, y);
        g.DrawString(ticket.TotalIva5 > 0 ? FormatearNumero(ticket.TotalIva5) : "-", fuentePequeña, Brushes.Black, col5, y);
        g.DrawString(ticket.TotalIva10 > 0 ? FormatearNumero(ticket.TotalIva10) : "-", fuentePequeña, Brushes.Black, col10, y);
        y += 14;
        
        // Línea simple
        g.DrawLine(Pens.Black, 0, y, anchoTicket, y);
        y += 8;
        
        // === TOTAL EN RECUADRO NEGRO ===
        g.FillRectangle(Brushes.Black, 0, y, anchoTicket, 20);
        g.DrawString("TOTAL A PAGAR:", fuenteTotal, Brushes.White, 5, y + 3);
        g.DrawString($"Gs. {FormatearNumero(ticket.Total)}", fuenteTotal, Brushes.White, anchoTicket - 5, y + 3, formatoDerecha);
        y += 26;
        
        // === LÍNEA DOBLE ===
        g.DrawLine(Pens.Black, 0, y, anchoTicket, y);
        g.DrawLine(Pens.Black, 0, y + 2, anchoTicket, y + 2);
        y += 10;
        
        // === LIQUIDACIÓN IVA (en formato de columnas como la vista previa) ===
        g.DrawString("LIQUIDACIÓN DEL IVA:", fuenteSubtitulo, Brushes.Black, 0, y);
        y += 16;
        
        // Encabezados de columna
        float colIvaLabel = 0;
        float colIvaBase = anchoTicket * 0.40f;
        float colIvaMonto = anchoTicket * 0.70f;
        
        g.DrawString("CONCEPTO", fuenteEncabezado, Brushes.Black, colIvaLabel, y);
        g.DrawString("GRAVADA", fuenteEncabezado, Brushes.Black, colIvaBase, y);
        g.DrawString("IVA", fuenteEncabezado, Brushes.Black, colIvaMonto, y);
        y += 12;
        
        g.DrawLine(Pens.Black, 0, y, anchoTicket, y);
        y += 6;
        
        // Exentas
        g.DrawString("Exentas:", fuentePequeña, Brushes.Black, colIvaLabel, y);
        g.DrawString(ticket.TotalExentas > 0 ? FormatearNumero(ticket.TotalExentas) : "-", fuentePequeña, Brushes.Black, colIvaBase, y);
        g.DrawString("-", fuentePequeña, Brushes.Black, colIvaMonto, y);
        y += 12;
        
        // IVA 5%
        g.DrawString("IVA 5%:", fuentePequeña, Brushes.Black, colIvaLabel, y);
        g.DrawString(ticket.TotalIva5 > 0 ? FormatearNumero(ticket.TotalIva5) : "-", fuentePequeña, Brushes.Black, colIvaBase, y);
        g.DrawString(ticket.MontoIva5 > 0 ? FormatearNumero(ticket.MontoIva5) : "-", fuentePequeña, Brushes.Black, colIvaMonto, y);
        y += 12;
        
        // IVA 10%
        g.DrawString("IVA 10%:", fuentePequeña, Brushes.Black, colIvaLabel, y);
        g.DrawString(ticket.TotalIva10 > 0 ? FormatearNumero(ticket.TotalIva10) : "-", fuentePequeña, Brushes.Black, colIvaBase, y);
        g.DrawString(ticket.MontoIva10 > 0 ? FormatearNumero(ticket.MontoIva10) : "-", fuentePequeña, Brushes.Black, colIvaMonto, y);
        y += 12;
        
        // Total IVA
        g.DrawLine(Pens.Black, colIvaMonto - 10, y, anchoTicket, y);
        y += 6;
        g.DrawString("Total IVA:", fuenteNormal, Brushes.Black, colIvaLabel, y);
        g.DrawString(FormatearNumero(ticket.TotalIva), fuenteNormal, Brushes.Black, colIvaMonto, y);
        y += 16;
        
        // === CDC (si existe) ===
        if (!string.IsNullOrEmpty(ticket.CDC))
        {
            g.DrawLine(Pens.Black, 0, y, anchoTicket, y);
            y += 6;
            g.DrawString("CDC:", fuentePequeña, Brushes.Black, 0, y);
            y += 10;
            
            // El CDC es largo, lo dividimos en partes
            for (int i = 0; i < ticket.CDC.Length; i += 26)
            {
                var parte = ticket.CDC.Substring(i, Math.Min(26, ticket.CDC.Length - i));
                g.DrawString(parte, fuentePequeña, Brushes.Black, anchoTicket / 2, y, formatoCentro);
                y += 10;
            }
        }
        
        // === PIE DE PÁGINA ===
        y += 6;
        g.DrawLine(Pens.Black, 0, y, anchoTicket, y);
        g.DrawLine(Pens.Black, 0, y + 2, anchoTicket, y + 2);
        y += 12;
        
        g.DrawString("¡Gracias por su compra!", fuenteSubtitulo, Brushes.Black, anchoTicket / 2, y, formatoCentro);
        y += 16;
        
        g.DrawString("SistemIA", fuentePequeña, Brushes.Black, anchoTicket / 2, y, formatoCentro);
        y += 15;
        
        // Establecer el tamaño real de la página
        e.HasMorePages = false;
    }
    
    /// <summary>
    /// Dibuja una fila de datos con etiqueta y valor.
    /// </summary>
    private void DibujarFilaDatos(Graphics g, string etiqueta, string valor, Font fuente, float xLabel, float xValor, float y)
    {
        g.DrawString(etiqueta, fuente, Brushes.Black, xLabel, y);
        g.DrawString(valor ?? "-", fuente, Brushes.Black, xValor, y);
    }
    
    /// <summary>
    /// Trunca el texto si excede el largo máximo.
    /// </summary>
    private string TruncarTexto(string? texto, int maxLargo)
    {
        if (string.IsNullOrEmpty(texto)) return "";
        return texto.Length > maxLargo ? texto.Substring(0, maxLargo - 2) + ".." : texto;
    }
    
    /// <summary>
    /// Formatea un número sin decimales con separador de miles.
    /// </summary>
    private string FormatearNumero(decimal valor)
    {
        return valor.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("es-PY"));
    }
    
    /// <summary>
    /// Divide un texto largo en múltiples líneas según el ancho disponible.
    /// </summary>
    private List<string> DividirTextoEnLineas(Graphics g, string texto, Font fuente, float anchoMaximo)
    {
        var lineas = new List<string>();
        if (string.IsNullOrEmpty(texto))
        {
            lineas.Add("");
            return lineas;
        }
        
        var palabras = texto.Split(' ');
        var lineaActual = "";
        
        foreach (var palabra in palabras)
        {
            var pruebaLinea = string.IsNullOrEmpty(lineaActual) ? palabra : lineaActual + " " + palabra;
            var tamanio = g.MeasureString(pruebaLinea, fuente);
            
            if (tamanio.Width > anchoMaximo && !string.IsNullOrEmpty(lineaActual))
            {
                lineas.Add(lineaActual);
                lineaActual = palabra;
            }
            else
            {
                lineaActual = pruebaLinea;
            }
        }
        
        if (!string.IsNullOrEmpty(lineaActual))
        {
            lineas.Add(lineaActual);
        }
        
        // Máximo 3 líneas para no ocupar demasiado espacio
        if (lineas.Count > 3)
        {
            lineas = lineas.Take(3).ToList();
            lineas[2] = TruncarTexto(lineas[2], 30) + "...";
        }
        
        return lineas;
    }

    private void DibujarFacturaA4(PrintPageEventArgs e, DatosTicket ticket)
    {
        var g = e.Graphics!;
        var bounds = e.MarginBounds;
        
        // Fuentes para A4 (más grandes)
        var fuenteTitulo = new Font("Arial", 14, FontStyle.Bold);
        var fuenteSubtitulo = new Font("Arial", 11, FontStyle.Bold);
        var fuenteNormal = new Font("Arial", 10);
        var fuentePequeña = new Font("Arial", 9);
        var fuenteTotal = new Font("Arial", 12, FontStyle.Bold);
        
        float y = bounds.Top;
        var formatoCentro = new StringFormat { Alignment = StringAlignment.Center };
        var formatoDerecha = new StringFormat { Alignment = StringAlignment.Far };
        
        // === ENCABEZADO ===
        g.DrawString(ticket.NombreEmpresa, fuenteTitulo, Brushes.Black, bounds.Width / 2 + bounds.Left, y, formatoCentro);
        y += 22;
        
        g.DrawString(ticket.RucEmpresa, fuenteSubtitulo, Brushes.Black, bounds.Width / 2 + bounds.Left, y, formatoCentro);
        y += 18;
        
        g.DrawString(ticket.DireccionEmpresa, fuenteNormal, Brushes.Black, bounds.Width / 2 + bounds.Left, y, formatoCentro);
        y += 16;
        
        g.DrawString($"Tel: {ticket.TelefonoEmpresa}", fuenteNormal, Brushes.Black, bounds.Width / 2 + bounds.Left, y, formatoCentro);
        y += 25;
        
        // Línea
        g.DrawLine(Pens.Black, bounds.Left, y, bounds.Right, y);
        y += 15;
        
        // === DATOS FACTURA (derecha) ===
        g.DrawString("FACTURA ELECTRÓNICA", fuenteTitulo, Brushes.Black, bounds.Right, y, formatoDerecha);
        y += 22;
        
        g.DrawString($"Nro: {ticket.NumeroFactura}", fuenteSubtitulo, Brushes.Black, bounds.Right, y, formatoDerecha);
        y += 18;
        
        g.DrawString($"Fecha: {ticket.Fecha:dd/MM/yyyy HH:mm}", fuenteNormal, Brushes.Black, bounds.Right, y, formatoDerecha);
        y += 16;
        
        g.DrawString($"Timbrado: {ticket.Timbrado}", fuenteNormal, Brushes.Black, bounds.Right, y, formatoDerecha);
        y += 25;
        
        // === DATOS CLIENTE ===
        g.DrawString("DATOS DEL CLIENTE:", fuenteSubtitulo, Brushes.Black, bounds.Left, y);
        y += 18;
        
        g.DrawString($"Nombre: {ticket.NombreCliente}", fuenteNormal, Brushes.Black, bounds.Left, y);
        y += 16;
        
        g.DrawString($"RUC/CI: {ticket.RucCliente}", fuenteNormal, Brushes.Black, bounds.Left, y);
        y += 16;
        
        if (!string.IsNullOrEmpty(ticket.DireccionCliente))
        {
            g.DrawString($"Dirección: {ticket.DireccionCliente}", fuenteNormal, Brushes.Black, bounds.Left, y);
            y += 16;
        }
        y += 15;
        
        // Línea
        g.DrawLine(Pens.Black, bounds.Left, y, bounds.Right, y);
        y += 10;
        
        // === CABECERA TABLA ===
        var colCant = bounds.Left;
        var colDesc = bounds.Left + 60;
        var colPrecio = bounds.Left + 400;
        var colSubtotal = bounds.Right - 80;
        
        g.FillRectangle(Brushes.LightGray, bounds.Left, y, bounds.Width, 20);
        g.DrawString("Cant.", fuenteSubtitulo, Brushes.Black, colCant, y + 3);
        g.DrawString("Descripción", fuenteSubtitulo, Brushes.Black, colDesc, y + 3);
        g.DrawString("P. Unit.", fuenteSubtitulo, Brushes.Black, colPrecio, y + 3);
        g.DrawString("Subtotal", fuenteSubtitulo, Brushes.Black, colSubtotal, y + 3);
        y += 25;
        
        // === DETALLE ===
        foreach (var item in ticket.Items)
        {
            g.DrawString($"{item.Cantidad}", fuenteNormal, Brushes.Black, colCant, y);
            g.DrawString(item.Descripcion, fuenteNormal, Brushes.Black, colDesc, y);
            g.DrawString($"{item.PrecioUnitario:N0}", fuenteNormal, Brushes.Black, colPrecio, y);
            g.DrawString($"{item.Subtotal:N0}", fuenteNormal, Brushes.Black, colSubtotal, y);
            y += 18;
        }
        
        y += 10;
        g.DrawLine(Pens.Black, bounds.Left, y, bounds.Right, y);
        y += 15;
        
        // === TOTALES ===
        g.DrawString("Subtotal:", fuenteNormal, Brushes.Black, colPrecio, y);
        g.DrawString($"Gs. {ticket.Subtotal:N0}", fuenteNormal, Brushes.Black, bounds.Right, y, formatoDerecha);
        y += 18;
        
        if (ticket.Descuento > 0)
        {
            g.DrawString("Descuento:", fuenteNormal, Brushes.Black, colPrecio, y);
            g.DrawString($"-Gs. {ticket.Descuento:N0}", fuenteNormal, Brushes.Black, bounds.Right, y, formatoDerecha);
            y += 18;
        }
        
        // Total
        y += 5;
        g.FillRectangle(Brushes.Black, colPrecio - 10, y - 3, bounds.Right - colPrecio + 10, 25);
        g.DrawString("TOTAL:", fuenteTotal, Brushes.White, colPrecio, y);
        g.DrawString($"Gs. {ticket.Total:N0}", fuenteTotal, Brushes.White, bounds.Right - 5, y, formatoDerecha);
        y += 35;
        
        // === LIQUIDACIÓN IVA ===
        g.DrawString("Liquidación del IVA:", fuenteSubtitulo, Brushes.Black, bounds.Left, y);
        y += 18;
        
        var ivaInfo = new List<string>();
        if (ticket.TotalExentas > 0)
            ivaInfo.Add($"Exentas: Gs. {ticket.TotalExentas:N0}");
        if (ticket.TotalIva5 > 0)
            ivaInfo.Add($"Gravadas 5%: Gs. {ticket.TotalIva5:N0} (IVA: Gs. {ticket.MontoIva5:N0})");
        if (ticket.TotalIva10 > 0)
            ivaInfo.Add($"Gravadas 10%: Gs. {ticket.TotalIva10:N0} (IVA: Gs. {ticket.MontoIva10:N0})");
        ivaInfo.Add($"Total IVA: Gs. {ticket.TotalIva:N0}");
        
        foreach (var info in ivaInfo)
        {
            g.DrawString(info, fuenteNormal, Brushes.Black, bounds.Left + 20, y);
            y += 16;
        }
        
        y += 15;
        
        // === CONDICIÓN Y CDC ===
        g.DrawString($"Condición de venta: {ticket.CondicionVenta}", fuenteNormal, Brushes.Black, bounds.Left, y);
        y += 20;
        
        if (!string.IsNullOrEmpty(ticket.CDC))
        {
            g.DrawString($"CDC: {ticket.CDC}", fuentePequeña, Brushes.Black, bounds.Left, y);
        }
        
        // === PIE DE PÁGINA ===
        var piePagina = bounds.Bottom - 30;
        g.DrawLine(Pens.Gray, bounds.Left, piePagina - 10, bounds.Right, piePagina - 10);
        g.DrawString("Documento generado por SistemIA", fuentePequeña, Brushes.Gray, bounds.Width / 2 + bounds.Left, piePagina, formatoCentro);
        
        e.HasMorePages = false;
    }
}

/// <summary>
/// Datos necesarios para imprimir un ticket.
/// </summary>
public class DatosTicket
{
    // Logo (ruta del archivo o base64)
    public string? LogoPath { get; set; }
    public byte[]? LogoBytes { get; set; }
    
    // Datos de la empresa
    public string NombreEmpresa { get; set; } = "";
    public string RucEmpresa { get; set; } = "";
    public string DireccionEmpresa { get; set; } = "";
    public string TelefonoEmpresa { get; set; } = "";
    public string ActividadEconomica { get; set; } = "";
    
    // Datos de la factura
    public string NumeroFactura { get; set; } = "";
    public DateTime Fecha { get; set; }
    public string Timbrado { get; set; } = "";
    public DateTime? VigenciaDel { get; set; }
    public DateTime? VigenciaAl { get; set; }
    public string CDC { get; set; } = "";
    public string CondicionVenta { get; set; } = "Contado";
    public string TipoDocumento { get; set; } = "FACTURA";
    public bool EsElectronica { get; set; } = true;
    
    // Datos del cliente
    public string NombreCliente { get; set; } = "";
    public string RucCliente { get; set; } = "";
    public string? DvCliente { get; set; }
    public string DireccionCliente { get; set; } = "";
    
    // Items
    public List<ItemTicket> Items { get; set; } = new();
    
    // Totales
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal Total { get; set; }
    
    // IVA
    public decimal TotalExentas { get; set; }
    public decimal TotalIva5 { get; set; }
    public decimal TotalIva10 { get; set; }
    public decimal MontoIva5 { get; set; }
    public decimal MontoIva10 { get; set; }
    public decimal TotalIva { get; set; }
}

public class ItemTicket
{
    public string Descripcion { get; set; } = "";
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Exenta { get; set; }
    public decimal Gravado5 { get; set; }
    public decimal Gravado10 { get; set; }
}

public class ResultadoImpresion
{
    public bool Exitoso { get; set; }
    public string Mensaje { get; set; } = "";
}
