using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class CrearTablaSociedades_Fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Asegura que el índice exista antes de intentar eliminarlo (evita error 3701)
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.indexes i
    WHERE i.name = N'IX_Presupuestos_suc'
      AND i.object_id = OBJECT_ID(N'[dbo].[Presupuestos]')
)
BEGIN
    DROP INDEX [IX_Presupuestos_suc] ON [dbo].[Presupuestos];
END
");

            // Crear índice en IdVentaConvertida si no existe
            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes i
    WHERE i.name = N'IX_Presupuestos_IdVentaConvertida'
      AND i.object_id = OBJECT_ID(N'[dbo].[Presupuestos]')
)
BEGIN
    CREATE INDEX [IX_Presupuestos_IdVentaConvertida] ON [dbo].[Presupuestos] ([IdVentaConvertida]);
END
");

            // Crear índice compuesto suc + NumeroPresupuesto si no existe
            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes i
    WHERE i.name = N'IX_Presupuestos_suc_NumeroPresupuesto'
      AND i.object_id = OBJECT_ID(N'[dbo].[Presupuestos]')
)
BEGIN
    CREATE INDEX [IX_Presupuestos_suc_NumeroPresupuesto] ON [dbo].[Presupuestos] ([suc], [NumeroPresupuesto]);
END
");

            // Agregar FK si no existe
            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys fk
    WHERE fk.name = N'FK_Presupuestos_Ventas_IdVentaConvertida'
      AND fk.parent_object_id = OBJECT_ID(N'[dbo].[Presupuestos]')
)
BEGIN
    ALTER TABLE [dbo].[Presupuestos] WITH CHECK ADD CONSTRAINT [FK_Presupuestos_Ventas_IdVentaConvertida]
    FOREIGN KEY([IdVentaConvertida]) REFERENCES [dbo].[Ventas] ([IdVenta]) ON DELETE SET NULL;
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Presupuestos_Ventas_IdVentaConvertida",
                table: "Presupuestos");

            migrationBuilder.DropIndex(
                name: "IX_Presupuestos_IdVentaConvertida",
                table: "Presupuestos");

            migrationBuilder.DropIndex(
                name: "IX_Presupuestos_suc_NumeroPresupuesto",
                table: "Presupuestos");

            migrationBuilder.CreateIndex(
                name: "IX_Presupuestos_suc",
                table: "Presupuestos",
                column: "suc");
        }
    }
}
