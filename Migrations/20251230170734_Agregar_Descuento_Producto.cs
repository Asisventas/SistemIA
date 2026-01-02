using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Descuento_Producto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ========== ConfiguracionSistema - PermitirVenderConDescuento (idempotente) ==========
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'ConfiguracionSistema') AND name = 'PermitirVenderConDescuento')
                BEGIN
                    ALTER TABLE [ConfiguracionSistema] ADD [PermitirVenderConDescuento] BIT NOT NULL DEFAULT 0;
                END
            ");
            
            // ========== ConfiguracionSistema - PorcentajeDescuentoMaximo (idempotente) ==========
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'ConfiguracionSistema') AND name = 'PorcentajeDescuentoMaximo')
                BEGIN
                    ALTER TABLE [ConfiguracionSistema] ADD [PorcentajeDescuentoMaximo] DECIMAL(5,2) NULL;
                END
            ");
            
            // ========== Productos - PermiteDescuento (idempotente) ==========
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Productos') AND name = 'PermiteDescuento')
                BEGIN
                    ALTER TABLE [Productos] ADD [PermiteDescuento] BIT NOT NULL DEFAULT 1;
                END
            ");
            
            // ========== Productos - DescuentoMaximoProducto (idempotente) ==========
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Productos') AND name = 'DescuentoMaximoProducto')
                BEGIN
                    ALTER TABLE [Productos] ADD [DescuentoMaximoProducto] DECIMAL(5,2) NULL;
                END
            ");
            
            // ========== VentasDetalles - PrecioMinisterio (idempotente) ==========
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'VentasDetalles') AND name = 'PrecioMinisterio')
                BEGIN
                    ALTER TABLE [VentasDetalles] ADD [PrecioMinisterio] DECIMAL(18,4) NULL;
                END
            ");
            
            // ========== VentasDetalles - PorcentajeDescuento (idempotente) ==========
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'VentasDetalles') AND name = 'PorcentajeDescuento')
                BEGIN
                    ALTER TABLE [VentasDetalles] ADD [PorcentajeDescuento] DECIMAL(18,4) NULL;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'ConfiguracionSistema') AND name = 'PermitirVenderConDescuento')
                    ALTER TABLE [ConfiguracionSistema] DROP COLUMN [PermitirVenderConDescuento];
            ");
            
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'ConfiguracionSistema') AND name = 'PorcentajeDescuentoMaximo')
                    ALTER TABLE [ConfiguracionSistema] DROP COLUMN [PorcentajeDescuentoMaximo];
            ");
            
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Productos') AND name = 'PermiteDescuento')
                    ALTER TABLE [Productos] DROP COLUMN [PermiteDescuento];
            ");
            
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Productos') AND name = 'DescuentoMaximoProducto')
                    ALTER TABLE [Productos] DROP COLUMN [DescuentoMaximoProducto];
            ");
            
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'VentasDetalles') AND name = 'PrecioMinisterio')
                    ALTER TABLE [VentasDetalles] DROP COLUMN [PrecioMinisterio];
            ");
            
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'VentasDetalles') AND name = 'PorcentajeDescuento')
                    ALTER TABLE [VentasDetalles] DROP COLUMN [PorcentajeDescuento];
            ");
        }
    }
}
