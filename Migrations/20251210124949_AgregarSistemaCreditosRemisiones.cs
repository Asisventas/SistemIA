using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class AgregarSistemaCreditosRemisiones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CuentasPorCobrar",
                columns: table => new
                {
                    IdCuentaPorCobrar = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdVenta = table.Column<int>(type: "int", nullable: false),
                    IdCliente = table.Column<int>(type: "int", nullable: false),
                    IdSucursal = table.Column<int>(type: "int", nullable: false),
                    FechaCredito = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MontoTotal = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    SaldoPendiente = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    NumeroCuotas = table.Column<int>(type: "int", nullable: false),
                    PlazoDias = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IdMoneda = table.Column<int>(type: "int", nullable: true),
                    MonedaIdMoneda = table.Column<int>(type: "int", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(280)", maxLength: 280, nullable: true),
                    IdUsuarioAutorizo = table.Column<int>(type: "int", nullable: true),
                    UsuarioAutorizoId_Usu = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuentasPorCobrar", x => x.IdCuentaPorCobrar);
                    table.ForeignKey(
                        name: "FK_CuentasPorCobrar_Clientes_IdCliente",
                        column: x => x.IdCliente,
                        principalTable: "Clientes",
                        principalColumn: "IdCliente",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuentasPorCobrar_Monedas_MonedaIdMoneda",
                        column: x => x.MonedaIdMoneda,
                        principalTable: "Monedas",
                        principalColumn: "IdMoneda");
                    table.ForeignKey(
                        name: "FK_CuentasPorCobrar_Sucursal_IdSucursal",
                        column: x => x.IdSucursal,
                        principalTable: "Sucursal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuentasPorCobrar_Usuarios_UsuarioAutorizoId_Usu",
                        column: x => x.UsuarioAutorizoId_Usu,
                        principalTable: "Usuarios",
                        principalColumn: "Id_Usu");
                    table.ForeignKey(
                        name: "FK_CuentasPorCobrar_Ventas_IdVenta",
                        column: x => x.IdVenta,
                        principalTable: "Ventas",
                        principalColumn: "IdVenta",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RemisionesInternas",
                columns: table => new
                {
                    IdRemision = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdVenta = table.Column<int>(type: "int", nullable: false),
                    IdCliente = table.Column<int>(type: "int", nullable: false),
                    IdSucursal = table.Column<int>(type: "int", nullable: false),
                    SucursalId = table.Column<int>(type: "int", nullable: true),
                    FechaRemision = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumeroRemision = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MontoTotal = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IdVentaFactura = table.Column<int>(type: "int", nullable: true),
                    VentaFacturaIdVenta = table.Column<int>(type: "int", nullable: true),
                    FechaFacturacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdMoneda = table.Column<int>(type: "int", nullable: true),
                    MonedaIdMoneda = table.Column<int>(type: "int", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(280)", maxLength: 280, nullable: true),
                    IdUsuarioEmitio = table.Column<int>(type: "int", nullable: true),
                    UsuarioEmitioId_Usu = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RemisionesInternas", x => x.IdRemision);
                    table.ForeignKey(
                        name: "FK_RemisionesInternas_Clientes_IdCliente",
                        column: x => x.IdCliente,
                        principalTable: "Clientes",
                        principalColumn: "IdCliente",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RemisionesInternas_Monedas_MonedaIdMoneda",
                        column: x => x.MonedaIdMoneda,
                        principalTable: "Monedas",
                        principalColumn: "IdMoneda");
                    table.ForeignKey(
                        name: "FK_RemisionesInternas_Sucursal_SucursalId",
                        column: x => x.SucursalId,
                        principalTable: "Sucursal",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RemisionesInternas_Usuarios_UsuarioEmitioId_Usu",
                        column: x => x.UsuarioEmitioId_Usu,
                        principalTable: "Usuarios",
                        principalColumn: "Id_Usu");
                    table.ForeignKey(
                        name: "FK_RemisionesInternas_Ventas_IdVenta",
                        column: x => x.IdVenta,
                        principalTable: "Ventas",
                        principalColumn: "IdVenta",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RemisionesInternas_Ventas_VentaFacturaIdVenta",
                        column: x => x.VentaFacturaIdVenta,
                        principalTable: "Ventas",
                        principalColumn: "IdVenta");
                });

            migrationBuilder.CreateTable(
                name: "CobrosCuotas",
                columns: table => new
                {
                    IdCobro = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCuentaPorCobrar = table.Column<int>(type: "int", nullable: false),
                    IdCliente = table.Column<int>(type: "int", nullable: false),
                    FechaCobro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MontoTotal = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IdMoneda = table.Column<int>(type: "int", nullable: true),
                    MonedaIdMoneda = table.Column<int>(type: "int", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(280)", maxLength: 280, nullable: true),
                    IdUsuario = table.Column<int>(type: "int", nullable: true),
                    IdCaja = table.Column<int>(type: "int", nullable: true),
                    NumeroRecibo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CobrosCuotas", x => x.IdCobro);
                    table.ForeignKey(
                        name: "FK_CobrosCuotas_Cajas_IdCaja",
                        column: x => x.IdCaja,
                        principalTable: "Cajas",
                        principalColumn: "id_caja",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CobrosCuotas_Clientes_IdCliente",
                        column: x => x.IdCliente,
                        principalTable: "Clientes",
                        principalColumn: "IdCliente",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CobrosCuotas_CuentasPorCobrar_IdCuentaPorCobrar",
                        column: x => x.IdCuentaPorCobrar,
                        principalTable: "CuentasPorCobrar",
                        principalColumn: "IdCuentaPorCobrar",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CobrosCuotas_Monedas_MonedaIdMoneda",
                        column: x => x.MonedaIdMoneda,
                        principalTable: "Monedas",
                        principalColumn: "IdMoneda");
                    table.ForeignKey(
                        name: "FK_CobrosCuotas_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id_Usu",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CuentasPorCobrarCuotas",
                columns: table => new
                {
                    IdCuota = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCuentaPorCobrar = table.Column<int>(type: "int", nullable: false),
                    NumeroCuota = table.Column<int>(type: "int", nullable: false),
                    MontoCuota = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    SaldoCuota = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(280)", maxLength: 280, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuentasPorCobrarCuotas", x => x.IdCuota);
                    table.ForeignKey(
                        name: "FK_CuentasPorCobrarCuotas_CuentasPorCobrar_IdCuentaPorCobrar",
                        column: x => x.IdCuentaPorCobrar,
                        principalTable: "CuentasPorCobrar",
                        principalColumn: "IdCuentaPorCobrar",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RemisionesInternasDetalles",
                columns: table => new
                {
                    IdRemisionDetalle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdRemision = table.Column<int>(type: "int", nullable: false),
                    IdProducto = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Gravado5 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Gravado10 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Exenta = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IVA5 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IVA10 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(280)", maxLength: 280, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RemisionesInternasDetalles", x => x.IdRemisionDetalle);
                    table.ForeignKey(
                        name: "FK_RemisionesInternasDetalles_Productos_IdProducto",
                        column: x => x.IdProducto,
                        principalTable: "Productos",
                        principalColumn: "IdProducto",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RemisionesInternasDetalles_RemisionesInternas_IdRemision",
                        column: x => x.IdRemision,
                        principalTable: "RemisionesInternas",
                        principalColumn: "IdRemision",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CobrosDetalles",
                columns: table => new
                {
                    IdCobroDetalle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCobro = table.Column<int>(type: "int", nullable: false),
                    IdCuota = table.Column<int>(type: "int", nullable: true),
                    MedioPago = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IdMoneda = table.Column<int>(type: "int", nullable: true),
                    MonedaIdMoneda = table.Column<int>(type: "int", nullable: true),
                    BancoTarjeta = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Ultimos4Tarjeta = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    NumeroAutorizacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NumeroCheque = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    BancoCheque = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NumeroTransferencia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(280)", maxLength: 280, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CobrosDetalles", x => x.IdCobroDetalle);
                    table.ForeignKey(
                        name: "FK_CobrosDetalles_CobrosCuotas_IdCobro",
                        column: x => x.IdCobro,
                        principalTable: "CobrosCuotas",
                        principalColumn: "IdCobro",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CobrosDetalles_CuentasPorCobrarCuotas_IdCuota",
                        column: x => x.IdCuota,
                        principalTable: "CuentasPorCobrarCuotas",
                        principalColumn: "IdCuota",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CobrosDetalles_Monedas_MonedaIdMoneda",
                        column: x => x.MonedaIdMoneda,
                        principalTable: "Monedas",
                        principalColumn: "IdMoneda");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CobrosCuotas_IdCaja",
                table: "CobrosCuotas",
                column: "IdCaja");

            migrationBuilder.CreateIndex(
                name: "IX_CobrosCuotas_IdCliente",
                table: "CobrosCuotas",
                column: "IdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_CobrosCuotas_IdCuentaPorCobrar",
                table: "CobrosCuotas",
                column: "IdCuentaPorCobrar");

            migrationBuilder.CreateIndex(
                name: "IX_CobrosCuotas_IdUsuario",
                table: "CobrosCuotas",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_CobrosCuotas_MonedaIdMoneda",
                table: "CobrosCuotas",
                column: "MonedaIdMoneda");

            migrationBuilder.CreateIndex(
                name: "IX_CobrosDetalles_IdCobro",
                table: "CobrosDetalles",
                column: "IdCobro");

            migrationBuilder.CreateIndex(
                name: "IX_CobrosDetalles_IdCuota",
                table: "CobrosDetalles",
                column: "IdCuota");

            migrationBuilder.CreateIndex(
                name: "IX_CobrosDetalles_MonedaIdMoneda",
                table: "CobrosDetalles",
                column: "MonedaIdMoneda");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorCobrar_IdCliente",
                table: "CuentasPorCobrar",
                column: "IdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorCobrar_IdSucursal",
                table: "CuentasPorCobrar",
                column: "IdSucursal");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorCobrar_IdVenta",
                table: "CuentasPorCobrar",
                column: "IdVenta");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorCobrar_MonedaIdMoneda",
                table: "CuentasPorCobrar",
                column: "MonedaIdMoneda");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorCobrar_UsuarioAutorizoId_Usu",
                table: "CuentasPorCobrar",
                column: "UsuarioAutorizoId_Usu");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorCobrarCuotas_IdCuentaPorCobrar_NumeroCuota",
                table: "CuentasPorCobrarCuotas",
                columns: new[] { "IdCuentaPorCobrar", "NumeroCuota" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RemisionesInternas_IdCliente",
                table: "RemisionesInternas",
                column: "IdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_RemisionesInternas_IdVenta",
                table: "RemisionesInternas",
                column: "IdVenta");

            migrationBuilder.CreateIndex(
                name: "IX_RemisionesInternas_MonedaIdMoneda",
                table: "RemisionesInternas",
                column: "MonedaIdMoneda");

            migrationBuilder.CreateIndex(
                name: "IX_RemisionesInternas_NumeroRemision",
                table: "RemisionesInternas",
                column: "NumeroRemision",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RemisionesInternas_SucursalId",
                table: "RemisionesInternas",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_RemisionesInternas_UsuarioEmitioId_Usu",
                table: "RemisionesInternas",
                column: "UsuarioEmitioId_Usu");

            migrationBuilder.CreateIndex(
                name: "IX_RemisionesInternas_VentaFacturaIdVenta",
                table: "RemisionesInternas",
                column: "VentaFacturaIdVenta");

            migrationBuilder.CreateIndex(
                name: "IX_RemisionesInternasDetalles_IdProducto",
                table: "RemisionesInternasDetalles",
                column: "IdProducto");

            migrationBuilder.CreateIndex(
                name: "IX_RemisionesInternasDetalles_IdRemision",
                table: "RemisionesInternasDetalles",
                column: "IdRemision");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CobrosDetalles");

            migrationBuilder.DropTable(
                name: "RemisionesInternasDetalles");

            migrationBuilder.DropTable(
                name: "CobrosCuotas");

            migrationBuilder.DropTable(
                name: "CuentasPorCobrarCuotas");

            migrationBuilder.DropTable(
                name: "RemisionesInternas");

            migrationBuilder.DropTable(
                name: "CuentasPorCobrar");
        }
    }
}
