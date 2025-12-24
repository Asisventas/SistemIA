using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class AddTiposDocumentoOperacion_CompraFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdTipoDocumentoOperacion",
                table: "Compras",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoDocumentoOperacionIdTipoDocumentoOperacion",
                table: "Compras",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TiposDocumentoOperacion",
                columns: table => new
                {
                    IdTipoDocumentoOperacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Orden = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposDocumentoOperacion", x => x.IdTipoDocumentoOperacion);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Compras_IdTipoDocumentoOperacion",
                table: "Compras",
                column: "IdTipoDocumentoOperacion");

            migrationBuilder.CreateIndex(
                name: "IX_Compras_TipoDocumentoOperacionIdTipoDocumentoOperacion",
                table: "Compras",
                column: "TipoDocumentoOperacionIdTipoDocumentoOperacion");

            migrationBuilder.CreateIndex(
                name: "IX_TiposDocumentoOperacion_Nombre",
                table: "TiposDocumentoOperacion",
                column: "Nombre",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Compras_TiposDocumentoOperacion_IdTipoDocumentoOperacion",
                table: "Compras",
                column: "IdTipoDocumentoOperacion",
                principalTable: "TiposDocumentoOperacion",
                principalColumn: "IdTipoDocumentoOperacion",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Compras_TiposDocumentoOperacion_TipoDocumentoOperacionIdTipoDocumentoOperacion",
                table: "Compras",
                column: "TipoDocumentoOperacionIdTipoDocumentoOperacion",
                principalTable: "TiposDocumentoOperacion",
                principalColumn: "IdTipoDocumentoOperacion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Compras_TiposDocumentoOperacion_IdTipoDocumentoOperacion",
                table: "Compras");

            migrationBuilder.DropForeignKey(
                name: "FK_Compras_TiposDocumentoOperacion_TipoDocumentoOperacionIdTipoDocumentoOperacion",
                table: "Compras");

            migrationBuilder.DropTable(
                name: "TiposDocumentoOperacion");

            migrationBuilder.DropIndex(
                name: "IX_Compras_IdTipoDocumentoOperacion",
                table: "Compras");

            migrationBuilder.DropIndex(
                name: "IX_Compras_TipoDocumentoOperacionIdTipoDocumentoOperacion",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "IdTipoDocumentoOperacion",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "TipoDocumentoOperacionIdTipoDocumentoOperacion",
                table: "Compras");
        }
    }
}
