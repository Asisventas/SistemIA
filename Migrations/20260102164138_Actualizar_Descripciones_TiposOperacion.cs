using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Actualizar_Descripciones_TiposOperacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Actualizar descripciones de TiposOperacion
            // B2C NO es "Consumidor Final", es "Empresa a Cliente"
            // La regla es: RUC >= 50,000,000 = B2B (empresas/extranjeros), RUC < 50,000,000 = B2C (clientes)
            
            migrationBuilder.Sql(@"
                UPDATE TiposOperacion 
                SET Descripcion = 'B2B - Empresa a Empresa/Extranjero', 
                    Comentario = 'RUC >= 50.000.000 (Empresas y Extranjeros)'
                WHERE Codigo = '1';
                
                UPDATE TiposOperacion 
                SET Descripcion = 'B2C - Empresa a Cliente', 
                    Comentario = 'RUC < 50.000.000 (Personas Físicas)'
                WHERE Codigo = '2';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revertir a los valores anteriores
            migrationBuilder.Sql(@"
                UPDATE TiposOperacion 
                SET Descripcion = 'B2B - Empresa a Empresa', 
                    Comentario = NULL
                WHERE Codigo = '1';
                
                UPDATE TiposOperacion 
                SET Descripcion = 'B2C - Empresa a Consumidor Final', 
                    Comentario = NULL
                WHERE Codigo = '2';
            ");
        }
    }
}
