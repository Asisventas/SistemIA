using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Presupuestos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoCondicion",
                table: "Ventas",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CreditoSaldo",
                table: "Ventas",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EsMonedaExtranjera",
                table: "Ventas",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdTipoPago",
                table: "Ventas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedioPago",
                table: "Ventas",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TipoIngreso",
                table: "Ventas",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TipoPagoIdTipoPago",
                table: "Ventas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Presupuestos",
                columns: table => new
                {
                    IdPresupuesto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    suc = table.Column<int>(type: "int", nullable: false),
                    IdCliente = table.Column<int>(type: "int", nullable: true),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumeroPresupuesto = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IdMoneda = table.Column<int>(type: "int", nullable: true),
                    SimboloMoneda = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    CambioDelDia = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    EsMonedaExtranjera = table.Column<bool>(type: "bit", nullable: true),
                    Total = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalEnLetras = table.Column<string>(type: "nvarchar(280)", maxLength: 280, nullable: true),
                    ValidezDias = table.Column<int>(type: "int", nullable: true),
                    ValidoHasta = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Comentario = table.Column<string>(type: "nvarchar(280)", maxLength: 280, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IdVentaConvertida = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Presupuestos", x => x.IdPresupuesto);
                    table.ForeignKey(
                        name: "FK_Presupuestos_Clientes_IdCliente",
                        column: x => x.IdCliente,
                        principalTable: "Clientes",
                        principalColumn: "IdCliente",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Presupuestos_Monedas_IdMoneda",
                        column: x => x.IdMoneda,
                        principalTable: "Monedas",
                        principalColumn: "IdMoneda",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Presupuestos_Sucursal_suc",
                        column: x => x.suc,
                        principalTable: "Sucursal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PresupuestosDetalles",
                columns: table => new
                {
                    IdPresupuestoDetalle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdPresupuesto = table.Column<int>(type: "int", nullable: false),
                    IdProducto = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Importe = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IVA10 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IVA5 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Exenta = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Grabado10 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Grabado5 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CambioDelDia = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    IdTipoIva = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PresupuestosDetalles", x => x.IdPresupuestoDetalle);
                    table.ForeignKey(
                        name: "FK_PresupuestosDetalles_Presupuestos_IdPresupuesto",
                        column: x => x.IdPresupuesto,
                        principalTable: "Presupuestos",
                        principalColumn: "IdPresupuesto",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PresupuestosDetalles_Productos_IdProducto",
                        column: x => x.IdProducto,
                        principalTable: "Productos",
                        principalColumn: "IdProducto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_TipoPagoIdTipoPago",
                table: "Ventas",
                column: "TipoPagoIdTipoPago");

            migrationBuilder.CreateIndex(
                name: "IX_Presupuestos_IdCliente",
                table: "Presupuestos",
                column: "IdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_Presupuestos_IdMoneda",
                table: "Presupuestos",
                column: "IdMoneda");

            migrationBuilder.CreateIndex(
                name: "IX_Presupuestos_suc",
                table: "Presupuestos",
                column: "suc");

            migrationBuilder.CreateIndex(
                name: "IX_PresupuestosDetalles_IdPresupuesto",
                table: "PresupuestosDetalles",
                column: "IdPresupuesto");

            migrationBuilder.CreateIndex(
                name: "IX_PresupuestosDetalles_IdProducto",
                table: "PresupuestosDetalles",
                column: "IdProducto");

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_TiposPago_TipoPagoIdTipoPago",
                table: "Ventas",
                column: "TipoPagoIdTipoPago",
                principalTable: "TiposPago",
                principalColumn: "IdTipoPago");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_TiposPago_TipoPagoIdTipoPago",
                table: "Ventas");

            migrationBuilder.DropTable(
                name: "PresupuestosDetalles");

            migrationBuilder.DropTable(
                name: "Presupuestos");

            migrationBuilder.DropIndex(
                name: "IX_Ventas_TipoPagoIdTipoPago",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "CodigoCondicion",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "CreditoSaldo",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "EsMonedaExtranjera",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "IdTipoPago",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "MedioPago",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "TipoIngreso",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "TipoPagoIdTipoPago",
                table: "Ventas");
        }
    }
}
