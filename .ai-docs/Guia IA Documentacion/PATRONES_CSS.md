# Patrones CSS del Proyecto SistemIA

**Última actualización:** 22 de diciembre de 2025

Este documento describe los patrones CSS establecidos en el proyecto para mantener consistencia.

---

## 1. Estructura de Archivos CSS

```
wwwroot/css/
├── bootstrap/           # Bootstrap base (NO modificar)
├── main-layout.css      # Layout principal (.page, .sidebar, .main-content)
├── nav-menu.css         # Estilos del menú de navegación
├── site.css             # Estilos globales y overrides (archivo principal)
├── registro-directo.css # Página de registro directo
└── registro-asistencia.css # Página de asistencia
```

### Orden de Carga (en _Host.cshtml)
1. `bootstrap.min.css`
2. `main-layout.css`
3. `nav-menu.css`
4. `site.css` ← **Tiene prioridad sobre los anteriores**
5. Otros CSS específicos
6. `SistemIA.styles.css` ← CSS aislado de componentes (generado automáticamente)

---

## 2. Sistema de Temas

El proyecto soporta 3 temas: **tenue** (default), **claro**, **oscuro**.

```css
/* Variables definidas en site.css */
:root,
html[data-theme="tenue"] {
    --bg-page: #f5f7fb;
    --bg-surface: #ffffff;
    --text-primary: #1f2937;
    --text-muted: #64748b;
    --bar-bg: #eef2f7;
    --bar-border: #e3e8ef;
}

html[data-theme="claro"] { /* ... */ }
html[data-theme="oscuro"] { /* ... */ }
```

### Uso de Variables
```css
.mi-elemento {
    background-color: var(--bg-surface);
    color: var(--text-primary);
}
```

---

## 3. Patrones del Sidebar

### Estructura HTML
```html
<div class="page sidebar-open">
    <div class="sidebar open">
        <div class="nav-menu d-flex flex-column">
            <!-- contenido -->
        </div>
    </div>
    <main class="main-content">
        <!-- contenido principal -->
    </main>
</div>
```

### Clases Importantes
| Clase | Descripción |
|-------|-------------|
| `.sidebar` | Barra lateral colapsada |
| `.sidebar.open` | Barra lateral expandida |
| `.nav-menu` | Contenedor del menú con scroll |
| `.nav-pills` | Lista de navegación (Bootstrap) |
| `.nav-item` | Item individual del menú |
| `.submenu-container` | Contenedor de submenú |
| `.submenu-container.show` | Submenú expandido |
| `.submenu-items` | Items dentro del submenú |

### Reglas de Scroll (ver FLEXBOX_SCROLL_SIDEBAR.md)
- Solo `.nav-menu` tiene `overflow-y: auto`
- `.sidebar` tiene `overflow: hidden`
- Submenús tienen `max-height: none` cuando están abiertos

---

## 4. Breakpoints Responsivos

```css
/* Desktop grande */
@media (min-width: 1200px) { }

/* Desktop */
@media (min-width: 992px) { }

/* Tablet */
@media (min-width: 768px) { }

/* Móvil grande */
@media (max-width: 767.98px) { }

/* Móvil */
@media (max-width: 575.98px) { }

/* Móvil pequeño */
@media (max-width: 479.98px) { }
```

---

## 5. Convenciones de Nomenclatura

### Clases BEM (parcialmente usado)
```css
.componente { }
.componente__elemento { }
.componente--modificador { }
```

### Clases de Estado
```css
.is-active { }
.is-loading { }
.is-disabled { }
.has-error { }
.has-submenu-open { }
```

### Prefijos de Utilidad
```css
.btn-*     /* Botones */
.text-*    /* Texto */
.bg-*      /* Fondos */
.border-*  /* Bordes */
.p-*, .m-* /* Padding/Margin (Bootstrap) */
```

---

## 6. Uso de !important

### Cuándo Usar
- Override de Bootstrap que no funciona con especificidad normal
- Reglas críticas que NO deben ser sobrescritas
- Media queries que deben prevalecer

### Cuándo NO Usar
- Estilos normales de componentes
- Primera implementación de estilos
- Cuando se puede aumentar especificidad

### Ejemplo Correcto
```css
/* En site.css - override de Bootstrap */
.sidebar .nav-pills {
    flex-direction: column !important; /* Bootstrap lo pone horizontal */
}
```

---

## 7. Patrón para Submenús

```css
/* Estado cerrado */
.submenu-container {
    position: relative !important;
    max-height: 0;
    overflow: hidden;
    transition: max-height 0.3s ease-out;
}

/* Estado abierto */
.submenu-container.show {
    max-height: 9999px !important;
    overflow: visible;
    display: block !important;
}
```

### Transiciones
- Usar `transition` en propiedades específicas, no `all`
- Duración estándar: `0.2s` a `0.3s`
- Easing: `ease`, `ease-out`, `ease-in-out`

---

## 8. Scrollbars Personalizados

```css
/* Firefox */
.elemento-scroll {
    scrollbar-width: thin;
    scrollbar-color: rgba(255,255,255,0.2) transparent;
}

/* Chrome, Safari, Edge */
.elemento-scroll::-webkit-scrollbar {
    width: 6px;
}
.elemento-scroll::-webkit-scrollbar-track {
    background: transparent;
}
.elemento-scroll::-webkit-scrollbar-thumb {
    background: rgba(255,255,255,0.3);
    border-radius: 3px;
}
```

---

## 9. Z-Index Scale

```css
/* Definidos implícitamente en el proyecto */
z-index: 1      /* Elementos elevados básicos */
z-index: 10     /* Dropdowns */
z-index: 100    /* Headers fijos */
z-index: 1000   /* Sidebar */
z-index: 1040   /* Modal backdrop (Bootstrap) */
z-index: 1050   /* Modales */
z-index: 9999   /* Toasts/Notificaciones */
```

---

## 10. Colores del Sidebar

```css
/* Fondo sidebar */
background-color: #1a1f25;

/* Texto */
color: rgba(255, 255, 255, 0.7);  /* Normal */
color: #ffffff;                    /* Hover/Active */

/* Hover */
background-color: rgba(255, 255, 255, 0.1);

/* Active */
background-color: rgba(59, 130, 246, 0.2);
color: #60a5fa;
border-left: 3px solid #60a5fa;
```

---

## 11. Tips de Debugging CSS

1. **Ver estilos aplicados:** F12 → Elements → Computed
2. **Ver qué regla gana:** F12 → Elements → Styles (muestra tachados los perdedores)
3. **Forzar estado hover:** F12 → Elements → :hov → marcar :hover
4. **Cache de CSS:** Ctrl+Shift+R para reload sin cache
5. **Hot reload:** `dotnet watch run` actualiza CSS automáticamente

---

## Notas Importantes

1. **site.css** es el archivo principal para overrides globales
2. Los estilos inline en componentes Razor tienen máxima prioridad
3. CSS aislado (`.razor.css`) se compila en `SistemIA.styles.css`
4. Siempre probar en múltiples breakpoints
5. Verificar que los cambios no rompan otros componentes
