-- Script para crear las tablas esenciales del sistema
-- Ejecutar en la base de datos asiswebapp

USE [asiswebapp]
GO

-- Crear tabla Roles
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Roles' AND xtype='U')
CREATE TABLE [dbo].[Roles](
    [Id_Rol] [int] IDENTITY(1,1) NOT NULL,
    [Nombre_Rol] [nvarchar](50) NOT NULL,
    [Descripcion] [nvarchar](255) NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY CLUSTERED ([Id_Rol] ASC)
)
GO

-- Crear tabla Sucursal
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Sucursal' AND xtype='U')
CREATE TABLE [dbo].[Sucursal](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [NombreSucursal] [nvarchar](100) NOT NULL,
    [Direccion] [nvarchar](255) NULL,
    [Telefono] [nvarchar](20) NULL,
    [Email] [nvarchar](100) NULL,
    [Logo] [varbinary](max) NULL,
    [Estado] [bit] NOT NULL DEFAULT 1,
    [FechaCreacion] [datetime2](7) NOT NULL DEFAULT GETDATE(),
    -- Campos de tolerancia para control de asistencia
    [ToleranciaEntradaMinutos] [int] NOT NULL DEFAULT 10,
    [ToleranciaSalidaMinutos] [int] NOT NULL DEFAULT 10,
    [RequiereJustificacionTardanza] [bit] NOT NULL DEFAULT 1,
    [RequiereJustificacionSalidaTemprana] [bit] NOT NULL DEFAULT 1,
    [MaximoHorasExtraDia] [int] NOT NULL DEFAULT 120,
    [CalculoAutomaticoHorasExtra] [bit] NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Sucursal] PRIMARY KEY CLUSTERED ([Id] ASC)
)
GO

-- Crear tabla Usuarios
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Usuarios' AND xtype='U')
CREATE TABLE [dbo].[Usuarios](
    [Id_Usu] [int] IDENTITY(1,1) NOT NULL,
    [Nombres] [nvarchar](100) NOT NULL,
    [Apellidos] [nvarchar](100) NOT NULL,
    [Cedula] [nvarchar](20) NOT NULL,
    [Email] [nvarchar](100) NULL,
    [Telefono] [nvarchar](20) NULL,
    [Password] [nvarchar](255) NOT NULL,
    [Id_Rol] [int] NOT NULL,
    [EmbeddingFacial] [varbinary](max) NULL,
    [FechaCreacion] [datetime2](7) NOT NULL DEFAULT GETDATE(),
    [Estado] [bit] NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Usuarios] PRIMARY KEY CLUSTERED ([Id_Usu] ASC),
    CONSTRAINT [FK_Usuarios_Roles] FOREIGN KEY([Id_Rol]) REFERENCES [dbo].[Roles] ([Id_Rol])
)
GO

-- Crear tabla HorarioTrabajo
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='HorarioTrabajo' AND xtype='U')
CREATE TABLE [dbo].[HorarioTrabajo](
    [Id_Horario] [int] IDENTITY(1,1) NOT NULL,
    [Nombre] [nvarchar](100) NOT NULL,
    [DiaSemana] [int] NOT NULL,
    [HoraEntrada] [time](7) NOT NULL,
    [HoraSalida] [time](7) NOT NULL,
    [EsLaborable] [bit] NOT NULL DEFAULT 1,
    [Id_Sucursal] [int] NOT NULL,
    CONSTRAINT [PK_HorarioTrabajo] PRIMARY KEY CLUSTERED ([Id_Horario] ASC),
    CONSTRAINT [FK_HorarioTrabajo_Sucursal] FOREIGN KEY([Id_Sucursal]) REFERENCES [dbo].[Sucursal] ([Id])
)
GO

-- Crear tabla Asistencias (con todos los campos profesionales)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Asistencias' AND xtype='U')
CREATE TABLE [dbo].[Asistencias](
    [Id_Asistencia] [int] IDENTITY(1,1) NOT NULL,
    [Id_Usuario] [int] NOT NULL,
    [Sucursal] [int] NOT NULL,
    [FechaHora] [datetime2](7) NOT NULL,
    [TipoRegistro] [nvarchar](50) NOT NULL,
    [Notas] [nvarchar](255) NULL,
    [Ubicacion] [nvarchar](50) NULL,
    [FechaInicioAusencia] [datetime2](7) NULL,
    [FechaFinAusencia] [datetime2](7) NULL,
    [MotivoAusencia] [nvarchar](100) NULL,
    [AprobadaPorGerencia] [bit] NOT NULL DEFAULT 0,
    [AprobadoPorId_Usuario] [int] NULL,
    [FechaAprobacion] [datetime2](7) NULL,
    [ImagenRegistro] [varbinary](max) NULL,
    
    -- === CAMPOS DE CONTROL DE TIEMPO Y ASISTENCIA PROFESIONAL ===
    [HoraProgramada] [datetime2](7) NULL,
    [DiferenciaMinutos] [int] NULL,
    [EstadoPuntualidad] [nvarchar](20) NULL,
    [MinutosToleranciAplicada] [int] NULL,
    [Id_HorarioAplicado] [int] NULL,
    [DiaSemana] [int] NULL,
    [EsDiaLaborable] [bit] NULL,
    [TiempoTrabajadoMinutos] [int] NULL,
    [HorasExtraMinutos] [int] NULL,
    [RequiereJustificacion] [bit] NOT NULL DEFAULT 0,
    [EstadoJustificacion] [nvarchar](20) NULL,
    [TextoJustificacion] [nvarchar](500) NULL,
    [EsRegistroAutomatico] [bit] NOT NULL DEFAULT 0,
    [MetodoRegistro] [nvarchar](30) NULL,
    [UbicacionRegistro] [nvarchar](50) NULL,
    [Temperatura] [decimal](4,1) NULL,
    [ObservacionesSistema] [nvarchar](500) NULL,
    
    CONSTRAINT [PK_Asistencias] PRIMARY KEY CLUSTERED ([Id_Asistencia] ASC),
    CONSTRAINT [FK_Asistencias_Usuarios] FOREIGN KEY([Id_Usuario]) REFERENCES [dbo].[Usuarios] ([Id_Usu]),
    CONSTRAINT [FK_Asistencias_Sucursal] FOREIGN KEY([Sucursal]) REFERENCES [dbo].[Sucursal] ([Id]),
    CONSTRAINT [FK_Asistencias_UsuarioAprobador] FOREIGN KEY([AprobadoPorId_Usuario]) REFERENCES [dbo].[Usuarios] ([Id_Usu]),
    CONSTRAINT [FK_Asistencias_HorarioAplicado] FOREIGN KEY([Id_HorarioAplicado]) REFERENCES [dbo].[HorarioTrabajo] ([Id_Horario])
)
GO

-- Crear tabla Clientes
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Clientes' AND xtype='U')
CREATE TABLE [dbo].[Clientes](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [Nombres] [nvarchar](100) NOT NULL,
    [RazonSocial] [nvarchar](200) NULL,
    [Ruc] [nvarchar](20) NULL,
    [Telefono] [nvarchar](20) NULL,
    [Email] [nvarchar](100) NULL,
    [Direccion] [nvarchar](255) NULL,
    [FechaCreacion] [datetime2](7) NOT NULL DEFAULT GETDATE(),
    [Estado] [bit] NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Clientes] PRIMARY KEY CLUSTERED ([Id] ASC)
)
GO

-- Insertar datos iniciales
-- Rol administrador
IF NOT EXISTS (SELECT * FROM Roles WHERE Nombre_Rol = 'Administrador')
INSERT INTO Roles (Nombre_Rol, Descripcion) VALUES ('Administrador', 'Acceso completo al sistema')
GO

-- Sucursal principal
IF NOT EXISTS (SELECT * FROM Sucursal WHERE NombreSucursal = 'Sucursal Principal')
INSERT INTO Sucursal (NombreSucursal, Direccion, ToleranciaEntradaMinutos, ToleranciaSalidaMinutos) 
VALUES ('Sucursal Principal', 'Dirección Principal', 10, 10)
GO

-- Usuario administrador por defecto
IF NOT EXISTS (SELECT * FROM Usuarios WHERE Cedula = '12345678')
INSERT INTO Usuarios (Nombres, Apellidos, Cedula, Email, Password, Id_Rol) 
VALUES ('Admin', 'Sistema', '12345678', 'admin@sistema.com', 'admin123', 1)
GO

-- Horario de trabajo estándar (Lunes a Viernes 8:00-17:00)
IF NOT EXISTS (SELECT * FROM HorarioTrabajo WHERE Nombre = 'Horario Estándar')
BEGIN
    DECLARE @SucursalId INT = (SELECT TOP 1 Id FROM Sucursal)
    
    INSERT INTO HorarioTrabajo (Nombre, DiaSemana, HoraEntrada, HoraSalida, EsLaborable, Id_Sucursal) VALUES 
    ('Horario Estándar', 1, '08:00:00', '17:00:00', 1, @SucursalId), -- Lunes
    ('Horario Estándar', 2, '08:00:00', '17:00:00', 1, @SucursalId), -- Martes
    ('Horario Estándar', 3, '08:00:00', '17:00:00', 1, @SucursalId), -- Miércoles
    ('Horario Estándar', 4, '08:00:00', '17:00:00', 1, @SucursalId), -- Jueves
    ('Horario Estándar', 5, '08:00:00', '17:00:00', 1, @SucursalId), -- Viernes
    ('Horario Estándar', 6, '08:00:00', '12:00:00', 0, @SucursalId), -- Sábado (no laborable)
    ('Horario Estándar', 7, '08:00:00', '12:00:00', 0, @SucursalId)  -- Domingo (no laborable)
END
GO

PRINT 'Tablas creadas correctamente con campos de asistencia profesional'
