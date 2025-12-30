# SistemIA - Certificados HTTPS

Este paquete contiene herramientas para configurar certificados HTTPS válidos para SistemIA, permitiendo que Chrome y otros navegadores confíen en la conexión HTTPS sin mostrar advertencias.

## ¿Por qué es necesario?

- La función de **cámara web** (reconocimiento facial) requiere HTTPS
- Los navegadores no permiten acceso a `getUserMedia()` sin conexión segura
- Los certificados autofirmados normales muestran advertencias en Chrome

## Contenido del paquete

| Archivo | Descripción |
|---------|-------------|
| `mkcert.exe` | Herramienta para generar certificados (no se instala, solo se usa) |
| `Instalar-Certificado-Servidor.bat` | Ejecutar en el servidor para generar certificados |
| `Instalar-Certificado-Cliente.bat` | Ejecutar en cada PC cliente |
| `SistemIA-CA.crt` | (Se genera) Certificado CA para distribuir a clientes |

## Instrucciones

### Paso 1: En el SERVIDOR

1. Copie la carpeta `Certificados` al servidor
2. Ejecute `Instalar-Certificado-Servidor.bat` como **Administrador**
3. Ingrese la IP del servidor cuando se solicite (ej: `192.168.0.6`)
4. Ingrese el nombre del equipo o presione Enter para usar el predeterminado
5. Se generará:
   - `certificate.p12` en `C:\SistemIA` (para la aplicación)
   - `SistemIA-CA.crt` en `C:\SistemIA` (para distribuir a clientes)

### Paso 2: En cada PC CLIENTE

1. Copie estos archivos al PC cliente:
   - `SistemIA-CA.crt` (generado en el servidor)
   - `Instalar-Certificado-Cliente.bat`
   - `Instalar-Certificado-Cliente.ps1`
2. Ejecute `Instalar-Certificado-Cliente.bat` como **Administrador**
3. **Reinicie Chrome** si estaba abierto

### Paso 3: Acceder a SistemIA

Ahora puede acceder desde cualquier PC cliente:
```
https://192.168.0.6:7060
```

Chrome mostrará el candado verde ✅ sin advertencias.

## Configuración en appsettings.json

El instalador del servidor actualiza automáticamente `appsettings.json`. Si necesita hacerlo manualmente:

```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:5095"
      },
      "Https": {
        "Url": "https://0.0.0.0:7060",
        "Certificate": {
          "Path": "certificate.p12",
          "Password": "changeit"
        }
      }
    }
  }
}
```

## Solución de problemas

### Chrome sigue mostrando "No seguro"
- Asegúrese de haber reiniciado Chrome después de instalar el certificado
- Verifique que el certificado CA esté en "Entidades de certificación raíz de confianza"
- Cierre TODAS las ventanas de Chrome y abra una nueva

### Error "El certificado no es válido para esta IP"
- Regenere el certificado en el servidor incluyendo la IP correcta
- Ejecute nuevamente `Instalar-Certificado-Servidor.bat`

### La aplicación no inicia con HTTPS
- Verifique que `certificate.p12` esté en `C:\SistemIA`
- Verifique la configuración en `appsettings.json`
- La contraseña del certificado es: `changeit`

## Notas técnicas

- Los certificados son válidos por 2 años y 3 meses
- Se usa `mkcert` para generar certificados confiables localmente
- La CA raíz se instala en el almacén de certificados de Windows
- Compatible con Chrome, Edge, Firefox y otros navegadores

## Créditos

- [mkcert](https://github.com/FiloSottile/mkcert) - Herramienta para generar certificados locales confiables
