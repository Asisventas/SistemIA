using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class CrearTablasTransferenciasDepositos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TransferenciasDeposito",
                columns: table => new
                {
                    IdTransferencia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdDepositoOrigen = table.Column<int>(type: "int", nullable: false),
                    IdDepositoDestino = table.Column<int>(type: "int", nullable: false),
                    FechaTransferencia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Comentario = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferenciasDeposito", x => x.IdTransferencia);
                    table.ForeignKey(
                        name: "FK_TransferenciasDeposito_Depositos_IdDepositoDestino",
                        column: x => x.IdDepositoDestino,
                        principalTable: "Depositos",
                        principalColumn: "IdDeposito",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_TransferenciasDeposito_Depositos_IdDepositoOrigen",
                        column: x => x.IdDepositoOrigen,
                        principalTable: "Depositos",
                        principalColumn: "IdDeposito",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "TransferenciasDepositoDetalle",
                columns: table => new
                {
                    IdTransferenciaDetalle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdTransferencia = table.Column<int>(type: "int", nullable: false),
                    IdProducto = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CostoUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferenciasDepositoDetalle", x => x.IdTransferenciaDetalle);
                    table.ForeignKey(
                        name: "FK_TransferenciasDepositoDetalle_Productos_IdProducto",
                        column: x => x.IdProducto,
                        principalTable: "Productos",
                        principalColumn: "IdProducto",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransferenciasDepositoDetalle_TransferenciasDeposito_IdTransferencia",
                        column: x => x.IdTransferencia,
                        principalTable: "TransferenciasDeposito",
                        principalColumn: "IdTransferencia",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransferenciasDeposito_IdDepositoDestino",
                table: "TransferenciasDeposito",
                column: "IdDepositoDestino");

            migrationBuilder.CreateIndex(
                name: "IX_TransferenciasDeposito_IdDepositoOrigen",
                table: "TransferenciasDeposito",
                column: "IdDepositoOrigen");

            migrationBuilder.CreateIndex(
                name: "IX_TransferenciasDepositoDetalle_IdProducto",
                table: "TransferenciasDepositoDetalle",
                column: "IdProducto");

            migrationBuilder.CreateIndex(
                name: "IX_TransferenciasDepositoDetalle_IdTransferencia",
                table: "TransferenciasDepositoDetalle",
                column: "IdTransferencia");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransferenciasDepositoDetalle");

            migrationBuilder.DropTable(
                name: "TransferenciasDeposito");
        }
    }
}
