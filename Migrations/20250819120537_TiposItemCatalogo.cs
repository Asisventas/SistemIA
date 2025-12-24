using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class TiposItemCatalogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TiposItem",
                columns: table => new
                {
                    IdTipoItem = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EsGasto = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposItem", x => x.IdTipoItem);
                });

            // Sembrar datos básicos para no romper la FK con productos existentes (1=Producto, 2=Servicio, 3=Gasto)
            migrationBuilder.Sql(@"
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
");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_TipoItem",
                table: "Productos",
                column: "TipoItem");

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_TiposItem_TipoItem",
                table: "Productos",
                column: "TipoItem",
                principalTable: "TiposItem",
                principalColumn: "IdTipoItem",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Productos_TiposItem_TipoItem",
                table: "Productos");

            migrationBuilder.DropTable(
                name: "TiposItem");

            migrationBuilder.DropIndex(
                name: "IX_Productos_TipoItem",
                table: "Productos");
        }
    }
}
