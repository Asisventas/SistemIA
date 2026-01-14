# GUÍA RÁPIDA SIFEN v150 - Resumen Técnico

## 📋 Información Extraída del Manual Técnico Oficial

**Fuente**: Manual Técnico SIFEN v150 (SET Paraguay)  
**Total páginas**: 217  
**Fecha documento**: Septiembre 2019

---

## 1. 🔤 CODIFICACIÓN Y DECLARACIÓN XML

### 1.1 Estándar de Codificación (Sección 7.2.1)

```xml
<?xml version="1.0" encoding="UTF-8"?>
```

**Nota importante**: El manual menciona `version="150"` pero esto es un ERROR tipográfico. 
La versión XML estándar es **"1.0"**.

### 1.2 Declaración Namespace (Sección 7.2.2)

```xml
<rDE
  xmlns="http://ekuatia.set.gov.py/sifen/xsd"
  xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
  xsi:schemaLocation="http://ekuatia.set.gov.py/sifen/xsd siRecepDE_v150.xsd">
```

**⚠️ IMPORTANTE - schemaLocation tiene DOS formatos en el manual:**

1. **Formato con ESPACIO** (namespace + archivo separados):
   ```
   xsi:schemaLocation="http://ekuatia.set.gov.py/sifen/xsd siRecepDE_v150.xsd"
   ```

2. **Formato URL completa** (aparece más frecuentemente):
   ```
   xsi:schemaLocation="http://ekuatia.set.gov.py/sifen/xsd/siRecepDE_v150.xsd"
   ```

### 1.3 Restricciones de Namespace

**NO SE PERMITE:**
- ❌ Namespace distintos a los definidos
- ❌ Prefijos de namespace
- Cada documento XML debe tener su namespace individual en el elemento raíz

---

## 2. 📡 SERVICIOS WEB (Web Services)

### 2.1 Servicio SÍNCRONO - siRecepDE (Sección 9.1)

| Aspecto | Valor |
|---------|-------|
| **Función** | Recibir UN solo DE |
| **Proceso** | Sincrónico |
| **Método** | SiRecepDE |
| **Endpoint TEST** | `https://sifen-test.set.gov.py/de/ws/sync/recibe.wsdl` |
| **Endpoint PROD** | `https://sifen.set.gov.py/de/ws/sync/recibe.wsdl` |

#### Estructura de Entrada (Schema XML 2: siRecepDE_v150.xsd)

| ID | Campo | Descripción | Tipo | Longitud | Ocu |
|----|-------|-------------|------|----------|-----|
| ASch01 | `rEnviDe` | Elemento raíz | - | - | - |
| ASch02 | `dId` | ID control envío | N | 1-15 | 1-1 |
| ASch03 | `xDE` | XML del DE | **XML** | - | 1-1 |

**⚠️ CLAVE: El campo `xDE` es de tipo XML (NO comprimido, NO Base64)**

#### Ejemplo SOAP Síncrono:

```xml
<soap:Envelope xmlns:soap="http://www.w3.org/2003/05/soap-envelope">
  <soap:Header/>
  <soap:Body>
    <rEnviDe xmlns="http://ekuatia.set.gov.py/sifen/xsd">
      <dId>10000011111111</dId>
      <xDE>
        <rDE 
          xmlns="http://ekuatia.set.gov.py/sifen/xsd"
          xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
          xsi:schemaLocation="http://ekuatia.set.gov.py/sifen/xsd/siRecepDE_v150.xsd">
          <!-- contenido del DE firmado -->
        </rDE>
      </xDE>
    </rEnviDe>
  </soap:Body>
</soap:Envelope>
```

### 2.2 Servicio ASÍNCRONO/Lote - siRecepLoteDE (Sección 9.2)

| Aspecto | Valor |
|---------|-------|
| **Función** | Recibir lote de varios DE |
| **Proceso** | Asíncrono |
| **Método** | SiRecepLoteDE |
| **Particularidad** | Archivo comprimido ".zip" |
| **Máximo** | 50 DE del mismo tipo por lote |
| **Endpoint TEST** | `https://sifen-test.set.gov.py/de/ws/async/recibe-lote.wsdl` |
| **Endpoint PROD** | `https://sifen.set.gov.py/de/ws/async/recibe-lote.wsdl` |

#### Estructura de Entrada (Schema XML 5: SiRecepLoteDE_v150.xsd)

| ID | Campo | Descripción | Tipo | Longitud | Ocu |
|----|-------|-------------|------|----------|-----|
| BSch01 | `rEnvioLote` | Elemento raíz | - | - | - |
| BSch02 | `dId` | ID control envío | N | 1-15 | 1-1 |
| BSch03 | `xDE` | Archivo comprimido | **B (Base64)** | - | 1-1 |

**⚠️ CLAVE: El campo `xDE` está comprimido en ZIP y codificado en Base64**

---

## 3. 🚨 CÓDIGOS DE ERROR

### 3.1 Validaciones Genéricas (Sección 12.2.6)

| ID | Resultado | Código | Descripción |
|----|-----------|--------|-------------|
| AE01 | XML malformado | **0160** | Error de estructura XML |
| AE02 | Servidor sin respuesta | 0161 | Temporal |
| AE03 | Servidor paralizado | 0162 | Sin tiempo de regreso |
| AE04 | Versión no soportada | 0163 | Formato WS incorrecto |

### 3.2 Causas Comunes del Error 0160 "XML Malformado"

1. **Estructura XML inválida** - tags mal cerrados
2. **Namespace incorrecto** - URL mal formada
3. **schemaLocation incorrecto** - formato o URL errónea
4. **Codificación incorrecta** - caracteres especiales mal escapados
5. **Campos faltantes** - elementos obligatorios ausentes
6. **Formato de datos** - tipos de datos incorrectos
7. **Declaración XML** - falta o incorrecta
8. **Contenido de xDE incorrecto** - comprimido cuando no debe o viceversa

---

## 4. 📐 ESTRUCTURA DEL DOCUMENTO ELECTRÓNICO (DE)

### 4.1 Elemento Raíz `<rDE>`

```xml
<rDE xmlns="http://ekuatia.set.gov.py/sifen/xsd"
     xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
     xsi:schemaLocation="http://ekuatia.set.gov.py/sifen/xsd/siRecepDE_v150.xsd">
  <dVerFor>150</dVerFor>
  <DE Id="CDC_44_DIGITOS">
    <!-- contenido del documento -->
  </DE>
  <Signature xmlns="http://www.w3.org/2000/09/xmldsig#">
    <!-- firma digital -->
  </Signature>
</rDE>
```

### 4.2 Campo CDC (Código de Control)

- **Longitud**: 44 caracteres
- **Usado como**: Atributo `Id` del elemento `<DE>`
- **Estructura**: Ver generación específica en documentación

---

## 5. 🔐 FIRMA DIGITAL (Sección 7.6)

### 5.1 Particularidad de la Firma

La declaración namespace de la firma digital debe realizarse en `<Signature>`:

```xml
<rDE xmlns="http://ekuatia.set.gov.py/sifen/xsd">
  <dVerFor>150</dVerFor>
  <DE Id="CDC...">
    <!-- contenido -->
  </DE>
  <Signature xmlns="http://www.w3.org/2000/09/xmldsig#">
    <!-- contenido de firma -->
  </Signature>
</rDE>
```

### 5.2 Especificaciones de Firma

- **Algoritmo firma**: RSA-SHA256
- **Algoritmo hash**: SHA-256
- **Codificación**: Base64
- **Canonicalización**: http://www.w3.org/TR/2001/REC-xml-c14n-20010315

---

## 6. 📨 ESTÁNDAR DE COMUNICACIÓN (Sección 7.4)

| Aspecto | Especificación |
|---------|----------------|
| **Protocolo** | SOAP versión 1.2 |
| **Style/Encoding** | Document/Literal |
| **Namespace SOAP** | `http://www.w3.org/2003/05/soap-envelope` |
| **TLS** | Obligatorio |

### 6.1 Estructura SOAP

```xml
<soap:Envelope xmlns:soap="http://www.w3.org/2003/05/soap-envelope">
  <soap:Header/>
  <soap:Body>
    <!-- contenido del request -->
  </soap:Body>
</soap:Envelope>
```

**⚠️ NOTA del Manual**: El ejemplo muestra `<soap:body>` (minúscula) pero el estándar SOAP 1.2 usa `<soap:Body>` (mayúscula B). Verificar comportamiento del servidor.

---

## 7. 📊 RESUMEN DE URLs

### Ambiente TEST

| Servicio | URL |
|----------|-----|
| Recepción DE (sync) | `https://sifen-test.set.gov.py/de/ws/sync/recibe.wsdl` |
| Recepción Lote (async) | `https://sifen-test.set.gov.py/de/ws/async/recibe-lote.wsdl` |
| Consulta Lote | `https://sifen-test.set.gov.py/de/ws/consultas/consulta-lote.wsdl` |
| Consulta DE | `https://sifen-test.set.gov.py/de/ws/consultas/consulta.wsdl` |
| Consulta RUC | `https://sifen-test.set.gov.py/de/ws/consultas/consulta-ruc.wsdl` |
| Eventos | `https://sifen-test.set.gov.py/de/ws/eventos/evento.wsdl` |

### Ambiente PRODUCCIÓN

| Servicio | URL |
|----------|-----|
| Recepción DE (sync) | `https://sifen.set.gov.py/de/ws/sync/recibe.wsdl` |
| Recepción Lote (async) | `https://sifen.set.gov.py/de/ws/async/recibe-lote.wsdl` |
| Consulta Lote | `https://sifen.set.gov.py/de/ws/consultas/consulta-lote.wsdl` |
| Consulta DE | `https://sifen.set.gov.py/de/ws/consultas/consulta.wsdl` |
| Consulta RUC | `https://sifen.set.gov.py/de/ws/consultas/consulta-ruc.wsdl` |
| Eventos | `https://sifen.set.gov.py/de/ws/eventos/evento.wsdl` |

---

## 8. ✅ CHECKLIST DE VERIFICACIÓN

### Para Servicio SÍNCRONO (siRecepDE):

- [ ] Declaración XML: `<?xml version="1.0" encoding="UTF-8"?>`
- [ ] Namespace SOAP: `http://www.w3.org/2003/05/soap-envelope`
- [ ] Elemento raíz request: `rEnviDe`
- [ ] Campo `dId`: numérico, 1-15 dígitos
- [ ] Campo `xDE`: contiene el XML del `<rDE>` **SIN comprimir**
- [ ] Namespace rDE: `http://ekuatia.set.gov.py/sifen/xsd`
- [ ] Campo `dVerFor`: valor "150"
- [ ] Atributo `Id` en `<DE>`: CDC de 44 dígitos
- [ ] Firma digital incluida dentro de `<rDE>`
- [ ] URL endpoint correcta: `.../sync/recibe.wsdl`

### Para Servicio ASÍNCRONO/Lote (siRecepLoteDE):

- [ ] Declaración XML: `<?xml version="1.0" encoding="UTF-8"?>`
- [ ] Namespace SOAP: `http://www.w3.org/2003/05/soap-envelope`
- [ ] Elemento raíz request: `rEnvioLote`
- [ ] Campo `dId`: numérico, 1-15 dígitos
- [ ] Campo `xDE`: archivo ZIP codificado en Base64
- [ ] Contenido ZIP: XML con `<rLoteDE>` conteniendo múltiples `<rDE>`
- [ ] Cada `<rDE>` debe tener su propio namespace declarado
- [ ] Máximo 50 DE por lote
- [ ] Todos los DE deben ser del mismo tipo
- [ ] URL endpoint correcta: `.../async/recibe-lote.wsdl`

---

## 9. 🖼️ IMÁGENES EXTRAÍDAS

Se extrajeron **514 imágenes** del manual a la carpeta:
`C:\asis\SistemIA\.ai-docs\SIFEN\Manual_Extraido\imagenes\`

Las imágenes más relevantes incluyen:
- Diagramas de flujo de procesos
- Estructura de schemas XML
- Ejemplos de mensajes SOAP
- Diagramas de arquitectura

---

*Documento generado automáticamente a partir del Manual Técnico SIFEN v150*
