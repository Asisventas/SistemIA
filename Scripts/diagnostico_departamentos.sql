-- Diagnóstico de columnas y conteos de referencias a departamentos antiguos
SET NOCOUNT ON;
PRINT '=== Diagnóstico columnas ciudad/distrito ===';
IF COL_LENGTH('dbo.ciudad','Departamento') IS NOT NULL PRINT 'ciudad.Departamento existe';
IF COL_LENGTH('dbo.ciudad','IdDepartamento') IS NOT NULL PRINT 'ciudad.IdDepartamento existe';
IF COL_LENGTH('dbo.ciudad','DepartamentoId') IS NOT NULL PRINT 'ciudad.DepartamentoId existe';
IF COL_LENGTH('dbo.distrito','Departamento') IS NOT NULL PRINT 'distrito.Departamento existe';
IF COL_LENGTH('dbo.distrito','IdDepartamento') IS NOT NULL PRINT 'distrito.IdDepartamento existe';
IF COL_LENGTH('dbo.distrito','DepartamentoId') IS NOT NULL PRINT 'distrito.DepartamentoId existe';

-- Construir #Map rápidamente según mapeo actual del script de reindex
IF OBJECT_ID('tempdb..#Map') IS NOT NULL DROP TABLE #Map;
CREATE TABLE #Map (OldCodigo int PRIMARY KEY, NewCodigo int NOT NULL);
INSERT INTO #Map(OldCodigo, NewCodigo) VALUES
(4,3),(5,4),(6,5),(7,6),(8,7),(9,8),(10,9),(11,10),(12,11),(13,12),(14,13),(18,14);

DECLARE @cnt int;
IF COL_LENGTH('dbo.ciudad','Departamento') IS NOT NULL
BEGIN
    SET @cnt = 0;
    EXEC sp_executesql N'SELECT @o = COUNT(*) FROM dbo.ciudad WHERE Departamento IN (SELECT OldCodigo FROM #Map)', N'@o int OUTPUT', @o=@cnt OUTPUT;
    PRINT CONCAT('ciudad.Departamento pendientes: ', @cnt);
END
IF COL_LENGTH('dbo.ciudad','IdDepartamento') IS NOT NULL
BEGIN
    SET @cnt = 0;
    EXEC sp_executesql N'SELECT @o = COUNT(*) FROM dbo.ciudad WHERE IdDepartamento IN (SELECT OldCodigo FROM #Map)', N'@o int OUTPUT', @o=@cnt OUTPUT;
    PRINT CONCAT('ciudad.IdDepartamento pendientes: ', @cnt);
END
IF COL_LENGTH('dbo.ciudad','DepartamentoId') IS NOT NULL
BEGIN
    SET @cnt = 0;
    EXEC sp_executesql N'SELECT @o = COUNT(*) FROM dbo.ciudad WHERE DepartamentoId IN (SELECT OldCodigo FROM #Map)', N'@o int OUTPUT', @o=@cnt OUTPUT;
    PRINT CONCAT('ciudad.DepartamentoId pendientes: ', @cnt);
END

IF COL_LENGTH('dbo.distrito','Departamento') IS NOT NULL
BEGIN
    SET @cnt = 0;
    EXEC sp_executesql N'SELECT @o = COUNT(*) FROM dbo.distrito WHERE Departamento IN (SELECT OldCodigo FROM #Map)', N'@o int OUTPUT', @o=@cnt OUTPUT;
    PRINT CONCAT('distrito.Departamento pendientes: ', @cnt);
END
IF COL_LENGTH('dbo.distrito','IdDepartamento') IS NOT NULL
BEGIN
    SET @cnt = 0;
    EXEC sp_executesql N'SELECT @o = COUNT(*) FROM dbo.distrito WHERE IdDepartamento IN (SELECT OldCodigo FROM #Map)', N'@o int OUTPUT', @o=@cnt OUTPUT;
    PRINT CONCAT('distrito.IdDepartamento pendientes: ', @cnt);
END
IF COL_LENGTH('dbo.distrito','DepartamentoId') IS NOT NULL
BEGIN
    SET @cnt = 0;
    EXEC sp_executesql N'SELECT @o = COUNT(*) FROM dbo.distrito WHERE DepartamentoId IN (SELECT OldCodigo FROM #Map)', N'@o int OUTPUT', @o=@cnt OUTPUT;
    PRINT CONCAT('distrito.DepartamentoId pendientes: ', @cnt);
END
