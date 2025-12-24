using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Turno_Cobros_Pagos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Turno",
                table: "PagosProveedores",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Turno",
                table: "CobrosCuotas",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Turno",
                table: "PagosProveedores");

            migrationBuilder.DropColumn(
                name: "Turno",
                table: "CobrosCuotas");
        }
    }
}
