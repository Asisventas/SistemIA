-- Script para aplicar migraciones pendientes de forma idempotente
-- Generado: 30/12/2025

BEGIN TRANSACTION;

-- ============================================
-- 1. PrecioMinisterio en Productos
-- ============================================
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Productos') AND name = 'PrecioMinisterio')
BEGIN
    ALTER TABLE [Productos] ADD [PrecioMinisterio] decimal(18,4) NULL;
    PRINT 'Columna PrecioMinisterio agregada a Productos';
END
ELSE
BEGIN
    PRINT 'Columna PrecioMinisterio ya existe en Productos';
END

-- ============================================
-- 2. Tabla ConfiguracionSistema
-- ============================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ConfiguracionSistema')
BEGIN
    CREATE TABLE [ConfiguracionSistema] (
        [IdConfiguracion] int NOT NULL IDENTITY,
        [FarmaciaModoActivo] bit NOT NULL,
        [FarmaciaValidarPrecioMinisterio] bit NOT NULL,
        [FarmaciaMostrarPrecioMinisterio] bit NOT NULL,
        [FarmaciaMostrarPrecioMinisterioEnCompras] bit NOT NULL,
        [NombreNegocio] nvarchar(200) NULL,
        [FechaModificacion] datetime2 NULL,
        [UsuarioModificacion] nvarchar(50) NULL,
        CONSTRAINT [PK_ConfiguracionSistema] PRIMARY KEY ([IdConfiguracion])
    );
    PRINT 'Tabla ConfiguracionSistema creada';
END
ELSE
BEGIN
    PRINT 'Tabla ConfiguracionSistema ya existe';
END

-- ============================================
-- 3. Tabla NotasCreditoCompras
-- ============================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NotasCreditoCompras')
BEGIN
    CREATE TABLE [NotasCreditoCompras] (
        [IdNotaCreditoCompra] int NOT NULL IDENTITY,
        [Establecimiento] nvarchar(3) NULL,
        [PuntoExpedicion] nvarchar(3) NULL,
        [NumeroNota] nvarchar(7) NULL,
        [IdSucursal] int NOT NULL,
        [IdCaja] int NULL,
        [IdProveedor] int NOT NULL,
        [NombreProveedor] nvarchar(200) NULL,
        [RucProveedor] nvarchar(20) NULL,
        [IdCompraAsociada] int NULL,
        [EstablecimientoAsociado] nvarchar(3) NULL,
        [PuntoExpedicionAsociado] nvarchar(3) NULL,
        [NumeroFacturaAsociado] nvarchar(7) NULL,
        [TimbradoAsociado] nvarchar(8) NULL,
        [Fecha] datetime2 NOT NULL,
        [FechaContable] datetime2 NULL,
        [Turno] int NULL,
        [Motivo] nvarchar(50) NOT NULL,
        [Observaciones] nvarchar(500) NULL,
        [IdMoneda] int NULL,
        [SimboloMoneda] nvarchar(4) NULL,
        [CambioDelDia] decimal(18,4) NOT NULL,
        [EsMonedaExtranjera] bit NOT NULL,
        [Subtotal] decimal(18,4) NOT NULL,
        [TotalIVA10] decimal(18,4) NOT NULL,
        [TotalIVA5] decimal(18,4) NOT NULL,
        [TotalExenta] decimal(18,4) NOT NULL,
        [TotalDescuento] decimal(18,4) NOT NULL,
        [Total] decimal(18,4) NOT NULL,
        [TotalEnLetras] nvarchar(280) NULL,
        [Estado] nvarchar(20) NOT NULL,
        [ImputarIVA] bit NULL,
        [ImputarIRP] bit NULL,
        [ImputarIRE] bit NULL,
        [NoImputar] bit NULL,
        [IdDeposito] int NULL,
        [AfectaStock] int NOT NULL,
        [IdUsuario] int NULL,
        [CreadoPor] nvarchar(100) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [ModificadoPor] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        CONSTRAINT [PK_NotasCreditoCompras] PRIMARY KEY ([IdNotaCreditoCompra])
    );
    PRINT 'Tabla NotasCreditoCompras creada';
END
ELSE
BEGIN
    PRINT 'Tabla NotasCreditoCompras ya existe';
END

-- ============================================
-- 4. Tabla NotasCreditoComprasDetalles
-- ============================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NotasCreditoComprasDetalles')
BEGIN
    CREATE TABLE [NotasCreditoComprasDetalles] (
        [IdNotaCreditoCompraDetalle] int NOT NULL IDENTITY,
        [IdNotaCreditoCompra] int NOT NULL,
        [IdProducto] int NOT NULL,
        [CodigoProducto] nvarchar(50) NULL,
        [NombreProducto] nvarchar(250) NULL,
        [Cantidad] decimal(18,4) NOT NULL,
        [PrecioUnitario] decimal(18,4) NOT NULL,
        [PorcentajeDescuento] decimal(18,4) NOT NULL,
        [MontoDescuento] decimal(18,4) NOT NULL,
        [Importe] decimal(18,4) NOT NULL,
        [TasaIVA] int NOT NULL,
        [IVA10] decimal(18,4) NOT NULL,
        [IVA5] decimal(18,4) NOT NULL,
        [Exenta] decimal(18,4) NOT NULL,
        [Grabado10] decimal(18,4) NOT NULL,
        [Grabado5] decimal(18,4) NOT NULL,
        [IdDepositoItem] int NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [UsuarioCreacion] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [UsuarioModificacion] nvarchar(100) NULL,
        CONSTRAINT [PK_NotasCreditoComprasDetalles] PRIMARY KEY ([IdNotaCreditoCompraDetalle])
    );
    PRINT 'Tabla NotasCreditoComprasDetalles creada';
END
ELSE
BEGIN
    PRINT 'Tabla NotasCreditoComprasDetalles ya existe';
END

-- ============================================
-- 5. PrecioMinisterio en ComprasDetalles
-- ============================================
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'ComprasDetalles') AND name = 'PrecioMinisterio')
BEGIN
    ALTER TABLE [ComprasDetalles] ADD [PrecioMinisterio] decimal(18,4) NULL;
    PRINT 'Columna PrecioMinisterio agregada a ComprasDetalles';
END
ELSE
BEGIN
    PRINT 'Columna PrecioMinisterio ya existe en ComprasDetalles';
END

-- ============================================
-- 6. Foreign Keys para NotasCreditoCompras
-- ============================================
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_NotasCreditoCompras_Cajas_IdCaja')
BEGIN
    ALTER TABLE [NotasCreditoCompras] ADD CONSTRAINT [FK_NotasCreditoCompras_Cajas_IdCaja] 
        FOREIGN KEY ([IdCaja]) REFERENCES [Cajas] ([id_caja]) ON DELETE NO ACTION;
    PRINT 'FK NotasCreditoCompras -> Cajas creada';
END

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_NotasCreditoCompras_Compras_IdCompraAsociada')
BEGIN
    ALTER TABLE [NotasCreditoCompras] ADD CONSTRAINT [FK_NotasCreditoCompras_Compras_IdCompraAsociada] 
        FOREIGN KEY ([IdCompraAsociada]) REFERENCES [Compras] ([IdCompra]) ON DELETE NO ACTION;
    PRINT 'FK NotasCreditoCompras -> Compras creada';
END

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_NotasCreditoCompras_Depositos_IdDeposito')
BEGIN
    ALTER TABLE [NotasCreditoCompras] ADD CONSTRAINT [FK_NotasCreditoCompras_Depositos_IdDeposito] 
        FOREIGN KEY ([IdDeposito]) REFERENCES [Depositos] ([IdDeposito]) ON DELETE NO ACTION;
    PRINT 'FK NotasCreditoCompras -> Depositos creada';
END

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_NotasCreditoCompras_Monedas_IdMoneda')
BEGIN
    ALTER TABLE [NotasCreditoCompras] ADD CONSTRAINT [FK_NotasCreditoCompras_Monedas_IdMoneda] 
        FOREIGN KEY ([IdMoneda]) REFERENCES [Monedas] ([IdMoneda]) ON DELETE NO ACTION;
    PRINT 'FK NotasCreditoCompras -> Monedas creada';
END

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_NotasCreditoCompras_ProveedoresSifen_IdProveedor')
BEGIN
    ALTER TABLE [NotasCreditoCompras] ADD CONSTRAINT [FK_NotasCreditoCompras_ProveedoresSifen_IdProveedor] 
        FOREIGN KEY ([IdProveedor]) REFERENCES [ProveedoresSifen] ([IdProveedor]) ON DELETE NO ACTION;
    PRINT 'FK NotasCreditoCompras -> ProveedoresSifen creada';
END

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_NotasCreditoCompras_Sucursal_IdSucursal')
BEGIN
    ALTER TABLE [NotasCreditoCompras] ADD CONSTRAINT [FK_NotasCreditoCompras_Sucursal_IdSucursal] 
        FOREIGN KEY ([IdSucursal]) REFERENCES [Sucursal] ([Id]) ON DELETE NO ACTION;
    PRINT 'FK NotasCreditoCompras -> Sucursal creada';
END

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_NotasCreditoCompras_Usuarios_IdUsuario')
BEGIN
    ALTER TABLE [NotasCreditoCompras] ADD CONSTRAINT [FK_NotasCreditoCompras_Usuarios_IdUsuario] 
        FOREIGN KEY ([IdUsuario]) REFERENCES [Usuarios] ([Id_Usu]) ON DELETE NO ACTION;
    PRINT 'FK NotasCreditoCompras -> Usuarios creada';
END

-- ============================================
-- 7. Foreign Keys para NotasCreditoComprasDetalles
-- ============================================
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_NotasCreditoComprasDetalles_Depositos_IdDepositoItem')
BEGIN
    ALTER TABLE [NotasCreditoComprasDetalles] ADD CONSTRAINT [FK_NotasCreditoComprasDetalles_Depositos_IdDepositoItem] 
        FOREIGN KEY ([IdDepositoItem]) REFERENCES [Depositos] ([IdDeposito]) ON DELETE NO ACTION;
    PRINT 'FK NotasCreditoComprasDetalles -> Depositos creada';
END

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_NotasCreditoComprasDetalles_NotasCreditoCompras_IdNotaCreditoCompra')
BEGIN
    ALTER TABLE [NotasCreditoComprasDetalles] ADD CONSTRAINT [FK_NotasCreditoComprasDetalles_NotasCreditoCompras_IdNotaCreditoCompra] 
        FOREIGN KEY ([IdNotaCreditoCompra]) REFERENCES [NotasCreditoCompras] ([IdNotaCreditoCompra]) ON DELETE CASCADE;
    PRINT 'FK NotasCreditoComprasDetalles -> NotasCreditoCompras creada';
END

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_NotasCreditoComprasDetalles_Productos_IdProducto')
BEGIN
    ALTER TABLE [NotasCreditoComprasDetalles] ADD CONSTRAINT [FK_NotasCreditoComprasDetalles_Productos_IdProducto] 
        FOREIGN KEY ([IdProducto]) REFERENCES [Productos] ([IdProducto]) ON DELETE NO ACTION;
    PRINT 'FK NotasCreditoComprasDetalles -> Productos creada';
END

-- ============================================
-- 8. Índices
-- ============================================
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'NotasCreditoCompras') AND name = 'IX_NotaCreditoCompras_Numeracion')
BEGIN
    CREATE UNIQUE INDEX [IX_NotaCreditoCompras_Numeracion] ON [NotasCreditoCompras] ([IdSucursal], [NumeroNota]) WHERE [NumeroNota] IS NOT NULL;
    PRINT 'Indice IX_NotaCreditoCompras_Numeracion creado';
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'NotasCreditoCompras') AND name = 'IX_NotasCreditoCompras_IdCaja')
BEGIN
    CREATE INDEX [IX_NotasCreditoCompras_IdCaja] ON [NotasCreditoCompras] ([IdCaja]);
    PRINT 'Indice IX_NotasCreditoCompras_IdCaja creado';
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'NotasCreditoCompras') AND name = 'IX_NotasCreditoCompras_IdCompraAsociada')
BEGIN
    CREATE INDEX [IX_NotasCreditoCompras_IdCompraAsociada] ON [NotasCreditoCompras] ([IdCompraAsociada]);
    PRINT 'Indice IX_NotasCreditoCompras_IdCompraAsociada creado';
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'NotasCreditoCompras') AND name = 'IX_NotasCreditoCompras_IdDeposito')
BEGIN
    CREATE INDEX [IX_NotasCreditoCompras_IdDeposito] ON [NotasCreditoCompras] ([IdDeposito]);
    PRINT 'Indice IX_NotasCreditoCompras_IdDeposito creado';
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'NotasCreditoCompras') AND name = 'IX_NotasCreditoCompras_IdMoneda')
BEGIN
    CREATE INDEX [IX_NotasCreditoCompras_IdMoneda] ON [NotasCreditoCompras] ([IdMoneda]);
    PRINT 'Indice IX_NotasCreditoCompras_IdMoneda creado';
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'NotasCreditoCompras') AND name = 'IX_NotasCreditoCompras_IdProveedor')
BEGIN
    CREATE INDEX [IX_NotasCreditoCompras_IdProveedor] ON [NotasCreditoCompras] ([IdProveedor]);
    PRINT 'Indice IX_NotasCreditoCompras_IdProveedor creado';
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'NotasCreditoCompras') AND name = 'IX_NotasCreditoCompras_IdUsuario')
BEGIN
    CREATE INDEX [IX_NotasCreditoCompras_IdUsuario] ON [NotasCreditoCompras] ([IdUsuario]);
    PRINT 'Indice IX_NotasCreditoCompras_IdUsuario creado';
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'NotasCreditoComprasDetalles') AND name = 'IX_NotasCreditoComprasDetalles_IdDepositoItem')
BEGIN
    CREATE INDEX [IX_NotasCreditoComprasDetalles_IdDepositoItem] ON [NotasCreditoComprasDetalles] ([IdDepositoItem]);
    PRINT 'Indice IX_NotasCreditoComprasDetalles_IdDepositoItem creado';
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'NotasCreditoComprasDetalles') AND name = 'IX_NotasCreditoComprasDetalles_IdNotaCreditoCompra')
BEGIN
    CREATE INDEX [IX_NotasCreditoComprasDetalles_IdNotaCreditoCompra] ON [NotasCreditoComprasDetalles] ([IdNotaCreditoCompra]);
    PRINT 'Indice IX_NotasCreditoComprasDetalles_IdNotaCreditoCompra creado';
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'NotasCreditoComprasDetalles') AND name = 'IX_NotasCreditoComprasDetalles_IdProducto')
BEGIN
    CREATE INDEX [IX_NotasCreditoComprasDetalles_IdProducto] ON [NotasCreditoComprasDetalles] ([IdProducto]);
    PRINT 'Indice IX_NotasCreditoComprasDetalles_IdProducto creado';
END

-- ============================================
-- 9. Registrar migraciones en historial
-- ============================================
IF NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '20251230135900_Agregar_NCCompra_PrecioMinisterio_Configuracion')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251230135900_Agregar_NCCompra_PrecioMinisterio_Configuracion', N'8.0.0');
    PRINT 'Migracion 20251230135900 registrada';
END
ELSE
BEGIN
    PRINT 'Migracion 20251230135900 ya estaba registrada';
END

IF NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '20251230140633_Agregar_PrecioMinisterio_CompraDetalle')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251230140633_Agregar_PrecioMinisterio_CompraDetalle', N'8.0.0');
    PRINT 'Migracion 20251230140633 registrada';
END
ELSE
BEGIN
    PRINT 'Migracion 20251230140633 ya estaba registrada';
END

COMMIT;

PRINT '';
PRINT '=== MIGRACIONES APLICADAS CORRECTAMENTE ===';
