using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Pagos_Cuotas_Timbrado_ComposicionCaja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComposicionesCaja",
                columns: table => new
                {
                    IdComposicionCaja = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdVenta = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdMoneda = table.Column<int>(type: "int", nullable: true),
                    TipoCambioAplicado = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    MontoTotal = table.Column<decimal>(type: "decimal(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComposicionesCaja", x => x.IdComposicionCaja);
                    table.ForeignKey(
                        name: "FK_ComposicionesCaja_Monedas_IdMoneda",
                        column: x => x.IdMoneda,
                        principalTable: "Monedas",
                        principalColumn: "IdMoneda",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComposicionesCaja_Ventas_IdVenta",
                        column: x => x.IdVenta,
                        principalTable: "Ventas",
                        principalColumn: "IdVenta",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Timbrados",
                columns: table => new
                {
                    IdTimbrado = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroTimbrado = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    FechaInicioVigencia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFinVigencia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Establecimiento = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    PuntoExpedicion = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    TipoDocumento = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    IdSucursal = table.Column<int>(type: "int", nullable: false),
                    IdCaja = table.Column<int>(type: "int", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Timbrados", x => x.IdTimbrado);
                    table.ForeignKey(
                        name: "FK_Timbrados_Cajas_IdCaja",
                        column: x => x.IdCaja,
                        principalTable: "Cajas",
                        principalColumn: "id_caja",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Timbrados_Sucursal_IdSucursal",
                        column: x => x.IdSucursal,
                        principalTable: "Sucursal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VentasPagos",
                columns: table => new
                {
                    IdVentaPago = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdVenta = table.Column<int>(type: "int", nullable: false),
                    CondicionOperacion = table.Column<int>(type: "int", nullable: false),
                    IdMoneda = table.Column<int>(type: "int", nullable: true),
                    TipoCambio = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    ImporteTotal = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Anticipo = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    DescuentoTotal = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    RecargoTotal = table.Column<decimal>(type: "decimal(18,4)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VentasPagos", x => x.IdVentaPago);
                    table.ForeignKey(
                        name: "FK_VentasPagos_Monedas_IdMoneda",
                        column: x => x.IdMoneda,
                        principalTable: "Monedas",
                        principalColumn: "IdMoneda",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VentasPagos_Ventas_IdVenta",
                        column: x => x.IdVenta,
                        principalTable: "Ventas",
                        principalColumn: "IdVenta",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComposicionesCajaDetalles",
                columns: table => new
                {
                    IdComposicionCajaDetalle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdComposicionCaja = table.Column<int>(type: "int", nullable: false),
                    Medio = table.Column<int>(type: "int", nullable: false),
                    IdMoneda = table.Column<int>(type: "int", nullable: true),
                    TipoCambio = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Factor = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    MontoGs = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    BancoCheque = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    NumeroCheque = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    FechaCobroCheque = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TipoTarjeta = table.Column<int>(type: "int", nullable: true),
                    MarcaTarjeta = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Ultimos4 = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    NumeroAutorizacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NombreEmisorTarjeta = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    BancoTransferencia = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NumeroComprobante = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Observacion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComposicionesCajaDetalles", x => x.IdComposicionCajaDetalle);
                    table.ForeignKey(
                        name: "FK_ComposicionesCajaDetalles_ComposicionesCaja_IdComposicionCaja",
                        column: x => x.IdComposicionCaja,
                        principalTable: "ComposicionesCaja",
                        principalColumn: "IdComposicionCaja",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComposicionesCajaDetalles_Monedas_IdMoneda",
                        column: x => x.IdMoneda,
                        principalTable: "Monedas",
                        principalColumn: "IdMoneda",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VentasCuotas",
                columns: table => new
                {
                    IdVentaCuota = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdVentaPago = table.Column<int>(type: "int", nullable: false),
                    NumeroCuota = table.Column<int>(type: "int", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MontoCuota = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Pagada = table.Column<bool>(type: "bit", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VentasCuotas", x => x.IdVentaCuota);
                    table.ForeignKey(
                        name: "FK_VentasCuotas_VentasPagos_IdVentaPago",
                        column: x => x.IdVentaPago,
                        principalTable: "VentasPagos",
                        principalColumn: "IdVentaPago",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VentasPagosDetalles",
                columns: table => new
                {
                    IdVentaPagoDetalle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdVentaPago = table.Column<int>(type: "int", nullable: false),
                    Medio = table.Column<int>(type: "int", nullable: false),
                    IdMoneda = table.Column<int>(type: "int", nullable: true),
                    TipoCambio = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Monto = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    MontoGs = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TipoTarjeta = table.Column<int>(type: "int", nullable: true),
                    MarcaTarjeta = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NombreEmisorTarjeta = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Ultimos4 = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    NumeroAutorizacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BancoCheque = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    NumeroCheque = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    FechaCobroCheque = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BancoTransferencia = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NumeroComprobante = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Observacion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VentasPagosDetalles", x => x.IdVentaPagoDetalle);
                    table.ForeignKey(
                        name: "FK_VentasPagosDetalles_Monedas_IdMoneda",
                        column: x => x.IdMoneda,
                        principalTable: "Monedas",
                        principalColumn: "IdMoneda",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VentasPagosDetalles_VentasPagos_IdVentaPago",
                        column: x => x.IdVentaPago,
                        principalTable: "VentasPagos",
                        principalColumn: "IdVentaPago",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComposicionesCaja_IdMoneda",
                table: "ComposicionesCaja",
                column: "IdMoneda");

            migrationBuilder.CreateIndex(
                name: "IX_ComposicionesCaja_IdVenta",
                table: "ComposicionesCaja",
                column: "IdVenta",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComposicionesCajaDetalles_IdComposicionCaja",
                table: "ComposicionesCajaDetalles",
                column: "IdComposicionCaja");

            migrationBuilder.CreateIndex(
                name: "IX_ComposicionesCajaDetalles_IdMoneda",
                table: "ComposicionesCajaDetalles",
                column: "IdMoneda");

            migrationBuilder.CreateIndex(
                name: "IX_Timbrados_IdCaja",
                table: "Timbrados",
                column: "IdCaja");

            migrationBuilder.CreateIndex(
                name: "IX_Timbrados_IdSucursal",
                table: "Timbrados",
                column: "IdSucursal");

            migrationBuilder.CreateIndex(
                name: "IX_Timbrados_NumeroTimbrado_Establecimiento_PuntoExpedicion_TipoDocumento_IdSucursal_IdCaja",
                table: "Timbrados",
                columns: new[] { "NumeroTimbrado", "Establecimiento", "PuntoExpedicion", "TipoDocumento", "IdSucursal", "IdCaja" },
                unique: true,
                filter: "[TipoDocumento] IS NOT NULL AND [IdCaja] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VentasCuotas_IdVentaPago_NumeroCuota",
                table: "VentasCuotas",
                columns: new[] { "IdVentaPago", "NumeroCuota" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VentasPagos_IdMoneda",
                table: "VentasPagos",
                column: "IdMoneda");

            migrationBuilder.CreateIndex(
                name: "IX_VentasPagos_IdVenta",
                table: "VentasPagos",
                column: "IdVenta",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VentasPagosDetalles_IdMoneda",
                table: "VentasPagosDetalles",
                column: "IdMoneda");

            migrationBuilder.CreateIndex(
                name: "IX_VentasPagosDetalles_IdVentaPago",
                table: "VentasPagosDetalles",
                column: "IdVentaPago");

            // No se tocan tablas existentes; solo se crean nuevas.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Solo se revierten las tablas creadas por esta migración.
            migrationBuilder.DropTable(
                name: "ComposicionesCajaDetalles");

            migrationBuilder.DropTable(
                name: "Timbrados");

            migrationBuilder.DropTable(
                name: "VentasCuotas");

            migrationBuilder.DropTable(
                name: "VentasPagosDetalles");

            migrationBuilder.DropTable(
                name: "ComposicionesCaja");

            migrationBuilder.DropTable(
                name: "VentasPagos");
        }
    }
}
