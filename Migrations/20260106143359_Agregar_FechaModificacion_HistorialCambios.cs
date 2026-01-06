using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_FechaModificacion_HistorialCambios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Crear tabla ConversacionesIAHistorial si no existe
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ConversacionesIAHistorial')
                BEGIN
                    CREATE TABLE [ConversacionesIAHistorial] (
                        [IdConversacionIA] int NOT NULL IDENTITY,
                        [FechaInicio] datetime2 NOT NULL,
                        [FechaFin] datetime2 NULL,
                        [ModeloIA] nvarchar(50) NOT NULL,
                        [Titulo] nvarchar(200) NOT NULL,
                        [ResumenEjecutivo] nvarchar(max) NOT NULL DEFAULT '',
                        [ObjetivosSesion] nvarchar(max) NULL,
                        [ResultadosObtenidos] nvarchar(max) NULL,
                        [TareasPendientes] nvarchar(max) NULL,
                        [ModulosTrabajados] nvarchar(max) NULL,
                        [ArchivosCreados] nvarchar(max) NULL,
                        [ArchivosModificados] nvarchar(max) NULL,
                        [MigracionesGeneradas] nvarchar(max) NULL,
                        [ProblemasResoluciones] nvarchar(max) NULL,
                        [DecisionesTecnicas] nvarchar(max) NULL,
                        [Etiquetas] nvarchar(500) NULL,
                        [Complejidad] nvarchar(20) NOT NULL DEFAULT 'Moderado',
                        [DuracionMinutos] int NULL,
                        [CantidadCambios] int NOT NULL DEFAULT 0,
                        [FechaCreacion] datetime2 NOT NULL DEFAULT GETDATE(),
                        [FechaModificacion] datetime2 NULL,
                        CONSTRAINT [PK_ConversacionesIAHistorial] PRIMARY KEY ([IdConversacionIA])
                    );
                END
            ");

            // Crear tabla HistorialCambiosSistema si no existe
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'HistorialCambiosSistema')
                BEGIN
                    CREATE TABLE [HistorialCambiosSistema] (
                        [IdHistorialCambio] int NOT NULL IDENTITY,
                        [Version] nvarchar(20) NULL,
                        [FechaCambio] datetime2 NOT NULL,
                        [TituloCambio] nvarchar(100) NOT NULL,
                        [Tema] nvarchar(100) NULL,
                        [TipoCambio] nvarchar(50) NOT NULL,
                        [ModuloAfectado] nvarchar(50) NULL,
                        [Prioridad] nvarchar(20) NOT NULL DEFAULT 'Media',
                        [Tags] nvarchar(500) NULL,
                        [Referencias] nvarchar(500) NULL,
                        [DescripcionBreve] nvarchar(500) NOT NULL,
                        [DescripcionTecnica] nvarchar(max) NULL,
                        [ArchivosModificados] nvarchar(max) NULL,
                        [Notas] nvarchar(max) NULL,
                        [ImplementadoPor] nvarchar(100) NULL,
                        [ReferenciaTicket] nvarchar(50) NULL,
                        [IdConversacionIA] int NULL,
                        [Estado] nvarchar(30) NOT NULL DEFAULT 'Implementado',
                        [RequiereMigracion] bit NOT NULL DEFAULT 0,
                        [NombreMigracion] nvarchar(100) NULL,
                        [FechaCreacion] datetime2 NOT NULL DEFAULT GETDATE(),
                        [FechaModificacion] datetime2 NULL,
                        CONSTRAINT [PK_HistorialCambiosSistema] PRIMARY KEY ([IdHistorialCambio]),
                        CONSTRAINT [FK_HistorialCambiosSistema_ConversacionesIAHistorial_IdConversacionIA] 
                            FOREIGN KEY ([IdConversacionIA]) REFERENCES [ConversacionesIAHistorial] ([IdConversacionIA])
                    );
                    CREATE INDEX [IX_HistorialCambiosSistema_IdConversacionIA] ON [HistorialCambiosSistema] ([IdConversacionIA]);
                END
            ");

            // Agregar columna FechaModificacion a HistorialCambiosSistema si no existe
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('HistorialCambiosSistema') AND name = 'FechaModificacion')
                BEGIN
                    ALTER TABLE [HistorialCambiosSistema] ADD [FechaModificacion] datetime2 NULL;
                END
            ");

            // Agregar columna FechaModificacion a ConversacionesIAHistorial si no existe
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ConversacionesIAHistorial') AND name = 'FechaModificacion')
                BEGIN
                    ALTER TABLE [ConversacionesIAHistorial] ADD [FechaModificacion] datetime2 NULL;
                END
            ");

            // Crear índice si no existe
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_HistorialCambiosSistema_IdConversacionIA' AND object_id = OBJECT_ID('HistorialCambiosSistema'))
                BEGIN
                    CREATE INDEX [IX_HistorialCambiosSistema_IdConversacionIA] ON [HistorialCambiosSistema] ([IdConversacionIA]);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Eliminar columna FechaModificacion si existe
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('HistorialCambiosSistema') AND name = 'FechaModificacion')
                BEGIN
                    ALTER TABLE [HistorialCambiosSistema] DROP COLUMN [FechaModificacion];
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ConversacionesIAHistorial') AND name = 'FechaModificacion')
                BEGIN
                    ALTER TABLE [ConversacionesIAHistorial] DROP COLUMN [FechaModificacion];
                END
            ");
        }
    }
}
