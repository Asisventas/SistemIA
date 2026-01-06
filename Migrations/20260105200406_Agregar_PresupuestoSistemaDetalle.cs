using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_PresupuestoSistemaDetalle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PresupuestosSistemaDetalles",
                columns: table => new
                {
                    IdDetalle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdPresupuesto = table.Column<int>(type: "int", nullable: false),
                    NumeroLinea = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    DescripcionAdicional = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Unidad = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PrecioUnitarioUsd = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrecioUnitarioGs = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    PorcentajeDescuento = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    SubtotalUsd = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubtotalGs = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    TipoItem = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EsOpcional = table.Column<bool>(type: "bit", nullable: false),
                    Incluido = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PresupuestosSistemaDetalles", x => x.IdDetalle);
                    table.ForeignKey(
                        name: "FK_PresupuestosSistemaDetalles_PresupuestosSistema_IdPresupuesto",
                        column: x => x.IdPresupuesto,
                        principalTable: "PresupuestosSistema",
                        principalColumn: "IdPresupuesto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PresupuestosSistemaDetalles_IdPresupuesto",
                table: "PresupuestosSistemaDetalles",
                column: "IdPresupuesto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PresupuestosSistemaDetalles");
        }
    }
}
