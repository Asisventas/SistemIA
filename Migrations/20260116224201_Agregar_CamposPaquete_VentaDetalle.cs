using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_CamposPaquete_VentaDetalle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CantidadPorPaqueteMomento",
                table: "VentasDetalles",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModoIngresoPersistido",
                table: "VentasDetalles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioMinisterioPaqueteMomento",
                table: "VentasDetalles",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioPaqueteMomento",
                table: "VentasDetalles",
                type: "decimal(18,4)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CantidadPorPaqueteMomento",
                table: "VentasDetalles");

            migrationBuilder.DropColumn(
                name: "ModoIngresoPersistido",
                table: "VentasDetalles");

            migrationBuilder.DropColumn(
                name: "PrecioMinisterioPaqueteMomento",
                table: "VentasDetalles");

            migrationBuilder.DropColumn(
                name: "PrecioPaqueteMomento",
                table: "VentasDetalles");
        }
    }
}
