using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_EnvioAutomatico_Correo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RecibeResumenCierre",
                table: "DestinatariosInforme",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "DiaEnvioMensual",
                table: "ConfiguracionCorreo",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiaEnvioSemanal",
                table: "ConfiguracionCorreo",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnviarAlCierreSistema",
                table: "ConfiguracionCorreo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "HoraEnvioDiario",
                table: "ConfiguracionCorreo",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecibeResumenCierre",
                table: "DestinatariosInforme");

            migrationBuilder.DropColumn(
                name: "DiaEnvioMensual",
                table: "ConfiguracionCorreo");

            migrationBuilder.DropColumn(
                name: "DiaEnvioSemanal",
                table: "ConfiguracionCorreo");

            migrationBuilder.DropColumn(
                name: "EnviarAlCierreSistema",
                table: "ConfiguracionCorreo");

            migrationBuilder.DropColumn(
                name: "HoraEnvioDiario",
                table: "ConfiguracionCorreo");
        }
    }
}
