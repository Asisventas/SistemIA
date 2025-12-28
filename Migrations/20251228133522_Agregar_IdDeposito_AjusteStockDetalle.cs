using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_IdDeposito_AjusteStockDetalle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Primero verificar si existe la columna sombra y eliminarla
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_AjustesStockDetalles_AjustesStock_AjusteStockIdAjusteStock')
                BEGIN
                    ALTER TABLE [AjustesStockDetalles] DROP CONSTRAINT [FK_AjustesStockDetalles_AjustesStock_AjusteStockIdAjusteStock];
                END
            ");
            
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AjustesStockDetalles_AjusteStockIdAjusteStock')
                BEGIN
                    DROP INDEX [IX_AjustesStockDetalles_AjusteStockIdAjusteStock] ON [AjustesStockDetalles];
                END
            ");
            
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'AjusteStockIdAjusteStock' AND Object_ID = Object_ID(N'AjustesStockDetalles'))
                BEGIN
                    ALTER TABLE [AjustesStockDetalles] DROP COLUMN [AjusteStockIdAjusteStock];
                END
            ");

            // Agregar columna IdDeposito con valor por defecto 1 (Depósito Principal)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'IdDeposito' AND Object_ID = Object_ID(N'AjustesStockDetalles'))
                BEGIN
                    ALTER TABLE [AjustesStockDetalles] ADD [IdDeposito] int NOT NULL DEFAULT 1;
                END
            ");

            // Crear índice
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AjustesStockDetalles_IdDeposito')
                BEGIN
                    CREATE INDEX [IX_AjustesStockDetalles_IdDeposito] ON [AjustesStockDetalles] ([IdDeposito]);
                END
            ");

            // Agregar FK a Depositos
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_AjustesStockDetalles_Depositos_IdDeposito')
                BEGIN
                    ALTER TABLE [AjustesStockDetalles] ADD CONSTRAINT [FK_AjustesStockDetalles_Depositos_IdDeposito] 
                        FOREIGN KEY ([IdDeposito]) REFERENCES [Depositos] ([IdDeposito]) ON DELETE NO ACTION;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AjustesStockDetalles_Depositos_IdDeposito",
                table: "AjustesStockDetalles");

            migrationBuilder.DropIndex(
                name: "IX_AjustesStockDetalles_IdDeposito",
                table: "AjustesStockDetalles");

            migrationBuilder.DropColumn(
                name: "IdDeposito",
                table: "AjustesStockDetalles");
        }
    }
}
