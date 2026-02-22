using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_ConfiguracionVPN : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfiguracionesVPN",
                columns: table => new
                {
                    IdConfiguracionVPN = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServidorVPN = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PuertoPPTP = table.Column<int>(type: "int", nullable: false),
                    UsuarioVPN = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ContrasenaVPN = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NombreConexionWindows = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RangoRedVPN = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IpLocalVPN = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ConectarAlIniciar = table.Column<bool>(type: "bit", nullable: false),
                    IntentosReconexion = table.Column<int>(type: "int", nullable: false),
                    SegundosEntreIntentos = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UltimaConexionExitosa = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionesVPN", x => x.IdConfiguracionVPN);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracionesVPN");
        }
    }
}
