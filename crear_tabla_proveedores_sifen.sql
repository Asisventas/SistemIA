-- Script de creación manual para tabla ProveedoresSifen
-- Basado en el modelo ProveedorSifenMejorado compatible con SIFEN Paraguay v150

CREATE TABLE [dbo].[ProveedoresSifen] (
    [IdProveedor] int IDENTITY(1,1) NOT NULL,
    
    -- Campos básicos empresariales
    [CodigoProveedor] nvarchar(50) NULL,
    [RazonSocial] nvarchar(200) NOT NULL,
    [NombreFantasia] nvarchar(200) NULL,
    
    -- Campos tributarios SIFEN (obligatorios para emisor)
    [RUC] nchar(8) NOT NULL,
    [DV] int NOT NULL,
    [TipoContribuyente] int NOT NULL DEFAULT 2,
    [IdTipoContribuyenteCatalogo] int NOT NULL,
    [TipoRegimen] int NOT NULL DEFAULT 1,
    [Timbrado] nchar(8) NOT NULL,
    [VencimientoTimbrado] datetime2 NOT NULL,
    [Establecimiento] nchar(3) NOT NULL DEFAULT '001',
    [PuntoExpedicion] nchar(3) NOT NULL DEFAULT '001',
    
    -- Ubicación geográfica SIFEN
    [Direccion] nvarchar(255) NOT NULL,
    [NumeroCasa] nvarchar(10) NULL,
    [CodigoDepartamento] nvarchar(2) NULL,
    [DescripcionDepartamento] nvarchar(16) NULL,
    [CodigoCiudad] int NOT NULL,
    [DescripcionCiudad] nvarchar(30) NULL,
    
    -- Datos de contacto
    [Telefono] nvarchar(15) NOT NULL,
    [Email] nvarchar(80) NOT NULL,
    [Celular] nvarchar(15) NULL,
    [PersonaContacto] nvarchar(100) NULL,
    
    -- Actividad económica SIFEN
    [CodigoActividadEconomica] nchar(5) NOT NULL,
    [DescripcionActividadEconomica] nvarchar(300) NOT NULL,
    
    -- Campos comerciales y financieros
    [Rubro] nvarchar(100) NULL,
    [LimiteCredito] decimal(18,4) NULL,
    [SaldoPendiente] decimal(18,4) NOT NULL DEFAULT 0,
    [PlazoPagoDias] int NULL,
    [CodigoPais] nchar(3) NOT NULL DEFAULT 'PRY',
    
    -- Certificados SIFEN
    [CertificadoRuta] nvarchar(500) NULL,
    [CertificadoPassword] nvarchar(100) NULL,
    [Ambiente] nvarchar(10) NOT NULL DEFAULT 'test',
    
    -- Control de documentos
    [UltimoNumeroDocumento] bigint NOT NULL DEFAULT 0,
    [UltimaSerie] nvarchar(2) NULL,
    
    -- Campos de auditoría y control
    [Estado] bit NOT NULL DEFAULT 1,
    [FechaCreacion] datetime2 NOT NULL DEFAULT GETDATE(),
    [UsuarioCreacion] nvarchar(100) NULL,
    [FechaModificacion] datetime2 NULL,
    [UsuarioModificacion] nvarchar(100) NULL,
    [FechaUltimaFacturacion] datetime2 NULL,
    
    CONSTRAINT [PK_ProveedoresSifen] PRIMARY KEY ([IdProveedor]),
    CONSTRAINT [FK_ProveedoresSifen_TiposContribuyentes] FOREIGN KEY ([IdTipoContribuyenteCatalogo]) REFERENCES [TiposContribuyentes] ([IdTipoContribuyente]),
    CONSTRAINT [CK_ProveedoresSifen_TipoContribuyente] CHECK ([TipoContribuyente] IN (1, 2)),
    CONSTRAINT [CK_ProveedoresSifen_TipoRegimen] CHECK ([TipoRegimen] IN (1, 2, 3)),
    CONSTRAINT [CK_ProveedoresSifen_DV] CHECK ([DV] >= 0 AND [DV] <= 11),
    CONSTRAINT [UQ_ProveedoresSifen_RUC] UNIQUE ([RUC])
);

-- Índices para optimizar consultas
CREATE INDEX [IX_ProveedoresSifen_RazonSocial] ON [ProveedoresSifen] ([RazonSocial]);
CREATE INDEX [IX_ProveedoresSifen_Estado] ON [ProveedoresSifen] ([Estado]);
CREATE INDEX [IX_ProveedoresSifen_VencimientoTimbrado] ON [ProveedoresSifen] ([VencimientoTimbrado]);
CREATE INDEX [IX_ProveedoresSifen_Ambiente] ON [ProveedoresSifen] ([Ambiente]);

-- Datos de ejemplo (proveedor de prueba)
INSERT INTO [ProveedoresSifen] (
    [CodigoProveedor], [RazonSocial], [RUC], [DV], [TipoContribuyente], [IdTipoContribuyenteCatalogo],
    [TipoRegimen], [Timbrado], [VencimientoTimbrado], [Establecimiento], [PuntoExpedicion],
    [Direccion], [CodigoDepartamento], [DescripcionDepartamento], [CodigoCiudad], [DescripcionCiudad],
    [Telefono], [Email], [CodigoActividadEconomica], [DescripcionActividadEconomica],
    [Ambiente], [UsuarioCreacion]
) VALUES (
    'PROV-20250125-001', 
    'EMPRESA DE PRUEBA SIFEN SA',
    '80000001', 
    9,
    2, -- Persona Jurídica
    2, -- Asumiendo ID 2 para tipo contribuyente
    1, -- Régimen General
    '12345678',
    '2025-12-31',
    '001',
    '001',
    'AVENIDA ESPAÑA 123 C/ ESTADOS UNIDOS',
    '01',
    'CAPITAL',
    1,
    'ASUNCION (DISTRITO)',
    '021123456',
    'facturacion@empresaprueba.com.py',
    '46510',
    'VENTA AL POR MAYOR DE COMPUTADORAS, EQUIPO PERIFÉRICO Y PROGRAMAS INFORMÁTICOS',
    'test',
    'Sistema'
);

PRINT 'Tabla ProveedoresSifen creada exitosamente con datos de ejemplo';
