using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_IdProductoLote_AjustesStockDetalles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdProductoLote",
                table: "AjustesStockDetalles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AjustesStockDetalles_IdProductoLote",
                table: "AjustesStockDetalles",
                column: "IdProductoLote");

            migrationBuilder.AddForeignKey(
                name: "FK_AjustesStockDetalles_ProductosLotes_IdProductoLote",
                table: "AjustesStockDetalles",
                column: "IdProductoLote",
                principalTable: "ProductosLotes",
                principalColumn: "IdProductoLote");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AjustesStockDetalles_ProductosLotes_IdProductoLote",
                table: "AjustesStockDetalles");

            migrationBuilder.DropIndex(
                name: "IX_AjustesStockDetalles_IdProductoLote",
                table: "AjustesStockDetalles");

            migrationBuilder.DropColumn(
                name: "IdProductoLote",
                table: "AjustesStockDetalles");
        }
    }
}
