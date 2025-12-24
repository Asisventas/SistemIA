using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Producto_AgregarDepositoPredeterminado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdDepositoPredeterminado",
                table: "Productos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_IdDepositoPredeterminado",
                table: "Productos",
                column: "IdDepositoPredeterminado");

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_Depositos_IdDepositoPredeterminado",
                table: "Productos",
                column: "IdDepositoPredeterminado",
                principalTable: "Depositos",
                principalColumn: "IdDeposito",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Productos_Depositos_IdDepositoPredeterminado",
                table: "Productos");

            migrationBuilder.DropIndex(
                name: "IX_Productos_IdDepositoPredeterminado",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "IdDepositoPredeterminado",
                table: "Productos");
        }
    }
}
