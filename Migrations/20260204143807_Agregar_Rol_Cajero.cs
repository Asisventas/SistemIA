using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Rol_Cajero : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Crear rol Cajero con permisos para ventas, caja, cobros y clientes
            migrationBuilder.Sql(@"
                DECLARE @IdRolCajero INT;

                -- 1. Crear el rol Cajero si no existe
                IF NOT EXISTS (SELECT 1 FROM Rol WHERE NombreRol = 'Cajero')
                BEGIN
                    INSERT INTO Rol (NombreRol, Descripcion, Estado)
                    VALUES ('Cajero', 'Rol para cajeros - Acceso a ventas, caja, cobros y clientes', 1);
                    SET @IdRolCajero = SCOPE_IDENTITY();
                END
                ELSE
                BEGIN
                    SELECT @IdRolCajero = Id_Rol FROM Rol WHERE NombreRol = 'Cajero';
                END

                -- Helper para asignar permisos (VIEW=1, CREATE=2, EDIT=3, DELETE=4)
                -- Ventas (IdModulo=1) - Ver, Crear, Editar (sin eliminar)
                DECLARE @IdVentas INT = (SELECT IdModulo FROM Modulos WHERE RutaPagina = '/ventas');
                IF @IdVentas IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM RolesModulosPermisos WHERE IdRol = @IdRolCajero AND IdModulo = @IdVentas AND IdPermiso = 1)
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion) VALUES (@IdRolCajero, @IdVentas, 1, 1, GETDATE());
                    IF NOT EXISTS (SELECT 1 FROM RolesModulosPermisos WHERE IdRol = @IdRolCajero AND IdModulo = @IdVentas AND IdPermiso = 2)
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion) VALUES (@IdRolCajero, @IdVentas, 2, 1, GETDATE());
                    IF NOT EXISTS (SELECT 1 FROM RolesModulosPermisos WHERE IdRol = @IdRolCajero AND IdModulo = @IdVentas AND IdPermiso = 3)
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion) VALUES (@IdRolCajero, @IdVentas, 3, 1, GETDATE());
                END

                -- Historial de Ventas (IdModulo=10) - Solo Ver
                DECLARE @IdHistVentas INT = (SELECT IdModulo FROM Modulos WHERE RutaPagina = '/ventas/explorar');
                IF @IdHistVentas IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM RolesModulosPermisos WHERE IdRol = @IdRolCajero AND IdModulo = @IdHistVentas AND IdPermiso = 1)
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion) VALUES (@IdRolCajero, @IdHistVentas, 1, 1, GETDATE());
                END

                -- Cierre de Caja (IdModulo=42) - Ver, Crear, Editar
                DECLARE @IdCierreCaja INT = (SELECT IdModulo FROM Modulos WHERE RutaPagina = '/caja/cierre');
                IF @IdCierreCaja IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM RolesModulosPermisos WHERE IdRol = @IdRolCajero AND IdModulo = @IdCierreCaja AND IdPermiso = 1)
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion) VALUES (@IdRolCajero, @IdCierreCaja, 1, 1, GETDATE());
                    IF NOT EXISTS (SELECT 1 FROM RolesModulosPermisos WHERE IdRol = @IdRolCajero AND IdModulo = @IdCierreCaja AND IdPermiso = 2)
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion) VALUES (@IdRolCajero, @IdCierreCaja, 2, 1, GETDATE());
                    IF NOT EXISTS (SELECT 1 FROM RolesModulosPermisos WHERE IdRol = @IdRolCajero AND IdModulo = @IdCierreCaja AND IdPermiso = 3)
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion) VALUES (@IdRolCajero, @IdCierreCaja, 3, 1, GETDATE());
                END

                -- Historial de Cierres (IdModulo=43) - Solo Ver
                DECLARE @IdHistCierres INT = (SELECT IdModulo FROM Modulos WHERE RutaPagina = '/caja/historial-cierres');
                IF @IdHistCierres IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM RolesModulosPermisos WHERE IdRol = @IdRolCajero AND IdModulo = @IdHistCierres AND IdPermiso = 1)
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion) VALUES (@IdRolCajero, @IdHistCierres, 1, 1, GETDATE());
                END

                -- Resumen de Caja (IdModulo=44) - Solo Ver
                DECLARE @IdResumenCaja INT = (SELECT IdModulo FROM Modulos WHERE RutaPagina = '/informes/resumen-caja');
                IF @IdResumenCaja IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM RolesModulosPermisos WHERE IdRol = @IdRolCajero AND IdModulo = @IdResumenCaja AND IdPermiso = 1)
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion) VALUES (@IdRolCajero, @IdResumenCaja, 1, 1, GETDATE());
                END

                -- Clientes (IdModulo=4) - Ver, Crear, Editar (sin eliminar)
                DECLARE @IdClientes INT = (SELECT IdModulo FROM Modulos WHERE RutaPagina = '/clientes');
                IF @IdClientes IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM RolesModulosPermisos WHERE IdRol = @IdRolCajero AND IdModulo = @IdClientes AND IdPermiso = 1)
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion) VALUES (@IdRolCajero, @IdClientes, 1, 1, GETDATE());
                    IF NOT EXISTS (SELECT 1 FROM RolesModulosPermisos WHERE IdRol = @IdRolCajero AND IdModulo = @IdClientes AND IdPermiso = 2)
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion) VALUES (@IdRolCajero, @IdClientes, 2, 1, GETDATE());
                    IF NOT EXISTS (SELECT 1 FROM RolesModulosPermisos WHERE IdRol = @IdRolCajero AND IdModulo = @IdClientes AND IdPermiso = 3)
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion) VALUES (@IdRolCajero, @IdClientes, 3, 1, GETDATE());
                END

                -- Historial de Clientes (IdModulo=22) - Solo Ver
                DECLARE @IdHistClientes INT = (SELECT IdModulo FROM Modulos WHERE RutaPagina = '/clientes/explorar');
                IF @IdHistClientes IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM RolesModulosPermisos WHERE IdRol = @IdRolCajero AND IdModulo = @IdHistClientes AND IdPermiso = 1)
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion) VALUES (@IdRolCajero, @IdHistClientes, 1, 1, GETDATE());
                END

                -- Cobros (IdModulo=23) - Ver, Crear, Editar
                DECLARE @IdCobros INT = (SELECT IdModulo FROM Modulos WHERE RutaPagina = '/clientes/cobros');
                IF @IdCobros IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM RolesModulosPermisos WHERE IdRol = @IdRolCajero AND IdModulo = @IdCobros AND IdPermiso = 1)
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion) VALUES (@IdRolCajero, @IdCobros, 1, 1, GETDATE());
                    IF NOT EXISTS (SELECT 1 FROM RolesModulosPermisos WHERE IdRol = @IdRolCajero AND IdModulo = @IdCobros AND IdPermiso = 2)
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion) VALUES (@IdRolCajero, @IdCobros, 2, 1, GETDATE());
                    IF NOT EXISTS (SELECT 1 FROM RolesModulosPermisos WHERE IdRol = @IdRolCajero AND IdModulo = @IdCobros AND IdPermiso = 3)
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion) VALUES (@IdRolCajero, @IdCobros, 3, 1, GETDATE());
                END

                -- Pendientes de Cobro (IdModulo=46) - Solo Ver
                DECLARE @IdPendCobro INT = (SELECT IdModulo FROM Modulos WHERE RutaPagina = '/informes/cuentas-por-cobrar');
                IF @IdPendCobro IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM RolesModulosPermisos WHERE IdRol = @IdRolCajero AND IdModulo = @IdPendCobro AND IdPermiso = 1)
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion) VALUES (@IdRolCajero, @IdPendCobro, 1, 1, GETDATE());
                END

                -- Nota de Crédito (IdModulo=48) - Ver, Crear, Editar
                DECLARE @IdNC INT = (SELECT IdModulo FROM Modulos WHERE RutaPagina = '/notas-credito');
                IF @IdNC IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM RolesModulosPermisos WHERE IdRol = @IdRolCajero AND IdModulo = @IdNC AND IdPermiso = 1)
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion) VALUES (@IdRolCajero, @IdNC, 1, 1, GETDATE());
                    IF NOT EXISTS (SELECT 1 FROM RolesModulosPermisos WHERE IdRol = @IdRolCajero AND IdModulo = @IdNC AND IdPermiso = 2)
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion) VALUES (@IdRolCajero, @IdNC, 2, 1, GETDATE());
                    IF NOT EXISTS (SELECT 1 FROM RolesModulosPermisos WHERE IdRol = @IdRolCajero AND IdModulo = @IdNC AND IdPermiso = 3)
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion) VALUES (@IdRolCajero, @IdNC, 3, 1, GETDATE());
                END

                -- Explorar NC Ventas (IdModulo=49) - Solo Ver
                DECLARE @IdExpNC INT = (SELECT IdModulo FROM Modulos WHERE RutaPagina = '/notas-credito/explorar');
                IF @IdExpNC IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM RolesModulosPermisos WHERE IdRol = @IdRolCajero AND IdModulo = @IdExpNC AND IdPermiso = 1)
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion) VALUES (@IdRolCajero, @IdExpNC, 1, 1, GETDATE());
                END

                -- Productos (IdModulo=11) - Solo Ver (para buscar en ventas)
                DECLARE @IdProductos INT = (SELECT IdModulo FROM Modulos WHERE RutaPagina = '/productos');
                IF @IdProductos IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM RolesModulosPermisos WHERE IdRol = @IdRolCajero AND IdModulo = @IdProductos AND IdPermiso = 1)
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion) VALUES (@IdRolCajero, @IdProductos, 1, 1, GETDATE());
                END

                PRINT 'Rol Cajero creado con permisos para Ventas, Caja, Cobros, Clientes, NC y Productos';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DECLARE @IdRolCajero INT = (SELECT Id_Rol FROM Rol WHERE NombreRol = 'Cajero');
                IF @IdRolCajero IS NOT NULL
                BEGIN
                    -- Eliminar permisos asignados al rol
                    DELETE FROM RolesModulosPermisos WHERE IdRol = @IdRolCajero;
                    
                    -- Eliminar el rol solo si no hay usuarios asignados
                    IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Id_Rol = @IdRolCajero)
                        DELETE FROM Rol WHERE Id_Rol = @IdRolCajero;
                END
            ");
        }
    }
}
