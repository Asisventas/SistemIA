using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_IdProductoLote_VentaCompraDetalle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaVencimientoLoteMomento",
                table: "VentasDetalles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdProductoLote",
                table: "VentasDetalles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroLoteMomento",
                table: "VentasDetalles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdProductoLote",
                table: "ComprasDetalles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroLote",
                table: "ComprasDetalles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VentasDetalles_IdProductoLote",
                table: "VentasDetalles",
                column: "IdProductoLote");

            migrationBuilder.CreateIndex(
                name: "IX_ComprasDetalles_IdProductoLote",
                table: "ComprasDetalles",
                column: "IdProductoLote");

            migrationBuilder.AddForeignKey(
                name: "FK_ComprasDetalles_ProductosLotes_IdProductoLote",
                table: "ComprasDetalles",
                column: "IdProductoLote",
                principalTable: "ProductosLotes",
                principalColumn: "IdProductoLote");

            migrationBuilder.AddForeignKey(
                name: "FK_VentasDetalles_ProductosLotes_IdProductoLote",
                table: "VentasDetalles",
                column: "IdProductoLote",
                principalTable: "ProductosLotes",
                principalColumn: "IdProductoLote");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComprasDetalles_ProductosLotes_IdProductoLote",
                table: "ComprasDetalles");

            migrationBuilder.DropForeignKey(
                name: "FK_VentasDetalles_ProductosLotes_IdProductoLote",
                table: "VentasDetalles");

            migrationBuilder.DropIndex(
                name: "IX_VentasDetalles_IdProductoLote",
                table: "VentasDetalles");

            migrationBuilder.DropIndex(
                name: "IX_ComprasDetalles_IdProductoLote",
                table: "ComprasDetalles");

            migrationBuilder.DropColumn(
                name: "FechaVencimientoLoteMomento",
                table: "VentasDetalles");

            migrationBuilder.DropColumn(
                name: "IdProductoLote",
                table: "VentasDetalles");

            migrationBuilder.DropColumn(
                name: "NumeroLoteMomento",
                table: "VentasDetalles");

            migrationBuilder.DropColumn(
                name: "IdProductoLote",
                table: "ComprasDetalles");

            migrationBuilder.DropColumn(
                name: "NumeroLote",
                table: "ComprasDetalles");
        }
    }
}
