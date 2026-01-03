using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_IdSucursal_ConfiguracionCorreo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdSucursal",
                table: "ConfiguracionCorreo",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionCorreo_IdSucursal",
                table: "ConfiguracionCorreo",
                column: "IdSucursal");

            migrationBuilder.AddForeignKey(
                name: "FK_ConfiguracionCorreo_Sucursal_IdSucursal",
                table: "ConfiguracionCorreo",
                column: "IdSucursal",
                principalTable: "Sucursal",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConfiguracionCorreo_Sucursal_IdSucursal",
                table: "ConfiguracionCorreo");

            migrationBuilder.DropIndex(
                name: "IX_ConfiguracionCorreo_IdSucursal",
                table: "ConfiguracionCorreo");

            migrationBuilder.DropColumn(
                name: "IdSucursal",
                table: "ConfiguracionCorreo");
        }
    }
}
