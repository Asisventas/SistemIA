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
-- 1.5. CIUDAD (Requerida por Sucursal)
-- ============================================
IF NOT EXISTS (SELECT 1 FROM Ciudades WHERE IdCiudad = 1)
BEGIN
    SET IDENTITY_INSERT Ciudades ON;
    INSERT INTO Ciudades (IdCiudad, Nombre, CodigoDepartamento)
    VALUES (1, 'ASUNCION', '01');
    SET IDENTITY_INSERT Ciudades OFF;
    PRINT '✓ Ciudad Asunción creada'
END
ELSE
    PRINT '- Ciudad ya existe'

-- ============================================
-- 2. SUCURSAL (Establecimiento principal)
-- ============================================
IF NOT EXISTS (SELECT 1 FROM Sucursal WHERE Id = 1)
BEGIN
    SET IDENTITY_INSERT Sucursal ON;
    INSERT INTO Sucursal (
        Id, NumSucursal, NombreSucursal, NombreEmpresa, Direccion, 
        IdCiudad, Telefono, RUC, DV, SistemaPlaya, Automatizado,
        ToleranciaEntradaMinutos, ToleranciaSalidaMinutos,
        RequiereJustificacionTardanza, RequiereJustificacionSalidaTemprana,
        MaximoHorasExtraDia, CalculoAutomaticoHorasExtra
    )
    VALUES (
        1, '001', 'Casa Matriz', 'MI EMPRESA S.A.', 'Dirección de la sucursal',
        1, '', '80000000', 0, 0, 0,
        10, 10,
        1, 1,
        240, 1
    );
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
    INSERT INTO Depositos (IdDeposito, Nombre, Descripcion, suc, Activo, FechaCreacion)
    VALUES (1, 'Depósito Principal', 'Almacén general', 1, 1, GETDATE());
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
    INSERT INTO Cajas (id_caja, Nivel1, Nivel2, FacturaInicial, Serie, Timbrado, VigenciaDel, VigenciaAl)
    VALUES (1, '001', '001', '0000001', 1, '00000000', GETDATE(), DATEADD(year, 1, GETDATE()));
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
-- NOTA: Las unidades de medida ahora son códigos SIFEN en la columna Productos.UnidadMedidaCodigo
-- No existe una tabla UnidadesMedida separada. Los valores posibles son:
-- 77 = Unidad, 006 = Paquete
PRINT '- Unidades de Medida: Usando códigos SIFEN directamente (77=Unidad, 006=Paquete)'

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
-- 15. CONFIGURACIÓN DEL SISTEMA
-- ============================================
IF NOT EXISTS (SELECT 1 FROM ConfiguracionSistema WHERE IdConfiguracion = 1)
BEGIN
    SET IDENTITY_INSERT ConfiguracionSistema ON;
    INSERT INTO ConfiguracionSistema (IdConfiguracion, ModoFarmacia, UsaPrecioMinisterio, FechaModificacion)
    VALUES (1, 0, 0, GETDATE());
    SET IDENTITY_INSERT ConfiguracionSistema OFF;
    PRINT '✓ Configuración del sistema creada'
END
ELSE
    PRINT '- Configuración del sistema ya existe'

-- ============================================
-- FINALIZACIÓN
-- ============================================
-- ============================================
-- 20. ARTÍCULOS DE CONOCIMIENTO - ASISTENTE IA
-- ============================================
PRINT ''
PRINT 'Inicializando artículos de conocimiento del Asistente IA...'

-- VENTAS
IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Crear una nueva venta')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Ventas', 'Operaciones', 'Crear una nueva venta',
'Para **crear una nueva venta**:
1. Ve a **Ventas → Nueva Venta**
2. Selecciona el **cliente**
3. Agrega productos
4. Selecciona **forma de pago**
5. **Confirma** la venta',
'venta, factura, vender, facturar, nueva venta', '/ventas', 'bi-cart', 9, GETDATE(), GETDATE(), 1, 0);

IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Anular una venta')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Ventas', 'Operaciones', 'Anular una venta',
'Para **anular una venta**:
1. Ve a **Ventas → Explorador**
2. Busca la venta
3. Click en **Anular**
Nota: Solo ventas del día. Usa NC para ventas anteriores.',
'anular venta, cancelar venta, eliminar venta', '/ventas/explorar', 'bi-x-circle', 8, GETDATE(), GETDATE(), 1, 0);

IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Crear Nota de Crédito')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Ventas', 'Notas de Crédito', 'Crear Nota de Crédito',
'Para crear una **Nota de Crédito**:
1. Ve a **Ventas → Notas de Crédito**
2. Busca la factura original
3. Selecciona productos a devolver
4. **Confirma** la NC',
'nota credito, devolucion, nc, devolver', '/notas-credito', 'bi-receipt-cutoff', 8, GETDATE(), GETDATE(), 1, 0);

-- COMPRAS
IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Registrar una compra')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Compras', 'Operaciones', 'Registrar una compra',
'Para **registrar una compra**:
1. Ve a **Compras → Nueva Compra**
2. Selecciona el proveedor
3. Ingresa productos y cantidades
4. **Confirma** la compra',
'compra, comprar, nueva compra, factura proveedor', '/compras', 'bi-bag', 9, GETDATE(), GETDATE(), 1, 0);

IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Pagar a proveedores')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Compras', 'Pagos', 'Pagar a proveedores',
'Para **pagar a un proveedor**:
1. Ve a **Compras → Pagos**
2. Selecciona el proveedor
3. Elige facturas a pagar
4. Registra el monto y forma de pago',
'pago proveedor, pagar proveedor, cuentas pagar', '/pagos-proveedores', 'bi-cash-coin', 8, GETDATE(), GETDATE(), 1, 0);

-- CAJA
IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Cierre de caja')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Caja', 'Operaciones', 'Cierre de caja',
'Para **cerrar caja**:
1. Ve a **Ventas → Cierre de Caja**
2. Revisa el resumen de operaciones
3. Ingresa efectivo contado
4. **Confirma** el cierre',
'cierre caja, cerrar caja, arqueo, cuadrar caja', '/caja/cierre', 'bi-cash-stack', 9, GETDATE(), GETDATE(), 1, 0);

-- INVENTARIO
IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Ajustar stock de productos')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Inventario', 'Stock', 'Ajustar stock de productos',
'Para **ajustar stock**:
1. Ve a **Inventario → Ajustes**
2. Busca el producto
3. Ingresa cantidad nueva o ajuste
4. Selecciona motivo
5. **Confirma**',
'ajuste stock, inventario, merma, corregir stock', '/inventario/ajustes', 'bi-box-seam', 8, GETDATE(), GETDATE(), 1, 0);

IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Transferir stock entre depósitos')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Inventario', 'Stock', 'Transferir stock entre depósitos',
'Para **transferir stock**:
1. Ve a **Inventario → Transferencias**
2. Selecciona origen y destino
3. Agrega productos y cantidades
4. **Confirma** la transferencia',
'transferir, mover productos, transferencia deposito', '/inventario/transferencias', 'bi-arrow-left-right', 7, GETDATE(), GETDATE(), 1, 0);

-- CLIENTES Y COBROS
IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Cobrar cuotas a clientes')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Clientes', 'Cobros', 'Cobrar cuotas a clientes',
'Para **cobrar a un cliente**:
1. Ve a **Ventas → Cuentas por Cobrar**
2. Selecciona el cliente
3. Marca cuotas a cobrar
4. Registra monto y forma de pago',
'cobro, cobrar cliente, cuota, deuda cliente', '/cobros', 'bi-currency-dollar', 8, GETDATE(), GETDATE(), 1, 0);

-- SISTEMA (BACKUP)
IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Hacer backup de la base de datos')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Sistema', 'Backup', 'Hacer backup de la base de datos',
'Para hacer **backup**:
1. Abre SQL Server Management Studio
2. Click derecho en asiswebapp
3. Tareas → Copia de seguridad
4. Selecciona destino (.bak)
Haz backup diario y guarda copia externa.',
'backup, copia seguridad, respaldo, guardar datos', NULL, 'bi-hdd', 10, GETDATE(), GETDATE(), 1, 0);

IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Restaurar backup de base de datos')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Sistema', 'Backup', 'Restaurar backup de base de datos',
'Para **restaurar backup**:
1. Cierra la aplicación
2. En SSMS: Restaurar base de datos
3. Selecciona archivo .bak
4. Confirma la restauración
La restauración sobrescribe datos actuales.',
'restaurar, restore, recuperar, cargar backup', NULL, 'bi-arrow-counterclockwise', 9, GETDATE(), GETDATE(), 1, 0);

IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Actualizar el sistema')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Sistema', 'Mantenimiento', 'Actualizar el sistema',
'Para **actualizar SistemIA**:
1. Haz backup primero
2. Descarga la nueva versión
3. Ejecuta Actualizar.bat
4. Reinicia la aplicación
Las migraciones de BD se aplican automáticamente.',
'actualizar, update, version, nueva version', '/actualizacion-sistema', 'bi-cloud-download', 8, GETDATE(), GETDATE(), 1, 0);

-- USUARIOS
IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Crear nuevo usuario')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Usuarios', 'Gestión', 'Crear nuevo usuario',
'Para **crear usuario**:
1. Ve a **Personal → Usuarios**
2. Click en Nuevo Usuario
3. Completa datos y rol
4. Configura permisos
5. Guarda',
'usuario, crear usuario, nuevo usuario, empleado', '/menu-usuarios', 'bi-person-plus', 8, GETDATE(), GETDATE(), 1, 0);

IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Configurar permisos de usuario')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Usuarios', 'Permisos', 'Configurar permisos de usuario',
'Para **configurar permisos**:
1. Ve a **Personal → Permisos**
2. Selecciona usuario o rol
3. Marca permisos por módulo
4. Guarda cambios',
'permisos, acceso, roles, seguridad', '/personal/permisos-usuarios', 'bi-shield-lock', 8, GETDATE(), GETDATE(), 1, 0);

-- PRODUCTOS
IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Crear nuevo producto')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Productos', 'Gestión', 'Crear nuevo producto',
'Para **crear producto**:
1. Ve a **Productos → Administrar**
2. Click en Nuevo Producto
3. Ingresa código, descripción, precio
4. Selecciona tipo IVA
5. Guarda',
'producto, crear producto, nuevo producto, articulo', '/productos', 'bi-box', 9, GETDATE(), GETDATE(), 1, 0);

-- CONFIGURACIÓN
IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Configurar timbrado y facturación electrónica')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Configuración', 'SIFEN', 'Configurar timbrado y facturación electrónica',
'Para configurar **SIFEN**:
1. Ve a Configuración → Sociedad
2. Carga certificado .pfx
3. Ve a Configuración → Cajas
4. Ingresa timbrado y vigencia
5. Selecciona ambiente (Test/Producción)',
'sifen, timbrado, factura electronica, certificado, set', '/configuracion/cajas', 'bi-patch-check', 9, GETDATE(), GETDATE(), 1, 0);

IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Configurar envío automático de correo')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Configuración', 'Correo', 'Configurar envío automático de correo',
'Para configurar **correo**:
1. Ve a Configuración → Correo
2. Configura servidor SMTP (smtp.gmail.com:587)
3. Ingresa usuario y contraseña app
4. Agrega destinatarios
5. Activa envío automático',
'correo, email, smtp, gmail, notificacion', '/configuracion/correo', 'bi-envelope', 9, GETDATE(), GETDATE(), 1, 0);

-- PRESUPUESTOS
IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Crear un presupuesto')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Ventas', 'Presupuestos', 'Crear un presupuesto',
'Para crear **presupuesto**:
1. Ve a Ventas → Presupuestos
2. Nuevo Presupuesto
3. Agrega cliente y productos
4. Define validez
5. Guarda (convierte a venta cuando acepten)',
'presupuesto, cotizacion, proforma', '/presupuestos/explorar', 'bi-file-earmark-text', 8, GETDATE(), GETDATE(), 1, 0);

PRINT '✓ Artículos de conocimiento creados'

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
