using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Configuracion_SifenCola : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SifenColaActiva",
                table: "ConfiguracionSistema",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SifenIntervaloMinutos",
                table: "ConfiguracionSistema",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SifenMaxDocumentosPorCiclo",
                table: "ConfiguracionSistema",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SifenMaxReintentos",
                table: "ConfiguracionSistema",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SifenColaActiva",
                table: "ConfiguracionSistema");

            migrationBuilder.DropColumn(
                name: "SifenIntervaloMinutos",
                table: "ConfiguracionSistema");

            migrationBuilder.DropColumn(
                name: "SifenMaxDocumentosPorCiclo",
                table: "ConfiguracionSistema");

            migrationBuilder.DropColumn(
                name: "SifenMaxReintentos",
                table: "ConfiguracionSistema");
        }
    }
}
