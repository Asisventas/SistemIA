using System.Drawing;
using System.Drawing.Printing;
using System.Net.Sockets;
using System.Runtime.Versioning;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SistemIA.Models;

namespace SistemIA.Services;

/// <summary>
/// Servicio para imprimir comandas de cocina/barra.
/// Soporta impresión local (Windows) y de red (TCP directo a impresoras térmicas).
/// </summary>
[SupportedOSPlatform("windows")]
public class ComandaService
{
    private readonly ILogger<ComandaService> _logger;
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    // Códigos ESC/POS comunes para impresoras térmicas
    private static class EscPos
    {
        public static readonly byte[] Initialize = new byte[] { 0x1B, 0x40 }; // ESC @
        public static readonly byte[] BoldOn = new byte[] { 0x1B, 0x45, 0x01 }; // ESC E 1
        public static readonly byte[] BoldOff = new byte[] { 0x1B, 0x45, 0x00 }; // ESC E 0
        public static readonly byte[] DoubleHeight = new byte[] { 0x1B, 0x21, 0x10 }; // ESC ! 16
        public static readonly byte[] DoubleWidth = new byte[] { 0x1B, 0x21, 0x20 }; // ESC ! 32
        public static readonly byte[] DoubleSize = new byte[] { 0x1B, 0x21, 0x30 }; // ESC ! 48
        public static readonly byte[] NormalSize = new byte[] { 0x1B, 0x21, 0x00 }; // ESC ! 0
        public static readonly byte[] CenterAlign = new byte[] { 0x1B, 0x61, 0x01 }; // ESC a 1
        public static readonly byte[] LeftAlign = new byte[] { 0x1B, 0x61, 0x00 }; // ESC a 0
        public static readonly byte[] LineFeed = new byte[] { 0x0A }; // LF
        public static readonly byte[] CutPaper = new byte[] { 0x1D, 0x56, 0x00 }; // GS V 0 (corte total)
        public static readonly byte[] PartialCut = new byte[] { 0x1D, 0x56, 0x01 }; // GS V 1 (corte parcial)
        public static readonly byte[] OpenDrawer = new byte[] { 0x1B, 0x70, 0x00, 0x19, 0xFA }; // Abrir cajón
    }

    public ComandaService(ILogger<ComandaService> logger, IDbContextFactory<AppDbContext> dbFactory)
    {
        _logger = logger;
        _dbFactory = dbFactory;
    }

    /// <summary>
    /// Datos de una comanda para impresión
    /// </summary>
    public class DatosComanda
    {
        public int NumeroComanda { get; set; }
        public int NumeroPedido { get; set; }
        public string? NombreMesa { get; set; }
        public string? NumeroMesa { get; set; }
        public string? Mesero { get; set; }
        public int Comensales { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        public string TipoDestino { get; set; } = "COCINA"; // COCINA o BARRA
        public List<ItemComanda> Items { get; set; } = new();
        public string? Observaciones { get; set; }
        public bool EsReimpresion { get; set; }
        public bool EsAnulacion { get; set; }
    }

    public class ItemComanda
    {
        public int Cantidad { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string? Modificadores { get; set; }
        public string? NotasCocina { get; set; }
        public bool EsUrgente { get; set; }
    }

    /// <summary>
    /// Imprime comanda en la impresora configurada para cocina o barra.
    /// </summary>
    public async Task<ResultadoComanda> ImprimirComandaAsync(DatosComanda comanda, Caja caja)
    {
        try
        {
            string? impresora;
            int? puerto;
            bool esRed;
            int copias;

            if (comanda.TipoDestino == "BARRA")
            {
                impresora = caja.ImpresoraComandaBarra;
                puerto = caja.PuertoImpresoraBarra ?? 9100;
                esRed = caja.ComandaBarraEsRed ?? false;
                copias = caja.CopiasComandaBarra ?? 1;
            }
            else // COCINA por defecto
            {
                impresora = caja.ImpresoraComandaCocina;
                puerto = caja.PuertoImpresoraCocina ?? 9100;
                esRed = caja.ComandaCocinaEsRed ?? false;
                copias = caja.CopiasComandaCocina ?? 1;
            }

            if (string.IsNullOrWhiteSpace(impresora))
            {
                return new ResultadoComanda
                {
                    Exitoso = false,
                    Mensaje = $"No hay impresora configurada para {comanda.TipoDestino}"
                };
            }

            // Generar contenido de la comanda
            var contenido = GenerarContenidoComanda(comanda);

            ResultadoComanda resultado;
            for (int i = 0; i < copias; i++)
            {
                if (esRed)
                {
                    resultado = await ImprimirPorRedAsync(impresora, puerto.Value, contenido);
                }
                else
                {
                    resultado = await ImprimirPorWindowsAsync(impresora, comanda);
                }

                if (!resultado.Exitoso)
                {
                    return resultado;
                }
            }

            _logger.LogInformation("Comanda {NumComanda} impresa en {Destino} ({Impresora})",
                comanda.NumeroComanda, comanda.TipoDestino, impresora);

            return new ResultadoComanda
            {
                Exitoso = true,
                Mensaje = $"Comanda enviada a {comanda.TipoDestino}",
                NumeroComanda = comanda.NumeroComanda
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al imprimir comanda {NumComanda}", comanda.NumeroComanda);
            return new ResultadoComanda
            {
                Exitoso = false,
                Mensaje = $"Error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Genera el contenido de la comanda en formato ESC/POS bytes.
    /// </summary>
    private byte[] GenerarContenidoComanda(DatosComanda comanda)
    {
        using var ms = new MemoryStream();
        var encoding = Encoding.GetEncoding("IBM437"); // Codepage para caracteres especiales

        // Inicializar impresora
        ms.Write(EscPos.Initialize);

        // ========== ENCABEZADO ==========
        ms.Write(EscPos.CenterAlign);
        ms.Write(EscPos.DoubleSize);

        // Título según tipo
        if (comanda.EsAnulacion)
        {
            EscribirLinea(ms, encoding, "*** ANULACION ***");
        }
        else if (comanda.EsReimpresion)
        {
            EscribirLinea(ms, encoding, "** REIMPRESION **");
        }

        EscribirLinea(ms, encoding, $"=== {comanda.TipoDestino} ===");
        
        ms.Write(EscPos.NormalSize);
        ms.Write(EscPos.BoldOn);
        
        // Mesa y comanda
        EscribirLinea(ms, encoding, $"MESA: {comanda.NombreMesa ?? comanda.NumeroMesa ?? "-"}");
        EscribirLinea(ms, encoding, $"COMANDA: #{comanda.NumeroComanda}");
        
        ms.Write(EscPos.BoldOff);
        ms.Write(EscPos.LeftAlign);
        
        // Info adicional
        EscribirLinea(ms, encoding, new string('-', 42));
        EscribirLinea(ms, encoding, $"Pedido: #{comanda.NumeroPedido}");
        EscribirLinea(ms, encoding, $"Hora: {comanda.Fecha:HH:mm:ss}");
        if (!string.IsNullOrEmpty(comanda.Mesero))
            EscribirLinea(ms, encoding, $"Mesero: {comanda.Mesero}");
        EscribirLinea(ms, encoding, $"Comensales: {comanda.Comensales}");
        EscribirLinea(ms, encoding, new string('-', 42));

        // ========== ITEMS ==========
        ms.Write(EscPos.LineFeed);
        ms.Write(EscPos.BoldOn);
        EscribirLinea(ms, encoding, "CANT  PRODUCTO");
        ms.Write(EscPos.BoldOff);
        EscribirLinea(ms, encoding, new string('-', 42));

        foreach (var item in comanda.Items)
        {
            // Cantidad y descripción
            if (item.EsUrgente)
            {
                ms.Write(EscPos.DoubleHeight);
                ms.Write(EscPos.BoldOn);
                EscribirLinea(ms, encoding, $"!!! URGENTE !!!");
            }
            
            ms.Write(EscPos.BoldOn);
            // Si la descripción es larga, dividir en varias líneas
            var lineasDescripcion = DividirTexto(item.Descripcion, 34);
            for (int i = 0; i < lineasDescripcion.Count; i++)
            {
                if (i == 0)
                {
                    EscribirLinea(ms, encoding, $"{item.Cantidad,3}x  {lineasDescripcion[i]}");
                }
                else
                {
                    EscribirLinea(ms, encoding, $"      {lineasDescripcion[i]}"); // Continuación con indentación
                }
            }
            ms.Write(EscPos.BoldOff);
            ms.Write(EscPos.NormalSize);

            // Modificadores
            if (!string.IsNullOrWhiteSpace(item.Modificadores))
            {
                EscribirLinea(ms, encoding, $"      >> {item.Modificadores}");
            }

            // Notas de cocina
            if (!string.IsNullOrWhiteSpace(item.NotasCocina))
            {
                ms.Write(EscPos.BoldOn);
                EscribirLinea(ms, encoding, $"      ** {item.NotasCocina}");
                ms.Write(EscPos.BoldOff);
            }

            ms.Write(EscPos.LineFeed);
        }

        // ========== OBSERVACIONES ==========
        if (!string.IsNullOrWhiteSpace(comanda.Observaciones))
        {
            EscribirLinea(ms, encoding, new string('-', 42));
            ms.Write(EscPos.BoldOn);
            EscribirLinea(ms, encoding, "OBSERVACIONES:");
            ms.Write(EscPos.BoldOff);
            // Dividir en líneas de 42 caracteres
            foreach (var linea in DividirTexto(comanda.Observaciones, 42))
            {
                EscribirLinea(ms, encoding, linea);
            }
        }

        // ========== PIE ==========
        ms.Write(EscPos.LineFeed);
        ms.Write(EscPos.LineFeed);
        ms.Write(EscPos.CenterAlign);
        EscribirLinea(ms, encoding, new string('=', 42));
        EscribirLinea(ms, encoding, comanda.Fecha.ToString("dd/MM/yyyy HH:mm:ss"));
        ms.Write(EscPos.LineFeed);
        ms.Write(EscPos.LineFeed);
        ms.Write(EscPos.LineFeed);

        // Cortar papel
        ms.Write(EscPos.PartialCut);

        return ms.ToArray();
    }

    private void EscribirLinea(MemoryStream ms, Encoding encoding, string texto)
    {
        var bytes = encoding.GetBytes(texto);
        ms.Write(bytes);
        ms.Write(EscPos.LineFeed);
    }

    private string TruncarTexto(string texto, int maxLen)
    {
        if (string.IsNullOrEmpty(texto)) return "";
        return texto.Length <= maxLen ? texto : texto.Substring(0, maxLen - 2) + "..";
    }

    private List<string> DividirTexto(string texto, int maxLen)
    {
        var lineas = new List<string>();
        if (string.IsNullOrEmpty(texto)) return lineas;

        var palabras = texto.Split(' ');
        var lineaActual = "";

        foreach (var palabra in palabras)
        {
            if ((lineaActual + " " + palabra).Trim().Length <= maxLen)
            {
                lineaActual = (lineaActual + " " + palabra).Trim();
            }
            else
            {
                if (!string.IsNullOrEmpty(lineaActual))
                    lineas.Add(lineaActual);
                lineaActual = palabra;
            }
        }
        if (!string.IsNullOrEmpty(lineaActual))
            lineas.Add(lineaActual);

        return lineas;
    }

    /// <summary>
    /// Imprime directamente a impresora de red por TCP (puerto 9100 típico).
    /// </summary>
    private async Task<ResultadoComanda> ImprimirPorRedAsync(string ip, int puerto, byte[] contenido)
    {
        try
        {
            using var client = new TcpClient();
            var connectTask = client.ConnectAsync(ip, puerto);
            
            // Timeout de 5 segundos para conexión
            if (await Task.WhenAny(connectTask, Task.Delay(5000)) != connectTask)
            {
                return new ResultadoComanda
                {
                    Exitoso = false,
                    Mensaje = $"Timeout conectando a {ip}:{puerto}"
                };
            }

            await connectTask; // Propagar excepción si hubo

            using var stream = client.GetStream();
            await stream.WriteAsync(contenido);
            await stream.FlushAsync();

            return new ResultadoComanda { Exitoso = true, Mensaje = "Enviado por red" };
        }
        catch (SocketException ex)
        {
            _logger.LogError(ex, "Error de conexión a impresora {IP}:{Puerto}", ip, puerto);
            return new ResultadoComanda
            {
                Exitoso = false,
                Mensaje = $"No se pudo conectar a {ip}:{puerto} - {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Imprime usando el sistema de impresión de Windows (impresora local o compartida).
    /// </summary>
    private async Task<ResultadoComanda> ImprimirPorWindowsAsync(string nombreImpresora, DatosComanda comanda)
    {
        return await Task.Run(() =>
        {
            try
            {
                var printDoc = new PrintDocument();
                printDoc.PrinterSettings.PrinterName = nombreImpresora;

                if (!printDoc.PrinterSettings.IsValid)
                {
                    return new ResultadoComanda
                    {
                        Exitoso = false,
                        Mensaje = $"Impresora '{nombreImpresora}' no encontrada"
                    };
                }

                printDoc.DocumentName = $"Comanda-{comanda.NumeroComanda}";
                printDoc.DefaultPageSettings.PaperSize = new PaperSize("Ticket80mm", 283, 0);
                printDoc.DefaultPageSettings.Margins = new Margins(5, 5, 5, 5);

                printDoc.PrintPage += (sender, e) => DibujarComandaGDI(e, comanda);
                printDoc.Print();

                return new ResultadoComanda { Exitoso = true, Mensaje = "Impreso por Windows" };
            }
            catch (Exception ex)
            {
                return new ResultadoComanda
                {
                    Exitoso = false,
                    Mensaje = ex.Message
                };
            }
        });
    }

    /// <summary>
    /// Dibuja la comanda usando GDI+ para impresión Windows.
    /// </summary>
    private void DibujarComandaGDI(PrintPageEventArgs e, DatosComanda comanda)
    {
        var g = e.Graphics!;
        float y = 5;
        float x = 5;
        float ancho = 270; // ~80mm

        var fuenteNormal = new Font("Consolas", 9);
        var fuenteGrande = new Font("Consolas", 12, FontStyle.Bold);
        var fuentePequena = new Font("Consolas", 8);

        var formatoCentro = new StringFormat { Alignment = StringAlignment.Center };

        // Título
        if (comanda.EsAnulacion)
        {
            g.DrawString("*** ANULACION ***", fuenteGrande, Brushes.Black, ancho / 2, y, formatoCentro);
            y += 20;
        }
        else if (comanda.EsReimpresion)
        {
            g.DrawString("** REIMPRESION **", fuenteNormal, Brushes.Black, ancho / 2, y, formatoCentro);
            y += 15;
        }

        g.DrawString($"=== {comanda.TipoDestino} ===", fuenteGrande, Brushes.Black, ancho / 2, y, formatoCentro);
        y += 25;

        g.DrawString($"MESA: {comanda.NombreMesa ?? comanda.NumeroMesa ?? "-"}", fuenteGrande, Brushes.Black, x, y);
        y += 20;
        g.DrawString($"COMANDA: #{comanda.NumeroComanda}", fuenteGrande, Brushes.Black, x, y);
        y += 20;

        g.DrawString(new string('-', 42), fuentePequena, Brushes.Black, x, y);
        y += 12;

        g.DrawString($"Pedido: #{comanda.NumeroPedido}  Hora: {comanda.Fecha:HH:mm}", fuentePequena, Brushes.Black, x, y);
        y += 12;
        if (!string.IsNullOrEmpty(comanda.Mesero))
        {
            g.DrawString($"Mesero: {comanda.Mesero}  Comensales: {comanda.Comensales}", fuentePequena, Brushes.Black, x, y);
            y += 12;
        }

        g.DrawString(new string('-', 42), fuentePequena, Brushes.Black, x, y);
        y += 15;

        // Items
        foreach (var item in comanda.Items)
        {
            if (item.EsUrgente)
            {
                g.DrawString("!!! URGENTE !!!", fuenteGrande, Brushes.Black, x, y);
                y += 18;
            }

            // Si la descripción es larga, dividir en varias líneas
            var lineasDescGrafico = DividirTexto(item.Descripcion, 34);
            for (int i = 0; i < lineasDescGrafico.Count; i++)
            {
                if (i == 0)
                {
                    g.DrawString($"{item.Cantidad,3}x {lineasDescGrafico[i]}", fuenteGrande, Brushes.Black, x, y);
                }
                else
                {
                    g.DrawString($"      {lineasDescGrafico[i]}", fuenteNormal, Brushes.Black, x, y); // Continuación
                }
                y += 18;
            }

            if (!string.IsNullOrWhiteSpace(item.Modificadores))
            {
                g.DrawString($"      >> {item.Modificadores}", fuentePequena, Brushes.Black, x, y);
                y += 12;
            }

            if (!string.IsNullOrWhiteSpace(item.NotasCocina))
            {
                g.DrawString($"      ** {item.NotasCocina}", fuenteNormal, Brushes.Black, x, y);
                y += 14;
            }

            y += 5;
        }

        // Observaciones
        if (!string.IsNullOrWhiteSpace(comanda.Observaciones))
        {
            g.DrawString(new string('-', 42), fuentePequena, Brushes.Black, x, y);
            y += 12;
            g.DrawString("OBSERVACIONES:", fuenteNormal, Brushes.Black, x, y);
            y += 14;
            foreach (var linea in DividirTexto(comanda.Observaciones, 42))
            {
                g.DrawString(linea, fuentePequena, Brushes.Black, x, y);
                y += 12;
            }
        }

        // Pie
        y += 10;
        g.DrawString(new string('=', 42), fuentePequena, Brushes.Black, x, y);
        y += 12;
        g.DrawString(comanda.Fecha.ToString("dd/MM/yyyy HH:mm:ss"), fuentePequena, Brushes.Black, ancho / 2, y, formatoCentro);

        e.HasMorePages = false;
    }

    /// <summary>
    /// Envía comandas para los items nuevos de un pedido (agrupa por destino cocina/barra).
    /// </summary>
    public async Task<List<ResultadoComanda>> EnviarComandasPedidoAsync(
        int idPedido, 
        int idCaja, 
        string? mesero = null,
        List<int>? idsDetallesEspecificos = null)
    {
        var resultados = new List<ResultadoComanda>();

        await using var ctx = await _dbFactory.CreateDbContextAsync();

        var pedido = await ctx.Pedidos
            .Include(p => p.Mesa)
            .Include(p => p.Detalles)!.ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(p => p.IdPedido == idPedido);

        if (pedido == null)
        {
            resultados.Add(new ResultadoComanda { Exitoso = false, Mensaje = "Pedido no encontrado" });
            return resultados;
        }

        var caja = await ctx.Cajas.FindAsync(idCaja);
        if (caja == null)
        {
            resultados.Add(new ResultadoComanda { Exitoso = false, Mensaje = "Caja no configurada" });
            return resultados;
        }

        // Obtener IDs de tipos que son servicios
        var tiposServicio = await ctx.TiposItem.Where(t => t.EsServicio).Select(t => t.IdTipoItem).ToListAsync();
        
        // Filtrar items no enviados a cocina Y que NO sean servicios
        var detalles = pedido.Detalles?
            .Where(d => !d.EnviadoCocina && d.Estado != "Cancelado")
            .Where(d => idsDetallesEspecificos == null || idsDetallesEspecificos.Contains(d.IdPedidoDetalle))
            .Where(d => d.Producto == null || !tiposServicio.Contains(d.Producto.TipoItem)) // Excluir servicios
            .ToList() ?? new();

        if (!detalles.Any())
        {
            resultados.Add(new ResultadoComanda { Exitoso = true, Mensaje = "No hay items nuevos para enviar" });
            return resultados;
        }

        // Obtener siguiente número de comanda del día
        var fechaHoy = DateTime.Today;
        var ultimaComanda = await ctx.Set<PedidoDetalle>()
            .Where(d => d.NumeroComanda != null && d.FechaEnvioCocina != null && d.FechaEnvioCocina.Value.Date == fechaHoy)
            .MaxAsync(d => (int?)d.NumeroComanda) ?? 0;
        var numeroComanda = ultimaComanda + 1;

        // Agrupar por destino (según categoría del producto o configuración)
        // Por ahora todos van a COCINA, se puede mejorar con categoría de producto
        var itemsCocina = detalles.Select(d => new ItemComanda
        {
            Cantidad = (int)d.Cantidad,
            Descripcion = d.Descripcion,
            Modificadores = d.Modificadores,
            NotasCocina = d.NotasCocina,
            EsUrgente = false // Se puede agregar campo al detalle
        }).ToList();

        if (itemsCocina.Any())
        {
            var comandaCocina = new DatosComanda
            {
                NumeroComanda = numeroComanda,
                NumeroPedido = pedido.NumeroPedido,
                NombreMesa = pedido.Mesa?.Nombre,
                NumeroMesa = pedido.Mesa?.Numero,
                Mesero = mesero,
                Comensales = pedido.Comensales,
                TipoDestino = "COCINA",
                Items = itemsCocina
            };

            var resultado = await ImprimirComandaAsync(comandaCocina, caja);
            resultados.Add(resultado);

            // Marcar como enviados a cocina
            if (resultado.Exitoso)
            {
                foreach (var detalle in detalles)
                {
                    detalle.EnviadoCocina = true;
                    detalle.FechaEnvioCocina = DateTime.Now;
                    detalle.NumeroComanda = numeroComanda;
                    detalle.Estado = "EnPreparacion";
                }
                await ctx.SaveChangesAsync();
            }
        }

        return resultados;
    }

    /// <summary>
    /// Prueba la conexión a una impresora de red.
    /// </summary>
    public async Task<ResultadoComanda> ProbarConexionAsync(string ip, int puerto = 9100)
    {
        try
        {
            using var client = new TcpClient();
            var connectTask = client.ConnectAsync(ip, puerto);

            if (await Task.WhenAny(connectTask, Task.Delay(3000)) != connectTask)
            {
                return new ResultadoComanda
                {
                    Exitoso = false,
                    Mensaje = $"Timeout conectando a {ip}:{puerto}"
                };
            }

            await connectTask;

            return new ResultadoComanda
            {
                Exitoso = true,
                Mensaje = $"Conexión exitosa a {ip}:{puerto}"
            };
        }
        catch (Exception ex)
        {
            return new ResultadoComanda
            {
                Exitoso = false,
                Mensaje = $"Error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Imprime una comanda de prueba.
    /// </summary>
    public async Task<ResultadoComanda> ImprimirPruebaAsync(Caja caja, string destino = "COCINA")
    {
        var comandaPrueba = new DatosComanda
        {
            NumeroComanda = 999,
            NumeroPedido = 0,
            NombreMesa = "PRUEBA",
            Comensales = 2,
            TipoDestino = destino,
            Items = new List<ItemComanda>
            {
                new() { Cantidad = 2, Descripcion = "HAMBURGUESA COMPLETA", Modificadores = "Sin cebolla" },
                new() { Cantidad = 1, Descripcion = "PIZZA NAPOLITANA", NotasCocina = "Extra queso" },
                new() { Cantidad = 3, Descripcion = "GASEOSA 500ML" }
            },
            Observaciones = "Esta es una comanda de prueba para verificar la configuración de la impresora."
        };

        return await ImprimirComandaAsync(comandaPrueba, caja);
    }
}

/// <summary>
/// Resultado de operación de comanda.
/// </summary>
public class ResultadoComanda
{
    public bool Exitoso { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public int? NumeroComanda { get; set; }
}
