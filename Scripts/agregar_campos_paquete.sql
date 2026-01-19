-- Script para agregar campos de precios de paquete al modelo Productos
-- Ejecutar solo si las columnas no existen

-- CostoPaqueteGs: Costo de compra por paquete completo
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Productos' AND COLUMN_NAME = 'CostoPaqueteGs')
BEGIN
    ALTER TABLE Productos ADD CostoPaqueteGs decimal(18,4) NULL;
    PRINT 'Columna CostoPaqueteGs agregada';
END
ELSE
BEGIN
    PRINT 'Columna CostoPaqueteGs ya existe';
END
GO

-- FactorPaquete: Factor multiplicador para calcular precio de paquete
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Productos' AND COLUMN_NAME = 'FactorPaquete')
BEGIN
    ALTER TABLE Productos ADD FactorPaquete decimal(18,4) NULL;
    PRINT 'Columna FactorPaquete agregada';
END
ELSE
BEGIN
    PRINT 'Columna FactorPaquete ya existe';
END
GO

-- MarkupPaquetePct: Porcentaje de mark-up para el paquete
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Productos' AND COLUMN_NAME = 'MarkupPaquetePct')
BEGIN
    ALTER TABLE Productos ADD MarkupPaquetePct decimal(5,2) NULL;
    PRINT 'Columna MarkupPaquetePct agregada';
END
ELSE
BEGIN
    PRINT 'Columna MarkupPaquetePct ya existe';
END
GO

-- PrecioPaqueteGs: Precio de venta por paquete completo
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Productos' AND COLUMN_NAME = 'PrecioPaqueteGs')
BEGIN
    ALTER TABLE Productos ADD PrecioPaqueteGs decimal(18,4) NULL;
    PRINT 'Columna PrecioPaqueteGs agregada';
END
ELSE
BEGIN
    PRINT 'Columna PrecioPaqueteGs ya existe';
END
GO

-- PrecioMinisterioPaquete: Precio máximo regulado por el Ministerio para el paquete
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Productos' AND COLUMN_NAME = 'PrecioMinisterioPaquete')
BEGIN
    ALTER TABLE Productos ADD PrecioMinisterioPaquete decimal(18,4) NULL;
    PRINT 'Columna PrecioMinisterioPaquete agregada';
END
ELSE
BEGIN
    PRINT 'Columna PrecioMinisterioPaquete ya existe';
END
GO

PRINT '=== Script de campos de paquete completado ==='
GO
