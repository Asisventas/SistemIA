-- Verificar últimas migraciones
SELECT TOP 5 MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId DESC;

-- Verificar columnas Paquete en Productos
SELECT name FROM sys.columns WHERE object_id = OBJECT_ID('Productos') AND name LIKE '%Paquete%';
