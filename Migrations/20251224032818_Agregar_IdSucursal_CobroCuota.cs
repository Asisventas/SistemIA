using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_IdSucursal_CobroCuota : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdSucursal",
                table: "CobrosCuotas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CobrosCuotas_IdSucursal",
                table: "CobrosCuotas",
                column: "IdSucursal");

            migrationBuilder.AddForeignKey(
                name: "FK_CobrosCuotas_Sucursal_IdSucursal",
                table: "CobrosCuotas",
                column: "IdSucursal",
                principalTable: "Sucursal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CobrosCuotas_Sucursal_IdSucursal",
                table: "CobrosCuotas");

            migrationBuilder.DropIndex(
                name: "IX_CobrosCuotas_IdSucursal",
                table: "CobrosCuotas");

            migrationBuilder.DropColumn(
                name: "IdSucursal",
                table: "CobrosCuotas");
        }
    }
}
