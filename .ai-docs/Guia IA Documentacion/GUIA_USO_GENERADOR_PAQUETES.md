# 📦 Guía de Uso: Generador de Paquetes de Actualización

## 🎯 Resumen

Ahora puedes **generar paquetes de actualización** directamente desde la interfaz web del sistema, sin necesidad de usar scripts PowerShell o línea de comandos.

## 🚀 Acceso

1. Inicia sesión en el sistema
2. Ve a **Configuración → Actualización del Sistema**
3. Haz clic en la pestaña **"Generar Paquete"**

## 📋 Proceso de Generación

### Paso 1: Completar Información

Completa los siguientes campos:

**1. Versión del Paquete**
- Formato: `X.Y.Z` (ej: `1.2.0`, `2.0.1`)
- Usa versionado semántico:
  - **X** (Major): Cambios importantes o incompatibles
  - **Y** (Minor): Nuevas características compatibles
  - **Z** (Patch): Correcciones de bugs

**2. Descripción de Cambios**
- Detalla **todas** las modificaciones incluidas:
  ```
  NUEVAS CARACTERÍSTICAS:
  - Módulo de gestión de inventario
  - Reportes de ventas por período
  
  MEJORAS:
  - Optimización de consultas en módulo de clientes
  - Mejora en interfaz de facturación
  
  CORRECCIONES:
  - Fix: Error al guardar productos sin stock
  - Fix: Cálculo incorrecto de IVA en notas de crédito
  
  BASE DE DATOS:
  - Nueva migración: Agregar_Campo_CodigoBarras
  ```

### Paso 2: Generar

1. Haz clic en **"Generar Paquete"**
2. Confirma la operación
3. Espera mientras el sistema:
   - ✓ Limpia compilaciones previas
   - ✓ Compila en modo Release
   - ✓ Publica la aplicación
   - ✓ Verifica archivos críticos
   - ✓ Crea el archivo ZIP
   - ✓ Calcula hash SHA256
   - ✓ Genera CHANGELOG automático

### Paso 3: Resultado

Al finalizar, verás:
- ✅ **Nombre del archivo**: `SistemIA_Update_v1.2.0_20251216_1430.zip`
- ✅ **Ubicación**: `C:\asis\SistemIA\Releases\`
- ✅ **Tamaño**: Ej. 45.67 MB
- ✅ **Hash SHA256**: Para verificar integridad después de transferir
- ✅ **CHANGELOG**: Archivo `.txt` con toda la información

### Paso 4: Distribución

Haz clic en **"Abrir Carpeta Releases"** y encontrarás:

```
📁 Releases/
├── 📦 SistemIA_Update_v1.2.0_20251216_1430.zip
├── 📄 SistemIA_Update_v1.2.0_20251216_1430.zip.sha256
└── 📄 CHANGELOG_v1.2.0.txt
```

**Transfiere estos 3 archivos al cliente:**

#### Opción A: USB/Disco Externo
```powershell
# En el servidor de origen
Copy-Item "C:\asis\SistemIA\Releases\*v1.2.0*" -Destination "E:\Actualizaciones\" -Force

# En el servidor destino, verificar integridad
$esperado = Get-Content "E:\Actualizaciones\SistemIA_Update_v1.2.0_20251216_1430.zip.sha256" | Select-Object -First 1 | ForEach-Object { $_.Split(' ')[0] }
$actual = (Get-FileHash "E:\Actualizaciones\SistemIA_Update_v1.2.0_20251216_1430.zip").Hash
if ($esperado -eq $actual) { Write-Host "✓ Archivo íntegro" -ForegroundColor Green } else { Write-Host "✗ Archivo corrupto" -ForegroundColor Red }
```

#### Opción B: Red Local
```powershell
# Copiar a carpeta compartida
Copy-Item "C:\asis\SistemIA\Releases\*v1.2.0*" -Destination "\\servidor-cliente\Compartida\Actualizaciones\" -Force
```

#### Opción C: Correo/Nube
- **Correo**: Si el archivo es < 25 MB (Gmail) o < 50 MB (Outlook)
- **OneDrive/Dropbox**: Para archivos más grandes
- **IMPORTANTE**: Siempre verifica el hash SHA256 después de descargar

## 🔧 Aplicar la Actualización

Una vez transferido el paquete al servidor cliente:

### Método 1: Interfaz Web (Recomendado)
1. En el servidor cliente, ve a **Configuración → Actualización del Sistema**
2. Pestaña **"Aplicar Actualización"**
3. Selecciona el archivo ZIP
4. Marca **"Aplicar migraciones"** si el CHANGELOG indica cambios en BD
5. Haz clic en **"Iniciar Actualización"**
6. Espera a que complete (NO cerrar navegador)
7. Reinicia la aplicación cuando se indique

### Método 2: Script PowerShell
```powershell
# Ejecutar como Administrador
cd C:\Apps\SistemIA
.\Scripts\ActualizarSistemIA.ps1 -ArchivoZip "C:\Temp\SistemIA_Update_v1.2.0_20251216_1430.zip"
```

## 📊 Información del Proceso

### ¿Qué Incluye el Paquete?

El ZIP generado contiene:
- ✅ Todos los archivos compilados (.dll, .exe)
- ✅ Archivos de configuración base (appsettings.json)
- ✅ Vistas Razor compiladas
- ✅ Archivos estáticos (CSS, JS, imágenes)
- ✅ Dependencias NuGet necesarias
- ✅ Runtime configuration

### ¿Qué NO Incluye?

❌ Configuración del cliente (connection strings)  
❌ Base de datos (solo se aplican migraciones)  
❌ Logs existentes  
❌ Certificados personalizados  
❌ Archivos de usuario (uploads, reportes)

### Verificación de Archivos Críticos

El generador verifica automáticamente:
- ✓ SistemIA.dll (aplicación principal)
- ✓ SistemIA.exe (ejecutable)
- ✓ appsettings.json (configuración base)
- ✓ SistemIA.deps.json (dependencias)
- ✓ SistemIA.runtimeconfig.json (configuración de runtime)

Si falta alguno, la generación se detiene con error.

## ⚠️ Consideraciones Importantes

### Antes de Generar

- [ ] **Compilar y probar** localmente primero
- [ ] **Commit de git** de todos los cambios
- [ ] **Documentar cambios** en detalle
- [ ] **Probar migraciones** en BD de desarrollo
- [ ] **Incrementar versión** correctamente

### Durante la Generación

- ⏳ **No cerrar navegador** durante el proceso
- ⏳ **Esperar a completar** (puede tardar 2-5 minutos)
- ⏳ **No modificar código** mientras genera

### Después de Generar

- [ ] **Revisar CHANGELOG** generado automáticamente
- [ ] **Editar CHANGELOG** si es necesario (agregar detalles específicos)
- [ ] **Probar en entorno de pruebas** antes de producción
- [ ] **Verificar hash SHA256** después de transferir
- [ ] **Guardar copia** del paquete para historial

## 🛡️ Seguridad y Backups

### Backups Automáticos

Al aplicar la actualización, se crean automáticamente:
- 💾 **Backup de aplicación**: ZIP completo del estado anterior
- 💾 **Backup de base de datos**: Archivo .bak de SQL Server

Ubicación: `C:\Backups\SistemIA\`

### Rollback

Si algo sale mal durante la actualización:
1. El sistema **intenta rollback automático**
2. Si falla, los backups están disponibles para **restauración manual**
3. Consulta `MODULO_ACTUALIZACION_README.md` para procedimientos de rollback

## 📞 Checklist de Actualización

### Pre-Actualización
- [ ] Notificar a usuarios del mantenimiento programado
- [ ] Programar en horario de baja actividad
- [ ] Backup manual adicional (recomendado)
- [ ] Verificar espacio en disco suficiente
- [ ] Leer CHANGELOG completamente

### Durante Actualización
- [ ] No interrumpir el proceso
- [ ] Monitorear logs en tiempo real
- [ ] Anotar cualquier advertencia o error

### Post-Actualización
- [ ] Verificar que servicio/aplicación inició correctamente
- [ ] Probar login y funciones principales
- [ ] Revisar logs de errores
- [ ] Confirmar que migraciones se aplicaron
- [ ] Notificar a usuarios que sistema está disponible
- [ ] Monitorear por 24-48 horas

## 🔄 Versionado Recomendado

### Convención de Versiones

```
v1.2.3
│ │ └─ PATCH (Z): Correcciones de bugs, sin nuevas características
│ └─── MINOR (Y): Nuevas características, compatibles con versión anterior
└───── MAJOR (X): Cambios importantes, pueden romper compatibilidad
```

### Ejemplos

- `1.0.0` → `1.0.1`: Fix de bug en cálculo de IVA
- `1.0.1` → `1.1.0`: Nuevo módulo de reportes
- `1.1.0` → `2.0.0`: Reestructuración completa de base de datos

## 💡 Consejos y Mejores Prácticas

1. **Probar Siempre en Desarrollo**
   - Nunca generes un paquete sin probar localmente

2. **Documentación Completa**
   - Cuanto más detallado el CHANGELOG, mejor
   - Incluye instrucciones especiales si las hay

3. **Versionado Consistente**
   - Sigue siempre el mismo formato
   - Registra versiones en control de versiones (git tags)

4. **Comunicación con Cliente**
   - Envía CHANGELOG antes de actualizar
   - Coordina horario conveniente
   - Ofrece soporte durante actualización

5. **Historial de Paquetes**
   - Guarda una copia de cada paquete generado
   - Útil para rollback o auditorías

6. **Verificación de Integridad**
   - Siempre verifica hash SHA256 después de transferir
   - No uses archivos con hash incorrecto

## 🎓 Recursos Adicionales

- **Documentación completa**: `MODULO_ACTUALIZACION_README.md`
- **Guía para desarrolladores**: `GUIA_CREAR_PAQUETE_ACTUALIZACION.md`
- **Plan de implementación**: `PLAN_IMPLEMENTACION.md`
- **Script PowerShell alternativo**: `Scripts/CrearPaqueteActualizacion.ps1`

## 📝 Ejemplo Completo

### Escenario: Actualización v1.3.0

**1. Generar Paquete**
```
Versión: 1.3.0
Descripción:
  NUEVAS CARACTERÍSTICAS:
  - Sistema de notificaciones por email
  - Exportación de reportes a Excel
  
  MEJORAS:
  - Interfaz de productos rediseñada
  - Búsqueda más rápida de clientes
  
  CORRECCIONES:
  - Fix: Error al eliminar productos con stock
  - Fix: Cálculo de descuentos en facturas
  
  BASE DE DATOS:
  - Migración: Agregar_Tabla_Notificaciones
  - Migración: Agregar_Indices_Rendimiento
```

**2. Resultado**
```
✓ Paquete generado: SistemIA_Update_v1.3.0_20251216_1500.zip
✓ Tamaño: 52.34 MB
✓ Hash: a3f5c2d8e1b4f7c9...
✓ CHANGELOG: CHANGELOG_v1.3.0.txt
```

**3. Transferir**
```powershell
# Copiar a USB
Copy-Item "C:\asis\SistemIA\Releases\*v1.3.0*" -Destination "E:\" -Force
```

**4. Aplicar en Cliente**
```
- Acceder a interfaz web
- Subir SistemIA_Update_v1.3.0_20251216_1500.zip
- Marcar "Aplicar migraciones" ✓
- Iniciar actualización
- Esperar 5-10 minutos
- Reiniciar aplicación
```

**5. Verificar**
```
✓ Sistema inicia correctamente
✓ Versión mostrada: 1.3.0
✓ Nuevas funcionalidades visibles
✓ No hay errores en logs
```

---

## 🆘 Soporte

Si encuentras problemas:

1. **Revisa los logs** en tiempo real durante la generación
2. **Consulta CHANGELOG** para detalles específicos
3. **Verifica requisitos** (espacio, permisos, etc.)
4. **Prueba en desarrollo** antes de producción
5. **Contacta soporte** si el problema persiste

---

**Última actualización**: 16/12/2025  
**Versión de la guía**: 1.0  
**Aplica a**: SistemIA v1.0+
