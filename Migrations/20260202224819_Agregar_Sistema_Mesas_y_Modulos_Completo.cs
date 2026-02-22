using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Sistema_Mesas_y_Modulos_Completo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanchasModoActivo",
                table: "ConfiguracionSistema",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ReservasHabilitadas",
                table: "ConfiguracionSistema",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ReservasRecordatorioMinutos",
                table: "ConfiguracionSistema",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RestauranteCargoServicio",
                table: "ConfiguracionSistema",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RestauranteImpresoraBarra",
                table: "ConfiguracionSistema",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RestauranteImpresoraCocina",
                table: "ConfiguracionSistema",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RestauranteImprimirComandaAuto",
                table: "ConfiguracionSistema",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RestauranteModoActivo",
                table: "ConfiguracionSistema",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RestauranteMostrarTiempo",
                table: "ConfiguracionSistema",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RestaurantePermitirDivisionCuenta",
                table: "ConfiguracionSistema",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RestaurantePermitirPagosParciales",
                table: "ConfiguracionSistema",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Mesas",
                columns: table => new
                {
                    IdMesa = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdSucursal = table.Column<int>(type: "int", nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Zona = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Capacidad = table.Column<int>(type: "int", nullable: false),
                    PosicionX = table.Column<int>(type: "int", nullable: false),
                    PosicionY = table.Column<int>(type: "int", nullable: false),
                    Ancho = table.Column<int>(type: "int", nullable: false),
                    Alto = table.Column<int>(type: "int", nullable: false),
                    Forma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ColorLibre = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ColorOcupada = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ColorReservada = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ImagenUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Icono = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CobraPorTiempo = table.Column<bool>(type: "bit", nullable: false),
                    TarifaPorHora = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    TiempoMinimoMinutos = table.Column<int>(type: "int", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mesas", x => x.IdMesa);
                    table.ForeignKey(
                        name: "FK_Mesas_Sucursal_IdSucursal",
                        column: x => x.IdSucursal,
                        principalTable: "Sucursal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reservas",
                columns: table => new
                {
                    IdReserva = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdSucursal = table.Column<int>(type: "int", nullable: false),
                    IdMesa = table.Column<int>(type: "int", nullable: false),
                    NumeroReserva = table.Column<int>(type: "int", nullable: false),
                    NombreCliente = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IdCliente = table.Column<int>(type: "int", nullable: true),
                    FechaReserva = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "time", nullable: false),
                    HoraFin = table.Column<TimeSpan>(type: "time", nullable: true),
                    DuracionMinutos = table.Column<int>(type: "int", nullable: true),
                    CantidadPersonas = table.Column<int>(type: "int", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RecordatorioEnviado = table.Column<bool>(type: "bit", nullable: false),
                    FechaRecordatorio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RequiereSena = table.Column<bool>(type: "bit", nullable: false),
                    MontoSena = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    SenaPagada = table.Column<bool>(type: "bit", nullable: false),
                    FechaPagoSena = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TarifaPorHora = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    TotalEstimado = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    IdPedido = table.Column<int>(type: "int", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdUsuarioCreacion = table.Column<int>(type: "int", nullable: true),
                    IdUsuarioModificacion = table.Column<int>(type: "int", nullable: true),
                    MotivoCancelacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservas", x => x.IdReserva);
                    table.ForeignKey(
                        name: "FK_Reservas_Clientes_IdCliente",
                        column: x => x.IdCliente,
                        principalTable: "Clientes",
                        principalColumn: "IdCliente",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservas_Mesas_IdMesa",
                        column: x => x.IdMesa,
                        principalTable: "Mesas",
                        principalColumn: "IdMesa",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservas_Sucursal_IdSucursal",
                        column: x => x.IdSucursal,
                        principalTable: "Sucursal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pedidos",
                columns: table => new
                {
                    IdPedido = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdSucursal = table.Column<int>(type: "int", nullable: false),
                    IdCaja = table.Column<int>(type: "int", nullable: true),
                    Turno = table.Column<int>(type: "int", nullable: true),
                    FechaCaja = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdMesa = table.Column<int>(type: "int", nullable: false),
                    NumeroPedido = table.Column<int>(type: "int", nullable: false),
                    Comensales = table.Column<int>(type: "int", nullable: false),
                    NombreCliente = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IdCliente = table.Column<int>(type: "int", nullable: true),
                    FechaApertura = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HoraInicio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HoraFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Subtotal = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalDescuento = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CargoServicio = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PorcentajeServicio = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    CargoPorTiempo = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalIva10 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalIva5 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalExenta = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    MontoPagado = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ComandaImpresa = table.Column<bool>(type: "bit", nullable: false),
                    ImpresionesComanda = table.Column<int>(type: "int", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IdReserva = table.Column<int>(type: "int", nullable: true),
                    IdUsuario = table.Column<int>(type: "int", nullable: true),
                    NombreMesero = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IdUsuarioApertura = table.Column<int>(type: "int", nullable: true),
                    IdUsuarioCierre = table.Column<int>(type: "int", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedidos", x => x.IdPedido);
                    table.ForeignKey(
                        name: "FK_Pedidos_Cajas_IdCaja",
                        column: x => x.IdCaja,
                        principalTable: "Cajas",
                        principalColumn: "id_caja",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pedidos_Clientes_IdCliente",
                        column: x => x.IdCliente,
                        principalTable: "Clientes",
                        principalColumn: "IdCliente",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pedidos_Mesas_IdMesa",
                        column: x => x.IdMesa,
                        principalTable: "Mesas",
                        principalColumn: "IdMesa",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pedidos_Reservas_IdReserva",
                        column: x => x.IdReserva,
                        principalTable: "Reservas",
                        principalColumn: "IdReserva",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Pedidos_Sucursal_IdSucursal",
                        column: x => x.IdSucursal,
                        principalTable: "Sucursal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pedidos_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id_Usu",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PedidoDetalles",
                columns: table => new
                {
                    IdPedidoDetalle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdPedido = table.Column<int>(type: "int", nullable: false),
                    IdProducto = table.Column<int>(type: "int", nullable: false),
                    CodigoProducto = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CantidadEntregada = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Importe = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PorcentajeDescuento = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Descuento = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IdTipoIva = table.Column<int>(type: "int", nullable: true),
                    IVA10 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IVA5 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Exenta = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Grabado10 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Grabado5 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EnviadoCocina = table.Column<bool>(type: "bit", nullable: false),
                    FechaEnvioCocina = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaEntrega = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NumeroComanda = table.Column<int>(type: "int", nullable: true),
                    Modificadores = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NotasCocina = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NumeroComensal = table.Column<int>(type: "int", nullable: true),
                    Facturado = table.Column<bool>(type: "bit", nullable: false),
                    IdVenta = table.Column<int>(type: "int", nullable: true),
                    IdUsuario = table.Column<int>(type: "int", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PedidoDetalles", x => x.IdPedidoDetalle);
                    table.ForeignKey(
                        name: "FK_PedidoDetalles_Pedidos_IdPedido",
                        column: x => x.IdPedido,
                        principalTable: "Pedidos",
                        principalColumn: "IdPedido",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PedidoDetalles_Productos_IdProducto",
                        column: x => x.IdProducto,
                        principalTable: "Productos",
                        principalColumn: "IdProducto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PedidoPagos",
                columns: table => new
                {
                    IdPedidoPago = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdPedido = table.Column<int>(type: "int", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    FormaPago = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Referencia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MontoRecibido = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Vuelto = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Propina = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    NumeroComensal = table.Column<int>(type: "int", nullable: true),
                    NombrePagador = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DetallesIncluidos = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Facturado = table.Column<bool>(type: "bit", nullable: false),
                    IdVenta = table.Column<int>(type: "int", nullable: true),
                    IdCliente = table.Column<int>(type: "int", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdUsuario = table.Column<int>(type: "int", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PedidoPagos", x => x.IdPedidoPago);
                    table.ForeignKey(
                        name: "FK_PedidoPagos_Clientes_IdCliente",
                        column: x => x.IdCliente,
                        principalTable: "Clientes",
                        principalColumn: "IdCliente",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PedidoPagos_Pedidos_IdPedido",
                        column: x => x.IdPedido,
                        principalTable: "Pedidos",
                        principalColumn: "IdPedido",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PedidoPagos_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id_Usu",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PedidoPagos_Ventas_IdVenta",
                        column: x => x.IdVenta,
                        principalTable: "Ventas",
                        principalColumn: "IdVenta",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Mesas_IdSucursal",
                table: "Mesas",
                column: "IdSucursal");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoDetalles_IdPedido",
                table: "PedidoDetalles",
                column: "IdPedido");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoDetalles_IdProducto",
                table: "PedidoDetalles",
                column: "IdProducto");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoPagos_IdCliente",
                table: "PedidoPagos",
                column: "IdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoPagos_IdPedido",
                table: "PedidoPagos",
                column: "IdPedido");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoPagos_IdUsuario",
                table: "PedidoPagos",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoPagos_IdVenta",
                table: "PedidoPagos",
                column: "IdVenta");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_IdCaja",
                table: "Pedidos",
                column: "IdCaja");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_IdCliente",
                table: "Pedidos",
                column: "IdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_IdMesa",
                table: "Pedidos",
                column: "IdMesa");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_IdReserva",
                table: "Pedidos",
                column: "IdReserva",
                unique: true,
                filter: "[IdReserva] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_IdSucursal",
                table: "Pedidos",
                column: "IdSucursal");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_IdUsuario",
                table: "Pedidos",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_IdCliente",
                table: "Reservas",
                column: "IdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_IdMesa",
                table: "Reservas",
                column: "IdMesa");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_IdSucursal",
                table: "Reservas",
                column: "IdSucursal");

            // ========================================================
            // Agregar módulos al sistema de permisos
            // ========================================================
            migrationBuilder.Sql(@"
                DECLARE @IdModuloPadre INT;

                -- 1. Crear módulo padre 'Mesas / Canchas'
                IF NOT EXISTS (SELECT 1 FROM Modulos WHERE Nombre = 'Mesas / Canchas')
                BEGIN
                    INSERT INTO Modulos (Nombre, Descripcion, RutaPagina, Icono, IdModuloPadre, Orden, Activo, FechaCreacion)
                    VALUES ('Mesas / Canchas', 'Gestión de mesas de restaurante y canchas deportivas', NULL, 'bi-grid-3x3-gap', NULL, 15, 1, GETDATE());
                    SET @IdModuloPadre = SCOPE_IDENTITY();
                END
                ELSE
                BEGIN
                    SELECT @IdModuloPadre = IdModulo FROM Modulos WHERE Nombre = 'Mesas / Canchas';
                END

                -- 2. Panel de Mesas
                IF NOT EXISTS (SELECT 1 FROM Modulos WHERE RutaPagina = '/mesas/panel')
                BEGIN
                    INSERT INTO Modulos (Nombre, Descripcion, RutaPagina, Icono, IdModuloPadre, Orden, Activo, FechaCreacion)
                    VALUES ('Panel de Mesas', 'Vista de panel visual de mesas y su estado', '/mesas/panel', 'bi-grid-3x3-gap-fill', @IdModuloPadre, 1, 1, GETDATE());
                    DECLARE @IdPanelMesas INT = SCOPE_IDENTITY();
                    INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion)
                    VALUES (1, @IdPanelMesas, 1, 1, GETDATE()), (1, @IdPanelMesas, 2, 1, GETDATE()), (1, @IdPanelMesas, 3, 1, GETDATE()), (1, @IdPanelMesas, 4, 1, GETDATE());
                END

                -- 3. Administrar Mesas
                IF NOT EXISTS (SELECT 1 FROM Modulos WHERE RutaPagina = '/mesas')
                BEGIN
                    INSERT INTO Modulos (Nombre, Descripcion, RutaPagina, Icono, IdModuloPadre, Orden, Activo, FechaCreacion)
                    VALUES ('Administrar Mesas', 'CRUD de mesas y canchas', '/mesas', 'bi-table', @IdModuloPadre, 2, 1, GETDATE());
                    DECLARE @IdAdminMesas INT = SCOPE_IDENTITY();
                    INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion)
                    VALUES (1, @IdAdminMesas, 1, 1, GETDATE()), (1, @IdAdminMesas, 2, 1, GETDATE()), (1, @IdAdminMesas, 3, 1, GETDATE()), (1, @IdAdminMesas, 4, 1, GETDATE());
                END

                -- 4. Pedido de Mesa
                IF NOT EXISTS (SELECT 1 FROM Modulos WHERE RutaPagina = '/mesas/pedido')
                BEGIN
                    INSERT INTO Modulos (Nombre, Descripcion, RutaPagina, Icono, IdModuloPadre, Orden, Activo, FechaCreacion)
                    VALUES ('Pedido de Mesa', 'Toma de pedidos en mesas', '/mesas/pedido', 'bi-receipt', @IdModuloPadre, 3, 1, GETDATE());
                    DECLARE @IdPedidoMesa INT = SCOPE_IDENTITY();
                    INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion)
                    VALUES (1, @IdPedidoMesa, 1, 1, GETDATE()), (1, @IdPedidoMesa, 2, 1, GETDATE()), (1, @IdPedidoMesa, 3, 1, GETDATE()), (1, @IdPedidoMesa, 4, 1, GETDATE());
                END

                -- 5. Explorar Pedidos
                IF NOT EXISTS (SELECT 1 FROM Modulos WHERE RutaPagina = '/pedidos/explorar')
                BEGIN
                    INSERT INTO Modulos (Nombre, Descripcion, RutaPagina, Icono, IdModuloPadre, Orden, Activo, FechaCreacion)
                    VALUES ('Explorar Pedidos', 'Historial y búsqueda de pedidos', '/pedidos/explorar', 'bi-search', @IdModuloPadre, 4, 1, GETDATE());
                    DECLARE @IdExplorarPedidos INT = SCOPE_IDENTITY();
                    INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion)
                    VALUES (1, @IdExplorarPedidos, 1, 1, GETDATE()), (1, @IdExplorarPedidos, 2, 1, GETDATE()), (1, @IdExplorarPedidos, 3, 1, GETDATE()), (1, @IdExplorarPedidos, 4, 1, GETDATE());
                END

                -- 6. Reservas
                IF NOT EXISTS (SELECT 1 FROM Modulos WHERE RutaPagina = '/reservas')
                BEGIN
                    INSERT INTO Modulos (Nombre, Descripcion, RutaPagina, Icono, IdModuloPadre, Orden, Activo, FechaCreacion)
                    VALUES ('Reservas', 'Gestión de reservas de mesas y canchas', '/reservas', 'bi-calendar-check', @IdModuloPadre, 5, 1, GETDATE());
                    DECLARE @IdReservas INT = SCOPE_IDENTITY();
                    INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion)
                    VALUES (1, @IdReservas, 1, 1, GETDATE()), (1, @IdReservas, 2, 1, GETDATE()), (1, @IdReservas, 3, 1, GETDATE()), (1, @IdReservas, 4, 1, GETDATE());
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PedidoDetalles");

            migrationBuilder.DropTable(
                name: "PedidoPagos");

            migrationBuilder.DropTable(
                name: "Pedidos");

            migrationBuilder.DropTable(
                name: "Reservas");

            migrationBuilder.DropTable(
                name: "Mesas");

            migrationBuilder.DropColumn(
                name: "CanchasModoActivo",
                table: "ConfiguracionSistema");

            migrationBuilder.DropColumn(
                name: "ReservasHabilitadas",
                table: "ConfiguracionSistema");

            migrationBuilder.DropColumn(
                name: "ReservasRecordatorioMinutos",
                table: "ConfiguracionSistema");

            migrationBuilder.DropColumn(
                name: "RestauranteCargoServicio",
                table: "ConfiguracionSistema");

            migrationBuilder.DropColumn(
                name: "RestauranteImpresoraBarra",
                table: "ConfiguracionSistema");

            migrationBuilder.DropColumn(
                name: "RestauranteImpresoraCocina",
                table: "ConfiguracionSistema");

            migrationBuilder.DropColumn(
                name: "RestauranteImprimirComandaAuto",
                table: "ConfiguracionSistema");

            migrationBuilder.DropColumn(
                name: "RestauranteModoActivo",
                table: "ConfiguracionSistema");

            migrationBuilder.DropColumn(
                name: "RestauranteMostrarTiempo",
                table: "ConfiguracionSistema");

            migrationBuilder.DropColumn(
                name: "RestaurantePermitirDivisionCuenta",
                table: "ConfiguracionSistema");

            migrationBuilder.DropColumn(
                name: "RestaurantePermitirPagosParciales",
                table: "ConfiguracionSistema");
        }
    }
}
