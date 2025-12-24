-- Script para corregir la estructura de la tabla Clientes
-- Elimina índices y restricciones problemáticas

USE asiswebapp;
GO

-- Paso 1: Eliminar índices que impiden modificar las columnas
DROP INDEX IF EXISTS IX_Clientes_TipoDocumento ON Clientes;
DROP INDEX IF EXISTS IX_Clientes_IdTipoContribuyente ON Clientes;

-- Paso 2: Eliminar restricciones de clave foránea
ALTER TABLE Clientes DROP CONSTRAINT IF EXISTS FK_Clientes_TiposDocumentosIdentidad_TipoDocumento;
ALTER TABLE Clientes DROP CONSTRAINT IF EXISTS FK_Clientes_TipoOperacion_TipoOperacion;
ALTER TABLE Clientes DROP CONSTRAINT IF EXISTS FK_Clientes_TiposContribuyentes_IdTipoContribuyente;
ALTER TABLE Clientes DROP CONSTRAINT IF EXISTS FK_Clientes_Paises_CodigoPais;
ALTER TABLE Clientes DROP CONSTRAINT IF EXISTS FK_Clientes_Ciudades_IdCiudad;

-- Paso 3: Modificar las columnas según el modelo Cliente.cs
PRINT 'Modificando estructura de columnas...';

-- RUC: cambiar a varchar(50)
ALTER TABLE Clientes ALTER COLUMN RUC varchar(50) NOT NULL;

-- TipoDocumento: asegurar varchar(2)
ALTER TABLE Clientes ALTER COLUMN TipoDocumento varchar(2) NOT NULL;

-- TipoOperacion: cambiar a varchar(1)
ALTER TABLE Clientes ALTER COLUMN TipoOperacion varchar(1) NULL;

-- RazonSocial: asegurar varchar(250)
ALTER TABLE Clientes ALTER COLUMN RazonSocial varchar(250) NOT NULL;

-- Direccion: asegurar varchar(150)
ALTER TABLE Clientes ALTER COLUMN Direccion varchar(150) NULL;

-- Telefono: asegurar varchar(20)
ALTER TABLE Clientes ALTER COLUMN Telefono varchar(20) NULL;

-- Email: asegurar varchar(150)
ALTER TABLE Clientes ALTER COLUMN Email varchar(150) NULL;

-- Contacto: asegurar varchar(100)
ALTER TABLE Clientes ALTER COLUMN Contacto varchar(100) NULL;

-- Timbrado: asegurar varchar(8)
ALTER TABLE Clientes ALTER COLUMN Timbrado varchar(8) NULL;

-- CodigoPais: asegurar varchar(3)
ALTER TABLE Clientes ALTER COLUMN CodigoPais varchar(3) NOT NULL;

-- Asegurar que los campos booleanos estén correctamente definidos
ALTER TABLE Clientes ALTER COLUMN Estado bit NOT NULL;
ALTER TABLE Clientes ALTER COLUMN PrecioDiferenciado bit NOT NULL;
ALTER TABLE Clientes ALTER COLUMN EsExtranjero bit NOT NULL;

-- Paso 4: Recrear índices importantes (sin restricciones de FK por ahora)
CREATE INDEX IX_Clientes_TipoDocumento ON Clientes(TipoDocumento);
CREATE INDEX IX_Clientes_IdTipoContribuyente ON Clientes(IdTipoContribuyente);

PRINT 'Estructura de la tabla Clientes actualizada correctamente.';
