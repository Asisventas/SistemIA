using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class CrearTablaProductos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    IdProducto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoInterno = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    GTIN = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: true),
                    NCM = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    UnidadMedidaCodigo = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    TipoItem = table.Column<int>(type: "int", nullable: false),
                    IdMarca = table.Column<int>(type: "int", nullable: true),
                    IdTipoIva = table.Column<int>(type: "int", nullable: false),
                    CostoUnitarioGs = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    PrecioUnitarioGs = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PrecioUnitarioUsd = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Stock = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    StockMinimo = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.IdProducto);
                    table.ForeignKey(
                        name: "FK_Productos_Marcas_IdMarca",
                        column: x => x.IdMarca,
                        principalTable: "Marcas",
                        principalColumn: "Id_Marca");
                    table.ForeignKey(
                        name: "FK_Productos_TiposIva_IdTipoIva",
                        column: x => x.IdTipoIva,
                        principalTable: "TiposIva",
                        principalColumn: "IdTipoIva",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Productos_IdMarca",
                table: "Productos",
                column: "IdMarca");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_IdTipoIva",
                table: "Productos",
                column: "IdTipoIva");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Productos");
        }
    }
}
