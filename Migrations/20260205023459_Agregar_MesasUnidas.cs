using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_MesasUnidas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdMesaPrincipal",
                table: "Mesas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mesas_IdMesaPrincipal",
                table: "Mesas",
                column: "IdMesaPrincipal");

            migrationBuilder.AddForeignKey(
                name: "FK_Mesas_Mesas_IdMesaPrincipal",
                table: "Mesas",
                column: "IdMesaPrincipal",
                principalTable: "Mesas",
                principalColumn: "IdMesa");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mesas_Mesas_IdMesaPrincipal",
                table: "Mesas");

            migrationBuilder.DropIndex(
                name: "IX_Mesas_IdMesaPrincipal",
                table: "Mesas");

            migrationBuilder.DropColumn(
                name: "IdMesaPrincipal",
                table: "Mesas");
        }
    }
}
