using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_IdLote_en_Ventas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Solo agregar la columna IdLote a Ventas si no existe
            migrationBuilder.Sql(@"IF COL_LENGTH('dbo.Ventas','IdLote') IS NULL
ALTER TABLE [dbo].[Ventas] ADD [IdLote] NVARCHAR(50) NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Solo quitar la columna IdLote si existe
            migrationBuilder.Sql(@"IF COL_LENGTH('dbo.Ventas','IdLote') IS NOT NULL
ALTER TABLE [dbo].[Ventas] DROP COLUMN [IdLote];");
        }
    }
}
