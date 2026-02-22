# Guía Rápida: Cómo Generar un Paquete de Actualización

## 📦 Contenido del ZIP de Actualización

El archivo ZIP debe contener todos los archivos de la carpeta `publish` generada por `dotnet publish`. La estructura debe ser:

```
SistemIA_Update_YYYYMMDD_HHMM.zip
│
├── SistemIA.dll                    (Aplicación principal)
├── SistemIA.exe                    (Ejecutable)
├── SistemIA.deps.json              (Dependencias)
├── SistemIA.runtimeconfig.json     (Configuración runtime)
├── web.config                      (Configuración IIS)
│
├── wwwroot/                        (Archivos estáticos)
│   ├── css/
│   ├── js/
│   ├── images/
│   └── ...
│
├── appsettings.json               (⚠️ SE PRESERVARÁ - no se sobrescribe)
├── appsettings.Production.json    (⚠️ SE PRESERVARÁ - no se sobrescribe)
│
├── [Todas las DLL de dependencias]
│   ├── Microsoft.*.dll
│   ├── System.*.dll
│   └── ...
│
└── [Otros archivos necesarios]
    ├── _Imports.razor
    ├── App.razor
    └── ...
```

## 🛠️ Pasos para Crear Actualización (Desarrollo → Cliente)

### Paso 1: Preparar el Código

```powershell
# En tu servidor de desarrollo
cd C:\asis\SistemIA

# Asegúrate de que todo compile
dotnet build -c Release

# Ejecuta pruebas si las tienes
# dotnet test
```

### Paso 2: Generar Migraciones (si hay cambios en BD)

```powershell
# Si modificaste models o agregaste nuevas tablas
dotnet ef migrations add NombreDeLaMigracion --project SistemIA.csproj

# Verificar la migración generada
dotnet ef migrations list
```

### Paso 3: Publicar en modo Release

```powershell
# Limpiar compilaciones anteriores
dotnet clean -c Release

# Publicar
dotnet publish -c Release -o ./publish --self-contained false

# La carpeta ./publish ahora contiene todos los archivos necesarios
```

### Paso 4: Crear el ZIP

```powershell
# Opción 1: PowerShell
$fecha = Get-Date -Format "yyyyMMdd_HHmm"
$zipPath = "SistemIA_Update_$fecha.zip"

# Verificar que la carpeta publish existe
if (Test-Path ".\publish") {
    # Crear ZIP
    Compress-Archive -Path ".\publish\*" -DestinationPath $zipPath -CompressionLevel Optimal
    
    Write-Host "✓ Paquete creado: $zipPath" -ForegroundColor Green
    Write-Host "  Tamaño: $([math]::Round((Get-Item $zipPath).Length / 1MB, 2)) MB"
} else {
    Write-Host "✗ Error: No existe la carpeta 'publish'" -ForegroundColor Red
}
```

```powershell
# Opción 2: Script automatizado
.\Scripts\CrearPaqueteActualizacion.ps1
```

### Paso 5: Verificar el ZIP

```powershell
# Extraer en temporal para verificar
$tempDir = "$env:TEMP\verify_update"
Expand-Archive -Path $zipPath -DestinationPath $tempDir -Force

# Verificar archivos críticos
$archivosRequeridos = @(
    "$tempDir\SistemIA.dll",
    "$tempDir\SistemIA.exe",
    "$tempDir\appsettings.json",
    "$tempDir\web.config"
)

$todoOk = $true
foreach ($archivo in $archivosRequeridos) {
    if (Test-Path $archivo) {
        Write-Host "✓ $([System.IO.Path]::GetFileName($archivo))" -ForegroundColor Green
    } else {
        Write-Host "✗ FALTA: $([System.IO.Path]::GetFileName($archivo))" -ForegroundColor Red
        $todoOk = $false
    }
}

# Limpiar
Remove-Item -Path $tempDir -Recurse -Force

if ($todoOk) {
    Write-Host "`n✓ Paquete válido y listo para transferir" -ForegroundColor Green
} else {
    Write-Host "`n✗ Paquete incompleto - revisar compilación" -ForegroundColor Red
}
```

### Paso 6: Documentar Cambios

Crea un archivo `CHANGELOG.txt` o `VERSION.txt` junto al ZIP:

```
========================================
SistemIA - Actualización 15/12/2025
Versión: 1.1.0
========================================

NUEVAS CARACTERÍSTICAS:
- Módulo de actualización automática
- Gestión de backups integrada
- Mejoras en el sistema de pagos a proveedores

CORRECCIONES:
- Corregido error en cálculo de stock
- Mejorado rendimiento en listados

CAMBIOS EN BASE DE DATOS:
- Se agregan migraciones: [nombres]
- Nuevas tablas: [si aplica]

REQUISITOS:
- .NET 8.0 Runtime
- SQL Server 2019+

INSTRUCCIONES:
1. Crear backup manual (recomendado)
2. Usar interfaz web o script PowerShell
3. Reiniciar aplicación después
4. Verificar funcionamiento

CONTACTO:
soporte@sistemiacorp.com
```

### Paso 7: Transferir al Cliente

**Opciones seguras:**

1. **USB/Disco externo**
   ```powershell
   Copy-Item $zipPath -Destination "E:\Actualizaciones"
   ```

2. **Red compartida**
   ```powershell
   Copy-Item $zipPath -Destination "\\SERVIDOR-CLIENTE\Compartido\Updates"
   ```

3. **FTP/SFTP** (si está configurado)

4. **Servicio en la nube** (OneDrive, Dropbox, Google Drive)

**⚠️ Verificar integridad después de transferir:**

```powershell
# En el servidor del cliente
$hashOriginal = "ABC123..."  # Hash del archivo original
$hashTransferido = (Get-FileHash "C:\Temp\SistemIA_Update.zip").Hash

if ($hashOriginal -eq $hashTransferido) {
    Write-Host "✓ Archivo transferido correctamente" -ForegroundColor Green
} else {
    Write-Host "✗ El archivo se corrompió durante la transferencia" -ForegroundColor Red
}
```

## 🔄 Flujo Completo (Resumen Visual)

```
┌──────────────────────────────────────────────────────────┐
│            SERVIDOR DE DESARROLLO                        │
│                                                          │
│  1. Modificar código                                     │
│  2. Crear migraciones (si aplica)                        │
│  3. Compilar: dotnet build -c Release                    │
│  4. Publicar: dotnet publish -c Release -o ./publish     │
│  5. Crear ZIP: Compress-Archive                          │
│  6. Verificar contenido                                  │
│  7. Documentar cambios                                   │
│                                                          │
└─────────────────┬────────────────────────────────────────┘
                  │ Transferir ZIP
                  ↓
┌──────────────────────────────────────────────────────────┐
│            SERVIDOR DEL CLIENTE                          │
│                                                          │
│  8. Recibir archivo ZIP                                  │
│  9. Verificar integridad                                 │
│ 10. Opción A: Usar interfaz web                          │
│     - Ir a /actualizacion-sistema                        │
│     - Subir archivo                                      │
│     - Iniciar actualización                              │
│                                                          │
│     Opción B: Usar script PowerShell                     │
│     - Ejecutar ActualizarSistemIA.ps1                    │
│     - Especificar ruta del ZIP                           │
│                                                          │
│ 11. Esperar a que complete                               │
│ 12. Reiniciar aplicación                                 │
│ 13. Verificar funcionamiento                             │
│                                                          │
└──────────────────────────────────────────────────────────┘
```

## 📋 Checklist Rápido

### Antes de Crear el Paquete
- [ ] Código compilando sin errores
- [ ] Migraciones creadas (si hay cambios en BD)
- [ ] Pruebas pasando (si existen)
- [ ] Cambios documentados

### Crear el Paquete
- [ ] `dotnet publish -c Release` ejecutado
- [ ] ZIP creado con todos los archivos
- [ ] Contenido del ZIP verificado
- [ ] Tamaño del archivo razonable (< 500 MB recomendado)

### Documentación
- [ ] CHANGELOG.txt creado
- [ ] Versión actualizada
- [ ] Instrucciones especiales (si aplica)
- [ ] Requisitos documentados

### Transferencia
- [ ] Archivo transferido al cliente
- [ ] Integridad verificada (hash)
- [ ] Cliente notificado
- [ ] Horario coordinado

### Después de Actualizar
- [ ] Aplicación iniciada correctamente
- [ ] Migraciones aplicadas
- [ ] Funcionalidades críticas probadas
- [ ] Logs revisados
- [ ] Usuarios notificados

## 🚀 Script Automatizado Completo

Guarda esto como `Scripts\CrearPaqueteActualizacion.ps1`:

```powershell
# Script para crear paquete de actualización completo

param(
    [string]$Version = "",
    [string]$OutputDir = ".\Releases"
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  CREACIÓN DE PAQUETE DE ACTUALIZACIÓN" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Obtener versión si no se especificó
if ([string]::IsNullOrEmpty($Version)) {
    $Version = Read-Host "Ingrese número de versión (ej: 1.1.0)"
}

$fecha = Get-Date -Format "yyyyMMdd_HHmm"
$nombreZip = "SistemIA_Update_v${Version}_$fecha.zip"
$changelog = "CHANGELOG_v${Version}.txt"

try {
    # 1. Limpiar
    Write-Host "[1/6] Limpiando compilaciones anteriores..." -ForegroundColor Yellow
    if (Test-Path ".\publish") {
        Remove-Item ".\publish" -Recurse -Force
    }
    dotnet clean -c Release | Out-Null
    Write-Host "✓ Limpieza completada`n" -ForegroundColor Green

    # 2. Compilar
    Write-Host "[2/6] Compilando en modo Release..." -ForegroundColor Yellow
    $buildOutput = dotnet build -c Release 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Error en compilación"
    }
    Write-Host "✓ Compilación exitosa`n" -ForegroundColor Green

    # 3. Publicar
    Write-Host "[3/6] Publicando aplicación..." -ForegroundColor Yellow
    $publishOutput = dotnet publish -c Release -o ./publish --self-contained false 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Error en publicación"
    }
    Write-Host "✓ Publicación completada`n" -ForegroundColor Green

    # 4. Crear directorio de salida
    if (-not (Test-Path $OutputDir)) {
        New-Item -Path $OutputDir -ItemType Directory | Out-Null
    }

    # 5. Crear ZIP
    Write-Host "[4/6] Creando archivo ZIP..." -ForegroundColor Yellow
    $zipPath = Join-Path $OutputDir $nombreZip
    Compress-Archive -Path ".\publish\*" -DestinationPath $zipPath -CompressionLevel Optimal -Force
    $tamanoMB = [math]::Round((Get-Item $zipPath).Length / 1MB, 2)
    Write-Host "✓ ZIP creado: $nombreZip ($tamanoMB MB)`n" -ForegroundColor Green

    # 6. Crear CHANGELOG
    Write-Host "[5/6] Generando CHANGELOG..." -ForegroundColor Yellow
    $changelogPath = Join-Path $OutputDir $changelog
    $changelogContent = @"
========================================
SistemIA - Actualización
Versión: $Version
Fecha: $(Get-Date -Format 'dd/MM/yyyy HH:mm')
========================================

NUEVAS CARACTERÍSTICAS:
- [Agregar aquí]

CORRECCIONES:
- [Agregar aquí]

CAMBIOS EN BASE DE DATOS:
- [Agregar aquí si aplica]

REQUISITOS:
- .NET 8.0 Runtime
- SQL Server 2019+

INSTRUCCIONES:
1. Crear backup manual
2. Usar interfaz web o script PowerShell
3. Reiniciar aplicación
4. Verificar funcionamiento

========================================
"@
    Set-Content -Path $changelogPath -Value $changelogContent
    Write-Host "✓ CHANGELOG creado`n" -ForegroundColor Green

    # 7. Calcular hash
    Write-Host "[6/6] Calculando hash SHA256..." -ForegroundColor Yellow
    $hash = (Get-FileHash -Path $zipPath -Algorithm SHA256).Hash
    Write-Host "✓ Hash: $hash`n" -ForegroundColor Green

    # Resumen
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  PAQUETE CREADO EXITOSAMENTE" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Archivo:   $nombreZip"
    Write-Host "Ubicación: $OutputDir"
    Write-Host "Tamaño:    $tamanoMB MB"
    Write-Host "Hash:      $hash"
    Write-Host ""
    Write-Host "Archivos generados:" -ForegroundColor Cyan
    Write-Host "  - $zipPath"
    Write-Host "  - $changelogPath"
    Write-Host ""
    Write-Host "Próximos pasos:" -ForegroundColor Yellow
    Write-Host "  1. Editar $changelogPath con los cambios reales"
    Write-Host "  2. Transferir ambos archivos al cliente"
    Write-Host "  3. Verificar hash después de transferir"
    Write-Host ""

} catch {
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "  ERROR AL CREAR PAQUETE" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
```

## 💡 Consejos Finales

1. **Nombra los paquetes de forma consistente**
   - Incluye versión y fecha
   - Ejemplo: `SistemIA_Update_v1.1.0_20251215_1430.zip`

2. **Mantén un registro de versiones**
   - Guarda cada ZIP en una carpeta organizada
   - Documenta qué cambios incluye cada versión

3. **Prueba en entorno de desarrollo primero**
   - Crea un servidor de pruebas
   - Aplica la actualización allí primero

4. **Comunica con el cliente**
   - Avisa con anticipación
   - Proporciona ventana de tiempo
   - Ten plan de respaldo

5. **Automatiza lo que puedas**
   - Usa los scripts proporcionados
   - Crea tus propias variaciones según necesidad

---

**¿Necesitas ayuda?** Consulta [MODULO_ACTUALIZACION_README.md](./MODULO_ACTUALIZACION_README.md) para más detalles.
