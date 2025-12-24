using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Serie_Venta_Vuelto_Composicion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Agregar columna Serie a Ventas (si no existe)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Ventas') AND name = 'Serie')
                BEGIN
                    ALTER TABLE [Ventas] ADD [Serie] int NULL;
                END
            ");

            // Agregar columna Vuelto a ComposicionesCaja (si no existe)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'ComposicionesCaja') AND name = 'Vuelto')
                BEGIN
                    ALTER TABLE [ComposicionesCaja] ADD [Vuelto] decimal(18,4) NOT NULL DEFAULT 0;
                END
            ");

            // Crear índice único para evitar duplicados de numeración (si no existe)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'Ventas') AND name = 'IX_Ventas_Numeracion_Unica')
                BEGIN
                    CREATE UNIQUE INDEX [IX_Ventas_Numeracion_Unica] 
                    ON [Ventas] ([Establecimiento], [NumeroFactura], [PuntoExpedicion], [Serie], [Timbrado])
                    WHERE [NumeroFactura] IS NOT NULL AND [Serie] IS NOT NULL AND [Timbrado] IS NOT NULL;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Eliminar índice único
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'Ventas') AND name = 'IX_Ventas_Numeracion_Unica')
                BEGIN
                    DROP INDEX [IX_Ventas_Numeracion_Unica] ON [Ventas];
                END
            ");

            // Eliminar columna Vuelto de ComposicionesCaja (primero eliminar constraint default)
            migrationBuilder.Sql(@"
                DECLARE @constraintName NVARCHAR(200);
                SELECT @constraintName = dc.name 
                FROM sys.default_constraints dc
                JOIN sys.columns c ON dc.parent_column_id = c.column_id AND dc.parent_object_id = c.object_id
                WHERE dc.parent_object_id = OBJECT_ID(N'ComposicionesCaja') AND c.name = 'Vuelto';
                
                IF @constraintName IS NOT NULL
                BEGIN
                    EXEC('ALTER TABLE [ComposicionesCaja] DROP CONSTRAINT [' + @constraintName + ']');
                END
                
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'ComposicionesCaja') AND name = 'Vuelto')
                BEGIN
                    ALTER TABLE [ComposicionesCaja] DROP COLUMN [Vuelto];
                END
            ");

            // Eliminar columna Serie de Ventas
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Ventas') AND name = 'Serie')
                BEGIN
                    ALTER TABLE [Ventas] DROP COLUMN [Serie];
                END
            ");
        }
    }
}
