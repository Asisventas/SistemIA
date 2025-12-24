using iTextSharp.text;
using iTextSharp.text.pdf;
using SistemIA.Models;
using System.Text;

namespace SistemIA.Services
{
    public class InformeAsistenciaService
    {
        public async Task<byte[]> GenerarInformePDF(List<Asistencia> asistencias, Sucursal sucursal, DateTime fechaDesde, DateTime fechaHasta, string tipoInforme)
        {
            using (var memoryStream = new MemoryStream())
            {
                // Configurar el documento PDF
                var document = new Document(PageSize.A4, 25, 25, 80, 50); // Aumentar margen superior para logo
                var writer = PdfWriter.GetInstance(document, memoryStream);
                
                // Agregar eventos para pie de página
                var footerEvent = new PieEventHandler(sucursal);
                writer.PageEvent = footerEvent;
                
                document.Open();

                // Configurar fuentes
                var fontTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, new BaseColor(128, 128, 128));
                var fontSubtitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, new BaseColor(128, 128, 128));
                var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 10, new BaseColor(0, 0, 0));
                var fontTabla = FontFactory.GetFont(FontFactory.HELVETICA, 9, new BaseColor(0, 0, 0));

                // Encabezado del documento con logo
                AgregarEncabezadoConLogoPDF(document, sucursal, fechaDesde, fechaHasta, fontTitulo, fontSubtitulo, fontNormal);

                // Agregar contenido según el tipo de informe
                switch (tipoInforme)
                {
                    case "DETALLADO":
                        AgregarInformeDetalladoConTiemposPDF(document, asistencias, fontTabla, fontSubtitulo);
                        break;
                    case "RESUMEN":
                        AgregarInformeResumenPDF(document, asistencias, fontTabla, fontSubtitulo);
                        break;
                    case "ESTADISTICO":
                        AgregarInformeEstadisticoPDF(document, asistencias, fontTabla, fontSubtitulo);
                        break;
                }

                document.Close();
                return memoryStream.ToArray();
            }
        }

        public async Task<byte[]> GenerarInformeWord(List<Asistencia> asistencias, Sucursal sucursal, DateTime fechaDesde, DateTime fechaHasta, string tipoInforme)
        {
            // Para simplificar, generamos un HTML que se puede abrir como Word
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset='utf-8'>");
            html.AppendLine("<title>Informe de Asistencia</title>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
            html.AppendLine("h1, h2 { text-align: center; color: #333; }");
            html.AppendLine("table { width: 100%; border-collapse: collapse; margin: 20px 0; }");
            html.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
            html.AppendLine("th { background-color: #f2f2f2; font-weight: bold; }");
            html.AppendLine(".header { text-align: center; margin-bottom: 30px; }");
            html.AppendLine(".footer { text-align: center; margin-top: 30px; font-size: 12px; color: #666; }");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");

            // Encabezado
            html.AppendLine("<div class='header'>");
            html.AppendLine($"<h1>{sucursal.NombreEmpresa}</h1>");
            html.AppendLine($"<h2>{sucursal.NombreSucursal}</h2>");
            if (!string.IsNullOrEmpty(sucursal.Direccion))
                html.AppendLine($"<p>Dirección: {sucursal.Direccion}</p>");
            if (!string.IsNullOrEmpty(sucursal.Telefono))
                html.AppendLine($"<p>Teléfono: {sucursal.Telefono}</p>");
            html.AppendLine("<h2>INFORME DE ASISTENCIA</h2>");
            html.AppendLine($"<p>Período: {fechaDesde:dd/MM/yyyy} - {fechaHasta:dd/MM/yyyy}</p>");
            html.AppendLine("</div>");

            // Contenido según tipo
            switch (tipoInforme)
            {
                case "DETALLADO":
                    AgregarInformeDetalladoHTML(html, asistencias);
                    break;
                case "RESUMEN":
                    AgregarInformeResumenHTML(html, asistencias);
                    break;
                case "ESTADISTICO":
                    AgregarInformeEstadisticoHTML(html, asistencias);
                    break;
            }

            // Pie de página
            html.AppendLine("<div class='footer'>");
            html.AppendLine($"<p>Informe generado el {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>");
            html.AppendLine("</div>");

            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return Encoding.UTF8.GetBytes(html.ToString());
        }

        private void AgregarEncabezadoPDF(Document document, Sucursal sucursal, DateTime fechaDesde, DateTime fechaHasta, 
            Font fontTitulo, Font fontSubtitulo, Font fontNormal)
        {
            // Título principal
            var titulo = new Paragraph($"{sucursal.NombreEmpresa}", fontTitulo)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 10
            };
            document.Add(titulo);

            // Información de la sucursal
            var infoSucursal = new Paragraph($"{sucursal.NombreSucursal}", fontSubtitulo)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 5
            };
            document.Add(infoSucursal);

            if (!string.IsNullOrEmpty(sucursal.Direccion))
            {
                var direccion = new Paragraph($"Dirección: {sucursal.Direccion}", fontNormal)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 5
                };
                document.Add(direccion);
            }

            if (!string.IsNullOrEmpty(sucursal.Telefono))
            {
                var telefono = new Paragraph($"Teléfono: {sucursal.Telefono}", fontNormal)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 15
                };
                document.Add(telefono);
            }

            // Título del informe
            var tituloInforme = new Paragraph("INFORME DE ASISTENCIA", fontTitulo)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 10
            };
            document.Add(tituloInforme);

            // Período del informe
            var periodo = new Paragraph($"Período: {fechaDesde:dd/MM/yyyy} - {fechaHasta:dd/MM/yyyy}", fontSubtitulo)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20
            };
            document.Add(periodo);
        }

        private void AgregarEncabezadoConLogoPDF(Document document, Sucursal sucursal, DateTime fechaDesde, DateTime fechaHasta, 
            Font fontTitulo, Font fontSubtitulo, Font fontNormal)
        {
            try
            {
                // Tabla para organizar logo y información de empresa
                var tablaEncabezado = new PdfPTable(2) { WidthPercentage = 100 };
                tablaEncabezado.SetWidths(new float[] { 20f, 80f });

                // Celda para el logo
                var celdaLogo = new PdfPCell();
                celdaLogo.Border = Rectangle.NO_BORDER;
                celdaLogo.HorizontalAlignment = Element.ALIGN_CENTER;
                celdaLogo.VerticalAlignment = Element.ALIGN_MIDDLE;

                if (sucursal.Logo != null && sucursal.Logo.Length > 0)
                {
                    try
                    {
                        var logo = Image.GetInstance(sucursal.Logo);
                        logo.ScaleToFit(60f, 60f);
                        celdaLogo.AddElement(logo);
                    }
                    catch
                    {
                        // Si no se puede cargar el logo, agregar un placeholder
                        var placeholderLogo = new Paragraph("LOGO", FontFactory.GetFont(FontFactory.HELVETICA, 8, new BaseColor(128, 128, 128)));
                        placeholderLogo.Alignment = Element.ALIGN_CENTER;
                        celdaLogo.AddElement(placeholderLogo);
                    }
                }
                else
                {
                    var placeholderLogo = new Paragraph("LOGO", FontFactory.GetFont(FontFactory.HELVETICA, 8, new BaseColor(128, 128, 128)));
                    placeholderLogo.Alignment = Element.ALIGN_CENTER;
                    celdaLogo.AddElement(placeholderLogo);
                }

                tablaEncabezado.AddCell(celdaLogo);

                // Celda para información de la empresa
                var celdaInfo = new PdfPCell();
                celdaInfo.Border = Rectangle.NO_BORDER;
                celdaInfo.HorizontalAlignment = Element.ALIGN_LEFT;

                var tituloEmpresa = new Paragraph($"{sucursal.NombreEmpresa}", fontTitulo);
                celdaInfo.AddElement(tituloEmpresa);

                var infoEmpresa = new Paragraph($"RUC: {sucursal.RUC} - {sucursal.Direccion}", fontNormal);
                celdaInfo.AddElement(infoEmpresa);

                if (!string.IsNullOrEmpty(sucursal.Telefono))
                {
                    var telefono = new Paragraph($"Teléfono: {sucursal.Telefono}", fontNormal);
                    celdaInfo.AddElement(telefono);
                }

                tablaEncabezado.AddCell(celdaInfo);
                document.Add(tablaEncabezado);

                // Título del informe
                var tituloInforme = new Paragraph("INFORME DE ASISTENCIA", fontTitulo)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingBefore = 20,
                    SpacingAfter = 10
                };
                document.Add(tituloInforme);

                // Período del informe
                var periodo = new Paragraph($"Período: {fechaDesde:dd/MM/yyyy} - {fechaHasta:dd/MM/yyyy}", fontSubtitulo)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20
                };
                document.Add(periodo);

                // Línea separadora
                var linea = new Paragraph("_".PadRight(80, '_'), FontFactory.GetFont(FontFactory.HELVETICA, 8, new BaseColor(128, 128, 128)))
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 15
                };
                document.Add(linea);
            }
            catch (Exception)
            {
                // Encabezado de respaldo sin logo
                var titulo = new Paragraph($"{sucursal.NombreEmpresa}", fontTitulo)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10
                };
                document.Add(titulo);

                var subtitulo = new Paragraph($"RUC: {sucursal.RUC}", fontNormal)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 5
                };
                document.Add(subtitulo);

                var tituloInforme = new Paragraph("INFORME DE ASISTENCIA", fontTitulo)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingBefore = 20,
                    SpacingAfter = 10
                };
                document.Add(tituloInforme);

                var periodo = new Paragraph($"Período: {fechaDesde:dd/MM/yyyy} - {fechaHasta:dd/MM/yyyy}", fontSubtitulo)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20
                };
                document.Add(periodo);
            }
        }

        private void AgregarInformeDetalladoConTiemposPDF(Document document, List<Asistencia> asistencias, 
            Font fontTabla, Font fontSubtitulo)
        {
            var subtitulo = new Paragraph("DETALLE DE REGISTROS CON ANÁLISIS DE TIEMPOS", fontSubtitulo)
            {
                SpacingAfter = 15
            };
            document.Add(subtitulo);

            // Crear tabla con columna adicional para retrasos/adelantos
            var tabla = new PdfPTable(6) { WidthPercentage = 100 };
            tabla.SetWidths(new float[] { 12f, 12f, 25f, 12f, 15f, 24f });

            // Encabezados
            tabla.AddCell(new PdfPCell(new Phrase("Fecha", fontSubtitulo)) { BackgroundColor = new BaseColor(200, 200, 200), Padding = 8 });
            tabla.AddCell(new PdfPCell(new Phrase("Hora", fontSubtitulo)) { BackgroundColor = new BaseColor(200, 200, 200), Padding = 8 });
            tabla.AddCell(new PdfPCell(new Phrase("Usuario", fontSubtitulo)) { BackgroundColor = new BaseColor(200, 200, 200), Padding = 8 });
            tabla.AddCell(new PdfPCell(new Phrase("Tipo", fontSubtitulo)) { BackgroundColor = new BaseColor(200, 200, 200), Padding = 8 });
            tabla.AddCell(new PdfPCell(new Phrase("Sucursal", fontSubtitulo)) { BackgroundColor = new BaseColor(200, 200, 200), Padding = 8 });
            tabla.AddCell(new PdfPCell(new Phrase("Estado de Tiempo", fontSubtitulo)) { BackgroundColor = new BaseColor(200, 200, 200), Padding = 8 });

            // Datos con análisis de tiempos
            foreach (var asistencia in asistencias)
            {
                tabla.AddCell(new PdfPCell(new Phrase(asistencia.FechaHora.ToString("dd/MM/yyyy"), fontTabla)) { Padding = 5 });
                tabla.AddCell(new PdfPCell(new Phrase(asistencia.FechaHora.ToString("HH:mm:ss"), fontTabla)) { Padding = 5 });
                tabla.AddCell(new PdfPCell(new Phrase($"{asistencia.Usuario?.Nombres ?? ""} {asistencia.Usuario?.Apellidos ?? ""}", fontTabla)) { Padding = 5 });
                tabla.AddCell(new PdfPCell(new Phrase(asistencia.TipoRegistro, fontTabla)) { Padding = 5 });
                tabla.AddCell(new PdfPCell(new Phrase(asistencia.SucursalNavigation?.NombreSucursal ?? "", fontTabla)) { Padding = 5 });
                
                // Calcular estado de tiempo
                var estadoTiempo = CalcularEstadoTiempo(asistencia);
                var colorTiempo = ObtenerColorEstadoTiempo(estadoTiempo);
                var celdaEstado = new PdfPCell(new Phrase(estadoTiempo, fontTabla)) 
                { 
                    Padding = 5,
                    BackgroundColor = colorTiempo
                };
                tabla.AddCell(celdaEstado);
            }

            document.Add(tabla);

            // Resumen con estadísticas de tiempo
            AgregarResumenTiempos(document, asistencias, fontSubtitulo, fontTabla);
        }

        private string CalcularEstadoTiempo(Asistencia asistencia)
        {
            try
            {
                // Horarios estándar (esto debería venir de la base de datos en una implementación real)
                var horaEntradaEstandar = new TimeSpan(8, 0, 0); // 08:00
                var horaSalidaEstandar = new TimeSpan(17, 0, 0); // 17:00
                var toleranciaMinutos = 15; // 15 minutos de tolerancia

                var horaRegistro = asistencia.FechaHora.TimeOfDay;

                if (asistencia.TipoRegistro == "Entrada")
                {
                    var diferencia = horaRegistro - horaEntradaEstandar;
                    if (diferencia.TotalMinutes <= -toleranciaMinutos)
                    {
                        return $"Adelanto {Math.Abs((int)diferencia.TotalMinutes)} min";
                    }
                    else if (diferencia.TotalMinutes > toleranciaMinutos)
                    {
                        return $"Retraso {(int)diferencia.TotalMinutes} min";
                    }
                    else
                    {
                        return "A tiempo";
                    }
                }
                else if (asistencia.TipoRegistro == "Salida")
                {
                    var diferencia = horaRegistro - horaSalidaEstandar;
                    if (diferencia.TotalMinutes <= -toleranciaMinutos)
                    {
                        return $"Salida temprana {Math.Abs((int)diferencia.TotalMinutes)} min";
                    }
                    else if (diferencia.TotalMinutes > toleranciaMinutos)
                    {
                        return $"Tiempo extra {(int)diferencia.TotalMinutes} min";
                    }
                    else
                    {
                        return "A tiempo";
                    }
                }

                return "Normal";
            }
            catch
            {
                return "Sin datos";
            }
        }

        private BaseColor ObtenerColorEstadoTiempo(string estadoTiempo)
        {
            if (estadoTiempo.Contains("Retraso") || estadoTiempo.Contains("Salida temprana"))
            {
                return new BaseColor(255, 200, 200); // Rojo claro
            }
            else if (estadoTiempo.Contains("Adelanto") || estadoTiempo.Contains("Tiempo extra"))
            {
                return new BaseColor(200, 255, 200); // Verde claro
            }
            else if (estadoTiempo == "A tiempo")
            {
                return new BaseColor(200, 230, 255); // Azul claro
            }
            else
            {
                return new BaseColor(255, 255, 255); // Blanco por defecto
            }
        }

        private void AgregarResumenTiempos(Document document, List<Asistencia> asistencias, Font fontSubtitulo, Font fontTabla)
        {
            var entradas = asistencias.Where(a => a.TipoRegistro == "Entrada").ToList();
            var salidas = asistencias.Where(a => a.TipoRegistro == "Salida").ToList();

            var entradasRetraso = entradas.Count(a => CalcularEstadoTiempo(a).Contains("Retraso"));
            var entradasAdelanto = entradas.Count(a => CalcularEstadoTiempo(a).Contains("Adelanto"));
            var entradasATiempo = entradas.Count(a => CalcularEstadoTiempo(a) == "A tiempo");

            var resumenTiempos = new Paragraph("RESUMEN DE PUNTUALIDAD", fontSubtitulo)
            {
                SpacingBefore = 20,
                SpacingAfter = 10
            };
            document.Add(resumenTiempos);

            var estadisticas = new StringBuilder();
            estadisticas.AppendLine($"• Total de registros: {asistencias.Count}");
            estadisticas.AppendLine($"• Entradas a tiempo: {entradasATiempo}");
            estadisticas.AppendLine($"• Entradas con retraso: {entradasRetraso}");
            estadisticas.AppendLine($"• Entradas adelantadas: {entradasAdelanto}");

            if (entradas.Count > 0)
            {
                var porcentajePuntualidad = ((double)entradasATiempo / entradas.Count) * 100;
                estadisticas.AppendLine($"• Porcentaje de puntualidad: {porcentajePuntualidad:F1}%");
            }

            var resumen = new Paragraph(estadisticas.ToString(), fontTabla)
            {
                SpacingAfter = 10
            };
            document.Add(resumen);
        }

        private void AgregarInformeDetalladoPDF(Document document, List<Asistencia> asistencias, 
            Font fontTabla, Font fontSubtitulo)
        {
            var subtitulo = new Paragraph("DETALLE DE REGISTROS", fontSubtitulo)
            {
                SpacingAfter = 15
            };
            document.Add(subtitulo);

            // Crear tabla
            var tabla = new PdfPTable(5) { WidthPercentage = 100 };
            tabla.SetWidths(new float[] { 15f, 15f, 30f, 15f, 25f });

            // Encabezados
            tabla.AddCell(new PdfPCell(new Phrase("Fecha", fontSubtitulo)) { BackgroundColor = new BaseColor(200, 200, 200), Padding = 8 });
            tabla.AddCell(new PdfPCell(new Phrase("Hora", fontSubtitulo)) { BackgroundColor = new BaseColor(200, 200, 200), Padding = 8 });
            tabla.AddCell(new PdfPCell(new Phrase("Usuario", fontSubtitulo)) { BackgroundColor = new BaseColor(200, 200, 200), Padding = 8 });
            tabla.AddCell(new PdfPCell(new Phrase("Tipo", fontSubtitulo)) { BackgroundColor = new BaseColor(200, 200, 200), Padding = 8 });
            tabla.AddCell(new PdfPCell(new Phrase("Sucursal", fontSubtitulo)) { BackgroundColor = new BaseColor(200, 200, 200), Padding = 8 });

            // Datos
            foreach (var asistencia in asistencias)
            {
                tabla.AddCell(new PdfPCell(new Phrase(asistencia.FechaHora.ToString("dd/MM/yyyy"), fontTabla)) { Padding = 5 });
                tabla.AddCell(new PdfPCell(new Phrase(asistencia.FechaHora.ToString("HH:mm:ss"), fontTabla)) { Padding = 5 });
                tabla.AddCell(new PdfPCell(new Phrase($"{asistencia.Usuario?.Nombres ?? ""} {asistencia.Usuario?.Apellidos ?? ""}", fontTabla)) { Padding = 5 });
                tabla.AddCell(new PdfPCell(new Phrase(asistencia.TipoRegistro, fontTabla)) { Padding = 5 });
                tabla.AddCell(new PdfPCell(new Phrase(asistencia.SucursalNavigation?.NombreSucursal ?? "", fontTabla)) { Padding = 5 });
            }

            document.Add(tabla);

            // Resumen
            var resumen = new Paragraph($"\nTotal de registros: {asistencias.Count}", fontSubtitulo)
            {
                SpacingBefore = 20
            };
            document.Add(resumen);
        }

        private void AgregarInformeResumenPDF(Document document, List<Asistencia> asistencias, 
            Font fontTabla, Font fontSubtitulo)
        {
            var subtitulo = new Paragraph("RESUMEN POR USUARIO", fontSubtitulo)
            {
                SpacingAfter = 15
            };
            document.Add(subtitulo);

            var resumenPorUsuario = asistencias
                .GroupBy(a => new { a.Id_Usuario, a.Usuario?.Nombres, a.Usuario?.Apellidos })
                .Select(g => new
                {
                    Usuario = $"{g.Key.Nombres} {g.Key.Apellidos}",
                    TotalRegistros = g.Count(),
                    Entradas = g.Count(a => a.TipoRegistro == "Entrada"),
                    Salidas = g.Count(a => a.TipoRegistro == "Salida")
                })
                .OrderBy(r => r.Usuario)
                .ToList();

            // Crear tabla
            var tabla = new PdfPTable(4) { WidthPercentage = 100 };
            tabla.SetWidths(new float[] { 40f, 20f, 20f, 20f });

            // Encabezados
            tabla.AddCell(new PdfPCell(new Phrase("Usuario", fontSubtitulo)) { BackgroundColor = new BaseColor(200, 200, 200), Padding = 8 });
            tabla.AddCell(new PdfPCell(new Phrase("Total", fontSubtitulo)) { BackgroundColor = new BaseColor(200, 200, 200), Padding = 8 });
            tabla.AddCell(new PdfPCell(new Phrase("Entradas", fontSubtitulo)) { BackgroundColor = new BaseColor(200, 200, 200), Padding = 8 });
            tabla.AddCell(new PdfPCell(new Phrase("Salidas", fontSubtitulo)) { BackgroundColor = new BaseColor(200, 200, 200), Padding = 8 });

            // Datos
            foreach (var resumen in resumenPorUsuario)
            {
                tabla.AddCell(new PdfPCell(new Phrase(resumen.Usuario, fontTabla)) { Padding = 5 });
                tabla.AddCell(new PdfPCell(new Phrase(resumen.TotalRegistros.ToString(), fontTabla)) { Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER });
                tabla.AddCell(new PdfPCell(new Phrase(resumen.Entradas.ToString(), fontTabla)) { Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER });
                tabla.AddCell(new PdfPCell(new Phrase(resumen.Salidas.ToString(), fontTabla)) { Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER });
            }

            document.Add(tabla);
        }

        private void AgregarInformeEstadisticoPDF(Document document, List<Asistencia> asistencias, 
            Font fontTabla, Font fontSubtitulo)
        {
            var subtitulo = new Paragraph("ESTADÍSTICAS GENERALES", fontSubtitulo)
            {
                SpacingAfter = 15
            };
            document.Add(subtitulo);

            var totalRegistros = asistencias.Count;
            var totalEntradas = asistencias.Count(a => a.TipoRegistro == "Entrada");
            var totalSalidas = asistencias.Count(a => a.TipoRegistro == "Salida");
            var usuariosUnicos = asistencias.Select(a => a.Id_Usuario).Distinct().Count();

            var estadisticas = new List<Paragraph>
            {
                new Paragraph($"Total de registros: {totalRegistros}", fontTabla) { SpacingAfter = 8 },
                new Paragraph($"Total de entradas: {totalEntradas}", fontTabla) { SpacingAfter = 8 },
                new Paragraph($"Total de salidas: {totalSalidas}", fontTabla) { SpacingAfter = 8 },
                new Paragraph($"Usuarios diferentes: {usuariosUnicos}", fontTabla) { SpacingAfter = 8 }
            };

            foreach (var estadistica in estadisticas)
            {
                document.Add(estadistica);
            }
        }

        private void AgregarPiePaginaPDF(Document document, Font fontNormal)
        {
            var piePagina = new Paragraph($"\nInforme generado el {DateTime.Now:dd/MM/yyyy HH:mm:ss}", fontNormal)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingBefore = 30
            };
            document.Add(piePagina);
        }

        // Métodos para HTML (que se puede abrir como Word)
        private void AgregarInformeDetalladoHTML(StringBuilder html, List<Asistencia> asistencias)
        {
            html.AppendLine("<h3>DETALLE DE REGISTROS</h3>");
            html.AppendLine("<table>");
            html.AppendLine("<tr><th>Fecha</th><th>Hora</th><th>Usuario</th><th>Tipo</th><th>Sucursal</th></tr>");

            foreach (var asistencia in asistencias)
            {
                html.AppendLine("<tr>");
                html.AppendLine($"<td>{asistencia.FechaHora:dd/MM/yyyy}</td>");
                html.AppendLine($"<td>{asistencia.FechaHora:HH:mm:ss}</td>");
                html.AppendLine($"<td>{asistencia.Usuario?.Nombres ?? ""} {asistencia.Usuario?.Apellidos ?? ""}</td>");
                html.AppendLine($"<td>{asistencia.TipoRegistro}</td>");
                html.AppendLine($"<td>{asistencia.SucursalNavigation?.NombreSucursal ?? ""}</td>");
                html.AppendLine("</tr>");
            }

            html.AppendLine("</table>");
            html.AppendLine($"<p><strong>Total de registros: {asistencias.Count}</strong></p>");
        }

        private void AgregarInformeResumenHTML(StringBuilder html, List<Asistencia> asistencias)
        {
            var resumenPorUsuario = asistencias
                .GroupBy(a => new { a.Id_Usuario, a.Usuario?.Nombres, a.Usuario?.Apellidos })
                .Select(g => new
                {
                    Usuario = $"{g.Key.Nombres} {g.Key.Apellidos}",
                    TotalRegistros = g.Count(),
                    Entradas = g.Count(a => a.TipoRegistro == "Entrada"),
                    Salidas = g.Count(a => a.TipoRegistro == "Salida")
                })
                .OrderBy(r => r.Usuario)
                .ToList();

            html.AppendLine("<h3>RESUMEN POR USUARIO</h3>");
            html.AppendLine("<table>");
            html.AppendLine("<tr><th>Usuario</th><th>Total</th><th>Entradas</th><th>Salidas</th></tr>");

            foreach (var resumen in resumenPorUsuario)
            {
                html.AppendLine("<tr>");
                html.AppendLine($"<td>{resumen.Usuario}</td>");
                html.AppendLine($"<td style='text-align: center'>{resumen.TotalRegistros}</td>");
                html.AppendLine($"<td style='text-align: center'>{resumen.Entradas}</td>");
                html.AppendLine($"<td style='text-align: center'>{resumen.Salidas}</td>");
                html.AppendLine("</tr>");
            }

            html.AppendLine("</table>");
        }

        private void AgregarInformeEstadisticoHTML(StringBuilder html, List<Asistencia> asistencias)
        {
            var totalRegistros = asistencias.Count;
            var totalEntradas = asistencias.Count(a => a.TipoRegistro == "Entrada");
            var totalSalidas = asistencias.Count(a => a.TipoRegistro == "Salida");
            var usuariosUnicos = asistencias.Select(a => a.Id_Usuario).Distinct().Count();

            html.AppendLine("<h3>ESTADÍSTICAS GENERALES</h3>");
            html.AppendLine("<ul>");
            html.AppendLine($"<li><strong>Total de registros:</strong> {totalRegistros}</li>");
            html.AppendLine($"<li><strong>Total de entradas:</strong> {totalEntradas}</li>");
            html.AppendLine($"<li><strong>Total de salidas:</strong> {totalSalidas}</li>");
            html.AppendLine($"<li><strong>Usuarios diferentes:</strong> {usuariosUnicos}</li>");
            html.AppendLine("</ul>");
        }
    }

    // Clase para manejar el pie de página en PDF
    public class PieEventHandler : PdfPageEventHelper 
    {
        private readonly Sucursal _sucursal;
        private Font _fontPie = FontFactory.GetFont(FontFactory.HELVETICA, 8, new BaseColor(128, 128, 128));

        public PieEventHandler(Sucursal sucursal)
        {
            _sucursal = sucursal;
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            var cb = writer.DirectContent;
            var template = cb.CreateTemplate(100, 100);
            var pageNumber = writer.PageNumber;
            
            // Información de la empresa en pie izquierdo
            var empresaInfo = new Phrase($"{_sucursal.NombreEmpresa} - RUC: {_sucursal.RUC}", _fontPie);
            ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, empresaInfo, 
                document.LeftMargin, document.BottomMargin - 10, 0);
            
            // Fecha de generación en centro
            var fechaGeneracion = new Phrase($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}", _fontPie);
            ColumnText.ShowTextAligned(cb, Element.ALIGN_CENTER, fechaGeneracion, 
                (document.Right + document.Left) / 2, document.BottomMargin - 10, 0);
            
            // Número de página en pie derecho
            var numeroPagina = new Phrase($"Página {pageNumber}", _fontPie);
            ColumnText.ShowTextAligned(cb, Element.ALIGN_RIGHT, numeroPagina, 
                document.Right, document.BottomMargin - 10, 0);
            
            // Línea separadora
            cb.SetLineWidth(0.5f);
            cb.SetGrayStroke(0.5f);
            cb.MoveTo(document.LeftMargin, document.BottomMargin - 5);
            cb.LineTo(document.Right, document.BottomMargin - 5);
            cb.Stroke();
        }
    }
}
