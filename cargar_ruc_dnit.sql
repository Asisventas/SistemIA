-- Script para cargar datos de RUC desde DNIT
-- Formato: RUC|NOMBRE|DV|CODIGO|ESTADO|
-- Ejemplo: 913871|SERVIN VILLALBA, LUIS ANTONIO|4|SEVL651200Z|ACTIVO|

-- Crear tabla temporal para importación
IF OBJECT_ID('tempdb..#RucTemp') IS NOT NULL DROP TABLE #RucTemp;
CREATE TABLE #RucTemp (
    Linea NVARCHAR(500)
);

-- Cargar cada archivo usando BULK INSERT
-- Nota: Cambiar las rutas según ubicación real

PRINT 'Cargando ruc0.txt...'
BULK INSERT #RucTemp FROM 'C:\asis\SistemIA\DatosRUC\ruc0.txt' WITH (CODEPAGE = '65001', ROWTERMINATOR = '\n');
PRINT 'Cargando ruc1.txt...'
BULK INSERT #RucTemp FROM 'C:\asis\SistemIA\DatosRUC\ruc1.txt' WITH (CODEPAGE = '65001', ROWTERMINATOR = '\n');
PRINT 'Cargando ruc2.txt...'
BULK INSERT #RucTemp FROM 'C:\asis\SistemIA\DatosRUC\ruc2.txt' WITH (CODEPAGE = '65001', ROWTERMINATOR = '\n');
PRINT 'Cargando ruc4.txt...'
BULK INSERT #RucTemp FROM 'C:\asis\SistemIA\DatosRUC\ruc4.txt' WITH (CODEPAGE = '65001', ROWTERMINATOR = '\n');
PRINT 'Cargando ruc6.txt...'
BULK INSERT #RucTemp FROM 'C:\asis\SistemIA\DatosRUC\ruc6.txt' WITH (CODEPAGE = '65001', ROWTERMINATOR = '\n');
PRINT 'Cargando ruc7.txt...'
BULK INSERT #RucTemp FROM 'C:\asis\SistemIA\DatosRUC\ruc7.txt' WITH (CODEPAGE = '65001', ROWTERMINATOR = '\n');
PRINT 'Cargando ruc8.txt...'
BULK INSERT #RucTemp FROM 'C:\asis\SistemIA\DatosRUC\ruc8.txt' WITH (CODEPAGE = '65001', ROWTERMINATOR = '\n');
PRINT 'Cargando ruc9.txt...'
BULK INSERT #RucTemp FROM 'C:\asis\SistemIA\DatosRUC\ruc9.txt' WITH (CODEPAGE = '65001', ROWTERMINATOR = '\n');

PRINT 'Total registros cargados en temporal:'
SELECT COUNT(*) FROM #RucTemp;

-- Parsear y cargar en tabla final
PRINT 'Parseando e insertando en RucDnit...'

-- Limpiar tabla destino
TRUNCATE TABLE RucDnit;

-- Insertar parseando el pipe-delimited
INSERT INTO RucDnit (RUC, RazonSocial, DV, Estado, FechaActualizacion)
SELECT 
    LTRIM(RTRIM(PARSENAME(REPLACE(Linea, '|', '.'), 6))) AS RUC,
    LTRIM(RTRIM(PARSENAME(REPLACE(Linea, '|', '.'), 5))) AS RazonSocial,
    TRY_CAST(LTRIM(RTRIM(PARSENAME(REPLACE(Linea, '|', '.'), 4))) AS INT) AS DV,
    LTRIM(RTRIM(PARSENAME(REPLACE(Linea, '|', '.'), 2))) AS Estado,
    GETDATE() AS FechaActualizacion
FROM #RucTemp
WHERE Linea IS NOT NULL AND LEN(Linea) > 5;

PRINT 'Registros insertados en RucDnit:'
SELECT COUNT(*) FROM RucDnit;

-- Verificar muestra
SELECT TOP 10 * FROM RucDnit ORDER BY RUC;

DROP TABLE #RucTemp;
