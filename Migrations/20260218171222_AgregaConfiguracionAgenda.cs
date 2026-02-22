using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class AgregaConfiguracionAgenda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AgendaColoresPorCliente",
                table: "ConfiguracionSistema",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AgendaEnviarRecordatoriosEmail",
                table: "ConfiguracionSistema",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AgendaModoActivo",
                table: "ConfiguracionSistema",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AgendaMostrarUbicacionGPS",
                table: "ConfiguracionSistema",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "AgendaRecordatorioMinutos",
                table: "ConfiguracionSistema",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgendaColoresPorCliente",
                table: "ConfiguracionSistema");

            migrationBuilder.DropColumn(
                name: "AgendaEnviarRecordatoriosEmail",
                table: "ConfiguracionSistema");

            migrationBuilder.DropColumn(
                name: "AgendaModoActivo",
                table: "ConfiguracionSistema");

            migrationBuilder.DropColumn(
                name: "AgendaMostrarUbicacionGPS",
                table: "ConfiguracionSistema");

            migrationBuilder.DropColumn(
                name: "AgendaRecordatorioMinutos",
                table: "ConfiguracionSistema");
        }
    }
}
