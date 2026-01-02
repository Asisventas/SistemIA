using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_MargenAdicionalCajero_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MargenAdicionalCajeroProducto",
                table: "Productos",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MargenAdicionalCajero",
                table: "DescuentosCategorias",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MargenAdicionalCajeroProducto",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "MargenAdicionalCajero",
                table: "DescuentosCategorias");
        }
    }
}
