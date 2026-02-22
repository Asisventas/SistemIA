using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_IdProducto_Gimnasio_Bidireccional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdProducto",
                table: "PlanesMembresia",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdProducto",
                table: "MembresiasClientes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanesMembresia_IdProducto",
                table: "PlanesMembresia",
                column: "IdProducto");

            migrationBuilder.CreateIndex(
                name: "IX_MembresiasClientes_IdProducto",
                table: "MembresiasClientes",
                column: "IdProducto");

            migrationBuilder.AddForeignKey(
                name: "FK_MembresiasClientes_Productos_IdProducto",
                table: "MembresiasClientes",
                column: "IdProducto",
                principalTable: "Productos",
                principalColumn: "IdProducto");

            migrationBuilder.AddForeignKey(
                name: "FK_PlanesMembresia_Productos_IdProducto",
                table: "PlanesMembresia",
                column: "IdProducto",
                principalTable: "Productos",
                principalColumn: "IdProducto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MembresiasClientes_Productos_IdProducto",
                table: "MembresiasClientes");

            migrationBuilder.DropForeignKey(
                name: "FK_PlanesMembresia_Productos_IdProducto",
                table: "PlanesMembresia");

            migrationBuilder.DropIndex(
                name: "IX_PlanesMembresia_IdProducto",
                table: "PlanesMembresia");

            migrationBuilder.DropIndex(
                name: "IX_MembresiasClientes_IdProducto",
                table: "MembresiasClientes");

            migrationBuilder.DropColumn(
                name: "IdProducto",
                table: "PlanesMembresia");

            migrationBuilder.DropColumn(
                name: "IdProducto",
                table: "MembresiasClientes");
        }
    }
}
