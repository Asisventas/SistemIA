using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Eliminar_UnidadesMedida_Excepto_Unidad_Paquete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Primero, actualizar productos que usan unidades a eliminar para que usen "Unidad" (77)
            // Actualizamos el código SIFEN a 77 (Unidad) para productos que usan códigos que serán eliminados
            migrationBuilder.Sql(@"
                -- Actualizar productos que usan Caja (005), Kilogramo (003), Metro (001), Litro (002)
                -- para que usen Unidad (077)
                UPDATE Productos 
                SET UnidadMedidaCodigo = '077'
                WHERE UnidadMedidaCodigo IN ('001', '002', '003', '005')
            ");

            // Eliminar unidades de medida excepto Unidad (IdUnidadMedida=1) y Paquete (IdUnidadMedida=7)
            // Nombres según tabla: Kilogramo(2), Gramo(3), Litro(4), Metro(5), Caja(6), Docena(8)
            migrationBuilder.Sql(@"
                DELETE FROM UnidadesMedida 
                WHERE IdUnidadMedida NOT IN (1, 7)
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restaurar las unidades de medida eliminadas
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT UnidadesMedida ON;
                
                IF NOT EXISTS (SELECT 1 FROM UnidadesMedida WHERE IdUnidadMedida = 2)
                    INSERT INTO UnidadesMedida (IdUnidadMedida, Nombre, Abreviatura, CodigoSifen, Activo) 
                    VALUES (2, 'Kilogramo', 'KG', 83, 1);
                
                IF NOT EXISTS (SELECT 1 FROM UnidadesMedida WHERE IdUnidadMedida = 3)
                    INSERT INTO UnidadesMedida (IdUnidadMedida, Nombre, Abreviatura, CodigoSifen, Activo) 
                    VALUES (3, 'Gramo', 'GR', 85, 1);
                
                IF NOT EXISTS (SELECT 1 FROM UnidadesMedida WHERE IdUnidadMedida = 4)
                    INSERT INTO UnidadesMedida (IdUnidadMedida, Nombre, Abreviatura, CodigoSifen, Activo) 
                    VALUES (4, 'Litro', 'LT', 79, 1);
                
                IF NOT EXISTS (SELECT 1 FROM UnidadesMedida WHERE IdUnidadMedida = 5)
                    INSERT INTO UnidadesMedida (IdUnidadMedida, Nombre, Abreviatura, CodigoSifen, Activo) 
                    VALUES (5, 'Metro', 'MT', 86, 1);
                
                IF NOT EXISTS (SELECT 1 FROM UnidadesMedida WHERE IdUnidadMedida = 6)
                    INSERT INTO UnidadesMedida (IdUnidadMedida, Nombre, Abreviatura, CodigoSifen, Activo) 
                    VALUES (6, 'Caja', 'CJ', 77, 1);
                
                IF NOT EXISTS (SELECT 1 FROM UnidadesMedida WHERE IdUnidadMedida = 8)
                    INSERT INTO UnidadesMedida (IdUnidadMedida, Nombre, Abreviatura, CodigoSifen, Activo) 
                    VALUES (8, 'Docena', 'DOC', 77, 1);
                
                SET IDENTITY_INSERT UnidadesMedida OFF;
            ");
        }
    }
}
