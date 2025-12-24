using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Sociedad_AfterKill : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sociedades",
                columns: table => new
                {
                    IdSociedad = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    RUC = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    DV = table.Column<int>(type: "int", nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    NumeroCasa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Departamento = table.Column<int>(type: "int", nullable: true),
                    Ciudad = table.Column<int>(type: "int", nullable: true),
                    Distrito = table.Column<int>(type: "int", nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TipoContribuyente = table.Column<int>(type: "int", nullable: true),
                    IdCsc = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Csc = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PathCertificadoP12 = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    PasswordCertificadoP12 = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    PathCertificadoPem = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    PathCertificadoCrt = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    PathArchivoSinFirma = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    PathArchivoFirmado = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    DeUrlQr = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    DeUrlEnvioDocumento = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    DeUrlEnvioEvento = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    DeUrlEnvioDocumentoLote = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    DeUrlConsultaDocumento = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    DeUrlConsultaDocumentoLote = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    DeUrlConsultaRuc = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    ServidorSifen = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeConexion = table.Column<int>(type: "int", nullable: true),
                    FechaAuditoria = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Usuario = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sociedades", x => x.IdSociedad);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sociedades");
        }
    }
}
