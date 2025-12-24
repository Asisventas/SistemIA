-- Script para insertar datos de ejemplo en el sistema
USE [asiswebapp]
GO

-- Insertar más usuarios de ejemplo
INSERT INTO Usuarios (Nombres, Apellidos, Cedula, Email, Password, Id_Rol, Estado) VALUES 
('Juan Carlos', 'Pérez González', '12345679', 'juan.perez@empresa.com', 'password123', 1, 1),
('María Elena', 'Rodríguez Silva', '98765432', 'maria.rodriguez@empresa.com', 'password123', 1, 1),
('Pedro Luis', 'Martínez López', '11223344', 'pedro.martinez@empresa.com', 'password123', 1, 1),
('Ana Sofía', 'García Morales', '55667788', 'ana.garcia@empresa.com', 'password123', 1, 1),
('Carlos Eduardo', 'Fernández Ruiz', '99887766', 'carlos.fernandez@empresa.com', 'password123', 1, 1);

-- Insertar más sucursales
INSERT INTO Sucursal (NombreSucursal, Direccion, Telefono, Email, ToleranciaEntradaMinutos, ToleranciaSalidaMinutos) VALUES 
('Sucursal Centro', 'Av. España 123', '021-123456', 'centro@empresa.com', 15, 15),
('Sucursal Shopping', 'Shopping del Sol Local 45', '021-789012', 'shopping@empresa.com', 10, 10);

-- Insertar registros de asistencia de ejemplo
DECLARE @FechaHoy DATETIME2 = GETDATE()
DECLARE @FechaAyer DATETIME2 = DATEADD(DAY, -1, GETDATE())

-- Asistencias del día anterior
INSERT INTO Asistencias (Id_Usuario, Sucursal, FechaHora, TipoRegistro, Notas, MetodoRegistro, EsRegistroAutomatico, EstadoPuntualidad, DiferenciaMinutos, RequiereJustificacion) VALUES
(2, 1, DATEADD(HOUR, 8, CAST(CAST(@FechaAyer AS DATE) AS DATETIME2)), 'Entrada', 'Reconocimiento Facial', 'Facial', 1, 'Puntual', 0, 0),
(3, 1, DATEADD(MINUTE, 15, DATEADD(HOUR, 8, CAST(CAST(@FechaAyer AS DATE) AS DATETIME2))), 'Entrada', 'Reconocimiento Facial', 'Facial', 1, 'Tardanza', 15, 1),
(4, 1, DATEADD(MINUTE, -10, DATEADD(HOUR, 8, CAST(CAST(@FechaAyer AS DATE) AS DATETIME2))), 'Entrada', 'Reconocimiento Facial', 'Facial', 1, 'Adelanto', -10, 0),
(2, 1, DATEADD(HOUR, 17, CAST(CAST(@FechaAyer AS DATE) AS DATETIME2)), 'Salida', 'Salida puntual', 'Facial', 1, 'Puntual', 0, 0),
(3, 1, DATEADD(MINUTE, 30, DATEADD(HOUR, 17, CAST(CAST(@FechaAyer AS DATE) AS DATETIME2))), 'Salida', 'Tiempo extra', 'Facial', 1, 'TiempoExtra', 30, 0);

-- Asistencias del día actual
INSERT INTO Asistencias (Id_Usuario, Sucursal, FechaHora, TipoRegistro, Notas, MetodoRegistro, EsRegistroAutomatico, EstadoPuntualidad, DiferenciaMinutos, RequiereJustificacion) VALUES
(2, 1, DATEADD(MINUTE, 5, DATEADD(HOUR, 8, CAST(CAST(@FechaHoy AS DATE) AS DATETIME2))), 'Entrada', 'Reconocimiento Facial', 'Facial', 1, 'Tardanza', 5, 0),
(4, 1, DATEADD(HOUR, 8, CAST(CAST(@FechaHoy AS DATE) AS DATETIME2)), 'Entrada', 'Reconocimiento Facial', 'Facial', 1, 'Puntual', 0, 0),
(5, 1, DATEADD(MINUTE, 20, DATEADD(HOUR, 8, CAST(CAST(@FechaHoy AS DATE) AS DATETIME2))), 'Entrada', 'Registro manual', 'Manual', 0, 'Tardanza', 20, 1);

PRINT 'Datos de ejemplo insertados correctamente'
