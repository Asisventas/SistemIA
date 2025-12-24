using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class AjustesStock_CrearTablas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Columnas condicionales para evitar errores si ya existen en la BD
            migrationBuilder.Sql(@"
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

");

            migrationBuilder.CreateTable(
                name: "AjustesStock",
                columns: table => new
                {
                    IdAjusteStock = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    suc = table.Column<int>(type: "int", nullable: false),
                    IdCaja = table.Column<int>(type: "int", nullable: true),
                    Turno = table.Column<int>(type: "int", nullable: true),
                    FechaAjuste = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Comentario = table.Column<string>(type: "nvarchar(280)", maxLength: 280, nullable: true),
                    TotalMonto = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AjustesStock", x => x.IdAjusteStock);
                });

            migrationBuilder.CreateTable(
                name: "AjustesStockDetalles",
                columns: table => new
                {
                    IdAjusteStockDetalle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdAjusteStock = table.Column<int>(type: "int", nullable: false),
                    IdProducto = table.Column<int>(type: "int", nullable: false),
                    StockAjuste = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StockSistema = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Diferencia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    FechaAjuste = table.Column<DateTime>(type: "datetime2", nullable: false),
                    suc = table.Column<int>(type: "int", nullable: false),
                    IdCaja = table.Column<int>(type: "int", nullable: true),
                    Turno = table.Column<int>(type: "int", nullable: true),
                    Usuario = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AjustesStockDetalles", x => x.IdAjusteStockDetalle);
                    table.ForeignKey(
                        name: "FK_AjustesStockDetalles_AjustesStock_IdAjusteStock",
                        column: x => x.IdAjusteStock,
                        principalTable: "AjustesStock",
                        principalColumn: "IdAjusteStock",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AjustesStockDetalles_Productos_IdProducto",
                        column: x => x.IdProducto,
                        principalTable: "Productos",
                        principalColumn: "IdProducto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AjustesStockDetalles_IdAjusteStock",
                table: "AjustesStockDetalles",
                column: "IdAjusteStock");

            migrationBuilder.CreateIndex(
                name: "IX_AjustesStockDetalles_IdProducto",
                table: "AjustesStockDetalles",
                column: "IdProducto");

            // FK ya creada arriba condicionalmente (si no existía)
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Productos_Monedas_IdMonedaPrecio",
                table: "Productos");

            migrationBuilder.DropTable(
                name: "AjustesStockDetalles");

            migrationBuilder.DropTable(
                name: "AjustesStock");

            migrationBuilder.DropIndex(
                name: "IX_Productos_IdMonedaPrecio",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "IdMonedaPrecio",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "CambioDelDia",
                table: "ComprasDetalles");

            migrationBuilder.DropColumn(
                name: "MedioPago",
                table: "Compras");
        }
    }
}
