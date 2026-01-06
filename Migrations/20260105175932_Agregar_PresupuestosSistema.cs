using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_PresupuestosSistema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PresupuestosSistema",
                columns: table => new
                {
                    IdPresupuesto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreCliente = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RucCliente = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DireccionCliente = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TelefonoCliente = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EmailCliente = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ContactoCliente = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CargoContacto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NumeroPresupuesto = table.Column<int>(type: "int", nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaVigencia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PrecioContadoUsd = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrecioContadoGs = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    TipoCambio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrecioLeasingMensualUsd = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrecioLeasingMensualGs = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    CantidadCuotasLeasing = table.Column<int>(type: "int", nullable: false),
                    EntradaLeasingUsd = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EntradaLeasingGs = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    CostoImplementacionUsd = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostoImplementacionGs = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    CostoCapacitacionUsd = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostoCapacitacionGs = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    HorasCapacitacion = table.Column<int>(type: "int", nullable: false),
                    CostoMantenimientoMensualUsd = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostoMantenimientoMensualGs = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    CostoSucursalAdicionalUsd = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostoSucursalAdicionalGs = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    CostoUsuarioAdicionalUsd = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostoUsuarioAdicionalGs = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    CostoHoraDesarrolloUsd = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostoHoraDesarrolloGs = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    CostoVisitaTecnicaUsd = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostoVisitaTecnicaGs = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    ModuloVentas = table.Column<bool>(type: "bit", nullable: false),
                    ModuloCompras = table.Column<bool>(type: "bit", nullable: false),
                    ModuloInventario = table.Column<bool>(type: "bit", nullable: false),
                    ModuloClientes = table.Column<bool>(type: "bit", nullable: false),
                    ModuloCaja = table.Column<bool>(type: "bit", nullable: false),
                    ModuloSifen = table.Column<bool>(type: "bit", nullable: false),
                    ModuloInformes = table.Column<bool>(type: "bit", nullable: false),
                    ModuloUsuarios = table.Column<bool>(type: "bit", nullable: false),
                    ModuloCorreo = table.Column<bool>(type: "bit", nullable: false),
                    ModuloAsistenteIA = table.Column<bool>(type: "bit", nullable: false),
                    CantidadSucursalesIncluidas = table.Column<int>(type: "int", nullable: false),
                    CantidadUsuariosIncluidos = table.Column<int>(type: "int", nullable: false),
                    CantidadCajasIncluidas = table.Column<int>(type: "int", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CondicionesPago = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Garantias = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaEnvio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaRespuesta = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IdSucursal = table.Column<int>(type: "int", nullable: false),
                    SucursalId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PresupuestosSistema", x => x.IdPresupuesto);
                    table.ForeignKey(
                        name: "FK_PresupuestosSistema_Sucursal_SucursalId",
                        column: x => x.SucursalId,
                        principalTable: "Sucursal",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PresupuestosSistema_SucursalId",
                table: "PresupuestosSistema",
                column: "SucursalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PresupuestosSistema");
        }
    }
}
