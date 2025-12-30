-- Script para cargar datos de RUC desde DNIT (versión mejorada)
-- Formato entrada: RUC|NOMBRE|DV|CODIGO|ESTADO|
-- Solo guardamos: RUC, NOMBRE, DV, ESTADO

-- Paso 1: Crear tabla temporal para importación de líneas raw
IF OBJECT_ID('tempdb..#RucRaw') IS NOT NULL DROP TABLE #RucRaw;
CREATE TABLE #RucRaw (
    Linea NVARCHAR(500)
);

-- Paso 2: Cargar cada archivo (ajustar rutas si es necesario)
BULK INSERT #RucRaw FROM 'C:\asis\SistemIA\DatosRUC\ruc0.txt' WITH (CODEPAGE = '65001', ROWTERMINATOR = '\n');
BULK INSERT #RucRaw FROM 'C:\asis\SistemIA\DatosRUC\ruc1.txt' WITH (CODEPAGE = '65001', ROWTERMINATOR = '\n');
BULK INSERT #RucRaw FROM 'C:\asis\SistemIA\DatosRUC\ruc2.txt' WITH (CODEPAGE = '65001', ROWTERMINATOR = '\n');
BULK INSERT #RucRaw FROM 'C:\asis\SistemIA\DatosRUC\ruc4.txt' WITH (CODEPAGE = '65001', ROWTERMINATOR = '\n');
BULK INSERT #RucRaw FROM 'C:\asis\SistemIA\DatosRUC\ruc6.txt' WITH (CODEPAGE = '65001', ROWTERMINATOR = '\n');
BULK INSERT #RucRaw FROM 'C:\asis\SistemIA\DatosRUC\ruc7.txt' WITH (CODEPAGE = '65001', ROWTERMINATOR = '\n');
BULK INSERT #RucRaw FROM 'C:\asis\SistemIA\DatosRUC\ruc8.txt' WITH (CODEPAGE = '65001', ROWTERMINATOR = '\n');
BULK INSERT #RucRaw FROM 'C:\asis\SistemIA\DatosRUC\ruc9.txt' WITH (CODEPAGE = '65001', ROWTERMINATOR = '\n');

SELECT 'Registros raw cargados: ' + CAST(COUNT(*) AS VARCHAR(20)) FROM #RucRaw;

-- Paso 3: Limpiar tabla destino
TRUNCATE TABLE RucDnit;

-- Paso 4: Parsear con CHARINDEX e insertar
;WITH Parsed AS (
    SELECT 
        Linea,
        -- Posición de cada pipe
        CHARINDEX('|', Linea) AS P1,
        CHARINDEX('|', Linea, CHARINDEX('|', Linea) + 1) AS P2,
        CHARINDEX('|', Linea, CHARINDEX('|', Linea, CHARINDEX('|', Linea) + 1) + 1) AS P3,
        CHARINDEX('|', Linea, CHARINDEX('|', Linea, CHARINDEX('|', Linea, CHARINDEX('|', Linea) + 1) + 1) + 1) AS P4,
        CHARINDEX('|', Linea, CHARINDEX('|', Linea, CHARINDEX('|', Linea, CHARINDEX('|', Linea, CHARINDEX('|', Linea) + 1) + 1) + 1) + 1) AS P5
    FROM #RucRaw
    WHERE Linea IS NOT NULL AND LEN(Linea) > 5
)
INSERT INTO RucDnit (RUC, RazonSocial, DV, Estado, FechaActualizacion)
SELECT 
    LTRIM(RTRIM(LEFT(Linea, P1 - 1))) AS RUC,                                           -- Campo 1
    LTRIM(RTRIM(SUBSTRING(Linea, P1 + 1, P2 - P1 - 1))) AS RazonSocial,                -- Campo 2
    ISNULL(TRY_CAST(LTRIM(RTRIM(SUBSTRING(Linea, P2 + 1, P3 - P2 - 1))) AS INT), 0),   -- Campo 3 (DV)
    LTRIM(RTRIM(SUBSTRING(Linea, P4 + 1, CASE WHEN P5 > 0 THEN P5 - P4 - 1 ELSE LEN(Linea) - P4 END))) AS Estado, -- Campo 5
    GETDATE()
FROM Parsed
WHERE P1 > 0 AND P2 > P1 AND P3 > P2;

SELECT 'Registros insertados en RucDnit: ' + CAST(COUNT(*) AS VARCHAR(20)) FROM RucDnit;

-- Verificar muestra
SELECT TOP 20 * FROM RucDnit ORDER BY RUC;

-- Limpiar
DROP TABLE #RucRaw;
