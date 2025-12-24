# 📦 Instalador de SistemIA

## Descripción

Este instalador permite configurar y desplegar SistemIA como un servicio de Windows con inicio automático.

## Archivos Incluidos

| Archivo | Descripción |
|---------|-------------|
| `Instalar.bat` | Ejecutable principal del instalador (doble clic) |
| `Install-SistemIA.ps1` | Script PowerShell principal con menú interactivo |
| `config.json` | Configuración de la instalación |
| `SistemIA_Base.bak` | Backup de base de datos limpia para restaurar |
| `LimpiarDatos.sql` | Script SQL para limpiar datos del sistema |
| `CrearBaseDatos.sql` | Script SQL alternativo para crear estructura |
| `InicializarDatos.sql` | Script SQL alternativo para datos iniciales |

## Requisitos Previos

- Windows 10/11 o Windows Server 2016+
- .NET 8.0 Runtime (se instala automáticamente si no existe)
- SQL Server 2017+ (Express, Standard o Enterprise)
- Permisos de Administrador

## Instalación Rápida

1. **Ejecutar como Administrador**: Haga clic derecho en `Instalar.bat` y seleccione "Ejecutar como administrador"

2. **Seguir el menú interactivo**:
   - Opción 1: Instalación completa (recomendado para primera instalación)
   - Opción 2: Solo configurar servidor y base de datos
   - Opción 3: Instalar servicio de Windows
   - Opción 5: Crear/Restaurar base de datos

## Menú de Opciones

```
┌─────────────────────────────────────┐
│         MENÚ DE INSTALACIÓN         │
├─────────────────────────────────────┤
│  1. Instalación completa            │
│  2. Solo configurar servidor/BD     │
│  3. Instalar servicio Windows       │
│  4. Desinstalar servicio            │
│  5. Crear/Restaurar base de datos   │
│  6. Limpiar datos del sistema       │
│  7. Ver configuración actual        │
│  8. Probar conexión a BD            │
│  0. Salir                           │
└─────────────────────────────────────┘
```

## Restauración de Base de Datos

La **opción 5** permite crear/restaurar la base de datos usando dos métodos:

### Método 1: Restaurar desde Backup (Recomendado) ⭐

Restaura la base de datos desde el archivo `SistemIA_Base.bak`:
- ✅ Más rápido y confiable
- ✅ Estructura exacta del sistema de desarrollo
- ✅ Incluye todos los datos iniciales necesarios
- ⚠️ Sobrescribe cualquier BD existente con el mismo nombre

### Método 2: Crear desde Scripts SQL

Ejecuta los scripts `CrearBaseDatos.sql` e `InicializarDatos.sql`:
- ✅ Útil si el backup no funciona
- ✅ Permite personalizar durante la creación
- ⚠️ Puede ser más lento

## Configuración

### config.json

```json
{
  "Instalacion": {
    "RutaInstalacion": "C:\\SistemIA",
    "NombreServicio": "SistemIA",
    "PuertoHttp": 5095,
    "PuertoHttps": 7060,
    "InicioAutomatico": true
  },
  "BaseDatos": {
    "Servidor": ".\\SQLEXPRESS",
    "BaseDatos": "SistemIA_DB",
    "Usuario": "sa",
    "Password": "su_contraseña",
    "AutenticacionWindows": false
  },
  "Sociedad": {
    "Nombre": "Mi Empresa S.A.",
    "RUC": "80000000-0",
    "Direccion": "Dirección"
  }
}
```

### Parámetros de Instalación

| Parámetro | Descripción | Valor por defecto |
|-----------|-------------|-------------------|
| `RutaInstalacion` | Carpeta donde se instala la aplicación | `C:\SistemIA` |
| `PuertoHttp` | Puerto HTTP del servidor web | `5095` |
| `PuertoHttps` | Puerto HTTPS del servidor web | `7060` |
| `InicioAutomatico` | Iniciar servicio con Windows | `true` |

### Parámetros de Base de Datos

| Parámetro | Descripción | Ejemplo |
|-----------|-------------|---------|
| `Servidor` | Nombre del servidor SQL | `.\SQLEXPRESS`, `servidor\instancia` |
| `BaseDatos` | Nombre de la base de datos | `SistemIA_DB` |
| `AutenticacionWindows` | Usar autenticación integrada | `true`/`false` |

## Limpieza de Datos

La opción 6 del menú permite limpiar todos los datos del sistema manteniendo:

- ✅ **Proveedor ID 1**: Proveedor General
- ✅ **Cliente ID 1**: CONSUMIDOR FINAL
- ✅ **Usuario ID 1**: Administrador
- ✅ **Sociedad ID 1**: Datos genéricos
- ✅ **Sucursal ID 1**: Sucursal Principal
- ✅ **Caja ID 1**: Caja Principal
- ✅ **Depósito ID 1**: Depósito Principal

### Datos que se eliminan:

- ❌ Todas las ventas y detalles
- ❌ Todas las compras y detalles
- ❌ Todos los presupuestos
- ❌ Todos los productos
- ❌ Todos los movimientos de stock
- ❌ Todos los cobros y pagos
- ❌ Todos los timbrados
- ❌ Clientes adicionales (ID > 1)
- ❌ Proveedores adicionales (ID > 1)

## Administración del Servicio

### Comandos útiles (PowerShell como Administrador):

```powershell
# Ver estado del servicio
Get-Service SistemIA

# Iniciar servicio
Start-Service SistemIA

# Detener servicio
Stop-Service SistemIA

# Reiniciar servicio
Restart-Service SistemIA

# Ver logs del servicio
Get-EventLog -LogName Application -Source SistemIA -Newest 20
```

### Usando sc.exe (CMD como Administrador):

```cmd
:: Ver estado
sc query SistemIA

:: Iniciar
sc start SistemIA

:: Detener
sc stop SistemIA

:: Eliminar servicio
sc delete SistemIA
```

## Acceso al Sistema

Después de la instalación, acceda al sistema desde cualquier navegador:

- **HTTP**: `http://localhost:5095` o `http://[IP_SERVIDOR]:5095`
- **HTTPS**: `https://localhost:7060` o `https://[IP_SERVIDOR]:7060`

### Acceso desde red local

Para acceder desde otros equipos en la red, use la IP del servidor:
- `http://192.168.x.x:5095`

## Solución de Problemas

### El servicio no inicia

1. Verifique los logs en `C:\SistemIA\logs\`
2. Compruebe que SQL Server esté corriendo
3. Verifique la cadena de conexión en `appsettings.json`

### No se puede conectar a la base de datos

1. Verifique que SQL Server esté iniciado
2. Compruebe el nombre del servidor/instancia
3. Verifique las credenciales
4. Habilite TCP/IP en SQL Server Configuration Manager

### Puerto en uso

1. Cambie los puertos en `config.json`
2. Verifique que no haya otro proceso usando el puerto:
   ```cmd
   netstat -ano | findstr :5095
   ```

### Firewall bloqueando conexiones

El instalador crea automáticamente las reglas de firewall. Si tiene problemas:

```powershell
# Crear regla manualmente
New-NetFirewallRule -DisplayName "SistemIA HTTP" -Direction Inbound -Protocol TCP -LocalPort 5095 -Action Allow
New-NetFirewallRule -DisplayName "SistemIA HTTPS" -Direction Inbound -Protocol TCP -LocalPort 7060 -Action Allow
```

## Desinstalación

1. Ejecute `Instalar.bat` como Administrador
2. Seleccione la opción 4 "Desinstalar servicio"
3. Elimine manualmente la carpeta `C:\SistemIA` si desea

---

## Soporte

Para soporte técnico, contacte al equipo de desarrollo.

**SistemIA** - Sistema de Gestión Empresarial  
© 2025 - Todos los derechos reservados
