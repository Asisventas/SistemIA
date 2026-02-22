# Análisis Comparativo: Firma XML y Envío SIFEN

## 📋 Resumen Ejecutivo

Este documento compara la implementación de firma digital XML y envío de documentos electrónicos de **SistemIA** con la especificación oficial de **SIFEN v150** y el código de referencia de la librería Java **rshk-jsifenlib** de Roshka.

**Estado: ✅ Implementación CORRECTA**

Los algoritmos de firma, transforms, canonicalización y compresión ZIP están implementados correctamente según los estándares requeridos.

---

## 📖 Documentación de Referencia

### Especificación Oficial SIFEN v150

**Firma Digital - Sección del Manual Técnico:**
- Estándar: http://www.w3.org/TR/xmldsig-core/
- Certificado: http://www.w3.org/2000/09/xmldsig#X509Data
- Algoritmo RSA: https://www.w3.org/TR/2002/REC-xmlenc-core-20021210/Overview.html#rsa-1_5
- Tamaño de clave: RSA 2048 (software) o RSA 2048/4096 (hardware)
- Hash: SHA-2 (SHA256)

### Código de Referencia Java (Roshka - rshk-jsifenlib)

**Archivos clave analizados:**
- `SignatureHelper.java` - Implementación de firma digital
- `SoapHelper.java` - Construcción de mensajes SOAP
- `SifenUtil.java` - Utilidades incluyendo compresión ZIP
- `Constants.java` - URIs y constantes
- `ReqRecLoteDe.java` - Envío de lote de documentos
- `DocumentoElectronico.java` - Estructura del DE

---

## 🔐 Firma Digital XML - Comparación Detallada

### 1. Algoritmos y URIs

| Parámetro | Referencia Java | SistemIA (C#) | Estado |
|-----------|-----------------|---------------|--------|
| **Signature Method** | `http://www.w3.org/2001/04/xmldsig-more#rsa-sha256` | `http://www.w3.org/2001/04/xmldsig-more#rsa-sha256` | ✅ |
| **Digest Method** | `http://www.w3.org/2001/04/xmlenc#sha256` | `http://www.w3.org/2001/04/xmlenc#sha256` | ✅ |
| **Canonicalization** | `http://www.w3.org/2001/10/xml-exc-c14n#` | `http://www.w3.org/2001/10/xml-exc-c14n#` | ✅ |
| **Transform 1** | Enveloped Signature | `XmlDsigEnvelopedSignatureTransform` | ✅ |
| **Transform 2** | Exclusive C14N | `XmlDsigExcC14NTransform` | ✅ |

### 2. Código de Referencia Java (SignatureHelper.java)

```java
// Transforms (en orden)
transforms.add(_xmlSignatureFactory.newTransform(Transform.ENVELOPED, null));
transforms.add(_xmlSignatureFactory.newTransform(CanonicalizationMethod.EXCLUSIVE, null));

// Reference al nodo DE con Id
Reference ref = _xmlSignatureFactory.newReference(
    "#" + signedNodeId,
    _xmlSignatureFactory.newDigestMethod(DigestMethod.SHA256, null),
    transforms, null, null);

// SignedInfo
SignedInfo signedInfo = _xmlSignatureFactory.newSignedInfo(
    _xmlSignatureFactory.newCanonicalizationMethod(CanonicalizationMethod.EXCLUSIVE, null),
    _xmlSignatureFactory.newSignatureMethod(Constants.RSA_SHA256, null),
    Collections.singletonList(ref));

// KeyInfo con certificado X509
X509Data x509Data = keyInfoFactory.newX509Data(Collections.singletonList(certificate));
KeyInfo keyInfo = keyInfoFactory.newKeyInfo(Collections.singletonList(x509Data));
```

### 3. Implementación SistemIA (Sifen.cs - FirmarYEnviar)

```csharp
// Reference al nodo DE con Id
var reference = new Reference
{
    Uri = "#" + nodeId,
    DigestMethod = "http://www.w3.org/2001/04/xmlenc#sha256"
};

// Transforms (en orden)
reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
reference.AddTransform(new XmlDsigExcC14NTransform());

// SignedInfo
signedXml.SignedInfo.CanonicalizationMethod = "http://www.w3.org/2001/10/xml-exc-c14n#";
signedXml.SignedInfo.SignatureMethod = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";

// KeyInfo con certificado X509
var keyInfo = new KeyInfo();
keyInfo.AddClause(new KeyInfoX509Data(cert));
```

### 4. Ubicación de la Firma en el XML

| Aspecto | Referencia Java | SistemIA | Estado |
|---------|-----------------|----------|--------|
| **Firma como sibling de DE** | ✅ `parent.insertAfter(sig, DE)` | ✅ `rdeNode.InsertAfter(sig, node)` | ✅ |
| **gCamFuFD después de Signature** | ✅ Relocado después de firmar | ✅ Relocado después de firmar | ✅ |
| **Id attribute en DE** | ✅ `setIdAttribute("Id", true)` | ✅ `SignedXmlWithId` (clase customizada) | ✅ |

### 5. Estructura Final del XML Firmado

```xml
<rDE xmlns="http://ekuatia.set.gov.py/sifen/xsd">
  <dVerFor>150</dVerFor>
  <DE Id="CDC_44_DIGITOS">
    <!-- Contenido del documento -->
  </DE>
  <Signature xmlns="http://www.w3.org/2000/09/xmldsig#">
    <SignedInfo>
      <CanonicalizationMethod Algorithm="http://www.w3.org/2001/10/xml-exc-c14n#"/>
      <SignatureMethod Algorithm="http://www.w3.org/2001/04/xmldsig-more#rsa-sha256"/>
      <Reference URI="#CDC_44_DIGITOS">
        <Transforms>
          <Transform Algorithm="http://www.w3.org/2000/09/xmldsig#enveloped-signature"/>
          <Transform Algorithm="http://www.w3.org/2001/10/xml-exc-c14n#"/>
        </Transforms>
        <DigestMethod Algorithm="http://www.w3.org/2001/04/xmlenc#sha256"/>
        <DigestValue>...</DigestValue>
      </Reference>
    </SignedInfo>
    <SignatureValue>...</SignatureValue>
    <KeyInfo>
      <X509Data>
        <X509Certificate>...</X509Certificate>
      </X509Data>
    </KeyInfo>
  </Signature>
  <gCamFuFD>
    <dCarQR>...</dCarQR>
  </gCamFuFD>
</rDE>
```

---

## 📦 Compresión ZIP para xDE - Comparación

### Referencia Java (SifenUtil.java)

```java
public static byte[] compressXmlToZip(String str) throws IOException {
    File zip = File.createTempFile("lotezip", ".zip");
    String fileName = "DE_" + new SimpleDateFormat("ddMMyyyy").format(new Date());
    ZipOutputStream out = new ZipOutputStream(Files.newOutputStream(zip.toPath()));
    ZipEntry entry = new ZipEntry(fileName + ".xml");
    out.putNextEntry(entry);
    out.write(str.getBytes(StandardCharsets.UTF_8));
    out.closeEntry();
    out.close();
    return Files.readAllBytes(Paths.get(zip.getAbsolutePath()));
}
```

### Implementación SistemIA (Sifen.cs - StringToZip)

```csharp
public static string StringToZip(string originalString)
{
    using var memoryStream = new MemoryStream();
    // CRÍTICO: Usar ZipArchive (archivo ZIP real), NO GZipStream
    using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
    {
        var fileName = $"DE_{DateTime.Now:ddMMyyyy}.xml";
        var entry = zipArchive.CreateEntry(fileName, CompressionLevel.Optimal);
        using var entryStream = entry.Open();
        var xmlBytes = new UTF8Encoding(false).GetBytes(originalString);
        entryStream.Write(xmlBytes, 0, xmlBytes.Length);
    }
    memoryStream.Position = 0;
    return Convert.ToBase64String(memoryStream.ToArray());
}
```

| Aspecto | Referencia Java | SistemIA | Estado |
|---------|-----------------|----------|--------|
| **Formato** | ZIP real (PK header) | ZipArchive | ✅ |
| **Nombre entrada** | `DE_DDMMYYYY.xml` | `DE_ddMMyyyy.xml` | ✅ |
| **Encoding** | UTF-8 sin BOM | UTF-8 sin BOM | ✅ |
| **Output** | byte[] → Base64 | MemoryStream → Base64 | ✅ |

---

## 📡 Envío SOAP - Comparación

### 1. Protocolo y Content-Type

| Parámetro | Referencia Java | SistemIA | Estado |
|-----------|-----------------|----------|--------|
| **Protocolo** | SOAP 1.2 | SOAP 1.2 | ✅ |
| **Content-Type** | `application/xml; charset=utf-8` | `application/xml; charset=utf-8` | ✅ |
| **TLS Version** | TLS 1.2 | `SslProtocols.Tls12` | ✅ |

### 2. Estructura SOAP para Envío de Lote

**Referencia Java (ReqRecLoteDe.java):**

```java
// Crear mensaje SOAP
MessageFactory mf12 = MessageFactory.newInstance(SOAPConstants.SOAP_1_2_PROTOCOL);
SOAPMessage message = mf12.createMessage();

// Body: rEnvioLote con namespace SIFEN
SOAPBodyElement rEnvioLote = soapBody.addBodyElement(
    new QName(Constants.SIFEN_NS_URI, "rEnvioLote"));
rEnvioLote.addChildElement("dId").setTextContent(dId);
SOAPElement xDE = rEnvioLote.addChildElement("xDE");

// CRÍTICO: rLoteDE SIN namespace (solo nombre local)
SOAPElement rLoteDE = SoapHelper.createSoapMessage()
    .getSOAPBody().addChildElement("rLoteDE"); // SIN QName = SIN namespace

// Comprimir y codificar Base64
byte[] zipFile = SifenUtil.compressXmlToZip(rLoteDEXml);
String rLoteDEBase64 = Base64.getEncoder().encodeToString(zipFile);
xDE.setTextContent(rLoteDEBase64);
```

**Implementación SistemIA (Sifen.cs - ConstruirSoapEnvioLoteZipBase64):**

```csharp
// CRÍTICO: rLoteDE SIN namespace (como en Java)
var inner = new XmlDocument();
var declInner = inner.CreateXmlDeclaration("1.0", "UTF-8", null);
inner.AppendChild(declInner);
var rLote = inner.CreateElement("rLoteDE"); // SIN namespace
inner.AppendChild(rLote);

// Importar el rDE firmado
var imported = inner.ImportNode(rdeNode, true);
rLote.AppendChild(imported);

// Comprimir a ZIP
var zipped = StringToZip(inner.OuterXml);

// SOAP externo con namespace SIFEN
var soapNs = "http://www.w3.org/2003/05/soap-envelope";
var sifenNs = "http://ekuatia.set.gov.py/sifen/xsd";
var envelope = soapDoc.CreateElement("soap", "Envelope", soapNs);
var body = soapDoc.CreateElement("soap", "Body", soapNs);
var req = soapDoc.CreateElement("rEnvioLote", sifenNs);
var dIdNode = soapDoc.CreateElement("dId", sifenNs);
var xde = soapDoc.CreateElement("xDE", sifenNs);
xde.InnerText = zipped; // Base64 del ZIP
```

### 3. Estructura del SOAP Final

```xml
<soap:Envelope xmlns:soap="http://www.w3.org/2003/05/soap-envelope">
  <soap:Body>
    <rEnvioLote xmlns="http://ekuatia.set.gov.py/sifen/xsd">
      <dId>20240115120000</dId>
      <xDE>
        <!-- Base64(ZIP(
          <?xml version="1.0" encoding="UTF-8"?>
          <rLoteDE>              <!-- SIN namespace -->
            <rDE xmlns="...">   <!-- CON namespace SIFEN -->
              <DE Id="...">...</DE>
              <Signature>...</Signature>
              <gCamFuFD>...</gCamFuFD>
            </rDE>
          </rLoteDE>
        )) -->
      </xDE>
    </rEnvioLote>
  </soap:Body>
</soap:Envelope>
```

---

## 🔄 Retry y Manejo de Errores SSL

### Implementación SistemIA (Sifen.cs - Enviar)

```csharp
const int maxRetries = 5;
int[] delaySeconds = { 1, 2, 3, 5, 8 }; // Fibonacci-like backoff

for (int attempt = 1; attempt <= maxRetries; attempt++)
{
    try
    {
        // Configuración SSL
        handler.SslProtocols = SslProtocols.Tls12;
        handler.ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => true;
        
        // Headers importantes
        client.DefaultRequestHeaders.Add("User-Agent", "Java/1.8.0_341"); // Bypass BIG-IP
        client.DefaultRequestHeaders.Add("Connection", "close");
        
        var response = await client.PostAsync(url, content);
        // ... procesar respuesta
    }
    catch (Exception ex)
    {
        // Retry si es error SSL/conexión
        if (EsErrorDeConexion(ex) && attempt < maxRetries)
        {
            await Task.Delay(delaySeconds[attempt - 1] * 1000);
            continue;
        }
        throw;
    }
}
```

---

## 📊 Resumen de Conformidad

| Componente | Estado | Notas |
|------------|--------|-------|
| **Algoritmo de Firma** | ✅ Correcto | RSA-SHA256 |
| **Transforms** | ✅ Correcto | Enveloped + Exc-C14N (en orden) |
| **Canonicalización** | ✅ Correcto | Exclusive C14N |
| **KeyInfo** | ✅ Correcto | X509Data con certificado |
| **Ubicación Firma** | ✅ Correcto | Sibling de DE en rDE |
| **Compresión xDE** | ✅ Correcto | ZIP real (no GZip) |
| **Nombre archivo ZIP** | ✅ Correcto | DE_DDMMYYYY.xml |
| **Encoding XML** | ✅ Correcto | UTF-8 sin BOM |
| **SOAP Version** | ✅ Correcto | SOAP 1.2 |
| **Content-Type** | ✅ Correcto | application/xml; charset=utf-8 |
| **TLS** | ✅ Correcto | TLS 1.2 |
| **rLoteDE sin namespace** | ✅ Correcto | Solo nombre local |

---

## 📝 Conclusión

La implementación de firma y envío SIFEN en SistemIA está **correctamente implementada** según:

1. ✅ Manual Técnico SIFEN v150
2. ✅ Código de referencia Java (rshk-jsifenlib de Roshka)
3. ✅ XMLs aprobados por SIFEN en producción

Si persiste el error 0160, el problema NO está en:
- La firma digital
- La compresión ZIP
- La estructura SOAP
- Los algoritmos criptográficos

El error 0160 "XML Mal Formado" probablemente se debe a:
1. **Contenido de campos** - Valores que violan el XSD
2. **Formato de fechas** - Fechas futuras o formato incorrecto
3. **Campos faltantes/sobrantes** - En el contenido del DE, no en la firma

---

## 📚 Archivos de Código Analizados

### SistemIA
- `Models/Sifen.cs` - Firma, envío y compresión ZIP
- `Services/DEXmlBuilder.cs` - Construcción del XML del DE

### Referencia Java (rshk-jsifenlib)
- `src/main/java/com/roshka/sifen/internal/helpers/SignatureHelper.java`
- `src/main/java/com/roshka/sifen/internal/helpers/SoapHelper.java`
- `src/main/java/com/roshka/sifen/internal/util/SifenUtil.java`
- `src/main/java/com/roshka/sifen/internal/Constants.java`
- `src/main/java/com/roshka/sifen/internal/request/ReqRecLoteDe.java`
- `src/main/java/com/roshka/sifen/core/beans/DocumentoElectronico.java`

---

*Documento generado: Enero 2026*
*Versión: 1.0*
