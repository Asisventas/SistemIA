<#
.SYNOPSIS
    Script de migracion automatica para SistemIA
.DESCRIPTION
    Facilita la creacion, aplicacion y reversion de migraciones EF Core
.EXAMPLE
    .\Migrar.ps1                              # Ver estado
    .\Migrar.ps1 -Nombre "MiMigracion"        # Crear migracion
    .\Migrar.ps1 -Aplicar                     # Aplicar pendientes
    .\Migrar.ps1 -Revertir "NombreMigracion"  # Revertir
    .\Migrar.ps1 -Script                      # Generar SQL
    .\Migrar.ps1 -Eliminar                    # Eliminar ultima migracion
#>

param(
    [string]$Nombre,
    [switch]$Aplicar,
    [string]$Revertir,
    [switch]$Script,
    [switch]$Eliminar,
    [switch]$Forzar,
    [switch]$Detalle
)

$ErrorActionPreference = "Stop"
$projectPath = $PSScriptRoot
$csprojFile = "$projectPath\SistemIA.csproj"

# Funciones de escritura con colores
function Write-Info($msg) { Write-Host "[INFO] " -ForegroundColor Cyan -NoNewline; Write-Host $msg }
function Write-Success($msg) { Write-Host "[OK] " -ForegroundColor Green -NoNewline; Write-Host $msg }
function Write-Warn($msg) { Write-Host "[WARN] " -ForegroundColor Yellow -NoNewline; Write-Host $msg }
function Write-Err($msg) { Write-Host "[ERROR] " -ForegroundColor Red -NoNewline; Write-Host $msg }
function Write-Step($msg) { Write-Host "`n>> $msg" -ForegroundColor Magenta }

# Banner
function Show-Banner {
    Write-Host ""
    Write-Host "  ============================================" -ForegroundColor DarkCyan
    Write-Host "    MIGRACIONES EF CORE - SistemIA" -ForegroundColor DarkCyan
    Write-Host "    .NET 8.0 + SQL Server" -ForegroundColor DarkCyan
    Write-Host "  ============================================" -ForegroundColor DarkCyan
    Write-Host ""
}

# Verificar que estamos en el directorio correcto
function Test-ProjectDirectory {
    if (-not (Test-Path $csprojFile)) {
        Write-Err "No se encontro SistemIA.csproj en: $projectPath"
        Write-Info "Ejecuta este script desde la raiz del proyecto"
        return $false
    }
    return $true
}

# Compilar proyecto
function Build-Project {
    Write-Step "Compilando proyecto..."
    
    $buildOutput = & dotnet build "$csprojFile" -c Debug 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Err "Error de compilacion:"
        $buildOutput | Where-Object { $_ -match "error" } | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
        return $false
    }
    
    Write-Success "Compilacion exitosa"
    return $true
}

# Verificar conexion a BD
function Test-DatabaseConnection {
    Write-Step "Verificando conexion a base de datos..."
    
    $dbOutput = & dotnet ef dbcontext info --project "$csprojFile" --no-build 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Err "No se puede conectar a la base de datos"
        if ($Detalle) {
            Write-Host $dbOutput -ForegroundColor Gray
        }
        return $false
    }
    
    Write-Success "Conexion establecida"
    return $true
}

# Obtener migraciones
function Get-Migrations {
    $listOutput = & dotnet ef migrations list --project "$csprojFile" --no-build 2>&1
    $migrations = $listOutput | Where-Object { $_ -match "^\d{14}_" }
    
    $result = @{
        All = @()
        Applied = @()
        Pending = @()
    }
    
    foreach ($m in $migrations) {
        $cleanName = $m -replace "\s*\(Pending\)\s*$", ""
        $isPending = $m -match "\(Pending\)"
        
        $result.All += $cleanName
        if ($isPending) {
            $result.Pending += $cleanName
        } else {
            $result.Applied += $cleanName
        }
    }
    
    return $result
}

# Mostrar estado
function Show-Status {
    Write-Step "Estado de migraciones"
    
    $migrations = Get-Migrations
    
    Write-Host ""
    Write-Host "  Aplicadas: " -NoNewline -ForegroundColor White
    Write-Host "$($migrations.Applied.Count)" -ForegroundColor Green
    
    Write-Host "  Pendientes: " -NoNewline -ForegroundColor White
    if ($migrations.Pending.Count -gt 0) {
        Write-Host "$($migrations.Pending.Count)" -ForegroundColor Yellow
        foreach ($p in $migrations.Pending) {
            Write-Host "    - $p" -ForegroundColor Yellow
        }
    } else {
        Write-Host "0" -ForegroundColor Green
    }
    
    Write-Host ""
    Write-Host "  Ultima aplicada: " -NoNewline -ForegroundColor White
    if ($migrations.Applied.Count -gt 0) {
        Write-Host "$($migrations.Applied[-1])" -ForegroundColor Cyan
    } else {
        Write-Host "(ninguna)" -ForegroundColor Gray
    }
}

# Crear nueva migracion
function New-Migration {
    param([string]$MigrationName)
    
    Write-Step "Creando migracion: $MigrationName"
    
    # Verificar pendientes
    $migrations = Get-Migrations
    if ($migrations.Pending.Count -gt 0 -and -not $Forzar) {
        Write-Warn "Hay $($migrations.Pending.Count) migracion(es) pendiente(s):"
        foreach ($p in $migrations.Pending) {
            Write-Host "    - $p" -ForegroundColor Yellow
        }
        Write-Info "Usa -Aplicar primero, o -Forzar para crear de todos modos"
        return $false
    }
    
    $addOutput = & dotnet ef migrations add $MigrationName --project "$csprojFile" --no-build 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Err "Error al crear migracion:"
        Write-Host $addOutput -ForegroundColor Red
        return $false
    }
    
    # Buscar archivo creado
    $migrationFile = Get-ChildItem "$projectPath\Migrations\*$MigrationName*.cs" -ErrorAction SilentlyContinue | 
                     Where-Object { $_.Name -notmatch "\.Designer\.cs$" } | 
                     Select-Object -First 1
    
    if ($migrationFile) {
        Write-Success "Migracion creada: $($migrationFile.Name)"
        Write-Host ""
        Write-Warn "IMPORTANTE: Revisa el archivo antes de aplicar"
        Write-Info "Archivo: Migrations\$($migrationFile.Name)"
    }
    
    return $true
}

# Aplicar migraciones
function Apply-Migrations {
    Write-Step "Aplicando migraciones pendientes"
    
    $migrations = Get-Migrations
    
    if ($migrations.Pending.Count -eq 0) {
        Write-Success "No hay migraciones pendientes"
        return $true
    }
    
    Write-Info "Migraciones a aplicar:"
    foreach ($p in $migrations.Pending) {
        Write-Host "    - $p" -ForegroundColor Cyan
    }
    
    if (-not $Forzar) {
        Write-Host ""
        $confirm = Read-Host "Aplicar? (S/n)"
        if ($confirm -eq 'n' -or $confirm -eq 'N') {
            Write-Info "Operacion cancelada"
            return $false
        }
    }
    
    Write-Info "Aplicando..."
    $updateOutput = & dotnet ef database update --project "$csprojFile" --no-build 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Err "Error al aplicar migraciones:"
        Write-Host $updateOutput -ForegroundColor Red
        Write-Host ""
        Write-Warn "Sugerencias:"
        Write-Host "  1. Revisa el archivo de migracion en Migrations\" -ForegroundColor Gray
        Write-Host "  2. Genera el script SQL para analizar: .\Migrar.ps1 -Script" -ForegroundColor Gray
        Write-Host "  3. Consulta GUIA_MIGRACIONES_EF_CORE.md para errores comunes" -ForegroundColor Gray
        return $false
    }
    
    Write-Success "Migraciones aplicadas exitosamente"
    return $true
}

# Revertir migracion
function Undo-Migration {
    param([string]$TargetMigration)
    
    Write-Step "Revirtiendo a migracion: $TargetMigration"
    Write-Warn "ADVERTENCIA: Esto puede causar perdida de datos"
    
    if (-not $Forzar) {
        Write-Host ""
        $confirm = Read-Host "Continuar? (escribir 'SI' para confirmar)"
        if ($confirm -ne 'SI') {
            Write-Info "Operacion cancelada"
            return $false
        }
    }
    
    Write-Info "Revirtiendo..."
    $revertOutput = & dotnet ef database update $TargetMigration --project "$csprojFile" --no-build 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Err "Error al revertir:"
        Write-Host $revertOutput -ForegroundColor Red
        return $false
    }
    
    Write-Success "Revertido exitosamente"
    return $true
}

# Eliminar ultima migracion
function Remove-LastMigration {
    Write-Step "Eliminando ultima migracion"
    
    $migrations = Get-Migrations
    if ($migrations.All.Count -eq 0) {
        Write-Warn "No hay migraciones para eliminar"
        return $false
    }
    
    $lastMigration = $migrations.All[-1]
    $isPending = $migrations.Pending -contains $lastMigration
    
    if (-not $isPending -and -not $Forzar) {
        Write-Err "La ultima migracion ($lastMigration) ya esta aplicada"
        if ($migrations.All.Count -gt 1) {
            Write-Info "Primero revierte con: .\Migrar.ps1 -Revertir '$($migrations.All[-2])'"
        }
        return $false
    }
    
    Write-Info "Eliminando: $lastMigration"
    
    $removeOutput = & dotnet ef migrations remove --project "$csprojFile" --no-build 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Err "Error al eliminar:"
        Write-Host $removeOutput -ForegroundColor Red
        return $false
    }
    
    Write-Success "Migracion eliminada"
    return $true
}

# Generar script SQL
function New-SqlScript {
    Write-Step "Generando script SQL idempotente"
    
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $scriptFile = "$projectPath\Migrations\script_$timestamp.sql"
    
    $scriptOutput = & dotnet ef migrations script --idempotent --project "$csprojFile" --no-build -o $scriptFile 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Err "Error al generar script:"
        Write-Host $scriptOutput -ForegroundColor Red
        return $false
    }
    
    Write-Success "Script generado: Migrations\script_$timestamp.sql"
    Write-Info "Este script es seguro para ejecutar en produccion (idempotente)"
    
    return $true
}

# Mostrar ayuda
function Show-Help {
    Write-Host ""
    Write-Host "USO:" -ForegroundColor White
    Write-Host "  .\Migrar.ps1 [opciones]" -ForegroundColor Gray
    Write-Host ""
    Write-Host "OPCIONES:" -ForegroundColor White
    Write-Host "  -Nombre [nombre]     Crear nueva migracion" -ForegroundColor Gray
    Write-Host "  -Aplicar             Aplicar migraciones pendientes" -ForegroundColor Gray
    Write-Host "  -Revertir [nombre]   Revertir hasta la migracion indicada" -ForegroundColor Gray
    Write-Host "  -Script              Generar script SQL idempotente" -ForegroundColor Gray
    Write-Host "  -Eliminar            Eliminar ultima migracion (si no esta aplicada)" -ForegroundColor Gray
    Write-Host "  -Forzar              Saltar confirmaciones" -ForegroundColor Gray
    Write-Host "  -Detalle             Mostrar mas detalles" -ForegroundColor Gray
    Write-Host ""
    Write-Host "EJEMPLOS:" -ForegroundColor White
    Write-Host "  .\Migrar.ps1 -Nombre 'Agregar_Campo_Ventas'" -ForegroundColor Gray
    Write-Host "  .\Migrar.ps1 -Aplicar -Forzar" -ForegroundColor Gray
    Write-Host "  .\Migrar.ps1 -Script" -ForegroundColor Gray
    Write-Host ""
}

# === MAIN ===

Set-Location $projectPath
Show-Banner

if (-not (Test-ProjectDirectory)) { exit 1 }
if (-not (Build-Project)) { exit 1 }
if (-not (Test-DatabaseConnection)) { exit 1 }

$actionTaken = $false

if ($Nombre) {
    $actionTaken = $true
    if (-not (New-Migration -MigrationName $Nombre)) { exit 1 }
}

if ($Aplicar) {
    $actionTaken = $true
    if (-not (Apply-Migrations)) { exit 1 }
}

if ($Revertir) {
    $actionTaken = $true
    if (-not (Undo-Migration -TargetMigration $Revertir)) { exit 1 }
}

if ($Eliminar) {
    $actionTaken = $true
    if (-not (Remove-LastMigration)) { exit 1 }
}

if ($Script) {
    $actionTaken = $true
    if (-not (New-SqlScript)) { exit 1 }
}

if (-not $actionTaken) {
    Show-Status
    Show-Help
}

Write-Host ""
