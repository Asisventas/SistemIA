using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Campos_SoporteRemoto_Cliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CadenaConexionBD",
                table: "Clientes",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HabilitadoSoporteRemoto",
                table: "Clientes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "IpVpnAsignada",
                table: "Clientes",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreServicioWindows",
                table: "Clientes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NotasAccesoRemoto",
                table: "Clientes",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PuertoSistema",
                table: "Clientes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RutaCarpetaSistema",
                table: "Clientes",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimaConexionRemota",
                table: "Clientes",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CadenaConexionBD",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "HabilitadoSoporteRemoto",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "IpVpnAsignada",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "NombreServicioWindows",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "NotasAccesoRemoto",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "PuertoSistema",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "RutaCarpetaSistema",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "UltimaConexionRemota",
                table: "Clientes");
        }
    }
}
