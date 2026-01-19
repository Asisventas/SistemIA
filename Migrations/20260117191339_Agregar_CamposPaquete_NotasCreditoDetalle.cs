using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_CamposPaquete_NotasCreditoDetalle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CantidadPorPaqueteMomento",
                table: "NotasCreditoVentasDetalles",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModoIngresoPersistido",
                table: "NotasCreditoVentasDetalles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CantidadPorPaqueteMomento",
                table: "NotasCreditoComprasDetalles",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModoIngresoPersistido",
                table: "NotasCreditoComprasDetalles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CantidadPorPaqueteMomento",
                table: "NotasCreditoVentasDetalles");

            migrationBuilder.DropColumn(
                name: "ModoIngresoPersistido",
                table: "NotasCreditoVentasDetalles");

            migrationBuilder.DropColumn(
                name: "CantidadPorPaqueteMomento",
                table: "NotasCreditoComprasDetalles");

            migrationBuilder.DropColumn(
                name: "ModoIngresoPersistido",
                table: "NotasCreditoComprasDetalles");
        }
    }
}
