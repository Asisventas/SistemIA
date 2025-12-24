using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class AddTiposPago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdTipoPago",
                table: "Compras",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TiposPago",
                columns: table => new
                {
                    IdTipoPago = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EsCredito = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Orden = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposPago", x => x.IdTipoPago);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Compras_IdTipoPago",
                table: "Compras",
                column: "IdTipoPago");

            migrationBuilder.CreateIndex(
                name: "IX_TiposPago_Nombre",
                table: "TiposPago",
                column: "Nombre",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Compras_TiposPago_IdTipoPago",
                table: "Compras",
                column: "IdTipoPago",
                principalTable: "TiposPago",
                principalColumn: "IdTipoPago",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Compras_TiposPago_IdTipoPago",
                table: "Compras");

            migrationBuilder.DropTable(
                name: "TiposPago");

            migrationBuilder.DropIndex(
                name: "IX_Compras_IdTipoPago",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "IdTipoPago",
                table: "Compras");
        }
    }
}
