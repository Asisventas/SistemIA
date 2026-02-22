# Módulo de Restaurante - Mesas, Reservas e Informes

## 📋 Descripción General

Sistema de gestión de mesas, reservas e informes de cierre para restaurantes y complejos (canchas, salones), integrado con el sistema de ventas (pedidos) de SistemIA.

## 📅 Historial de Implementación

### Sesión 5-6 de Febrero 2026 - Informe de Cierre de Complejos
- ✅ Nueva página **InformeCierreRestaurante.razor** con informe completo
- ✅ Cabecera con logo de empresa y datos de sucursal
- ✅ Integración con **ICajaService** para fecha/caja/turno predeterminados
- ✅ Gráficos interactivos con Chart.js (productos y tiempos)
- ✅ Vista previa fullscreen con mismo diseño que el informe principal
- ✅ Impresión Ticket (térmica) y A4 (PDF)
- ✅ Filtros por fecha, caja, turno y mesero
- ✅ Secciones: Resumen General, Ventas por Mesero, Top 10 Productos, Detalle por Mesas, Análisis de Tiempos, Formas de Pago

### Sesión 3 de Febrero 2026 - Implementación Inicial
- ✅ Modelos Mesa, Reserva, Pedido
- ✅ Páginas Mesas, MesasPanel, Reservas, PedidoMesa
- ✅ Sistema de correo de reservas

---

## 🗂️ Estructura de Archivos

### Páginas Principales
| Archivo | Ruta | Descripción |
|---------|------|-------------|
| `Mesas.razor` | `/mesas` | Gestión CRUD de mesas |
| `MesasPanel.razor` | `/mesas/panel` | Panel visual interactivo de mesas |
| `Reservas.razor` | `/reservas` | Gestión de reservas |
| `PedidoMesa.razor` | `/mesas/pedido/{IdMesa}` | Tomar pedido en mesa |
| `InformeCierreRestaurante.razor` | `/informes/cierre-restaurante` | **Informe de Cierre de Complejos**

### Modelos
| Archivo | Descripción |
|---------|-------------|
| `Models/Mesa.cs` | Entidad Mesa con estado, capacidad, ubicación, visualización |
| `Models/Reserva.cs` | Entidad Reserva con cliente, fecha, hora, estado |
| `Models/Pedido.cs` | Pedido asociado a mesa con mesero, tiempos, totales |
| `Models/PedidoDetalle.cs` | Detalle de productos en pedido |

### Servicios
| Archivo | Descripción |
|---------|-------------|
| `Services/ReservaCorreoService.cs` | Envío de correos de confirmación de reservas |
| `Services/ICajaService.cs` | Obtener caja actual con fecha/turno |

---

## 📊 Informe de Cierre de Complejos (NUEVO Feb 2026)

### Características Implementadas

**Archivo:** `Pages/InformeCierreRestaurante.razor` (1369 líneas)

#### Cabecera
- Logo de empresa desde `Sucursal.Logo`
- Nombre de empresa y sucursal
- Badges con fecha, caja y turno activos

#### Filtros
- **Fecha:** Predeterminada desde `CajaService.ObtenerCajaActualAsync()`
- **Caja:** Filtrar por caja específica
- **Turno:** Filtrar por turno (1-4)
- **Mesero:** Filtrar por mesero que atendió

#### Secciones del Informe
1. **Resumen General** - 4 cards (Mesas, Ventas, Tiempo Promedio, Ticket Promedio)
2. **Desglose de Ventas** - Contado, Crédito, Cargo Servicio
3. **Ventas por Mesero** - Tabla con mesas, pedidos, total, ticket y tiempo
4. **Productos Más Vendidos** - Top 10 + gráfico Chart.js
5. **Detalle por Mesas** - Cada mesa con comensales, mesero, tiempo, total
6. **Análisis de Tiempos** - Gráfico + indicadores (rápido/normal/demorado)
7. **Formas de Pago** - Desglose por medio de pago
8. **Resumen Final** - Card oscura con total neto

#### Gráficos Chart.js
```javascript
// Gráfico de productos más vendidos
crearGraficoRestaurante('chartProductos', 'bar', labels, data, 'Productos Vendidos');

// Gráfico de tiempos de atención
crearGraficoRestaurante('chartTiempos', 'line', labels, data, 'Tiempo (min)');
```

#### Vista Previa Fullscreen
```razor
@if (mostrarVistaPrevia)
{
    <div class="modal fade show d-block" style="background:rgba(0,0,0,0.85);">
        <div class="modal-dialog modal-fullscreen">
            <div class="modal-content" style="background:var(--bg-page);">
                <!-- Mismo contenido que informe principal -->
            </div>
        </div>
    </div>
}
```
- Modal fullscreen con tema del sistema (`var(--bg-page)`)
- Botones de impresión en header
- Replica exactamente el diseño del informe principal

#### Integración con Caja
```csharp
// En OnInitializedAsync
var cajaActual = await CajaService.ObtenerCajaActualAsync();
if (cajaActual != null)
{
    fFecha = cajaActual.FechaActualCaja ?? DateTime.Today;
    fIdCaja = cajaActual.IdCaja;
    fTurno = cajaActual.TurnoActual ?? 1;
}
```

#### Clases de Datos
```csharp
private class ResumenRestaurante
{
    public int MesasAtendidas { get; set; }
    public int TotalComensales { get; set; }
    public decimal TotalVentas { get; set; }
    public int CantVentas { get; set; }
    public decimal VentasContado { get; set; }
    public decimal VentasCredito { get; set; }
    public decimal TotalCargoServicio { get; set; }
    public decimal TotalNC { get; set; }
    public decimal TotalNeto { get; set; }
    public decimal TicketPromedio { get; set; }
    public int TiempoPromedioMinutos { get; set; }
    public string TiempoPromedioTexto => TiempoPromedioMinutos > 0 ? $"{TiempoPromedioMinutos} min" : "--";
}

private class VentaMesero { ... }
private class ProductoVendido { ... }
private class PagoMedio { ... }
private class DetalleMesa { ... }
private class TiempoMesa { ... }
```

---

## 📊 Modelo de Datos

### Mesa (Actualizado)
```csharp
public class Mesa
{
    public int IdMesa { get; set; }
    public int IdSucursal { get; set; }
    
    // Identificación
    public string Numero { get; set; } = string.Empty;  // "1", "VIP-1", "Cancha A"
    public string? Nombre { get; set; }                  // "Mesa VIP", "Cancha de Fútbol 5"
    public string? Descripcion { get; set; }
    
    // Tipo y categoría
    public string Tipo { get; set; } = "Mesa";          // Mesa, Cancha, Sala, Terraza
    public string? Zona { get; set; }                    // Interior, Terraza, VIP, Planta Alta
    public int Capacidad { get; set; } = 4;
    
    // Visualización en panel
    public int PosicionX { get; set; } = 0;
    public int PosicionY { get; set; } = 0;
    public int Ancho { get; set; } = 100;
    public int Alto { get; set; } = 100;
    public string Forma { get; set; } = "Cuadrado";     // Cuadrado, Circulo, Rectangulo
    public string ColorLibre { get; set; } = "#28a745";  // Verde
    public string ColorOcupada { get; set; } = "#dc3545"; // Rojo
    public string ColorReservada { get; set; } = "#17a2b8"; // Azul
    
    // Estado
    public string Estado { get; set; } = "Disponible";   // Disponible, Ocupada, Reservada, EnLimpieza
    public bool Activa { get; set; } = true;
    
    // Propiedad calculada
    public string TextoMostrar => !string.IsNullOrEmpty(Nombre) ? Nombre : $"Mesa {Numero}";
}
```

### Pedido (Modelo Completo)
```csharp
public class Pedido
{
    public int IdPedido { get; set; }
    
    // Sucursal, Caja y Turno (igual que Venta)
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
    public string? NombreCliente { get; set; }
    public int? IdCliente { get; set; }
    
    // Mesero que atiende
    public string? NombreMesero { get; set; }
    
    // Tiempos
    public DateTime FechaApertura { get; set; } = DateTime.Now;
    public DateTime? FechaCierre { get; set; }
    public DateTime? HoraInicio { get; set; }  // Para canchas con reserva
    public DateTime? HoraFin { get; set; }
    
    // Totales
    public decimal Subtotal { get; set; }
    public decimal TotalDescuento { get; set; }
    public decimal CargoServicio { get; set; }  // Propina sugerida
    public decimal Total { get; set; }
    
    // Estado: Abierto, EnPreparacion, Servido, Pagado, Cerrado, Cancelado
    public string Estado { get; set; } = "Abierto";
    
    // Navegación
    public List<PedidoDetalle> Detalles { get; set; } = new();
}
```

### Reserva
```csharp
public class Reserva
{
    public int IdReserva { get; set; }
    public int IdMesa { get; set; }
    public int IdSucursal { get; set; }
    public int? IdCliente { get; set; } // FK opcional a Clientes
    public string NumeroReserva { get; set; }
    
    // Datos del cliente (pueden venir de Cliente o ser manuales)
    public string NombreCliente { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    
    // Fecha y hora
    public DateTime FechaReserva { get; set; }
    public TimeSpan HoraInicio { get; set; }
    public TimeSpan? HoraFin { get; set; }
    public int? DuracionMinutos { get; set; }
    
    // Hora real de uso (para tracking)
    public TimeSpan? HoraInicioReal { get; set; }  // Cuando el cliente llega
    public TimeSpan? HoraFinReal { get; set; }     // Cuando se cierra la mesa
    
    public int CantidadPersonas { get; set; }
    public string Estado { get; set; } = "Pendiente";
    public string? Observaciones { get; set; }
    
    // Auditoría
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    
    // Navegación
    public Mesa? Mesa { get; set; }
    public Sucursal? Sucursal { get; set; }
    public Cliente? Cliente { get; set; }
}
```

### Estados de Reserva
| Estado | Descripción | Badge CSS |
|--------|-------------|-----------|
| `Pendiente` | Reserva creada, sin confirmar | `bg-warning text-dark` |
| `Confirmada` | Reserva confirmada, esperando cliente | `bg-info` |
| `EnCurso` | Cliente llegó, mesa en uso | `bg-primary` |
| `Completada` | Reserva finalizada | `bg-success` |
| `Cancelada` | Reserva cancelada | `bg-danger` |
| `NoShow` | Cliente no se presentó | `bg-secondary` |

### Estados de Mesa
| Estado | Descripción | Color Panel |
|--------|-------------|-------------|
| `Disponible` | Mesa libre | Verde |
| `Ocupada` | Mesa con clientes | Rojo |
| `Reservada` | Mesa con reserva próxima | Azul |
| `EnLimpieza` | Mesa siendo limpiada | Amarillo |

---

## 🎨 MesasPanel - Panel Visual de Mesas

### Características Implementadas

1. **Vista de Mesas en Grid**
   - Cards visuales por cada mesa
   - Color según estado (Disponible=verde, Ocupada=rojo, etc.)
   - Muestra número de mesa, capacidad, pedido activo

2. **Acciones por Mesa**
   - **Abrir Mesa**: Inicia un nuevo pedido
   - **Ver Pedido**: Muestra detalle del pedido actual
   - **Agregar Items**: Añade productos al pedido
   - **Cerrar Mesa**: Genera venta y libera mesa
   - **Agenda**: Ver reservas de la mesa

3. **Panel de Agenda (Reservas por Mesa)**
   - Lista de reservas del día para la mesa seleccionada
   - Botón **"Iniciar"** para reservas confirmadas
   - **Contador de tiempo** para reservas en curso
   - Estados visuales con badges de colores

4. **Timer de Actualización**
   - Actualiza cada 60 segundos el contador de tiempo de reservas en curso

### Código del Panel de Agenda
```razor
<!-- Panel de Agenda -->
@if (_mesaAgendaSeleccionada != null)
{
    <div class="card shadow-sm">
        <div class="card-header bg-info text-white">
            <h6 class="mb-0">
                <i class="bi bi-calendar-check me-2"></i>
                Agenda: @_mesaAgendaSeleccionada.TextoMostrar
            </h6>
        </div>
        <div class="card-body p-0">
            @foreach (var reserva in _reservasMesaSeleccionada)
            {
                <div class="agenda-item @(reserva.Estado == "EnCurso" ? "en-curso" : "")">
                    <div class="d-flex justify-content-between align-items-start">
                        <div>
                            <strong>@reserva.HoraInicio.ToString(@"hh\:mm")</strong>
                            <span class="badge @GetBadgeClassReserva(reserva.Estado) ms-2">
                                @reserva.Estado
                            </span>
                        </div>
                        @if (reserva.Estado == "Confirmada")
                        {
                            <button class="btn btn-sm btn-success" @onclick="() => IniciarReserva(reserva)">
                                <i class="bi bi-play-fill me-1"></i>Iniciar
                            </button>
                        }
                    </div>
                    <div class="small text-muted">
                        @reserva.NombreCliente - @reserva.CantidadPersonas pers.
                    </div>
                    @if (reserva.Estado == "EnCurso" && reserva.HoraInicioReal.HasValue)
                    {
                        var minutos = (int)(DateTime.Now.TimeOfDay - reserva.HoraInicioReal.Value).TotalMinutes;
                        <div class="tiempo-reserva">
                            <i class="bi bi-clock me-1"></i>@minutos min
                        </div>
                    }
                </div>
            }
        </div>
    </div>
}
```

### Métodos de Reserva en MesasPanel
```csharp
private async Task IniciarReserva(Reserva reserva)
{
    await using var db = await DbFactory.CreateDbContextAsync();
    var reservaDb = await db.Reservas.FindAsync(reserva.IdReserva);
    if (reservaDb != null)
    {
        reservaDb.Estado = "EnCurso";
        reservaDb.HoraInicioReal = DateTime.Now.TimeOfDay;
        await db.SaveChangesAsync();
        
        // Actualizar estado de la mesa a Ocupada
        var mesa = await db.Mesas.FindAsync(reserva.IdMesa);
        if (mesa != null)
        {
            mesa.Estado = "Ocupada";
            await db.SaveChangesAsync();
        }
        
        await CargarReservasMesa(_mesaAgendaSeleccionada!.IdMesa);
        StateHasChanged();
    }
}

private async Task CompletarReserva(Reserva reserva)
{
    await using var db = await DbFactory.CreateDbContextAsync();
    var reservaDb = await db.Reservas.FindAsync(reserva.IdReserva);
    if (reservaDb != null)
    {
        reservaDb.Estado = "Completada";
        reservaDb.HoraFinReal = DateTime.Now.TimeOfDay;
        await db.SaveChangesAsync();
    }
}
```

### Auto-completar Reserva al Cerrar Mesa
```csharp
private async Task CerrarMesaConfirmado()
{
    // ... código de cierre de mesa y generación de venta ...
    
    // Completar reservas en curso para esta mesa
    var reservasEnCurso = await db.Reservas
        .Where(r => r.IdMesa == _mesaCerrandoId && r.Estado == "EnCurso")
        .ToListAsync();
    
    foreach (var reserva in reservasEnCurso)
    {
        await CompletarReserva(reserva);
    }
}
```

---

## 📧 Sistema de Correo de Reservas

### ReservaCorreoService

**Archivo:** `Services/ReservaCorreoService.cs`

**Interface:**
```csharp
public interface IReservaCorreoService
{
    Task<(bool Exito, string Mensaje)> EnviarConfirmacionAsync(int idReserva);
}
```

**Características:**
- Obtiene configuración de correo específica por sucursal (`IdSucursal`)
- Genera HTML profesional con logo de la empresa (base64)
- Incluye datos de la reserva: fecha, hora, mesa, personas
- Agradecimiento personalizado

**Flujo de Envío:**
1. Usuario crea reserva NUEVA con email
2. Se guarda la reserva en BD
3. Se llama a `ReservaCorreoService.EnviarConfirmacionAsync(idReserva)`
4. Se obtiene `ConfiguracionCorreo` por `IdSucursal`
5. Se genera HTML del correo con logo de `Sucursal.Logo`
6. Se envía vía `ICorreoService.EnviarCorreoAsync()`

**Código de Envío en Reservas.razor:**
```csharp
// Enviar correo de confirmación SOLO si es reserva nueva y tiene email
if (esNueva && !string.IsNullOrWhiteSpace(emailCliente))
{
    var resultado = await ReservaCorreoService.EnviarConfirmacionAsync(_reservaEditando.IdReserva);
    if (resultado.Exito)
    {
        await JS.InvokeVoidAsync("alert", $"✅ Reserva creada y correo enviado a {emailCliente}");
    }
    else
    {
        await JS.InvokeVoidAsync("alert", $"⚠️ Reserva creada, pero no se pudo enviar correo: {resultado.Mensaje}");
    }
}
```

---

## 🔄 Migraciones Creadas

### Agregar_HoraRealReservas (3-Feb-2026)
```csharp
// Campos agregados a Reserva
public TimeSpan? HoraInicioReal { get; set; }
public TimeSpan? HoraFinReal { get; set; }
```

### Agregar_Sistema_Mesas_y_Modulos_Completo (2-Feb-2026)
- Tabla `Mesas`
- Tabla `Reservas`
- Permisos del módulo

---

## 🎯 Pendientes / Mejoras Futuras

### 🔴 Alta Prioridad - Búsqueda de Cliente en Reservas

**Objetivo:** Integrar la búsqueda y creación de clientes en el formulario de reservas, igual que en Ventas.razor

#### Funcionalidad Requerida:

1. **Campo de búsqueda con autocompletado**
   - Input para buscar por nombre, RUC o teléfono
   - Dropdown con sugerencias de clientes existentes
   - Similar a `BuscarCliente` en Ventas.razor.cs (líneas 373-377, 1122-1145)

2. **Búsqueda en cascada (como Ventas)**
   - Primero buscar en BD local (`Clientes`)
   - Si no encuentra, buscar en catálogo `RucDnit` (1.5M registros DNIT)
   - Si no encuentra, consultar webservice SIFEN
   - Referencia: `BuscarRucAutoClienteAsync()` en Ventas.razor.cs (líneas 1175-1260)

3. **Auto-registro de cliente**
   - Si el RUC existe en RucDnit o SIFEN pero no en Clientes, crear automáticamente
   - Usar `RegistrarClienteDesdeRucAsync()` de Ventas.razor.cs (líneas 1285-1355)
   - Asignar campos SIFEN correctos (NaturalezaReceptor, TipoContribuyente, etc.)

4. **Modal de cliente rápido**
   - Si el cliente no existe en ningún lado, mostrar modal para crear
   - Copiar estructura de modal de Ventas.razor (líneas 780-860)
   - Incluir botón de búsqueda SIFEN: `BuscarClienteEnSifenAsync()` (líneas 4343-4470)

5. **Precargar datos del cliente seleccionado**
   - Al seleccionar cliente, llenar automáticamente:
     - `NombreCliente` ← `Cliente.RazonSocial`
     - `Telefono` ← `Cliente.Telefono` o `Cliente.Celular`
     - `Email` ← `Cliente.Email`
   - Guardar `IdCliente` en la reserva para historial

#### Campos a Agregar en Reservas.razor:

```csharp
// Variables para búsqueda de cliente
private List<Cliente> _clientes = new();
private List<Cliente> _clientesFiltrados = new();
private bool _mostrarSugerenciasCliente = false;
private string _buscarClienteTexto = string.Empty;
private Cliente? _clienteSeleccionado = null;
private bool _mouseDownEnSugerencia = false;

// Modal nuevo cliente
private bool _mostrarModalNuevoCliente = false;
private string _nuevoCliRuc = string.Empty;
private string _nuevoCliRazonSocial = string.Empty;
private string _nuevoCliTelefono = string.Empty;
private string _nuevoCliEmail = string.Empty;
private int _nuevoCliDv = 0;
private bool _buscandoClienteSifen = false;
private string? _mensajeSifenCliente = null;
private bool _esSifenClienteError = false;
```

#### UI del Campo de Búsqueda (reemplazar input actual):

```razor
<div class="mb-3">
    <label class="form-label">Cliente *</label>
    <div class="position-relative">
        @if (_clienteSeleccionado != null)
        {
            <!-- Cliente seleccionado -->
            <div class="input-group">
                <input type="text" class="form-control" 
                       value="@_clienteSeleccionado.RazonSocial" readonly />
                <button class="btn btn-outline-secondary" type="button" 
                        @onclick="CambiarCliente" title="Cambiar cliente">
                    <i class="bi bi-x-lg"></i>
                </button>
            </div>
        }
        else
        {
            <!-- Buscador de cliente -->
            <div class="input-group">
                <input type="text" class="form-control" 
                       placeholder="Buscar por nombre, RUC o teléfono..."
                       @bind="_buscarClienteTexto" 
                       @bind:event="oninput"
                       @onkeydown="OnClienteKeyDown"
                       @onfocus="OnClienteFocus"
                       @onblur="OnClienteBlur" />
                <button class="btn btn-outline-primary" type="button"
                        @onclick="AbrirModalNuevoCliente" title="Nuevo cliente">
                    <i class="bi bi-person-plus"></i>
                </button>
            </div>
            
            <!-- Dropdown de sugerencias -->
            @if (_mostrarSugerenciasCliente && _clientesFiltrados.Any())
            {
                <div class="dropdown-menu show w-100 shadow" style="max-height: 250px; overflow-y: auto;">
                    @foreach (var cli in _clientesFiltrados.Take(15))
                    {
                        <button type="button" class="dropdown-item"
                                @onmousedown="() => SeleccionarCliente(cli)">
                            <div>@cli.RazonSocial</div>
                            <small class="text-muted">
                                @(cli.RUC ?? cli.NumeroDocumento) 
                                @if (!string.IsNullOrEmpty(cli.Telefono)) { <span>| @cli.Telefono</span> }
                            </small>
                        </button>
                    }
                </div>
            }
        }
    </div>
</div>
```

#### Métodos a Implementar:

```csharp
private void AplicarFiltroClientes()
{
    var texto = (_buscarClienteTexto ?? "").Trim().ToLowerInvariant();
    if (string.IsNullOrWhiteSpace(texto))
    {
        _clientesFiltrados = new();
        _mostrarSugerenciasCliente = false;
        return;
    }
    
    var digitos = new string(texto.Where(char.IsDigit).ToArray());
    _clientesFiltrados = _clientes
        .Where(c => (c.RazonSocial ?? "").ToLowerInvariant().Contains(texto)
                    || (!string.IsNullOrEmpty(digitos) && (c.RUC ?? "").Contains(digitos))
                    || (!string.IsNullOrEmpty(digitos) && (c.Telefono ?? "").Contains(digitos))
                    || (!string.IsNullOrEmpty(digitos) && (c.Celular ?? "").Contains(digitos)))
        .OrderBy(c => c.RazonSocial)
        .Take(30)
        .ToList();
    
    _mostrarSugerenciasCliente = _clientesFiltrados.Any();
}

private void SeleccionarCliente(Cliente cliente)
{
    _clienteSeleccionado = cliente;
    _reservaEditando.IdCliente = cliente.IdCliente;
    _reservaEditando.NombreCliente = cliente.RazonSocial ?? "";
    _reservaEditando.Telefono = cliente.Telefono ?? cliente.Celular;
    _reservaEditando.Email = cliente.Email;
    _buscarClienteTexto = "";
    _mostrarSugerenciasCliente = false;
    _mouseDownEnSugerencia = false;
}

private void CambiarCliente()
{
    _clienteSeleccionado = null;
    _reservaEditando.IdCliente = null;
    // Mantener los datos ingresados manualmente
}

// Copiar de Ventas.razor.cs:
// - BuscarRucAutoClienteAsync()
// - RegistrarClienteDesdeRucAsync()
// - BuscarClienteEnSifenAsync()
// - GuardarNuevoClienteAsync()
```

#### Agregar IdCliente a Reserva (si no existe):

```csharp
// En Models/Reserva.cs
public int? IdCliente { get; set; }

// Navegación
[ForeignKey("IdCliente")]
public Cliente? Cliente { get; set; }
```

#### Referencias de Código en Ventas.razor.cs:
| Método | Líneas | Descripción |
|--------|--------|-------------|
| `AplicarFiltroClientes()` | 1122-1145 | Filtrar lista de clientes |
| `OnClienteKeyDown()` | 1145-1190 | Manejo de Enter/Tab/Escape |
| `BuscarRucAutoClienteAsync()` | 1175-1260 | Búsqueda automática RUC |
| `RegistrarClienteDesdeRucAsync()` | 1285-1355 | Crear cliente desde RucDnit/SIFEN |
| `OnClienteSuggestionMouseDownAsync()` | 1395-1410 | Seleccionar sugerencia |
| `BuscarClienteEnSifenAsync()` | 4343-4470 | Consultar webservice SIFEN |
| `GuardarNuevoClienteAsync()` | 4475-4550 | Guardar cliente nuevo |

### Media Prioridad
- [ ] Calendario visual de reservas (vista semanal/mensual)
- [ ] Notificaciones push cuando llega hora de reserva
- [ ] Exportar agenda del día a PDF
- [ ] Integración con WhatsApp para confirmaciones
- [ ] Historial de reservas por cliente (usando IdCliente)

### Baja Prioridad
- [ ] Configuración de horarios de atención por sucursal
- [ ] Bloqueo automático de mesas por mantenimiento
- [ ] Historial de uso de mesas (analytics)
- [ ] Estadísticas de no-show por cliente

---

## 📝 Notas Técnicas

### Configuración de Correo
- La configuración se obtiene de `ConfiguracionesCorreo` filtrado por `IdSucursal`
- Requiere campo `ConfiguracionCompleta = true` para enviar
- Soporta Gmail con App Password

### Estados de Mesa vs Reserva
- Mesa `Ocupada` puede tener reserva `EnCurso` o pedido sin reserva
- Mesa `Reservada` tiene reserva `Confirmada` para las próximas horas
- Al cerrar mesa se completa automáticamente la reserva si existe

### Timer de Actualización
```csharp
private Timer? _timerActualizacion;

protected override void OnInitialized()
{
    // Timer cada 60 segundos para actualizar contador de tiempo
    _timerActualizacion = new Timer(async _ =>
    {
        await InvokeAsync(StateHasChanged);
    }, null, TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(60));
}

public void Dispose()
{
    _timerActualizacion?.Dispose();
}
```

---

## 🔗 Referencias

- **Ventas.razor.cs**: Modelo de búsqueda de cliente con SIFEN (~líneas 1145-1350, 4343-4470)
- **ICorreoService**: Servicio base de envío de correos
- **ConfiguracionCorreo**: Modelo de configuración SMTP por sucursal
- **ICajaService**: Servicio para obtener caja actual y datos de turno
- **wwwroot/js/graficos.js**: Funciones Chart.js para gráficos de restaurante

---

## 📊 Rutas del Módulo

| Ruta | Página | Descripción |
|------|--------|-------------|
| `/mesas` | Mesas.razor | CRUD de mesas |
| `/mesas/panel` | MesasPanel.razor | Panel visual interactivo |
| `/mesas/pedido/{IdMesa}` | PedidoMesa.razor | Tomar pedido en mesa |
| `/mesas/pedido/{IdMesa}/{IdPedido}` | PedidoMesa.razor | Editar pedido existente |
| `/mesas/pedido/editar/{IdPedido}` | PedidoMesa.razor | Editar pedido por ID |
| `/mesas/pedido/cobrar/{IdPedido}` | PedidoMesa.razor | Cobrar pedido |
| `/reservas` | Reservas.razor | Gestión de reservas |
| `/informes/cierre-restaurante` | InformeCierreRestaurante.razor | Informe de cierre |

---

*Última actualización: 6 de Febrero de 2026*
