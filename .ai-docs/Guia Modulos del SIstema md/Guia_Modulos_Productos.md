# 📦 Guía del Módulo de Productos - SistemIA

## Descripción General

El módulo de **Productos** es el núcleo del sistema de gestión de inventario de SistemIA. Permite administrar productos, servicios, configuración de precios, control de lotes y vencimientos. Este documento sirve como referencia técnica para la IA del sistema y como base para generar manuales de usuario.

---

## 📋 Índice de Contenidos

1. [Modelo de Datos](#modelo-de-datos)
2. [Funcionalidades Principales](#funcionalidades-principales)
3. [Sistema de Paquetes](#sistema-de-paquetes)
4. [Control de Lotes y Vencimientos](#control-de-lotes-y-vencimientos)
5. [Configuración de Precios](#configuración-de-precios)
6. [Descuentos por Producto](#descuentos-por-producto)
7. [Stock por Depósito](#stock-por-depósito)
8. [Productos Combo](#productos-combo)
9. [Interfaz de Usuario](#interfaz-de-usuario)
10. [Validaciones y Reglas de Negocio](#validaciones-y-reglas-de-negocio)
11. [Integración con Otros Módulos](#integración-con-otros-módulos)
12. [Preguntas Frecuentes](#preguntas-frecuentes)

---

## 1. Modelo de Datos

### Entidad Principal: `Producto`

**Tabla:** `Productos`

#### Campos de Identificación
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `IdProducto` | int (PK) | Identificador único |
| `CodigoInterno` | string(50) | Código interno del sistema (SIFEN: dCodInt) |
| `Descripcion` | string(200) | Nombre del producto o servicio (SIFEN: dDesProSer) |
| `CodigoBarras` | string(14) | Código de barras GTIN (8/12/13/14 dígitos) |
| `Foto` | string(180) | Ruta o URL de la imagen del producto |

#### Campos de Clasificación
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `TipoItem` | int | 1=Producto, 2=Servicio |
| `IdMarca` | int? | Relación con tabla Marcas |
| `IdClasificacion` | int? | Relación con tabla Clasificaciones |
| `IdTipoIva` | int | Tipo de IVA aplicable (10%, 5%, Exento) |
| `IdSucursal` | int | Sucursal propietaria del producto |

#### Campos de Unidades
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `UndMedida` | char(10) | Descripción corta (UNIDAD, CAJA, etc.) |
| `UnidadMedidaCodigo` | string(3) | Código SIFEN (77=Unidad, 006=Paquete) |
| `CantidadPorPaquete` | decimal? | Unidades contenidas en un paquete/caja |
| `PermiteVentaPorUnidad` | bool | Si se puede vender unitario y por paquete |

#### Campos de Precios
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `CostoUnitarioGs` | decimal? | Costo de adquisición por unidad |
| `PrecioUnitarioGs` | decimal | Precio de venta por unidad |
| `PrecioUnitarioUsd` | decimal? | Precio en dólares (opcional) |
| `PrecioMinisterio` | decimal? | Precio máximo regulado (farmacias) |
| `FactorMultiplicador` | decimal? | Factor para calcular precio desde costo |

#### Campos de Paquete
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `CostoPaqueteGs` | decimal? | Costo por paquete completo |
| `PrecioPaqueteGs` | decimal? | Precio de venta por paquete |
| `FactorPaquete` | decimal? | Factor de margen para paquete |
| `MarkupPaquetePct` | decimal? | Porcentaje de mark-up para paquete |
| `PrecioMinisterioPaquete` | decimal? | Precio máximo por paquete (farmacias) |

#### Campos de Control de Stock
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Stock` | decimal | Stock actual total |
| `StockMinimo` | decimal | Nivel mínimo para alerta |
| `IdDepositoPredeterminado` | int? | Depósito por defecto |
| `PermiteDecimal` | bool | Permite vender fracciones (ej: 0.5 kg) |

#### Campos de Control de Lotes
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `ControlaLote` | bool | Activa gestión de lotes |
| `ControlaVencimiento` | bool | Activa control de vencimientos |
| `DiasAlertaVencimiento` | int | Días de anticipación para alertas (default: 30) |
| `PermiteVentaVencido` | bool | Permite vender productos vencidos |
| `LoteInicialCreado` | bool | Indica si se migró stock a lotes |
| `FechaVencimiento` | DateTime? | Vencimiento simple (sin lotes) |
| `ControlarVencimiento` | bool | Control de vencimiento sin lotes |

#### Campos de Descuentos
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `PermiteDescuento` | bool | Si se pueden aplicar descuentos |
| `PermiteVentaBajoCosto` | bool | Permite vender bajo el costo |
| `UsaDescuentoEspecifico` | bool | Usa descuento propio vs configuración |
| `DescuentoAutomaticoProducto` | decimal? | % de descuento automático |
| `DescuentoMaximoProducto` | decimal? | % máximo de descuento permitido |
| `MargenAdicionalCajeroProducto` | decimal? | % adicional que puede dar el cajero |

#### Otros Campos
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `EsCombo` | bool | Si es producto compuesto (descuenta componentes) |
| `ControladoReceta` | bool | Medicamento controlado con receta |
| `Activo` | bool | Estado del producto |

---

## 2. Funcionalidades Principales

### 2.1 Listado de Productos (Explorador)

**Ruta:** `/productos`

El explorador muestra todos los productos con las siguientes columnas:
- **Foto**: Miniatura del producto (48x48px)
- **Código**: Código interno con badge
- **Descripción**: Nombre + código de barras si existe
- **Unidad**: Código de unidad SIFEN + unidades por paquete
- **IVA**: Tipo de IVA aplicable
- **Costo Gs**: Costo unitario (+ costo paquete si aplica)
- **Precio Gs**: Precio unitario (+ precio paquete si aplica)
- **P.Min.**: Precio Ministerio (solo modo farmacia)
- **Stock**: Total con desglose por depósito y paquetes
- **Marca**: Marca del producto
- **Sucursal**: Sucursal asignada
- **Vencimiento**: Fecha y días restantes
- **Estado**: Activo/Inactivo
- **Acciones**: Botones de operaciones

### 2.2 Filtros Disponibles

| Filtro | Descripción |
|--------|-------------|
| Texto | Busca en descripción, código o código de barras |
| Depósito | Filtra por stock en depósito específico |
| IVA | Filtra por tipo de IVA |
| Estado | Activos / Todos |

### 2.3 Acciones por Producto

| Acción | Icono | Descripción |
|--------|-------|-------------|
| Ver Lotes | `bi-collection` | Abre modal solo lectura de lotes (si controla lote) |
| Stock por Depósito | `bi-box-seam` | Muestra desglose de stock por depósito |
| Etiqueta | `bi-upc` | Genera código de barras para impresión |
| Editar | `bi-pencil-square` | Abre formulario de edición |
| Eliminar | `bi-trash` | Elimina el producto (requiere confirmación) |

### 2.4 Paginación

- **Registros por página**: 100
- **Navegación**: Primera, Anterior, Páginas, Siguiente, Última
- **Información**: "Mostrando X-Y de Z productos"

---

## 3. Sistema de Paquetes

### 3.1 Concepto

Un **paquete** es una presentación que agrupa múltiples unidades de un producto. Por ejemplo:
- Caja de 12 botellas de agua
- Blíster de 10 pastillas
- Pack de 6 unidades

### 3.2 Configuración de Paquete

**Códigos de Unidad para Paquetes:**
- `006` = Paquete
- `005` = Caja

**Campos Relevantes:**

```
CantidadPorPaquete = 10       (10 unidades por paquete)
CostoPaqueteGs = 100,000      (costo de comprar el paquete)
CostoUnitarioGs = 10,000      (calculado: 100,000 ÷ 10)
PrecioPaqueteGs = 150,000     (precio de venta del paquete)
PrecioUnitarioGs = 15,000     (precio por unidad suelta)
```

### 3.3 Cálculo de Costos y Precios

**Desde Paquete a Unitario:**
```csharp
CostoUnitarioGs = CostoPaqueteGs / CantidadPorPaquete
```

**Precio Paquete desde Factor:**
```csharp
PrecioPaqueteGs = CostoPaqueteGs * FactorPaquete
// Ejemplo: 100,000 * 1.5 = 150,000
```

**Precio Paquete desde Mark-up:**
```csharp
PrecioPaqueteGs = CostoPaqueteGs * (1 + MarkupPaquetePct / 100)
// Ejemplo: 100,000 * (1 + 50/100) = 150,000
```

### 3.4 Visualización en Listado

Cuando un producto es paquete (`UnidadMedidaCodigo == "006"`):
- Unidad: `006 (10 u/paq)`
- Costo: `10,000` / `Paq: 100,000`
- Precio: `15,000` / `Paq: 150,000`
- Stock: `500` / `50 paq. + 0 u.`

### 3.5 Reglas de Negocio

1. **CantidadPorPaquete** debe ser ≥ 1
2. El **stock siempre se maneja en unidades base**
3. Si `PermiteVentaPorUnidad = true`, el usuario puede elegir cómo cargar
4. El costo unitario se calcula automáticamente desde el costo del paquete

---

## 4. Control de Lotes y Vencimientos

### 4.1 Entidad: `ProductoLote`

**Tabla:** `ProductosLotes`

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `IdProductoLote` | int (PK) | Identificador único del lote |
| `IdProducto` | int (FK) | Producto al que pertenece |
| `IdDeposito` | int (FK) | Depósito donde está el lote |
| `NumeroLote` | string(50) | Código del lote (ej: L2026001) |
| `FechaVencimiento` | DateTime? | **Obligatoria** al crear/editar |
| `FechaFabricacion` | DateTime? | Opcional |
| `Stock` | decimal | Stock actual del lote |
| `StockInicial` | decimal | Stock al momento de creación |
| `CostoUnitario` | decimal? | Costo de adquisición |
| `Estado` | string(20) | Activo, Agotado, Vencido, Bloqueado |
| `EsLoteInicial` | bool | Si fue creado por migración |
| `Observacion` | string(500) | Notas adicionales |
| `IdCompra` | int? | Compra que originó el lote |

### 4.2 Propiedades Calculadas

```csharp
// ¿Está vencido?
EstaVencido => FechaVencimiento.HasValue && FechaVencimiento.Value.Date < DateTime.Today

// ¿Próximo a vencer? (30 días)
EstaProximoAVencer => FechaVencimiento.HasValue 
    && FechaVencimiento.Value.Date >= DateTime.Today 
    && FechaVencimiento.Value.Date <= DateTime.Today.AddDays(30)

// Días para vencimiento
DiasParaVencimiento => FechaVencimiento.HasValue 
    ? (FechaVencimiento.Value.Date - DateTime.Today).TotalDays 
    : null

// ¿Tiene stock disponible?
TieneStockDisponible => Stock > 0 && Estado == "Activo" && !EstaVencido
```

### 4.3 Activación del Control de Lotes

**En el formulario del producto:**
1. Marcar checkbox "Controla Lote"
2. Opcionalmente marcar "Controla Vencimiento"
3. Configurar "Días Alerta Vencimiento" (default: 30)
4. Guardar el producto

**Al guardar con stock existente:**
- El sistema crea automáticamente un lote "STOCK-INICIAL"
- Transfiere el stock actual al lote inicial
- Marca `LoteInicialCreado = true`

### 4.4 Modal de Administración de Lotes

**Acceso:** Botón "Ver Lotes" en formulario de edición del producto

**Funcionalidades:**
- **Lista de lotes existentes**: Muestra todos los lotes con estado visual
- **Formulario de nuevo lote**: Permite crear lotes manualmente
- **Edición de lotes**: Modificar número, fechas, estado, stock
- **Eliminación de lotes**: Solo si stock = 0 y tiene permiso DELETE

**Campos del Formulario:**
| Campo | Obligatorio | Descripción |
|-------|-------------|-------------|
| Número de Lote | ✅ | Identificador único del lote |
| Depósito | ✅ | Ubicación física |
| Fecha Vencimiento | ✅ | **Siempre obligatoria** |
| Fecha Fabricación | ❌ | Opcional |
| Stock | ❌ | Solo ajustes manuales |
| Costo Unitario | ❌ | Costo de adquisición |
| Estado | ✅ | Activo/Bloqueado/Agotado |
| Observaciones | ❌ | Notas adicionales |

### 4.5 Modal de Visualización de Lotes (Solo Lectura)

**Acceso:** Botón "Ver Lotes" (`bi-collection`) en el listado de productos

**Características:**
- Vista de solo lectura (sin edición)
- Tabla con: Lote, Depósito, Vencimiento, Stock, Estado
- Indicadores visuales de estado:
  - 🔴 Rojo: Vencido
  - 🟡 Amarillo: Próximo a vencer
  - 🟢 Verde: Activo
- Alertas resumidas:
  - Lotes sin fecha de vencimiento
  - Lotes vencidos con stock
  - Lotes próximos a vencer
- Botón "Editar Lotes": Abre formulario de edición del producto

### 4.6 Validación de Fecha de Vencimiento

**Regla:** La fecha de vencimiento es **OBLIGATORIA** al crear o editar lotes.

**Mensaje de Validación:**
```
⚠️ FECHA DE VENCIMIENTO OBLIGATORIA

Debe ingresar la fecha de vencimiento del lote para evitar 
inconvenientes en el control de stock y despacho de productos.
```

**Ubicaciones de Validación:**
1. `Productos.razor` → Método `GuardarLote()`
2. `Compras.razor` → Método `AgregarDetalleAsync()`
3. `Productos.razor` → Método `Guardar()` (valida lotes existentes)

### 4.7 Estados de Lote

| Estado | Color | Descripción |
|--------|-------|-------------|
| Activo | Verde | Disponible para venta |
| Bloqueado | Gris | No disponible (cuarentena, revisión) |
| Agotado | Gris | Stock = 0 |
| Vencido | Rojo | Fecha de vencimiento pasada |

### 4.8 Movimientos de Lote

**Tabla:** `MovimientosLotes`

Registra todas las operaciones sobre lotes:
- **Inicial**: Creación del lote
- **Venta**: Salida por venta
- **Compra**: Entrada por compra
- **Ajuste**: Ajuste manual
- **Transferencia**: Movimiento entre depósitos

---

## 5. Configuración de Precios

### 5.1 Precio Unitario

**Campos Principales:**
- `CostoUnitarioGs`: Costo de compra
- `PrecioUnitarioGs`: Precio de venta
- `FactorMultiplicador`: Factor costo → precio

**Cálculo:**
```csharp
PrecioUnitarioGs = CostoUnitarioGs * FactorMultiplicador
// Ejemplo: 10,000 * 1.30 = 13,000
```

### 5.2 Porcentaje de Utilidad

El sistema muestra automáticamente el % de utilidad:
```csharp
PorcentajeUtilidad = ((Precio - Costo) / Precio) * 100
// Ejemplo: ((13,000 - 10,000) / 13,000) * 100 = 23.08%
```

**Indicador Visual:**
- 🟢 Verde: Utilidad ≥ 25%
- 🔴 Rojo: Utilidad < 25%

### 5.3 Precio Ministerio (Farmacias)

**Activación:** Configuración del Sistema → Modo Farmacia

Cuando está activo:
- Se muestra columna "P.Min." en el listado
- Campo `PrecioMinisterio` en formulario
- Validación: `PrecioUnitarioGs ≤ PrecioMinisterio`
- Mensaje de error si el precio supera el máximo

---

## 6. Descuentos por Producto

### 6.1 Configuración General

| Campo | Descripción |
|-------|-------------|
| `PermiteDescuento` | Habilita/deshabilita descuentos |
| `PermiteVentaBajoCosto` | Permite vender a pérdida |

### 6.2 Descuento Específico

Si `UsaDescuentoEspecifico = true`:
- `DescuentoAutomaticoProducto`: % que se aplica automáticamente
- `DescuentoMaximoProducto`: % máximo permitido
- `MargenAdicionalCajeroProducto`: % adicional que el cajero puede agregar

### 6.3 Prioridad de Descuentos

1. Descuento específico del producto (si `UsaDescuentoEspecifico`)
2. Descuento por clasificación
3. Descuento por marca
4. Descuento general del sistema

---

## 7. Stock por Depósito

### 7.1 Tabla ProductosDepositos

Almacena el stock de cada producto por depósito:

| Campo | Descripción |
|-------|-------------|
| `IdProducto` | Producto |
| `IdDeposito` | Depósito |
| `Stock` | Cantidad actual |
| `StockMinimo` | Nivel de alerta |

### 7.2 Visualización en Listado

- **Sin filtro de depósito**: Muestra stock total + icono ℹ️ si hay múltiples depósitos
- **Con filtro**: Muestra solo stock del depósito seleccionado
- **Tooltip**: Desglose completo al pasar el mouse

### 7.3 Modal Stock por Depósito

**Acceso:** Botón `bi-box-seam` en acciones

Muestra tabla con:
- Nombre del depósito
- Stock en ese depósito

---

## 8. Productos Combo

### 8.1 Concepto

Un producto **combo** es una agrupación virtual de otros productos. Al vender un combo, el sistema descuenta el stock de sus componentes.

**Ejemplo:**
```
Combo Desayuno (precio: 50,000)
├── 1x Café (costo: 5,000)
├── 2x Medialunas (costo: 3,000 c/u)
└── 1x Jugo (costo: 8,000)
```

### 8.2 Configuración

- Marcar `EsCombo = true`
- Agregar componentes en la tabla `ProductoComponente`:
  - `IdProducto`: ID del combo
  - `IdComponente`: ID del producto componente
  - `Cantidad`: Unidades que se descuentan

### 8.3 Comportamiento en Ventas

Al vender un combo:
1. Se registra la línea del combo
2. Se descuenta stock de cada componente según su cantidad
3. El precio del combo puede ser diferente a la suma de componentes

---

## 9. Interfaz de Usuario

### 9.1 Formulario de Producto (Modal)

**Secciones del Formulario:**

#### Card: Información Básica
- Código (automático)
- Descripción *
- Código de Barras

#### Card: Clasificación y Unidades
- Tipo Ítem (Producto/Servicio)
- Marca (con botón +)
- Clasificación (con botón +)
- Depósito Predeterminado
- Unidad de Medida (77-Unidad / 006-Paquete)
- Tipo de IVA
- Moneda Precio
- Cantidad por Paquete (si es paquete)

#### Card: Precios y Costos
**Sección Paquete** (si aplica):
- Costo Paquete Gs
- Factor Paquete
- % Mark-up Paquete
- Precio Paquete Gs
- P.Min. Paquete (farmacia)

**Sección Unitario:**
- Costo Unit. Gs (readonly si es paquete)
- Factor
- % Mark-up
- Precio Unit. Gs
- P.Min. Unit. (farmacia)
- Precio USD (opcional)

#### Card: Inventario
- Stock
- Stock Mínimo

#### Card: Opciones
- Activo
- Permite Descuento
- Permite Venta Bajo Costo
- Permite Venta Decimal
- Controlado con Receta
- Es Combo

#### Card: Control de Lotes (lado derecho)
- Controla Lote (switch)
- Controla Vencimiento (switch)
- Días Alerta Vencimiento
- Permite Venta Vencido
- Botón "Ver Lotes" (si tiene lotes)

#### Card: Vencimiento Simple
(Solo si no controla lote pero sí vencimiento)
- Fecha de Vencimiento

#### Card: Imagen (lado derecho)
- Preview de imagen
- Botón "Seleccionar imagen"
- Botón "Quitar imagen"

### 9.2 Etiquetas de Código de Barras

**Modal de Etiqueta:**
- Formato: Code128 / EAN-13 / UPC-A
- Código (editable o auto-generado)
- Ancho de barra (1-6)
- Alto (40-200 px)
- Margen (0-30 px)
- Mostrar texto (checkbox)
- Mostrar nombre producto (checkbox)
- Preview del código de barras
- Botón Imprimir

---

## 10. Validaciones y Reglas de Negocio

### 10.1 Validaciones de Producto

| Regla | Mensaje |
|-------|---------|
| Descripción obligatoria | "El campo Descripción es obligatorio" |
| Código de barras formato | "Código de barras inválido. Use 8, 12, 13 o 14 dígitos" |
| Precio ≤ Precio Ministerio | "El precio supera el máximo permitido" |
| CantidadPorPaquete ≥ 1 | "La cantidad por paquete debe ser al menos 1" |

### 10.2 Validaciones de Lote

| Regla | Mensaje |
|-------|---------|
| Número de lote obligatorio | "El número de lote es obligatorio" |
| Depósito obligatorio | "Debe seleccionar un depósito" |
| **Fecha vencimiento obligatoria** | "La fecha de vencimiento es obligatoria" |
| Lote duplicado | "Ya existe un lote 'X' para este producto en ese depósito" |
| Eliminar con stock | "No se puede eliminar... primero debe realizar un Ajuste de Stock" |

### 10.3 Validaciones de Compras

Al agregar detalle de compra para producto que controla lote:
- **Fecha de vencimiento obligatoria** antes de agregar

### 10.4 Validaciones al Guardar Producto

Si el producto controla lote y tiene lotes sin fecha de vencimiento:
- Bloquea el guardado
- Muestra mensaje pidiendo completar fechas de vencimiento

---

## 11. Integración con Otros Módulos

### 11.1 Compras

- Al registrar compra de producto con control de lote:
  - Se crea automáticamente el lote
  - Se vincula con `IdCompra` e `IdCompraDetalle`
  - **Requiere fecha de vencimiento**

### 11.2 Ventas

- Al vender producto con control de lote:
  - Se aplica método **FEFO** (First Expired, First Out)
  - Se descuenta del lote más próximo a vencer
  - Bloquea venta de lotes vencidos (salvo `PermiteVentaVencido`)

### 11.3 Ajustes de Stock

- Permite ajustar stock por lote específico
- Registra movimientos de tipo "Ajuste"

### 11.4 Inventario/Conteo

- El conteo físico puede realizarse por lote
- Las diferencias generan ajustes automáticos

---

## 12. Preguntas Frecuentes

### ¿Cómo activo el control de lotes para un producto existente?

1. Ir a Productos → Buscar el producto → Editar
2. En la sección "Control de Lotes", marcar "Controla Lote"
3. Opcionalmente marcar "Controla Vencimiento"
4. Guardar
5. El sistema creará automáticamente un lote "STOCK-INICIAL" con el stock existente

### ¿Por qué no puedo guardar un lote sin fecha de vencimiento?

La fecha de vencimiento es **obligatoria** para todos los lotes. Esto garantiza:
- Control adecuado de productos perecederos
- Aplicación correcta del método FEFO
- Alertas oportunas de productos próximos a vencer

### ¿Cómo configuro un producto como paquete?

1. En el campo "Unidad de Medida", seleccionar "006 - Paquete"
2. Ingresar "Cantidad por Paquete" (ej: 10)
3. Ingresar "Costo Paquete Gs"
4. El sistema calcula automáticamente el costo unitario
5. Configurar precio de paquete y precio unitario

### ¿Puedo vender un producto vencido?

Solo si el producto tiene marcada la opción `PermiteVentaVencido = true`. 
Por defecto está desactivada por seguridad.

### ¿Qué significa el lote "STOCK-INICIAL"?

Es un lote creado automáticamente cuando se activa el control de lotes en un producto que ya tenía stock. Representa el stock existente antes de implementar la trazabilidad por lotes.

Se recomienda:
1. Asignarle fecha de vencimiento real
2. O dividirlo en lotes más específicos según el stock físico real

### ¿Cómo elimino un lote?

1. El lote debe tener stock = 0
2. Primero realizar un Ajuste de Stock (Inventario → Ajustes) para llevar el stock a 0
3. Luego en Productos → Editar → Ver Lotes → Eliminar
4. Requiere permiso DELETE en el módulo de productos

### ¿Cómo veo los lotes de un producto sin editarlo?

En el listado de productos, si el producto controla lotes, aparece el botón "Ver Lotes" (`bi-collection`). Este abre un modal de **solo lectura** que muestra todos los lotes con su estado.

---

## 📚 Referencias Técnicas

### Archivos Principales

| Archivo | Descripción |
|---------|-------------|
| `Pages/Productos.razor` | Página principal con listado, formulario y modales |
| `Models/Producto.cs` | Modelo de datos del producto |
| `Models/ProductoLote.cs` | Modelo de datos del lote |
| `Models/MovimientoLote.cs` | Registro de movimientos de lotes |
| `Models/ProductoDeposito.cs` | Stock por depósito |
| `Models/ProductoComponente.cs` | Componentes de combos |

### Servicios Relacionados

| Servicio | Función |
|----------|---------|
| `IInventarioService` | Operaciones de stock y movimientos |
| `PermisosService` | Verificación de permisos de usuario |

### Tablas de Base de Datos

- `Productos` - Catálogo de productos
- `ProductosLotes` - Lotes con vencimiento
- `ProductosDepositos` - Stock por ubicación
- `ProductosComponentes` - Componentes de combos
- `MovimientosLotes` - Historial de movimientos
- `Marcas` - Catálogo de marcas
- `Clasificaciones` - Categorías de productos
- `TiposIva` - Tipos de IVA
- `TiposItem` - Tipos de ítems

---

*Documento generado: 31 de enero de 2026*
*Versión del Sistema: SistemIA v2.x*
