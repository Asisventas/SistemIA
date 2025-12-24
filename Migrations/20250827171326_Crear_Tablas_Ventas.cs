using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Crear_Tablas_Ventas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ventas",
                columns: table => new
                {
                    IdVenta = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    suc = table.Column<int>(type: "int", nullable: false),
                    IdCliente = table.Column<int>(type: "int", nullable: true),
                    Timbrado = table.Column<string>(type: "nchar(8)", fixedLength: true, maxLength: 8, nullable: true),
                    Establecimiento = table.Column<string>(type: "nchar(3)", fixedLength: true, maxLength: 3, nullable: true),
                    PuntoExpedicion = table.Column<string>(type: "nchar(3)", fixedLength: true, maxLength: 3, nullable: true),
                    NumeroFactura = table.Column<string>(type: "nchar(7)", fixedLength: true, maxLength: 7, nullable: true),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdMoneda = table.Column<int>(type: "int", nullable: true),
                    SimboloMoneda = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    CambioDelDia = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    FormaPago = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Plazo = table.Column<int>(type: "int", nullable: true),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Total = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalEnLetras = table.Column<string>(type: "nvarchar(280)", maxLength: 280, nullable: true),
                    TipoRegistro = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CodigoRegistro = table.Column<int>(type: "int", nullable: true),
                    Comentario = table.Column<string>(type: "nvarchar(280)", maxLength: 280, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IdCaja = table.Column<int>(type: "int", nullable: true),
                    Turno = table.Column<int>(type: "int", nullable: true),
                    IdUsuario = table.Column<int>(type: "int", nullable: true),
                    Vendedor = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CDC = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    EstadoSifen = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    FechaEnvioSifen = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MensajeSifen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    XmlCDE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TipoDocumento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IdTipoDocumentoOperacion = table.Column<int>(type: "int", nullable: true),
                    TipoDocumentoOperacionIdTipoDocumentoOperacion = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ventas", x => x.IdVenta);
                    table.ForeignKey(
                        name: "FK_Ventas_Cajas_IdCaja",
                        column: x => x.IdCaja,
                        principalTable: "Cajas",
                        principalColumn: "id_caja",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ventas_Clientes_IdCliente",
                        column: x => x.IdCliente,
                        principalTable: "Clientes",
                        principalColumn: "IdCliente",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ventas_Monedas_IdMoneda",
                        column: x => x.IdMoneda,
                        principalTable: "Monedas",
                        principalColumn: "IdMoneda",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ventas_Sucursal_suc",
                        column: x => x.suc,
                        principalTable: "Sucursal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ventas_TiposDocumentoOperacion_IdTipoDocumentoOperacion",
                        column: x => x.IdTipoDocumentoOperacion,
                        principalTable: "TiposDocumentoOperacion",
                        principalColumn: "IdTipoDocumentoOperacion",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ventas_TiposDocumentoOperacion_TipoDocumentoOperacionIdTipoDocumentoOperacion",
                        column: x => x.TipoDocumentoOperacionIdTipoDocumentoOperacion,
                        principalTable: "TiposDocumentoOperacion",
                        principalColumn: "IdTipoDocumentoOperacion");
                    table.ForeignKey(
                        name: "FK_Ventas_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id_Usu",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VentasDetalles",
                columns: table => new
                {
                    IdVentaDetalle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdVenta = table.Column<int>(type: "int", nullable: false),
                    IdProducto = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Importe = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IVA10 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IVA5 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Exenta = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Grabado10 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Grabado5 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CambioDelDia = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    IdTipoIva = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VentasDetalles", x => x.IdVentaDetalle);
                    table.ForeignKey(
                        name: "FK_VentasDetalles_Productos_IdProducto",
                        column: x => x.IdProducto,
                        principalTable: "Productos",
                        principalColumn: "IdProducto",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VentasDetalles_Ventas_IdVenta",
                        column: x => x.IdVenta,
                        principalTable: "Ventas",
                        principalColumn: "IdVenta",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_IdCaja",
                table: "Ventas",
                column: "IdCaja");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_IdCliente",
                table: "Ventas",
                column: "IdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_IdMoneda",
                table: "Ventas",
                column: "IdMoneda");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_IdTipoDocumentoOperacion",
                table: "Ventas",
                column: "IdTipoDocumentoOperacion");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_IdUsuario",
                table: "Ventas",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_suc",
                table: "Ventas",
                column: "suc");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_TipoDocumentoOperacionIdTipoDocumentoOperacion",
                table: "Ventas",
                column: "TipoDocumentoOperacionIdTipoDocumentoOperacion");

            migrationBuilder.CreateIndex(
                name: "IX_VentasDetalles_IdProducto",
                table: "VentasDetalles",
                column: "IdProducto");

            migrationBuilder.CreateIndex(
                name: "IX_VentasDetalles_IdVenta",
                table: "VentasDetalles",
                column: "IdVenta");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VentasDetalles");

            migrationBuilder.DropTable(
                name: "Ventas");
        }
    }
}
