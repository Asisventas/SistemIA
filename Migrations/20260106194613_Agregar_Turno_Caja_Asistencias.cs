using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Turno_Caja_Asistencias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdCaja",
                table: "Asistencias",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Turno",
                table: "Asistencias",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Asistencias_IdCaja",
                table: "Asistencias",
                column: "IdCaja");

            migrationBuilder.AddForeignKey(
                name: "FK_Asistencias_Cajas_IdCaja",
                table: "Asistencias",
                column: "IdCaja",
                principalTable: "Cajas",
                principalColumn: "id_caja");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Asistencias_Cajas_IdCaja",
                table: "Asistencias");

            migrationBuilder.DropIndex(
                name: "IX_Asistencias_IdCaja",
                table: "Asistencias");

            migrationBuilder.DropColumn(
                name: "IdCaja",
                table: "Asistencias");

            migrationBuilder.DropColumn(
                name: "Turno",
                table: "Asistencias");
        }
    }
}
