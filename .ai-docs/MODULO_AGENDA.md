# Módulo de Agenda - Documentación Técnica

## 📋 Descripción General

El módulo de Agenda permite gestionar citas, eventos y recordatorios con un calendario visual completo. Incluye sistema de alarmas/notificaciones y mini-calendario en el Panel de Control.

## 📁 Estructura de Archivos

```
Pages/
├── Agenda.razor              # Página principal del calendario/agenda
│
Models/
├── CitaAgenda.cs             # Modelo principal de citas
│
Shared/
├── AlarmaRecordatorio.razor  # Componente de alarma global (MainLayout)
├── MiniCalendario.razor      # Widget de calendario para Panel de Control
│
Services/
├── IAgendaService.cs         # Interface del servicio
├── AgendaService.cs          # Implementación del servicio
```

## 🗃️ Modelo de Datos (CitaAgenda.cs)

```csharp
public class CitaAgenda
{
    public int IdCita { get; set; }                    // PK
    public int IdSucursal { get; set; }                // FK a Sucursal
    
    // ========== INFORMACIÓN BÁSICA ==========
    public string Titulo { get; set; }                 // Título de la cita
    public string? Descripcion { get; set; }           // Descripción detallada
    public string TipoCita { get; set; }               // Consulta, Reunión, Recordatorio, etc.
    
    // ========== FECHAS Y HORARIOS ==========
    public DateTime FechaHoraInicio { get; set; }      // Inicio de la cita
    public DateTime FechaHoraFin { get; set; }         // Fin de la cita
    public bool TodoElDia { get; set; }                // Si abarca todo el día
    
    // ========== CLIENTE ==========
    public int? IdCliente { get; set; }                // FK opcional a Cliente
    public string? NombreCliente { get; set; }         // Nombre (si no tiene IdCliente)
    public string? TelefonoCliente { get; set; }
    public string? EmailCliente { get; set; }
    
    // ========== UBICACIÓN ==========
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    
    // ========== ASIGNACIÓN ==========
    public int? IdUsuarioAsignado { get; set; }        // FK a Usuario
    public string? NombreAsignado { get; set; }        // Nombre del asignado
    
    // ========== ESTADO Y PRIORIDAD ==========
    public string Estado { get; set; }                 // Programada, Completada, Cancelada, etc.
    public string Prioridad { get; set; }              // Alta, Media, Baja
    
    // ========== APARIENCIA ==========
    public string ColorFondo { get; set; }             // Color de fondo en calendario
    public string ColorTexto { get; set; }             // Color del texto
    
    // ========== RECORDATORIOS ==========
    public bool TieneRecordatorio { get; set; }        // Si tiene recordatorio activo
    public int? MinutosRecordatorio1 { get; set; }     // Minutos antes (ej: 30)
    public int? MinutosRecordatorio2 { get; set; }     // Segundo recordatorio opcional
    public bool NotificacionMostrada { get; set; }     // Si ya se mostró la notificación
    public bool MostrarNotificacion { get; set; }      // Control manual de notificación
    
    // ========== AUDITORÍA ==========
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public int? IdUsuarioCreador { get; set; }
    
    // ========== RECURRENCIA (Futuro) ==========
    public bool EsRecurrente { get; set; }
    public string? PatronRecurrencia { get; set; }     // Diario, Semanal, Mensual
    public DateTime? FechaFinRecurrencia { get; set; }
    
    // ========== NAVEGACIÓN ==========
    public virtual Sucursal? Sucursal { get; set; }
    public virtual Cliente? Cliente { get; set; }
    public virtual Usuario? UsuarioAsignado { get; set; }
}
```

## 🖼️ Página Principal (Agenda.razor)

### Vistas Disponibles
| Vista | Valor `_vistaActual` | Descripción |
|-------|---------------------|-------------|
| Mensual | `mes` | Calendario tradicional con días del mes |
| Semanal | `semana` | Vista de 7 días con franjas horarias |
| Diaria | `dia` | Vista detallada de un solo día |
| Lista | `agenda` | Listado de citas con filtros |

### Navegación por URL
```
/agenda                    → Vista mensual (default)
/agenda?vista=mes          → Vista mensual
/agenda?vista=semana       → Vista semanal
/agenda?vista=dia          → Vista diaria
/agenda?vista=lista        → Vista lista/agenda
/agenda?vista=dia&fecha=2026-02-18  → Vista día con fecha específica
```

### Manejo de Navegación
El componente implementa `IDisposable` y escucha `Navigation.LocationChanged` para detectar cambios en la URL sin recargar la página:

```csharp
@implements IDisposable

protected override async Task OnInitializedAsync()
{
    Navigation.LocationChanged += HandleLocationChanged;
    LeerVistaDeUrl();
    // ...
}

private void LeerVistaDeUrl()
{
    var uri = new Uri(Navigation.Uri);
    if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("vista", out var vistaParam))
    {
        var vista = vistaParam.ToString().ToLower();
        _vistaActual = vista == "lista" ? "agenda" : vista;
    }
    
    if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("fecha", out var fechaParam))
    {
        if (DateTime.TryParse(fechaParam.ToString(), out var fecha))
        {
            _fechaActual = fecha;
            _diaSeleccionado = fecha;
        }
    }
}

private void HandleLocationChanged(object? sender, LocationChangedEventArgs e)
{
    if (e.Location.Contains("/agenda"))
    {
        LeerVistaDeUrl();
        GenerarCalendario();
        InvokeAsync(StateHasChanged);
    }
}

public void Dispose()
{
    Navigation.LocationChanged -= HandleLocationChanged;
}
```

### Filtros Disponibles
- **FiltroCliente**: Filtrar por cliente
- **FiltroTipo**: Filtrar por tipo de cita
- **FiltroEstado**: Filtrar por estado
- **FiltroFechaDesde / FiltroFechaHasta**: Rango de fechas (vista lista)

## 🔔 Sistema de Alarmas (AlarmaRecordatorio.razor)

### Ubicación
Integrado en `MainLayout.razor`, se muestra sobre todo el sistema.

### Funcionalidades
1. **Timer automático**: Cada 30 segundos verifica citas pendientes
2. **Modal full-screen**: z-index 99999, overlay oscuro
3. **Sonido**: Web Audio API (880Hz sine wave, beep cada 800ms)
4. **Botones**: "Posponer 5 min" y "Entendido"
5. **Persistencia**: Marca `NotificacionMostrada = true` en BD

### Consulta de Recordatorios Pendientes
```csharp
private async Task<CitaAgenda?> ObtenerRecordatorioPendiente()
{
    var ahora = DateTime.Now;
    var margen = TimeSpan.FromMinutes(60);

    return await ctx.CitasAgenda
        .Where(c => c.TieneRecordatorio 
            && !c.NotificacionMostrada
            && c.Estado != "Cancelada" && c.Estado != "Completada")
        .Where(c => c.FechaHoraInicio >= ahora && c.FechaHoraInicio <= ahora.Add(margen))
        .Where(c => 
            (c.MinutosRecordatorio1.HasValue 
                && c.FechaHoraInicio.AddMinutes(-c.MinutosRecordatorio1.Value) <= ahora)
            || (c.MinutosRecordatorio2.HasValue 
                && c.FechaHoraInicio.AddMinutes(-c.MinutosRecordatorio2.Value) <= ahora))
        .OrderBy(c => c.FechaHoraInicio)
        .FirstOrDefaultAsync();
}
```

### Generación de Sonido (Web Audio API)
```javascript
window.alarmaSound = {
    audioContext: null,
    oscillator: null,
    gainNode: null,
    intervalId: null,
    
    play: function() {
        this.audioContext = new (window.AudioContext || window.webkitAudioContext)();
        this.gainNode = this.audioContext.createGain();
        this.gainNode.connect(this.audioContext.destination);
        this.gainNode.gain.value = 0.3;
        
        const playBeep = () => {
            const osc = this.audioContext.createOscillator();
            osc.type = 'sine';
            osc.frequency.value = 880;
            osc.connect(this.gainNode);
            osc.start();
            osc.stop(this.audioContext.currentTime + 0.15);
        };
        
        playBeep();
        this.intervalId = setInterval(playBeep, 800);
    },
    
    stop: function() {
        if (this.intervalId) clearInterval(this.intervalId);
        if (this.audioContext) this.audioContext.close();
    }
};
```

## 📅 Mini-Calendario (MiniCalendario.razor)

### Ubicación
Panel de Control (Index.razor), junto a TablaTiposCambio.

### Funcionalidades
1. **Calendario mensual navegable**: Flechas para mes anterior/siguiente
2. **Indicador de citas**: Punto rojo en días con citas
3. **Día actual resaltado**: Fondo azul
4. **Citas de hoy**: Muestra hasta 3 citas del día
5. **Navegación**: Click en día → `/agenda?vista=dia&fecha=YYYY-MM-DD`

### Integración en Index.razor
```razor
<!-- Tipos de cambio y Mini Calendario -->
<div class="row mt-4">
    <div class="col-12 col-lg-5">
        <TablaTiposCambio />
    </div>
    <div class="col-12 col-lg-7 d-flex justify-content-end">
        <MiniCalendario />
    </div>
</div>
```

## 🧭 Menú de Navegación (NavMenu.razor)

### Estructura del Submenú Agenda
```razor
<!-- Agenda -->
<div class="nav-item mb-1">
    <button class="nav-link submenu-button ...">
        <i class="bi bi-calendar-week me-2"></i>
        <span>Agenda</span>
        <i class="bi bi-chevron-..."></i>
    </button>
    
    <div class="submenu-container @(submenuAgendaOpen ? "show" : "collapse")">
        <div class="submenu-items">
            <NavLink href="/agenda">
                <i class="bi bi-calendar3 me-2"></i>Calendario
            </NavLink>
            <NavLink href="/agenda?vista=dia">
                <i class="bi bi-calendar-day me-2"></i>Vista Día
            </NavLink>
            <NavLink href="/agenda?vista=lista">
                <i class="bi bi-list-ul me-2"></i>Lista de Citas
            </NavLink>
        </div>
    </div>
</div>
```

## 🎨 Estilos CSS Principales

### Colores de Citas Disponibles
```csharp
private List<(string Color, string Nombre)> _coloresDisponibles = new()
{
    ("#3788d8", "Azul"),
    ("#28a745", "Verde"),
    ("#dc3545", "Rojo"),
    ("#ffc107", "Amarillo"),
    ("#6f42c1", "Morado"),
    ("#fd7e14", "Naranja"),
    ("#20c997", "Turquesa"),
    ("#e83e8c", "Rosa"),
    ("#6c757d", "Gris")
};
```

### Estados de Cita
| Estado | Descripción |
|--------|-------------|
| Programada | Cita pendiente |
| Confirmada | Cliente confirmó asistencia |
| En Progreso | Cita en curso |
| Completada | Cita finalizada |
| Cancelada | Cita cancelada |
| No Asistió | Cliente no se presentó |

### Tipos de Cita
| Tipo | Descripción |
|------|-------------|
| Consulta | Consulta general |
| Reunión | Reunión de trabajo |
| Recordatorio | Nota personal |
| Seguimiento | Seguimiento de cliente |
| Llamada | Llamada telefónica |
| Visita | Visita a domicilio |
| Otro | Otros tipos |

### Prioridades
| Prioridad | Color |
|-----------|-------|
| Alta | Rojo |
| Media | Amarillo |
| Baja | Gris |

## 🔐 Permisos del Módulo

### Módulos Registrados en BD
| Nombre | RutaPagina | IdCategoria |
|--------|------------|-------------|
| Agenda | /agenda | 11 (Agenda) |
| CitasAgenda | /citas-agenda | 11 (Agenda) |

### Permisos por Rol
Los permisos VIEW, CREATE, EDIT, DELETE se asignan según el rol del usuario.

## 🗄️ Consultas Comunes

### Obtener Citas por Rango
```csharp
public async Task<List<CitaAgenda>> ObtenerCitasPorRangoAsync(
    int idSucursal, DateTime desde, DateTime hasta)
{
    return await _db.CitasAgenda
        .Include(c => c.Cliente)
        .Include(c => c.UsuarioAsignado)
        .Where(c => c.IdSucursal == idSucursal)
        .Where(c => c.FechaHoraInicio >= desde && c.FechaHoraInicio <= hasta)
        .OrderBy(c => c.FechaHoraInicio)
        .ToListAsync();
}
```

### Citas de Hoy
```csharp
var citasHoy = await ctx.CitasAgenda
    .Where(c => c.FechaHoraInicio.Date == DateTime.Today)
    .Where(c => c.Estado != "Cancelada")
    .OrderBy(c => c.FechaHoraInicio)
    .ToListAsync();
```

## ⚠️ Consideraciones Importantes

1. **Zona Horaria**: Las fechas se manejan en hora local del servidor
2. **Recordatorios**: Solo se disparan si `NotificacionMostrada = false`
3. **Vista Lista**: El filtro `?vista=lista` se mapea internamente a `agenda`
4. **IDisposable**: Agenda.razor implementa IDisposable para limpiar el evento LocationChanged
5. **Web Audio API**: Requiere interacción del usuario antes de reproducir sonido (política de navegadores)

## 📝 Historial de Cambios

### Febrero 2026 - Implementación Inicial
- **Agenda.razor**: Página principal con 4 vistas (mes, semana, día, lista)
- **AlarmaRecordatorio.razor**: Sistema de notificaciones con sonido
- **MiniCalendario.razor**: Widget para Panel de Control
- **Navegación por URL**: Soporte para `?vista=` y `?fecha=`
- **LocationChanged handler**: Navegación sin recarga de página

---

*Última actualización: 18 de febrero de 2026*
