# Guía de Migraciones EF Core - SistemIA

## Índice
1. [Errores Comunes y Soluciones](#errores-comunes-y-soluciones)
2. [Script de Migración Automática](#script-de-migración-automática)
3. [Buenas Prácticas](#buenas-prácticas)
4. [Plantillas de Código](#plantillas-de-código)

---

## Errores Comunes y Soluciones

### 1. Error: Nombre de tabla incorrecto (pluralización)

**Síntoma:**
```
Invalid object name 'ComposicionCaja'
```

**Causa:** EF Core pluraliza los nombres de tabla por defecto.

**Solución:** Verificar el nombre real de la tabla en el DbContext o usar:
```csharp
// En el modelo
[Table("NombreExactoTabla")]
public class MiEntidad { }

// O en OnModelCreating
modelBuilder.Entity<MiEntidad>().ToTable("NombreExactoTabla");
```

**Verificar nombre de tabla:**
```sql
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME LIKE '%Composicion%'
```

---

### 2. Error: Constraint DEFAULT impide eliminar columna

**Síntoma:**
```
El objeto 'DF__Tabla__Columna__XXXXX' de tipo objeto es dependiente de columna 'NombreColumna'
```

**Causa:** SQL Server crea constraints DEFAULT automáticos para columnas con valores por defecto.

**Solución en el método Down:**
```csharp
protected override void Down(MigrationBuilder migrationBuilder)
{
    // Primero eliminar el constraint DEFAULT dinámicamente
    migrationBuilder.Sql(@"
        DECLARE @constraintName NVARCHAR(200);
        SELECT @constraintName = dc.name 
        FROM sys.default_constraints dc
        JOIN sys.columns c ON dc.parent_column_id = c.column_id AND dc.parent_object_id = c.object_id
        WHERE dc.parent_object_id = OBJECT_ID(N'NombreTabla') AND c.name = 'NombreColumna';
        
        IF @constraintName IS NOT NULL
        BEGIN
            EXEC('ALTER TABLE [NombreTabla] DROP CONSTRAINT [' + @constraintName + ']');
        END
    ");
    
    // Luego eliminar la columna
    migrationBuilder.DropColumn(name: "NombreColumna", table: "NombreTabla");
}
```

---

### 3. Error: Migración aplicada parcialmente

**Síntoma:**
- La migración aparece en `__EFMigrationsHistory` pero las columnas/tablas no existen
- O viceversa: las columnas existen pero la migración no está registrada

**Solución A - Columnas existen pero migración no registrada:**
```sql
INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
VALUES ('20251222020630_NombreMigracion', '8.0.0');
```

**Solución B - Migración registrada pero columnas no existen:**
```sql
DELETE FROM __EFMigrationsHistory WHERE MigrationId = '20251222020630_NombreMigracion';
```
Luego ejecutar: `dotnet ef database update`

---

### 4. Error: Migración vacía o sin cambios

**Síntoma:**
```
An operation was scaffolded that may result in the loss of data
```
O la migración Up/Down están vacíos.

**Causa:** El modelo no cambió o hay discrepancia entre el snapshot y la BD.

**Solución:**
```powershell
# Eliminar última migración
dotnet ef migrations remove

# Regenerar snapshot
dotnet ef migrations add NombreMigracion --force
```

---

### 5. Error: Índice duplicado o ya existe

**Síntoma:**
```
Cannot create index 'IX_Tabla_Columna' because it already exists
```

**Solución - Hacer idempotente:**
```csharp
migrationBuilder.Sql(@"
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'Tabla') AND name = 'IX_Tabla_Columna')
    BEGIN
        CREATE INDEX [IX_Tabla_Columna] ON [Tabla]([Columna]);
    END
");
```

---

### 6. Error: Foreign Key constraint violation

**Síntoma:**
```
The ALTER TABLE statement conflicted with the FOREIGN KEY constraint
```

**Solución:** Agregar datos de referencia primero o hacer la FK nullable:
```csharp
migrationBuilder.AddColumn<int?>(
    name: "IdReferencia",
    table: "Tabla",
    type: "int",
    nullable: true);  // Nullable primero

// Luego actualizar datos existentes
migrationBuilder.Sql("UPDATE Tabla SET IdReferencia = 1 WHERE IdReferencia IS NULL");

// Finalmente hacer NOT NULL si es necesario
migrationBuilder.AlterColumn<int>(
    name: "IdReferencia",
    table: "Tabla",
    type: "int",
    nullable: false);
```

---

## Script de Migración Automática

Guarda este script como `Migrar.ps1` en la raíz del proyecto:

```powershell
# Migrar.ps1 - Script de migración automática para SistemIA
# Uso: .\Migrar.ps1 -Nombre "NombreMigracion"
# Uso: .\Migrar.ps1 -Aplicar (solo aplica migraciones pendientes)
# Uso: .\Migrar.ps1 -Revertir "NombreMigracion" (revierte hasta esa migración)
# Uso: .\Migrar.ps1 -Script (genera SQL sin ejecutar)

param(
    [string]$Nombre,
    [switch]$Aplicar,
    [string]$Revertir,
    [switch]$Script,
    [switch]$Forzar
)

$ErrorActionPreference = "Stop"
$projectPath = $PSScriptRoot

function Write-Info($msg) { Write-Host "[INFO] $msg" -ForegroundColor Cyan }
function Write-Success($msg) { Write-Host "[OK] $msg" -ForegroundColor Green }
function Write-Warn($msg) { Write-Host "[WARN] $msg" -ForegroundColor Yellow }
function Write-Err($msg) { Write-Host "[ERROR] $msg" -ForegroundColor Red }

# Función para verificar estado de la BD
function Test-DatabaseConnection {
    Write-Info "Verificando conexión a base de datos..."
    $result = dotnet ef dbcontext info --no-build 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Err "No se puede conectar a la base de datos"
        return $false
    }
    Write-Success "Conexión OK"
    return $true
}

# Función para listar migraciones pendientes
function Get-PendingMigrations {
    Write-Info "Verificando migraciones pendientes..."
    $migrations = dotnet ef migrations list --no-build 2>&1 | Where-Object { $_ -match "^\d{14}_" }
    $pending = $migrations | Where-Object { $_ -match "\(Pending\)$" }
    return $pending
}

# Compilar proyecto
function Build-Project {
    Write-Info "Compilando proyecto..."
    dotnet build --no-restore -c Debug 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        dotnet build -c Debug
        if ($LASTEXITCODE -ne 0) {
            Write-Err "Error de compilación"
            exit 1
        }
    }
    Write-Success "Compilación exitosa"
}

# Crear backup del historial de migraciones
function Backup-MigrationHistory {
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $backupFile = "$projectPath\Migrations\backup_history_$timestamp.sql"
    
    Write-Info "Creando backup del historial de migraciones..."
    $sql = "SELECT * FROM __EFMigrationsHistory"
    # Este es un backup conceptual - ajustar según tu conexión
    Write-Warn "Backup guardado en: $backupFile (conceptual)"
}

# --- MAIN ---

Set-Location $projectPath
Write-Host "`n========================================" -ForegroundColor Magenta
Write-Host "   MIGRACIONES EF CORE - SistemIA" -ForegroundColor Magenta
Write-Host "========================================`n" -ForegroundColor Magenta

# Siempre compilar primero
Build-Project

if (-not (Test-DatabaseConnection)) {
    exit 1
}

# Modo: Crear nueva migración
if ($Nombre) {
    Write-Info "Creando migración: $Nombre"
    
    # Verificar si hay cambios pendientes
    $pending = Get-PendingMigrations
    if ($pending -and -not $Forzar) {
        Write-Warn "Hay migraciones pendientes por aplicar:"
        $pending | ForEach-Object { Write-Host "  - $_" -ForegroundColor Yellow }
        Write-Warn "Use -Forzar para crear la migración de todos modos, o -Aplicar primero"
        exit 1
    }
    
    dotnet ef migrations add $Nombre --no-build
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Migración '$Nombre' creada exitosamente"
        
        # Mostrar archivo creado
        $migrationFile = Get-ChildItem "$projectPath\Migrations\*$Nombre*.cs" | Select-Object -First 1
        if ($migrationFile) {
            Write-Info "Archivo: $($migrationFile.Name)"
            Write-Warn "IMPORTANTE: Revisa el archivo antes de aplicar la migración"
        }
    } else {
        Write-Err "Error al crear migración"
        exit 1
    }
}

# Modo: Aplicar migraciones
if ($Aplicar) {
    $pending = Get-PendingMigrations
    if (-not $pending) {
        Write-Success "No hay migraciones pendientes"
    } else {
        Write-Info "Migraciones a aplicar:"
        $pending | ForEach-Object { Write-Host "  - $_" -ForegroundColor Cyan }
        
        Backup-MigrationHistory
        
        Write-Info "Aplicando migraciones..."
        dotnet ef database update --no-build
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Migraciones aplicadas exitosamente"
        } else {
            Write-Err "Error al aplicar migraciones"
            Write-Warn "Revisa el error y considera usar -Script para ver el SQL"
            exit 1
        }
    }
}

# Modo: Revertir a migración específica
if ($Revertir) {
    Write-Warn "Revirtiendo a migración: $Revertir"
    Write-Warn "ESTO PUEDE CAUSAR PÉRDIDA DE DATOS"
    
    $confirm = Read-Host "¿Continuar? (s/N)"
    if ($confirm -ne 's' -and $confirm -ne 'S') {
        Write-Info "Operación cancelada"
        exit 0
    }
    
    Backup-MigrationHistory
    
    dotnet ef database update $Revertir --no-build
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Revertido exitosamente a: $Revertir"
    } else {
        Write-Err "Error al revertir"
        exit 1
    }
}

# Modo: Generar script SQL
if ($Script) {
    $scriptFile = "$projectPath\Migrations\migration_script_$(Get-Date -Format 'yyyyMMdd_HHmmss').sql"
    Write-Info "Generando script SQL idempotente..."
    
    dotnet ef migrations script --idempotent --no-build -o $scriptFile
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Script generado: $scriptFile"
        Write-Info "Revisa el script antes de ejecutarlo manualmente en producción"
    } else {
        Write-Err "Error al generar script"
        exit 1
    }
}

# Si no se especificó ninguna acción, mostrar estado
if (-not $Nombre -and -not $Aplicar -and -not $Revertir -and -not $Script) {
    Write-Info "Estado actual de migraciones:"
    dotnet ef migrations list --no-build
    
    Write-Host "`nUso:" -ForegroundColor White
    Write-Host "  .\Migrar.ps1 -Nombre 'MiMigracion'  # Crear nueva migración" -ForegroundColor Gray
    Write-Host "  .\Migrar.ps1 -Aplicar              # Aplicar pendientes" -ForegroundColor Gray
    Write-Host "  .\Migrar.ps1 -Script               # Generar SQL" -ForegroundColor Gray
    Write-Host "  .\Migrar.ps1 -Revertir 'Migracion' # Revertir" -ForegroundColor Gray
}

Write-Host "`n"
```

---

## Buenas Prácticas

### Antes de crear una migración:
1. ✅ Compilar el proyecto: `dotnet build`
2. ✅ Verificar que no hay migraciones pendientes
3. ✅ Hacer commit de los cambios actuales en Git

### Al crear la migración:
1. ✅ Usar nombres descriptivos: `Agregar_Campo_Tabla` no `Update1`
2. ✅ Revisar el archivo generado antes de aplicar
3. ✅ Hacer las operaciones idempotentes cuando sea posible

### Al aplicar:
1. ✅ En desarrollo: `dotnet ef database update --no-build`
2. ✅ En producción: Generar script y revisar: `dotnet ef migrations script --idempotent`

### Estructura recomendada para migraciones complejas:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // 1. Agregar columnas nuevas (nullable primero si tienen FK)
    migrationBuilder.Sql(@"
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Tabla') AND name = 'NuevaColumna')
        BEGIN
            ALTER TABLE [Tabla] ADD [NuevaColumna] INT NULL;
        END
    ");
    
    // 2. Migrar datos existentes
    migrationBuilder.Sql(@"
        UPDATE [Tabla] SET [NuevaColumna] = [ValorDefault] WHERE [NuevaColumna] IS NULL;
    ");
    
    // 3. Hacer NOT NULL si es necesario
    migrationBuilder.Sql(@"
        ALTER TABLE [Tabla] ALTER COLUMN [NuevaColumna] INT NOT NULL;
    ");
    
    // 4. Crear índices
    migrationBuilder.Sql(@"
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Tabla_NuevaColumna')
        BEGIN
            CREATE INDEX [IX_Tabla_NuevaColumna] ON [Tabla]([NuevaColumna]);
        END
    ");
}
```

---

## Plantillas de Código

### Plantilla: Agregar columna con valor default
```csharp
// Up
migrationBuilder.Sql(@"
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[TABLA]') AND name = '[COLUMNA]')
    BEGIN
        ALTER TABLE [TABLA] ADD [COLUMNA] [TIPO] NOT NULL DEFAULT [VALOR];
    END
");

// Down
migrationBuilder.Sql(@"
    DECLARE @c NVARCHAR(200);
    SELECT @c = dc.name FROM sys.default_constraints dc
    JOIN sys.columns c ON dc.parent_column_id = c.column_id AND dc.parent_object_id = c.object_id
    WHERE dc.parent_object_id = OBJECT_ID(N'[TABLA]') AND c.name = '[COLUMNA]';
    IF @c IS NOT NULL EXEC('ALTER TABLE [TABLA] DROP CONSTRAINT [' + @c + ']');
    
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[TABLA]') AND name = '[COLUMNA]')
        ALTER TABLE [TABLA] DROP COLUMN [COLUMNA];
");
```

### Plantilla: Crear índice único
```csharp
// Up
migrationBuilder.Sql(@"
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[TABLA]') AND name = '[IX_NOMBRE]')
    BEGIN
        CREATE UNIQUE INDEX [IX_NOMBRE] ON [TABLA]([COL1], [COL2], [COL3]);
    END
");

// Down
migrationBuilder.Sql(@"
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[TABLA]') AND name = '[IX_NOMBRE]')
        DROP INDEX [IX_NOMBRE] ON [TABLA];
");
```

### Plantilla: Renombrar columna
```csharp
// Up
migrationBuilder.Sql(@"
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[TABLA]') AND name = '[NOMBRE_VIEJO]')
    AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[TABLA]') AND name = '[NOMBRE_NUEVO]')
    BEGIN
        EXEC sp_rename '[TABLA].[NOMBRE_VIEJO]', '[NOMBRE_NUEVO]', 'COLUMN';
    END
");

// Down
migrationBuilder.Sql(@"
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[TABLA]') AND name = '[NOMBRE_NUEVO]')
    AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[TABLA]') AND name = '[NOMBRE_VIEJO]')
    BEGIN
        EXEC sp_rename '[TABLA].[NOMBRE_NUEVO]', '[NOMBRE_VIEJO]', 'COLUMN';
    END
");
```

---

## Comandos Rápidos

```powershell
# Ver estado de migraciones
dotnet ef migrations list

# Crear migración
dotnet ef migrations add NombreMigracion

# Aplicar todas las pendientes
dotnet ef database update

# Revertir a una migración específica
dotnet ef database update NombreMigracionAnterior

# Eliminar última migración (solo si no está aplicada)
dotnet ef migrations remove

# Generar script SQL para producción
dotnet ef migrations script --idempotent -o script.sql

# Ver SQL que generaría una migración específica
dotnet ef migrations script MigracionAnterior MigracionNueva
```

---

*Documento creado: 21 de diciembre de 2025*
*Proyecto: SistemIA - Blazor Server .NET 8.0*
