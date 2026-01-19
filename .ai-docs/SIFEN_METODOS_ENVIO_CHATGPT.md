# 📤 SIFEN Paraguay - Documentación de Métodos de Envío

> **Documento para ChatGPT** - Referencia completa de los métodos de firma, envío y recepción de documentos electrónicos SIFEN v150.

---

## 📋 Índice

1. [Descripción General](#descripción-general)
2. [Arquitectura de Métodos](#arquitectura-de-métodos)
3. [Método Enviar() - Comunicación HTTP](#método-enviar---comunicación-http)
4. [Método FirmarYEnviar() - Firma y Envío Lote](#método-firmaryenviar---firma-y-envío-lote)
5. [Método FirmarSinEnviar() - Solo Firma](#método-firmarsinenviar---solo-firma)
6. [Método Consultar() - Consultas CDC/Lote/RUC](#método-consultar---consultas-cdcloteruc)
7. [Funciones Auxiliares de Compresión](#funciones-auxiliares-de-compresión)
8. [Estructura XML del Documento Electrónico](#estructura-xml-del-documento-electrónico)
9. [Formatos SOAP - Sync vs Lote](#formatos-soap---sync-vs-lote)
10. [Configuración SSL/TLS](#configuración-ssltls)
11. [Códigos de Respuesta SIFEN](#códigos-de-respuesta-sifen)
12. [Problemas Conocidos y Soluciones](#problemas-conocidos-y-soluciones)

---

## Descripción General

**SIFEN** (Sistema Integrado de Facturación Electrónica Nacional) es el sistema de facturación electrónica de Paraguay administrado por la SET (Subsecretaría de Estado de Tributación).

### Endpoints SIFEN

| Endpoint | URL Test | URL Producción | Uso |
|----------|----------|----------------|-----|
| **Recepción Sync** | `https://sifen-test.set.gov.py/de/ws/sync/recibe.wsdl` | `https://sifen.set.gov.py/de/ws/sync/recibe.wsdl` | Envío individual inmediato |
| **Recepción Lote** | `https://sifen-test.set.gov.py/de/ws/async/recibe-lote.wsdl` | `https://sifen.set.gov.py/de/ws/async/recibe-lote.wsdl` | Envío masivo asíncrono |
| **Consulta Lote** | `https://sifen-test.set.gov.py/de/ws/consultas/consulta-lote.wsdl` | `https://sifen.set.gov.py/de/ws/consultas/consulta-lote.wsdl` | Estado de lote enviado |
| **Consulta DE** | `https://sifen-test.set.gov.py/de/ws/consultas/consulta.wsdl` | `https://sifen.set.gov.py/de/ws/consultas/consulta.wsdl` | Estado por CDC |
| **Consulta RUC** | `https://sifen-test.set.gov.py/de/ws/consultas/consulta-ruc.wsdl` | `https://sifen.set.gov.py/de/ws/consultas/consulta-ruc.wsdl` | Datos contribuyente |
| **Eventos** | `https://sifen-test.set.gov.py/de/ws/eventos/evento.wsdl` | `https://sifen.set.gov.py/de/ws/eventos/evento.wsdl` | Anulación, cancelación |

---

## Arquitectura de Métodos

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           FLUJO DE ENVÍO SIFEN                              │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│   1. DEXmlBuilder.ConstruirXmlAsync()                                       │
│      └── Genera XML del DE (sin firma)                                      │
│                       │                                                     │
│                       ▼                                                     │
│   2. FirmarYEnviar() ──────────────────────────────────────────────────┐    │
│      │  - Carga certificado P12                                        │    │
│      │  - Firma con RSA-SHA256                                         │    │
│      │  - Inserta Signature dentro de <DE>                             │    │
│      │  - Calcula cHashQR                                              │    │
│      │  - Envuelve en <rLoteDE>                                        │    │
│      │  - Comprime a ZIP                                               │    │
│      │  - Construye SOAP                                               │    │
│      └──────────────────────────────────────────────────────────────────┘    │
│                       │                                                     │
│                       ▼                                                     │
│   3. Enviar()                                                               │
│      │  - Retry con backoff (5 intentos)                                    │
│      │  - TLS 1.2                                                           │
│      │  - Headers para BIG-IP                                               │
│      └── POST a endpoint SIFEN                                              │
│                       │                                                     │
│                       ▼                                                     │
│   4. Respuesta SIFEN                                                        │
│      ├── 0260: Aprobado                                                     │
│      ├── 0300: Lote recibido                                                │
│      ├── 0160: XML Mal Formado                                              │
│      └── Otros códigos de error                                             │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Método Enviar() - Comunicación HTTP

### Firma del Método
```csharp
public async Task<string> Enviar(string url, string documento, string p12FilePath, string certificatePassword)
```

### Parámetros
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `url` | string | URL del endpoint SIFEN (debe terminar en .wsdl) |
| `documento` | string | SOAP completo a enviar |
| `p12FilePath` | string | Ruta al certificado P12/PFX |
| `certificatePassword` | string | Contraseña del certificado |

### Características

#### 1. Retry con Exponential Backoff
```csharp
const int maxRetries = 5;
int[] delaySeconds = { 1, 2, 3, 5, 8 }; // Fibonacci-like backoff

// Errores que disparan retry:
// - SSL, conexión, connection
// - timeout, refused, reset
```

#### 2. Configuración TLS/SSL
```csharp
var handler = new HttpClientHandler
{
    SslProtocols = SslProtocols.Tls12, // OBLIGATORIO para SIFEN
    ClientCertificates = { certificate },
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
};
```

#### 3. Headers para Bypass BIG-IP
```csharp
// Los balanceadores BIG-IP del SET requieren estos headers:
client.DefaultRequestHeaders.Add("User-Agent", "Java/1.8.0_341");
client.DefaultRequestHeaders.Add("Origin", originHeader);
client.DefaultRequestHeaders.Add("Referer", url);
client.DefaultRequestHeaders.Add("Accept-Encoding", "identity");
client.DefaultRequestHeaders.Add("Connection", "close");
client.DefaultRequestHeaders.Add("Accept", "application/soap+xml, application/xml, text/xml");
```

#### 4. Content-Type
```csharp
// CRÍTICO: Igual que PowerBuilder funcional
string contentType = "text/xml; charset=utf-8";
```

#### 5. Certificado con Flags
```csharp
X509KeyStorageFlags[] flagsToTry = new X509KeyStorageFlags[]
{
    X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable,
    X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.Exportable,
    X509KeyStorageFlags.DefaultKeySet | X509KeyStorageFlags.Exportable,
    X509KeyStorageFlags.Exportable
};
```

### Código Completo
```csharp
private async Task<string> EnviarInterno(string url, string documento, string p12FilePath, string certificatePassword)
{
    X509Certificate2? certificate = null;
    try
    {
        // Cargar certificado
        certificate = new X509Certificate2(
            p12FilePath, 
            certificatePassword,
            X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);

        var handler = new HttpClientHandler
        {
            ClientCertificateOptions = ClientCertificateOption.Manual,
            SslProtocols = SslProtocols.Tls12,
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        handler.ClientCertificates.Add(certificate);

        using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(120) };
        
        // Headers
        client.DefaultRequestHeaders.Add("User-Agent", "Java/1.8.0_341");
        client.DefaultRequestHeaders.Add("Connection", "close");
        client.DefaultRequestHeaders.Add("Accept", "application/soap+xml, application/xml, text/xml");
        
        using var content = new StringContent(documento, Encoding.UTF8, "text/xml");
        content.Headers.Clear();
        content.Headers.Add("Content-Type", "text/xml; charset=utf-8");
        
        using var response = await client.PostAsync(url, content);
        return await response.Content.ReadAsStringAsync();
    }
    finally
    {
        certificate?.Dispose();
    }
}
```

---

## Método FirmarYEnviar() - Firma y Envío Lote

### Firma del Método
```csharp
public async Task<string> FirmarYEnviar(
    string url,           // URL del endpoint SIFEN
    string urlQR,         // URL base para el QR (consultas/qr o consultas-test/qr)
    string xmlString,     // XML del DE sin firmar (generado por DEXmlBuilder)
    string p12FilePath,   // Ruta al certificado P12
    string certificatePassword,
    string tipoFirmado = "1")  // "1" = DE, "2" = Evento
```

### Flujo Interno

#### Paso 1: Cargar XML con PreserveWhitespace
```csharp
var doc = new XmlDocument();
// ⚠️ CRÍTICO: Sin esto, el DigestValue no coincide
doc.PreserveWhitespace = true;
doc.LoadXml(xmlString);
```

#### Paso 2: Obtener nodo a firmar
```csharp
var node = doc.GetElementsByTagName(tipoFirmado == "1" ? "DE" : "rEve")[0];
var nodeId = node.Attributes?["Id"].Value;  // CDC de 44 dígitos
```

#### Paso 3: Cargar certificado y clave RSA
```csharp
// API moderna (Windows 10/11 compatible)
var cert = new X509Certificate2(p12FilePath, password, 
    X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);

RSA rsaKey = cert.GetRSAPrivateKey();  // NO usar RSACryptoServiceProvider
```

#### Paso 4: Configurar firma digital
```csharp
var signedXml = new SignedXmlWithId(doc) { SigningKey = rsaKey };

var reference = new Reference
{
    Uri = "#" + nodeId,  // Referencia al Id del DE (CDC)
    DigestMethod = "http://www.w3.org/2001/04/xmlenc#sha256"
};
reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
reference.AddTransform(new XmlDsigExcC14NTransform());

signedXml.AddReference(reference);
signedXml.SignedInfo.CanonicalizationMethod = "http://www.w3.org/2001/10/xml-exc-c14n#";
signedXml.SignedInfo.SignatureMethod = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";

var keyInfo = new KeyInfo();
keyInfo.AddClause(new KeyInfoX509Data(cert));
signedXml.KeyInfo = keyInfo;

signedXml.ComputeSignature();
var signature = signedXml.GetXml();
```

#### Paso 5: Insertar Signature en posición correcta
```csharp
// ⚠️ ESTRUCTURA OBLIGATORIA según Manual SIFEN v150:
// <rDE>
//   <DE>
//     ...contenido...
//     <gTotSub>...</gTotSub>
//     <gCamGen/>              ← OBLIGATORIO
//     <Signature>...</Signature>  ← DENTRO de DE, ÚLTIMO hijo
//   </DE>
//   <gCamFuFD>...</gCamFuFD>    ← FUERA de DE
// </rDE>

string nsSifen = "http://ekuatia.set.gov.py/sifen/xsd";

// 1) Asegurar gCamGen existe
var gCamGen = doc.GetElementsByTagName("gCamGen")[0];
if (gCamGen == null)
{
    gCamGen = doc.CreateElement("gCamGen", nsSifen);
    node.AppendChild(gCamGen);
}

// 2) Asegurar gCamFuFD está FUERA de DE
var gCamFuFD = doc.GetElementsByTagName("gCamFuFD")[0];
if (gCamFuFD?.ParentNode != doc.DocumentElement)
{
    gCamFuFD.ParentNode?.RemoveChild(gCamFuFD);
    doc.DocumentElement.AppendChild(gCamFuFD);
}

// 3) Insertar Signature DENTRO de DE, después de gCamGen
XmlNode importedSignature = doc.ImportNode(signature, true);
node.InsertAfter(importedSignature, gCamGen);
```

#### Paso 6: Calcular cHashQR
```csharp
// Obtener DigestValue de la firma y convertir a HEX
var digestValueBase64 = doc.GetElementsByTagName("DigestValue")[0].InnerText;
var digestValueHex = StringToHex(digestValueBase64);  // HEX de caracteres ASCII

// Extraer parámetros del QR (sin URL base)
string soloParametros = urlQR.Substring(urlQR.IndexOf('?') + 1);

// El hash se calcula sobre: parámetros + CSC (sin URL base)
string datosParaHash = soloParametros + cscValue;
string cHashQR = SHA256ToString(datosParaHash);

// URL final del QR
qrNode.InnerText = urlCompleta + "&cHashQR=" + cHashQR;
```

#### Paso 7: Construir SOAP para Lote
```csharp
// Envolver en <rLoteDE> y comprimir
var xmlConWrapper = $"<rLoteDE>{xmlFirmado}</rLoteDE>";
var zippedXml = StringToZip(xmlConWrapper);

// SOAP para endpoint LOTE
var finalXml = $@"<soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope"">
    <soap:Body>
        <rEnvioLote xmlns=""http://ekuatia.set.gov.py/sifen/xsd"">
            <dId>{dId}</dId>
            <xDE>{zippedXml}</xDE>
        </rEnvioLote>
    </soap:Body>
</soap:Envelope>";

return await Enviar(url, finalXml, p12FilePath, certificatePassword);
```

---

## Método FirmarSinEnviar() - Solo Firma

### Firma del Método
```csharp
public string FirmarSinEnviar(
    string urlQR,
    string xmlString,
    string p12FilePath,
    string certificatePassword,
    string tipoFirmado = "1",
    bool devolverBase64Zip = true)
```

### Descripción
Firma el XML sin enviarlo a SIFEN. Útil para:
- Validar en prevalidador antes de enviar
- Debugging
- Generar XML para envío manual

### Retorno según `devolverBase64Zip`

**Si `devolverBase64Zip = true`:**
Retorna SOAP completo para endpoint **SYNC** (recibe.wsdl):
```xml
<?xml version="1.0" encoding="UTF-8"?>
<env:Envelope xmlns:env="http://www.w3.org/2003/05/soap-envelope">
    <env:Header/>
    <env:Body>
        <rEnviDe xmlns="http://ekuatia.set.gov.py/sifen/xsd">
            <dId>20260115143022</dId>
            <xDE>
                <rDE xmlns="...">
                    <!-- XML directo, SIN comprimir -->
                </rDE>
            </xDE>
        </rEnviDe>
    </env:Body>
</env:Envelope>
```

**Si `devolverBase64Zip = false`:**
Retorna solo el XML firmado del rDE:
```xml
<?xml version="1.0" encoding="UTF-8"?>
<rDE xmlns="http://ekuatia.set.gov.py/sifen/xsd">
    <dVerFor>150</dVerFor>
    <DE Id="CDC44...">
        ...contenido...
        <gCamGen/>
        <Signature>...</Signature>
    </DE>
    <gCamFuFD>
        <dCarQR>https://...</dCarQR>
    </gCamFuFD>
</rDE>
```

---

## Método Consultar() - Consultas CDC/Lote/RUC

### Firma del Método
```csharp
public async Task<string> Consulta(
    string url,
    string id,              // RUC, CDC o IdLote según tipo
    string tipoConsulta,    // "1"=RUC, "2"=CDC, "3"=Lote
    string p12FilePath,
    string certificatePassword)
```

### Tipos de Consulta

#### Tipo "1" - Consulta RUC
```xml
<soap:Envelope xmlns:soap="http://www.w3.org/2003/05/soap-envelope">
    <soap:Body>
        <rEnviConsRUC xmlns="http://ekuatia.set.gov.py/sifen/xsd">
            <dId>1</dId>
            <dRUCCons>{id}</dRUCCons>
        </rEnviConsRUC>
    </soap:Body>
</soap:Envelope>
```

#### Tipo "2" - Consulta CDC (Documento)
```xml
<soap:Envelope xmlns:soap="http://www.w3.org/2003/05/soap-envelope">
    <soap:Body>
        <rEnviConsDeRequest xmlns="http://ekuatia.set.gov.py/sifen/xsd">
            <dId>1</dId>
            <dCDC>{id}</dCDC>
        </rEnviConsDeRequest>
    </soap:Body>
</soap:Envelope>
```

#### Tipo "3" - Consulta Lote
```xml
<soap:Envelope xmlns:soap="http://www.w3.org/2003/05/soap-envelope">
    <soap:Body>
        <rEnviConsLoteDe xmlns="http://ekuatia.set.gov.py/sifen/xsd">
            <dId>1</dId>
            <dProtConsLote>{id}</dProtConsLote>
        </rEnviConsLoteDe>
    </soap:Body>
</soap:Envelope>
```

---

## Funciones Auxiliares de Compresión

### StringToZip() - Comprimir XML a ZIP Base64
```csharp
public static string StringToZip(string originalString)
{
    using var memoryStream = new MemoryStream();
    
    // ⚠️ CRÍTICO: Usar ZipArchive, NO GZipStream
    // SIFEN requiere application/zip (ZIP real), no .gz
    using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
    {
        var fileName = $"DE_{DateTime.Now:ddMMyyyy}.xml";
        var entry = zipArchive.CreateEntry(fileName, CompressionLevel.Optimal);
        
        using (var entryStream = entry.Open())
        {
            var xmlBytes = new UTF8Encoding(false).GetBytes(originalString);
            entryStream.Write(xmlBytes, 0, xmlBytes.Length);
            entryStream.Flush();  // ⚠️ CRÍTICO: Flush antes de cerrar
        }
    }
    
    return Convert.ToBase64String(memoryStream.ToArray());
}
```

### SHA256ToString() - Hash para cHashQR
```csharp
public static string SHA256ToString(string s)
{
    using var alg = SHA256.Create();
    byte[] hash = alg.ComputeHash(Encoding.UTF8.GetBytes(s));
    return Convert.ToHexString(hash).ToLower();
}
```

### StringToHex() - DigestValue a HEX
```csharp
// ⚠️ CRÍTICO: Convertir caracteres ASCII del Base64 a HEX
// NO decodificar el Base64 primero
public string StringToHex(string textString)
{
    // "pmMQ..." → "706d4d51..." (hex de caracteres ASCII)
    return string.Concat(textString.Select(c => Convert.ToInt32(c).ToString("x2")));
}
```

---

## Estructura XML del Documento Electrónico

### Estructura Completa del rDE (SIFEN v150)
```xml
<?xml version="1.0" encoding="UTF-8"?>
<rDE xmlns="http://ekuatia.set.gov.py/sifen/xsd"
     xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
     xsi:schemaLocation="http://ekuatia.set.gov.py/sifen/xsd siRecepDE_v150.xsd">
    
    <dVerFor>150</dVerFor>
    
    <DE Id="01800123456001001000000122026011512345678901">
        
        <!-- Operación del DE -->
        <gOpeDE>
            <iTipEmi>1</iTipEmi>           <!-- 1=Normal, 2=Contingencia -->
            <dDesTipEmi>Normal</dDesTipEmi>
            <dCodSeg>123456789</dCodSeg>   <!-- 9 dígitos aleatorios -->
            <dInfoEmi>1</dInfoEmi>
            <dInfoFisc>...</dInfoFisc>
        </gOpeDE>
        
        <!-- Timbrado -->
        <gTimb>
            <iTiDE>1</iTiDE>               <!-- Tipo DE: 1=Factura -->
            <dDesTiDE>Factura electrónica</dDesTiDE>
            <dNumTim>12345678</dNumTim>    <!-- Número timbrado SET -->
            <dEst>001</dEst>               <!-- Establecimiento -->
            <dPunExp>001</dPunExp>         <!-- Punto expedición -->
            <dNumDoc>0000001</dNumDoc>     <!-- Número documento -->
            <dSerieNum>AA</dSerieNum>      <!-- Serie (opcional) -->
            <dFeIniT>2025-01-01</dFeIniT>  <!-- Inicio vigencia timbrado -->
        </gTimb>
        
        <!-- Datos Generales -->
        <gDatGralOpe>
            <dFeEmiDE>2026-01-15T14:30:22</dFeEmiDE>
            
            <!-- Operación Comercial -->
            <gOpeCom>
                <iTipTra>1</iTipTra>        <!-- Tipo transacción -->
                <dDesTipTra>Venta de mercadería</dDesTipTra>
                <iTImp>1</iTImp>            <!-- Tipo impuesto: 1=IVA -->
                <dDesTImp>IVA</dDesTImp>
                <cMoneOpe>PYG</cMoneOpe>
                <dDesMoneOpe>Guarani</dDesMoneOpe>
            </gOpeCom>
            
            <!-- Emisor -->
            <gEmis>
                <dRucEm>80012345</dRucEm>
                <dDVEmi>6</dDVEmi>
                <iTipCont>2</iTipCont>      <!-- 1=Física, 2=Jurídica -->
                <cTipReg>8</cTipReg>        <!-- Régimen tributario -->
                <dNomEmi>EMPRESA S.A.</dNomEmi>
                <dDirEmi>Calle Principal 123</dDirEmi>
                <dNumCas>123</dNumCas>
                <cDepEmi>11</cDepEmi>        <!-- Código departamento -->
                <dDesDepEmi>CENTRAL</dDesDepEmi>
                <cDisEmi>1</cDisEmi>
                <dDesDisEmi>ASUNCION</dDesDisEmi>
                <cCiuEmi>1</cCiuEmi>
                <dDesCiuEmi>ASUNCION</dDesCiuEmi>
                <dTelEmi>021-123456</dTelEmi>
                <dEmailE>empresa@email.com</dEmailE>
                <gActEco>
                    <cActEco>47111</cActEco>
                    <dDesActEco>COMERCIO AL POR MENOR</dDesActEco>
                </gActEco>
            </gEmis>
            
            <!-- Receptor -->
            <gDatRec>
                <iNatRec>1</iNatRec>         <!-- 1=Contribuyente, 2=No contribuyente -->
                <iTiOpe>1</iTiOpe>           <!-- Tipo operación: 1=B2B, 2=B2C -->
                <cPaisRec>PRY</cPaisRec>
                <dDesPaisRe>Paraguay</dDesPaisRe>
                <iTiContRec>2</iTiContRec>   <!-- Tipo contribuyente receptor -->
                <dRucRec>80098765</dRucRec>
                <dDVRec>4</dDVRec>
                <dNomRec>CLIENTE S.R.L.</dNomRec>
                <dDirRec>Avenida 456</dDirRec>
                <dNumCasRec>0</dNumCasRec>
                <dTelRec>021-654321</dTelRec>
                <dEmailRec>cliente@email.com</dEmailRec>
            </gDatRec>
        </gDatGralOpe>
        
        <!-- Datos Específicos por Tipo DE -->
        <gDtipDE>
            <!-- Campos Factura Electrónica -->
            <gCamFE>
                <iIndPres>1</iIndPres>       <!-- Indicador presencia -->
                <dDesIndPres>Operación presencial</dDesIndPres>
                <dFecEmNR>2026-01-15</dFecEmNR>
            </gCamFE>
            
            <!-- Items -->
            <gCamItem>
                <dCodInt>PROD001</dCodInt>
                <dDesProSer>Producto de prueba</dDesProSer>
                <cUniMed>77</cUniMed>        <!-- Unidad medida: 77=Unidad -->
                <dDesUniMed>UNI</dDesUniMed>
                <dCantProSer>1.0000</dCantProSer>
                
                <!-- Valores del Item -->
                <gValorItem>
                    <dPUniProSer>100000</dPUniProSer>
                    <dTotBruOpeItem>100000</dTotBruOpeItem>
                    <gValorRestaItem>
                        <dDescItem>0</dDescItem>
                        <dPorcDesIt>0.00</dPorcDesIt>
                        <dTotOpeItem>100000</dTotOpeItem>
                    </gValorRestaItem>
                </gValorItem>
                
                <!-- IVA del Item -->
                <gCamIVA>
                    <iAfecIVA>1</iAfecIVA>   <!-- 1=Gravado 10%, 2=Gravado 5%, 3=Exento -->
                    <dDesAfecIVA>Gravado IVA</dDesAfecIVA>
                    <dPropIVA>100</dPropIVA>
                    <dTasaIVA>10</dTasaIVA>
                    <dBasGravIVA>90909</dBasGravIVA>
                    <dLiqIVAItem>9091</dLiqIVAItem>
                </gCamIVA>
            </gCamItem>
            
            <!-- Totales -->
            <gTotSub>
                <dSubExe>0</dSubExe>         <!-- Subtotal exentas -->
                <dSub5>0</dSub5>             <!-- Subtotal 5% -->
                <dSub10>100000</dSub10>      <!-- Subtotal 10% -->
                <dTotOpe>100000</dTotOpe>    <!-- Total operación -->
                <dTotDesc>0</dTotDesc>
                <dTotDescGlowortem>0</dTotDescGlowortem>
                <dTotAntItem>0</dTotAntItem>
                <dTotAnt>0</dTotAnt>
                <dPorcDescTotal>0.00</dPorcDescTotal>
                <dDescTotal>0</dDescTotal>
                <dAnticipo>0</dAnticipo>
                <dRewordhab>0</dRewordhab>
                <dTotGralOpe>100000</dTotGralOpe>
                <dIVA5>0</dIVA5>
                <dIVA10>9091</dIVA10>
                <dLiqTotIVA5>0</dLiqTotIVA5>
                <dLiqTotIVA10>9091</dLiqTotIVA10>
                <dTotIVA>9091</dTotIVA>
                <dBaseGrav5>0</dBaseGrav5>
                <dBaseGrav10>90909</dBaseGrav10>
                <dTBasGraIVA>90909</dTBasGraIVA>
            </gTotSub>
        </gDtipDE>
        
        <!-- Campos Generales (OBLIGATORIO, aunque esté vacío) -->
        <gCamGen/>
        
        <!-- FIRMA DIGITAL (DENTRO de DE, como ÚLTIMO hijo) -->
        <Signature xmlns="http://www.w3.org/2000/09/xmldsig#">
            <SignedInfo>
                <CanonicalizationMethod Algorithm="http://www.w3.org/2001/10/xml-exc-c14n#"/>
                <SignatureMethod Algorithm="http://www.w3.org/2001/04/xmldsig-more#rsa-sha256"/>
                <Reference URI="#01800123456001001000000122026011512345678901">
                    <Transforms>
                        <Transform Algorithm="http://www.w3.org/2000/09/xmldsig#enveloped-signature"/>
                        <Transform Algorithm="http://www.w3.org/2001/10/xml-exc-c14n#"/>
                    </Transforms>
                    <DigestMethod Algorithm="http://www.w3.org/2001/04/xmlenc#sha256"/>
                    <DigestValue>pmMQga/706ZU8fGk0RZ+poychCgdWyCHfeFEQPBjJAk=</DigestValue>
                </Reference>
            </SignedInfo>
            <SignatureValue>...</SignatureValue>
            <KeyInfo>
                <X509Data>
                    <X509Certificate>MIIGxT...</X509Certificate>
                </X509Data>
            </KeyInfo>
        </Signature>
        
    </DE>
    
    <!-- Campos Fuentes Futuras del DE (FUERA de DE) -->
    <gCamFuFD>
        <dCarQR>https://ekuatia.set.gov.py/consultas/qr?nVersion=150&amp;Id=...&amp;cHashQR=...</dCarQR>
    </gCamFuFD>
    
</rDE>
```

---

## Formatos SOAP - Sync vs Lote

### Endpoint SYNC (recibe.wsdl)
**XML directo, SIN comprimir:**
```xml
<?xml version="1.0" encoding="UTF-8"?>
<env:Envelope xmlns:env="http://www.w3.org/2003/05/soap-envelope">
    <env:Header/>
    <env:Body>
        <rEnviDe xmlns="http://ekuatia.set.gov.py/sifen/xsd">
            <dId>20260115143022</dId>
            <xDE>
                <rDE xmlns="http://ekuatia.set.gov.py/sifen/xsd">
                    <!-- XML DEL DE DIRECTO, sin comprimir -->
                </rDE>
            </xDE>
        </rEnviDe>
    </env:Body>
</env:Envelope>
```

### Endpoint LOTE (recibe-lote.wsdl)
**XML comprimido en ZIP + Base64:**
```xml
<soap:Envelope xmlns:soap="http://www.w3.org/2003/05/soap-envelope">
    <soap:Body>
        <rEnvioLote xmlns="http://ekuatia.set.gov.py/sifen/xsd">
            <dId>20260115143022</dId>
            <xDE>UEsDBBQ... (ZIP Base64 de <rLoteDE><rDE>...</rDE></rLoteDE>)</xDE>
        </rEnvioLote>
    </soap:Body>
</soap:Envelope>
```

### Diferencias Clave

| Característica | SYNC (recibe.wsdl) | LOTE (recibe-lote.wsdl) |
|---------------|---------------------|--------------------------|
| **Elemento raíz** | `rEnviDe` | `rEnvioLote` |
| **Contenido xDE** | XML directo | ZIP + Base64 |
| **Prefijo SOAP** | `env:` recomendado | `soap:` común |
| **Wrapper interno** | Ninguno | `<rLoteDE>` |
| **Header** | Con `<env:Header/>` | Sin header |
| **Declaración XML** | Opcional | No en contenido |
| **Respuesta** | Inmediata | IdLote para consultar |

---

## Configuración SSL/TLS

### Requisitos
- **TLS 1.2 obligatorio** (no soporta TLS 1.3)
- Certificado P12 con clave privada exportable
- Validación de servidor deshabilitada (servidores SIFEN tienen certificados autofirmados)

### Handler HTTP Recomendado
```csharp
var handler = new HttpClientHandler
{
    SslProtocols = SslProtocols.Tls12,
    ClientCertificateOptions = ClientCertificateOption.Manual,
    ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => true
};
handler.ClientCertificates.Add(certificate);
```

### Timeout
```csharp
// Mínimo 120 segundos para operaciones SIFEN
client.Timeout = TimeSpan.FromSeconds(120);
```

---

## Códigos de Respuesta SIFEN

### Códigos de Éxito

| Código | Descripción | Endpoint |
|--------|-------------|----------|
| `0260` | Documento Electrónico aprobado | Sync/Lote |
| `0300` | Lote recibido con éxito | Lote |
| `0362` | Procesamiento de lote concluido | Consulta Lote |
| `0422` | CDC encontrado | Consulta DE |
| `0502` | Contribuyente encontrado | Consulta RUC |

### Códigos de Error

| Código | Descripción | Causa Común | Solución |
|--------|-------------|-------------|----------|
| `0160` | XML Mal Formado | Estructura incorrecta, firma inválida | Verificar estructura, PreserveWhitespace |
| `0300` | Error en firma digital | Certificado/clave incorrectos | Verificar P12 y password |
| `0400` | RUC no habilitado | RUC no registrado para FE | Habilitar en SET |
| `0500` | CDC duplicado | Documento ya enviado | No reenviar |
| `0600` | Timbrado vencido | Fuera de vigencia | Solicitar nuevo |
| `0423` | CDC no encontrado | Documento no existe | Verificar CDC |

---

## Problemas Conocidos y Soluciones

### 1. Error 0160 "XML Mal Formado"

**Causas y Soluciones:**

| Causa | Solución |
|-------|----------|
| `PreserveWhitespace = false` | Agregar `doc.PreserveWhitespace = true;` ANTES de `LoadXml()` |
| Signature fuera de `<DE>` | Insertar Signature DENTRO de DE, como último hijo |
| Falta `<gCamGen/>` | Crear elemento vacío antes de Signature |
| `gCamFuFD` dentro de DE | Mover FUERA de DE (hijo de rDE) |
| schemaLocation con HTTPS | Usar `http://` no `https://` |
| ZIP con GZip | Usar `ZipArchive` no `GZipStream` |

### 2. Error SSL/Conexión

**Síntoma:** Timeout o "connection forcibly closed"

**Solución:**
```csharp
// Retry con backoff
int[] delays = { 1, 2, 3, 5, 8 };
for (int i = 0; i < 5; i++) {
    try {
        return await EnviarInterno(...);
    } catch {
        await Task.Delay(delays[i] * 1000);
    }
}
```

### 3. DigestValue Incorrecto

**Síntoma:** "El documento XML no tiene firma" o firma inválida

**Solución:**
```csharp
// ⚠️ CRÍTICO: PreserveWhitespace ANTES de LoadXml
doc.PreserveWhitespace = true;
doc.LoadXml(xmlString);
```

### 4. cHashQR Incorrecto

**Síntoma:** Error de QR en prevalidador

**Solución:**
```csharp
// Hash se calcula sobre PARÁMETROS + CSC (sin URL base)
string soloParametros = urlQR.Substring(urlQR.IndexOf('?') + 1);
string hash = SHA256ToString(soloParametros + cscValue);
```

---

## Referencias

- **Manual Técnico SIFEN v150:** Especificación oficial del SET
- **Librería Java Roshka:** `github.com/roshkadev/rshk-jsifenlib`
- **Librería PHP:** `github.com/Juan804041/sifen`
- **Librería TypeScript:** `github.com/facturacionelectronicapy/facturacionelectronicapy-xmlgen`
- **Prevalidador SET:** `https://ekuatia.set.gov.py/prevalidador/validacion`

---

*Documento generado: 15 de Enero de 2026*
*Sistema: SistemIA - Blazor Server (.NET 8)*
