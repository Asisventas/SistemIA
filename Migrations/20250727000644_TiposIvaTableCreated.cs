using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class TiposIvaTableCreated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clientes_TiposOperacion_TipoOperacion",
                table: "Clientes");

            migrationBuilder.AlterColumn<string>(
                name: "TipoOperacion",
                table: "Clientes",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<string>(
                name: "RUC",
                table: "Clientes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(8)",
                oldMaxLength: 8);

            migrationBuilder.AlterColumn<bool>(
                name: "Estado",
                table: "Clientes",
                type: "bit",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "Celular",
                table: "Clientes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodigoCliente",
                table: "Clientes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodigoDepartamento",
                table: "Clientes",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodigoDistrito",
                table: "Clientes",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodigoEstablecimiento",
                table: "Clientes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompDireccion1",
                table: "Clientes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompDireccion2",
                table: "Clientes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescripcionDepartamento",
                table: "Clientes",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescripcionDistrito",
                table: "Clientes",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAlta",
                table: "Clientes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NaturalezaReceptor",
                table: "Clientes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "NombreFantasia",
                table: "Clientes",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroCasa",
                table: "Clientes",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroDocumento",
                table: "Clientes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroDocumentoIdentidad",
                table: "Clientes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoDocumentoIdentidadSifen",
                table: "Clientes",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaFin",
                table: "AsignacionesHorarios",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "AsignadoPorId_Usuario",
                table: "AsignacionesHorarios",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Estado",
                table: "AsignacionesHorarios",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAsignacion",
                table: "AsignacionesHorarios",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Observaciones",
                table: "AsignacionesHorarios",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProveedoresSifen",
                columns: table => new
                {
                    IdProveedor = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoProveedor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RazonSocial = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NombreFantasia = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RUC = table.Column<string>(type: "nchar(8)", fixedLength: true, maxLength: 8, nullable: false),
                    DV = table.Column<int>(type: "int", nullable: false),
                    TipoContribuyente = table.Column<int>(type: "int", nullable: false),
                    IdTipoContribuyenteCatalogo = table.Column<int>(type: "int", nullable: false),
                    TipoRegimen = table.Column<int>(type: "int", nullable: false),
                    Timbrado = table.Column<string>(type: "nchar(8)", fixedLength: true, maxLength: 8, nullable: false),
                    VencimientoTimbrado = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Establecimiento = table.Column<string>(type: "nchar(3)", fixedLength: true, maxLength: 3, nullable: false),
                    PuntoExpedicion = table.Column<string>(type: "nchar(3)", fixedLength: true, maxLength: 3, nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    NumeroCasa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CodigoDepartamento = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    DescripcionDepartamento = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    CodigoCiudad = table.Column<int>(type: "int", nullable: false),
                    DescripcionCiudad = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Celular = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    PersonaContacto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CodigoActividadEconomica = table.Column<string>(type: "nchar(5)", fixedLength: true, maxLength: 5, nullable: false),
                    DescripcionActividadEconomica = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Rubro = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LimiteCredito = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    SaldoPendiente = table.Column<decimal>(type: "decimal(18,4)", nullable: false, defaultValue: 0m),
                    PlazoPagoDias = table.Column<int>(type: "int", nullable: true),
                    CodigoPais = table.Column<string>(type: "nchar(3)", fixedLength: true, maxLength: 3, nullable: false, defaultValue: "PRY"),
                    CertificadoRuta = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CertificadoPassword = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Ambiente = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    UltimoNumeroDocumento = table.Column<long>(type: "bigint", nullable: false),
                    UltimaSerie = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaUltimaFacturacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProveedoresSifen", x => x.IdProveedor);
                    table.ForeignKey(
                        name: "FK_ProveedoresSifen_TiposContribuyentes_IdTipoContribuyenteCatalogo",
                        column: x => x.IdTipoContribuyenteCatalogo,
                        principalTable: "TiposContribuyentes",
                        principalColumn: "IdTipoContribuyente",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TiposIva",
                columns: table => new
                {
                    IdTipoIva = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoSifen = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Porcentaje = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    TasaSifen = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    EsPredeterminado = table.Column<bool>(type: "bit", nullable: false),
                    CodigoAbreviado = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposIva", x => x.IdTipoIva);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProveedoresSifen_IdTipoContribuyenteCatalogo",
                table: "ProveedoresSifen",
                column: "IdTipoContribuyenteCatalogo");

            migrationBuilder.AddForeignKey(
                name: "FK_Clientes_TiposOperacion_TipoOperacion",
                table: "Clientes",
                column: "TipoOperacion",
                principalTable: "TiposOperacion",
                principalColumn: "Codigo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clientes_TiposOperacion_TipoOperacion",
                table: "Clientes");

            migrationBuilder.DropTable(
                name: "ProveedoresSifen");

            migrationBuilder.DropTable(
                name: "TiposIva");

            migrationBuilder.DropColumn(
                name: "Celular",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "CodigoCliente",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "CodigoDepartamento",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "CodigoDistrito",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "CodigoEstablecimiento",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "CompDireccion1",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "CompDireccion2",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "DescripcionDepartamento",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "DescripcionDistrito",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "FechaAlta",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "NaturalezaReceptor",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "NombreFantasia",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "NumeroCasa",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "NumeroDocumento",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "NumeroDocumentoIdentidad",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "TipoDocumentoIdentidadSifen",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "AsignadoPorId_Usuario",
                table: "AsignacionesHorarios");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "AsignacionesHorarios");

            migrationBuilder.DropColumn(
                name: "FechaAsignacion",
                table: "AsignacionesHorarios");

            migrationBuilder.DropColumn(
                name: "Observaciones",
                table: "AsignacionesHorarios");

            migrationBuilder.AlterColumn<string>(
                name: "TipoOperacion",
                table: "Clientes",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(1)",
                oldMaxLength: 1,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RUC",
                table: "Clientes",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "Clientes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaFin",
                table: "AsignacionesHorarios",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Clientes_TiposOperacion_TipoOperacion",
                table: "Clientes",
                column: "TipoOperacion",
                principalTable: "TiposOperacion",
                principalColumn: "Codigo",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
