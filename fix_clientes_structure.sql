-- Script para corregir tipos de datos en la tabla Clientes
-- Basado en el modelo Cliente.cs

USE asiswebapp;
GO

-- Cambiar el campo Estado de bit a bit (ya está correcto, pero asegurar que no acepta NULL)
ALTER TABLE Clientes ALTER COLUMN Estado bit NOT NULL;

-- Cambiar el campo RUC para que acepte hasta 50 caracteres (actualmente es varchar(8))
ALTER TABLE Clientes ALTER COLUMN RUC varchar(50) NOT NULL;

-- Cambiar el campo TipoOperacion para que sea varchar(1) (actualmente puede ser más largo)
ALTER TABLE Clientes ALTER COLUMN TipoOperacion varchar(1) NULL;

-- Asegurar que los campos booleanos estén correctamente definidos
ALTER TABLE Clientes ALTER COLUMN PrecioDiferenciado bit NOT NULL;
ALTER TABLE Clientes ALTER COLUMN EsExtranjero bit NOT NULL;

-- Verificar las longitudes correctas según el modelo
-- RazonSocial: 250 caracteres
ALTER TABLE Clientes ALTER COLUMN RazonSocial varchar(250) NOT NULL;

-- TipoDocumento: 2 caracteres
ALTER TABLE Clientes ALTER COLUMN TipoDocumento varchar(2) NOT NULL;

-- Direccion: 150 caracteres
ALTER TABLE Clientes ALTER COLUMN Direccion varchar(150) NULL;

-- Telefono: 20 caracteres
ALTER TABLE Clientes ALTER COLUMN Telefono varchar(20) NULL;

-- Email: 150 caracteres
ALTER TABLE Clientes ALTER COLUMN Email varchar(150) NULL;

-- Contacto: 100 caracteres
ALTER TABLE Clientes ALTER COLUMN Contacto varchar(100) NULL;

-- Timbrado: 8 caracteres
ALTER TABLE Clientes ALTER COLUMN Timbrado varchar(8) NULL;

-- CodigoPais: 3 caracteres
ALTER TABLE Clientes ALTER COLUMN CodigoPais varchar(3) NOT NULL;

PRINT 'Estructura de la tabla Clientes actualizada correctamente.';
