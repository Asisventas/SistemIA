using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_DescuentosCategorias_y_Producto_Descuento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DescuentoAutomaticoProducto",
                table: "Productos",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UsaDescuentoEspecifico",
                table: "Productos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "DescuentosCategorias",
                columns: table => new
                {
                    IdDescuentoCategoria = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoCategoria = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IdMarca = table.Column<int>(type: "int", nullable: true),
                    IdClasificacion = table.Column<int>(type: "int", nullable: true),
                    PorcentajeDescuento = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    Prioridad = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DescuentosCategorias", x => x.IdDescuentoCategoria);
                    table.ForeignKey(
                        name: "FK_DescuentosCategorias_Clasificaciones_IdClasificacion",
                        column: x => x.IdClasificacion,
                        principalTable: "Clasificaciones",
                        principalColumn: "IdClasificacion");
                    table.ForeignKey(
                        name: "FK_DescuentosCategorias_Marcas_IdMarca",
                        column: x => x.IdMarca,
                        principalTable: "Marcas",
                        principalColumn: "Id_Marca");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DescuentosCategorias_IdClasificacion",
                table: "DescuentosCategorias",
                column: "IdClasificacion");

            migrationBuilder.CreateIndex(
                name: "IX_DescuentosCategorias_IdMarca",
                table: "DescuentosCategorias",
                column: "IdMarca");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DescuentosCategorias");

            migrationBuilder.DropColumn(
                name: "DescuentoAutomaticoProducto",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "UsaDescuentoEspecifico",
                table: "Productos");
        }
    }
}
