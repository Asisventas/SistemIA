using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Modulo_Gimnasio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GimnasioDiasGracia",
                table: "ConfiguracionSistema",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "GimnasioHabilitarVoz",
                table: "ConfiguracionSistema",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "GimnasioHorasAutoSalida",
                table: "ConfiguracionSistema",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "GimnasioMensajeBienvenida",
                table: "ConfiguracionSistema",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GimnasioMensajeDespedida",
                table: "ConfiguracionSistema",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GimnasioMensajeVencido",
                table: "ConfiguracionSistema",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GimnasioMensajeVencimientoProximo",
                table: "ConfiguracionSistema",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "GimnasioModoActivo",
                table: "ConfiguracionSistema",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "GimnasioPermitirAccesoDiasGracia",
                table: "ConfiguracionSistema",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "GimnasioReconocimientoFacialActivo",
                table: "ConfiguracionSistema",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "GimnasioUmbralCoincidenciaFacial",
                table: "ConfiguracionSistema",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CodigoGimnasio",
                table: "Clientes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CondicionesMedicas",
                table: "Clientes",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactoEmergencia",
                table: "Clientes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "EmbeddingFacialGimnasio",
                table: "Clientes",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EsMiembroGimnasio",
                table: "Clientes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAltaGimnasio",
                table: "Clientes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCaptuaFacial",
                table: "Clientes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaUltimaVisitaGimnasio",
                table: "Clientes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "FotoGimnasio",
                table: "Clientes",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MensajeBienvenidaPersonalizado",
                table: "Clientes",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObjetivoFitness",
                table: "Clientes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TelefonoEmergencia",
                table: "Clientes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalVisitasGimnasio",
                table: "Clientes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PlanesMembresia",
                columns: table => new
                {
                    IdPlan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdSucursal = table.Column<int>(type: "int", nullable: true),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TipoPeriodo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DuracionDias = table.Column<int>(type: "int", nullable: false),
                    PrecioInscripcion = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IdMoneda = table.Column<int>(type: "int", nullable: true),
                    ClasesIncluidasPorPeriodo = table.Column<int>(type: "int", nullable: false),
                    IncluyePersonalTrainer = table.Column<bool>(type: "bit", nullable: false),
                    SesionesPTIncluidas = table.Column<int>(type: "int", nullable: true),
                    AccesoTodasAreas = table.Column<bool>(type: "bit", nullable: false),
                    AreasIncluidas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PermiteCongelar = table.Column<bool>(type: "bit", nullable: false),
                    DiasCongelamientoMaximo = table.Column<int>(type: "int", nullable: false),
                    DiasGracia = table.Column<int>(type: "int", nullable: false),
                    RenovacionAutomatica = table.Column<bool>(type: "bit", nullable: false),
                    DiasRecordatorioVencimiento = table.Column<int>(type: "int", nullable: false),
                    HorarioAcceso = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DiasAcceso = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    ColorHex = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanesMembresia", x => x.IdPlan);
                    table.ForeignKey(
                        name: "FK_PlanesMembresia_Monedas_IdMoneda",
                        column: x => x.IdMoneda,
                        principalTable: "Monedas",
                        principalColumn: "IdMoneda");
                    table.ForeignKey(
                        name: "FK_PlanesMembresia_Sucursal_IdSucursal",
                        column: x => x.IdSucursal,
                        principalTable: "Sucursal",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MembresiasClientes",
                columns: table => new
                {
                    IdMembresia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdSucursal = table.Column<int>(type: "int", nullable: false),
                    IdCaja = table.Column<int>(type: "int", nullable: true),
                    Turno = table.Column<int>(type: "int", nullable: true),
                    FechaCaja = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdCliente = table.Column<int>(type: "int", nullable: false),
                    IdPlan = table.Column<int>(type: "int", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstaCongelada = table.Column<bool>(type: "bit", nullable: false),
                    FechaCongelamiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DiasCongeladosTotales = table.Column<int>(type: "int", nullable: false),
                    MotivoCongelamiento = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EstadoPago = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MontoTotal = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    MontoPagado = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    SaldoPendiente = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IdVenta = table.Column<int>(type: "int", nullable: true),
                    NumeroComprobante = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EsRenovacion = table.Column<bool>(type: "bit", nullable: false),
                    IdMembresiaAnterior = table.Column<int>(type: "int", nullable: true),
                    RenovacionAutomatica = table.Column<bool>(type: "bit", nullable: false),
                    RecordatorioEnviado = table.Column<bool>(type: "bit", nullable: false),
                    FechaRecordatorio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CantidadVisitas = table.Column<int>(type: "int", nullable: false),
                    FechaUltimaVisita = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClasesUtilizadas = table.Column<int>(type: "int", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MotivoCancelacion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IdUsuario = table.Column<int>(type: "int", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembresiasClientes", x => x.IdMembresia);
                    table.ForeignKey(
                        name: "FK_MembresiasClientes_Cajas_IdCaja",
                        column: x => x.IdCaja,
                        principalTable: "Cajas",
                        principalColumn: "id_caja");
                    table.ForeignKey(
                        name: "FK_MembresiasClientes_Clientes_IdCliente",
                        column: x => x.IdCliente,
                        principalTable: "Clientes",
                        principalColumn: "IdCliente",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MembresiasClientes_PlanesMembresia_IdPlan",
                        column: x => x.IdPlan,
                        principalTable: "PlanesMembresia",
                        principalColumn: "IdPlan",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MembresiasClientes_Sucursal_IdSucursal",
                        column: x => x.IdSucursal,
                        principalTable: "Sucursal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MembresiasClientes_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id_Usu");
                    table.ForeignKey(
                        name: "FK_MembresiasClientes_Ventas_IdVenta",
                        column: x => x.IdVenta,
                        principalTable: "Ventas",
                        principalColumn: "IdVenta");
                });

            migrationBuilder.CreateTable(
                name: "AccesosGimnasio",
                columns: table => new
                {
                    IdAcceso = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdSucursal = table.Column<int>(type: "int", nullable: false),
                    IdCaja = table.Column<int>(type: "int", nullable: true),
                    Turno = table.Column<int>(type: "int", nullable: true),
                    FechaCaja = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdCliente = table.Column<int>(type: "int", nullable: false),
                    IdMembresia = table.Column<int>(type: "int", nullable: true),
                    TipoAcceso = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdAccesoEntrada = table.Column<int>(type: "int", nullable: true),
                    MetodoVerificacion = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PorcentajeCoincidencia = table.Column<float>(type: "real", nullable: true),
                    FotoCaptura = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    EstadoMembresiaAlAcceso = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DiasRestantes = table.Column<int>(type: "int", nullable: true),
                    AccesoPermitido = table.Column<bool>(type: "bit", nullable: false),
                    MotivoDenegacion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AccesoPorExcepcion = table.Column<bool>(type: "bit", nullable: false),
                    TipoExcepcion = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    AreaAcceso = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PuntoAcceso = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MensajeMostrado = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    MensajeVozReproducido = table.Column<bool>(type: "bit", nullable: false),
                    DuracionVisitaMinutos = table.Column<int>(type: "int", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IdUsuario = table.Column<int>(type: "int", nullable: true),
                    RegistroAutomatico = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccesosGimnasio", x => x.IdAcceso);
                    table.ForeignKey(
                        name: "FK_AccesosGimnasio_Cajas_IdCaja",
                        column: x => x.IdCaja,
                        principalTable: "Cajas",
                        principalColumn: "id_caja");
                    table.ForeignKey(
                        name: "FK_AccesosGimnasio_Clientes_IdCliente",
                        column: x => x.IdCliente,
                        principalTable: "Clientes",
                        principalColumn: "IdCliente",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccesosGimnasio_MembresiasClientes_IdMembresia",
                        column: x => x.IdMembresia,
                        principalTable: "MembresiasClientes",
                        principalColumn: "IdMembresia");
                    table.ForeignKey(
                        name: "FK_AccesosGimnasio_Sucursal_IdSucursal",
                        column: x => x.IdSucursal,
                        principalTable: "Sucursal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccesosGimnasio_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id_Usu");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccesosGimnasio_IdCaja",
                table: "AccesosGimnasio",
                column: "IdCaja");

            migrationBuilder.CreateIndex(
                name: "IX_AccesosGimnasio_IdCliente",
                table: "AccesosGimnasio",
                column: "IdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_AccesosGimnasio_IdMembresia",
                table: "AccesosGimnasio",
                column: "IdMembresia");

            migrationBuilder.CreateIndex(
                name: "IX_AccesosGimnasio_IdSucursal",
                table: "AccesosGimnasio",
                column: "IdSucursal");

            migrationBuilder.CreateIndex(
                name: "IX_AccesosGimnasio_IdUsuario",
                table: "AccesosGimnasio",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_MembresiasClientes_IdCaja",
                table: "MembresiasClientes",
                column: "IdCaja");

            migrationBuilder.CreateIndex(
                name: "IX_MembresiasClientes_IdCliente",
                table: "MembresiasClientes",
                column: "IdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_MembresiasClientes_IdPlan",
                table: "MembresiasClientes",
                column: "IdPlan");

            migrationBuilder.CreateIndex(
                name: "IX_MembresiasClientes_IdSucursal",
                table: "MembresiasClientes",
                column: "IdSucursal");

            migrationBuilder.CreateIndex(
                name: "IX_MembresiasClientes_IdUsuario",
                table: "MembresiasClientes",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_MembresiasClientes_IdVenta",
                table: "MembresiasClientes",
                column: "IdVenta");

            migrationBuilder.CreateIndex(
                name: "IX_PlanesMembresia_IdMoneda",
                table: "PlanesMembresia",
                column: "IdMoneda");

            migrationBuilder.CreateIndex(
                name: "IX_PlanesMembresia_IdSucursal",
                table: "PlanesMembresia",
                column: "IdSucursal");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccesosGimnasio");

            migrationBuilder.DropTable(
                name: "MembresiasClientes");

            migrationBuilder.DropTable(
                name: "PlanesMembresia");

            migrationBuilder.DropColumn(
                name: "GimnasioDiasGracia",
                table: "ConfiguracionSistema");

            migrationBuilder.DropColumn(
                name: "GimnasioHabilitarVoz",
                table: "ConfiguracionSistema");

            migrationBuilder.DropColumn(
                name: "GimnasioHorasAutoSalida",
                table: "ConfiguracionSistema");

            migrationBuilder.DropColumn(
                name: "GimnasioMensajeBienvenida",
                table: "ConfiguracionSistema");

            migrationBuilder.DropColumn(
                name: "GimnasioMensajeDespedida",
                table: "ConfiguracionSistema");

            migrationBuilder.DropColumn(
                name: "GimnasioMensajeVencido",
                table: "ConfiguracionSistema");

            migrationBuilder.DropColumn(
                name: "GimnasioMensajeVencimientoProximo",
                table: "ConfiguracionSistema");

            migrationBuilder.DropColumn(
                name: "GimnasioModoActivo",
                table: "ConfiguracionSistema");

            migrationBuilder.DropColumn(
                name: "GimnasioPermitirAccesoDiasGracia",
                table: "ConfiguracionSistema");

            migrationBuilder.DropColumn(
                name: "GimnasioReconocimientoFacialActivo",
                table: "ConfiguracionSistema");

            migrationBuilder.DropColumn(
                name: "GimnasioUmbralCoincidenciaFacial",
                table: "ConfiguracionSistema");

            migrationBuilder.DropColumn(
                name: "CodigoGimnasio",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "CondicionesMedicas",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "ContactoEmergencia",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "EmbeddingFacialGimnasio",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "EsMiembroGimnasio",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "FechaAltaGimnasio",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "FechaCaptuaFacial",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "FechaUltimaVisitaGimnasio",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "FotoGimnasio",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "MensajeBienvenidaPersonalizado",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "ObjetivoFitness",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "TelefonoEmergencia",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "TotalVisitasGimnasio",
                table: "Clientes");
        }
    }
}
