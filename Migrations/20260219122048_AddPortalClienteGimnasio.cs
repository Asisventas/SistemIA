using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class AddPortalClienteGimnasio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RecibeInformeGimnasio",
                table: "DestinatariosInforme",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "RutinasGimnasio",
                columns: table => new
                {
                    IdRutina = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCliente = table.Column<int>(type: "int", nullable: false),
                    IdInstructor = table.Column<int>(type: "int", nullable: true),
                    IdSucursal = table.Column<int>(type: "int", nullable: true),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NivelDificultad = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DuracionMinutos = table.Column<int>(type: "int", nullable: false),
                    DiasRecomendados = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RutinasGimnasio", x => x.IdRutina);
                    table.ForeignKey(
                        name: "FK_RutinasGimnasio_Clientes_IdCliente",
                        column: x => x.IdCliente,
                        principalTable: "Clientes",
                        principalColumn: "IdCliente",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RutinasGimnasio_Instructores_IdInstructor",
                        column: x => x.IdInstructor,
                        principalTable: "Instructores",
                        principalColumn: "IdInstructor");
                    table.ForeignKey(
                        name: "FK_RutinasGimnasio_Sucursal_IdSucursal",
                        column: x => x.IdSucursal,
                        principalTable: "Sucursal",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TarjetasPagoCliente",
                columns: table => new
                {
                    IdTarjeta = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCliente = table.Column<int>(type: "int", nullable: false),
                    Alias = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TipoTarjeta = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MarcaTarjeta = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Ultimos4Digitos = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    MesExpiracion = table.Column<int>(type: "int", nullable: false),
                    AnioExpiracion = table.Column<int>(type: "int", nullable: false),
                    NombreTitular = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TokenProcesador = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IdProcesadorPago = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EsPredeterminada = table.Column<bool>(type: "bit", nullable: false),
                    PermiteCobrosAutomaticos = table.Column<bool>(type: "bit", nullable: false),
                    LimiteMontoTransaccion = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UltimoUso = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TransaccionesRealizadas = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TarjetasPagoCliente", x => x.IdTarjeta);
                    table.ForeignKey(
                        name: "FK_TarjetasPagoCliente_Clientes_IdCliente",
                        column: x => x.IdCliente,
                        principalTable: "Clientes",
                        principalColumn: "IdCliente",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EjerciciosRutina",
                columns: table => new
                {
                    IdEjercicioRutina = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdRutina = table.Column<int>(type: "int", nullable: false),
                    NombreEjercicio = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GrupoMuscular = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UrlVideo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ImagenDemostrativa = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Series = table.Column<int>(type: "int", nullable: false),
                    Repeticiones = table.Column<int>(type: "int", nullable: false),
                    PesoRecomendadoKg = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    DescansoSegundos = table.Column<int>(type: "int", nullable: false),
                    DuracionSegundos = table.Column<int>(type: "int", nullable: true),
                    Notas = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EjerciciosRutina", x => x.IdEjercicioRutina);
                    table.ForeignKey(
                        name: "FK_EjerciciosRutina_RutinasGimnasio_IdRutina",
                        column: x => x.IdRutina,
                        principalTable: "RutinasGimnasio",
                        principalColumn: "IdRutina",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgresosCliente",
                columns: table => new
                {
                    IdProgreso = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCliente = table.Column<int>(type: "int", nullable: false),
                    IdRutina = table.Column<int>(type: "int", nullable: true),
                    IdInstructor = table.Column<int>(type: "int", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PesoKg = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    AlturaCm = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    PorcentajeGrasa = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    PorcentajeMusculo = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    IMC = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    MedidaPecho = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    MedidaCintura = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    MedidaCadera = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    MedidaBrazoDerecho = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    MedidaBrazoIzquierdo = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    MedidaMusloDerecho = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    MedidaMusloIzquierdo = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    MedidaPantorrilla = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    FotoFrontal = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    FotoLateral = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    FotoPosterior = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Notas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Objetivos = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgresosCliente", x => x.IdProgreso);
                    table.ForeignKey(
                        name: "FK_ProgresosCliente_Clientes_IdCliente",
                        column: x => x.IdCliente,
                        principalTable: "Clientes",
                        principalColumn: "IdCliente",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProgresosCliente_Instructores_IdInstructor",
                        column: x => x.IdInstructor,
                        principalTable: "Instructores",
                        principalColumn: "IdInstructor");
                    table.ForeignKey(
                        name: "FK_ProgresosCliente_RutinasGimnasio_IdRutina",
                        column: x => x.IdRutina,
                        principalTable: "RutinasGimnasio",
                        principalColumn: "IdRutina");
                });

            migrationBuilder.CreateTable(
                name: "SesionesEntrenamiento",
                columns: table => new
                {
                    IdSesion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCliente = table.Column<int>(type: "int", nullable: false),
                    IdRutina = table.Column<int>(type: "int", nullable: true),
                    IdSucursal = table.Column<int>(type: "int", nullable: true),
                    FechaHoraInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaHoraFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DuracionMinutos = table.Column<int>(type: "int", nullable: false),
                    TipoSesion = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CaloriasQuemadas = table.Column<int>(type: "int", nullable: true),
                    FrecuenciaCardiacaPromedio = table.Column<int>(type: "int", nullable: true),
                    EsfuerzoPercibido = table.Column<int>(type: "int", nullable: true),
                    Notas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EstadoAnimo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SesionesEntrenamiento", x => x.IdSesion);
                    table.ForeignKey(
                        name: "FK_SesionesEntrenamiento_Clientes_IdCliente",
                        column: x => x.IdCliente,
                        principalTable: "Clientes",
                        principalColumn: "IdCliente",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SesionesEntrenamiento_RutinasGimnasio_IdRutina",
                        column: x => x.IdRutina,
                        principalTable: "RutinasGimnasio",
                        principalColumn: "IdRutina");
                    table.ForeignKey(
                        name: "FK_SesionesEntrenamiento_Sucursal_IdSucursal",
                        column: x => x.IdSucursal,
                        principalTable: "Sucursal",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DetallesSesionEjercicio",
                columns: table => new
                {
                    IdDetalle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdSesion = table.Column<int>(type: "int", nullable: false),
                    IdEjercicioRutina = table.Column<int>(type: "int", nullable: true),
                    NombreEjercicio = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GrupoMuscular = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    SeriesCompletadas = table.Column<int>(type: "int", nullable: false),
                    SeriesPlanificadas = table.Column<int>(type: "int", nullable: false),
                    RepeticionesPromedio = table.Column<int>(type: "int", nullable: false),
                    PesoUtilizadoKg = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    PesoMaximoKg = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    Completado = table.Column<bool>(type: "bit", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesSesionEjercicio", x => x.IdDetalle);
                    table.ForeignKey(
                        name: "FK_DetallesSesionEjercicio_EjerciciosRutina_IdEjercicioRutina",
                        column: x => x.IdEjercicioRutina,
                        principalTable: "EjerciciosRutina",
                        principalColumn: "IdEjercicioRutina");
                    table.ForeignKey(
                        name: "FK_DetallesSesionEjercicio_SesionesEntrenamiento_IdSesion",
                        column: x => x.IdSesion,
                        principalTable: "SesionesEntrenamiento",
                        principalColumn: "IdSesion",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetallesSesionEjercicio_IdEjercicioRutina",
                table: "DetallesSesionEjercicio",
                column: "IdEjercicioRutina");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesSesionEjercicio_IdSesion",
                table: "DetallesSesionEjercicio",
                column: "IdSesion");

            migrationBuilder.CreateIndex(
                name: "IX_EjerciciosRutina_IdRutina",
                table: "EjerciciosRutina",
                column: "IdRutina");

            migrationBuilder.CreateIndex(
                name: "IX_ProgresosCliente_IdCliente",
                table: "ProgresosCliente",
                column: "IdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_ProgresosCliente_IdInstructor",
                table: "ProgresosCliente",
                column: "IdInstructor");

            migrationBuilder.CreateIndex(
                name: "IX_ProgresosCliente_IdRutina",
                table: "ProgresosCliente",
                column: "IdRutina");

            migrationBuilder.CreateIndex(
                name: "IX_RutinasGimnasio_IdCliente",
                table: "RutinasGimnasio",
                column: "IdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_RutinasGimnasio_IdInstructor",
                table: "RutinasGimnasio",
                column: "IdInstructor");

            migrationBuilder.CreateIndex(
                name: "IX_RutinasGimnasio_IdSucursal",
                table: "RutinasGimnasio",
                column: "IdSucursal");

            migrationBuilder.CreateIndex(
                name: "IX_SesionesEntrenamiento_IdCliente",
                table: "SesionesEntrenamiento",
                column: "IdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_SesionesEntrenamiento_IdRutina",
                table: "SesionesEntrenamiento",
                column: "IdRutina");

            migrationBuilder.CreateIndex(
                name: "IX_SesionesEntrenamiento_IdSucursal",
                table: "SesionesEntrenamiento",
                column: "IdSucursal");

            migrationBuilder.CreateIndex(
                name: "IX_TarjetasPagoCliente_IdCliente",
                table: "TarjetasPagoCliente",
                column: "IdCliente");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetallesSesionEjercicio");

            migrationBuilder.DropTable(
                name: "ProgresosCliente");

            migrationBuilder.DropTable(
                name: "TarjetasPagoCliente");

            migrationBuilder.DropTable(
                name: "EjerciciosRutina");

            migrationBuilder.DropTable(
                name: "SesionesEntrenamiento");

            migrationBuilder.DropTable(
                name: "RutinasGimnasio");

            migrationBuilder.DropColumn(
                name: "RecibeInformeGimnasio",
                table: "DestinatariosInforme");
        }
    }
}
