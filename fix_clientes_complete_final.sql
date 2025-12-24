-- Script completo para corregir la estructura de la tabla Clientes
-- Elimina TODOS los índices y restricciones problemáticas

USE asiswebapp;
GO

PRINT 'Eliminando todos los índices y restricciones...';

-- Eliminar TODOS los índices no primarios
DROP INDEX IF EXISTS IX_Clientes_TipoDocumento ON Clientes;
DROP INDEX IF EXISTS IX_Clientes_IdTipoContribuyente ON Clientes;
DROP INDEX IF EXISTS IX_Clientes_CodigoPais ON Clientes;
DROP INDEX IF EXISTS IX_Clientes_IdCiudad ON Clientes;

-- Eliminar todas las restricciones de clave foránea posibles
DECLARE @sql NVARCHAR(MAX) = '';
SELECT @sql = @sql + 'ALTER TABLE Clientes DROP CONSTRAINT ' + CONSTRAINT_NAME + ';' + CHAR(13)
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
WHERE TABLE_NAME = 'Clientes' AND CONSTRAINT_TYPE = 'FOREIGN KEY';

EXEC sp_executesql @sql;

PRINT 'Modificando estructura de columnas...';

-- Ahora modificar las columnas
ALTER TABLE Clientes ALTER COLUMN RUC varchar(50) NOT NULL;
ALTER TABLE Clientes ALTER COLUMN TipoDocumento varchar(2) NOT NULL;
ALTER TABLE Clientes ALTER COLUMN TipoOperacion varchar(1) NULL;
ALTER TABLE Clientes ALTER COLUMN RazonSocial varchar(250) NOT NULL;
ALTER TABLE Clientes ALTER COLUMN Direccion varchar(150) NULL;
ALTER TABLE Clientes ALTER COLUMN Telefono varchar(20) NULL;
ALTER TABLE Clientes ALTER COLUMN Email varchar(150) NULL;
ALTER TABLE Clientes ALTER COLUMN Contacto varchar(100) NULL;
ALTER TABLE Clientes ALTER COLUMN Timbrado varchar(8) NULL;
ALTER TABLE Clientes ALTER COLUMN CodigoPais varchar(3) NOT NULL;
ALTER TABLE Clientes ALTER COLUMN Estado bit NOT NULL;
ALTER TABLE Clientes ALTER COLUMN PrecioDiferenciado bit NOT NULL;
ALTER TABLE Clientes ALTER COLUMN EsExtranjero bit NOT NULL;

PRINT 'Recreando índices básicos...';

-- Recrear índices básicos
CREATE INDEX IX_Clientes_TipoDocumento ON Clientes(TipoDocumento);
CREATE INDEX IX_Clientes_IdTipoContribuyente ON Clientes(IdTipoContribuyente);
CREATE INDEX IX_Clientes_CodigoPais ON Clientes(CodigoPais);
CREATE INDEX IX_Clientes_IdCiudad ON Clientes(IdCiudad);

PRINT 'Estructura de la tabla Clientes actualizada correctamente.';
PRINT 'NOTA: Las restricciones de clave foránea fueron eliminadas temporalmente.';
PRINT 'Recrear manualmente si es necesario después de verificar que las tablas de referencia existen.';
