using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Eliminar_PlanMembresia_Usar_Producto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NOTA: El modelo ya fue actualizado (IdPlan eliminado, IdProducto requerido)
            // pero la BD todavía tiene la estructura anterior.
            // Usamos SQL condicional para que funcione tanto en BD nuevas como existentes.
            
            // 1. Eliminar FK de IdPlan si existe
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_MembresiasClientes_PlanesMembresia_IdPlan')
                BEGIN
                    ALTER TABLE [MembresiasClientes] DROP CONSTRAINT [FK_MembresiasClientes_PlanesMembresia_IdPlan];
                END
            ");
            
            // 2. Eliminar índice de IdPlan si existe
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MembresiasClientes_IdPlan' AND object_id = OBJECT_ID('MembresiasClientes'))
                BEGIN
                    DROP INDEX [IX_MembresiasClientes_IdPlan] ON [MembresiasClientes];
                END
            ");
            
            // 3. Eliminar columna IdPlan si existe
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MembresiasClientes') AND name = 'IdPlan')
                BEGIN
                    ALTER TABLE [MembresiasClientes] DROP COLUMN [IdPlan];
                END
            ");
            
            // 4. Eliminar tabla PlanesMembresia si existe
            migrationBuilder.Sql(@"
                IF OBJECT_ID('PlanesMembresia', 'U') IS NOT NULL
                BEGIN
                    DROP TABLE [PlanesMembresia];
                END
            ");
            
            // 5. Asegurar que IdProducto sea NOT NULL (si hay nulls, establecer un valor por defecto primero)
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MembresiasClientes') AND name = 'IdProducto' AND is_nullable = 1)
                BEGIN
                    -- Primero eliminar cualquier registro sin IdProducto (no debería haber, ya limpiamos)
                    DELETE FROM [MembresiasClientes] WHERE [IdProducto] IS NULL;
                    
                    -- Eliminar FK existente de IdProducto si existe
                    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_MembresiasClientes_Productos_IdProducto')
                    BEGIN
                        ALTER TABLE [MembresiasClientes] DROP CONSTRAINT [FK_MembresiasClientes_Productos_IdProducto];
                    END
                    
                    -- Eliminar índice existente de IdProducto si existe
                    IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MembresiasClientes_IdProducto' AND object_id = OBJECT_ID('MembresiasClientes'))
                    BEGIN
                        DROP INDEX [IX_MembresiasClientes_IdProducto] ON [MembresiasClientes];
                    END
                    
                    -- Cambiar columna a NOT NULL
                    ALTER TABLE [MembresiasClientes] ALTER COLUMN [IdProducto] int NOT NULL;
                    
                    -- Recrear índice
                    CREATE INDEX [IX_MembresiasClientes_IdProducto] ON [MembresiasClientes] ([IdProducto]);
                    
                    -- Recrear FK con CASCADE DELETE
                    ALTER TABLE [MembresiasClientes] ADD CONSTRAINT [FK_MembresiasClientes_Productos_IdProducto] 
                        FOREIGN KEY ([IdProducto]) REFERENCES [Productos] ([IdProducto]) ON DELETE CASCADE;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // El Down es complejo porque requeriría recrear PlanesMembresia.
            // Para este caso, no implementamos rollback ya que es una migración destructiva intencional.
            // Si se necesita revertir, restaurar backup de la BD.
        }
    }
}
