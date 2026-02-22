# Sistema de Permisos y Auditoría - SistemIA

## 📋 Descripción

Sistema completo de control de acceso basado en roles (RBAC) con auditoría integral de acciones de usuarios.

## 🏗️ Arquitectura

### Modelos de Datos

#### 1. **Modulo** (`Models/Modulo.cs`)
Representa módulos y submódulos del sistema con estructura jerárquica.

```csharp
public class Modulo
{
    public int IdModulo { get; set; }
    public string Nombre { get; set; }              // "Ventas", "Compras", etc.
    public string? Descripcion { get; set; }
    public string? Icono { get; set; }              // "bi-cart", "bi-box"
    public int? Orden { get; set; }
    public int? IdModuloPadre { get; set; }         // Para jerarquía
    public string? RutaPagina { get; set; }         // "/ventas", "/productos"
    public bool Activo { get; set; }
}
```

#### 2. **Permiso** (`Models/Permiso.cs`)
Define tipos de acciones disponibles.

```csharp
public class Permiso
{
    public int IdPermiso { get; set; }
    public string Nombre { get; set; }              // "Ver", "Crear", "Editar"
    public string Codigo { get; set; }              // "VIEW", "CREATE", "EDIT"
    public string? Descripcion { get; set; }
    public int? Orden { get; set; }
    public bool Activo { get; set; }
}
```

**Permisos Estándar:**
- `VIEW`: Ver y consultar información
- `CREATE`: Crear nuevos registros
- `EDIT`: Modificar registros existentes
- `DELETE`: Eliminar registros
- `EXPORT`: Exportar datos a Excel/PDF
- `PRINT`: Imprimir documentos

#### 3. **RolModuloPermiso** (`Models/RolModuloPermiso.cs`)
Tabla de unión que relaciona Roles → Módulos → Permisos.

```csharp
public class RolModuloPermiso
{
    public int IdRolModuloPermiso { get; set; }
    public int IdRol { get; set; }
    public int IdModulo { get; set; }
    public int IdPermiso { get; set; }
    public bool Concedido { get; set; }
    public DateTime FechaAsignacion { get; set; }
    public string? UsuarioAsignacion { get; set; }
}
```

#### 4. **AuditoriaAccion** (`Models/AuditoriaAccion.cs`)
Registro completo de acciones de usuarios.

```csharp
public class AuditoriaAccion
{
    public int IdAuditoria { get; set; }
    public int IdUsuario { get; set; }
    public string NombreUsuario { get; set; }
    public string? RolUsuario { get; set; }
    public DateTime FechaHora { get; set; }
    public string? Modulo { get; set; }
    public string? Accion { get; set; }
    public string? TipoAccion { get; set; }         // CREATE, UPDATE, DELETE
    public string? Entidad { get; set; }
    public int? IdRegistroAfectado { get; set; }
    public string? Descripcion { get; set; }
    public string? DatosAntes { get; set; }         // JSON
    public string? DatosDespues { get; set; }       // JSON
    public string? DireccionIP { get; set; }
    public string? Navegador { get; set; }
    public bool Exitosa { get; set; }
    public string? MensajeError { get; set; }
    public string Severidad { get; set; }           // INFO, WARNING, ERROR, CRITICAL
}
```

## 🔧 Servicios

### PermisosService (`Services/PermisosService.cs`)

#### Métodos:

**`TienePermisoAsync(int idUsuario, string codigoModulo, string codigoPermiso)`**
Verifica si un usuario tiene un permiso específico.

```csharp
var tienePermiso = await PermisosService.TienePermisoAsync(1, "/ventas", "CREATE");
```

**`ObtenerModulosConPermisosAsync(int idRol)`**
Obtiene matriz completa de permisos para un rol.

```csharp
var modulos = await PermisosService.ObtenerModulosConPermisosAsync(1);
```

**`AsignarPermisoAsync(int idRol, int idModulo, int idPermiso, bool conceder, string usuarioAsignacion)`**
Otorga o revoca un permiso específico.

```csharp
var exitoso = await PermisosService.AsignarPermisoAsync(2, 5, 1, true, "admin");
```

**`ObtenerModulosAccesiblesAsync(int idUsuario)`**
Lista módulos a los que el usuario tiene acceso.

```csharp
var modulosAccesibles = await PermisosService.ObtenerModulosAccesiblesAsync(1);
```

### AuditoriaService (`Services/AuditoriaService.cs`)

#### Métodos:

**`RegistrarAccionAsync(...)`**
Crea un registro de auditoría con todos los detalles.

```csharp
await AuditoriaService.RegistrarAccionAsync(
    idUsuario: 1,
    nombreUsuario: "Juan Pérez",
    rolUsuario: "Administrador",
    accion: "Crear nueva venta",
    tipoAccion: "CREATE",
    modulo: "Ventas",
    entidad: "Venta",
    idRegistroAfectado: 123,
    descripcion: "Venta creada por $500.000",
    datosAntes: null,
    datosDespues: ventaObj,
    direccionIP: "192.168.1.100",
    navegador: "Chrome 120.0",
    exitosa: true
);
```

**`ObtenerHistorialAsync(...)`**
Consulta filtrada de auditorías.

```csharp
var historial = await AuditoriaService.ObtenerHistorialAsync(
    fechaDesde: DateTime.Today.AddDays(-7),
    fechaHasta: DateTime.Today,
    idUsuario: null,
    modulo: "Ventas",
    tipoAccion: "CREATE",
    limite: 100
);
```

**`ObtenerEstadisticasAsync(...)`**
Estadísticas agregadas por tipo de acción.

```csharp
var stats = await AuditoriaService.ObtenerEstadisticasAsync(
    fechaDesde: DateTime.Today.AddMonths(-1),
    fechaHasta: DateTime.Today
);
// Resultado: { "CREATE": 150, "UPDATE": 89, "DELETE": 12 }
```

## 🎨 Componentes UI

### RequirePermission (`Components/RequirePermission.razor`)

Componente para proteger contenido según permisos.

#### Uso básico:

```razor
<RequirePermission Modulo="/ventas" Permiso="CREATE">
    <button class="btn btn-primary">
        <i class="bi bi-plus"></i> Nueva Venta
    </button>
</RequirePermission>
```

#### Con mensaje de acceso denegado:

```razor
<RequirePermission Modulo="/productos" Permiso="DELETE" MostrarMensajeDenegado="true">
    <button class="btn btn-danger" @onclick="EliminarProducto">
        <i class="bi bi-trash"></i> Eliminar
    </button>
</RequirePermission>
```

#### Proteger sección completa:

```razor
<RequirePermission Modulo="/inventario" Permiso="VIEW">
    <div class="card">
        <div class="card-header">Ajustes de Stock</div>
        <div class="card-body">
            @* Contenido protegido *@
        </div>
    </div>
</RequirePermission>
```

## 📄 Páginas

### 1. Permisos de Usuarios (`/personal/permisos-usuarios`)

Interfaz para gestionar permisos por rol.

**Características:**
- Selector de roles
- Matriz de permisos (módulos × tipos de permiso)
- Estructura jerárquica de módulos
- Checkboxes para activar/desactivar
- Botones: Guardar Cambios, Recargar

**Acceso:** 
Menú → Gestión de Personal → Permisos de Usuarios

### 2. Auditoría del Sistema (`/configuracion/auditoria`)

Visor completo de auditorías con filtros avanzados.

**Características:**
- Filtros: Fecha, Usuario, Módulo, Tipo Acción, Límite
- Tabla con información detallada
- Modal de detalle con diff JSON (antes/después)
- Estadísticas agregadas
- Exportación a Excel (en desarrollo)

**Acceso:**
Menú → Configuración → Auditoría

## 🚀 Datos Iniciales (Seed)

El sistema inicializa automáticamente al arrancar:

### Permisos (6):
1. Ver (VIEW)
2. Crear (CREATE)
3. Editar (EDIT)
4. Eliminar (DELETE)
5. Exportar (EXPORT)
6. Imprimir (PRINT)

### Módulos Principales (8):
- Ventas (bi-cart)
- Compras (bi-bag)
- Inventario (bi-box-seam)
- Clientes (bi-people)
- Proveedores (bi-truck)
- Reportes (bi-graph-up)
- Gestión de Personal (bi-person-badge)
- Configuración (bi-gear)

### Submódulos (10):
- Ventas: Presupuestos, Historial
- Inventario: Productos, Ajustes de Stock
- Personal: Empleados, Asistencias, **Permisos de Usuarios**
- Configuración: Usuarios, Roles, **Auditoría**

### Permisos de Administrador:
- ✅ Automáticamente tiene **TODOS** los permisos en **TODOS** los módulos

## 🔒 Integración en Páginas Existentes

### Ejemplo 1: Proteger botón de eliminación

```razor
@page "/productos"
@using SistemIA.Components

<RequirePermission Modulo="/productos" Permiso="DELETE">
    <button class="btn btn-danger" @onclick="EliminarProducto">
        <i class="bi bi-trash"></i> Eliminar
    </button>
</RequirePermission>
```

### Ejemplo 2: Ocultar columna de acciones

```razor
<thead>
    <tr>
        <th>Código</th>
        <th>Descripción</th>
        <th>Precio</th>
        <RequirePermission Modulo="/productos" Permiso="EDIT">
            <th>Acciones</th>
        </RequirePermission>
    </tr>
</thead>
```

### Ejemplo 3: Verificar permiso en código C#

```csharp
@code {
    private bool puedeEditar = false;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        
        if (user?.Identity?.IsAuthenticated == true)
        {
            var idUsuarioClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (idUsuarioClaim != null && int.TryParse(idUsuarioClaim.Value, out int idUsuario))
            {
                puedeEditar = await PermisosService.TienePermisoAsync(idUsuario, "/productos", "EDIT");
            }
        }
    }
}
```

## 📊 Registrar Auditorías

### Ejemplo: Auditoría al crear venta

```csharp
try
{
    // Crear venta
    var venta = new Venta { ... };
    ctx.Ventas.Add(venta);
    await ctx.SaveChangesAsync();

    // Registrar auditoría
    await AuditoriaService.RegistrarAccionAsync(
        idUsuario: _idUsuarioActual,
        nombreUsuario: _nombreUsuarioActual,
        rolUsuario: _rolUsuarioActual,
        accion: "Crear venta",
        tipoAccion: "CREATE",
        modulo: "Ventas",
        entidad: "Venta",
        idRegistroAfectado: venta.IdVenta,
        descripcion: $"Venta #{venta.IdVenta} creada - Cliente: {cliente.RazonSocial} - Total: {venta.Total:N0}",
        datosDespues: venta,
        direccionIP: HttpContext.Connection.RemoteIpAddress?.ToString(),
        navegador: HttpContext.Request.Headers["User-Agent"].ToString(),
        exitosa: true
    );
}
catch (Exception ex)
{
    // Auditoría de error
    await AuditoriaService.RegistrarAccionAsync(
        idUsuario: _idUsuarioActual,
        nombreUsuario: _nombreUsuarioActual,
        rolUsuario: _rolUsuarioActual,
        accion: "Crear venta (error)",
        tipoAccion: "CREATE",
        modulo: "Ventas",
        entidad: "Venta",
        descripcion: "Error al crear venta",
        exitosa: false,
        mensajeError: ex.Message,
        severidad: "ERROR"
    );
}
```

## ⚙️ Configuración

Los servicios están registrados en `Program.cs`:

```csharp
builder.Services.AddScoped<AuditoriaService>();
builder.Services.AddScoped<PermisosService>();
```

El seed data se ejecuta automáticamente:

```csharp
using (var scope = app.Services.CreateScope())
{
    var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    await SeedPermisos.InicializarPermisosAsync(dbFactory);
}
```

## 🗃️ Base de Datos

### Tablas Creadas:
- `Modulos`: Módulos y submódulos del sistema
- `Permisos`: Tipos de permisos disponibles
- `RolesModulosPermisos`: Asignación de permisos a roles
- `AuditoriasAcciones`: Registro de auditorías

### Índices:
- `IX_Modulos_IdModuloPadre`
- `IX_RolesModulosPermisos_IdRol`
- `IX_RolesModulosPermisos_IdModulo`
- `IX_RolesModulosPermisos_IdPermiso`
- `IX_AuditoriasAcciones_IdUsuario`

## 📝 Notas Importantes

1. **Rendimiento**: El componente `RequirePermission` cachea la verificación de permisos durante la sesión del componente.

2. **Seguridad**: Las verificaciones de permisos se realizan en el servidor. No confíe únicamente en ocultar elementos en el frontend.

3. **Auditoría**: Los registros de auditoría están envueltos en try-catch para no romper operaciones críticas si falla el logging.

4. **JSON**: Los campos `DatosAntes` y `DatosDespues` usan `System.Text.Json` para serialización automática.

5. **Cascading Delete**: Las relaciones están configuradas con CASCADE, eliminar un módulo elimina sus permisos asignados.

## 🔜 Mejoras Futuras

- [ ] Cache de permisos con IMemoryCache (5 minutos de expiración)
- [ ] Atributo `[RequirePermission]` para proteger páginas completas
- [ ] Exportación de auditorías a Excel/PDF
- [ ] Dashboard de auditoría con gráficos
- [ ] Notificaciones en tiempo real de cambios de permisos
- [ ] Historial de cambios en permisos (quién modificó qué y cuándo)
- [ ] Roles predefinidos (Vendedor, Contador, Gerente, etc.)
- [ ] Copiar permisos entre roles
- [ ] Permisos por usuario individual (override de rol)

---

**Fecha de creación:** 16/12/2025  
**Versión:** 1.0  
**Sistema:** SistemIA - Sistema Integral de Gestión
