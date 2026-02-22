using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Campos_Membresia_Faltantes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Agregar columnas faltantes de membresía (con SQL condicional para BD existentes)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Productos') AND name = 'AccesoTodasAreasMembresia')
                BEGIN
                    ALTER TABLE [Productos] ADD [AccesoTodasAreasMembresia] bit NOT NULL DEFAULT 1;
                END
            ");
            
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Productos') AND name = 'AreasIncluidasMembresia')
                BEGIN
                    ALTER TABLE [Productos] ADD [AreasIncluidasMembresia] nvarchar(200) NULL;
                END
            ");
            
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Productos') AND name = 'DiasRecordatorioMembresia')
                BEGIN
                    ALTER TABLE [Productos] ADD [DiasRecordatorioMembresia] int NULL;
                END
            ");
            
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Productos') AND name = 'RenovacionAutomaticaMembresia')
                BEGIN
                    ALTER TABLE [Productos] ADD [RenovacionAutomaticaMembresia] bit NOT NULL DEFAULT 0;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "AccesoTodasAreasMembresia", table: "Productos");
            migrationBuilder.DropColumn(name: "AreasIncluidasMembresia", table: "Productos");
            migrationBuilder.DropColumn(name: "DiasRecordatorioMembresia", table: "Productos");
            migrationBuilder.DropColumn(name: "RenovacionAutomaticaMembresia", table: "Productos");
        }
    }
}
