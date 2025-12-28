using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Campos_Trazabilidad_MovimientosInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Agregar columnas de trazabilidad de forma idempotente
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'CantidadAnterior')
                BEGIN
                    ALTER TABLE [MovimientosInventario] ADD [CantidadAnterior] decimal(18,4) NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'SaldoPosterior')
                BEGIN
                    ALTER TABLE [MovimientosInventario] ADD [SaldoPosterior] decimal(18,4) NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'FechaCaja')
                BEGIN
                    ALTER TABLE [MovimientosInventario] ADD [FechaCaja] datetime2 NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'Turno')
                BEGIN
                    ALTER TABLE [MovimientosInventario] ADD [Turno] int NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'IdSucursal')
                BEGIN
                    ALTER TABLE [MovimientosInventario] ADD [IdSucursal] int NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'IdCaja')
                BEGIN
                    ALTER TABLE [MovimientosInventario] ADD [IdCaja] int NULL;
                END
            ");

            // Crear índices de forma idempotente
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'IX_MovimientosInventario_IdSucursal')
                BEGIN
                    CREATE INDEX [IX_MovimientosInventario_IdSucursal] ON [MovimientosInventario]([IdSucursal]);
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'IX_MovimientosInventario_IdCaja')
                BEGIN
                    CREATE INDEX [IX_MovimientosInventario_IdCaja] ON [MovimientosInventario]([IdCaja]);
                END
            ");

            // Crear FK de forma idempotente
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_MovimientosInventario_Sucursal_IdSucursal')
                BEGIN
                    ALTER TABLE [MovimientosInventario] ADD CONSTRAINT [FK_MovimientosInventario_Sucursal_IdSucursal] 
                        FOREIGN KEY ([IdSucursal]) REFERENCES [Sucursal]([Id]);
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_MovimientosInventario_Cajas_IdCaja')
                BEGIN
                    ALTER TABLE [MovimientosInventario] ADD CONSTRAINT [FK_MovimientosInventario_Cajas_IdCaja] 
                        FOREIGN KEY ([IdCaja]) REFERENCES [Cajas]([id_caja]);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Eliminar FKs
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_MovimientosInventario_Cajas_IdCaja')
                BEGIN
                    ALTER TABLE [MovimientosInventario] DROP CONSTRAINT [FK_MovimientosInventario_Cajas_IdCaja];
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_MovimientosInventario_Sucursal_IdSucursal')
                BEGIN
                    ALTER TABLE [MovimientosInventario] DROP CONSTRAINT [FK_MovimientosInventario_Sucursal_IdSucursal];
                END
            ");

            // Eliminar índices
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'IX_MovimientosInventario_IdCaja')
                BEGIN
                    DROP INDEX [IX_MovimientosInventario_IdCaja] ON [MovimientosInventario];
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'IX_MovimientosInventario_IdSucursal')
                BEGIN
                    DROP INDEX [IX_MovimientosInventario_IdSucursal] ON [MovimientosInventario];
                END
            ");

            // Eliminar columnas
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'CantidadAnterior')
                BEGIN
                    ALTER TABLE [MovimientosInventario] DROP COLUMN [CantidadAnterior];
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'SaldoPosterior')
                BEGIN
                    ALTER TABLE [MovimientosInventario] DROP COLUMN [SaldoPosterior];
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'FechaCaja')
                BEGIN
                    ALTER TABLE [MovimientosInventario] DROP COLUMN [FechaCaja];
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'Turno')
                BEGIN
                    ALTER TABLE [MovimientosInventario] DROP COLUMN [Turno];
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'IdSucursal')
                BEGIN
                    ALTER TABLE [MovimientosInventario] DROP COLUMN [IdSucursal];
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'IdCaja')
                BEGIN
                    ALTER TABLE [MovimientosInventario] DROP COLUMN [IdCaja];
                END
            ");
        }
    }
}
