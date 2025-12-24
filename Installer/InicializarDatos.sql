-- ============================================
-- SISTEMÍA - Script de Inicialización de Datos
-- Ejecutar DESPUÉS de CrearBaseDatos.sql
-- ============================================

-- Usar la base de datos (cambiar nombre si es diferente)
-- USE [SistemIA_DB]
-- GO

SET NOCOUNT ON;

PRINT 'Inicializando datos base de SistemIA...'

-- ============================================
-- 1. SOCIEDAD (Emisor principal)
-- ============================================
IF NOT EXISTS (SELECT 1 FROM Sociedades WHERE IdSociedad = 1)
BEGIN
    SET IDENTITY_INSERT Sociedades ON;
    INSERT INTO Sociedades (IdSociedad, Nombre, RUC, DV, Direccion, TipoContribuyente, ServidorSifen)
    VALUES (1, 'MI EMPRESA S.A.', '80000000', 0, 'Dirección de la empresa', 1, 'test');
    SET IDENTITY_INSERT Sociedades OFF;
    PRINT '✓ Sociedad creada'
END
ELSE
    PRINT '- Sociedad ya existe'

-- ============================================
-- 2. SUCURSAL (Establecimiento principal)
-- ============================================
IF NOT EXISTS (SELECT 1 FROM Sucursal WHERE IdSucursal = 1)
BEGIN
    SET IDENTITY_INSERT Sucursal ON;
    INSERT INTO Sucursal (IdSucursal, Nombre, Direccion, NroEstablecimiento, Telefono, IdSociedad, Estado)
    VALUES (1, 'Casa Matriz', 'Dirección de la sucursal', '001', '', 1, 1);
    SET IDENTITY_INSERT Sucursal OFF;
    PRINT '✓ Sucursal creada'
END
ELSE
    PRINT '- Sucursal ya existe'

-- ============================================
-- 3. DEPÓSITO (Almacén principal)
-- ============================================
IF NOT EXISTS (SELECT 1 FROM Depositos WHERE IdDeposito = 1)
BEGIN
    SET IDENTITY_INSERT Depositos ON;
    INSERT INTO Depositos (IdDeposito, Nombre, Ubicacion, IdSucursal, Estado)
    VALUES (1, 'Depósito Principal', 'Almacén general', 1, 1);
    SET IDENTITY_INSERT Depositos OFF;
    PRINT '✓ Depósito creado'
END
ELSE
    PRINT '- Depósito ya existe'

-- ============================================
-- 4. CAJA (Punto de expedición)
-- ============================================
IF NOT EXISTS (SELECT 1 FROM Cajas WHERE id_caja = 1)
BEGIN
    SET IDENTITY_INSERT Cajas ON;
    INSERT INTO Cajas (id_caja, Nombre, NroPuntoExpedicion, IdSucursal, Estado, IdDepositoDefault)
    VALUES (1, 'Caja Principal', '001', 1, 1, 1);
    SET IDENTITY_INSERT Cajas OFF;
    PRINT '✓ Caja creada'
END
ELSE
    PRINT '- Caja ya existe'

-- ============================================
-- 5. PROVEEDOR GENERAL
-- ============================================
IF NOT EXISTS (SELECT 1 FROM ProveedoresSifen WHERE IdProveedor = 1)
BEGIN
    SET IDENTITY_INSERT ProveedoresSifen ON;
    INSERT INTO ProveedoresSifen (IdProveedor, RazonSocial, NombreFantasia, TipoDocumento, RUC, SaldoPendiente, Estado)
    VALUES (1, 'Proveedor General', 'Proveedor General', 11, '00000000-0', 0, 1);
    SET IDENTITY_INSERT ProveedoresSifen OFF;
    PRINT '✓ Proveedor General creado'
END
ELSE
    PRINT '- Proveedor General ya existe'

-- ============================================
-- 6. CLIENTE CONSUMIDOR FINAL
-- ============================================
IF NOT EXISTS (SELECT 1 FROM Clientes WHERE IdCliente = 1)
BEGIN
    SET IDENTITY_INSERT Clientes ON;
    INSERT INTO Clientes (IdCliente, RazonSocial, TipoDocumento, NumeroDocumento, Saldo, Estado, TipoContribuyente)
    VALUES (1, 'CONSUMIDOR FINAL', 5, '0', 0, 1, 12);
    SET IDENTITY_INSERT Clientes OFF;
    PRINT '✓ Cliente Consumidor Final creado'
END
ELSE
    PRINT '- Cliente Consumidor Final ya existe'

-- ============================================
-- 7. ROL ADMINISTRADOR
-- ============================================
IF NOT EXISTS (SELECT 1 FROM Roles WHERE IdRol = 1)
BEGIN
    SET IDENTITY_INSERT Roles ON;
    INSERT INTO Roles (IdRol, Nombre, Descripcion, Activo, FechaCreacion)
    VALUES (1, 'Administrador', 'Acceso total al sistema', 1, GETDATE());
    SET IDENTITY_INSERT Roles OFF;
    PRINT '✓ Rol Administrador creado'
END
ELSE
    PRINT '- Rol Administrador ya existe'

-- ============================================
-- 8. USUARIO ADMIN
-- ============================================
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE IdUsuario = 1)
BEGIN
    SET IDENTITY_INSERT Usuarios ON;
    -- Password: admin (hash BCrypt)
    INSERT INTO Usuarios (IdUsuario, NombreCompleto, NombreUsuario, Email, PasswordHash, IdRol, Estado, FechaCreacion, IdSucursal, IdCaja)
    VALUES (1, 'Administrador', 'admin', 'admin@sistema.local', 
            '$2a$11$rKLWc5a5BmVVW1lHKIYqpOG3LDqBMPOCCl.HJ.KDHh9x5RLfxQYdq', 
            1, 1, GETDATE(), 1, 1);
    SET IDENTITY_INSERT Usuarios OFF;
    PRINT '✓ Usuario Admin creado (usuario: admin, contraseña: admin)'
END
ELSE
    PRINT '- Usuario Admin ya existe'

-- ============================================
-- 9. TIPOS DE IVA
-- ============================================
IF NOT EXISTS (SELECT 1 FROM TiposIVA WHERE IdTipoIVA = 1)
BEGIN
    SET IDENTITY_INSERT TiposIVA ON;
    INSERT INTO TiposIVA (IdTipoIVA, Nombre, Porcentaje, CodigoSifen, Activo) VALUES (1, 'Exento', 0.00, 1, 1);
    INSERT INTO TiposIVA (IdTipoIVA, Nombre, Porcentaje, CodigoSifen, Activo) VALUES (2, 'IVA 5%', 5.00, 2, 1);
    INSERT INTO TiposIVA (IdTipoIVA, Nombre, Porcentaje, CodigoSifen, Activo) VALUES (3, 'IVA 10%', 10.00, 3, 1);
    SET IDENTITY_INSERT TiposIVA OFF;
    PRINT '✓ Tipos de IVA creados'
END
ELSE
    PRINT '- Tipos de IVA ya existen'

-- ============================================
-- 10. TIPOS DE DOCUMENTO
-- ============================================
IF NOT EXISTS (SELECT 1 FROM TiposDocumento WHERE IdTipoDocumento = 1)
BEGIN
    SET IDENTITY_INSERT TiposDocumento ON;
    INSERT INTO TiposDocumento (IdTipoDocumento, Descripcion, CodigoSifen, Activo) VALUES (1, 'Cédula Paraguaya', 1, 1);
    INSERT INTO TiposDocumento (IdTipoDocumento, Descripcion, CodigoSifen, Activo) VALUES (2, 'Pasaporte', 2, 1);
    INSERT INTO TiposDocumento (IdTipoDocumento, Descripcion, CodigoSifen, Activo) VALUES (3, 'Cédula Extranjera', 3, 1);
    INSERT INTO TiposDocumento (IdTipoDocumento, Descripcion, CodigoSifen, Activo) VALUES (4, 'Carnet de Residencia', 4, 1);
    INSERT INTO TiposDocumento (IdTipoDocumento, Descripcion, CodigoSifen, Activo) VALUES (5, 'Innominado', 5, 1);
    INSERT INTO TiposDocumento (IdTipoDocumento, Descripcion, CodigoSifen, Activo) VALUES (6, 'Tarjeta Diplomática', 6, 1);
    INSERT INTO TiposDocumento (IdTipoDocumento, Descripcion, CodigoSifen, Activo) VALUES (9, 'Otro', 9, 1);
    INSERT INTO TiposDocumento (IdTipoDocumento, Descripcion, CodigoSifen, Activo) VALUES (11, 'RUC', 11, 1);
    SET IDENTITY_INSERT TiposDocumento OFF;
    PRINT '✓ Tipos de Documento creados'
END
ELSE
    PRINT '- Tipos de Documento ya existen'

-- ============================================
-- 11. TIPOS DE PAGO
-- ============================================
IF NOT EXISTS (SELECT 1 FROM TiposPago WHERE IdTipoPago = 1)
BEGIN
    SET IDENTITY_INSERT TiposPago ON;
    INSERT INTO TiposPago (IdTipoPago, Nombre, CodigoSifen, RequiereDatosTarjeta, RequiereDatosCheque, RequiereDatosTransferencia, Activo) 
    VALUES (1, 'Efectivo', 1, 0, 0, 0, 1);
    INSERT INTO TiposPago (IdTipoPago, Nombre, CodigoSifen, RequiereDatosTarjeta, RequiereDatosCheque, RequiereDatosTransferencia, Activo) 
    VALUES (2, 'Cheque', 2, 0, 1, 0, 1);
    INSERT INTO TiposPago (IdTipoPago, Nombre, CodigoSifen, RequiereDatosTarjeta, RequiereDatosCheque, RequiereDatosTransferencia, Activo) 
    VALUES (3, 'Tarjeta de Crédito', 3, 1, 0, 0, 1);
    INSERT INTO TiposPago (IdTipoPago, Nombre, CodigoSifen, RequiereDatosTarjeta, RequiereDatosCheque, RequiereDatosTransferencia, Activo) 
    VALUES (4, 'Tarjeta de Débito', 4, 1, 0, 0, 1);
    INSERT INTO TiposPago (IdTipoPago, Nombre, CodigoSifen, RequiereDatosTarjeta, RequiereDatosCheque, RequiereDatosTransferencia, Activo) 
    VALUES (5, 'Transferencia', 5, 0, 0, 1, 1);
    INSERT INTO TiposPago (IdTipoPago, Nombre, CodigoSifen, RequiereDatosTarjeta, RequiereDatosCheque, RequiereDatosTransferencia, Activo) 
    VALUES (6, 'Giro', 6, 0, 0, 0, 1);
    INSERT INTO TiposPago (IdTipoPago, Nombre, CodigoSifen, RequiereDatosTarjeta, RequiereDatosCheque, RequiereDatosTransferencia, Activo) 
    VALUES (7, 'Billetera Electrónica', 7, 0, 0, 0, 1);
    INSERT INTO TiposPago (IdTipoPago, Nombre, CodigoSifen, RequiereDatosTarjeta, RequiereDatosCheque, RequiereDatosTransferencia, Activo) 
    VALUES (8, 'Tarjeta Empresarial', 8, 1, 0, 0, 1);
    INSERT INTO TiposPago (IdTipoPago, Nombre, CodigoSifen, RequiereDatosTarjeta, RequiereDatosCheque, RequiereDatosTransferencia, Activo) 
    VALUES (9, 'Vale', 9, 0, 0, 0, 1);
    INSERT INTO TiposPago (IdTipoPago, Nombre, CodigoSifen, RequiereDatosTarjeta, RequiereDatosCheque, RequiereDatosTransferencia, Activo) 
    VALUES (10, 'Retención', 10, 0, 0, 0, 1);
    INSERT INTO TiposPago (IdTipoPago, Nombre, CodigoSifen, RequiereDatosTarjeta, RequiereDatosCheque, RequiereDatosTransferencia, Activo) 
    VALUES (11, 'Pago por Anticipo', 11, 0, 0, 0, 1);
    INSERT INTO TiposPago (IdTipoPago, Nombre, CodigoSifen, RequiereDatosTarjeta, RequiereDatosCheque, RequiereDatosTransferencia, Activo) 
    VALUES (12, 'Valor Fiscal', 12, 0, 0, 0, 1);
    INSERT INTO TiposPago (IdTipoPago, Nombre, CodigoSifen, RequiereDatosTarjeta, RequiereDatosCheque, RequiereDatosTransferencia, Activo) 
    VALUES (13, 'Valor Comercial', 13, 0, 0, 0, 1);
    INSERT INTO TiposPago (IdTipoPago, Nombre, CodigoSifen, RequiereDatosTarjeta, RequiereDatosCheque, RequiereDatosTransferencia, Activo) 
    VALUES (14, 'Compensación', 14, 0, 0, 0, 1);
    INSERT INTO TiposPago (IdTipoPago, Nombre, CodigoSifen, RequiereDatosTarjeta, RequiereDatosCheque, RequiereDatosTransferencia, Activo) 
    VALUES (15, 'Permuta', 15, 0, 0, 0, 1);
    INSERT INTO TiposPago (IdTipoPago, Nombre, CodigoSifen, RequiereDatosTarjeta, RequiereDatosCheque, RequiereDatosTransferencia, Activo) 
    VALUES (16, 'Pago Móvil', 16, 0, 0, 0, 1);
    INSERT INTO TiposPago (IdTipoPago, Nombre, CodigoSifen, RequiereDatosTarjeta, RequiereDatosCheque, RequiereDatosTransferencia, Activo) 
    VALUES (17, 'Cuenta Corriente', 17, 0, 0, 0, 1);
    INSERT INTO TiposPago (IdTipoPago, Nombre, CodigoSifen, RequiereDatosTarjeta, RequiereDatosCheque, RequiereDatosTransferencia, Activo) 
    VALUES (99, 'Otro', 99, 0, 0, 0, 1);
    SET IDENTITY_INSERT TiposPago OFF;
    PRINT '✓ Tipos de Pago creados'
END
ELSE
    PRINT '- Tipos de Pago ya existen'

-- ============================================
-- 12. CLASIFICACIONES BÁSICAS
-- ============================================
IF NOT EXISTS (SELECT 1 FROM Clasificaciones WHERE IdClasificacion = 1)
BEGIN
    SET IDENTITY_INSERT Clasificaciones ON;
    INSERT INTO Clasificaciones (IdClasificacion, Clasificacion, Estado) VALUES (1, 'General', 1);
    INSERT INTO Clasificaciones (IdClasificacion, Clasificacion, Estado) VALUES (2, 'Electrónica', 1);
    INSERT INTO Clasificaciones (IdClasificacion, Clasificacion, Estado) VALUES (3, 'Alimentos', 1);
    INSERT INTO Clasificaciones (IdClasificacion, Clasificacion, Estado) VALUES (4, 'Bebidas', 1);
    INSERT INTO Clasificaciones (IdClasificacion, Clasificacion, Estado) VALUES (5, 'Limpieza', 1);
    SET IDENTITY_INSERT Clasificaciones OFF;
    PRINT '✓ Clasificaciones creadas'
END
ELSE
    PRINT '- Clasificaciones ya existen'

-- ============================================
-- 13. MARCAS BÁSICAS
-- ============================================
IF NOT EXISTS (SELECT 1 FROM Marcas WHERE Id_Marca = 1)
BEGIN
    SET IDENTITY_INSERT Marcas ON;
    INSERT INTO Marcas (Id_Marca, Marca) VALUES (1, 'Sin Marca');
    INSERT INTO Marcas (Id_Marca, Marca) VALUES (2, 'Genérico');
    SET IDENTITY_INSERT Marcas OFF;
    PRINT '✓ Marcas creadas'
END
ELSE
    PRINT '- Marcas ya existen'

-- ============================================
-- 14. UNIDADES DE MEDIDA
-- ============================================
IF NOT EXISTS (SELECT 1 FROM UnidadesMedida WHERE IdUnidadMedida = 1)
BEGIN
    SET IDENTITY_INSERT UnidadesMedida ON;
    INSERT INTO UnidadesMedida (IdUnidadMedida, Nombre, Abreviatura, CodigoSifen, Activo) VALUES (1, 'Unidad', 'UNI', 77, 1);
    INSERT INTO UnidadesMedida (IdUnidadMedida, Nombre, Abreviatura, CodigoSifen, Activo) VALUES (2, 'Kilogramo', 'KG', 83, 1);
    INSERT INTO UnidadesMedida (IdUnidadMedida, Nombre, Abreviatura, CodigoSifen, Activo) VALUES (3, 'Gramo', 'GR', 85, 1);
    INSERT INTO UnidadesMedida (IdUnidadMedida, Nombre, Abreviatura, CodigoSifen, Activo) VALUES (4, 'Litro', 'LT', 79, 1);
    INSERT INTO UnidadesMedida (IdUnidadMedida, Nombre, Abreviatura, CodigoSifen, Activo) VALUES (5, 'Metro', 'MT', 86, 1);
    INSERT INTO UnidadesMedida (IdUnidadMedida, Nombre, Abreviatura, CodigoSifen, Activo) VALUES (6, 'Caja', 'CJ', 77, 1);
    INSERT INTO UnidadesMedida (IdUnidadMedida, Nombre, Abreviatura, CodigoSifen, Activo) VALUES (7, 'Paquete', 'PQ', 77, 1);
    INSERT INTO UnidadesMedida (IdUnidadMedida, Nombre, Abreviatura, CodigoSifen, Activo) VALUES (8, 'Docena', 'DOC', 77, 1);
    SET IDENTITY_INSERT UnidadesMedida OFF;
    PRINT '✓ Unidades de Medida creadas'
END
ELSE
    PRINT '- Unidades de Medida ya existen'

-- ============================================
-- 15. LISTA DE PRECIOS DEFAULT
-- ============================================
IF NOT EXISTS (SELECT 1 FROM ListasPrecios WHERE IdListaPrecio = 1)
BEGIN
    SET IDENTITY_INSERT ListasPrecios ON;
    INSERT INTO ListasPrecios (IdListaPrecio, Nombre, Descripcion, IdSucursal, FechaInicio, Activo, EsPredeterminada)
    VALUES (1, 'Precio General', 'Lista de precios predeterminada', 1, GETDATE(), 1, 1);
    SET IDENTITY_INSERT ListasPrecios OFF;
    PRINT '✓ Lista de Precios creada'
END
ELSE
    PRINT '- Lista de Precios ya existe'

-- ============================================
-- 16. MÓDULOS Y PERMISOS BASE
-- ============================================
IF NOT EXISTS (SELECT 1 FROM Modulos WHERE IdModulo = 1)
BEGIN
    SET IDENTITY_INSERT Modulos ON;
    INSERT INTO Modulos (IdModulo, Nombre, Descripcion, Icono, Orden, Activo) VALUES (1, 'Dashboard', 'Panel principal', 'bi-speedometer2', 1, 1);
    INSERT INTO Modulos (IdModulo, Nombre, Descripcion, Icono, Orden, Activo) VALUES (2, 'Ventas', 'Gestión de ventas', 'bi-cart', 2, 1);
    INSERT INTO Modulos (IdModulo, Nombre, Descripcion, Icono, Orden, Activo) VALUES (3, 'Compras', 'Gestión de compras', 'bi-bag', 3, 1);
    INSERT INTO Modulos (IdModulo, Nombre, Descripcion, Icono, Orden, Activo) VALUES (4, 'Productos', 'Gestión de productos', 'bi-box-seam', 4, 1);
    INSERT INTO Modulos (IdModulo, Nombre, Descripcion, Icono, Orden, Activo) VALUES (5, 'Clientes', 'Gestión de clientes', 'bi-people', 5, 1);
    INSERT INTO Modulos (IdModulo, Nombre, Descripcion, Icono, Orden, Activo) VALUES (6, 'Proveedores', 'Gestión de proveedores', 'bi-truck', 6, 1);
    INSERT INTO Modulos (IdModulo, Nombre, Descripcion, Icono, Orden, Activo) VALUES (7, 'Configuración', 'Configuración del sistema', 'bi-gear', 7, 1);
    SET IDENTITY_INSERT Modulos OFF;
    PRINT '✓ Módulos creados'
END
ELSE
    PRINT '- Módulos ya existen'

IF NOT EXISTS (SELECT 1 FROM Permisos WHERE IdPermiso = 1)
BEGIN
    SET IDENTITY_INSERT Permisos ON;
    INSERT INTO Permisos (IdPermiso, Nombre, Descripcion, Codigo, Activo) VALUES (1, 'Ver', 'Permite ver registros', 'VIEW', 1);
    INSERT INTO Permisos (IdPermiso, Nombre, Descripcion, Codigo, Activo) VALUES (2, 'Crear', 'Permite crear registros', 'CREATE', 1);
    INSERT INTO Permisos (IdPermiso, Nombre, Descripcion, Codigo, Activo) VALUES (3, 'Editar', 'Permite editar registros', 'EDIT', 1);
    INSERT INTO Permisos (IdPermiso, Nombre, Descripcion, Codigo, Activo) VALUES (4, 'Eliminar', 'Permite eliminar registros', 'DELETE', 1);
    INSERT INTO Permisos (IdPermiso, Nombre, Descripcion, Codigo, Activo) VALUES (5, 'Exportar', 'Permite exportar datos', 'EXPORT', 1);
    INSERT INTO Permisos (IdPermiso, Nombre, Descripcion, Codigo, Activo) VALUES (6, 'Imprimir', 'Permite imprimir', 'PRINT', 1);
    SET IDENTITY_INSERT Permisos OFF;
    PRINT '✓ Permisos creados'
END
ELSE
    PRINT '- Permisos ya existen'

-- Asignar todos los permisos al rol Administrador
IF NOT EXISTS (SELECT 1 FROM RolesPermisos WHERE IdRol = 1)
BEGIN
    INSERT INTO RolesPermisos (IdRol, IdModulo, IdPermiso, Conceder, FechaAsignacion)
    SELECT 1, m.IdModulo, p.IdPermiso, 1, GETDATE()
    FROM Modulos m
    CROSS JOIN Permisos p;
    PRINT '✓ Permisos asignados al Administrador'
END
ELSE
    PRINT '- Permisos del Administrador ya existen'

-- ============================================
-- FINALIZACIÓN
-- ============================================
PRINT ''
PRINT '============================================'
PRINT 'INICIALIZACIÓN COMPLETADA'
PRINT '============================================'
PRINT ''
PRINT 'Usuario: admin'
PRINT 'Contraseña: admin'
PRINT ''
PRINT 'IMPORTANTE: Cambie la contraseña después del primer inicio de sesión.'
PRINT ''

SET NOCOUNT OFF;
GO
