# RESUMEN DEL PROBLEMA Y SOLUCIONES - SIFEN

**Fecha:** 14 de enero de 2026  
**Sistema:** Windows con .NET Framework / Blazor Server C#  
**Componente:** Firma digital de documentos electrónicos SIFEN

---

## 🔴 PROBLEMAS PRINCIPALES

### Error 1: CryptographicException (Firma Digital)
```
CryptographicException - Se ha especificado un tipo de proveedor no válido.
```

### Error 2: XML Mal Formado (Código 0160)
```xml
<ns2:dCodRes>0160</ns2:dCodRes>
<ns2:dMsgRes>XML Mal Formado.</ns2:dMsgRes>
```
**Respuesta del servidor SIFEN:**
```xml
<?xml version="1.0" encoding="UTF-8"?>
<env:Envelope xmlns:env="http://www.w3.org/2003/05/soap-envelope">
  <env:Body>
    <ns2:rRetEnviDe xmlns:ns2="http://ekuatia.set.gov.py/sifen/xsd">
      <ns2:rProtDe>
        <ns2:dFecProc>2026-01-14T14:00:18-03:00</ns2:dFecProc>
        <ns2:dEstRes>Rechazado</ns2:dEstRes>
        <ns2:gResProc>
          <ns2:dCodRes>0160</ns2:dCodRes>
          <ns2:dMsgRes>XML Mal Formado.</ns2:dMsgRes>
        </ns2:gResProc>
      </ns2:rProtDe>
    </ns2:rRetEnviDe>
  </env:Body>
</env:Envelope>
```

### Stack Trace
```
en System.Security.Cryptography.Utils.CreateProvHandle(CspParameters parameters, Boolean randomKeyContainer)
en System.Security.Cryptography.Utils.GetKeyPairHelper(...)
en System.Security.Cryptography.RSACryptoServiceProvider.GetKeyPair()
en System.Security.Cryptography.RSACryptoServiceProvider..ctor(Int32 dwKeySize, CspParameters parameters, Boolean useDefaultKeySize)
en System.Security.Cryptography.X509Certificates.X509Certificate2.get_PrivateKey()
en System.Security.Cryptography.X509Certificates.RSACertificateExtensions.GetRSAPrivateKey(X509Certificate2 certificate)
```

### Ubicación del Error
**Archivo:** [Sifen.cs](Sifen.cs#L371-L375)  
**Línea:** Aproximadamente línea 371

```csharp
// Usar GetRSAPrivateKey() en lugar de cert.PrivateKey
RSA rsaKey = cert.GetRSAPrivateKey();  // ← FALLA AQUÍ

// Crear RSACryptoServiceProvider con proveedor AES (24)
RSACryptoServiceProvider key = new RSACryptoServiceProvider(new CspParameters(24));
```

---

## 📊 DIAGNÓSTICO

### Causa Raíz
El error ocurre porque se está intentando crear un `RSACryptoServiceProvider` con el proveedor CSP tipo **24** (Microsoft Enhanced RSA and AES Cryptographic Provider), pero:

1. **El certificado PFX** puede estar usando un proveedor CSP diferente o incompatible
2. **Windows moderno** (2025-2026) puede tener restricciones en proveedores CSP legacy
3. **Conflicto entre APIs**: Se usa `GetRSAPrivateKey()` (API moderna) pero luego se intenta crear `RSACryptoServiceProvider` (API legacy) con CSP tipo 24

### Evidencia del Log
```
2026-01-14 14:00:14 - Certificado para FIRMA: SERIALNUMBER=CI495219, CN=WENCESLAO ROJAS ALFONSO...
2026-01-14 14:00:14 - Certificado HasPrivateKey: True
2026-01-14 14:00:14 - Certificado Serial: 00E9312C9B1DB909AC49B307D8A7A6AE8B
2026-01-14 14:00:14 - ERROR en firmarYEnviar: CryptographicException
```

El certificado **SÍ tiene clave privada**, pero falla al intentar acceder a ella con el proveedor CSP tipo 24.

---

## ✅ SOLUCIONES PROPUESTAS

### SOLUCIÓN 1: Usar API Moderna Completamente (RECOMENDADA) ⭐

**Descripción:** Eliminar completamente el uso de `RSACryptoServiceProvider` y usar solo las APIs modernas de .NET.

**Ventajas:**
- ✅ Compatible con Windows moderno
- ✅ Evita problemas de CSP
- ✅ Código más simple y mantenible
- ✅ Soporta SHA-256 nativamente

**Cambios necesarios:**
1. Usar `RSA` en lugar de `RSACryptoServiceProvider`
2. Usar `signedXml.SigningKey` en lugar de `signedXml.SigningKey` con casting
3. Eliminar referencias a `CspParameters`

**Código modificado:**
```csharp
// Obtener la clave privada directamente
RSA rsaKey = cert.GetRSAPrivateKey();
if (rsaKey == null)
{
    throw new CryptographicException("No se pudo obtener la clave privada RSA");
}

// Usar directamente la clave RSA
signedXml.SigningKey = rsaKey;  // Sin necesidad de RSACryptoServiceProvider
signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA256Url;
```

---

### SOLUCIÓN 2: Cambiar Tipo de Proveedor CSP

**Descripción:** Cambiar el tipo de proveedor CSP de 24 a uno más compatible.

**Tipos de proveedor comunes:**
- **1** = PROV_RSA_FULL (Microsoft Base Cryptographic Provider)
- **13** = PROV_RSA_AES (Microsoft Enhanced RSA and AES Cryptographic Provider)
- **24** = Microsoft Enhanced RSA and AES Cryptographic Provider v1.0

**Cambio en código:**
```csharp
// Cambiar de tipo 24 a tipo 1 o 13
RSACryptoServiceProvider key = new RSACryptoServiceProvider(new CspParameters(1));
// O
RSACryptoServiceProvider key = new RSACryptoServiceProvider(new CspParameters(13));
```

**Ventajas:**
- ✅ Cambio mínimo
- ⚠️ Puede tener limitaciones con SHA-256

---

### SOLUCIÓN 3: Cargar Certificado con Diferentes Flags

**Descripción:** Modificar los flags de carga del certificado PFX.

**Cambios en código:**
```csharp
// Probar diferentes combinaciones de flags:

// Opción A: MachineKeySet
X509Certificate2 cert = new X509Certificate2(
    p12FilePath,
    certificatePassword,
    X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable
);

// Opción B: DefaultKeySet
X509Certificate2 cert = new X509Certificate2(
    p12FilePath,
    certificatePassword,
    X509KeyStorageFlags.DefaultKeySet | X509KeyStorageFlags.Exportable
);

// Opción C: Solo Exportable
X509Certificate2 cert = new X509Certificate2(
    p12FilePath,
    certificatePassword,
    X509KeyStorageFlags.Exportable
);
```

---

### SOLUCIÓN 4: Verificar y Reexportar Certificado PFX

**Descripción:** El certificado PFX puede tener problemas de compatibilidad. Reexportarlo puede resolverlos.

**Pasos:**
1. Abrir el PFX en el administrador de certificados de Windows
2. Exportar nuevamente con opciones:
   - ✅ Exportar clave privada
   - ✅ Habilitar todas las opciones de compatibilidad
   - ✅ Usar algoritmo de cifrado TripleDES-SHA1
3. Probar con el nuevo archivo PFX

**Comando PowerShell alternativo:**
```powershell
# Reexportar certificado con configuración compatible
$cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2
$cert.Import("C:\nextsys - GLP\sifen\WEN.pfx", "password", [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::Exportable)
$bytes = $cert.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Pkcs12, "password")
[System.IO.File]::WriteAllBytes("C:\nextsys - GLP\sifen\WEN_reexported.pfx", $bytes)
```

---

### SOLUCIÓN 5: Habilitar Proveedores Legacy (Windows)

**Descripción:** Habilitar proveedores criptográficos legacy en Windows.

**Nota:** Ya tienes el script `HabilitarCryptoLegacy.bat` en el proyecto.

**Pasos:**
1. Ejecutar como Administrador: `HabilitarCryptoLegacy.bat`
2. Reiniciar el sistema
3. Probar nuevamente

**Verificación:**
```powershell
# Verificar configuración de Schannel
.\Consultar_TLS_Schannel.ps1
```

---

## 🎯 PLAN DE ACCIÓN RECOMENDADO

### Paso 1: Aplicar Solución 1 (API Moderna) ✅
Es la solución más robusta y compatible con sistemas modernos.

### Paso 2: Si falla, probar Solución 3 (Flags de carga)
Cambiar flags puede resolver problemas de acceso a clave privada.

### Paso 3: Si persiste, verificar certificado (Solución 4)
Reexportar el certificado para asegurar compatibilidad.

---

## 🚨 ERROR "XML MAL FORMADO" (Código 0160)

### Descripción del Problema
Después de resolver el problema de firma, el servidor SIFEN rechaza el documento con el código **0160: "XML Mal Formado"**.

### Causas Comunes en Blazor Server C#

#### 1. **Codificación de Caracteres Incorrecta**
El XML debe estar en **UTF-8** sin BOM (Byte Order Mark).

```csharp
// ❌ INCORRECTO - Puede agregar BOM
string xmlString = File.ReadAllText(path);

// ✅ CORRECTO - Sin BOM
string xmlString = File.ReadAllText(path, new UTF8Encoding(false));

// ✅ CORRECTO - Al guardar/enviar
byte[] xmlBytes = new UTF8Encoding(false).GetBytes(xmlString);
```

#### 2. **Espacios en Blanco o Saltos de Línea Incorrectos**
SIFEN es muy estricto con el formato del XML.

```csharp
// ❌ PROBLEMAS COMUNES:
// - Espacios al inicio del documento
// - Líneas vacías antes del <?xml?>
// - Espacios después del cierre de tags

// ✅ SOLUCIÓN - Limpiar el XML:
xmlString = xmlString.Trim();
xmlString = System.Text.RegularExpressions.Regex.Replace(xmlString, @">\s+<", "><");
```

#### 3. **Declaración XML Incorrecta**
```csharp
// ❌ INCORRECTO
<?xml version="1.0" encoding="utf-8"?>  // minúscula

// ✅ CORRECTO
<?xml version="1.0" encoding="UTF-8"?>  // mayúscula
```

#### 4. **Problema con SOAP Envelope en Blazor Server**
Cuando envías desde Blazor Server, asegúrate de construir correctamente el envelope SOAP.

```csharp
// ✅ CORRECTO - Envelope SOAP para SIFEN
string soapEnvelope = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope"">
<soap:Body>
<rEnviLoteDe xmlns=""http://ekuatia.set.gov.py/sifen/xsd"">" + 
base64XmlFirmado + 
@"</rEnviLoteDe>
</soap:Body>
</soap:Envelope>";

// IMPORTANTE: Asegurarse que no haya espacios extras
soapEnvelope = soapEnvelope.Trim();
```

#### 5. **Headers HTTP Incorrectos**
```csharp
// ✅ CORRECTO - Headers necesarios
httpClient.DefaultRequestHeaders.Clear();
httpClient.DefaultRequestHeaders.Add("SOAPAction", "");
httpClient.DefaultRequestHeaders.Add("Accept", "application/soap+xml");

var content = new StringContent(
    soapEnvelope, 
    Encoding.UTF8,  // ← Sin BOM
    "application/soap+xml; charset=UTF-8"
);
```

#### 6. **Firma Digital Mal Formada**
Si la firma se genera incorrectamente, el XML resultante puede ser inválido.

```csharp
// Verificar que la firma esté correctamente insertada:
// - Debe estar después del nodo <DE> o <rEve>
// - No debe tener espacios extras
// - Debe tener todos los elementos requeridos:
//   <Signature>
//     <SignedInfo>
//     <SignatureValue>
//     <KeyInfo>
```

#### 7. **Base64 con Saltos de Línea**
El contenido base64 NO debe tener saltos de línea cuando se envía dentro del SOAP.

```csharp
// ❌ INCORRECTO
string base64 = Convert.ToBase64String(bytes); // Puede tener saltos

// ✅ CORRECTO
string base64 = Convert.ToBase64String(bytes, Base64FormattingOptions.None);
```

### Soluciones Específicas para Blazor Server

#### SOLUCIÓN A: Validar XML antes de Enviar
```csharp
public bool ValidarXmlSifen(string xmlString)
{
    try
    {
        // 1. Limpiar espacios en blanco
        xmlString = xmlString.Trim();
        
        // 2. Validar que inicie con <?xml
        if (!xmlString.StartsWith("<?xml"))
        {
            Log("ERROR: XML no inicia con declaración XML");
            return false;
        }
        
        // 3. Cargar como XmlDocument
        XmlDocument doc = new XmlDocument();
        doc.PreserveWhitespace = false;
        doc.LoadXml(xmlString);
        
        // 4. Verificar namespaces requeridos
        XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
        nsmgr.AddNamespace("ns", "http://ekuatia.set.gov.py/sifen/xsd");
        
        // 5. Verificar nodos principales
        var deNode = doc.SelectSingleNode("//ns:DE", nsmgr);
        if (deNode == null)
        {
            Log("ERROR: No se encontró nodo DE");
            return false;
        }
        
        Log("XML validado correctamente");
        return true;
    }
    catch (Exception ex)
    {
        Log($"ERROR validando XML: {ex.Message}");
        return false;
    }
}
```

#### SOLUCIÓN B: Método de Envío Correcto para Blazor
```csharp
public async Task<string> EnviarASifenBlazor(string xmlFirmado)
{
    try
    {
        // 1. Comprimir y convertir a Base64
        byte[] xmlBytes = new UTF8Encoding(false).GetBytes(xmlFirmado);
        byte[] compressedBytes = ComprimirGZip(xmlBytes);
        string base64 = Convert.ToBase64String(compressedBytes, Base64FormattingOptions.None);
        
        // 2. Construir SOAP Envelope
        string soapBody = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope"">
<soap:Body>
<rEnviLoteDe xmlns=""http://ekuatia.set.gov.py/sifen/xsd"">{base64}</rEnviLoteDe>
</soap:Body>
</soap:Envelope>".Trim();
        
        // 3. Configurar HttpClient
        using (var handler = new HttpClientHandler())
        {
            // Cargar certificado
            handler.ClientCertificates.Add(cargarCertificado());
            handler.ServerCertificateCustomValidationCallback = 
                (message, cert, chain, errors) => true;
            
            using (var client = new HttpClient(handler))
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                
                // 4. Configurar request
                var request = new HttpRequestMessage(HttpMethod.Post, urlSifen);
                request.Content = new StringContent(
                    soapBody,
                    new UTF8Encoding(false),
                    "application/soap+xml"
                );
                request.Headers.Add("SOAPAction", "");
                
                // 5. Enviar
                var response = await client.SendAsync(request);
                string resultado = await response.Content.ReadAsStringAsync();
                
                return resultado;
            }
        }
    }
    catch (Exception ex)
    {
        Log($"Error en envío: {ex.Message}");
        throw;
    }
}

private byte[] ComprimirGZip(byte[] data)
{
    using (var memoryStream = new MemoryStream())
    {
        using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
        {
            gzipStream.Write(data, 0, data.Length);
        }
        return memoryStream.ToArray();
    }
}
```

#### SOLUCIÓN C: Debugging - Guardar XMLs para Comparar
```csharp
// Guardar el XML que estás enviando
File.WriteAllText(@"C:\temp\xml_original.xml", xmlOriginal, new UTF8Encoding(false));
File.WriteAllText(@"C:\temp\xml_firmado.xml", xmlFirmado, new UTF8Encoding(false));
File.WriteAllText(@"C:\temp\soap_envelope.xml", soapEnvelope, new UTF8Encoding(false));

// Comparar con un XML que funcione
// Revisar byte por byte si es necesario
byte[] xmlBytes1 = File.ReadAllBytes(@"C:\temp\xml_firmado.xml");
byte[] xmlBytes2 = File.ReadAllBytes(@"C:\temp\xml_funcionando.xml");
```

### Checklist para Resolver Error 0160

- [ ] **Codificación UTF-8 sin BOM**
- [ ] **Sin espacios al inicio o final del documento**
- [ ] **Declaración XML correcta**: `<?xml version="1.0" encoding="UTF-8"?>`
- [ ] **Namespace correcto**: `http://ekuatia.set.gov.py/sifen/xsd`
- [ ] **SOAP Envelope bien formado**
- [ ] **Base64 sin saltos de línea** (usar `Base64FormattingOptions.None`)
- [ ] **Firma digital insertada correctamente**
- [ ] **Headers HTTP correctos** (`application/soap+xml`)
- [ ] **Compresión GZIP correcta** (si aplica)
- [ ] **Certificado configurado en HttpClientHandler**

### Diferencias entre DLL COM y Blazor Server

| Aspecto | DLL COM | Blazor Server |
|---------|---------|---------------|
| HttpClient | WebRequest (legacy) | HttpClient (moderno) |
| Async/Await | No | Sí (recomendado) |
| Certificado | ServicePointManager | HttpClientHandler |
| Encoding | Puede variar | Debe ser UTF-8 sin BOM |
| Timeout | Por defecto | Configurar explícitamente |

---

### Paso 4: Último recurso - Solución 5 (Legacy)
Habilitar proveedores legacy solo si las otras soluciones fallan.

---

## 📝 INFORMACIÓN ADICIONAL

### Certificado Actual
- **Subject:** SERIALNUMBER=CI495219, CN=WENCESLAO ROJAS ALFONSO
- **Serial:** 00E9312C9B1DB909AC49B307D8A7A6AE8B
- **Thumbprint:** 477AAEC61F0A09E5EC6DCE86FE7A75DA0F91F9C2
- **Ruta PFX:** C:\nextsys - GLP\sifen\WEN.pfx
- **Tiene clave privada:** SÍ ✅

### URLs SIFEN
- **Recepción:** https://sifen-test.set.gov.py/de/ws/async/recibe-lote.wsdl
- **Consulta:** https://sifen-test.set.gov.py/de/ws/consultas/consulta.wsdl
- **QR:** https://ekuatia.set.gov.py/consultas-test/qr?

### Archivos de Log
- **Log Principal:** C:\nextsys - GLP\sifen_log.txt
- **XML Input:** C:\nextsys - GLP\sifen_xml_input.txt

---

## 🔧 CÓDIGO DE EJEMPLO - SOLUCIÓN 1 (COMPLETA)

```csharp
// REEMPLAZO COMPLETO DEL BLOQUE DE FIRMA (líneas ~365-395)

// Obtener la clave privada usando API moderna
RSA rsaKey = cert.GetRSAPrivateKey();
if (rsaKey == null)
{
    throw new CryptographicException("No se pudo obtener la clave privada RSA del certificado");
}

Log("RSA Key obtenida via GetRSAPrivateKey()");
Log("RSA KeySize: " + rsaKey.KeySize);

// Configurar SignedXml con la clave RSA directamente
signedXml.SigningKey = rsaKey;  // ← API moderna, no necesita RSACryptoServiceProvider
signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA256Url;

Reference reference = new Reference();
reference.Uri = "#" + nodeId;
reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
reference.DigestMethod = SignedXml.XmlDsigSHA256Url;  // SHA-256 para digest
signedXml.AddReference(reference);

KeyInfo keyInfo = new KeyInfo();
keyInfo.AddClause(new KeyInfoX509Data(cert));
signedXml.KeyInfo = keyInfo;

// Firmar el documento
Log("Firmando documento XML...");
signedXml.ComputeSignature();
Log("Firma digital completada exitosamente");
```

---

## ⚠️ NOTAS IMPORTANTES

1. **No mezclar APIs:** Evitar usar `GetRSAPrivateKey()` y luego crear `RSACryptoServiceProvider` manualmente
2. **SHA-256 requiere:** Windows Vista+ y .NET Framework 4.5+
3. **Permisos:** El proceso debe tener acceso al almacén de certificados
4. **CSP Legacy:** Solo habilitar si es absolutamente necesario
5. **Testing:** Probar en ambiente de pruebas SIFEN antes de producción

---

**Próximo paso sugerido:** Implementar **Solución 1** con API moderna completa.
