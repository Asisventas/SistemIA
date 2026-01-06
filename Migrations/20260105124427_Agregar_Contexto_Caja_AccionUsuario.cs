using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Contexto_Caja_AccionUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCaja",
                table: "AuditoriasAcciones",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaHoraEquipo",
                table: "AuditoriasAcciones",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdCaja",
                table: "AuditoriasAcciones",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdSucursal",
                table: "AuditoriasAcciones",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreCaja",
                table: "AuditoriasAcciones",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreSucursal",
                table: "AuditoriasAcciones",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Turno",
                table: "AuditoriasAcciones",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCaja",
                table: "AccionesUsuario",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEquipo",
                table: "AccionesUsuario",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdCaja",
                table: "AccionesUsuario",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdSucursal",
                table: "AccionesUsuario",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreCaja",
                table: "AccionesUsuario",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreSucursal",
                table: "AccionesUsuario",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Turno",
                table: "AccionesUsuario",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaCaja",
                table: "AuditoriasAcciones");

            migrationBuilder.DropColumn(
                name: "FechaHoraEquipo",
                table: "AuditoriasAcciones");

            migrationBuilder.DropColumn(
                name: "IdCaja",
                table: "AuditoriasAcciones");

            migrationBuilder.DropColumn(
                name: "IdSucursal",
                table: "AuditoriasAcciones");

            migrationBuilder.DropColumn(
                name: "NombreCaja",
                table: "AuditoriasAcciones");

            migrationBuilder.DropColumn(
                name: "NombreSucursal",
                table: "AuditoriasAcciones");

            migrationBuilder.DropColumn(
                name: "Turno",
                table: "AuditoriasAcciones");

            migrationBuilder.DropColumn(
                name: "FechaCaja",
                table: "AccionesUsuario");

            migrationBuilder.DropColumn(
                name: "FechaEquipo",
                table: "AccionesUsuario");

            migrationBuilder.DropColumn(
                name: "IdCaja",
                table: "AccionesUsuario");

            migrationBuilder.DropColumn(
                name: "IdSucursal",
                table: "AccionesUsuario");

            migrationBuilder.DropColumn(
                name: "NombreCaja",
                table: "AccionesUsuario");

            migrationBuilder.DropColumn(
                name: "NombreSucursal",
                table: "AccionesUsuario");

            migrationBuilder.DropColumn(
                name: "Turno",
                table: "AccionesUsuario");
        }
    }
}
