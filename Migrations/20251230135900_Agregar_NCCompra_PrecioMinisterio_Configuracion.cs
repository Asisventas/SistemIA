using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_NCCompra_PrecioMinisterio_Configuracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PrecioMinisterio",
                table: "Productos",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ConfiguracionSistema",
                columns: table => new
                {
                    IdConfiguracion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmaciaModoActivo = table.Column<bool>(type: "bit", nullable: false),
                    FarmaciaValidarPrecioMinisterio = table.Column<bool>(type: "bit", nullable: false),
                    FarmaciaMostrarPrecioMinisterio = table.Column<bool>(type: "bit", nullable: false),
                    FarmaciaMostrarPrecioMinisterioEnCompras = table.Column<bool>(type: "bit", nullable: false),
                    NombreNegocio = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionSistema", x => x.IdConfiguracion);
                });

            migrationBuilder.CreateTable(
                name: "NotasCreditoCompras",
                columns: table => new
                {
                    IdNotaCreditoCompra = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Establecimiento = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    PuntoExpedicion = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    NumeroNota = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    IdSucursal = table.Column<int>(type: "int", nullable: false),
                    IdCaja = table.Column<int>(type: "int", nullable: true),
                    IdProveedor = table.Column<int>(type: "int", nullable: false),
                    NombreProveedor = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RucProveedor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IdCompraAsociada = table.Column<int>(type: "int", nullable: true),
                    EstablecimientoAsociado = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    PuntoExpedicionAsociado = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    NumeroFacturaAsociado = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    TimbradoAsociado = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaContable = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Turno = table.Column<int>(type: "int", nullable: true),
                    Motivo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IdMoneda = table.Column<int>(type: "int", nullable: true),
                    SimboloMoneda = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    CambioDelDia = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    EsMonedaExtranjera = table.Column<bool>(type: "bit", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalIVA10 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalIVA5 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalExenta = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalDescuento = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalEnLetras = table.Column<string>(type: "nvarchar(280)", maxLength: 280, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ImputarIVA = table.Column<bool>(type: "bit", nullable: true),
                    ImputarIRP = table.Column<bool>(type: "bit", nullable: true),
                    ImputarIRE = table.Column<bool>(type: "bit", nullable: true),
                    NoImputar = table.Column<bool>(type: "bit", nullable: true),
                    IdDeposito = table.Column<int>(type: "int", nullable: true),
                    AfectaStock = table.Column<int>(type: "int", nullable: false),
                    IdUsuario = table.Column<int>(type: "int", nullable: true),
                    CreadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotasCreditoCompras", x => x.IdNotaCreditoCompra);
                    table.ForeignKey(
                        name: "FK_NotasCreditoCompras_Cajas_IdCaja",
                        column: x => x.IdCaja,
                        principalTable: "Cajas",
                        principalColumn: "id_caja",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotasCreditoCompras_Compras_IdCompraAsociada",
                        column: x => x.IdCompraAsociada,
                        principalTable: "Compras",
                        principalColumn: "IdCompra",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotasCreditoCompras_Depositos_IdDeposito",
                        column: x => x.IdDeposito,
                        principalTable: "Depositos",
                        principalColumn: "IdDeposito",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotasCreditoCompras_Monedas_IdMoneda",
                        column: x => x.IdMoneda,
                        principalTable: "Monedas",
                        principalColumn: "IdMoneda",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotasCreditoCompras_ProveedoresSifen_IdProveedor",
                        column: x => x.IdProveedor,
                        principalTable: "ProveedoresSifen",
                        principalColumn: "IdProveedor",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotasCreditoCompras_Sucursal_IdSucursal",
                        column: x => x.IdSucursal,
                        principalTable: "Sucursal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotasCreditoCompras_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id_Usu",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotasCreditoComprasDetalles",
                columns: table => new
                {
                    IdNotaCreditoCompraDetalle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdNotaCreditoCompra = table.Column<int>(type: "int", nullable: false),
                    IdProducto = table.Column<int>(type: "int", nullable: false),
                    CodigoProducto = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NombreProducto = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PorcentajeDescuento = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    MontoDescuento = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Importe = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TasaIVA = table.Column<int>(type: "int", nullable: false),
                    IVA10 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IVA5 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Exenta = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Grabado10 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Grabado5 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IdDepositoItem = table.Column<int>(type: "int", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotasCreditoComprasDetalles", x => x.IdNotaCreditoCompraDetalle);
                    table.ForeignKey(
                        name: "FK_NotasCreditoComprasDetalles_Depositos_IdDepositoItem",
                        column: x => x.IdDepositoItem,
                        principalTable: "Depositos",
                        principalColumn: "IdDeposito",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotasCreditoComprasDetalles_NotasCreditoCompras_IdNotaCreditoCompra",
                        column: x => x.IdNotaCreditoCompra,
                        principalTable: "NotasCreditoCompras",
                        principalColumn: "IdNotaCreditoCompra",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotasCreditoComprasDetalles_Productos_IdProducto",
                        column: x => x.IdProducto,
                        principalTable: "Productos",
                        principalColumn: "IdProducto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotaCreditoCompras_Numeracion",
                table: "NotasCreditoCompras",
                columns: new[] { "IdSucursal", "NumeroNota" },
                unique: true,
                filter: "[NumeroNota] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_NotasCreditoCompras_IdCaja",
                table: "NotasCreditoCompras",
                column: "IdCaja");

            migrationBuilder.CreateIndex(
                name: "IX_NotasCreditoCompras_IdCompraAsociada",
                table: "NotasCreditoCompras",
                column: "IdCompraAsociada");

            migrationBuilder.CreateIndex(
                name: "IX_NotasCreditoCompras_IdDeposito",
                table: "NotasCreditoCompras",
                column: "IdDeposito");

            migrationBuilder.CreateIndex(
                name: "IX_NotasCreditoCompras_IdMoneda",
                table: "NotasCreditoCompras",
                column: "IdMoneda");

            migrationBuilder.CreateIndex(
                name: "IX_NotasCreditoCompras_IdProveedor",
                table: "NotasCreditoCompras",
                column: "IdProveedor");

            migrationBuilder.CreateIndex(
                name: "IX_NotasCreditoCompras_IdUsuario",
                table: "NotasCreditoCompras",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_NotasCreditoComprasDetalles_IdDepositoItem",
                table: "NotasCreditoComprasDetalles",
                column: "IdDepositoItem");

            migrationBuilder.CreateIndex(
                name: "IX_NotasCreditoComprasDetalles_IdNotaCreditoCompra",
                table: "NotasCreditoComprasDetalles",
                column: "IdNotaCreditoCompra");

            migrationBuilder.CreateIndex(
                name: "IX_NotasCreditoComprasDetalles_IdProducto",
                table: "NotasCreditoComprasDetalles",
                column: "IdProducto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracionSistema");

            migrationBuilder.DropTable(
                name: "NotasCreditoComprasDetalles");

            migrationBuilder.DropTable(
                name: "NotasCreditoCompras");

            migrationBuilder.DropColumn(
                name: "PrecioMinisterio",
                table: "Productos");
        }
    }
}
