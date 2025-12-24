using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class OptimizarConsultasFactura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Índice para consultas frecuentes de Cajas (CajaActual = 1)
            migrationBuilder.CreateIndex(
                name: "IX_Cajas_CajaActual",
                table: "Cajas",
                column: "CajaActual",
                filter: "[CajaActual] = 1");

            // Índice compuesto para VentasDetalles (IdVenta + IdProducto) para joins optimizados
            migrationBuilder.CreateIndex(
                name: "IX_VentasDetalles_IdVenta_IdProducto",
                table: "VentasDetalles",
                columns: new[] { "IdVenta", "IdProducto" });

            // Índice para Ventas por fecha (consultas de reportes)
            migrationBuilder.CreateIndex(
                name: "IX_Ventas_Fecha",
                table: "Ventas",
                column: "Fecha");

            // Índice para Productos por estado activo
            migrationBuilder.CreateIndex(
                name: "IX_Productos_Activo",
                table: "Productos",
                column: "Activo",
                filter: "[Activo] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cajas_CajaActual",
                table: "Cajas");

            migrationBuilder.DropIndex(
                name: "IX_VentasDetalles_IdVenta_IdProducto",
                table: "VentasDetalles");

            migrationBuilder.DropIndex(
                name: "IX_Ventas_Fecha",
                table: "Ventas");

            migrationBuilder.DropIndex(
                name: "IX_Productos_Activo",
                table: "Productos");
        }
    }
}
