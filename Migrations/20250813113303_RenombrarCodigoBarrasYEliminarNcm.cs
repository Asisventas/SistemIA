using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class RenombrarCodigoBarrasYEliminarNcm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NCM",
                table: "Productos");

            migrationBuilder.RenameColumn(
                name: "GTIN",
                table: "Productos",
                newName: "CodigoBarras");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CodigoBarras",
                table: "Productos",
                newName: "GTIN");

            migrationBuilder.AddColumn<string>(
                name: "NCM",
                table: "Productos",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true);
        }
    }
}
