using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class AgregaCamposRecordatorio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ColorAgenda",
                table: "Clientes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DireccionCompleta",
                table: "Clientes",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Latitud",
                table: "Clientes",
                type: "decimal(10,7)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitud",
                table: "Clientes",
                type: "decimal(10,7)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CitasAgenda",
                columns: table => new
                {
                    IdCita = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdSucursal = table.Column<int>(type: "int", nullable: false),
                    SucursalId = table.Column<int>(type: "int", nullable: true),
                    TipoCita = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IdCliente = table.Column<int>(type: "int", nullable: true),
                    ClienteIdCliente = table.Column<int>(type: "int", nullable: true),
                    NombreCliente = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    FechaHoraInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaHoraFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TodoElDia = table.Column<bool>(type: "bit", nullable: false),
                    DuracionMinutos = table.Column<int>(type: "int", nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Latitud = table.Column<decimal>(type: "decimal(10,7)", nullable: true),
                    Longitud = table.Column<decimal>(type: "decimal(10,7)", nullable: true),
                    UrlMaps = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ColorFondo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ColorTexto = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Icono = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Prioridad = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TieneRecordatorio = table.Column<bool>(type: "bit", nullable: false),
                    MinutosRecordatorio1 = table.Column<int>(type: "int", nullable: true),
                    MinutosRecordatorio2 = table.Column<int>(type: "int", nullable: true),
                    EnviarCorreo = table.Column<bool>(type: "bit", nullable: false),
                    FechaEnvioCorreo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MostrarNotificacion = table.Column<bool>(type: "bit", nullable: false),
                    NotificacionMostrada = table.Column<bool>(type: "bit", nullable: false),
                    EsRecurrente = table.Column<bool>(type: "bit", nullable: false),
                    TipoRecurrencia = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IntervaloRecurrencia = table.Column<int>(type: "int", nullable: true),
                    FechaFinRecurrencia = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdCitaPadre = table.Column<int>(type: "int", nullable: true),
                    IdUsuarioAsignado = table.Column<int>(type: "int", nullable: true),
                    UsuarioAsignadoId_Usu = table.Column<int>(type: "int", nullable: true),
                    NombreAsignado = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdUsuarioCreador = table.Column<int>(type: "int", nullable: true),
                    UsuarioCreadorId_Usu = table.Column<int>(type: "int", nullable: true),
                    Notas = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Resultado = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CitasAgenda", x => x.IdCita);
                    table.ForeignKey(
                        name: "FK_CitasAgenda_Clientes_ClienteIdCliente",
                        column: x => x.ClienteIdCliente,
                        principalTable: "Clientes",
                        principalColumn: "IdCliente");
                    table.ForeignKey(
                        name: "FK_CitasAgenda_Sucursal_SucursalId",
                        column: x => x.SucursalId,
                        principalTable: "Sucursal",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CitasAgenda_Usuarios_UsuarioAsignadoId_Usu",
                        column: x => x.UsuarioAsignadoId_Usu,
                        principalTable: "Usuarios",
                        principalColumn: "Id_Usu");
                    table.ForeignKey(
                        name: "FK_CitasAgenda_Usuarios_UsuarioCreadorId_Usu",
                        column: x => x.UsuarioCreadorId_Usu,
                        principalTable: "Usuarios",
                        principalColumn: "Id_Usu");
                });

            migrationBuilder.CreateTable(
                name: "ColoresClientesAgenda",
                columns: table => new
                {
                    IdColorCliente = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCliente = table.Column<int>(type: "int", nullable: false),
                    ClienteIdCliente = table.Column<int>(type: "int", nullable: true),
                    IdSucursal = table.Column<int>(type: "int", nullable: false),
                    SucursalId = table.Column<int>(type: "int", nullable: true),
                    ColorFondo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ColorTexto = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColoresClientesAgenda", x => x.IdColorCliente);
                    table.ForeignKey(
                        name: "FK_ColoresClientesAgenda_Clientes_ClienteIdCliente",
                        column: x => x.ClienteIdCliente,
                        principalTable: "Clientes",
                        principalColumn: "IdCliente");
                    table.ForeignKey(
                        name: "FK_ColoresClientesAgenda_Sucursal_SucursalId",
                        column: x => x.SucursalId,
                        principalTable: "Sucursal",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RecordatoriosCitas",
                columns: table => new
                {
                    IdRecordatorio = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCita = table.Column<int>(type: "int", nullable: false),
                    CitaIdCita = table.Column<int>(type: "int", nullable: true),
                    FechaHoraProgramada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Enviado = table.Column<bool>(type: "bit", nullable: false),
                    FechaEnvio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Mensaje = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MinutosAntes = table.Column<int>(type: "int", nullable: false),
                    Resultado = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecordatoriosCitas", x => x.IdRecordatorio);
                    table.ForeignKey(
                        name: "FK_RecordatoriosCitas_CitasAgenda_CitaIdCita",
                        column: x => x.CitaIdCita,
                        principalTable: "CitasAgenda",
                        principalColumn: "IdCita");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CitasAgenda_ClienteIdCliente",
                table: "CitasAgenda",
                column: "ClienteIdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_CitasAgenda_SucursalId",
                table: "CitasAgenda",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_CitasAgenda_UsuarioAsignadoId_Usu",
                table: "CitasAgenda",
                column: "UsuarioAsignadoId_Usu");

            migrationBuilder.CreateIndex(
                name: "IX_CitasAgenda_UsuarioCreadorId_Usu",
                table: "CitasAgenda",
                column: "UsuarioCreadorId_Usu");

            migrationBuilder.CreateIndex(
                name: "IX_ColoresClientesAgenda_ClienteIdCliente",
                table: "ColoresClientesAgenda",
                column: "ClienteIdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_ColoresClientesAgenda_SucursalId",
                table: "ColoresClientesAgenda",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_RecordatoriosCitas_CitaIdCita",
                table: "RecordatoriosCitas",
                column: "CitaIdCita");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ColoresClientesAgenda");

            migrationBuilder.DropTable(
                name: "RecordatoriosCitas");

            migrationBuilder.DropTable(
                name: "CitasAgenda");

            migrationBuilder.DropColumn(
                name: "ColorAgenda",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "DireccionCompleta",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Latitud",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Longitud",
                table: "Clientes");
        }
    }
}
