using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Lotes_TransferenciasDeposito : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaVencimientoLote",
                table: "TransferenciasDepositoDetalle",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdProductoLoteDestino",
                table: "TransferenciasDepositoDetalle",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdProductoLoteOrigen",
                table: "TransferenciasDepositoDetalle",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroLote",
                table: "TransferenciasDepositoDetalle",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransferenciasDepositoDetalle_IdProductoLoteDestino",
                table: "TransferenciasDepositoDetalle",
                column: "IdProductoLoteDestino");

            migrationBuilder.CreateIndex(
                name: "IX_TransferenciasDepositoDetalle_IdProductoLoteOrigen",
                table: "TransferenciasDepositoDetalle",
                column: "IdProductoLoteOrigen");

            migrationBuilder.AddForeignKey(
                name: "FK_TransferenciasDepositoDetalle_ProductosLotes_IdProductoLoteDestino",
                table: "TransferenciasDepositoDetalle",
                column: "IdProductoLoteDestino",
                principalTable: "ProductosLotes",
                principalColumn: "IdProductoLote");

            migrationBuilder.AddForeignKey(
                name: "FK_TransferenciasDepositoDetalle_ProductosLotes_IdProductoLoteOrigen",
                table: "TransferenciasDepositoDetalle",
                column: "IdProductoLoteOrigen",
                principalTable: "ProductosLotes",
                principalColumn: "IdProductoLote");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransferenciasDepositoDetalle_ProductosLotes_IdProductoLoteDestino",
                table: "TransferenciasDepositoDetalle");

            migrationBuilder.DropForeignKey(
                name: "FK_TransferenciasDepositoDetalle_ProductosLotes_IdProductoLoteOrigen",
                table: "TransferenciasDepositoDetalle");

            migrationBuilder.DropIndex(
                name: "IX_TransferenciasDepositoDetalle_IdProductoLoteDestino",
                table: "TransferenciasDepositoDetalle");

            migrationBuilder.DropIndex(
                name: "IX_TransferenciasDepositoDetalle_IdProductoLoteOrigen",
                table: "TransferenciasDepositoDetalle");

            migrationBuilder.DropColumn(
                name: "FechaVencimientoLote",
                table: "TransferenciasDepositoDetalle");

            migrationBuilder.DropColumn(
                name: "IdProductoLoteDestino",
                table: "TransferenciasDepositoDetalle");

            migrationBuilder.DropColumn(
                name: "IdProductoLoteOrigen",
                table: "TransferenciasDepositoDetalle");

            migrationBuilder.DropColumn(
                name: "NumeroLote",
                table: "TransferenciasDepositoDetalle");
        }
    }
}
