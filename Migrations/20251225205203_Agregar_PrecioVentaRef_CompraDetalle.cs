using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_PrecioVentaRef_CompraDetalle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Agregar columna PrecioVentaRef si no existe
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                               WHERE TABLE_NAME = 'ComprasDetalles' AND COLUMN_NAME = 'PrecioVentaRef')
                BEGIN
                    ALTER TABLE [ComprasDetalles] ADD [PrecioVentaRef] decimal(18,4) NOT NULL DEFAULT 0;
                END
            ");

            // Actualizar FK de CuentasPorPagar si existe el constraint viejo
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CuentasPorPagar_Proveedores_IdProveedor')
                BEGIN
                    ALTER TABLE [CuentasPorPagar] DROP CONSTRAINT [FK_CuentasPorPagar_Proveedores_IdProveedor];
                END
                
                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CuentasPorPagar_ProveedoresSifen_IdProveedor')
                BEGIN
                    ALTER TABLE [CuentasPorPagar] ADD CONSTRAINT [FK_CuentasPorPagar_ProveedoresSifen_IdProveedor] 
                        FOREIGN KEY ([IdProveedor]) REFERENCES [ProveedoresSifen]([IdProveedor]) ON DELETE NO ACTION;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CuentasPorPagar_ProveedoresSifen_IdProveedor')
                BEGIN
                    ALTER TABLE [CuentasPorPagar] DROP CONSTRAINT [FK_CuentasPorPagar_ProveedoresSifen_IdProveedor];
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                           WHERE TABLE_NAME = 'ComprasDetalles' AND COLUMN_NAME = 'PrecioVentaRef')
                BEGIN
                    ALTER TABLE [ComprasDetalles] DROP COLUMN [PrecioVentaRef];
                END
            ");
        }
    }
}
