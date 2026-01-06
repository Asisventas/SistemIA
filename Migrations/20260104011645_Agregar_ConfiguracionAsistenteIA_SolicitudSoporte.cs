using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_ConfiguracionAsistenteIA_SolicitudSoporte : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfiguracionesAsistenteIA",
                columns: table => new
                {
                    IdConfiguracion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CorreoSoporte = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    NombreSoporte = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HabilitarEnvioSoporte = table.Column<bool>(type: "bit", nullable: false),
                    MensajeBienvenida = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MensajeSinRespuesta = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    HabilitarVozEntrada = table.Column<bool>(type: "bit", nullable: false),
                    HabilitarVozSalida = table.Column<bool>(type: "bit", nullable: false),
                    HabilitarCapturaPantalla = table.Column<bool>(type: "bit", nullable: false),
                    HabilitarGrabacionVideo = table.Column<bool>(type: "bit", nullable: false),
                    MaxSegundosVideo = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionesAsistenteIA", x => x.IdConfiguracion);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesSoporteIA",
                columns: table => new
                {
                    IdSolicitud = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdUsuario = table.Column<int>(type: "int", nullable: true),
                    NombreUsuario = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaginaOrigen = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PreguntaOriginal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DescripcionProblema = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScreenshotBase64 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VideoBase64 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InfoNavegador = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CorreoEnviado = table.Column<bool>(type: "bit", nullable: false),
                    ErrorEnvio = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NotasSoporte = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaResolucion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesSoporteIA", x => x.IdSolicitud);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracionesAsistenteIA");

            migrationBuilder.DropTable(
                name: "SolicitudesSoporteIA");
        }
    }
}
