using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_NotasCredito_Columnas_Faltantes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoProducto",
                table: "NotasCreditoVentasDetalles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoDescuento",
                table: "NotasCreditoVentasDetalles",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "NombreProducto",
                table: "NotasCreditoVentasDetalles",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TasaIVA",
                table: "NotasCreditoVentasDetalles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Turno",
                table: "NotasCreditoVentas",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "IdMoneda",
                table: "NotasCreditoVentas",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "EsMonedaExtranjera",
                table: "NotasCreditoVentas",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CambioDelDia",
                table: "NotasCreditoVentas",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreadoPor",
                table: "NotasCreditoVentas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "NotasCreditoVentas",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaModificacion",
                table: "NotasCreditoVentas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModificadoPor",
                table: "NotasCreditoVentas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreCliente",
                table: "NotasCreditoVentas",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalDescuento",
                table: "NotasCreditoVentas",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoProducto",
                table: "NotasCreditoVentasDetalles");

            migrationBuilder.DropColumn(
                name: "MontoDescuento",
                table: "NotasCreditoVentasDetalles");

            migrationBuilder.DropColumn(
                name: "NombreProducto",
                table: "NotasCreditoVentasDetalles");

            migrationBuilder.DropColumn(
                name: "TasaIVA",
                table: "NotasCreditoVentasDetalles");

            migrationBuilder.DropColumn(
                name: "CreadoPor",
                table: "NotasCreditoVentas");

            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "NotasCreditoVentas");

            migrationBuilder.DropColumn(
                name: "FechaModificacion",
                table: "NotasCreditoVentas");

            migrationBuilder.DropColumn(
                name: "ModificadoPor",
                table: "NotasCreditoVentas");

            migrationBuilder.DropColumn(
                name: "NombreCliente",
                table: "NotasCreditoVentas");

            migrationBuilder.DropColumn(
                name: "TotalDescuento",
                table: "NotasCreditoVentas");

            migrationBuilder.AlterColumn<int>(
                name: "Turno",
                table: "NotasCreditoVentas",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "IdMoneda",
                table: "NotasCreditoVentas",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "EsMonedaExtranjera",
                table: "NotasCreditoVentas",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<decimal>(
                name: "CambioDelDia",
                table: "NotasCreditoVentas",
                type: "decimal(18,4)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");
        }
    }
}
