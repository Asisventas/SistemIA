using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html.simpleparser;
using System.Text;
using System.Text.RegularExpressions;

namespace SistemIA.Services
{
    /// <summary>
    /// Servicio para convertir HTML de informes a PDF usando iTextSharp.
    /// Diseñado para ser usado desde el componente EnviarInformeCorreo.
    /// </summary>
    public interface IHtmlToPdfService
    {
        /// <summary>
        /// Convierte contenido HTML a PDF
        /// </summary>
        /// <param name="htmlContent">Contenido HTML a convertir</param>
        /// <param name="titulo">Título del documento</param>
        /// <param name="filtros">Filtros aplicados (opcional)</param>
        /// <param name="nombreEmpresa">Nombre de la empresa</param>
        /// <param name="nombreSucursal">Nombre de la sucursal</param>
        /// <returns>Bytes del PDF generado</returns>
        byte[] ConvertirHtmlAPdf(string htmlContent, string titulo, string? filtros = null, 
            string? nombreEmpresa = null, string? nombreSucursal = null);
    }

    public class HtmlToPdfService : IHtmlToPdfService
    {
        private readonly ILogger<HtmlToPdfService> _logger;

        public HtmlToPdfService(ILogger<HtmlToPdfService> logger)
        {
            _logger = logger;
        }

        public byte[] ConvertirHtmlAPdf(string htmlContent, string titulo, string? filtros = null,
            string? nombreEmpresa = null, string? nombreSucursal = null)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                
                // Configurar documento A4 horizontal para tablas anchas
                var document = new Document(PageSize.A4.Rotate(), 20, 20, 60, 40);
                var writer = PdfWriter.GetInstance(document, memoryStream);
                
                // Agregar encabezado y pie de página
                writer.PageEvent = new InformePdfPageEventHelper(titulo, nombreEmpresa, nombreSucursal);
                
                document.Open();

                // Fuentes
                var fontTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.DarkGray);
                var fontSubtitulo = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.Gray);
                var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.Black);

                // Encabezado del documento
                AgregarEncabezado(document, titulo, filtros, nombreEmpresa, nombreSucursal, fontTitulo, fontSubtitulo);

                // Convertir HTML a elementos PDF
                ConvertirHtmlAElementos(document, htmlContent, fontNormal);

                document.Close();
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al convertir HTML a PDF");
                throw;
            }
        }

        private void AgregarEncabezado(Document document, string titulo, string? filtros,
            string? nombreEmpresa, string? nombreSucursal, Font fontTitulo, Font fontSubtitulo)
        {
            // Nombre de empresa
            if (!string.IsNullOrEmpty(nombreEmpresa))
            {
                var pEmpresa = new Paragraph(nombreEmpresa, fontTitulo)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 2
                };
                document.Add(pEmpresa);
            }

            // Nombre de sucursal
            if (!string.IsNullOrEmpty(nombreSucursal))
            {
                var pSucursal = new Paragraph(nombreSucursal, fontSubtitulo)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 5
                };
                document.Add(pSucursal);
            }

            // Título del informe
            var pTitulo = new Paragraph(titulo, fontTitulo)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 5
            };
            document.Add(pTitulo);

            // Filtros aplicados
            if (!string.IsNullOrEmpty(filtros))
            {
                var pFiltros = new Paragraph($"Filtros: {filtros}", fontSubtitulo)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10
                };
                document.Add(pFiltros);
            }

            // Fecha de generación
            var pFecha = new Paragraph($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}", fontSubtitulo)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 15
            };
            document.Add(pFecha);

            // Línea separadora
            var linea = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(1f, 100f, BaseColor.LightGray, Element.ALIGN_CENTER, -2)))
            {
                SpacingAfter = 10
            };
            document.Add(linea);
        }

        private void ConvertirHtmlAElementos(Document document, string htmlContent, Font fontNormal)
        {
            // Extraer tablas del HTML y convertirlas
            var tablas = ExtraerTablas(htmlContent);
            
            if (tablas.Any())
            {
                foreach (var tablaHtml in tablas)
                {
                    var pdfTable = ConvertirTablaHtmlAPdf(tablaHtml, fontNormal);
                    if (pdfTable != null)
                    {
                        document.Add(pdfTable);
                        document.Add(new Paragraph(" ") { SpacingAfter = 10 });
                    }
                }
            }
            else
            {
                // Si no hay tablas, intentar mostrar el texto plano
                var textoPlano = Regex.Replace(htmlContent, "<[^>]+>", " ");
                textoPlano = Regex.Replace(textoPlano, @"\s+", " ").Trim();
                if (!string.IsNullOrEmpty(textoPlano))
                {
                    document.Add(new Paragraph(textoPlano, fontNormal));
                }
            }
        }

        private List<string> ExtraerTablas(string html)
        {
            var tablas = new List<string>();
            var matches = Regex.Matches(html, @"<table[^>]*>(.*?)</table>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            
            foreach (Match match in matches)
            {
                tablas.Add(match.Value);
            }
            
            return tablas;
        }

        private PdfPTable? ConvertirTablaHtmlAPdf(string tablaHtml, Font fontNormal)
        {
            try
            {
                // Extraer filas
                var filas = Regex.Matches(tablaHtml, @"<tr[^>]*>(.*?)</tr>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                if (filas.Count == 0) return null;

                // Determinar número de columnas de la primera fila
                var primeraFila = filas[0].Groups[1].Value;
                var celdas = Regex.Matches(primeraFila, @"<(th|td)[^>]*>(.*?)</\1>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                int numColumnas = celdas.Count;
                
                if (numColumnas == 0) return null;

                // Crear tabla PDF
                var pdfTable = new PdfPTable(numColumnas)
                {
                    WidthPercentage = 100,
                    SpacingBefore = 5,
                    SpacingAfter = 5
                };

                // Fuentes
                var fontHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.White);
                var fontCell = FontFactory.GetFont(FontFactory.HELVETICA, 7, BaseColor.Black);
                var fontTotal = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7, BaseColor.Black);

                bool esEncabezado = true;

                foreach (Match fila in filas)
                {
                    var contenidoFila = fila.Groups[1].Value;
                    var celdasFila = Regex.Matches(contenidoFila, @"<(th|td)[^>]*>(.*?)</\1>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    
                    // Detectar si es fila de totales
                    bool esFilaTotal = contenidoFila.Contains("total", StringComparison.OrdinalIgnoreCase) ||
                                       contenidoFila.Contains("class=\"total", StringComparison.OrdinalIgnoreCase) ||
                                       contenidoFila.Contains("fw-bold", StringComparison.OrdinalIgnoreCase);

                    foreach (Match celda in celdasFila)
                    {
                        var tipoCelda = celda.Groups[1].Value.ToLower();
                        var contenido = celda.Groups[2].Value;
                        
                        // Limpiar HTML del contenido
                        contenido = Regex.Replace(contenido, "<[^>]+>", " ");
                        contenido = System.Net.WebUtility.HtmlDecode(contenido);
                        contenido = Regex.Replace(contenido, @"\s+", " ").Trim();

                        // Determinar alineación
                        int alineacion = Element.ALIGN_LEFT;
                        if (celda.Value.Contains("text-end") || celda.Value.Contains("text-right"))
                            alineacion = Element.ALIGN_RIGHT;
                        else if (celda.Value.Contains("text-center"))
                            alineacion = Element.ALIGN_CENTER;

                        // Crear celda PDF
                        Font fuente;
                        BaseColor colorFondo;
                        
                        if (tipoCelda == "th" || esEncabezado)
                        {
                            fuente = fontHeader;
                            colorFondo = new BaseColor(13, 110, 253); // Bootstrap primary
                        }
                        else if (esFilaTotal)
                        {
                            fuente = fontTotal;
                            colorFondo = new BaseColor(233, 236, 239); // Bootstrap light
                        }
                        else
                        {
                            fuente = fontCell;
                            colorFondo = BaseColor.White;
                        }

                        var pdfCell = new PdfPCell(new Phrase(contenido, fuente))
                        {
                            BackgroundColor = colorFondo,
                            HorizontalAlignment = alineacion,
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            Padding = 4,
                            BorderColor = new BaseColor(222, 226, 230)
                        };

                        pdfTable.AddCell(pdfCell);
                    }

                    // Después de la primera fila con <th>, ya no es encabezado
                    if (contenidoFila.Contains("<th", StringComparison.OrdinalIgnoreCase))
                        esEncabezado = false;
                    else if (esEncabezado && celdasFila.Count > 0)
                        esEncabezado = false;
                }

                return pdfTable;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al convertir tabla HTML a PDF");
                return null;
            }
        }

        /// <summary>
        /// Event handler para agregar encabezado/pie de página en cada página
        /// </summary>
        private class InformePdfPageEventHelper : PdfPageEventHelper
        {
            private readonly string _titulo;
            private readonly string? _empresa;
            private readonly string? _sucursal;

            public InformePdfPageEventHelper(string titulo, string? empresa, string? sucursal)
            {
                _titulo = titulo;
                _empresa = empresa;
                _sucursal = sucursal;
            }

            public override void OnEndPage(PdfWriter writer, Document document)
            {
                var fontPie = FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.Gray);
                
                // Pie de página con número de página
                var cb = writer.DirectContent;
                var pie = new Phrase($"Página {writer.PageNumber} | {_titulo} | Generado: {DateTime.Now:dd/MM/yyyy HH:mm}", fontPie);
                
                ColumnText.ShowTextAligned(cb, Element.ALIGN_CENTER, pie,
                    (document.PageSize.Width) / 2,
                    document.Bottom - 20, 0);
            }
        }
    }
}
