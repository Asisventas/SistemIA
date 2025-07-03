using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class ModeloDeAsignacionDeHorarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HorariosTrabajo_Sucursal_Id_Sucursal",
                table: "HorariosTrabajo");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_HorariosTrabajo_Id_HorarioTrabajo",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_Id_HorarioTrabajo",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Id_HorarioTrabajo",
                table: "Usuarios");

            migrationBuilder.CreateTable(
                name: "AsignacionHorario",
                columns: table => new
                {
                    Id_Asignacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id_Usuario = table.Column<int>(type: "int", nullable: false),
                    Id_Horario = table.Column<int>(type: "int", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsignacionHorario", x => x.Id_Asignacion);
                    table.ForeignKey(
                        name: "FK_AsignacionHorario_HorariosTrabajo_Id_Horario",
                        column: x => x.Id_Horario,
                        principalTable: "HorariosTrabajo",
                        principalColumn: "Id_Horario",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AsignacionHorario_Usuarios_Id_Usuario",
                        column: x => x.Id_Usuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id_Usu",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionHorario_Id_Horario",
                table: "AsignacionHorario",
                column: "Id_Horario");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionHorario_Id_Usuario",
                table: "AsignacionHorario",
                column: "Id_Usuario");

            migrationBuilder.AddForeignKey(
                name: "FK_HorariosTrabajo_Sucursal_Id_Sucursal",
                table: "HorariosTrabajo",
                column: "Id_Sucursal",
                principalTable: "Sucursal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HorariosTrabajo_Sucursal_Id_Sucursal",
                table: "HorariosTrabajo");

            migrationBuilder.DropTable(
                name: "AsignacionHorario");

            migrationBuilder.AddColumn<int>(
                name: "Id_HorarioTrabajo",
                table: "Usuarios",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Id_HorarioTrabajo",
                table: "Usuarios",
                column: "Id_HorarioTrabajo");

            migrationBuilder.AddForeignKey(
                name: "FK_HorariosTrabajo_Sucursal_Id_Sucursal",
                table: "HorariosTrabajo",
                column: "Id_Sucursal",
                principalTable: "Sucursal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_HorariosTrabajo_Id_HorarioTrabajo",
                table: "Usuarios",
                column: "Id_HorarioTrabajo",
                principalTable: "HorariosTrabajo",
                principalColumn: "Id_Horario",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
