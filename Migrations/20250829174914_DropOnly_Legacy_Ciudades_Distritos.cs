using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class DropOnly_Legacy_Ciudades_Distritos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Quitar FKs legacy si existen
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Clientes_Ciudades_IdCiudad')
    ALTER TABLE [Clientes] DROP CONSTRAINT [FK_Clientes_Ciudades_IdCiudad];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Sociedades_Distritos_Distrito')
    ALTER TABLE [Sociedades] DROP CONSTRAINT [FK_Sociedades_Distritos_Distrito];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Sucursal_Ciudades_IdCiudad')
    ALTER TABLE [Sucursal] DROP CONSTRAINT [FK_Sucursal_Ciudades_IdCiudad];
");

            // 2) Eliminar tablas legacy si existen
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[Distritos]', N'U') IS NOT NULL DROP TABLE [dbo].[Distritos];
IF OBJECT_ID(N'[dbo].[Ciudades]', N'U') IS NOT NULL DROP TABLE [dbo].[Ciudades];
");

            // 3) Crear índices si no existen
            migrationBuilder.Sql(@"
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
");

            // 4) Crear FKs hacia el catálogo si no existen
            migrationBuilder.Sql(@"
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
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revertir de forma condicional (no siempre necesario). Mantener por compatibilidad.
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Clientes_ciudad_IdCiudad')
    ALTER TABLE [Clientes] DROP CONSTRAINT [FK_Clientes_ciudad_IdCiudad];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Sociedades_ciudad_Ciudad')
    ALTER TABLE [Sociedades] DROP CONSTRAINT [FK_Sociedades_ciudad_Ciudad];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Sociedades_departamento_Departamento')
    ALTER TABLE [Sociedades] DROP CONSTRAINT [FK_Sociedades_departamento_Departamento];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Sociedades_distrito_Distrito')
    ALTER TABLE [Sociedades] DROP CONSTRAINT [FK_Sociedades_distrito_Distrito];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Sucursal_ciudad_IdCiudad')
    ALTER TABLE [Sucursal] DROP CONSTRAINT [FK_Sucursal_ciudad_IdCiudad];

IF EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = N'IX_Sociedades_Ciudad' AND object_id = OBJECT_ID(N'[dbo].[Sociedades]')
)
    DROP INDEX [IX_Sociedades_Ciudad] ON [dbo].[Sociedades];

IF EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = N'IX_Sociedades_Departamento' AND object_id = OBJECT_ID(N'[dbo].[Sociedades]')
)
    DROP INDEX [IX_Sociedades_Departamento] ON [dbo].[Sociedades];

IF OBJECT_ID(N'[dbo].[Ciudades]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Ciudades](
        [IdCiudad] [int] IDENTITY(1,1) NOT NULL,
        [Departamento] [nvarchar](100) NULL,
        [Nombre] [nvarchar](100) NOT NULL,
        CONSTRAINT [PK_Ciudades] PRIMARY KEY CLUSTERED ([IdCiudad] ASC)
    );
END

IF OBJECT_ID(N'[dbo].[Distritos]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Distritos](
        [IdDistrito] [int] IDENTITY(1,1) NOT NULL,
        [IdCiudad] [int] NULL,
        [Nombre] [nvarchar](120) NOT NULL,
        CONSTRAINT [PK_Distritos] PRIMARY KEY CLUSTERED ([IdDistrito] ASC)
    );
    ALTER TABLE [dbo].[Distritos]  WITH CHECK ADD  CONSTRAINT [FK_Distritos_Ciudades_IdCiudad] FOREIGN KEY([IdCiudad])
    REFERENCES [dbo].[Ciudades] ([IdCiudad]) ON DELETE SET NULL;
    CREATE INDEX [IX_Distritos_IdCiudad] ON [dbo].[Distritos]([IdCiudad]);
END

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Clientes_Ciudades_IdCiudad')
    ALTER TABLE [Clientes]  WITH CHECK ADD  CONSTRAINT [FK_Clientes_Ciudades_IdCiudad] FOREIGN KEY([IdCiudad])
    REFERENCES [Ciudades] ([IdCiudad]) ON DELETE CASCADE;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Sociedades_Distritos_Distrito')
    ALTER TABLE [Sociedades]  WITH CHECK ADD  CONSTRAINT [FK_Sociedades_Distritos_Distrito] FOREIGN KEY([Distrito])
    REFERENCES [Distritos] ([IdDistrito]) ON DELETE SET NULL;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Sucursal_Ciudades_IdCiudad')
    ALTER TABLE [Sucursal]  WITH CHECK ADD  CONSTRAINT [FK_Sucursal_Ciudades_IdCiudad] FOREIGN KEY([IdCiudad])
    REFERENCES [Ciudades] ([IdCiudad]) ON DELETE CASCADE;
");
        }
    }
}
