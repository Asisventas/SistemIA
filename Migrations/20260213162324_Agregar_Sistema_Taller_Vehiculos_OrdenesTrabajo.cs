using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Sistema_Taller_Vehiculos_OrdenesTrabajo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SuscripcionesClientes",
                columns: table => new
                {
                    IdSuscripcion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdSucursal = table.Column<int>(type: "int", nullable: false),
                    IdCliente = table.Column<int>(type: "int", nullable: false),
                    IdProducto = table.Column<int>(type: "int", nullable: false),
                    IdCaja = table.Column<int>(type: "int", nullable: true),
                    MontoFacturar = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DiaFacturacion = table.Column<int>(type: "int", nullable: false),
                    TipoPeriodo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaProximaFactura = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaUltimaFactura = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FacturacionActiva = table.Column<bool>(type: "bit", nullable: false),
                    EnviarCorreoFactura = table.Column<bool>(type: "bit", nullable: false),
                    EnviarCorreoRecibo = table.Column<bool>(type: "bit", nullable: false),
                    DescripcionFactura = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdUsuarioCreacion = table.Column<int>(type: "int", nullable: true),
                    IdUsuarioModificacion = table.Column<int>(type: "int", nullable: true),
                    TotalFacturasGeneradas = table.Column<int>(type: "int", nullable: false),
                    TotalFacturasCobradas = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuscripcionesClientes", x => x.IdSuscripcion);
                    table.ForeignKey(
                        name: "FK_SuscripcionesClientes_Cajas_IdCaja",
                        column: x => x.IdCaja,
                        principalTable: "Cajas",
                        principalColumn: "id_caja",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SuscripcionesClientes_Clientes_IdCliente",
                        column: x => x.IdCliente,
                        principalTable: "Clientes",
                        principalColumn: "IdCliente",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SuscripcionesClientes_Productos_IdProducto",
                        column: x => x.IdProducto,
                        principalTable: "Productos",
                        principalColumn: "IdProducto",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SuscripcionesClientes_Sucursal_IdSucursal",
                        column: x => x.IdSucursal,
                        principalTable: "Sucursal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Vehiculos",
                columns: table => new
                {
                    IdVehiculo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCliente = table.Column<int>(type: "int", nullable: true),
                    IdSucursal = table.Column<int>(type: "int", nullable: false),
                    Matricula = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Marca = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Modelo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Anio = table.Column<int>(type: "int", nullable: true),
                    Color = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    NumeroChasis = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NumeroMotor = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TipoCombustible = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TipoVehiculo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    KilometrajeActual = table.Column<int>(type: "int", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehiculos", x => x.IdVehiculo);
                    table.ForeignKey(
                        name: "FK_Vehiculos_Clientes_IdCliente",
                        column: x => x.IdCliente,
                        principalTable: "Clientes",
                        principalColumn: "IdCliente",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Vehiculos_Sucursal_IdSucursal",
                        column: x => x.IdSucursal,
                        principalTable: "Sucursal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FacturasAutomaticas",
                columns: table => new
                {
                    IdFacturaAutomatica = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdSuscripcion = table.Column<int>(type: "int", nullable: false),
                    IdSucursal = table.Column<int>(type: "int", nullable: false),
                    IdCliente = table.Column<int>(type: "int", nullable: false),
                    IdVenta = table.Column<int>(type: "int", nullable: true),
                    PeriodoFacturado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaInicioPeriodo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFinPeriodo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MontoFacturado = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    EstadoFactura = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MensajeError = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IntentosGeneracion = table.Column<int>(type: "int", nullable: false),
                    EstadoCorreoFactura = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaEnvioCorreoFactura = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorCorreoFactura = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IntentosEnvioCorreo = table.Column<int>(type: "int", nullable: false),
                    EstadoCobro = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaCobro = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MontoCobrado = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    EstadoCorreoRecibo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaEnvioCorreoRecibo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorCorreoRecibo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaProgramada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaGeneracion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumeroFactura = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CDC = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    EstadoSifen = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacturasAutomaticas", x => x.IdFacturaAutomatica);
                    table.ForeignKey(
                        name: "FK_FacturasAutomaticas_Clientes_IdCliente",
                        column: x => x.IdCliente,
                        principalTable: "Clientes",
                        principalColumn: "IdCliente",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FacturasAutomaticas_Sucursal_IdSucursal",
                        column: x => x.IdSucursal,
                        principalTable: "Sucursal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FacturasAutomaticas_SuscripcionesClientes_IdSuscripcion",
                        column: x => x.IdSuscripcion,
                        principalTable: "SuscripcionesClientes",
                        principalColumn: "IdSuscripcion",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FacturasAutomaticas_Ventas_IdVenta",
                        column: x => x.IdVenta,
                        principalTable: "Ventas",
                        principalColumn: "IdVenta",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "OrdenesTrabajo",
                columns: table => new
                {
                    IdOrdenTrabajo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdSucursal = table.Column<int>(type: "int", nullable: false),
                    IdCaja = table.Column<int>(type: "int", nullable: true),
                    Turno = table.Column<int>(type: "int", nullable: true),
                    FechaCaja = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdMesa = table.Column<int>(type: "int", nullable: true),
                    IdVehiculo = table.Column<int>(type: "int", nullable: false),
                    IdCliente = table.Column<int>(type: "int", nullable: true),
                    NumeroOrden = table.Column<int>(type: "int", nullable: false),
                    AnioOrden = table.Column<int>(type: "int", nullable: false),
                    CodigoOrden = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    KilometrajeIngreso = table.Column<int>(type: "int", nullable: true),
                    NivelCombustible = table.Column<int>(type: "int", nullable: true),
                    EstadoIngreso = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FotosIngreso = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MotivoConsulta = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Diagnostico = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TrabajosARealizar = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TrabajosRealizados = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Recomendaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaIngreso = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaInicioTrabajo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaFinTrabajo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaEntrega = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaPrometida = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdMecanico = table.Column<int>(type: "int", nullable: true),
                    NombreMecanico = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MecanicosAdicionales = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdUsuarioCreacion = table.Column<int>(type: "int", nullable: true),
                    IdUsuarioCierre = table.Column<int>(type: "int", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Prioridad = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TipoServicio = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TotalManoObra = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalRepuestos = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalDescuento = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalIva10 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalIva5 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalExenta = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    MontoPagado = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DiasGarantia = table.Column<int>(type: "int", nullable: true),
                    FechaVencimientoGarantia = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CondicionesGarantia = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IdPedido = table.Column<int>(type: "int", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenesTrabajo", x => x.IdOrdenTrabajo);
                    table.ForeignKey(
                        name: "FK_OrdenesTrabajo_Cajas_IdCaja",
                        column: x => x.IdCaja,
                        principalTable: "Cajas",
                        principalColumn: "id_caja",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OrdenesTrabajo_Clientes_IdCliente",
                        column: x => x.IdCliente,
                        principalTable: "Clientes",
                        principalColumn: "IdCliente",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OrdenesTrabajo_Mesas_IdMesa",
                        column: x => x.IdMesa,
                        principalTable: "Mesas",
                        principalColumn: "IdMesa",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OrdenesTrabajo_Pedidos_IdPedido",
                        column: x => x.IdPedido,
                        principalTable: "Pedidos",
                        principalColumn: "IdPedido",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OrdenesTrabajo_Sucursal_IdSucursal",
                        column: x => x.IdSucursal,
                        principalTable: "Sucursal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenesTrabajo_Usuarios_IdMecanico",
                        column: x => x.IdMecanico,
                        principalTable: "Usuarios",
                        principalColumn: "Id_Usu",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OrdenesTrabajo_Vehiculos_IdVehiculo",
                        column: x => x.IdVehiculo,
                        principalTable: "Vehiculos",
                        principalColumn: "IdVehiculo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrdenTrabajoDetalles",
                columns: table => new
                {
                    IdOrdenTrabajoDetalle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdOrdenTrabajo = table.Column<int>(type: "int", nullable: false),
                    TipoLinea = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IdProducto = table.Column<int>(type: "int", nullable: true),
                    CodigoProducto = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CostoUnitario = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    PorcentajeDescuento = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    MontoDescuento = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TipoIva = table.Column<int>(type: "int", nullable: false),
                    MontoIva10 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    MontoIva5 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    MontoExenta = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    HorasTrabajo = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    IdMecanico = table.Column<int>(type: "int", nullable: true),
                    NombreMecanico = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenTrabajoDetalles", x => x.IdOrdenTrabajoDetalle);
                    table.ForeignKey(
                        name: "FK_OrdenTrabajoDetalles_OrdenesTrabajo_IdOrdenTrabajo",
                        column: x => x.IdOrdenTrabajo,
                        principalTable: "OrdenesTrabajo",
                        principalColumn: "IdOrdenTrabajo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrdenTrabajoDetalles_Productos_IdProducto",
                        column: x => x.IdProducto,
                        principalTable: "Productos",
                        principalColumn: "IdProducto",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FacturasAutomaticas_EstadoCobro",
                table: "FacturasAutomaticas",
                column: "EstadoCobro");

            migrationBuilder.CreateIndex(
                name: "IX_FacturasAutomaticas_EstadoFactura",
                table: "FacturasAutomaticas",
                column: "EstadoFactura");

            migrationBuilder.CreateIndex(
                name: "IX_FacturasAutomaticas_FechaGeneracion",
                table: "FacturasAutomaticas",
                column: "FechaGeneracion");

            migrationBuilder.CreateIndex(
                name: "IX_FacturasAutomaticas_FechaProgramada",
                table: "FacturasAutomaticas",
                column: "FechaProgramada");

            migrationBuilder.CreateIndex(
                name: "IX_FacturasAutomaticas_IdCliente",
                table: "FacturasAutomaticas",
                column: "IdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_FacturasAutomaticas_IdSucursal",
                table: "FacturasAutomaticas",
                column: "IdSucursal");

            migrationBuilder.CreateIndex(
                name: "IX_FacturasAutomaticas_IdSuscripcion",
                table: "FacturasAutomaticas",
                column: "IdSuscripcion");

            migrationBuilder.CreateIndex(
                name: "IX_FacturasAutomaticas_IdVenta",
                table: "FacturasAutomaticas",
                column: "IdVenta");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesTrabajo_AnioOrden_NumeroOrden",
                table: "OrdenesTrabajo",
                columns: new[] { "AnioOrden", "NumeroOrden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesTrabajo_Estado",
                table: "OrdenesTrabajo",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesTrabajo_FechaCreacion",
                table: "OrdenesTrabajo",
                column: "FechaCreacion");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesTrabajo_IdCaja",
                table: "OrdenesTrabajo",
                column: "IdCaja");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesTrabajo_IdCliente",
                table: "OrdenesTrabajo",
                column: "IdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesTrabajo_IdMecanico",
                table: "OrdenesTrabajo",
                column: "IdMecanico");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesTrabajo_IdMesa",
                table: "OrdenesTrabajo",
                column: "IdMesa");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesTrabajo_IdPedido",
                table: "OrdenesTrabajo",
                column: "IdPedido");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesTrabajo_IdSucursal",
                table: "OrdenesTrabajo",
                column: "IdSucursal");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesTrabajo_IdVehiculo",
                table: "OrdenesTrabajo",
                column: "IdVehiculo");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenTrabajoDetalles_Estado",
                table: "OrdenTrabajoDetalles",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenTrabajoDetalles_IdOrdenTrabajo",
                table: "OrdenTrabajoDetalles",
                column: "IdOrdenTrabajo");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenTrabajoDetalles_IdProducto",
                table: "OrdenTrabajoDetalles",
                column: "IdProducto");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenTrabajoDetalles_TipoLinea",
                table: "OrdenTrabajoDetalles",
                column: "TipoLinea");

            migrationBuilder.CreateIndex(
                name: "IX_SuscripcionesClientes_Estado",
                table: "SuscripcionesClientes",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_SuscripcionesClientes_FechaProximaFactura",
                table: "SuscripcionesClientes",
                column: "FechaProximaFactura");

            migrationBuilder.CreateIndex(
                name: "IX_SuscripcionesClientes_IdCaja",
                table: "SuscripcionesClientes",
                column: "IdCaja");

            migrationBuilder.CreateIndex(
                name: "IX_SuscripcionesClientes_IdCliente",
                table: "SuscripcionesClientes",
                column: "IdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_SuscripcionesClientes_IdProducto",
                table: "SuscripcionesClientes",
                column: "IdProducto");

            migrationBuilder.CreateIndex(
                name: "IX_SuscripcionesClientes_IdSucursal",
                table: "SuscripcionesClientes",
                column: "IdSucursal");

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculos_IdCliente",
                table: "Vehiculos",
                column: "IdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculos_IdSucursal_Matricula",
                table: "Vehiculos",
                columns: new[] { "IdSucursal", "Matricula" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculos_Matricula",
                table: "Vehiculos",
                column: "Matricula");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FacturasAutomaticas");

            migrationBuilder.DropTable(
                name: "OrdenTrabajoDetalles");

            migrationBuilder.DropTable(
                name: "SuscripcionesClientes");

            migrationBuilder.DropTable(
                name: "OrdenesTrabajo");

            migrationBuilder.DropTable(
                name: "Vehiculos");
        }
    }
}
