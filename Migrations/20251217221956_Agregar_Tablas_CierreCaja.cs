using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Tablas_CierreCaja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CierresCaja",
                columns: table => new
                {
                    IdCierreCaja = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCaja = table.Column<int>(type: "int", nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaCaja = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Turno = table.Column<int>(type: "int", nullable: false),
                    UsuarioCierre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TotalVentasContado = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalVentasCredito = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCobrosCredito = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAnulaciones = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalEfectivo = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalTarjetas = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCheques = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalTransferencias = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalQR = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalOtros = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalEsperado = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalEntregado = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Diferencia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CierresCaja", x => x.IdCierreCaja);
                    table.ForeignKey(
                        name: "FK_CierresCaja_Cajas_IdCaja",
                        column: x => x.IdCaja,
                        principalTable: "Cajas",
                        principalColumn: "id_caja",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EntregasCaja",
                columns: table => new
                {
                    IdEntregaCaja = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCierreCaja = table.Column<int>(type: "int", nullable: false),
                    Medio = table.Column<int>(type: "int", nullable: false),
                    IdMoneda = table.Column<int>(type: "int", nullable: true),
                    MontoEsperado = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontoEntregado = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Diferencia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReceptorEntrega = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DetalleCheques = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CantidadVouchers = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntregasCaja", x => x.IdEntregaCaja);
                    table.ForeignKey(
                        name: "FK_EntregasCaja_CierresCaja_IdCierreCaja",
                        column: x => x.IdCierreCaja,
                        principalTable: "CierresCaja",
                        principalColumn: "IdCierreCaja",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntregasCaja_Monedas_IdMoneda",
                        column: x => x.IdMoneda,
                        principalTable: "Monedas",
                        principalColumn: "IdMoneda",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CierresCaja_Caja_Fecha_Turno",
                table: "CierresCaja",
                columns: new[] { "IdCaja", "FechaCaja", "Turno" });

            migrationBuilder.CreateIndex(
                name: "IX_EntregasCaja_IdCierreCaja",
                table: "EntregasCaja",
                column: "IdCierreCaja");

            migrationBuilder.CreateIndex(
                name: "IX_EntregasCaja_IdMoneda",
                table: "EntregasCaja",
                column: "IdMoneda");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntregasCaja");

            migrationBuilder.DropTable(
                name: "CierresCaja");
        }
    }
}
