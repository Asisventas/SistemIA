using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_ConfiguracionServidorCentral_AsistenteIA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BaseDatosCentral",
                table: "ConfiguracionesAsistenteIA",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ConexionCentralExitosa",
                table: "ConfiguracionesAsistenteIA",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContrasenaCentral",
                table: "ConfiguracionesAsistenteIA",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HabilitarServidorCentral",
                table: "ConfiguracionesAsistenteIA",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PuertoCentral",
                table: "ConfiguracionesAsistenteIA",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ServidorCentral",
                table: "ConfiguracionesAsistenteIA",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimeoutConexionCentral",
                table: "ConfiguracionesAsistenteIA",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimaVerificacionCentral",
                table: "ConfiguracionesAsistenteIA",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UsarWindowsAuthCentral",
                table: "ConfiguracionesAsistenteIA",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UsuarioCentral",
                table: "ConfiguracionesAsistenteIA",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseDatosCentral",
                table: "ConfiguracionesAsistenteIA");

            migrationBuilder.DropColumn(
                name: "ConexionCentralExitosa",
                table: "ConfiguracionesAsistenteIA");

            migrationBuilder.DropColumn(
                name: "ContrasenaCentral",
                table: "ConfiguracionesAsistenteIA");

            migrationBuilder.DropColumn(
                name: "HabilitarServidorCentral",
                table: "ConfiguracionesAsistenteIA");

            migrationBuilder.DropColumn(
                name: "PuertoCentral",
                table: "ConfiguracionesAsistenteIA");

            migrationBuilder.DropColumn(
                name: "ServidorCentral",
                table: "ConfiguracionesAsistenteIA");

            migrationBuilder.DropColumn(
                name: "TimeoutConexionCentral",
                table: "ConfiguracionesAsistenteIA");

            migrationBuilder.DropColumn(
                name: "UltimaVerificacionCentral",
                table: "ConfiguracionesAsistenteIA");

            migrationBuilder.DropColumn(
                name: "UsarWindowsAuthCentral",
                table: "ConfiguracionesAsistenteIA");

            migrationBuilder.DropColumn(
                name: "UsuarioCentral",
                table: "ConfiguracionesAsistenteIA");
        }
    }
}
