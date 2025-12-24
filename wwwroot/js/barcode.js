window.BarcodeInterop = {
  isReady: function(){
    return typeof window.JsBarcode === 'function';
  },
  render: function(selector, code, options){
    try {
      if (!window.JsBarcode) {
        console.warn('BarcodeInterop: JsBarcode not ready');
        return false;
      }
      var el = document.querySelector(selector);
      if(!el) {
        console.warn('BarcodeInterop: selector not found', selector);
        return false;
      }
      var fmt = (options && options.format ? String(options.format).toUpperCase() : 'EAN13');
      var opts = Object.assign({
        format: fmt,
        lineColor: '#000',
        width: 2,
        height: 80,
        displayValue: true,
        margin: 10,
        fontSize: 16,
        // extras opcionales para segunda línea
  subText: null,
  subTextFontSize: 10,
  subTextMarginTop: 0
      }, options||{});
      opts.format = fmt; // asegurar formato normalizado a mayúsculas
      // console.debug('BarcodeInterop.render', { selector, code, opts });
      JsBarcode(el, code, opts);

      // Agregar una segunda línea (nombre de producto) si se solicitó
      if (opts.subText) {
        try {
          var svg = el;
          var widthAttr = parseFloat(svg.getAttribute('width'));
          if (!widthAttr || isNaN(widthAttr)) {
            // intentar con bbox
            try { widthAttr = svg.getBBox().width; } catch(_) { widthAttr = 200; }
          }
          var heightAttr = parseFloat(svg.getAttribute('height'));
          if (!heightAttr || isNaN(heightAttr)) {
            try { heightAttr = svg.getBBox().height; } catch(_) { heightAttr = opts.height || 80; }
          }
          // Asegurar que contenido extra sea visible
          if (svg && svg.style) svg.style.overflow = 'visible';
          var fs = (opts.subTextFontSize || 10);
          var mt = (opts.subTextMarginTop != null ? opts.subTextMarginTop : 0);
          // margen mínimo para que no quede pegado al número del código
          var minGap = 10;
          var gap = Math.max(minGap, mt);

          var text = document.createElementNS('http://www.w3.org/2000/svg', 'text');
          text.setAttribute('x', String(widthAttr / 2));
          // Calcular el anclaje: si existe texto del valor, pegarse a su borde inferior
          var texts = svg.querySelectorAll('text');
          var baseTextEl = texts && texts.length ? texts[texts.length - 1] : null;
          var anchorY;
          if (baseTextEl) {
            try {
              var bb = baseTextEl.getBBox();
              anchorY = bb.y + bb.height + gap;
            } catch(_) {
              anchorY = heightAttr + gap;
            }
          } else {
            anchorY = heightAttr + gap;
          }
          text.setAttribute('y', String(anchorY));
          text.setAttribute('dominant-baseline', 'text-before-edge');
          text.setAttribute('text-anchor', 'middle');
          text.setAttribute('font-size', String(fs));
          text.setAttribute('font-family', 'sans-serif');
          text.style.pointerEvents = 'none';
          text.textContent = String(opts.subText);
          svg.appendChild(text);
          // Ajustar altura del SVG para incluir el subtexto
          var newHeight = Math.max(heightAttr, anchorY + fs);
          svg.setAttribute('height', String(newHeight));

          // Ajuste automático del tamaño si el texto excede el ancho disponible
          try {
            var available = (widthAttr - 2 * (opts.margin || 10));
            if (!available || available <= 0) available = widthAttr;
            var currentSize = fs;
            var len = text.getComputedTextLength ? text.getComputedTextLength() : 0;
            var guard = 0;
            while (len && len > available && currentSize > 6 && guard < 20) {
              currentSize -= 1;
              text.setAttribute('font-size', String(currentSize));
              len = text.getComputedTextLength ? text.getComputedTextLength() : 0;
              guard++;
            }
          } catch(fitErr) {
            // ignorar problemas de medición
          }
        } catch(addTextErr) {
          console.warn('BarcodeInterop: could not append subText', addTextErr);
        }
      }
      return true;
    } catch(e){
      console.error('Barcode render error', e);
      return false;
    }
  },
  print: function(selector){
    try {
      var el = document.querySelector(selector);
      if(!el) return false;
      var w = window.open('', '_blank');
      w.document.write('<html><head><title>Imprimir Código</title>');
      w.document.write('<style>body{margin:0;padding:16px;font-family:sans-serif} .wrap{display:flex;flex-direction:column;align-items:center}</style>');
      w.document.write('</head><body><div class="wrap">');
      w.document.write(el.outerHTML);
      w.document.write('</div></body></html>');
      w.document.close();
      setTimeout(function(){ w.focus(); w.print(); w.close(); }, 250);
      return true;
    } catch(e){
      console.error('Barcode print error', e);
      return false;
    }
  }
};
