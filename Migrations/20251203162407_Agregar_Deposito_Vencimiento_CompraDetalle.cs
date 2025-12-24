using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Deposito_Vencimiento_CompraDetalle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaVencimientoItem",
                table: "ComprasDetalles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdDepositoItem",
                table: "ComprasDetalles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComprasDetalles_IdDepositoItem",
                table: "ComprasDetalles",
                column: "IdDepositoItem");

            migrationBuilder.AddForeignKey(
                name: "FK_ComprasDetalles_Depositos_IdDepositoItem",
                table: "ComprasDetalles",
                column: "IdDepositoItem",
                principalTable: "Depositos",
                principalColumn: "IdDeposito");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComprasDetalles_Depositos_IdDepositoItem",
                table: "ComprasDetalles");

            migrationBuilder.DropIndex(
                name: "IX_ComprasDetalles_IdDepositoItem",
                table: "ComprasDetalles");

            migrationBuilder.DropColumn(
                name: "FechaVencimientoItem",
                table: "ComprasDetalles");

            migrationBuilder.DropColumn(
                name: "IdDepositoItem",
                table: "ComprasDetalles");
        }
    }
}
