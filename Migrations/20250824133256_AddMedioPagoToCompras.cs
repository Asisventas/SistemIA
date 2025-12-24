using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class AddMedioPagoToCompras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Agregar MedioPago a Compras si no existe
            migrationBuilder.Sql(@"IF COL_LENGTH('Compras','MedioPago') IS NULL ALTER TABLE [Compras] ADD [MedioPago] NVARCHAR(13) NULL;");
            // Setear valor por defecto donde esté nulo o vacío (ejecuta en un batch separado)
            migrationBuilder.Sql(@"IF COL_LENGTH('Compras','MedioPago') IS NOT NULL UPDATE [Compras] SET [MedioPago] = 'EFECTIVO' WHERE [MedioPago] IS NULL OR LTRIM(RTRIM([MedioPago])) = ''; ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revertir de forma segura (solo si existe)
            migrationBuilder.Sql(@"
IF COL_LENGTH('Compras','MedioPago') IS NOT NULL
    ALTER TABLE [Compras] DROP COLUMN [MedioPago];
");
        }
    }
}
