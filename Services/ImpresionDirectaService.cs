using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Runtime.Versioning;
using QRCoder;

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

    // ========== IMPRESIÓN TICKET DE PEDIDO (RESTAURANTE) ==========

    /// <summary>
    /// Imprime un ticket de pedido/comprobante para el cliente en formato 80mm.
    /// </summary>
    public async Task<ResultadoImpresion> ImprimirTicketPedido80mm(DatosPedidoTicket pedido, string? nombreImpresora = null)
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

                printDoc.DocumentName = $"Pedido-{pedido.NumeroPedido}";
                printDoc.DefaultPageSettings.PaperSize = new PaperSize("Ticket80mm", 283, 0);
                printDoc.DefaultPageSettings.Margins = new Margins(5, 5, 5, 5);

                printDoc.PrintPage += (sender, e) => DibujarTicketPedido80mm(e, pedido);
                
                printDoc.Print();
                
                _logger.LogInformation("Ticket pedido #{NumeroPedido} impreso en {Impresora}", 
                    pedido.NumeroPedido, printDoc.PrinterSettings.PrinterName);
                
                return new ResultadoImpresion
                {
                    Exitoso = true,
                    Mensaje = $"Impreso en {printDoc.PrinterSettings.PrinterName}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al imprimir ticket pedido #{NumeroPedido}", pedido.NumeroPedido);
                return new ResultadoImpresion
                {
                    Exitoso = false,
                    Mensaje = ex.Message
                };
            }
        });
    }

    /// <summary>
    /// Dibuja el ticket de pedido para el cliente.
    /// </summary>
    private void DibujarTicketPedido80mm(PrintPageEventArgs e, DatosPedidoTicket pedido)
    {
        var g = e.Graphics!;
        
        var fuenteTitulo = new Font("Arial", 11, FontStyle.Bold);
        var fuenteSubtitulo = new Font("Arial", 9, FontStyle.Bold);
        var fuenteNormal = new Font("Arial", 8);
        var fuentePequeña = new Font("Arial", 7);
        var fuenteTotal = new Font("Arial", 11, FontStyle.Bold);
        var fuenteMesa = new Font("Arial", 14, FontStyle.Bold);
        
        float y = 5;
        float anchoTicket = e.MarginBounds.Width;
        var formatoCentro = new StringFormat { Alignment = StringAlignment.Center };
        var formatoDerecha = new StringFormat { Alignment = StringAlignment.Far };
        
        // === LOGO (si existe) ===
        if (pedido.LogoBytes != null && pedido.LogoBytes.Length > 0)
        {
            try
            {
                using var ms = new MemoryStream(pedido.LogoBytes);
                using var logo = Image.FromStream(ms);
                
                float maxAncho = 140;
                float maxAlto = 60;
                float escala = Math.Min(maxAncho / logo.Width, maxAlto / logo.Height);
                float logoAncho = logo.Width * escala;
                float logoAlto = logo.Height * escala;
                
                float logoX = (anchoTicket - logoAncho) / 2;
                g.DrawImage(logo, logoX, y, logoAncho, logoAlto);
                y += logoAlto + 8;
            }
            catch { /* ignorar error de logo */ }
        }

        // === ENCABEZADO ===
        if (!string.IsNullOrEmpty(pedido.NombreEmpresa))
        {
            g.DrawString(pedido.NombreEmpresa, fuenteTitulo, Brushes.Black, anchoTicket / 2, y, formatoCentro);
            y += 16;
        }
        
        // Tipo de documento
        var tipoDoc = pedido.EsComanda ? $"*** COMANDA {pedido.DestinoComanda} ***" : "COMPROBANTE DE PEDIDO";
        g.DrawString(tipoDoc, fuenteSubtitulo, Brushes.Black, anchoTicket / 2, y, formatoCentro);
        y += 14;

        // Línea separadora
        g.DrawLine(Pens.Black, 0, y, anchoTicket, y);
        y += 5;

        // === DATOS DEL PEDIDO ===
        // Mesa grande y destacada
        g.DrawString($"MESA: {pedido.NombreMesa}", fuenteMesa, Brushes.Black, anchoTicket / 2, y, formatoCentro);
        y += 22;
        
        // Número de pedido y comanda
        g.DrawString($"Pedido #: {pedido.NumeroPedido}", fuenteNormal, Brushes.Black, 0, y);
        if (pedido.NumeroComanda > 0)
        {
            g.DrawString($"Comanda #: {pedido.NumeroComanda}", fuenteNormal, Brushes.Black, anchoTicket, y, formatoDerecha);
        }
        y += 14;
        
        // Fecha y hora
        g.DrawString($"Fecha: {pedido.FechaPedido:dd/MM/yyyy HH:mm}", fuenteNormal, Brushes.Black, 0, y);
        y += 12;
        
        // Mesero
        if (!string.IsNullOrEmpty(pedido.NombreMesero))
        {
            g.DrawString($"Mesero: {pedido.NombreMesero}", fuenteNormal, Brushes.Black, 0, y);
            y += 12;
        }
        
        // Cliente
        if (!string.IsNullOrEmpty(pedido.NombreCliente))
        {
            g.DrawString($"Cliente: {pedido.NombreCliente}", fuenteNormal, Brushes.Black, 0, y);
            y += 12;
        }
        
        // Comensales
        if (pedido.CantidadComensales > 0)
        {
            g.DrawString($"Comensales: {pedido.CantidadComensales}", fuenteNormal, Brushes.Black, 0, y);
            y += 12;
        }
        
        // Observaciones del pedido (destacadas)
        if (!string.IsNullOrWhiteSpace(pedido.ObservacionesPedido))
        {
            y += 3;
            g.DrawLine(Pens.Black, 0, y, anchoTicket, y);
            y += 4;
            g.DrawString("OBSERVACIONES:", fuenteSubtitulo, Brushes.Black, 0, y);
            y += 12;
            // Dividir en líneas si es largo
            var lineasObs = DividirTextoEnLineas(pedido.ObservacionesPedido, 38);
            foreach (var linea in lineasObs)
            {
                g.DrawString(linea, fuenteNormal, Brushes.Black, 5, y);
                y += 11;
            }
        }

        // Línea separadora doble
        y += 3;
        g.DrawLine(Pens.Black, 0, y, anchoTicket, y);
        y += 2;
        g.DrawLine(Pens.Black, 0, y, anchoTicket, y);
        y += 6;

        // === ENCABEZADO DE ITEMS ===
        if (pedido.MostrarPrecios)
        {
            g.DrawString("CANT", fuentePequeña, Brushes.Black, 0, y);
            g.DrawString("DESCRIPCIÓN", fuentePequeña, Brushes.Black, 35, y);
            g.DrawString("IMPORTE", fuentePequeña, Brushes.Black, anchoTicket, y, formatoDerecha);
        }
        else
        {
            g.DrawString("CANT", fuentePequeña, Brushes.Black, 0, y);
            g.DrawString("DESCRIPCIÓN", fuentePequeña, Brushes.Black, 35, y);
        }
        y += 12;
        g.DrawLine(Pens.Black, 0, y, anchoTicket, y);
        y += 4;

        // === ITEMS DEL PEDIDO ===
        foreach (var item in pedido.Items)
        {
            // Si es urgente, marcar
            if (item.EsUrgente)
            {
                g.DrawString("*** URGENTE ***", fuenteSubtitulo, Brushes.Black, anchoTicket / 2, y, formatoCentro);
                y += 12;
            }
            
            // Cantidad y descripción
            string cantidadStr = item.Cantidad == Math.Floor(item.Cantidad) 
                ? ((int)item.Cantidad).ToString() 
                : item.Cantidad.ToString("N1");
            
            g.DrawString(cantidadStr, fuenteNormal, Brushes.Black, 0, y);
            
            // Descripción (truncar si es muy larga)
            string descripcion = item.Descripcion;
            float anchoDisponible = pedido.MostrarPrecios ? anchoTicket * 0.55f : anchoTicket * 0.85f;
            while (g.MeasureString(descripcion, fuenteNormal).Width > anchoDisponible && descripcion.Length > 3)
            {
                descripcion = descripcion[..^1];
            }
            if (descripcion != item.Descripcion) descripcion = descripcion.TrimEnd() + "..";
            
            g.DrawString(descripcion, fuenteNormal, Brushes.Black, 35, y);
            
            // Importe (si se muestran precios)
            if (pedido.MostrarPrecios)
            {
                g.DrawString(item.Subtotal.ToString("N0"), fuenteNormal, Brushes.Black, anchoTicket, y, formatoDerecha);
            }
            y += 13;
            
            // Modificadores
            if (!string.IsNullOrEmpty(item.Modificadores))
            {
                g.DrawString($"  → {item.Modificadores}", fuentePequeña, Brushes.Black, 30, y);
                y += 11;
            }
            
            // Notas de cocina (destacadas)
            if (!string.IsNullOrEmpty(item.NotasCocina))
            {
                g.DrawString($"  ⚠ {item.NotasCocina}", fuenteSubtitulo, Brushes.Black, 30, y);
                y += 12;
            }
        }

        // Línea separadora
        y += 3;
        g.DrawLine(Pens.Black, 0, y, anchoTicket, y);
        y += 6;

        // === TOTALES (solo si se muestran precios) ===
        if (pedido.MostrarPrecios)
        {
            // Subtotal
            g.DrawString("Subtotal:", fuenteNormal, Brushes.Black, 0, y);
            g.DrawString($"Gs {pedido.Subtotal:N0}", fuenteNormal, Brushes.Black, anchoTicket, y, formatoDerecha);
            y += 13;
            
            // Descuento (si hay)
            if (pedido.Descuento > 0)
            {
                g.DrawString("Descuento:", fuenteNormal, Brushes.Black, 0, y);
                g.DrawString($"-Gs {pedido.Descuento:N0}", fuenteNormal, Brushes.Black, anchoTicket, y, formatoDerecha);
                y += 13;
            }
            
            // Cargo por servicio (si hay)
            if (pedido.CargoServicio > 0)
            {
                g.DrawString("Servicio:", fuenteNormal, Brushes.Black, 0, y);
                g.DrawString($"Gs {pedido.CargoServicio:N0}", fuenteNormal, Brushes.Black, anchoTicket, y, formatoDerecha);
                y += 13;
            }
            
            // Total grande
            y += 2;
            g.DrawLine(new Pen(Color.Black, 2), 0, y, anchoTicket, y);
            y += 5;
            g.DrawString("TOTAL:", fuenteTotal, Brushes.Black, 0, y);
            g.DrawString($"Gs {pedido.Total:N0}", fuenteTotal, Brushes.Black, anchoTicket, y, formatoDerecha);
            y += 18;
        }
        else
        {
            // Sin precios - solo cantidad de items
            g.DrawString($"Total items: {pedido.Items.Sum(i => i.Cantidad):N0}", fuenteSubtitulo, Brushes.Black, anchoTicket / 2, y, formatoCentro);
            y += 15;
        }

        // Línea separadora
        g.DrawLine(Pens.Black, 0, y, anchoTicket, y);
        y += 8;

        // === PIE ===
        if (!string.IsNullOrEmpty(pedido.MensajePie))
        {
            g.DrawString(pedido.MensajePie, fuentePequeña, Brushes.Black, anchoTicket / 2, y, formatoCentro);
            y += 12;
        }
        
        g.DrawString("¡Gracias por su preferencia!", fuenteNormal, Brushes.Black, anchoTicket / 2, y, formatoCentro);
        y += 12;
        
        g.DrawString(pedido.TelefonoEmpresa, fuentePequeña, Brushes.Black, anchoTicket / 2, y, formatoCentro);
        y += 15;

        // Espacio para corte
        y += 20;

        e.HasMorePages = false;
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
        
        // Posiciones de columnas para el detalle (NUEVO FORMATO: CANT | DESCRIPCIÓN | IMPORTE | IVA)
        // En modo farmacia se agregan: %Desc y P.MIN entre DESCRIPCIÓN e IMPORTE
        float colCant = 0;
        float colDesc = anchoTicket * 0.10f;
        // Columnas farmacia (solo se usan si ModoFarmacia = true)
        float colPorcDesc = anchoTicket * 0.40f;
        float colPMin = anchoTicket * 0.52f;
        // Columnas finales
        float colImporte = anchoTicket * 0.68f;
        float colIva = anchoTicket * 0.88f;
        
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
            // Actividad económica - dibujar en múltiples líneas si es necesario
            var actividadLineas = DividirTextoEnLineas(ticket.ActividadEconomica, 40);
            foreach (var lineaAct in actividadLineas)
            {
                g.DrawString(lineaAct, fuentePequeña, Brushes.Black, anchoTicket / 2, y, formatoCentro);
                y += 11;
            }
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
        g.DrawString("CLIENTE:", fuenteEncabezado, Brushes.Black, 0, y);
        y += 14;
        
        // Nombre del cliente - en una sola línea alineada
        var nombreCliente = TruncarTexto(ticket.NombreCliente ?? "SIN NOMBRE", 38);
        g.DrawString(nombreCliente, fuenteNormal, Brushes.Black, 0, y);
        y += 12;
        
        // RUC/CI (ya viene formateado con DV desde FormatearRucConDv)
        g.DrawString($"RUC: {ticket.RucCliente}", fuenteNormal, Brushes.Black, 0, y);
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
        y += 8;
        
        // === ENCABEZADO DE DETALLE ===
        // Posiciones de columnas para modo farmacia (más compactas)
        float cCant = 0;
        float cDesc = anchoTicket * 0.08f;
        float cPD = anchoTicket * 0.50f;
        float cPMin = anchoTicket * 0.58f;
        float cImp = anchoTicket * 0.72f;
        float cIva = anchoTicket * 0.92f;
        
        if (ticket.ModoFarmacia)
        {
            // Encabezado farmacia en UNA línea
            g.DrawString("CN", fuenteEncabezado, Brushes.Black, cCant, y);
            g.DrawString("DESCRIPCIÓN", fuenteEncabezado, Brushes.Black, cDesc, y);
            g.DrawString("%D", fuenteEncabezado, Brushes.Black, cPD, y);
            g.DrawString("P.MN", fuenteEncabezado, Brushes.Black, cPMin, y);
            g.DrawString("IMPORTE", fuenteEncabezado, Brushes.Black, cImp, y);
            g.DrawString("IVA", fuenteEncabezado, Brushes.Black, cIva, y);
        }
        else
        {
            // Modo normal: 4 columnas
            g.DrawString("CANT.", fuenteEncabezado, Brushes.Black, colCant, y);
            g.DrawString("DESCRIPCIÓN", fuenteEncabezado, Brushes.Black, colDesc, y);
            g.DrawString("IMPORTE", fuenteEncabezado, Brushes.Black, colImporte, y);
            g.DrawString("IVA", fuenteEncabezado, Brushes.Black, colIva, y);
        }
        y += 11;
        g.DrawLine(Pens.Black, 0, y, anchoTicket, y);
        y += 4;
        
        // === DETALLE DE PRODUCTOS ===
        foreach (var item in ticket.Items)
        {
            // Cantidad: sin decimales si es entero, sino con 2 decimales
            var cantidadStr = item.Cantidad == Math.Truncate(item.Cantidad) 
                ? item.Cantidad.ToString("N0") 
                : item.Cantidad.ToString("N2");
            
            // Usar Subtotal que contiene el precio CON IVA (d.Importe)
            // NO usar item.Exenta + item.Gravado5 + item.Gravado10 porque son bases gravadas SIN IVA
            decimal importe = item.Subtotal;
            
            // Determinar código IVA (10, 5, o E para exenta)
            string codigoIva = item.Gravado10 > 0 ? "10" : (item.Gravado5 > 0 ? "5" : "E");
            
            if (ticket.ModoFarmacia)
            {
                // Formato: CN | DESCRIPCION | %D | P.MIN | IMPORTE | IVA (todo en una línea)
                g.DrawString(cantidadStr, fuentePequeña, Brushes.Black, cCant, y);
                g.DrawString(TruncarTexto(item.Descripcion, 18), fuentePequeña, Brushes.Black, cDesc, y);
                
                // %Desc y P.Min
                var descStr = item.PorcentajeDescuento > 0 ? item.PorcentajeDescuento.ToString("N0") : "-";
                var pminStr = item.PrecioMinisterio > 0 ? FormatearNumero(item.PrecioMinisterio) : "-";
                g.DrawString(descStr, fuentePequeña, Brushes.Black, cPD, y);
                g.DrawString(pminStr, fuentePequeña, Brushes.Black, cPMin, y);
                
                // Importe e IVA
                g.DrawString(FormatearNumero(importe), fuentePequeña, Brushes.Black, cImp, y);
                g.DrawString(codigoIva, fuentePequeña, Brushes.Black, cIva, y);
                y += 11;
            }
            else
            {
                // Modo normal: descripción más larga
                g.DrawString(cantidadStr, fuentePequeña, Brushes.Black, colCant, y);
                g.DrawString(TruncarTexto(item.Descripcion, 28), fuentePequeña, Brushes.Black, colDesc, y);
                g.DrawString(FormatearNumero(importe), fuentePequeña, Brushes.Black, colImporte, y);
                g.DrawString(codigoIva, fuentePequeña, Brushes.Black, colIva, y);
                y += 12;
            }
        }
        
        // === LÍNEA DOBLE ===
        y += 4;
        g.DrawLine(Pens.Black, 0, y, anchoTicket, y);
        g.DrawLine(Pens.Black, 0, y + 2, anchoTicket, y + 2);
        y += 10;
        
        // === SUB-TOTALES (formato simplificado) ===
        // Los sub-totales por tipo de IVA se muestran en la sección LIQUIDACIÓN DEL IVA
        y += 4;
        
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
        g.DrawString("IMPORTE", fuenteEncabezado, Brushes.Black, colIvaBase, y);  // Importe CON IVA
        g.DrawString("IVA", fuenteEncabezado, Brushes.Black, colIvaMonto, y);
        y += 12;
        
        g.DrawLine(Pens.Black, 0, y, anchoTicket, y);
        y += 6;
        
        // Exentas (no tienen IVA, el importe es igual)
        g.DrawString("Exentas:", fuentePequeña, Brushes.Black, colIvaLabel, y);
        g.DrawString(ticket.TotalExentas > 0 ? FormatearNumero(ticket.TotalExentas) : "-", fuentePequeña, Brushes.Black, colIvaBase, y);
        g.DrawString("-", fuentePequeña, Brushes.Black, colIvaMonto, y);
        y += 12;
        
        // IVA 5% - Importe CON IVA = Base gravada + IVA
        var importe5 = ticket.TotalIva5 + ticket.MontoIva5;
        g.DrawString("IVA 5%:", fuentePequeña, Brushes.Black, colIvaLabel, y);
        g.DrawString(importe5 > 0 ? FormatearNumero(importe5) : "-", fuentePequeña, Brushes.Black, colIvaBase, y);
        g.DrawString(ticket.MontoIva5 > 0 ? FormatearNumero(ticket.MontoIva5) : "-", fuentePequeña, Brushes.Black, colIvaMonto, y);
        y += 12;
        
        // IVA 10% - Importe CON IVA = Base gravada + IVA
        var importe10 = ticket.TotalIva10 + ticket.MontoIva10;
        g.DrawString("IVA 10%:", fuentePequeña, Brushes.Black, colIvaLabel, y);
        g.DrawString(importe10 > 0 ? FormatearNumero(importe10) : "-", fuentePequeña, Brushes.Black, colIvaBase, y);
        g.DrawString(ticket.MontoIva10 > 0 ? FormatearNumero(ticket.MontoIva10) : "-", fuentePequeña, Brushes.Black, colIvaMonto, y);
        y += 12;
        
        // Total IVA
        g.DrawLine(Pens.Black, colIvaMonto - 10, y, anchoTicket, y);
        y += 6;
        g.DrawString("Total IVA:", fuenteNormal, Brushes.Black, colIvaLabel, y);
        g.DrawString(FormatearNumero(ticket.TotalIva), fuenteNormal, Brushes.Black, colIvaMonto, y);
        y += 16;
        
        // === SECCIÓN SIFEN (FACTURA ELECTRÓNICA) ===
        if (ticket.EsElectronica && !string.IsNullOrEmpty(ticket.CDC))
        {
            g.DrawLine(Pens.Black, 0, y, anchoTicket, y);
            g.DrawLine(Pens.Black, 0, y + 2, anchoTicket, y + 2);
            y += 8;
            
            // Generar QR si tenemos la URL
            var urlQr = ticket.UrlQrSifen;
            if (string.IsNullOrEmpty(urlQr) && !string.IsNullOrEmpty(ticket.CDC))
            {
                // Fallback: generar URL con CDC
                urlQr = $"https://ekuatia.set.gov.py/consultas/qr?CDC={ticket.CDC}";
            }
            
            Image? qrImage = null;
            if (!string.IsNullOrEmpty(urlQr))
            {
                try
                {
                    using var qrGenerator = new QRCodeGenerator();
                    // Usar nivel M (Medium 15%) - balance entre densidad y corrección
                    // H es demasiado denso para URLs largas de SIFEN
                    using var qrCodeData = qrGenerator.CreateQrCode(urlQr, QRCodeGenerator.ECCLevel.M);
                    // Usar PngByteQRCode con zona de silencio (quiet zone) incluida
                    using var qrCode = new PngByteQRCode(qrCodeData);
                    // 3 pixels por módulo - el QR resultante será del tamaño natural
                    // La zona de silencio (4 módulos blancos alrededor) se incluye automáticamente
                    var qrBytes = qrCode.GetGraphic(3);
                    using var msQr = new MemoryStream(qrBytes);
                    qrImage = Image.FromStream(msQr);
                }
                catch { /* Si falla el QR, continuar sin él */ }
            }
            
            // Layout: QR a la izquierda, textos a la derecha
            // Usar el tamaño real del QR generado o 120 si no hay imagen
            float qrSize = qrImage?.Width ?? 120;
            // Limitar tamaño máximo a 140px para que quepa en el ticket
            if (qrSize > 140) qrSize = 140;
            float qrX = 5;
            float textoX = qrX + qrSize + 8;
            float textoAncho = anchoTicket - textoX - 5;
            float yInicio = y;
            
            // Dibujar QR - SIN redimensionar para mantener nitidez
            if (qrImage != null)
            {
                // Si el QR es más pequeño que qrSize, centrarlo. Si es más grande, escalarlo.
                if (qrImage.Width <= qrSize)
                {
                    // Dibujar a tamaño natural (sin escalar)
                    float offsetX = (qrSize - qrImage.Width) / 2;
                    g.DrawImage(qrImage, qrX + offsetX, y);
                }
                else
                {
                    // Escalar proporcionalmente solo si es necesario
                    g.DrawImage(qrImage, qrX, y, qrSize, qrSize);
                }
            }
            else
            {
                // Si no hay QR, dibujar recuadro vacío
                g.DrawRectangle(Pens.Black, qrX, y, qrSize, qrSize);
                g.DrawString("QR", fuentePequeña, Brushes.Black, qrX + qrSize / 2, y + qrSize / 2 - 5, formatoCentro);
            }
            
            // Textos de validación a la derecha del QR
            var fuenteValidacion = new Font("Arial", 6f);
            g.DrawString("Consulte la validez de", fuenteValidacion, Brushes.Black, textoX, y);
            y += 10;
            g.DrawString("este documento en:", fuenteValidacion, Brushes.Black, textoX, y);
            y += 12;
            g.DrawString("ekuatia.set.gov.py/consultas/", fuenteValidacion, Brushes.Black, textoX, y);
            y += 14;
            
            // Ajustar Y al final del QR si es más alto que los textos
            y = Math.Max(y, yInicio + qrSize + 5);
            
            // CDC debajo del QR
            g.DrawString("CDC:", fuenteEncabezado, Brushes.Black, 0, y);
            y += 10;
            
            // El CDC es largo, lo dividimos en 2 partes para que entre en el ticket
            if (ticket.CDC.Length > 22)
            {
                g.DrawString(ticket.CDC.Substring(0, 22), fuentePequeña, Brushes.Black, anchoTicket / 2, y, formatoCentro);
                y += 10;
                g.DrawString(ticket.CDC.Substring(22), fuentePequeña, Brushes.Black, anchoTicket / 2, y, formatoCentro);
                y += 10;
            }
            else
            {
                g.DrawString(ticket.CDC, fuentePequeña, Brushes.Black, anchoTicket / 2, y, formatoCentro);
                y += 10;
            }
            
            // Nota legal
            y += 4;
            var fuenteNota = new Font("Arial", 5.5f);
            g.DrawString("REPRESENTACIÓN GRÁFICA DE", fuenteNota, Brushes.Black, anchoTicket / 2, y, formatoCentro);
            y += 8;
            g.DrawString("DOCUMENTO ELECTRÓNICO (XML)", fuenteNota, Brushes.Black, anchoTicket / 2, y, formatoCentro);
            y += 10;
            
            qrImage?.Dispose();
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
    /// Divide un texto en líneas por cantidad de caracteres (para actividad económica).
    /// </summary>
    private List<string> DividirTextoEnLineas(string texto, int maxCaracteres)
    {
        var lineas = new List<string>();
        if (string.IsNullOrEmpty(texto)) return lineas;
        
        var palabras = texto.Split(' ');
        var lineaActual = "";
        
        foreach (var palabra in palabras)
        {
            var pruebaLinea = string.IsNullOrEmpty(lineaActual) ? palabra : lineaActual + " " + palabra;
            
            if (pruebaLinea.Length > maxCaracteres && !string.IsNullOrEmpty(lineaActual))
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
            lineas.Add(lineaActual);
        
        return lineas;
    }
    
    /// <summary>
    /// Formatea un número sin decimales con separador de miles.
    /// </summary>
    private string FormatearNumero(decimal valor)
    {
        return valor.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("es-PY"));
    }
    
    /// <summary>
    /// Formatea un número de forma corta (sin separador de miles para números pequeños).
    /// Para tickets con espacio limitado (modo farmacia).
    /// </summary>
    private string FormatearNumeroCorto(decimal valor)
    {
        if (valor >= 1000000)
            return (valor / 1000000m).ToString("0.#") + "M";
        if (valor >= 10000)
            return (valor / 1000m).ToString("0") + "k";
        return valor.ToString("N0");
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
    public string? UrlQrSifen { get; set; }  // URL oficial del QR con cHashQR (dCarQR del XML firmado)
    public string CondicionVenta { get; set; } = "Contado";
    public string TipoDocumento { get; set; } = "FACTURA";
    public bool EsElectronica { get; set; } = true;
    
    // Datos del cliente
    public string NombreCliente { get; set; } = "";
    public string RucCliente { get; set; } = "";
    public string? DvCliente { get; set; }
    public string DireccionCliente { get; set; } = "";
    
    // Modo farmacia
    public bool ModoFarmacia { get; set; } = false;
    
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
    
    // Campos farmacia
    public decimal PorcentajeDescuento { get; set; }
    public decimal PrecioMinisterio { get; set; }
}

public class ResultadoImpresion
{
    public bool Exitoso { get; set; }
    public string Mensaje { get; set; } = "";
}

// ========== CLASES PARA TICKET DE PEDIDO (RESTAURANTE) ==========

/// <summary>
/// Datos necesarios para imprimir un ticket de pedido (comprobante para el cliente).
/// </summary>
public class DatosPedidoTicket
{
    // Logo
    public byte[]? LogoBytes { get; set; }
    
    // Datos de la empresa
    public string NombreEmpresa { get; set; } = "";
    public string TelefonoEmpresa { get; set; } = "";
    public string DireccionEmpresa { get; set; } = "";
    
    // Datos del pedido
    public int NumeroPedido { get; set; }
    public int NumeroComanda { get; set; }
    public DateTime FechaPedido { get; set; }
    public string NombreMesa { get; set; } = "";
    public string? NombreMesero { get; set; }
    public string? NombreCliente { get; set; }
    public int CantidadComensales { get; set; }
    
    // Observaciones del pedido
    public string? ObservacionesPedido { get; set; }
    
    // Items del pedido
    public List<ItemPedidoTicket> Items { get; set; } = new();
    
    // Totales
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal CargoServicio { get; set; }
    public decimal Total { get; set; }
    
    // Opciones de impresión
    public bool MostrarPrecios { get; set; } = true;
    public bool EsComanda { get; set; } = false; // Si es true, formato comanda cocina
    public string? DestinoComanda { get; set; } // "COCINA", "BARRA", etc.
    public string? MensajePie { get; set; }
}

/// <summary>
/// Item individual del pedido para impresión.
/// </summary>
public class ItemPedidoTicket
{
    public string Descripcion { get; set; } = "";
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
    public string? Modificadores { get; set; }
    public string? NotasCocina { get; set; }
    public string? Categoria { get; set; }
    public bool EsUrgente { get; set; }
}
