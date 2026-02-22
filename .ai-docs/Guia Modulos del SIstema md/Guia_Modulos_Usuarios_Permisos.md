# Guía del Módulo de Usuarios y Permisos

## Descripción General

El módulo de Usuarios y Permisos gestiona:
- Registro y administración de usuarios del sistema
- Roles con agrupación de permisos
- Permisos granulares por módulo
- Control de acceso a páginas y funciones
- Reconocimiento facial para asistencia
- Datos laborales y nómina básica

---

## Modelo de Datos

### Entidad: `Usuario`

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id_Usu` | int (PK) | Identificador único |
| `Nombres` | string(100) | Nombres (obligatorio) |
| `Apellidos` | string(100) | Apellidos (obligatorio) |
| `CI` | string(15)? | Cédula de Identidad |
| `Direccion` | string(200)? | Dirección |
| `Ciudad` | string(100)? | Ciudad |
| `Telefono` | string(20)? | Teléfono |
| `Correo` | string(150)? | Correo electrónico |
| `Fecha_Nacimiento` | DateTime? | Fecha de nacimiento |
| `Fecha_Ingreso` | DateTime | Fecha de ingreso al sistema |

#### Credenciales
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `UsuarioNombre` | string(50) | Nombre de usuario para login |
| `ContrasenaHash` | byte[] | Contraseña hasheada (SHA256) |
| `Estado_Usu` | bool | Activo/Inactivo |
| `Id_Rol` | int (FK) | Rol asignado |

#### Reconocimiento Facial
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Foto` | byte[]? | Imagen del usuario |
| `EmbeddingFacial` | byte[]? | Vector de reconocimiento facial |
| `HuellaDigital` | byte[]? | Huella digital (opcional) |

#### Datos Laborales
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Salario` | decimal(18,2)? | Salario mensual |
| `IPS` | decimal(18,2)? | Aporte IPS |
| `Comision` | decimal(18,2)? | Comisión (%) |
| `Descuento` | decimal(18,2)? | Otros descuentos |

**Propiedad calculada:**
```csharp
Salario_Neto = Salario + Comision - Descuento - IPS
```

---

### Entidad: `Rol`

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id_Rol` | int (PK) | Identificador único |
| `NombreRol` | string | Nombre del rol (obligatorio) |
| `Descripcion` | string? | Descripción del rol |
| `Estado` | bool | Activo/Inactivo |

**Relaciones:**
- `Usuarios` → Usuarios con este rol
- `PermisosModulos` → Permisos asignados al rol

**Roles típicos:**
| Id | Nombre | Descripción |
|----|--------|-------------|
| 1 | Administrador | Acceso total al sistema |
| 2 | Vendedor | Ventas, cobros, consultas |
| 3 | Cajero | Caja, ventas contado |
| 4 | Almacén | Inventario, compras |
| 5 | Contador | Reportes, SIFEN |

---

### Entidad: `Modulo`

Representa una sección o funcionalidad del sistema.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `IdModulo` | int (PK) | Identificador único |
| `Nombre` | string(100) | Nombre del módulo |
| `Descripcion` | string(500)? | Descripción |
| `Icono` | string(50)? | Icono Bootstrap Icons |
| `Orden` | int? | Orden en menú |
| `IdModuloPadre` | int? (FK) | Módulo padre (jerarquía) |
| `RutaPagina` | string(200)? | Ruta Blazor (ej: "/ventas") |
| `Activo` | bool | Si está activo |
| `FechaCreacion` | DateTime | Fecha de creación |

**Jerarquía de módulos:**
```
📁 Ventas (padre)
    📄 /ventas (crear venta)
    📄 /ventas/explorar (explorador)
    📄 /ventas/presupuestos
📁 Compras (padre)
    📄 /compras
    📄 /compras/explorar
📁 Inventario (padre)
    📄 /productos
    📄 /ajustes-stock
    📄 /transferencias
```

---

### Entidad: `Permiso`

Representa una acción específica.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `IdPermiso` | int (PK) | Identificador único |
| `Nombre` | string(50) | Nombre del permiso |
| `Codigo` | string(50) | Código único (VIEW, CREATE...) |
| `Descripcion` | string(500)? | Descripción |
| `Orden` | int? | Orden de visualización |
| `Activo` | bool | Si está activo |

**Permisos estándar:**
| Código | Nombre | Descripción |
|--------|--------|-------------|
| VIEW | Ver | Consultar y listar |
| CREATE | Crear | Crear nuevos registros |
| EDIT | Editar | Modificar registros |
| DELETE | Eliminar | Eliminar registros |
| EXPORT | Exportar | Exportar a Excel/PDF |
| PRINT | Imprimir | Imprimir documentos |

---

### Entidad: `RolModuloPermiso`

Tabla intermedia que relaciona Roles, Módulos y Permisos.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `IdRolModuloPermiso` | int (PK) | Identificador único |
| `IdRol` | int (FK) | Rol |
| `IdModulo` | int (FK) | Módulo |
| `IdPermiso` | int (FK) | Permiso |
| `Concedido` | bool | Si está concedido |
| `FechaAsignacion` | DateTime | Fecha de asignación |
| `UsuarioAsignacion` | string(100)? | Quién asignó |

**Ejemplo de matriz:**
| Rol | Módulo | VIEW | CREATE | EDIT | DELETE |
|-----|--------|------|--------|------|--------|
| Administrador | /ventas | ✅ | ✅ | ✅ | ✅ |
| Vendedor | /ventas | ✅ | ✅ | ✅ | ❌ |
| Cajero | /ventas | ✅ | ✅ | ❌ | ❌ |

---

## Servicio de Permisos

### `PermisosService`

Servicio central para verificación de permisos.

#### Métodos principales:

**TienePermisoAsync**
```csharp
Task<bool> TienePermisoAsync(int idUsuario, string codigoModulo, string codigoPermiso)
```
Verifica si un usuario tiene un permiso específico en un módulo.

**ObtenerModulosConPermisosAsync**
```csharp
Task<List<ModuloConPermisos>> ObtenerModulosConPermisosAsync(int idRol)
```
Obtiene todos los módulos con sus permisos para un rol (para la matriz de permisos).

**AsignarPermisoAsync**
```csharp
Task<bool> AsignarPermisoAsync(int idRol, int idModulo, int idPermiso, bool conceder, string usuarioAsignacion)
```
Asigna o revoca un permiso para un rol en un módulo.

**ObtenerModulosAccesiblesAsync**
```csharp
Task<List<Modulo>> ObtenerModulosAccesiblesAsync(int idUsuario)
```
Obtiene los módulos a los que un usuario tiene acceso (para menú dinámico).

---

## Componentes de Protección

### `PageProtection`

Componente que protege páginas completas.

**Uso:**
```razor
<PageProtection Modulo="/ventas" Permiso="VIEW">
    <!-- Contenido de la página -->
</PageProtection>
```

**Comportamiento:**
1. Muestra spinner mientras verifica
2. Si tiene acceso → Renderiza contenido
3. Si no tiene acceso → Muestra mensaje de acceso denegado

**Parámetros:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `Modulo` | string | Ruta del módulo (ej: "/ventas") |
| `Permiso` | string | Código del permiso (VIEW, CREATE...) |
| `ChildContent` | RenderFragment | Contenido a proteger |

---

### `RequirePermission`

Componente para ocultar secciones o botones según permisos.

**Uso:**
```razor
<RequirePermission Modulo="/ventas" Permiso="DELETE">
    <button class="btn btn-danger">Eliminar</button>
</RequirePermission>
```

**Comportamiento:**
- Si tiene permiso → Muestra contenido
- Si no tiene permiso → Oculta (o muestra mensaje si `MostrarMensajeDenegado=true`)

**Parámetros:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `Modulo` | string | Ruta del módulo |
| `Permiso` | string | Código del permiso |
| `MostrarMensajeDenegado` | bool | Mostrar alerta si denegado |
| `ChildContent` | RenderFragment | Contenido a proteger |

---

## Páginas del Módulo

### 1. Lista de Usuarios (`/menu-usuarios`)

**Ruta:** `/menu-usuarios`  
**Permiso:** VIEW sobre `/personal`

**Funcionalidad:**
- Tabla paginada de usuarios (15 por página)
- Columnas: Id, Nombres, Apellidos, Correo, Usuario, Rol, Estado
- Botón "Registrar Usuario"
- Botón "Editar" por registro
- Botón "Eliminar" con confirmación SA

**Eliminación protegida:**
- Requiere contraseña del usuario SA de SQL Server
- Evita eliminación accidental de usuarios

### 2. Crear Usuario (`/usuarios/CrearUsuario`)

**Ruta:** `/usuarios/CrearUsuario`

**Secciones:**

#### Datos Personales
- Nombres (obligatorio)
- Apellidos (obligatorio)
- CI
- Fecha de Nacimiento

#### Información de Contacto
- Dirección
- Ciudad
- Teléfono
- Correo Electrónico

#### Credenciales y Rol
- Nombre de Usuario (obligatorio)
- Contraseña (obligatorio, hasheada con SHA256)
- Rol (selección de roles activos)
- Usuario Activo (switch)

#### Reconocimiento Facial
- Subir foto desde archivo
- Capturar con cámara web
- Genera embedding facial automáticamente
- Requisito: Exactamente un rostro en la imagen

#### Datos Laborales
- Salario
- Aporte IPS
- Comisión (%)
- Otros Descuentos

### 3. Editar Usuario (`/usuarios/editarUsu/{id}`)

**Ruta:** `/usuarios/editarUsu/{id:int}`

Mismas secciones que Crear, con:
- Campo contraseña opcional (solo si se desea cambiar)
- Visualización de foto actual
- Botón cancelar para volver

### 4. Permisos de Usuarios (`/personal/permisos-usuarios`)

**Ruta:** `/personal/permisos-usuarios`  
**Permiso:** VIEW sobre `/personal`

**Funcionalidad:**
- Selector de rol a configurar
- Matriz de permisos visual (estilo Excel)
- Checkboxes para cada combinación Módulo-Permiso
- Jerarquía visual: módulos padres e hijos
- Botón "Guardar Cambios"
- Botón "Recargar"

**Estructura de la matriz:**
```
| Módulo              | Ver | Crear | Editar | Eliminar | Exportar | Imprimir |
|---------------------|-----|-------|--------|----------|----------|----------|
| ▼ Ventas            | ☑   | ☑     | ☑      | ☐        | ☑        | ☑        |
|   └ Explorador      | ☑   | ☑     | ☑      | ☐        | ☑        | ☑        |
|   └ Presupuestos    | ☑   | ☑     | ☐      | ☐        | ☑        | ☑        |
| ▼ Compras           | ☑   | ☑     | ☐      | ☐        | ☐        | ☐        |
```

**Nota importante:**
Los cambios no se aplican inmediatamente. Los usuarios con sesiones activas deben cerrar sesión y volver a iniciarla para que surtan efecto.

---

## Flujo de Autenticación

### Login
```
1. Usuario ingresa credenciales
2. Sistema busca usuario por UsuarioNombre
3. Hashea contraseña ingresada (SHA256)
4. Compara con ContrasenaHash almacenado
5. Si coincide y Estado_Usu = true:
   - Crea Claims (NameIdentifier, Name)
   - Establece sesión autenticada
6. Si no → Muestra error
```

### Verificación de Permisos
```
1. Componente PageProtection/RequirePermission se renderiza
2. Obtiene AuthenticationState
3. Extrae IdUsuario de Claims
4. Llama a PermisosService.TienePermisoAsync()
5. Busca en RolesModulosPermisos:
   - Usuario.Id_Rol coincide
   - Modulo.RutaPagina coincide
   - Permiso.Codigo coincide
   - Concedido = true
   - Modulo.Activo = true
   - Permiso.Activo = true
6. Retorna resultado
```

---

## Reconocimiento Facial

El sistema soporta reconocimiento facial para:
- Registro de asistencia
- Verificación de identidad

### Biblioteca utilizada
`FaceRecognitionDotNet` - Wrapper de dlib para .NET

### Flujo de captura
```
1. Usuario sube foto o captura con cámara
2. Sistema carga imagen en memoria
3. Detecta rostros con FaceRecognition
4. Valida que haya exactamente UN rostro
5. Genera embedding facial (vector 128D)
6. Convierte a byte[] y guarda en EmbeddingFacial
```

### Requisitos
- Modelos de reconocimiento en `face_recognition_models/`
- Foto con exactamente un rostro
- Máximo 5MB por imagen

---

## Permisos Requeridos

| Acción | Módulo | Permiso |
|--------|--------|---------|
| Ver usuarios | `/personal` | VIEW |
| Crear usuario | `/personal` | CREATE |
| Editar usuario | `/personal` | EDIT |
| Eliminar usuario | `/personal` | DELETE + SA password |
| Configurar permisos | `/personal` | VIEW (o EDIT) |

---

## Diagrama de Relaciones

```
Usuario
    └── Id_Rol → Rol
                    └── PermisosModulos[]
                            ├── IdModulo → Modulo
                            │               └── IdModuloPadre → Modulo (recursivo)
                            └── IdPermiso → Permiso
```

---

## Casos de Uso

### Crear Nuevo Usuario

1. Ir a `/menu-usuarios`
2. Click en "Registrar Usuario"
3. Completar datos personales
4. Ingresar credenciales
5. Seleccionar rol
6. (Opcional) Capturar foto para reconocimiento
7. (Opcional) Ingresar datos laborales
8. Click "Crear Usuario"

### Configurar Permisos de un Rol

1. Ir a `/personal/permisos-usuarios`
2. Seleccionar rol en el dropdown
3. Se carga matriz de permisos
4. Marcar/desmarcar checkboxes según necesidad
5. Click "Guardar Cambios"
6. Los usuarios del rol deben cerrar y abrir sesión

### Desactivar Usuario

1. Ir a `/usuarios/editarUsu/{id}`
2. Desmarcar "Usuario Activo"
3. Click "Guardar Cambios"
4. El usuario no podrá iniciar sesión

### Eliminar Usuario

1. Ir a `/menu-usuarios`
2. Click "Eliminar" en el usuario
3. Ingresar contraseña SA de SQL Server
4. Confirmar eliminación

---

## Seguridad

### Contraseñas
- Hasheadas con SHA256
- Nunca almacenadas en texto plano
- No se pueden recuperar, solo restablecer

### Protección de eliminación
- Eliminación de usuarios requiere contraseña SA
- Previene eliminación accidental o maliciosa

### Validaciones de acceso
- Cada página verifica permisos al cargar
- Usuario debe estar activo
- Rol debe estar activo
- Permiso debe estar concedido

### Sesiones
- Basadas en cookies de autenticación
- Cambios de permisos requieren re-login
- Sesión expira según configuración del servidor

---

## Integración con Otros Módulos

### Todo el sistema
- Todas las páginas usan `PageProtection`
- Botones sensibles usan `RequirePermission`

### Asistencia
- Usa `EmbeddingFacial` para registro por rostro
- Relaciona con `AsignacionHorario`

### Ventas/Caja
- Identifica vendedor/cajero en operaciones
- Registra usuario que realiza acciones

### Auditoría
- Registra qué usuario realizó cada operación
- Fecha y hora de acciones importantes

---

## Configuración Inicial

### Datos Semilla (Seed Data)

**Permisos básicos:**
```sql
INSERT INTO Permisos (Nombre, Codigo, Orden, Activo) VALUES
('Ver', 'VIEW', 1, 1),
('Crear', 'CREATE', 2, 1),
('Editar', 'EDIT', 3, 1),
('Eliminar', 'DELETE', 4, 1),
('Exportar', 'EXPORT', 5, 1),
('Imprimir', 'PRINT', 6, 1);
```

**Rol Administrador:**
```sql
INSERT INTO Rol (NombreRol, Descripcion, Estado) VALUES
('Administrador', 'Acceso total al sistema', 1);
```

**Usuario Admin inicial:**
```sql
-- Contraseña: admin123 (hasheada)
INSERT INTO Usuarios (Nombres, Apellidos, UsuarioNombre, ContrasenaHash, Id_Rol, Estado_Usu, Fecha_Ingreso)
VALUES ('Admin', 'Sistema', 'admin', 0x..., 1, 1, GETDATE());
```

---

## Mantenimiento

### Agregar nuevo módulo al sistema

1. Insertar en tabla `Modulos`:
```sql
INSERT INTO Modulos (Nombre, Descripcion, RutaPagina, Icono, IdModuloPadre, Orden, Activo)
VALUES ('Mi Módulo', 'Descripción', '/mi-modulo', 'bi-star', NULL, 99, 1);
```

2. Asignar permisos al rol Admin:
```sql
DECLARE @IdModulo INT = SCOPE_IDENTITY();
INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido)
SELECT 1, @IdModulo, IdPermiso, 1 FROM Permisos WHERE Activo = 1;
```

3. Aplicar migración si se usa EF Core

### Auditar permisos de un rol

```sql
SELECT m.Nombre AS Modulo, p.Nombre AS Permiso, rmp.Concedido
FROM RolesModulosPermisos rmp
JOIN Modulos m ON rmp.IdModulo = m.IdModulo
JOIN Permisos p ON rmp.IdPermiso = p.IdPermiso
WHERE rmp.IdRol = @IdRol
ORDER BY m.Nombre, p.Orden;
```
