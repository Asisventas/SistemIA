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
    }
}
