using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Horarios_Suscripcion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "HoraEnvioCorreo",
                table: "SuscripcionesClientes",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "HoraFacturacion",
                table: "SuscripcionesClientes",
                type: "time",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HoraEnvioCorreo",
                table: "SuscripcionesClientes");

            migrationBuilder.DropColumn(
                name: "HoraFacturacion",
                table: "SuscripcionesClientes");
        }
    }
}
