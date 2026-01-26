using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class CloudSync_TablaConfiguracionYHistorial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfiguracionesCloudSync",
                columns: table => new
                {
                    IdConfiguracion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApiKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UrlApi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CarpetaBackup = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExtensionesIncluir = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BackupAutomaticoHabilitado = table.Column<bool>(type: "bit", nullable: false),
                    HoraBackup = table.Column<TimeSpan>(type: "time", nullable: true),
                    DiasBackup = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RetenerUltimosBackups = table.Column<int>(type: "int", nullable: false),
                    ComprimirAntesDeSubir = table.Column<bool>(type: "bit", nullable: false),
                    UltimaSincronizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstadoUltimaSincronizacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MensajeUltimaSincronizacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionesCloudSync", x => x.IdConfiguracion);
                });

            migrationBuilder.CreateTable(
                name: "HistorialBackupsCloud",
                columns: table => new
                {
                    IdHistorial = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreArchivo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    RutaLocal = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RutaRemota = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TamanoBytes = table.Column<long>(type: "bigint", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DuracionSegundos = table.Column<int>(type: "int", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MensajeError = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdArchivoRemoto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HashMD5 = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    FueAutomatico = table.Column<bool>(type: "bit", nullable: false),
                    IdUsuario = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialBackupsCloud", x => x.IdHistorial);
                    table.ForeignKey(
                        name: "FK_HistorialBackupsCloud_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id_Usu");
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistorialBackupsCloud_IdUsuario",
                table: "HistorialBackupsCloud",
                column: "IdUsuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracionesCloudSync");

            migrationBuilder.DropTable(
                name: "HistorialBackupsCloud");
        }
    }
}
