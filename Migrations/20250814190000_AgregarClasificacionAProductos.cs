using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SistemIA.Migrations
{
    public partial class AgregarClasificacionAProductos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Crear tabla Clasificaciones si no existe
            migrationBuilder.CreateTable(
                name: "Clasificaciones",
                columns: table => new
                {
                    IdClasificacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clasificaciones", x => x.IdClasificacion);
                });

            // Agregar columna nullable en Productos
            migrationBuilder.AddColumn<int>(
                name: "IdClasificacion",
                table: "Productos",
                type: "int",
                nullable: true);

            // Crear índice y FK con SET NULL
            migrationBuilder.CreateIndex(
                name: "IX_Productos_IdClasificacion",
                table: "Productos",
                column: "IdClasificacion");

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_Clasificaciones_IdClasificacion",
                table: "Productos",
                column: "IdClasificacion",
                principalTable: "Clasificaciones",
                principalColumn: "IdClasificacion",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Productos_Clasificaciones_IdClasificacion",
                table: "Productos");

            migrationBuilder.DropIndex(
                name: "IX_Productos_IdClasificacion",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "IdClasificacion",
                table: "Productos");

            migrationBuilder.DropTable(
                name: "Clasificaciones");
        }
    }
}
