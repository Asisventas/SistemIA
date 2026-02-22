# Guía del Módulo de Compras - SistemIA

## 📋 Descripción General

El **módulo de Compras** permite registrar las compras de mercaderías y servicios a proveedores, controlar los documentos fiscales recibidos, gestionar el ingreso de inventario con control de lotes y vencimientos, y administrar las cuentas por pagar.

---

## 🗃️ Modelo de Datos

### 1. Compra (Cabecera)

**Modelo:** `Models/Compra.cs`

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `IdCompra` | int (PK) | Identificador único interno |
| `IdSucursal` | int (FK) | Sucursal donde se registra |
| `IdProveedor` | int (FK) | Proveedor de la compra |
| `IdUsuario` | int (FK) | Usuario que registra |
| `IdMoneda` | int (FK) | Moneda de la operación |
| `IdDeposito` | int (FK) | Depósito predeterminado |
| `IdCaja` | int? (FK) | Caja donde se registra |
| `IdTipoPago` | int? (FK) | Tipo de pago (Contado/Crédito) |
| `IdTipoDocumentoOperacion` | int? (FK) | Tipo de documento (Factura, NC, etc.) |
| `Fecha` | DateTime | Fecha de la compra |
| `FechaVencimiento` | DateTime? | Fecha vencimiento (crédito) |
| `Turno` | int? | Turno de trabajo |
| **Documento Fiscal** | | |
| `Establecimiento` | string(3) | Código de establecimiento proveedor |
| `PuntoExpedicion` | string(3) | Punto de expedición proveedor |
| `NumeroFactura` | string(15) | Número de factura |
| `Timbrado` | string(15) | Timbrado del proveedor |
| **Operación** | | |
| `Total` | decimal(18,4) | Total de la compra |
| `TotalEnLetras` | string | Total expresado en palabras |
| `FormaPago` | string | "Contado" / "Crédito" |
| `MedioPago` | string | EFECTIVO/TARJETA/CHEQUE/TRANSFERENCIA/QR |
| `CodigoCondicion` | string | CONTADO / CREDITO |
| `PlazoDias` | int? | Plazo en días (crédito) |
| **Multi-Moneda** | | |
| `EsMonedaExtranjera` | bool | Si la compra es en moneda extranjera |
| `CambioDelDia` | decimal? | Tipo de cambio usado |
| `SimboloMoneda` | string | Símbolo de la moneda |
| **Imputación Fiscal** | | |
| `ImputarIVA` | bool | Si imputa al IVA |
| `ImputarIRP` | bool | Si imputa al IRP |
| `ImputarIRE` | bool | Si imputa al IRE |
| `NoImputar` | bool | Si no imputa a ningún impuesto |
| **Estado** | | |
| `Estado` | string | Borrador / Confirmado / Anulado |
| `TipoDocumento` | string | FACTURA / N/C / RECIBO |
| `TipoIngreso` | string | COMPRA / GASTO / etc. |

### 2. CompraDetalle (Líneas de Compra)

**Modelo:** `Models/CompraDetalle.cs`

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `IdCompraDetalle` | int (PK) | Identificador único |
| `IdCompra` | int (FK) | Compra a la que pertenece |
| `IdProducto` | int (FK) | Producto comprado |
| `IdDepositoItem` | int? (FK) | Depósito específico del ítem |
| **Cantidades y Precios** | | |
| `Cantidad` | decimal | Cantidad comprada (unidades) |
| `PrecioUnitario` | decimal(18,4) | Costo unitario |
| `Importe` | decimal(18,4) | Importe total de la línea |
| **Desglose IVA** | | |
| `IVA10` | decimal | IVA 10% de la línea |
| `IVA5` | decimal | IVA 5% de la línea |
| `Exenta` | decimal | Monto exento |
| `Grabado10` | decimal | Base gravada 10% |
| `Grabado5` | decimal | Base gravada 5% |
| **Cálculo de Precio de Venta** | | |
| `PrecioVentaRef` | decimal | Precio de venta de referencia |
| `FactorMultiplicador` | decimal? | Factor para calcular precio venta |
| `PorcentajeMargen` | decimal? | Porcentaje de margen/markup |
| `PrecioMinisterio` | decimal? | Precio Ministerio (farmacia) |
| **Control de Lotes** | | |
| `IdProductoLote` | int? (FK) | Lote creado/asignado |
| `NumeroLote` | string(50) | Número de lote |
| `FechaVencimientoItem` | DateTime? | Fecha de vencimiento del lote |
| **Modo Paquete** | | |
| `ModoIngresoPersistido` | string? | "paquete" o "unidad" |
| `CantidadPorPaqueteMomento` | int? | Unidades por paquete al momento |
| `PrecioPaqueteMomento` | decimal? | Precio del paquete al momento |
| `PrecioMinisterioPaqueteMomento` | decimal? | P. Ministerio paquete al momento |
| **Multi-Moneda** | | |
| `CambioDelDia` | decimal? | Tipo de cambio del ítem |

---

## 📄 Páginas del Módulo

### 1. Registro de Compras (`/compras`)

**Archivo:** `Pages/Compras.razor`

Página principal para el registro de compras con las siguientes secciones:

#### Encabezado
- **Información de Caja**: Muestra caja actual, turno y fecha de caja
- **Botones**: Reimprimir, Ir a Explorador

#### Datos del Proveedor
- Buscador con autocompletado por nombre o RUC
- Consulta automática al SIFEN (RUC Service) al presionar Tab
- Creación rápida de proveedor mediante modal
- Visualización del timbrado y vencimiento del proveedor

#### Documento Fiscal
- **Establecimiento** (3 dígitos): Código del establecimiento del proveedor
- **Punto de Expedición** (3 dígitos): Punto de emisión del proveedor
- **Número de Factura** (15 dígitos): Número del documento
- **Timbrado** (8-15 dígitos): Timbrado del documento
- **Vencimiento Timbrado**: Fecha de vigencia

#### Tipo de Documento y Pago
- **Tipo Documento**: FACTURA, N/C, RECIBO, etc.
- **Tipo de Pago**: Contado o Crédito (configurable)
- **Medio de Pago**: EFECTIVO, TARJETA, CHEQUE, TRANSFERENCIA, QR

#### Configuración de Crédito (si aplica)
- **Número de Cuotas**: Cantidad de cuotas a generar
- **Plazo en Días**: Días entre cuotas
- **Fecha de Vencimiento**: Primera cuota

#### Multi-Moneda
- **Moneda**: Selector de moneda (PYG, USD, etc.)
- **Tipo de Cambio**: Se carga automáticamente del día, editable

#### Agregar Productos
- **Buscador de Productos**: Autocompletado por descripción o código
- **Modo Ingreso**: Selector Paquete/Unidad
- **Cantidad**: Cantidad a ingresar
- **Precio Compra**: Costo del producto (total paquete o unitario según modo)
- **Factor**: Multiplicador para calcular precio de venta
- **% Mark-up**: Porcentaje de margen (se sincroniza con factor)
- **Precio de Venta**: Precio calculado o manual
- **Depósito**: Depósito destino (puede variar por línea)
- **Número de Lote**: Si el producto controla lotes
- **Fecha de Vencimiento**: Si el producto controla vencimiento (OBLIGATORIO)
- **Precio Ministerio**: Solo si está habilitado modo farmacia

#### Tabla de Detalles
Columnas mostradas:
- Producto (descripción)
- Depósito
- Lote
- Vencimiento
- Paq/Unid (modo de ingreso)
- Precio Compra
- Factor
- Markup %
- Precio Venta
- TC (tipo de cambio)
- IVA (porcentaje)
- Cantidad
- Total

#### Resumen de Totales
- Gravado 10%
- Gravado 5%
- IVA 10%
- IVA 5%
- Exentas
- **TOTAL**

### 2. Explorador de Compras (`/compras/explorar`)

**Archivo:** `Pages/ComprasExplorar.razor`

#### Filtros de Búsqueda
- **Nº Interno**: Buscar por ID de compra
- **RUC/Razón Social**: Buscar por proveedor
- **Desde/Hasta**: Rango de fechas
- **Estado**: Filtrar por estado (Borrador/Confirmado/Anulado)

#### Exportación
- **CSV**: Exportar listado a CSV con separador `;`
- **XLSX**: Exportar a Excel con formato

#### Tabla de Resultados
| Columna | Descripción |
|---------|-------------|
| Nº Int. | ID interno de la compra |
| Fecha | Fecha de la operación |
| Proveedor | Nombre + RUC del proveedor |
| Documento | Nº de factura + Timbrado |
| Total | Monto total |
| Estado | Badge con color según estado |

#### Acciones por Registro
- **Reimprimir**: Abre la compra e imprime automáticamente
- **Ver detalles**: Abre la compra en modo solo lectura
- **Recuperar**: Solo para compras Anuladas - repone el stock

#### Paginación
- 100 registros por página
- Navegación: Primera, Anterior, [páginas], Siguiente, Última

---

## 🏪 Sistema de Proveedores

### Búsqueda de Proveedores
1. El usuario escribe en el campo de búsqueda
2. Se filtran proveedores por nombre o RUC
3. Al seleccionar, se cargan los datos de timbrado

### Consulta SIFEN (RUC Service)
Al presionar **Tab** en el campo de búsqueda:
1. Si el texto es un RUC válido (8+ dígitos con guión DV)
2. Se consulta el servicio RUC del SET
3. Si existe, se muestra información del contribuyente
4. Si no existe en BD local, se ofrece crear

### Creación Rápida de Proveedor
Modal con campos:
- **Razón Social**: Nombre del proveedor
- **RUC**: Número de RUC (se puede buscar en SIFEN)
- **DV**: Dígito verificador
- **Timbrado**: Timbrado del proveedor
- **Vencimiento Timbrado**: Fecha de vigencia

---

## 💱 Sistema Multi-Moneda

### Configuración
- **Moneda Base**: Guaraníes (PYG) - configurado en `Monedas.EsMonedaBase`
- **Monedas Extranjeras**: USD, BRL, ARS, etc.

### Tipo de Cambio
1. Al seleccionar una moneda extranjera
2. Se busca el tipo de cambio del día (`TiposCambio`)
3. Se usa la **Tasa de Compra** (para compras a proveedores)
4. El usuario puede modificar manualmente el TC

### Conversión de Precios
- Los precios de productos están en Guaraníes (`CostoUnitarioGs`, `PrecioUnitarioGs`)
- Al comprar en moneda extranjera, se dividen por el TC
- Al guardar, se actualiza el costo en Guaraníes

---

## 💳 Tipos de Pago y Crédito

### Pago Contado
- `CodigoCondicion = "CONTADO"`
- No genera cuenta por pagar
- Medios: EFECTIVO, TARJETA, CHEQUE, TRANSFERENCIA, QR

### Pago Crédito
- `CodigoCondicion = "CREDITO"`
- `TipoPago.EsCredito = true`
- Genera **CuentaPorPagar** con cuotas

### Generación Automática de Cuotas
Al guardar una compra a crédito:
1. Se crea `CuentaPorPagar` con el monto total
2. Se generan N cuotas (`CuentaPorPagarCuota`)
3. Primera cuota vence en `FechaVencimiento`
4. Siguientes cuotas cada `PlazoDias` días
5. Última cuota ajusta centavos

---

## 📦 Sistema de Lotes en Compras

### Productos con Control de Lotes
Cuando `Producto.ControlaLote = true`:

#### Al Agregar Línea
1. Se habilita el campo **Número de Lote**
2. Si `ControlaVencimiento = true`, el campo **Fecha de Vencimiento** es **OBLIGATORIO**

#### Al Guardar la Compra
1. **Si se especificó número de lote**:
   - Se busca lote existente con ese número
   - Si existe: se incrementa el stock del lote
   - Si no existe: se crea nuevo lote

2. **Si NO se especificó número de lote**:
   - Se genera automáticamente: `C{IdCompra}-{fecha}`
   - Formato ejemplo: `C123-20260128`

### Creación de Lotes
```csharp
await LoteService.CrearLoteAsync(
    idProducto,
    idDeposito,
    numeroLote,
    fechaVencimiento,
    stockInicial: cantidad,
    costoUnitario: precioCompra,
    idCompra,
    idCompraDetalle,
    usuario
);
```

### Productos Tipo Gasto
Los productos marcados como **Gasto** (`TipoItem` en lista `_tiposGastoIds`):
- NO afectan inventario
- NO permiten lote ni vencimiento
- Solo registran el gasto contablemente

---

## 🧮 Cálculo de Precios de Venta

### Modo Paquete vs Unidad

El sistema permite ingresar compras por **paquete** o por **unidad**:

| Modo | Cantidad Real | Precio Mostrado | Factor Aplica A |
|------|---------------|-----------------|-----------------|
| Paquete | `CantidadIngresada × CantPorPaq` | Precio del paquete completo | Precio paquete |
| Unidad | `CantidadIngresada` | Precio unitario | Precio unitario |

### Factor Multiplicador

El **Factor** multiplica el costo para obtener el precio de venta:

```
Precio Venta = Costo × Factor
```

**Ejemplo:**
- Costo: Gs 10.000
- Factor: 1.30
- Precio Venta: Gs 13.000

### Porcentaje de Mark-up

El **Mark-up** es el porcentaje de ganancia sobre el costo:

```
Mark-up = (Factor - 1) × 100
```

**Ejemplo:**
- Factor: 1.30
- Mark-up: 30%

### Sincronización Factor ↔ Mark-up
- Al cambiar el Factor, se recalcula el Mark-up
- Al cambiar el Mark-up, se recalcula el Factor
- Al ingresar Precio de Venta directamente, se calculan ambos

### Actualización de Precios del Producto
Al guardar la compra, se actualizan automáticamente:

**Modo Unidad:**
- `Producto.CostoUnitarioGs` = Costo de compra
- `Producto.PrecioUnitarioGs` = Precio venta calculado
- `Producto.FactorMultiplicador` = Factor ingresado

**Modo Paquete:**
- `Producto.CostoPaqueteGs` = Costo paquete
- `Producto.PrecioPaqueteGs` = Precio venta paquete
- `Producto.FactorPaquete` = Factor paquete

---

## 🏥 Modo Farmacia (Precio Ministerio)

### Configuración
En `ConfiguracionSistema`:
- `FarmaciaModoActivo`: Habilita funciones de farmacia
- `FarmaciaMostrarPrecioMinisterioEnCompras`: Muestra campo P. Ministerio
- `FarmaciaValidarPrecioMinisterio`: Valida que precio venta ≤ P. Ministerio

### Campo Precio Ministerio
- Se muestra en el formulario de agregar línea
- Se guarda en `CompraDetalle.PrecioMinisterio`
- Al guardar, actualiza `Producto.PrecioMinisterio`

### Validación
Si `ValidarPrecioMinisterio = true`:
```csharp
if (precioVenta > precioMinisterio)
{
    Error = "El precio de venta supera el Precio Ministerio";
}
```

---

## 🖨️ Impresión de Compras

### Formato de Impresión
Al guardar una compra, se genera automáticamente un comprobante en formato **A4** con:
- Datos de la empresa
- Logo (si existe)
- Datos del proveedor
- Documento fiscal completo
- Detalle de productos con precios
- Totales desglosados por IVA

### Reimpresión
Desde el explorador o la página principal:
1. Se carga la compra completa
2. Se genera el HTML de impresión
3. Se abre en nueva ventana para imprimir

---

## ♻️ Funcionalidad Reciclar Compra

### Propósito
Permite eliminar una compra existente **manteniendo los datos en pantalla** para corregir y guardar como nueva.

### Requisitos
- Usuario con rol **Administrador**
- Permisos **EDIT** y **DELETE** en módulo Compras
- Confirmar con contraseña del usuario

### Proceso
1. **Verificar** que no haya pagos ni NC asociadas
2. **Revertir stock**: Ajuste de salida por cada línea
3. **Eliminar registros**: Cuenta por pagar, cuotas, detalles, compra
4. **Preparar edición**: IdCompra = 0, detalles en memoria
5. El usuario puede modificar y guardar como nueva compra

### Restricciones
No se puede reciclar si existen:
- Órdenes de pago confirmadas
- Notas de crédito asociadas

---

## ✅ Validaciones Principales

### Al Agregar Línea
| Validación | Mensaje |
|------------|---------|
| Producto requerido | "Debe seleccionar un producto" |
| Cantidad > 0 | "La cantidad debe ser mayor a 0" |
| Precio > 0 | "El precio debe ser mayor a 0" |
| Fecha vencimiento (si controla) | "⚠️ OBLIGATORIO: Debe ingresar la fecha de vencimiento" |
| Precio ≤ P. Ministerio (si activo) | "El precio supera el Precio Ministerio" |

### Al Guardar Compra
| Validación | Mensaje |
|------------|---------|
| Proveedor requerido | "Debe seleccionar un proveedor" |
| Al menos 1 detalle | "Debe agregar al menos un producto" |
| Depósito configurado | "Debe seleccionar un depósito" |
| Datos fiscales completos | "Complete el número de factura" |

---

## 🔄 Integración con Otros Módulos

### Inventario
- **Entrada de stock**: Al confirmar compra, +stock en depósito
- **Salida de stock**: Al anular/reciclar, -stock

### Cuentas por Pagar
- **Crear deuda**: Al comprar a crédito
- **Generar cuotas**: Automático según configuración

### Lotes (FEFO)
- **Crear lote**: Automático o manual
- **Incrementar stock**: Si lote existente

### Productos
- **Actualizar costos**: CostoUnitarioGs, CostoPaqueteGs
- **Actualizar precios**: Si se ingresó factor/precio venta
- **Actualizar vencimiento**: FechaVencimiento del producto

### Auditoría
- Registro de cada compra creada con detalle completo

---

## ⌨️ Atajos de Teclado

| Tecla | Acción |
|-------|--------|
| **F2** | Guardar compra |
| **Tab** (en proveedor) | Buscar RUC en SIFEN |
| **Enter** (en sugerencias) | Seleccionar elemento |
| **Tab** (en Agregar) | Agregar línea y volver a producto |

---

## 📊 Estados de Compra

| Estado | Badge | Descripción |
|--------|-------|-------------|
| `Borrador` | Gris | Compra no confirmada |
| `Confirmado` | Verde | Compra procesada |
| `Anulado` | Rojo | Compra revertida |

---

## 🛡️ Permisos Requeridos

| Permiso | Acción |
|---------|--------|
| **VIEW** | Ver compras, explorador |
| **CREATE** | Crear nuevas compras |
| **EDIT** | Modificar compras (borrador) |
| **DELETE** | Anular/Eliminar compras |

### Permisos Especiales
- **Reciclar Compra**: Requiere rol Admin + EDIT + DELETE

---

## 📁 Archivos Relacionados

| Archivo | Propósito |
|---------|-----------|
| `Models/Compra.cs` | Modelo de cabecera |
| `Models/CompraDetalle.cs` | Modelo de líneas |
| `Pages/Compras.razor` | Página de registro |
| `Pages/ComprasExplorar.razor` | Explorador/listado |
| `Services/LoteService.cs` | Gestión de lotes |
| `Services/InventarioService.cs` | Ajustes de stock |
| `Services/CajaService.cs` | Info de caja actual |
| `Services/RucService.cs` | Consulta SIFEN RUC |

---

## 💡 Consejos de Uso

### Para Farmacia
1. Activar `FarmaciaModoActivo` en configuración
2. Habilitar validación de Precio Ministerio
3. Siempre ingresar el P. Ministerio en compras

### Para Productos con Lotes
1. Asegurar que el producto tenga `ControlaLote = true`
2. Si `ControlaVencimiento = true`, la fecha es OBLIGATORIA
3. Usar numeración de lotes consistente (ej: LOTE-PROVEEDOR-FECHA)

### Para Compras a Crédito
1. Seleccionar tipo de pago crédito
2. Configurar número de cuotas
3. Definir plazo entre cuotas
4. Establecer fecha de primer vencimiento

### Para Multi-Moneda
1. Configurar tipos de cambio del día previamente
2. Al seleccionar moneda, se carga TC automático
3. Verificar/ajustar TC antes de guardar

---

*Documentación generada para referencia del Asistente IA - SistemIA v2.0*
