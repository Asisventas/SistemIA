using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_ConfiguracionCorreo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfiguracionCorreo",
                columns: table => new
                {
                    IdConfiguracionCorreo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    TipoProveedor = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ServidorSmtp = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Puerto = table.Column<int>(type: "int", nullable: false),
                    TipoSeguridad = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UsuarioSmtp = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContrasenaSmtp = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UsarOAuth2 = table.Column<bool>(type: "bit", nullable: false),
                    OAuth2ClientId = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    OAuth2ClientSecret = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OAuth2RefreshToken = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    OAuth2AccessToken = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    OAuth2TokenExpira = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OAuth2Scopes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CorreoRemitente = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NombreRemitente = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CorreoReplyTo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CorreoBccAuditoria = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TimeoutSegundos = table.Column<int>(type: "int", nullable: false),
                    MaxReintentos = table.Column<int>(type: "int", nullable: false),
                    HabilitarLogDetallado = table.Column<bool>(type: "bit", nullable: false),
                    IgnorarErroresCertificado = table.Column<bool>(type: "bit", nullable: false),
                    ApiKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DominioApi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FirmaHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UsarHtmlPorDefecto = table.Column<bool>(type: "bit", nullable: false),
                    IdSociedad = table.Column<int>(type: "int", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UltimaPruebaResultado = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UltimaPruebaFecha = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionCorreo", x => x.IdConfiguracionCorreo);
                    table.ForeignKey(
                        name: "FK_ConfiguracionCorreo_Sociedades_IdSociedad",
                        column: x => x.IdSociedad,
                        principalTable: "Sociedades",
                        principalColumn: "IdSociedad");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionCorreo_IdSociedad",
                table: "ConfiguracionCorreo",
                column: "IdSociedad");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracionCorreo");
        }
    }
}
