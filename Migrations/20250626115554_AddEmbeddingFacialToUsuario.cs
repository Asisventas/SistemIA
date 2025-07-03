using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class AddEmbeddingFacialToUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clientes_TiposOperacion_TipoOperacion",
                table: "Clientes");

            migrationBuilder.AddColumn<byte[]>(
                name: "EmbeddingFacial",
                table: "Usuarios",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TipoOperacion",
                table: "Clientes",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "Clientes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Clientes_TiposOperacion_TipoOperacion",
                table: "Clientes",
                column: "TipoOperacion",
                principalTable: "TiposOperacion",
                principalColumn: "Codigo",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clientes_TiposOperacion_TipoOperacion",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "EmbeddingFacial",
                table: "Usuarios");

            migrationBuilder.AlterColumn<string>(
                name: "TipoOperacion",
                table: "Clientes",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "Clientes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddForeignKey(
                name: "FK_Clientes_TiposOperacion_TipoOperacion",
                table: "Clientes",
                column: "TipoOperacion",
                principalTable: "TiposOperacion",
                principalColumn: "Codigo");
        }
    }
}
