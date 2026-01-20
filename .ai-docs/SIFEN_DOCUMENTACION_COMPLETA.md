# Sistema SIFEN - Documentación Completa

## 📋 Descripción General

**SIFEN** (Sistema Integrado de Facturación Electrónica Nacional) es el sistema de facturación electrónica de Paraguay, administrado por la **SET** (Subsecretaría de Estado de Tributación).

SistemIA tiene una implementación **avanzada** del SIFEN que incluye:
- Generación del CDC (Código de Control del Documento)
- Construcción del XML del Documento Electrónico (DE)
- Firma digital con certificado .p12
- Envío a los webservices del SET
- Generación del QR para impresión
- Consultas de estado de documentos

---

## 🗃️ Tablas y Campos Relacionados con SIFEN

### 1. **Venta** (Factura Electrónica)
```csharp
// Numeración SIFEN
[MaxLength(8)] public string? Timbrado { get; set; }
[MaxLength(3)] public string? Establecimiento { get; set; }  // 001-999
[MaxLength(3)] public string? PuntoExpedicion { get; set; }  // 001-999
[MaxLength(7)] public string? NumeroFactura { get; set; }    // 0000001-9999999
public int? Serie { get; set; }

// SIFEN
[MaxLength(64)] public string? CDC { get; set; }             // Código de Control (44 dígitos)
[MaxLength(9)] public string? CodigoSeguridad { get; set; }  // 9 dígitos aleatorios
[MaxLength(30)] public string? EstadoSifen { get; set; }     // PENDIENTE/ENVIADO/ACEPTADO/RECHAZADO
public DateTime? FechaEnvioSifen { get; set; }
public string? MensajeSifen { get; set; }
public string? XmlCDE { get; set; }                          // XML firmado guardado
[MaxLength(50)] public string? IdLote { get; set; }          // ID del lote enviado
public string? UrlQrSifen { get; set; }                      // URL completa del QR con cHashQR (dCarQR del XML firmado)
```

### 2. **Sucursal** (Emisor)
```csharp
public string? RUC { get; set; }                // RUC del emisor
public int? DV { get; set; }                    // Dígito verificador
public int NumSucursal { get; set; }            // Número de establecimiento
public int? IdCiudad { get; set; }              // Código ciudad catálogo SIFEN
public string? Direccion { get; set; }
public string? NombreEmpresa { get; set; }
```

### 3. **Cliente** (Receptor)
```csharp
public int NaturalezaReceptor { get; set; }     // 1=Contribuyente, 2=No contribuyente
public string? RUC { get; set; }
public int DV { get; set; }
public int? TipoDocumentoIdentidadSifen { get; set; }  // 1=CI, 2=RUC, 3=PAS, 5=Innominado, 9=Sin doc
public string? NumeroDocumentoIdentidad { get; set; }
public string? CodigoPais { get; set; }         // PRY para Paraguay
public int? IdCiudad { get; set; }              // Catálogo SIFEN
public int? IdDistrito { get; set; }
public int? IdDepartamento { get; set; }
```

### 4. **Sociedad** (Configuración CSC para QR)
```csharp
public string? IdCsc { get; set; }              // ID del CSC proporcionado por SET
public string? Csc { get; set; }                // Código de Seguridad del Contribuyente
public string? ActividadEconomicaPrincipal { get; set; }
public string? CodigoActividadEconomica { get; set; }
```

### 5. **Caja** (Punto de Expedición)
```csharp
public string? Nivel2 { get; set; }             // Punto de expedición (001, 002, etc.)
public string? TipoFacturacion { get; set; }    // "Factura Electrónica" o "Autoimpresor"
```

### 6. **Catálogos SIFEN** (SifenCatalogos.cs)
- `DepartamentosCatalogo` - Códigos de departamentos
- `DistritosCatalogo` - Códigos de distritos
- `CiudadesCatalogo` - Códigos de ciudades
- `PaisesCatalogo` - Códigos ISO de países
- `MonedasCatalogo` - Códigos de monedas (PYG, USD, BRL)
- `TiposIvaCatalogo` - Tasas de IVA (10%, 5%, Exenta)

---

## 🔧 Servicios Implementados

### 1. **CdcGenerator** (`Utils/CdcGenerator.cs`)
Genera el CDC de 44 dígitos según especificación SIFEN v150.

**Estructura del CDC:**
```
Posición | Longitud | Campo
---------|----------|------------------------------------------
01-02    | 2        | Tipo de documento (01=Factura, 04=Autofactura, etc.)
03-10    | 8        | RUC del emisor (sin DV)
11       | 1        | Dígito verificador del RUC
12-14    | 3        | Establecimiento
15-17    | 3        | Punto de expedición
18-24    | 7        | Número de documento
25       | 1        | Tipo de contribuyente (1=Física, 2=Jurídica)
26-33    | 8        | Fecha de emisión (AAAAMMDD)
34       | 1        | Tipo de emisión (1=Normal, 2=Contingencia)
35-43    | 9        | Código de seguridad (aleatorio)
44       | 1        | Dígito verificador del CDC (módulo 11)
```

**Ejemplo de uso:**
```csharp
var cdc = CdcGenerator.GenerarCDC(
    tipoDocumento: "01",           // 01 = Factura
    rucEmisor: "80012345",
    dvEmisor: "6",
    establecimiento: "001",
    puntoExpedicion: "001",
    numeroFactura: "0000001",
    tipoContribuyente: "2",        // 2 = Persona Jurídica
    fechaEmision: DateTime.Now,
    tipoEmision: "1"               // 1 = Normal
);
// Resultado: 01800123456001001000000122024010611234567890
```

### 2. **DEXmlBuilder** (`Services/DEXmlBuilder.cs`)
Construye el XML del Documento Electrónico según especificación SIFEN v150.

**Estructura XML generada:**
```xml
<?xml version="1.0" encoding="UTF-8"?>
<rDE xmlns="http://ekuatia.set.gov.py/sifen/xsd">
  <dVerFor>150</dVerFor>
  <DE Id="cdc44">
    <gOpeDE>...</gOpeDE>        <!-- Operación del DE -->
    <gTimb>...</gTimb>          <!-- Timbrado -->
    <gDatGralOpe>
      <dFeEmiDE>...</dFeEmiDE>  <!-- Fecha emisión -->
      <gOpeCom>...</gOpeCom>    <!-- Operación comercial -->
      <gEmis>...</gEmis>        <!-- Emisor -->
      <gDatRec>...</gDatRec>    <!-- Receptor -->
    </gDatGralOpe>
    <gDtipDE>
      <gCamFE>...</gCamFE>      <!-- Campos Factura Electrónica -->
      <gCamItem>...</gCamItem>  <!-- Items -->
      <gTotSub>...</gTotSub>    <!-- Totales -->
    </gDtipDE>
    <gCamGen>
      <gPagCred>...</gPagCred>  <!-- Pagos a crédito -->
    </gCamGen>
  </DE>
  <Signature>...</Signature>    <!-- Firma digital -->
  <gCamFuFD>
    <dCarQR>...</dCarQR>        <!-- URL del QR -->
  </gCamFuFD>
</rDE>
```

### 3. **ClienteSifenService** (`Services/ClienteSifenService.cs`)
Convierte datos del Cliente al formato `gDatRec` (Datos del Receptor) requerido por SIFEN.

### 4. **DEBuilderService** (`Services/DEBuilderService.cs`)
Valida que una venta tenga todos los datos necesarios para generar un DE válido.

### 5. **Sifen** (`Models/Sifen.cs`)
Clase principal que maneja:
- `FirmarYEnviar()` - Firma el XML con certificado .p12 y envía al SET
- `Enviar()` - Comunicación HTTP con los webservices del SET
- `SHA256ToString()` - Hash para el cHashQR
- `StringToZip()` - Compresión GZip para el xDE

---

## 📡 Endpoints API Implementados

### Envío de Facturas

```http
POST /ventas/{idVenta}/enviar-sifen
```
Envía una venta a SIFEN usando el modo de lote asíncrono (rEnviLoteDe).

```http
POST /ventas/{idVenta}/enviar-sifen-sync
```
Envía una venta a SIFEN usando el modo síncrono (recibe-de).

### Consultas

```http
GET /ventas/{idVenta}/consultar-sifen
```
Consulta el estado de un documento enviado usando el IdLote.

### Debug/Desarrollo

```http
GET /debug/ventas/{idVenta}/mensaje-sifen
```
Obtiene información de debug del último envío SIFEN.

```http
GET /debug/ventas/{idVenta}/de-firmado
```
Genera el DE firmado sin enviarlo.

```http
GET /debug/ventas/{idVenta}/soap-lote
```
Genera el sobre SOAP completo para ver qué se enviaría.

---

## 🌐 URLs de Webservices SET

### Ambiente de PRUEBAS (test.sifen.set.gov.py)
```
Recepción DE:      https://test.sifen.set.gov.py/de/ws/sync/recibe.wsdl
Recepción Lote:    https://test.sifen.set.gov.py/de/ws/async/recibe-lote.wsdl
Consulta Lote:     https://test.sifen.set.gov.py/de/ws/consultas/consulta-lote.wsdl
Consulta RUC:      https://test.sifen.set.gov.py/de/ws/consultas/consulta-ruc.wsdl
Consulta DE:       https://test.sifen.set.gov.py/de/ws/consultas/consulta.wsdl
URL Base QR:       https://test.sifen.set.gov.py/de/consulta-de?cdc=
```

### Ambiente de PRODUCCIÓN (sifen.set.gov.py)
```
Recepción DE:      https://sifen.set.gov.py/de/ws/sync/recibe.wsdl
Recepción Lote:    https://sifen.set.gov.py/de/ws/async/recibe-lote.wsdl
Consulta Lote:     https://sifen.set.gov.py/de/ws/consultas/consulta-lote.wsdl
Consulta RUC:      https://sifen.set.gov.py/de/ws/consultas/consulta-ruc.wsdl
Consulta DE:       https://sifen.set.gov.py/de/ws/consultas/consulta.wsdl
URL Base QR:       https://sifen.set.gov.py/de/consulta-de?cdc=
```

---

## 🔐 Configuración del Certificado

El sistema requiere un certificado digital .p12 emitido por el SET:

```json
// appsettings.json
{
  "Sifen": {
    "Environment": "Test",
    "CertificatePath": "certificados/certificado.p12",
    "CertificatePassword": "password",
    "IdCsc": "0001",
    "Csc": "ABCD1234..."
  }
}
```

---

## 📊 Flujo de Envío SIFEN

```
1. Venta Confirmada
        ↓
2. Validar datos (DEBuilderService.ValidarVentaAsync)
        ↓
3. Generar CDC (CdcGenerator.GenerarCDC)
        ↓
4. Construir XML (DEXmlBuilder.ConstruirXmlAsync)
        ↓
5. Firmar con certificado (Sifen.FirmarYEnviar)
        ↓
6. Enviar al SET
        ↓
7. Procesar respuesta
        ↓
8. Actualizar EstadoSifen en BD
        ↓
9. Si éxito: Guardar CDC, IdLote, URL QR
```

---

## 🖼️ Generación del QR

El QR contiene la URL de consulta del documento:

```
https://sifen.set.gov.py/de/consulta-de?cdc={CDC}&cHashQR={HASH}
```

Donde:
- **CDC**: Código de Control de 44 dígitos
- **cHashQR**: SHA256 de la URL completa (sin el hash)

**Ejemplo en código:**
```csharp
string urlQr = $"https://sifen.set.gov.py/de/consulta-de?cdc={cdc}";
string hash = SHA256ToString(urlQr);
urlQr = $"{urlQr}&cHashQR={hash}";
```

---

## 📄 Tipos de Documentos Electrónicos

| Código | Tipo | Descripción |
|--------|------|-------------|
| 01 | FE | Factura Electrónica |
| 02 | FEE | Factura Electrónica de Exportación |
| 03 | FCE | Factura Electrónica de Crédito |
| 04 | AFE | Autofactura Electrónica |
| 05 | NCE | Nota de Crédito Electrónica |
| 06 | NDE | Nota de Débito Electrónica |
| 07 | NRE | Nota de Remisión Electrónica |

---

## 📂 Archivos del Proyecto

### Modelos
- `Models/Sifen.cs` - Clase principal con firma y envío
- `Models/SifenCatalogos.cs` - Catálogos de códigos
- `Models/ClienteSifenMejorado.cs` - Modelo de cliente SIFEN

### Servicios
- `Services/DEXmlBuilder.cs` - Constructor del XML
- `Services/DEBuilderService.cs` - Validador de datos
- `Services/ClienteSifenService.cs` - Datos del receptor

### Utilidades
- `Utils/CdcGenerator.cs` - Generador del CDC
- `Utils/SifenConfig.cs` - Configuración SIFEN
- `Utils/SifenTester.cs` - Pruebas de conexión

### Documentación
- `ManualSifen/Manual_Tecnico_Version_150.txt` - Manual oficial SET
- `ManualSifen/Extructura xml_DE.xml` - Ejemplo de estructura XML
- `ManualSifen/catalogo_geografico.csv` - Catálogo geográfico

---

## ⚠️ Códigos de Error Comunes

| Código | Descripción | Solución | Ejemplo |
|--------|-------------|----------|--------|
| 0160 | XML Mal Formado | Revisar estructura del XML, fechas, campos requeridos | Fechas en el futuro, campos vacíos |
| 0300 | Certificado inválido | Verificar certificado .p12 | Certificado expirado o revocado |
| 0400 | RUC no habilitado | Verificar habilitación en SET | RUC no registrado para FE |
| 0500 | CDC duplicado | Ya existe ese documento | Envío repetido |
| 0600 | Timbrado vencido | Solicitar nuevo timbrado | Fecha fuera de vigencia |

### Detalle del Error 0160 - XML Mal Formado

Este error es uno de los más comunes y puede tener múltiples causas:

| Causa | Descripción | Solución |
|-------|-------------|----------|
| **Fechas futuras** | `dFeEmiDE`, `dFeIniT`, `dFecFirma` con año incorrecto | Verificar que las fechas sean actuales |
| **Caracteres especiales** | Caracteres no escapados en nombres o descripciones | Usar XML encoding para &, <, >, etc. |
| **Campos vacíos** | Campos requeridos sin valor | Validar datos antes de enviar |
| **Formato numérico** | Decimales con coma en lugar de punto | Usar `CultureInfo.InvariantCulture` |
| **CDC inválido** | Longitud o dígito verificador incorrecto | Usar `CdcGenerator.GenerarCDC()` |
| **Namespace incorrecto** | Falta o incorrecto `xmlns` | Usar `http://ekuatia.set.gov.py/sifen/xsd` |

---

## 🔄 Estados SIFEN

| Estado | Descripción |
|--------|-------------|
| `PENDIENTE` | Aún no enviado |
| `ENVIADO` | Enviado, esperando confirmación |
| `ACEPTADO` | Aceptado por el SET |
| `RECHAZADO` | Rechazado por el SET |
| `ANULADO` | Documento anulado |

---

## 🎛️ Páginas de Administración SIFEN

| Página | Ruta | Descripción |
|--------|------|-------------|
| DiagnosticoSifen | `/admin/sifen/diagnostico` | Verifica configuración, certificado, URLs |
| ProveedoresSifen | `/proveedores/sifen` | Gestión de proveedores SIFEN |
| ValidacionProveedoresSifen | `/proveedores/sifen/validacion` | Validación de datos SIFEN |
| ControlTimbradosSifen | `/configuracion/timbrados` | Control de timbrados |
| PruebasXmlExterno | `/admin/sifen/pruebas-xml` | Pruebas de envío XML |

---

## ✅ Estado de Implementación (Actualizado 20 Enero 2026)

### Funcionalidades IMPLEMENTADAS y PROBADAS:
- ✅ **Generación de CDC** (44 dígitos con dígito verificador)
- ✅ **Construcción XML DE** v150 para Facturas
- ✅ **Firma digital** con certificado .p12 - VALIDADA por SIFEN
- ✅ **Envío a SET** (lote asíncrono) - **FUNCIONANDO** (código 0300)
- ✅ **Formato dId correcto** - 12 dígitos DDMMYYYYHHMM
- ✅ **Posición Signature** - FUERA de `</DE>`, CON namespace XMLDSIG
- ✅ **Compresión ZIP** - ZipArchive real (no GZip)
- ✅ **Consulta de RUC** desde SET
- ✅ **Consulta de estado de lote**
- ✅ **Generación de QR** con cHashQR y DigestValue
- ✅ **Catálogos SIFEN** (departamentos, ciudades, etc.)
- ✅ **Validación de datos** antes del envío
- ✅ **Diagnóstico de configuración** (página admin)
- ✅ **Soporte dual** (ambiente Test/Producción)
- ✅ **Impresión KuDE** (formato A4 con QR)
- ✅ **Tickets** con CDC y QR

### Funcionalidades PENDIENTES/MEJORAS:
- ⬜ **Notas de Crédito Electrónicas** (NCE) - XML parcialmente implementado
- ⬜ **Notas de Débito Electrónicas** (NDE)
- ⬜ **Autofacturas Electrónicas** (AFE)
- ⬜ **Notas de Remisión Electrónicas** (NRE)
- ⬜ **Eventos de anulación** (inutilización de documentos)
- ⬜ **Modo contingencia** (tipo emisión 2)
- ⬜ **Reenvío automático** de documentos rechazados
- ⬜ **Dashboard SIFEN** con estadísticas
- ⬜ **Alertas** de documentos pendientes/rechazados
- ⬜ **Validación XSD** del XML generado

---

## � Conexión SSL/TLS - Hallazgos Importantes (Enero 2026)

### Problema Conocido: Conexiones Intermitentes

Los servidores de SIFEN (tanto test como producción) presentan **problemas de conexión SSL intermitentes**. Esto se manifiesta como:
- Errores "Unable to read data from the transport connection"
- Errores "An existing connection was forcibly closed"
- Timeouts en la primera conexión

**Causa raíz:** Los servidores del SET usan balanceadores BIG-IP que ocasionalmente rechazan conexiones SSL iniciales.

### ✅ Solución Implementada: Retry con Exponential Backoff

Se implementó un mecanismo de **retry automático** en `Models/Sifen.cs`:

```csharp
// Configuración actual (Sifen.cs - método Enviar)
const int maxRetries = 5;
int[] delaySeconds = { 1, 2, 3, 5, 8 }; // Fibonacci-like backoff

// Errores que disparan retry automático:
// - SSL, conexión, connection
// - timeout, refused, reset
```

### Configuración SSL Requerida

```csharp
// TLS 1.2 es OBLIGATORIO para SIFEN Paraguay
handler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;

// Aceptar todos los certificados del servidor (desarrollo)
handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

// Headers importantes para bypass de BIG-IP
client.DefaultRequestHeaders.Add("User-Agent", "Java/1.8.0_341");
client.DefaultRequestHeaders.Add("Connection", "close");
```

### URLs Correctas (con .wsdl)

Las URLs de SIFEN **DEBEN** terminar en `.wsdl`:

| Servicio | URL Test | URL Producción |
|----------|----------|----------------|
| Recepción | `https://sifen-test.set.gov.py/de/ws/sync/recibe.wsdl` | `https://sifen.set.gov.py/de/ws/sync/recibe.wsdl` |
| Lote | `https://sifen-test.set.gov.py/de/ws/async/recibe-lote.wsdl` | `https://sifen.set.gov.py/de/ws/async/recibe-lote.wsdl` |
| Consulta Lote | `https://sifen-test.set.gov.py/de/ws/consultas/consulta-lote.wsdl` | `https://sifen.set.gov.py/de/ws/consultas/consulta-lote.wsdl` |
| Consulta RUC | `https://sifen-test.set.gov.py/de/ws/consultas/consulta-ruc.wsdl` | `https://sifen.set.gov.py/de/ws/consultas/consulta-ruc.wsdl` |
| Consulta DE | `https://sifen-test.set.gov.py/de/ws/consultas/consulta.wsdl` | `https://sifen.set.gov.py/de/ws/consultas/consulta.wsdl` |
| Eventos | `https://sifen-test.set.gov.py/de/ws/eventos/evento.wsdl` | `https://sifen.set.gov.py/de/ws/eventos/evento.wsdl` |

### Content-Type para SOAP 1.2

```
application/xml; charset=utf-8
```

**Nota:** El código detecta automáticamente la operación para establecer el action correcto en el Content-Type.

### Diagnóstico de Problemas

Si las conexiones siguen fallando:

1. **Verificar certificado cliente:** El .pfx debe estar válido y con clave privada exportable
2. **Verificar TLS:** Debe ser TLS 1.2 estrictamente
3. **Reintentar:** El sistema reintenta automáticamente hasta 5 veces
4. **Logs:** Revisar consola para mensajes `[SIFEN]` y `[SSL]`

```bash
# Desde PowerShell, probar conectividad:
curl.exe -v --tlsv1.2 "https://sifen-test.set.gov.py/de/ws/consultas/consulta-ruc.wsdl"
```

---

## �📚 Archivos de Referencia Incluidos

### ManualSifen/
| Archivo | Descripción |
|---------|-------------|
| `Manual Técnico Versión 150.pdf` | Manual oficial del SET |
| `Manual_Tecnico_Version_150.txt` | Versión texto del manual |
| `Extructura xml_DE.xml` | Ejemplo oficial de XML |
| `catalogo_geografico.csv` | Códigos de ciudades/distritos |
| `Guía de Mejores Prácticas...pdf` | Recomendaciones del SET |
| `codigoabierto/` | Librería Java de referencia (Gradle) |
| `facturacionelectronicapy/` | Librería TypeScript de referencia |

---

## � Configuración CSC para Ambiente de Pruebas

### Valores Oficiales de TEST del SET

El SET provee valores CSC específicos para el ambiente de pruebas:

```
IdCsc: "0001"
Csc: "ABCD0000000000000000000000000000"  (32 caracteres)
```

**⚠️ IMPORTANTE:** Estos valores están documentados en el Manual Técnico del SET. Usar valores incorrectos causará error de firma QR.

### Configuración en Sociedad

```sql
UPDATE Sociedades 
SET IdCsc = '0001', 
    Csc = 'ABCD0000000000000000000000000000'
WHERE IdSociedad = 1;
```

### Verificación del QR

El campo `cHashQR` se calcula como:
```csharp
string datosQR = $"nVersion=150&Id={cdc}&dFeEmiDE={fechaEmision:yyyy-MM-ddTHH:mm:ss}&dRucRec={rucReceptor}&dTotGralOpe={total}&dTotIVA={iva}&cItems={cantItems}&DigestValue={digestValue}&IdCSC={idCsc}";
string cHashQR = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(datosQR + csc)).ToHex();
```

---

## 🐛 Problemas Resueltos y Soluciones

### 1. Error SSL en VentasExplorar.razor (Enero 2026)

**Problema:** Al usar el botón "Enviar SIFEN" desde VentasExplorar, aparecía error SSL para llamadas internas HTTP.

**Causa:** El `HttpClient` interno no tenía configuración SSL para llamadas localhost.

**Solución aplicada en** `Pages/VentasExplorar.razor` (líneas ~1358-1420):
```csharp
// Crear handler con bypass SSL para localhost
var handler = new System.Net.Http.HttpClientHandler();
handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
var http = new System.Net.Http.HttpClient(handler);
http.Timeout = TimeSpan.FromSeconds(120);
```

### 2. Error 0160 por Fechas Futuras

**Problema:** SIFEN rechaza documentos con código 0160 "XML Mal Formado".

**Causa:** La fecha de emisión (`dFeEmiDE`) o fecha de firma (`dFecFirma`) están en el futuro.

**Solución:** Verificar que las ventas tengan fecha actual, no futura. El campo `dFeIniT` (fecha inicio timbrado) también debe ser <= fecha actual.

### 3. Valores CSC de Prueba Incorrectos

**Problema:** Error de hash QR incorrecto.

**Causa:** Usar valores CSC de producción o inventados en ambiente de pruebas.

**Solución:** Usar valores oficiales de TEST:
```
IdCsc: "0001"
Csc: "ABCD0000000000000000000000000000"
```

### 4. ⚠️ Error 0160 "XML Mal Formado" - CRÍTICO (7 Enero 2026)

**Problema:** SIFEN rechazaba TODOS los envíos de lote con error 0160 "XML Mal Formado" y mensaje "CDC: Tag not found. IdLote: Tag not found."

**Causa raíz identificada:** La función `StringToZip()` en `Models/Sifen.cs` usaba **GZipStream** que produce archivos `.gz` (gzip), pero el XSD de SIFEN especifica `xmime:expectedContentTypes="application/zip"` que requiere un **archivo ZIP real**.

| Formato | Magic Bytes | Estructura |
|---------|-------------|------------|
| GZip (❌ incorrecto) | `\x1F\x8B` | Datos comprimidos directamente |
| ZIP (✅ correcto) | `PK\x03\x04` | Archivo con entradas nombradas |

**Solución implementada en** `Models/Sifen.cs` - función `StringToZip()`:

```csharp
// ANTES (INCORRECTO) - GZipStream
using (var gzip = new GZipStream(memoryStream, CompressionMode.Compress))
{
    using var writer = new StreamWriter(gzip, new UTF8Encoding(false));
    writer.Write(originalString);
}

// DESPUÉS (CORRECTO) - ZipArchive
using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
{
    var fileName = $"DE_{DateTime.Now:ddMMyyyy}.xml";
    var entry = zipArchive.CreateEntry(fileName, CompressionLevel.Optimal);
    using var entryStream = entry.Open();
    var xmlBytes = new UTF8Encoding(false).GetBytes(originalString);
    entryStream.Write(xmlBytes, 0, xmlBytes.Length);
}
```

**Referencia:** Código Java oficial en `ManualSifen/codigoabierto/src/main/java/com/roshka/sifen/internal/util/SifenUtil.java`:
```java
public static byte[] compressXmlToZip(String str) throws IOException {
    String fileName = "DE_" + new SimpleDateFormat("ddMMyyyy").format(new Date());
    ZipOutputStream out = new ZipOutputStream(Files.newOutputStream(zip.toPath()));
    ZipEntry entry = new ZipEntry(fileName + ".xml");
    out.putNextEntry(entry);
    out.write(str.getBytes(StandardCharsets.UTF_8));
    // ...
}
```

**Documentos de referencia descargados:**
- `.ai-docs/SIFEN/Manual_Tecnico_v150.pdf` (5.2 MB)
- `.ai-docs/SIFEN/Guia_Mejores_Practicas_Envio_DE.pdf` (520 KB)
- `.ai-docs/SIFEN/XML_Ejemplos/Extructura xml_DE.xml` - Ejemplo oficial
- `.ai-docs/SIFEN/XSD_Schemas/Estructura_DE xsd.xml` - Schema XSD
- `ManualSifen/codigoabierto/docs/set/ekuatia.set.gov.py/sifen/xsd/WS_SiRecepLoteDE_v141.xsd`

---

## � Comparación XML Funcional vs Generado (Enero 2026)

Se obtuvo un XML que **ya fue ACEPTADO** por SIFEN de otro sistema de la misma empresa (Gasparini Informática). A continuación se documentan las diferencias críticas:

### ✅ XML Funcional ACEPTADO por SIFEN
```xml
<rDE xmlns="http://ekuatia.set.gov.py/sifen/xsd" 
     xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" 
     xsi:schemaLocation="http://ekuatia.set.gov.py/sifen/xsd siRecepDE_v150.xsd">
```

### 🔍 Diferencias Clave Encontradas

#### 1. **Campo `gOblAfe` (Obligaciones Afectadas) - IMPORTANTE**
El XML funcional **INCLUYE** obligaciones fiscales del contribuyente:
```xml
<gOpeCom>
  <!-- ... otros campos ... -->
  <gOblAfe>
    <cOblAfe>211</cOblAfe>
    <dDesOblAfe>IMPUESTO AL VALOR AGREGADO - GRAVADAS Y EXONERADAS - EXPORTADORES</dDesOblAfe>
  </gOblAfe>
  <gOblAfe>
    <cOblAfe>700</cOblAfe>
    <dDesOblAfe>IMPUESTO A LA RENTA EMPRESARIAL - RÉGIMEN GENERAL</dDesOblAfe>
  </gOblAfe>
</gOpeCom>
```
**⚠️ Este campo NO lo generamos actualmente.** Es obligatorio según el tipo de contribuyente.

#### 2. **Campo `dBasExe` dentro de `gCamIVA`**
El XML funcional incluye el campo `dBasExe` (base exenta) incluso cuando es 0:
```xml
<gCamIVA>
  <!-- ... otros campos ... -->
  <dLiqIVAItem>40000</dLiqIVAItem>
  <dBasExe>0</dBasExe>    <!-- ✅ XML funcional lo incluye -->
</gCamIVA>
```

#### 3. **Campo `dSubExo` OMITIDO**
El XML funcional **NO incluye** el campo `<dSubExo>` (subtotal exonerado):
```xml
<gTotSub>
  <dSubExe>0</dSubExe>
  <!-- NO tiene dSubExo -->
  <dSub5>0</dSub5>
  <dSub10>440000</dSub10>
```
**Nuestro XML incluye** `<dSubExo>0</dSubExo>` que podría causar error 0160.

#### 4. **Formato de Decimales**
| Campo | XML Funcional | Nuestro XML |
|-------|---------------|-------------|
| `dCantProSer` | `1.0000` (4 decimales) | `1` (sin decimales) |
| `dPorcDesIt` | `0.00` (2 decimales) | `0` (sin decimales) |

#### 5. **Campos del Receptor Simplificados**
El XML funcional **NO incluye** datos geográficos opcionales del receptor:
```xml
<gDatRec>
  <iNatRec>1</iNatRec>
  <iTiOpe>1</iTiOpe>
  <cPaisRec>PRY</cPaisRec>
  <dDesPaisRe>Paraguay</dDesPaisRe>
  <iTiContRec>2</iTiContRec>
  <dRucRec>80031086</dRucRec>
  <dDVRec>1</dDVRec>
  <dNomRec>CLUB NAUTICO SAN BERNARDINO</dNomRec>
  <dNumCasRec>0</dNumCasRec>    <!-- Solo número de casa -->
  <dTelRec>0984-129-036</dTelRec>
  <dEmailRec>ariel.figueredo@cnsb.org.py</dEmailRec>
  <!-- NO tiene: cDepRec, dDesDepRec, cDisRec, dDesDisRec, cCiuRec, dDesCiuRec -->
</gDatRec>
```
**Nuestro XML incluye** todos los campos geográficos que podrían causar error si los códigos son inválidos.

#### 6. **Estructura de `gValorRestaItem` Simplificada**
El XML funcional **NO incluye** `dDescGloItem`:
```xml
<gValorRestaItem>
  <dDescItem>0</dDescItem>
  <dPorcDesIt>0.00</dPorcDesIt>
  <!-- NO tiene dDescGloItem -->
  <dTotOpeItem>440000</dTotOpeItem>
</gValorRestaItem>
```

### 📋 Tabla Resumen de Campos

| Campo | XML Funcional | Nuestro XML | Acción Recomendada |
|-------|---------------|-------------|-------------------|
| `gOblAfe` | ✅ Incluye | ❌ No genera | **AGREGAR** |
| `dBasExe` en gCamIVA | ✅ Incluye | ❌ No genera | Agregar |
| `dSubExo` | ❌ No incluye | ✅ Genera | **ELIMINAR** |
| `dDescGloItem` | ❌ No incluye | ✅ Genera | Revisar si necesario |
| Decimales cantidad | `1.0000` | `1` | Formatear a 4 decimales |
| Decimales porcentaje | `0.00` | `0` | Formatear a 2 decimales |
| Campos geográficos receptor | ❌ Omitidos | ✅ Incluidos | Hacer opcionales |

### 🛠️ Código Java de Referencia (rshk-jsifenlib)

Se analizó la librería oficial de Roshka en GitHub:
- **Repositorio:** `roshkadev/rshk-jsifenlib`
- **Archivo clave:** `src/main/java/com/roshka/sifen/internal/request/ReqRecLoteDe.java`

```java
// Estructura del SOAP para envío de lote:
SOAPBodyElement rEnvioLote = soapBody.addBodyElement(
    new QName(Constants.SIFEN_NS_URI, "rEnvioLote"));  // CON namespace
rEnvioLote.addChildElement("dId").setTextContent(...);  // Hereda namespace
SOAPElement xDE = rEnvioLote.addChildElement("xDE");    // Hereda namespace

// rLoteDE se crea en mensaje SOAP SEPARADO (sin namespace heredado):
SOAPElement rLoteDE = SoapHelper.createSoapMessage()
    .getSOAPBody().addChildElement("rLoteDE");  // SIN QName = SIN namespace
```

### 📦 Estructura del ZIP Correcto

```
archivo.zip
└── DE_DDMMYYYY.xml
    └── <rLoteDE>                    ← SIN namespace
          └── <rDE xmlns="...">      ← CON namespace SIFEN
                └── <DE Id="CDC44">
                      └── (contenido del documento)
                └── <Signature>
                └── <gCamFuFD>
```

### 🔧 Archivos a Modificar para Corrección

1. **`Services/DEXmlBuilder.cs`** - Agregar campo `gOblAfe`, quitar `dSubExo`, agregar `dBasExe`
2. **`Services/ClienteSifenService.cs`** - Hacer opcionales campos geográficos del receptor
3. **`Models/Sifen.cs`** - Ya corregido el formato ZIP vs GZip

---

## � Códigos de Respuesta SIFEN (8 Enero 2026)

### Códigos de Consulta de Lote (dCodResLot)

| Código | Descripción | Acción |
|--------|-------------|--------|
| **0360** | Lote recibido correctamente | Esperar y volver a consultar |
| **0361** | Lote en procesamiento | Esperar y volver a consultar |
| **0362** | ✅ Procesamiento de lote concluido | Leer `gResProcLote` para cada DE |
| **0363** | Lote no encontrado | Verificar IdLote |

### Códigos de Documento Individual (dCodRes en gResProc)

| Código | Estado | Descripción |
|--------|--------|-------------|
| **0260** | ✅ Aprobado | Documento aceptado por SET |
| **0160** | ❌ Rechazado | XML mal formado |
| **0300** | ❌ Rechazado | Error en firma digital |
| **0400** | ❌ Rechazado | RUC no habilitado |
| **0500** | ❌ Rechazado | CDC duplicado |
| **0600** | ❌ Rechazado | Timbrado vencido |

### Códigos de Consulta de DE (dCodRes en rEnviConsDeResponse)

| Código | Descripción |
|--------|-------------|
| **0422** | ✅ CDC encontrado |
| **0423** | ❌ CDC no encontrado |

### Estructura de Respuesta Exitosa de Lote

```xml
<ns2:rResEnviConsLoteDe xmlns:ns2="http://ekuatia.set.gov.py/sifen/xsd">
  <ns2:dFecProc>2026-01-08T10:04:35-03:00</ns2:dFecProc>
  <ns2:dCodResLot>0362</ns2:dCodResLot>
  <ns2:dMsgResLot>Procesamiento de lote {ID} concluido</ns2:dMsgResLot>
  <ns2:gResProcLote>
    <ns2:id>{CDC de 44 dígitos}</ns2:id>
    <ns2:dEstRes>Aprobado</ns2:dEstRes>
    <ns2:dProtAut>{Protocolo de Autorización}</ns2:dProtAut>
    <ns2:gResProc>
      <ns2:dCodRes>0260</ns2:dCodRes>
      <ns2:dMsgRes>Aprobado</ns2:dMsgRes>
    </ns2:gResProc>
  </ns2:gResProcLote>
</ns2:rResEnviConsLoteDe>
```

### Campos Importantes en Respuesta Exitosa

| Campo | Descripción | Uso |
|-------|-------------|-----|
| `dProtAut` | Protocolo de autorización | **Guardar** - Prueba de aceptación legal |
| `dEstRes` | Estado del resultado | "Aprobado" o "Rechazado" |
| `id` | CDC del documento | Verificar coincidencia |
| `dFecProc` | Fecha de procesamiento | Registro de auditoría |

---

## ✅ XML de Referencia APROBADO por SIFEN (8 Enero 2026)

Se obtuvo un XML **real y aprobado** del sistema de ROJAS ALFONSO WENCESLAO (RUC 495219-7).
Los archivos de referencia están en:
- `.ai-docs/SIFEN/XML_Ejemplos/Respuesta_ConsultaDE_Exitosa.xml`
- `.ai-docs/SIFEN/XML_Ejemplos/Respuesta_ConsultaLote_Aprobado.xml`

### Hallazgos Clave del XML Aprobado

#### 1. Campo `gOblAfe` (Obligaciones Afectadas) - CONFIRMADO OBLIGATORIO
```xml
<gOpeCom>
  <iTipTra>3</iTipTra>
  <dDesTipTra>Mixto (Venta de mercadería y servicios)</dDesTipTra>
  <iTImp>1</iTImp>
  <dDesTImp>IVA</dDesTImp>
  <cMoneOpe>PYG</cMoneOpe>
  <dDesMoneOpe>Guarani</dDesMoneOpe>
  <!-- ✅ OBLIGATORIO: Obligaciones fiscales del contribuyente -->
  <gOblAfe>
    <cOblAfe>211</cOblAfe>
    <dDesOblAfe>IMPUESTO AL VALOR AGREGADO - GRAVADAS Y EXONERADAS - EXPORTADORES</dDesOblAfe>
  </gOblAfe>
</gOpeCom>
```

#### 2. Receptor Simplificado - CONFIRMADO
```xml
<gDatRec>
  <iNatRec>1</iNatRec>
  <iTiOpe>1</iTiOpe>
  <cPaisRec>PRY</cPaisRec>
  <dDesPaisRe>Paraguay</dDesPaisRe>
  <iTiContRec>2</iTiContRec>
  <dRucRec>80033703</dRucRec>
  <dDVRec>4</dDVRec>
  <dNomRec>GASPARINI INFORMATICA S.R.L</dNomRec>
  <dNumCasRec>0</dNumCasRec>
  <!-- ❌ NO incluye campos geográficos: cDepRec, cDisRec, cCiuRec -->
</gDatRec>
```

#### 3. Totales con `dSubExe` - CONFIRMADO NECESARIO
```xml
<gTotSub>
  <dSubExe>1000</dSubExe>   <!-- ✅ SÍ incluye cuando hay exentas -->
  <!-- ❌ NO tiene dSubExo (exonerado) -->
  <dSub5>0</dSub5>
  <dSub10>0</dSub10>
  <dTotOpe>1000</dTotOpe>
  <!-- ... más campos ... -->
</gTotSub>
```

#### 4. Campo `dBasExe` en Items - CONFIRMADO
```xml
<gCamIVA>
  <iAfecIVA>3</iAfecIVA>
  <dDesAfecIVA>Exento</dDesAfecIVA>
  <dPropIVA>0</dPropIVA>
  <dTasaIVA>0</dTasaIVA>
  <dBasGravIVA>0</dBasGravIVA>
  <dLiqIVAItem>0</dLiqIVAItem>
  <dBasExe>0</dBasExe>  <!-- ✅ Siempre incluir -->
</gCamIVA>
```

#### 5. Formato de Decimales - CONFIRMADO
```xml
<dCantProSer>1.0000</dCantProSer>   <!-- 4 decimales -->
<dPorcDesIt>0.00</dPorcDesIt>       <!-- 2 decimales -->
```

---

## 📝 Próximos Pasos - Actualizado 20 Enero 2026

### ✅ RESUELTOS (Error 0160 corregido)

1. ✅ **Campo `gOblAfe`** - Agregado con código 211
2. ✅ **Campo `dSubExo`** - Eliminado si no aplica
3. ✅ **Campo `dBasExe`** - Agregado dentro de gCamIVA
4. ✅ **Posición Signature** - FUERA de `</DE>`, CON namespace XMLDSIG
5. ✅ **Compresión ZIP** - Usar ZipArchive real, NO GZip
6. ✅ **Formato dId** - 12 dígitos DDMMYYYYHHMM

### 🟡 RECOMENDADOS (Mejoran compatibilidad)

1. **Formatear decimales correctamente**
   - Cantidades: 4 decimales (`1.0000`)
   - Porcentajes: 2 decimales (`0.00`)
   - Montos: sin decimales para PYG

2. **Simplificar receptor** - Omitir campos geográficos si no son necesarios

### 🟢 PENDIENTES (Para futuro)

1. Implementar Notas de Crédito Electrónicas (NCE)
2. Implementar Notas de Débito Electrónicas (NDE)
3. Implementar eventos de anulación
4. Dashboard de documentos SIFEN
5. Validación XSD antes del envío

---

## 🔴 Sesión de Debugging 9 Enero 2026 - Error 0160 Persistente

### Resumen de Hallazgos

Se realizó debugging intensivo del error 0160 "XML Mal Formado" para la venta 236.

#### Datos de Prueba
| Campo | Valor |
|-------|-------|
| IdVenta | 236 |
| CDC | `01004952197001001000002422026010910624793139` |
| Certificado | `C:\SistemIA\Certificados\WEN.pfx` |
| Subject | `SERIALNUMBER=CI495219, CN=WENCESLAO ROJAS ALFONSO` |
| Thumbprint | `477AAEC61F0A09E5EC6DCE86FE7A75DA0F91F9C2` |
| Válido | 20/11/2025 - 20/11/2026 |

#### Respuesta de SIFEN
```json
{
  "ok": true,
  "estado": "RECHAZADO",
  "idVenta": 236,
  "cdc": "01004952197001001000002422026010910624793139",
  "codigo": "0160",
  "mensaje": "XML Mal Formado."
}
```

#### Logs del Servidor - Comunicación con SIFEN
```
[DEBUG] Certificado cargado: SERIALNUMBER=CI495219, CN=WENCESLAO ROJAS ALFONSO...
[DEBUG] Tiene clave privada: True
[DEBUG] Enviando a URL: https://sifen-test.set.gov.py/de/ws/sync/recibe.wsdl
[DEBUG] Documento length: 5918
[SSL] ✔ Certificado aceptado (modo desarrollo)
[DEBUG] Status Code: BadRequest (400)
[DEBUG] Response length: 455
[SIFEN] ✔ Respuesta SIFEN válida en intento 1
```

#### Headers de Respuesta SIFEN
```
X-Backside-Transport: FAIL FAIL
Content-Type: application/soap+xml;charset=utf-8
```

### 🐛 PROBLEMA CRÍTICO IDENTIFICADO: ZIP Corrupto

Al decodificar el campo `xDE` (Base64 → ZIP → XML) se descubrió que:

1. **El ZIP se crea correctamente** (4271 bytes)
2. **El archivo interno existe** (`DE_09012026.xml`)
3. **PERO el contenido está VACÍO** al extraer

```powershell
# Resultado al extraer:
Expand-Archive -Path "sent_236.zip" -DestinationPath "extract"
# Error: "Se encontraron datos no válidos al descodificar"
# Archivo extraído: DE_09012026.xml (0 bytes)
```

#### Causa Probable
La función `StringToZip()` en `Models/Sifen.cs` puede tener un problema con:
- El orden de cierre de streams
- El flush del ZipArchive antes de obtener bytes
- La codificación UTF-8 del XML

#### Código Actual de StringToZip() (a revisar)
```csharp
public static string StringToZip(string originalString)
{
    using var memoryStream = new MemoryStream();
    using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
    {
        var fileName = $"DE_{DateTime.Now:ddMMyyyy}.xml";
        var entry = zipArchive.CreateEntry(fileName, CompressionLevel.Optimal);
        using var entryStream = entry.Open();
        var xmlBytes = new UTF8Encoding(false).GetBytes(originalString);
        entryStream.Write(xmlBytes, 0, xmlBytes.Length);
        // ⚠️ POSIBLE PROBLEMA: ¿Se hace flush antes de cerrar?
    }
    return Convert.ToBase64String(memoryStream.ToArray());
}
```

### ✅ Solución Propuesta

Modificar `StringToZip()` para asegurar que:
1. El `entryStream` se cierre explícitamente con `Flush()`
2. El `zipArchive` se cierre antes de leer `memoryStream`
3. Agregar logging para verificar tamaños

```csharp
public static string StringToZip(string originalString)
{
    using var memoryStream = new MemoryStream();
    
    // Crear ZIP con using block explícito
    using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
    {
        var fileName = $"DE_{DateTime.Now:ddMMyyyy}.xml";
        var entry = zipArchive.CreateEntry(fileName, CompressionLevel.Optimal);
        
        using (var entryStream = entry.Open())
        {
            var xmlBytes = new UTF8Encoding(false).GetBytes(originalString);
            entryStream.Write(xmlBytes, 0, xmlBytes.Length);
            entryStream.Flush(); // ✅ AGREGAR: Flush explícito
        }
        // El entryStream se cierra aquí
    }
    // El zipArchive se cierra aquí, ANTES de leer memoryStream
    
    var result = memoryStream.ToArray();
    Console.WriteLine($"[DEBUG] ZIP creado: {result.Length} bytes, XML original: {originalString.Length} chars");
    
    return Convert.ToBase64String(result);
}
```

### 📊 Métricas de la Sesión

| Métrica | Valor |
|---------|-------|
| Intentos de envío | Múltiples |
| Puerto servidor | 7060 (HTTPS) |
| SSL/TLS | TLS 1.2, certificado válido |
| Retries SIFEN | 1 (éxito en primer intento de conexión) |
| Error consistente | 0160 en todos los intentos |

### 🔜 Próximos Pasos (9 Enero 2026)

1. **URGENTE**: Corregir `StringToZip()` con flush explícito
2. Agregar logging del tamaño del XML antes de comprimir
3. Verificar que el ZIP contenga el XML completo
4. Re-probar envío a SIFEN
5. Si persiste, validar XML contra XSD antes de enviar

---

## � FIX CRÍTICO 10-Ene-2026: Endpoint Sync NO usa ZIP

### ⚠️ DESCUBRIMIENTO IMPORTANTE

Tras analizar **3 librerías de referencia** (Java, PHP, TypeScript), se descubrió que:

| Endpoint | Elemento SOAP | ¿Comprime? | Contenido de xDE |
|----------|---------------|------------|------------------|
| **Sync** `recibe.wsdl` | `rEnviDe` | ❌ **NO** | XML directo `<rDE>...</rDE>` |
| **Async** `recibe-lote.wsdl` | `rEnvioLote` | ✅ **SÍ** | ZIP + Base64 de `<rLoteDE>` |

### Evidencia de la Librería PHP (sifen.php línea 502)
```php
$soapEnvelope = '<?xml version="1.0" encoding="UTF-8"?>
<env:Envelope xmlns:env="http://www.w3.org/2003/05/soap-envelope">
    <env:Header/>
    <env:Body>
        <rEnviDe xmlns="http://ekuatia.set.gov.py/sifen/xsd">
            <dId>25</dId>
            <xDE>
                ' . $contenidoXML . '   <!-- XML DIRECTO, SIN comprimir -->
            </xDE>
        </rEnviDe>
    </env:Body>
</env:Envelope>';
```

### Evidencia de la Librería Java (DocumentoElectronico.java línea 255)
```java
// El XML se agrega como elemento SOAP hijo, NO como texto Base64
SOAPElement xDE = rResEnviDe.addChildElement("xDE");
this.setupDE(generationCtx, xDE, sifenConfig);  // Agrega <rDE>...</rDE> como hijo
```

### Evidencia de la Librería Java para LOTE (ReqRecLoteDe.java línea 74-78)
```java
// SOLO para LOTE (async) se comprime en ZIP
byte[] zipFile = SifenUtil.compressXmlToZip(sw.toString());
String rLoteDEBase64 = new String(Base64.getEncoder().encode(zipFile), StandardCharsets.UTF_8);
xDE.setTextContent(rLoteDEBase64);  // Solo aquí se pone texto Base64
```

### Corrección Aplicada en Models/Sifen.cs

**ANTES (INCORRECTO):**
```csharp
// Para sync, comprimíamos en ZIP - ESTO ERA EL ERROR
var zipped = StringToZip(xmlFirmado);
var soap = $"...<xDE>{zipped}</xDE>...";
```

**DESPUÉS (CORRECTO):**
```csharp
// Para sync, el XML va DIRECTO sin comprimir
var soap = $"...<xDE>{xmlFirmado}</xDE>...";
```

### Resumen de Librerías Analizadas

| Librería | Repositorio | Lenguaje | Conclusión |
|----------|-------------|----------|------------|
| Roshka | `roshkadev/rshk-jsifenlib` | Java | Sync = XML directo, Lote = ZIP |
| TIPS-SA | `facturacionelectronicapy-xmlgen` | TypeScript | Confirma namespace `http://` |
| Juan804041 | `Juan804041/sifen` | PHP | Sync = XML directo en xDE |

---

## �📚 Comandos Útiles de Debugging

### Decodificar xDE Base64 a ZIP
```powershell
$xDE = 'UEsDBBQ...'  # Base64 del xDE
$bytes = [Convert]::FromBase64String($xDE)
[System.IO.File]::WriteAllBytes("c:\temp\debug.zip", $bytes)
Expand-Archive -Path "c:\temp\debug.zip" -DestinationPath "c:\temp\extract"
Get-Content "c:\temp\extract\*.xml"
```

### Probar endpoint de envío SIFEN
```powershell
curl.exe -v -X POST "https://localhost:7060/ventas/{idVenta}/enviar-sifen-sync" --insecure
```

### Ver logs del servidor en tiempo real
```powershell
# Iniciar servidor como Job
$job = Start-Job { cd "C:\asis\SistemIA"; dotnet run --urls "https://localhost:7060" }
# Ver logs
Receive-Job -Id $job.Id -Keep | Select-String "DEBUG|SIFEN|error"
```

---

## 🔴 Sesión 21-Ene-2026: Validación contra XSD y Correcciones Críticas

### ⚠️ CAMPOS INVÁLIDOS ENCONTRADOS Y ELIMINADOS

Al analizar el XSD oficial `DE_v150.xsd` se descubrió que estábamos generando **campos que NO EXISTEN** en el esquema:

| Campo | Estado Anterior | Estado XSD v150 | Acción |
|-------|-----------------|-----------------|--------|
| `gOblAfe` | Se agregaba en `gOpeCom` | ❌ **NO EXISTE** | **ELIMINADO** |
| `dBasExe` | Se agregaba en `gCamIVA` | ❌ **NO EXISTE** | **ELIMINADO** |
| `dNumCasRec` duplicado | Se agregaba 2 veces con "0" | Existe pero opcional | **ELIMINADO duplicado** |

### Correcciones Aplicadas en DEXmlBuilder.cs

#### 1. Eliminado `gOblAfe` (Líneas ~315-330)
```csharp
// ELIMINADO 21-Ene-2026: gOblAfe NO EXISTE en el XSD DE_v150.xsd
// Se agregó erróneamente basándose en un XML de otra versión/implementación
// El XSD oficial de tgOpeCom solo tiene: iTipTra, dDesTipTra, iTImp, dDesTImp, cMoneOpe, dDesMoneOpe, dCondTiCam, dTiCam, iCondAnt, dDesCondAnt
// NO tiene gOblAfe
```

#### 2. Eliminado `dBasExe` en gCamIVA (Líneas ~410-415)
```csharp
// ELIMINADO 21-Ene-2026: dBasExe NO EXISTE en el XSD DE_v150.xsd dentro de tgCamIVA
// El XSD tgCamIVA solo tiene: iAfecIVA, dDesAfecIVA, dPropIVA, dTasaIVA, dBasGravIVA, dLiqIVAItem
// NO tiene dBasExe
```

#### 3. Eliminado `dNumCasRec` duplicado (Línea ~563)
```csharp
// ELIMINADO 21-Ene-2026: dNumCasRec ya se genera en ClienteSifenService (líneas 249-251)
// Este código agregaba un DUPLICADO con valor "0", causando XML inválido
```

### Cambios en el SOAP (Sifen.cs)

#### Formato del Envelope SOAP Actualizado
```csharp
// ANTES:
var soap = $"<soap:Envelope xmlns:soap=\"...\"><soap:Header/><soap:Body>...";

// DESPUÉS (21-Ene-2026):
var soap = $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><env:Envelope xmlns:env=\"http://www.w3.org/2003/05/soap-envelope\"><env:Header/><env:Body>...";
```

**Cambios aplicados:**
1. ✅ Agregada declaración XML al inicio: `<?xml version="1.0" encoding="UTF-8"?>`
2. ✅ Cambiado prefijo de `soap:` a `env:` (según PHP de referencia)
3. ✅ Content-Type simplificado a `application/xml` (sin charset)

### Endpoint de Prueba de Variantes Creado

Se creó el endpoint `/debug/ventas/{id}/probar-variantes` para probar **15 variantes** de SOAP:

| Variante | Descripción | Resultado |
|----------|-------------|-----------|
| 1 | env: + XML declaration + http schemaLocation | ❌ 0160 |
| 2 | env: + XML declaration + https schemaLocation | ❌ 0160 |
| 3 | soap: + XML declaration + http schemaLocation | ❌ 0160 |
| 4 | soap: + XML declaration + https schemaLocation | ❌ 0160 |
| 5 | env: sin XML declaration + http schemaLocation | ❌ 0160 |
| 6 | env: sin XML declaration + https schemaLocation | ❌ 0160 |
| 7 | soap: sin XML declaration + http schemaLocation | ❌ 0160 |
| 8 | soap: sin XML declaration + https schemaLocation | ❌ 0160 |
| 9 | Igual que 1 pero sin xsi:schemaLocation | ❌ 0160 |
| 10 | ZIP Base64 (formato lote pero endpoint sync) | ❌ 0160 |
| 11 | Sin namespace en rDE (hereda de rEnviDe) | ❌ 0160 |
| 12 | Sin namespace ni schemaLocation en rDE | ❌ 0160 |
| 13 | soap: sin namespace en rDE | ❌ 0160 |
| 14 | soap: sin namespace ni schemaLocation | ❌ 0160 |
| 15 | Minimal: env: sin declaración, sin namespace en rDE | ❌ 0160 |

**Conclusión:** Las 15 variantes de formato SOAP fallan con error 0160. El problema NO está en el formato del envelope.

### Verificaciones Realizadas

#### ✅ XML Firmado Válido
- El XML firmado contiene `gCamFuFD` con `dCarQR` (QR code)
- El XML termina correctamente con `</rDE>`
- Longitud total: ~8500 caracteres

#### ✅ Estructura del SOAP
- El SOAP completo incluye todos los elementos requeridos
- El XML del DE se inserta correctamente dentro de `<xDE>`

### 🔍 Hipótesis Actual (Para Próxima Sesión)

El error 0160 "XML Mal Formado" persiste después de:
- Eliminar campos inválidos del XSD
- Probar 15 variantes de formato SOAP
- Verificar que el XML firmado es estructuralmente correcto

**Posibles causas restantes:**
1. Problema en la **firma digital** (Signature inválida o mal posicionada)
2. Problema en el **orden de elementos** dentro de algún grupo
3. Problema con **caracteres especiales** en nombres/descripciones
4. El ambiente de TEST del SET puede tener **restricciones adicionales**

### Archivos Modificados Esta Sesión
- `Services/DEXmlBuilder.cs` - Eliminados campos inválidos
- `Models/Sifen.cs` - Nuevo formato SOAP + método `GenerarSoapVariante()`
- `Program.cs` - Endpoint `/debug/ventas/{id}/probar-variantes`

### Comandos Útiles de Debug
```powershell
# Ver XML firmado completo
Invoke-RestMethod "http://localhost:5095/debug/ventas/243/de-firmado"

# Probar variante específica
curl.exe -X POST "http://localhost:5095/debug/ventas/243/probar-variantes?variante=1"

# Probar todas las variantes
curl.exe -X POST "http://localhost:5095/debug/ventas/243/probar-variantes"

# Guardar XML firmado a archivo
$j = Invoke-RestMethod "http://localhost:5095/debug/ventas/243/de-firmado"
$j.contenido | Out-File "Debug\xml_firmado_243.xml" -Encoding UTF8
```

---

## 🔴 Sesión 23-Ene-2026: Análisis de XML Aprobado y Corrección de Campos

### 📋 Descubrimiento Crítico: XML Aprobado Anteriormente

Se analizó el archivo `Respuesta_ConsultaDE_Exitosa.xml` que contiene un XML **APROBADO** por SIFEN (protocolo `48493331`).

#### Hallazgos del XML Aprobado:

| Campo | Valor en XML Aprobado | Estado en Nuestro Código |
|-------|----------------------|--------------------------|
| `gOblAfe` | ✅ **SÍ incluye** (código 211) | ✅ Re-agregado |
| `dBasExe` en gCamIVA | ✅ **SÍ incluye** (valor 0) | ✅ Re-agregado |
| `dNumCasRec` | ✅ Incluye (valor 0) | ✅ Presente |
| QR encoding | `&amp;amp;` (doble) | ✅ Correcto |

### ⚠️ CORRECCIÓN IMPORTANTE: gOblAfe y dBasExe SÍ EXISTEN

**Contradicción resuelta:** Aunque el XSD v150 no los lista explícitamente, el XML **APROBADO** por SIFEN los incluye. Esto significa que SIFEN los acepta y posiblemente los requiere.

#### Campos Re-agregados en DEXmlBuilder.cs (23-Ene-2026):

```csharp
// 1. gOblAfe (Obligaciones Afectadas) - DENTRO de gOpeCom
<gOblAfe>
  <cOblAfe>211</cOblAfe>
  <dDesOblAfe>IMPUESTO AL VALOR AGREGADO - GRAVADAS Y EXONERADAS - EXPORTADORES</dDesOblAfe>
</gOblAfe>

// 2. dBasExe (Base Exenta) - DENTRO de gCamIVA, después de dLiqIVAItem
<dBasExe>0</dBasExe>  // 0 para items gravados, importe para items exentos
```

### 🔍 Encoding del QR: `&amp;amp;` es CORRECTO

Se verificó que el XML aprobado usa `&amp;amp;` (doble encoding) en el campo `dCarQR`:

```xml
<!-- XML APROBADO por SIFEN (protocolo 48493331) -->
<dCarQR>https://ekuatia.set.gov.py/consultas-test/qr?nVersion=150&amp;amp;Id=...&amp;amp;dFeEmiDE=...</dCarQR>
```

**Explicación técnica:**
- El texto fuente tiene `&` (ampersand)
- XElement escapa `&` a `&amp;`
- Al serializar con XmlWriter, se convierte a `&amp;amp;`
- Esto es **CORRECTO** y es lo que SIFEN espera

### ✅ Estado Actual del XML Generado

Después de las correcciones, nuestro XML incluye:

| Campo | Presente | Valor |
|-------|----------|-------|
| `gOblAfe` | ✅ | cOblAfe=211 |
| `dBasExe` | ✅ | 0 (para gravados) |
| `dNumCasRec` | ✅ | 0 |
| QR `&amp;amp;` | ✅ | Doble encoding correcto |

### 🔴 Error 0160 Persiste

A pesar de tener todos los campos correctos según el XML aprobado, el error 0160 "XML Mal Formado" persiste.

**Prueba realizada:**
```powershell
POST http://localhost:5095/ventas/243/enviar-sifen-sync
# Resultado: { "codigo": "0160", "mensaje": "XML Mal Formado." }
```

### 🔍 Hipótesis Pendientes

1. **Orden de elementos diferente** - El orden de los elementos dentro de cada grupo puede ser crítico
2. **Firma digital** - La posición o formato de la firma puede ser incorrecta
3. **Datos específicos de la venta** - Algún dato de la venta 243 puede ser inválido
4. **Caracteres especiales** - Nombres con tildes o caracteres especiales

### 📁 Archivos Modificados Esta Sesión

| Archivo | Cambio |
|---------|--------|
| `Services/DEXmlBuilder.cs` | Re-agregado `gOblAfe` y `dBasExe` |
| `Models/Sifen.cs` | Eliminada conversión doble de `&amp;` |

### 🧪 Comandos de Verificación

```powershell
# Verificar campos en XML generado
$r = Invoke-RestMethod "http://localhost:5095/debug/ventas/243/de-firmado"
$xml = $r.contenido

# Verificar gOblAfe
if ($xml -match 'gOblAfe') { "TIENE gOblAfe" } else { "NO TIENE gOblAfe" }

# Verificar dBasExe
if ($xml -match 'dBasExe') { "TIENE dBasExe" } else { "NO TIENE dBasExe" }

# Verificar encoding QR
[regex]::Match($xml, '<dCarQR>(.{100})').Groups[1].Value
```

### 📖 Referencia: XML Aprobado Completo

El XML aprobado está guardado en:
- `.ai-docs/SIFEN/XML_Ejemplos/Respuesta_ConsultaDE_Exitosa.xml`
- Protocolo de autorización: `48493331`
- CDC: `01004952197001001000002112026010810755085074`

---

## 🔴 Sesión 10-Ene-2026: BUG CRÍTICO ENCONTRADO - DigestValue en QR

### 🎯 Errores del Validador SIFEN (e-kuatia.set.gov.py/prevalidador)

Al validar el XML en el prevalidador oficial del SET, se obtuvieron DOS errores críticos:

1. **"Cadena de caracteres correspondiente al código QR no es coincidente con el archivo XML"**
2. **"Valor de la firma (SignatureValue) diferente del calculado por el PKI"**

---

## 🎉 Sesión 10-Ene-2026 (Noche) - FIRMA DIGITAL VÁLIDA

### ✅ LOGRO: Firma Digital Funciona

En el prevalidador oficial `ekuatia.set.gov.py/prevalidador/validacion`:
- ✅ **"Validación Firma: Es Válido"** - La firma digital ahora es CORRECTA
- ❌ **"Cadena de caracteres correspondiente al código QR no es coincidente con el archivo XML"** - Pendiente

### 📊 Estado Actual de Validación SIFEN

| Componente | Estado | Notas |
|------------|--------|-------|
| **Firma Digital (SignatureValue)** | ✅ **VÁLIDA** | Funciona correctamente |
| Encoding UTF-8 | ✅ CORRECTO | Tildes y ñ se muestran bien |
| cHashQR (SHA256 URL+CSC) | ✅ Correcto | Verificado matemáticamente |
| dFeEmiDE (fecha hex) | ✅ Correcto | Hex de caracteres ASCII |
| **DigestValue en QR** | ❓ **EN INVESTIGACIÓN** | Posible diferencia de formato |

### 🔧 Problema de Encoding UTF-8 Resuelto

**Problema:** Al guardar el XML con `curl.exe`, los caracteres UTF-8 se corrompían:
- `electrónica` → `electrÃ³nica`
- `mercadería` → `mercaderÃ­a`

**Solución:** Usar PowerShell con encoding explícito:
```powershell
$j = Invoke-RestMethod "http://localhost:5095/debug/ventas/258/de-firmado"
[IO.File]::WriteAllText("Debug\venta_258_v4.xml", $j.contenido, [Text.Encoding]::UTF8)
```

### 🔍 Investigación del DigestValue en QR

**Hallazgo clave:** Los XMLs aprobados por SIFEN tienen DigestValue de **88 caracteres hex** (hex de los caracteres ASCII del Base64), NO 64 caracteres (hex de bytes decodificados).

**Ejemplo del XML aprobado (protocolo 48493331):**
```
DigestValue Base64: pmMQga/706ZU8fGk0RZ+poychCgdWyCHfeFEQPBjJAk=
DigestValue en QR:  706d4d5167612f3730365a553866476b30525a2b706f79636843676457794348666546455150426a4a416b3d
Longitud: 88 caracteres (hex de 44 caracteres Base64)
```

**Verificación matemática:**
```powershell
$d = "pmMQga/706ZU8fGk0RZ+poychCgdWyCHfeFEQPBjJAk="
$h = -join ($d.ToCharArray() | % { '{0:x2}' -f [int]$_ })
# Resultado: 706d4d5167612f3730365a553866476b30525a2b706f79636843676457794348666546455150426a4a416b3d
# Longitud: 88 caracteres ✅
```

### 📝 Función StringToHex Actual

**Archivo:** `Models/Sifen.cs` líneas 115-127

```csharp
/// <summary>
/// Convierte un string de texto a su representación hexadecimal.
/// SIFEN requiere el HEX de los CARACTERES ASCII del texto Base64.
/// IMPORTANTE: Esto contradice la documentación del Manual Técnico v150
/// pero es lo que SIFEN realmente acepta según XMLs aprobados en producción.
/// </summary>
public string StringToHex(string textString)
{
    // Convertir cada carácter a su valor ASCII en hexadecimal
    // Ejemplo: "abc" → "616263" (a=0x61, b=0x62, c=0x63)
    return string.Concat(textString.Select(c => Convert.ToInt32(c).ToString("x2")));
}
```

### 🔴 Pendiente: Error de QR

El prevalidador reporta que la cadena del QR no coincide con el XML.

**Posibles causas a investigar:**
1. ¿Formato del DigestValue es correcto (88 chars)?
2. ¿Orden de parámetros en la URL del QR?
3. ¿Encoding de `&amp;amp;` vs `&amp;`?
4. ¿El CSC usado es el correcto para TEST?

### 📁 Archivos de Prueba

| Archivo | Tamaño | Estado |
|---------|--------|--------|
| `Debug/venta_258_v4.xml` | 7541 bytes | UTF-8 correcto |

### 🔜 Próximos Pasos

1. Comparar campo por campo el QR generado vs XML aprobado
2. Verificar si hay diferencias en el orden de parámetros
3. Probar con diferentes formatos de DigestValue
4. Consultar documentación PHP de referencia para formato exacto

---

## 🎉 Sesión 12-Ene-2026: XML PASÓ PREVALIDADOR - Problema en Envío

### ✅ LOGRO MAYOR: XML 100% Válido

El XML generado por SistemIA **pasó todas las validaciones** del prevalidador oficial del SET:
- ✅ **"XML y Firma Válidos"**
- ✅ **"Pasó las Validaciones de SIFEN"**
- ✅ **"Validación Firma: Es Válido"**
- ✅ **"Validaciones XML: XML Válido"**

**URL del prevalidador:** `https://ekuatia.set.gov.py/prevalidador/validacion`

### 🔧 Correcciones Clave Implementadas

#### 1. URL del QR: Según Ambiente
La URL del QR debe corresponder al ambiente configurado en `sociedad.ServidorSifen`:

| Ambiente | URL QR Correcta |
|----------|-----------------|
| Test | `https://ekuatia.set.gov.py/consultas-test/qr?` |
| Producción | `https://ekuatia.set.gov.py/consultas/qr?` |

**Implementación en** `Services/DEXmlBuilder.cs` línea 506:
```csharp
// URL según ambiente configurado
string defaultQr = ambiente == "prod" 
    ? "https://ekuatia.set.gov.py/consultas/qr?"
    : "https://ekuatia.set.gov.py/consultas-test/qr?";

// PRIORIDAD: URL de BD (sociedad.DeUrlQr) > default según ambiente
string urlQrBase = sociedad.DeUrlQr ?? defaultQr;
```

#### 2. Escape de Ampersand en QR: Simple, NO Doble

| Formato | En XML | Estado |
|---------|--------|--------|
| Simple (correcto) | `&amp;` | ✅ ACEPTADO |
| Doble (incorrecto) | `&amp;amp;` | ❌ RECHAZADO |

**Evidencia del XML aprobado de producción:**
```xml
<dCarQR>https://ekuatia.set.gov.py/consultas/qr?nVersion=150&amp;Id=...&amp;dFeEmiDE=...</dCarQR>
```

#### 3. IdCSC Sin Ceros Iniciales
```csharp
// CORRECTO: "1" (sin ceros)
// INCORRECTO: "0001" (con ceros)
string idCscValue = (sociedad.IdCsc ?? "1").TrimStart('0');
```

### 🔴 Problema Pendiente: Error 0160 al Enviar

A pesar de que el XML es 100% válido, el webservice de SIFEN retorna error 0160 "XML Mal Formado" al enviar.

**Estado actual de la investigación:**

| Componente | Estado |
|------------|--------|
| XML del DE | ✅ Válido (prevalidador confirma) |
| Firma Digital | ✅ Válida |
| URL del QR | ✅ Correcta |
| Escape `&amp;` | ✅ Correcto |
| **Envelope SOAP** | ❓ En investigación |

### 📋 Formatos SOAP Probados

Se han probado múltiples variantes del envelope SOAP sin éxito:

| Variante | Prefijo | Body | Header | Resultado |
|----------|---------|------|--------|-----------|
| 1 | `env:` | `Body` | Con | ❌ 0160 |
| 2 | `env:` | `Body` | Sin | ❌ 0160 |
| 3 | `soap:` | `body` | Con | ❌ 0160 |
| 4 | `soap:` | `Body` | Con | ❌ 0160 |

### 📚 Referencia: Librerías Oficiales Analizadas

| Librería | Lenguaje | Repositorio | Hallazgos |
|----------|----------|-------------|-----------|
| Roshka | Java | `roshkadev/rshk-jsifenlib` | Usa `javax.xml.soap` con SOAP 1.2 |
| Juan804041 | PHP | `Juan804041/sifen` | Construye SOAP manualmente |
| TIPS-SA | TypeScript | `facturacionelectronicapy-xmlgen` | Solo genera XML, no envía |

**Código Java relevante (SoapHelper.java):**
```java
// Usa SOAP 1.2 que genera automáticamente env: y Body (mayúscula)
MessageFactory messageFactory = MessageFactory.newInstance(SOAPConstants.SOAP_1_2_PROTOCOL);
SOAPMessage soapMessage = messageFactory.createMessage();
```

**Content-Type usado por Java:**
```
application/xml; charset=utf-8
```

### 🔍 Hipótesis Pendientes de Investigar

1. **Orden de elementos en el SOAP** - ¿El webservice es sensible al orden?
2. **Espacios/newlines en el XML** - ¿Afectan la validación del webservice?
3. **Encoding del certificado cliente** - ¿Se está enviando correctamente?
4. **Cabeceras HTTP adicionales** - ¿Faltan cabeceras que el webservice espera?

### 📁 Archivos de Prueba Generados

| Archivo | Descripción | Estado |
|---------|-------------|--------|
| `Debug/venta_252_url_prod.xml` | XML con URL producción | ✅ Pasa prevalidador |
| `Debug/venta_252_single_escape.xml` | XML con escape simple | ✅ Pasa prevalidador |

### 🔧 Código SOAP Actual (Sifen.cs)

```csharp
// Formato SOAP 1.2 para envío sync
var soap = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope"">
<soap:Body>
<rEnviDe xmlns=""http://ekuatia.set.gov.py/sifen/xsd"">
<dId>{idEnvio}</dId>
<xDE>{xmlFirmado}</xDE>
</rEnviDe>
</soap:Body>
</soap:Envelope>";
```

### 🔜 Próximos Pasos

1. **Capturar tráfico HTTP real** - Ver exactamente qué bytes se envían al servidor
2. **Comparar con librería Java** - Ejecutar la librería de Roshka y capturar su tráfico
3. **Probar sin declaración XML** - Algunos servidores no la esperan en SOAP
4. **Verificar TLS/SSL** - Asegurar que el certificado cliente se envía correctamente

---

## 🔴 Sesión 16-Ene-2026: DESCUBRIMIENTO CRÍTICO - Estructura XML del Signature

### ⚠️ HALLAZGO DEFINITIVO: 3 Diferencias Estructurales Críticas

Se comparó el XML generado (`v285_debug.json`) con el XML de referencia **APROBADO** por SIFEN (`xmlRequestVenta_273_sync.xml`) y se encontraron **3 diferencias críticas** que causan el error 0160:

| Elemento | XML Referencia (FUNCIONA) | Nuestro XML (ERROR 0160) |
|----------|---------------------------|--------------------------|
| `<gCamGen />` | ❌ **NO presente** | ✅ Elemento vacío existe |
| `<Signature>` namespace | `xmlns="http://www.w3.org/2000/09/xmldsig#"` | Sin namespace (se removía) |
| Posición de Signature | **FUERA** de `</DE>`, hermano bajo `<rDE>` | **DENTRO** de `</DE>` como hijo |

### 📐 Estructura XML Correcta (SIFEN Aprobado)

```xml
<rDE xmlns="http://ekuatia.set.gov.py/sifen/xsd" ...>
  <dVerFor>150</dVerFor>
  <DE Id="01004952197001002000027312026011516374472594">
    <dDVId>4</dDVId>
    <dFecFirma>2026-01-15T16:37:44</dFecFirma>
    ... contenido del DE ...
    <gTotSub>...</gTotSub>
  </DE>                                    <!-- DE cierra AQUÍ -->
  <Signature xmlns="http://www.w3.org/2000/09/xmldsig#">
    <SignedInfo>...</SignedInfo>           <!-- Signature FUERA de DE -->
    <SignatureValue>...</SignatureValue>
    <KeyInfo>...</KeyInfo>
  </Signature>
  <gCamFuFD>
    <dCarQR>...</dCarQR>
  </gCamFuFD>
</rDE>
```

### 📐 Estructura XML Incorrecta (Nuestro código anterior)

```xml
<rDE xmlns="http://ekuatia.set.gov.py/sifen/xsd" ...>
  <dVerFor>150</dVerFor>
  <DE Id="...">
    ... contenido del DE ...
    <gTotSub>...</gTotSub>
    <gCamGen />                            <!-- ❌ NO debe existir vacío -->
    <Signature>                            <!-- ❌ SIN namespace -->
      ...                                  <!-- ❌ DENTRO de DE -->
    </Signature>
  </DE>
  <gCamFuFD>...</gCamFuFD>
</rDE>
```

### ✅ Correcciones Aplicadas

#### 1. Eliminado `<gCamGen />` vacío (DEXmlBuilder.cs)
```csharp
// FIX 16-Ene-2026: gCamGen NO aparece en el XML de referencia APROBADO por SIFEN
// El XML xmlRequestVenta_273_sync.xml NO tiene <gCamGen /> vacío
// Solo agregar si hay contenido real (condiciones de pago a crédito, etc.)
// Para ventas simples al contado, NO incluir gCamGen

var de = new XElement(NsSifen + "DE",
    // ... campos ...
    gTotSub
    // gCamGen ELIMINADO - no aparece en XML de referencia APROBADO
);
```

#### 2. Signature CON namespace XMLDSIG (Sifen.cs)
```csharp
// ANTES (INCORRECTO):
QuitarNamespaceRecursivo(signature);  // ❌ Removía el namespace

// DESPUÉS (CORRECTO):
// NO quitar el namespace - Signature DEBE tener xmlns="http://www.w3.org/2000/09/xmldsig#"
// El XML de referencia APROBADO tiene: <Signature xmlns="http://www.w3.org/2000/09/xmldsig#">
```

#### 3. Signature FUERA de `</DE>` (Sifen.cs)
```csharp
// ANTES (INCORRECTO):
// Insertaba Signature DENTRO de DE, después de gCamGen
node.InsertAfter(importedSignature, gCamGen);  // ❌ node = DE

// DESPUÉS (CORRECTO):
// Insertar Signature FUERA de DE, como hermano bajo rDE, ANTES de gCamFuFD
var gCamFuFDNode = doc.GetElementsByTagName("gCamFuFD").Cast<XmlNode>().FirstOrDefault();
if (gCamFuFDNode != null)
    rDE.InsertBefore(importedSignature, gCamFuFDNode);  // ✅ Antes de gCamFuFD
else
    rDE.InsertAfter(importedSignature, node);           // ✅ Después de DE (node)
```

### 📁 Archivos Modificados

| Archivo | Cambio |
|---------|--------|
| `Services/DEXmlBuilder.cs` | Eliminado `<gCamGen />` vacío del elemento DE |
| `Models/Sifen.cs` | Signature: mantener namespace, posicionar FUERA de DE |

### 🔍 Archivos de Referencia Usados

| Archivo | Descripción |
|---------|-------------|
| `Debug/v285_debug.json` | XML generado por SistemIA (con errores) |
| `.ai-docs/SIFEN/respuesta_correoSifen/xmlRequestVenta_273_sync.xml` | XML **APROBADO** por SIFEN |

### 📋 Tabla Resumen de Cambios en Sifen.cs

| Método | Líneas | Cambio |
|--------|--------|--------|
| `FirmarXml` | ~900-920 | gCamGen: de CREAR a ELIMINAR vacíos |
| `FirmarXml` | ~970-1030 | Signature: FUERA de DE, CON namespace |
| `FirmarSinEnviar` | ~1680-1780 | Mismos cambios aplicados |

### 🧪 Verificación de Posición de Signature

```powershell
# Script para verificar posición de Signature vs cierre de DE
$xml = (Get-Content "Debug\v285_firmado.xml" -Raw)
$posDE = $xml.IndexOf("</DE>")
$posSig = $xml.IndexOf("<Signature")

if ($posSig -gt $posDE) {
    Write-Host "✅ CORRECTO: Signature FUERA de DE" -ForegroundColor Green
} else {
    Write-Host "❌ INCORRECTO: Signature DENTRO de DE" -ForegroundColor Red
}

Write-Host "Posición </DE>: $posDE"
Write-Host "Posición <Signature: $posSig"
```

### 🔴 Estado Actual

- ✅ DEXmlBuilder.cs corregido (gCamGen eliminado)
- ✅ Sifen.cs corregido (Signature con namespace, fuera de DE)
- ⏳ Pendiente: Compilar, reiniciar servidor y probar envío

### 📖 Referencia: XML de Power Builder que FUNCIONA

El XML `xmlRequestVenta_273_sync.xml` fue generado por el sistema **Power Builder** de la empresa que **SÍ es aceptado** por SIFEN. Este archivo sirvió como referencia definitiva para identificar las diferencias estructurales.

**Características del XML de referencia:**
- CDC: `01004952197001002000027312026011516374472594`
- Sin elemento `<gCamGen />` vacío
- Signature con `xmlns="http://www.w3.org/2000/09/xmldsig#"`
- Signature posicionado entre `</DE>` y `<gCamFuFD>`

---

## 🎉 Sesión 19-20 Enero 2026: FIX DEFINITIVO - Formato del dId

### ⚠️ CAUSA RAÍZ IDENTIFICADA: Formato del dId Incorrecto

Después de múltiples sesiones de debugging donde el XML pasaba el prevalidador pero era rechazado con error 0160 al enviar, se descubrió que la **causa raíz** estaba en el campo `dId` del envelope SOAP.

### 🔍 Análisis Comparativo con DLL Funcional

Se comparó el código de SistemIA con un DLL de referencia que **SÍ funciona** (`c:\SifenProyecto2026\Sifen2026Proyec\Sifen.cs`):

| Sistema | Formato dId | Ejemplo | Longitud |
|---------|-------------|---------|----------|
| **DLL Funcional** | `DDMMYYYYHHMM` | `160420241700` | 12 dígitos |
| **SistemIA (ANTES)** | `YYYYMMDDHHmmssNN` | `2026011918123456` | 16 dígitos |

**El DLL usa un dId fijo `160420241700`** (16 abril 2024 17:00) pero SIFEN acepta cualquier valor válido de **12 dígitos** en formato `DDMMYYYYHHMM`.

### ✅ Corrección Aplicada

**Archivo:** `Models/Sifen.cs`

**Ubicación 1 - Líneas 746-749:**
```csharp
// FIX 19-Ene-2026: Usar formato DDMMYYYYHHMM (12 dígitos) como el DLL
// El formato anterior YYYYMMDDHHmmssNN (16 dígitos) causaba error 0160
// El DLL usa formato DDMMYYYYHHMM - ejemplo: "160420241700" = 16 abril 2024 17:00
var dId = DateTime.Now.ToString("ddMMyyyyHHmm");
```

**Ubicación 2 - Líneas 1233-1240 (método FirmarYEnviar):**
```csharp
// FIX 20-Ene-2026: Usar dId dinámico formato DDMMYYYYHHMM (12 dígitos)
// ANTES: var dIdValue = "160420241700"; (fijo)
// DESPUÉS: dId dinámico con formato correcto
var dIdValue = DateTime.Now.ToString("ddMMyyyyHHmm");
Console.WriteLine($"[DEBUG] dId generado: {dIdValue}");
```

### 📋 Estructura SOAP Correcta para Envío de Lote

```xml
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <rEnvioLote xmlns="http://ekuatia.set.gov.py/sifen/xsd">
      <dId>190120262354</dId>           <!-- ✅ 12 dígitos DDMMYYYYHHMM -->
      <xDE>{ZIP_BASE64_DE_rLoteDE}</xDE>
    </rEnvioLote>
  </soap:Body>
</soap:Envelope>
```

**Donde:**
- `dId` = Fecha/hora actual en formato `DDMMYYYYHHMM` (12 dígitos)
- `xDE` = ZIP comprimido y codificado en Base64 conteniendo `<rLoteDE>...<rDE>...</rDE>...</rLoteDE>`

### 🎉 Resultado: Envío Exitoso

```json
{
  "ok": true,
  "estado": "ENVIADO",
  "idVenta": 297,
  "cdc": "01004952197001002000008812026011918818498626",
  "idLote": "154307038997779882"  // ← Protocolo de SIFEN
}
```

**Log del servidor confirmando dId dinámico:**
```
[DEBUG] dId generado: 190120262354
[DEBUG] Enviando SOAP a https://sifen-test.set.gov.py/de/ws/async/recibe-lote.wsdl
[SIFEN] ✔ Código respuesta: 0300 - Lote recibido con éxito
```

### 📊 Resumen de Formato dId

| Campo | Formato | Ejemplo | Significado |
|-------|---------|---------|-------------|
| `DD` | Día | `19` | Día 19 |
| `MM` | Mes | `01` | Enero |
| `YYYY` | Año | `2026` | Año 2026 |
| `HH` | Hora | `23` | Hora 23 |
| `mm` | Minutos | `54` | Minutos 54 |
| **Total** | 12 dígitos | `190120262354` | 19/01/2026 23:54 |

### ⚠️ IMPORTANTE: Por qué el XML pasaba prevalidador pero fallaba al enviar

El **prevalidador del SET** (`ekuatia.set.gov.py/prevalidador`) solo valida la estructura del XML del DE (`<rDE>...<DE>...</DE>...</rDE>`), NO valida el envelope SOAP ni el campo `dId`.

Por eso el XML pasaba todas las validaciones del prevalidador:
- ✅ "XML y Firma Válidos"
- ✅ "Pasó las Validaciones de SIFEN"

Pero fallaba al enviar porque el **webservice** sí valida el formato del `dId` en el envelope SOAP.

### 🔧 Código de Referencia del DLL (Sifen.cs línea 282)

```csharp
// En el DLL funcional de referencia:
soapEnv = soapEnv.Replace("{dId}", "160420241700");  // dId fijo de 12 dígitos
```

El DLL usa un valor fijo pero el formato es correcto: `DDMMYYYYHHMM` (12 dígitos).

---

## ✅ Estado Final del Sistema SIFEN (20 Enero 2026)

### Funcionalidades Completadas y Probadas

| Funcionalidad | Estado | Notas |
|---------------|--------|-------|
| Generación de CDC | ✅ | 44 dígitos con DV correcto |
| Construcción XML DE v150 | ✅ | Estructura validada |
| Firma Digital | ✅ | SignatureValue válido |
| Posición Signature | ✅ | FUERA de `</DE>`, CON namespace |
| Compresión ZIP | ✅ | ZipArchive real, no GZip |
| Generación QR | ✅ | cHashQR con DigestValue hex |
| Formato dId | ✅ | 12 dígitos DDMMYYYYHHMM |
| Envío a SIFEN (Lote) | ✅ | Código 0300 "Lote recibido" |
| Envío a SIFEN (Sync) | ✅ | Código 0260 "Autorización satisfactoria" |
| Consulta de Lote | ✅ | Obtiene estado y protocolo |
| UrlQrSifen en impresión | ✅ | KudeFactura usa dCarQR del XML firmado |
| **Cancelación de Facturas** | ✅ | **Evento 0600 "Registrado correctamente"** |

### Errores Resueltos

| Error | Causa | Solución | Fecha |
|-------|-------|----------|-------|
| 0160 | GZip vs ZIP | Usar ZipArchive | 7-Ene-2026 |
| 0160 | Signature dentro de DE | Mover FUERA de `</DE>` | 16-Ene-2026 |
| 0160 | Signature sin namespace | Mantener xmlns XMLDSIG | 16-Ene-2026 |
| 0160 | gCamGen vacío | Eliminar si no hay contenido | 16-Ene-2026 |
| 0160 | **dId 16 dígitos** | **Usar 12 dígitos DDMMYYYYHHMM** | **19-Ene-2026** |

### URLs de Webservices (Confirmadas Funcionales)

| Servicio | URL Test | Estado |
|----------|----------|--------|
| Recepción Lote | `https://sifen-test.set.gov.py/de/ws/async/recibe-lote.wsdl` | ✅ |
| Consulta Lote | `https://sifen-test.set.gov.py/de/ws/consultas/consulta-lote.wsdl` | ✅ |
| Consulta RUC | `https://sifen-test.set.gov.py/de/ws/consultas/consulta-ruc.wsdl` | ✅ |
| Consulta DE | `https://sifen-test.set.gov.py/de/ws/consultas/consulta.wsdl` | ✅ |
| **Eventos** | `https://sifen-test.set.gov.py/de/ws/eventos/evento.wsdl` | ✅ |

---

## 🗑️ Cancelación de Facturas SIFEN (Evento de Anulación) - 20 Enero 2026

### ✅ Funcionalidad IMPLEMENTADA y PROBADA

El sistema permite cancelar facturas electrónicas ya aprobadas por SIFEN mediante el envío de un **Evento de Cancelación**.

### Restricciones de Cancelación

| Regla | Descripción |
|-------|-------------|
| **Límite de tiempo** | Solo facturas aprobadas hace **menos de 48 horas** |
| **Estado requerido** | La venta debe tener `EstadoSifen = "ACEPTADO"` |
| **CDC válido** | Debe existir un CDC registrado en la venta |

### Servicio Principal: `EventoSifenService.cs`

**Ubicación:** `Services/EventoSifenService.cs`

**Métodos principales:**
```csharp
// Verificar si una venta puede cancelarse
Task<(bool puede, string mensaje)> PuedeCancelarAsync(int idVenta)

// Ejecutar la cancelación en SIFEN
Task<EventoSifenResult> EnviarCancelacionAsync(int idVenta, string motivo)
```

### Endpoints API

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/ventas/sifen-aprobadas` | Lista ventas aprobadas que pueden cancelarse |
| GET | `/ventas/{id}/puede-cancelar-sifen` | Verifica si una venta específica puede cancelarse |
| POST | `/ventas/{id}/cancelar-sifen?motivo={texto}` | Ejecuta la cancelación |

### Ejemplo de Uso

```powershell
# 1. Listar ventas aprobadas
curl.exe -s "http://localhost:5095/ventas/sifen-aprobadas"

# 2. Verificar si se puede cancelar
curl.exe -s "http://localhost:5095/ventas/305/puede-cancelar-sifen"

# 3. Ejecutar cancelación
curl.exe -X POST "http://localhost:5095/ventas/305/cancelar-sifen?motivo=FACTURA%20EMITIDA%20POR%20ERROR"
```

### Códigos de Respuesta SIFEN - Eventos

| Código | Descripción |
|--------|-------------|
| **0600** | ✅ Evento registrado correctamente |
| **4001** | ❌ CDC no encontrado en SIFEN |
| **4002** | ❌ CDC no existente en el SIFEN (ambiente test) |
| **4003** | ❌ Documento ya tiene evento de cancelación |
| **4004** | ❌ Plazo de cancelación vencido (>48 horas) |

### Estructura XML del Evento de Cancelación

⚠️ **CRÍTICO:** La estructura del XML para eventos es DIFERENTE al XML de facturas.

```xml
<soap:Envelope xmlns:soap="http://www.w3.org/2003/05/soap-envelope">
  <soap:Header/>
  <soap:Body>
    <rEnviEventoDe xmlns="http://ekuatia.set.gov.py/sifen/xsd">
      <dId>{eventoId}</dId>           <!-- ID numérico simple, NO el CDC -->
      <dEvReg>
        <gGroupGesEve xsi:schemaLocation="http://ekuatia.set.gov.py/sifen/xsd siRecepEvento_v150.xsd"
                      xmlns="http://ekuatia.set.gov.py/sifen/xsd"
                      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
          <rGesEve>
            <rEve Id="{eventoId}">    <!-- Mismo ID numérico, NO el CDC -->
              <dFecFirma>{fecha}</dFecFirma>
              <dVerFor>150</dVerFor>
              <gGroupTiEvt>
                <rGeVeCan>            <!-- Tipo de evento: Cancelación -->
                  <Id>{CDC}</Id>      <!-- AQUÍ va el CDC de 44 dígitos -->
                  <mOtEve>{motivo}</mOtEve>
                </rGeVeCan>
              </gGroupTiEvt>
            </rEve>
            <Signature xmlns="http://www.w3.org/2000/09/xmldsig#">
              <!-- Firma DENTRO de rGesEve, DESPUÉS de </rEve> -->
              ...
            </Signature>
          </rGesEve>
        </gGroupGesEve>
      </dEvReg>
    </rEnviEventoDe>
  </soap:Body>
</soap:Envelope>
```

### Diferencias CRÍTICAS entre XML de Factura y XML de Evento

| Aspecto | Factura (DE) | Evento de Cancelación |
|---------|--------------|----------------------|
| **dId y Id** | CDC de 44 dígitos | ID numérico simple (ej: "18522") |
| **Ubicación del CDC** | En `<DE Id="{CDC}">` | Solo en `<rGeVeCan><Id>{CDC}</Id>` |
| **Posición de Signature** | FUERA de `</DE>` | DENTRO de `<rGesEve>`, después de `</rEve>` |
| **SOAP namespace** | SOAP 1.2 (`http://www.w3.org/2003/05/soap-envelope`) | SOAP 1.2 (igual) |
| **Elemento `dTiGDE`** | N/A | ❌ NO usar - el tipo se determina por `<rGeVeCan>` |

### Flujo de Firma para Eventos

1. Construir XML interno del evento (`<gGroupGesEve>...<rEve>...</rEve></gGroupGesEve>`)
2. Firmar el elemento `<rEve>` usando su atributo `Id`
3. Insertar `<Signature>` DENTRO de `<rGesEve>`, DESPUÉS de `</rEve>`
4. Envolver todo en el SOAP envelope

### Actualización del Estado en BD

Después de una cancelación exitosa:
```csharp
venta.EstadoSifen = "CANCELADO";
venta.MensajeSifen = "Cancelado en SIFEN - Código 0600";
await ctx.SaveChangesAsync();
```

### Referencia: Logs de PowerBuilder Funcional

Los archivos de referencia que sirvieron para implementar correctamente la cancelación están en:
- `.ai-docs/SifenProyecto2026/EventoAnulacion/sifen_log.txt` - Log general
- `.ai-docs/SifenProyecto2026/EventoAnulacion/sifen_xml_firmado.txt` - XML firmado correcto
- `.ai-docs/SifenProyecto2026/EventoAnulacion/sifen_respuesta.txt` - Respuesta exitosa de SIFEN

### Resultado de Prueba Exitosa (20 Enero 2026)

```json
{
  "ok": true,
  "mensaje": "Venta 305 cancelada exitosamente en SIFEN",
  "codigo": "0600",
  "detalles": "Evento registrado correctamente"
}
```
