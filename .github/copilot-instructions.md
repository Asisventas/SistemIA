# Instrucciones para GitHub Copilot - SistemIA

## 📋 Descripción del Proyecto
SistemIA es un sistema de gestión empresarial desarrollado en **Blazor Server** con integración a **SIFEN** (Facturación Electrónica de Paraguay - SET).

## 🛠️ Stack Tecnológico
- **Framework:** Blazor Server (.NET 8)
- **ORM:** Entity Framework Core
- **Base de datos:** SQL Server (`SERVERSIS\SQL2022`, BD: `asiswebapp`)
- **UI:** Bootstrap 5 + CSS personalizado con sistema de temas
- **Facturación Electrónica:** SIFEN (Sistema Integrado de Facturación Electrónica - Paraguay)

## 📁 Estructura del Proyecto

```
Models/          → Entidades y modelos de datos
Pages/           → Páginas Razor (CRUD, listados, impresión)
Services/        → Servicios de negocio (SIFEN, impresión, etc.)
Shared/          → Componentes compartidos, layouts, vistas previas
Components/      → Componentes de protección y permisos
Controllers/     → API endpoints (descargas, PDF, impresión)
Migrations/      → Migraciones de EF Core
wwwroot/css/     → Estilos (site.css es el principal)
.ai-docs/        → Documentación técnica de referencia
```

## 📖 Documentación de Referencia
**IMPORTANTE:** Consultar `.ai-docs/` antes de implementar:
- `MODULO_NUEVO_GUIA.md` - Guía completa para crear módulos nuevos
- `PATRONES_CSS.md` - Patrones CSS y sistema de temas
- `GUIA_MIGRACIONES_EF_CORE.md` - Migraciones Entity Framework
- `PUBLICACION_DEPLOY.md` - Publicación y problemas de cultura/decimales
- `FLEXBOX_SCROLL_SIDEBAR.md` - Solución para scroll en sidebar

## 🔑 Convenciones de Código

### Idioma
- **Nombres de variables, métodos, clases:** Español
- **Comentarios:** Español
- **Nombres de tablas y columnas:** Español

### Modelos
- PK con prefijo `Id` + Entidad: `IdCliente`, `IdVenta`, `IdProducto`
- El modelo `Usuario` usa `Id_Usu` como PK (excepción histórica)
- Contraseñas: `ContrasenaHash` (SHA256)
- Usar `[Column(TypeName = "decimal(18,4)")]` para montos
- Agrupar propiedades con comentarios: `// ========== SECCIÓN ==========`

### Páginas Razor
- CRUD principal: `[Modulo].razor`
- Listado/Explorador: `[Modulo]Explorar.razor`
- Impresión: `[Modulo]Imprimir.razor`
- Vista previa: `[Modulo]VistaPrevia.razor` en Shared/

### CSS
- Usar variables de tema: `var(--bg-surface)`, `var(--text-primary)`
- Estilos globales en `wwwroot/css/site.css`
- Temas soportados: tenue (default), claro, oscuro

## ⚙️ Configuración

### Puertos de desarrollo
- **HTTP:** `http://localhost:5095`
- **HTTPS:** `https://localhost:7060`

### Contraseñas importantes
- Certificado instalador (PFX): `SistemIA2024!`
- Certificado mkcert: `changeit`

## 🧾 SIFEN (Facturación Electrónica)

### Tipos de Documentos
- Factura Electrónica (FE)
- Nota de Crédito Electrónica (NCE)
- Nota de Débito Electrónica (NDE)
- Autofactura Electrónica (AFE)
- Nota de Remisión Electrónica (NRE)

### Estructura XML
- Seguir estrictamente la especificación del SET
- Namespace: `http://ekuatia.set.gov.py/sifen/xsd`
- Los servicios SIFEN están en `Services/`

### Campos SIFEN comunes
- `CDC` - Código de Control (44 caracteres)
- `IdLote` - Identificador de lote enviado
- `EstadoSifen` - Estado del documento en SIFEN
- `MensajeSifen` - Mensaje de respuesta del SET

## 🗃️ Entity Framework Core

### Comandos frecuentes
```powershell
# Agregar migración
dotnet ef migrations add NombreMigracion --no-build

# Aplicar migración
dotnet ef database update --no-build

# Remover última migración
dotnet ef migrations remove
```

### Convenciones
- Nombres de migración descriptivos en español
- Verificar que el proyecto compile antes de `--no-build`
- Revisar `.ai-docs/GUIA_MIGRACIONES_EF_CORE.md` para casos especiales

## ⚠️ Consideraciones Importantes

1. **Decimales en publicación:** Usar cultura invariante para evitar problemas con separador decimal
2. **Usuario.Id_Usu:** NO usar "Id" para el modelo Usuario
3. **Scroll en sidebar:** Usar patrón flexbox documentado
4. **Permisos:** Sistema de permisos con componentes `RequirePermission.razor` y `PageProtection.razor`

## 🚀 Tareas Disponibles (tasks.json)
- `build` - Compilar proyecto
- `watch` - Ejecutar con hot reload
- `Run Blazor Server (watch)` - Ejecutar en modo desarrollo
- Varias tareas para migraciones EF Core
