using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Add_SenasReservas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservas_Cajas_CajaSenaIdCaja",
                table: "Reservas");

            migrationBuilder.DropIndex(
                name: "IX_Reservas_CajaSenaIdCaja",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "CajaSenaIdCaja",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "IdCajaSena",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "MedioPagoSena",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "TurnoSena",
                table: "Reservas");

            migrationBuilder.CreateTable(
                name: "SenasReservas",
                columns: table => new
                {
                    IdSenaReserva = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdReserva = table.Column<int>(type: "int", nullable: false),
                    IdCliente = table.Column<int>(type: "int", nullable: true),
                    ClienteIdCliente = table.Column<int>(type: "int", nullable: true),
                    NombreCliente = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IdMoneda = table.Column<int>(type: "int", nullable: true),
                    MonedaIdMoneda = table.Column<int>(type: "int", nullable: true),
                    TipoCambio = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    MontoGs = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    MedioPago = table.Column<int>(type: "int", nullable: false),
                    TipoTarjeta = table.Column<int>(type: "int", nullable: true),
                    MarcaTarjeta = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Ultimos4 = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    NumeroAutorizacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BancoTransferencia = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NumeroComprobante = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    BancoCheque = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    NumeroCheque = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    FechaCobroCheque = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdSucursal = table.Column<int>(type: "int", nullable: true),
                    SucursalId = table.Column<int>(type: "int", nullable: true),
                    IdCaja = table.Column<int>(type: "int", nullable: true),
                    CajaIdCaja = table.Column<int>(type: "int", nullable: true),
                    Turno = table.Column<int>(type: "int", nullable: true),
                    FechaCaja = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdUsuario = table.Column<int>(type: "int", nullable: true),
                    UsuarioId_Usu = table.Column<int>(type: "int", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Devuelta = table.Column<bool>(type: "bit", nullable: false),
                    FechaDevolucion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MotivoAnulacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NumeroRecibo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SenasReservas", x => x.IdSenaReserva);
                    table.ForeignKey(
                        name: "FK_SenasReservas_Cajas_CajaIdCaja",
                        column: x => x.CajaIdCaja,
                        principalTable: "Cajas",
                        principalColumn: "id_caja");
                    table.ForeignKey(
                        name: "FK_SenasReservas_Clientes_ClienteIdCliente",
                        column: x => x.ClienteIdCliente,
                        principalTable: "Clientes",
                        principalColumn: "IdCliente");
                    table.ForeignKey(
                        name: "FK_SenasReservas_Monedas_MonedaIdMoneda",
                        column: x => x.MonedaIdMoneda,
                        principalTable: "Monedas",
                        principalColumn: "IdMoneda");
                    table.ForeignKey(
                        name: "FK_SenasReservas_Reservas_IdReserva",
                        column: x => x.IdReserva,
                        principalTable: "Reservas",
                        principalColumn: "IdReserva",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SenasReservas_Sucursal_SucursalId",
                        column: x => x.SucursalId,
                        principalTable: "Sucursal",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SenasReservas_Usuarios_UsuarioId_Usu",
                        column: x => x.UsuarioId_Usu,
                        principalTable: "Usuarios",
                        principalColumn: "Id_Usu");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SenasReservas_CajaIdCaja",
                table: "SenasReservas",
                column: "CajaIdCaja");

            migrationBuilder.CreateIndex(
                name: "IX_SenasReservas_ClienteIdCliente",
                table: "SenasReservas",
                column: "ClienteIdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_SenasReservas_IdReserva",
                table: "SenasReservas",
                column: "IdReserva",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SenasReservas_MonedaIdMoneda",
                table: "SenasReservas",
                column: "MonedaIdMoneda");

            migrationBuilder.CreateIndex(
                name: "IX_SenasReservas_SucursalId",
                table: "SenasReservas",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_SenasReservas_UsuarioId_Usu",
                table: "SenasReservas",
                column: "UsuarioId_Usu");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SenasReservas");

            migrationBuilder.AddColumn<int>(
                name: "CajaSenaIdCaja",
                table: "Reservas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdCajaSena",
                table: "Reservas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MedioPagoSena",
                table: "Reservas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TurnoSena",
                table: "Reservas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_CajaSenaIdCaja",
                table: "Reservas",
                column: "CajaSenaIdCaja");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservas_Cajas_CajaSenaIdCaja",
                table: "Reservas",
                column: "CajaSenaIdCaja",
                principalTable: "Cajas",
                principalColumn: "id_caja");
        }
    }
}
