using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Index_IdLote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ya existe la columna IdLote, solo crear índice si no existe
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Ventas]') AND name = N'IX_Ventas_IdLote')
CREATE INDEX [IX_Ventas_IdLote] ON [dbo].[Ventas] ([IdLote]);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Ventas]') AND name = N'IX_Ventas_IdLote')
DROP INDEX [IX_Ventas_IdLote] ON [dbo].[Ventas];");
        }
    }
}
