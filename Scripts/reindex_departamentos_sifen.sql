/*
Reindexa los códigos (Numero) de la tabla departamento para alinear con SIFEN.
- No depende de ON UPDATE CASCADE.
- Inserta filas destino (NewCodigo) si no existen, actualiza FKs (distrito/ciudad),
  y elimina las filas con OldCodigo.
- Realiza chequeos de conflicto y aborta si hay colisiones (mismo código con otro nombre).

Revise el mapeo antes de ejecutar en producción.
*/
SET XACT_ABORT ON;
DECLARE @DoCommit bit = 1; -- Cambie a 0 para dry-run (no commitea)
BEGIN TRANSACTION;

-- Mapeo (nombre, codigo BD actual, codigo SIFEN deseado)
DECLARE @Map TABLE (
    Nombre sysname NOT NULL,
    OldCodigo int NOT NULL,
    NewCodigo int NOT NULL,
    PRIMARY KEY (OldCodigo)
);

INSERT INTO @Map (Nombre, OldCodigo, NewCodigo) VALUES
('CORDILLERA', 4, 3),
('GUAIRA', 5, 4),
('CAAGUAZU', 6, 5),
('CAAZAPA', 7, 6),
('ITAPUA', 8, 7),
('MISIONES', 9, 8),
('PARAGUARI', 10, 9),
('ALTO PARANA', 11, 10),
('CENTRAL', 12, 11),
('NEEMBUCU', 13, 12),
('AMAMBAY', 14, 13),
('CANINDEYU', 18, 14);

/* Chequeos previos */
-- 1) Verificar que todos los OldCodigo existan y correspondan por nombre (tolerante a mayúsculas/minúsculas)
IF EXISTS (
    SELECT 1
    FROM @Map m
    LEFT JOIN departamento d ON d.Numero = m.OldCodigo
    WHERE d.Numero IS NULL OR UPPER(LTRIM(RTRIM(d.Nombre))) <> UPPER(LTRIM(RTRIM(m.Nombre)))
)
BEGIN
    RAISERROR('Conflicto: algún OldCodigo no existe o el nombre no coincide. Revise @Map vs tabla departamento.', 16, 1);
    ROLLBACK TRANSACTION;
    RETURN;
END

-- 2) Discrepancias de nombre en NewCodigo: se resolverán actualizando el nombre destino
DECLARE @NombreDiffs TABLE (CodigoDestino int, NombreExistente nvarchar(200), NombreEsperado nvarchar(200));
INSERT INTO @NombreDiffs(CodigoDestino, NombreExistente, NombreEsperado)
SELECT m.NewCodigo, d.Nombre, m.Nombre
FROM @Map m
JOIN departamento d ON d.Numero = m.NewCodigo
WHERE UPPER(LTRIM(RTRIM(d.Nombre))) <> UPPER(LTRIM(RTRIM(m.Nombre)));

IF EXISTS (SELECT 1 FROM @NombreDiffs)
BEGIN
    PRINT 'Hay discrepancias de nombre en códigos destino (se actualizarán):';
    DECLARE @msg nvarchar(4000);
    SELECT @msg = STRING_AGG(CONCAT('Destino ', CodigoDestino, ': existe "', NombreExistente, '" -> esperado "', NombreEsperado, '"'), CHAR(10)) FROM @NombreDiffs;
    IF @msg IS NOT NULL PRINT @msg;
END

-- 3) Detectar FKs adicionales a departamento.Numero distintas de distrito/ciudad y prepararlas para actualización
DECLARE @RefAll TABLE (SchemaName sysname, TableName sysname, ColumnName sysname, FKName sysname, KeyOrder int);
INSERT INTO @RefAll (SchemaName, TableName, ColumnName, FKName, KeyOrder)
SELECT OBJECT_SCHEMA_NAME(fk.parent_object_id), OBJECT_NAME(fk.parent_object_id),
       COL_NAME(fkc.parent_object_id, fkc.parent_column_id), fk.name, fkc.constraint_column_id
FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
JOIN sys.columns rc ON rc.object_id = fk.referenced_object_id AND rc.column_id = fkc.referenced_column_id
WHERE OBJECT_NAME(fk.referenced_object_id) = 'departamento'
  AND rc.name = 'Numero'
  AND OBJECT_NAME(fk.parent_object_id) NOT IN ('distrito','ciudad');

DECLARE @HasMultiCol bit = 0;
IF EXISTS (
    SELECT 1 FROM (
        SELECT SchemaName, TableName, FKName, COUNT(*) AS cnt
        FROM @RefAll
        GROUP BY SchemaName, TableName, FKName
    ) x WHERE x.cnt > 1
)
    SET @HasMultiCol = 1;

IF @HasMultiCol = 1
BEGIN
    PRINT 'Se detectaron FKs multicolumna hacia departamento.Numero (no soportadas).';
    SELECT SchemaName, TableName, FKName FROM (
        SELECT SchemaName, TableName, FKName, COUNT(*) AS cnt
        FROM @RefAll
        GROUP BY SchemaName, TableName, FKName
    ) x WHERE x.cnt > 1;

    RAISERROR('Existen FKs multicolumna a departamento.Numero. Abortando.', 16, 1);
    ROLLBACK TRANSACTION;
    RETURN;
END

DECLARE @RefCols TABLE (SchemaName sysname, TableName sysname, ColumnName sysname);
INSERT INTO @RefCols (SchemaName, TableName, ColumnName)
SELECT DISTINCT SchemaName, TableName, ColumnName FROM @RefAll;

IF EXISTS (SELECT 1 FROM @RefCols)
BEGIN
    PRINT 'Se actualizarán FKs adicionales a departamento.Numero en:';
    DECLARE @list nvarchar(max) = (
        SELECT STRING_AGG(CONCAT(QUOTENAME(SchemaName),'.',QUOTENAME(TableName),'(',QUOTENAME(ColumnName),')'), CHAR(10)) FROM @RefCols
    );
    IF @list IS NOT NULL PRINT @list;
END

-- Hacer disponible el mapeo para SQL dinámico
IF OBJECT_ID('tempdb..#Map') IS NOT NULL DROP TABLE #Map;
CREATE TABLE #Map (OldCodigo int PRIMARY KEY, NewCodigo int NOT NULL);
INSERT INTO #Map(OldCodigo, NewCodigo)
SELECT OldCodigo, NewCodigo FROM @Map;

/* Resumen de impacto previo */
DECLARE @AfectaDistritos int = (SELECT COUNT(*) FROM distrito di JOIN @Map m ON m.OldCodigo = di.Departamento AND di.Departamento <> m.NewCodigo);
DECLARE @AfectaCiudades int  = (SELECT COUNT(*) FROM ciudad   c  JOIN @Map m ON m.OldCodigo = c.Departamento  AND c.Departamento  <> m.NewCodigo);
PRINT CONCAT('Distritos a actualizar: ', @AfectaDistritos);
PRINT CONCAT('Ciudades a actualizar: ',  @AfectaCiudades);
-- Vista previa (opcional)
SELECT TOP 10 'distrito' AS Tabla, di.Departamento AS OldDepto, m.NewCodigo AS NewDepto, di.Numero AS Id, di.Nombre
FROM distrito di JOIN @Map m ON m.OldCodigo = di.Departamento AND di.Departamento <> m.NewCodigo
UNION ALL
SELECT TOP 10 'ciudad', c.Departamento, m.NewCodigo, c.Numero, c.Nombre
FROM ciudad c JOIN @Map m ON m.OldCodigo = c.Departamento AND c.Departamento <> m.NewCodigo;

/* Fase 1: asegurar fila destino (insertar si falta) */
INSERT INTO departamento (Numero, Nombre)
SELECT m.NewCodigo, d.Nombre
FROM @Map m
JOIN departamento d ON d.Numero = m.OldCodigo
LEFT JOIN departamento dx ON dx.Numero = m.NewCodigo
WHERE dx.Numero IS NULL; -- sólo si no existe ya

/* Fase 2: actualizar FKs de hijos hacia NewCodigo */
-- ciudad primero (por posible FK compuesta Ciudad(Departamento,Distrito) -> Distrito)
UPDATE c
SET c.Departamento = m.NewCodigo
FROM ciudad c
JOIN @Map m ON m.OldCodigo = c.Departamento
WHERE c.Departamento <> m.NewCodigo;

-- distrito después
UPDATE di
SET di.Departamento = m.NewCodigo
FROM distrito di
JOIN @Map m ON m.OldCodigo = di.Departamento
WHERE di.Departamento <> m.NewCodigo;

/* Fase 2b: Alinear nombres de destino según SIFEN */
UPDATE d
SET d.Nombre = m.Nombre
FROM departamento d
JOIN @Map m ON m.NewCodigo = d.Numero
WHERE UPPER(LTRIM(RTRIM(d.Nombre))) <> UPPER(LTRIM(RTRIM(m.Nombre)));

/* Fase 2c: Actualizar FKs adicionales de una sola columna */
DECLARE @schema sysname, @tbl sysname, @col sysname;
DECLARE cur CURSOR FAST_FORWARD FOR
SELECT SchemaName, TableName, ColumnName FROM @RefCols;
OPEN cur;
FETCH NEXT FROM cur INTO @schema, @tbl, @col;
WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @sql nvarchar(max) = N'UPDATE t SET ' + QUOTENAME(@col) + N' = m.NewCodigo ' +
        N'FROM ' + QUOTENAME(@schema) + N'.' + QUOTENAME(@tbl) + N' AS t ' +
        N'JOIN #Map AS m ON m.OldCodigo = t.' + QUOTENAME(@col) + N' WHERE t.' + QUOTENAME(@col) + N' <> m.NewCodigo;';
    EXEC (@sql);
    FETCH NEXT FROM cur INTO @schema, @tbl, @col;
END
CLOSE cur; DEALLOCATE cur;

/* Fase 3: Validar que no queden referencias a OldCodigo */
DECLARE @PendDistritos int = (SELECT COUNT(*) FROM distrito di JOIN @Map m ON m.OldCodigo = di.Departamento);
DECLARE @PendCiudades int  = (SELECT COUNT(*) FROM ciudad   c  JOIN @Map m ON m.OldCodigo = c.Departamento);

DECLARE @RefPend nvarchar(max) = N'';
DECLARE @cnt int;
DECLARE @schema2 sysname, @tbl2 sysname, @col2 sysname;
DECLARE cur2 CURSOR FAST_FORWARD FOR SELECT SchemaName, TableName, ColumnName FROM @RefCols;
OPEN cur2;
FETCH NEXT FROM cur2 INTO @schema2, @tbl2, @col2;
WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @q nvarchar(max) = N'SELECT @o = COUNT(*) FROM ' + QUOTENAME(@schema2)+N'.'+QUOTENAME(@tbl2) + N' WHERE '+QUOTENAME(@col2)+N' IN (SELECT OldCodigo FROM #Map)';
    SET @cnt = 0;
    EXEC sp_executesql @q, N'@o int OUTPUT', @o=@cnt OUTPUT;
    IF @cnt > 0 SET @RefPend = CONCAT(@RefPend, CHAR(10), QUOTENAME(@schema2),'.',QUOTENAME(@tbl2),'(',QUOTENAME(@col2),'): ', @cnt);
    FETCH NEXT FROM cur2 INTO @schema2, @tbl2, @col2;
END
CLOSE cur2; DEALLOCATE cur2;

IF (@PendDistritos > 0 OR @PendCiudades > 0 OR LEN(@RefPend) > 0)
BEGIN
    PRINT CONCAT('Referencias pendientes - Distritos: ', @PendDistritos, ' | Ciudades: ', @PendCiudades);
    IF LEN(@RefPend) > 0 PRINT CONCAT('Otras referencias:', @RefPend);

    PRINT 'Detalle pendientes (TOP 20) - Distritos:';
    SELECT TOP 20 di.Numero, di.Nombre, di.Departamento AS OldDepto
    FROM distrito di
    WHERE di.Departamento IN (SELECT OldCodigo FROM #Map)
    ORDER BY di.Departamento, di.Numero;

    PRINT 'Detalle pendientes (TOP 20) - Ciudades:';
    SELECT TOP 20 c.Numero, c.Nombre, c.Departamento AS OldDepto
    FROM ciudad c
    WHERE c.Departamento IN (SELECT OldCodigo FROM #Map)
    ORDER BY c.Departamento, c.Numero;

    PRINT 'Pendientes por OldCodigo (distrito):';
    SELECT di.Departamento AS OldCodigo, COUNT(*) AS Cantidad
    FROM distrito di
    WHERE di.Departamento IN (SELECT OldCodigo FROM #Map)
    GROUP BY di.Departamento
    ORDER BY OldCodigo;

    PRINT 'Pendientes por OldCodigo (ciudad):';
    SELECT c.Departamento AS OldCodigo, COUNT(*) AS Cantidad
    FROM ciudad c
    WHERE c.Departamento IN (SELECT OldCodigo FROM #Map)
    GROUP BY c.Departamento
    ORDER BY OldCodigo;

    IF @DoCommit = 1
    BEGIN
        PRINT 'Advertencia: Persisten referencias a departamentos antiguos. Se hará COMMIT sin eliminar departamentos antiguos.';
        COMMIT TRANSACTION;
        RETURN;
    END
    ELSE
    BEGIN
        RAISERROR('Persisten referencias a departamentos antiguos. Dry-run: haciendo ROLLBACK.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END

/* Fase 4: eliminar filas antiguas de departamento (OldCodigo) */
IF @DoCommit = 1
BEGIN
    DELETE d
    FROM departamento d
    JOIN @Map m ON m.OldCodigo = d.Numero
    WHERE d.Numero <> m.NewCodigo; -- seguridad extra
END

/* Resumen */
PRINT 'Reindex finalizado.';
SELECT Numero, Nombre FROM departamento ORDER BY Numero;

IF @DoCommit = 1
    COMMIT TRANSACTION;
ELSE
BEGIN
    PRINT 'Dry-run activado: ejecutando ROLLBACK.';
    ROLLBACK TRANSACTION;
END
