using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Modulo_LibroIVA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Agregar módulo Libro IVA como submódulo de Reportes (idempotente)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM Modulos WHERE RutaPagina = '/informes/libro-iva')
                BEGIN
                    DECLARE @IdModuloPadre INT = (SELECT IdModulo FROM Modulos WHERE RutaPagina = '/reportes' AND IdModuloPadre IS NULL);
                    
                    INSERT INTO Modulos (Nombre, Descripcion, RutaPagina, Icono, IdModuloPadre, Orden, Activo, FechaCreacion)
                    VALUES (
                        'Libro IVA',
                        'Libro IVA de Ventas y Compras para declaraciones fiscales SET/SIFEN',
                        '/informes/libro-iva',
                        'bi-journal-bookmark-fill',
                        @IdModuloPadre,
                        20,
                        1,
                        GETDATE()
                    );
                    
                    DECLARE @NuevoIdModulo INT = SCOPE_IDENTITY();
                    
                    -- Permisos para rol Admin (IdRol=1): VIEW=1, CREATE=2, EDIT=3, DELETE=4
                    INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion)
                    VALUES 
                        (1, @NuevoIdModulo, 1, 1, GETDATE()),
                        (1, @NuevoIdModulo, 2, 1, GETDATE()),
                        (1, @NuevoIdModulo, 3, 1, GETDATE()),
                        (1, @NuevoIdModulo, 4, 1, GETDATE());
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DECLARE @IdModulo INT = (SELECT IdModulo FROM Modulos WHERE RutaPagina = '/informes/libro-iva');
                IF @IdModulo IS NOT NULL
                BEGIN
                    DELETE FROM RolesModulosPermisos WHERE IdModulo = @IdModulo;
                    DELETE FROM Modulos WHERE IdModulo = @IdModulo;
                END
            ");
        }
    }
}
