BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251230135900_Agregar_NCCompra_PrecioMinisterio_Configuracion'
)
BEGIN
    ALTER TABLE [Productos] ADD [PrecioMinisterio] decimal(18,4) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251230135900_Agregar_NCCompra_PrecioMinisterio_Configuracion'
)
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251230135900_Agregar_NCCompra_PrecioMinisterio_Configuracion'
)
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
        CONSTRAINT [PK_NotasCreditoCompras] PRIMARY KEY ([IdNotaCreditoCompra]),
        CONSTRAINT [FK_NotasCreditoCompras_Cajas_IdCaja] FOREIGN KEY ([IdCaja]) REFERENCES [Cajas] ([id_caja]) ON DELETE NO ACTION,
        CONSTRAINT [FK_NotasCreditoCompras_Compras_IdCompraAsociada] FOREIGN KEY ([IdCompraAsociada]) REFERENCES [Compras] ([IdCompra]) ON DELETE NO ACTION,
        CONSTRAINT [FK_NotasCreditoCompras_Depositos_IdDeposito] FOREIGN KEY ([IdDeposito]) REFERENCES [Depositos] ([IdDeposito]) ON DELETE NO ACTION,
        CONSTRAINT [FK_NotasCreditoCompras_Monedas_IdMoneda] FOREIGN KEY ([IdMoneda]) REFERENCES [Monedas] ([IdMoneda]) ON DELETE NO ACTION,
        CONSTRAINT [FK_NotasCreditoCompras_ProveedoresSifen_IdProveedor] FOREIGN KEY ([IdProveedor]) REFERENCES [ProveedoresSifen] ([IdProveedor]) ON DELETE NO ACTION,
        CONSTRAINT [FK_NotasCreditoCompras_Sucursal_IdSucursal] FOREIGN KEY ([IdSucursal]) REFERENCES [Sucursal] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_NotasCreditoCompras_Usuarios_IdUsuario] FOREIGN KEY ([IdUsuario]) REFERENCES [Usuarios] ([Id_Usu]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251230135900_Agregar_NCCompra_PrecioMinisterio_Configuracion'
)
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
        CONSTRAINT [PK_NotasCreditoComprasDetalles] PRIMARY KEY ([IdNotaCreditoCompraDetalle]),
        CONSTRAINT [FK_NotasCreditoComprasDetalles_Depositos_IdDepositoItem] FOREIGN KEY ([IdDepositoItem]) REFERENCES [Depositos] ([IdDeposito]) ON DELETE NO ACTION,
        CONSTRAINT [FK_NotasCreditoComprasDetalles_NotasCreditoCompras_IdNotaCreditoCompra] FOREIGN KEY ([IdNotaCreditoCompra]) REFERENCES [NotasCreditoCompras] ([IdNotaCreditoCompra]) ON DELETE CASCADE,
        CONSTRAINT [FK_NotasCreditoComprasDetalles_Productos_IdProducto] FOREIGN KEY ([IdProducto]) REFERENCES [Productos] ([IdProducto]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251230135900_Agregar_NCCompra_PrecioMinisterio_Configuracion'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_NotaCreditoCompras_Numeracion] ON [NotasCreditoCompras] ([IdSucursal], [NumeroNota]) WHERE [NumeroNota] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251230135900_Agregar_NCCompra_PrecioMinisterio_Configuracion'
)
BEGIN
    CREATE INDEX [IX_NotasCreditoCompras_IdCaja] ON [NotasCreditoCompras] ([IdCaja]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251230135900_Agregar_NCCompra_PrecioMinisterio_Configuracion'
)
BEGIN
    CREATE INDEX [IX_NotasCreditoCompras_IdCompraAsociada] ON [NotasCreditoCompras] ([IdCompraAsociada]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251230135900_Agregar_NCCompra_PrecioMinisterio_Configuracion'
)
BEGIN
    CREATE INDEX [IX_NotasCreditoCompras_IdDeposito] ON [NotasCreditoCompras] ([IdDeposito]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251230135900_Agregar_NCCompra_PrecioMinisterio_Configuracion'
)
BEGIN
    CREATE INDEX [IX_NotasCreditoCompras_IdMoneda] ON [NotasCreditoCompras] ([IdMoneda]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251230135900_Agregar_NCCompra_PrecioMinisterio_Configuracion'
)
BEGIN
    CREATE INDEX [IX_NotasCreditoCompras_IdProveedor] ON [NotasCreditoCompras] ([IdProveedor]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251230135900_Agregar_NCCompra_PrecioMinisterio_Configuracion'
)
BEGIN
    CREATE INDEX [IX_NotasCreditoCompras_IdUsuario] ON [NotasCreditoCompras] ([IdUsuario]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251230135900_Agregar_NCCompra_PrecioMinisterio_Configuracion'
)
BEGIN
    CREATE INDEX [IX_NotasCreditoComprasDetalles_IdDepositoItem] ON [NotasCreditoComprasDetalles] ([IdDepositoItem]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251230135900_Agregar_NCCompra_PrecioMinisterio_Configuracion'
)
BEGIN
    CREATE INDEX [IX_NotasCreditoComprasDetalles_IdNotaCreditoCompra] ON [NotasCreditoComprasDetalles] ([IdNotaCreditoCompra]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251230135900_Agregar_NCCompra_PrecioMinisterio_Configuracion'
)
BEGIN
    CREATE INDEX [IX_NotasCreditoComprasDetalles_IdProducto] ON [NotasCreditoComprasDetalles] ([IdProducto]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251230135900_Agregar_NCCompra_PrecioMinisterio_Configuracion'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251230135900_Agregar_NCCompra_PrecioMinisterio_Configuracion', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251230140633_Agregar_PrecioMinisterio_CompraDetalle'
)
BEGIN
    ALTER TABLE [ComprasDetalles] ADD [PrecioMinisterio] decimal(18,4) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251230140633_Agregar_PrecioMinisterio_CompraDetalle'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251230140633_Agregar_PrecioMinisterio_CompraDetalle', N'8.0.0');
END;
GO

COMMIT;
GO

