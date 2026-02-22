# SistemIA MAUI Blazor Hybrid - Estado del Proyecto

## Resumen

Se ha creado la estructura de proyectos para soportar una aplicación móvil MAUI Blazor Hybrid que comparte código con la aplicación web.

## Estructura de Proyectos

```
c:\asis\
├── SistemIA/              # Blazor Server (Web) - .NET 9
├── SistemIA.Shared/       # Biblioteca compartida - .NET 9
└── SistemIA.Mobile/       # MAUI Blazor Hybrid - .NET 9
```

## Estado Actual

| Proyecto | Estado | Compila |
|----------|--------|---------|
| SistemIA (Web) | ✅ Funcional | ✅ 0 errores |
| SistemIA.Shared | ✅ Creado | ✅ 0 errores |
| SistemIA.Mobile | ⚠️ Pendiente SDK | ❌ Falta android-35 |

## Cambios Realizados

### 1. Actualización a .NET 9
- `SistemIA.csproj` actualizado de `net8.0` a `net9.0`
- Paquetes EF Core actualizados a `9.0.0`
- `System.Drawing.Common` actualizado a `9.0.0`
- `Microsoft.Extensions.Hosting.WindowsServices` actualizado a `9.0.0`

### 2. Proyecto SistemIA.Shared
- Biblioteca de clases Razor para componentes compartidos
- Target: `net9.0`
- Referenciado por SistemIA y SistemIA.Mobile

### 3. Proyecto SistemIA.Mobile
- Plantilla MAUI Blazor Hybrid
- Targets: `net9.0-android`, `net9.0-ios`, `net9.0-maccatalyst`, `net9.0-windows`
- Referencia a SistemIA.Shared agregada

## Pendiente: Instalar Android SDK 35

Para compilar el proyecto móvil, necesitas instalar Android SDK Platform 35:

### Opción 1: Desde Visual Studio
1. Abrir Visual Studio Installer
2. Modificar → Componentes individuales
3. Buscar "Android SDK Platform 35"
4. Instalar

### Opción 2: Desde Android Studio
1. SDK Manager → SDK Platforms
2. Instalar "Android 15 (VanillaIceCream)" API 35

### Opción 3: Desde línea de comandos (requiere Java 17+)
```cmd
sdkmanager "platforms;android-35"
```

## Próximos Pasos

1. **Instalar Android SDK 35** (ver opciones arriba)
2. **Mover componentes compartidos** a SistemIA.Shared:
   - Páginas que se usen en móvil
   - Modelos de datos
   - Servicios de lógica de negocio
3. **Configurar API endpoints** para comunicación móvil-servidor
4. **Implementar servicios nativos** en SistemIA.Mobile:
   - Push notifications
   - Cámara/escáner
   - Almacenamiento local

## Compilar Proyectos

```powershell
# Compilar proyecto web
cd c:\asis\SistemIA
dotnet build

# Compilar proyecto móvil (después de instalar SDK)
cd c:\asis\SistemIA.Mobile
dotnet build -f net9.0-android
```

## Generar APK

```powershell
cd c:\asis\SistemIA.Mobile
dotnet publish -f net9.0-android -c Release
```

El APK estará en: `bin\Release\net9.0-android\publish\`

---
*Creado: 21 Feb 2026*
