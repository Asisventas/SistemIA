using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Campos_Membresia_Producto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClasesIncluidasMembresia",
                table: "Productos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ColorMembresia",
                table: "Productos",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiasAccesoMembresia",
                table: "Productos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiasGraciaMembresia",
                table: "Productos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiasMaxCongelarMembresia",
                table: "Productos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DuracionDiasMembresia",
                table: "Productos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EsMembresia",
                table: "Productos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "HorarioAccesoMembresia",
                table: "Productos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IncluyePTMembresia",
                table: "Productos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PermiteCongelarMembresia",
                table: "Productos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioInscripcionMembresia",
                table: "Productos",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SesionesPTMembresia",
                table: "Productos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoPeriodoMembresia",
                table: "Productos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClasesIncluidasMembresia",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "ColorMembresia",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "DiasAccesoMembresia",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "DiasGraciaMembresia",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "DiasMaxCongelarMembresia",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "DuracionDiasMembresia",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "EsMembresia",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "HorarioAccesoMembresia",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "IncluyePTMembresia",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "PermiteCongelarMembresia",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "PrecioInscripcionMembresia",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "SesionesPTMembresia",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "TipoPeriodoMembresia",
                table: "Productos");
        }
    }
}
