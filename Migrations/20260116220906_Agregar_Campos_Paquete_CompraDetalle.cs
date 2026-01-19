using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Campos_Paquete_CompraDetalle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CantidadPorPaqueteMomento",
                table: "ComprasDetalles",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioMinisterioPaqueteMomento",
                table: "ComprasDetalles",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioPaqueteMomento",
                table: "ComprasDetalles",
                type: "decimal(18,4)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CantidadPorPaqueteMomento",
                table: "ComprasDetalles");

            migrationBuilder.DropColumn(
                name: "PrecioMinisterioPaqueteMomento",
                table: "ComprasDetalles");

            migrationBuilder.DropColumn(
                name: "PrecioPaqueteMomento",
                table: "ComprasDetalles");
        }
    }
}
