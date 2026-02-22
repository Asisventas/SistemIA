# Módulos Especiales de SistemIA - Estado de Implementación

**Última actualización:** 14 de febrero de 2026, 12:15

---

# 📋 ÍNDICE DE MÓDULOS

1. [Módulo de Suscripciones y Facturación Automática](#módulo-de-suscripciones-y-facturación-automática)
2. [Módulo de Restaurante (Mesas/Pedidos)](#módulo-de-restaurante-mesaspedidos)
3. [Módulo de Complejos Deportivos (Canchas/Reservas)](#módulo-de-complejos-deportivos-canchasreservas)
4. [Módulo de Taller Mecánico](#módulo-de-taller-mecánico)

---

# 🔄 Módulo de Suscripciones y Facturación Automática

Sistema de facturación recurrente que permite:
- Crear suscripciones para clientes con productos/servicios recurrentes
- Generar la primera factura como "plantilla" que se replica automáticamente
- Facturación automática según período (mensual, bimestral, trimestral, etc.)
- Envío automático de facturas por correo

---

## 🗂️ Archivos del Módulo

### Modelos
| Archivo | Descripción |
|---------|-------------|
| `Models/Suscripciones/SuscripcionCliente.cs` | Entidad principal de suscripción |
| `Models/Suscripciones/FacturaAutomatica.cs` | Registro de facturas generadas automáticamente |

### Servicios
| Archivo | Descripción |
|---------|-------------|
| `Services/FacturacionAutomaticaService.cs` | Lógica de generación de facturas |
| `Services/FacturacionAutomaticaBackgroundService.cs` | Servicio en segundo plano que ejecuta facturación |

### Páginas
| Archivo | Descripción |
|---------|-------------|
| `Pages/SuscripcionesClientes.razor` | Página principal CRUD de suscripciones |

---

## 🔧 Modelo SuscripcionCliente - Campos Principales

```csharp
public class SuscripcionCliente
{
    public int IdSuscripcion { get; set; }
    public int IdCliente { get; set; }
    public int IdSucursal { get; set; }
    
    // Producto individual (modo legacy)
    public int? IdProducto { get; set; }
    public decimal Cantidad { get; set; } = 1;
    public decimal MontoFacturar { get; set; }
    
    // ========== FACTURA PLANTILLA (NUEVO) ==========
    public int? IdVentaReferencia { get; set; }  // FK a Ventas - La primera factura sirve como plantilla
    public Venta? VentaReferencia { get; set; }
    
    // Configuración de recurrencia
    public string TipoPeriodo { get; set; } = "Mensual";  // Mensual, Bimestral, Trimestral, Semestral, Anual
    public int DiaFacturacion { get; set; } = 1;          // Día del mes
    public TimeSpan HoraFacturacion { get; set; }         // Hora de generación
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    
    // Estado
    public string Estado { get; set; } = "Activa";        // Activa, Pausada, Cancelada
    public bool FacturacionActiva { get; set; } = true;
    public bool EnviarPorCorreo { get; set; } = true;
    public TimeSpan? HoraEnvioCorreo { get; set; }
    
    // Historial
    public DateTime? FechaUltimaFactura { get; set; }
    public DateTime? FechaProximaFactura { get; set; }
    
    // Configuración adicional
    public int? IdCaja { get; set; }                      // Caja donde se registran las facturas
    public string CondicionPago { get; set; } = "Contado";
    public string? Observaciones { get; set; }
}
```

---

## 🔄 Flujo de Creación de Suscripción

### Paso 1: Nueva Suscripción
1. Usuario abre `/suscripciones`
2. Click en "Nueva Suscripción"
3. Se abre modal "Editar Suscripción"

### Paso 2: Seleccionar Cliente
1. Buscar cliente por nombre o RUC
2. Seleccionar de la lista

### Paso 3: Crear Primera Factura (Plantilla)
1. Click en "Crear Primera Factura (Plantilla)"
2. Se abre modal con iframe a `/ventas?modal=1&idCliente=X&suscripcion=1`
3. Usuario agrega productos, precios, condiciones
4. Guarda la factura → Se imprime ticket
5. **Al cerrar el ticket** → postMessage al padre → cierra modal → muestra plantilla creada

### Paso 4: Configurar Recurrencia
- Período: Mensual, Bimestral, etc.
- Día del mes: 1-28
- Hora de facturación
- Fecha de primera factura (próxima)
- Caja donde se registrarán las facturas

### Paso 5: Guardar Suscripción
- Se guarda con `IdVentaReferencia` apuntando a la factura plantilla

---

## ⚠️ Estado Actual - IMPLEMENTADO (14-Feb-2026)

### ✅ Correcciones Aplicadas

#### 1. Modal cierra al presionar "Cerrar" en ticket (modo suscripción)
**Problema:** Después de guardar e imprimir la factura plantilla, al presionar "Cerrar" en el ticket, el modal iframe no se cerraba.

**Causa:** `CerrarVistaPrevia()` era sync y no enviaba postMessage al padre. Solo `OnImprimirCompletado` notificaba.

**Solución:** `CerrarVistaPrevia()` ahora es `async Task` y envía postMessage al padre en modo suscripción y modo modal.

```csharp
// Pages/Ventas.razor.cs - CerrarVistaPrevia()
private async Task CerrarVistaPrevia()
{
    var idVentaCerrada = _idVentaParaVistaPrevia;
    _mostrarVistaPrevia = false;
    _idVentaParaVistaPrevia = 0;

    if (EsModoSuscripcion && idVentaCerrada > 0)
    {
        await JS.InvokeVoidAsync("window.parent.postMessage", 
            new { tipo = "ventaSuscripcionCreada", idVenta = idVentaCerrada, impreso = false }, "*");
    }
}
```

#### 2. Guardar plantilla en suscripción
Al guardar la suscripción con factura plantilla:
- Se vincula la venta con `IdSuscripcion` en la tabla Ventas
- Se crea registro en `FacturasAutomaticas` como primera factura generada
- Se actualiza `FechaUltimaFactura`, `FechaProximaFactura` y `TotalFacturasGeneradas`

#### 3. Eliminar plantilla y recrear
Dos opciones disponibles:
- **Eliminar plantilla**: Desvincula la venta de la suscripción (la venta emitida NO se elimina)
- **Cambiar plantilla**: Desvincula la anterior y abre modal para crear nueva factura

#### 4. Relación Venta ↔ Suscripción
- Nuevo campo `IdSuscripcion` (int?) en `Models/Venta.cs`
- Migración: `Agregar_IdSuscripcion_En_Ventas`
- Permite identificar qué ventas pertenecen a qué suscripción
- La facturación automática futura puede consultar ventas por suscripción

**Código relevante:**

**Ventas.razor.cs (~línea 2687):**
```csharp
// Notificar al padre si es modo suscripción
if (EsModoSuscripcion)
{
    try
    {
        await JS.InvokeVoidAsync("window.parent.postMessage", 
            new { tipo = "ventaSuscripcionCreada", idVenta = Cab.IdVenta, idCliente = Cab.IdCliente }, "*");
    }
    catch { /* Ignorar si no está en iframe */ }
}
```

**Ventas.razor.cs - OnImprimirCompletado (~línea 1782):**
```csharp
private async Task OnImprimirCompletado()
{
    Console.WriteLine($"[Ventas] Impresión completada para venta: {_idVentaParaVistaPrevia}");
    
    // En modo suscripción, cerrar automáticamente y notificar al padre
    if (EsModoSuscripcion)
    {
        _mostrarVistaPrevia = false;
        try
        {
            await JS.InvokeVoidAsync("window.parent.postMessage", 
                new { tipo = "ventaSuscripcionCreada", idVenta = _idVentaParaVistaPrevia, impreso = true }, "*");
        }
        catch { /* Ignorar si no está en iframe */ }
        _idVentaParaVistaPrevia = 0;
    }
}
```

**SuscripcionesClientes.razor - Listener (~línea 483):**
```csharp
await JS.InvokeVoidAsync("eval", @"
    if (!window._suscripcionesListenerAdded) {
        window._suscripcionesListenerAdded = true;
        window.addEventListener('message', function(e) {
            console.log('[Suscripciones] PostMessage recibido:', e.data);
            if (e.data && e.data.tipo === 'ventaSuscripcionCreada' && e.data.idVenta) {
                console.log('[Suscripciones] Invocando método .NET con idVenta:', e.data.idVenta);
                DotNet.invokeMethodAsync('SistemIA', 'RecibirVentaSuscripcionGlobal', e.data.idVenta)
                    .then(function() { console.log('[Suscripciones] Método .NET invocado exitosamente'); })
                    .catch(function(err) { console.error('[Suscripciones] Error invocando método .NET:', err); });
            }
        });
        console.log('[Suscripciones] Listener de postMessage registrado');
    }
");
```

**SuscripcionesClientes.razor - Método estático (~línea 642):**
```csharp
[JSInvokable("RecibirVentaSuscripcionGlobal")]
public static void RecibirVentaSuscripcionGlobal(int idVenta)
{
    _ultimaVentaCreada = idVenta;
    OnVentaSuscripcionCreada?.Invoke(idVenta);
}

private async Task ManejarVentaCreada(int idVenta)
{
    // Carga la venta, actualiza _suscripcionEdit.IdVentaReferencia
    // Muestra mensaje de confirmación
    // Cierra modal: _mostrarModalVenta = false;
}
```

### 🔍 Debug Recomendado
1. Abrir consola del navegador (F12)
2. Buscar mensajes `[Suscripciones]` y `[Ventas]`
3. Verificar si postMessage se envía y recibe correctamente

---

## ✅ Funcionalidades Completadas

1. **Modelo SuscripcionCliente** con campo `IdVentaReferencia`
2. **Página SuscripcionesClientes.razor** rediseñada con modal iframe
3. **UI de plantilla creada** con card verde y botones Ver/Eliminar/Cambiar
4. **Mensaje informativo** explicando que se genera la primera factura
5. **FacturacionAutomaticaService** soporta `VentaReferencia` para copiar detalles
6. **Ventas.razor.cs** con parámetros `ModoSuscripcion` y `IdClienteParam`
7. **postMessage** enviándose al guardar venta, al imprimir Y al cerrar ticket
8. **CerrarVistaPrevia** (async) envía postMessage al cerrar ticket en modo suscripción
9. **Botón "Generar factura ahora"** usa `FacturacionService.GenerarFacturaAsync()`
10. **Eliminar plantilla**: Desvincula venta de suscripción sin eliminar la factura
11. **Cambiar plantilla**: Desvincula anterior y abre modal para crear nueva
12. **Relación Venta.IdSuscripcion**: Vínculo bidireccional suscripción ↔ venta
13. **Registro FacturaAutomatica**: Primera factura se registra en historial al guardar
14. **FechaProximaFactura**: Se calcula automáticamente al guardar suscripción con plantilla

---

## 📝 Próximos Pasos

1. **Probar flujo completo** en navegador: crear suscripción → factura → cerrar ticket → guardar
2. **Probar generación automática** de facturas recurrentes
3. **Agregar Monitor de Facturación** para ver estado de suscripciones y facturas generadas
4. **Histórico de facturas** por suscripción (explorador de FacturasAutomaticas)

---

## 🔗 Dependencias entre Archivos

```
SuscripcionesClientes.razor
    ├── Usa iframe con Ventas.razor
    │       └── Ventas.razor.cs (ModoSuscripcion, postMessage)
    ├── Llama a FacturacionAutomaticaService
    │       └── GenerarFacturaAsync() copia de VentaReferencia
    └── Modelos:
            ├── SuscripcionCliente
            └── FacturaAutomatica
```

---

## 🛠️ Comandos Útiles

```powershell
# Reiniciar servidor
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Process -FilePath "dotnet" -ArgumentList "run","--urls","http://localhost:5095" -WorkingDirectory "c:\asis\SistemIA" -WindowStyle Normal

# Ver errores de compilación
cd c:\asis\SistemIA; dotnet build

# Crear migración (si se modifica modelo)
dotnet ef migrations add Nombre_Migracion
dotnet ef database update
```

---

## 📊 Tablas de Base de Datos

| Tabla | Descripción |
|-------|-------------|
| `SuscripcionesClientes` | Suscripciones activas |
| `FacturasAutomaticas` | Historial de facturas generadas |
| `Ventas` | Facturas (incluyendo plantillas) |
| `VentasDetalles` | Líneas de productos de cada factura |

---

## 🔴 NOTA IMPORTANTE - Error VentaIdVenta Resuelto

Se eliminó la propiedad de navegación `VentasDetalles` del modelo `Venta.cs` porque causaba que EF Core generara una columna `VentaIdVenta` inexistente.

**Solución aplicada:**
- Eliminado `[InverseProperty("Venta")] public ICollection<VentaDetalle>? VentasDetalles` de `Venta.cs`
- Cargar detalles por separado: `db.VentasDetalles.Where(d => d.IdVenta == idVenta).ToListAsync()`

---

# 🍽️ Módulo de Restaurante (Mesas/Pedidos)

Sistema para gestión de mesas, pedidos y facturación en restaurantes/bares.

## Estado: ✅ COMPLETADO

## 🗂️ Archivos del Módulo

### Modelos
| Archivo | Descripción |
|---------|-------------|
| `Models/Mesa.cs` | Mesas, zonas, capacidad, posición visual |
| `Models/Pedido.cs` | Pedido/comanda de una mesa |
| `Models/PedidoDetalle.cs` | Líneas de productos del pedido |
| `Models/PedidoPago.cs` | Pagos parciales del pedido |

### Páginas
| Archivo | Descripción |
|---------|-------------|
| `Pages/MesasPanel.razor` | Panel visual interactivo de mesas (drag & drop) |
| `Pages/Mesas.razor` | CRUD de configuración de mesas |
| `Pages/PedidoMesa.razor` | Toma de pedido en una mesa específica |
| `Pages/PedidosExplorar.razor` | Explorador de pedidos |

## 🔧 Modelo Mesa - Campos Principales

```csharp
public class Mesa
{
    public int IdMesa { get; set; }
    public int IdSucursal { get; set; }
    
    // Identificación
    [MaxLength(20)] public string Numero { get; set; }      // "1", "VIP-1", "Cancha A"
    [MaxLength(100)] public string? Nombre { get; set; }
    [MaxLength(500)] public string? Descripcion { get; set; }
    
    // Tipo y categoría
    [MaxLength(50)] public string Tipo { get; set; } = "Mesa";  // Mesa, Cancha, Sala, Terraza
    [MaxLength(50)] public string? Zona { get; set; }            // Interior, VIP, Planta Alta
    public int Capacidad { get; set; } = 4;
    
    // Visualización en panel (drag & drop)
    public int PosicionX { get; set; }
    public int PosicionY { get; set; }
    public int Ancho { get; set; } = 100;
    public int Alto { get; set; } = 100;
    
    // Estado
    [MaxLength(20)] public string Estado { get; set; } = "Libre";  // Libre, Ocupada, Reservada, Mantenimiento
    public bool Activo { get; set; } = true;
    
    // Configuración para canchas
    public decimal? PrecioPorHora { get; set; }
    public int? DuracionMinima { get; set; }
}
```

## 🔧 Modelo Pedido - Campos Principales

```csharp
public class Pedido
{
    public int IdPedido { get; set; }
    public int IdSucursal { get; set; }
    public int? IdCaja { get; set; }
    public int? Turno { get; set; }
    public DateTime? FechaCaja { get; set; }
    
    // Mesa/Espacio
    public int IdMesa { get; set; }
    public Mesa? Mesa { get; set; }
    
    // Identificación
    public int NumeroPedido { get; set; }
    public int Comensales { get; set; } = 1;
    [MaxLength(200)] public string? NombreCliente { get; set; }
    public int? IdCliente { get; set; }
    
    // Tiempos
    public DateTime FechaApertura { get; set; }
    public DateTime? FechaCierre { get; set; }
    public DateTime? HoraInicio { get; set; }   // Para canchas
    public DateTime? HoraFin { get; set; }
    
    // Estado
    [MaxLength(20)] public string Estado { get; set; } = "Abierto";  // Abierto, Cerrado, Cancelado
    
    // Totales
    [Column(TypeName = "decimal(18,4)")] public decimal Subtotal { get; set; }
    [Column(TypeName = "decimal(18,4)")] public decimal Descuento { get; set; }
    [Column(TypeName = "decimal(18,4)")] public decimal Total { get; set; }
    [Column(TypeName = "decimal(18,4)")] public decimal TotalPagado { get; set; }
    [Column(TypeName = "decimal(18,4)")] public decimal Saldo { get; set; }
    
    // Usuario
    public int? IdUsuarioApertura { get; set; }
    public int? IdUsuarioCierre { get; set; }
}
```

## 🎯 Funcionalidades del Panel de Mesas

1. **Vista visual** de mesas con colores según estado (verde=libre, rojo=ocupada, amarillo=reservada)
2. **Drag & drop** para reposicionar mesas
3. **Menú contextual** al hacer clic derecho en mesa
4. **Abrir mesa** → Crea pedido nuevo
5. **Agregar productos** al pedido activo
6. **Cobrar** → Abre modal de ventas con los productos del pedido
7. **División de cuenta** (pagar parcial, agregar comensales)
8. **Historial** de consumos por mesa

## 🔄 Flujo de Operación

```
Mesa Libre → Abrir Mesa → Pedido Abierto → Agregar Productos → Cobrar → Venta Generada → Mesa Libre
```

---

# ⚽ Módulo de Complejos Deportivos (Canchas/Reservas)

Sistema para gestión de canchas deportivas, alquiler por hora y reservas.

## Estado: ✅ COMPLETADO

## 🗂️ Archivos del Módulo

### Modelos
| Archivo | Descripción |
|---------|-------------|
| `Models/Mesa.cs` | Canchas (Tipo="Cancha") |
| `Models/Reserva.cs` | Reservas de canchas |
| `Models/Pedido.cs` | Ocupación activa de cancha |

### Páginas
| Archivo | Descripción |
|---------|-------------|
| `Pages/MesasPanel.razor` | Panel visual de canchas (mismo componente) |
| `Pages/Reservas.razor` | Gestión de reservas |

## 🔧 Modelo Reserva - Campos Principales

```csharp
public class Reserva
{
    public int IdReserva { get; set; }
    public int IdSucursal { get; set; }
    public int IdMesa { get; set; }           // Cancha
    public Mesa? Mesa { get; set; }
    
    // Identificación
    public int NumeroReserva { get; set; }
    
    // Cliente
    [MaxLength(200)] public string NombreCliente { get; set; }
    [MaxLength(30)] public string? Telefono { get; set; }
    [MaxLength(100)] public string? Email { get; set; }
    public int? IdCliente { get; set; }
    
    // Fecha y hora
    public DateTime FechaReserva { get; set; }
    public TimeSpan HoraInicio { get; set; }
    public TimeSpan? HoraFin { get; set; }
    public int? DuracionMinutos { get; set; }
    
    // Estado
    [MaxLength(20)] public string Estado { get; set; } = "Confirmada";  // Pendiente, Confirmada, EnCurso, Completada, Cancelada, NoShow
    
    // Precios
    [Column(TypeName = "decimal(18,4)")] public decimal? PrecioTotal { get; set; }
    [Column(TypeName = "decimal(18,4)")] public decimal? SeniaAbonada { get; set; }
    
    // Notificaciones
    public bool NotificarPorWhatsApp { get; set; }
    public bool NotificarPorEmail { get; set; }
    public DateTime? UltimaNotificacion { get; set; }
}
```

## 🎯 Funcionalidades de Canchas

1. **Configuración por hora** con precio por hora
2. **Reservas anticipadas** con fecha/hora
3. **Seña/anticipo** para confirmar reserva
4. **Notificaciones** por WhatsApp/Email
5. **Cronómetro** de tiempo restante
6. **Cobro al finalizar** con cálculo automático por tiempo

## 🔄 Flujo de Cancha

```
Cancha Libre → Reservar/Abrir → HoraInicio → Cronómetro → HoraFin → Cobrar → Cancha Libre
```

---

# 🔧 Módulo de Taller Mecánico

Sistema para gestión de talleres mecánicos con órdenes de trabajo, vehículos y seguimiento.

## Estado: ✅ COMPLETADO

## 🗂️ Archivos del Módulo

### Modelos
| Archivo | Descripción |
|---------|-------------|
| `Models/Vehiculo.cs` | Vehículos de clientes |
| `Models/OrdenTrabajo.cs` | Orden de trabajo (OT) |
| `Models/OrdenTrabajoDetalle.cs` | Líneas de servicios/repuestos |
| `Models/Mesa.cs` | Bahías de trabajo (Tipo="Bahía") |

### Páginas
| Archivo | Descripción |
|---------|-------------|
| `Pages/PantallaTaller.razor` | Panel visual de bahías |
| `Pages/Vehiculos.razor` | CRUD de vehículos |

## 🔧 Modelo Vehiculo - Campos Principales

```csharp
public class Vehiculo
{
    public int IdVehiculo { get; set; }
    public int? IdCliente { get; set; }
    public int IdSucursal { get; set; }
    
    // Identificación
    [MaxLength(20)] public string Matricula { get; set; }      // "ABC 123"
    [MaxLength(50)] public string? Marca { get; set; }         // Toyota, Ford
    [MaxLength(50)] public string? Modelo { get; set; }        // Corolla, Focus
    public int? Anio { get; set; }
    [MaxLength(30)] public string? Color { get; set; }
    [MaxLength(20)] public string? NumeroChasis { get; set; }  // VIN (17 chars)
    [MaxLength(30)] public string? NumeroMotor { get; set; }
    [MaxLength(20)] public string? TipoCombustible { get; set; }  // Nafta, Diesel, GNC
    [MaxLength(20)] public string? TipoVehiculo { get; set; }     // Auto, Camioneta, Moto
    
    // Estado
    public int? UltimoKilometraje { get; set; }
    public DateTime? UltimoServicio { get; set; }
    public bool Activo { get; set; } = true;
}
```

## 🔧 Modelo OrdenTrabajo - Campos Principales

```csharp
public class OrdenTrabajo
{
    public int IdOrdenTrabajo { get; set; }
    public int IdSucursal { get; set; }
    public int? IdCaja { get; set; }
    public int? Turno { get; set; }
    public DateTime? FechaCaja { get; set; }
    
    // Bahía y Vehículo
    public int? IdMesa { get; set; }           // Bahía
    public int IdVehiculo { get; set; }
    public Vehiculo? Vehiculo { get; set; }
    public int? IdCliente { get; set; }
    
    // Identificación
    public int NumeroOrden { get; set; }
    public int AnioOrden { get; set; }
    [MaxLength(30)] public string? CodigoOrden { get; set; }  // "OT-2026-0001"
    
    // Datos al ingreso
    public int? KilometrajeIngreso { get; set; }
    public int? NivelCombustible { get; set; }  // 0-100%
    [MaxLength(1000)] public string? EstadoIngreso { get; set; }
    public string? FotosIngreso { get; set; }    // JSON con URLs
    
    // Descripción del trabajo
    [MaxLength(1000)] public string? MotivoConsulta { get; set; }
    [MaxLength(2000)] public string? Diagnostico { get; set; }
    [MaxLength(2000)] public string? TrabajoRealizado { get; set; }
    
    // Estado
    [MaxLength(30)] public string Estado { get; set; } = "Recepcion";
    // Estados: Recepcion, Diagnostico, Esperando (repuestos/aprobación), EnProceso, Listo, Entregado, Cancelado
    
    // Tiempos
    public DateTime FechaIngreso { get; set; }
    public DateTime? FechaInicioTrabajo { get; set; }
    public DateTime? FechaFinTrabajo { get; set; }
    public DateTime? FechaEntrega { get; set; }
    public DateTime? FechaEntregaEstimada { get; set; }
    
    // Totales
    [Column(TypeName = "decimal(18,4)")] public decimal TotalManoObra { get; set; }
    [Column(TypeName = "decimal(18,4)")] public decimal TotalRepuestos { get; set; }
    [Column(TypeName = "decimal(18,4)")] public decimal Descuento { get; set; }
    [Column(TypeName = "decimal(18,4)")] public decimal Total { get; set; }
    
    // Garantía
    public int? GarantiaDias { get; set; }
    public int? GarantiaKilometros { get; set; }
}
```

## 🎯 Funcionalidades del Taller

1. **Registro de vehículos** con historial de servicios
2. **Panel visual de bahías** (similar a mesas)
3. **Flujo de estados** de la OT (Recepción → Diagnóstico → En Proceso → Listo → Entregado)
4. **Fotos del vehículo** al ingreso
5. **Control de kilometraje** y combustible
6. **Detalle de mano de obra** + repuestos
7. **Presupuesto previo** antes de trabajar
8. **Garantía** configurable por OT

## 🔄 Flujo de Orden de Trabajo

```
Recepción → Diagnóstico → Presupuesto → Aprobación → En Proceso → Control Calidad → Listo → Cobrar → Entregado
```

---

# 📊 Resumen de Tablas de Base de Datos por Módulo

## Suscripciones
| Tabla | Descripción |
|-------|-------------|
| `SuscripcionesClientes` | Suscripciones activas |
| `FacturasAutomaticas` | Historial de facturas generadas |

## Restaurante/Canchas/Taller
| Tabla | Descripción |
|-------|-------------|
| `Mesas` | Mesas, canchas o bahías |
| `Pedidos` | Pedidos/comandas de mesas |
| `PedidosDetalles` | Líneas de productos |
| `PedidosPagos` | Pagos parciales |
| `Reservas` | Reservas de mesas/canchas |
| `Vehiculos` | Vehículos de clientes |
| `OrdenesTrabajo` | Órdenes de trabajo (taller) |
| `OrdenesTrabajoDetalles` | Líneas de servicios/repuestos |

---

*Este archivo sirve como referencia para continuar el desarrollo de los módulos especiales de SistemIA.*
