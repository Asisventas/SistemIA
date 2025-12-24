using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Factor_Compras_Producto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Usar SQL condicional para evitar errores si las columnas ya existen
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Productos') AND name = 'FactorMultiplicador')
                BEGIN
                    ALTER TABLE [Productos] ADD [FactorMultiplicador] decimal(18,4) NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'ComprasDetalles') AND name = 'FactorMultiplicador')
                BEGIN
                    ALTER TABLE [ComprasDetalles] ADD [FactorMultiplicador] decimal(18,4) NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'ComprasDetalles') AND name = 'PorcentajeMargen')
                BEGIN
                    ALTER TABLE [ComprasDetalles] ADD [PorcentajeMargen] decimal(18,4) NULL;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FactorMultiplicador",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "FactorMultiplicador",
                table: "ComprasDetalles");

            migrationBuilder.DropColumn(
                name: "PorcentajeMargen",
                table: "ComprasDetalles");
        }
    }
}
