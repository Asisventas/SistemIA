-- Script para crear las tablas de Historial de Cambios
-- Ejecutar en la base de datos asiswebapp

-- Verificar si la tabla existe
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HistorialCambiosSistema]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[HistorialCambiosSistema] (
        [IdHistorialCambio] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Version] NVARCHAR(20) NULL,
        [FechaCambio] DATETIME2 NOT NULL,
        [TituloCambio] NVARCHAR(200) NOT NULL,
        [Tema] NVARCHAR(100) NULL,
        [TipoCambio] NVARCHAR(50) NULL,
        [ModuloAfectado] NVARCHAR(100) NULL,
        [Prioridad] NVARCHAR(20) NULL,
        [DescripcionBreve] NVARCHAR(500) NULL,
        [DescripcionTecnica] NVARCHAR(MAX) NULL,
        [ArchivosModificados] NVARCHAR(MAX) NULL,
        [Tags] NVARCHAR(500) NULL,
        [Referencias] NVARCHAR(500) NULL,
        [Notas] NVARCHAR(MAX) NULL,
        [ImplementadoPor] NVARCHAR(100) NULL,
        [ReferenciaTicket] NVARCHAR(100) NULL,
        [IdConversacionIA] INT NULL,
        [Estado] NVARCHAR(30) NULL,
        [RequiereMigracion] BIT NOT NULL DEFAULT(0),
        [NombreMigracion] NVARCHAR(200) NULL,
        [FechaCreacion] DATETIME2 NOT NULL DEFAULT(GETDATE())
    );
    PRINT 'Tabla HistorialCambiosSistema creada exitosamente';
END
ELSE
BEGIN
    PRINT 'Tabla HistorialCambiosSistema ya existe';
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ConversacionesIAHistorial]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ConversacionesIAHistorial] (
        [IdConversacionIA] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [FechaInicio] DATETIME2 NOT NULL,
        [FechaFin] DATETIME2 NULL,
        [ModeloIA] NVARCHAR(50) NULL,
        [Titulo] NVARCHAR(200) NULL,
        [ResumenEjecutivo] NVARCHAR(MAX) NULL,
        [ObjetivosSesion] NVARCHAR(MAX) NULL,
        [ResultadosObtenidos] NVARCHAR(MAX) NULL,
        [TareasPendientes] NVARCHAR(MAX) NULL,
        [ModulosTrabajados] NVARCHAR(500) NULL,
        [ArchivosCreados] NVARCHAR(MAX) NULL,
        [ArchivosModificados] NVARCHAR(MAX) NULL,
        [MigracionesGeneradas] NVARCHAR(MAX) NULL,
        [ProblemasResoluciones] NVARCHAR(MAX) NULL,
        [DecisionesTecnicas] NVARCHAR(MAX) NULL,
        [Etiquetas] NVARCHAR(500) NULL,
        [Complejidad] NVARCHAR(20) NULL,
        [DuracionMinutos] INT NULL,
        [CantidadCambios] INT NOT NULL DEFAULT(0),
        [FechaCreacion] DATETIME2 NOT NULL DEFAULT(GETDATE())
    );
    PRINT 'Tabla ConversacionesIAHistorial creada exitosamente';
END
ELSE
BEGIN
    PRINT 'Tabla ConversacionesIAHistorial ya existe';
END

-- Crear FK si aplica
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HistorialCambiosSistema]'))
   AND EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ConversacionesIAHistorial]'))
   AND NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_HistorialCambiosSistema_ConversacionesIAHistorial')
BEGIN
    ALTER TABLE [dbo].[HistorialCambiosSistema]
    ADD CONSTRAINT [FK_HistorialCambiosSistema_ConversacionesIAHistorial]
    FOREIGN KEY ([IdConversacionIA]) REFERENCES [dbo].[ConversacionesIAHistorial]([IdConversacionIA]);
    PRINT 'Foreign Key creada exitosamente';
END

PRINT 'Script completado';
