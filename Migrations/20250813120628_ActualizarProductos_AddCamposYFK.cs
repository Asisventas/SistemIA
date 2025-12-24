using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarProductos_AddCamposYFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Foto",
                table: "Productos",
                type: "varchar(180)",
                maxLength: 180,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IP",
                table: "Productos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UndMedida",
                table: "Productos",
                type: "char(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "suc",
                table: "Productos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_suc",
                table: "Productos",
                column: "suc");

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_Sucursal_suc",
                table: "Productos",
                column: "suc",
                principalTable: "Sucursal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Productos_Sucursal_suc",
                table: "Productos");

            migrationBuilder.DropIndex(
                name: "IX_Productos_suc",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Foto",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "IP",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "UndMedida",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "suc",
                table: "Productos");
        }
    }
}
