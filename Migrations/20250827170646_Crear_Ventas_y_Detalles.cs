using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Crear_Ventas_y_Detalles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
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
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientesPrecios");
        }
    }
}
