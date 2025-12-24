using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Producto_DepositoPredeterminado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductosDepositos_Productos_ProductoIdProducto",
                table: "ProductosDepositos");

            migrationBuilder.DropIndex(
                name: "IX_ProductosDepositos_ProductoIdProducto",
                table: "ProductosDepositos");

            migrationBuilder.DropColumn(
                name: "ProductoIdProducto",
                table: "ProductosDepositos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductoIdProducto",
                table: "ProductosDepositos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductosDepositos_ProductoIdProducto",
                table: "ProductosDepositos",
                column: "ProductoIdProducto");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductosDepositos_Productos_ProductoIdProducto",
                table: "ProductosDepositos",
                column: "ProductoIdProducto",
                principalTable: "Productos",
                principalColumn: "IdProducto");
        }
    }
}
