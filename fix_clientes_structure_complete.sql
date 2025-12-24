-- Script para corregir tipos de datos en la tabla Clientes
-- Maneja las restricciones de clave foránea

USE asiswebapp;
GO

-- Paso 1: Eliminar las restricciones de clave foránea temporalmente
ALTER TABLE Clientes DROP CONSTRAINT IF EXISTS FK_Clientes_TiposDocumentosIdentidad_TipoDocumento;
ALTER TABLE Clientes DROP CONSTRAINT IF EXISTS FK_Clientes_TipoOperacion_TipoOperacion;
ALTER TABLE Clientes DROP CONSTRAINT IF EXISTS FK_Clientes_TiposContribuyentes_IdTipoContribuyente;
ALTER TABLE Clientes DROP CONSTRAINT IF EXISTS FK_Clientes_Paises_CodigoPais;
ALTER TABLE Clientes DROP CONSTRAINT IF EXISTS FK_Clientes_Ciudades_IdCiudad;

-- Paso 2: Modificar las columnas según el modelo Cliente.cs
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

-- Paso 3: Restaurar las restricciones de clave foránea (si existen las tablas referenciadas)
-- Nota: Solo agregar si las tablas de referencia existen
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TiposDocumentosIdentidad')
BEGIN
    ALTER TABLE Clientes ADD CONSTRAINT FK_Clientes_TiposDocumentosIdentidad_TipoDocumento 
    FOREIGN KEY (TipoDocumento) REFERENCES TiposDocumentosIdentidad(TipoDocumento);
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TipoOperacion')
BEGIN
    ALTER TABLE Clientes ADD CONSTRAINT FK_Clientes_TipoOperacion_TipoOperacion 
    FOREIGN KEY (TipoOperacion) REFERENCES TipoOperacion(TipoOperacion);
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TiposContribuyentes')
BEGIN
    ALTER TABLE Clientes ADD CONSTRAINT FK_Clientes_TiposContribuyentes_IdTipoContribuyente 
    FOREIGN KEY (IdTipoContribuyente) REFERENCES TiposContribuyentes(IdTipoContribuyente);
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Paises')
BEGIN
    ALTER TABLE Clientes ADD CONSTRAINT FK_Clientes_Paises_CodigoPais 
    FOREIGN KEY (CodigoPais) REFERENCES Paises(CodigoPais);
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Ciudades')
BEGIN
    ALTER TABLE Clientes ADD CONSTRAINT FK_Clientes_Ciudades_IdCiudad 
    FOREIGN KEY (IdCiudad) REFERENCES Ciudades(IdCiudad);
END

PRINT 'Estructura de la tabla Clientes actualizada correctamente.';
