using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Crear_NotasCreditoVentas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotasCreditoVentas",
                columns: table => new
                {
                    IdNotaCredito = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Establecimiento = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    PuntoExpedicion = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    NumeroNota = table.Column<int>(type: "int", nullable: false),
                    IdSucursal = table.Column<int>(type: "int", nullable: false),
                    IdCaja = table.Column<int>(type: "int", nullable: false),
                    IdCliente = table.Column<int>(type: "int", nullable: true),
                    IdVentaAsociada = table.Column<int>(type: "int", nullable: true),
                    TipoDocumentoAsociado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaContable = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Turno = table.Column<int>(type: "int", nullable: true),
                    Motivo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IdMoneda = table.Column<int>(type: "int", nullable: true),
                    SimboloMoneda = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    CambioDelDia = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    EsMonedaExtranjera = table.Column<bool>(type: "bit", nullable: true),
                    Subtotal = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalIVA10 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalIVA5 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalExenta = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalEnLetras = table.Column<string>(type: "nvarchar(280)", maxLength: 280, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Timbrado = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    Serie = table.Column<int>(type: "int", nullable: true),
                    CDC = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CodigoSeguridad = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: true),
                    EstadoSifen = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    FechaEnvioSifen = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MensajeSifen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    XmlCDE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdLote = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IdUsuario = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotasCreditoVentas", x => x.IdNotaCredito);
                    table.ForeignKey(
                        name: "FK_NotasCreditoVentas_Cajas_IdCaja",
                        column: x => x.IdCaja,
                        principalTable: "Cajas",
                        principalColumn: "id_caja",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotasCreditoVentas_Clientes_IdCliente",
                        column: x => x.IdCliente,
                        principalTable: "Clientes",
                        principalColumn: "IdCliente",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotasCreditoVentas_Monedas_IdMoneda",
                        column: x => x.IdMoneda,
                        principalTable: "Monedas",
                        principalColumn: "IdMoneda",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotasCreditoVentas_Sucursal_IdSucursal",
                        column: x => x.IdSucursal,
                        principalTable: "Sucursal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotasCreditoVentas_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id_Usu",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotasCreditoVentas_Ventas_IdVentaAsociada",
                        column: x => x.IdVentaAsociada,
                        principalTable: "Ventas",
                        principalColumn: "IdVenta",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotasCreditoVentasDetalles",
                columns: table => new
                {
                    IdNotaCreditoDetalle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdNotaCredito = table.Column<int>(type: "int", nullable: false),
                    IdProducto = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PorcentajeDescuento = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Descuento = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Importe = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IVA10 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IVA5 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Exenta = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Grabado10 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Grabado5 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CambioDelDia = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    IdTipoIva = table.Column<int>(type: "int", nullable: true),
                    IdDeposito = table.Column<int>(type: "int", nullable: true),
                    Lote = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotasCreditoVentasDetalles", x => x.IdNotaCreditoDetalle);
                    table.ForeignKey(
                        name: "FK_NotasCreditoVentasDetalles_Depositos_IdDeposito",
                        column: x => x.IdDeposito,
                        principalTable: "Depositos",
                        principalColumn: "IdDeposito",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotasCreditoVentasDetalles_NotasCreditoVentas_IdNotaCredito",
                        column: x => x.IdNotaCredito,
                        principalTable: "NotasCreditoVentas",
                        principalColumn: "IdNotaCredito",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotasCreditoVentasDetalles_Productos_IdProducto",
                        column: x => x.IdProducto,
                        principalTable: "Productos",
                        principalColumn: "IdProducto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotaCreditoVentas_Numeracion",
                table: "NotasCreditoVentas",
                columns: new[] { "IdSucursal", "NumeroNota" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotasCreditoVentas_IdCaja",
                table: "NotasCreditoVentas",
                column: "IdCaja");

            migrationBuilder.CreateIndex(
                name: "IX_NotasCreditoVentas_IdCliente",
                table: "NotasCreditoVentas",
                column: "IdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_NotasCreditoVentas_IdMoneda",
                table: "NotasCreditoVentas",
                column: "IdMoneda");

            migrationBuilder.CreateIndex(
                name: "IX_NotasCreditoVentas_IdUsuario",
                table: "NotasCreditoVentas",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_NotasCreditoVentas_IdVentaAsociada",
                table: "NotasCreditoVentas",
                column: "IdVentaAsociada");

            migrationBuilder.CreateIndex(
                name: "IX_NotasCreditoVentasDetalles_IdDeposito",
                table: "NotasCreditoVentasDetalles",
                column: "IdDeposito");

            migrationBuilder.CreateIndex(
                name: "IX_NotasCreditoVentasDetalles_IdNotaCredito",
                table: "NotasCreditoVentasDetalles",
                column: "IdNotaCredito");

            migrationBuilder.CreateIndex(
                name: "IX_NotasCreditoVentasDetalles_IdProducto",
                table: "NotasCreditoVentasDetalles",
                column: "IdProducto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotasCreditoVentasDetalles");

            migrationBuilder.DropTable(
                name: "NotasCreditoVentas");
        }
    }
}
