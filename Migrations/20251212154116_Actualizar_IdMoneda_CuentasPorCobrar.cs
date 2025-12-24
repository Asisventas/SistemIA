using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Actualizar_IdMoneda_CuentasPorCobrar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Actualizar IdMoneda en CuentasPorCobrar desde la venta relacionada
            migrationBuilder.Sql(@"
                UPDATE cpc
                SET cpc.IdMoneda = v.IdMoneda
                FROM CuentasPorCobrar cpc
                INNER JOIN Ventas v ON cpc.IdVenta = v.IdVenta
                WHERE cpc.IdMoneda IS NULL AND v.IdMoneda IS NOT NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No hay necesidad de deshacer esta actualización de datos
        }
    }
}
