using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemIA.Models;

namespace SistemIA.Services
{
    /// <summary>
    /// Servicio para generar PDFs de informes usando QuestPDF con formato profesional.
    /// </summary>
    public interface IInformePdfService
    {
        Task<byte[]> GenerarPdfVentasDetalladoAsync(int sucursalId, DateTime fechaCaja, int? turno = null, int? idCaja = null);
        Task<byte[]> GenerarPdfVentasAgrupadoAsync(int sucursalId, DateTime fechaCaja, int? turno = null, int? idCaja = null);
        Task<byte[]> GenerarPdfComprasDetalladoAsync(int sucursalId, DateTime fechaCaja, int? turno = null, int? idCaja = null);
        Task<byte[]> GenerarPdfNCVentasAsync(int sucursalId, DateTime fechaCaja, int? turno = null, int? idCaja = null);
        Task<byte[]> GenerarPdfNCComprasAsync(int sucursalId, DateTime fechaCaja, int? turno = null, int? idCaja = null);
        Task<byte[]> GenerarPdfProductosValorizadoAsync(int sucursalId);
        Task<byte[]> GenerarPdfResumenCajaAsync(int sucursalId, DateTime fechaCaja, int? turno = null, int? idCaja = null);
        Task<byte[]> GenerarPdfCierreComplejosAsync(int sucursalId, DateTime fechaCaja, int? turno = null, int? idCaja = null);
        
        // ========== INFORMES DE GIMNASIO ==========
        Task<byte[]> GenerarPdfGimnasioAsistenciaClasesAsync(int sucursalId, DateTime fecha);
        Task<byte[]> GenerarPdfGimnasioMembresiasAsync(int sucursalId);
        Task<byte[]> GenerarPdfGimnasioClasesPopularesAsync(int sucursalId, DateTime fechaDesde, DateTime fechaHasta);
        Task<byte[]> GenerarPdfGimnasioInstructoresAsync(int sucursalId, DateTime fechaDesde, DateTime fechaHasta);
        Task<byte[]> GenerarPdfGimnasioMembresiasPorVencerAsync(int sucursalId, int diasAnticipacion = 15);
    }

    public class InformePdfService : IInformePdfService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        private readonly ILogger<InformePdfService> _logger;

        public InformePdfService(IDbContextFactory<AppDbContext> dbFactory, ILogger<InformePdfService> logger)
        {
            _dbFactory = dbFactory;
            _logger = logger;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        private async Task<(Sucursal? Sucursal, string Empresa, string NombreSucursal, byte[]? Logo)> ObtenerDatosSucursal(int sucursalId)
        {
            await using var ctx = await _dbFactory.CreateDbContextAsync();
            var suc = await ctx.Sucursal.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sucursalId);
            return (suc, suc?.NombreEmpresa ?? "Empresa", suc?.NombreSucursal ?? "Sucursal", suc?.Logo);
        }

        public async Task<byte[]> GenerarPdfVentasDetalladoAsync(int sucursalId, DateTime fechaCaja, int? turno = null, int? idCaja = null)
        {
            await using var ctx = await _dbFactory.CreateDbContextAsync();
            var (suc, empresa, sucursal, logo) = await ObtenerDatosSucursal(sucursalId);

            // Filtrar ventas por fecha, turno y caja
            var query = ctx.Ventas.AsNoTracking()
                .Where(v => v.IdSucursal == sucursalId && v.Estado != "Anulada");

            // Filtrar por Fecha
            query = query.Where(v => v.Fecha.Date == fechaCaja.Date);

            if (turno.HasValue && turno > 0)
                query = query.Where(v => v.Turno == turno.Value);
            if (idCaja.HasValue && idCaja > 0)
                query = query.Where(v => v.IdCaja == idCaja.Value);

            var ventas = await query
                .Include(v => v.Cliente)
                .OrderBy(v => v.Fecha)
                .ToListAsync();

            // Cargar detalles
            var ventaIds = ventas.Select(v => v.IdVenta).ToList();
            var detalles = await ctx.VentasDetalles.AsNoTracking()
                .Where(d => ventaIds.Contains(d.IdVenta))
                .Include(d => d.Producto)
                .ToListAsync();
            var detallesPorVenta = detalles.GroupBy(d => d.IdVenta).ToDictionary(g => g.Key, g => g.ToList());

            var filtros = $"Fecha: {fechaCaja:dd/MM/yyyy}" +
                         (turno.HasValue && turno > 0 ? $" | Turno: {turno}" : "") +
                         (idCaja.HasValue && idCaja > 0 ? $" | Caja: {idCaja}" : "");

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Element(c => GenerarEncabezado(c, "Ventas Detallado", empresa, sucursal, filtros, logo));

                    page.Content().Element(content =>
                    {
                        content.Column(col =>
                        {
                            // Resumen
                            col.Item().PaddingBottom(10).Row(row =>
                            {
                                row.RelativeItem().Background(Colors.Blue.Lighten4).Padding(10).Column(c =>
                                {
                                    c.Item().Text("Total de Ventas:").Bold();
                                    c.Item().Text(ventas.Count.ToString()).FontSize(14);
                                });
                                row.ConstantItem(20);
                                row.RelativeItem().Background(Colors.Green.Lighten4).Padding(10).Column(c =>
                                {
                                    c.Item().Text("Monto Total:").Bold();
                                    c.Item().Text($"Gs. {ventas.Sum(v => v.Total):N0}").FontSize(14).FontColor(Colors.Green.Darken2);
                                });
                            });

                            // Tabla de ventas por factura con detalles
                            foreach (var v in ventas)
                            {
                                col.Item().PaddingBottom(8).Border(1).BorderColor(Colors.Grey.Lighten2).Column(ventaCol =>
                                {
                                    // Encabezado de factura
                                    ventaCol.Item().Background(Colors.Blue.Darken2).Padding(6).Row(headerRow =>
                                    {
                                        headerRow.RelativeItem().Text($"Factura: {v.NumeroFactura ?? "-"}").FontColor(Colors.White).Bold();
                                        headerRow.RelativeItem().Text($"Cliente: {v.Cliente?.RazonSocial ?? "Sin cliente"}").FontColor(Colors.White);
                                        headerRow.ConstantItem(120).AlignRight().Text($"{v.Fecha:dd/MM/yyyy HH:mm}").FontColor(Colors.White);
                                    });

                                    // Detalles de productos
                                    if (detallesPorVenta.TryGetValue(v.IdVenta, out var ventaDetalles) && ventaDetalles.Any())
                                    {
                                        ventaCol.Item().Table(tabla =>
                                        {
                                            tabla.ColumnsDefinition(columns =>
                                            {
                                                columns.RelativeColumn(3); // Producto
                                                columns.ConstantColumn(60); // Cant
                                                columns.ConstantColumn(80); // P.Unit
                                                columns.ConstantColumn(80); // Subtotal
                                            });

                                            tabla.Header(header =>
                                            {
                                                header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Producto").Bold().FontSize(8);
                                                header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Cant.").Bold().FontSize(8);
                                                header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("P.Unit.").Bold().FontSize(8);
                                                header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Subtotal").Bold().FontSize(8);
                                            });

                                            foreach (var d in ventaDetalles)
                                            {
                                                tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(3).Text(d.Producto?.Descripcion ?? "Producto").FontSize(8);
                                                tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(3).AlignRight().Text($"{d.Cantidad:N2}").FontSize(8);
                                                tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(3).AlignRight().Text($"Gs. {d.PrecioUnitario:N0}").FontSize(8);
                                                tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(3).AlignRight().Text($"Gs. {d.Importe:N0}").FontSize(8);
                                            }
                                        });
                                    }

                                    // Total de la factura
                                    ventaCol.Item().Background(Colors.Grey.Lighten4).Padding(4).AlignRight()
                                        .Text($"Total: Gs. {v.Total:N0}").Bold().FontColor(Colors.Green.Darken2);
                                });
                            }
                        });
                    });

                    page.Footer().Element(c => GenerarPie(c));
                });
            }).GeneratePdf();
        }

        public async Task<byte[]> GenerarPdfVentasAgrupadoAsync(int sucursalId, DateTime fechaCaja, int? turno = null, int? idCaja = null)
        {
            await using var ctx = await _dbFactory.CreateDbContextAsync();
            var (suc, empresa, sucursal, logo) = await ObtenerDatosSucursal(sucursalId);

            var query = ctx.Ventas.AsNoTracking()
                .Where(v => v.IdSucursal == sucursalId && v.Estado != "Anulada");

            query = query.Where(v => v.Fecha.Date == fechaCaja.Date);

            if (turno.HasValue && turno > 0)
                query = query.Where(v => v.Turno == turno.Value);
            if (idCaja.HasValue && idCaja > 0)
                query = query.Where(v => v.IdCaja == idCaja.Value);

            var ventas = await query.ToListAsync();

            // Agrupar por cliente
            var porCliente = ventas.GroupBy(v => v.IdCliente ?? 0)
                .Select(g => new { IdCliente = g.Key, Count = g.Count(), Total = g.Sum(x => x.Total) })
                .OrderByDescending(x => x.Total)
                .ToList();

            var filtros = $"Fecha: {fechaCaja:dd/MM/yyyy}" +
                         (turno.HasValue && turno > 0 ? $" | Turno: {turno}" : "") +
                         (idCaja.HasValue && idCaja > 0 ? $" | Caja: {idCaja}" : "");

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(25);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(c => GenerarEncabezado(c, "Ventas Agrupado", empresa, sucursal, filtros, logo));

                    page.Content().Column(col =>
                    {
                        // Resumen
                        col.Item().PaddingBottom(15).Row(row =>
                        {
                            row.RelativeItem().Background(Colors.Blue.Lighten4).Padding(12).Column(c =>
                            {
                                c.Item().Text("Total Ventas:").Bold();
                                c.Item().Text(ventas.Count.ToString()).FontSize(16);
                            });
                            row.ConstantItem(20);
                            row.RelativeItem().Background(Colors.Green.Lighten4).Padding(12).Column(c =>
                            {
                                c.Item().Text("Monto Total:").Bold();
                                c.Item().Text($"Gs. {ventas.Sum(v => v.Total):N0}").FontSize(16).FontColor(Colors.Green.Darken2);
                            });
                        });

                        // Tabla resumen
                        col.Item().Table(tabla =>
                        {
                            tabla.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(80);
                                columns.RelativeColumn();
                            });

                            tabla.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Darken2).Padding(8).Text("Cant. Ventas").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(8).AlignRight().Text("Monto").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Blue.Darken2).Padding(8).Text("Observación").FontColor(Colors.White).Bold();
                            });

                            tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(ventas.Count.ToString());
                            tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignRight().Text($"Gs. {ventas.Sum(v => v.Total):N0}");
                            tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text($"Turno {turno}, Caja {idCaja}");
                        });
                    });

                    page.Footer().Element(c => GenerarPie(c));
                });
            }).GeneratePdf();
        }

        public async Task<byte[]> GenerarPdfComprasDetalladoAsync(int sucursalId, DateTime fechaCaja, int? turno = null, int? idCaja = null)
        {
            await using var ctx = await _dbFactory.CreateDbContextAsync();
            var (suc, empresa, sucursal, logo) = await ObtenerDatosSucursal(sucursalId);

            var query = ctx.Compras.AsNoTracking()
                .Where(c => c.IdSucursal == sucursalId && c.Estado != "Anulada" && c.Fecha.Date == fechaCaja.Date);

            var compras = await query
                .Include(c => c.Proveedor)
                .OrderBy(c => c.Fecha)
                .ToListAsync();

            var compraIds = compras.Select(c => c.IdCompra).ToList();
            var detalles = await ctx.ComprasDetalles.AsNoTracking()
                .Where(d => compraIds.Contains(d.IdCompra))
                .Include(d => d.Producto)
                .ToListAsync();
            var detallesPorCompra = detalles.GroupBy(d => d.IdCompra).ToDictionary(g => g.Key, g => g.ToList());

            var filtros = $"Fecha: {fechaCaja:dd/MM/yyyy}";

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Element(c => GenerarEncabezado(c, "Compras Detallado", empresa, sucursal, filtros, logo));

                    page.Content().Column(col =>
                    {
                        col.Item().PaddingBottom(10).Row(row =>
                        {
                            row.RelativeItem().Background(Colors.Orange.Lighten4).Padding(10).Column(c =>
                            {
                                c.Item().Text("Total Compras:").Bold();
                                c.Item().Text(compras.Count.ToString()).FontSize(14);
                            });
                            row.ConstantItem(20);
                            row.RelativeItem().Background(Colors.Red.Lighten4).Padding(10).Column(c =>
                            {
                                c.Item().Text("Monto Total:").Bold();
                                c.Item().Text($"Gs. {compras.Sum(c => c.Total):N0}").FontSize(14).FontColor(Colors.Red.Darken2);
                            });
                        });

                        foreach (var comp in compras)
                        {
                            col.Item().PaddingBottom(8).Border(1).BorderColor(Colors.Grey.Lighten2).Column(compCol =>
                            {
                                compCol.Item().Background(Colors.Orange.Darken1).Padding(6).Row(r =>
                                {
                                    r.RelativeItem().Text($"Factura: {comp.NumeroFactura ?? "-"}").FontColor(Colors.White).Bold();
                                    r.RelativeItem().Text($"Proveedor: {comp.Proveedor?.RazonSocial ?? "Sin proveedor"}").FontColor(Colors.White);
                                    r.ConstantItem(120).AlignRight().Text($"{comp.Fecha:dd/MM/yyyy}").FontColor(Colors.White);
                                });

                                if (detallesPorCompra.TryGetValue(comp.IdCompra, out var compDetalles) && compDetalles.Any())
                                {
                                    compCol.Item().Table(tabla =>
                                    {
                                        tabla.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(3);
                                            columns.ConstantColumn(60);
                                            columns.ConstantColumn(80);
                                            columns.ConstantColumn(80);
                                        });

                                        tabla.Header(header =>
                                        {
                                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Producto").Bold().FontSize(8);
                                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Cant.").Bold().FontSize(8);
                                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("P.Unit.").Bold().FontSize(8);
                                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Subtotal").Bold().FontSize(8);
                                        });

                                        foreach (var d in compDetalles)
                                        {
                                            tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(3).Text(d.Producto?.Descripcion ?? "Producto").FontSize(8);
                                            tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(3).AlignRight().Text($"{d.Cantidad:N2}").FontSize(8);
                                            tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(3).AlignRight().Text($"Gs. {d.PrecioUnitario:N0}").FontSize(8);
                                            tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(3).AlignRight().Text($"Gs. {d.Importe:N0}").FontSize(8);
                                        }
                                    });
                                }

                                compCol.Item().Background(Colors.Grey.Lighten4).Padding(4).AlignRight()
                                    .Text($"Total: Gs. {comp.Total:N0}").Bold().FontColor(Colors.Red.Darken2);
                            });
                        }
                    });

                    page.Footer().Element(c => GenerarPie(c));
                });
            }).GeneratePdf();
        }

        public async Task<byte[]> GenerarPdfNCVentasAsync(int sucursalId, DateTime fechaCaja, int? turno = null, int? idCaja = null)
        {
            await using var ctx = await _dbFactory.CreateDbContextAsync();
            var (suc, empresa, sucursal, logo) = await ObtenerDatosSucursal(sucursalId);

            var query = ctx.NotasCreditoVentas.AsNoTracking()
                .Where(nc => nc.IdSucursal == sucursalId && nc.Estado == "Confirmada" && nc.Fecha.Date == fechaCaja.Date);

            if (turno.HasValue && turno > 0)
                query = query.Where(nc => nc.Turno == turno.Value.ToString());
            if (idCaja.HasValue && idCaja > 0)
                query = query.Where(nc => nc.IdCaja == idCaja.Value);

            var ncs = await query.Include(nc => nc.Cliente).OrderBy(nc => nc.Fecha).ToListAsync();

            var filtros = $"Fecha: {fechaCaja:dd/MM/yyyy}" +
                         (turno.HasValue && turno > 0 ? $" | Turno: {turno}" : "") +
                         (idCaja.HasValue && idCaja > 0 ? $" | Caja: {idCaja}" : "");

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(25);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(c => GenerarEncabezado(c, "Notas de Crédito Ventas", empresa, sucursal, filtros, logo));

                    page.Content().Column(col =>
                    {
                        col.Item().PaddingBottom(15).Row(row =>
                        {
                            row.RelativeItem().Background(Colors.Purple.Lighten4).Padding(12).Column(c =>
                            {
                                c.Item().Text("Total NC:").Bold();
                                c.Item().Text(ncs.Count.ToString()).FontSize(16);
                            });
                            row.ConstantItem(20);
                            row.RelativeItem().Background(Colors.Purple.Lighten3).Padding(12).Column(c =>
                            {
                                c.Item().Text("Monto Total:").Bold();
                                c.Item().Text($"Gs. {ncs.Sum(nc => nc.Total):N0}").FontSize(16).FontColor(Colors.Purple.Darken2);
                            });
                        });

                        if (ncs.Any())
                        {
                            col.Item().Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(100);
                                    columns.RelativeColumn();
                                    columns.ConstantColumn(100);
                                    columns.ConstantColumn(100);
                                });

                                tabla.Header(header =>
                                {
                                    header.Cell().Background(Colors.Purple.Darken2).Padding(8).Text("Número").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Purple.Darken2).Padding(8).Text("Cliente").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Purple.Darken2).Padding(8).Text("Fecha").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Purple.Darken2).Padding(8).AlignRight().Text("Total").FontColor(Colors.White).Bold();
                                });

                                foreach (var nc in ncs)
                                {
                                    tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(nc.NumeroNota.ToString());
                                    tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(nc.Cliente?.RazonSocial ?? "Sin cliente");
                                    tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text($"{nc.Fecha:dd/MM/yyyy}");
                                    tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignRight().Text($"Gs. {nc.Total:N0}");
                                }
                            });
                        }
                        else
                        {
                            col.Item().Padding(20).AlignCenter().Text("No hay notas de crédito en este período").FontColor(Colors.Grey.Medium);
                        }
                    });

                    page.Footer().Element(c => GenerarPie(c));
                });
            }).GeneratePdf();
        }

        public async Task<byte[]> GenerarPdfNCComprasAsync(int sucursalId, DateTime fechaCaja, int? turno = null, int? idCaja = null)
        {
            await using var ctx = await _dbFactory.CreateDbContextAsync();
            var (suc, empresa, sucursal, logo) = await ObtenerDatosSucursal(sucursalId);

            var query = ctx.NotasCreditoCompras.AsNoTracking()
                .Where(nc => nc.IdSucursal == sucursalId && nc.Estado == "Confirmada" && nc.Fecha.Date == fechaCaja.Date);

            if (turno.HasValue && turno > 0)
                query = query.Where(nc => nc.Turno == turno);
            if (idCaja.HasValue && idCaja > 0)
                query = query.Where(nc => nc.IdCaja == idCaja.Value);

            var ncs = await query.Include(nc => nc.Proveedor).OrderBy(nc => nc.Fecha).ToListAsync();

            var filtros = $"Fecha: {fechaCaja:dd/MM/yyyy}";

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(25);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(c => GenerarEncabezado(c, "Notas de Crédito Compras", empresa, sucursal, filtros, logo));

                    page.Content().Column(col =>
                    {
                        col.Item().PaddingBottom(15).Row(row =>
                        {
                            row.RelativeItem().Background(Colors.Teal.Lighten4).Padding(12).Column(c =>
                            {
                                c.Item().Text("Total NC:").Bold();
                                c.Item().Text(ncs.Count.ToString()).FontSize(16);
                            });
                            row.ConstantItem(20);
                            row.RelativeItem().Background(Colors.Teal.Lighten3).Padding(12).Column(c =>
                            {
                                c.Item().Text("Monto Total:").Bold();
                                c.Item().Text($"Gs. {ncs.Sum(nc => nc.Total):N0}").FontSize(16).FontColor(Colors.Teal.Darken2);
                            });
                        });

                        if (ncs.Any())
                        {
                            col.Item().Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(100);
                                    columns.RelativeColumn();
                                    columns.ConstantColumn(100);
                                    columns.ConstantColumn(100);
                                });

                                tabla.Header(header =>
                                {
                                    header.Cell().Background(Colors.Teal.Darken2).Padding(8).Text("Número").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Teal.Darken2).Padding(8).Text("Proveedor").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Teal.Darken2).Padding(8).Text("Fecha").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Teal.Darken2).Padding(8).AlignRight().Text("Total").FontColor(Colors.White).Bold();
                                });

                                foreach (var nc in ncs)
                                {
                                    tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(nc.NumeroNota ?? "-");
                                    tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(nc.Proveedor?.RazonSocial ?? "Sin proveedor");
                                    tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text($"{nc.Fecha:dd/MM/yyyy}");
                                    tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignRight().Text($"Gs. {nc.Total:N0}");
                                }
                            });
                        }
                        else
                        {
                            col.Item().Padding(20).AlignCenter().Text("No hay notas de crédito de compras en este período").FontColor(Colors.Grey.Medium);
                        }
                    });

                    page.Footer().Element(c => GenerarPie(c));
                });
            }).GeneratePdf();
        }

        public async Task<byte[]> GenerarPdfProductosValorizadoAsync(int sucursalId)
        {
            await using var ctx = await _dbFactory.CreateDbContextAsync();
            var (suc, empresa, sucursal, logo) = await ObtenerDatosSucursal(sucursalId);

            var productos = await ctx.Productos.AsNoTracking()
                .Where(p => p.Activo)
                .Include(p => p.Clasificacion)
                .OrderBy(p => p.Descripcion)
                .ToListAsync();

            var productosConStock = productos.Select(p => new
            {
                p.IdProducto,
                Codigo = p.CodigoInterno,
                p.Descripcion,
                Clasificacion = p.Clasificacion?.Nombre ?? "Sin clasificación",
                p.Stock,
                Costo = p.CostoUnitarioGs,
                Valorizado = p.Stock * p.CostoUnitarioGs
            }).Where(p => p.Stock > 0).ToList();

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(25);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Element(c => GenerarEncabezado(c, "Stock Valorizado", empresa, sucursal, $"Fecha: {DateTime.Now:dd/MM/yyyy}", logo));

                    page.Content().Column(col =>
                    {
                        col.Item().PaddingBottom(15).Row(row =>
                        {
                            row.RelativeItem().Background(Colors.Indigo.Lighten4).Padding(12).Column(c =>
                            {
                                c.Item().Text("Total Productos:").Bold();
                                c.Item().Text(productosConStock.Count.ToString()).FontSize(16);
                            });
                            row.ConstantItem(20);
                            row.RelativeItem().Background(Colors.Green.Lighten4).Padding(12).Column(c =>
                            {
                                c.Item().Text("Valor Total Stock:").Bold();
                                c.Item().Text($"Gs. {productosConStock.Sum(p => p.Valorizado):N0}").FontSize(16).FontColor(Colors.Green.Darken2);
                            });
                        });

                        col.Item().Table(tabla =>
                        {
                            tabla.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(60);
                                columns.RelativeColumn();
                                columns.ConstantColumn(100);
                                columns.ConstantColumn(60);
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(90);
                            });

                            tabla.Header(header =>
                            {
                                header.Cell().Background(Colors.Indigo.Darken2).Padding(6).Text("Código").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Indigo.Darken2).Padding(6).Text("Producto").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Indigo.Darken2).Padding(6).Text("Clasificación").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Indigo.Darken2).Padding(6).AlignRight().Text("Stock").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Indigo.Darken2).Padding(6).AlignRight().Text("P. Compra").FontColor(Colors.White).Bold();
                                header.Cell().Background(Colors.Indigo.Darken2).Padding(6).AlignRight().Text("Valorizado").FontColor(Colors.White).Bold();
                            });

                            foreach (var p in productosConStock)
                            {
                                tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(p.Codigo ?? "-").FontSize(8);
                                tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(p.Descripcion).FontSize(8);
                                tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(p.Clasificacion).FontSize(8);
                                tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignRight().Text($"{p.Stock:N0}").FontSize(8);
                                tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignRight().Text($"Gs. {p.Costo:N0}").FontSize(8);
                                tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignRight().Text($"Gs. {p.Valorizado:N0}").FontSize(8).FontColor(Colors.Green.Darken1);
                            }
                        });
                    });

                    page.Footer().Element(c => GenerarPie(c));
                });
            }).GeneratePdf();
        }

        public async Task<byte[]> GenerarPdfResumenCajaAsync(int sucursalId, DateTime fechaCaja, int? turno = null, int? idCaja = null)
        {
            await using var ctx = await _dbFactory.CreateDbContextAsync();
            var (suc, empresa, sucursal, logo) = await ObtenerDatosSucursal(sucursalId);

            // Buscar cierre de caja si existe
            var cierreQuery = ctx.CierresCaja.AsNoTracking()
                .Where(c => c.IdSucursal == sucursalId && c.FechaCaja.Date == fechaCaja.Date);
            
            if (turno.HasValue && turno > 0)
                cierreQuery = cierreQuery.Where(c => c.Turno == turno.Value);
            if (idCaja.HasValue && idCaja > 0)
                cierreQuery = cierreQuery.Where(c => c.IdCaja == idCaja.Value);

            var cierre = await cierreQuery.FirstOrDefaultAsync();

            var filtros = $"Fecha: {fechaCaja:dd/MM/yyyy}" +
                         (turno.HasValue && turno > 0 ? $" | Turno: {turno}" : "") +
                         (idCaja.HasValue && idCaja > 0 ? $" | Caja: {idCaja}" : "");

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Element(c => GenerarEncabezado(c, "Resumen de Caja", empresa, sucursal, filtros, logo));

                    page.Content().Column(col =>
                    {
                        if (cierre != null)
                        {
                            col.Item().PaddingBottom(20).Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.ConstantColumn(150);
                                });

                                void AgregarFila(string concepto, decimal valor, string color = "#333")
                                {
                                    tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(8).Text(concepto);
                                    tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(8).AlignRight()
                                        .Text($"Gs. {valor:N0}").FontColor(color);
                                }

                                tabla.Cell().ColumnSpan(2).Background(Colors.Blue.Darken2).Padding(10)
                                    .Text("INGRESOS").FontColor(Colors.White).Bold();
                                AgregarFila("Ventas Contado", cierre.TotalVentasContado, "#28a745");
                                AgregarFila("Cobros de Créditos", cierre.TotalCobrosCredito, "#28a745");
                                AgregarFila("NC Compras (Crédito proveedor)", cierre.TotalNotasCreditoCompras, "#28a745");

                                tabla.Cell().ColumnSpan(2).Background(Colors.Red.Darken2).Padding(10)
                                    .Text("EGRESOS").FontColor(Colors.White).Bold();
                                AgregarFila("Compras Efectivo", cierre.TotalComprasEfectivo, "#dc3545");
                                AgregarFila("Notas de Crédito Ventas", cierre.TotalNotasCredito, "#dc3545");

                                tabla.Cell().ColumnSpan(2).Background(Colors.Green.Darken2).Padding(10)
                                    .Text("TOTALES POR MEDIO DE PAGO").FontColor(Colors.White).Bold();
                                AgregarFila("Efectivo", cierre.TotalEfectivo);
                                AgregarFila("Tarjetas", cierre.TotalTarjetas);
                                AgregarFila("Transferencias", cierre.TotalTransferencias);
                                AgregarFila("Cheques", cierre.TotalCheques);

                                tabla.Cell().ColumnSpan(2).Background(Colors.Grey.Darken3).Padding(12);
                                tabla.Cell().Padding(10).Text("TOTAL EN CAJA").Bold().FontSize(14);
                                var totalEnCaja = cierre.TotalEfectivo + cierre.TotalTarjetas + cierre.TotalTransferencias + cierre.TotalCheques;
                                tabla.Cell().Padding(10).AlignRight().Text($"Gs. {totalEnCaja:N0}")
                                    .Bold().FontSize(14).FontColor(Colors.Green.Darken2);
                            });
                        }
                        else
                        {
                            col.Item().Padding(30).AlignCenter().Text("No hay cierre de caja para este período").FontColor(Colors.Grey.Medium);
                        }
                    });

                    page.Footer().Element(c => GenerarPie(c));
                });
            }).GeneratePdf();
        }

        // ========== HELPERS ==========

        private void GenerarEncabezado(IContainer container, string titulo, string empresa, string sucursal, string filtros, byte[]? logo)
        {
            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    if (logo != null && logo.Length > 0)
                    {
                        row.ConstantItem(80).Height(50).Image(logo).FitArea();
                        row.ConstantItem(15);
                    }

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text(empresa).Bold().FontSize(16);
                        c.Item().Text(sucursal).FontSize(10).FontColor(Colors.Grey.Darken1);
                    });

                    row.RelativeItem().AlignRight().Column(c =>
                    {
                        c.Item().Text(titulo).Bold().FontSize(14).FontColor(Colors.Blue.Darken2);
                        c.Item().Text($"Filtros: {filtros}").FontSize(8).FontColor(Colors.Grey.Medium);
                        c.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Medium);
                    });
                });

                col.Item().PaddingTop(10).LineHorizontal(2).LineColor(Colors.Blue.Darken2);
                col.Item().PaddingBottom(15);
            });
        }

        private void GenerarPie(IContainer container)
        {
            container.AlignCenter().Text(text =>
            {
                text.Span("Página ");
                text.CurrentPageNumber();
                text.Span(" de ");
                text.TotalPages();
                text.Span($" | SistemIA - {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Medium);
            });
        }

        // ========== INFORME DE CIERRE DE COMPLEJOS ==========
        public async Task<byte[]> GenerarPdfCierreComplejosAsync(int sucursalId, DateTime fechaCaja, int? turno = null, int? idCaja = null)
        {
            var (suc, empresa, sucursal, logo) = await ObtenerDatosSucursal(sucursalId);
            await using var ctx = await _dbFactory.CreateDbContextAsync();

            var filtros = $"Fecha: {fechaCaja:dd/MM/yyyy}";
            if (turno.HasValue && turno > 0) filtros += $" | Turno: {turno}";
            if (idCaja.HasValue && idCaja > 0) filtros += $" | Caja: {idCaja}";

            // Consulta base de pedidos cerrados (pagados)
            var queryPedidos = ctx.Pedidos
                .Include(p => p.Detalles)
                .Include(p => p.Mesa)
                .Where(p => p.IdSucursal == sucursalId
                    && p.FechaCaja.HasValue && p.FechaCaja.Value.Date == fechaCaja.Date
                    && (p.Estado == "Pagado" || p.Estado == "Cerrado"));

            if (idCaja.HasValue && idCaja > 0)
                queryPedidos = queryPedidos.Where(p => p.IdCaja == idCaja);

            if (turno.HasValue && turno > 0)
                queryPedidos = queryPedidos.Where(p => p.Turno == turno);

            var pedidos = await queryPedidos.ToListAsync();

            // Consulta de ventas para desglose financiero
            var queryVentas = ctx.Ventas
                .Where(v => v.IdSucursal == sucursalId
                    && v.Fecha.Date == fechaCaja.Date
                    && v.Estado != "Anulada");

            if (idCaja.HasValue && idCaja > 0)
                queryVentas = queryVentas.Where(v => v.IdCaja == idCaja);

            if (turno.HasValue && turno > 0)
                queryVentas = queryVentas.Where(v => v.Turno.HasValue && v.Turno.Value == turno);

            var ventas = await queryVentas.ToListAsync();

            // Calcular resumen
            var mesasAtendidas = pedidos.Select(p => p.IdMesa).Distinct().Count();
            var totalComensales = pedidos.Sum(p => p.Comensales);
            var totalVentas = ventas.Sum(v => v.Total);
            var cantVentas = ventas.Count;
            var ventasContado = ventas.Where(v => v.FormaPago?.ToUpper() == "CONTADO").Sum(v => v.Total);
            var cantVentasContado = ventas.Count(v => v.FormaPago?.ToUpper() == "CONTADO");
            var ventasCredito = ventas.Where(v => v.FormaPago?.ToUpper() == "CREDITO").Sum(v => v.Total);
            var cantVentasCredito = ventas.Count(v => v.FormaPago?.ToUpper() == "CREDITO");
            var totalCargoServicio = pedidos.Sum(p => p.CargoServicio);

            // Tiempo promedio
            var tiempos = pedidos
                .Where(p => p.FechaCierre.HasValue)
                .Select(p => (p.FechaCierre!.Value - p.FechaApertura).TotalMinutes)
                .ToList();
            var tiempoPromedioMin = tiempos.Any() ? (int)tiempos.Average() : 0;
            var tiempoPromedioTexto = tiempoPromedioMin > 0 ? $"{tiempoPromedioMin / 60}h {tiempoPromedioMin % 60}m" : "0m";

            // Ticket promedio
            var ticketPromedio = mesasAtendidas > 0 ? totalVentas / mesasAtendidas : 0;

            // Ventas por mesero
            var ventasPorMesero = pedidos
                .GroupBy(p => p.NombreMesero ?? "Sin asignar")
                .Select(g => new 
                {
                    Mesero = g.Key,
                    Mesas = g.Select(p => p.IdMesa).Distinct().Count(),
                    Pedidos = g.Count(),
                    Total = g.Sum(p => p.Total),
                    TiempoPromMin = g.Where(p => p.FechaCierre.HasValue)
                        .Select(p => (p.FechaCierre!.Value - p.FechaApertura).TotalMinutes)
                        .DefaultIfEmpty(0).Average()
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            // Productos más vendidos
            var productosMasVendidos = pedidos
                .SelectMany(p => p.Detalles ?? new List<PedidoDetalle>())
                .GroupBy(d => d.Descripcion ?? "Producto")
                .Select(g => new 
                {
                    Producto = g.Key,
                    Cantidad = g.Sum(d => d.Cantidad),
                    Total = g.Sum(d => d.Importe)
                })
                .OrderByDescending(x => x.Cantidad)
                .Take(10)
                .ToList();

            // Generar PDF
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(25);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Element(h => GenerarEncabezado(h, "🏢 INFORME DE CIERRE DE COMPLEJOS", empresa, sucursal, filtros, logo));

                    page.Content().Column(col =>
                    {
                        // ========== RESUMEN GENERAL ==========
                        col.Item().PaddingBottom(10).Text("RESUMEN GENERAL").Bold().FontSize(11).FontColor(Colors.Blue.Darken2);
                        col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"Mesas Atendidas: {mesasAtendidas}").Bold();
                                c.Item().Text($"Total Comensales: {totalComensales}");
                                c.Item().Text($"Tiempo Promedio: {tiempoPromedioTexto}");
                            });
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"Total Ventas: {totalVentas:N0} ₲").Bold().FontColor(Colors.Green.Darken2);
                                c.Item().Text($"Cantidad Ventas: {cantVentas}");
                                c.Item().Text($"Ticket Promedio: {ticketPromedio:N0} ₲");
                            });
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"Contado: {ventasContado:N0} ₲ ({cantVentasContado})");
                                c.Item().Text($"Crédito: {ventasCredito:N0} ₲ ({cantVentasCredito})");
                                c.Item().Text($"Cargo Servicio: {totalCargoServicio:N0} ₲");
                            });
                        });

                        // ========== VENTAS POR MESERO ==========
                        if (ventasPorMesero.Any())
                        {
                            col.Item().PaddingTop(15).PaddingBottom(5).Text("VENTAS POR MESERO").Bold().FontSize(11).FontColor(Colors.Blue.Darken2);
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.ConstantColumn(60);
                                    columns.ConstantColumn(60);
                                    columns.ConstantColumn(100);
                                    columns.ConstantColumn(80);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Darken3).Padding(5).Text("Mesero").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Grey.Darken3).Padding(5).AlignCenter().Text("Mesas").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Grey.Darken3).Padding(5).AlignCenter().Text("Pedidos").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Grey.Darken3).Padding(5).AlignRight().Text("Total Vendido").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Grey.Darken3).Padding(5).AlignCenter().Text("Tiempo Prom").FontColor(Colors.White).Bold();
                                });

                                foreach (var m in ventasPorMesero)
                                {
                                    var tp = (int)m.TiempoPromMin;
                                    var tiempoTxt = tp > 0 ? $"{tp / 60}h {tp % 60}m" : "-";
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(m.Mesero);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(m.Mesas.ToString());
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(m.Pedidos.ToString());
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text($"{m.Total:N0} ₲").Bold();
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(tiempoTxt);
                                }

                                // Total
                                table.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("TOTAL").Bold();
                                table.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignCenter().Text(ventasPorMesero.Sum(m => m.Mesas).ToString()).Bold();
                                table.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignCenter().Text(ventasPorMesero.Sum(m => m.Pedidos).ToString()).Bold();
                                table.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text($"{ventasPorMesero.Sum(m => m.Total):N0} ₲").Bold();
                                table.Cell().Background(Colors.Grey.Lighten3).Padding(4);
                            });
                        }

                        // ========== PRODUCTOS MÁS VENDIDOS ==========
                        if (productosMasVendidos.Any())
                        {
                            col.Item().PaddingTop(15).PaddingBottom(5).Text("TOP 10 PRODUCTOS MÁS VENDIDOS").Bold().FontSize(11).FontColor(Colors.Blue.Darken2);
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(30);
                                    columns.RelativeColumn(4);
                                    columns.ConstantColumn(80);
                                    columns.ConstantColumn(100);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Orange.Darken2).Padding(5).AlignCenter().Text("#").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Orange.Darken2).Padding(5).Text("Producto").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Orange.Darken2).Padding(5).AlignCenter().Text("Cantidad").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Orange.Darken2).Padding(5).AlignRight().Text("Total").FontColor(Colors.White).Bold();
                                });

                                var i = 1;
                                foreach (var p in productosMasVendidos)
                                {
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(i.ToString());
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(p.Producto);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(p.Cantidad.ToString("N0"));
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text($"{p.Total:N0} ₲");
                                    i++;
                                }
                            });
                        }
                    });

                    page.Footer().Element(GenerarPie);
                });
            });

            return document.GeneratePdf();
        }

        // ========== INFORMES DE GIMNASIO ==========

        /// <summary>
        /// Genera PDF de asistencia a clases grupales del gimnasio para una fecha específica.
        /// </summary>
        public async Task<byte[]> GenerarPdfGimnasioAsistenciaClasesAsync(int sucursalId, DateTime fecha)
        {
            var (suc, empresa, sucursal, logo) = await ObtenerDatosSucursal(sucursalId);
            await using var ctx = await _dbFactory.CreateDbContextAsync();

            var filtros = $"Fecha: {fecha:dd/MM/yyyy}";

            // Obtener reservas del día con datos relacionados
            var reservas = await ctx.ReservasClases
                .Include(r => r.Horario)
                    .ThenInclude(h => h.Clase)
                .Include(r => r.Horario)
                    .ThenInclude(h => h.Instructor)
                .Include(r => r.Cliente)
                .Where(r => r.FechaClase.Date == fecha.Date)
                .OrderBy(r => r.Horario.HoraInicio)
                .ThenBy(r => r.Horario.Clase.Nombre)
                .ToListAsync();

            // Agrupar por clase
            var reservasPorClase = reservas
                .GroupBy(r => new { IdHorario = r.IdHorario, NombreClase = r.Horario?.Clase?.Nombre ?? "Sin Clase", Hora = r.Horario?.HoraInicio })
                .Select(g => new
                {
                    Clase = g.Key.NombreClase,
                    Hora = g.Key.Hora?.ToString(@"hh\:mm") ?? "--:--",
                    Instructor = g.FirstOrDefault()?.Horario?.Instructor?.NombreCompleto ?? "N/A",
                    TotalReservas = g.Count(),
                    Asistieron = g.Count(r => r.Estado == "Asistió"),
                    NoAsistieron = g.Count(r => r.Estado == "NoAsistió"),
                    Canceladas = g.Count(r => r.Estado == "Cancelada"),
                    Pendientes = g.Count(r => r.Estado == "Confirmada" || r.Estado == "Pendiente"),
                    Reservas = g.ToList()
                })
                .OrderBy(x => x.Hora)
                .ToList();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Element(c => GenerarEncabezado(c, "Asistencia a Clases Grupales", empresa, sucursal, filtros, logo));

                    page.Content().Element(content =>
                    {
                        content.Column(col =>
                        {
                            // Resumen del día
                            col.Item().PaddingBottom(15).Row(row =>
                            {
                                row.RelativeItem().Background(Colors.Blue.Lighten4).Padding(10).Column(c =>
                                {
                                    c.Item().Text("Total Reservas").Bold();
                                    c.Item().Text(reservas.Count.ToString()).FontSize(14);
                                });
                                row.ConstantItem(10);
                                row.RelativeItem().Background(Colors.Green.Lighten4).Padding(10).Column(c =>
                                {
                                    c.Item().Text("Asistieron").Bold();
                                    c.Item().Text(reservas.Count(r => r.Estado == "Asistió").ToString()).FontSize(14);
                                });
                                row.ConstantItem(10);
                                row.RelativeItem().Background(Colors.Red.Lighten4).Padding(10).Column(c =>
                                {
                                    c.Item().Text("No Asistieron").Bold();
                                    c.Item().Text(reservas.Count(r => r.Estado == "NoAsistió").ToString()).FontSize(14);
                                });
                                row.ConstantItem(10);
                                row.RelativeItem().Background(Colors.Orange.Lighten4).Padding(10).Column(c =>
                                {
                                    c.Item().Text("Canceladas").Bold();
                                    c.Item().Text(reservas.Count(r => r.Estado == "Cancelada").ToString()).FontSize(14);
                                });
                            });

                            // Por cada clase
                            foreach (var clase in reservasPorClase)
                            {
                                col.Item().PaddingBottom(10).Column(claseCol =>
                                {
                                    // Header de la clase
                                    claseCol.Item().Background(Colors.Grey.Lighten3).Padding(8).Row(row =>
                                    {
                                        row.RelativeItem().Text($"{clase.Hora} - {clase.Clase}").Bold().FontSize(11);
                                        row.RelativeItem().AlignRight().Text($"Instructor: {clase.Instructor}").FontSize(9);
                                    });

                                    // Tabla de asistentes
                                    claseCol.Item().Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(3); // Cliente
                                            columns.ConstantColumn(80); // Estado
                                            columns.ConstantColumn(60); // Check-In
                                            columns.RelativeColumn(2); // Membresía
                                        });

                                        table.Header(header =>
                                        {
                                            header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Cliente").FontColor(Colors.White).Bold();
                                            header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("Estado").FontColor(Colors.White).Bold();
                                            header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("Check-In").FontColor(Colors.White).Bold();
                                            header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Membresía").FontColor(Colors.White).Bold();
                                        });

                                        foreach (var r in clase.Reservas)
                                        {
                                            var bgColor = r.Estado == "Asistió" ? Colors.Green.Lighten5 :
                                                          r.Estado == "NoAsistió" ? Colors.Red.Lighten5 :
                                                          r.Estado == "Cancelada" ? Colors.Orange.Lighten5 : Colors.White;

                                            table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(r.Cliente?.RazonSocial ?? "N/A");
                                            table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(r.Estado ?? "-");
                                            table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(r.HoraCheckIn?.ToString(@"HH\:mm") ?? "-");
                                            table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(r.Membresia?.Producto?.Descripcion ?? "-");
                                        }
                                    });

                                    // Resumen de la clase
                                    claseCol.Item().PaddingTop(5).PaddingBottom(10).AlignRight().Text(
                                        $"Reservas: {clase.TotalReservas} | ✓ {clase.Asistieron} | ✗ {clase.NoAsistieron} | Canceladas: {clase.Canceladas}"
                                    ).FontSize(8).FontColor(Colors.Grey.Darken1);
                                });
                            }

                            if (reservasPorClase.Count == 0)
                            {
                                col.Item().Padding(20).AlignCenter().Text("No hay reservas para esta fecha").FontColor(Colors.Grey.Medium);
                            }
                        });
                    });

                    page.Footer().Element(GenerarPie);
                });
            });

            return document.GeneratePdf();
        }

        /// <summary>
        /// Genera PDF con el estado actual de todas las membresías del gimnasio.
        /// </summary>
        public async Task<byte[]> GenerarPdfGimnasioMembresiasAsync(int sucursalId)
        {
            var (suc, empresa, sucursal, logo) = await ObtenerDatosSucursal(sucursalId);
            await using var ctx = await _dbFactory.CreateDbContextAsync();

            var filtros = $"Fecha: {DateTime.Now:dd/MM/yyyy}";

            var membresias = await ctx.MembresiasClientes
                .Include(m => m.Cliente)
                .Include(m => m.Producto)
                .Where(m => m.Estado == "Activa" || m.Estado == "Congelada" || m.Estado == "PorVencer")
                .OrderBy(m => m.FechaVencimiento)
                .ToListAsync();

            // Agrupar por estado
            var activas = membresias.Where(m => m.Estado == "Activa").ToList();
            var congeladas = membresias.Where(m => m.Estado == "Congelada").ToList();
            var porVencer = membresias.Where(m => m.FechaVencimiento <= DateTime.Now.AddDays(15)).ToList();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Element(c => GenerarEncabezado(c, "Resumen de Membresías", empresa, sucursal, filtros, logo));

                    page.Content().Element(content =>
                    {
                        content.Column(col =>
                        {
                            // Resumen
                            col.Item().PaddingBottom(15).Row(row =>
                            {
                                row.RelativeItem().Background(Colors.Green.Lighten4).Padding(10).Column(c =>
                                {
                                    c.Item().Text("Activas").Bold();
                                    c.Item().Text(activas.Count.ToString()).FontSize(14);
                                });
                                row.ConstantItem(10);
                                row.RelativeItem().Background(Colors.Blue.Lighten4).Padding(10).Column(c =>
                                {
                                    c.Item().Text("Congeladas").Bold();
                                    c.Item().Text(congeladas.Count.ToString()).FontSize(14);
                                });
                                row.ConstantItem(10);
                                row.RelativeItem().Background(Colors.Orange.Lighten4).Padding(10).Column(c =>
                                {
                                    c.Item().Text("Por Vencer (15 días)").Bold();
                                    c.Item().Text(porVencer.Count.ToString()).FontSize(14);
                                });
                                row.ConstantItem(10);
                                row.RelativeItem().Background(Colors.Grey.Lighten3).Padding(10).Column(c =>
                                {
                                    c.Item().Text("Ingresos Totales").Bold();
                                    c.Item().Text($"{activas.Sum(m => m.MontoTotal):N0} ₲").FontSize(12);
                                });
                            });

                            // Tabla de membresías
                            col.Item().PaddingTop(10).Text("Listado de Membresías Activas").Bold().FontSize(11);
                            col.Item().PaddingTop(5).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3); // Cliente
                                    columns.RelativeColumn(2); // Plan
                                    columns.ConstantColumn(70); // Inicio
                                    columns.ConstantColumn(70); // Fin
                                    columns.ConstantColumn(80); // Monto
                                    columns.ConstantColumn(60); // Estado
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Green.Darken2).Padding(5).Text("Cliente").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Green.Darken2).Padding(5).Text("Plan").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Green.Darken2).Padding(5).AlignCenter().Text("Inicio").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Green.Darken2).Padding(5).AlignCenter().Text("Vence").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Green.Darken2).Padding(5).AlignRight().Text("Monto").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Green.Darken2).Padding(5).AlignCenter().Text("Estado").FontColor(Colors.White).Bold();
                                });

                                foreach (var m in membresias.Take(100)) // Limitar para no generar PDFs enormes
                                {
                                    var diasRestantes = (m.FechaVencimiento - DateTime.Now).Days;
                                    var bgColor = diasRestantes <= 7 ? Colors.Red.Lighten5 :
                                                  diasRestantes <= 15 ? Colors.Orange.Lighten5 : Colors.White;

                                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(m.Cliente?.RazonSocial ?? "N/A");
                                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(m.Producto?.Descripcion ?? "N/A");
                                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(m.FechaInicio.ToString("dd/MM/yy"));
                                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(m.FechaVencimiento.ToString("dd/MM/yy"));
                                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text($"{m.MontoTotal:N0}");
                                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(m.Estado ?? "-");
                                }
                            });
                        });
                    });

                    page.Footer().Element(GenerarPie);
                });
            });

            return document.GeneratePdf();
        }

        /// <summary>
        /// Genera PDF de ranking de clases más populares del gimnasio.
        /// </summary>
        public async Task<byte[]> GenerarPdfGimnasioClasesPopularesAsync(int sucursalId, DateTime fechaDesde, DateTime fechaHasta)
        {
            var (suc, empresa, sucursal, logo) = await ObtenerDatosSucursal(sucursalId);
            await using var ctx = await _dbFactory.CreateDbContextAsync();

            var filtros = $"Período: {fechaDesde:dd/MM/yyyy} al {fechaHasta:dd/MM/yyyy}";

            // Obtener reservas del período
            var reservas = await ctx.ReservasClases
                .Include(r => r.Horario)
                    .ThenInclude(h => h.Clase)
                .Where(r => r.FechaClase.Date >= fechaDesde.Date && r.FechaClase.Date <= fechaHasta.Date)
                .ToListAsync();

            // Agrupar por clase
            var rankingClases = reservas
                .GroupBy(r => r.Horario?.Clase?.Nombre ?? "Sin Clase")
                .Select(g => new
                {
                    Clase = g.Key,
                    TotalReservas = g.Count(),
                    Asistencias = g.Count(r => r.Estado == "Asistió"),
                    Cancelaciones = g.Count(r => r.Estado == "Cancelada"),
                    TasaAsistencia = g.Count() > 0 ? (decimal)g.Count(r => r.Estado == "Asistió") / g.Count() * 100 : 0
                })
                .OrderByDescending(x => x.TotalReservas)
                .ToList();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Element(c => GenerarEncabezado(c, "Ranking de Clases Populares", empresa, sucursal, filtros, logo));

                    page.Content().Element(content =>
                    {
                        content.Column(col =>
                        {
                            // Resumen
                            col.Item().PaddingBottom(15).Row(row =>
                            {
                                row.RelativeItem().Background(Colors.Purple.Lighten4).Padding(10).Column(c =>
                                {
                                    c.Item().Text("Total Reservas").Bold();
                                    c.Item().Text(reservas.Count.ToString()).FontSize(14);
                                });
                                row.ConstantItem(10);
                                row.RelativeItem().Background(Colors.Green.Lighten4).Padding(10).Column(c =>
                                {
                                    c.Item().Text("Asistencias").Bold();
                                    c.Item().Text(reservas.Count(r => r.Estado == "Asistió").ToString()).FontSize(14);
                                });
                                row.ConstantItem(10);
                                row.RelativeItem().Background(Colors.Blue.Lighten4).Padding(10).Column(c =>
                                {
                                    c.Item().Text("Clases Diferentes").Bold();
                                    c.Item().Text(rankingClases.Count.ToString()).FontSize(14);
                                });
                            });

                            // Tabla ranking
                            col.Item().PaddingTop(10).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(40); // Posición
                                    columns.RelativeColumn(3); // Clase
                                    columns.ConstantColumn(80); // Reservas
                                    columns.ConstantColumn(80); // Asistencias
                                    columns.ConstantColumn(80); // Cancelaciones
                                    columns.ConstantColumn(80); // Tasa
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Purple.Darken2).Padding(5).AlignCenter().Text("#").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Purple.Darken2).Padding(5).Text("Clase").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Purple.Darken2).Padding(5).AlignCenter().Text("Reservas").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Purple.Darken2).Padding(5).AlignCenter().Text("Asistencias").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Purple.Darken2).Padding(5).AlignCenter().Text("Canceladas").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Purple.Darken2).Padding(5).AlignCenter().Text("% Asist.").FontColor(Colors.White).Bold();
                                });

                                int pos = 1;
                                foreach (var c in rankingClases)
                                {
                                    var bgColor = pos <= 3 ? Colors.Yellow.Lighten4 : Colors.White;
                                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(pos.ToString()).Bold();
                                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(c.Clase);
                                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(c.TotalReservas.ToString());
                                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(c.Asistencias.ToString());
                                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(c.Cancelaciones.ToString());
                                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text($"{c.TasaAsistencia:N1}%");
                                    pos++;
                                }
                            });
                        });
                    });

                    page.Footer().Element(GenerarPie);
                });
            });

            return document.GeneratePdf();
        }

        /// <summary>
        /// Genera PDF de rendimiento de instructores del gimnasio.
        /// </summary>
        public async Task<byte[]> GenerarPdfGimnasioInstructoresAsync(int sucursalId, DateTime fechaDesde, DateTime fechaHasta)
        {
            var (suc, empresa, sucursal, logo) = await ObtenerDatosSucursal(sucursalId);
            await using var ctx = await _dbFactory.CreateDbContextAsync();

            var filtros = $"Período: {fechaDesde:dd/MM/yyyy} al {fechaHasta:dd/MM/yyyy}";

            // Obtener instructores activos
            var instructores = await ctx.Instructores.Where(i => i.Activo).ToListAsync();

            // Obtener horarios del período con reservas
            var horarios = await ctx.HorariosClases
                .Include(h => h.Clase)
                .Include(h => h.Instructor)
                .Where(h => (h.TipoHorario == "Recurrente" && h.FechaEspecifica == null) ||
                            (h.FechaEspecifica >= fechaDesde && h.FechaEspecifica <= fechaHasta))
                .ToListAsync();

            var reservas = await ctx.ReservasClases
                .Include(r => r.Horario)
                .Where(r => r.FechaClase.Date >= fechaDesde.Date && r.FechaClase.Date <= fechaHasta.Date)
                .ToListAsync();

            // Estadísticas por instructor
            var estadisticas = instructores.Select(i =>
            {
                var horariosInstructor = horarios.Where(h => h.IdInstructor == i.IdInstructor).ToList();
                var reservasInstructor = reservas.Where(r => r.Horario?.IdInstructor == i.IdInstructor).ToList();

                return new
                {
                    Instructor = i.NombreCompleto,
                    Especialidades = i.Especialidades ?? "-",
                    ClasesImpartidas = horariosInstructor.Count,
                    TotalAlumnos = reservasInstructor.Count,
                    Asistencias = reservasInstructor.Count(r => r.Estado == "Asistió"),
                    TasaAsistencia = reservasInstructor.Count > 0 ?
                        (decimal)reservasInstructor.Count(r => r.Estado == "Asistió") / reservasInstructor.Count * 100 : 0,
                    TarifaHora = i.TarifaHora,
                    HorasTrabajadas = horariosInstructor.Sum(h => h.DuracionMinutos) / 60m,
                    IngresoEstimado = horariosInstructor.Sum(h => h.DuracionMinutos) / 60m * i.TarifaHora
                };
            })
            .OrderByDescending(x => x.Asistencias)
            .ToList();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Element(c => GenerarEncabezado(c, "Informe de Instructores", empresa, sucursal, filtros, logo));

                    page.Content().Element(content =>
                    {
                        content.Column(col =>
                        {
                            // Resumen
                            col.Item().PaddingBottom(15).Row(row =>
                            {
                                row.RelativeItem().Background(Colors.Indigo.Lighten4).Padding(10).Column(c =>
                                {
                                    c.Item().Text("Instructores Activos").Bold();
                                    c.Item().Text(instructores.Count.ToString()).FontSize(14);
                                });
                                row.ConstantItem(10);
                                row.RelativeItem().Background(Colors.Blue.Lighten4).Padding(10).Column(c =>
                                {
                                    c.Item().Text("Total Clases").Bold();
                                    c.Item().Text(horarios.Count.ToString()).FontSize(14);
                                });
                                row.ConstantItem(10);
                                row.RelativeItem().Background(Colors.Green.Lighten4).Padding(10).Column(c =>
                                {
                                    c.Item().Text("Total Alumnos").Bold();
                                    c.Item().Text(reservas.Count.ToString()).FontSize(14);
                                });
                                row.ConstantItem(10);
                                row.RelativeItem().Background(Colors.Orange.Lighten4).Padding(10).Column(c =>
                                {
                                    c.Item().Text("Inversión Instructores").Bold();
                                    c.Item().Text($"{estadisticas.Sum(e => e.IngresoEstimado):N0} ₲").FontSize(12);
                                });
                            });

                            // Tabla
                            col.Item().PaddingTop(10).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2); // Instructor
                                    columns.RelativeColumn(2); // Especialidades
                                    columns.ConstantColumn(60); // Clases
                                    columns.ConstantColumn(60); // Alumnos
                                    columns.ConstantColumn(60); // Asistencias
                                    columns.ConstantColumn(60); // Tasa
                                    columns.ConstantColumn(60); // Horas
                                    columns.ConstantColumn(80); // Tarifa
                                    columns.ConstantColumn(80); // Total
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Indigo.Darken2).Padding(5).Text("Instructor").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Indigo.Darken2).Padding(5).Text("Especialidades").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Indigo.Darken2).Padding(5).AlignCenter().Text("Clases").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Indigo.Darken2).Padding(5).AlignCenter().Text("Alumnos").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Indigo.Darken2).Padding(5).AlignCenter().Text("Asist.").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Indigo.Darken2).Padding(5).AlignCenter().Text("%").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Indigo.Darken2).Padding(5).AlignCenter().Text("Horas").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Indigo.Darken2).Padding(5).AlignRight().Text("₲/Hora").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Indigo.Darken2).Padding(5).AlignRight().Text("Total").FontColor(Colors.White).Bold();
                                });

                                foreach (var e in estadisticas)
                                {
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(e.Instructor);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(e.Especialidades).FontSize(8);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(e.ClasesImpartidas.ToString());
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(e.TotalAlumnos.ToString());
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(e.Asistencias.ToString());
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text($"{e.TasaAsistencia:N1}%");
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text($"{e.HorasTrabajadas:N1}");
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text($"{e.TarifaHora:N0}");
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text($"{e.IngresoEstimado:N0}");
                                }
                            });
                        });
                    });

                    page.Footer().Element(GenerarPie);
                });
            });

            return document.GeneratePdf();
        }

        /// <summary>
        /// Genera PDF de membresías próximas a vencer.
        /// </summary>
        public async Task<byte[]> GenerarPdfGimnasioMembresiasPorVencerAsync(int sucursalId, int diasAnticipacion = 15)
        {
            var (suc, empresa, sucursal, logo) = await ObtenerDatosSucursal(sucursalId);
            await using var ctx = await _dbFactory.CreateDbContextAsync();

            var fechaLimite = DateTime.Now.AddDays(diasAnticipacion);
            var filtros = $"Vencen antes del: {fechaLimite:dd/MM/yyyy}";

            var membresias = await ctx.MembresiasClientes
                .Include(m => m.Cliente)
                .Include(m => m.Producto)
                .Where(m => m.Estado == "Activa" && m.FechaVencimiento <= fechaLimite && m.FechaVencimiento >= DateTime.Now)
                .OrderBy(m => m.FechaVencimiento)
                .ToListAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Element(c => GenerarEncabezado(c, "Membresías por Vencer", empresa, sucursal, filtros, logo));

                    page.Content().Element(content =>
                    {
                        content.Column(col =>
                        {
                            // Alerta
                            col.Item().PaddingBottom(15).Background(Colors.Orange.Lighten4).Padding(15).Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text($"⚠️ {membresias.Count} membresías vencen en los próximos {diasAnticipacion} días").Bold().FontSize(12);
                                    c.Item().Text($"Ingreso en riesgo: {membresias.Sum(m => m.MontoTotal):N0} ₲").FontSize(10);
                                });
                            });

                            // Tabla
                            col.Item().PaddingTop(10).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3); // Cliente
                                    columns.RelativeColumn(2); // Plan
                                    columns.ConstantColumn(80); // Vence
                                    columns.ConstantColumn(60); // Días rest
                                    columns.ConstantColumn(80); // Monto
                                    columns.RelativeColumn(2); // Teléfono
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Orange.Darken2).Padding(5).Text("Cliente").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Orange.Darken2).Padding(5).Text("Plan").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Orange.Darken2).Padding(5).AlignCenter().Text("Vence").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Orange.Darken2).Padding(5).AlignCenter().Text("Días").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Orange.Darken2).Padding(5).AlignRight().Text("Monto").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Orange.Darken2).Padding(5).Text("Teléfono").FontColor(Colors.White).Bold();
                                });

                                foreach (var m in membresias)
                                {
                                    var diasRestantes = (m.FechaVencimiento - DateTime.Now).Days;
                                    var bgColor = diasRestantes <= 3 ? Colors.Red.Lighten4 :
                                                  diasRestantes <= 7 ? Colors.Orange.Lighten4 : Colors.Yellow.Lighten4;

                                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(m.Cliente?.RazonSocial ?? "N/A");
                                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(m.Producto?.Descripcion ?? "N/A");
                                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(m.FechaVencimiento.ToString("dd/MM/yy"));
                                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(diasRestantes.ToString()).Bold();
                                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text($"{m.MontoTotal:N0}");
                                    table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(m.Cliente?.Telefono ?? "-");
                                }
                            });

                            if (membresias.Count == 0)
                            {
                                col.Item().Padding(20).AlignCenter().Text("No hay membresías próximas a vencer 🎉").FontColor(Colors.Green.Darken1);
                            }
                        });
                    });

                    page.Footer().Element(GenerarPie);
                });
            });

            return document.GeneratePdf();
        }
    }
}
