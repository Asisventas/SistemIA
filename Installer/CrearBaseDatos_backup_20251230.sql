IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721133828_InitialCreate'
)
BEGIN
    CREATE TABLE [Ciudades] (
        [IdCiudad] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        [Departamento] nvarchar(100) NULL,
        CONSTRAINT [PK_Ciudades] PRIMARY KEY ([IdCiudad])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721133828_InitialCreate'
)
BEGIN
    CREATE TABLE [Marcas] (
        [Id_Marca] int NOT NULL IDENTITY,
        [Marca] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_Marcas] PRIMARY KEY ([Id_Marca])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721133828_InitialCreate'
)
BEGIN
    CREATE TABLE [Paises] (
        [CodigoPais] nvarchar(3) NOT NULL,
        [Nombre] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_Paises] PRIMARY KEY ([CodigoPais])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721133828_InitialCreate'
)
BEGIN
    CREATE TABLE [Rol] (
        [Id_Rol] int NOT NULL IDENTITY,
        [NombreRol] nvarchar(max) NOT NULL,
        [Descripcion] nvarchar(max) NULL,
        [Estado] bit NOT NULL,
        CONSTRAINT [PK_Rol] PRIMARY KEY ([Id_Rol])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721133828_InitialCreate'
)
BEGIN
    CREATE TABLE [TiposContribuyentes] (
        [IdTipoContribuyente] int NOT NULL IDENTITY,
        [NombreTipo] nvarchar(100) NOT NULL,
        [Descripcion] nvarchar(max) NULL,
        CONSTRAINT [PK_TiposContribuyentes] PRIMARY KEY ([IdTipoContribuyente])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721133828_InitialCreate'
)
BEGIN
    CREATE TABLE [TiposDocumentosIdentidad] (
        [TipoDocumento] nvarchar(2) NOT NULL,
        [Descripcion] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_TiposDocumentosIdentidad] PRIMARY KEY ([TipoDocumento])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721133828_InitialCreate'
)
BEGIN
    CREATE TABLE [TiposOperacion] (
        [Codigo] nvarchar(3) NOT NULL,
        [Descripcion] nvarchar(100) NOT NULL,
        [Comentario] nvarchar(200) NULL,
        CONSTRAINT [PK_TiposOperacion] PRIMARY KEY ([Codigo])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721133828_InitialCreate'
)
BEGIN
    CREATE TABLE [Sucursal] (
        [Id] int NOT NULL IDENTITY,
        [NumSucursal] nchar(7) NOT NULL,
        [NombreSucursal] nvarchar(150) NOT NULL,
        [NombreEmpresa] nvarchar(150) NOT NULL,
        [RubroEmpresa] nvarchar(150) NULL,
        [Direccion] nvarchar(200) NOT NULL,
        [IdCiudad] int NOT NULL,
        [Telefono] nvarchar(20) NULL,
        [Correo] nvarchar(100) NULL,
        [RUC] nchar(8) NOT NULL,
        [DV] int NOT NULL,
        [CertificadoRuta] nvarchar(250) NULL,
        [CertificadoPassword] nvarchar(250) NULL,
        [Ambiente] nvarchar(10) NULL,
        [Timbrado] nvarchar(20) NULL,
        [PuntoExpedicion] nvarchar(10) NULL,
        [SistemaPlaya] bit NOT NULL,
        [Automatizado] bit NOT NULL,
        [IpConsola] nvarchar(15) NULL,
        [PuertoConsola] nvarchar(10) NULL,
        [Logo] VARBINARY(MAX) NULL,
        [Conexion] nvarchar(200) NULL,
        [ToleranciaEntradaMinutos] int NOT NULL,
        [ToleranciaSalidaMinutos] int NOT NULL,
        [RequiereJustificacionTardanza] bit NOT NULL,
        [RequiereJustificacionSalidaTemprana] bit NOT NULL,
        [MaximoHorasExtraDia] int NOT NULL,
        [CalculoAutomaticoHorasExtra] bit NOT NULL,
        CONSTRAINT [PK_Sucursal] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Sucursal_Ciudades_IdCiudad] FOREIGN KEY ([IdCiudad]) REFERENCES [Ciudades] ([IdCiudad]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721133828_InitialCreate'
)
BEGIN
    CREATE TABLE [Usuarios] (
        [Id_Usu] int NOT NULL IDENTITY,
        [Nombres] nvarchar(100) NOT NULL,
        [Apellidos] nvarchar(100) NOT NULL,
        [Direccion] nvarchar(200) NULL,
        [Ciudad] nvarchar(100) NULL,
        [CI] nvarchar(15) NULL,
        [Correo] nvarchar(150) NOT NULL,
        [Telefono] nvarchar(20) NULL,
        [Fecha_Ingreso] datetime2 NOT NULL,
        [Id_Rol] int NOT NULL,
        [Estado_Usu] bit NOT NULL,
        [UsuarioNombre] nvarchar(50) NOT NULL,
        [ContrasenaHash] varbinary(max) NOT NULL,
        [Foto] varbinary(max) NULL,
        [EmbeddingFacial] varbinary(max) NOT NULL,
        [Fecha_Nacimiento] datetime2 NULL,
        [HuellaDigital] varbinary(max) NULL,
        [Salario] decimal(18,2) NULL,
        [IPS] decimal(18,2) NULL,
        [Comision] decimal(18,2) NULL,
        [Descuento] decimal(18,2) NULL,
        CONSTRAINT [PK_Usuarios] PRIMARY KEY ([Id_Usu]),
        CONSTRAINT [FK_Usuarios_Rol_Id_Rol] FOREIGN KEY ([Id_Rol]) REFERENCES [Rol] ([Id_Rol]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721133828_InitialCreate'
)
BEGIN
    CREATE TABLE [Proveedores] (
        [Id_Proveedor] int NOT NULL IDENTITY,
        [CodigoProveedor] nvarchar(max) NOT NULL,
        [Nombre] nvarchar(max) NOT NULL,
        [Direccion] nvarchar(max) NULL,
        [Telefono] nvarchar(max) NULL,
        [Rubro] nvarchar(max) NULL,
        [RUC] nvarchar(max) NOT NULL,
        [DV] int NULL,
        [Correo] nvarchar(max) NULL,
        [Contacto] nvarchar(max) NULL,
        [FotoLogo] varbinary(max) NULL,
        [EstadoProveedor] nvarchar(max) NULL,
        [VencimientoTimbrado] datetime2 NULL,
        [Timbrado] nvarchar(max) NULL,
        [IdTipoContribuyente] int NULL,
        [CodigoContribuyente] int NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [UsuarioCreacion] nvarchar(max) NULL,
        [FechaModificacion] datetime2 NULL,
        [UsuarioModificacion] nvarchar(max) NULL,
        CONSTRAINT [PK_Proveedores] PRIMARY KEY ([Id_Proveedor]),
        CONSTRAINT [FK_Proveedores_TiposContribuyentes_IdTipoContribuyente] FOREIGN KEY ([IdTipoContribuyente]) REFERENCES [TiposContribuyentes] ([IdTipoContribuyente])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721133828_InitialCreate'
)
BEGIN
    CREATE TABLE [Clientes] (
        [IdCliente] int NOT NULL IDENTITY,
        [RazonSocial] nvarchar(250) NOT NULL,
        [RUC] nvarchar(8) NOT NULL,
        [TipoDocumento] nvarchar(2) NOT NULL,
        [DV] int NOT NULL,
        [Direccion] nvarchar(150) NULL,
        [Telefono] nvarchar(20) NULL,
        [Email] nvarchar(150) NULL,
        [FechaNacimiento] datetime2 NULL,
        [Contacto] nvarchar(100) NULL,
        [LimiteCredito] decimal(18,4) NULL,
        [Estado] nvarchar(20) NOT NULL,
        [Saldo] decimal(18,4) NOT NULL,
        [IdTipoContribuyente] int NOT NULL,
        [Timbrado] nvarchar(8) NULL,
        [VencimientoTimbrado] datetime2 NULL,
        [PrecioDiferenciado] bit NOT NULL,
        [PlazoDiasCredito] int NULL,
        [CodigoPais] nvarchar(3) NOT NULL,
        [IdCiudad] int NOT NULL,
        [EsExtranjero] bit NOT NULL,
        [TipoOperacion] nvarchar(3) NOT NULL,
        CONSTRAINT [PK_Clientes] PRIMARY KEY ([IdCliente]),
        CONSTRAINT [FK_Clientes_Ciudades_IdCiudad] FOREIGN KEY ([IdCiudad]) REFERENCES [Ciudades] ([IdCiudad]) ON DELETE CASCADE,
        CONSTRAINT [FK_Clientes_Paises_CodigoPais] FOREIGN KEY ([CodigoPais]) REFERENCES [Paises] ([CodigoPais]) ON DELETE CASCADE,
        CONSTRAINT [FK_Clientes_TiposContribuyentes_IdTipoContribuyente] FOREIGN KEY ([IdTipoContribuyente]) REFERENCES [TiposContribuyentes] ([IdTipoContribuyente]) ON DELETE CASCADE,
        CONSTRAINT [FK_Clientes_TiposDocumentosIdentidad_TipoDocumento] FOREIGN KEY ([TipoDocumento]) REFERENCES [TiposDocumentosIdentidad] ([TipoDocumento]) ON DELETE CASCADE,
        CONSTRAINT [FK_Clientes_TiposOperacion_TipoOperacion] FOREIGN KEY ([TipoOperacion]) REFERENCES [TiposOperacion] ([Codigo]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721133828_InitialCreate'
)
BEGIN
    CREATE TABLE [HorariosTrabajo] (
        [Id_Horario] int NOT NULL IDENTITY,
        [Id_Sucursal] int NOT NULL,
        [NombreHorario] nvarchar(100) NOT NULL,
        [HoraEntrada] time NOT NULL,
        [HoraSalida] time NOT NULL,
        [InicioBreak] time NULL,
        [FinBreak] time NULL,
        [Lunes] bit NOT NULL,
        [Martes] bit NOT NULL,
        [Miercoles] bit NOT NULL,
        [Jueves] bit NOT NULL,
        [Viernes] bit NOT NULL,
        [Sabado] bit NOT NULL,
        [Domingo] bit NOT NULL,
        [EsActivo] bit NOT NULL,
        [Descripcion] nvarchar(255) NULL,
        CONSTRAINT [PK_HorariosTrabajo] PRIMARY KEY ([Id_Horario]),
        CONSTRAINT [FK_HorariosTrabajo_Sucursal_Id_Sucursal] FOREIGN KEY ([Id_Sucursal]) REFERENCES [Sucursal] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721133828_InitialCreate'
)
BEGIN
    CREATE TABLE [AsignacionesHorarios] (
        [Id_Asignacion] int NOT NULL IDENTITY,
        [Id_Usuario] int NOT NULL,
        [Id_Horario] int NOT NULL,
        [FechaInicio] datetime2 NOT NULL,
        [FechaFin] datetime2 NOT NULL,
        CONSTRAINT [PK_AsignacionesHorarios] PRIMARY KEY ([Id_Asignacion]),
        CONSTRAINT [FK_AsignacionesHorarios_HorariosTrabajo_Id_Horario] FOREIGN KEY ([Id_Horario]) REFERENCES [HorariosTrabajo] ([Id_Horario]) ON DELETE CASCADE,
        CONSTRAINT [FK_AsignacionesHorarios_Usuarios_Id_Usuario] FOREIGN KEY ([Id_Usuario]) REFERENCES [Usuarios] ([Id_Usu]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721133828_InitialCreate'
)
BEGIN
    CREATE TABLE [Asistencias] (
        [Id_Asistencia] int NOT NULL IDENTITY,
        [Id_Usuario] int NOT NULL,
        [Sucursal] int NOT NULL,
        [FechaHora] datetime2 NOT NULL,
        [TipoRegistro] nvarchar(50) NOT NULL,
        [Notas] nvarchar(255) NULL,
        [Ubicacion] nvarchar(50) NULL,
        [FechaInicioAusencia] datetime2 NULL,
        [FechaFinAusencia] datetime2 NULL,
        [MotivoAusencia] nvarchar(100) NULL,
        [AprobadaPorGerencia] bit NOT NULL,
        [AprobadoPorId_Usuario] int NULL,
        [FechaAprobacion] datetime2 NULL,
        [ImagenRegistro] varbinary(max) NULL,
        [HoraProgramada] datetime2 NULL,
        [DiferenciaMinutos] int NULL,
        [EstadoPuntualidad] nvarchar(20) NULL,
        [MinutosToleranciAplicada] int NULL,
        [Id_HorarioAplicado] int NULL,
        [DiaSemana] int NULL,
        [EsDiaLaborable] bit NULL,
        [TiempoTrabajadoMinutos] int NULL,
        [HorasExtraMinutos] int NULL,
        [RequiereJustificacion] bit NOT NULL,
        [EstadoJustificacion] nvarchar(20) NULL,
        [TextoJustificacion] nvarchar(500) NULL,
        [EsRegistroAutomatico] bit NOT NULL,
        [MetodoRegistro] nvarchar(30) NULL,
        [UbicacionRegistro] nvarchar(50) NULL,
        [Temperatura] decimal(4,1) NULL,
        [ObservacionesSistema] nvarchar(500) NULL,
        CONSTRAINT [PK_Asistencias] PRIMARY KEY ([Id_Asistencia]),
        CONSTRAINT [FK_Asistencias_HorariosTrabajo_Id_HorarioAplicado] FOREIGN KEY ([Id_HorarioAplicado]) REFERENCES [HorariosTrabajo] ([Id_Horario]),
        CONSTRAINT [FK_Asistencias_Sucursal_Sucursal] FOREIGN KEY ([Sucursal]) REFERENCES [Sucursal] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Asistencias_Usuarios_AprobadoPorId_Usuario] FOREIGN KEY ([AprobadoPorId_Usuario]) REFERENCES [Usuarios] ([Id_Usu]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Asistencias_Usuarios_Id_Usuario] FOREIGN KEY ([Id_Usuario]) REFERENCES [Usuarios] ([Id_Usu]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721133828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AsignacionesHorarios_Id_Horario] ON [AsignacionesHorarios] ([Id_Horario]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721133828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AsignacionesHorarios_Id_Usuario] ON [AsignacionesHorarios] ([Id_Usuario]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721133828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Asistencias_AprobadoPorId_Usuario] ON [Asistencias] ([AprobadoPorId_Usuario]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721133828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Asistencias_Id_HorarioAplicado] ON [Asistencias] ([Id_HorarioAplicado]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721133828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Asistencias_Id_Usuario] ON [Asistencias] ([Id_Usuario]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721133828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Asistencias_Sucursal] ON [Asistencias] ([Sucursal]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721133828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Clientes_CodigoPais] ON [Clientes] ([CodigoPais]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721133828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Clientes_IdCiudad] ON [Clientes] ([IdCiudad]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721133828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Clientes_IdTipoContribuyente] ON [Clientes] ([IdTipoContribuyente]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721133828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Clientes_TipoDocumento] ON [Clientes] ([TipoDocumento]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721133828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Clientes_TipoOperacion] ON [Clientes] ([TipoOperacion]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721133828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_HorariosTrabajo_Id_Sucursal] ON [HorariosTrabajo] ([Id_Sucursal]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721133828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Proveedores_IdTipoContribuyente] ON [Proveedores] ([IdTipoContribuyente]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721133828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Sucursal_IdCiudad] ON [Sucursal] ([IdCiudad]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721133828_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Usuarios_Id_Rol] ON [Usuarios] ([Id_Rol]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250721133828_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250721133828_InitialCreate', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727000644_TiposIvaTableCreated'
)
BEGIN
    ALTER TABLE [Clientes] DROP CONSTRAINT [FK_Clientes_TiposOperacion_TipoOperacion];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727000644_TiposIvaTableCreated'
)
BEGIN
    DROP INDEX [IX_Clientes_TipoOperacion] ON [Clientes];
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clientes]') AND [c].[name] = N'TipoOperacion');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Clientes] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Clientes] ALTER COLUMN [TipoOperacion] nvarchar(1) NULL;
    CREATE INDEX [IX_Clientes_TipoOperacion] ON [Clientes] ([TipoOperacion]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727000644_TiposIvaTableCreated'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clientes]') AND [c].[name] = N'RUC');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Clientes] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Clientes] ALTER COLUMN [RUC] nvarchar(50) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727000644_TiposIvaTableCreated'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clientes]') AND [c].[name] = N'Estado');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Clientes] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [Clientes] ALTER COLUMN [Estado] bit NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727000644_TiposIvaTableCreated'
)
BEGIN
    ALTER TABLE [Clientes] ADD [Celular] nvarchar(20) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727000644_TiposIvaTableCreated'
)
BEGIN
    ALTER TABLE [Clientes] ADD [CodigoCliente] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727000644_TiposIvaTableCreated'
)
BEGIN
    ALTER TABLE [Clientes] ADD [CodigoDepartamento] nvarchar(2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727000644_TiposIvaTableCreated'
)
BEGIN
    ALTER TABLE [Clientes] ADD [CodigoDistrito] nvarchar(4) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727000644_TiposIvaTableCreated'
)
BEGIN
    ALTER TABLE [Clientes] ADD [CodigoEstablecimiento] nvarchar(20) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727000644_TiposIvaTableCreated'
)
BEGIN
    ALTER TABLE [Clientes] ADD [CompDireccion1] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727000644_TiposIvaTableCreated'
)
BEGIN
    ALTER TABLE [Clientes] ADD [CompDireccion2] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727000644_TiposIvaTableCreated'
)
BEGIN
    ALTER TABLE [Clientes] ADD [DescripcionDepartamento] nvarchar(16) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727000644_TiposIvaTableCreated'
)
BEGIN
    ALTER TABLE [Clientes] ADD [DescripcionDistrito] nvarchar(30) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727000644_TiposIvaTableCreated'
)
BEGIN
    ALTER TABLE [Clientes] ADD [FechaAlta] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727000644_TiposIvaTableCreated'
)
BEGIN
    ALTER TABLE [Clientes] ADD [NaturalezaReceptor] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727000644_TiposIvaTableCreated'
)
BEGIN
    ALTER TABLE [Clientes] ADD [NombreFantasia] nvarchar(255) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727000644_TiposIvaTableCreated'
)
BEGIN
    ALTER TABLE [Clientes] ADD [NumeroCasa] nvarchar(10) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727000644_TiposIvaTableCreated'
)
BEGIN
    ALTER TABLE [Clientes] ADD [NumeroDocumento] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727000644_TiposIvaTableCreated'
)
BEGIN
    ALTER TABLE [Clientes] ADD [NumeroDocumentoIdentidad] nvarchar(20) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727000644_TiposIvaTableCreated'
)
BEGIN
    ALTER TABLE [Clientes] ADD [TipoDocumentoIdentidadSifen] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727000644_TiposIvaTableCreated'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AsignacionesHorarios]') AND [c].[name] = N'FechaFin');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [AsignacionesHorarios] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [AsignacionesHorarios] ALTER COLUMN [FechaFin] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727000644_TiposIvaTableCreated'
)
BEGIN
    ALTER TABLE [AsignacionesHorarios] ADD [AsignadoPorId_Usuario] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727000644_TiposIvaTableCreated'
)
BEGIN
    ALTER TABLE [AsignacionesHorarios] ADD [Estado] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727000644_TiposIvaTableCreated'
)
BEGIN
    ALTER TABLE [AsignacionesHorarios] ADD [FechaAsignacion] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727000644_TiposIvaTableCreated'
)
BEGIN
    ALTER TABLE [AsignacionesHorarios] ADD [Observaciones] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727000644_TiposIvaTableCreated'
)
BEGIN
    CREATE TABLE [ProveedoresSifen] (
        [IdProveedor] int NOT NULL IDENTITY,
        [CodigoProveedor] nvarchar(50) NULL,
        [RazonSocial] nvarchar(200) NOT NULL,
        [NombreFantasia] nvarchar(200) NULL,
        [RUC] nchar(8) NOT NULL,
        [DV] int NOT NULL,
        [TipoContribuyente] int NOT NULL,
        [IdTipoContribuyenteCatalogo] int NOT NULL,
        [TipoRegimen] int NOT NULL,
        [Timbrado] nchar(8) NOT NULL,
        [VencimientoTimbrado] datetime2 NOT NULL,
        [Establecimiento] nchar(3) NOT NULL,
        [PuntoExpedicion] nchar(3) NOT NULL,
        [Direccion] nvarchar(255) NOT NULL,
        [NumeroCasa] nvarchar(10) NULL,
        [CodigoDepartamento] nvarchar(2) NOT NULL,
        [DescripcionDepartamento] nvarchar(16) NOT NULL,
        [CodigoCiudad] int NOT NULL,
        [DescripcionCiudad] nvarchar(30) NOT NULL,
        [Telefono] nvarchar(15) NOT NULL,
        [Email] nvarchar(80) NOT NULL,
        [Celular] nvarchar(15) NULL,
        [PersonaContacto] nvarchar(100) NULL,
        [CodigoActividadEconomica] nchar(5) NOT NULL,
        [DescripcionActividadEconomica] nvarchar(300) NOT NULL,
        [Rubro] nvarchar(100) NULL,
        [LimiteCredito] decimal(18,4) NULL,
        [SaldoPendiente] decimal(18,4) NOT NULL DEFAULT 0.0,
        [PlazoPagoDias] int NULL,
        [CodigoPais] nchar(3) NOT NULL DEFAULT N'PRY',
        [CertificadoRuta] nvarchar(500) NULL,
        [CertificadoPassword] nvarchar(100) NULL,
        [Ambiente] nvarchar(10) NOT NULL,
        [UltimoNumeroDocumento] bigint NOT NULL,
        [UltimaSerie] nvarchar(2) NULL,
        [Estado] bit NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [UsuarioCreacion] nvarchar(100) NULL,
        [FechaModificacion] datetime2 NULL,
        [UsuarioModificacion] nvarchar(100) NULL,
        [FechaUltimaFacturacion] datetime2 NULL,
        CONSTRAINT [PK_ProveedoresSifen] PRIMARY KEY ([IdProveedor]),
        CONSTRAINT [FK_ProveedoresSifen_TiposContribuyentes_IdTipoContribuyenteCatalogo] FOREIGN KEY ([IdTipoContribuyenteCatalogo]) REFERENCES [TiposContribuyentes] ([IdTipoContribuyente]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727000644_TiposIvaTableCreated'
)
BEGIN
    CREATE TABLE [TiposIva] (
        [IdTipoIva] int NOT NULL IDENTITY,
        [CodigoSifen] int NOT NULL,
        [Descripcion] nvarchar(100) NOT NULL,
        [Porcentaje] decimal(5,2) NOT NULL,
        [TasaSifen] decimal(5,2) NOT NULL,
        [Estado] bit NOT NULL,
        [EsPredeterminado] bit NOT NULL,
        [CodigoAbreviado] nvarchar(10) NULL,
        [Observaciones] nvarchar(255) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [UsuarioCreacion] nvarchar(50) NULL,
        [FechaModificacion] datetime2 NULL,
        [UsuarioModificacion] nvarchar(50) NULL,
        CONSTRAINT [PK_TiposIva] PRIMARY KEY ([IdTipoIva])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727000644_TiposIvaTableCreated'
)
BEGIN
    CREATE INDEX [IX_ProveedoresSifen_IdTipoContribuyenteCatalogo] ON [ProveedoresSifen] ([IdTipoContribuyenteCatalogo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727000644_TiposIvaTableCreated'
)
BEGIN
    ALTER TABLE [Clientes] ADD CONSTRAINT [FK_Clientes_TiposOperacion_TipoOperacion] FOREIGN KEY ([TipoOperacion]) REFERENCES [TiposOperacion] ([Codigo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727000644_TiposIvaTableCreated'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250727000644_TiposIvaTableCreated', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727010854_AgregarListasPreciosCorregido'
)
BEGIN
    CREATE TABLE [Monedas] (
        [IdMoneda] int NOT NULL IDENTITY,
        [CodigoISO] nvarchar(3) NOT NULL,
        [Nombre] nvarchar(50) NOT NULL,
        [Simbolo] nvarchar(10) NOT NULL,
        [EsMonedaBase] bit NOT NULL,
        [Estado] bit NOT NULL,
        [Orden] int NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [UsuarioCreacion] nvarchar(50) NOT NULL,
        [FechaModificacion] datetime2 NULL,
        [UsuarioModificacion] nvarchar(50) NULL,
        CONSTRAINT [PK_Monedas] PRIMARY KEY ([IdMoneda])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727010854_AgregarListasPreciosCorregido'
)
BEGIN
    CREATE TABLE [ListasPrecios] (
        [IdListaPrecio] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        [Descripcion] nvarchar(255) NULL,
        [IdMoneda] int NOT NULL,
        [EsPredeterminada] bit NOT NULL,
        [Estado] bit NOT NULL,
        [AplicarDescuentoGlobal] bit NOT NULL,
        [PorcentajeDescuento] decimal(5,2) NOT NULL,
        [FechaVigenciaDesde] datetime2 NULL,
        [FechaVigenciaHasta] datetime2 NULL,
        [Orden] int NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [UsuarioCreacion] nvarchar(50) NOT NULL,
        [FechaModificacion] datetime2 NULL,
        [UsuarioModificacion] nvarchar(50) NULL,
        CONSTRAINT [PK_ListasPrecios] PRIMARY KEY ([IdListaPrecio]),
        CONSTRAINT [FK_ListasPrecios_Monedas_IdMoneda] FOREIGN KEY ([IdMoneda]) REFERENCES [Monedas] ([IdMoneda]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727010854_AgregarListasPreciosCorregido'
)
BEGIN
    CREATE TABLE [TiposCambio] (
        [IdTipoCambio] int NOT NULL IDENTITY,
        [IdMonedaOrigen] int NOT NULL,
        [IdMonedaDestino] int NOT NULL,
        [TasaCambio] decimal(18,6) NOT NULL,
        [FechaTipoCambio] datetime2 NOT NULL,
        [Fuente] nvarchar(100) NULL,
        [EsAutomatico] bit NOT NULL,
        [Estado] bit NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [UsuarioCreacion] nvarchar(50) NOT NULL,
        [FechaModificacion] datetime2 NULL,
        [UsuarioModificacion] nvarchar(50) NULL,
        CONSTRAINT [PK_TiposCambio] PRIMARY KEY ([IdTipoCambio]),
        CONSTRAINT [FK_TiposCambio_Monedas_IdMonedaDestino] FOREIGN KEY ([IdMonedaDestino]) REFERENCES [Monedas] ([IdMoneda]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TiposCambio_Monedas_IdMonedaOrigen] FOREIGN KEY ([IdMonedaOrigen]) REFERENCES [Monedas] ([IdMoneda]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727010854_AgregarListasPreciosCorregido'
)
BEGIN
    CREATE TABLE [ListasPreciosDetalles] (
        [IdDetalle] int NOT NULL IDENTITY,
        [IdListaPrecio] int NOT NULL,
        [CodigoProducto] nvarchar(50) NOT NULL,
        [Precio] decimal(18,2) NOT NULL,
        [PrecioAnterior] decimal(18,2) NULL,
        [FechaUltimaActualizacion] datetime2 NOT NULL,
        [AplicarDescuento] bit NOT NULL,
        [DescuentoEspecial] decimal(5,2) NOT NULL,
        [Estado] bit NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [UsuarioCreacion] nvarchar(50) NOT NULL,
        [FechaModificacion] datetime2 NULL,
        [UsuarioModificacion] nvarchar(50) NULL,
        CONSTRAINT [PK_ListasPreciosDetalles] PRIMARY KEY ([IdDetalle]),
        CONSTRAINT [FK_ListasPreciosDetalles_ListasPrecios_IdListaPrecio] FOREIGN KEY ([IdListaPrecio]) REFERENCES [ListasPrecios] ([IdListaPrecio]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727010854_AgregarListasPreciosCorregido'
)
BEGIN
    CREATE INDEX [IX_ListasPrecios_IdMoneda] ON [ListasPrecios] ([IdMoneda]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727010854_AgregarListasPreciosCorregido'
)
BEGIN
    CREATE INDEX [IX_ListasPreciosDetalles_IdListaPrecio] ON [ListasPreciosDetalles] ([IdListaPrecio]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727010854_AgregarListasPreciosCorregido'
)
BEGIN
    CREATE INDEX [IX_TiposCambio_IdMonedaDestino] ON [TiposCambio] ([IdMonedaDestino]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727010854_AgregarListasPreciosCorregido'
)
BEGIN
    CREATE INDEX [IX_TiposCambio_IdMonedaOrigen] ON [TiposCambio] ([IdMonedaOrigen]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727010854_AgregarListasPreciosCorregido'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250727010854_AgregarListasPreciosCorregido', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250813110329_CrearTablaProductos'
)
BEGIN
    CREATE TABLE [Productos] (
        [IdProducto] int NOT NULL IDENTITY,
        [CodigoInterno] nvarchar(50) NOT NULL,
        [Descripcion] nvarchar(200) NOT NULL,
        [GTIN] nvarchar(14) NULL,
        [NCM] nvarchar(8) NULL,
        [UnidadMedidaCodigo] nvarchar(3) NOT NULL,
        [TipoItem] int NOT NULL,
        [IdMarca] int NULL,
        [IdTipoIva] int NOT NULL,
        [CostoUnitarioGs] decimal(18,4) NULL,
        [PrecioUnitarioGs] decimal(18,4) NOT NULL,
        [PrecioUnitarioUsd] decimal(18,4) NULL,
        [Stock] decimal(18,4) NOT NULL,
        [StockMinimo] decimal(18,4) NOT NULL,
        [Activo] bit NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [UsuarioCreacion] nvarchar(50) NULL,
        [FechaModificacion] datetime2 NULL,
        [UsuarioModificacion] nvarchar(50) NULL,
        CONSTRAINT [PK_Productos] PRIMARY KEY ([IdProducto]),
        CONSTRAINT [FK_Productos_Marcas_IdMarca] FOREIGN KEY ([IdMarca]) REFERENCES [Marcas] ([Id_Marca]),
        CONSTRAINT [FK_Productos_TiposIva_IdTipoIva] FOREIGN KEY ([IdTipoIva]) REFERENCES [TiposIva] ([IdTipoIva]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250813110329_CrearTablaProductos'
)
BEGIN
    CREATE INDEX [IX_Productos_IdMarca] ON [Productos] ([IdMarca]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250813110329_CrearTablaProductos'
)
BEGIN
    CREATE INDEX [IX_Productos_IdTipoIva] ON [Productos] ([IdTipoIva]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250813110329_CrearTablaProductos'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250813110329_CrearTablaProductos', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250813113303_RenombrarCodigoBarrasYEliminarNcm'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Productos]') AND [c].[name] = N'NCM');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Productos] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [Productos] DROP COLUMN [NCM];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250813113303_RenombrarCodigoBarrasYEliminarNcm'
)
BEGIN
    EXEC sp_rename N'[Productos].[GTIN]', N'CodigoBarras', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250813113303_RenombrarCodigoBarrasYEliminarNcm'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250813113303_RenombrarCodigoBarrasYEliminarNcm', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250813120628_ActualizarProductos_AddCamposYFK'
)
BEGIN
    ALTER TABLE [Productos] ADD [Foto] varchar(180) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250813120628_ActualizarProductos_AddCamposYFK'
)
BEGIN
    ALTER TABLE [Productos] ADD [IP] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250813120628_ActualizarProductos_AddCamposYFK'
)
BEGIN
    ALTER TABLE [Productos] ADD [UndMedida] char(10) NOT NULL DEFAULT '';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250813120628_ActualizarProductos_AddCamposYFK'
)
BEGIN
    ALTER TABLE [Productos] ADD [suc] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250813120628_ActualizarProductos_AddCamposYFK'
)
BEGIN
    CREATE INDEX [IX_Productos_suc] ON [Productos] ([suc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250813120628_ActualizarProductos_AddCamposYFK'
)
BEGIN
    ALTER TABLE [Productos] ADD CONSTRAINT [FK_Productos_Sucursal_suc] FOREIGN KEY ([suc]) REFERENCES [Sucursal] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250813120628_ActualizarProductos_AddCamposYFK'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250813120628_ActualizarProductos_AddCamposYFK', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250813175802_ProductosSucursalNotNull'
)
BEGIN
    ALTER TABLE [Productos] DROP CONSTRAINT [FK_Productos_Sucursal_suc];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250813175802_ProductosSucursalNotNull'
)
BEGIN
                    DECLARE @sid INT;
                    SELECT TOP 1 @sid = Id FROM Sucursal ORDER BY Id;
                    IF @sid IS NULL
                    BEGIN
                        -- Si no hay sucursales, creamos una mínima para cumplir la FK
                        INSERT INTO Sucursal (NumSucursal, NombreSucursal, NombreEmpresa, Direccion, IdCiudad, RUC, DV)
                        VALUES ('0000001','Sucursal Principal','Empresa','Sin Direccion', 1, '00000000', 0);
                        SELECT TOP 1 @sid = Id FROM Sucursal ORDER BY Id;
                    END
                    UPDATE Productos SET suc = @sid WHERE suc IS NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250813175802_ProductosSucursalNotNull'
)
BEGIN
    DROP INDEX [IX_Productos_suc] ON [Productos];
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Productos]') AND [c].[name] = N'suc');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Productos] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [Productos] ALTER COLUMN [suc] int NOT NULL;
    CREATE INDEX [IX_Productos_suc] ON [Productos] ([suc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250813175802_ProductosSucursalNotNull'
)
BEGIN
    ALTER TABLE [Productos] ADD CONSTRAINT [FK_Productos_Sucursal_suc] FOREIGN KEY ([suc]) REFERENCES [Sucursal] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250813175802_ProductosSucursalNotNull'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250813175802_ProductosSucursalNotNull', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814182201_AgregarClasificacionReal'
)
BEGIN
    ALTER TABLE [Productos] ADD [IdClasificacion] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814182201_AgregarClasificacionReal'
)
BEGIN
    CREATE TABLE [Clasificaciones] (
        [IdClasificacion] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        [Descripcion] nvarchar(200) NULL,
        [Activo] bit NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        CONSTRAINT [PK_Clasificaciones] PRIMARY KEY ([IdClasificacion])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814182201_AgregarClasificacionReal'
)
BEGIN
    CREATE INDEX [IX_Productos_IdClasificacion] ON [Productos] ([IdClasificacion]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814182201_AgregarClasificacionReal'
)
BEGIN
    ALTER TABLE [Productos] ADD CONSTRAINT [FK_Productos_Clasificaciones_IdClasificacion] FOREIGN KEY ([IdClasificacion]) REFERENCES [Clasificaciones] ([IdClasificacion]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250814182201_AgregarClasificacionReal'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250814182201_AgregarClasificacionReal', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250818163743_InventarioDepositos'
)
BEGIN
    CREATE TABLE [Depositos] (
        [IdDeposito] int NOT NULL IDENTITY,
        [Nombre] nvarchar(120) NOT NULL,
        [Descripcion] nvarchar(250) NULL,
        [suc] int NOT NULL,
        [Activo] bit NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [UsuarioCreacion] nvarchar(50) NULL,
        [FechaModificacion] datetime2 NULL,
        [UsuarioModificacion] nvarchar(50) NULL,
        CONSTRAINT [PK_Depositos] PRIMARY KEY ([IdDeposito]),
        CONSTRAINT [FK_Depositos_Sucursal_suc] FOREIGN KEY ([suc]) REFERENCES [Sucursal] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250818163743_InventarioDepositos'
)
BEGIN
    CREATE TABLE [MovimientosInventario] (
        [IdMovimiento] int NOT NULL IDENTITY,
        [IdProducto] int NOT NULL,
        [IdDeposito] int NOT NULL,
        [Tipo] int NOT NULL,
        [Cantidad] decimal(18,4) NOT NULL,
        [Motivo] nvarchar(250) NULL,
        [Fecha] datetime2 NOT NULL,
        [Usuario] nvarchar(50) NULL,
        CONSTRAINT [PK_MovimientosInventario] PRIMARY KEY ([IdMovimiento]),
        CONSTRAINT [FK_MovimientosInventario_Depositos_IdDeposito] FOREIGN KEY ([IdDeposito]) REFERENCES [Depositos] ([IdDeposito]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MovimientosInventario_Productos_IdProducto] FOREIGN KEY ([IdProducto]) REFERENCES [Productos] ([IdProducto]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250818163743_InventarioDepositos'
)
BEGIN
    CREATE TABLE [ProductosDepositos] (
        [IdProductoDeposito] int NOT NULL IDENTITY,
        [IdProducto] int NOT NULL,
        [IdDeposito] int NOT NULL,
        [Stock] decimal(18,4) NOT NULL,
        [StockMinimo] decimal(18,4) NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [UsuarioCreacion] nvarchar(50) NULL,
        [FechaModificacion] datetime2 NULL,
        [UsuarioModificacion] nvarchar(50) NULL,
        [ProductoIdProducto] int NULL,
        CONSTRAINT [PK_ProductosDepositos] PRIMARY KEY ([IdProductoDeposito]),
        CONSTRAINT [FK_ProductosDepositos_Depositos_IdDeposito] FOREIGN KEY ([IdDeposito]) REFERENCES [Depositos] ([IdDeposito]) ON DELETE CASCADE,
        CONSTRAINT [FK_ProductosDepositos_Productos_IdProducto] FOREIGN KEY ([IdProducto]) REFERENCES [Productos] ([IdProducto]) ON DELETE CASCADE,
        CONSTRAINT [FK_ProductosDepositos_Productos_ProductoIdProducto] FOREIGN KEY ([ProductoIdProducto]) REFERENCES [Productos] ([IdProducto])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250818163743_InventarioDepositos'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Depositos_suc_Nombre] ON [Depositos] ([suc], [Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250818163743_InventarioDepositos'
)
BEGIN
    CREATE INDEX [IX_MovimientosInventario_IdDeposito] ON [MovimientosInventario] ([IdDeposito]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250818163743_InventarioDepositos'
)
BEGIN
    CREATE INDEX [IX_MovimientosInventario_IdProducto] ON [MovimientosInventario] ([IdProducto]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250818163743_InventarioDepositos'
)
BEGIN
    CREATE INDEX [IX_ProductosDepositos_IdDeposito] ON [ProductosDepositos] ([IdDeposito]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250818163743_InventarioDepositos'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProductosDepositos_IdProducto_IdDeposito] ON [ProductosDepositos] ([IdProducto], [IdDeposito]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250818163743_InventarioDepositos'
)
BEGIN
    CREATE INDEX [IX_ProductosDepositos_ProductoIdProducto] ON [ProductosDepositos] ([ProductoIdProducto]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250818163743_InventarioDepositos'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250818163743_InventarioDepositos', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250818164406_FixProductoDepositoMapping'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250818164406_FixProductoDepositoMapping', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819112616_Producto_DepositoPredeterminado'
)
BEGIN
    ALTER TABLE [ProductosDepositos] DROP CONSTRAINT [FK_ProductosDepositos_Productos_ProductoIdProducto];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819112616_Producto_DepositoPredeterminado'
)
BEGIN
    DROP INDEX [IX_ProductosDepositos_ProductoIdProducto] ON [ProductosDepositos];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819112616_Producto_DepositoPredeterminado'
)
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProductosDepositos]') AND [c].[name] = N'ProductoIdProducto');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [ProductosDepositos] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [ProductosDepositos] DROP COLUMN [ProductoIdProducto];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819112616_Producto_DepositoPredeterminado'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250819112616_Producto_DepositoPredeterminado', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819113925_Producto_AgregarDepositoPredeterminado'
)
BEGIN
    ALTER TABLE [Productos] ADD [IdDepositoPredeterminado] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819113925_Producto_AgregarDepositoPredeterminado'
)
BEGIN
    CREATE INDEX [IX_Productos_IdDepositoPredeterminado] ON [Productos] ([IdDepositoPredeterminado]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819113925_Producto_AgregarDepositoPredeterminado'
)
BEGIN
    ALTER TABLE [Productos] ADD CONSTRAINT [FK_Productos_Depositos_IdDepositoPredeterminado] FOREIGN KEY ([IdDepositoPredeterminado]) REFERENCES [Depositos] ([IdDeposito]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819113925_Producto_AgregarDepositoPredeterminado'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250819113925_Producto_AgregarDepositoPredeterminado', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819120537_TiposItemCatalogo'
)
BEGIN
    CREATE TABLE [TiposItem] (
        [IdTipoItem] int NOT NULL IDENTITY,
        [Nombre] nvarchar(50) NOT NULL,
        [EsGasto] bit NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [UsuarioCreacion] nvarchar(50) NULL,
        [FechaModificacion] datetime2 NULL,
        [UsuarioModificacion] nvarchar(50) NULL,
        CONSTRAINT [PK_TiposItem] PRIMARY KEY ([IdTipoItem])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819120537_TiposItemCatalogo'
)
BEGIN
    SET IDENTITY_INSERT [dbo].[TiposItem] ON;
    IF NOT EXISTS (SELECT 1 FROM [dbo].[TiposItem] WHERE [IdTipoItem] = 1)
        INSERT INTO [dbo].[TiposItem] ([IdTipoItem],[Nombre],[EsGasto],[FechaCreacion]) VALUES (1,'Producto',0,GETDATE());
    IF NOT EXISTS (SELECT 1 FROM [dbo].[TiposItem] WHERE [IdTipoItem] = 2)
        INSERT INTO [dbo].[TiposItem] ([IdTipoItem],[Nombre],[EsGasto],[FechaCreacion]) VALUES (2,'Servicio',0,GETDATE());
    IF NOT EXISTS (SELECT 1 FROM [dbo].[TiposItem] WHERE [IdTipoItem] = 3)
        INSERT INTO [dbo].[TiposItem] ([IdTipoItem],[Nombre],[EsGasto],[FechaCreacion]) VALUES (3,'Gasto',1,GETDATE());
    SET IDENTITY_INSERT [dbo].[TiposItem] OFF;
    -- Normalizar posibles valores fuera de catálogo
    UPDATE P SET P.TipoItem = 1 FROM [dbo].[Productos] P WHERE P.TipoItem NOT IN (1,2,3);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819120537_TiposItemCatalogo'
)
BEGIN
    CREATE INDEX [IX_Productos_TipoItem] ON [Productos] ([TipoItem]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819120537_TiposItemCatalogo'
)
BEGIN
    ALTER TABLE [Productos] ADD CONSTRAINT [FK_Productos_TiposItem_TipoItem] FOREIGN KEY ([TipoItem]) REFERENCES [TiposItem] ([IdTipoItem]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819120537_TiposItemCatalogo'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250819120537_TiposItemCatalogo', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819132526_CombosProductos'
)
BEGIN
    ALTER TABLE [Productos] ADD [EsCombo] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819132526_CombosProductos'
)
BEGIN
    CREATE TABLE [ProductosComponentes] (
        [IdProductoComponente] int NOT NULL IDENTITY,
        [IdProducto] int NOT NULL,
        [IdComponente] int NOT NULL,
        [Cantidad] decimal(18,4) NOT NULL,
        [UnidadMedida] nvarchar(20) NULL,
        [Activo] bit NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaModificacion] datetime2 NULL,
        [UsuarioCreacion] nvarchar(50) NULL,
        [UsuarioModificacion] nvarchar(50) NULL,
        CONSTRAINT [PK_ProductosComponentes] PRIMARY KEY ([IdProductoComponente]),
        CONSTRAINT [FK_ProductosComponentes_Productos_IdComponente] FOREIGN KEY ([IdComponente]) REFERENCES [Productos] ([IdProducto]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductosComponentes_Productos_IdProducto] FOREIGN KEY ([IdProducto]) REFERENCES [Productos] ([IdProducto]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819132526_CombosProductos'
)
BEGIN
    CREATE INDEX [IX_ProductosComponentes_IdComponente] ON [ProductosComponentes] ([IdComponente]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819132526_CombosProductos'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProductosComponentes_IdProducto_IdComponente] ON [ProductosComponentes] ([IdProducto], [IdComponente]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819132526_CombosProductos'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250819132526_CombosProductos', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819143653_CrearCompras'
)
BEGIN
    CREATE TABLE [Compras] (
        [IdCompra] int NOT NULL IDENTITY,
        [Establecimiento] nchar(3) NULL,
        [PuntoExpedicion] nchar(3) NULL,
        [NumeroFactura] nvarchar(7) NULL,
        [Timbrado] nvarchar(8) NULL,
        [suc] int NOT NULL,
        [IdProveedor] int NOT NULL,
        [IdUsuario] int NULL,
        [IdMoneda] int NULL,
        [IdDeposito] int NULL,
        [Fecha] datetime2 NOT NULL,
        [FechaVencimiento] datetime2 NULL,
        [Total] decimal(18,4) NOT NULL,
        [FormaPago] nvarchar(50) NULL,
        [PlazoDias] int NULL,
        [Estado] nvarchar(20) NULL,
        [TipoDocumento] nvarchar(12) NOT NULL,
        [TipoIngreso] nvarchar(13) NOT NULL,
        [CodigoDocumento] int NULL,
        [TotalEnLetras] nvarchar(280) NULL,
        [Turno] int NULL,
        [IdCaja] int NULL,
        [ImputarIVA] bit NULL,
        [ImputarIRP] bit NULL,
        [ImputarIRE] bit NULL,
        [NoImputar] bit NULL,
        [Comentario] nvarchar(280) NULL,
        [EsFacturaElectronica] bit NULL,
        [TipoRegistro] nvarchar(20) NULL,
        [CodigoRegistro] int NULL,
        [CodigoCondicion] nvarchar(10) NULL,
        [EsMonedaExtranjera] bit NULL,
        [CreditoSaldo] decimal(18,4) NULL,
        [CambioDelDia] decimal(18,4) NULL,
        [SimboloMoneda] nvarchar(4) NULL,
        [IdCajaChica] int NULL,
        [Vendedor] nchar(20) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [UsuarioCreacion] nvarchar(max) NULL,
        [FechaModificacion] datetime2 NULL,
        [UsuarioModificacion] nvarchar(max) NULL,
        CONSTRAINT [PK_Compras] PRIMARY KEY ([IdCompra]),
        CONSTRAINT [FK_Compras_Depositos_IdDeposito] FOREIGN KEY ([IdDeposito]) REFERENCES [Depositos] ([IdDeposito]) ON DELETE SET NULL,
        CONSTRAINT [FK_Compras_Monedas_IdMoneda] FOREIGN KEY ([IdMoneda]) REFERENCES [Monedas] ([IdMoneda]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Compras_Proveedores_IdProveedor] FOREIGN KEY ([IdProveedor]) REFERENCES [Proveedores] ([Id_Proveedor]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Compras_Sucursal_suc] FOREIGN KEY ([suc]) REFERENCES [Sucursal] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Compras_Usuarios_IdUsuario] FOREIGN KEY ([IdUsuario]) REFERENCES [Usuarios] ([Id_Usu]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819143653_CrearCompras'
)
BEGIN
    CREATE TABLE [ComprasDetalles] (
        [IdCompraDetalle] int NOT NULL IDENTITY,
        [IdCompra] int NOT NULL,
        [IdProducto] int NOT NULL,
        [PrecioUnitario] decimal(18,4) NOT NULL,
        [Cantidad] decimal(18,4) NOT NULL,
        [Importe] decimal(18,4) NOT NULL,
        [IVA10] decimal(18,4) NOT NULL,
        [IVA5] decimal(18,4) NOT NULL,
        [Exenta] decimal(18,4) NOT NULL,
        [Grabado10] decimal(18,4) NOT NULL,
        [Grabado5] decimal(18,4) NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [UsuarioCreacion] nvarchar(max) NULL,
        [FechaModificacion] datetime2 NULL,
        [UsuarioModificacion] nvarchar(max) NULL,
        CONSTRAINT [PK_ComprasDetalles] PRIMARY KEY ([IdCompraDetalle]),
        CONSTRAINT [FK_ComprasDetalles_Compras_IdCompra] FOREIGN KEY ([IdCompra]) REFERENCES [Compras] ([IdCompra]) ON DELETE CASCADE,
        CONSTRAINT [FK_ComprasDetalles_Productos_IdProducto] FOREIGN KEY ([IdProducto]) REFERENCES [Productos] ([IdProducto]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819143653_CrearCompras'
)
BEGIN
    CREATE INDEX [IX_Compras_IdDeposito] ON [Compras] ([IdDeposito]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819143653_CrearCompras'
)
BEGIN
    CREATE INDEX [IX_Compras_IdMoneda] ON [Compras] ([IdMoneda]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819143653_CrearCompras'
)
BEGIN
    CREATE INDEX [IX_Compras_IdProveedor] ON [Compras] ([IdProveedor]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819143653_CrearCompras'
)
BEGIN
    CREATE INDEX [IX_Compras_IdUsuario] ON [Compras] ([IdUsuario]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819143653_CrearCompras'
)
BEGIN
    CREATE INDEX [IX_Compras_suc] ON [Compras] ([suc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819143653_CrearCompras'
)
BEGIN
    CREATE INDEX [IX_ComprasDetalles_IdCompra] ON [ComprasDetalles] ([IdCompra]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819143653_CrearCompras'
)
BEGIN
    CREATE INDEX [IX_ComprasDetalles_IdProducto] ON [ComprasDetalles] ([IdProducto]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819143653_CrearCompras'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250819143653_CrearCompras', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819164337_AgregarTablaCajas'
)
BEGIN
    CREATE TABLE [Cajas] (
        [id_caja] int NOT NULL IDENTITY,
        [CantTurnos] int NULL,
        [TurnoActual] int NULL,
        [Nivel1] nchar(3) NULL,
        [Nivel2] nchar(3) NULL,
        [FacturaInicial] nchar(7) NULL,
        [Serie] int NULL,
        [Timbrado] nchar(8) NULL,
        [VigenciaDel] datetime2 NULL,
        [VigenciaAl] datetime2 NULL,
        [NombreImpresora] nvarchar(40) NULL,
        [BloquearFactura] bit NULL,
        [CajaActual] int NULL,
        [FechaActualCaja] datetime2 NULL,
        [Imprimir_Factura] int NULL,
        [Nivel1R] nchar(3) NULL,
        [Nivel2R] nchar(3) NULL,
        [FacturaInicialR] nchar(7) NULL,
        [SerieR] int NULL,
        [TimbradoR] nchar(8) NULL,
        [VigenciaDelR] datetime2 NULL,
        [VigenciaAlR] datetime2 NULL,
        [NombreImpresoraR] nvarchar(40) NULL,
        [BloquearFacturaR] bit NULL,
        [Imprimir_remisionR] bit NULL,
        [Nivel1NC] nchar(3) NULL,
        [Nivel2NC] nchar(3) NULL,
        [NumeroNC] nchar(7) NULL,
        [SerieNC] int NULL,
        [TimbradoNC] nchar(8) NULL,
        [VigenciaDelNC] datetime2 NULL,
        [VigenciaAlNC] datetime2 NULL,
        [NombreImpresoraNC] nvarchar(40) NULL,
        [BloquearFacturaNC] bit NULL,
        [Imprimir_remisionNC] bit NULL,
        [Nivel1Recibo] nchar(3) NULL,
        [Nivel2Recibo] nchar(3) NULL,
        [NumeroRecibo] nchar(7) NULL,
        [SerieRecibo] int NULL,
        [TimbradoRecibo] nchar(8) NULL,
        [VigenciaDelRecibo] datetime2 NULL,
        [VigenciaAlRecibo] datetime2 NULL,
        [NombreImpresoraRecibo] nvarchar(40) NULL,
        [BloquearFacturaRecibo] bit NULL,
        [Imprimir_remisionRecibo] bit NULL,
        [modelo_factura] nvarchar(10) NULL,
        [anular_item] nvarchar(13) NULL,
        [bloquear_fechaCaja] bit NULL,
        [cierre_simultaneo] nvarchar(50) NULL,
        [numero_correlativo] bit NULL,
        CONSTRAINT [PK_Cajas] PRIMARY KEY ([id_caja])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819164337_AgregarTablaCajas'
)
BEGIN
    CREATE INDEX [IX_Compras_IdCaja] ON [Compras] ([IdCaja]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819164337_AgregarTablaCajas'
)
BEGIN
    ALTER TABLE [Compras] ADD CONSTRAINT [FK_Compras_Cajas_IdCaja] FOREIGN KEY ([IdCaja]) REFERENCES [Cajas] ([id_caja]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819164337_AgregarTablaCajas'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250819164337_AgregarTablaCajas', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819174306_AddTiposPago'
)
BEGIN
    ALTER TABLE [Compras] ADD [IdTipoPago] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819174306_AddTiposPago'
)
BEGIN
    CREATE TABLE [TiposPago] (
        [IdTipoPago] int NOT NULL IDENTITY,
        [Nombre] nvarchar(50) NOT NULL,
        [EsCredito] bit NOT NULL DEFAULT CAST(0 AS bit),
        [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
        [Orden] int NOT NULL DEFAULT 0,
        CONSTRAINT [PK_TiposPago] PRIMARY KEY ([IdTipoPago])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819174306_AddTiposPago'
)
BEGIN
    CREATE INDEX [IX_Compras_IdTipoPago] ON [Compras] ([IdTipoPago]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819174306_AddTiposPago'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TiposPago_Nombre] ON [TiposPago] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819174306_AddTiposPago'
)
BEGIN
    ALTER TABLE [Compras] ADD CONSTRAINT [FK_Compras_TiposPago_IdTipoPago] FOREIGN KEY ([IdTipoPago]) REFERENCES [TiposPago] ([IdTipoPago]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819174306_AddTiposPago'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250819174306_AddTiposPago', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819183333_AddTiposDocumentoOperacion_CompraFK'
)
BEGIN
    ALTER TABLE [Compras] ADD [IdTipoDocumentoOperacion] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819183333_AddTiposDocumentoOperacion_CompraFK'
)
BEGIN
    ALTER TABLE [Compras] ADD [TipoDocumentoOperacionIdTipoDocumentoOperacion] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819183333_AddTiposDocumentoOperacion_CompraFK'
)
BEGIN
    CREATE TABLE [TiposDocumentoOperacion] (
        [IdTipoDocumentoOperacion] int NOT NULL IDENTITY,
        [Nombre] nvarchar(50) NOT NULL,
        [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
        [Orden] int NOT NULL DEFAULT 0,
        CONSTRAINT [PK_TiposDocumentoOperacion] PRIMARY KEY ([IdTipoDocumentoOperacion])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819183333_AddTiposDocumentoOperacion_CompraFK'
)
BEGIN
    CREATE INDEX [IX_Compras_IdTipoDocumentoOperacion] ON [Compras] ([IdTipoDocumentoOperacion]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819183333_AddTiposDocumentoOperacion_CompraFK'
)
BEGIN
    CREATE INDEX [IX_Compras_TipoDocumentoOperacionIdTipoDocumentoOperacion] ON [Compras] ([TipoDocumentoOperacionIdTipoDocumentoOperacion]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819183333_AddTiposDocumentoOperacion_CompraFK'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TiposDocumentoOperacion_Nombre] ON [TiposDocumentoOperacion] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819183333_AddTiposDocumentoOperacion_CompraFK'
)
BEGIN
    ALTER TABLE [Compras] ADD CONSTRAINT [FK_Compras_TiposDocumentoOperacion_IdTipoDocumentoOperacion] FOREIGN KEY ([IdTipoDocumentoOperacion]) REFERENCES [TiposDocumentoOperacion] ([IdTipoDocumentoOperacion]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819183333_AddTiposDocumentoOperacion_CompraFK'
)
BEGIN
    ALTER TABLE [Compras] ADD CONSTRAINT [FK_Compras_TiposDocumentoOperacion_TipoDocumentoOperacionIdTipoDocumentoOperacion] FOREIGN KEY ([TipoDocumentoOperacionIdTipoDocumentoOperacion]) REFERENCES [TiposDocumentoOperacion] ([IdTipoDocumentoOperacion]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819183333_AddTiposDocumentoOperacion_CompraFK'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250819183333_AddTiposDocumentoOperacion_CompraFK', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250824133256_AddMedioPagoToCompras'
)
BEGIN
    IF COL_LENGTH('Compras','MedioPago') IS NULL ALTER TABLE [Compras] ADD [MedioPago] NVARCHAR(13) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250824133256_AddMedioPagoToCompras'
)
BEGIN
    IF COL_LENGTH('Compras','MedioPago') IS NOT NULL UPDATE [Compras] SET [MedioPago] = 'EFECTIVO' WHERE [MedioPago] IS NULL OR LTRIM(RTRIM([MedioPago])) = ''; 
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250824133256_AddMedioPagoToCompras'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250824133256_AddMedioPagoToCompras', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250825220702_AjustesStock_CrearTablas'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'IdMonedaPrecio' AND Object_ID = Object_ID(N'[dbo].[Productos]'))
        ALTER TABLE [dbo].[Productos] ADD [IdMonedaPrecio] int NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'CambioDelDia' AND Object_ID = Object_ID(N'[dbo].[ComprasDetalles]'))
        ALTER TABLE [dbo].[ComprasDetalles] ADD [CambioDelDia] decimal(18,4) NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'MedioPago' AND Object_ID = Object_ID(N'[dbo].[Compras]'))
    BEGIN
        ALTER TABLE [dbo].[Compras] ADD [MedioPago] nvarchar(13) NOT NULL CONSTRAINT [DF_Compras_MedioPago] DEFAULT N'';
    END
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Productos_IdMonedaPrecio' AND object_id = OBJECT_ID(N'[dbo].[Productos]'))
        CREATE INDEX [IX_Productos_IdMonedaPrecio] ON [dbo].[Productos]([IdMonedaPrecio]);
    IF NOT EXISTS (
        SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Productos_Monedas_IdMonedaPrecio' AND parent_object_id = OBJECT_ID(N'[dbo].[Productos]')
    )
    BEGIN
        ALTER TABLE [dbo].[Productos]  WITH CHECK ADD  CONSTRAINT [FK_Productos_Monedas_IdMonedaPrecio] FOREIGN KEY([IdMonedaPrecio])
        REFERENCES [dbo].[Monedas] ([IdMoneda]);
        ALTER TABLE [dbo].[Productos] CHECK CONSTRAINT [FK_Productos_Monedas_IdMonedaPrecio];
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250825220702_AjustesStock_CrearTablas'
)
BEGIN
    CREATE TABLE [AjustesStock] (
        [IdAjusteStock] int NOT NULL IDENTITY,
        [suc] int NOT NULL,
        [IdCaja] int NULL,
        [Turno] int NULL,
        [FechaAjuste] datetime2 NOT NULL,
        [Usuario] nvarchar(50) NOT NULL,
        [Comentario] nvarchar(280) NULL,
        [TotalMonto] decimal(18,0) NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [UsuarioCreacion] nvarchar(50) NULL,
        [FechaModificacion] datetime2 NULL,
        [UsuarioModificacion] nvarchar(50) NULL,
        CONSTRAINT [PK_AjustesStock] PRIMARY KEY ([IdAjusteStock])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250825220702_AjustesStock_CrearTablas'
)
BEGIN
    CREATE TABLE [AjustesStockDetalles] (
        [IdAjusteStockDetalle] int NOT NULL IDENTITY,
        [IdAjusteStock] int NOT NULL,
        [IdProducto] int NOT NULL,
        [StockAjuste] decimal(18,2) NOT NULL,
        [StockSistema] decimal(18,2) NOT NULL,
        [Diferencia] decimal(18,2) NOT NULL,
        [Monto] decimal(18,0) NOT NULL,
        [FechaAjuste] datetime2 NOT NULL,
        [suc] int NOT NULL,
        [IdCaja] int NULL,
        [Turno] int NULL,
        [Usuario] nvarchar(50) NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [UsuarioCreacion] nvarchar(50) NULL,
        [FechaModificacion] datetime2 NULL,
        [UsuarioModificacion] nvarchar(50) NULL,
        CONSTRAINT [PK_AjustesStockDetalles] PRIMARY KEY ([IdAjusteStockDetalle]),
        CONSTRAINT [FK_AjustesStockDetalles_AjustesStock_IdAjusteStock] FOREIGN KEY ([IdAjusteStock]) REFERENCES [AjustesStock] ([IdAjusteStock]) ON DELETE CASCADE,
        CONSTRAINT [FK_AjustesStockDetalles_Productos_IdProducto] FOREIGN KEY ([IdProducto]) REFERENCES [Productos] ([IdProducto]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250825220702_AjustesStock_CrearTablas'
)
BEGIN
    CREATE INDEX [IX_AjustesStockDetalles_IdAjusteStock] ON [AjustesStockDetalles] ([IdAjusteStock]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250825220702_AjustesStock_CrearTablas'
)
BEGIN
    CREATE INDEX [IX_AjustesStockDetalles_IdProducto] ON [AjustesStockDetalles] ([IdProducto]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250825220702_AjustesStock_CrearTablas'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250825220702_AjustesStock_CrearTablas', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250827170646_Crear_Ventas_y_Detalles'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[ClientesPrecios]', 'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[ClientesPrecios](
            [IdClientePrecio] [int] IDENTITY(1,1) NOT NULL,
            [IdCliente] [int] NOT NULL,
            [IdProducto] [int] NOT NULL,
            [PrecioFijoGs] [decimal](18,4) NULL,
            [PorcentajeDescuento] [decimal](18,2) NULL,
            [Activo] [bit] NOT NULL,
            [FechaCreacion] [datetime2] NOT NULL,
            [UsuarioCreacion] [nvarchar](50) NULL,
            CONSTRAINT [PK_ClientesPrecios] PRIMARY KEY CLUSTERED ([IdClientePrecio] ASC),
            CONSTRAINT [FK_ClientesPrecios_Clientes_IdCliente] FOREIGN KEY([IdCliente]) REFERENCES [dbo].[Clientes]([IdCliente]) ON DELETE CASCADE,
            CONSTRAINT [FK_ClientesPrecios_Productos_IdProducto] FOREIGN KEY([IdProducto]) REFERENCES [dbo].[Productos]([IdProducto]) ON DELETE NO ACTION
        );
        CREATE UNIQUE INDEX [IX_ClientesPrecios_IdCliente_IdProducto] ON [dbo].[ClientesPrecios]([IdCliente],[IdProducto]);
        CREATE INDEX [IX_ClientesPrecios_IdProducto] ON [dbo].[ClientesPrecios]([IdProducto]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250827170646_Crear_Ventas_y_Detalles'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250827170646_Crear_Ventas_y_Detalles', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250827171326_Crear_Tablas_Ventas'
)
BEGIN
    CREATE TABLE [Ventas] (
        [IdVenta] int NOT NULL IDENTITY,
        [suc] int NOT NULL,
        [IdCliente] int NULL,
        [Timbrado] nchar(8) NULL,
        [Establecimiento] nchar(3) NULL,
        [PuntoExpedicion] nchar(3) NULL,
        [NumeroFactura] nchar(7) NULL,
        [Fecha] datetime2 NOT NULL,
        [IdMoneda] int NULL,
        [SimboloMoneda] nvarchar(4) NULL,
        [CambioDelDia] decimal(18,4) NULL,
        [FormaPago] nvarchar(50) NULL,
        [Plazo] int NULL,
        [FechaVencimiento] datetime2 NULL,
        [Total] decimal(18,4) NOT NULL,
        [TotalEnLetras] nvarchar(280) NULL,
        [TipoRegistro] nvarchar(20) NULL,
        [CodigoRegistro] int NULL,
        [Comentario] nvarchar(280) NULL,
        [Estado] nvarchar(20) NULL,
        [IdCaja] int NULL,
        [Turno] int NULL,
        [IdUsuario] int NULL,
        [Vendedor] nvarchar(250) NULL,
        [CDC] nvarchar(64) NULL,
        [EstadoSifen] nvarchar(30) NULL,
        [FechaEnvioSifen] datetime2 NULL,
        [MensajeSifen] nvarchar(max) NULL,
        [XmlCDE] nvarchar(max) NULL,
        [TipoDocumento] nvarchar(50) NULL,
        [IdTipoDocumentoOperacion] int NULL,
        [TipoDocumentoOperacionIdTipoDocumentoOperacion] int NULL,
        CONSTRAINT [PK_Ventas] PRIMARY KEY ([IdVenta]),
        CONSTRAINT [FK_Ventas_Cajas_IdCaja] FOREIGN KEY ([IdCaja]) REFERENCES [Cajas] ([id_caja]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Ventas_Clientes_IdCliente] FOREIGN KEY ([IdCliente]) REFERENCES [Clientes] ([IdCliente]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Ventas_Monedas_IdMoneda] FOREIGN KEY ([IdMoneda]) REFERENCES [Monedas] ([IdMoneda]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Ventas_Sucursal_suc] FOREIGN KEY ([suc]) REFERENCES [Sucursal] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Ventas_TiposDocumentoOperacion_IdTipoDocumentoOperacion] FOREIGN KEY ([IdTipoDocumentoOperacion]) REFERENCES [TiposDocumentoOperacion] ([IdTipoDocumentoOperacion]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Ventas_TiposDocumentoOperacion_TipoDocumentoOperacionIdTipoDocumentoOperacion] FOREIGN KEY ([TipoDocumentoOperacionIdTipoDocumentoOperacion]) REFERENCES [TiposDocumentoOperacion] ([IdTipoDocumentoOperacion]),
        CONSTRAINT [FK_Ventas_Usuarios_IdUsuario] FOREIGN KEY ([IdUsuario]) REFERENCES [Usuarios] ([Id_Usu]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250827171326_Crear_Tablas_Ventas'
)
BEGIN
    CREATE TABLE [VentasDetalles] (
        [IdVentaDetalle] int NOT NULL IDENTITY,
        [IdVenta] int NOT NULL,
        [IdProducto] int NOT NULL,
        [Cantidad] decimal(18,4) NOT NULL,
        [PrecioUnitario] decimal(18,4) NOT NULL,
        [Importe] decimal(18,4) NOT NULL,
        [IVA10] decimal(18,4) NOT NULL,
        [IVA5] decimal(18,4) NOT NULL,
        [Exenta] decimal(18,4) NOT NULL,
        [Grabado10] decimal(18,4) NOT NULL,
        [Grabado5] decimal(18,4) NOT NULL,
        [CambioDelDia] decimal(18,4) NULL,
        [IdTipoIva] int NULL,
        CONSTRAINT [PK_VentasDetalles] PRIMARY KEY ([IdVentaDetalle]),
        CONSTRAINT [FK_VentasDetalles_Productos_IdProducto] FOREIGN KEY ([IdProducto]) REFERENCES [Productos] ([IdProducto]) ON DELETE NO ACTION,
        CONSTRAINT [FK_VentasDetalles_Ventas_IdVenta] FOREIGN KEY ([IdVenta]) REFERENCES [Ventas] ([IdVenta]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250827171326_Crear_Tablas_Ventas'
)
BEGIN
    CREATE INDEX [IX_Ventas_IdCaja] ON [Ventas] ([IdCaja]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250827171326_Crear_Tablas_Ventas'
)
BEGIN
    CREATE INDEX [IX_Ventas_IdCliente] ON [Ventas] ([IdCliente]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250827171326_Crear_Tablas_Ventas'
)
BEGIN
    CREATE INDEX [IX_Ventas_IdMoneda] ON [Ventas] ([IdMoneda]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250827171326_Crear_Tablas_Ventas'
)
BEGIN
    CREATE INDEX [IX_Ventas_IdTipoDocumentoOperacion] ON [Ventas] ([IdTipoDocumentoOperacion]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250827171326_Crear_Tablas_Ventas'
)
BEGIN
    CREATE INDEX [IX_Ventas_IdUsuario] ON [Ventas] ([IdUsuario]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250827171326_Crear_Tablas_Ventas'
)
BEGIN
    CREATE INDEX [IX_Ventas_suc] ON [Ventas] ([suc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250827171326_Crear_Tablas_Ventas'
)
BEGIN
    CREATE INDEX [IX_Ventas_TipoDocumentoOperacionIdTipoDocumentoOperacion] ON [Ventas] ([TipoDocumentoOperacionIdTipoDocumentoOperacion]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250827171326_Crear_Tablas_Ventas'
)
BEGIN
    CREATE INDEX [IX_VentasDetalles_IdProducto] ON [VentasDetalles] ([IdProducto]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250827171326_Crear_Tablas_Ventas'
)
BEGIN
    CREATE INDEX [IX_VentasDetalles_IdVenta] ON [VentasDetalles] ([IdVenta]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250827171326_Crear_Tablas_Ventas'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250827171326_Crear_Tablas_Ventas', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250828115502_Agregar_Presupuestos'
)
BEGIN
    ALTER TABLE [Ventas] ADD [CodigoCondicion] nvarchar(10) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250828115502_Agregar_Presupuestos'
)
BEGIN
    ALTER TABLE [Ventas] ADD [CreditoSaldo] decimal(18,4) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250828115502_Agregar_Presupuestos'
)
BEGIN
    ALTER TABLE [Ventas] ADD [EsMonedaExtranjera] bit NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250828115502_Agregar_Presupuestos'
)
BEGIN
    ALTER TABLE [Ventas] ADD [IdTipoPago] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250828115502_Agregar_Presupuestos'
)
BEGIN
    ALTER TABLE [Ventas] ADD [MedioPago] nvarchar(13) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250828115502_Agregar_Presupuestos'
)
BEGIN
    ALTER TABLE [Ventas] ADD [TipoIngreso] nvarchar(13) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250828115502_Agregar_Presupuestos'
)
BEGIN
    ALTER TABLE [Ventas] ADD [TipoPagoIdTipoPago] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250828115502_Agregar_Presupuestos'
)
BEGIN
    CREATE TABLE [Presupuestos] (
        [IdPresupuesto] int NOT NULL IDENTITY,
        [suc] int NOT NULL,
        [IdCliente] int NULL,
        [Fecha] datetime2 NOT NULL,
        [NumeroPresupuesto] nvarchar(20) NULL,
        [IdMoneda] int NULL,
        [SimboloMoneda] nvarchar(4) NULL,
        [CambioDelDia] decimal(18,4) NULL,
        [EsMonedaExtranjera] bit NULL,
        [Total] decimal(18,4) NOT NULL,
        [TotalEnLetras] nvarchar(280) NULL,
        [ValidezDias] int NULL,
        [ValidoHasta] datetime2 NULL,
        [Comentario] nvarchar(280) NULL,
        [Estado] nvarchar(20) NULL,
        [IdVentaConvertida] int NULL,
        CONSTRAINT [PK_Presupuestos] PRIMARY KEY ([IdPresupuesto]),
        CONSTRAINT [FK_Presupuestos_Clientes_IdCliente] FOREIGN KEY ([IdCliente]) REFERENCES [Clientes] ([IdCliente]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Presupuestos_Monedas_IdMoneda] FOREIGN KEY ([IdMoneda]) REFERENCES [Monedas] ([IdMoneda]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Presupuestos_Sucursal_suc] FOREIGN KEY ([suc]) REFERENCES [Sucursal] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250828115502_Agregar_Presupuestos'
)
BEGIN
    CREATE TABLE [PresupuestosDetalles] (
        [IdPresupuestoDetalle] int NOT NULL IDENTITY,
        [IdPresupuesto] int NOT NULL,
        [IdProducto] int NOT NULL,
        [Cantidad] decimal(18,4) NOT NULL,
        [PrecioUnitario] decimal(18,4) NOT NULL,
        [Importe] decimal(18,4) NOT NULL,
        [IVA10] decimal(18,4) NOT NULL,
        [IVA5] decimal(18,4) NOT NULL,
        [Exenta] decimal(18,4) NOT NULL,
        [Grabado10] decimal(18,4) NOT NULL,
        [Grabado5] decimal(18,4) NOT NULL,
        [CambioDelDia] decimal(18,4) NULL,
        [IdTipoIva] int NULL,
        CONSTRAINT [PK_PresupuestosDetalles] PRIMARY KEY ([IdPresupuestoDetalle]),
        CONSTRAINT [FK_PresupuestosDetalles_Presupuestos_IdPresupuesto] FOREIGN KEY ([IdPresupuesto]) REFERENCES [Presupuestos] ([IdPresupuesto]) ON DELETE CASCADE,
        CONSTRAINT [FK_PresupuestosDetalles_Productos_IdProducto] FOREIGN KEY ([IdProducto]) REFERENCES [Productos] ([IdProducto]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250828115502_Agregar_Presupuestos'
)
BEGIN
    CREATE INDEX [IX_Ventas_TipoPagoIdTipoPago] ON [Ventas] ([TipoPagoIdTipoPago]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250828115502_Agregar_Presupuestos'
)
BEGIN
    CREATE INDEX [IX_Presupuestos_IdCliente] ON [Presupuestos] ([IdCliente]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250828115502_Agregar_Presupuestos'
)
BEGIN
    CREATE INDEX [IX_Presupuestos_IdMoneda] ON [Presupuestos] ([IdMoneda]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250828115502_Agregar_Presupuestos'
)
BEGIN
    CREATE INDEX [IX_Presupuestos_suc] ON [Presupuestos] ([suc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250828115502_Agregar_Presupuestos'
)
BEGIN
    CREATE INDEX [IX_PresupuestosDetalles_IdPresupuesto] ON [PresupuestosDetalles] ([IdPresupuesto]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250828115502_Agregar_Presupuestos'
)
BEGIN
    CREATE INDEX [IX_PresupuestosDetalles_IdProducto] ON [PresupuestosDetalles] ([IdProducto]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250828115502_Agregar_Presupuestos'
)
BEGIN
    ALTER TABLE [Ventas] ADD CONSTRAINT [FK_Ventas_TiposPago_TipoPagoIdTipoPago] FOREIGN KEY ([TipoPagoIdTipoPago]) REFERENCES [TiposPago] ([IdTipoPago]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250828115502_Agregar_Presupuestos'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250828115502_Agregar_Presupuestos', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829112004_CrearTablaSociedades'
)
BEGIN
    IF EXISTS (
        SELECT 1 FROM sys.indexes i
        WHERE i.name = N'IX_Presupuestos_suc'
          AND i.object_id = OBJECT_ID(N'[dbo].[Presupuestos]')
    )
    BEGIN
        DROP INDEX [IX_Presupuestos_suc] ON [dbo].[Presupuestos];
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829112004_CrearTablaSociedades'
)
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes i
        WHERE i.name = N'IX_Presupuestos_IdVentaConvertida'
          AND i.object_id = OBJECT_ID(N'[dbo].[Presupuestos]')
    )
    BEGIN
        CREATE INDEX [IX_Presupuestos_IdVentaConvertida] ON [dbo].[Presupuestos] ([IdVentaConvertida]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829112004_CrearTablaSociedades'
)
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes i
        WHERE i.name = N'IX_Presupuestos_suc_NumeroPresupuesto'
          AND i.object_id = OBJECT_ID(N'[dbo].[Presupuestos]')
    )
    BEGIN
        CREATE INDEX [IX_Presupuestos_suc_NumeroPresupuesto] ON [dbo].[Presupuestos] ([suc], [NumeroPresupuesto]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829112004_CrearTablaSociedades'
)
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.foreign_keys fk
        WHERE fk.name = N'FK_Presupuestos_Ventas_IdVentaConvertida'
          AND fk.parent_object_id = OBJECT_ID(N'[dbo].[Presupuestos]')
    )
    BEGIN
        ALTER TABLE [dbo].[Presupuestos] WITH CHECK ADD CONSTRAINT [FK_Presupuestos_Ventas_IdVentaConvertida]
        FOREIGN KEY([IdVentaConvertida]) REFERENCES [dbo].[Ventas] ([IdVenta]) ON DELETE SET NULL;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829112004_CrearTablaSociedades'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250829112004_CrearTablaSociedades', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829113157_CrearTablaSociedades_Fix'
)
BEGIN
    IF EXISTS (
        SELECT 1 FROM sys.indexes i
        WHERE i.name = N'IX_Presupuestos_suc'
          AND i.object_id = OBJECT_ID(N'[dbo].[Presupuestos]')
    )
    BEGIN
        DROP INDEX [IX_Presupuestos_suc] ON [dbo].[Presupuestos];
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829113157_CrearTablaSociedades_Fix'
)
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes i
        WHERE i.name = N'IX_Presupuestos_IdVentaConvertida'
          AND i.object_id = OBJECT_ID(N'[dbo].[Presupuestos]')
    )
    BEGIN
        CREATE INDEX [IX_Presupuestos_IdVentaConvertida] ON [dbo].[Presupuestos] ([IdVentaConvertida]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829113157_CrearTablaSociedades_Fix'
)
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes i
        WHERE i.name = N'IX_Presupuestos_suc_NumeroPresupuesto'
          AND i.object_id = OBJECT_ID(N'[dbo].[Presupuestos]')
    )
    BEGIN
        CREATE INDEX [IX_Presupuestos_suc_NumeroPresupuesto] ON [dbo].[Presupuestos] ([suc], [NumeroPresupuesto]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829113157_CrearTablaSociedades_Fix'
)
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.foreign_keys fk
        WHERE fk.name = N'FK_Presupuestos_Ventas_IdVentaConvertida'
          AND fk.parent_object_id = OBJECT_ID(N'[dbo].[Presupuestos]')
    )
    BEGIN
        ALTER TABLE [dbo].[Presupuestos] WITH CHECK ADD CONSTRAINT [FK_Presupuestos_Ventas_IdVentaConvertida]
        FOREIGN KEY([IdVentaConvertida]) REFERENCES [dbo].[Ventas] ([IdVenta]) ON DELETE SET NULL;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829113157_CrearTablaSociedades_Fix'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250829113157_CrearTablaSociedades_Fix', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829113743_CrearTablaSociedades_Build2'
)
BEGIN
    IF EXISTS (
        SELECT 1 FROM sys.indexes i
        WHERE i.name = N'IX_Presupuestos_suc'
          AND i.object_id = OBJECT_ID(N'[dbo].[Presupuestos]')
    )
    BEGIN
        DROP INDEX [IX_Presupuestos_suc] ON [dbo].[Presupuestos];
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829113743_CrearTablaSociedades_Build2'
)
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes i
        WHERE i.name = N'IX_Presupuestos_IdVentaConvertida'
          AND i.object_id = OBJECT_ID(N'[dbo].[Presupuestos]')
    )
    BEGIN
        CREATE INDEX [IX_Presupuestos_IdVentaConvertida] ON [dbo].[Presupuestos] ([IdVentaConvertida]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829113743_CrearTablaSociedades_Build2'
)
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes i
        WHERE i.name = N'IX_Presupuestos_suc_NumeroPresupuesto'
          AND i.object_id = OBJECT_ID(N'[dbo].[Presupuestos]')
    )
    BEGIN
        CREATE INDEX [IX_Presupuestos_suc_NumeroPresupuesto] ON [dbo].[Presupuestos] ([suc], [NumeroPresupuesto]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829113743_CrearTablaSociedades_Build2'
)
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.foreign_keys fk
        WHERE fk.name = N'FK_Presupuestos_Ventas_IdVentaConvertida'
          AND fk.parent_object_id = OBJECT_ID(N'[dbo].[Presupuestos]')
    )
    BEGIN
        ALTER TABLE [dbo].[Presupuestos] WITH CHECK ADD CONSTRAINT [FK_Presupuestos_Ventas_IdVentaConvertida]
        FOREIGN KEY([IdVentaConvertida]) REFERENCES [dbo].[Ventas] ([IdVenta]) ON DELETE SET NULL;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829113743_CrearTablaSociedades_Build2'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250829113743_CrearTablaSociedades_Build2', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829114504_CrearTablaSociedades_Final'
)
BEGIN
    IF EXISTS (
        SELECT 1 FROM sys.indexes i
        WHERE i.name = N'IX_Presupuestos_suc'
          AND i.object_id = OBJECT_ID(N'[dbo].[Presupuestos]')
    )
    BEGIN
        DROP INDEX [IX_Presupuestos_suc] ON [dbo].[Presupuestos];
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829114504_CrearTablaSociedades_Final'
)
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes i
        WHERE i.name = N'IX_Presupuestos_IdVentaConvertida'
          AND i.object_id = OBJECT_ID(N'[dbo].[Presupuestos]')
    )
    BEGIN
        CREATE INDEX [IX_Presupuestos_IdVentaConvertida] ON [dbo].[Presupuestos] ([IdVentaConvertida]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829114504_CrearTablaSociedades_Final'
)
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes i
        WHERE i.name = N'IX_Presupuestos_suc_NumeroPresupuesto'
          AND i.object_id = OBJECT_ID(N'[dbo].[Presupuestos]')
    )
    BEGIN
        CREATE INDEX [IX_Presupuestos_suc_NumeroPresupuesto] ON [dbo].[Presupuestos] ([suc], [NumeroPresupuesto]);
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829114504_CrearTablaSociedades_Final'
)
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.foreign_keys fk
        WHERE fk.name = N'FK_Presupuestos_Ventas_IdVentaConvertida'
          AND fk.parent_object_id = OBJECT_ID(N'[dbo].[Presupuestos]')
    )
    BEGIN
        ALTER TABLE [dbo].[Presupuestos] WITH CHECK ADD CONSTRAINT [FK_Presupuestos_Ventas_IdVentaConvertida]
        FOREIGN KEY([IdVentaConvertida]) REFERENCES [dbo].[Ventas] ([IdVenta]) ON DELETE SET NULL;
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829114504_CrearTablaSociedades_Final'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250829114504_CrearTablaSociedades_Final', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829121914_Sociedad_AfterKill'
)
BEGIN
    CREATE TABLE [Sociedades] (
        [IdSociedad] int NOT NULL IDENTITY,
        [Nombre] nvarchar(150) NOT NULL,
        [RUC] nvarchar(15) NOT NULL,
        [DV] int NULL,
        [Direccion] nvarchar(255) NOT NULL,
        [NumeroCasa] nvarchar(10) NULL,
        [Departamento] int NULL,
        [Ciudad] int NULL,
        [Distrito] int NULL,
        [Telefono] nvarchar(50) NULL,
        [Email] nvarchar(200) NULL,
        [TipoContribuyente] int NULL,
        [IdCsc] nvarchar(50) NULL,
        [Csc] nvarchar(50) NULL,
        [PathCertificadoP12] nvarchar(400) NULL,
        [PasswordCertificadoP12] nvarchar(400) NULL,
        [PathCertificadoPem] nvarchar(400) NULL,
        [PathCertificadoCrt] nvarchar(400) NULL,
        [PathArchivoSinFirma] nvarchar(400) NULL,
        [PathArchivoFirmado] nvarchar(400) NULL,
        [DeUrlQr] nvarchar(400) NULL,
        [DeUrlEnvioDocumento] nvarchar(400) NULL,
        [DeUrlEnvioEvento] nvarchar(400) NULL,
        [DeUrlEnvioDocumentoLote] nvarchar(400) NULL,
        [DeUrlConsultaDocumento] nvarchar(400) NULL,
        [DeUrlConsultaDocumentoLote] nvarchar(400) NULL,
        [DeUrlConsultaRuc] nvarchar(400) NULL,
        [ServidorSifen] nvarchar(50) NULL,
        [DeConexion] int NULL,
        [FechaAuditoria] datetime2 NULL,
        [Usuario] nvarchar(10) NULL,
        CONSTRAINT [PK_Sociedades] PRIMARY KEY ([IdSociedad])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829121914_Sociedad_AfterKill'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250829121914_Sociedad_AfterKill', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829133007_Distrito_CreateAndLink'
)
BEGIN
    CREATE TABLE [Distritos] (
        [IdDistrito] int NOT NULL IDENTITY,
        [Nombre] nvarchar(120) NOT NULL,
        CONSTRAINT [PK_Distritos] PRIMARY KEY ([IdDistrito])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829133007_Distrito_CreateAndLink'
)
BEGIN
    CREATE INDEX [IX_Sociedades_Distrito] ON [Sociedades] ([Distrito]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829133007_Distrito_CreateAndLink'
)
BEGIN
    UPDATE Sociedades SET Distrito = NULL WHERE Distrito IS NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829133007_Distrito_CreateAndLink'
)
BEGIN
                    ALTER TABLE [Sociedades] WITH NOCHECK
                    ADD CONSTRAINT [FK_Sociedades_Distritos_Distrito]
                    FOREIGN KEY ([Distrito]) REFERENCES [Distritos]([IdDistrito]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829133007_Distrito_CreateAndLink'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250829133007_Distrito_CreateAndLink', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829135146_Distrito_AddIdCiudad_FK'
)
BEGIN
    ALTER TABLE [Distritos] ADD [IdCiudad] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829135146_Distrito_AddIdCiudad_FK'
)
BEGIN
    CREATE INDEX [IX_Distritos_IdCiudad] ON [Distritos] ([IdCiudad]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829135146_Distrito_AddIdCiudad_FK'
)
BEGIN
    ALTER TABLE [Distritos] ADD CONSTRAINT [FK_Distritos_Ciudades_IdCiudad] FOREIGN KEY ([IdCiudad]) REFERENCES [Ciudades] ([IdCiudad]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829135146_Distrito_AddIdCiudad_FK'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250829135146_Distrito_AddIdCiudad_FK', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829164413_Crear_Catalogo_Geografico_Sifen'
)
BEGIN
    CREATE TABLE [departamento] (
        [Numero] int NOT NULL,
        [Nombre] varchar(100) NOT NULL,
        CONSTRAINT [PK_departamento] PRIMARY KEY ([Numero])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829164413_Crear_Catalogo_Geografico_Sifen'
)
BEGIN
    CREATE TABLE [distrito] (
        [Numero] int NOT NULL,
        [Nombre] varchar(120) NOT NULL,
        [Departamento] int NOT NULL,
        CONSTRAINT [PK_distrito] PRIMARY KEY ([Numero]),
        CONSTRAINT [FK_distrito_departamento_Departamento] FOREIGN KEY ([Departamento]) REFERENCES [departamento] ([Numero]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829164413_Crear_Catalogo_Geografico_Sifen'
)
BEGIN
    CREATE TABLE [ciudad] (
        [Numero] int NOT NULL,
        [Nombre] varchar(120) NOT NULL,
        [Departamento] int NOT NULL,
        [Distrito] int NOT NULL,
        CONSTRAINT [PK_ciudad] PRIMARY KEY ([Numero]),
        CONSTRAINT [FK_ciudad_departamento_Departamento] FOREIGN KEY ([Departamento]) REFERENCES [departamento] ([Numero]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ciudad_distrito_Distrito] FOREIGN KEY ([Distrito]) REFERENCES [distrito] ([Numero]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829164413_Crear_Catalogo_Geografico_Sifen'
)
BEGIN
    CREATE INDEX [IX_departamento_Nombre] ON [departamento] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829164413_Crear_Catalogo_Geografico_Sifen'
)
BEGIN
    CREATE INDEX [IX_distrito_Departamento_Nombre] ON [distrito] ([Departamento], [Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829164413_Crear_Catalogo_Geografico_Sifen'
)
BEGIN
    CREATE INDEX [IX_ciudad_Departamento_Distrito_Nombre] ON [ciudad] ([Departamento], [Distrito], [Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829164413_Crear_Catalogo_Geografico_Sifen'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250829164413_Crear_Catalogo_Geografico_Sifen', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829173731_Drop_Legacy_Ciudades_Distritos'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250829173731_Drop_Legacy_Ciudades_Distritos', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829174914_DropOnly_Legacy_Ciudades_Distritos'
)
BEGIN
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Clientes_Ciudades_IdCiudad')
        ALTER TABLE [Clientes] DROP CONSTRAINT [FK_Clientes_Ciudades_IdCiudad];
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Sociedades_Distritos_Distrito')
        ALTER TABLE [Sociedades] DROP CONSTRAINT [FK_Sociedades_Distritos_Distrito];
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Sucursal_Ciudades_IdCiudad')
        ALTER TABLE [Sucursal] DROP CONSTRAINT [FK_Sucursal_Ciudades_IdCiudad];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829174914_DropOnly_Legacy_Ciudades_Distritos'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[Distritos]', N'U') IS NOT NULL DROP TABLE [dbo].[Distritos];
    IF OBJECT_ID(N'[dbo].[Ciudades]', N'U') IS NOT NULL DROP TABLE [dbo].[Ciudades];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829174914_DropOnly_Legacy_Ciudades_Distritos'
)
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes 
        WHERE name = N'IX_Sociedades_Ciudad' AND object_id = OBJECT_ID(N'[dbo].[Sociedades]')
    )
        CREATE INDEX [IX_Sociedades_Ciudad] ON [dbo].[Sociedades]([Ciudad]);
    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes 
        WHERE name = N'IX_Sociedades_Departamento' AND object_id = OBJECT_ID(N'[dbo].[Sociedades]')
    )
        CREATE INDEX [IX_Sociedades_Departamento] ON [dbo].[Sociedades]([Departamento]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829174914_DropOnly_Legacy_Ciudades_Distritos'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Clientes_ciudad_IdCiudad')
        ALTER TABLE [Clientes]  WITH CHECK ADD  CONSTRAINT [FK_Clientes_ciudad_IdCiudad] FOREIGN KEY([IdCiudad])
        REFERENCES [ciudad] ([Numero]);
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Sucursal_ciudad_IdCiudad')
        ALTER TABLE [Sucursal]  WITH CHECK ADD  CONSTRAINT [FK_Sucursal_ciudad_IdCiudad] FOREIGN KEY([IdCiudad])
        REFERENCES [ciudad] ([Numero]);
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Sociedades_ciudad_Ciudad')
        ALTER TABLE [Sociedades]  WITH CHECK ADD  CONSTRAINT [FK_Sociedades_ciudad_Ciudad] FOREIGN KEY([Ciudad])
        REFERENCES [ciudad] ([Numero]) ON DELETE SET NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Sociedades_departamento_Departamento')
        ALTER TABLE [Sociedades]  WITH CHECK ADD  CONSTRAINT [FK_Sociedades_departamento_Departamento] FOREIGN KEY([Departamento])
        REFERENCES [departamento] ([Numero]) ON DELETE SET NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Sociedades_distrito_Distrito')
        ALTER TABLE [Sociedades]  WITH CHECK ADD  CONSTRAINT [FK_Sociedades_distrito_Distrito] FOREIGN KEY([Distrito])
        REFERENCES [distrito] ([Numero]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829174914_DropOnly_Legacy_Ciudades_Distritos'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250829174914_DropOnly_Legacy_Ciudades_Distritos', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250829175735_DropLegacy_Ciudades_Distritos_NoCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250829175735_DropLegacy_Ciudades_Distritos_NoCreate', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250909182804_Agregar_Pagos_Cuotas_Timbrado_ComposicionCaja'
)
BEGIN
    CREATE TABLE [ComposicionesCaja] (
        [IdComposicionCaja] int NOT NULL IDENTITY,
        [IdVenta] int NOT NULL,
        [Fecha] datetime2 NOT NULL,
        [IdMoneda] int NULL,
        [TipoCambioAplicado] decimal(18,4) NULL,
        [MontoTotal] decimal(18,4) NOT NULL,
        CONSTRAINT [PK_ComposicionesCaja] PRIMARY KEY ([IdComposicionCaja]),
        CONSTRAINT [FK_ComposicionesCaja_Monedas_IdMoneda] FOREIGN KEY ([IdMoneda]) REFERENCES [Monedas] ([IdMoneda]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ComposicionesCaja_Ventas_IdVenta] FOREIGN KEY ([IdVenta]) REFERENCES [Ventas] ([IdVenta]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250909182804_Agregar_Pagos_Cuotas_Timbrado_ComposicionCaja'
)
BEGIN
    CREATE TABLE [Timbrados] (
        [IdTimbrado] int NOT NULL IDENTITY,
        [NumeroTimbrado] nvarchar(8) NOT NULL,
        [FechaInicioVigencia] datetime2 NOT NULL,
        [FechaFinVigencia] datetime2 NOT NULL,
        [Establecimiento] nvarchar(3) NOT NULL,
        [PuntoExpedicion] nvarchar(3) NOT NULL,
        [TipoDocumento] nvarchar(12) NULL,
        [IdSucursal] int NOT NULL,
        [IdCaja] int NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Timbrados] PRIMARY KEY ([IdTimbrado]),
        CONSTRAINT [FK_Timbrados_Cajas_IdCaja] FOREIGN KEY ([IdCaja]) REFERENCES [Cajas] ([id_caja]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Timbrados_Sucursal_IdSucursal] FOREIGN KEY ([IdSucursal]) REFERENCES [Sucursal] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250909182804_Agregar_Pagos_Cuotas_Timbrado_ComposicionCaja'
)
BEGIN
    CREATE TABLE [VentasPagos] (
        [IdVentaPago] int NOT NULL IDENTITY,
        [IdVenta] int NOT NULL,
        [CondicionOperacion] int NOT NULL,
        [IdMoneda] int NULL,
        [TipoCambio] decimal(18,4) NULL,
        [ImporteTotal] decimal(18,4) NOT NULL,
        [Anticipo] decimal(18,4) NULL,
        [DescuentoTotal] decimal(18,4) NULL,
        [RecargoTotal] decimal(18,4) NULL,
        CONSTRAINT [PK_VentasPagos] PRIMARY KEY ([IdVentaPago]),
        CONSTRAINT [FK_VentasPagos_Monedas_IdMoneda] FOREIGN KEY ([IdMoneda]) REFERENCES [Monedas] ([IdMoneda]) ON DELETE NO ACTION,
        CONSTRAINT [FK_VentasPagos_Ventas_IdVenta] FOREIGN KEY ([IdVenta]) REFERENCES [Ventas] ([IdVenta]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250909182804_Agregar_Pagos_Cuotas_Timbrado_ComposicionCaja'
)
BEGIN
    CREATE TABLE [ComposicionesCajaDetalles] (
        [IdComposicionCajaDetalle] int NOT NULL IDENTITY,
        [IdComposicionCaja] int NOT NULL,
        [Medio] int NOT NULL,
        [IdMoneda] int NULL,
        [TipoCambio] decimal(18,4) NULL,
        [Factor] decimal(18,4) NOT NULL,
        [Monto] decimal(18,4) NOT NULL,
        [MontoGs] decimal(18,4) NOT NULL,
        [BancoCheque] nvarchar(40) NULL,
        [NumeroCheque] nvarchar(30) NULL,
        [FechaCobroCheque] datetime2 NULL,
        [TipoTarjeta] int NULL,
        [MarcaTarjeta] nvarchar(50) NULL,
        [Ultimos4] nvarchar(4) NULL,
        [NumeroAutorizacion] nvarchar(50) NULL,
        [NombreEmisorTarjeta] nvarchar(80) NULL,
        [BancoTransferencia] nvarchar(50) NULL,
        [NumeroComprobante] nvarchar(60) NULL,
        [Observacion] nvarchar(200) NULL,
        CONSTRAINT [PK_ComposicionesCajaDetalles] PRIMARY KEY ([IdComposicionCajaDetalle]),
        CONSTRAINT [FK_ComposicionesCajaDetalles_ComposicionesCaja_IdComposicionCaja] FOREIGN KEY ([IdComposicionCaja]) REFERENCES [ComposicionesCaja] ([IdComposicionCaja]) ON DELETE CASCADE,
        CONSTRAINT [FK_ComposicionesCajaDetalles_Monedas_IdMoneda] FOREIGN KEY ([IdMoneda]) REFERENCES [Monedas] ([IdMoneda]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250909182804_Agregar_Pagos_Cuotas_Timbrado_ComposicionCaja'
)
BEGIN
    CREATE TABLE [VentasCuotas] (
        [IdVentaCuota] int NOT NULL IDENTITY,
        [IdVentaPago] int NOT NULL,
        [NumeroCuota] int NOT NULL,
        [FechaVencimiento] datetime2 NOT NULL,
        [MontoCuota] decimal(18,4) NOT NULL,
        [Pagada] bit NOT NULL,
        [FechaPago] datetime2 NULL,
        CONSTRAINT [PK_VentasCuotas] PRIMARY KEY ([IdVentaCuota]),
        CONSTRAINT [FK_VentasCuotas_VentasPagos_IdVentaPago] FOREIGN KEY ([IdVentaPago]) REFERENCES [VentasPagos] ([IdVentaPago]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250909182804_Agregar_Pagos_Cuotas_Timbrado_ComposicionCaja'
)
BEGIN
    CREATE TABLE [VentasPagosDetalles] (
        [IdVentaPagoDetalle] int NOT NULL IDENTITY,
        [IdVentaPago] int NOT NULL,
        [Medio] int NOT NULL,
        [IdMoneda] int NULL,
        [TipoCambio] decimal(18,4) NULL,
        [Monto] decimal(18,4) NOT NULL,
        [MontoGs] decimal(18,4) NOT NULL,
        [TipoTarjeta] int NULL,
        [MarcaTarjeta] nvarchar(50) NULL,
        [NombreEmisorTarjeta] nvarchar(80) NULL,
        [Ultimos4] nvarchar(4) NULL,
        [NumeroAutorizacion] nvarchar(50) NULL,
        [BancoCheque] nvarchar(40) NULL,
        [NumeroCheque] nvarchar(30) NULL,
        [FechaCobroCheque] datetime2 NULL,
        [BancoTransferencia] nvarchar(50) NULL,
        [NumeroComprobante] nvarchar(60) NULL,
        [Observacion] nvarchar(200) NULL,
        CONSTRAINT [PK_VentasPagosDetalles] PRIMARY KEY ([IdVentaPagoDetalle]),
        CONSTRAINT [FK_VentasPagosDetalles_Monedas_IdMoneda] FOREIGN KEY ([IdMoneda]) REFERENCES [Monedas] ([IdMoneda]) ON DELETE NO ACTION,
        CONSTRAINT [FK_VentasPagosDetalles_VentasPagos_IdVentaPago] FOREIGN KEY ([IdVentaPago]) REFERENCES [VentasPagos] ([IdVentaPago]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250909182804_Agregar_Pagos_Cuotas_Timbrado_ComposicionCaja'
)
BEGIN
    CREATE INDEX [IX_ComposicionesCaja_IdMoneda] ON [ComposicionesCaja] ([IdMoneda]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250909182804_Agregar_Pagos_Cuotas_Timbrado_ComposicionCaja'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ComposicionesCaja_IdVenta] ON [ComposicionesCaja] ([IdVenta]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250909182804_Agregar_Pagos_Cuotas_Timbrado_ComposicionCaja'
)
BEGIN
    CREATE INDEX [IX_ComposicionesCajaDetalles_IdComposicionCaja] ON [ComposicionesCajaDetalles] ([IdComposicionCaja]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250909182804_Agregar_Pagos_Cuotas_Timbrado_ComposicionCaja'
)
BEGIN
    CREATE INDEX [IX_ComposicionesCajaDetalles_IdMoneda] ON [ComposicionesCajaDetalles] ([IdMoneda]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250909182804_Agregar_Pagos_Cuotas_Timbrado_ComposicionCaja'
)
BEGIN
    CREATE INDEX [IX_Timbrados_IdCaja] ON [Timbrados] ([IdCaja]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250909182804_Agregar_Pagos_Cuotas_Timbrado_ComposicionCaja'
)
BEGIN
    CREATE INDEX [IX_Timbrados_IdSucursal] ON [Timbrados] ([IdSucursal]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250909182804_Agregar_Pagos_Cuotas_Timbrado_ComposicionCaja'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Timbrados_NumeroTimbrado_Establecimiento_PuntoExpedicion_TipoDocumento_IdSucursal_IdCaja] ON [Timbrados] ([NumeroTimbrado], [Establecimiento], [PuntoExpedicion], [TipoDocumento], [IdSucursal], [IdCaja]) WHERE [TipoDocumento] IS NOT NULL AND [IdCaja] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250909182804_Agregar_Pagos_Cuotas_Timbrado_ComposicionCaja'
)
BEGIN
    CREATE UNIQUE INDEX [IX_VentasCuotas_IdVentaPago_NumeroCuota] ON [VentasCuotas] ([IdVentaPago], [NumeroCuota]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250909182804_Agregar_Pagos_Cuotas_Timbrado_ComposicionCaja'
)
BEGIN
    CREATE INDEX [IX_VentasPagos_IdMoneda] ON [VentasPagos] ([IdMoneda]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250909182804_Agregar_Pagos_Cuotas_Timbrado_ComposicionCaja'
)
BEGIN
    CREATE UNIQUE INDEX [IX_VentasPagos_IdVenta] ON [VentasPagos] ([IdVenta]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250909182804_Agregar_Pagos_Cuotas_Timbrado_ComposicionCaja'
)
BEGIN
    CREATE INDEX [IX_VentasPagosDetalles_IdMoneda] ON [VentasPagosDetalles] ([IdMoneda]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250909182804_Agregar_Pagos_Cuotas_Timbrado_ComposicionCaja'
)
BEGIN
    CREATE INDEX [IX_VentasPagosDetalles_IdVentaPago] ON [VentasPagosDetalles] ([IdVentaPago]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250909182804_Agregar_Pagos_Cuotas_Timbrado_ComposicionCaja'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250909182804_Agregar_Pagos_Cuotas_Timbrado_ComposicionCaja', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250910172515_Fix_ClientePrecio_FKs'
)
BEGIN
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Presupuestos_Clientes_ClienteIdCliente')
        ALTER TABLE [dbo].[Presupuestos] DROP CONSTRAINT [FK_Presupuestos_Clientes_ClienteIdCliente];
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Presupuestos_Monedas_MonedaIdMoneda')
        ALTER TABLE [dbo].[Presupuestos] DROP CONSTRAINT [FK_Presupuestos_Monedas_MonedaIdMoneda];
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Presupuestos_Sucursal_SucursalId')
        ALTER TABLE [dbo].[Presupuestos] DROP CONSTRAINT [FK_Presupuestos_Sucursal_SucursalId];
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PresupuestosDetalles_Presupuestos_PresupuestoIdPresupuesto')
        ALTER TABLE [dbo].[PresupuestosDetalles] DROP CONSTRAINT [FK_PresupuestosDetalles_Presupuestos_PresupuestoIdPresupuesto];
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PresupuestosDetalles_Productos_ProductoIdProducto')
        ALTER TABLE [dbo].[PresupuestosDetalles] DROP CONSTRAINT [FK_PresupuestosDetalles_Productos_ProductoIdProducto];
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Ventas_Cajas_CajaIdCaja')
        ALTER TABLE [dbo].[Ventas] DROP CONSTRAINT [FK_Ventas_Cajas_CajaIdCaja];
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Ventas_Clientes_ClienteIdCliente')
        ALTER TABLE [dbo].[Ventas] DROP CONSTRAINT [FK_Ventas_Clientes_ClienteIdCliente];
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Ventas_Monedas_MonedaIdMoneda')
        ALTER TABLE [dbo].[Ventas] DROP CONSTRAINT [FK_Ventas_Monedas_MonedaIdMoneda];
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Ventas_Sucursal_SucursalId')
        ALTER TABLE [dbo].[Ventas] DROP CONSTRAINT [FK_Ventas_Sucursal_SucursalId];
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Ventas_TiposDocumentoOperacion_TipoDocumentoOperacionIdTipoDocumentoOperacion')
        ALTER TABLE [dbo].[Ventas] DROP CONSTRAINT [FK_Ventas_TiposDocumentoOperacion_TipoDocumentoOperacionIdTipoDocumentoOperacion];
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Ventas_TiposPago_TipoPagoIdTipoPago')
        ALTER TABLE [dbo].[Ventas] DROP CONSTRAINT [FK_Ventas_TiposPago_TipoPagoIdTipoPago];
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Ventas_Usuarios_UsuarioId_Usu')
        ALTER TABLE [dbo].[Ventas] DROP CONSTRAINT [FK_Ventas_Usuarios_UsuarioId_Usu];
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_VentasDetalles_Productos_ProductoIdProducto')
        ALTER TABLE [dbo].[VentasDetalles] DROP CONSTRAINT [FK_VentasDetalles_Productos_ProductoIdProducto];
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_VentasDetalles_Ventas_VentaIdVenta')
        ALTER TABLE [dbo].[VentasDetalles] DROP CONSTRAINT [FK_VentasDetalles_Ventas_VentaIdVenta];
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_VentasDetalles_ProductoIdProducto' AND object_id = OBJECT_ID(N'[dbo].[VentasDetalles]'))
        DROP INDEX [IX_VentasDetalles_ProductoIdProducto] ON [dbo].[VentasDetalles];
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_VentasDetalles_VentaIdVenta' AND object_id = OBJECT_ID(N'[dbo].[VentasDetalles]'))
        DROP INDEX [IX_VentasDetalles_VentaIdVenta] ON [dbo].[VentasDetalles];
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Ventas_CajaIdCaja' AND object_id = OBJECT_ID(N'[dbo].[Ventas]'))
        DROP INDEX [IX_Ventas_CajaIdCaja] ON [dbo].[Ventas];
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Ventas_ClienteIdCliente' AND object_id = OBJECT_ID(N'[dbo].[Ventas]'))
        DROP INDEX [IX_Ventas_ClienteIdCliente] ON [dbo].[Ventas];
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Ventas_MonedaIdMoneda' AND object_id = OBJECT_ID(N'[dbo].[Ventas]'))
        DROP INDEX [IX_Ventas_MonedaIdMoneda] ON [dbo].[Ventas];
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Ventas_SucursalId' AND object_id = OBJECT_ID(N'[dbo].[Ventas]'))
        DROP INDEX [IX_Ventas_SucursalId] ON [dbo].[Ventas];
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Ventas_TipoDocumentoOperacionIdTipoDocumentoOperacion' AND object_id = OBJECT_ID(N'[dbo].[Ventas]'))
        DROP INDEX [IX_Ventas_TipoDocumentoOperacionIdTipoDocumentoOperacion] ON [dbo].[Ventas];
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Ventas_TipoPagoIdTipoPago' AND object_id = OBJECT_ID(N'[dbo].[Ventas]'))
        DROP INDEX [IX_Ventas_TipoPagoIdTipoPago] ON [dbo].[Ventas];
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Ventas_UsuarioId_Usu' AND object_id = OBJECT_ID(N'[dbo].[Ventas]'))
        DROP INDEX [IX_Ventas_UsuarioId_Usu] ON [dbo].[Ventas];
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PresupuestosDetalles_PresupuestoIdPresupuesto' AND object_id = OBJECT_ID(N'[dbo].[PresupuestosDetalles]'))
        DROP INDEX [IX_PresupuestosDetalles_PresupuestoIdPresupuesto] ON [dbo].[PresupuestosDetalles];
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PresupuestosDetalles_ProductoIdProducto' AND object_id = OBJECT_ID(N'[dbo].[PresupuestosDetalles]'))
        DROP INDEX [IX_PresupuestosDetalles_ProductoIdProducto] ON [dbo].[PresupuestosDetalles];
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Presupuestos_ClienteIdCliente' AND object_id = OBJECT_ID(N'[dbo].[Presupuestos]'))
        DROP INDEX [IX_Presupuestos_ClienteIdCliente] ON [dbo].[Presupuestos];
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Presupuestos_MonedaIdMoneda' AND object_id = OBJECT_ID(N'[dbo].[Presupuestos]'))
        DROP INDEX [IX_Presupuestos_MonedaIdMoneda] ON [dbo].[Presupuestos];
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Presupuestos_SucursalId' AND object_id = OBJECT_ID(N'[dbo].[Presupuestos]'))
        DROP INDEX [IX_Presupuestos_SucursalId] ON [dbo].[Presupuestos];
    IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'ProductoIdProducto' AND Object_ID = Object_ID(N'[dbo].[VentasDetalles]'))
        ALTER TABLE [dbo].[VentasDetalles] DROP COLUMN [ProductoIdProducto];
    IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'VentaIdVenta' AND Object_ID = Object_ID(N'[dbo].[VentasDetalles]'))
        ALTER TABLE [dbo].[VentasDetalles] DROP COLUMN [VentaIdVenta];
    IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'CajaIdCaja' AND Object_ID = Object_ID(N'[dbo].[Ventas]'))
        ALTER TABLE [dbo].[Ventas] DROP COLUMN [CajaIdCaja];
    IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'ClienteIdCliente' AND Object_ID = Object_ID(N'[dbo].[Ventas]'))
        ALTER TABLE [dbo].[Ventas] DROP COLUMN [ClienteIdCliente];
    IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'MonedaIdMoneda' AND Object_ID = Object_ID(N'[dbo].[Ventas]'))
        ALTER TABLE [dbo].[Ventas] DROP COLUMN [MonedaIdMoneda];
    IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'SucursalId' AND Object_ID = Object_ID(N'[dbo].[Ventas]'))
        ALTER TABLE [dbo].[Ventas] DROP COLUMN [SucursalId];
    IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'TipoDocumentoOperacionIdTipoDocumentoOperacion' AND Object_ID = Object_ID(N'[dbo].[Ventas]'))
        ALTER TABLE [dbo].[Ventas] DROP COLUMN [TipoDocumentoOperacionIdTipoDocumentoOperacion];
    IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'TipoPagoIdTipoPago' AND Object_ID = Object_ID(N'[dbo].[Ventas]'))
        ALTER TABLE [dbo].[Ventas] DROP COLUMN [TipoPagoIdTipoPago];
    IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'UsuarioId_Usu' AND Object_ID = Object_ID(N'[dbo].[Ventas]'))
        ALTER TABLE [dbo].[Ventas] DROP COLUMN [UsuarioId_Usu];
    IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'PresupuestoIdPresupuesto' AND Object_ID = Object_ID(N'[dbo].[PresupuestosDetalles]'))
        ALTER TABLE [dbo].[PresupuestosDetalles] DROP COLUMN [PresupuestoIdPresupuesto];
    IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'ProductoIdProducto' AND Object_ID = Object_ID(N'[dbo].[PresupuestosDetalles]'))
        ALTER TABLE [dbo].[PresupuestosDetalles] DROP COLUMN [ProductoIdProducto];
    IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'ClienteIdCliente' AND Object_ID = Object_ID(N'[dbo].[Presupuestos]'))
        ALTER TABLE [dbo].[Presupuestos] DROP COLUMN [ClienteIdCliente];
    IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'MonedaIdMoneda' AND Object_ID = Object_ID(N'[dbo].[Presupuestos]'))
        ALTER TABLE [dbo].[Presupuestos] DROP COLUMN [MonedaIdMoneda];
    IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'SucursalId' AND Object_ID = Object_ID(N'[dbo].[Presupuestos]'))
        ALTER TABLE [dbo].[Presupuestos] DROP COLUMN [SucursalId];
    -- Renombrar IdSucursal -> suc solo si corresponde
    IF COL_LENGTH('dbo.Ventas','suc') IS NULL AND COL_LENGTH('dbo.Ventas','IdSucursal') IS NOT NULL
        EXEC sp_rename 'dbo.Ventas.IdSucursal','suc','COLUMN';
    IF COL_LENGTH('dbo.Presupuestos','suc') IS NULL AND COL_LENGTH('dbo.Presupuestos','IdSucursal') IS NOT NULL
        EXEC sp_rename 'dbo.Presupuestos.IdSucursal','suc','COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250910172515_Fix_ClientePrecio_FKs'
)
BEGIN
    DECLARE @var7 sysname;
    SELECT @var7 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Ventas]') AND [c].[name] = N'PuntoExpedicion');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Ventas] DROP CONSTRAINT [' + @var7 + '];');
    ALTER TABLE [Ventas] ALTER COLUMN [PuntoExpedicion] nchar(3) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250910172515_Fix_ClientePrecio_FKs'
)
BEGIN
    DECLARE @var8 sysname;
    SELECT @var8 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Ventas]') AND [c].[name] = N'Establecimiento');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [Ventas] DROP CONSTRAINT [' + @var8 + '];');
    ALTER TABLE [Ventas] ALTER COLUMN [Establecimiento] nchar(3) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250910172515_Fix_ClientePrecio_FKs'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_VentasDetalles_IdProducto' AND object_id = OBJECT_ID(N'[dbo].[VentasDetalles]'))
        CREATE INDEX [IX_VentasDetalles_IdProducto] ON [dbo].[VentasDetalles]([IdProducto]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_VentasDetalles_IdVenta' AND object_id = OBJECT_ID(N'[dbo].[VentasDetalles]'))
        CREATE INDEX [IX_VentasDetalles_IdVenta] ON [dbo].[VentasDetalles]([IdVenta]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Ventas_IdCaja' AND object_id = OBJECT_ID(N'[dbo].[Ventas]'))
        CREATE INDEX [IX_Ventas_IdCaja] ON [dbo].[Ventas]([IdCaja]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Ventas_IdCliente' AND object_id = OBJECT_ID(N'[dbo].[Ventas]'))
        CREATE INDEX [IX_Ventas_IdCliente] ON [dbo].[Ventas]([IdCliente]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Ventas_IdMoneda' AND object_id = OBJECT_ID(N'[dbo].[Ventas]'))
        CREATE INDEX [IX_Ventas_IdMoneda] ON [dbo].[Ventas]([IdMoneda]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Ventas_IdTipoDocumentoOperacion' AND object_id = OBJECT_ID(N'[dbo].[Ventas]'))
        CREATE INDEX [IX_Ventas_IdTipoDocumentoOperacion] ON [dbo].[Ventas]([IdTipoDocumentoOperacion]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Ventas_IdTipoPago' AND object_id = OBJECT_ID(N'[dbo].[Ventas]'))
        CREATE INDEX [IX_Ventas_IdTipoPago] ON [dbo].[Ventas]([IdTipoPago]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Ventas_IdUsuario' AND object_id = OBJECT_ID(N'[dbo].[Ventas]'))
        CREATE INDEX [IX_Ventas_IdUsuario] ON [dbo].[Ventas]([IdUsuario]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Ventas_suc' AND object_id = OBJECT_ID(N'[dbo].[Ventas]'))
        CREATE INDEX [IX_Ventas_suc] ON [dbo].[Ventas]([suc]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PresupuestosDetalles_IdPresupuesto' AND object_id = OBJECT_ID(N'[dbo].[PresupuestosDetalles]'))
        CREATE INDEX [IX_PresupuestosDetalles_IdPresupuesto] ON [dbo].[PresupuestosDetalles]([IdPresupuesto]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PresupuestosDetalles_IdProducto' AND object_id = OBJECT_ID(N'[dbo].[PresupuestosDetalles]'))
        CREATE INDEX [IX_PresupuestosDetalles_IdProducto] ON [dbo].[PresupuestosDetalles]([IdProducto]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Presupuestos_IdCliente' AND object_id = OBJECT_ID(N'[dbo].[Presupuestos]'))
        CREATE INDEX [IX_Presupuestos_IdCliente] ON [dbo].[Presupuestos]([IdCliente]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Presupuestos_IdMoneda' AND object_id = OBJECT_ID(N'[dbo].[Presupuestos]'))
        CREATE INDEX [IX_Presupuestos_IdMoneda] ON [dbo].[Presupuestos]([IdMoneda]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Presupuestos_suc_NumeroPresupuesto' AND object_id = OBJECT_ID(N'[dbo].[Presupuestos]'))
        CREATE INDEX [IX_Presupuestos_suc_NumeroPresupuesto] ON [dbo].[Presupuestos]([suc],[NumeroPresupuesto]);
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Presupuestos_Clientes_IdCliente')
    BEGIN
        ALTER TABLE [dbo].[Presupuestos]  WITH CHECK ADD  CONSTRAINT [FK_Presupuestos_Clientes_IdCliente] FOREIGN KEY([IdCliente]) REFERENCES [dbo].[Clientes]([IdCliente]) ON DELETE NO ACTION;
        ALTER TABLE [dbo].[Presupuestos] CHECK CONSTRAINT [FK_Presupuestos_Clientes_IdCliente];
    END
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Presupuestos_Monedas_IdMoneda')
    BEGIN
        ALTER TABLE [dbo].[Presupuestos]  WITH CHECK ADD  CONSTRAINT [FK_Presupuestos_Monedas_IdMoneda] FOREIGN KEY([IdMoneda]) REFERENCES [dbo].[Monedas]([IdMoneda]) ON DELETE NO ACTION;
        ALTER TABLE [dbo].[Presupuestos] CHECK CONSTRAINT [FK_Presupuestos_Monedas_IdMoneda];
    END
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Presupuestos_Sucursal_suc')
    BEGIN
        ALTER TABLE [dbo].[Presupuestos]  WITH CHECK ADD  CONSTRAINT [FK_Presupuestos_Sucursal_suc] FOREIGN KEY([suc]) REFERENCES [dbo].[Sucursal]([Id]) ON DELETE NO ACTION;
        ALTER TABLE [dbo].[Presupuestos] CHECK CONSTRAINT [FK_Presupuestos_Sucursal_suc];
    END
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PresupuestosDetalles_Presupuestos_IdPresupuesto')
    BEGIN
        ALTER TABLE [dbo].[PresupuestosDetalles]  WITH CHECK ADD  CONSTRAINT [FK_PresupuestosDetalles_Presupuestos_IdPresupuesto] FOREIGN KEY([IdPresupuesto]) REFERENCES [dbo].[Presupuestos]([IdPresupuesto]) ON DELETE CASCADE;
        ALTER TABLE [dbo].[PresupuestosDetalles] CHECK CONSTRAINT [FK_PresupuestosDetalles_Presupuestos_IdPresupuesto];
    END
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PresupuestosDetalles_Productos_IdProducto')
    BEGIN
        ALTER TABLE [dbo].[PresupuestosDetalles]  WITH CHECK ADD  CONSTRAINT [FK_PresupuestosDetalles_Productos_IdProducto] FOREIGN KEY([IdProducto]) REFERENCES [dbo].[Productos]([IdProducto]) ON DELETE NO ACTION;
        ALTER TABLE [dbo].[PresupuestosDetalles] CHECK CONSTRAINT [FK_PresupuestosDetalles_Productos_IdProducto];
    END
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Ventas_Cajas_IdCaja')
    BEGIN
        ALTER TABLE [dbo].[Ventas]  WITH CHECK ADD  CONSTRAINT [FK_Ventas_Cajas_IdCaja] FOREIGN KEY([IdCaja]) REFERENCES [dbo].[Cajas]([id_caja]) ON DELETE NO ACTION;
        ALTER TABLE [dbo].[Ventas] CHECK CONSTRAINT [FK_Ventas_Cajas_IdCaja];
    END
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Ventas_Clientes_IdCliente')
    BEGIN
        ALTER TABLE [dbo].[Ventas]  WITH CHECK ADD  CONSTRAINT [FK_Ventas_Clientes_IdCliente] FOREIGN KEY([IdCliente]) REFERENCES [dbo].[Clientes]([IdCliente]) ON DELETE NO ACTION;
        ALTER TABLE [dbo].[Ventas] CHECK CONSTRAINT [FK_Ventas_Clientes_IdCliente];
    END
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Ventas_Monedas_IdMoneda')
    BEGIN
        ALTER TABLE [dbo].[Ventas]  WITH CHECK ADD  CONSTRAINT [FK_Ventas_Monedas_IdMoneda] FOREIGN KEY([IdMoneda]) REFERENCES [dbo].[Monedas]([IdMoneda]) ON DELETE NO ACTION;
        ALTER TABLE [dbo].[Ventas] CHECK CONSTRAINT [FK_Ventas_Monedas_IdMoneda];
    END
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Ventas_Sucursal_suc')
    BEGIN
        ALTER TABLE [dbo].[Ventas]  WITH CHECK ADD  CONSTRAINT [FK_Ventas_Sucursal_suc] FOREIGN KEY([suc]) REFERENCES [dbo].[Sucursal]([Id]) ON DELETE NO ACTION;
        ALTER TABLE [dbo].[Ventas] CHECK CONSTRAINT [FK_Ventas_Sucursal_suc];
    END
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Ventas_TiposDocumentoOperacion_IdTipoDocumentoOperacion')
    BEGIN
        ALTER TABLE [dbo].[Ventas]  WITH CHECK ADD  CONSTRAINT [FK_Ventas_TiposDocumentoOperacion_IdTipoDocumentoOperacion] FOREIGN KEY([IdTipoDocumentoOperacion]) REFERENCES [dbo].[TiposDocumentoOperacion]([IdTipoDocumentoOperacion]) ON DELETE NO ACTION;
        ALTER TABLE [dbo].[Ventas] CHECK CONSTRAINT [FK_Ventas_TiposDocumentoOperacion_IdTipoDocumentoOperacion];
    END
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Ventas_TiposPago_IdTipoPago')
    BEGIN
        ALTER TABLE [dbo].[Ventas]  WITH CHECK ADD  CONSTRAINT [FK_Ventas_TiposPago_IdTipoPago] FOREIGN KEY([IdTipoPago]) REFERENCES [dbo].[TiposPago]([IdTipoPago]) ON DELETE NO ACTION;
        ALTER TABLE [dbo].[Ventas] CHECK CONSTRAINT [FK_Ventas_TiposPago_IdTipoPago];
    END
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Ventas_Usuarios_IdUsuario')
    BEGIN
        ALTER TABLE [dbo].[Ventas]  WITH CHECK ADD  CONSTRAINT [FK_Ventas_Usuarios_IdUsuario] FOREIGN KEY([IdUsuario]) REFERENCES [dbo].[Usuarios]([Id_Usu]) ON DELETE NO ACTION;
        ALTER TABLE [dbo].[Ventas] CHECK CONSTRAINT [FK_Ventas_Usuarios_IdUsuario];
    END
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_VentasDetalles_Productos_IdProducto')
    BEGIN
        ALTER TABLE [dbo].[VentasDetalles]  WITH CHECK ADD  CONSTRAINT [FK_VentasDetalles_Productos_IdProducto] FOREIGN KEY([IdProducto]) REFERENCES [dbo].[Productos]([IdProducto]) ON DELETE NO ACTION;
        ALTER TABLE [dbo].[VentasDetalles] CHECK CONSTRAINT [FK_VentasDetalles_Productos_IdProducto];
    END
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_VentasDetalles_Ventas_IdVenta')
    BEGIN
        ALTER TABLE [dbo].[VentasDetalles]  WITH CHECK ADD  CONSTRAINT [FK_VentasDetalles_Ventas_IdVenta] FOREIGN KEY([IdVenta]) REFERENCES [dbo].[Ventas]([IdVenta]) ON DELETE CASCADE;
        ALTER TABLE [dbo].[VentasDetalles] CHECK CONSTRAINT [FK_VentasDetalles_Ventas_IdVenta];
    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250910172515_Fix_ClientePrecio_FKs'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250910172515_Fix_ClientePrecio_FKs', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250911134736_Agregar_IdLote_en_Ventas'
)
BEGIN
    IF COL_LENGTH('dbo.Ventas','IdLote') IS NULL
    ALTER TABLE [dbo].[Ventas] ADD [IdLote] NVARCHAR(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250911134736_Agregar_IdLote_en_Ventas'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250911134736_Agregar_IdLote_en_Ventas', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250911142527_Agregar_Index_IdLote'
)
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Ventas]') AND name = N'IX_Ventas_IdLote')
    CREATE INDEX [IX_Ventas_IdLote] ON [dbo].[Ventas] ([IdLote]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250911142527_Agregar_Index_IdLote'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250911142527_Agregar_Index_IdLote', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250918180701_OptimizarConsultasFactura'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_Cajas_CajaActual] ON [Cajas] ([CajaActual]) WHERE [CajaActual] = 1');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250918180701_OptimizarConsultasFactura'
)
BEGIN
    CREATE INDEX [IX_VentasDetalles_IdVenta_IdProducto] ON [VentasDetalles] ([IdVenta], [IdProducto]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250918180701_OptimizarConsultasFactura'
)
BEGIN
    CREATE INDEX [IX_Ventas_Fecha] ON [Ventas] ([Fecha]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250918180701_OptimizarConsultasFactura'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_Productos_Activo] ON [Productos] ([Activo]) WHERE [Activo] = 1');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250918180701_OptimizarConsultasFactura'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250918180701_OptimizarConsultasFactura', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250922151240_Agregar_Actividad_Economica_Sociedad'
)
BEGIN
    CREATE TABLE [SociedadesActividades] (
        [Numero] int NOT NULL IDENTITY,
        [IdSociedad] int NOT NULL,
        [CodigoActividad] varchar(50) NOT NULL,
        [NombreActividad] varchar(300) NULL,
        [ActividadPrincipal] char(1) NULL,
        CONSTRAINT [PK_SociedadesActividades] PRIMARY KEY ([Numero]),
        CONSTRAINT [FK_SociedadesActividades_Sociedades_IdSociedad] FOREIGN KEY ([IdSociedad]) REFERENCES [Sociedades] ([IdSociedad]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250922151240_Agregar_Actividad_Economica_Sociedad'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SociedadesActividades_IdSociedad_CodigoActividad] ON [SociedadesActividades] ([IdSociedad], [CodigoActividad]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250922151240_Agregar_Actividad_Economica_Sociedad'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250922151240_Agregar_Actividad_Economica_Sociedad', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251018200931_Agregar_CodigoSeguridad_Ventas'
)
BEGIN
    ALTER TABLE [Ventas] ADD [CodigoSeguridad] nvarchar(9) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251018200931_Agregar_CodigoSeguridad_Ventas'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251018200931_Agregar_CodigoSeguridad_Ventas', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251203162407_Agregar_Deposito_Vencimiento_CompraDetalle'
)
BEGIN
    ALTER TABLE [ComprasDetalles] ADD [FechaVencimientoItem] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251203162407_Agregar_Deposito_Vencimiento_CompraDetalle'
)
BEGIN
    ALTER TABLE [ComprasDetalles] ADD [IdDepositoItem] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251203162407_Agregar_Deposito_Vencimiento_CompraDetalle'
)
BEGIN
    CREATE INDEX [IX_ComprasDetalles_IdDepositoItem] ON [ComprasDetalles] ([IdDepositoItem]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251203162407_Agregar_Deposito_Vencimiento_CompraDetalle'
)
BEGIN
    ALTER TABLE [ComprasDetalles] ADD CONSTRAINT [FK_ComprasDetalles_Depositos_IdDepositoItem] FOREIGN KEY ([IdDepositoItem]) REFERENCES [Depositos] ([IdDeposito]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251203162407_Agregar_Deposito_Vencimiento_CompraDetalle'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251203162407_Agregar_Deposito_Vencimiento_CompraDetalle', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251209145958_CrearTablasTransferenciasDepositos'
)
BEGIN
    CREATE TABLE [TransferenciasDeposito] (
        [IdTransferencia] int NOT NULL IDENTITY,
        [IdDepositoOrigen] int NOT NULL,
        [IdDepositoDestino] int NOT NULL,
        [FechaTransferencia] datetime2 NOT NULL,
        [Comentario] nvarchar(500) NULL,
        [UsuarioCreacion] nvarchar(100) NULL,
        CONSTRAINT [PK_TransferenciasDeposito] PRIMARY KEY ([IdTransferencia]),
        CONSTRAINT [FK_TransferenciasDeposito_Depositos_IdDepositoDestino] FOREIGN KEY ([IdDepositoDestino]) REFERENCES [Depositos] ([IdDeposito]),
        CONSTRAINT [FK_TransferenciasDeposito_Depositos_IdDepositoOrigen] FOREIGN KEY ([IdDepositoOrigen]) REFERENCES [Depositos] ([IdDeposito])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251209145958_CrearTablasTransferenciasDepositos'
)
BEGIN
    CREATE TABLE [TransferenciasDepositoDetalle] (
        [IdTransferenciaDetalle] int NOT NULL IDENTITY,
        [IdTransferencia] int NOT NULL,
        [IdProducto] int NOT NULL,
        [Cantidad] decimal(18,4) NOT NULL,
        [CostoUnitario] decimal(18,2) NULL,
        CONSTRAINT [PK_TransferenciasDepositoDetalle] PRIMARY KEY ([IdTransferenciaDetalle]),
        CONSTRAINT [FK_TransferenciasDepositoDetalle_Productos_IdProducto] FOREIGN KEY ([IdProducto]) REFERENCES [Productos] ([IdProducto]) ON DELETE CASCADE,
        CONSTRAINT [FK_TransferenciasDepositoDetalle_TransferenciasDeposito_IdTransferencia] FOREIGN KEY ([IdTransferencia]) REFERENCES [TransferenciasDeposito] ([IdTransferencia]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251209145958_CrearTablasTransferenciasDepositos'
)
BEGIN
    CREATE INDEX [IX_TransferenciasDeposito_IdDepositoDestino] ON [TransferenciasDeposito] ([IdDepositoDestino]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251209145958_CrearTablasTransferenciasDepositos'
)
BEGIN
    CREATE INDEX [IX_TransferenciasDeposito_IdDepositoOrigen] ON [TransferenciasDeposito] ([IdDepositoOrigen]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251209145958_CrearTablasTransferenciasDepositos'
)
BEGIN
    CREATE INDEX [IX_TransferenciasDepositoDetalle_IdProducto] ON [TransferenciasDepositoDetalle] ([IdProducto]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251209145958_CrearTablasTransferenciasDepositos'
)
BEGIN
    CREATE INDEX [IX_TransferenciasDepositoDetalle_IdTransferencia] ON [TransferenciasDepositoDetalle] ([IdTransferencia]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251209145958_CrearTablasTransferenciasDepositos'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251209145958_CrearTablasTransferenciasDepositos', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210002129_Agregar_IdPresupuestoOrigen_en_Ventas'
)
BEGIN
    ALTER TABLE [Ventas] ADD [IdPresupuestoOrigen] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210002129_Agregar_IdPresupuestoOrigen_en_Ventas'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251210002129_Agregar_IdPresupuestoOrigen_en_Ventas', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210005308_Agregar_NroPedido_y_TipoFacturacion'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251210005308_Agregar_NroPedido_y_TipoFacturacion', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210005929_Agregar_Columnas_NroPedido_TipoFacturacion'
)
BEGIN
    ALTER TABLE [Ventas] ADD [NroPedido] nvarchar(20) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210005929_Agregar_Columnas_NroPedido_TipoFacturacion'
)
BEGIN
    ALTER TABLE [Cajas] ADD [TipoFacturacion] nvarchar(20) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210005929_Agregar_Columnas_NroPedido_TipoFacturacion'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251210005929_Agregar_Columnas_NroPedido_TipoFacturacion', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210124949_AgregarSistemaCreditosRemisiones'
)
BEGIN
    CREATE TABLE [CuentasPorCobrar] (
        [IdCuentaPorCobrar] int NOT NULL IDENTITY,
        [IdVenta] int NOT NULL,
        [IdCliente] int NOT NULL,
        [IdSucursal] int NOT NULL,
        [FechaCredito] datetime2 NOT NULL,
        [FechaVencimiento] datetime2 NULL,
        [MontoTotal] decimal(18,4) NOT NULL,
        [SaldoPendiente] decimal(18,4) NOT NULL,
        [NumeroCuotas] int NOT NULL,
        [PlazoDias] int NOT NULL,
        [Estado] nvarchar(20) NOT NULL,
        [IdMoneda] int NULL,
        [MonedaIdMoneda] int NULL,
        [Observaciones] nvarchar(280) NULL,
        [IdUsuarioAutorizo] int NULL,
        [UsuarioAutorizoId_Usu] int NULL,
        CONSTRAINT [PK_CuentasPorCobrar] PRIMARY KEY ([IdCuentaPorCobrar]),
        CONSTRAINT [FK_CuentasPorCobrar_Clientes_IdCliente] FOREIGN KEY ([IdCliente]) REFERENCES [Clientes] ([IdCliente]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CuentasPorCobrar_Monedas_MonedaIdMoneda] FOREIGN KEY ([MonedaIdMoneda]) REFERENCES [Monedas] ([IdMoneda]),
        CONSTRAINT [FK_CuentasPorCobrar_Sucursal_IdSucursal] FOREIGN KEY ([IdSucursal]) REFERENCES [Sucursal] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CuentasPorCobrar_Usuarios_UsuarioAutorizoId_Usu] FOREIGN KEY ([UsuarioAutorizoId_Usu]) REFERENCES [Usuarios] ([Id_Usu]),
        CONSTRAINT [FK_CuentasPorCobrar_Ventas_IdVenta] FOREIGN KEY ([IdVenta]) REFERENCES [Ventas] ([IdVenta]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210124949_AgregarSistemaCreditosRemisiones'
)
BEGIN
    CREATE TABLE [RemisionesInternas] (
        [IdRemision] int NOT NULL IDENTITY,
        [IdVenta] int NOT NULL,
        [IdCliente] int NOT NULL,
        [IdSucursal] int NOT NULL,
        [SucursalId] int NULL,
        [FechaRemision] datetime2 NOT NULL,
        [NumeroRemision] nvarchar(20) NOT NULL,
        [MontoTotal] decimal(18,4) NOT NULL,
        [Estado] nvarchar(20) NOT NULL,
        [IdVentaFactura] int NULL,
        [VentaFacturaIdVenta] int NULL,
        [FechaFacturacion] datetime2 NULL,
        [IdMoneda] int NULL,
        [MonedaIdMoneda] int NULL,
        [Observaciones] nvarchar(280) NULL,
        [IdUsuarioEmitio] int NULL,
        [UsuarioEmitioId_Usu] int NULL,
        CONSTRAINT [PK_RemisionesInternas] PRIMARY KEY ([IdRemision]),
        CONSTRAINT [FK_RemisionesInternas_Clientes_IdCliente] FOREIGN KEY ([IdCliente]) REFERENCES [Clientes] ([IdCliente]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RemisionesInternas_Monedas_MonedaIdMoneda] FOREIGN KEY ([MonedaIdMoneda]) REFERENCES [Monedas] ([IdMoneda]),
        CONSTRAINT [FK_RemisionesInternas_Sucursal_SucursalId] FOREIGN KEY ([SucursalId]) REFERENCES [Sucursal] ([Id]),
        CONSTRAINT [FK_RemisionesInternas_Usuarios_UsuarioEmitioId_Usu] FOREIGN KEY ([UsuarioEmitioId_Usu]) REFERENCES [Usuarios] ([Id_Usu]),
        CONSTRAINT [FK_RemisionesInternas_Ventas_IdVenta] FOREIGN KEY ([IdVenta]) REFERENCES [Ventas] ([IdVenta]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RemisionesInternas_Ventas_VentaFacturaIdVenta] FOREIGN KEY ([VentaFacturaIdVenta]) REFERENCES [Ventas] ([IdVenta])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210124949_AgregarSistemaCreditosRemisiones'
)
BEGIN
    CREATE TABLE [CobrosCuotas] (
        [IdCobro] int NOT NULL IDENTITY,
        [IdCuentaPorCobrar] int NOT NULL,
        [IdCliente] int NOT NULL,
        [FechaCobro] datetime2 NOT NULL,
        [MontoTotal] decimal(18,4) NOT NULL,
        [IdMoneda] int NULL,
        [MonedaIdMoneda] int NULL,
        [Estado] nvarchar(20) NOT NULL,
        [Observaciones] nvarchar(280) NULL,
        [IdUsuario] int NULL,
        [IdCaja] int NULL,
        [NumeroRecibo] nvarchar(30) NULL,
        CONSTRAINT [PK_CobrosCuotas] PRIMARY KEY ([IdCobro]),
        CONSTRAINT [FK_CobrosCuotas_Cajas_IdCaja] FOREIGN KEY ([IdCaja]) REFERENCES [Cajas] ([id_caja]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CobrosCuotas_Clientes_IdCliente] FOREIGN KEY ([IdCliente]) REFERENCES [Clientes] ([IdCliente]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CobrosCuotas_CuentasPorCobrar_IdCuentaPorCobrar] FOREIGN KEY ([IdCuentaPorCobrar]) REFERENCES [CuentasPorCobrar] ([IdCuentaPorCobrar]) ON DELETE CASCADE,
        CONSTRAINT [FK_CobrosCuotas_Monedas_MonedaIdMoneda] FOREIGN KEY ([MonedaIdMoneda]) REFERENCES [Monedas] ([IdMoneda]),
        CONSTRAINT [FK_CobrosCuotas_Usuarios_IdUsuario] FOREIGN KEY ([IdUsuario]) REFERENCES [Usuarios] ([Id_Usu]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210124949_AgregarSistemaCreditosRemisiones'
)
BEGIN
    CREATE TABLE [CuentasPorCobrarCuotas] (
        [IdCuota] int NOT NULL IDENTITY,
        [IdCuentaPorCobrar] int NOT NULL,
        [NumeroCuota] int NOT NULL,
        [MontoCuota] decimal(18,4) NOT NULL,
        [SaldoCuota] decimal(18,4) NOT NULL,
        [FechaVencimiento] datetime2 NOT NULL,
        [FechaPago] datetime2 NULL,
        [Estado] nvarchar(20) NOT NULL,
        [Observaciones] nvarchar(280) NULL,
        CONSTRAINT [PK_CuentasPorCobrarCuotas] PRIMARY KEY ([IdCuota]),
        CONSTRAINT [FK_CuentasPorCobrarCuotas_CuentasPorCobrar_IdCuentaPorCobrar] FOREIGN KEY ([IdCuentaPorCobrar]) REFERENCES [CuentasPorCobrar] ([IdCuentaPorCobrar]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210124949_AgregarSistemaCreditosRemisiones'
)
BEGIN
    CREATE TABLE [RemisionesInternasDetalles] (
        [IdRemisionDetalle] int NOT NULL IDENTITY,
        [IdRemision] int NOT NULL,
        [IdProducto] int NOT NULL,
        [Cantidad] decimal(18,4) NOT NULL,
        [PrecioUnitario] decimal(18,4) NOT NULL,
        [Subtotal] decimal(18,4) NOT NULL,
        [Gravado5] decimal(18,4) NOT NULL,
        [Gravado10] decimal(18,4) NOT NULL,
        [Exenta] decimal(18,4) NOT NULL,
        [IVA5] decimal(18,4) NOT NULL,
        [IVA10] decimal(18,4) NOT NULL,
        [Observaciones] nvarchar(280) NULL,
        CONSTRAINT [PK_RemisionesInternasDetalles] PRIMARY KEY ([IdRemisionDetalle]),
        CONSTRAINT [FK_RemisionesInternasDetalles_Productos_IdProducto] FOREIGN KEY ([IdProducto]) REFERENCES [Productos] ([IdProducto]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RemisionesInternasDetalles_RemisionesInternas_IdRemision] FOREIGN KEY ([IdRemision]) REFERENCES [RemisionesInternas] ([IdRemision]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210124949_AgregarSistemaCreditosRemisiones'
)
BEGIN
    CREATE TABLE [CobrosDetalles] (
        [IdCobroDetalle] int NOT NULL IDENTITY,
        [IdCobro] int NOT NULL,
        [IdCuota] int NULL,
        [MedioPago] nvarchar(20) NOT NULL,
        [Monto] decimal(18,4) NOT NULL,
        [IdMoneda] int NULL,
        [MonedaIdMoneda] int NULL,
        [BancoTarjeta] nvarchar(100) NULL,
        [Ultimos4Tarjeta] nvarchar(4) NULL,
        [NumeroAutorizacion] nvarchar(50) NULL,
        [NumeroCheque] nvarchar(30) NULL,
        [BancoCheque] nvarchar(100) NULL,
        [NumeroTransferencia] nvarchar(100) NULL,
        [Observaciones] nvarchar(280) NULL,
        CONSTRAINT [PK_CobrosDetalles] PRIMARY KEY ([IdCobroDetalle]),
        CONSTRAINT [FK_CobrosDetalles_CobrosCuotas_IdCobro] FOREIGN KEY ([IdCobro]) REFERENCES [CobrosCuotas] ([IdCobro]) ON DELETE CASCADE,
        CONSTRAINT [FK_CobrosDetalles_CuentasPorCobrarCuotas_IdCuota] FOREIGN KEY ([IdCuota]) REFERENCES [CuentasPorCobrarCuotas] ([IdCuota]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CobrosDetalles_Monedas_MonedaIdMoneda] FOREIGN KEY ([MonedaIdMoneda]) REFERENCES [Monedas] ([IdMoneda])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210124949_AgregarSistemaCreditosRemisiones'
)
BEGIN
    CREATE INDEX [IX_CobrosCuotas_IdCaja] ON [CobrosCuotas] ([IdCaja]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210124949_AgregarSistemaCreditosRemisiones'
)
BEGIN
    CREATE INDEX [IX_CobrosCuotas_IdCliente] ON [CobrosCuotas] ([IdCliente]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210124949_AgregarSistemaCreditosRemisiones'
)
BEGIN
    CREATE INDEX [IX_CobrosCuotas_IdCuentaPorCobrar] ON [CobrosCuotas] ([IdCuentaPorCobrar]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210124949_AgregarSistemaCreditosRemisiones'
)
BEGIN
    CREATE INDEX [IX_CobrosCuotas_IdUsuario] ON [CobrosCuotas] ([IdUsuario]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210124949_AgregarSistemaCreditosRemisiones'
)
BEGIN
    CREATE INDEX [IX_CobrosCuotas_MonedaIdMoneda] ON [CobrosCuotas] ([MonedaIdMoneda]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210124949_AgregarSistemaCreditosRemisiones'
)
BEGIN
    CREATE INDEX [IX_CobrosDetalles_IdCobro] ON [CobrosDetalles] ([IdCobro]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210124949_AgregarSistemaCreditosRemisiones'
)
BEGIN
    CREATE INDEX [IX_CobrosDetalles_IdCuota] ON [CobrosDetalles] ([IdCuota]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210124949_AgregarSistemaCreditosRemisiones'
)
BEGIN
    CREATE INDEX [IX_CobrosDetalles_MonedaIdMoneda] ON [CobrosDetalles] ([MonedaIdMoneda]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210124949_AgregarSistemaCreditosRemisiones'
)
BEGIN
    CREATE INDEX [IX_CuentasPorCobrar_IdCliente] ON [CuentasPorCobrar] ([IdCliente]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210124949_AgregarSistemaCreditosRemisiones'
)
BEGIN
    CREATE INDEX [IX_CuentasPorCobrar_IdSucursal] ON [CuentasPorCobrar] ([IdSucursal]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210124949_AgregarSistemaCreditosRemisiones'
)
BEGIN
    CREATE INDEX [IX_CuentasPorCobrar_IdVenta] ON [CuentasPorCobrar] ([IdVenta]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210124949_AgregarSistemaCreditosRemisiones'
)
BEGIN
    CREATE INDEX [IX_CuentasPorCobrar_MonedaIdMoneda] ON [CuentasPorCobrar] ([MonedaIdMoneda]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210124949_AgregarSistemaCreditosRemisiones'
)
BEGIN
    CREATE INDEX [IX_CuentasPorCobrar_UsuarioAutorizoId_Usu] ON [CuentasPorCobrar] ([UsuarioAutorizoId_Usu]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210124949_AgregarSistemaCreditosRemisiones'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CuentasPorCobrarCuotas_IdCuentaPorCobrar_NumeroCuota] ON [CuentasPorCobrarCuotas] ([IdCuentaPorCobrar], [NumeroCuota]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210124949_AgregarSistemaCreditosRemisiones'
)
BEGIN
    CREATE INDEX [IX_RemisionesInternas_IdCliente] ON [RemisionesInternas] ([IdCliente]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210124949_AgregarSistemaCreditosRemisiones'
)
BEGIN
    CREATE INDEX [IX_RemisionesInternas_IdVenta] ON [RemisionesInternas] ([IdVenta]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210124949_AgregarSistemaCreditosRemisiones'
)
BEGIN
    CREATE INDEX [IX_RemisionesInternas_MonedaIdMoneda] ON [RemisionesInternas] ([MonedaIdMoneda]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210124949_AgregarSistemaCreditosRemisiones'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RemisionesInternas_NumeroRemision] ON [RemisionesInternas] ([NumeroRemision]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210124949_AgregarSistemaCreditosRemisiones'
)
BEGIN
    CREATE INDEX [IX_RemisionesInternas_SucursalId] ON [RemisionesInternas] ([SucursalId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210124949_AgregarSistemaCreditosRemisiones'
)
BEGIN
    CREATE INDEX [IX_RemisionesInternas_UsuarioEmitioId_Usu] ON [RemisionesInternas] ([UsuarioEmitioId_Usu]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210124949_AgregarSistemaCreditosRemisiones'
)
BEGIN
    CREATE INDEX [IX_RemisionesInternas_VentaFacturaIdVenta] ON [RemisionesInternas] ([VentaFacturaIdVenta]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210124949_AgregarSistemaCreditosRemisiones'
)
BEGIN
    CREATE INDEX [IX_RemisionesInternasDetalles_IdProducto] ON [RemisionesInternasDetalles] ([IdProducto]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210124949_AgregarSistemaCreditosRemisiones'
)
BEGIN
    CREATE INDEX [IX_RemisionesInternasDetalles_IdRemision] ON [RemisionesInternasDetalles] ([IdRemision]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210124949_AgregarSistemaCreditosRemisiones'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251210124949_AgregarSistemaCreditosRemisiones', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210132603_Agregar_PermiteCredito_Cliente'
)
BEGIN
    ALTER TABLE [Clientes] ADD [PermiteCredito] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210132603_Agregar_PermiteCredito_Cliente'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251210132603_Agregar_PermiteCredito_Cliente', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210184719_Agregar_Formato_Impresion_Cajas'
)
BEGIN
    ALTER TABLE [Cajas] ADD [AnchoTicket] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210184719_Agregar_Formato_Impresion_Cajas'
)
BEGIN
    ALTER TABLE [Cajas] ADD [FormatoImpresion] nvarchar(20) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210184719_Agregar_Formato_Impresion_Cajas'
)
BEGIN
    ALTER TABLE [Cajas] ADD [MostrarLogo] bit NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251210184719_Agregar_Formato_Impresion_Cajas'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251210184719_Agregar_Formato_Impresion_Cajas', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251211205625_Agregar_CambioDelDia_Cobros'
)
BEGIN
    ALTER TABLE [CobrosDetalles] ADD [CambioDelDia] decimal(18,4) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251211205625_Agregar_CambioDelDia_Cobros'
)
BEGIN
    ALTER TABLE [CobrosCuotas] ADD [CambioDelDia] decimal(18,4) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251211205625_Agregar_CambioDelDia_Cobros'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251211205625_Agregar_CambioDelDia_Cobros', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251212154116_Actualizar_IdMoneda_CuentasPorCobrar'
)
BEGIN
                    UPDATE cpc
                    SET cpc.IdMoneda = v.IdMoneda
                    FROM CuentasPorCobrar cpc
                    INNER JOIN Ventas v ON cpc.IdVenta = v.IdVenta
                    WHERE cpc.IdMoneda IS NULL AND v.IdMoneda IS NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251212154116_Actualizar_IdMoneda_CuentasPorCobrar'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251212154116_Actualizar_IdMoneda_CuentasPorCobrar', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251215135659_Nullable_ProveedoresSifen'
)
BEGIN
    DECLARE @var9 sysname;
    SELECT @var9 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProveedoresSifen]') AND [c].[name] = N'TipoRegimen');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [ProveedoresSifen] DROP CONSTRAINT [' + @var9 + '];');
    ALTER TABLE [ProveedoresSifen] ALTER COLUMN [TipoRegimen] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251215135659_Nullable_ProveedoresSifen'
)
BEGIN
    DECLARE @var10 sysname;
    SELECT @var10 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProveedoresSifen]') AND [c].[name] = N'TipoContribuyente');
    IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [ProveedoresSifen] DROP CONSTRAINT [' + @var10 + '];');
    ALTER TABLE [ProveedoresSifen] ALTER COLUMN [TipoContribuyente] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251215135659_Nullable_ProveedoresSifen'
)
BEGIN
    DECLARE @var11 sysname;
    SELECT @var11 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProveedoresSifen]') AND [c].[name] = N'Telefono');
    IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [ProveedoresSifen] DROP CONSTRAINT [' + @var11 + '];');
    ALTER TABLE [ProveedoresSifen] ALTER COLUMN [Telefono] nvarchar(15) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251215135659_Nullable_ProveedoresSifen'
)
BEGIN
    DECLARE @var12 sysname;
    SELECT @var12 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProveedoresSifen]') AND [c].[name] = N'IdTipoContribuyenteCatalogo');
    IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [ProveedoresSifen] DROP CONSTRAINT [' + @var12 + '];');
    ALTER TABLE [ProveedoresSifen] ALTER COLUMN [IdTipoContribuyenteCatalogo] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251215135659_Nullable_ProveedoresSifen'
)
BEGIN
    DECLARE @var13 sysname;
    SELECT @var13 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProveedoresSifen]') AND [c].[name] = N'Email');
    IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [ProveedoresSifen] DROP CONSTRAINT [' + @var13 + '];');
    ALTER TABLE [ProveedoresSifen] ALTER COLUMN [Email] nvarchar(80) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251215135659_Nullable_ProveedoresSifen'
)
BEGIN
    DECLARE @var14 sysname;
    SELECT @var14 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProveedoresSifen]') AND [c].[name] = N'Direccion');
    IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [ProveedoresSifen] DROP CONSTRAINT [' + @var14 + '];');
    ALTER TABLE [ProveedoresSifen] ALTER COLUMN [Direccion] nvarchar(255) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251215135659_Nullable_ProveedoresSifen'
)
BEGIN
    DECLARE @var15 sysname;
    SELECT @var15 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProveedoresSifen]') AND [c].[name] = N'DescripcionDepartamento');
    IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [ProveedoresSifen] DROP CONSTRAINT [' + @var15 + '];');
    ALTER TABLE [ProveedoresSifen] ALTER COLUMN [DescripcionDepartamento] nvarchar(16) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251215135659_Nullable_ProveedoresSifen'
)
BEGIN
    DECLARE @var16 sysname;
    SELECT @var16 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProveedoresSifen]') AND [c].[name] = N'DescripcionCiudad');
    IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [ProveedoresSifen] DROP CONSTRAINT [' + @var16 + '];');
    ALTER TABLE [ProveedoresSifen] ALTER COLUMN [DescripcionCiudad] nvarchar(30) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251215135659_Nullable_ProveedoresSifen'
)
BEGIN
    DECLARE @var17 sysname;
    SELECT @var17 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProveedoresSifen]') AND [c].[name] = N'DescripcionActividadEconomica');
    IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [ProveedoresSifen] DROP CONSTRAINT [' + @var17 + '];');
    ALTER TABLE [ProveedoresSifen] ALTER COLUMN [DescripcionActividadEconomica] nvarchar(300) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251215135659_Nullable_ProveedoresSifen'
)
BEGIN
    DECLARE @var18 sysname;
    SELECT @var18 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProveedoresSifen]') AND [c].[name] = N'CodigoDepartamento');
    IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [ProveedoresSifen] DROP CONSTRAINT [' + @var18 + '];');
    ALTER TABLE [ProveedoresSifen] ALTER COLUMN [CodigoDepartamento] nvarchar(2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251215135659_Nullable_ProveedoresSifen'
)
BEGIN
    DECLARE @var19 sysname;
    SELECT @var19 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProveedoresSifen]') AND [c].[name] = N'CodigoCiudad');
    IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [ProveedoresSifen] DROP CONSTRAINT [' + @var19 + '];');
    ALTER TABLE [ProveedoresSifen] ALTER COLUMN [CodigoCiudad] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251215135659_Nullable_ProveedoresSifen'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251215135659_Nullable_ProveedoresSifen', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251215154434_AgregarEsServicioTiposItem'
)
BEGIN
    ALTER TABLE [TiposItem] ADD [EsServicio] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251215154434_AgregarEsServicioTiposItem'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251215154434_AgregarEsServicioTiposItem', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216004751_Agregar_Sistema_Permisos_Auditoria'
)
BEGIN
    CREATE TABLE [AuditoriasAcciones] (
        [IdAuditoria] int NOT NULL IDENTITY,
        [IdUsuario] int NOT NULL,
        [NombreUsuario] nvarchar(200) NOT NULL,
        [RolUsuario] nvarchar(100) NULL,
        [FechaHora] datetime2 NOT NULL,
        [Modulo] nvarchar(100) NULL,
        [Accion] nvarchar(200) NOT NULL,
        [TipoAccion] nvarchar(50) NULL,
        [Entidad] nvarchar(100) NULL,
        [IdRegistroAfectado] int NULL,
        [Descripcion] nvarchar(2000) NULL,
        [DatosAntes] nvarchar(max) NULL,
        [DatosDespues] nvarchar(max) NULL,
        [DireccionIP] nvarchar(50) NULL,
        [Navegador] nvarchar(500) NULL,
        [Exitosa] bit NOT NULL,
        [MensajeError] nvarchar(2000) NULL,
        [Severidad] nvarchar(20) NULL,
        CONSTRAINT [PK_AuditoriasAcciones] PRIMARY KEY ([IdAuditoria]),
        CONSTRAINT [FK_AuditoriasAcciones_Usuarios_IdUsuario] FOREIGN KEY ([IdUsuario]) REFERENCES [Usuarios] ([Id_Usu]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216004751_Agregar_Sistema_Permisos_Auditoria'
)
BEGIN
    CREATE TABLE [Modulos] (
        [IdModulo] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        [Descripcion] nvarchar(500) NULL,
        [Icono] nvarchar(50) NULL,
        [Orden] int NULL,
        [IdModuloPadre] int NULL,
        [RutaPagina] nvarchar(200) NULL,
        [Activo] bit NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        CONSTRAINT [PK_Modulos] PRIMARY KEY ([IdModulo]),
        CONSTRAINT [FK_Modulos_Modulos_IdModuloPadre] FOREIGN KEY ([IdModuloPadre]) REFERENCES [Modulos] ([IdModulo])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216004751_Agregar_Sistema_Permisos_Auditoria'
)
BEGIN
    CREATE TABLE [Permisos] (
        [IdPermiso] int NOT NULL IDENTITY,
        [Nombre] nvarchar(50) NOT NULL,
        [Codigo] nvarchar(50) NOT NULL,
        [Descripcion] nvarchar(500) NULL,
        [Orden] int NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Permisos] PRIMARY KEY ([IdPermiso])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216004751_Agregar_Sistema_Permisos_Auditoria'
)
BEGIN
    CREATE TABLE [RolesModulosPermisos] (
        [IdRolModuloPermiso] int NOT NULL IDENTITY,
        [IdRol] int NOT NULL,
        [IdModulo] int NOT NULL,
        [IdPermiso] int NOT NULL,
        [Concedido] bit NOT NULL,
        [FechaAsignacion] datetime2 NOT NULL,
        [UsuarioAsignacion] nvarchar(100) NULL,
        CONSTRAINT [PK_RolesModulosPermisos] PRIMARY KEY ([IdRolModuloPermiso]),
        CONSTRAINT [FK_RolesModulosPermisos_Modulos_IdModulo] FOREIGN KEY ([IdModulo]) REFERENCES [Modulos] ([IdModulo]) ON DELETE CASCADE,
        CONSTRAINT [FK_RolesModulosPermisos_Permisos_IdPermiso] FOREIGN KEY ([IdPermiso]) REFERENCES [Permisos] ([IdPermiso]) ON DELETE CASCADE,
        CONSTRAINT [FK_RolesModulosPermisos_Rol_IdRol] FOREIGN KEY ([IdRol]) REFERENCES [Rol] ([Id_Rol]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216004751_Agregar_Sistema_Permisos_Auditoria'
)
BEGIN
    CREATE INDEX [IX_AuditoriasAcciones_IdUsuario] ON [AuditoriasAcciones] ([IdUsuario]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216004751_Agregar_Sistema_Permisos_Auditoria'
)
BEGIN
    CREATE INDEX [IX_Modulos_IdModuloPadre] ON [Modulos] ([IdModuloPadre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216004751_Agregar_Sistema_Permisos_Auditoria'
)
BEGIN
    CREATE INDEX [IX_RolesModulosPermisos_IdModulo] ON [RolesModulosPermisos] ([IdModulo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216004751_Agregar_Sistema_Permisos_Auditoria'
)
BEGIN
    CREATE INDEX [IX_RolesModulosPermisos_IdPermiso] ON [RolesModulosPermisos] ([IdPermiso]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216004751_Agregar_Sistema_Permisos_Auditoria'
)
BEGIN
    CREATE INDEX [IX_RolesModulosPermisos_IdRol] ON [RolesModulosPermisos] ([IdRol]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251216004751_Agregar_Sistema_Permisos_Auditoria'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251216004751_Agregar_Sistema_Permisos_Auditoria', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251217141418_Usuario_CorreoYEmbeddingOpcional'
)
BEGIN
    DECLARE @var20 sysname;
    SELECT @var20 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Usuarios]') AND [c].[name] = N'EmbeddingFacial');
    IF @var20 IS NOT NULL EXEC(N'ALTER TABLE [Usuarios] DROP CONSTRAINT [' + @var20 + '];');
    ALTER TABLE [Usuarios] ALTER COLUMN [EmbeddingFacial] varbinary(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251217141418_Usuario_CorreoYEmbeddingOpcional'
)
BEGIN
    DECLARE @var21 sysname;
    SELECT @var21 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Usuarios]') AND [c].[name] = N'Correo');
    IF @var21 IS NOT NULL EXEC(N'ALTER TABLE [Usuarios] DROP CONSTRAINT [' + @var21 + '];');
    ALTER TABLE [Usuarios] ALTER COLUMN [Correo] nvarchar(150) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251217141418_Usuario_CorreoYEmbeddingOpcional'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251217141418_Usuario_CorreoYEmbeddingOpcional', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251217221956_Agregar_Tablas_CierreCaja'
)
BEGIN
    CREATE TABLE [CierresCaja] (
        [IdCierreCaja] int NOT NULL IDENTITY,
        [IdCaja] int NOT NULL,
        [FechaCierre] datetime2 NOT NULL,
        [FechaCaja] datetime2 NOT NULL,
        [Turno] int NOT NULL,
        [UsuarioCierre] nvarchar(100) NULL,
        [TotalVentasContado] decimal(18,2) NOT NULL,
        [TotalVentasCredito] decimal(18,2) NOT NULL,
        [TotalCobrosCredito] decimal(18,2) NOT NULL,
        [TotalAnulaciones] decimal(18,2) NOT NULL,
        [TotalEfectivo] decimal(18,2) NOT NULL,
        [TotalTarjetas] decimal(18,2) NOT NULL,
        [TotalCheques] decimal(18,2) NOT NULL,
        [TotalTransferencias] decimal(18,2) NOT NULL,
        [TotalQR] decimal(18,2) NOT NULL,
        [TotalOtros] decimal(18,2) NOT NULL,
        [TotalEsperado] decimal(18,2) NOT NULL,
        [TotalEntregado] decimal(18,2) NOT NULL,
        [Diferencia] decimal(18,2) NOT NULL,
        [Observaciones] nvarchar(500) NULL,
        [Estado] nvarchar(20) NOT NULL,
        CONSTRAINT [PK_CierresCaja] PRIMARY KEY ([IdCierreCaja]),
        CONSTRAINT [FK_CierresCaja_Cajas_IdCaja] FOREIGN KEY ([IdCaja]) REFERENCES [Cajas] ([id_caja]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251217221956_Agregar_Tablas_CierreCaja'
)
BEGIN
    CREATE TABLE [EntregasCaja] (
        [IdEntregaCaja] int NOT NULL IDENTITY,
        [IdCierreCaja] int NOT NULL,
        [Medio] int NOT NULL,
        [IdMoneda] int NULL,
        [MontoEsperado] decimal(18,2) NOT NULL,
        [MontoEntregado] decimal(18,2) NOT NULL,
        [Diferencia] decimal(18,2) NOT NULL,
        [ReceptorEntrega] nvarchar(100) NULL,
        [Observaciones] nvarchar(300) NULL,
        [DetalleCheques] nvarchar(500) NULL,
        [CantidadVouchers] int NULL,
        CONSTRAINT [PK_EntregasCaja] PRIMARY KEY ([IdEntregaCaja]),
        CONSTRAINT [FK_EntregasCaja_CierresCaja_IdCierreCaja] FOREIGN KEY ([IdCierreCaja]) REFERENCES [CierresCaja] ([IdCierreCaja]) ON DELETE CASCADE,
        CONSTRAINT [FK_EntregasCaja_Monedas_IdMoneda] FOREIGN KEY ([IdMoneda]) REFERENCES [Monedas] ([IdMoneda]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251217221956_Agregar_Tablas_CierreCaja'
)
BEGIN
    CREATE INDEX [IX_CierresCaja_Caja_Fecha_Turno] ON [CierresCaja] ([IdCaja], [FechaCaja], [Turno]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251217221956_Agregar_Tablas_CierreCaja'
)
BEGIN
    CREATE INDEX [IX_EntregasCaja_IdCierreCaja] ON [EntregasCaja] ([IdCierreCaja]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251217221956_Agregar_Tablas_CierreCaja'
)
BEGIN
    CREATE INDEX [IX_EntregasCaja_IdMoneda] ON [EntregasCaja] ([IdMoneda]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251217221956_Agregar_Tablas_CierreCaja'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251217221956_Agregar_Tablas_CierreCaja', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219142806_Agregar_ControladoReceta_Y_RecetasVentas'
)
BEGIN
                    IF COL_LENGTH('Productos', 'ControladoReceta') IS NULL
                    BEGIN
                        ALTER TABLE [Productos] ADD [ControladoReceta] bit NOT NULL DEFAULT CAST(0 AS bit);
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219142806_Agregar_ControladoReceta_Y_RecetasVentas'
)
BEGIN
                    IF OBJECT_ID('RecetasVentas', 'U') IS NULL
                    BEGIN
                        CREATE TABLE [RecetasVentas] (
                            [IdRecetaVenta] int NOT NULL IDENTITY,
                            [IdVenta] int NOT NULL,
                            [IdProducto] int NOT NULL,
                            [NumeroRegistro] nvarchar(50) NOT NULL,
                            [FechaReceta] datetime2 NOT NULL,
                            [NombreMedico] nvarchar(200) NOT NULL,
                            [NombrePaciente] nvarchar(200) NOT NULL,
                            [FechaRegistro] datetime2 NOT NULL,
                            [UsuarioRegistro] nvarchar(50) NULL,
                            CONSTRAINT [PK_RecetasVentas] PRIMARY KEY ([IdRecetaVenta]),
                            CONSTRAINT [FK_RecetasVentas_Productos_IdProducto] FOREIGN KEY ([IdProducto]) REFERENCES [Productos] ([IdProducto]) ON DELETE CASCADE,
                            CONSTRAINT [FK_RecetasVentas_Ventas_IdVenta] FOREIGN KEY ([IdVenta]) REFERENCES [Ventas] ([IdVenta]) ON DELETE CASCADE
                        );
                        CREATE INDEX [IX_RecetasVentas_IdProducto] ON [RecetasVentas] ([IdProducto]);
                        CREATE INDEX [IX_RecetasVentas_IdVenta] ON [RecetasVentas] ([IdVenta]);
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251219142806_Agregar_ControladoReceta_Y_RecetasVentas'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251219142806_Agregar_ControladoReceta_Y_RecetasVentas', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251220174856_Agregar_PermiteDecimal_Producto'
)
BEGIN
    ALTER TABLE [Productos] ADD [PermiteDecimal] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251220174856_Agregar_PermiteDecimal_Producto'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251220174856_Agregar_PermiteDecimal_Producto', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251222020630_Agregar_Serie_Venta_Vuelto_Composicion'
)
BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Ventas') AND name = 'Serie')
                    BEGIN
                        ALTER TABLE [Ventas] ADD [Serie] int NULL;
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251222020630_Agregar_Serie_Venta_Vuelto_Composicion'
)
BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'ComposicionesCaja') AND name = 'Vuelto')
                    BEGIN
                        ALTER TABLE [ComposicionesCaja] ADD [Vuelto] decimal(18,4) NOT NULL DEFAULT 0;
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251222020630_Agregar_Serie_Venta_Vuelto_Composicion'
)
BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'Ventas') AND name = 'IX_Ventas_Numeracion_Unica')
                    BEGIN
                        CREATE UNIQUE INDEX [IX_Ventas_Numeracion_Unica] 
                        ON [Ventas] ([Establecimiento], [NumeroFactura], [PuntoExpedicion], [Serie], [Timbrado])
                        WHERE [NumeroFactura] IS NOT NULL AND [Serie] IS NOT NULL AND [Timbrado] IS NOT NULL;
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251222020630_Agregar_Serie_Venta_Vuelto_Composicion'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251222020630_Agregar_Serie_Venta_Vuelto_Composicion', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251222030501_Agregar_Nombre_IdSucursal_Caja'
)
BEGIN
    ALTER TABLE [Cajas] ADD [IdSucursal] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251222030501_Agregar_Nombre_IdSucursal_Caja'
)
BEGIN
    ALTER TABLE [Cajas] ADD [Nombre] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251222030501_Agregar_Nombre_IdSucursal_Caja'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251222030501_Agregar_Nombre_IdSucursal_Caja', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251222031949_Fix_IdCaja_Ventas'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251222031949_Fix_IdCaja_Ventas', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251222124654_Agregar_Turno_Cobros_Pagos'
)
BEGIN
    ALTER TABLE [PagosProveedores] ADD [Turno] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251222124654_Agregar_Turno_Cobros_Pagos'
)
BEGIN
    ALTER TABLE [CobrosCuotas] ADD [Turno] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251222124654_Agregar_Turno_Cobros_Pagos'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251222124654_Agregar_Turno_Cobros_Pagos', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251222142336_Crear_Tablas_CuentasPorPagar'
)
BEGIN
                    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CuentasPorPagar')
                    BEGIN
                        CREATE TABLE [CuentasPorPagar] (
                            [IdCuentaPorPagar] int NOT NULL IDENTITY,
                            [IdCompra] int NOT NULL,
                            [IdProveedor] int NOT NULL,
                            [IdSucursal] int NOT NULL,
                            [IdMoneda] int NULL,
                            [MontoTotal] decimal(18,4) NOT NULL,
                            [SaldoPendiente] decimal(18,4) NOT NULL,
                            [FechaCredito] datetime2 NOT NULL,
                            [FechaVencimiento] datetime2 NULL,
                            [Estado] nvarchar(20) NOT NULL,
                            [NumeroCuotas] int NOT NULL,
                            [PlazoDias] int NOT NULL,
                            [Observaciones] nvarchar(280) NULL,
                            [IdUsuarioAutorizo] int NULL,
                            CONSTRAINT [PK_CuentasPorPagar] PRIMARY KEY ([IdCuentaPorPagar]),
                            CONSTRAINT [FK_CuentasPorPagar_Compras_IdCompra] FOREIGN KEY ([IdCompra]) REFERENCES [Compras] ([IdCompra]) ON DELETE NO ACTION,
                            CONSTRAINT [FK_CuentasPorPagar_ProveedoresSifen_IdProveedor] FOREIGN KEY ([IdProveedor]) REFERENCES [ProveedoresSifen] ([IdProveedor]) ON DELETE NO ACTION,
                            CONSTRAINT [FK_CuentasPorPagar_Monedas_IdMoneda] FOREIGN KEY ([IdMoneda]) REFERENCES [Monedas] ([IdMoneda]),
                            CONSTRAINT [FK_CuentasPorPagar_Usuarios_IdUsuarioAutorizo] FOREIGN KEY ([IdUsuarioAutorizo]) REFERENCES [Usuarios] ([Id_Usu])
                        );
                        CREATE INDEX [IX_CuentasPorPagar_IdCompra] ON [CuentasPorPagar] ([IdCompra]);
                        CREATE INDEX [IX_CuentasPorPagar_IdProveedor] ON [CuentasPorPagar] ([IdProveedor]);
                        CREATE INDEX [IX_CuentasPorPagar_IdMoneda] ON [CuentasPorPagar] ([IdMoneda]);
                        CREATE INDEX [IX_CuentasPorPagar_IdUsuarioAutorizo] ON [CuentasPorPagar] ([IdUsuarioAutorizo]);
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251222142336_Crear_Tablas_CuentasPorPagar'
)
BEGIN
                    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CuentasPorPagarCuotas')
                    BEGIN
                        CREATE TABLE [CuentasPorPagarCuotas] (
                            [IdCuota] int NOT NULL IDENTITY,
                            [IdCuentaPorPagar] int NOT NULL,
                            [NumeroCuota] int NOT NULL,
                            [MontoCuota] decimal(18,4) NOT NULL,
                            [SaldoCuota] decimal(18,4) NOT NULL,
                            [FechaVencimiento] datetime2 NOT NULL,
                            [FechaPago] datetime2 NULL,
                            [Estado] nvarchar(20) NOT NULL,
                            [Observaciones] nvarchar(280) NULL,
                            CONSTRAINT [PK_CuentasPorPagarCuotas] PRIMARY KEY ([IdCuota]),
                            CONSTRAINT [FK_CuentasPorPagarCuotas_CuentasPorPagar_IdCuentaPorPagar] FOREIGN KEY ([IdCuentaPorPagar]) REFERENCES [CuentasPorPagar] ([IdCuentaPorPagar]) ON DELETE CASCADE
                        );
                        CREATE INDEX [IX_CuentasPorPagarCuotas_IdCuentaPorPagar] ON [CuentasPorPagarCuotas] ([IdCuentaPorPagar]);
                    END
                    ELSE
                    BEGIN
                        -- Si la tabla ya existe pero le falta SaldoCuota, agregar la columna
                        IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CuentasPorPagarCuotas' AND COLUMN_NAME = 'SaldoCuota')
                        BEGIN
                            ALTER TABLE [CuentasPorPagarCuotas] ADD [SaldoCuota] decimal(18,4) NOT NULL DEFAULT 0;
                        END
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251222142336_Crear_Tablas_CuentasPorPagar'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251222142336_Crear_Tablas_CuentasPorPagar', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251223011802_Agregar_Factor_Compras_Producto'
)
BEGIN
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Productos') AND name = 'FactorMultiplicador')
                    BEGIN
                        ALTER TABLE [Productos] ADD [FactorMultiplicador] decimal(18,4) NULL;
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251223011802_Agregar_Factor_Compras_Producto'
)
BEGIN
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'ComprasDetalles') AND name = 'FactorMultiplicador')
                    BEGIN
                        ALTER TABLE [ComprasDetalles] ADD [FactorMultiplicador] decimal(18,4) NULL;
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251223011802_Agregar_Factor_Compras_Producto'
)
BEGIN
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'ComprasDetalles') AND name = 'PorcentajeMargen')
                    BEGIN
                        ALTER TABLE [ComprasDetalles] ADD [PorcentajeMargen] decimal(18,4) NULL;
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251223011802_Agregar_Factor_Compras_Producto'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251223011802_Agregar_Factor_Compras_Producto', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251224023749_Agregar_IdSucursal_CierreCaja'
)
BEGIN
    ALTER TABLE [CierresCaja] ADD [IdSucursal] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251224023749_Agregar_IdSucursal_CierreCaja'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251224023749_Agregar_IdSucursal_CierreCaja', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251224032818_Agregar_IdSucursal_CobroCuota'
)
BEGIN
    ALTER TABLE [CobrosCuotas] ADD [IdSucursal] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251224032818_Agregar_IdSucursal_CobroCuota'
)
BEGIN
    CREATE INDEX [IX_CobrosCuotas_IdSucursal] ON [CobrosCuotas] ([IdSucursal]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251224032818_Agregar_IdSucursal_CobroCuota'
)
BEGIN
    ALTER TABLE [CobrosCuotas] ADD CONSTRAINT [FK_CobrosCuotas_Sucursal_IdSucursal] FOREIGN KEY ([IdSucursal]) REFERENCES [Sucursal] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251224032818_Agregar_IdSucursal_CobroCuota'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251224032818_Agregar_IdSucursal_CobroCuota', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251225205203_Agregar_PrecioVentaRef_CompraDetalle'
)
BEGIN
                    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                                   WHERE TABLE_NAME = 'ComprasDetalles' AND COLUMN_NAME = 'PrecioVentaRef')
                    BEGIN
                        ALTER TABLE [ComprasDetalles] ADD [PrecioVentaRef] decimal(18,4) NOT NULL DEFAULT 0;
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251225205203_Agregar_PrecioVentaRef_CompraDetalle'
)
BEGIN
                    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CuentasPorPagar_Proveedores_IdProveedor')
                    BEGIN
                        ALTER TABLE [CuentasPorPagar] DROP CONSTRAINT [FK_CuentasPorPagar_Proveedores_IdProveedor];
                    END
                    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CuentasPorPagar_ProveedoresSifen_IdProveedor')
                    BEGIN
                        ALTER TABLE [CuentasPorPagar] ADD CONSTRAINT [FK_CuentasPorPagar_ProveedoresSifen_IdProveedor] 
                            FOREIGN KEY ([IdProveedor]) REFERENCES [ProveedoresSifen]([IdProveedor]) ON DELETE NO ACTION;
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251225205203_Agregar_PrecioVentaRef_CompraDetalle'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251225205203_Agregar_PrecioVentaRef_CompraDetalle', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227011006_Crear_NotasCreditoVentas'
)
BEGIN
    CREATE TABLE [NotasCreditoVentas] (
        [IdNotaCredito] int NOT NULL IDENTITY,
        [Establecimiento] nvarchar(3) NULL,
        [PuntoExpedicion] nvarchar(3) NULL,
        [NumeroNota] int NOT NULL,
        [IdSucursal] int NOT NULL,
        [IdCaja] int NOT NULL,
        [IdCliente] int NULL,
        [IdVentaAsociada] int NULL,
        [TipoDocumentoAsociado] nvarchar(20) NULL,
        [Fecha] datetime2 NOT NULL,
        [FechaContable] datetime2 NULL,
        [Turno] int NULL,
        [Motivo] nvarchar(50) NOT NULL,
        [Observaciones] nvarchar(500) NULL,
        [IdMoneda] int NULL,
        [SimboloMoneda] nvarchar(4) NULL,
        [CambioDelDia] decimal(18,4) NULL,
        [EsMonedaExtranjera] bit NULL,
        [Subtotal] decimal(18,4) NOT NULL,
        [TotalIVA10] decimal(18,4) NOT NULL,
        [TotalIVA5] decimal(18,4) NOT NULL,
        [TotalExenta] decimal(18,4) NOT NULL,
        [Total] decimal(18,4) NOT NULL,
        [TotalEnLetras] nvarchar(280) NULL,
        [Estado] nvarchar(20) NOT NULL,
        [Timbrado] nvarchar(8) NULL,
        [Serie] int NULL,
        [CDC] nvarchar(64) NULL,
        [CodigoSeguridad] nvarchar(9) NULL,
        [EstadoSifen] nvarchar(30) NULL,
        [FechaEnvioSifen] datetime2 NULL,
        [MensajeSifen] nvarchar(max) NULL,
        [XmlCDE] nvarchar(max) NULL,
        [IdLote] nvarchar(50) NULL,
        [IdUsuario] int NULL,
        CONSTRAINT [PK_NotasCreditoVentas] PRIMARY KEY ([IdNotaCredito]),
        CONSTRAINT [FK_NotasCreditoVentas_Cajas_IdCaja] FOREIGN KEY ([IdCaja]) REFERENCES [Cajas] ([id_caja]) ON DELETE NO ACTION,
        CONSTRAINT [FK_NotasCreditoVentas_Clientes_IdCliente] FOREIGN KEY ([IdCliente]) REFERENCES [Clientes] ([IdCliente]) ON DELETE NO ACTION,
        CONSTRAINT [FK_NotasCreditoVentas_Monedas_IdMoneda] FOREIGN KEY ([IdMoneda]) REFERENCES [Monedas] ([IdMoneda]) ON DELETE NO ACTION,
        CONSTRAINT [FK_NotasCreditoVentas_Sucursal_IdSucursal] FOREIGN KEY ([IdSucursal]) REFERENCES [Sucursal] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_NotasCreditoVentas_Usuarios_IdUsuario] FOREIGN KEY ([IdUsuario]) REFERENCES [Usuarios] ([Id_Usu]) ON DELETE NO ACTION,
        CONSTRAINT [FK_NotasCreditoVentas_Ventas_IdVentaAsociada] FOREIGN KEY ([IdVentaAsociada]) REFERENCES [Ventas] ([IdVenta]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227011006_Crear_NotasCreditoVentas'
)
BEGIN
    CREATE TABLE [NotasCreditoVentasDetalles] (
        [IdNotaCreditoDetalle] int NOT NULL IDENTITY,
        [IdNotaCredito] int NOT NULL,
        [IdProducto] int NOT NULL,
        [Cantidad] decimal(18,4) NOT NULL,
        [PrecioUnitario] decimal(18,4) NOT NULL,
        [PorcentajeDescuento] decimal(18,4) NOT NULL,
        [Descuento] decimal(18,4) NOT NULL,
        [Importe] decimal(18,4) NOT NULL,
        [IVA10] decimal(18,4) NOT NULL,
        [IVA5] decimal(18,4) NOT NULL,
        [Exenta] decimal(18,4) NOT NULL,
        [Grabado10] decimal(18,4) NOT NULL,
        [Grabado5] decimal(18,4) NOT NULL,
        [CambioDelDia] decimal(18,4) NULL,
        [IdTipoIva] int NULL,
        [IdDeposito] int NULL,
        [Lote] nvarchar(50) NULL,
        CONSTRAINT [PK_NotasCreditoVentasDetalles] PRIMARY KEY ([IdNotaCreditoDetalle]),
        CONSTRAINT [FK_NotasCreditoVentasDetalles_Depositos_IdDeposito] FOREIGN KEY ([IdDeposito]) REFERENCES [Depositos] ([IdDeposito]) ON DELETE NO ACTION,
        CONSTRAINT [FK_NotasCreditoVentasDetalles_NotasCreditoVentas_IdNotaCredito] FOREIGN KEY ([IdNotaCredito]) REFERENCES [NotasCreditoVentas] ([IdNotaCredito]) ON DELETE CASCADE,
        CONSTRAINT [FK_NotasCreditoVentasDetalles_Productos_IdProducto] FOREIGN KEY ([IdProducto]) REFERENCES [Productos] ([IdProducto]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227011006_Crear_NotasCreditoVentas'
)
BEGIN
    CREATE UNIQUE INDEX [IX_NotaCreditoVentas_Numeracion] ON [NotasCreditoVentas] ([IdSucursal], [NumeroNota]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227011006_Crear_NotasCreditoVentas'
)
BEGIN
    CREATE INDEX [IX_NotasCreditoVentas_IdCaja] ON [NotasCreditoVentas] ([IdCaja]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227011006_Crear_NotasCreditoVentas'
)
BEGIN
    CREATE INDEX [IX_NotasCreditoVentas_IdCliente] ON [NotasCreditoVentas] ([IdCliente]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227011006_Crear_NotasCreditoVentas'
)
BEGIN
    CREATE INDEX [IX_NotasCreditoVentas_IdMoneda] ON [NotasCreditoVentas] ([IdMoneda]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227011006_Crear_NotasCreditoVentas'
)
BEGIN
    CREATE INDEX [IX_NotasCreditoVentas_IdUsuario] ON [NotasCreditoVentas] ([IdUsuario]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227011006_Crear_NotasCreditoVentas'
)
BEGIN
    CREATE INDEX [IX_NotasCreditoVentas_IdVentaAsociada] ON [NotasCreditoVentas] ([IdVentaAsociada]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227011006_Crear_NotasCreditoVentas'
)
BEGIN
    CREATE INDEX [IX_NotasCreditoVentasDetalles_IdDeposito] ON [NotasCreditoVentasDetalles] ([IdDeposito]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227011006_Crear_NotasCreditoVentas'
)
BEGIN
    CREATE INDEX [IX_NotasCreditoVentasDetalles_IdNotaCredito] ON [NotasCreditoVentasDetalles] ([IdNotaCredito]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227011006_Crear_NotasCreditoVentas'
)
BEGIN
    CREATE INDEX [IX_NotasCreditoVentasDetalles_IdProducto] ON [NotasCreditoVentasDetalles] ([IdProducto]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227011006_Crear_NotasCreditoVentas'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251227011006_Crear_NotasCreditoVentas', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227020637_Agregar_NotasCredito_Columnas_Faltantes'
)
BEGIN
    ALTER TABLE [NotasCreditoVentasDetalles] ADD [CodigoProducto] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227020637_Agregar_NotasCredito_Columnas_Faltantes'
)
BEGIN
    ALTER TABLE [NotasCreditoVentasDetalles] ADD [MontoDescuento] decimal(18,4) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227020637_Agregar_NotasCredito_Columnas_Faltantes'
)
BEGIN
    ALTER TABLE [NotasCreditoVentasDetalles] ADD [NombreProducto] nvarchar(250) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227020637_Agregar_NotasCredito_Columnas_Faltantes'
)
BEGIN
    ALTER TABLE [NotasCreditoVentasDetalles] ADD [TasaIVA] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227020637_Agregar_NotasCredito_Columnas_Faltantes'
)
BEGIN
    DECLARE @var22 sysname;
    SELECT @var22 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[NotasCreditoVentas]') AND [c].[name] = N'Turno');
    IF @var22 IS NOT NULL EXEC(N'ALTER TABLE [NotasCreditoVentas] DROP CONSTRAINT [' + @var22 + '];');
    ALTER TABLE [NotasCreditoVentas] ALTER COLUMN [Turno] nvarchar(3) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227020637_Agregar_NotasCredito_Columnas_Faltantes'
)
BEGIN
    DROP INDEX [IX_NotasCreditoVentas_IdMoneda] ON [NotasCreditoVentas];
    DECLARE @var23 sysname;
    SELECT @var23 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[NotasCreditoVentas]') AND [c].[name] = N'IdMoneda');
    IF @var23 IS NOT NULL EXEC(N'ALTER TABLE [NotasCreditoVentas] DROP CONSTRAINT [' + @var23 + '];');
    EXEC(N'UPDATE [NotasCreditoVentas] SET [IdMoneda] = 0 WHERE [IdMoneda] IS NULL');
    ALTER TABLE [NotasCreditoVentas] ALTER COLUMN [IdMoneda] int NOT NULL;
    ALTER TABLE [NotasCreditoVentas] ADD DEFAULT 0 FOR [IdMoneda];
    CREATE INDEX [IX_NotasCreditoVentas_IdMoneda] ON [NotasCreditoVentas] ([IdMoneda]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227020637_Agregar_NotasCredito_Columnas_Faltantes'
)
BEGIN
    DECLARE @var24 sysname;
    SELECT @var24 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[NotasCreditoVentas]') AND [c].[name] = N'EsMonedaExtranjera');
    IF @var24 IS NOT NULL EXEC(N'ALTER TABLE [NotasCreditoVentas] DROP CONSTRAINT [' + @var24 + '];');
    EXEC(N'UPDATE [NotasCreditoVentas] SET [EsMonedaExtranjera] = CAST(0 AS bit) WHERE [EsMonedaExtranjera] IS NULL');
    ALTER TABLE [NotasCreditoVentas] ALTER COLUMN [EsMonedaExtranjera] bit NOT NULL;
    ALTER TABLE [NotasCreditoVentas] ADD DEFAULT CAST(0 AS bit) FOR [EsMonedaExtranjera];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227020637_Agregar_NotasCredito_Columnas_Faltantes'
)
BEGIN
    DECLARE @var25 sysname;
    SELECT @var25 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[NotasCreditoVentas]') AND [c].[name] = N'CambioDelDia');
    IF @var25 IS NOT NULL EXEC(N'ALTER TABLE [NotasCreditoVentas] DROP CONSTRAINT [' + @var25 + '];');
    EXEC(N'UPDATE [NotasCreditoVentas] SET [CambioDelDia] = 0.0 WHERE [CambioDelDia] IS NULL');
    ALTER TABLE [NotasCreditoVentas] ALTER COLUMN [CambioDelDia] decimal(18,4) NOT NULL;
    ALTER TABLE [NotasCreditoVentas] ADD DEFAULT 0.0 FOR [CambioDelDia];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227020637_Agregar_NotasCredito_Columnas_Faltantes'
)
BEGIN
    ALTER TABLE [NotasCreditoVentas] ADD [CreadoPor] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227020637_Agregar_NotasCredito_Columnas_Faltantes'
)
BEGIN
    ALTER TABLE [NotasCreditoVentas] ADD [FechaCreacion] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227020637_Agregar_NotasCredito_Columnas_Faltantes'
)
BEGIN
    ALTER TABLE [NotasCreditoVentas] ADD [FechaModificacion] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227020637_Agregar_NotasCredito_Columnas_Faltantes'
)
BEGIN
    ALTER TABLE [NotasCreditoVentas] ADD [ModificadoPor] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227020637_Agregar_NotasCredito_Columnas_Faltantes'
)
BEGIN
    ALTER TABLE [NotasCreditoVentas] ADD [NombreCliente] nvarchar(200) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227020637_Agregar_NotasCredito_Columnas_Faltantes'
)
BEGIN
    ALTER TABLE [NotasCreditoVentas] ADD [TotalDescuento] decimal(18,4) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251227020637_Agregar_NotasCredito_Columnas_Faltantes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251227020637_Agregar_NotasCredito_Columnas_Faltantes', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228133522_Agregar_IdDeposito_AjusteStockDetalle'
)
BEGIN
                    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_AjustesStockDetalles_AjustesStock_AjusteStockIdAjusteStock')
                    BEGIN
                        ALTER TABLE [AjustesStockDetalles] DROP CONSTRAINT [FK_AjustesStockDetalles_AjustesStock_AjusteStockIdAjusteStock];
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228133522_Agregar_IdDeposito_AjusteStockDetalle'
)
BEGIN
                    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AjustesStockDetalles_AjusteStockIdAjusteStock')
                    BEGIN
                        DROP INDEX [IX_AjustesStockDetalles_AjusteStockIdAjusteStock] ON [AjustesStockDetalles];
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228133522_Agregar_IdDeposito_AjusteStockDetalle'
)
BEGIN
                    IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'AjusteStockIdAjusteStock' AND Object_ID = Object_ID(N'AjustesStockDetalles'))
                    BEGIN
                        ALTER TABLE [AjustesStockDetalles] DROP COLUMN [AjusteStockIdAjusteStock];
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228133522_Agregar_IdDeposito_AjusteStockDetalle'
)
BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'IdDeposito' AND Object_ID = Object_ID(N'AjustesStockDetalles'))
                    BEGIN
                        ALTER TABLE [AjustesStockDetalles] ADD [IdDeposito] int NOT NULL DEFAULT 1;
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228133522_Agregar_IdDeposito_AjusteStockDetalle'
)
BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AjustesStockDetalles_IdDeposito')
                    BEGIN
                        CREATE INDEX [IX_AjustesStockDetalles_IdDeposito] ON [AjustesStockDetalles] ([IdDeposito]);
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228133522_Agregar_IdDeposito_AjusteStockDetalle'
)
BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_AjustesStockDetalles_Depositos_IdDeposito')
                    BEGIN
                        ALTER TABLE [AjustesStockDetalles] ADD CONSTRAINT [FK_AjustesStockDetalles_Depositos_IdDeposito] 
                            FOREIGN KEY ([IdDeposito]) REFERENCES [Depositos] ([IdDeposito]) ON DELETE NO ACTION;
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228133522_Agregar_IdDeposito_AjusteStockDetalle'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251228133522_Agregar_IdDeposito_AjusteStockDetalle', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228144125_Agregar_Campos_Trazabilidad_MovimientosInventario'
)
BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'CantidadAnterior')
                    BEGIN
                        ALTER TABLE [MovimientosInventario] ADD [CantidadAnterior] decimal(18,4) NULL;
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228144125_Agregar_Campos_Trazabilidad_MovimientosInventario'
)
BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'SaldoPosterior')
                    BEGIN
                        ALTER TABLE [MovimientosInventario] ADD [SaldoPosterior] decimal(18,4) NULL;
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228144125_Agregar_Campos_Trazabilidad_MovimientosInventario'
)
BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'FechaCaja')
                    BEGIN
                        ALTER TABLE [MovimientosInventario] ADD [FechaCaja] datetime2 NULL;
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228144125_Agregar_Campos_Trazabilidad_MovimientosInventario'
)
BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'Turno')
                    BEGIN
                        ALTER TABLE [MovimientosInventario] ADD [Turno] int NULL;
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228144125_Agregar_Campos_Trazabilidad_MovimientosInventario'
)
BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'IdSucursal')
                    BEGIN
                        ALTER TABLE [MovimientosInventario] ADD [IdSucursal] int NULL;
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228144125_Agregar_Campos_Trazabilidad_MovimientosInventario'
)
BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'IdCaja')
                    BEGIN
                        ALTER TABLE [MovimientosInventario] ADD [IdCaja] int NULL;
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228144125_Agregar_Campos_Trazabilidad_MovimientosInventario'
)
BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'IX_MovimientosInventario_IdSucursal')
                    BEGIN
                        CREATE INDEX [IX_MovimientosInventario_IdSucursal] ON [MovimientosInventario]([IdSucursal]);
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228144125_Agregar_Campos_Trazabilidad_MovimientosInventario'
)
BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'IX_MovimientosInventario_IdCaja')
                    BEGIN
                        CREATE INDEX [IX_MovimientosInventario_IdCaja] ON [MovimientosInventario]([IdCaja]);
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228144125_Agregar_Campos_Trazabilidad_MovimientosInventario'
)
BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_MovimientosInventario_Sucursal_IdSucursal')
                    BEGIN
                        ALTER TABLE [MovimientosInventario] ADD CONSTRAINT [FK_MovimientosInventario_Sucursal_IdSucursal] 
                            FOREIGN KEY ([IdSucursal]) REFERENCES [Sucursal]([Id]);
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228144125_Agregar_Campos_Trazabilidad_MovimientosInventario'
)
BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_MovimientosInventario_Cajas_IdCaja')
                    BEGIN
                        ALTER TABLE [MovimientosInventario] ADD CONSTRAINT [FK_MovimientosInventario_Cajas_IdCaja] 
                            FOREIGN KEY ([IdCaja]) REFERENCES [Cajas]([id_caja]);
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228144125_Agregar_Campos_Trazabilidad_MovimientosInventario'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251228144125_Agregar_Campos_Trazabilidad_MovimientosInventario', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228150142_Agregar_Valorizacion_MovimientosInventario'
)
BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'PrecioCosto')
                    BEGIN
                        ALTER TABLE [MovimientosInventario] ADD [PrecioCosto] decimal(18,4) NULL;
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228150142_Agregar_Valorizacion_MovimientosInventario'
)
BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'PrecioVenta')
                    BEGIN
                        ALTER TABLE [MovimientosInventario] ADD [PrecioVenta] decimal(18,4) NULL;
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228150142_Agregar_Valorizacion_MovimientosInventario'
)
BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'IdMoneda')
                    BEGIN
                        ALTER TABLE [MovimientosInventario] ADD [IdMoneda] int NULL;
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228150142_Agregar_Valorizacion_MovimientosInventario'
)
BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'TipoCambio')
                    BEGIN
                        ALTER TABLE [MovimientosInventario] ADD [TipoCambio] decimal(18,6) NULL;
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228150142_Agregar_Valorizacion_MovimientosInventario'
)
BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'PrecioCostoGs')
                    BEGIN
                        ALTER TABLE [MovimientosInventario] ADD [PrecioCostoGs] decimal(18,4) NULL;
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228150142_Agregar_Valorizacion_MovimientosInventario'
)
BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'PrecioVentaGs')
                    BEGIN
                        ALTER TABLE [MovimientosInventario] ADD [PrecioVentaGs] decimal(18,4) NULL;
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228150142_Agregar_Valorizacion_MovimientosInventario'
)
BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'MovimientosInventario') AND name = 'IX_MovimientosInventario_IdMoneda')
                    BEGIN
                        CREATE INDEX [IX_MovimientosInventario_IdMoneda] ON [MovimientosInventario]([IdMoneda]);
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228150142_Agregar_Valorizacion_MovimientosInventario'
)
BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_MovimientosInventario_Monedas_IdMoneda')
                    BEGIN
                        ALTER TABLE [MovimientosInventario] ADD CONSTRAINT [FK_MovimientosInventario_Monedas_IdMoneda] 
                            FOREIGN KEY ([IdMoneda]) REFERENCES [Monedas]([IdMoneda]);
                    END
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228150142_Agregar_Valorizacion_MovimientosInventario'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251228150142_Agregar_Valorizacion_MovimientosInventario', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228200333_Agregar_AfectaStock_NotaCredito'
)
BEGIN
    ALTER TABLE [NotasCreditoVentas] ADD [AfectaStock] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228200333_Agregar_AfectaStock_NotaCredito'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251228200333_Agregar_AfectaStock_NotaCredito', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228205647_Crear_Tabla_RucDnit'
)
BEGIN
    CREATE TABLE [RucDnit] (
        [RUC] nvarchar(20) NOT NULL,
        [RazonSocial] nvarchar(300) NOT NULL,
        [DV] int NOT NULL,
        [Estado] nvarchar(20) NULL,
        [FechaActualizacion] datetime2 NULL,
        CONSTRAINT [PK_RucDnit] PRIMARY KEY ([RUC])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251228205647_Crear_Tabla_RucDnit'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251228205647_Crear_Tabla_RucDnit', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251229145529_Fix_TipoOperacion_StringLength'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251229145529_Fix_TipoOperacion_StringLength', N'8.0.0');
END;
GO

COMMIT;
GO

