using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemIA.Models;

namespace SistemIA.Services;

/// <summary>
/// Servicio para generar PDF de presupuestos del sistema usando QuestPDF.
/// Genera exactamente el mismo diseño que PresupuestoSistemaPdf.razor
/// </summary>
public class PdfPresupuestoSistemaService
{
    private readonly IServiceProvider _serviceProvider;

    public PdfPresupuestoSistemaService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<byte[]> GenerarPdfPresupuesto(int idPresupuesto)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var presupuesto = await context.PresupuestosSistema
            .Include(p => p.Sucursal)
            .Include(p => p.Detalles)
            .FirstOrDefaultAsync(p => p.IdPresupuesto == idPresupuesto);

        if (presupuesto == null)
            throw new ArgumentException($"Presupuesto #{idPresupuesto} no encontrado");

        var items = presupuesto.Detalles?.ToList() ?? new List<PresupuestoSistemaDetalle>();

        // Obtener sucursal
        Sucursal? sucursal = presupuesto.Sucursal;
        if (sucursal == null && presupuesto.IdSucursal > 0)
        {
            sucursal = await context.Sucursal.FirstOrDefaultAsync(s => s.Id == presupuesto.IdSucursal);
        }
        sucursal ??= await context.Sucursal.FirstOrDefaultAsync();

        return GenerarPdf(presupuesto, sucursal, items);
    }

    private byte[] GenerarPdf(PresupuestoSistema p, Sucursal? suc, List<PresupuestoSistemaDetalle> items)
    {
        // Calcular totales
        var totalItemsUsd = items.Where(i => !i.EsOpcional || i.Incluido).Sum(i => i.SubtotalUsd);
        var totalItemsGs = items.Where(i => !i.EsOpcional || i.Incluido).Sum(i => i.SubtotalGs);
        var totalContadoUsd = p.PrecioContadoUsd + p.CostoImplementacionUsd + p.CostoCapacitacionUsd + totalItemsUsd;
        var totalContadoGs = p.PrecioContadoGs + p.CostoImplementacionGs + p.CostoCapacitacionGs + totalItemsGs;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(25);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                page.Header().Element(c => ComposeHeader(c, p, suc));
                page.Content().Element(c => ComposeContent(c, p, suc, items, totalItemsUsd, totalItemsGs, totalContadoUsd, totalContadoGs));
                page.Footer().Element(c => ComposeFooter(c, p));
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container, PresupuestoSistema p, Sucursal? suc)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                // Logo
                row.ConstantItem(120).Height(60).AlignMiddle().Element(c =>
                {
                    if (suc?.Logo != null && suc.Logo.Length > 0)
                    {
                        c.Image(suc.Logo).FitArea();
                    }
                    else
                    {
                        c.Border(1).BorderColor(Colors.Grey.Lighten2)
                         .Background(Colors.Grey.Lighten4)
                         .AlignCenter().AlignMiddle()
                         .Text("LOGO").FontSize(10).FontColor(Colors.Grey.Medium);
                    }
                });

                row.RelativeItem().PaddingLeft(15).Column(c =>
                {
                    c.Item().Text(suc?.NombreEmpresa ?? "SistemIA").Bold().FontSize(16);
                    c.Item().Text($"RUC: {suc?.RUC ?? ""}-{suc?.DV?.ToString() ?? ""}").FontSize(9);
                    if (!string.IsNullOrEmpty(suc?.NombreSucursal))
                        c.Item().Text(suc.NombreSucursal).FontSize(9);
                    if (!string.IsNullOrEmpty(suc?.Direccion))
                        c.Item().Text(suc.Direccion).FontSize(8).FontColor(Colors.Grey.Darken1);
                    if (!string.IsNullOrEmpty(suc?.Telefono))
                        c.Item().Text($"Tel: {suc.Telefono}").FontSize(8).FontColor(Colors.Grey.Darken1);
                });

                row.ConstantItem(150).AlignRight().Column(c =>
                {
                    c.Item().Background("#374151").Padding(8).AlignCenter()
                     .Text($"PRESUPUESTO #{p.NumeroPresupuesto}").Bold().FontSize(12).FontColor(Colors.White);
                    c.Item().PaddingTop(5).Text($"Fecha: {p.FechaEmision:dd/MM/yyyy}").FontSize(9);
                    var dias = (p.FechaVigencia - p.FechaEmision).Days;
                    c.Item().Text($"Vigencia: {dias} días").FontSize(8).FontColor(Colors.Grey.Darken1);
                });
            });

            col.Item().PaddingTop(10).LineHorizontal(2).LineColor("#374151");
        });
    }

    private void ComposeContent(IContainer container, PresupuestoSistema p, Sucursal? suc,
        List<PresupuestoSistemaDetalle> items, decimal totalItemsUsd, decimal totalItemsGs,
        decimal totalContadoUsd, decimal totalContadoGs)
    {
        container.PaddingTop(10).Column(col =>
        {
            // DATOS DEL CLIENTE
            col.Item().Element(c => SectionTitle(c, "DATOS DEL CLIENTE"));
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(cd =>
                {
                    cd.ConstantColumn(100);
                    cd.RelativeColumn();
                });

                table.Cell().Text("Cliente:").Bold();
                table.Cell().Text(p.NombreCliente ?? "");

                if (!string.IsNullOrEmpty(p.ContactoCliente))
                {
                    table.Cell().Text("Contacto:").Bold();
                    table.Cell().Text(p.ContactoCliente);
                }

                if (!string.IsNullOrEmpty(p.RucCliente))
                {
                    table.Cell().Text("RUC/CI:").Bold();
                    table.Cell().Text(p.RucCliente);
                }

                if (!string.IsNullOrEmpty(p.TelefonoCliente))
                {
                    table.Cell().Text("Teléfono:").Bold();
                    table.Cell().Text(p.TelefonoCliente);
                }

                if (!string.IsNullOrEmpty(p.EmailCliente))
                {
                    table.Cell().Text("Email:").Bold();
                    table.Cell().Text(p.EmailCliente);
                }
            });

            col.Item().PaddingTop(10);

            // MÓDULOS INCLUIDOS - con descripciones detalladas
            col.Item().Element(c => SectionTitle(c, "FUNCIONALIDADES DEL SISTEMA INCLUIDAS"));
            
            // Definir módulos con descripción
            var modulosDetallados = new List<(string nombre, string descripcion, bool incluido)>
            {
                ("Ventas y Facturación", "Facturación completa, presupuestos, notas de crédito, control de precios diferenciados, historial de ventas y reportes.", p.ModuloVentas),
                ("Compras y Proveedores", "Registro de compras, gestión de proveedores, cuentas por pagar, control de pagos y análisis de compras.", p.ModuloCompras),
                ("Inventario y Stock", "Control de stock en tiempo real, múltiples depósitos, transferencias, ajustes de inventario, alertas de stock bajo y reportes valorizados.", p.ModuloInventario),
                ("Clientes y Cuentas por Cobrar", "Gestión completa de clientes, créditos, cobranzas, plan de cuotas, historial de pagos y reportes de morosidad.", p.ModuloClientes),
                ("Caja y Arqueos", "Apertura/cierre de caja, múltiples turnos, composición de billetes, arqueo diario y control de ingresos/egresos.", p.ModuloCaja),
                ("Facturación Electrónica SIFEN", "Integración completa con el SET: generación de XML, firma digital, envío/consulta de lotes, KuDE y gestión de eventos.", p.ModuloSifen),
                ("Informes y Reportes", "Reportes de ventas, compras, stock, rentabilidad, gráficos de tendencias, exportación a Excel/PDF y envío automático por correo.", p.ModuloInformes),
                ("Usuarios y Permisos", "Control de acceso por roles, permisos granulares por módulo/función, auditoría de cambios y registro de actividad.", p.ModuloUsuarios),
                ("Envío de Correos", "Envío automático de facturas, informes programados, notificaciones de cobranza y recordatorios de vencimiento.", p.ModuloCorreo),
                ("Asistente IA Integrado", "Chatbot de ayuda contextual, guías paso a paso, soporte en línea y aprendizaje continuo del sistema.", p.ModuloAsistenteIA)
            };

            var modulosIncluidos = modulosDetallados.Where(m => m.incluido).ToList();
            
            // Tabla de módulos (2 columnas)
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(cd =>
                {
                    cd.RelativeColumn();
                    cd.RelativeColumn();
                });

                for (int i = 0; i < modulosIncluidos.Count; i += 2)
                {
                    // Primera columna
                    table.Cell().Padding(4).Border(1).BorderColor("#e5e7eb").Column(c =>
                    {
                        c.Item().Text($"✓ {modulosIncluidos[i].nombre}").Bold().FontSize(8).FontColor("#059669");
                        c.Item().Text(modulosIncluidos[i].descripcion).FontSize(7).FontColor("#6b7280");
                    });

                    // Segunda columna (si existe)
                    if (i + 1 < modulosIncluidos.Count)
                    {
                        table.Cell().Padding(4).Border(1).BorderColor("#e5e7eb").Column(c =>
                        {
                            c.Item().Text($"✓ {modulosIncluidos[i + 1].nombre}").Bold().FontSize(8).FontColor("#059669");
                            c.Item().Text(modulosIncluidos[i + 1].descripcion).FontSize(7).FontColor("#6b7280");
                        });
                    }
                    else
                    {
                        table.Cell().Padding(4); // Celda vacía
                    }
                }
            });

            col.Item().PaddingTop(8);

            // CARACTERÍSTICAS ADICIONALES
            col.Item().Background("#f3f4f6").Padding(8).Column(caracCol =>
            {
                caracCol.Item().Row(row =>
                {
                    row.AutoItem().Text("🌐").FontSize(9);
                    row.RelativeItem().PaddingLeft(4).Column(c =>
                    {
                        c.Item().Text("Aplicación Web Responsive:").Bold().FontSize(8).FontColor("#3b82f6");
                        c.Item().Text("Acceda desde cualquier dispositivo (PC, tablet, smartphone) con navegador web.").FontSize(7).FontColor("#374151");
                    });
                });
                caracCol.Item().PaddingTop(4).Row(row =>
                {
                    row.AutoItem().Text("📱").FontSize(9);
                    row.RelativeItem().PaddingLeft(4).Column(c =>
                    {
                        c.Item().Text("Cámara como Escáner:").Bold().FontSize(8).FontColor("#059669");
                        c.Item().Text("Lea códigos de barras con la cámara del celular para agilizar ventas e inventario.").FontSize(7).FontColor("#374151");
                    });
                });
                caracCol.Item().PaddingTop(4).Row(row =>
                {
                    row.AutoItem().Text("☁️").FontSize(9);
                    row.RelativeItem().PaddingLeft(4).Column(c =>
                    {
                        c.Item().Text("Acceso Remoto:").Bold().FontSize(8).FontColor("#8b5cf6");
                        c.Item().Text("Trabaje desde cualquier ubicación con conexión a internet, configuración LAN/WAN disponible.").FontSize(7).FontColor("#374151");
                    });
                });
            });

            // AVISO VPN MIKROTIK (destacado)
            col.Item().PaddingTop(5).Background("#fff3cd").Border(1).BorderColor("#ffc107").Padding(8).Row(row =>
            {
                row.AutoItem().Text("⚠️").FontSize(10);
                row.RelativeItem().PaddingLeft(6).Column(c =>
                {
                    c.Item().Text("IMPORTANTE - Acceso Remoto por Internet").Bold().FontSize(8).FontColor("#856404");
                    c.Item().Text("Para el acceso remoto a través de Internet, se requiere instalación de VPN Mikrotik (COSTO ADICIONAL). Se debe solicitar al proveedor de Internet la IP pública del servicio.")
                       .FontSize(7).FontColor("#92400e");
                });
            });

            col.Item().PaddingTop(10);

            // ITEMS/PRODUCTOS (si hay)
            if (items.Any())
            {
                col.Item().Element(c => SectionTitle(c, "PRODUCTOS / ITEMS ADICIONALES"));
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(cd =>
                    {
                        cd.RelativeColumn(4); // Descripción
                        cd.ConstantColumn(50); // Cant
                        cd.ConstantColumn(70); // P.Unit USD
                        cd.ConstantColumn(70); // Subtotal USD
                        cd.ConstantColumn(90); // Subtotal Gs
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Background("#374151").Padding(4).Text("Descripción").Bold().FontColor(Colors.White).FontSize(8);
                        header.Cell().Background("#374151").Padding(4).AlignRight().Text("Cant.").Bold().FontColor(Colors.White).FontSize(8);
                        header.Cell().Background("#374151").Padding(4).AlignRight().Text("P.Unit USD").Bold().FontColor(Colors.White).FontSize(8);
                        header.Cell().Background("#374151").Padding(4).AlignRight().Text("Subtotal USD").Bold().FontColor(Colors.White).FontSize(8);
                        header.Cell().Background("#374151").Padding(4).AlignRight().Text("Subtotal Gs").Bold().FontColor(Colors.White).FontSize(8);
                    });

                    foreach (var item in items.Where(i => !i.EsOpcional || i.Incluido).OrderBy(i => i.NumeroLinea))
                    {
                        var bg = items.IndexOf(item) % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                        
                        table.Cell().Background(bg).Padding(3).Column(c =>
                        {
                            c.Item().Text(item.Descripcion ?? "").FontSize(8);
                            if (!string.IsNullOrEmpty(item.DescripcionAdicional))
                                c.Item().Text(item.DescripcionAdicional).FontSize(7).FontColor(Colors.Grey.Darken1);
                        });
                        table.Cell().Background(bg).Padding(3).AlignRight().Text($"{item.Cantidad:N2}").FontSize(8);
                        table.Cell().Background(bg).Padding(3).AlignRight().Text($"$ {item.PrecioUnitarioUsd:N2}").FontSize(8);
                        table.Cell().Background(bg).Padding(3).AlignRight().Text($"$ {item.SubtotalUsd:N2}").FontSize(8);
                        table.Cell().Background(bg).Padding(3).AlignRight().Text($"Gs {item.SubtotalGs:N0}").FontSize(8);
                    }

                    // Total items
                    table.Cell().ColumnSpan(3).Background("#f3f4f6").Padding(4).Text("TOTAL ITEMS").Bold().FontSize(8);
                    table.Cell().Background("#f3f4f6").Padding(4).AlignRight().Text($"$ {totalItemsUsd:N2}").Bold().FontSize(8);
                    table.Cell().Background("#f3f4f6").Padding(4).AlignRight().Text($"Gs {totalItemsGs:N0}").Bold().FontSize(8);
                });

                col.Item().PaddingTop(10);
            }

            // OPCIONES DE PAGO
            col.Item().Element(c => SectionTitle(c, "OPCIONES DE PAGO"));

            // Opción Contado
            if (p.PrecioContadoUsd > 0)
            {
                col.Item().Border(2).BorderColor("#374151").Column(card =>
                {
                    card.Item().Background("#374151").Padding(8).Text("OPCIÓN CONTADO").Bold().FontColor(Colors.White).FontSize(10);
                    card.Item().Padding(10).Column(c =>
                    {
                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Precio Software:").FontSize(9);
                            r.ConstantItem(100).AlignRight().Text($"$ {p.PrecioContadoUsd:N2}").FontSize(9);
                        });
                        if (p.CostoImplementacionUsd > 0)
                        {
                            c.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Implementación:").FontSize(9);
                                r.ConstantItem(100).AlignRight().Text($"$ {p.CostoImplementacionUsd:N2}").FontSize(9);
                            });
                        }
                        if (p.CostoCapacitacionUsd > 0)
                        {
                            c.Item().Row(r =>
                            {
                                r.RelativeItem().Text($"Capacitación ({p.HorasCapacitacion} hs):").FontSize(9);
                                r.ConstantItem(100).AlignRight().Text($"$ {p.CostoCapacitacionUsd:N2}").FontSize(9);
                            });
                        }
                        if (totalItemsUsd > 0)
                        {
                            c.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Items/Productos:").FontSize(9);
                                r.ConstantItem(100).AlignRight().Text($"$ {totalItemsUsd:N2}").FontSize(9);
                            });
                        }
                        c.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        c.Item().PaddingTop(5).Row(r =>
                        {
                            r.RelativeItem().Text("TOTAL USD:").Bold().FontSize(11);
                            r.ConstantItem(100).AlignRight().Text($"$ {totalContadoUsd:N2}").Bold().FontSize(11);
                        });
                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text("TOTAL Gs:").Bold().FontSize(11);
                            r.ConstantItem(120).AlignRight().Text($"Gs {totalContadoGs:N0}").Bold().FontSize(11);
                        });
                    });
                });
            }

            // Opción Leasing
            if (p.PrecioLeasingMensualUsd > 0)
            {
                col.Item().PaddingTop(10).Border(2).BorderColor("#374151").Column(card =>
                {
                    card.Item().Background("#374151").Padding(8).Text("OPCIÓN LEASING").Bold().FontColor(Colors.White).FontSize(10);
                    card.Item().Padding(10).Column(c =>
                    {
                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Cuota Mensual USD:").FontSize(9);
                            r.ConstantItem(100).AlignRight().Text($"$ {p.PrecioLeasingMensualUsd:N2}").Bold().FontSize(10);
                        });
                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Cuota Mensual Gs:").FontSize(9);
                            r.ConstantItem(120).AlignRight().Text($"Gs {p.PrecioLeasingMensualGs:N0}").Bold().FontSize(10);
                        });
                        if (p.EntradaLeasingUsd > 0)
                        {
                            c.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Entrada:").FontSize(9);
                                r.ConstantItem(150).AlignRight().Text($"$ {p.EntradaLeasingUsd:N2} / Gs {p.EntradaLeasingGs:N0}").FontSize(9);
                            });
                        }
                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Plazo:").FontSize(9);
                            r.ConstantItem(100).AlignRight().Text($"{p.CantidadCuotasLeasing} cuotas").FontSize(9);
                        });
                    });
                });
            }

            col.Item().PaddingTop(10);

            // COSTOS ADICIONALES
            col.Item().Element(c => SectionTitle(c, "COSTOS ADICIONALES DE REFERENCIA"));
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(cd =>
                {
                    cd.RelativeColumn();
                    cd.ConstantColumn(80);
                    cd.ConstantColumn(100);
                });

                table.Header(header =>
                {
                    header.Cell().Background("#e5e7eb").Padding(4).Text("Concepto").Bold().FontSize(8);
                    header.Cell().Background("#e5e7eb").Padding(4).AlignRight().Text("USD").Bold().FontSize(8);
                    header.Cell().Background("#e5e7eb").Padding(4).AlignRight().Text("Guaraníes").Bold().FontSize(8);
                });

                void AddRow(string concepto, decimal usd, decimal gs)
                {
                    if (usd > 0)
                    {
                        table.Cell().Padding(3).Text(concepto).FontSize(8);
                        table.Cell().Padding(3).AlignRight().Text($"$ {usd:N2}").FontSize(8);
                        table.Cell().Padding(3).AlignRight().Text($"Gs {gs:N0}").FontSize(8);
                    }
                }

                AddRow("Mantenimiento Mensual", p.CostoMantenimientoMensualUsd, p.CostoMantenimientoMensualGs);
                AddRow("Sucursal Adicional", p.CostoSucursalAdicionalUsd, p.CostoSucursalAdicionalGs);
                AddRow("Usuario Adicional", p.CostoUsuarioAdicionalUsd, p.CostoUsuarioAdicionalGs);
                AddRow("Hora de Desarrollo", p.CostoHoraDesarrolloUsd, p.CostoHoraDesarrolloGs);
                AddRow("Visita Técnica", p.CostoVisitaTecnicaUsd, p.CostoVisitaTecnicaGs);
            });

            // CONFIGURACIÓN INCLUIDA
            col.Item().PaddingTop(10);
            col.Item().Element(c => SectionTitle(c, "CONFIGURACIÓN INCLUIDA"));
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text($"• Sucursales: {p.CantidadSucursalesIncluidas}").FontSize(9);
                });
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text($"• Usuarios: {p.CantidadUsuariosIncluidos}").FontSize(9);
                });
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text($"• Cajas: {p.CantidadCajasIncluidas}").FontSize(9);
                });
            });

            // CONDICIONES DE PAGO
            if (!string.IsNullOrWhiteSpace(p.CondicionesPago))
            {
                col.Item().PaddingTop(10);
                col.Item().Element(c => SectionTitle(c, "CONDICIONES DE PAGO"));
                col.Item().Text(p.CondicionesPago).FontSize(8);
            }

            // GARANTÍAS
            if (!string.IsNullOrWhiteSpace(p.Garantias))
            {
                col.Item().PaddingTop(10);
                col.Item().Element(c => SectionTitle(c, "GARANTÍAS"));
                col.Item().Text(p.Garantias).FontSize(8);
            }

            // OBSERVACIONES
            if (!string.IsNullOrWhiteSpace(p.Observaciones))
            {
                col.Item().PaddingTop(10);
                col.Item().Element(c => SectionTitle(c, "OBSERVACIONES"));
                col.Item().Text(p.Observaciones).FontSize(8);
            }

            // Tipo de cambio
            col.Item().PaddingTop(15).AlignCenter()
               .Text($"Tipo de cambio aplicado: 1 USD = Gs {p.TipoCambio:N0}")
               .FontSize(7).FontColor(Colors.Grey.Darken1);
        });
    }

    private void SectionTitle(IContainer container, string title)
    {
        container.PaddingBottom(5).Row(row =>
        {
            row.AutoItem().Width(4).Height(14).Background("#374151");
            row.RelativeItem().PaddingLeft(8).Text(title).Bold().FontSize(10).FontColor("#374151");
        });
    }

    private void ComposeFooter(IContainer container, PresupuestoSistema p)
    {
        container.Column(col =>
        {
            col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            col.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Text($"Presupuesto #{p.NumeroPresupuesto}").FontSize(7).FontColor(Colors.Grey.Medium);
                row.RelativeItem().AlignCenter().Text($"Válido hasta: {p.FechaVigencia:dd/MM/yyyy}").FontSize(7).FontColor(Colors.Grey.Medium);
                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.Span("Página ").FontSize(7).FontColor(Colors.Grey.Medium);
                    text.CurrentPageNumber().FontSize(7).FontColor(Colors.Grey.Medium);
                    text.Span(" de ").FontSize(7).FontColor(Colors.Grey.Medium);
                    text.TotalPages().FontSize(7).FontColor(Colors.Grey.Medium);
                });
            });
        });
    }
}
