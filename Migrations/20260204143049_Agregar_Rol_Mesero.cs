using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Rol_Mesero : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Crear rol Mesero con permisos limitados al sistema de mesas
            migrationBuilder.Sql(@"
                DECLARE @IdRolMesero INT;

                -- 1. Crear el rol Mesero si no existe
                IF NOT EXISTS (SELECT 1 FROM Rol WHERE NombreRol = 'Mesero')
                BEGIN
                    INSERT INTO Rol (NombreRol, Descripcion, Estado)
                    VALUES ('Mesero', 'Rol para meseros - Acceso limitado al panel de mesas y pedidos', 1);
                    SET @IdRolMesero = SCOPE_IDENTITY();
                END
                ELSE
                BEGIN
                    SELECT @IdRolMesero = Id_Rol FROM Rol WHERE NombreRol = 'Mesero';
                END

                -- 2. Asignar permisos al Panel de Mesas (VIEW, CREATE, EDIT)
                DECLARE @IdPanelMesas INT = (SELECT IdModulo FROM Modulos WHERE RutaPagina = '/mesas/panel');
                IF @IdPanelMesas IS NOT NULL
                BEGIN
                    -- VIEW (IdPermiso = 1)
                    IF NOT EXISTS (SELECT 1 FROM RolesModulosPermisos WHERE IdRol = @IdRolMesero AND IdModulo = @IdPanelMesas AND IdPermiso = 1)
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion)
                        VALUES (@IdRolMesero, @IdPanelMesas, 1, 1, GETDATE());
                    
                    -- CREATE (IdPermiso = 2)
                    IF NOT EXISTS (SELECT 1 FROM RolesModulosPermisos WHERE IdRol = @IdRolMesero AND IdModulo = @IdPanelMesas AND IdPermiso = 2)
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion)
                        VALUES (@IdRolMesero, @IdPanelMesas, 2, 1, GETDATE());
                    
                    -- EDIT (IdPermiso = 3)
                    IF NOT EXISTS (SELECT 1 FROM RolesModulosPermisos WHERE IdRol = @IdRolMesero AND IdModulo = @IdPanelMesas AND IdPermiso = 3)
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion)
                        VALUES (@IdRolMesero, @IdPanelMesas, 3, 1, GETDATE());
                END

                -- 3. Asignar permisos al Pedido de Mesa (VIEW, CREATE, EDIT)
                DECLARE @IdPedidoMesa INT = (SELECT IdModulo FROM Modulos WHERE RutaPagina = '/mesas/pedido');
                IF @IdPedidoMesa IS NOT NULL
                BEGIN
                    -- VIEW
                    IF NOT EXISTS (SELECT 1 FROM RolesModulosPermisos WHERE IdRol = @IdRolMesero AND IdModulo = @IdPedidoMesa AND IdPermiso = 1)
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion)
                        VALUES (@IdRolMesero, @IdPedidoMesa, 1, 1, GETDATE());
                    
                    -- CREATE
                    IF NOT EXISTS (SELECT 1 FROM RolesModulosPermisos WHERE IdRol = @IdRolMesero AND IdModulo = @IdPedidoMesa AND IdPermiso = 2)
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion)
                        VALUES (@IdRolMesero, @IdPedidoMesa, 2, 1, GETDATE());
                    
                    -- EDIT
                    IF NOT EXISTS (SELECT 1 FROM RolesModulosPermisos WHERE IdRol = @IdRolMesero AND IdModulo = @IdPedidoMesa AND IdPermiso = 3)
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion)
                        VALUES (@IdRolMesero, @IdPedidoMesa, 3, 1, GETDATE());
                END

                -- 4. Asignar permiso VIEW al módulo padre 'Mesas / Canchas' para que aparezca en el menú
                DECLARE @IdModuloPadre INT = (SELECT IdModulo FROM Modulos WHERE Nombre = 'Mesas / Canchas' AND IdModuloPadre IS NULL);
                IF @IdModuloPadre IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM RolesModulosPermisos WHERE IdRol = @IdRolMesero AND IdModulo = @IdModuloPadre AND IdPermiso = 1)
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion)
                        VALUES (@IdRolMesero, @IdModuloPadre, 1, 1, GETDATE());
                END

                -- 5. Asignar permiso VIEW a Pantalla Cocina (para que vea los pedidos)
                DECLARE @IdPantallaCocina INT = (SELECT IdModulo FROM Modulos WHERE RutaPagina = '/cocina');
                IF @IdPantallaCocina IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM RolesModulosPermisos WHERE IdRol = @IdRolMesero AND IdModulo = @IdPantallaCocina AND IdPermiso = 1)
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion)
                        VALUES (@IdRolMesero, @IdPantallaCocina, 1, 1, GETDATE());
                END

                PRINT 'Rol Mesero creado con permisos para Panel de Mesas, Pedido de Mesa y Pantalla Cocina';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DECLARE @IdRolMesero INT = (SELECT Id_Rol FROM Rol WHERE NombreRol = 'Mesero');
                IF @IdRolMesero IS NOT NULL
                BEGIN
                    -- Eliminar permisos asignados al rol
                    DELETE FROM RolesModulosPermisos WHERE IdRol = @IdRolMesero;
                    
                    -- Eliminar el rol solo si no hay usuarios asignados
                    IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Id_Rol = @IdRolMesero)
                        DELETE FROM Rol WHERE Id_Rol = @IdRolMesero;
                END
            ");
        }
    }
}
