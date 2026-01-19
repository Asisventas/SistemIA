# SIFEN Interceptor DLL

## Propósito

Este DLL intercepta las llamadas de PowerBuilder al método `firmarYEnviar` y guarda TODO lo que recibe y envía en archivos de log.

Esto permite capturar:
1. **El XML exacto** que genera PowerBuilder
2. **Los parámetros** (URL, certificado, contraseña)
3. **El SOAP firmado** que se envía a SIFEN
4. **La respuesta** de SIFEN

## Archivos Generados

Al ejecutar una factura desde PowerBuilder, se crean estos archivos en `C:\nextsys - GLP\`:

| Archivo | Contenido |
|---------|-----------|
| `interceptor_log.txt` | Log cronológico de todo el proceso |
| `interceptor_entrada.json` | Parámetros de entrada (URL, certificado, etc.) |
| `interceptor_xml_input.txt` | **XML completo de entrada** (antes de firmar) |
| `interceptor_xml_firmado.txt` | XML después de agregar la firma |
| `interceptor_soap_completo.txt` | **SOAP que se envía a SIFEN** |
| `interceptor_respuesta.txt` | Respuesta del servidor SIFEN |
| `interceptor_retorno.json` | JSON retornado a PowerBuilder |

## Instrucciones de Uso

### 1. Compilar el DLL

```batch
cd SifenInterceptor
COMPILAR.bat
```

### 2. Respaldar el DLL Original

```batch
cd "C:\nextsys - GLP"
copy Sifen.dll Sifen_ORIGINAL.dll
```

### 3. Copiar el Interceptor

```batch
copy "SifenInterceptor\bin\Release\net472\Sifen.dll" "C:\nextsys - GLP\Sifen.dll"
```

### 4. Registrar el DLL (como Administrador)

```batch
REGISTRAR_DLL.bat
```

### 5. Ejecutar una Factura desde PowerBuilder

Generar una factura de prueba en el sistema nextsys.

### 6. Analizar los Archivos

Los archivos `interceptor_*.txt` contendrán toda la información capturada.

## Restaurar DLL Original

Para volver al DLL original:

```batch
cd "C:\nextsys - GLP"
copy Sifen_ORIGINAL.dll Sifen.dll
REGISTRAR_SIFEN.bat
```

## Diferencias Clave Detectadas

### 1. API de Firma RSA

**DLL Funcional (PowerBuilder):**
```csharp
RSA rsaKey = cert.GetRSAPrivateKey();  // API moderna
signedXml.SigningKey = rsaKey;          // DIRECTO
```

**Nuestro código (Blazor):**
```csharp
RSACryptoServiceProvider key = new RSACryptoServiceProvider(new CspParameters(24));
// ... ImportParameters ...
signedXml.SigningKey = key;  // API legacy
```

### 2. SOAP Envelope

**DLL Funcional:**
```xml
<soap:Envelope xmlns:soap="http://www.w3.org/2003/05/soap-envelope">
<soap:Body>
<rEnvioLote xmlns="http://ekuatia.set.gov.py/sifen/xsd">
```

### 3. Content-Type

**DLL Funcional:** `application/xml`  
**Nuestro código:** `application/soap+xml`
