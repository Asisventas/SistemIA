# Documentación de Referencia para IA

Esta carpeta contiene documentación técnica y referencias que ayudan a la IA a entender patrones de solución utilizados en el proyecto SistemIA.

## 🔗 Instrucciones de Copilot
Las instrucciones principales para GitHub Copilot están en:
- **[../.github/copilot-instructions.md](../.github/copilot-instructions.md)** - Reglas y convenciones del proyecto

## Contenido

| Archivo | Descripción |
|---------|-------------|
| [FLEXBOX_SCROLL_SIDEBAR.md](FLEXBOX_SCROLL_SIDEBAR.md) | Solución para scroll en sidebar con flexbox |
| [PATRONES_CSS.md](PATRONES_CSS.md) | Patrones CSS comunes del proyecto |
| [PUBLICACION_DEPLOY.md](PUBLICACION_DEPLOY.md) | Guía de publicación self-contained y problemas de cultura/decimales |
| [GUIA_MIGRACIONES_EF_CORE.md](GUIA_MIGRACIONES_EF_CORE.md) | Guía de migraciones Entity Framework Core |
| [MODULO_NUEVO_GUIA.md](MODULO_NUEVO_GUIA.md) | **Guía completa para crear módulos nuevos** (ejemplo: Notas de Crédito) |
| [NOTAS_SESIONES_RECIENTES.md](NOTAS_SESIONES_RECIENTES.md) | **Notas de sesiones recientes** (HTTPS, mkcert, patrones) |

## Propósito

Cuando se presenten problemas similares en el futuro, la IA puede consultar estos documentos para:
1. Entender soluciones previas aplicadas
2. Evitar repetir errores ya solucionados
3. Mantener consistencia en las soluciones
4. Crear nuevos módulos siguiendo patrones establecidos
5. Recordar configuraciones y contraseñas importantes

## Referencias Rápidas

### Conexión BD
- **Servidor:** `SERVERSIS\SQL2022`
- **Base de datos:** `asiswebapp`

### Puertos
- **HTTP:** `5095`
- **HTTPS:** `7060`

### Contraseñas
- **Certificado instalador (PFX):** `SistemIA2024!`
- **Certificado mkcert:** `changeit`

### Usuario (Modelo)
- PK: `Id_Usu` (NO es "Id")
- Password: `ContrasenaHash` (SHA256)

## Última actualización
29 de diciembre de 2025
