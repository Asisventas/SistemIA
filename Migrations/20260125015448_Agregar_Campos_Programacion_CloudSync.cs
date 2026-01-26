using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Campos_Programacion_CloudSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IntervaloHoras",
                table: "ConfiguracionesCloudSync",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProximaEjecucion",
                table: "ConfiguracionesCloudSync",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoProgramacion",
                table: "ConfiguracionesCloudSync",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimoBackupExitoso",
                table: "ConfiguracionesCloudSync",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IntervaloHoras",
                table: "ConfiguracionesCloudSync");

            migrationBuilder.DropColumn(
                name: "ProximaEjecucion",
                table: "ConfiguracionesCloudSync");

            migrationBuilder.DropColumn(
                name: "TipoProgramacion",
                table: "ConfiguracionesCloudSync");

            migrationBuilder.DropColumn(
                name: "UltimoBackupExitoso",
                table: "ConfiguracionesCloudSync");
        }
    }
}
