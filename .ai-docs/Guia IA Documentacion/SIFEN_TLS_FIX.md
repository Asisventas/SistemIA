# Solución para Problema SSL/TLS con SIFEN Paraguay

## Diagnóstico Actual

**Error**: "The SSL connection could not be established, see inner exception" / "Error inesperado de envío"

**Causa**: El servidor SIFEN requiere cipher suites y configuraciones TLS específicas que no están habilitadas por defecto en Windows.

## Problema Identificado

1. ✓ **Puertos accesibles**: sifen-test.set.gov.py:443 y sifen.set.gov.py:443 responden
2. ✗ **Negociación TLS falla**: Windows (Schannel) cierra la conexión antes de completar el handshake
3. ✓ **Certificado cliente**: Se carga correctamente (F1T_37793.p12)

## Soluciones Posibles

### Opción 1: Habilitar Cipher Suites Compatibles en Windows (RECOMENDADO)

SIFEN Paraguay probablemente requiere uno de estos cipher suites:

```
TLS_RSA_WITH_AES_128_CBC_SHA
TLS_RSA_WITH_AES_256_CBC_SHA
TLS_RSA_WITH_AES_128_CBC_SHA256
TLS_RSA_WITH_AES_256_CBC_SHA256
TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256
TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384
```

#### Pasos para habilitar (Requiere permisos de administrador):

```powershell
# Ejecutar como Administrador
Enable-TlsCipherSuite -Name "TLS_RSA_WITH_AES_128_CBC_SHA" -Position 0
Enable-TlsCipherSuite -Name "TLS_RSA_WITH_AES_256_CBC_SHA" -Position 1
Enable-TlsCipherSuite -Name "TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256" -Position 2
Enable-TlsCipherSuite -Name "TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384" -Position 3

# Verificar
Get-TlsCipherSuite | Where-Object {$_.Name -like "*RSA*"}
```

### Opción 2: Verificar que TLS 1.2 esté habilitado en Registry

Ejecutar como Administrador:

```powershell
# Habilitar TLS 1.2 Client
New-Item -Path "HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.2\Client" -Force
New-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.2\Client" -Name "Enabled" -Value 1 -PropertyType DWord -Force
New-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.2\Client" -Name "DisabledByDefault" -Value 0 -PropertyType DWord -Force

# Reiniciar puede ser necesario
```

### Opción 3: Usar HttpClient con SocketsHttpHandler (Alternative)

Modificar `Sifen.cs` para usar SocketsHttpHandler en lugar de HttpClientHandler:

```csharp
using var handler = new SocketsHttpHandler
{
    SslOptions = new SslClientAuthenticationOptions
    {
        ClientCertificates = new X509Certificate2Collection(certificate),
        EnabledSslProtocols = SslProtocols.Tls12,
        CertificateRevocationCheckMode = X509RevocationMode.NoCheck,
        RemoteCertificateValidationCallback = (sender, cert, chain, errors) => true
    },
    // ... resto de configuración
};
```

### Opción 4: Contactar Soporte SIFEN

Si las opciones anteriores fallan, contactar con soporte técnico de SIFEN para:
1. Confirmar cipher suites requeridos
2. Verificar que el certificado P12 esté activo
3. Confirmar configuración de red/firewall

Contacto: soporte.sifen@set.gov.py

## Información Técnica Actual

### Configuración Actual en Código

- **TLS Protocol**: TLS 1.2 (explícitamente configurado)
- **Certificate Validation**: Deshabilitada (desarrollo)
- **Client Certificate**: Cargado desde P12
- **User-Agent**: "Java/1.8.0_341" (bypass BIG-IP)

### Archivos Modificados

- `Models/Sifen.cs` (líneas 60-200)
- `Program.cs` (líneas 20-30)

### Próximos Pasos

1. **Verificar cipher suites**: Ejecutar comandos de Opción 1 como administrador
2. **Reiniciar aplicación**: Después de cambios en registry/cipher suites
3. **Probar conexión**: Desde página /pruebas-xml
4. **Revisar logs**: Console.WriteLine mostrará detalles de SSL

## Referencias

- [SIFEN Manual Técnico v1.50](https://www.set.gov.py/portal/PARAGUAY-SET/detail?folder-id=repository:collaboration:/sites/PARAGUAY-SET/categories/SET/Factura%20Electr%C3%B3nica/documentFolder.0004)
- [TLS Configuration Windows](https://docs.microsoft.com/en-us/windows-server/security/tls/tls-registry-settings)

## Notas

⚠️ **IMPORTANTE**: Los cambios en cipher suites pueden afectar otras aplicaciones. Realizar en entorno de pruebas primero.

✅ **Estado actual**: Código actualizado con TLS 1.2 explícito. Pendiente verificar cipher suites de Windows.
