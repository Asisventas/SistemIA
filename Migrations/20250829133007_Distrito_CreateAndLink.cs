using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Distrito_CreateAndLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Distritos",
                columns: table => new
                {
                    IdDistrito = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Distritos", x => x.IdDistrito);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sociedades_Distrito",
                table: "Sociedades",
                column: "Distrito");

            // Asegurar integridad antes de agregar la FK: poner en NULL cualquier valor existente
            migrationBuilder.Sql("UPDATE Sociedades SET Distrito = NULL WHERE Distrito IS NOT NULL;");

            // Agregar la FK sin validar datos existentes para evitar conflictos en bases antiguas
            migrationBuilder.Sql(@"
                ALTER TABLE [Sociedades] WITH NOCHECK
                ADD CONSTRAINT [FK_Sociedades_Distritos_Distrito]
                FOREIGN KEY ([Distrito]) REFERENCES [Distritos]([IdDistrito]) ON DELETE SET NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sociedades_Distritos_Distrito",
                table: "Sociedades");

            migrationBuilder.DropTable(
                name: "Distritos");

            migrationBuilder.DropIndex(
                name: "IX_Sociedades_Distrito",
                table: "Sociedades");
        }
    }
}
