# Guía del Módulo de Clientes

## Descripción General

El módulo de Clientes gestiona el registro completo de clientes del sistema, incluyendo:
- Datos de identificación (RUC, CI)
- Información de contacto
- Condiciones comerciales (crédito, plazos)
- Precios diferenciados por cliente
- Campos específicos para SIFEN (facturación electrónica)
- Envío automático de facturas por correo

---

## Modelo de Datos

### Entidad Principal: `Cliente`

#### Identificación
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `IdCliente` | int (PK) | Identificador único |
| `CodigoCliente` | string(50)? | Código interno opcional |
| `RazonSocial` | string(250) | Nombre o razón social (obligatorio) |
| `RUC` | string(50) | RUC o CI (obligatorio) |
| `DV` | int | Dígito verificador (obligatorio) |
| `TipoDocumento` | string(2) | Tipo de documento |
| `NumeroDocumento` | string(50)? | Número de documento |

#### Ubicación y Contacto
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Direccion` | string(150)? | Dirección principal |
| `NumeroCasa` | string(10)? | Número de casa |
| `CompDireccion1` | string(100)? | Complemento dirección 1 |
| `CompDireccion2` | string(100)? | Complemento dirección 2 |
| `Telefono` | string(20)? | Teléfono fijo |
| `Celular` | string(20)? | Teléfono celular |
| `Email` | string(150)? | Correo electrónico |
| `CodigoPais` | string(3) | Código de país (PRY, USA, etc.) |
| `IdCiudad` | int | Ciudad según catálogo |
| `EsExtranjero` | bool | ¿Es extranjero? |

#### Datos Personales/Comerciales
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `FechaNacimiento` | DateTime? | Fecha de nacimiento |
| `FechaAlta` | DateTime? | Fecha de registro |
| `Contacto` | string(100)? | Nombre del contacto |
| `Estado` | bool | Activo/Inactivo (obligatorio) |

#### Condiciones Comerciales
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `PermiteCredito` | bool | ¿Habilita venta a crédito? |
| `LimiteCredito` | decimal(18,4)? | Límite de crédito en Gs |
| `PlazoDiasCredito` | int? | Días de plazo para pagos |
| `Saldo` | decimal(18,4) | Saldo pendiente actual |
| `PrecioDiferenciado` | bool | ¿Tiene precios especiales? |

#### Facturación Automática
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `EnviarFacturaPorCorreo` | bool | Enviar PDF de factura al confirmar venta |
| `Timbrado` | string(8)? | Timbrado asociado |
| `VencimientoTimbrado` | DateTime? | Fecha vencimiento timbrado |

---

### Campos SIFEN (Facturación Electrónica)

El sistema soporta todos los campos requeridos por el Manual Técnico SIFEN v150.

#### Naturaleza del Receptor
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `NaturalezaReceptor` | int | **1** = Contribuyente (con RUC), **2** = No contribuyente |
| `IdTipoContribuyente` | int | Tipo de contribuyente SIFEN |
| `TipoContribuyente` | enum? | Enumeración adicional |

#### Tipo de Operación
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `TipoOperacion` | string(3)? | Código de operación SIFEN |

**Valores de TipoOperacion:**
| Código | Descripción |
|--------|-------------|
| 1 | B2B - Empresa a Empresa/Extranjero |
| 2 | B2C - Empresa a Consumidor Final |
| 3 | B2G - Empresa a Gobierno |
| 4 | B2F - Empresa a Extranjero |

#### Datos Geográficos SIFEN
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `CodigoDepartamento` | string(2)? | Código departamento SIFEN (ej: "01"=Capital) |
| `DescripcionDepartamento` | string(16)? | Nombre departamento |
| `CodigoDistrito` | string(4)? | Código distrito SIFEN (ej: "0001"=Asunción) |
| `DescripcionDistrito` | string(30)? | Nombre distrito |

#### Documentos para No Contribuyentes
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `TipoDocumentoIdentidadSifen` | int? | Tipo de documento SIFEN |
| `NumeroDocumentoIdentidad` | string(20)? | Número del documento |

**Valores de TipoDocumentoIdentidadSifen:**
| Código | Descripción |
|--------|-------------|
| 1 | Cédula paraguaya |
| 2 | Cédula extranjera |
| 3 | Pasaporte |
| 4 | Carnet de residencia |
| 5 | Innominado (sin documento) |

#### Otros Campos SIFEN
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `NombreFantasia` | string(255)? | Nombre comercial |
| `CodigoEstablecimiento` | string(20)? | Código establecimiento receptor |

---

### Entidad: `ClientePrecio` (Precios Diferenciados)

Permite definir precios especiales para clientes específicos.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `IdClientePrecio` | int (PK) | Identificador único |
| `IdCliente` | int (FK) | Cliente |
| `IdProducto` | int (FK) | Producto |
| `PrecioFijoGs` | decimal(18,4)? | Precio fijo en Gs |
| `PorcentajeDescuento` | decimal(18,2)? | % de descuento (0-100) |
| `Activo` | bool | Si el precio está vigente |
| `FechaCreacion` | DateTime | Fecha de creación |
| `UsuarioCreacion` | string(50)? | Usuario que creó |

**Lógica de aplicación:**
```
Si PrecioFijoGs tiene valor → Usar precio fijo
Si no → Aplicar PorcentajeDescuento sobre precio de lista
```

---

## Páginas del Módulo

### 1. Listado Simple (`/clientes`)

**Ruta:** `/clientes`  
**Permiso:** VIEW sobre `/clientes`

**Funcionalidad:**
- Tabla paginada (50 registros por página)
- Columnas: ID, Razón Social, RUC, Teléfono, Email, Saldo
- Botón editar por registro
- Botón flotante para nuevo cliente
- Link a configuración DNIT

### 2. Explorador (`/clientes/explorar`)

**Ruta:** `/clientes/explorar`  
**Permiso:** VIEW sobre `/clientes`

**Filtros:**
- RUC o Razón Social (búsqueda parcial)
- Estado (Todos, Activos, Inactivos)

**Columnas:**
- Razón Social
- RUC (formato completo con DV)
- Contacto
- Teléfono
- Email
- Acciones (Editar)

### 3. Crear Cliente (`/clientes/crear`)

**Ruta:** `/clientes/crear`  
**Permiso:** CREATE sobre `/clientes`

**Secciones del formulario:**

#### Identificación
- Razón Social (obligatorio)
- Tipo Documento + Documento
- Botón DNIT (consulta automática RUC/CI)
- DV (se calcula automáticamente)

#### Ubicación y Contacto
- Dirección
- País + Ciudad
- Teléfono + Email
- Fecha de Nacimiento

#### Condiciones Comerciales
- Cliente Activo (switch)
- ¿Extranjero? (switch)
- Naturaleza SIFEN (Contribuyente / No contribuyente)
- Tipo de Contribuyente
- Tipo de Operación
- Límite Crédito + Plazo Días
- Permite Crédito (switch)
- Precio Diferenciado (switch)
- Enviar Factura por Correo (switch)

#### Datos SIFEN Avanzados (colapsable)
- Tipo Doc. Identidad SIFEN
- Número Doc. Identidad
- Código/Descripción Departamento
- Código/Descripción Distrito
- Naturaleza Receptor
- Número Casa
- Complementos Dirección
- Código Establecimiento

### 4. Editar Cliente (`/clientes/editar/{id}`)

**Ruta:** `/clientes/editar/{IdCliente:int}`  
**Permiso:** EDIT sobre `/clientes`

Incluye todas las secciones de Crear, más:

#### Estado de Crédito (si permite crédito)
Muestra panel con:
- Límite de Crédito configurado
- Saldo Pendiente actual
- Facturas Vencidas (cantidad y monto)
- Facturas Por Vencer en 7 días (cantidad y monto)
- Alertas visuales si hay vencidas

#### Precios Diferenciados (si está habilitado)
- Buscador de productos con autocompletado
- Campos: Precio fijo Gs, % Descuento
- Tabla de precios configurados:
  - Producto
  - Precio lista
  - Precio fijo
  - % Descuento
  - Precio final calculado
  - Botón eliminar

---

## Funcionalidades Especiales

### Consulta DNIT (RUC/CI)

El botón "DNIT" consulta el servicio de la SET para obtener datos del contribuyente:

**Servicio:** `IRucDnitService`  
**Clase de apoyo:** `Sifen` (SifenService)

```
1. Usuario ingresa RUC o CI
2. Click en botón DNIT
3. Sistema consulta servicios SET
4. Si encuentra:
   - Completa Razón Social
   - Calcula y establece DV
   - Detecta Tipo Documento (RUC o CI)
   - Establece Naturaleza Receptor
5. Muestra mensaje de éxito o error
```

### Precios Diferenciados

Cuando `PrecioDiferenciado = true`:

1. **En Crear**: Mensaje indicando guardar primero y luego configurar
2. **En Editar**: Panel completo para gestionar precios

**Flujo en Ventas:**
```
1. Se selecciona cliente con PrecioDiferenciado = true
2. Al agregar producto, sistema busca en ClientePrecio
3. Si existe registro:
   - PrecioFijoGs tiene valor → Usar ese precio
   - Si no → Aplicar descuento sobre precio lista
4. Precio especial se aplica automáticamente
```

### Envío de Factura por Correo

Cuando `EnviarFacturaPorCorreo = true`:

```
1. Venta se confirma con cliente que tiene el flag activo
2. Sistema genera PDF de la factura (KuDE o formato según caja)
3. Se envía por correo a cliente.Email
4. Requiere configuración de correo SMTP activa
```

**Requisitos:**
- Cliente debe tener Email configurado
- Sistema debe tener ConfiguracionCorreo activa
- Solo se activa si hay email válido

### Validaciones SIFEN

#### Para Contribuyentes (NaturalezaReceptor = 1):
- Requiere RUC válido
- Tipo de Contribuyente obligatorio
- Campos de documento identidad SIFEN se ignoran

#### Para No Contribuyentes (NaturalezaReceptor = 2):
- Requiere TipoDocumentoIdentidadSifen
- Requiere NumeroDocumentoIdentidad (excepto si es Innominado)
- Si TipoDocumentoIdentidadSifen = 5 (Innominado), documento se deshabilita

---

## Permisos Requeridos

| Acción | Módulo | Permiso |
|--------|--------|---------|
| Ver listado | `/clientes` | VIEW |
| Ver explorador | `/clientes` | VIEW |
| Crear cliente | `/clientes` | CREATE |
| Editar cliente | `/clientes` | EDIT |
| Gestionar precios | `/clientes` | EDIT |

---

## Integración con Otros Módulos

### Ventas
- Se selecciona cliente al crear venta
- Valida límite de crédito si es venta a crédito
- Aplica precios diferenciados si corresponde
- Actualiza saldo pendiente al confirmar

### Cobros
- Muestra facturas pendientes del cliente
- Permite registrar pagos que reducen saldo
- Control de vencimientos por cliente

### SIFEN
- Datos del cliente se envían en la factura electrónica
- Campos específicos mapean a estructura gDatRec
- Validación de naturaleza receptor obligatoria

### Presupuestos
- Cliente se asocia al presupuesto
- Datos se transfieren al convertir a venta

---

## Diagrama de Relaciones

```
Cliente
    ├── CodigoPais → Paises
    ├── IdCiudad → CiudadCatalogo
    ├── TipoOperacion → TipoOperacion
    ├── IdTipoContribuyente → TiposContribuyentes
    ├── TipoDocumento → TiposDocumentosIdentidad
    └── ClientePrecios[]
            └── IdProducto → Producto

Ventas[] → IdCliente
Cobros[] → IdCliente
Presupuestos[] → IdCliente
```

---

## Casos de Uso

### Crear Cliente Contribuyente (Empresa)

1. Ir a `/clientes/crear`
2. Ingresar RUC y presionar DNIT
3. Sistema completa datos automáticamente
4. Verificar Naturaleza = "Contribuyente"
5. Configurar límite de crédito si aplica
6. Habilitar "Permite Crédito" si venderá a crédito
7. Configurar "Enviar Factura por Correo" si tiene email
8. Guardar

### Crear Cliente Consumidor Final

1. Ir a `/clientes/crear`
2. Ingresar CI y presionar DNIT
3. Cambiar Naturaleza a "No Contribuyente"
4. Expandir "Datos SIFEN avanzados"
5. Seleccionar Tipo Doc. Identidad (1=CI Paraguaya)
6. Verificar número de documento
7. Guardar

### Configurar Precios Especiales

1. Ir a `/clientes/editar/{id}`
2. Habilitar "Precio Diferenciado"
3. Guardar cambios
4. Click en "Gestionar precios por producto"
5. Buscar producto por nombre o código
6. Ingresar precio fijo O porcentaje de descuento
7. Click "Agregar"
8. Repetir para cada producto con precio especial

### Verificar Estado de Crédito

1. Ir a `/clientes/editar/{id}`
2. Si tiene "Permite Crédito" habilitado:
   - Ver panel "Estado de Crédito"
   - Revisar saldo pendiente
   - Ver facturas vencidas
   - Ver facturas por vencer
3. Si hay vencidas, aparece alerta roja

---

## Validaciones del Sistema

### Al Crear/Editar
- Razón Social: Obligatorio, máx 250 caracteres
- RUC: Obligatorio, máx 50 caracteres
- DV: Se valida contra algoritmo módulo 11
- Email: Formato válido si se ingresa
- Teléfono: Máx 20 caracteres

### Al Vender a Crédito
```
Si cliente.PermiteCredito = true:
    Si cliente.LimiteCredito tiene valor:
        Crédito disponible = LimiteCredito - Saldo
        Si Total venta > Crédito disponible:
            → Bloquear venta o advertir
```

### SIFEN al Facturar
```
Si NaturalezaReceptor = 1 (Contribuyente):
    → Validar RUC válido
    → Validar TipoContribuyente
    
Si NaturalezaReceptor = 2 (No Contribuyente):
    → Validar TipoDocumentoIdentidadSifen
    → Validar NumeroDocumentoIdentidad (si no es Innominado)
```

---

## Exportaciones

El explorador de clientes permite:
- Búsqueda con filtros
- Máximo 500 resultados por consulta
- Vista en tabla con datos principales

Para exportación masiva, usar reportes del módulo de informes.
