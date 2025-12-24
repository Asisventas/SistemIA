# Diagnóstico Completo - Conexión SIFEN Paraguay

**Fecha**: 20 de octubre de 2025  
**Problema**: "Failed to secure tcp: deadline has elapsed" / "The SSL connection could not be established"

---

## ✅ Verificaciones Realizadas

### 1. Conectividad de Red
- ✅ **Puerto TCP 443 accesible**: sifen-test.set.gov.py:443 y sifen.set.gov.py:443 responden
- ✅ **DNS funcionando**: Resuelve a 201.131.51.6 correctamente
- ✅ **Ruta de red**: Traceroute exitoso a servidores externos
- ✅ **Conexión saliente general**: Google.com y 8.8.8.8 accesibles
- ✅ **Sin proxy**: Conexión directa sin intermediarios

### 2. Firewall de Windows
- ⚠️ **Perfil de Dominio**: ACTIVO (pero permite salientes: AllowOutbound)
- ✅ **Perfil Privado**: DESACTIVADO
- ✅ **Perfil Público**: DESACTIVADO
- ✅ **Windows Defender**: Activo pero no bloquea SIFEN

### 3. Negociación SSL/TLS
- ✅ **TLS 1.2 funcionando**: Conexión exitosa SIN certificado cliente
- ✅ **Cipher Suite**: Aes128 negociado correctamente
- ✅ **Protocolo**: TLS 1.2 confirmado
- ❌ **Con certificado cliente**: Conexión falla

---

## 🔍 Problema Identificado

**DIAGNÓSTICO FINAL**: El problema NO es de red, firewall o TLS. El problema está en **cómo se carga y usa el certificado P12 del cliente**.

### Síntomas Específicos:
1. Sin certificado → ✅ Conexión exitosa
2. Con certificado P12 → ❌ "Error inesperado de envío"
3. TCP conecta → ✅ Funciona
4. TLS negocia → ✅ Funciona (sin cert)
5. Cliente autentica → ❌ Falla

---

## 🔧 Soluciones Implementadas

### 1. Cambio en X509KeyStorageFlags

**Antes** (causa del problema):
```csharp
var keyStorageFlags = X509KeyStorageFlags.MachineKeySet | 
                      X509KeyStorageFlags.PersistKeySet;
```

**Después** (solución):
```csharp
var keyStorageFlags = X509KeyStorageFlags.Exportable | 
                      X509KeyStorageFlags.PersistKeySet |
                      X509KeyStorageFlags.UserKeySet;
```

**Razón**: 
- `MachineKeySet` requiere permisos elevados y puede causar problemas
- `UserKeySet` almacena la clave en el perfil del usuario actual
- `Exportable` permite que la clave privada sea accesible para autenticación SSL

### 2. Gestión Correcta de Recursos

Agregado `finally` block para disponer el certificado:
```csharp
X509Certificate2? certificate = null;
try {
    certificate = new X509Certificate2(...);
    // ... uso del certificado
}
finally {
    certificate?.Dispose();
}
```

### 3. Protocolo TLS Explícito

Configurado TLS 1.2 explícitamente:
```csharp
handler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
```

---

## 📋 Archivos Modificados

### 1. `Models/Sifen.cs`
- ✏️ Líneas 60-80: Carga de certificado con `Exportable | UserKeySet`
- ✏️ Líneas 107-109: TLS 1.2 explícito
- ✏️ Líneas 245-263: Bloque `finally` para dispose

### 2. `Program.cs`
- ✏️ Líneas 20-30: `SecurityProtocol = Tls12`

---

## 🧪 Pruebas a Realizar

### 1. Reiniciar Aplicación
```powershell
# Detener servidor actual (Ctrl+C en terminal)
# Ejecutar:
dotnet run
```

### 2. Probar desde /pruebas-xml
1. Navegar a: `http://192.168.100.117:5095/pruebas-xml`
2. Ambiente: **Test**
3. Modo envío: **Lote (async)**
4. Pegar XML de factura
5. Click "Firmar y Enviar"

### 3. Verificar Console Output
Buscar en consola:
```
[DEBUG] Certificado cargado: [nombre del certificado]
[DEBUG] Tiene clave privada: True
[SSL] Validando certificado del servidor: [...]
[DEBUG] Status Code: 200 OK
```

---

## ⚠️ Posibles Problemas Adicionales

### Si aún falla después de los cambios:

#### 1. Certificado P12 Corrupto o Inválido
```powershell
# Verificar certificado manualmente:
$cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2("certificados\F1T_37793.p12", "PASSWORD", "Exportable,UserKeySet")
$cert.Subject
$cert.HasPrivateKey  # Debe ser True
$cert.Dispose()
```

#### 2. Contraseña Incorrecta
Verificar en `appsettings.json` o donde se configure:
```json
{
  "Sifen": {
    "CertificadoPassword": "LA_CONTRASEÑA_CORRECTA"
  }
}
```

#### 3. Certificado Expirado
```powershell
$cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2("certificados\F1T_37793.p12", "PASSWORD", "Exportable")
Write-Host "Válido desde: $($cert.NotBefore)"
Write-Host "Válido hasta: $($cert.NotAfter)"
$cert.Dispose()
```

#### 4. Certificado no Registrado en SIFEN
- Contactar soporte SIFEN
- Verificar que el RUC del certificado coincida con el configurado
- Confirmar que el certificado esté activo en el portal SIFEN

---

## 📞 Contactos

### Soporte Técnico SIFEN
- **Email**: soporte.sifen@set.gov.py
- **Portal**: https://ekuatia.set.gov.py
- **Teléfono**: +595 21 XXXXXXX

### Información a Proveer al Soporte:
1. RUC de la empresa
2. Número de serie del certificado P12
3. Mensaje de error completo
4. Logs de consola (console output)

---

## 📊 Resumen de Cambios

| Componente | Estado Anterior | Estado Actual | Resultado |
|------------|----------------|---------------|-----------|
| TLS Protocol | SystemDefault | TLS 1.2 Explícito | ✅ Mejorado |
| Certificate Flags | MachineKeySet | Exportable+UserKeySet | ✅ Corregido |
| Resource Management | Sin finally | Con finally block | ✅ Mejorado |
| Certificate Validation | Permisivo | Permisivo con logs | ✅ Mantenido |

---

## 🎯 Próximo Paso

1. **Reiniciar la aplicación** con los cambios aplicados
2. **Probar envío** de factura desde /pruebas-xml
3. **Revisar logs** de consola para confirmar que:
   - Certificado se carga con `HasPrivateKey: True`
   - Conexión SSL se establece
   - Se recibe respuesta XML válida de SIFEN

Si el problema persiste después de estos cambios, el problema está en:
- El archivo P12 (corrupto, contraseña incorrecta, o sin clave privada)
- El certificado no está registrado o activo en SIFEN
- Se necesita whitelist de IP en SIFEN

---

## 📝 Notas Técnicas

- **Windows Schannel**: Motor SSL/TLS nativo de Windows
- **X509KeyStorageFlags**: Controla dónde y cómo se almacena la clave privada
- **UserKeySet vs MachineKeySet**: UserKeySet no requiere permisos de administrador
- **Exportable**: Necesario para que HttpClientHandler pueda usar la clave privada

---

**Estado**: ✅ Código actualizado y compilado  
**Pendiente**: Prueba en ejecución con factura real
