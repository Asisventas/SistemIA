using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Modulo_CloudSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insertar módulo CloudSync si no existe
            // IdModuloPadre = 8 corresponde a "Configuración"
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM Modulos WHERE RutaPagina = '/configuracion/cloudsync')
                BEGIN
                    INSERT INTO Modulos (Nombre, Descripcion, RutaPagina, Icono, IdModuloPadre, Orden, Activo, FechaCreacion)
                    VALUES ('CloudSync (Backups)', 'Gestión de copias de seguridad en la nube', 
                            '/configuracion/cloudsync', 'bi-cloud-arrow-up', 8, 11, 1, GETDATE());
                    
                    DECLARE @NuevoIdModulo INT = SCOPE_IDENTITY();
                    
                    -- Agregar permisos VIEW, CREATE, EDIT, DELETE al rol Admin (IdRol=1)
                    IF NOT EXISTS (SELECT 1 FROM RolesModulosPermisos WHERE IdRol = 1 AND IdModulo = @NuevoIdModulo AND IdPermiso = 1)
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion)
                        VALUES (1, @NuevoIdModulo, 1, 1, GETDATE());  -- VIEW
                    
                    IF NOT EXISTS (SELECT 1 FROM RolesModulosPermisos WHERE IdRol = 1 AND IdModulo = @NuevoIdModulo AND IdPermiso = 2)
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion)
                        VALUES (1, @NuevoIdModulo, 2, 1, GETDATE());  -- CREATE
                    
                    IF NOT EXISTS (SELECT 1 FROM RolesModulosPermisos WHERE IdRol = 1 AND IdModulo = @NuevoIdModulo AND IdPermiso = 3)
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion)
                        VALUES (1, @NuevoIdModulo, 3, 1, GETDATE());  -- EDIT
                    
                    IF NOT EXISTS (SELECT 1 FROM RolesModulosPermisos WHERE IdRol = 1 AND IdModulo = @NuevoIdModulo AND IdPermiso = 4)
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion)
                        VALUES (1, @NuevoIdModulo, 4, 1, GETDATE());  -- DELETE
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Eliminar permisos y módulo CloudSync
            migrationBuilder.Sql(@"
                DECLARE @IdModulo INT = (SELECT IdModulo FROM Modulos WHERE RutaPagina = '/configuracion/cloudsync');
                IF @IdModulo IS NOT NULL
                BEGIN
                    DELETE FROM RolesModulosPermisos WHERE IdModulo = @IdModulo;
                    DELETE FROM Modulos WHERE IdModulo = @IdModulo;
                END
            ");
        }
    }
}
