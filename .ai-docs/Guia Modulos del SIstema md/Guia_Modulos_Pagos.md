# Guía del Módulo de Pagos a Proveedores (Cuentas por Pagar)

## Descripción General

El módulo de **Pagos a Proveedores** gestiona las cuentas por pagar originadas por compras a crédito. Permite registrar pagos parciales o totales sobre cuotas, con soporte para múltiples medios de pago y monedas.

---

## Modelo de Datos

### Entidad Principal: `CuentaPorPagar` (Deuda a Proveedor)

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `IdCuentaPorPagar` | int (PK) | Identificador único de la deuda |
| `IdCompra` | int (FK) | Compra que originó la deuda |
| `IdProveedor` | int (FK) | Proveedor acreedor |
| `IdSucursal` | int (FK) | Sucursal donde se originó |
| `IdMoneda` | int? | Moneda de la deuda |
| `MontoTotal` | decimal(18,4) | Monto total de la deuda |
| `SaldoPendiente` | decimal(18,4) | Saldo por pagar |
| `FechaCredito` | DateTime | Fecha de otorgamiento del crédito |
| `FechaVencimiento` | DateTime? | Fecha de vencimiento general |
| `Estado` | string(20) | Abierta, Pagada, Anulada |
| `NumeroCuotas` | int | Cantidad de cuotas (default: 1) |
| `PlazoDias` | int | Plazo en días (default: 0) |
| `Observaciones` | string(280)? | Notas adicionales |
| `IdUsuarioAutorizo` | int? | Usuario que autorizó el crédito |

**Navegaciones:**
- `Compra` → Compra origen
- `Proveedor` → Proveedor acreedor
- `Moneda` → Moneda de la deuda
- `UsuarioAutorizo` → Usuario autorizante
- `Cuotas` → Colección de `CuentaPorPagarCuota`

---

### Entidad: `CuentaPorPagarCuota` (Cuotas de la Deuda)

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `IdCuota` | int (PK) | Identificador único de la cuota |
| `IdCuentaPorPagar` | int (FK, Required) | Deuda a la que pertenece |
| `NumeroCuota` | int | Número de cuota (1, 2, 3...) |
| `MontoCuota` | decimal(18,4) | Monto original de la cuota |
| `SaldoCuota` | decimal(18,4) | Saldo pendiente de la cuota |
| `FechaVencimiento` | DateTime | Fecha de vencimiento |
| `FechaPago` | DateTime? | Fecha en que se pagó completamente |
| `Estado` | string(20) | Pendiente, Pagada, Anulada |
| `Observaciones` | string(280)? | Notas |

**Navegaciones:**
- `CuentaPorPagar` → Deuda padre
- `PagoDetalles` → Colección de `PagoProveedorDetalle`

---

### Entidad: `PagoProveedor` (Registro de Pago)

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `IdPagoProveedor` | int (PK) | Identificador único del pago |
| `IdCompra` | int (FK) | Compra que se está pagando |
| `IdProveedor` | int (FK) | Proveedor que recibe el pago |
| `IdSucursal` | int (FK) | Sucursal donde se realizó |
| `IdMoneda` | int? | Moneda del pago |
| `IdCaja` | int? | Caja donde se realizó |
| `IdUsuario` | int? | Usuario que registró |
| `Turno` | int? | Turno de caja |
| `FechaPago` | DateTime | Fecha y hora del pago |
| `MontoTotal` | decimal(18,4) | Monto total pagado |
| `CambioDelDia` | decimal(18,4)? | Tipo de cambio si hay conversión |
| `Estado` | string(20) | Pagado, Pendiente, Anulado |
| `NumeroRecibo` | string(30)? | Número de comprobante |
| `Observaciones` | string(280)? | Notas del pago |

**Navegaciones:**
- `Compra` → Compra
- `Proveedor` → Proveedor
- `Moneda` → Moneda
- `Caja` → Caja
- `Usuario` → Usuario
- `Detalles` → Colección de `PagoProveedorDetalle`

---

### Entidad: `PagoProveedorDetalle` (Medios de Pago)

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `IdPagoProveedorDetalle` | int (PK) | Identificador único |
| `IdPagoProveedor` | int (FK, Required) | Pago al que pertenece |
| `IdCuota` | int? (FK) | Cuota específica (para pagos parciales) |
| `MedioPago` | string(20, Required) | EFECTIVO, CHEQUE, TRANSFERENCIA, TARJETA |
| `Monto` | decimal(18,4) | Monto de este medio de pago |
| `CambioDelDia` | decimal(18,4)? | Tipo de cambio específico |
| `BancoCheque` | string(100)? | Banco del cheque |
| `NumeroCheque` | string(30)? | Número del cheque |
| `FechaCobroCheque` | DateTime? | Fecha de cobro del cheque |
| `BancoTransferencia` | string(100)? | Banco de la transferencia |
| `NumeroTransferencia` | string(100)? | Referencia de transferencia |
| `BancoTarjeta` | string(100)? | Banco/Emisor de tarjeta |
| `MarcaTarjeta` | string(50)? | Marca (Visa, Mastercard, etc.) |
| `Ultimos4Tarjeta` | string(4)? | Últimos 4 dígitos |
| `NumeroAutorizacion` | string(50)? | Código de autorización |
| `Observaciones` | string(280)? | Notas |

---

## Estructura de Páginas

### 1. Página Principal: `/pagos-proveedores` (PagosProveedores.razor)

**Ruta:** `/pagos-proveedores`  
**Permiso:** VIEW sobre `/pagos`

**Características:**
- Vista de cuentas por pagar pendientes
- Filtros por proveedor y estado
- Expansión de cuotas por cuenta
- Modal de registro de pago
- Soporte multi-moneda con conversión

---

### 2. Listado Histórico: `/pagos-proveedores/listado` (PagosProveedoresListado.razor)

**Ruta:** `/pagos-proveedores/listado`  
**Permiso:** VIEW sobre `/proveedores/pagos`

**Características:**
- Historial de todos los pagos realizados
- Filtros: Fecha, Caja, Turno, Proveedor
- Resumen por medio de pago
- Exportación a Excel
- Impresión de listado

---

### 3. Historial por Compra: `/pagos-proveedores/historial/{idCompra}` (PagosProveedoresHistorial.razor)

**Ruta:** `/pagos-proveedores/historial/{idCompra}`  
**Permiso:** VIEW sobre `/proveedores/pagos`

**Características:**
- Ver todos los pagos de una compra específica
- Detalle de medios de pago utilizados
- Saldo actualizado de la deuda

---

## Flujo del Proceso de Pago

### Origen de la Deuda (en Compras):

```
1. Compra a crédito confirmada
    ↓
2. Sistema crea CuentaPorPagar:
    - MontoTotal = Total de la compra
    - SaldoPendiente = Total (sin pagos)
    - NumeroCuotas según condición de pago
    ↓
3. Sistema crea CuentaPorPagarCuota (una por cuota):
    - MontoCuota = MontoTotal / NumeroCuotas
    - SaldoCuota = MontoCuota
    - FechaVencimiento calculada según plazo
    ↓
4. Cuenta aparece en lista de pendientes
```

### Registro de Pago:

```
1. Seleccionar cuenta por pagar
    ↓
2. Sistema muestra:
    - Saldo pendiente
    - Detalle de cuotas
    - Moneda de la deuda
    ↓
3. Configurar pago:
    - Seleccionar caja y turno
    - Fecha del pago
    ↓
4. Agregar medios de pago:
    │
    ├─ EFECTIVO: Solo monto
    ├─ CHEQUE: Número, banco, fecha cobro
    ├─ TRANSFERENCIA: Número transacción, banco
    └─ TARJETA: Banco, marca, últimos 4, autorización
    ↓
5. Si moneda diferente:
    - Cargar tipo de cambio del día
    - Convertir montos automáticamente
    ↓
6. Confirmar pago:
    │
    ├─ Crear PagoProveedor
    ├─ Crear PagoProveedorDetalle por cada medio
    ├─ Actualizar SaldoPendiente de CuentaPorPagar
    ├─ Actualizar SaldoCuota de cuotas afectadas
    ├─ Si SaldoPendiente = 0 → Estado = Pagada
    └─ Generar número de recibo
    ↓
7. Mostrar opciones:
    - Imprimir recibo
    - Nuevo pago
```

---

## Modal de Pago

### Información Mostrada:

| Campo | Descripción |
|-------|-------------|
| Proveedor | Nombre del proveedor acreedor |
| Saldo Pendiente | Monto que falta pagar |
| Saldo después del pago | Cálculo en tiempo real |
| Tipo de Cambio | Si hay monedas diferentes |

### Configuración de Caja:

| Campo | Descripción |
|-------|-------------|
| Fecha del Pago | Editable (default: fecha de caja) |
| Caja | Selector de cajas disponibles |
| Turno | Selector de turno |

**Nota:** Si se selecciona "Sin caja", el pago no afecta el resumen de caja.

### Medios de Pago Soportados:

| Medio | Campos Adicionales |
|-------|-------------------|
| EFECTIVO | Solo monto |
| CHEQUE | Número de cheque, Banco, Fecha cobro |
| TRANSFERENCIA | Número de transacción, Banco |
| TARJETA | Banco, Marca, Últimos 4 dígitos |

### Multi-Moneda:

- Cada medio de pago puede tener su propia moneda
- El sistema convierte al tipo de cambio del día
- Muestra detalle de conversión en resumen

---

## Diferencias con Módulo de Cobros

| Aspecto | Cobros (A Clientes) | Pagos (A Proveedores) |
|---------|---------------------|----------------------|
| Origen | Ventas a crédito | Compras a crédito |
| Contraparte | Cliente (nos debe) | Proveedor (le debemos) |
| Flujo dinero | Entrada | Salida |
| Color tema | Warning (amarillo) | Danger (rojo) |
| Estados | CONFIRMADO/ANULADO | Pagado/Anulado |
| QR | Soportado | No aplica |

---

## Estados del Sistema

### Estados de CuentaPorPagar:

| Estado | Descripción | Color |
|--------|-------------|-------|
| Abierta | Tiene saldo por pagar | Amarillo |
| VENCIDO | Fecha vencimiento pasada | Rojo |
| Pagada | SaldoPendiente = 0 | Verde |
| Anulada | Cancelada manualmente | Gris |

### Estados de PagoProveedor:

| Estado | Descripción |
|--------|-------------|
| Pagado | Pago válido y activo |
| Pendiente | Pago en proceso |
| Anulado | Pago revertido |

### Estados de Cuota:

| Estado | Descripción |
|--------|-------------|
| Pendiente | Tiene saldo por pagar |
| Pagada | SaldoCuota = 0 |
| Anulada | Cuota cancelada |

---

## Integración con Otros Módulos

### Compras
- Las compras a crédito generan CuentaPorPagar automáticamente
- Referencia a IdCompra para trazabilidad
- También soporta compras sin cuenta formal (legacy)

### Caja
- Los pagos afectan el resumen de caja del turno
- Selector de caja y turno en el pago
- Pagos sin caja no afectan resumen

### Monedas y Tipos de Cambio
- Soporte para deudas en moneda extranjera
- Conversión automática en pagos
- Tipo de cambio del día desde BD

### Proveedores
- Vínculo directo con el proveedor acreedor
- Filtros por proveedor en todos los listados

### Sucursales
- Filtrado por sucursal del usuario
- Trazabilidad de origen de la deuda

---

## Informes Disponibles

### 1. Listado de Pagos (PagosProveedoresListado)

**Filtros:**
- Rango de fechas
- Caja específica
- Turno
- Proveedor

**Resumen:**
- Total pagado (₲)
- Total en efectivo
- Total en transferencias
- Total otros medios

### 2. Historial por Compra (PagosProveedoresHistorial)

**Información:**
- Todos los pagos de una compra
- Medios utilizados por pago
- Saldo actual de la deuda

---

## Cálculos Importantes

### Saldo Restante:
```
SaldoRestante = SaldoPendiente - TotalPago
```

### Total Pago (moneda única):
```
TotalPago = Σ(Monto de cada detalle de pago)
```

### Total Pago con Conversión:
```
TotalConvertido = Σ(Monto × TipoCambio para cada detalle en moneda diferente)
```

### Distribución a Cuotas:
Los pagos se aplican automáticamente a las cuotas pendientes en orden de vencimiento.

---

## Validaciones

### Al Registrar Pago:
- Caja seleccionada (o explícitamente sin caja)
- Al menos un medio de pago con monto > 0
- Total del pago no puede exceder saldo pendiente (con confirmación)
- Datos completos según medio de pago seleccionado

### Tipo de Cambio:
- Obligatorio cuando hay monedas diferentes
- Se carga automáticamente del día
- Puede editarse manualmente

### Integridad:
- No se puede pagar una cuenta ya Pagada
- Los pagos Anulados revierten saldos

---

## Consideraciones Técnicas

### Compatibilidad Legacy:
- Soporta compras a crédito sin CuentaPorPagar formal
- Calcula saldo basado en pagos existentes vs total de compra

### Transaccionalidad:
- El pago y actualización de saldos se procesan atómicamente
- Si falla la actualización, se revierte todo

### Concurrencia:
- Verificación de saldo antes de confirmar
- Previene pagos duplicados

### Auditoría:
- Usuario que registró el pago
- Fecha y hora exactas
- Caja y turno para cierre

### Rendimiento:
- Carga bajo demanda de cuotas expandidas
- Consultas optimizadas con Include

---

## Casos de Uso Comunes

### 1. Pago Total de Deuda
- Pagar el saldo completo al proveedor
- Una transacción liquida toda la deuda
- Estado cambia a Pagada

### 2. Pago Parcial
- Pagar parte del saldo
- Se actualiza SaldoPendiente
- Continúa en estado Abierta

### 3. Pago en Múltiples Medios
- Parte en efectivo, parte con transferencia
- Cada medio se registra como PagoProveedorDetalle
- Total suma de todos los medios

### 4. Pago en Moneda Diferente
- Deuda en USD, pago en Guaraníes
- Sistema convierte al tipo de cambio del día
- Registra moneda original y convertida

### 5. Pago con Cheque Diferido
- Registrar cheque para fecha futura
- Fecha de cobro del cheque registrada
- Seguimiento de cheques pendientes

### 6. Consulta de Vencidos
- Filtrar por estado VENCIDO
- Ver días de atraso por cuenta
- Priorizar pagos urgentes

---

## Diagrama de Relaciones

```
Compra (origen de la deuda)
    │
    └── CuentaPorPagar
            │
            ├── Proveedor (acreedor)
            │
            ├── Moneda
            │
            ├── Cuotas → CuentaPorPagarCuota[]
            │               │
            │               └── Estado (Pendiente/Pagada)
            │
            └── Pagos (via IdCompra) → PagoProveedor[]
                                          │
                                          ├── Proveedor
                                          ├── Caja
                                          ├── Sucursal
                                          ├── Moneda
                                          │
                                          └── Detalles → PagoProveedorDetalle[]
                                                          │
                                                          ├── MedioPago
                                                          ├── Cuota (opcional)
                                                          └── Datos específicos
```

---

## Flujo de Estados

```
[Compra a Crédito]
        ↓
[CuentaPorPagar: Abierta]
        │
        ├── (Fecha vence) → [VENCIDO]
        │                        │
        │                        ├── Pago Total → [Pagada]
        │                        └── Pago Parcial → Actualiza saldo
        │
        ├── Pago Parcial → Actualiza saldo, sigue Abierta
        │
        └── Pago Total → [Pagada]

[PagoProveedor: Pagado]
        │
        └── Anulación → [Anulado] → Revierte saldos
```

---

## Permisos Sugeridos

| Permiso | Descripción |
|---------|-------------|
| VIEW | Ver cuentas por pagar y pagos realizados |
| CREATE | Registrar nuevos pagos |
| EDIT | Modificar observaciones (pagos no se editan) |
| DELETE | Anular pagos |

**Nota:** Los pagos normalmente no se editan, solo se anulan y se registra uno nuevo si hay error.

---

## Servicio: PagoProveedorService

**Ubicación:** `Services/PagoProveedorService.cs`

**Responsabilidades:**
- Registrar pagos con validaciones
- Actualizar saldos de cuentas por pagar
- Distribuir pagos a cuotas
- Generar números de recibo

**Métodos principales:**
- `RegistrarPagoAsync()` - Registra un nuevo pago
- `AnularPagoAsync()` - Anula un pago existente
- `ObtenerSaldoCompraAsync()` - Calcula saldo actual de una compra
