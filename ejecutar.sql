BEGIN TRANSACTION;
;
ALTER TABLE [ComprasDetalles] ADD [PrecioMinisterio] decimal(18,4) NULL;
;
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251230140633_Agregar_PrecioMinisterio_CompraDetalle', N'8.0.0');
;
COMMIT;
;

