-- Script para crear tablas de Monedas, Tipos de Cambio y Listas de Precios
-- SistemIA - Sistema de gestión integral

-- =============================================
-- Tabla: Monedas
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Monedas')
BEGIN
    CREATE TABLE [dbo].[Monedas] (
        [IdMoneda] int IDENTITY(1,1) NOT NULL,
        [CodigoISO] nvarchar(3) NOT NULL,
        [Nombre] nvarchar(50) NOT NULL,
        [Simbolo] nvarchar(10) NOT NULL,
        [EsMonedaBase] bit NOT NULL DEFAULT(0),
        [Estado] bit NOT NULL DEFAULT(1),
        [Orden] int NOT NULL DEFAULT(0),
        [FechaCreacion] datetime2(7) NOT NULL DEFAULT(GETDATE()),
        [UsuarioCreacion] nvarchar(50) NOT NULL,
        [FechaModificacion] datetime2(7) NULL,
        [UsuarioModificacion] nvarchar(50) NULL,
        CONSTRAINT [PK_Monedas] PRIMARY KEY CLUSTERED ([IdMoneda] ASC),
        CONSTRAINT [UQ_Monedas_CodigoISO] UNIQUE ([CodigoISO])
    );
    
    PRINT 'Tabla Monedas creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'Tabla Monedas ya existe.';
END

-- =============================================
-- Tabla: TiposCambio
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TiposCambio')
BEGIN
    CREATE TABLE [dbo].[TiposCambio] (
        [IdTipoCambio] int IDENTITY(1,1) NOT NULL,
        [IdMonedaOrigen] int NOT NULL,
        [IdMonedaDestino] int NOT NULL,
        [TasaCambio] decimal(18,6) NOT NULL,
        [FechaTipoCambio] datetime2(7) NOT NULL DEFAULT(GETDATE()),
        [Fuente] nvarchar(100) NULL,
        [EsAutomatico] bit NOT NULL DEFAULT(1),
        [Estado] bit NOT NULL DEFAULT(1),
        [FechaCreacion] datetime2(7) NOT NULL DEFAULT(GETDATE()),
        [UsuarioCreacion] nvarchar(50) NOT NULL,
        [FechaModificacion] datetime2(7) NULL,
        [UsuarioModificacion] nvarchar(50) NULL,
        CONSTRAINT [PK_TiposCambio] PRIMARY KEY CLUSTERED ([IdTipoCambio] ASC),
        CONSTRAINT [FK_TiposCambio_MonedaOrigen] FOREIGN KEY([IdMonedaOrigen]) REFERENCES [dbo].[Monedas] ([IdMoneda]),
        CONSTRAINT [FK_TiposCambio_MonedaDestino] FOREIGN KEY([IdMonedaDestino]) REFERENCES [dbo].[Monedas] ([IdMoneda])
    );
    
    -- Índices para optimizar consultas
    CREATE INDEX [IX_TiposCambio_FechaTipoCambio] ON [dbo].[TiposCambio] ([FechaTipoCambio]);
    CREATE INDEX [IX_TiposCambio_Estado] ON [dbo].[TiposCambio] ([Estado]);
    CREATE UNIQUE INDEX [IX_TiposCambio_Unique] ON [dbo].[TiposCambio] ([IdMonedaOrigen], [IdMonedaDestino], [FechaTipoCambio]) WHERE [Estado] = 1;
    
    PRINT 'Tabla TiposCambio creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'Tabla TiposCambio ya existe.';
END

-- =============================================
-- Tabla: ListasPrecios
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ListasPrecios')
BEGIN
    CREATE TABLE [dbo].[ListasPrecios] (
        [IdListaPrecio] int IDENTITY(1,1) NOT NULL,
        [Nombre] nvarchar(100) NOT NULL,
        [Descripcion] nvarchar(255) NULL,
        [IdMoneda] int NOT NULL,
        [EsPredeterminada] bit NOT NULL DEFAULT(0),
        [Estado] bit NOT NULL DEFAULT(1),
        [AplicarDescuentoGlobal] bit NOT NULL DEFAULT(0),
        [PorcentajeDescuento] decimal(5,2) NOT NULL DEFAULT(0),
        [FechaVigenciaDesde] datetime2(7) NULL,
        [FechaVigenciaHasta] datetime2(7) NULL,
        [Orden] int NOT NULL DEFAULT(0),
        [FechaCreacion] datetime2(7) NOT NULL DEFAULT(GETDATE()),
        [UsuarioCreacion] nvarchar(50) NOT NULL,
        [FechaModificacion] datetime2(7) NULL,
        [UsuarioModificacion] nvarchar(50) NULL,
        CONSTRAINT [PK_ListasPrecios] PRIMARY KEY CLUSTERED ([IdListaPrecio] ASC),
        CONSTRAINT [FK_ListasPrecios_Moneda] FOREIGN KEY([IdMoneda]) REFERENCES [dbo].[Monedas] ([IdMoneda])
    );
    
    -- Índices para optimizar consultas
    CREATE INDEX [IX_ListasPrecios_Estado] ON [dbo].[ListasPrecios] ([Estado]);
    CREATE INDEX [IX_ListasPrecios_Vigencia] ON [dbo].[ListasPrecios] ([FechaVigenciaDesde], [FechaVigenciaHasta]);
    
    PRINT 'Tabla ListasPrecios creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'Tabla ListasPrecios ya existe.';
END

-- =============================================
-- Tabla: ListasPreciosDetalles
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ListasPreciosDetalles')
BEGIN
    CREATE TABLE [dbo].[ListasPreciosDetalles] (
        [IdDetalle] int IDENTITY(1,1) NOT NULL,
        [IdListaPrecio] int NOT NULL,
        [CodigoProducto] nvarchar(50) NOT NULL,
        [Precio] decimal(18,2) NOT NULL,
        [PrecioAnterior] decimal(18,2) NULL,
        [FechaUltimaActualizacion] datetime2(7) NOT NULL DEFAULT(GETDATE()),
        [AplicarDescuento] bit NOT NULL DEFAULT(1),
        [DescuentoEspecial] decimal(5,2) NOT NULL DEFAULT(0),
        [Estado] bit NOT NULL DEFAULT(1),
        [FechaCreacion] datetime2(7) NOT NULL DEFAULT(GETDATE()),
        [UsuarioCreacion] nvarchar(50) NOT NULL,
        [FechaModificacion] datetime2(7) NULL,
        [UsuarioModificacion] nvarchar(50) NULL,
        CONSTRAINT [PK_ListasPreciosDetalles] PRIMARY KEY CLUSTERED ([IdDetalle] ASC),
        CONSTRAINT [FK_ListasPreciosDetalles_ListaPrecio] FOREIGN KEY([IdListaPrecio]) REFERENCES [dbo].[ListasPrecios] ([IdListaPrecio]),
        CONSTRAINT [UQ_ListasPreciosDetalles] UNIQUE ([IdListaPrecio], [CodigoProducto])
    );
    
    -- Índices para optimizar consultas
    CREATE INDEX [IX_ListasPreciosDetalles_CodigoProducto] ON [dbo].[ListasPreciosDetalles] ([CodigoProducto]);
    CREATE INDEX [IX_ListasPreciosDetalles_Estado] ON [dbo].[ListasPreciosDetalles] ([Estado]);
    CREATE INDEX [IX_ListasPreciosDetalles_FechaActualizacion] ON [dbo].[ListasPreciosDetalles] ([FechaUltimaActualizacion]);
    
    PRINT 'Tabla ListasPreciosDetalles creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'Tabla ListasPreciosDetalles ya existe.';
END

-- =============================================
-- DATOS INICIALES: Monedas
-- =============================================
IF NOT EXISTS (SELECT 1 FROM [dbo].[Monedas] WHERE [CodigoISO] = 'PYG')
BEGIN
    INSERT INTO [dbo].[Monedas] ([CodigoISO], [Nombre], [Simbolo], [EsMonedaBase], [Estado], [Orden], [UsuarioCreacion])
    VALUES 
        ('PYG', 'Guaraní Paraguayo', '₲', 1, 1, 1, 'Sistema'),
        ('USD', 'Dólar Estadounidense', '$', 0, 1, 2, 'Sistema'),
        ('ARS', 'Peso Argentino', '$', 0, 1, 3, 'Sistema'),
        ('BRL', 'Real Brasileño', 'R$', 0, 1, 4, 'Sistema');
    
    PRINT 'Monedas iniciales insertadas: PYG (Guaraní), USD (Dólar), ARS (Peso Argentino), BRL (Real Brasileño).';
END
ELSE
BEGIN
    PRINT 'Las monedas iniciales ya existen.';
END

-- =============================================
-- DATOS INICIALES: Listas de Precios
-- =============================================
DECLARE @IdMonedaPYG int = (SELECT IdMoneda FROM [dbo].[Monedas] WHERE CodigoISO = 'PYG');
DECLARE @IdMonedaUSD int = (SELECT IdMoneda FROM [dbo].[Monedas] WHERE CodigoISO = 'USD');
DECLARE @IdMonedaARS int = (SELECT IdMoneda FROM [dbo].[Monedas] WHERE CodigoISO = 'ARS');
DECLARE @IdMonedaBRL int = (SELECT IdMoneda FROM [dbo].[Monedas] WHERE CodigoISO = 'BRL');

IF NOT EXISTS (SELECT 1 FROM [dbo].[ListasPrecios] WHERE [Nombre] = 'Lista General Guaraníes')
BEGIN
    INSERT INTO [dbo].[ListasPrecios] ([Nombre], [Descripcion], [IdMoneda], [EsPredeterminada], [Estado], [Orden], [UsuarioCreacion])
    VALUES 
        ('Lista General Guaraníes', 'Lista de precios principal en Guaraníes Paraguayos', @IdMonedaPYG, 1, 1, 1, 'Sistema'),
        ('Lista Precios USD', 'Lista de precios en Dólares Estadounidenses', @IdMonedaUSD, 0, 1, 2, 'Sistema'),
        ('Lista Precios ARS', 'Lista de precios en Pesos Argentinos', @IdMonedaARS, 0, 1, 3, 'Sistema'),
        ('Lista Precios BRL', 'Lista de precios en Reales Brasileños', @IdMonedaBRL, 0, 1, 4, 'Sistema');
    
    PRINT 'Listas de precios iniciales creadas para todas las monedas.';
END
ELSE
BEGIN
    PRINT 'Las listas de precios iniciales ya existen.';
END

-- =============================================
-- DATOS DE EJEMPLO: Tipos de Cambio
-- =============================================
-- Insertar tipos de cambio de ejemplo (se actualizarán automáticamente desde la API)
IF NOT EXISTS (SELECT 1 FROM [dbo].[TiposCambio])
BEGIN
    INSERT INTO [dbo].[TiposCambio] ([IdMonedaOrigen], [IdMonedaDestino], [TasaCambio], [FechaTipoCambio], [Fuente], [EsAutomatico], [UsuarioCreacion])
    VALUES 
        -- USD a otras monedas (valores aproximados, se actualizarán desde API)
        (@IdMonedaUSD, @IdMonedaPYG, 7200.00, GETDATE(), 'Valor inicial - Se actualizará desde API', 1, 'Sistema'),
        (@IdMonedaUSD, @IdMonedaARS, 1000.00, GETDATE(), 'Valor inicial - Se actualizará desde API', 1, 'Sistema'),
        (@IdMonedaUSD, @IdMonedaBRL, 5.50, GETDATE(), 'Valor inicial - Se actualizará desde API', 1, 'Sistema'),
        
        -- PYG a otras monedas
        (@IdMonedaPYG, @IdMonedaUSD, 0.000139, GETDATE(), 'Valor inicial - Se actualizará desde API', 1, 'Sistema'),
        (@IdMonedaPYG, @IdMonedaARS, 0.139, GETDATE(), 'Valor inicial - Se actualizará desde API', 1, 'Sistema'),
        (@IdMonedaPYG, @IdMonedaBRL, 0.000764, GETDATE(), 'Valor inicial - Se actualizará desde API', 1, 'Sistema');
    
    PRINT 'Tipos de cambio iniciales insertados (se actualizarán automáticamente desde APIs).';
END
ELSE
BEGIN
    PRINT 'Los tipos de cambio ya existen.';
END

PRINT '==========================================';
PRINT 'Script completado exitosamente.';
PRINT 'Tablas creadas: Monedas, TiposCambio, ListasPrecios, ListasPreciosDetalles';
PRINT 'Datos iniciales: 4 monedas, 4 listas de precios, tipos de cambio de ejemplo';
PRINT 'El sistema está listo para gestionar precios en múltiples monedas.';
PRINT '==========================================';
