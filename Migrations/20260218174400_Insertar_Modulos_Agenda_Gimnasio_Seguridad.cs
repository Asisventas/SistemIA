using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Insertar_Modulos_Agenda_Gimnasio_Seguridad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ============================================
            // INSERTAR MÓDULO AGENDA EN SISTEMA DE PERMISOS
            // ============================================
            migrationBuilder.Sql(@"
                -- Agregar modulo Agenda si no existe
                IF NOT EXISTS (SELECT 1 FROM Modulos WHERE RutaPagina = '/agenda')
                BEGIN
                    INSERT INTO Modulos (Nombre, Descripcion, RutaPagina, Icono, IdModuloPadre, Orden, Activo, FechaCreacion)
                    VALUES ('Agenda', 'Gestion de citas, visitas y calendario', '/agenda', 'bi-calendar3', NULL, 15, 1, GETDATE());
                    
                    DECLARE @IdAgenda INT = SCOPE_IDENTITY();
                    
                    -- Asignar permisos al rol Administrador (Id_Rol = 1)
                    IF EXISTS (SELECT 1 FROM Rol WHERE Id_Rol = 1)
                    BEGIN
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion)
                        VALUES (1, @IdAgenda, 1, 1, GETDATE()), -- VIEW
                               (1, @IdAgenda, 2, 1, GETDATE()), -- CREATE
                               (1, @IdAgenda, 3, 1, GETDATE()), -- EDIT
                               (1, @IdAgenda, 4, 1, GETDATE()); -- DELETE
                    END
                END
            ");

            // ============================================
            // INSERTAR MÓDULO GIMNASIO EN SISTEMA DE PERMISOS
            // ============================================
            migrationBuilder.Sql(@"
                -- Agregar modulo Gimnasio si no existe
                IF NOT EXISTS (SELECT 1 FROM Modulos WHERE RutaPagina = '/gimnasio')
                BEGIN
                    INSERT INTO Modulos (Nombre, Descripcion, RutaPagina, Icono, IdModuloPadre, Orden, Activo, FechaCreacion)
                    VALUES ('Gimnasio', 'Control de acceso, membresias y asistencia', '/gimnasio', 'bi-heart-pulse', NULL, 16, 1, GETDATE());
                    
                    DECLARE @IdGimnasio INT = SCOPE_IDENTITY();
                    
                    -- Asignar permisos al rol Administrador (Id_Rol = 1)
                    IF EXISTS (SELECT 1 FROM Rol WHERE Id_Rol = 1)
                    BEGIN
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion)
                        VALUES (1, @IdGimnasio, 1, 1, GETDATE()), -- VIEW
                               (1, @IdGimnasio, 2, 1, GETDATE()), -- CREATE
                               (1, @IdGimnasio, 3, 1, GETDATE()), -- EDIT
                               (1, @IdGimnasio, 4, 1, GETDATE()); -- DELETE
                    END
                END

                -- Agregar submodulo Control de Acceso
                IF NOT EXISTS (SELECT 1 FROM Modulos WHERE RutaPagina = '/gimnasio/control-acceso')
                BEGIN
                    DECLARE @IdGimnasioPadre INT;
                    SELECT @IdGimnasioPadre = IdModulo FROM Modulos WHERE RutaPagina = '/gimnasio';
                    
                    IF @IdGimnasioPadre IS NOT NULL
                    BEGIN
                        INSERT INTO Modulos (Nombre, Descripcion, RutaPagina, Icono, IdModuloPadre, Orden, Activo, FechaCreacion)
                        VALUES ('Control de Acceso', 'Entrada y salida de clientes con camara', '/gimnasio/control-acceso', 'bi-camera', @IdGimnasioPadre, 1, 1, GETDATE());
                        
                        DECLARE @IdControlAcceso INT = SCOPE_IDENTITY();
                        
                        IF EXISTS (SELECT 1 FROM Rol WHERE Id_Rol = 1)
                        BEGIN
                            INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion)
                            VALUES (1, @IdControlAcceso, 1, 1, GETDATE()), -- VIEW
                                   (1, @IdControlAcceso, 2, 1, GETDATE()), -- CREATE
                                   (1, @IdControlAcceso, 3, 1, GETDATE()); -- EDIT
                        END
                    END
                END

                -- Agregar submodulo Membresias
                IF NOT EXISTS (SELECT 1 FROM Modulos WHERE RutaPagina = '/gimnasio/membresias')
                BEGIN
                    DECLARE @IdGimnasioMem INT;
                    SELECT @IdGimnasioMem = IdModulo FROM Modulos WHERE RutaPagina = '/gimnasio';
                    
                    IF @IdGimnasioMem IS NOT NULL
                    BEGIN
                        INSERT INTO Modulos (Nombre, Descripcion, RutaPagina, Icono, IdModuloPadre, Orden, Activo, FechaCreacion)
                        VALUES ('Membresias', 'Gestion de planes y membresias de clientes', '/gimnasio/membresias', 'bi-credit-card', @IdGimnasioMem, 2, 1, GETDATE());
                        
                        DECLARE @IdMembresias INT = SCOPE_IDENTITY();
                        
                        IF EXISTS (SELECT 1 FROM Rol WHERE Id_Rol = 1)
                        BEGIN
                            INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion)
                            VALUES (1, @IdMembresias, 1, 1, GETDATE()), -- VIEW
                                   (1, @IdMembresias, 2, 1, GETDATE()), -- CREATE
                                   (1, @IdMembresias, 3, 1, GETDATE()), -- EDIT
                                   (1, @IdMembresias, 4, 1, GETDATE()); -- DELETE
                        END
                    END
                END

                -- Agregar submodulo Historial de Asistencia
                IF NOT EXISTS (SELECT 1 FROM Modulos WHERE RutaPagina = '/gimnasio/asistencia')
                BEGIN
                    DECLARE @IdGimnasioAsis INT;
                    SELECT @IdGimnasioAsis = IdModulo FROM Modulos WHERE RutaPagina = '/gimnasio';
                    
                    IF @IdGimnasioAsis IS NOT NULL
                    BEGIN
                        INSERT INTO Modulos (Nombre, Descripcion, RutaPagina, Icono, IdModuloPadre, Orden, Activo, FechaCreacion)
                        VALUES ('Historial de Asistencia', 'Registro de entradas y salidas', '/gimnasio/asistencia', 'bi-clock-history', @IdGimnasioAsis, 3, 1, GETDATE());
                        
                        DECLARE @IdAsistencia INT = SCOPE_IDENTITY();
                        
                        IF EXISTS (SELECT 1 FROM Rol WHERE Id_Rol = 1)
                        BEGIN
                            INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion)
                            VALUES (1, @IdAsistencia, 1, 1, GETDATE()); -- VIEW
                        END
                    END
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- Eliminar permisos de los modulos de Gimnasio
                DELETE FROM RolesModulosPermisos WHERE IdModulo IN (
                    SELECT IdModulo FROM Modulos WHERE RutaPagina IN (
                        '/gimnasio', '/gimnasio/control-acceso', '/gimnasio/membresias', '/gimnasio/asistencia'
                    )
                );
                
                -- Eliminar submodulos de Gimnasio
                DELETE FROM Modulos WHERE RutaPagina IN (
                    '/gimnasio/control-acceso', '/gimnasio/membresias', '/gimnasio/asistencia'
                );
                
                -- Eliminar modulo Gimnasio
                DELETE FROM Modulos WHERE RutaPagina = '/gimnasio';
                
                -- Eliminar permisos del modulo Agenda
                DELETE FROM RolesModulosPermisos WHERE IdModulo IN (
                    SELECT IdModulo FROM Modulos WHERE RutaPagina = '/agenda'
                );
                
                -- Eliminar modulo Agenda
                DELETE FROM Modulos WHERE RutaPagina = '/agenda';
            ");
        }
    }
}
