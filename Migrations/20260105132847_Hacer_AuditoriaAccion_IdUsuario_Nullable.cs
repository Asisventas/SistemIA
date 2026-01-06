using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Hacer_AuditoriaAccion_IdUsuario_Nullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditoriasAcciones_Usuarios_IdUsuario",
                table: "AuditoriasAcciones");

            migrationBuilder.AlterColumn<string>(
                name: "NombreUsuario",
                table: "AuditoriasAcciones",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<int>(
                name: "IdUsuario",
                table: "AuditoriasAcciones",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditoriasAcciones_Usuarios_IdUsuario",
                table: "AuditoriasAcciones",
                column: "IdUsuario",
                principalTable: "Usuarios",
                principalColumn: "Id_Usu");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditoriasAcciones_Usuarios_IdUsuario",
                table: "AuditoriasAcciones");

            migrationBuilder.AlterColumn<string>(
                name: "NombreUsuario",
                table: "AuditoriasAcciones",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "IdUsuario",
                table: "AuditoriasAcciones",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditoriasAcciones_Usuarios_IdUsuario",
                table: "AuditoriasAcciones",
                column: "IdUsuario",
                principalTable: "Usuarios",
                principalColumn: "Id_Usu",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
