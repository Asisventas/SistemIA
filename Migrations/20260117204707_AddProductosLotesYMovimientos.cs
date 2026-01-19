using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class AddProductosLotesYMovimientos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ControlaLote",
                table: "Productos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ControlaVencimiento",
                table: "Productos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "DiasAlertaVencimiento",
                table: "Productos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "LoteInicialCreado",
                table: "Productos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PermiteVentaVencido",
                table: "Productos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ProductosLotes",
                columns: table => new
                {
                    IdProductoLote = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProducto = table.Column<int>(type: "int", nullable: false),
                    IdDeposito = table.Column<int>(type: "int", nullable: false),
                    NumeroLote = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaFabricacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Stock = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    StockInicial = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CostoUnitario = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    IdCompra = table.Column<int>(type: "int", nullable: true),
                    IdCompraDetalle = table.Column<int>(type: "int", nullable: true),
                    FechaIngreso = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Activo"),
                    Observacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EsLoteInicial = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductosLotes", x => x.IdProductoLote);
                    table.ForeignKey(
                        name: "FK_ProductosLotes_Compras_IdCompra",
                        column: x => x.IdCompra,
                        principalTable: "Compras",
                        principalColumn: "IdCompra",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProductosLotes_Depositos_IdDeposito",
                        column: x => x.IdDeposito,
                        principalTable: "Depositos",
                        principalColumn: "IdDeposito",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductosLotes_Productos_IdProducto",
                        column: x => x.IdProducto,
                        principalTable: "Productos",
                        principalColumn: "IdProducto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosLotes",
                columns: table => new
                {
                    IdMovimientoLote = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProductoLote = table.Column<int>(type: "int", nullable: false),
                    TipoMovimiento = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    StockAnterior = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    StockPosterior = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TipoDocumento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IdDocumento = table.Column<int>(type: "int", nullable: true),
                    IdDocumentoDetalle = table.Column<int>(type: "int", nullable: true),
                    ReferenciaDocumento = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Motivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaMovimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IdProductoLoteDestino = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosLotes", x => x.IdMovimientoLote);
                    table.ForeignKey(
                        name: "FK_MovimientosLotes_ProductosLotes_IdProductoLote",
                        column: x => x.IdProductoLote,
                        principalTable: "ProductosLotes",
                        principalColumn: "IdProductoLote",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovimientosLotes_ProductosLotes_IdProductoLoteDestino",
                        column: x => x.IdProductoLoteDestino,
                        principalTable: "ProductosLotes",
                        principalColumn: "IdProductoLote");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosLotes_FechaMovimiento",
                table: "MovimientosLotes",
                column: "FechaMovimiento");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosLotes_IdProductoLote",
                table: "MovimientosLotes",
                column: "IdProductoLote");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosLotes_IdProductoLoteDestino",
                table: "MovimientosLotes",
                column: "IdProductoLoteDestino");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosLotes_TipoDocumento_IdDocumento",
                table: "MovimientosLotes",
                columns: new[] { "TipoDocumento", "IdDocumento" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductosLotes_FechaVencimiento",
                table: "ProductosLotes",
                column: "FechaVencimiento");

            migrationBuilder.CreateIndex(
                name: "IX_ProductosLotes_IdCompra",
                table: "ProductosLotes",
                column: "IdCompra");

            migrationBuilder.CreateIndex(
                name: "IX_ProductosLotes_IdDeposito",
                table: "ProductosLotes",
                column: "IdDeposito");

            migrationBuilder.CreateIndex(
                name: "IX_ProductosLotes_IdProducto_IdDeposito_NumeroLote",
                table: "ProductosLotes",
                columns: new[] { "IdProducto", "IdDeposito", "NumeroLote" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovimientosLotes");

            migrationBuilder.DropTable(
                name: "ProductosLotes");

            migrationBuilder.DropColumn(
                name: "ControlaLote",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "ControlaVencimiento",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "DiasAlertaVencimiento",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "LoteInicialCreado",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "PermiteVentaVencido",
                table: "Productos");
        }
    }
}
