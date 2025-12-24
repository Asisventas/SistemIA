using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Distrito_AddIdCiudad_FK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdCiudad",
                table: "Distritos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Distritos_IdCiudad",
                table: "Distritos",
                column: "IdCiudad");

            migrationBuilder.AddForeignKey(
                name: "FK_Distritos_Ciudades_IdCiudad",
                table: "Distritos",
                column: "IdCiudad",
                principalTable: "Ciudades",
                principalColumn: "IdCiudad",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Distritos_Ciudades_IdCiudad",
                table: "Distritos");

            migrationBuilder.DropIndex(
                name: "IX_Distritos_IdCiudad",
                table: "Distritos");

            migrationBuilder.DropColumn(
                name: "IdCiudad",
                table: "Distritos");
        }
    }
}
