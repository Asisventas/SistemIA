-- Agregar columna UrlQrSifen a NotasCreditoVentas si no existe
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'NotasCreditoVentas' AND COLUMN_NAME = 'UrlQrSifen'
)
BEGIN
    ALTER TABLE [NotasCreditoVentas] ADD [UrlQrSifen] nvarchar(max) NULL;
    PRINT 'Columna UrlQrSifen agregada a NotasCreditoVentas';
END
ELSE
BEGIN
    PRINT 'Columna UrlQrSifen ya existe en NotasCreditoVentas';
END
GO

-- Registrar la migración si no está registrada
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260122153527_Add_UrlQrSifen_NotasCreditoVentas'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260122153527_Add_UrlQrSifen_NotasCreditoVentas', N'8.0.0');
    PRINT 'Migración registrada en historial';
END
GO
