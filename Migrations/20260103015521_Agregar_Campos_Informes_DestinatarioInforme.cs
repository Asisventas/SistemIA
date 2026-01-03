using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Campos_Informes_DestinatarioInforme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RecibeAjustesStock",
                table: "DestinatariosInforme",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RecibeAsistencia",
                table: "DestinatariosInforme",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RecibeComprasDetallado",
                table: "DestinatariosInforme",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RecibeCuentasPorCobrar",
                table: "DestinatariosInforme",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RecibeCuentasPorPagar",
                table: "DestinatariosInforme",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RecibeMovimientosStock",
                table: "DestinatariosInforme",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RecibeNCCompras",
                table: "DestinatariosInforme",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RecibeNCDetallado",
                table: "DestinatariosInforme",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RecibeNotasCredito",
                table: "DestinatariosInforme",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RecibeProductosDetallado",
                table: "DestinatariosInforme",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RecibeProductosValorizado",
                table: "DestinatariosInforme",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RecibeResumenCaja",
                table: "DestinatariosInforme",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RecibeVentasAgrupado",
                table: "DestinatariosInforme",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RecibeVentasClasificacion",
                table: "DestinatariosInforme",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RecibeVentasDetallado",
                table: "DestinatariosInforme",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecibeAjustesStock",
                table: "DestinatariosInforme");

            migrationBuilder.DropColumn(
                name: "RecibeAsistencia",
                table: "DestinatariosInforme");

            migrationBuilder.DropColumn(
                name: "RecibeComprasDetallado",
                table: "DestinatariosInforme");

            migrationBuilder.DropColumn(
                name: "RecibeCuentasPorCobrar",
                table: "DestinatariosInforme");

            migrationBuilder.DropColumn(
                name: "RecibeCuentasPorPagar",
                table: "DestinatariosInforme");

            migrationBuilder.DropColumn(
                name: "RecibeMovimientosStock",
                table: "DestinatariosInforme");

            migrationBuilder.DropColumn(
                name: "RecibeNCCompras",
                table: "DestinatariosInforme");

            migrationBuilder.DropColumn(
                name: "RecibeNCDetallado",
                table: "DestinatariosInforme");

            migrationBuilder.DropColumn(
                name: "RecibeNotasCredito",
                table: "DestinatariosInforme");

            migrationBuilder.DropColumn(
                name: "RecibeProductosDetallado",
                table: "DestinatariosInforme");

            migrationBuilder.DropColumn(
                name: "RecibeProductosValorizado",
                table: "DestinatariosInforme");

            migrationBuilder.DropColumn(
                name: "RecibeResumenCaja",
                table: "DestinatariosInforme");

            migrationBuilder.DropColumn(
                name: "RecibeVentasAgrupado",
                table: "DestinatariosInforme");

            migrationBuilder.DropColumn(
                name: "RecibeVentasClasificacion",
                table: "DestinatariosInforme");

            migrationBuilder.DropColumn(
                name: "RecibeVentasDetallado",
                table: "DestinatariosInforme");
        }
    }
}
