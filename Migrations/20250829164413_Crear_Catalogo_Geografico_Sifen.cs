using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Crear_Catalogo_Geografico_Sifen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "departamento",
                columns: table => new
                {
                    Numero = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_departamento", x => x.Numero);
                });

            migrationBuilder.CreateTable(
                name: "distrito",
                columns: table => new
                {
                    Numero = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    Departamento = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_distrito", x => x.Numero);
                    table.ForeignKey(
                        name: "FK_distrito_departamento_Departamento",
                        column: x => x.Departamento,
                        principalTable: "departamento",
                        principalColumn: "Numero",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ciudad",
                columns: table => new
                {
                    Numero = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    Departamento = table.Column<int>(type: "int", nullable: false),
                    Distrito = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ciudad", x => x.Numero);
                    table.ForeignKey(
                        name: "FK_ciudad_departamento_Departamento",
                        column: x => x.Departamento,
                        principalTable: "departamento",
                        principalColumn: "Numero",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ciudad_distrito_Distrito",
                        column: x => x.Distrito,
                        principalTable: "distrito",
                        principalColumn: "Numero",
                        onDelete: ReferentialAction.Restrict);
                });

            // Índices útiles para búsquedas frecuentes
            migrationBuilder.CreateIndex(
                name: "IX_departamento_Nombre",
                table: "departamento",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "IX_distrito_Departamento_Nombre",
                table: "distrito",
                columns: new[] { "Departamento", "Nombre" });

            migrationBuilder.CreateIndex(
                name: "IX_ciudad_Departamento_Distrito_Nombre",
                table: "ciudad",
                columns: new[] { "Departamento", "Distrito", "Nombre" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ciudad");

            migrationBuilder.DropTable(
                name: "distrito");

            migrationBuilder.DropTable(
                name: "departamento");
        }
    }
}
