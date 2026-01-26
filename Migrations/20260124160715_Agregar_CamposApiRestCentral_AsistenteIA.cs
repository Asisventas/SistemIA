using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_CamposApiRestCentral_AsistenteIA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApiKeyCentral",
                table: "ConfiguracionesAsistenteIA",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApiSecretCentral",
                table: "ConfiguracionesAsistenteIA",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModoConexionCentral",
                table: "ConfiguracionesAsistenteIA",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UrlApiCentral",
                table: "ConfiguracionesAsistenteIA",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiKeyCentral",
                table: "ConfiguracionesAsistenteIA");

            migrationBuilder.DropColumn(
                name: "ApiSecretCentral",
                table: "ConfiguracionesAsistenteIA");

            migrationBuilder.DropColumn(
                name: "ModoConexionCentral",
                table: "ConfiguracionesAsistenteIA");

            migrationBuilder.DropColumn(
                name: "UrlApiCentral",
                table: "ConfiguracionesAsistenteIA");
        }
    }
}
