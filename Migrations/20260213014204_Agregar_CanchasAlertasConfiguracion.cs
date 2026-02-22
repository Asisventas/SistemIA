using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_CanchasAlertasConfiguracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CanchasMinutosAlerta",
                table: "ConfiguracionSistema",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CanchasRepetirSonidoSegundos",
                table: "ConfiguracionSistema",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CanchasSonidoAlerta",
                table: "ConfiguracionSistema",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CanchasSonidoFin",
                table: "ConfiguracionSistema",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanchasMinutosAlerta",
                table: "ConfiguracionSistema");

            migrationBuilder.DropColumn(
                name: "CanchasRepetirSonidoSegundos",
                table: "ConfiguracionSistema");

            migrationBuilder.DropColumn(
                name: "CanchasSonidoAlerta",
                table: "ConfiguracionSistema");

            migrationBuilder.DropColumn(
                name: "CanchasSonidoFin",
                table: "ConfiguracionSistema");
        }
    }
}
