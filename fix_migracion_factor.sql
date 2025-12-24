-- Script para arreglar migración FactorMultiplicador en BD remota
-- Ejecutar en SQL Server Management Studio conectado a 192.168.100.219\SQL2022, BD: Sistema

-- 1. Eliminar registro de migración vacía (si existe)
DELETE FROM __EFMigrationsHistory WHERE MigrationId = '20251223011802_Agregar_Factor_Compras_Producto';
GO

-- 2. Agregar columna FactorMultiplicador a Productos (si no existe)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Productos') AND name = 'FactorMultiplicador')
BEGIN
    ALTER TABLE Productos ADD FactorMultiplicador DECIMAL(18,4) NULL;
    PRINT 'Columna FactorMultiplicador agregada a Productos';
END
ELSE
    PRINT 'Columna FactorMultiplicador ya existe en Productos';
GO

-- 3. Agregar columna FactorMultiplicador a ComprasDetalles (si no existe)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'ComprasDetalles') AND name = 'FactorMultiplicador')
BEGIN
    ALTER TABLE ComprasDetalles ADD FactorMultiplicador DECIMAL(18,4) NULL;
    PRINT 'Columna FactorMultiplicador agregada a ComprasDetalles';
END
ELSE
    PRINT 'Columna FactorMultiplicador ya existe en ComprasDetalles';
GO

-- 4. Agregar columna PorcentajeMargen a ComprasDetalles (si no existe)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'ComprasDetalles') AND name = 'PorcentajeMargen')
BEGIN
    ALTER TABLE ComprasDetalles ADD PorcentajeMargen DECIMAL(18,4) NULL;
    PRINT 'Columna PorcentajeMargen agregada a ComprasDetalles';
END
ELSE
    PRINT 'Columna PorcentajeMargen ya existe en ComprasDetalles';
GO

-- 5. Registrar la migración como aplicada
IF NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '20251223011802_Agregar_Factor_Compras_Producto')
BEGIN
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) 
    VALUES ('20251223011802_Agregar_Factor_Compras_Producto', '8.0.0');
    PRINT 'Migración registrada en __EFMigrationsHistory';
END
ELSE
    PRINT 'La migración ya estaba registrada';
GO

-- 6. Verificar resultado
SELECT 'Columnas en Productos:' AS Info;
SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Productos' AND COLUMN_NAME = 'FactorMultiplicador';

SELECT 'Columnas en ComprasDetalles:' AS Info;
SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'ComprasDetalles' AND COLUMN_NAME IN ('FactorMultiplicador', 'PorcentajeMargen');

SELECT 'Migración registrada:' AS Info;
SELECT MigrationId FROM __EFMigrationsHistory WHERE MigrationId LIKE '%Factor%';

PRINT '=== SCRIPT COMPLETADO ===';
