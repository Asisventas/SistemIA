-- =====================================================
-- SCRIPT DE DIAGNÓSTICO DE PRODUCTOS - SistemIA
-- Ejecutar en la BD del cliente para detectar problemas
-- con precios, factores y cálculos
-- =====================================================

USE [SISTEMIA_2025]  -- Ajustar nombre de BD si es diferente
GO

PRINT '========================================='
PRINT '1. ESTRUCTURA DE TABLA PRODUCTOS'
PRINT '========================================='

-- Verificar columnas críticas existen
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    NUMERIC_PRECISION,
    NUMERIC_SCALE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Productos'
  AND COLUMN_NAME IN ('CostoUnitarioGs', 'PrecioUnitarioGs', 'FactorMultiplicador', 'IdMonedaPrecio', 'PrecioUnitarioUsd')
ORDER BY ORDINAL_POSITION;

PRINT ''
PRINT '========================================='
PRINT '2. PRODUCTOS SIN PRECIO O CON PRECIO CERO'
PRINT '========================================='

SELECT 
    IdProducto,
    CodigoInterno,
    LEFT(Descripcion, 50) AS Descripcion,
    CostoUnitarioGs,
    PrecioUnitarioGs,
    FactorMultiplicador,
    IdMonedaPrecio,
    Activo
FROM Productos
WHERE PrecioUnitarioGs = 0 
   OR PrecioUnitarioGs IS NULL
   OR CostoUnitarioGs = 0
   OR CostoUnitarioGs IS NULL
ORDER BY Descripcion;

PRINT ''
PRINT '========================================='
PRINT '3. PRODUCTOS CON FACTOR PERO SIN PRECIO CALCULADO'
PRINT '========================================='

-- Detectar productos donde el factor * costo != precio
SELECT 
    IdProducto,
    CodigoInterno,
    LEFT(Descripcion, 50) AS Descripcion,
    CostoUnitarioGs,
    FactorMultiplicador,
    PrecioUnitarioGs AS PrecioActual,
    ROUND(ISNULL(CostoUnitarioGs, 0) * ISNULL(FactorMultiplicador, 1), 0) AS PrecioEsperado,
    ROUND(PrecioUnitarioGs - (ISNULL(CostoUnitarioGs, 0) * ISNULL(FactorMultiplicador, 1)), 0) AS Diferencia
FROM Productos
WHERE FactorMultiplicador IS NOT NULL 
  AND FactorMultiplicador > 0
  AND CostoUnitarioGs IS NOT NULL
  AND CostoUnitarioGs > 0
  AND ABS(PrecioUnitarioGs - (CostoUnitarioGs * FactorMultiplicador)) > 1 -- tolerancia de 1 Gs
ORDER BY ABS(PrecioUnitarioGs - (CostoUnitarioGs * FactorMultiplicador)) DESC;

PRINT ''
PRINT '========================================='
PRINT '4. RESUMEN GENERAL DE PRODUCTOS'
PRINT '========================================='

SELECT 
    COUNT(*) AS TotalProductos,
    SUM(CASE WHEN Activo = 1 THEN 1 ELSE 0 END) AS Activos,
    SUM(CASE WHEN PrecioUnitarioGs > 0 THEN 1 ELSE 0 END) AS ConPrecio,
    SUM(CASE WHEN CostoUnitarioGs > 0 THEN 1 ELSE 0 END) AS ConCosto,
    SUM(CASE WHEN FactorMultiplicador IS NOT NULL AND FactorMultiplicador > 0 THEN 1 ELSE 0 END) AS ConFactor,
    SUM(CASE WHEN PrecioUnitarioGs = 0 OR PrecioUnitarioGs IS NULL THEN 1 ELSE 0 END) AS SinPrecio,
    SUM(CASE WHEN CostoUnitarioGs = 0 OR CostoUnitarioGs IS NULL THEN 1 ELSE 0 END) AS SinCosto
FROM Productos;

PRINT ''
PRINT '========================================='
PRINT '5. VERIFICAR TIPOS DE IVA'
PRINT '========================================='

SELECT 
    t.IdTipoIva,
    t.Nombre,
    t.Porcentaje,
    t.CodigoSifen,
    COUNT(p.IdProducto) AS CantidadProductos
FROM TiposIva t
LEFT JOIN Productos p ON p.IdTipoIva = t.IdTipoIva
GROUP BY t.IdTipoIva, t.Nombre, t.Porcentaje, t.CodigoSifen
ORDER BY t.IdTipoIva;

PRINT ''
PRINT '========================================='
PRINT '6. ÚLTIMAS COMPRAS Y SU IMPACTO EN PRECIOS'
PRINT '========================================='

SELECT TOP 20
    c.IdCompra,
    c.Fecha,
    c.NumeroFactura,
    cd.IdProducto,
    LEFT(p.Descripcion, 40) AS Producto,
    cd.PrecioUnitario AS CostoCompra,
    cd.FactorMultiplicador AS FactorCompra,
    p.CostoUnitarioGs AS CostoActualProducto,
    p.PrecioUnitarioGs AS PrecioActualProducto,
    p.FactorMultiplicador AS FactorActualProducto
FROM Compras c
JOIN ComprasDetalles cd ON cd.IdCompra = c.IdCompra
JOIN Productos p ON p.IdProducto = cd.IdProducto
ORDER BY c.Fecha DESC, c.IdCompra DESC;

PRINT ''
PRINT '========================================='
PRINT '7. VERIFICAR MONEDAS DISPONIBLES'
PRINT '========================================='

SELECT 
    IdMoneda,
    Nombre,
    CodigoISO,
    Simbolo,
    EsMonedaBase,
    Activa
FROM Monedas
ORDER BY IdMoneda;

PRINT ''
PRINT '========================================='
PRINT '8. PRODUCTOS CON MONEDA DIFERENTE A BASE'
PRINT '========================================='

SELECT 
    p.IdProducto,
    p.CodigoInterno,
    LEFT(p.Descripcion, 50) AS Descripcion,
    p.IdMonedaPrecio,
    m.Nombre AS NombreMoneda,
    m.CodigoISO,
    p.CostoUnitarioGs,
    p.PrecioUnitarioGs,
    p.PrecioUnitarioUsd
FROM Productos p
LEFT JOIN Monedas m ON m.IdMoneda = p.IdMonedaPrecio
WHERE p.IdMonedaPrecio IS NOT NULL
ORDER BY p.IdMonedaPrecio, p.Descripcion;

PRINT ''
PRINT '========================================='
PRINT '9. SCRIPT PARA CORREGIR PRECIOS POR FACTOR'
PRINT '========================================='
PRINT '-- EJECUTAR CON CUIDADO - SOLO SI ES NECESARIO --'
PRINT ''

-- Este es un script de ejemplo para corregir. NO ejecutar automáticamente.
SELECT 
    'UPDATE Productos SET PrecioUnitarioGs = ' + 
    CAST(ROUND(ISNULL(CostoUnitarioGs, 0) * ISNULL(FactorMultiplicador, 1), 0) AS VARCHAR) + 
    ' WHERE IdProducto = ' + CAST(IdProducto AS VARCHAR) + ';' AS ScriptCorreccion
FROM Productos
WHERE FactorMultiplicador IS NOT NULL 
  AND FactorMultiplicador > 0
  AND CostoUnitarioGs IS NOT NULL
  AND CostoUnitarioGs > 0
  AND ABS(PrecioUnitarioGs - (CostoUnitarioGs * FactorMultiplicador)) > 1;

PRINT ''
PRINT '========================================='
PRINT 'FIN DEL DIAGNÓSTICO'
PRINT '========================================='
