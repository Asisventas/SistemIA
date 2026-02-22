# Guía del Módulo de Ajustes de Stock

## Descripción General

El módulo de **Ajustes de Stock** permite realizar correcciones del inventario físico comparando el stock real (contado físicamente) contra el stock registrado en el sistema. Es fundamental para mantener la integridad del inventario y detectar diferencias por mermas, robos, deterioros o errores de registro.

---

## Modelo de Datos

### Entidad Principal: `AjusteStock` (Cabecera)

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `IdAjusteStock` | int (PK) | Identificador único del ajuste |
| `IdSucursal` | int | Sucursal donde se realiza el ajuste |
| `IdCaja` | int? | Caja asociada (opcional, para contexto de turno) |
| `Turno` | int? | Turno activo al momento del ajuste |
| `FechaAjuste` | DateTime | Fecha y hora del ajuste |
| `Usuario` | string(50) | Usuario que realizó el ajuste |
| `Comentario` | string(280) | Descripción o motivo del ajuste |
| `TotalMonto` | decimal(18,0) | Valor monetario total del ajuste |
| `UsuarioCreacion` | string | Usuario que creó el registro |
| `FechaCreacion` | DateTime | Fecha de creación |
| `UsuarioModificacion` | string? | Último usuario que modificó |
| `FechaModificacion` | DateTime? | Fecha de última modificación |

**Navegación:**
- `Detalles` → Colección de `AjusteStockDetalle`

---

### Entidad de Detalle: `AjusteStockDetalle` (Líneas)

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `IdAjusteStockDetalle` | int (PK) | Identificador único del detalle |
| `IdAjusteStock` | int (FK) | Referencia a la cabecera |
| `IdProducto` | int (FK) | Producto ajustado |
| `IdDeposito` | int (FK) | Depósito donde se ajusta (puede variar por línea) |
| `IdProductoLote` | int? (FK) | Lote específico si el producto controla lotes |
| `StockAjuste` | decimal(18,4) | Stock físico contado (cantidad real) |
| `StockSistema` | decimal(18,4) | Stock que tenía el sistema al momento del ajuste |
| `Diferencia` | decimal(18,4) | Calculado: StockAjuste - StockSistema |
| `Monto` | decimal(18,0) | Valor monetario: \|Diferencia\| × Costo |
| `FechaAjuste` | DateTime | Fecha del ajuste (heredada de cabecera) |
| `IdSucursal` | int | Sucursal (heredada) |
| `IdCaja` | int? | Caja (heredada) |
| `Turno` | int? | Turno (heredado) |
| `Usuario` | string | Usuario (heredado) |
| `UsuarioCreacion` | string | Usuario que creó |
| `FechaCreacion` | DateTime | Fecha de creación |

**Propiedad Calculada (NotMapped):**
- `PermiteDecimal` → Indica si el producto permite cantidades decimales

**Navegaciones:**
- `Ajuste` → `AjusteStock` (cabecera)
- `Producto` → `Producto`
- `Deposito` → `Deposito`
- `ProductoLote` → `ProductoLote` (si aplica)

---

## Estructura de Páginas

### 1. Página Principal: `/inventario/ajustes` (AjustesStock.razor)

**Ruta:** `/inventario/ajustes`  
**Permiso requerido:** VIEW sobre módulo `/inventario/ajustes`

#### Características Principales:

1. **Contexto de Caja y Turno**
   - Selector de caja activa
   - Muestra fecha y turno actual
   - El ajuste queda asociado al contexto de caja/turno

2. **Selección de Depósito**
   - Dropdown con depósitos activos
   - Muestra sucursal asociada
   - El depósito puede ser diferente para cada línea

3. **Búsqueda de Productos**
   - Autocompletado por nombre o código
   - Indica si el producto controla lotes (badge "Lotes")
   - Integración con escáner de código de barras

4. **Soporte de Lotes**
   - Si el producto controla lotes, aparece selector de lote
   - Muestra stock disponible por lote
   - Incluye fecha de vencimiento

5. **Soporte de Paquetes**
   - Selector de modo: "Unidad" o "Paquete"
   - Conversión automática entre unidades y paquetes
   - Muestra equivalencia (ej: "1 paquete = 12 unidades")

6. **Grilla de Líneas**
   
   | Columna | Descripción |
   |---------|-------------|
   | # | Número de línea |
   | Producto | Descripción y código interno |
   | Lote | Número de lote y vencimiento (si aplica) |
   | Paq. | Cantidad en paquetes (si aplica) |
   | Depósito | Depósito de la línea |
   | Stock sistema | Cantidad que tenía el sistema |
   | Stock ajuste | Campo editable para ingresar conteo físico |
   | Diferencia | Calculado automáticamente (rojo si negativo) |
   | Costo | Costo unitario del producto |
   | Precio venta | Precio de venta referencial |
   | Monto | Valor monetario del ajuste |
   | Acciones | Botón eliminar línea |

7. **Totales**
   - Suma del monto total de todas las líneas

8. **Acciones Post-Guardado**
   - Botón "Imprimir" para generar comprobante
   - Botón "Nuevo ajuste" para iniciar otro

#### Escáner de Código de Barras:
- Modal integrado para escanear productos
- Selector de cámara si hay múltiples
- Muestra último código escaneado
- Agrega producto automáticamente al escanear

---

### 2. Explorador: `/inventario/ajustes/explorar` (AjustesExplorar.razor)

**Ruta:** `/inventario/ajustes/explorar`  
**Permiso requerido:** VIEW sobre módulo `/inventario/ajustes/explorar`

#### Filtros Disponibles:

| Filtro | Tipo | Descripción |
|--------|------|-------------|
| Desde | date | Fecha inicial del rango |
| Hasta | date | Fecha final del rango |
| Usuario | text | Filtrar por usuario que realizó el ajuste |

#### Columnas de la Grilla:

| Columna | Descripción |
|---------|-------------|
| # | ID del ajuste |
| Fecha | Fecha y hora del ajuste |
| Sucursal | ID de la sucursal |
| Usuario | Usuario que realizó |
| Monto total | Valor monetario total |
| Acciones | Botón imprimir |

#### Características:
- Carga los últimos 7 días por defecto
- Límite de 200 registros
- Ordenado por fecha descendente
- Impresión de comprobante individual

---

## Servicio de Negocio: `AjusteStockService`

### Interfaz: `IAjusteStockService`

```csharp
Task<int> CrearAjusteAsync(
    int idSucursal, 
    int? idCaja, 
    int? turno, 
    string usuario, 
    string? comentario,
    IEnumerable<LineaAjusteInput> lineas, 
    DateTime? fechaAjuste = null
);
```

### Input de Línea: `LineaAjusteInput`

```csharp
public record LineaAjusteInput(
    int IdProducto, 
    int IdDeposito, 
    decimal StockSistema, 
    decimal StockAjuste, 
    decimal PrecioCostoGs, 
    int? IdLote = null
);
```

### Flujo del Proceso:

```
1. Iniciar transacción
    ↓
2. Normalizar datos (usuario, comentario, fecha)
    ↓
3. Crear cabecera AjusteStock
    ↓
4. Por cada línea:
    │
    ├─ Calcular diferencia = StockAjuste - StockSistema
    ├─ Calcular monto = |diferencia| × costo
    ├─ Crear AjusteStockDetalle
    │
    └─ Si diferencia ≠ 0:
         │
         ├─ Si diferencia > 0 → Entrada de inventario
         └─ Si diferencia < 0 → Salida de inventario
              │
              └─ Llamar _inventario.AjustarStockAsync()
    ↓
5. Guardar cambios y commit
    ↓
6. Retornar IdAjusteStock
```

### Interacción con Inventario:

El servicio utiliza `IInventarioService.AjustarStockAsync()` para:
- **Diferencia positiva (entrada):** Incrementar stock en ProductosDepositos
- **Diferencia negativa (salida):** Decrementar stock en ProductosDepositos
- **Con lotes:** Afectar el stock del lote específico en ProductoLote

---

## Cálculos Importantes

### Diferencia:
```
Diferencia = StockAjuste - StockSistema
```

- **Diferencia > 0:** Sobrante (se encontró más de lo esperado)
- **Diferencia < 0:** Faltante (se encontró menos de lo esperado)
- **Diferencia = 0:** Sin cambios (stock coincide)

### Monto del Ajuste:
```
Monto = |Diferencia| × PrecioCosto
```

El monto siempre es positivo y representa el valor monetario del ajuste.

### Total del Ajuste:
```
TotalMonto = Σ(Monto de cada línea)
```

---

## Integración con Otros Módulos

### Productos
- Obtiene stock actual del sistema
- Usa costo para valorizar ajustes
- Respeta configuración de decimales
- Detecta si controla lotes

### Depósitos
- Lista depósitos activos
- Permite ajustar diferentes depósitos en un mismo documento
- Muestra sucursal del depósito

### Lotes (ProductoLote)
- Si el producto controla lotes, el ajuste es por lote
- Muestra stock disponible por lote
- Incluye fecha de vencimiento para referencia
- El ajuste afecta el stock del lote específico

### Inventario (ProductosDepositos)
- Actualiza el stock real del producto
- Registra movimiento de inventario
- Mantiene trazabilidad del ajuste

### Caja/Turno
- Asocia el ajuste al contexto operativo
- Permite filtrar ajustes por turno/caja

---

## Impresión de Comprobante

### Formato del Comprobante (A4):

**Cabecera:**
- Logo de la empresa
- Nombre de empresa, RUC, sucursal
- Título "Ajuste de Stock"
- Número de ajuste
- Fecha y hora
- Usuario

**Detalle (tabla):**
| Código | Descripción | Paq. | Depósito | Stock sistema | Stock ajuste | Diferencia | Precio | Monto |

**Pie:**
- Total del ajuste
- Leyenda "** Ajuste generado por el sistema **"

---

## Flujo de Trabajo Típico

### Escenario: Inventario Físico

1. **Preparación:**
   - Seleccionar depósito a inventariar
   - Tener lista de productos a contar

2. **Registro:**
   - Buscar producto por nombre o escanear código
   - El sistema muestra stock actual
   - Ingresar cantidad contada (stock ajuste)
   - Agregar línea

3. **Revisión:**
   - Verificar diferencias calculadas
   - Las diferencias negativas se muestran en rojo
   - Agregar comentario explicativo

4. **Guardado:**
   - El sistema ajusta el inventario automáticamente
   - Genera número de ajuste
   - Permite imprimir comprobante

---

## Validaciones

### Al Agregar Línea:
- Debe seleccionar depósito
- Debe seleccionar producto
- Si producto controla lotes, debe seleccionar lote

### Al Guardar:
- Debe tener al menos una línea
- El comentario no debe exceder 280 caracteres
- El usuario debe estar identificado

### Cantidad (Stock Ajuste):
- Mínimo 0 (no se permite negativo)
- Respeta configuración de decimales del producto
- Si es paquete, convierte a unidades antes de guardar

---

## Consideraciones Técnicas

### Transaccionalidad:
- Todo el ajuste se procesa en una transacción
- Si falla alguna línea, se revierte todo
- Los movimientos de inventario son atómicos

### Auditoría:
- Registra usuario y fecha de creación
- El ajuste queda asociado a caja y turno
- Los movimientos de inventario referencian el ajuste

### Rendimiento:
- Carga de productos con autocompletado (límite 12 sugerencias)
- Stocks se cargan bajo demanda
- Explorador limitado a 200 registros

---

## Casos de Uso Comunes

### 1. Inventario Parcial
Ajustar solo ciertos productos de un depósito:
- Seleccionar depósito
- Agregar solo productos inventariados
- El resto mantiene su stock

### 2. Corrección de Error
Corregir un registro incorrecto:
- Buscar producto afectado
- Ingresar cantidad correcta
- Documentar en comentario

### 3. Registro de Merma
Documentar productos deteriorados:
- Ingresar stock ajuste menor al sistema
- Detallar en comentario el motivo
- El sistema registra la salida

### 4. Ingreso por Devolución Interna
Productos que regresan al depósito:
- Ingresar stock ajuste mayor al sistema
- El sistema registra la entrada

---

## Relación con el Diagrama de Base de Datos

```
AjusteStock (Cabecera)
    │
    ├── IdSucursal → Sucursal
    ├── IdCaja → Caja
    │
    └── Detalles → AjusteStockDetalle[]
                        │
                        ├── IdProducto → Producto
                        ├── IdDeposito → Deposito
                        └── IdProductoLote → ProductoLote
```
