using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ciudades",
                columns: table => new
                {
                    IdCiudad = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Departamento = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ciudades", x => x.IdCiudad);
                });

            migrationBuilder.CreateTable(
                name: "Marcas",
                columns: table => new
                {
                    Id_Marca = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Marca = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Marcas", x => x.Id_Marca);
                });

            migrationBuilder.CreateTable(
                name: "Paises",
                columns: table => new
                {
                    CodigoPais = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paises", x => x.CodigoPais);
                });

            migrationBuilder.CreateTable(
                name: "Rol",
                columns: table => new
                {
                    Id_Rol = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreRol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rol", x => x.Id_Rol);
                });

            migrationBuilder.CreateTable(
                name: "TiposContribuyentes",
                columns: table => new
                {
                    IdTipoContribuyente = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreTipo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposContribuyentes", x => x.IdTipoContribuyente);
                });

            migrationBuilder.CreateTable(
                name: "TiposDocumentosIdentidad",
                columns: table => new
                {
                    TipoDocumento = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposDocumentosIdentidad", x => x.TipoDocumento);
                });

            migrationBuilder.CreateTable(
                name: "TiposOperacion",
                columns: table => new
                {
                    Codigo = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Comentario = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposOperacion", x => x.Codigo);
                });

            migrationBuilder.CreateTable(
                name: "Sucursal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumSucursal = table.Column<string>(type: "nchar(7)", fixedLength: true, maxLength: 7, nullable: false),
                    NombreSucursal = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NombreEmpresa = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    RubroEmpresa = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IdCiudad = table.Column<int>(type: "int", nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Correo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RUC = table.Column<string>(type: "nchar(8)", fixedLength: true, maxLength: 8, nullable: false),
                    DV = table.Column<int>(type: "int", nullable: false),
                    CertificadoRuta = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CertificadoPassword = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Ambiente = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Timbrado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PuntoExpedicion = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    SistemaPlaya = table.Column<bool>(type: "bit", nullable: false),
                    Automatizado = table.Column<bool>(type: "bit", nullable: false),
                    IpConsola = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    PuertoConsola = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Logo = table.Column<byte[]>(type: "VARBINARY(MAX)", nullable: true),
                    Conexion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ToleranciaEntradaMinutos = table.Column<int>(type: "int", nullable: false),
                    ToleranciaSalidaMinutos = table.Column<int>(type: "int", nullable: false),
                    RequiereJustificacionTardanza = table.Column<bool>(type: "bit", nullable: false),
                    RequiereJustificacionSalidaTemprana = table.Column<bool>(type: "bit", nullable: false),
                    MaximoHorasExtraDia = table.Column<int>(type: "int", nullable: false),
                    CalculoAutomaticoHorasExtra = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sucursal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sucursal_Ciudades_IdCiudad",
                        column: x => x.IdCiudad,
                        principalTable: "Ciudades",
                        principalColumn: "IdCiudad",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id_Usu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombres = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Ciudad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CI = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Correo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Fecha_Ingreso = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Id_Rol = table.Column<int>(type: "int", nullable: false),
                    Estado_Usu = table.Column<bool>(type: "bit", nullable: false),
                    UsuarioNombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ContrasenaHash = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    Foto = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    EmbeddingFacial = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    Fecha_Nacimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HuellaDigital = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Salario = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IPS = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Comision = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Descuento = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id_Usu);
                    table.ForeignKey(
                        name: "FK_Usuarios_Rol_Id_Rol",
                        column: x => x.Id_Rol,
                        principalTable: "Rol",
                        principalColumn: "Id_Rol",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Proveedores",
                columns: table => new
                {
                    Id_Proveedor = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoProveedor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rubro = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RUC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DV = table.Column<int>(type: "int", nullable: true),
                    Correo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Contacto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FotoLogo = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    EstadoProveedor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VencimientoTimbrado = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Timbrado = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdTipoContribuyente = table.Column<int>(type: "int", nullable: true),
                    CodigoContribuyente = table.Column<int>(type: "int", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proveedores", x => x.Id_Proveedor);
                    table.ForeignKey(
                        name: "FK_Proveedores_TiposContribuyentes_IdTipoContribuyente",
                        column: x => x.IdTipoContribuyente,
                        principalTable: "TiposContribuyentes",
                        principalColumn: "IdTipoContribuyente");
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    IdCliente = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RazonSocial = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    RUC = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    TipoDocumento = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    DV = table.Column<int>(type: "int", nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    FechaNacimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Contacto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LimiteCredito = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Saldo = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IdTipoContribuyente = table.Column<int>(type: "int", nullable: false),
                    Timbrado = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    VencimientoTimbrado = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PrecioDiferenciado = table.Column<bool>(type: "bit", nullable: false),
                    PlazoDiasCredito = table.Column<int>(type: "int", nullable: true),
                    CodigoPais = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    IdCiudad = table.Column<int>(type: "int", nullable: false),
                    EsExtranjero = table.Column<bool>(type: "bit", nullable: false),
                    TipoOperacion = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.IdCliente);
                    table.ForeignKey(
                        name: "FK_Clientes_Ciudades_IdCiudad",
                        column: x => x.IdCiudad,
                        principalTable: "Ciudades",
                        principalColumn: "IdCiudad",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Clientes_Paises_CodigoPais",
                        column: x => x.CodigoPais,
                        principalTable: "Paises",
                        principalColumn: "CodigoPais",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Clientes_TiposContribuyentes_IdTipoContribuyente",
                        column: x => x.IdTipoContribuyente,
                        principalTable: "TiposContribuyentes",
                        principalColumn: "IdTipoContribuyente",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Clientes_TiposDocumentosIdentidad_TipoDocumento",
                        column: x => x.TipoDocumento,
                        principalTable: "TiposDocumentosIdentidad",
                        principalColumn: "TipoDocumento",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Clientes_TiposOperacion_TipoOperacion",
                        column: x => x.TipoOperacion,
                        principalTable: "TiposOperacion",
                        principalColumn: "Codigo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HorariosTrabajo",
                columns: table => new
                {
                    Id_Horario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id_Sucursal = table.Column<int>(type: "int", nullable: false),
                    NombreHorario = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HoraEntrada = table.Column<TimeSpan>(type: "time", nullable: false),
                    HoraSalida = table.Column<TimeSpan>(type: "time", nullable: false),
                    InicioBreak = table.Column<TimeSpan>(type: "time", nullable: true),
                    FinBreak = table.Column<TimeSpan>(type: "time", nullable: true),
                    Lunes = table.Column<bool>(type: "bit", nullable: false),
                    Martes = table.Column<bool>(type: "bit", nullable: false),
                    Miercoles = table.Column<bool>(type: "bit", nullable: false),
                    Jueves = table.Column<bool>(type: "bit", nullable: false),
                    Viernes = table.Column<bool>(type: "bit", nullable: false),
                    Sabado = table.Column<bool>(type: "bit", nullable: false),
                    Domingo = table.Column<bool>(type: "bit", nullable: false),
                    EsActivo = table.Column<bool>(type: "bit", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorariosTrabajo", x => x.Id_Horario);
                    table.ForeignKey(
                        name: "FK_HorariosTrabajo_Sucursal_Id_Sucursal",
                        column: x => x.Id_Sucursal,
                        principalTable: "Sucursal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AsignacionesHorarios",
                columns: table => new
                {
                    Id_Asignacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id_Usuario = table.Column<int>(type: "int", nullable: false),
                    Id_Horario = table.Column<int>(type: "int", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsignacionesHorarios", x => x.Id_Asignacion);
                    table.ForeignKey(
                        name: "FK_AsignacionesHorarios_HorariosTrabajo_Id_Horario",
                        column: x => x.Id_Horario,
                        principalTable: "HorariosTrabajo",
                        principalColumn: "Id_Horario",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AsignacionesHorarios_Usuarios_Id_Usuario",
                        column: x => x.Id_Usuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id_Usu",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Asistencias",
                columns: table => new
                {
                    Id_Asistencia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id_Usuario = table.Column<int>(type: "int", nullable: false),
                    Sucursal = table.Column<int>(type: "int", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TipoRegistro = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Ubicacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaInicioAusencia = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaFinAusencia = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MotivoAusencia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AprobadaPorGerencia = table.Column<bool>(type: "bit", nullable: false),
                    AprobadoPorId_Usuario = table.Column<int>(type: "int", nullable: true),
                    FechaAprobacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ImagenRegistro = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    HoraProgramada = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DiferenciaMinutos = table.Column<int>(type: "int", nullable: true),
                    EstadoPuntualidad = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MinutosToleranciAplicada = table.Column<int>(type: "int", nullable: true),
                    Id_HorarioAplicado = table.Column<int>(type: "int", nullable: true),
                    DiaSemana = table.Column<int>(type: "int", nullable: true),
                    EsDiaLaborable = table.Column<bool>(type: "bit", nullable: true),
                    TiempoTrabajadoMinutos = table.Column<int>(type: "int", nullable: true),
                    HorasExtraMinutos = table.Column<int>(type: "int", nullable: true),
                    RequiereJustificacion = table.Column<bool>(type: "bit", nullable: false),
                    EstadoJustificacion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TextoJustificacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EsRegistroAutomatico = table.Column<bool>(type: "bit", nullable: false),
                    MetodoRegistro = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    UbicacionRegistro = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Temperatura = table.Column<decimal>(type: "decimal(4,1)", nullable: true),
                    ObservacionesSistema = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asistencias", x => x.Id_Asistencia);
                    table.ForeignKey(
                        name: "FK_Asistencias_HorariosTrabajo_Id_HorarioAplicado",
                        column: x => x.Id_HorarioAplicado,
                        principalTable: "HorariosTrabajo",
                        principalColumn: "Id_Horario");
                    table.ForeignKey(
                        name: "FK_Asistencias_Sucursal_Sucursal",
                        column: x => x.Sucursal,
                        principalTable: "Sucursal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Asistencias_Usuarios_AprobadoPorId_Usuario",
                        column: x => x.AprobadoPorId_Usuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id_Usu",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Asistencias_Usuarios_Id_Usuario",
                        column: x => x.Id_Usuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id_Usu",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesHorarios_Id_Horario",
                table: "AsignacionesHorarios",
                column: "Id_Horario");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesHorarios_Id_Usuario",
                table: "AsignacionesHorarios",
                column: "Id_Usuario");

            migrationBuilder.CreateIndex(
                name: "IX_Asistencias_AprobadoPorId_Usuario",
                table: "Asistencias",
                column: "AprobadoPorId_Usuario");

            migrationBuilder.CreateIndex(
                name: "IX_Asistencias_Id_HorarioAplicado",
                table: "Asistencias",
                column: "Id_HorarioAplicado");

            migrationBuilder.CreateIndex(
                name: "IX_Asistencias_Id_Usuario",
                table: "Asistencias",
                column: "Id_Usuario");

            migrationBuilder.CreateIndex(
                name: "IX_Asistencias_Sucursal",
                table: "Asistencias",
                column: "Sucursal");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_CodigoPais",
                table: "Clientes",
                column: "CodigoPais");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_IdCiudad",
                table: "Clientes",
                column: "IdCiudad");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_IdTipoContribuyente",
                table: "Clientes",
                column: "IdTipoContribuyente");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_TipoDocumento",
                table: "Clientes",
                column: "TipoDocumento");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_TipoOperacion",
                table: "Clientes",
                column: "TipoOperacion");

            migrationBuilder.CreateIndex(
                name: "IX_HorariosTrabajo_Id_Sucursal",
                table: "HorariosTrabajo",
                column: "Id_Sucursal");

            migrationBuilder.CreateIndex(
                name: "IX_Proveedores_IdTipoContribuyente",
                table: "Proveedores",
                column: "IdTipoContribuyente");

            migrationBuilder.CreateIndex(
                name: "IX_Sucursal_IdCiudad",
                table: "Sucursal",
                column: "IdCiudad");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Id_Rol",
                table: "Usuarios",
                column: "Id_Rol");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AsignacionesHorarios");

            migrationBuilder.DropTable(
                name: "Asistencias");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Marcas");

            migrationBuilder.DropTable(
                name: "Proveedores");

            migrationBuilder.DropTable(
                name: "HorariosTrabajo");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Paises");

            migrationBuilder.DropTable(
                name: "TiposDocumentosIdentidad");

            migrationBuilder.DropTable(
                name: "TiposOperacion");

            migrationBuilder.DropTable(
                name: "TiposContribuyentes");

            migrationBuilder.DropTable(
                name: "Sucursal");

            migrationBuilder.DropTable(
                name: "Rol");

            migrationBuilder.DropTable(
                name: "Ciudades");
        }
    }
}
