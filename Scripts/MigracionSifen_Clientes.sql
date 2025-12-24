-- Script SQL para agregar campos requeridos por SIFEN al modelo Cliente existente
-- Basado en Manual Técnico SIFEN v150

USE [SistemIA]
GO

-- Agregar campos para cumplir con especificaciones SIFEN
ALTER TABLE [Clientes] ADD 
    -- Campo para iNatRec (Naturaleza del receptor)
    [NaturalezaReceptor] INT NOT NULL DEFAULT 1,
    
    -- Campos geográficos requeridos por SIFEN
    [CodigoDepartamento] NVARCHAR(2) NULL,
    [DescripcionDepartamento] NVARCHAR(16) NULL,
    [CodigoDistrito] NVARCHAR(4) NULL,
    [DescripcionDistrito] NVARCHAR(30) NULL,
    
    -- Campo adicional para nombre de fantasía
    [NombreFantasia] NVARCHAR(255) NULL,
    
    -- Campos para no contribuyentes
    [TipoDocumentoIdentidadSifen] INT NULL,
    [NumeroDocumentoIdentidad] NVARCHAR(20) NULL;

GO

-- Agregar comentarios a los campos
EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Naturaleza del receptor SIFEN: 1=Contribuyente, 2=No contribuyente', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Clientes', 
    @level2type = N'COLUMN', @level2name = N'NaturalezaReceptor';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Código del departamento según XSD SIFEN (cDepRec)', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Clientes', 
    @level2type = N'COLUMN', @level2name = N'CodigoDepartamento';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Descripción del departamento SIFEN (dDesDepRec)', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Clientes', 
    @level2type = N'COLUMN', @level2name = N'DescripcionDepartamento';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Código del distrito según XSD SIFEN (cDisRec)', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Clientes', 
    @level2type = N'COLUMN', @level2name = N'CodigoDistrito';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Descripción del distrito SIFEN (dDesDisRec)', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Clientes', 
    @level2type = N'COLUMN', @level2name = N'DescripcionDistrito';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Nombre de fantasía del receptor SIFEN (dNomFanRec)', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Clientes', 
    @level2type = N'COLUMN', @level2name = N'NombreFantasia';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Tipo de documento de identidad para no contribuyentes SIFEN (iTipIDRec)', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Clientes', 
    @level2type = N'COLUMN', @level2name = N'TipoDocumentoIdentidadSifen';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Número de documento de identidad para no contribuyentes SIFEN (dNumIDRec)', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Clientes', 
    @level2type = N'COLUMN', @level2name = N'NumeroDocumentoIdentidad';

GO

-- Actualizar clientes existentes con valores por defecto apropiados
UPDATE [Clientes] 
SET 
    [NaturalezaReceptor] = CASE 
        WHEN RUC IS NOT NULL AND LEN(RUC) > 2 AND RUC != '0' THEN 1 -- Contribuyente
        ELSE 2 -- No contribuyente
    END;

GO

-- Crear índices para mejorar rendimiento en consultas SIFEN
CREATE NONCLUSTERED INDEX [IX_Clientes_NaturalezaReceptor] 
ON [Clientes] ([NaturalezaReceptor]);

CREATE NONCLUSTERED INDEX [IX_Clientes_CodigoPais] 
ON [Clientes] ([CodigoPais]);

CREATE NONCLUSTERED INDEX [IX_Clientes_RUC_DV] 
ON [Clientes] ([RUC], [DV]);

GO

-- Crear vista para facilitar consultas SIFEN
CREATE VIEW [vw_ClientesSifen] AS
SELECT 
    c.IdCliente,
    c.CodigoCliente,
    c.RazonSocial,
    c.RUC,
    c.DV,
    c.TipoDocumento,
    c.NumeroDocumento,
    c.NaturalezaReceptor,
    c.Direccion,
    c.NumeroCasa,
    c.CodigoDepartamento,
    c.DescripcionDepartamento,
    c.CodigoDistrito,
    c.DescripcionDistrito,
    c.Telefono,
    c.Email,
    c.CodigoPais,
    p.NombrePais,
    c.IdCiudad,
    ci.Nombre as NombreCiudad,
    c.IdTipoContribuyente,
    tc.Descripcion as TipoContribuyenteDescripcion,
    c.TipoOperacion,
    c.NombreFantasia,
    c.TipoDocumentoIdentidadSifen,
    c.NumeroDocumentoIdentidad,
    -- Campos calculados para SIFEN
    CASE 
        WHEN c.RUC IS NOT NULL AND LEN(c.RUC) > 2 AND c.RUC != '0' THEN 1 
        ELSE 2 
    END as iNatRec,
    CASE 
        WHEN c.TipoOperacion = '1' THEN 1
        WHEN c.TipoOperacion = '2' THEN 2
        WHEN c.TipoOperacion = '3' THEN 3
        WHEN c.TipoOperacion = '4' THEN 4
        ELSE 1
    END as iTiOpe,
    c.CodigoPais as cPaisRec,
    p.NombrePais as dDesPaisRe,
    c.IdTipoContribuyente as iTiContRec,
    c.RUC as dRucRec,
    c.DV as dDVRec,
    c.RazonSocial as dNomRec,
    c.Direccion as dDirRec,
    c.NumeroCasa as dNumCasRec,
    c.CodigoDepartamento as cDepRec,
    c.DescripcionDepartamento as dDesDepRec,
    c.CodigoDistrito as cDisRec,
    c.DescripcionDistrito as dDesDisRec,
    c.IdCiudad as cCiuRec,
    ci.Nombre as dDesCiuRec,
    c.Telefono as dTelRec,
    c.CodigoCliente as dCodCliente,
    c.NombreFantasia as dNomFanRec,
    c.TipoDocumentoIdentidadSifen as iTipIDRec,
    c.NumeroDocumentoIdentidad as dNumIDRec
FROM [Clientes] c
LEFT JOIN [Paises] p ON c.CodigoPais = p.CodigoPais
LEFT JOIN [ciudad] ci ON c.IdCiudad = ci.Numero
LEFT JOIN [TiposContribuyentes] tc ON c.IdTipoContribuyente = tc.Id;

GO

-- Crear función para validar datos SIFEN
CREATE FUNCTION [dbo].[fn_ValidarClienteSifen](@IdCliente INT)
RETURNS TABLE
AS
RETURN
(
    SELECT 
        c.IdCliente,
        c.RazonSocial,
        c.RUC,
        CASE 
            WHEN c.RazonSocial IS NULL OR LEN(LTRIM(RTRIM(c.RazonSocial))) = 0 THEN 0
            WHEN c.CodigoPais IS NULL OR LEN(LTRIM(RTRIM(c.CodigoPais))) = 0 THEN 0
            WHEN c.IdTipoContribuyente <= 0 THEN 0
            WHEN c.NaturalezaReceptor = 1 AND (c.RUC IS NULL OR LEN(LTRIM(RTRIM(c.RUC))) = 0) THEN 0
            WHEN c.NaturalezaReceptor = 1 AND c.DV <= 0 THEN 0
            WHEN c.NaturalezaReceptor = 2 AND (c.TipoDocumento IS NULL OR LEN(LTRIM(RTRIM(c.TipoDocumento))) = 0) THEN 0
            WHEN c.NaturalezaReceptor = 2 AND (c.NumeroDocumento IS NULL OR LEN(LTRIM(RTRIM(c.NumeroDocumento))) = 0) THEN 0
            ELSE 1
        END as EsValidoParaSifen,
        CASE 
            WHEN c.RazonSocial IS NULL OR LEN(LTRIM(RTRIM(c.RazonSocial))) = 0 THEN 'Razón social requerida'
            WHEN c.CodigoPais IS NULL OR LEN(LTRIM(RTRIM(c.CodigoPais))) = 0 THEN 'Código de país requerido'
            WHEN c.IdTipoContribuyente <= 0 THEN 'Tipo de contribuyente inválido'
            WHEN c.NaturalezaReceptor = 1 AND (c.RUC IS NULL OR LEN(LTRIM(RTRIM(c.RUC))) = 0) THEN 'RUC requerido para contribuyentes'
            WHEN c.NaturalezaReceptor = 1 AND c.DV <= 0 THEN 'Dígito verificador inválido'
            WHEN c.NaturalezaReceptor = 2 AND (c.TipoDocumento IS NULL OR LEN(LTRIM(RTRIM(c.TipoDocumento))) = 0) THEN 'Tipo de documento requerido para no contribuyentes'
            WHEN c.NaturalezaReceptor = 2 AND (c.NumeroDocumento IS NULL OR LEN(LTRIM(RTRIM(c.NumeroDocumento))) = 0) THEN 'Número de documento requerido para no contribuyentes'
            ELSE 'Válido para SIFEN'
        END as MensajeValidacion
    FROM [Clientes] c
    WHERE c.IdCliente = @IdCliente
);

GO

PRINT 'Script de migración SIFEN completado exitosamente';
PRINT 'Se agregaron los siguientes campos:';
PRINT '- NaturalezaReceptor (iNatRec)';
PRINT '- CodigoDepartamento (cDepRec)';
PRINT '- DescripcionDepartamento (dDesDepRec)';
PRINT '- CodigoDistrito (cDisRec)';
PRINT '- DescripcionDistrito (dDesDisRec)';
PRINT '- NombreFantasia (dNomFanRec)';
PRINT '- TipoDocumentoIdentidadSifen (iTipIDRec)';
PRINT '- NumeroDocumentoIdentidad (dNumIDRec)';
PRINT '';
PRINT 'Se crearon:';
PRINT '- Vista: vw_ClientesSifen';
PRINT '- Función: fn_ValidarClienteSifen';
PRINT '- Índices para optimizar consultas SIFEN';
