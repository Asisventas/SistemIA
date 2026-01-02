BEGIN TRANSACTION;
GO

ALTER TABLE [ComprasDetalles] ADD [PrecioMinisterio] decimal(18,4) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251230140633_Agregar_PrecioMinisterio_CompraDetalle', N'8.0.0');
GO

COMMIT;
GO

