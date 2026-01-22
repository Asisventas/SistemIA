using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class AddSalidasStock_Tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalidasStock",
                columns: table => new
                {
                    IdSalidaStock = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdDeposito = table.Column<int>(type: "int", nullable: false),
                    FechaSalida = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MotivoSalida = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    NumeroDocumento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaConfirmacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioConfirmacion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaAnulacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioAnulacion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MotivoAnulacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalidasStock", x => x.IdSalidaStock);
                    table.ForeignKey(
                        name: "FK_SalidasStock_Depositos_IdDeposito",
                        column: x => x.IdDeposito,
                        principalTable: "Depositos",
                        principalColumn: "IdDeposito",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalidasStockDetalle",
                columns: table => new
                {
                    IdSalidaStockDetalle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdSalidaStock = table.Column<int>(type: "int", nullable: false),
                    IdProducto = table.Column<int>(type: "int", nullable: false),
                    IdProductoLote = table.Column<int>(type: "int", nullable: true),
                    NumeroLote = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaVencimientoLote = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CostoUnitario = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Observacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalidasStockDetalle", x => x.IdSalidaStockDetalle);
                    table.ForeignKey(
                        name: "FK_SalidasStockDetalle_ProductosLotes_IdProductoLote",
                        column: x => x.IdProductoLote,
                        principalTable: "ProductosLotes",
                        principalColumn: "IdProductoLote");
                    table.ForeignKey(
                        name: "FK_SalidasStockDetalle_Productos_IdProducto",
                        column: x => x.IdProducto,
                        principalTable: "Productos",
                        principalColumn: "IdProducto",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalidasStockDetalle_SalidasStock_IdSalidaStock",
                        column: x => x.IdSalidaStock,
                        principalTable: "SalidasStock",
                        principalColumn: "IdSalidaStock",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalidasStock_IdDeposito",
                table: "SalidasStock",
                column: "IdDeposito");

            migrationBuilder.CreateIndex(
                name: "IX_SalidasStockDetalle_IdProducto",
                table: "SalidasStockDetalle",
                column: "IdProducto");

            migrationBuilder.CreateIndex(
                name: "IX_SalidasStockDetalle_IdProductoLote",
                table: "SalidasStockDetalle",
                column: "IdProductoLote");

            migrationBuilder.CreateIndex(
                name: "IX_SalidasStockDetalle_IdSalidaStock",
                table: "SalidasStockDetalle",
                column: "IdSalidaStock");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalidasStockDetalle");

            migrationBuilder.DropTable(
                name: "SalidasStock");
        }
    }
}
