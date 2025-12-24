# Solución: Scroll en Sidebar con Flexbox

**Fecha:** 22 de diciembre de 2025  
**Problema:** Los submenús expandidos en el sidebar no mostraban todos sus items y no era posible hacer scroll para verlos.  
**Severidad:** Media - Afectaba usabilidad en menús con muchos items (ej: "Informes" con ~20 opciones)

---

## Descripción del Problema

Cuando un submenú como "Informes" se expandía, los items inferiores quedaban cortados y no era posible hacer scroll para alcanzarlos. El problema se manifestaba especialmente a niveles de zoom > 65%.

### Síntomas Observados
- El menú principal (Inicio, Ventas, etc.) se desplazaba correctamente
- Los submenús expandidos quedaban truncados
- No aparecía barra de scroll dentro del submenú
- Afectaba principalmente al submenú "Informes" que tiene ~20 items

---

## Investigación y Referencia

### Fuente Principal
**StackOverflow:** [Scrolling a flexbox with overflowing content](https://stackoverflow.com/questions/21515042/scrolling-a-flexbox-with-overflowing-content)

### Conceptos Clave Aprendidos

#### 1. `min-height: 0` es CRUCIAL en Flexbox
Por defecto, los flex items tienen `min-height: auto`, lo que significa que NO pueden ser más pequeños que su contenido. Esto **IMPIDE** que el contenido haga scroll porque el contenedor siempre crece para acomodar todo.

```css
/* INCORRECTO - el contenedor crece indefinidamente */
.flex-container {
    display: flex;
    flex-direction: column;
    /* min-height: auto (implícito) - PROBLEMA */
}

/* CORRECTO - permite que el contenedor se reduzca */
.flex-container {
    display: flex;
    flex-direction: column;
    min-height: 0; /* SOLUCIÓN */
}
```

#### 2. Un Solo Contenedor con `overflow-y: auto`
En una jerarquía flex anidada, **SOLO UN** contenedor debe manejar el scroll. Los contenedores padres deben tener `overflow: hidden` y los hijos `overflow: visible`.

```
.sidebar (overflow: hidden, height: 100vh)
  └── .nav-menu (overflow-y: auto, min-height: 0, flex: 1) ← ÚNICO scroll
        └── .nav-pills (overflow: visible)
              └── .submenu-container.show (overflow: visible, max-height: none)
```

#### 3. Todos los Flex Parents Necesitan `min-height: 0`
Si hay múltiples niveles de flexbox anidados, CADA nivel padre necesita `min-height: 0` para que el scroll funcione en el nivel más interno.

---

## Solución Implementada

### Archivos Modificados
1. `wwwroot/css/site.css` - Reglas principales
2. `wwwroot/css/nav-menu.css` - Estilos del menú de navegación
3. `Shared/NavMenu.razor` - Estilos inline y sección `<style>`

### Reglas CSS Clave

```css
/* 1. El sidebar contiene pero NO hace scroll */
.sidebar,
.sidebar.open {
    overflow: hidden !important;
    height: 100vh;
}

/* 2. nav-menu es el ÚNICO elemento que hace scroll */
.nav-menu {
    display: flex;
    flex-direction: column;
    flex: 1 1 auto;
    min-height: 0;        /* CRUCIAL para flexbox scroll */
    height: 100%;
    overflow-y: auto;     /* ÚNICO scroll container */
    overflow-x: hidden;
}

/* 3. nav-pills NO hace scroll, permite que contenido fluya */
.nav-menu > nav.nav-pills,
.nav-menu > .nav.nav-pills {
    overflow: visible !important;
    max-height: none !important;
    flex: 0 0 auto !important; /* NO flexionar, altura natural */
}

/* 4. Submenús expandidos SIN límite de altura */
.submenu-container.show {
    max-height: 9999px !important; /* Virtualmente infinito */
    height: auto !important;
    overflow: visible !important;
    display: block !important;
    position: relative !important;
}

/* 5. submenu-items también sin límite */
.submenu-items {
    max-height: none !important;
    height: auto !important;
    overflow: visible !important;
}
```

### Errores que se Corrigieron

| Error | Ubicación | Solución |
|-------|-----------|----------|
| `max-height: 60vh` en submenús | site.css líneas 371-386, 1329-1335 | Cambiar a `max-height: none` |
| `overflow-y: auto` en `.nav-pills` | site.css líneas 1485-1510 | Cambiar a `overflow: visible` |
| `max-height: 50vh` en móvil | nav-menu.css línea 300 | Cambiar a `max-height: none` |
| Inline style `max-height:45vh` | NavMenu.razor línea 486 | Eliminar completamente |
| Falta `min-height: 0` | Múltiples contenedores flex | Agregar a `.sidebar`, `.nav-menu` |

---

## Estructura Jerárquica del Sidebar

```html
<div class="sidebar">                          <!-- overflow: hidden, height: 100vh -->
    <div class="nav-menu d-flex flex-column">   <!-- overflow-y: auto, min-height: 0 -->
        <div class="nav-header">...</div>        <!-- flex-shrink: 0 -->
        <nav class="nav nav-pills flex-column">  <!-- overflow: visible, flex: 0 0 auto -->
            <div class="nav-item">
                <button class="submenu-button">Informes</button>
                <div class="submenu-container show">  <!-- max-height: none, overflow: visible -->
                    <div class="submenu-items">       <!-- height: auto -->
                        <!-- ~20 NavLinks aquí -->
                    </div>
                </div>
            </div>
        </nav>
    </div>
</div>
```

---

## Regla de Oro para Futuras Referencias

> **Para que scroll funcione en flexbox:**
> 1. El contenedor de scroll necesita `min-height: 0` y `overflow-y: auto`
> 2. TODOS los contenedores padre flex también necesitan `min-height: 0`
> 3. Solo UN contenedor debe tener `overflow-y: auto`
> 4. Los hijos del scroll container deben tener `overflow: visible`
> 5. NO usar `max-height` fijos en elementos que deben expandirse

---

## Verificación

Para verificar que la solución funciona:
1. Abrir la aplicación en el navegador
2. Expandir el menú "Informes" (tiene ~20 items)
3. Verificar que todos los items son visibles haciendo scroll en el sidebar
4. Probar en diferentes niveles de zoom (especialmente > 65%)
5. Probar en resoluciones móviles (< 768px)

---

## Referencias Adicionales

- [CSS-Tricks: A Complete Guide to Flexbox](https://css-tricks.com/snippets/css/a-guide-to-flexbox/)
- [MDN: min-height](https://developer.mozilla.org/en-US/docs/Web/CSS/min-height)
- [StackOverflow: Flexbox scroll issue](https://stackoverflow.com/questions/21515042/scrolling-a-flexbox-with-overflowing-content)
