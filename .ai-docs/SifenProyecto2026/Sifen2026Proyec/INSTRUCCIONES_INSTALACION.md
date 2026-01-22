# PAQUETE PARA INSTALAR EN PC CON WINDOWS ACTUALIZADO

## 📦 Archivos Incluidos

1. **Sifen.dll** - DLL corregido compatible con Windows 10/11
2. **REGISTRAR_SIFEN.bat** - Script de instalación automática

---

## 🚀 INSTRUCCIONES DE INSTALACIÓN

### Paso 1: Copiar Archivos
Copia estos archivos a la PC con Windows actualizado:
- `Sifen.dll`
- `REGISTRAR_SIFEN.bat`

**Ubicación sugerida:** `C:\nextsys - GLP\`

### Paso 2: Registrar el DLL
1. Haz **clic derecho** en `REGISTRAR_SIFEN.bat`
2. Selecciona **"Ejecutar como administrador"**
3. Espera a que aparezca el mensaje de éxito

### Paso 3: Verificar
Deberías ver:
```
[OK] Registro 64 bits EXITOSO
[OK] Registro 32 bits EXITOSO
```

---

## 🔧 USO DEL DLL

### Desde PowerBuilder
```vb
OLEObject lo_sifen
string ls_resultado

lo_sifen = CREATE OLEObject
lo_sifen.ConnectToNewObject("Sifen.Sifen")

ls_resultado = lo_sifen.firmarYEnviar( &
    "https://sifen-test.set.gov.py/de/ws/async/recibe-lote.wsdl", &
    "https://ekuatia.set.gov.py/consultas-test/qr?", &
    ls_xml, &
    "C:\ruta\certificado.pfx", &
    "password", &
    "1" &
)

MessageBox("Resultado", ls_resultado)
DESTROY lo_sifen
```

### Desde Blazor Server C#
```csharp
// Agregar referencia al DLL en el proyecto
// O copiar Sifen.cs al proyecto directamente

var sifen = new Sifen.Sifen();
string resultado = sifen.firmarYEnviar(
    url: "https://sifen-test.set.gov.py/de/ws/async/recibe-lote.wsdl",
    urlQR: "https://ekuatia.set.gov.py/consultas-test/qr?",
    xmlString: xmlContent,
    p12FilePath: @"C:\ruta\certificado.pfx",
    certificatePassword: "password",
    tipoFirmado: "1"
);
```

---

## ✅ VENTAJAS DE ESTA VERSIÓN

- ✅ Compatible con Windows 10/11 actualizados
- ✅ No requiere CSP tipo 24
- ✅ Usa API moderna de .NET
- ✅ Soporta SHA-256 nativo
- ✅ Sin error "tipo de proveedor no válido"

---

## 📝 LOGS

Los logs se guardan en:
```
C:\nextsys - GLP\sifen_log.txt
```

Para debugging, revisa este archivo si hay problemas.

---

## ⚠️ SOLUCIÓN PROBLEMA "XML MAL FORMADO"

Si recibes error 0160 "XML Mal Formado" desde Blazor Server:

1. **UTF-8 sin BOM**: `new UTF8Encoding(false)`
2. **Base64 sin saltos**: `Base64FormattingOptions.None`
3. **SOAP correcto**: Ver documentación completa

Consulta `RESUMEN_PROBLEMA_Y_SOLUCIONES.md` para más detalles.

---

## 🆘 SOPORTE

Si tienes problemas:
1. Verifica que ejecutaste como Administrador
2. Revisa `sifen_log.txt`
3. Confirma que el certificado .pfx es válido
4. Verifica que .NET Framework 4.7.2+ esté instalado

---

**Fecha de compilación:** 14 de enero de 2026  
**Versión:** Sifen_26 - Corregida para equipos modernos
