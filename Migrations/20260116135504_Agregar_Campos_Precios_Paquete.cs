using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Campos_Precios_Paquete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Migración IDEMPOTENTE: Solo agrega columnas si NO existen
            // Esto permite que funcione tanto en BD nuevas como existentes

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Productos') AND name = 'CostoPaqueteGs')
                BEGIN
                    ALTER TABLE [Productos] ADD [CostoPaqueteGs] decimal(18,4) NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Productos') AND name = 'FactorPaquete')
                BEGIN
                    ALTER TABLE [Productos] ADD [FactorPaquete] decimal(18,4) NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Productos') AND name = 'MarkupPaquetePct')
                BEGIN
                    ALTER TABLE [Productos] ADD [MarkupPaquetePct] decimal(5,2) NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Productos') AND name = 'PrecioMinisterioPaquete')
                BEGIN
                    ALTER TABLE [Productos] ADD [PrecioMinisterioPaquete] decimal(18,4) NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Productos') AND name = 'PrecioPaqueteGs')
                BEGIN
                    ALTER TABLE [Productos] ADD [PrecioPaqueteGs] decimal(18,4) NULL;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CostoPaqueteGs",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "FactorPaquete",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "MarkupPaquetePct",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "PrecioMinisterioPaquete",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "PrecioPaqueteGs",
                table: "Productos");
        }
    }
}
