using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Crear_Tablas_CuentasPorPagar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Crear tabla CuentasPorPagar si no existe
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CuentasPorPagar')
                BEGIN
                    CREATE TABLE [CuentasPorPagar] (
                        [IdCuentaPorPagar] int NOT NULL IDENTITY,
                        [IdCompra] int NOT NULL,
                        [IdProveedor] int NOT NULL,
                        [IdSucursal] int NOT NULL,
                        [IdMoneda] int NULL,
                        [MontoTotal] decimal(18,4) NOT NULL,
                        [SaldoPendiente] decimal(18,4) NOT NULL,
                        [FechaCredito] datetime2 NOT NULL,
                        [FechaVencimiento] datetime2 NULL,
                        [Estado] nvarchar(20) NOT NULL,
                        [NumeroCuotas] int NOT NULL,
                        [PlazoDias] int NOT NULL,
                        [Observaciones] nvarchar(280) NULL,
                        [IdUsuarioAutorizo] int NULL,
                        CONSTRAINT [PK_CuentasPorPagar] PRIMARY KEY ([IdCuentaPorPagar]),
                        CONSTRAINT [FK_CuentasPorPagar_Compras_IdCompra] FOREIGN KEY ([IdCompra]) REFERENCES [Compras] ([IdCompra]) ON DELETE NO ACTION,
                        CONSTRAINT [FK_CuentasPorPagar_ProveedoresSifen_IdProveedor] FOREIGN KEY ([IdProveedor]) REFERENCES [ProveedoresSifen] ([IdProveedor]) ON DELETE NO ACTION,
                        CONSTRAINT [FK_CuentasPorPagar_Monedas_IdMoneda] FOREIGN KEY ([IdMoneda]) REFERENCES [Monedas] ([IdMoneda]),
                        CONSTRAINT [FK_CuentasPorPagar_Usuarios_IdUsuarioAutorizo] FOREIGN KEY ([IdUsuarioAutorizo]) REFERENCES [Usuarios] ([Id_Usu])
                    );
                    CREATE INDEX [IX_CuentasPorPagar_IdCompra] ON [CuentasPorPagar] ([IdCompra]);
                    CREATE INDEX [IX_CuentasPorPagar_IdProveedor] ON [CuentasPorPagar] ([IdProveedor]);
                    CREATE INDEX [IX_CuentasPorPagar_IdMoneda] ON [CuentasPorPagar] ([IdMoneda]);
                    CREATE INDEX [IX_CuentasPorPagar_IdUsuarioAutorizo] ON [CuentasPorPagar] ([IdUsuarioAutorizo]);
                END
            ");

            // Crear tabla CuentasPorPagarCuotas si no existe
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CuentasPorPagarCuotas')
                BEGIN
                    CREATE TABLE [CuentasPorPagarCuotas] (
                        [IdCuota] int NOT NULL IDENTITY,
                        [IdCuentaPorPagar] int NOT NULL,
                        [NumeroCuota] int NOT NULL,
                        [MontoCuota] decimal(18,4) NOT NULL,
                        [SaldoCuota] decimal(18,4) NOT NULL,
                        [FechaVencimiento] datetime2 NOT NULL,
                        [FechaPago] datetime2 NULL,
                        [Estado] nvarchar(20) NOT NULL,
                        [Observaciones] nvarchar(280) NULL,
                        CONSTRAINT [PK_CuentasPorPagarCuotas] PRIMARY KEY ([IdCuota]),
                        CONSTRAINT [FK_CuentasPorPagarCuotas_CuentasPorPagar_IdCuentaPorPagar] FOREIGN KEY ([IdCuentaPorPagar]) REFERENCES [CuentasPorPagar] ([IdCuentaPorPagar]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_CuentasPorPagarCuotas_IdCuentaPorPagar] ON [CuentasPorPagarCuotas] ([IdCuentaPorPagar]);
                END
                ELSE
                BEGIN
                    -- Si la tabla ya existe pero le falta SaldoCuota, agregar la columna
                    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CuentasPorPagarCuotas' AND COLUMN_NAME = 'SaldoCuota')
                    BEGIN
                        ALTER TABLE [CuentasPorPagarCuotas] ADD [SaldoCuota] decimal(18,4) NOT NULL DEFAULT 0;
                    END
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CuentasPorPagarCuotas')
                BEGIN
                    DROP TABLE [CuentasPorPagarCuotas];
                END
            ");
            
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CuentasPorPagar')
                BEGIN
                    DROP TABLE [CuentasPorPagar];
                END
            ");
        }
    }
}
