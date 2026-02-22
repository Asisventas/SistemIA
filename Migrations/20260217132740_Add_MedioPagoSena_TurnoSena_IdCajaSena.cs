using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Add_MedioPagoSena_TurnoSena_IdCajaSena : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CajaSenaIdCaja",
                table: "Reservas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdCajaSena",
                table: "Reservas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MedioPagoSena",
                table: "Reservas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TurnoSena",
                table: "Reservas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_CajaSenaIdCaja",
                table: "Reservas",
                column: "CajaSenaIdCaja");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservas_Cajas_CajaSenaIdCaja",
                table: "Reservas",
                column: "CajaSenaIdCaja",
                principalTable: "Cajas",
                principalColumn: "id_caja");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservas_Cajas_CajaSenaIdCaja",
                table: "Reservas");

            migrationBuilder.DropIndex(
                name: "IX_Reservas_CajaSenaIdCaja",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "CajaSenaIdCaja",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "IdCajaSena",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "MedioPagoSena",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "TurnoSena",
                table: "Reservas");
        }
    }
}
