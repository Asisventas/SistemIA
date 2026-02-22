using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_CorreosPendientes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CorreosPendientes",
                columns: table => new
                {
                    IdCorreoPendiente = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Destinatario = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Asunto = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CuerpoHtml = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdSucursal = table.Column<int>(type: "int", nullable: false),
                    TipoCorreo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IdReferencia = table.Column<int>(type: "int", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Intentos = table.Column<int>(type: "int", nullable: false),
                    MaxIntentos = table.Column<int>(type: "int", nullable: false),
                    UltimoError = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaUltimoIntento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaEnvio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProximoIntento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdjuntosJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorreosPendientes", x => x.IdCorreoPendiente);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CorreosPendientes");
        }
    }
}
