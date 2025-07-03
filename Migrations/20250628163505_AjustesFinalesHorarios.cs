using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class AjustesFinalesHorarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AsignacionHorario_HorariosTrabajo_Id_Horario",
                table: "AsignacionHorario");

            migrationBuilder.DropForeignKey(
                name: "FK_AsignacionHorario_Usuarios_Id_Usuario",
                table: "AsignacionHorario");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AsignacionHorario",
                table: "AsignacionHorario");

            migrationBuilder.RenameTable(
                name: "AsignacionHorario",
                newName: "AsignacionesHorarios");

            migrationBuilder.RenameIndex(
                name: "IX_AsignacionHorario_Id_Usuario",
                table: "AsignacionesHorarios",
                newName: "IX_AsignacionesHorarios_Id_Usuario");

            migrationBuilder.RenameIndex(
                name: "IX_AsignacionHorario_Id_Horario",
                table: "AsignacionesHorarios",
                newName: "IX_AsignacionesHorarios_Id_Horario");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AsignacionesHorarios",
                table: "AsignacionesHorarios",
                column: "Id_Asignacion");

            migrationBuilder.AddForeignKey(
                name: "FK_AsignacionesHorarios_HorariosTrabajo_Id_Horario",
                table: "AsignacionesHorarios",
                column: "Id_Horario",
                principalTable: "HorariosTrabajo",
                principalColumn: "Id_Horario",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AsignacionesHorarios_Usuarios_Id_Usuario",
                table: "AsignacionesHorarios",
                column: "Id_Usuario",
                principalTable: "Usuarios",
                principalColumn: "Id_Usu",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AsignacionesHorarios_HorariosTrabajo_Id_Horario",
                table: "AsignacionesHorarios");

            migrationBuilder.DropForeignKey(
                name: "FK_AsignacionesHorarios_Usuarios_Id_Usuario",
                table: "AsignacionesHorarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AsignacionesHorarios",
                table: "AsignacionesHorarios");

            migrationBuilder.RenameTable(
                name: "AsignacionesHorarios",
                newName: "AsignacionHorario");

            migrationBuilder.RenameIndex(
                name: "IX_AsignacionesHorarios_Id_Usuario",
                table: "AsignacionHorario",
                newName: "IX_AsignacionHorario_Id_Usuario");

            migrationBuilder.RenameIndex(
                name: "IX_AsignacionesHorarios_Id_Horario",
                table: "AsignacionHorario",
                newName: "IX_AsignacionHorario_Id_Horario");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AsignacionHorario",
                table: "AsignacionHorario",
                column: "Id_Asignacion");

            migrationBuilder.AddForeignKey(
                name: "FK_AsignacionHorario_HorariosTrabajo_Id_Horario",
                table: "AsignacionHorario",
                column: "Id_Horario",
                principalTable: "HorariosTrabajo",
                principalColumn: "Id_Horario",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AsignacionHorario_Usuarios_Id_Usuario",
                table: "AsignacionHorario",
                column: "Id_Usuario",
                principalTable: "Usuarios",
                principalColumn: "Id_Usu",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
