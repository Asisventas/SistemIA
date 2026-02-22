using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Campos_Comprobante_Pedido_Caja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ComprobanteEsRed",
                table: "Cajas",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImpresoraComprobantePedido",
                table: "Cajas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ImprimirComprobantePedidoAuto",
                table: "Cajas",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PuertoImpresoraComprobante",
                table: "Cajas",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComprobanteEsRed",
                table: "Cajas");

            migrationBuilder.DropColumn(
                name: "ImpresoraComprobantePedido",
                table: "Cajas");

            migrationBuilder.DropColumn(
                name: "ImprimirComprobantePedidoAuto",
                table: "Cajas");

            migrationBuilder.DropColumn(
                name: "PuertoImpresoraComprobante",
                table: "Cajas");
        }
    }
}
