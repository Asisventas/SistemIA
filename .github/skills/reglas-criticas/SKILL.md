# 🚨 Reglas Críticas de Ejecución - SistemIA

## Descripción
Este skill contiene las reglas CRÍTICAS sobre comandos y operaciones que DEBEN o NO DEBEN ejecutarse en el proyecto SistemIA.

---

## ⚠️ REGLA PRIMORDIAL - Ejecución del Servidor

> **CRÍTICO:** Al ejecutar el servidor (`dotnet run`) y luego hacer solicitudes HTTP (Invoke-RestMethod, curl, etc.) desde la misma terminal o proceso, **el servidor se cierra automáticamente**.

### ❌ PROHIBIDO
```powershell
# NUNCA hacer esto - El servidor se cerrará
dotnet run --urls "http://localhost:5095" &
Invoke-RestMethod -Uri "http://localhost:5095/endpoint" -Method POST
```

### ✅ CORRECTO
```powershell
# Servidor como proceso independiente
Start-Process -FilePath "dotnet" -ArgumentList "run","--urls","http://localhost:5095" -WorkingDirectory "c:\asis\SistemIA" -WindowStyle Hidden
Start-Sleep -Seconds 20  # Esperar que compile e inicie

# Luego en OTRA terminal o comando separado:
Invoke-RestMethod -Uri "http://localhost:5095/endpoint" -Method POST
```

### Alternativas válidas:
- Usar tareas de VS Code separadas para servidor y pruebas
- Abrir el navegador manualmente
- Usar herramientas externas (Postman, Bruno)

---

## 🗃️ Entity Framework Core - Migraciones

### ⚠️ NUNCA usar `--no-build` al CREAR migraciones

```powershell
# ✅ CORRECTO - Crear migración (SIN --no-build)
dotnet ef migrations add NombreMigracion

# ✅ CORRECTO - Aplicar migración (puede usar --no-build)
dotnet ef database update --no-build

# ❌ INCORRECTO - Puede crear migración VACÍA
dotnet ef migrations add NombreMigracion --no-build
```

### 🚫 PROHIBIDO: Crear o Alterar Tablas por SQL Directo

> **NUNCA crear tablas, agregar columnas o modificar estructura de BD usando scripts SQL directos.**

```powershell
# ❌ PROHIBIDO - No crear tablas así
sqlcmd -Q "CREATE TABLE MiTabla (...)"

# ❌ PROHIBIDO - No alterar tablas así  
sqlcmd -Q "ALTER TABLE MiTabla ADD Columna INT"

# ✅ CORRECTO - Usar migraciones EF Core
# 1. Modificar el modelo en Models/
# 2. Crear migración: dotnet ef migrations add Agregar_Columna_MiTabla
# 3. Aplicar: dotnet ef database update
```

### Migraciones Idempotentes (Para Tablas que Podrían Existir)
```csharp
// En el método Up() de la migración:
migrationBuilder.Sql(@"
    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MiTabla')
    BEGIN
        CREATE TABLE [MiTabla] (...);
    END
");
```

### Comandos de Migraciones
```powershell
# Crear nueva migración
dotnet ef migrations add NombreDescriptivo

# Aplicar migraciones pendientes
dotnet ef database update

# Remover última migración (si no se aplicó)
dotnet ef migrations remove

# Generar script SQL idempotente
dotnet ef migrations script --idempotent -o "Installer\CrearBaseDatos.sql"
```

---

## 📦 Publicación - Reglas Obligatorias

### Siempre Self-Contained
```powershell
dotnet publish -c Release -o publish_selfcontained --self-contained true -r win-x64
```
**¿Por qué?** El cliente puede no tener .NET 8 instalado.

### Después de cada migración, regenerar script de BD
```powershell
dotnet ef migrations script --idempotent -o "Installer\CrearBaseDatos.sql"
```

---

## 🔢 Problema de Decimales (Cultura)

**Síntoma:** Error `"1,05" cannot be parsed` en inputs numéricos.

### ❌ INCORRECTO
```razor
<input type="number" value="@factorPrecio" />
```

### ✅ CORRECTO
```razor
<input type="number" value="@(factorPrecio?.ToString(CultureInfo.InvariantCulture))" />
```

---

## 🔄 Puertos de Desarrollo

| Protocolo | Puerto | Uso |
|-----------|--------|-----|
| HTTP | `http://localhost:5095` | Desarrollo normal |
| HTTPS | `https://localhost:7060` | Con certificado |

---

## 🐛 Anti-patrones a Evitar

```csharp
// ❌ INCORRECTO - Query en el render
@foreach (var item in _db.Productos.ToList())

// ✅ CORRECTO - Cargar en OnInitializedAsync
private List<Producto> productos = new();
protected override async Task OnInitializedAsync()
{
    productos = await _db.Productos.ToListAsync();
}

// ❌ INCORRECTO - StateHasChanged sin verificar
await Task.Delay(100);
StateHasChanged();

// ✅ CORRECTO - Verificar si componente está vivo
if (!disposed)
    await InvokeAsync(StateHasChanged);
```

---

## 📝 Convención de Nombres de Migraciones

Usar nombres descriptivos en español:
- `Agregar_Campo_Producto`
- `Crear_Tabla_Ventas`
- `Modificar_FK_Cliente`
- `Eliminar_Columna_Obsoleta`
