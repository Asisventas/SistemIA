-- Insertar la migración en el historial de EF Core
-- Esto registra que la migración ya fue aplicada (las columnas ya existen)

IF NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '20260116135504_Agregar_Campos_Precios_Paquete')
BEGIN
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES ('20260116135504_Agregar_Campos_Precios_Paquete', '8.0.11');
    PRINT 'Migración registrada correctamente';
END
ELSE
BEGIN
    PRINT 'La migración ya estaba registrada';
END

-- Verificar
SELECT TOP 5 MigrationId, ProductVersion FROM __EFMigrationsHistory ORDER BY MigrationId DESC;
