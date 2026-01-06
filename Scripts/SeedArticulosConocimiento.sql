-- =====================================================
-- Script para poblar artículos de conocimiento del Asistente IA
-- Ejecutar después de crear las tablas de Asistente IA
-- =====================================================

-- Limpiar artículos existentes (opcional, descomentar si necesario)
-- DELETE FROM ArticulosConocimiento WHERE IdArticulo > 0;

-- =====================================================
-- CATEGORÍA: VENTAS
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Crear una nueva venta')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Ventas', 'Operaciones', 'Crear una nueva venta',
'Para **crear una nueva venta**, sigue estos pasos:

1️⃣ Ve a **Ventas → Nueva Venta** o presiona el acceso directo
2️⃣ Selecciona el **cliente** (puedes buscar por nombre o RUC)
3️⃣ Agrega productos usando el **buscador** o escaneando código de barras
4️⃣ Ajusta las **cantidades** si es necesario
5️⃣ Selecciona la **forma de pago**: Contado, Crédito, etc.
6️⃣ Haz clic en **Confirmar Venta**

💡 **Tips**:
- Usa F2 para buscar cliente rápidamente
- Usa F3 para buscar producto
- Verifica el total antes de confirmar
- Si es crédito, define las cuotas y vencimientos',
'venta, factura, vender, facturar, nueva venta, crear venta', '/ventas', 'bi-cart', 9, GETDATE(), GETDATE(), 1, 0);

IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Anular una venta')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Ventas', 'Operaciones', 'Anular una venta',
'Para **anular una venta**, sigue estos pasos:

1️⃣ Ve a **Ventas → Explorador de Ventas**
2️⃣ Busca la venta por número, fecha o cliente
3️⃣ Haz clic en la venta para ver detalle
4️⃣ Presiona el botón **Anular** (icono de papelera)
5️⃣ Confirma la anulación

⚠️ **Importante**:
- Solo puedes anular ventas del día actual
- Si ya pasó el día, debes crear una **Nota de Crédito**
- Las ventas enviadas a SIFEN no se pueden anular directamente
- Al anular, el stock se devuelve automáticamente',
'anular venta, cancelar venta, eliminar venta, borrar factura', '/ventas/explorar', 'bi-x-circle', 8, GETDATE(), GETDATE(), 1, 0);

IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Crear Nota de Crédito')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Ventas', 'Notas de Crédito', 'Crear Nota de Crédito',
'Para **crear una Nota de Crédito** (devolución):

1️⃣ Ve a **Ventas → Notas de Crédito**
2️⃣ Haz clic en **Nueva NC**
3️⃣ Busca la **factura original** a la que aplicar la NC
4️⃣ Selecciona los **productos** a devolver
5️⃣ Ajusta las **cantidades** devueltas
6️⃣ Indica el **motivo** de la devolución
7️⃣ **Confirma** la Nota de Crédito

💡 **Opciones**:
- NC Total: devuelve toda la factura
- NC Parcial: devuelve solo algunos productos
- El stock se restaura automáticamente',
'nota credito, devolucion, nc, credito, devolver producto, anular factura anterior', '/notas-credito', 'bi-receipt-cutoff', 8, GETDATE(), GETDATE(), 1, 0);

-- =====================================================
-- CATEGORÍA: COMPRAS
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Registrar una compra')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Compras', 'Operaciones', 'Registrar una compra',
'Para **registrar una compra**, sigue estos pasos:

1️⃣ Ve a **Compras → Nueva Compra**
2️⃣ Selecciona el **proveedor**
3️⃣ Ingresa el **número de factura** del proveedor
4️⃣ Agrega los **productos** comprados
5️⃣ Verifica los **precios de costo** y cantidades
6️⃣ Selecciona **Contado o Crédito**
7️⃣ **Confirma** la compra

💡 **Tips**:
- Los precios de costo se actualizan automáticamente
- El stock se suma al confirmar
- Puedes adjuntar imagen de la factura del proveedor',
'compra, comprar, nueva compra, registrar compra, ingreso mercaderia, factura proveedor', '/compras', 'bi-bag', 9, GETDATE(), GETDATE(), 1, 0);

IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Pagar a proveedores')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Compras', 'Pagos', 'Pagar a proveedores',
'Para **registrar un pago a proveedor**:

1️⃣ Ve a **Compras → Pagos a Proveedores**
2️⃣ Selecciona el **proveedor**
3️⃣ Verás las **facturas pendientes** de pago
4️⃣ Selecciona qué facturas vas a pagar
5️⃣ Ingresa el **monto** del pago
6️⃣ Selecciona la **forma de pago** (efectivo, cheque, transferencia)
7️⃣ **Confirma** el pago

📊 Para ver el historial: **Pagos → Historial de Pagos**
📋 Para ver deudas: **Informes → Cuentas por Pagar**',
'pago proveedor, pagar proveedor, deuda proveedor, cuentas por pagar, pago factura', '/pagos-proveedores', 'bi-cash-coin', 8, GETDATE(), GETDATE(), 1, 0);

-- =====================================================
-- CATEGORÍA: CAJA
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Cierre de caja')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Caja', 'Operaciones', 'Cierre de caja',
'Para **realizar el cierre de caja**:

1️⃣ Ve a **Ventas → Cierre de Caja**
2️⃣ Verifica que todas las ventas estén **confirmadas**
3️⃣ Revisa el **resumen de operaciones**:
   - Ventas del turno
   - Cobros recibidos
   - Pagos realizados
   - Notas de crédito emitidas
4️⃣ Ingresa el **efectivo contado** físicamente
5️⃣ El sistema calcula la **diferencia** (sobrante/faltante)
6️⃣ **Confirma** el cierre

💡 **Recomendaciones**:
- Cierra caja al final de cada turno
- Revisa los informes antes de cerrar
- Documenta cualquier diferencia encontrada',
'cierre caja, cerrar caja, arqueo, cuadrar caja, diferencia caja, sobrante, faltante', '/caja/cierre', 'bi-cash-stack', 9, GETDATE(), GETDATE(), 1, 0);

IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Cambiar turno de caja')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Caja', 'Configuración', 'Cambiar turno de caja',
'Para **cambiar de turno** en la caja:

1️⃣ Primero realiza el **cierre del turno actual**
2️⃣ Ve a **Configuración → Cajas**
3️⃣ Selecciona la caja activa
4️⃣ Cambia el **número de turno** (1, 2, 3...)
5️⃣ Guarda los cambios

⚠️ **Importante**:
- Cada turno tiene su propio cierre independiente
- El historial de cierres separa por turno
- Configura la cantidad de turnos en la configuración de caja',
'turno, cambiar turno, siguiente turno, turno caja, turno mañana, turno tarde', '/configuracion/cajas', 'bi-clock', 7, GETDATE(), GETDATE(), 1, 0);

-- =====================================================
-- CATEGORÍA: INVENTARIO
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Ajustar stock de productos')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Inventario', 'Stock', 'Ajustar stock de productos',
'Para **ajustar el stock** de productos:

1️⃣ Ve a **Inventario → Ajustes de Stock**
2️⃣ Selecciona el **depósito** a ajustar
3️⃣ Busca el **producto**
4️⃣ Ingresa la **cantidad nueva** o el **ajuste (+/-)**
5️⃣ Selecciona el **motivo**:
   - Inventario físico
   - Merma/rotura
   - Vencimiento
   - Error de conteo
   - Otro
6️⃣ **Confirma** el ajuste

📊 Para ver historial: **Informes → Ajustes de Stock**',
'ajuste stock, ajustar inventario, modificar stock, corregir stock, merma, perdida, inventario fisico', '/inventario/ajustes', 'bi-box-seam', 8, GETDATE(), GETDATE(), 1, 0);

IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Transferir stock entre depósitos')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Inventario', 'Stock', 'Transferir stock entre depósitos',
'Para **transferir productos** entre depósitos:

1️⃣ Ve a **Inventario → Transferencias**
2️⃣ Selecciona el **depósito origen**
3️⃣ Selecciona el **depósito destino**
4️⃣ Agrega los **productos** a transferir
5️⃣ Indica las **cantidades**
6️⃣ **Confirma** la transferencia

💡 **Nota**: El stock se resta del origen y se suma al destino inmediatamente.',
'transferir stock, mover productos, transferencia deposito, enviar mercaderia, traslado', '/inventario/transferencias', 'bi-arrow-left-right', 7, GETDATE(), GETDATE(), 1, 0);

-- =====================================================
-- CATEGORÍA: CLIENTES Y COBROS
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Cobrar cuotas a clientes')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Clientes', 'Cobros', 'Cobrar cuotas a clientes',
'Para **registrar un cobro** de cliente:

1️⃣ Ve a **Ventas → Cuentas por Cobrar**
2️⃣ Selecciona el **cliente**
3️⃣ Verás las **cuotas pendientes**
4️⃣ Selecciona las cuotas a cobrar
5️⃣ Ingresa el **monto recibido**
6️⃣ Selecciona la **forma de pago**
7️⃣ **Confirma** el cobro

📊 Para ver historial: **Cobros → Historial de Cobros**
📋 Para ver deudas: **Informes → Cuentas por Cobrar**',
'cobro, cobrar cliente, cuota, deuda cliente, credito, cuentas por cobrar, pago cliente', '/cobros', 'bi-currency-dollar', 8, GETDATE(), GETDATE(), 1, 0);

-- =====================================================
-- CATEGORÍA: CONFIGURACIÓN
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Configurar datos de la empresa')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Configuración', 'Empresa', 'Configurar datos de la empresa',
'Para **configurar los datos de la empresa**:

1️⃣ Ve a **Configuración → Sociedad/Empresa**
2️⃣ Completa los datos:
   - **Razón Social**: nombre legal de la empresa
   - **RUC**: número de contribuyente
   - **Dirección**: dirección fiscal
   - **Teléfono** y **correo**
3️⃣ Sube el **logo** de la empresa
4️⃣ **Guarda** los cambios

💡 Estos datos aparecen en facturas y documentos impresos.',
'empresa, sociedad, razon social, ruc, datos empresa, configurar empresa, logo', '/configuracion/sociedad', 'bi-building', 7, GETDATE(), GETDATE(), 1, 0);

IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Configurar timbrado y facturación electrónica')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Configuración', 'SIFEN', 'Configurar timbrado y facturación electrónica',
'Para **configurar SIFEN** (Facturación Electrónica):

**1. Configurar Certificado Digital:**
- Ve a **Configuración → Sociedad**
- Carga el archivo **.pfx** del certificado
- Ingresa la **contraseña** del certificado

**2. Configurar Timbrado:**
- Ve a **Configuración → Cajas**
- Ingresa el **número de timbrado**
- Configura la **vigencia** (desde/hasta)
- Define el **número inicial** de facturas

**3. Seleccionar Ambiente:**
- **Test**: para pruebas (no válido fiscalmente)
- **Producción**: facturas reales

⚠️ El certificado debe estar vigente y ser emitido por el SET.',
'sifen, timbrado, factura electronica, certificado, set, cdc, vigencia, ambiente', '/configuracion/cajas', 'bi-patch-check', 9, GETDATE(), GETDATE(), 1, 0);

-- =====================================================
-- CATEGORÍA: BACKUP Y SISTEMA
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Hacer backup de la base de datos')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Sistema', 'Backup', 'Hacer backup de la base de datos',
'Para **realizar un backup** de la base de datos:

**Opción 1 - Desde SQL Server Management Studio:**
1. Abre SSMS y conecta al servidor
2. Click derecho en la base de datos **asiswebapp**
3. Tareas → **Copia de seguridad**
4. Selecciona destino y nombre del archivo .bak
5. Click en **Aceptar**

**Opción 2 - Comando SQL:**
```sql
BACKUP DATABASE asiswebapp 
TO DISK = ''C:\Backups\asiswebapp_YYYYMMDD.bak''
WITH FORMAT, COMPRESSION;
```

💡 **Recomendaciones**:
- Haz backup **diario** al menos
- Guarda copias en **ubicación externa** (nube, disco externo)
- Prueba restaurar periódicamente para verificar
- Programa backups automáticos en SQL Server Agent',
'backup, copia seguridad, respaldo, guardar datos, respaldar, base datos, bak', NULL, 'bi-hdd', 10, GETDATE(), GETDATE(), 1, 0);

IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Restaurar backup de base de datos')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Sistema', 'Backup', 'Restaurar backup de base de datos',
'Para **restaurar un backup**:

**Desde SQL Server Management Studio:**
1. Abre SSMS
2. Click derecho en **Bases de datos**
3. **Restaurar base de datos...**
4. Selecciona **Dispositivo** → busca el archivo .bak
5. Verifica el nombre de la base de datos destino
6. Click en **Aceptar**

⚠️ **Importante**:
- Cierra la aplicación antes de restaurar
- La restauración **sobrescribe** todos los datos actuales
- Haz un backup del estado actual antes de restaurar
- Si la BD está en uso, marca "Cerrar conexiones existentes"',
'restaurar, restore, recuperar, recuperar backup, cargar backup, reestablecer', NULL, 'bi-arrow-counterclockwise', 9, GETDATE(), GETDATE(), 1, 0);

IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Actualizar el sistema')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Sistema', 'Mantenimiento', 'Actualizar el sistema',
'Para **actualizar SistemIA**:

1️⃣ Ve a **Configuración → Actualización Sistema**
2️⃣ Haz clic en **Buscar Actualizaciones**
3️⃣ Si hay versión nueva disponible:
   - Revisa las **notas de la versión**
   - Haz **backup** antes de actualizar
   - Click en **Descargar e Instalar**
4️⃣ Reinicia la aplicación cuando termine

💡 **Recomendaciones**:
- Siempre haz backup antes de actualizar
- No interrumpas el proceso de actualización
- Actualiza fuera de horario pico
- Lee las notas de versión por cambios importantes',
'actualizar, update, version, nueva version, parche, actualizacion sistema', '/actualizacion-sistema', 'bi-cloud-download', 8, GETDATE(), GETDATE(), 1, 0);

-- =====================================================
-- CATEGORÍA: INFORMES
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Generar informes de ventas')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Informes', 'Ventas', 'Generar informes de ventas',
'Para **generar informes de ventas**:

1️⃣ Ve a **Informes** en el menú principal
2️⃣ Selecciona el tipo de informe:

📊 **Ventas Agrupado**: totales por día/vendedor/forma de pago
📋 **Ventas Detallado**: cada venta con sus productos
📈 **Ventas por Clasificación**: agrupado por categoría de producto
💰 **Resumen de Caja**: movimientos de efectivo

3️⃣ Selecciona el **rango de fechas**
4️⃣ Aplica **filtros** (cliente, vendedor, etc.)
5️⃣ Click en **Generar**

💡 Puedes **exportar a Excel** o **imprimir** los informes.',
'informe venta, reporte venta, estadistica venta, resumen venta, ver ventas', '/informes/ventas-agrupado', 'bi-graph-up', 8, GETDATE(), GETDATE(), 1, 0);

IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Ver cuentas por cobrar')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Informes', 'Financieros', 'Ver cuentas por cobrar',
'Para ver las **cuentas por cobrar** (deudas de clientes):

1️⃣ Ve a **Informes → Cuentas por Cobrar**
2️⃣ Filtra por:
   - **Cliente específico** o todos
   - **Estado**: vencidas, por vencer, todas
   - **Rango de fechas**
3️⃣ El informe muestra:
   - Total adeudado por cliente
   - Cuotas pendientes con vencimientos
   - Días de atraso

💡 **Acciones**:
- Click en un cliente para ver detalle
- Exportar a Excel para seguimiento
- Desde aquí puedes ir a registrar cobros',
'cuentas cobrar, deudas clientes, creditos pendientes, morosos, vencidos, cartera', '/informes/cuentas-por-cobrar', 'bi-people', 8, GETDATE(), GETDATE(), 1, 0);

-- =====================================================
-- CATEGORÍA: USUARIOS Y PERMISOS
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Crear nuevo usuario')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Usuarios', 'Gestión', 'Crear nuevo usuario',
'Para **crear un nuevo usuario**:

1️⃣ Ve a **Personal → Gestión de Usuarios**
2️⃣ Click en **Nuevo Usuario**
3️⃣ Completa los datos:
   - **Nombre de usuario** (para login)
   - **Contraseña**
   - **Nombres y apellidos**
   - **Rol** (Administrador, Vendedor, etc.)
4️⃣ Configura los **permisos** específicos
5️⃣ **Guarda** el usuario

💡 Los roles determinan los permisos base, pero puedes personalizar permisos individuales.',
'usuario, crear usuario, nuevo usuario, agregar usuario, empleado, personal', '/menu-usuarios', 'bi-person-plus', 8, GETDATE(), GETDATE(), 1, 0);

IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Configurar permisos de usuario')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Usuarios', 'Permisos', 'Configurar permisos de usuario',
'Para **configurar permisos**:

1️⃣ Ve a **Personal → Permisos de Usuarios**
2️⃣ Selecciona el **usuario** o **rol**
3️⃣ Marca/desmarca los permisos por módulo:
   - ✅ Ver (acceso al módulo)
   - ✅ Crear (agregar registros)
   - ✅ Editar (modificar)
   - ✅ Eliminar (borrar)
   - ✅ Anular (anular documentos)
4️⃣ **Guarda** los cambios

⚠️ **Roles predefinidos**:
- **Administrador**: acceso total
- **Vendedor**: ventas y cobros
- **Cajero**: solo caja y ventas',
'permisos, acceso, roles, restriccion, seguridad, configurar permisos', '/personal/permisos-usuarios', 'bi-shield-lock', 8, GETDATE(), GETDATE(), 1, 0);

-- =====================================================
-- CATEGORÍA: PRODUCTOS
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Crear nuevo producto')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Productos', 'Gestión', 'Crear nuevo producto',
'Para **crear un nuevo producto**:

1️⃣ Ve a **Productos → Administrar Productos**
2️⃣ Click en **Nuevo Producto**
3️⃣ Completa los datos obligatorios:
   - **Código** (único, puede ser código de barras)
   - **Descripción** del producto
   - **Precio de venta**
   - **Tipo de IVA** (10%, 5%, Exenta)
4️⃣ Datos opcionales:
   - Categoría y marca
   - Precio de costo
   - Stock mínimo
   - Imagen del producto
5️⃣ **Guarda** el producto

💡 El stock inicial se carga con una compra o ajuste de inventario.',
'producto, crear producto, nuevo producto, agregar producto, articulo, item', '/productos', 'bi-box', 9, GETDATE(), GETDATE(), 1, 0);

IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Configurar precios diferenciados')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Productos', 'Precios', 'Configurar precios diferenciados',
'Para configurar **precios diferenciados** por cliente:

1️⃣ Ve a **Configuración → Precios y Descuentos**
2️⃣ Crea **Listas de Precios** (Mayorista, Minorista, etc.)
3️⃣ Asigna precios específicos por producto en cada lista
4️⃣ Asigna la lista al cliente en su ficha

**Opciones de precio**:
- Precio fijo por lista
- Descuento porcentual sobre precio base
- Precio por cantidad (escalas)

💡 Al vender, el sistema aplica automáticamente el precio de la lista asignada al cliente.',
'precio, lista precio, descuento, mayorista, minorista, precio especial, cliente precio', '/configuracion/precios-descuentos', 'bi-tags', 7, GETDATE(), GETDATE(), 1, 0);

-- =====================================================
-- CATEGORÍA: CORREO ELECTRÓNICO
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Configurar envío automático de correo')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Configuración', 'Correo', 'Configurar envío automático de correo',
'Para configurar el **envío automático de correo**:

1️⃣ Ve a **Configuración → Correo Electrónico**
2️⃣ Configura el **servidor SMTP**:
   - Servidor: smtp.gmail.com (para Gmail)
   - Puerto: 587
   - Usar SSL: Sí
3️⃣ Ingresa las **credenciales**:
   - Usuario: tu correo
   - Contraseña: contraseña de aplicación (16 caracteres)
4️⃣ Configura los **destinatarios** y qué informes reciben
5️⃣ Activa **Enviar al cierre** o **Resumen diario**

💡 **Para Gmail**: 
- Activa verificación en 2 pasos
- Crea contraseña de aplicación en seguridad de Google
- Usa esa contraseña (xxxx xxxx xxxx xxxx) en el sistema',
'correo, email, smtp, enviar correo, notificacion, gmail, outlook, informe email', '/configuracion/correo', 'bi-envelope', 9, GETDATE(), GETDATE(), 1, 0);

-- =====================================================
-- CATEGORÍA: PRESUPUESTOS
-- =====================================================
IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = 'Crear un presupuesto')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES ('Ventas', 'Presupuestos', 'Crear un presupuesto',
'Para **crear un presupuesto**:

1️⃣ Ve a **Ventas → Presupuestos**
2️⃣ Click en **Nuevo Presupuesto**
3️⃣ Selecciona el **cliente**
4️⃣ Agrega los **productos** con precios y cantidades
5️⃣ Define la **validez** del presupuesto (días)
6️⃣ **Guarda** el presupuesto

**Opciones posteriores**:
- ✅ **Convertir a Venta**: cuando el cliente acepta
- 📧 **Enviar por correo**: al cliente
- 🖨️ **Imprimir**: para entregar físicamente

💡 Los presupuestos no afectan stock ni generan movimientos fiscales.',
'presupuesto, cotizacion, proforma, precio estimado, crear presupuesto', '/presupuestos/explorar', 'bi-file-earmark-text', 8, GETDATE(), GETDATE(), 1, 0);

PRINT 'Artículos de conocimiento insertados correctamente';
