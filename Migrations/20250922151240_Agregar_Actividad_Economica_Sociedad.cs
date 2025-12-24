using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Actividad_Economica_Sociedad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SociedadesActividades",
                columns: table => new
                {
                    Numero = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdSociedad = table.Column<int>(type: "int", nullable: false),
                    CodigoActividad = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    NombreActividad = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true),
                    ActividadPrincipal = table.Column<string>(type: "char(1)", maxLength: 1, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SociedadesActividades", x => x.Numero);
                    table.ForeignKey(
                        name: "FK_SociedadesActividades_Sociedades_IdSociedad",
                        column: x => x.IdSociedad,
                        principalTable: "Sociedades",
                        principalColumn: "IdSociedad",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SociedadesActividades_IdSociedad_CodigoActividad",
                table: "SociedadesActividades",
                columns: new[] { "IdSociedad", "CodigoActividad" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SociedadesActividades");
        }
    }
}
