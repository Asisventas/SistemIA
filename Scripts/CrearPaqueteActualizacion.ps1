# ============================================
# Script para Crear Paquete de Actualización
# ============================================

param(
    [string]$Version = "",
    [string]$OutputDir = ".\Releases",
    [switch]$IncluirMigraciones = $false
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "Continue"

# Función para escribir con color
function Write-ColorOutput {
    param([string]$Message, [string]$Color = "White")
    Write-Host $Message -ForegroundColor $Color
}

# Banner
Clear-Host
Write-ColorOutput "============================================" "Cyan"
Write-ColorOutput "  CREACIÓN DE PAQUETE DE ACTUALIZACIÓN" "Cyan"
Write-ColorOutput "  SistemIA" "Cyan"
Write-ColorOutput "============================================`n" "Cyan"

# Obtener versión si no se especificó
if ([string]::IsNullOrEmpty($Version)) {
    $Version = Read-Host "Ingrese número de versión (ej: 1.1.0)"
    if ([string]::IsNullOrEmpty($Version)) {
        Write-ColorOutput "✗ Versión requerida" "Red"
        exit 1
    }
}

$fecha = Get-Date -Format "yyyyMMdd_HHmm"
$nombreZip = "SistemIA_Update_v${Version}_$fecha.zip"
$changelog = "CHANGELOG_v${Version}.txt"

Write-ColorOutput "Versión: $Version" "White"
Write-ColorOutput "Fecha: $(Get-Date -Format 'dd/MM/yyyy HH:mm')`n" "White"

try {
    # ========================================
    # 1. LIMPIAR
    # ========================================
    Write-ColorOutput "[1/7] Limpiando compilaciones anteriores..." "Yellow"
    
    if (Test-Path ".\publish") {
        Remove-Item ".\publish" -Recurse -Force
    }
    
    $cleanOutput = dotnet clean -c Release 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-ColorOutput "⚠ Advertencia en limpieza (continuando...)" "Yellow"
    }
    
    Write-ColorOutput "✓ Limpieza completada`n" "Green"

    # ========================================
    # 2. COMPILAR
    # ========================================
    Write-ColorOutput "[2/7] Compilando en modo Release..." "Yellow"
    
    $buildOutput = dotnet build -c Release 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-ColorOutput "✗ Error en compilación:" "Red"
        Write-Host $buildOutput
        throw "La compilación falló"
    }
    
    # Contar advertencias
    $warnings = ($buildOutput | Select-String "warning").Count
    if ($warnings -gt 0) {
        Write-ColorOutput "⚠ Compilación exitosa con $warnings advertencias" "Yellow"
    } else {
        Write-ColorOutput "✓ Compilación exitosa sin advertencias" "Green"
    }
    Write-Host ""

    # ========================================
    # 3. PUBLICAR
    # ========================================
    Write-ColorOutput "[3/7] Publicando aplicación..." "Yellow"
    
    $publishArgs = @(
        "publish",
        "-c", "Release",
        "-o", "./publish",
        "--self-contained", "false",
        "--no-build"
    )
    
    $publishOutput = & dotnet $publishArgs 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-ColorOutput "✗ Error en publicación:" "Red"
        Write-Host $publishOutput
        throw "La publicación falló"
    }
    
    # Contar archivos publicados
    $fileCount = (Get-ChildItem -Path ".\publish" -Recurse -File).Count
    Write-ColorOutput "✓ Publicación completada ($fileCount archivos)`n" "Green"

    # ========================================
    # 4. VERIFICAR ARCHIVOS CRÍTICOS
    # ========================================
    Write-ColorOutput "[4/7] Verificando archivos críticos..." "Yellow"
    
    $archivosRequeridos = @(
        ".\publish\SistemIA.dll",
        ".\publish\SistemIA.exe",
        ".\publish\appsettings.json",
        ".\publish\web.config",
        ".\publish\SistemIA.deps.json",
        ".\publish\SistemIA.runtimeconfig.json"
    )
    
    $todosPresentes = $true
    foreach ($archivo in $archivosRequeridos) {
        $nombre = Split-Path -Leaf $archivo
        if (Test-Path $archivo) {
            Write-ColorOutput "  ✓ $nombre" "Green"
        } else {
            Write-ColorOutput "  ✗ FALTA: $nombre" "Red"
            $todosPresentes = $false
        }
    }
    
    if (-not $todosPresentes) {
        throw "Faltan archivos críticos en la publicación"
    }
    Write-Host ""

    # ========================================
    # 5. CREAR DIRECTORIO DE SALIDA
    # ========================================
    if (-not (Test-Path $OutputDir)) {
        New-Item -Path $OutputDir -ItemType Directory | Out-Null
        Write-ColorOutput "✓ Creado directorio: $OutputDir`n" "Green"
    }

    # ========================================
    # 6. CREAR ZIP
    # ========================================
    Write-ColorOutput "[5/7] Creando archivo ZIP..." "Yellow"
    
    $zipPath = Join-Path $OutputDir $nombreZip
    
    # Eliminar ZIP si ya existe
    if (Test-Path $zipPath) {
        Remove-Item $zipPath -Force
    }
    
    # Crear ZIP
    Write-Host "  Comprimiendo archivos..."
    Compress-Archive -Path ".\publish\*" -DestinationPath $zipPath -CompressionLevel Optimal
    
    $tamanoMB = [math]::Round((Get-Item $zipPath).Length / 1MB, 2)
    Write-ColorOutput "✓ ZIP creado: $nombreZip" "Green"
    Write-ColorOutput "  Tamaño: $tamanoMB MB`n" "Cyan"

    # ========================================
    # 7. CALCULAR HASH
    # ========================================
    Write-ColorOutput "[6/7] Calculando hash SHA256..." "Yellow"
    
    $hash = (Get-FileHash -Path $zipPath -Algorithm SHA256).Hash
    Write-ColorOutput "✓ Hash: $hash`n" "Green"
    
    # Guardar hash en archivo
    $hashFile = Join-Path $OutputDir "$nombreZip.sha256"
    Set-Content -Path $hashFile -Value "$hash  $nombreZip"

    # ========================================
    # 8. CREAR CHANGELOG
    # ========================================
    Write-ColorOutput "[7/7] Generando CHANGELOG..." "Yellow"
    
    $changelogPath = Join-Path $OutputDir $changelog
    
    # Obtener lista de migraciones si se solicita
    $migraciones = ""
    if ($IncluirMigraciones) {
        try {
            $migrationsList = dotnet ef migrations list --no-build 2>&1 | Select-String -Pattern "^\s*\d" | ForEach-Object { $_.Line.Trim() }
            if ($migrationsList) {
                $migraciones = "`nMIGRACIONES INCLUIDAS:`n"
                foreach ($mig in $migrationsList | Select-Object -Last 5) {
                    $migraciones += "- $mig`n"
                }
            }
        } catch {
            Write-ColorOutput "⚠ No se pudo obtener lista de migraciones" "Yellow"
        }
    }
    
    $changelogContent = @"
========================================
SistemIA - Actualización
Versión: $Version
Fecha: $(Get-Date -Format 'dd/MM/yyyy HH:mm')
Archivo: $nombreZip
Tamaño: $tamanoMB MB
Hash SHA256: $hash
========================================

NUEVAS CARACTERÍSTICAS:
- [Completar con las nuevas características implementadas]

MEJORAS:
- [Completar con las mejoras realizadas]

CORRECCIONES:
- [Completar con los bugs corregidos]

CAMBIOS EN BASE DE DATOS:
- [Indicar si hay migraciones nuevas]
$migraciones

ARCHIVOS INCLUIDOS:
- Total de archivos: $fileCount
- Archivos principales verificados: ✓

REQUISITOS:
- Windows Server 2019+ o Windows 10/11
- .NET 8.0 Runtime (ASP.NET Core)
- SQL Server 2019 o superior
- Espacio en disco: Mínimo $([math]::Round($tamanoMB * 3, 0)) MB libre

INSTRUCCIONES DE INSTALACIÓN:

Método 1 - Interfaz Web (Recomendado):
1. Acceder a https://servidor:7060/actualizacion-sistema
2. Seleccionar el archivo $nombreZip
3. Marcar "Aplicar migraciones de BD" si hay cambios en BD
4. Hacer clic en "Iniciar Actualización"
5. Esperar a que complete (NO cerrar navegador)
6. Reiniciar aplicación cuando se indique

Método 2 - Script PowerShell:
1. Copiar $nombreZip al servidor
2. Ejecutar PowerShell como Administrador
3. Ejecutar: .\Scripts\ActualizarSistemIA.ps1 -ArchivoZip "ruta\$nombreZip"
4. Seguir instrucciones en pantalla

VERIFICACIÓN POST-ACTUALIZACIÓN:
□ Servicio/aplicación inició correctamente
□ Login funciona
□ Funcionalidades principales operativas
□ No hay errores en logs
□ Base de datos actualizada correctamente

ROLLBACK (si es necesario):
Los backups se crean automáticamente en:
- Aplicación: C:\Backups\SistemIA\SistemIA_Backup_*.zip
- Base de datos: C:\Backups\SistemIA\SistemIA_backup_*.bak

Para restaurar, consultar: MODULO_ACTUALIZACION_README.md

SOPORTE:
Email: soporte@sistemiacorp.com
Teléfono: +595 21 XXX-XXXX

========================================
NOTAS TÉCNICAS:
- Generado automáticamente por CrearPaqueteActualizacion.ps1
- Verificar hash SHA256 después de transferir el archivo
- Leer GUIA_CREAR_PAQUETE_ACTUALIZACION.md para más detalles
========================================
"@
    
    Set-Content -Path $changelogPath -Value $changelogContent -Encoding UTF8
    Write-ColorOutput "✓ CHANGELOG creado`n" "Green"

    # ========================================
    # RESUMEN FINAL
    # ========================================
    Write-ColorOutput "============================================" "Green"
    Write-ColorOutput "  ✓ PAQUETE CREADO EXITOSAMENTE" "Green"
    Write-ColorOutput "============================================" "Green"
    Write-Host ""
    Write-ColorOutput "📦 ARCHIVOS GENERADOS:" "Cyan"
    Write-Host "   $zipPath"
    Write-Host "   $changelogPath"
    Write-Host "   $hashFile"
    Write-Host ""
    Write-ColorOutput "📊 INFORMACIÓN:" "Cyan"
    Write-Host "   Versión:        $Version"
    Write-Host "   Tamaño:         $tamanoMB MB"
    Write-Host "   Archivos:       $fileCount"
    Write-Host "   Hash SHA256:    $hash"
    Write-Host ""
    Write-ColorOutput "📝 PRÓXIMOS PASOS:" "Yellow"
    Write-Host "   1. Editar $changelogPath con los cambios reales"
    Write-Host "   2. Revisar que todos los cambios estén documentados"
    Write-Host "   3. Transferir los 3 archivos al cliente:"
    Write-Host "      - $nombreZip"
    Write-Host "      - $changelog"
    Write-Host "      - $nombreZip.sha256"
    Write-Host "   4. Verificar hash SHA256 después de transferir:"
    Write-Host "      PS> (Get-FileHash 'ruta\$nombreZip').Hash"
    Write-Host "   5. Aplicar actualización usando interfaz web o script"
    Write-Host ""
    Write-ColorOutput "⚠ IMPORTANTE:" "Yellow"
    Write-Host "   - Probar en entorno de pruebas primero si es posible"
    Write-Host "   - Notificar al cliente con anticipación"
    Write-Host "   - Programar actualización en horario de baja actividad"
    Write-Host "   - Asegurarse de que hay backups antes de actualizar"
    Write-Host ""
    
    # Abrir carpeta de salida
    $abrirCarpeta = Read-Host "¿Abrir carpeta de salida? (S/N)"
    if ($abrirCarpeta -match '^[Ss]$') {
        Invoke-Item $OutputDir
    }

    Write-Host ""
    exit 0

} catch {
    Write-ColorOutput "`n============================================" "Red"
    Write-ColorOutput "  ✗ ERROR AL CREAR PAQUETE" "Red"
    Write-ColorOutput "============================================" "Red"
    Write-Host ""
    Write-ColorOutput "Error: $($_.Exception.Message)" "Red"
    Write-Host ""
    Write-ColorOutput "Detalles:" "Yellow"
    Write-Host $_.ScriptStackTrace
    Write-Host ""
    
    # Limpiar archivos parciales
    if (Test-Path ".\publish") {
        Write-ColorOutput "Limpiando archivos temporales..." "Yellow"
        Remove-Item ".\publish" -Recurse -Force -ErrorAction SilentlyContinue
    }
    
    exit 1
}
