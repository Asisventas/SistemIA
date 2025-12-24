using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class CrearCompras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Compras",
                columns: table => new
                {
                    IdCompra = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Establecimiento = table.Column<string>(type: "nchar(3)", fixedLength: true, maxLength: 3, nullable: true),
                    PuntoExpedicion = table.Column<string>(type: "nchar(3)", fixedLength: true, maxLength: 3, nullable: true),
                    NumeroFactura = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    Timbrado = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    suc = table.Column<int>(type: "int", nullable: false),
                    IdProveedor = table.Column<int>(type: "int", nullable: false),
                    IdUsuario = table.Column<int>(type: "int", nullable: true),
                    IdMoneda = table.Column<int>(type: "int", nullable: true),
                    IdDeposito = table.Column<int>(type: "int", nullable: true),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Total = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    FormaPago = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PlazoDias = table.Column<int>(type: "int", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TipoDocumento = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    TipoIngreso = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    CodigoDocumento = table.Column<int>(type: "int", nullable: true),
                    TotalEnLetras = table.Column<string>(type: "nvarchar(280)", maxLength: 280, nullable: true),
                    Turno = table.Column<int>(type: "int", nullable: true),
                    IdCaja = table.Column<int>(type: "int", nullable: true),
                    ImputarIVA = table.Column<bool>(type: "bit", nullable: true),
                    ImputarIRP = table.Column<bool>(type: "bit", nullable: true),
                    ImputarIRE = table.Column<bool>(type: "bit", nullable: true),
                    NoImputar = table.Column<bool>(type: "bit", nullable: true),
                    Comentario = table.Column<string>(type: "nvarchar(280)", maxLength: 280, nullable: true),
                    EsFacturaElectronica = table.Column<bool>(type: "bit", nullable: true),
                    TipoRegistro = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CodigoRegistro = table.Column<int>(type: "int", nullable: true),
                    CodigoCondicion = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    EsMonedaExtranjera = table.Column<bool>(type: "bit", nullable: true),
                    CreditoSaldo = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    CambioDelDia = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    SimboloMoneda = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    IdCajaChica = table.Column<int>(type: "int", nullable: true),
                    Vendedor = table.Column<string>(type: "nchar(20)", fixedLength: true, maxLength: 20, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Compras", x => x.IdCompra);
                    table.ForeignKey(
                        name: "FK_Compras_Depositos_IdDeposito",
                        column: x => x.IdDeposito,
                        principalTable: "Depositos",
                        principalColumn: "IdDeposito",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Compras_Monedas_IdMoneda",
                        column: x => x.IdMoneda,
                        principalTable: "Monedas",
                        principalColumn: "IdMoneda",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Compras_Proveedores_IdProveedor",
                        column: x => x.IdProveedor,
                        principalTable: "Proveedores",
                        principalColumn: "Id_Proveedor",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Compras_Sucursal_suc",
                        column: x => x.suc,
                        principalTable: "Sucursal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Compras_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id_Usu",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComprasDetalles",
                columns: table => new
                {
                    IdCompraDetalle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCompra = table.Column<int>(type: "int", nullable: false),
                    IdProducto = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Importe = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IVA10 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IVA5 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Exenta = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Grabado10 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Grabado5 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComprasDetalles", x => x.IdCompraDetalle);
                    table.ForeignKey(
                        name: "FK_ComprasDetalles_Compras_IdCompra",
                        column: x => x.IdCompra,
                        principalTable: "Compras",
                        principalColumn: "IdCompra",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComprasDetalles_Productos_IdProducto",
                        column: x => x.IdProducto,
                        principalTable: "Productos",
                        principalColumn: "IdProducto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Compras_IdDeposito",
                table: "Compras",
                column: "IdDeposito");

            migrationBuilder.CreateIndex(
                name: "IX_Compras_IdMoneda",
                table: "Compras",
                column: "IdMoneda");

            migrationBuilder.CreateIndex(
                name: "IX_Compras_IdProveedor",
                table: "Compras",
                column: "IdProveedor");

            migrationBuilder.CreateIndex(
                name: "IX_Compras_IdUsuario",
                table: "Compras",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Compras_suc",
                table: "Compras",
                column: "suc");

            migrationBuilder.CreateIndex(
                name: "IX_ComprasDetalles_IdCompra",
                table: "ComprasDetalles",
                column: "IdCompra");

            migrationBuilder.CreateIndex(
                name: "IX_ComprasDetalles_IdProducto",
                table: "ComprasDetalles",
                column: "IdProducto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComprasDetalles");

            migrationBuilder.DropTable(
                name: "Compras");
        }
    }
}
