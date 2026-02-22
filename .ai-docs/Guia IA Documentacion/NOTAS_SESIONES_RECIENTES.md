# Notas de Sesiones Recientes - SistemIA

Este documento contiene información relevante de las conversaciones recientes para referencia de la IA.

---

## 📅 Diciembre 2025

### 🔐 HTTPS con MKCERT - Certificados Locales

**Problema resuelto:** El navegador requiere contexto seguro (HTTPS) para usar la API de cámara (`getUserMedia`).

**Solución implementada:** Usar [mkcert](https://github.com/FiloSottile/mkcert) para generar certificados locales de desarrollo.

#### Ubicación de archivos:
```
Installer/Certificados/
├── mkcert.exe                      # Herramienta de generación (v1.4.4)
├── Instalar-Certificado-Servidor.ps1  # Genera cert para el servidor
├── Instalar-Certificado-Servidor.bat
├── Instalar-Certificado-Cliente.ps1   # Instala CA en clientes
├── Instalar-Certificado-Cliente.bat
└── README.md
```

#### Contraseñas de certificados:
- **Certificado del instalador (PFX):** `SistemIA2024!`
- **Certificado mkcert:** `changeit` (por defecto)

#### Comandos mkcert:
```powershell
# Instalar CA local (una vez en la máquina)
.\mkcert.exe -install

# Generar certificado para el servidor
.\mkcert.exe -p12-file sistemIA.p12 localhost 127.0.0.1 192.168.1.100 "*.local"

# El archivo rootCA.pem se encuentra en:
# Windows: C:\Users\<usuario>\AppData\Local\mkcert
```

#### Configuración en appsettings.json:
```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://0.0.0.0:7060",
        "Certificate": {
          "Path": "sistemIA.p12",
          "Password": "changeit"
        }
      }
    }
  }
}
```

#### Para clientes remotos:
1. Copiar el archivo `rootCA.pem` del servidor al cliente
2. Ejecutar `Instalar-Certificado-Cliente.ps1` en el cliente
3. Reiniciar Chrome

---

### 📦 Publicación Self-Contained

**Comando de publicación:**
```powershell
dotnet publish -c Release -r win-x64 --self-contained true -o "publish"
```

**Ubicación del paquete:**
- `Releases/SistemIA_SelfContained_YYYY.MM.DD.HHMM.zip`

**Scripts relacionados:**
- `Installer/Crear-Paquete.ps1` - Crea paquete completo
- `Installer/Crear-Paquete-Actualizacion.ps1` - Crea paquete de actualización

---

### 🗄️ Error TipoOperacion - Longitud de Columna

**Problema:** Error FK constraint entre `Clientes.TipoOperacion` (length 1) y `TiposOperacion.Codigo` (length 3).

**Solución:** Cambiar `[StringLength(1)]` a `[StringLength(3)]` en `Models/Cliente.cs`:

```csharp
[StringLength(3)]
public string? TipoOperacion { get; set; }
```

---

### 🎨 Tema Oscuro - Badges Visibles

**Problema:** Los badges con clase `bg-info` no son visibles en tema oscuro.

**Solución:** Usar clases alternativas:
```razor
// Antes
<span class="badge bg-info">Depósito</span>
<span class="badge bg-info text-dark">Factor</span>

// Después  
<span class="badge bg-primary">Depósito</span>
<span class="badge bg-secondary">Factor</span>
```

---

### 🔒 Validación de Contraseña y Permisos

**Patrón de validación de contraseña con SHA256:**

```csharp
@using System.Security.Cryptography
@inject AuthenticationStateProvider AuthStateProvider
@inject SistemIA.Services.PermisosService PermisosService

private async Task<bool> ValidarAccesoSeguro(string password, string modulo, string permiso)
{
    // 1. Obtener usuario actual
    var authState = await AuthStateProvider.GetAuthenticationStateAsync();
    var userName = authState.User.Identity?.Name;
    
    if (string.IsNullOrEmpty(userName)) return false;
    
    await using var db = await DbFactory.CreateDbContextAsync();
    var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.UsuarioNombre == userName);
    
    if (usuario == null) return false;
    
    // 2. Verificar contraseña con SHA256
    using var sha = SHA256.Create();
    var passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
    var hash = sha.ComputeHash(passwordBytes);
    
    if (usuario.ContrasenaHash == null || !hash.SequenceEqual(usuario.ContrasenaHash))
        return false;
    
    // 3. Verificar permisos
    return await PermisosService.TienePermisoAsync(usuario.Id_Usu, modulo, permiso);
}
```

**Módulos y permisos comunes:**
- `/caja` - EDIT, VIEW
- `/panel` - EDIT, VIEW
- `/ventas` - CREATE, EDIT, DELETE, PRINT
- `/notas-credito` - CREATE, EDIT, DELETE, PRINT

---

### 📊 Panel de Control - Modal Fecha/Turno

**Implementación:** Modal en `Pages/Index.razor` para cambiar fecha y turno de caja.

**Características:**
- Requiere contraseña del usuario actual
- Verifica permiso `/caja` EDIT o `/panel` EDIT
- Actualiza campos `FechaActualCaja` y `TurnoActual` en tabla `Cajas`

---

### 🖨️ Impresión de Tickets

**Servicio:** `Services/ImpresionDirectaService.cs`

**Componentes de vista previa:**
- `Shared/NotaCreditoTicketVistaPrevia.razor`
- `Shared/VentaTicketVistaPrevia.razor`

---

### 📝 Modelo Usuario

**Propiedades importantes:**
```csharp
public class Usuario
{
    public int Id_Usu { get; set; }      // PK (NO es "Id")
    public string UsuarioNombre { get; set; }
    public byte[]? ContrasenaHash { get; set; }  // SHA256
    public int Id_Rol { get; set; }
}
```

---

### 🗃️ Estructura del Proyecto

```
SistemIA/
├── Models/           # Entidades y AppDbContext
├── Pages/            # Páginas Razor
├── Shared/           # Componentes compartidos
│   ├── Reportes/     # KUDEs y reportes
├── Services/         # Servicios de negocio
├── Components/       # Componentes de UI
├── Data/            # Seeds y datos iniciales
├── Installer/       # Scripts de instalación
│   └── Certificados/ # Herramientas HTTPS
├── Migrations/      # Migraciones EF Core
└── wwwroot/         # Archivos estáticos
```

---

### 🔧 Comandos Frecuentes

```powershell
# Compilar
dotnet build

# Ejecutar con watch
dotnet watch run

# Ejecutar en puerto específico
$env:ASPNETCORE_URLS='http://localhost:5095'; dotnet watch run

# Crear migración
dotnet ef migrations add NombreMigracion --no-build

# Aplicar migración
dotnet ef database update --no-build

# Publicar self-contained
dotnet publish -c Release -r win-x64 --self-contained true -o publish
```

---

### 📌 Conexión a Base de Datos

**Servidor:** `SERVERSIS\SQL2022`
**Base de datos:** `asiswebapp`

**Connection string en appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=SERVERSIS\\SQL2022;Database=asiswebapp;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

---

### ⚡ Providers de Contexto

```csharp
// Obtener sucursal activa
@inject ISucursalProvider SucursalProvider
var idSucursal = await SucursalProvider.GetSucursalIdAsync();

// Obtener caja activa
@inject ICajaProvider CajaProvider
var idCaja = await CajaProvider.GetCajaIdAsync();

// Servicio de caja completo
@inject ICajaService CajaService
var caja = await CajaService.ObtenerCajaActiva();
```

---

## 🔄 Actualizaciones Pendientes

- Integrar certificados HTTPS en paquete de instalación ✅
- Documentar proceso de creación de módulos ✅
- Fix tema oscuro en Compras ✅
- Modal Fecha/Turno con seguridad ✅
