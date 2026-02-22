using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Clases_Horarios_Gimnasio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Instructores",
                columns: table => new
                {
                    IdInstructor = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdSucursal = table.Column<int>(type: "int", nullable: true),
                    NombreCompleto = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Cedula = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Foto = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Especialidades = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Certificaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EsPersonalTrainer = table.Column<bool>(type: "bit", nullable: false),
                    DictaClasesGrupales = table.Column<bool>(type: "bit", nullable: false),
                    FechaIngreso = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TipoContrato = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TarifaHora = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    DiasDisponibles = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    HorarioDisponible = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    ColorHex = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instructores", x => x.IdInstructor);
                    table.ForeignKey(
                        name: "FK_Instructores_Sucursal_IdSucursal",
                        column: x => x.IdSucursal,
                        principalTable: "Sucursal",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ClasesGimnasio",
                columns: table => new
                {
                    IdClase = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdSucursal = table.Column<int>(type: "int", nullable: true),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Categoria = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nivel = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Icono = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ColorHex = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    Imagen = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    DuracionMinutos = table.Column<int>(type: "int", nullable: false),
                    CapacidadMaxima = table.Column<int>(type: "int", nullable: false),
                    MinimoAlumnos = table.Column<int>(type: "int", nullable: false),
                    Sala = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CaloriasAproximadas = table.Column<int>(type: "int", nullable: true),
                    IdInstructor = table.Column<int>(type: "int", nullable: true),
                    TieneCostoAdicional = table.Column<bool>(type: "bit", nullable: false),
                    PrecioPorSesion = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    PlanesIncluidos = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PermiteReservas = table.Column<bool>(type: "bit", nullable: false),
                    HorasAnticipacionReserva = table.Column<int>(type: "int", nullable: false),
                    HorasLimiteCancelacion = table.Column<int>(type: "int", nullable: false),
                    MaxInasistencias = table.Column<int>(type: "int", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClasesGimnasio", x => x.IdClase);
                    table.ForeignKey(
                        name: "FK_ClasesGimnasio_Instructores_IdInstructor",
                        column: x => x.IdInstructor,
                        principalTable: "Instructores",
                        principalColumn: "IdInstructor");
                    table.ForeignKey(
                        name: "FK_ClasesGimnasio_Sucursal_IdSucursal",
                        column: x => x.IdSucursal,
                        principalTable: "Sucursal",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "HorariosClases",
                columns: table => new
                {
                    IdHorario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdClase = table.Column<int>(type: "int", nullable: false),
                    IdInstructor = table.Column<int>(type: "int", nullable: true),
                    TipoHorario = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DiaSemana = table.Column<int>(type: "int", nullable: true),
                    FechaInicioRecurrencia = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaFinRecurrencia = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaEspecifica = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HoraInicio = table.Column<TimeSpan>(type: "time", nullable: false),
                    HoraFin = table.Column<TimeSpan>(type: "time", nullable: false),
                    DuracionMinutos = table.Column<int>(type: "int", nullable: false),
                    CapacidadMaxima = table.Column<int>(type: "int", nullable: true),
                    Sala = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    Cancelado = table.Column<bool>(type: "bit", nullable: false),
                    MotivoCancelacion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorariosClases", x => x.IdHorario);
                    table.ForeignKey(
                        name: "FK_HorariosClases_ClasesGimnasio_IdClase",
                        column: x => x.IdClase,
                        principalTable: "ClasesGimnasio",
                        principalColumn: "IdClase",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HorariosClases_Instructores_IdInstructor",
                        column: x => x.IdInstructor,
                        principalTable: "Instructores",
                        principalColumn: "IdInstructor");
                });

            migrationBuilder.CreateTable(
                name: "ReservasClases",
                columns: table => new
                {
                    IdReserva = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdSucursal = table.Column<int>(type: "int", nullable: false),
                    IdCaja = table.Column<int>(type: "int", nullable: true),
                    Turno = table.Column<int>(type: "int", nullable: true),
                    IdHorario = table.Column<int>(type: "int", nullable: false),
                    FechaClase = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdCliente = table.Column<int>(type: "int", nullable: false),
                    IdMembresia = table.Column<int>(type: "int", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PosicionListaEspera = table.Column<int>(type: "int", nullable: false),
                    FechaReserva = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaConfirmacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCancelacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MotivoCancelacion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    HoraCheckIn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LlegoTarde = table.Column<bool>(type: "bit", nullable: false),
                    IdUsuarioCheckIn = table.Column<int>(type: "int", nullable: true),
                    MetodoCheckIn = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Pagado = table.Column<bool>(type: "bit", nullable: false),
                    MontoPagado = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    IdVenta = table.Column<int>(type: "int", nullable: true),
                    Notas = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    RecordatorioEnviado = table.Column<bool>(type: "bit", nullable: false),
                    FechaRecordatorio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservasClases", x => x.IdReserva);
                    table.ForeignKey(
                        name: "FK_ReservasClases_Cajas_IdCaja",
                        column: x => x.IdCaja,
                        principalTable: "Cajas",
                        principalColumn: "id_caja");
                    table.ForeignKey(
                        name: "FK_ReservasClases_Clientes_IdCliente",
                        column: x => x.IdCliente,
                        principalTable: "Clientes",
                        principalColumn: "IdCliente",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReservasClases_HorariosClases_IdHorario",
                        column: x => x.IdHorario,
                        principalTable: "HorariosClases",
                        principalColumn: "IdHorario",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReservasClases_MembresiasClientes_IdMembresia",
                        column: x => x.IdMembresia,
                        principalTable: "MembresiasClientes",
                        principalColumn: "IdMembresia");
                    table.ForeignKey(
                        name: "FK_ReservasClases_Sucursal_IdSucursal",
                        column: x => x.IdSucursal,
                        principalTable: "Sucursal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReservasClases_Ventas_IdVenta",
                        column: x => x.IdVenta,
                        principalTable: "Ventas",
                        principalColumn: "IdVenta");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClasesGimnasio_IdInstructor",
                table: "ClasesGimnasio",
                column: "IdInstructor");

            migrationBuilder.CreateIndex(
                name: "IX_ClasesGimnasio_IdSucursal",
                table: "ClasesGimnasio",
                column: "IdSucursal");

            migrationBuilder.CreateIndex(
                name: "IX_HorariosClases_IdClase",
                table: "HorariosClases",
                column: "IdClase");

            migrationBuilder.CreateIndex(
                name: "IX_HorariosClases_IdInstructor",
                table: "HorariosClases",
                column: "IdInstructor");

            migrationBuilder.CreateIndex(
                name: "IX_Instructores_IdSucursal",
                table: "Instructores",
                column: "IdSucursal");

            migrationBuilder.CreateIndex(
                name: "IX_ReservasClases_IdCaja",
                table: "ReservasClases",
                column: "IdCaja");

            migrationBuilder.CreateIndex(
                name: "IX_ReservasClases_IdCliente",
                table: "ReservasClases",
                column: "IdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_ReservasClases_IdHorario",
                table: "ReservasClases",
                column: "IdHorario");

            migrationBuilder.CreateIndex(
                name: "IX_ReservasClases_IdMembresia",
                table: "ReservasClases",
                column: "IdMembresia");

            migrationBuilder.CreateIndex(
                name: "IX_ReservasClases_IdSucursal",
                table: "ReservasClases",
                column: "IdSucursal");

            migrationBuilder.CreateIndex(
                name: "IX_ReservasClases_IdVenta",
                table: "ReservasClases",
                column: "IdVenta");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReservasClases");

            migrationBuilder.DropTable(
                name: "HorariosClases");

            migrationBuilder.DropTable(
                name: "ClasesGimnasio");

            migrationBuilder.DropTable(
                name: "Instructores");
        }
    }
}
