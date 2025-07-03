using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceAndScheduleTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "EmbeddingFacial",
                table: "Usuarios",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0],
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Id_HorarioTrabajo",
                table: "Usuarios",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Asistencias",
                columns: table => new
                {
                    Id_Asistencia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id_Usuario = table.Column<int>(type: "int", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TipoRegistro = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Ubicacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaInicioAusencia = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaFinAusencia = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MotivoAusencia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AprobadaPorGerencia = table.Column<bool>(type: "bit", nullable: false),
                    AprobadoPorId_Usuario = table.Column<int>(type: "int", nullable: true),
                    FechaAprobacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asistencias", x => x.Id_Asistencia);
                    table.ForeignKey(
                        name: "FK_Asistencias_Usuarios_AprobadoPorId_Usuario",
                        column: x => x.AprobadoPorId_Usuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id_Usu",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Asistencias_Usuarios_Id_Usuario",
                        column: x => x.Id_Usuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id_Usu",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HorariosTrabajo",
                columns: table => new
                {
                    Id_Horario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreHorario = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HoraEntrada = table.Column<TimeSpan>(type: "time", nullable: false),
                    HoraSalida = table.Column<TimeSpan>(type: "time", nullable: false),
                    InicioBreak = table.Column<TimeSpan>(type: "time", nullable: true),
                    FinBreak = table.Column<TimeSpan>(type: "time", nullable: true),
                    Lunes = table.Column<bool>(type: "bit", nullable: false),
                    Martes = table.Column<bool>(type: "bit", nullable: false),
                    Miercoles = table.Column<bool>(type: "bit", nullable: false),
                    Jueves = table.Column<bool>(type: "bit", nullable: false),
                    Viernes = table.Column<bool>(type: "bit", nullable: false),
                    Sabado = table.Column<bool>(type: "bit", nullable: false),
                    Domingo = table.Column<bool>(type: "bit", nullable: false),
                    EsActivo = table.Column<bool>(type: "bit", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorariosTrabajo", x => x.Id_Horario);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Id_HorarioTrabajo",
                table: "Usuarios",
                column: "Id_HorarioTrabajo");

            migrationBuilder.CreateIndex(
                name: "IX_Asistencias_AprobadoPorId_Usuario",
                table: "Asistencias",
                column: "AprobadoPorId_Usuario");

            migrationBuilder.CreateIndex(
                name: "IX_Asistencias_Id_Usuario",
                table: "Asistencias",
                column: "Id_Usuario");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_HorariosTrabajo_Id_HorarioTrabajo",
                table: "Usuarios",
                column: "Id_HorarioTrabajo",
                principalTable: "HorariosTrabajo",
                principalColumn: "Id_Horario",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_HorariosTrabajo_Id_HorarioTrabajo",
                table: "Usuarios");

            migrationBuilder.DropTable(
                name: "Asistencias");

            migrationBuilder.DropTable(
                name: "HorariosTrabajo");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_Id_HorarioTrabajo",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Id_HorarioTrabajo",
                table: "Usuarios");

            migrationBuilder.AlterColumn<byte[]>(
                name: "EmbeddingFacial",
                table: "Usuarios",
                type: "varbinary(max)",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)");
        }
    }
}
