# Plan de Implementación - Contabilidad Paraguaya

## 📋 Descripción General

Este documento describe el plan de acción para integrar un módulo contable completo al sistema SistemIA, cumpliendo con las normativas contables paraguayas y aprovechando los datos ya existentes de ventas, compras, cobros y pagos.

**Estado:** PENDIENTE (Sistema en modo TEST en cliente)
**Fecha de Documentación:** 26 de Enero de 2026
**Complejidad Estimada:** MEDIA-ALTA
**Tiempo Estimado:** 12-18 semanas

---

## 🎯 Objetivos

1. Implementar Plan de Cuentas configurable según normativa paraguaya
2. Generar asientos contables automáticos desde operaciones existentes
3. Producir libros contables oficiales (Diario, Mayor, Balances)
4. Mantener trazabilidad entre operaciones comerciales y contabilidad
5. Soportar múltiples ejercicios fiscales

---

## 📊 Modelos de Base de Datos

### 1. CuentaContable (Plan de Cuentas)

```csharp
public class CuentaContable
{
    public int IdCuentaContable { get; set; }
    
    // Identificación
    [MaxLength(20)]
    public string Codigo { get; set; }              // Ej: "1.1.01.001"
    [MaxLength(200)]
    public string Nombre { get; set; }              // Ej: "Caja General"
    [MaxLength(500)]
    public string? Descripcion { get; set; }
    
    // Jerarquía
    public int? IdCuentaPadre { get; set; }
    public CuentaContable? CuentaPadre { get; set; }
    public int Nivel { get; set; }                  // 1=Grupo, 2=Subgrupo, 3=Cuenta, 4=Subcuenta
    
    // Clasificación
    [MaxLength(20)]
    public string TipoCuenta { get; set; }          // ACTIVO, PASIVO, PATRIMONIO, INGRESO, EGRESO
    [MaxLength(20)]
    public string Naturaleza { get; set; }          // DEUDORA, ACREEDORA
    
    // Control
    public bool EsCuentaMovimiento { get; set; }    // Solo las de movimiento reciben asientos
    public bool Activo { get; set; } = true;
    
    // Auditoría
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    
    // Navegación
    public ICollection<CuentaContable> SubCuentas { get; set; }
    public ICollection<DetalleAsiento> DetallesAsiento { get; set; }
}
```

### 2. PeriodoContable (Ejercicio Fiscal)

```csharp
public class PeriodoContable
{
    public int IdPeriodoContable { get; set; }
    
    [MaxLength(50)]
    public string Nombre { get; set; }              // "Ejercicio 2026"
    public int Anio { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    
    [MaxLength(20)]
    public string Estado { get; set; }              // ABIERTO, CERRADO
    
    public bool EsActual { get; set; }
    public DateTime? FechaCierre { get; set; }
    
    // Navegación
    public ICollection<AsientoContable> Asientos { get; set; }
}
```

### 3. AsientoContable (Libro Diario)

```csharp
public class AsientoContable
{
    public int IdAsientoContable { get; set; }
    
    // Identificación
    public int NumeroAsiento { get; set; }          // Correlativo por período
    public DateTime Fecha { get; set; }
    
    // Período
    public int IdPeriodoContable { get; set; }
    public PeriodoContable PeriodoContable { get; set; }
    
    // Descripción
    [MaxLength(500)]
    public string Concepto { get; set; }            // "Venta Factura 001-001-0000123"
    
    // Origen (trazabilidad)
    [MaxLength(50)]
    public string? TipoDocumentoOrigen { get; set; } // VENTA, COMPRA, COBRO, PAGO, NC_VENTA, NC_COMPRA, AJUSTE
    public int? IdDocumentoOrigen { get; set; }      // IdVenta, IdCompra, etc.
    
    // Control
    [MaxLength(20)]
    public string Estado { get; set; }              // BORRADOR, CONTABILIZADO, ANULADO
    public bool EsAutomatico { get; set; }          // true si fue generado automáticamente
    
    // Totales (para verificación rápida)
    [Column(TypeName = "decimal(18,4)")]
    public decimal TotalDebe { get; set; }
    [Column(TypeName = "decimal(18,4)")]
    public decimal TotalHaber { get; set; }
    
    // Auditoría
    public int? IdUsuarioCreacion { get; set; }
    public DateTime FechaCreacion { get; set; }
    
    // Navegación
    public ICollection<DetalleAsiento> Detalles { get; set; }
}
```

### 4. DetalleAsiento (Líneas del Asiento)

```csharp
public class DetalleAsiento
{
    public int IdDetalleAsiento { get; set; }
    
    public int IdAsientoContable { get; set; }
    public AsientoContable AsientoContable { get; set; }
    
    public int IdCuentaContable { get; set; }
    public CuentaContable CuentaContable { get; set; }
    
    // Montos (uno u otro, no ambos)
    [Column(TypeName = "decimal(18,4)")]
    public decimal Debe { get; set; }
    [Column(TypeName = "decimal(18,4)")]
    public decimal Haber { get; set; }
    
    // Descripción adicional
    [MaxLength(300)]
    public string? Glosa { get; set; }              // Detalle específico de la línea
    
    // Centro de costo (opcional, para futuro)
    public int? IdCentroCosto { get; set; }
}
```

### 5. ConfiguracionContable (Mapeo automático)

```csharp
public class ConfiguracionContable
{
    public int IdConfiguracionContable { get; set; }
    
    // Tipo de operación
    [MaxLength(50)]
    public string TipoOperacion { get; set; }       // VENTA_CONTADO, VENTA_CREDITO, COMPRA_CONTADO, etc.
    
    // Cuentas asociadas
    public int? IdCuentaDebe { get; set; }          // Cuenta que se debita
    public int? IdCuentaHaber { get; set; }         // Cuenta que se acredita
    
    // Para IVA
    public int? IdCuentaIVA { get; set; }           // Cuenta de IVA (débito fiscal o crédito fiscal)
    
    // Descripción
    [MaxLength(200)]
    public string Descripcion { get; set; }
    
    public bool Activo { get; set; } = true;
    
    // Navegación
    public CuentaContable? CuentaDebe { get; set; }
    public CuentaContable? CuentaHaber { get; set; }
    public CuentaContable? CuentaIVA { get; set; }
}
```

---

## 📝 Plan de Cuentas Sugerido (Paraguay)

### Estructura Básica

```
1. ACTIVO
   1.1. ACTIVO CORRIENTE
        1.1.01. Caja y Bancos
               1.1.01.001 Caja General
               1.1.01.002 Caja Chica
               1.1.01.003 Banco Itaú Cta. Cte.
               1.1.01.004 Banco Continental Cta. Cte.
        1.1.02. Cuentas por Cobrar
               1.1.02.001 Clientes
               1.1.02.002 Documentos a Cobrar
               1.1.02.003 Anticipos a Proveedores
        1.1.03. Inventarios
               1.1.03.001 Mercaderías
               1.1.03.002 Productos Terminados
        1.1.04. Créditos Fiscales
               1.1.04.001 IVA Crédito Fiscal
   1.2. ACTIVO NO CORRIENTE
        1.2.01. Propiedad, Planta y Equipo
               1.2.01.001 Terrenos
               1.2.01.002 Edificios
               1.2.01.003 Muebles y Enseres
               1.2.01.004 Equipos de Computación
               1.2.01.005 Vehículos
        1.2.02. Depreciaciones Acumuladas (-)
               1.2.02.001 Dep. Acum. Edificios
               1.2.02.002 Dep. Acum. Muebles
               1.2.02.003 Dep. Acum. Equipos
               1.2.02.004 Dep. Acum. Vehículos

2. PASIVO
   2.1. PASIVO CORRIENTE
        2.1.01. Cuentas por Pagar
               2.1.01.001 Proveedores
               2.1.01.002 Documentos a Pagar
               2.1.01.003 Anticipos de Clientes
        2.1.02. Obligaciones Fiscales
               2.1.02.001 IVA Débito Fiscal
               2.1.02.002 Retenciones IVA
               2.1.02.003 IRE a Pagar
               2.1.02.004 IPS a Pagar
        2.1.03. Obligaciones Laborales
               2.1.03.001 Sueldos a Pagar
               2.1.03.002 Aguinaldo a Pagar
   2.2. PASIVO NO CORRIENTE
        2.2.01. Préstamos Bancarios

3. PATRIMONIO NETO
   3.1. CAPITAL
        3.1.01. Capital Social
        3.1.02. Aportes de Socios
   3.2. RESERVAS
        3.2.01. Reserva Legal
        3.2.02. Reservas Facultativas
   3.3. RESULTADOS
        3.3.01. Resultados Acumulados
        3.3.02. Resultado del Ejercicio

4. INGRESOS
   4.1. INGRESOS OPERATIVOS
        4.1.01. Ventas de Mercaderías
               4.1.01.001 Ventas Gravadas 10%
               4.1.01.002 Ventas Gravadas 5%
               4.1.01.003 Ventas Exentas
        4.1.02. Devoluciones y Descuentos (-)
               4.1.02.001 Devoluciones sobre Ventas
               4.1.02.002 Descuentos sobre Ventas
   4.2. OTROS INGRESOS
        4.2.01. Ingresos Financieros
        4.2.02. Otros Ingresos

5. EGRESOS / GASTOS
   5.1. COSTO DE VENTAS
        5.1.01. Costo de Mercaderías Vendidas
   5.2. GASTOS OPERATIVOS
        5.2.01. Gastos de Personal
               5.2.01.001 Sueldos y Salarios
               5.2.01.002 Aguinaldo
               5.2.01.003 Aportes Patronales IPS
        5.2.02. Gastos Administrativos
               5.2.02.001 Alquileres
               5.2.02.002 Servicios Básicos
               5.2.02.003 Útiles de Oficina
               5.2.02.004 Depreciaciones
        5.2.03. Gastos de Ventas
               5.2.03.001 Comisiones
               5.2.03.002 Publicidad
   5.3. GASTOS FINANCIEROS
        5.3.01. Intereses Bancarios
        5.3.02. Comisiones Bancarias
```

---

## 🔄 Asientos Automáticos

### 1. Venta al Contado (Efectivo)

```
Fecha: [Fecha Venta]
Concepto: Venta Factura [Establecimiento]-[PuntoExp]-[Numero]

DEBE:
  1.1.01.001 Caja General ............... [Total]

HABER:
  4.1.01.001 Ventas Gravadas 10% ........ [BaseGravada10]
  4.1.01.002 Ventas Gravadas 5% ......... [BaseGravada5]
  4.1.01.003 Ventas Exentas ............. [TotalExenta]
  2.1.02.001 IVA Débito Fiscal .......... [TotalIVA10 + TotalIVA5]
```

### 2. Venta a Crédito

```
Fecha: [Fecha Venta]
Concepto: Venta Factura [Numero] - Cliente: [NombreCliente]

DEBE:
  1.1.02.001 Clientes ................... [Total]

HABER:
  4.1.01.001 Ventas Gravadas 10% ........ [BaseGravada10]
  4.1.01.002 Ventas Gravadas 5% ......... [BaseGravada5]
  4.1.01.003 Ventas Exentas ............. [TotalExenta]
  2.1.02.001 IVA Débito Fiscal .......... [TotalIVA10 + TotalIVA5]
```

### 3. Cobro de Crédito

```
Fecha: [Fecha Cobro]
Concepto: Cobro Factura [Numero] - Cliente: [NombreCliente]

DEBE:
  1.1.01.001 Caja General ............... [MontoCobrado]

HABER:
  1.1.02.001 Clientes ................... [MontoCobrado]
```

### 4. Compra al Contado

```
Fecha: [Fecha Compra]
Concepto: Compra Factura [Numero] - Proveedor: [NombreProveedor]

DEBE:
  5.1.01.001 Costo de Mercaderías ....... [BaseGravada] (o cuenta de gasto)
  1.1.04.001 IVA Crédito Fiscal ......... [TotalIVA]

HABER:
  1.1.01.001 Caja General ............... [Total]
```

### 5. Compra a Crédito

```
Fecha: [Fecha Compra]
Concepto: Compra Factura [Numero] - Proveedor: [NombreProveedor]

DEBE:
  5.1.01.001 Costo de Mercaderías ....... [BaseGravada]
  1.1.04.001 IVA Crédito Fiscal ......... [TotalIVA]

HABER:
  2.1.01.001 Proveedores ................ [Total]
```

### 6. Pago a Proveedor

```
Fecha: [Fecha Pago]
Concepto: Pago Factura [Numero] - Proveedor: [NombreProveedor]

DEBE:
  2.1.01.001 Proveedores ................ [MontoPagado]

HABER:
  1.1.01.001 Caja General ............... [MontoPagado]
```

### 7. Nota de Crédito Venta

```
Fecha: [Fecha NC]
Concepto: NC Venta [Numero] - Ref. Factura [NumeroFactura]

DEBE:
  4.1.02.001 Devoluciones sobre Ventas .. [BaseGravada]
  2.1.02.001 IVA Débito Fiscal .......... [TotalIVA]

HABER:
  1.1.02.001 Clientes ................... [Total] (si era crédito)
  1.1.01.001 Caja General ............... [Total] (si se devuelve efectivo)
```

### 8. Nota de Crédito Compra

```
Fecha: [Fecha NC]
Concepto: NC Compra [Numero] - Ref. Factura [NumeroFactura]

DEBE:
  2.1.01.001 Proveedores ................ [Total]

HABER:
  5.1.01.001 Costo de Mercaderías ....... [BaseGravada]
  1.1.04.001 IVA Crédito Fiscal ......... [TotalIVA]
```

---

## 📚 Libros Contables a Generar

### 1. Libro Diario
- Listado cronológico de todos los asientos
- Filtros: Período, Fecha desde/hasta, Tipo de operación
- Columnas: Fecha, N° Asiento, Concepto, Cuenta, Debe, Haber

### 2. Libro Mayor
- Movimientos por cuenta contable
- Saldo inicial, movimientos, saldo final
- Filtros: Cuenta, Período, Fecha desde/hasta

### 3. Balance de Sumas y Saldos (Balance de Comprobación)
- Resumen de todas las cuentas de movimiento
- Columnas: Cuenta, Sumas Debe, Sumas Haber, Saldo Deudor, Saldo Acreedor

### 4. Balance General
- Situación patrimonial a una fecha
- Activo = Pasivo + Patrimonio

### 5. Estado de Resultados
- Ingresos - Egresos = Resultado del período

---

## 🛠️ Servicios a Implementar

### 1. ContabilidadService

```csharp
public interface IContabilidadService
{
    // Asientos
    Task<AsientoContable> CrearAsientoAsync(AsientoContable asiento);
    Task<AsientoContable> GenerarAsientoVentaAsync(Venta venta);
    Task<AsientoContable> GenerarAsientoCompraAsync(Compra compra);
    Task<AsientoContable> GenerarAsientoCobroAsync(CobroCuota cobro);
    Task<AsientoContable> GenerarAsientoPagoAsync(PagoProveedor pago);
    Task<AsientoContable> GenerarAsientoNCVentaAsync(NotaCreditoVenta nc);
    Task<AsientoContable> GenerarAsientoNCCompraAsync(NotaCreditoCompra nc);
    Task AnularAsientoAsync(int idAsiento, string motivo);
    
    // Consultas
    Task<List<AsientoContable>> ObtenerLibroDiarioAsync(int idPeriodo, DateTime? desde, DateTime? hasta);
    Task<LibroMayorResult> ObtenerLibroMayorAsync(int idCuenta, int idPeriodo, DateTime? desde, DateTime? hasta);
    Task<BalanceComprobacionResult> ObtenerBalanceComprobacionAsync(int idPeriodo, DateTime? fechaCorte);
    Task<BalanceGeneralResult> ObtenerBalanceGeneralAsync(int idPeriodo, DateTime fechaCorte);
    Task<EstadoResultadosResult> ObtenerEstadoResultadosAsync(int idPeriodo, DateTime desde, DateTime hasta);
    
    // Períodos
    Task<PeriodoContable> AbrirPeriodoAsync(int anio);
    Task CerrarPeriodoAsync(int idPeriodo);
}
```

### 2. CuentaContableService

```csharp
public interface ICuentaContableService
{
    Task<List<CuentaContable>> ObtenerPlanCuentasAsync();
    Task<List<CuentaContable>> ObtenerCuentasMovimientoAsync();
    Task<CuentaContable> CrearCuentaAsync(CuentaContable cuenta);
    Task<CuentaContable> ActualizarCuentaAsync(CuentaContable cuenta);
    Task<bool> TieneMovimientosAsync(int idCuenta);
    Task DesactivarCuentaAsync(int idCuenta);
}
```

---

## 📄 Páginas a Crear

### Módulo Contabilidad

| Página | Ruta | Descripción |
|--------|------|-------------|
| PlanCuentas.razor | /contabilidad/plan-cuentas | CRUD del plan de cuentas (árbol) |
| PlanCuentasExplorar.razor | /contabilidad/plan-cuentas/explorar | Vista y búsqueda del plan |
| AsientosContables.razor | /contabilidad/asientos | Crear/editar asientos manuales |
| AsientosExplorar.razor | /contabilidad/asientos/explorar | Explorador de asientos |
| LibroDiario.razor | /contabilidad/libro-diario | Reporte Libro Diario |
| LibroMayor.razor | /contabilidad/libro-mayor | Reporte Libro Mayor |
| BalanceComprobacion.razor | /contabilidad/balance-comprobacion | Balance de Sumas y Saldos |
| BalanceGeneral.razor | /contabilidad/balance-general | Balance General |
| EstadoResultados.razor | /contabilidad/estado-resultados | Estado de Resultados |
| PeriodosContables.razor | /contabilidad/periodos | Gestión de ejercicios fiscales |
| ConfiguracionContable.razor | /contabilidad/configuracion | Mapeo de cuentas automático |

---

## 📅 Fases de Implementación

### FASE 1: Fundamentos (3-4 semanas)
- [ ] Crear modelos de datos (CuentaContable, PeriodoContable, AsientoContable, DetalleAsiento, ConfiguracionContable)
- [ ] Crear migraciones EF Core
- [ ] Implementar CuentaContableService
- [ ] Crear página PlanCuentas.razor (CRUD con vista de árbol)
- [ ] Crear página PlanCuentasExplorar.razor
- [ ] Insertar plan de cuentas inicial (datos semilla)
- [ ] Crear página PeriodosContables.razor

### FASE 2: Asientos Manuales (2-3 semanas)
- [ ] Implementar ContabilidadService (operaciones básicas)
- [ ] Crear página AsientosContables.razor (crear/editar asientos)
- [ ] Crear página AsientosExplorar.razor
- [ ] Validación de partida doble (Debe = Haber)
- [ ] Validación de cuentas de movimiento

### FASE 3: Asientos Automáticos (3-4 semanas)
- [ ] Crear página ConfiguracionContable.razor
- [ ] Implementar GenerarAsientoVentaAsync
- [ ] Implementar GenerarAsientoCompraAsync
- [ ] Implementar GenerarAsientoCobroAsync
- [ ] Implementar GenerarAsientoPagoAsync
- [ ] Implementar GenerarAsientoNCVentaAsync
- [ ] Implementar GenerarAsientoNCCompraAsync
- [ ] Integrar generación automática en flujos existentes
- [ ] Opción para contabilizar operaciones históricas

### FASE 4: Reportes (3-4 semanas)
- [ ] Crear página LibroDiario.razor
- [ ] Crear página LibroMayor.razor
- [ ] Crear página BalanceComprobacion.razor
- [ ] Crear página BalanceGeneral.razor
- [ ] Crear página EstadoResultados.razor
- [ ] Exportación a Excel/PDF de todos los reportes
- [ ] Cierre de período contable

### FASE 5: Mejoras (2-3 semanas)
- [ ] Dashboard contable con indicadores
- [ ] Centros de costo (opcional)
- [ ] Ajustes de cierre de ejercicio
- [ ] Asiento de apertura automático
- [ ] Comparativos entre períodos

---

## 🔗 Integración con Módulos Existentes

### Puntos de Integración

1. **Ventas.razor** - Al confirmar venta, opción de generar asiento automático
2. **Compras.razor** - Al confirmar compra, opción de generar asiento automático
3. **Cobros** - Al registrar cobro, generar asiento
4. **PagosProveedores** - Al registrar pago, generar asiento
5. **NotasCreditoVentas** - Al confirmar NC, generar asiento
6. **NotasCreditoCompras** - Al confirmar NC, generar asiento

### Configuración Global

En `ConfiguracionGeneral` agregar:
- `ContabilidadActiva` (bool) - Habilita/deshabilita módulo contable
- `GenerarAsientosAutomaticos` (bool) - Si es true, genera asientos al confirmar operaciones

---

## ⚠️ Consideraciones Importantes

### Normativas Paraguayas
- Resolución General N° 4/2019 SET - Libros contables obligatorios
- Ley 125/91 - Régimen tributario
- Código Civil - Obligaciones del comerciante

### Auditoría
- Todos los asientos deben ser trazables al documento origen
- Los asientos contabilizados NO pueden modificarse (solo anular y crear nuevo)
- Mantener log de cambios en asientos

### Performance
- Índices en campos de búsqueda frecuente (Fecha, NumeroAsiento, IdCuentaContable)
- Considerar vistas materializadas para balances grandes

---

## 📝 Notas Adicionales

- El módulo debe ser **opcional** - el sistema debe funcionar sin contabilidad
- Permitir contabilizar operaciones **retroactivamente** para migración inicial
- Los reportes deben ser **exportables** a formatos estándar (Excel, PDF)
- Considerar **multi-moneda** para empresas que operen en USD

---

## 📞 Contacto para Implementación

Cuando el cliente esté listo para implementar este módulo, coordinar:
1. Definición del plan de cuentas específico de la empresa
2. Mapeo de cuentas para operaciones automáticas
3. Migración de saldos iniciales (si existe contabilidad previa)
4. Capacitación al personal contable

---

*Documento generado el 26 de Enero de 2026*
*Versión: 1.0*
