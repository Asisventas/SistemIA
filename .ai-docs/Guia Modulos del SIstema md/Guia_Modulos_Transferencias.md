# Guía del Módulo de Transferencias entre Depósitos

## Descripción General

El módulo de **Transferencias entre Depósitos** permite mover productos de un depósito de origen a un depósito de destino dentro de la organización. Es fundamental para la gestión de inventario multi-depósito, reabastecimiento de puntos de venta, y redistribución de mercadería.

---

## Modelo de Datos

### Entidad Principal: `TransferenciaDeposito` (Cabecera)

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `IdTransferencia` | int (PK) | Identificador único de la transferencia |
| `IdDepositoOrigen` | int (FK, Required) | Depósito desde donde salen los productos |
| `IdDepositoDestino` | int (FK, Required) | Depósito donde ingresan los productos |
| `FechaTransferencia` | DateTime | Fecha y hora de la transferencia |
| `Comentario` | string? | Descripción o motivo de la transferencia |
| `UsuarioCreacion` | string? | Usuario que realizó la transferencia |

**Navegaciones:**
- `DepositoOrigen` → `Deposito`
- `DepositoDestino` → `Deposito`
- `Detalles` → Colección de `TransferenciaDepositoDetalle`

---

### Entidad de Detalle: `TransferenciaDepositoDetalle` (Líneas)

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `IdTransferenciaDetalle` | int (PK) | Identificador único del detalle |
| `IdTransferencia` | int (FK) | Referencia a la cabecera |
| `IdProducto` | int (FK) | Producto transferido |
| `Cantidad` | decimal(18,4) | Cantidad transferida |
| `CostoUnitario` | decimal(18,4)? | Costo unitario al momento de la transferencia |
| `IdProductoLoteOrigen` | int? (FK) | Lote de origen (si el producto controla lotes) |
| `IdProductoLoteDestino` | int? (FK) | Lote de destino (puede ser diferente) |
| `NumeroLote` | string? | Número del lote para referencia |
| `FechaVencimientoLote` | DateTime? | Fecha de vencimiento del lote |

**Navegaciones:**
- `Transferencia` → `TransferenciaDeposito` (cabecera)
- `Producto` → `Producto`
- `LoteOrigen` → `ProductoLote`
- `LoteDestino` → `ProductoLote`

---

## Estructura de Página

### Página Unificada: `/inventario/transferencias` (TransferenciasDeposito.razor)

**Ruta:** `/inventario/transferencias`  
**Permiso requerido:** VIEW sobre módulo `/inventario/transferencias`

La página combina listado de transferencias y formulario de creación en una vista única con alternancia.

---

## Vista de Listado (Explorador)

### Filtros Disponibles:

| Filtro | Tipo | Descripción |
|--------|------|-------------|
| ID | número | Buscar por número de transferencia |
| Fecha Desde | date | Inicio del rango de fechas |
| Fecha Hasta | date | Fin del rango de fechas |
| Depósito Origen | select | Filtrar por depósito de salida |
| Depósito Destino | select | Filtrar por depósito de entrada |

### Columnas de la Grilla:

| Columna | Descripción |
|---------|-------------|
| ID | Número de transferencia (formato #000001) |
| Fecha | Fecha y hora de la transferencia |
| Origen | Nombre del depósito de origen |
| Destino | Nombre del depósito de destino |
| Usuario | Usuario que realizó la transferencia |
| Comentario | Descripción de la transferencia |
| Acciones | Vista previa e imprimir |

### Características:
- Límite de 100 registros
- Ordenado por ID descendente (más reciente primero)
- Click en fila abre vista previa
- Botón "Nueva Transferencia" para crear

---

## Vista de Formulario (Nueva Transferencia)

### Configuración Principal:

| Campo | Tipo | Descripción |
|-------|------|-------------|
| Depósito Origen | select (requerido) | De dónde salen los productos |
| Depósito Destino | select (requerido) | A dónde van los productos |
| Comentario | text | Descripción opcional (máx. 500 caracteres) |

**Nota:** El selector de destino excluye automáticamente el depósito origen seleccionado.

### Búsqueda de Productos:

1. **Autocompletado:**
   - Busca por nombre o código
   - Muestra stock disponible en depósito origen
   - Máximo 15 sugerencias

2. **Información mostrada:**
   - Nombre del producto
   - Código interno
   - Stock disponible (badge)

### Soporte de Lotes:

Si el producto controla lotes:
- Aparece selector de lote obligatorio
- Lista lotes con stock > 0 en depósito origen
- Muestra: Número de lote, fecha de vencimiento, stock
- El stock disponible se actualiza según el lote seleccionado

### Soporte de Paquetes:

Si el producto se vende por paquete (unidad 005 o 006):
- Selector de modo: "Paquete" o "Unidad"
- Ingreso de cantidad en el modo seleccionado
- Conversión automática a unidades
- Muestra equivalencia (ej: "= 24 und.")

### Grilla de Líneas:

| Columna | Descripción |
|---------|-------------|
| # | Número de línea |
| Código | Código interno del producto |
| Producto | Descripción + info de paquete |
| Lote | Número de lote y vencimiento (si aplica) |
| Paq. | Cantidad en paquetes (si aplica) |
| Stock origen | Stock antes de transferir |
| Cantidad | Campo editable para ajustar |
| Costo unit. | Costo unitario del producto |
| Costo total | Cantidad × Costo unitario |
| Acciones | Botón eliminar línea |

### Totales:
- Suma del costo total de todas las líneas

### Acciones Post-Guardado:
- Botón "Imprimir" para generar comprobante
- Botón "Nueva transferencia" para crear otra
- Botón "Ver listado" para volver al explorador

---

## Modal de Vista Previa

### Características:
- Tamaño: 95% del viewport
- Fondo tipo papel (simula impresión)
- Incluye todos los datos de la transferencia
- Botones: Cerrar, Imprimir

### Contenido:
- Cabecera con logo, empresa, RUC
- Título "Transferencia entre Depósitos" con número
- Metadatos: Depósito origen y destino con sucursal
- Comentario (si existe)
- Tabla de productos con totales
- Pie de comprobante

---

## Flujo del Proceso

### Creación de Transferencia:

```
1. Seleccionar Depósito Origen
    ↓
2. Cargar stocks del depósito origen
    ↓
3. Seleccionar Depósito Destino (diferente al origen)
    ↓
4. Buscar y agregar productos:
    │
    ├─ Seleccionar producto
    ├─ Si controla lotes → seleccionar lote con stock
    ├─ Ingresar cantidad (por paquete o unidad)
    └─ Agregar a la grilla
    ↓
5. Revisar y ajustar cantidades
    ↓
6. Agregar comentario (opcional)
    ↓
7. Guardar:
    │
    ├─ Validar datos
    ├─ Crear cabecera TransferenciaDeposito
    ├─ Por cada línea:
    │     │
    │     ├─ Crear TransferenciaDepositoDetalle
    │     ├─ Decrementar stock en depósito origen
    │     │     └─ Si tiene lote → decrementar lote origen
    │     └─ Incrementar stock en depósito destino
    │           └─ Si tiene lote → crear/actualizar lote destino
    └─ Commit transacción
    ↓
8. Mostrar opciones post-guardado
```

---

## Cálculos Importantes

### Costo Total por Línea:
```
CostoTotal = Cantidad × CostoUnitario
```

### Total de la Transferencia:
```
Total = Σ(CostoTotal de cada línea)
```

### Conversión de Paquetes a Unidades:
```
CantidadUnidades = CantidadPaquetes × CantidadPorPaquete
```

---

## Integración con Otros Módulos

### Depósitos
- Lista depósitos activos con sucursal asociada
- Muestra nombre y sucursal en selectores
- Filtra destino para excluir origen seleccionado

### Productos
- Obtiene productos activos
- Lee configuración de paquetes
- Detecta si controla lotes
- Obtiene costo unitario

### Inventario (ProductosDepositos)
- Lee stock actual del depósito origen
- Actualiza stock en origen (decremento)
- Actualiza stock en destino (incremento)

### Lotes (ProductoLote)
- Lista lotes con stock en depósito origen
- Transfiere información de lote al destino
- Mantiene trazabilidad de origen a destino

### Sucursales
- Muestra sucursal asociada a cada depósito
- Incluye información en comprobante impreso

---

## Impresión de Comprobante

### Formato del Comprobante (A4):

**Cabecera:**
- Logo de la empresa
- Nombre de empresa, RUC
- Nombre de sucursal y dirección
- Título "Transferencia entre Depósitos"
- Número de transferencia (formato 000001)
- Fecha y hora
- Usuario

**Metadatos:**
- Depósito Origen (con sucursal)
- Depósito Destino (con sucursal)
- Comentario (si existe)

**Detalle (tabla):**
| # | Código | Descripción | Paq. | Cantidad | Costo unit. | Costo total |

**Totales:**
- Total general del costo

**Pie:**
- Leyenda "** Comprobante de Transferencia **"

---

## Validaciones

### Al Agregar Línea:
- Debe seleccionar depósito origen
- Debe seleccionar producto
- Cantidad debe ser mayor a 0
- Si producto controla lotes, debe seleccionar lote
- Cantidad no puede exceder stock disponible

### Al Guardar:
- Depósito origen seleccionado
- Depósito destino seleccionado (diferente al origen)
- Al menos una línea de productos
- Usuario identificado

### Durante Edición de Cantidad:
- No permite cantidades negativas
- Valida contra stock disponible
- Respeta decimales del producto

---

## Consideraciones Técnicas

### Transaccionalidad:
- La transferencia se procesa como transacción atómica
- Si falla el incremento en destino, se revierte el decremento en origen
- Garantiza consistencia del inventario

### Manejo de Lotes:
- El lote de origen y destino pueden ser diferentes
- Se copia información del lote (número, vencimiento) al detalle
- El destino puede crear nuevo lote o usar existente

### Rendimiento:
- Carga de stocks bajo demanda al seleccionar depósito
- Autocompletado limitado a 50 productos internos
- Listado limitado a 100 transferencias

### Auditoría:
- Registra usuario que realizó la transferencia
- Fecha y hora exactas
- Comentario para documentación

---

## Casos de Uso Comunes

### 1. Reabastecimiento de Punto de Venta
Mover productos del depósito central a sucursal:
- Origen: Depósito Principal
- Destino: Depósito de la sucursal
- Comentario: "Reabastecimiento semanal"

### 2. Devolución al Almacén
Productos que regresan de un punto de venta:
- Origen: Depósito de sucursal
- Destino: Depósito Principal
- Comentario: "Devolución por exceso de stock"

### 3. Redistribución entre Sucursales
Equilibrar stock entre puntos de venta:
- Origen: Sucursal A (exceso)
- Destino: Sucursal B (faltante)
- Comentario: "Redistribución por demanda"

### 4. Transferencia de Lotes
Mover lotes específicos (ej: por vencimiento próximo):
- Seleccionar lotes con fecha cercana
- Transferir a depósito con mayor rotación
- Documentar motivo

### 5. Centralización de Stock
Consolidar inventario disperso:
- Múltiples transferencias desde sucursales
- Destino: Depósito central
- Para inventario físico centralizado

---

## Estados del Flujo

A diferencia de otros módulos (ventas, compras), las transferencias:
- **No tienen estado**: Se ejecutan inmediatamente al guardar
- **Son irreversibles**: Una vez guardada, afecta inventario
- **Para revertir**: Se crea una nueva transferencia inversa

---

## Relación con el Diagrama de Base de Datos

```
TransferenciaDeposito (Cabecera)
    │
    ├── IdDepositoOrigen → Deposito → Sucursal
    ├── IdDepositoDestino → Deposito → Sucursal
    │
    └── Detalles → TransferenciaDepositoDetalle[]
                        │
                        ├── IdProducto → Producto
                        ├── IdProductoLoteOrigen → ProductoLote
                        └── IdProductoLoteDestino → ProductoLote
```

---

## Diferencias con Ajuste de Stock

| Aspecto | Transferencia | Ajuste de Stock |
|---------|---------------|-----------------|
| Propósito | Mover entre depósitos | Corregir diferencias |
| Depósitos | Siempre 2 (origen y destino) | Uno por línea |
| Stock | Se mantiene total de la empresa | Puede aumentar o disminuir |
| Costo | Usado para valorización | Usado para valorizar diferencia |
| Trazabilidad | Origen → Destino | Antes → Después |
| Lotes | Puede cambiar de depósito | Solo ajusta cantidad |

---

## Flujo de Inventario

### En Transferencia:
```
Depósito Origen:  Stock - Cantidad = Nuevo Stock
Depósito Destino: Stock + Cantidad = Nuevo Stock
Total Empresa:    Sin cambio (solo redistribución)
```

### Con Lotes:
```
Lote Origen:   Stock - Cantidad = Nuevo Stock (puede quedar en 0)
Lote Destino:  Stock + Cantidad = Nuevo Stock (puede crear nuevo lote)
```

---

## Permisos Sugeridos

| Permiso | Descripción |
|---------|-------------|
| VIEW | Ver listado y detalles de transferencias |
| CREATE | Crear nuevas transferencias |
| DELETE | Eliminar transferencias (si se implementa) |

**Nota:** Las transferencias normalmente no se editan ni eliminan una vez guardadas. Para correcciones, se crea una transferencia inversa.
