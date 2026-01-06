using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class ConfigCorreo_Simplificacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RecibeTodosLosInformes",
                table: "DestinatariosInforme",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "CorreoRemitente",
                table: "ConfiguracionCorreo",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<bool>(
                name: "UsarDatosEmpresaComoRemitente",
                table: "ConfiguracionCorreo",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecibeTodosLosInformes",
                table: "DestinatariosInforme");

            migrationBuilder.DropColumn(
                name: "UsarDatosEmpresaComoRemitente",
                table: "ConfiguracionCorreo");

            migrationBuilder.AlterColumn<string>(
                name: "CorreoRemitente",
                table: "ConfiguracionCorreo",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);
        }
    }
}
