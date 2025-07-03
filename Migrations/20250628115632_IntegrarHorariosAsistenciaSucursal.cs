using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class IntegrarHorariosAsistenciaSucursal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Asistencias_Usuarios_Id_Usuario",
                table: "Asistencias");

            migrationBuilder.RenameColumn(
                name: "sucursal",
                table: "HorariosTrabajo",
                newName: "Id_Sucursal");

            migrationBuilder.AlterColumn<string>(
                name: "CertificadoRuta",
                table: "Sucursal",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CertificadoPassword",
                table: "Sucursal",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HorariosTrabajo_Id_Sucursal",
                table: "HorariosTrabajo",
                column: "Id_Sucursal");

            migrationBuilder.CreateIndex(
                name: "IX_Asistencias_Sucursal",
                table: "Asistencias",
                column: "Sucursal");

            migrationBuilder.AddForeignKey(
                name: "FK_Asistencias_Sucursal_Sucursal",
                table: "Asistencias",
                column: "Sucursal",
                principalTable: "Sucursal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Asistencias_Usuarios_Id_Usuario",
                table: "Asistencias",
                column: "Id_Usuario",
                principalTable: "Usuarios",
                principalColumn: "Id_Usu",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HorariosTrabajo_Sucursal_Id_Sucursal",
                table: "HorariosTrabajo",
                column: "Id_Sucursal",
                principalTable: "Sucursal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Asistencias_Sucursal_Sucursal",
                table: "Asistencias");

            migrationBuilder.DropForeignKey(
                name: "FK_Asistencias_Usuarios_Id_Usuario",
                table: "Asistencias");

            migrationBuilder.DropForeignKey(
                name: "FK_HorariosTrabajo_Sucursal_Id_Sucursal",
                table: "HorariosTrabajo");

            migrationBuilder.DropIndex(
                name: "IX_HorariosTrabajo_Id_Sucursal",
                table: "HorariosTrabajo");

            migrationBuilder.DropIndex(
                name: "IX_Asistencias_Sucursal",
                table: "Asistencias");

            migrationBuilder.RenameColumn(
                name: "Id_Sucursal",
                table: "HorariosTrabajo",
                newName: "sucursal");

            migrationBuilder.AlterColumn<string>(
                name: "CertificadoRuta",
                table: "Sucursal",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CertificadoPassword",
                table: "Sucursal",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Asistencias_Usuarios_Id_Usuario",
                table: "Asistencias",
                column: "Id_Usuario",
                principalTable: "Usuarios",
                principalColumn: "Id_Usu",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
