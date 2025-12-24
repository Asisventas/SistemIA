using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Formato_Impresion_Cajas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AnchoTicket",
                table: "Cajas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormatoImpresion",
                table: "Cajas",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MostrarLogo",
                table: "Cajas",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnchoTicket",
                table: "Cajas");

            migrationBuilder.DropColumn(
                name: "FormatoImpresion",
                table: "Cajas");

            migrationBuilder.DropColumn(
                name: "MostrarLogo",
                table: "Cajas");
        }
    }
}
