using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_ControladoReceta_Y_RecetasVentas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Agregar columna ControladoReceta solo si no existe
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Productos', 'ControladoReceta') IS NULL
                BEGIN
                    ALTER TABLE [Productos] ADD [ControladoReceta] bit NOT NULL DEFAULT CAST(0 AS bit);
                END
            ");

            // Crear tabla RecetasVentas solo si no existe
            migrationBuilder.Sql(@"
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
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF OBJECT_ID('RecetasVentas', 'U') IS NOT NULL DROP TABLE [RecetasVentas];");
            
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Productos', 'ControladoReceta') IS NOT NULL
                BEGIN
                    ALTER TABLE [Productos] DROP COLUMN [ControladoReceta];
                END
            ");
        }
    }
}
