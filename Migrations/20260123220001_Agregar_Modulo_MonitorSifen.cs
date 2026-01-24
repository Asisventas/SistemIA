using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Modulo_MonitorSifen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Agregar módulo Monitor SIFEN como submódulo de Configuración
            // Esto permite que aparezca en la matriz de permisos y pueda asignarse a roles/usuarios
            //
            // El módulo MonitorSifen ya existe como página en Pages/MonitorSifen.razor
            // con protección PageProtection que usa el módulo /configuracion
            //
            // Datos del módulo:
            // - Nombre: Monitor SIFEN
            // - Descripción: Monitor de cola y documentos electrónicos SIFEN
            // - RutaPagina: /monitor-sifen
            // - Icono: bi-broadcast-pin (icono de broadcast/monitor)
            // - IdModuloPadre: 8 (Configuración)
            // - Orden: 10 (después de los demás submódulos de Configuración)
            
            migrationBuilder.Sql(@"
                -- Insertar módulo Monitor SIFEN si no existe
                IF NOT EXISTS (SELECT 1 FROM Modulos WHERE RutaPagina = '/monitor-sifen')
                BEGIN
                    INSERT INTO Modulos (Nombre, Descripcion, RutaPagina, Icono, IdModuloPadre, Orden, Activo, FechaCreacion)
                    VALUES (
                        'Monitor SIFEN',
                        'Monitor de cola y documentos electrónicos SIFEN',
                        '/monitor-sifen',
                        'bi-broadcast-pin',
                        8,  -- IdModuloPadre = Configuración
                        10, -- Orden después de los demás submódulos
                        1,  -- Activo
                        GETDATE()
                    );
                    
                    -- Agregar permisos al rol Admin (IdRol = 1) para el nuevo módulo
                    -- Tabla RolesModulosPermisos: IdPermiso 1=VIEW, 2=CREATE, 3=EDIT, 4=DELETE
                    DECLARE @NuevoIdModulo INT = SCOPE_IDENTITY();
                    
                    IF NOT EXISTS (SELECT 1 FROM RolesModulosPermisos WHERE IdRol = 1 AND IdModulo = @NuevoIdModulo AND IdPermiso = 1)
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion)
                        VALUES (1, @NuevoIdModulo, 1, 1, GETDATE()); -- VIEW
                    
                    IF NOT EXISTS (SELECT 1 FROM RolesModulosPermisos WHERE IdRol = 1 AND IdModulo = @NuevoIdModulo AND IdPermiso = 2)
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion)
                        VALUES (1, @NuevoIdModulo, 2, 1, GETDATE()); -- CREATE
                    
                    IF NOT EXISTS (SELECT 1 FROM RolesModulosPermisos WHERE IdRol = 1 AND IdModulo = @NuevoIdModulo AND IdPermiso = 3)
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion)
                        VALUES (1, @NuevoIdModulo, 3, 1, GETDATE()); -- EDIT
                    
                    IF NOT EXISTS (SELECT 1 FROM RolesModulosPermisos WHERE IdRol = 1 AND IdModulo = @NuevoIdModulo AND IdPermiso = 4)
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion)
                        VALUES (1, @NuevoIdModulo, 4, 1, GETDATE()); -- DELETE
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Eliminar permisos y módulo en orden inverso
            migrationBuilder.Sql(@"
                -- Obtener el IdModulo del Monitor SIFEN
                DECLARE @IdModulo INT = (SELECT IdModulo FROM Modulos WHERE RutaPagina = '/monitor-sifen');
                
                IF @IdModulo IS NOT NULL
                BEGIN
                    -- Eliminar permisos de rol asociados (tabla RolesModulosPermisos)
                    DELETE FROM RolesModulosPermisos WHERE IdModulo = @IdModulo;
                    
                    -- Eliminar el módulo
                    DELETE FROM Modulos WHERE IdModulo = @IdModulo;
                END
            ");
        }
    }
}
