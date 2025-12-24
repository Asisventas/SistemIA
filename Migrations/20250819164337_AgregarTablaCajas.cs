using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTablaCajas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Limpieza: esta migración no debe tocar la tabla Proveedores legacy

            migrationBuilder.CreateTable(
                name: "Cajas",
                columns: table => new
                {
                    id_caja = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CantTurnos = table.Column<int>(type: "int", nullable: true),
                    TurnoActual = table.Column<int>(type: "int", nullable: true),
                    Nivel1 = table.Column<string>(type: "nchar(3)", fixedLength: true, maxLength: 3, nullable: true),
                    Nivel2 = table.Column<string>(type: "nchar(3)", fixedLength: true, maxLength: 3, nullable: true),
                    FacturaInicial = table.Column<string>(type: "nchar(7)", fixedLength: true, maxLength: 7, nullable: true),
                    Serie = table.Column<int>(type: "int", nullable: true),
                    Timbrado = table.Column<string>(type: "nchar(8)", fixedLength: true, maxLength: 8, nullable: true),
                    VigenciaDel = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VigenciaAl = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NombreImpresora = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    BloquearFactura = table.Column<bool>(type: "bit", nullable: true),
                    CajaActual = table.Column<int>(type: "int", nullable: true),
                    FechaActualCaja = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Imprimir_Factura = table.Column<int>(type: "int", nullable: true),
                    Nivel1R = table.Column<string>(type: "nchar(3)", fixedLength: true, maxLength: 3, nullable: true),
                    Nivel2R = table.Column<string>(type: "nchar(3)", fixedLength: true, maxLength: 3, nullable: true),
                    FacturaInicialR = table.Column<string>(type: "nchar(7)", fixedLength: true, maxLength: 7, nullable: true),
                    SerieR = table.Column<int>(type: "int", nullable: true),
                    TimbradoR = table.Column<string>(type: "nchar(8)", fixedLength: true, maxLength: 8, nullable: true),
                    VigenciaDelR = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VigenciaAlR = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NombreImpresoraR = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    BloquearFacturaR = table.Column<bool>(type: "bit", nullable: true),
                    Imprimir_remisionR = table.Column<bool>(type: "bit", nullable: true),
                    Nivel1NC = table.Column<string>(type: "nchar(3)", fixedLength: true, maxLength: 3, nullable: true),
                    Nivel2NC = table.Column<string>(type: "nchar(3)", fixedLength: true, maxLength: 3, nullable: true),
                    NumeroNC = table.Column<string>(type: "nchar(7)", fixedLength: true, maxLength: 7, nullable: true),
                    SerieNC = table.Column<int>(type: "int", nullable: true),
                    TimbradoNC = table.Column<string>(type: "nchar(8)", fixedLength: true, maxLength: 8, nullable: true),
                    VigenciaDelNC = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VigenciaAlNC = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NombreImpresoraNC = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    BloquearFacturaNC = table.Column<bool>(type: "bit", nullable: true),
                    Imprimir_remisionNC = table.Column<bool>(type: "bit", nullable: true),
                    Nivel1Recibo = table.Column<string>(type: "nchar(3)", fixedLength: true, maxLength: 3, nullable: true),
                    Nivel2Recibo = table.Column<string>(type: "nchar(3)", fixedLength: true, maxLength: 3, nullable: true),
                    NumeroRecibo = table.Column<string>(type: "nchar(7)", fixedLength: true, maxLength: 7, nullable: true),
                    SerieRecibo = table.Column<int>(type: "int", nullable: true),
                    TimbradoRecibo = table.Column<string>(type: "nchar(8)", fixedLength: true, maxLength: 8, nullable: true),
                    VigenciaDelRecibo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VigenciaAlRecibo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NombreImpresoraRecibo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    BloquearFacturaRecibo = table.Column<bool>(type: "bit", nullable: true),
                    Imprimir_remisionRecibo = table.Column<bool>(type: "bit", nullable: true),
                    modelo_factura = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    anular_item = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    bloquear_fechaCaja = table.Column<bool>(type: "bit", nullable: true),
                    cierre_simultaneo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    numero_correlativo = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cajas", x => x.id_caja);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Compras_IdCaja",
                table: "Compras",
                column: "IdCaja");

            migrationBuilder.AddForeignKey(
                name: "FK_Compras_Cajas_IdCaja",
                table: "Compras",
                column: "IdCaja",
                principalTable: "Cajas",
                principalColumn: "id_caja",
                onDelete: ReferentialAction.Restrict);

            // FK a ProveedoresSifen ya existe por migraciones previas; no la toques aquí
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Compras_Cajas_IdCaja",
                table: "Compras");

            migrationBuilder.DropForeignKey(
                name: "FK_Compras_ProveedoresSifen_IdProveedor",
                table: "Compras");

            migrationBuilder.DropTable(
                name: "Cajas");

            migrationBuilder.DropIndex(
                name: "IX_Compras_IdCaja",
                table: "Compras");

            // Down limpio: no tocar Proveedores legacy
        }
    }
}
