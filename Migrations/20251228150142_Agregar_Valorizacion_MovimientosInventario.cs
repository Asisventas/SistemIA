using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Valorizacion_MovimientosInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Agregar columnas de valorización de forma idempotente
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'PrecioCosto')
                BEGIN
                    ALTER TABLE [MovimientosInventario] ADD [PrecioCosto] decimal(18,4) NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'PrecioVenta')
                BEGIN
                    ALTER TABLE [MovimientosInventario] ADD [PrecioVenta] decimal(18,4) NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'IdMoneda')
                BEGIN
                    ALTER TABLE [MovimientosInventario] ADD [IdMoneda] int NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'TipoCambio')
                BEGIN
                    ALTER TABLE [MovimientosInventario] ADD [TipoCambio] decimal(18,6) NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'PrecioCostoGs')
                BEGIN
                    ALTER TABLE [MovimientosInventario] ADD [PrecioCostoGs] decimal(18,4) NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'PrecioVentaGs')
                BEGIN
                    ALTER TABLE [MovimientosInventario] ADD [PrecioVentaGs] decimal(18,4) NULL;
                END
            ");

            // Crear índice de forma idempotente
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'IX_MovimientosInventario_IdMoneda')
                BEGIN
                    CREATE INDEX [IX_MovimientosInventario_IdMoneda] ON [MovimientosInventario]([IdMoneda]);
                END
            ");

            // Crear FK de forma idempotente
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_MovimientosInventario_Monedas_IdMoneda')
                BEGIN
                    ALTER TABLE [MovimientosInventario] ADD CONSTRAINT [FK_MovimientosInventario_Monedas_IdMoneda] 
                        FOREIGN KEY ([IdMoneda]) REFERENCES [Monedas]([IdMoneda]);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Eliminar FK
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_MovimientosInventario_Monedas_IdMoneda')
                BEGIN
                    ALTER TABLE [MovimientosInventario] DROP CONSTRAINT [FK_MovimientosInventario_Monedas_IdMoneda];
                END
            ");

            // Eliminar índice
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'IX_MovimientosInventario_IdMoneda')
                BEGIN
                    DROP INDEX [IX_MovimientosInventario_IdMoneda] ON [MovimientosInventario];
                END
            ");

            // Eliminar columnas
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'PrecioCosto')
                BEGIN
                    ALTER TABLE [MovimientosInventario] DROP COLUMN [PrecioCosto];
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'PrecioVenta')
                BEGIN
                    ALTER TABLE [MovimientosInventario] DROP COLUMN [PrecioVenta];
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'IdMoneda')
                BEGIN
                    ALTER TABLE [MovimientosInventario] DROP COLUMN [IdMoneda];
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'TipoCambio')
                BEGIN
                    ALTER TABLE [MovimientosInventario] DROP COLUMN [TipoCambio];
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'PrecioCostoGs')
                BEGIN
                    ALTER TABLE [MovimientosInventario] DROP COLUMN [PrecioCostoGs];
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'PrecioVentaGs')
                BEGIN
                    ALTER TABLE [MovimientosInventario] DROP COLUMN [PrecioVentaGs];
                END
            ");
        }
    }
}
