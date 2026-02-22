using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_VentaReferencia_Suscripcion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CantidadCuotas",
                table: "SuscripcionesClientes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CondicionPago",
                table: "SuscripcionesClientes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "IdVentaReferencia",
                table: "SuscripcionesClientes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SuscripcionesClientes_IdVentaReferencia",
                table: "SuscripcionesClientes",
                column: "IdVentaReferencia");

            migrationBuilder.AddForeignKey(
                name: "FK_SuscripcionesClientes_Ventas_IdVentaReferencia",
                table: "SuscripcionesClientes",
                column: "IdVentaReferencia",
                principalTable: "Ventas",
                principalColumn: "IdVenta");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SuscripcionesClientes_Ventas_IdVentaReferencia",
                table: "SuscripcionesClientes");

            migrationBuilder.DropIndex(
                name: "IX_SuscripcionesClientes_IdVentaReferencia",
                table: "SuscripcionesClientes");

            migrationBuilder.DropColumn(
                name: "CantidadCuotas",
                table: "SuscripcionesClientes");

            migrationBuilder.DropColumn(
                name: "CondicionPago",
                table: "SuscripcionesClientes");

            migrationBuilder.DropColumn(
                name: "IdVentaReferencia",
                table: "SuscripcionesClientes");
        }
    }
}
