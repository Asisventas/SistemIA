using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_DestinatariosInforme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DestinatariosInforme",
                columns: table => new
                {
                    IdDestinatarioInforme = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    RecibeVentasDiarias = table.Column<bool>(type: "bit", nullable: false),
                    RecibeResumenSemanal = table.Column<bool>(type: "bit", nullable: false),
                    RecibeResumenMensual = table.Column<bool>(type: "bit", nullable: false),
                    RecibeAlertaStock = table.Column<bool>(type: "bit", nullable: false),
                    RecibeCierreCaja = table.Column<bool>(type: "bit", nullable: false),
                    RecibeInformeCompras = table.Column<bool>(type: "bit", nullable: false),
                    RecibeResumenSifen = table.Column<bool>(type: "bit", nullable: false),
                    RecibeAlertaVencimientos = table.Column<bool>(type: "bit", nullable: false),
                    RecibeCopiaFacturas = table.Column<bool>(type: "bit", nullable: false),
                    TipoCopia = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IdSucursal = table.Column<int>(type: "int", nullable: true),
                    IdConfiguracionCorreo = table.Column<int>(type: "int", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DestinatariosInforme", x => x.IdDestinatarioInforme);
                    table.ForeignKey(
                        name: "FK_DestinatariosInforme_ConfiguracionCorreo_IdConfiguracionCorreo",
                        column: x => x.IdConfiguracionCorreo,
                        principalTable: "ConfiguracionCorreo",
                        principalColumn: "IdConfiguracionCorreo");
                    table.ForeignKey(
                        name: "FK_DestinatariosInforme_Sucursal_IdSucursal",
                        column: x => x.IdSucursal,
                        principalTable: "Sucursal",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DestinatariosInforme_IdConfiguracionCorreo",
                table: "DestinatariosInforme",
                column: "IdConfiguracionCorreo");

            migrationBuilder.CreateIndex(
                name: "IX_DestinatariosInforme_IdSucursal",
                table: "DestinatariosInforme",
                column: "IdSucursal");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DestinatariosInforme");
        }
    }
}
