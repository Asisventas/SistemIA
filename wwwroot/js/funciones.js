// =================== FUNCIONES PARA QR EN EXPLORADOR DE VENTAS ===================
window.mostrarModalQR = function (cdc, url) {
    // Configurar el contenido del modal
    document.getElementById('cdcDisplay').textContent = cdc;
    document.getElementById('consultarLink').href = url;
    
    // Generar QR
    const qrContainer = document.getElementById('qrContainer');
    qrContainer.innerHTML = '';
    
    // Crear contenedor para el QR con fondo blanco
    const qrWrapper = document.createElement('div');
    qrWrapper.style.display = 'inline-block';
    qrWrapper.style.padding = '15px';
    qrWrapper.style.background = 'white';
    qrWrapper.style.border = '2px solid #dee2e6';
    qrWrapper.style.borderRadius = '8px';
    qrContainer.appendChild(qrWrapper);
    
    // Usar endpoint LOCAL para generar QR (funciona sin internet)
    const qrImg = document.createElement('img');
    qrImg.style.width = '200px';
    qrImg.style.height = '200px';
    const encodedUrl = encodeURIComponent(url);
    qrImg.src = `/api/qr?url=${encodedUrl}`;
    qrImg.alt = 'Código QR';
    qrImg.onerror = function() {
        // Si falla, mostrar fallback
        qrWrapper.remove();
        mostrarFallbackQR(qrContainer, url);
    };
    qrWrapper.appendChild(qrImg);
    
    // Mostrar modal usando Bootstrap
    const modal = new bootstrap.Modal(document.getElementById('qrModal'));
    modal.show();
};

// =================== FUNCIONES PARA MODAL DE CONSULTA/RESPUESTA SIFEN ===================
window.mostrarModalSifen = function (codigo, mensaje, consulta, respuesta, dId, idLote) {
    try {
        const codigoEl = document.getElementById('sifenCodigo');
        const mensajeEl = document.getElementById('sifenMensaje');
        const consultaEl = document.getElementById('sifenConsultaPre');
        const respuestaEl = document.getElementById('sifenRespuestaPre');
        const dIdEl = document.getElementById('sifenDId');
        const idLoteEl = document.getElementById('sifenIdLote');
        if (codigoEl) codigoEl.textContent = codigo || '-';
        if (mensajeEl) mensajeEl.textContent = mensaje || '-';
        if (consultaEl) consultaEl.textContent = consulta || '';
        if (respuestaEl) respuestaEl.textContent = respuesta || '';
        if (dIdEl) dIdEl.textContent = dId || '-';
        if (idLoteEl) idLoteEl.textContent = idLote || '-';
        const modal = new bootstrap.Modal(document.getElementById('sifenModal'));
        modal.show();
    } catch (e) {
        console.error('Error mostrando modal SIFEN:', e);
        alert('No se pudo mostrar el detalle SIFEN: ' + e.message);
    }
};

function mostrarFallbackQR(container, url) {
    // Fallback: mostrar enlace directo con instrucciones
    container.innerHTML = `
        <div class="alert alert-info">
            <i class="bi bi-info-circle me-2"></i>
            <strong>Código QR no disponible</strong><br>
            Use el botón "Consultar en SIFEN" para verificar la factura directamente.
        </div>
        <div class="text-center">
            <i class="bi bi-qr-code" style="font-size: 4rem; color: #6c757d;"></i>
            <div class="mt-2 text-muted">QR no generado</div>
        </div>
    `;
}

// =================== FUNCIONES PARA REGISTRO DE USUARIOS (/usuarios) ===================
window.capturarFotoConCamara = async function (dotNetHelper) {
    const MAX_WIDTH = 800;

    let modal = document.createElement('div');
    modal.style.cssText = 'position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.6); display: flex; align-items: center; justify-content: center; z-index: 1050;';

    let modalContent = document.createElement('div');
    modalContent.style.cssText = 'background: white; padding: 20px; border-radius: 8px; text-align: center;';

    const video = document.createElement('video');
    const canvas = document.createElement('canvas');
    canvas.style.display = 'none';

    const buttonsContainer = document.createElement('div');
    buttonsContainer.style.marginTop = '15px';

    modalContent.appendChild(video);
    modalContent.appendChild(canvas);
    modalContent.appendChild(buttonsContainer);
    modal.appendChild(modalContent);
    document.body.appendChild(modal);

    let stream;

    const cleanupAndClose = () => {
        if (stream) {
            stream.getTracks().forEach(t => t.stop());
        }
        if (document.body.contains(modal)) {
            document.body.removeChild(modal);
        }
        dotNetHelper.dispose();
    };

    const showCaptureView = () => {
        video.style.display = 'block';
        canvas.style.display = 'none';
        buttonsContainer.innerHTML = `
            <button id="capture-btn" class="btn btn-primary">Capturar</button>
            <button id="cancel-btn" class="btn btn-secondary ms-2">Cancelar</button>
        `;
        document.getElementById('capture-btn').onclick = capturePhoto;
        document.getElementById('cancel-btn').onclick = () => {
            cleanupAndClose();
            dotNetHelper.invokeMethodAsync('RecibirImagenDesdeCamara', null);
        };
    };

    const showPreviewView = (imageDataUrl) => {
        video.style.display = 'none';
        canvas.style.display = 'block';

        const ctx = canvas.getContext('2d');
        const img = new Image();
        img.onload = () => {
            canvas.width = img.width;
            canvas.height = img.height;
            ctx.drawImage(img, 0, 0);
        };
        img.src = imageDataUrl;

        buttonsContainer.innerHTML = `
            <button id="use-photo-btn" class="btn btn-success">Usar esta foto</button>
            <button id="retake-btn" class="btn btn-warning ms-2">Tomar de nuevo</button>
        `;
        document.getElementById('use-photo-btn').onclick = () => {
            dotNetHelper.invokeMethodAsync('RecibirImagenDesdeCamara', imageDataUrl);
            cleanupAndClose();
        };
        document.getElementById('retake-btn').onclick = showCaptureView;
    };

    const capturePhoto = () => {
        const ratio = video.videoWidth / video.videoHeight;
        let newWidth = video.videoWidth;
        let newHeight = video.videoHeight;

        if (newWidth > MAX_WIDTH) {
            newWidth = MAX_WIDTH;
            newHeight = newWidth / ratio;
        }

        const tempCanvas = document.createElement('canvas');
        tempCanvas.width = newWidth;
        tempCanvas.height = newHeight;

        const ctx = tempCanvas.getContext('2d');
        ctx.drawImage(video, 0, 0, newWidth, newHeight);

        const imageDataUrl = tempCanvas.toDataURL('image/jpeg', 0.9);
        showPreviewView(imageDataUrl);
    };

    try {
        stream = await navigator.mediaDevices.getUserMedia({ video: true });
        video.srcObject = stream;
        video.style.width = '100%';
        video.style.maxWidth = '500px';
        video.onloadedmetadata = () => video.play();
        showCaptureView();
    } catch (e) {
        console.error("Error al acceder a la cámara:", e);
        modalContent.innerHTML = `<p class="text-danger">No se pudo acceder a la cámara. Verifique los permisos.</p><button id="close-error-btn" class="btn btn-secondary mt-2">Cerrar</button>`;
        document.getElementById('close-error-btn').onclick = cleanupAndClose;
    }
};

// =================== FUNCIONES PARA GIMNASIO (/clientes/editar - Sección Gimnasio) ===================
window.capturarFotoConCamaraGimnasio = async function (dotNetHelper) {
    const MAX_WIDTH = 800;

    let modal = document.createElement('div');
    modal.style.cssText = 'position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.6); display: flex; align-items: center; justify-content: center; z-index: 1050;';

    let modalContent = document.createElement('div');
    modalContent.style.cssText = 'background: white; padding: 20px; border-radius: 8px; text-align: center;';

    const video = document.createElement('video');
    const canvas = document.createElement('canvas');
    canvas.style.display = 'none';

    const buttonsContainer = document.createElement('div');
    buttonsContainer.style.marginTop = '15px';

    modalContent.appendChild(video);
    modalContent.appendChild(canvas);
    modalContent.appendChild(buttonsContainer);
    modal.appendChild(modalContent);
    document.body.appendChild(modal);

    let stream;

    const cleanupAndClose = () => {
        if (stream) {
            stream.getTracks().forEach(t => t.stop());
        }
        if (document.body.contains(modal)) {
            document.body.removeChild(modal);
        }
        dotNetHelper.dispose();
    };

    const showCaptureView = () => {
        video.style.display = 'block';
        canvas.style.display = 'none';
        buttonsContainer.innerHTML = `
            <button id="capture-btn-gym" class="btn btn-success"><i class="bi bi-camera-fill me-1"></i>Capturar</button>
            <button id="cancel-btn-gym" class="btn btn-secondary ms-2">Cancelar</button>
        `;
        document.getElementById('capture-btn-gym').onclick = capturePhoto;
        document.getElementById('cancel-btn-gym').onclick = () => {
            cleanupAndClose();
            dotNetHelper.invokeMethodAsync('RecibirImagenDesdeCamaraGimnasio', null);
        };
    };

    const showPreviewView = (imageDataUrl) => {
        video.style.display = 'none';
        canvas.style.display = 'block';

        const ctx = canvas.getContext('2d');
        const img = new Image();
        img.onload = () => {
            canvas.width = img.width;
            canvas.height = img.height;
            ctx.drawImage(img, 0, 0);
        };
        img.src = imageDataUrl;

        buttonsContainer.innerHTML = `
            <button id="use-photo-btn-gym" class="btn btn-success"><i class="bi bi-check-lg me-1"></i>Usar esta foto</button>
            <button id="retake-btn-gym" class="btn btn-warning ms-2"><i class="bi bi-arrow-clockwise me-1"></i>Tomar de nuevo</button>
        `;
        document.getElementById('use-photo-btn-gym').onclick = () => {
            dotNetHelper.invokeMethodAsync('RecibirImagenDesdeCamaraGimnasio', imageDataUrl);
            cleanupAndClose();
        };
        document.getElementById('retake-btn-gym').onclick = showCaptureView;
    };

    const capturePhoto = () => {
        const ratio = video.videoWidth / video.videoHeight;
        let newWidth = video.videoWidth;
        let newHeight = video.videoHeight;

        if (newWidth > MAX_WIDTH) {
            newWidth = MAX_WIDTH;
            newHeight = newWidth / ratio;
        }

        const tempCanvas = document.createElement('canvas');
        tempCanvas.width = newWidth;
        tempCanvas.height = newHeight;

        const ctx = tempCanvas.getContext('2d');
        ctx.drawImage(video, 0, 0, newWidth, newHeight);

        const imageDataUrl = tempCanvas.toDataURL('image/jpeg', 0.9);
        showPreviewView(imageDataUrl);
    };

    try {
        stream = await navigator.mediaDevices.getUserMedia({ video: true });
        video.srcObject = stream;
        video.style.width = '100%';
        video.style.maxWidth = '500px';
        video.onloadedmetadata = () => video.play();
        showCaptureView();
    } catch (e) {
        console.error("Error al acceder a la cámara:", e);
        modalContent.innerHTML = `<p class="text-danger">No se pudo acceder a la cámara. Verifique los permisos.</p><button id="close-error-btn-gym" class="btn btn-secondary mt-2">Cerrar</button>`;
        document.getElementById('close-error-btn-gym').onclick = cleanupAndClose;
    }
};

// =================== FUNCIONES PARA ASISTENCIA (/asistencia) ===================
let currentStream;

window.startCamera = async (videoId) => {
    try {
        const videoElement = document.getElementById(videoId);
        if (!videoElement) {
            console.error('Video element not found!');
            return;
        }

        if (currentStream) {
            currentStream.getTracks().forEach(track => track.stop());
        }

        const constraints = { video: true };
        currentStream = await navigator.mediaDevices.getUserMedia(constraints);
        videoElement.srcObject = currentStream;
        videoElement.play();
    } catch (err) {
        console.error("Error accessing camera: ", err);
    }
};

window.stopCamera = (videoId) => {
    const videoElement = document.getElementById(videoId);
    if (currentStream) {
        currentStream.getTracks().forEach(track => track.stop());
    }
    if (videoElement) {
        videoElement.srcObject = null;
    }
};

window.captureFrame = (videoId, canvasId) => {
    const videoElement = document.getElementById(videoId);
    const canvasElement = document.getElementById(canvasId);
    if (!videoElement || !canvasElement) {
        console.error('Video or Canvas element not found!');
        return null;
    }

    const context = canvasElement.getContext('2d');
    canvasElement.width = videoElement.videoWidth;
    canvasElement.height = videoElement.videoHeight;
    context.drawImage(videoElement, 0, 0, videoElement.videoWidth, videoElement.videoHeight);

    return canvasElement.toDataURL('image/jpeg');
};

// =================== SÍNTESIS DE VOZ ===================
window.hablarTexto = function (texto, idioma = 'es-ES') {
    if ('speechSynthesis' in window) {
        window.speechSynthesis.cancel();
        const utterance = new SpeechSynthesisUtterance(texto);
        utterance.lang = idioma;
        utterance.rate = 0.9;
        utterance.pitch = 1.1;
        window.speechSynthesis.speak(utterance);
    } else {
        console.error('El navegador no soporta la síntesis de voz.');
    }
};

// =================== FUNCIONES PARA LISTADO DE ASISTENCIA ===================
window.getAsistenciasHtml = function (tableId) {
    var table = document.getElementById(tableId);
    return table ? table.outerHTML : "";
};

window.printHtml = function (html) {
    var win = window.open('', '', 'height=700,width=900');
    win.document.write('<html><head><title>Imprimir Asistencias</title></head><body>');
    win.document.write(html);
    win.document.write('</body></html>');
    win.document.close();
    win.print();
};

// Función para imprimir HTML con cabecera personalizada (para facturas) - Versión mejorada sin loops
window.printHtmlWithHead = function (headContent, bodyContent) {
    try {
        console.log('🖨️ Iniciando impresión de factura...');
        
        // Crear un iframe oculto para la impresión (método más confiable)
        const iframe = document.createElement('iframe');
        iframe.style.cssText = 'position: absolute; top: -10000px; left: -10000px; width: 1px; height: 1px; border: none; visibility: hidden;';
        iframe.id = 'print-iframe-' + Date.now(); // ID único para evitar conflictos
        
        document.body.appendChild(iframe);
        
        const iframeDoc = iframe.contentWindow.document;
        
        // Crear el documento HTML completo
        const fullHtml = `<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    ${headContent}
</head>
<body>
    ${bodyContent}
</body>
</html>`;
        
        iframeDoc.open();
        iframeDoc.write(fullHtml);
        iframeDoc.close();
        
        console.log('📄 Documento HTML generado en iframe');
        
        // Manejar la carga del iframe de forma más robusta
        let printExecuted = false;
        
        const executePrint = () => {
            if (printExecuted) return; // Prevenir ejecución múltiple
            printExecuted = true;
            
            try {
                console.log('🔄 Ejecutando impresión...');
                
                // Asegurar que el iframe tenga foco
                iframe.contentWindow.focus();
                
                // Configurar evento de impresión
                iframe.contentWindow.addEventListener('beforeunload', () => {
                    console.log('🗑️ Limpiando iframe después de impresión');
                    setTimeout(() => cleanupIframe(iframe), 100);
                });
                
                // Ejecutar impresión
                iframe.contentWindow.print();
                console.log('✅ Comando de impresión enviado');
                
                // Cleanup automático después de 3 segundos como respaldo
                setTimeout(() => cleanupIframe(iframe), 3000);
                
            } catch (printError) {
                console.error('❌ Error al ejecutar print():', printError);
                printExecuted = false; // Resetear para intentar fallback
                tryPopupPrint(headContent, bodyContent, iframe);
            }
        };
        
        // Múltiples formas de detectar cuando está listo
        iframe.onload = executePrint;
        
        // Fallback con timeout
        setTimeout(() => {
            if (!printExecuted && iframe.contentDocument && iframe.contentDocument.readyState === 'complete') {
                executePrint();
            }
        }, 1000);
        
        // Último recurso
        setTimeout(() => {
            if (!printExecuted) {
                console.warn('⚠️ Timeout de carga, intentando impresión forzada');
                executePrint();
            }
        }, 2000);
        
    } catch (error) {
        console.error('💥 Error crítico en printHtmlWithHead:', error);
        tryPopupPrint(headContent, bodyContent);
    }
};

// Función para limpiar iframe de forma segura
function cleanupIframe(iframe) {
    try {
        if (iframe && iframe.parentNode && document.body.contains(iframe)) {
            iframe.parentNode.removeChild(iframe);
            console.log('🧹 Iframe de impresión eliminado');
        }
    } catch (e) {
        console.warn('⚠️ Error al limpiar iframe:', e);
    }
}

// Función fallback con pop-up mejorada
function tryPopupPrint(headContent, bodyContent, cleanupIframe) {
    try {
        console.log('🚀 Intentando impresión con ventana pop-up...');
        
        const winFeatures = 'height=800,width=1200,scrollbars=yes,resizable=yes,menubar=no,toolbar=no,location=no,status=no';
        const printWindow = window.open('', '_blank', winFeatures);
        
        if (!printWindow) {
            console.error('🚫 Bloqueador de pop-ups activo');
            showPrintInstructions();
            return;
        }
        
        const fullHtml = `<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    ${headContent}
    <script>
        // Auto-print cuando la ventana esté lista
        window.addEventListener('load', function() {
            setTimeout(function() {
                window.print();
            }, 500);
        });
        
        // Cerrar después de imprimir
        window.addEventListener('afterprint', function() {
            setTimeout(function() {
                window.close();
            }, 1000);
        });
    </script>
</head>
<body>
    ${bodyContent}
</body>
</html>`;
        
        printWindow.document.write(fullHtml);
        printWindow.document.close();
        printWindow.focus();
        
        console.log('✅ Ventana de impresión creada exitosamente');
        
    } catch (popupError) {
        console.error('💥 Error con ventana pop-up:', popupError);
        showPrintInstructions();
    } finally {
        // Limpiar iframe si existe
        if (cleanupIframe && typeof cleanupIframe === 'object') {
            cleanupIframe(cleanupIframe);
        }
    }
}

// Función para mostrar instrucciones cuando falla la impresión
function showPrintInstructions() {
    const mensaje = `🖨️ INSTRUCCIONES DE IMPRESIÓN

Para imprimir la factura:

1️⃣ Habilite los pop-ups para este sitio
2️⃣ O use Ctrl+P para imprimir esta página
3️⃣ O descargue el archivo HTML desde el botón de descarga

❓ ¿Necesita ayuda?
- Verifique la configuración de su navegador
- Asegúrese de tener una impresora configurada`;

    alert(mensaje);
}

window.downloadFileFromBase64 = function (filename, base64, mimeType) {
    var link = document.createElement('a');
    link.href = 'data:' + mimeType + ';base64,' + base64;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

window.exportAsistenciasToPdf = function (tableId) {
    var { jsPDF } = window.jspdf;
    var doc = new jsPDF();
    var table = document.getElementById(tableId);
    if (!table) return;
    var rows = [];
    for (var i = 0, row; row = table.rows[i]; i++) {
        var cols = [];
        for (var j = 0, col; col = row.cells[j]; j++) {
            cols.push(col.innerText);
        }
        rows.push(cols);
    }
    let y = 10;
    rows.forEach(function (rowArr) {
        let x = 10;
        rowArr.forEach(function (cell) {
            doc.text(cell, x, y);
            x += 40;
        });
        y += 10;
    });
    doc.save('asistencias.pdf');
};

// =================== FUNCIONES PARA LISTADO DE ASISTENCIAS ===================
window.descargarArchivo = (fileName, contentType, content) => {
    const blob = new Blob([new Uint8Array(content)], { type: contentType });
    const url = URL.createObjectURL(blob);
    
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    link.style.display = 'none';
    
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    
    URL.revokeObjectURL(url);
};

window.mostrarAlerta = (mensaje) => {
    alert(mensaje);
};

// =================== FUNCIONES PARA IMPRESIÓN DE TICKETS ===================

// Función para imprimir contenido HTML del ticket directamente
window.printTicketHtml = function (htmlContent, cssContent) {
    try {
        console.log('Preparando impresión directa del ticket...');
        
        // Crear iframe oculto para impresión
        var iframe = document.createElement('iframe');
        iframe.style.position = 'absolute';
        iframe.style.width = '0';
        iframe.style.height = '0';
        iframe.style.border = 'none';
        iframe.style.left = '-9999px';
        document.body.appendChild(iframe);
        
        var doc = iframe.contentWindow.document;
        doc.open();
        doc.write(`
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8">
                <title>Ticket de Venta</title>
                <style>
                    @page {
                        size: 80mm auto;
                        margin: 0;
                    }
                    body {
                        margin: 0;
                        padding: 0;
                        font-family: 'Courier New', Courier, monospace;
                        font-size: 11px;
                        width: 80mm;
                    }
                    ${cssContent || ''}
                </style>
            </head>
            <body>
                ${htmlContent}
            </body>
            </html>
        `);
        doc.close();
        
        // Esperar a que cargue y luego imprimir
        iframe.onload = function() {
            setTimeout(function() {
                try {
                    iframe.contentWindow.focus();
                    iframe.contentWindow.print();
                    
                    // Remover iframe después de un tiempo
                    setTimeout(function() {
                        if (document.body.contains(iframe)) {
                            document.body.removeChild(iframe);
                        }
                    }, 2000);
                } catch (e) {
                    console.error('Error al imprimir desde iframe:', e);
                    document.body.removeChild(iframe);
                }
            }, 300);
        };
        
        return true;
    } catch (error) {
        console.error('Error al preparar impresión del ticket:', error);
        return false;
    }
};

// Función para imprimir el elemento del ticket visible en la página
window.printTicketElement = function (elementId) {
    try {
        var element = document.getElementById(elementId);
        if (!element) {
            console.error('Elemento no encontrado:', elementId);
            return false;
        }
        
        var htmlContent = element.outerHTML;
        
        // Obtener estilos del ticket - Fuentes más grandes y legibles
        var styles = `
            .ticket-preview-container {
                width: 80mm;
                max-width: 80mm;
                background: white;
                padding: 4mm;
                font-family: 'Segoe UI', Tahoma, Arial, sans-serif;
                font-size: 12px;
                line-height: 1.4;
                color: #000;
            }
            .ticket-header { text-align: center; margin-bottom: 3mm; }
            .ticket-logo { text-align: center; margin-bottom: 3mm; }
            .ticket-logo img { max-width: 65%; max-height: 14mm; }
            .ticket-empresa { font-size: 14px; font-weight: bold; margin-bottom: 2mm; }
            .ticket-ruc { font-size: 12px; font-weight: bold; margin: 1mm 0; }
            .ticket-direccion, .ticket-telefono, .ticket-actividad { font-size: 11px; margin: 0.5mm 0; }
            .ticket-tipo-doc { text-align: center; font-size: 12px; font-weight: bold; padding: 2mm 0; border: 1px solid #000; margin: 2mm 0; }
            .ticket-separator { border-bottom: 1px dashed #000; margin: 2mm 0; }
            .ticket-separator-double { border-bottom: 1px solid #000; margin: 2mm 0; }
            .ticket-row { display: flex; justify-content: space-between; font-size: 11px; margin: 1mm 0; }
            .ticket-label { font-weight: bold; }
            .ticket-factura-nro { font-weight: bold; font-size: 12px; }
            .ticket-detalle-header { display: flex; font-weight: bold; font-size: 10px; border-bottom: 1px dashed #000; padding-bottom: 1mm; }
            .ticket-detalle-header .col-cant { width: 9mm; text-align: center; }
            .ticket-detalle-header .col-desc { flex: 1; text-align: left; padding-left: 1mm; }
            .ticket-detalle-header .col-precio { width: 13mm; text-align: right; }
            .ticket-detalle-header .col-exenta { width: 12mm; text-align: right; }
            .ticket-detalle-header .col-iva5 { width: 12mm; text-align: right; }
            .ticket-detalle-header .col-iva10 { width: 13mm; text-align: right; }
            .ticket-detalle-item { display: flex; font-size: 10px; padding: 1mm 0; border-bottom: 1px dotted #ccc; }
            .ticket-detalle-item .col-cant { width: 9mm; text-align: center; }
            .ticket-detalle-item .col-desc { flex: 1; text-align: left; padding-left: 1mm; overflow: hidden; }
            .ticket-detalle-item .col-precio { width: 13mm; text-align: right; }
            .ticket-detalle-item .col-exenta { width: 12mm; text-align: right; }
            .ticket-detalle-item .col-iva5 { width: 12mm; text-align: right; }
            .ticket-detalle-item .col-iva10 { width: 13mm; text-align: right; }
            .ticket-subtotales { font-size: 11px; }
            .col-exenta-total, .col-iva5-total, .col-iva10-total { width: 16mm; text-align: right; font-weight: bold; }
            .ticket-total-section { padding: 3mm 0; margin: 2mm 0; }
            .ticket-total { font-weight: bold; font-size: 16px; background: #000 !important; color: white !important; padding: 3mm 4mm; margin: 0 -4mm; -webkit-print-color-adjust: exact; print-color-adjust: exact; border: 0.5mm solid #000; }
            .ticket-liquidacion { font-size: 11px; padding: 2mm 0; }
            .ticket-liquidacion-header { font-weight: bold; text-align: center; font-size: 11px; margin-bottom: 2mm; }
            .ticket-liquidacion-grid { display: flex; justify-content: space-between; font-size: 10px; }
            .liquidacion-col { text-align: center; flex: 1; }
            .liq-label { font-weight: bold; display: block; }
            .liq-value { display: block; }
            .ticket-footer { text-align: center; margin-top: 4mm; padding-bottom: 3mm; }
            .ticket-gracias { font-weight: bold; font-size: 12px; margin-bottom: 2mm; }
            .ticket-sistema { font-size: 11px; color: #333; font-weight: 500; margin-top: 2mm; padding-bottom: 2mm; }
        `;
        
        return window.printTicketHtml(htmlContent, styles);
    } catch (error) {
        console.error('Error al imprimir elemento:', error);
        return false;
    }
};

// Función para imprimir ticket con configuración de impresora
window.printTicketDirect = function (printerName) {
    try {
        console.log('Imprimiendo ticket directo a impresora:', printerName || 'predeterminada');
        
        // Intentar imprimir - el navegador mostrará el diálogo pero se puede preseleccionar
        // la impresora si el usuario la tiene como predeterminada
        window.print();
        
        return true;
    } catch (error) {
        console.error('Error al imprimir ticket:', error);
        return false;
    }
};

// Función para auto-imprimir y cerrar después
window.autoPrintAndClose = function (delay) {
    try {
        setTimeout(function() {
            window.print();
            // Después de imprimir, cerrar la ventana si es popup
            setTimeout(function() {
                if (window.opener) {
                    window.close();
                }
            }, 1000);
        }, delay || 500);
        return true;
    } catch (error) {
        console.error('Error en auto-impresión:', error);
        return false;
    }
};

// Función para imprimir ventana actual con enfoque en ticket
window.printCurrentTicket = function () {
    try {
        // Forzar que el body se ajuste al contenido del ticket
        document.body.style.margin = '0';
        document.body.style.padding = '0';
        
        window.print();
        return true;
    } catch (error) {
        console.error('Error al imprimir ticket:', error);
        return false;
    }
};

// Función para abrir ventana de impresión de presupuesto
window.printPresupuesto = function (idPresupuesto) {
    try {
        const url = `/presupuestos/explorar?print=${idPresupuesto}`;
        window.open(url, '_blank');
    } catch (error) {
        console.error('Error al abrir impresión de presupuesto:', error);
    }
};

// Función para descargar archivos generados (PDF, Word, etc.)
window.downloadFile = function (base64Data, fileName, contentType) {
    try {
        // Convertir base64 a bytes
        const byteCharacters = atob(base64Data);
        const byteNumbers = new Array(byteCharacters.length);
        
        for (let i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
        }
        
        const byteArray = new Uint8Array(byteNumbers);
        const blob = new Blob([byteArray], { type: contentType });
        
        // Crear enlace de descarga
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        link.style.display = 'none';
        
        // Agregar al DOM, hacer clic y remover
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        
        // Liberar memoria
        window.URL.revokeObjectURL(url);
        
        console.log(`Archivo descargado: ${fileName}`);
    } catch (error) {
        console.error('Error al descargar archivo:', error);
        alert('Error al descargar el archivo: ' + error.message);
    }
};

// =================== GENERACIÓN PDF TICKET TÉRMICO (jsPDF) ===================

// Función para generar PDF de ticket térmico 80mm con opción de impresión directa
window.generarPdfTicket = function (ticketData, autoPrint = true) {
    try {
        if (typeof jspdf === 'undefined') {
            console.error('jsPDF no está cargado. Reintentando en 1 segundo...');
            setTimeout(() => window.generarPdfTicket(ticketData, autoPrint), 1000);
            return;
        }
        
        if (!jspdf.jsPDF) {
            console.error('jsPDF.jsPDF no está disponible');
            alert('jsPDF no está cargado correctamente. Por favor, recargue la página.');
            return;
        }

        const { jsPDF } = jspdf;
        
        const anchoPuntos = 227; // 80mm
        const alturaPuntos = 500;
        
        const doc = new jsPDF({
            unit: 'pt',
            format: [anchoPuntos, alturaPuntos],
            orientation: 'portrait'
        });
        
        let y = 10;
        const margen = 6;
        const anchoUtil = anchoPuntos - (margen * 2);
        
        function calcularAncho(texto, fontSize) {
            return doc.getTextWidth(texto);
        }
        
        function textoCentrado(texto, fontSize, negrita = false) {
            doc.setFontSize(fontSize);
            doc.setFont('courier', negrita ? 'bold' : 'normal');
            const ancho = calcularAncho(texto, fontSize);
            const x = (anchoPuntos - ancho) / 2;
            doc.text(texto, x, y);
            y += fontSize * 1.2;
        }
        
        function textoIzquierda(texto, fontSize, negrita = false) {
            doc.setFontSize(fontSize);
            doc.setFont('courier', negrita ? 'bold' : 'normal');
            doc.text(texto, margen, y);
            y += fontSize * 1.2;
        }
        
        function lineaDosColumnas(label, valor, fontSize = 8) {
            doc.setFontSize(fontSize);
            doc.setFont('courier', 'bold');
            doc.text(label + ' ', margen, y);
            doc.setFont('courier', 'normal');
            const anchoLabel = calcularAncho(label + ' ', fontSize);
            doc.text(valor || '', margen + anchoLabel, y);
            y += fontSize * 1.2;
        }
        
        function lineaSeparadora() {
            doc.setFontSize(7);
            doc.setFont('courier', 'normal');
            doc.text('-'.repeat(46), margen, y);
            y += 8;
        }
        
        function lineaSeparadoraDoble() {
            doc.setFontSize(7);
            doc.setFont('courier', 'normal');
            doc.text('='.repeat(46), margen, y);
            y += 8;
        }
        
        function dividirTexto(texto, maxAncho) {
            if (!texto) return [''];
            const palabras = texto.split(' ');
            const lineas = [];
            let lineaActual = '';
            
            for (const palabra of palabras) {
                const test = lineaActual ? lineaActual + ' ' + palabra : palabra;
                const ancho = calcularAncho(test, 8);
                
                if (ancho > maxAncho && lineaActual) {
                    lineas.push(lineaActual);
                    lineaActual = palabra;
                } else {
                    lineaActual = test;
                }
            }
            if (lineaActual) lineas.push(lineaActual);
            return lineas.length > 0 ? lineas : [''];
        }
        
        function dividirTextoChars(texto, maxChars) {
            if (!texto) return [''];
            const palabras = texto.split(' ');
            const lineas = [];
            let lineaActual = '';
            
            for (const palabra of palabras) {
                const test = lineaActual ? lineaActual + ' ' + palabra : palabra;
                if (test.length > maxChars && lineaActual) {
                    lineas.push(lineaActual);
                    lineaActual = palabra.length > maxChars ? palabra.substring(0, maxChars) : palabra;
                } else if (test.length > maxChars) {
                    lineas.push(test.substring(0, maxChars));
                    lineaActual = '';
                } else {
                    lineaActual = test;
                }
            }
            if (lineaActual) lineas.push(lineaActual);
            return lineas.length > 0 ? lineas : [''];
        }
        
        // ========== ENCABEZADO ==========
        
        // LOGO (si está habilitado)
        if (ticketData.mostrarLogo && ticketData.logoUrl) {
            try {
                const maxLogoWidth = anchoUtil * 0.75;
                const maxLogoHeight = 50;
                const logoX = (anchoPuntos - maxLogoWidth) / 2;
                doc.addImage(ticketData.logoUrl, 'PNG', logoX, y, maxLogoWidth, maxLogoHeight);
                y += maxLogoHeight + 5;
            } catch (e) {
                console.warn('No se pudo cargar el logo:', e);
            }
        }
        
        // Actividad económica / Rubro de la empresa
        if (ticketData.rubroEmpresa) {
            doc.setFontSize(7);
            const lineasRubro = dividirTexto(ticketData.rubroEmpresa, anchoUtil);
            lineasRubro.forEach(linea => textoCentrado(linea, 7));
        }
        
        // Dirección de la empresa
        if (ticketData.direccion) {
            doc.setFontSize(7);
            const lineasDir = dividirTexto(ticketData.direccion, anchoUtil);
            lineasDir.forEach(linea => textoCentrado(linea, 7));
        }
        
        // RUC y Sucursal en una línea
        doc.setFontSize(8);
        let lineaRucSuc = 'Ruc: ' + ticketData.ruc;
        if (ticketData.sucursal) {
            lineaRucSuc += '       ' + ticketData.sucursal;
        }
        textoCentrado(lineaRucSuc, 8);
        
        y += 6;
        
        // FACTURA (tipo de documento grande y negrita)
        textoCentrado(ticketData.tipoDoc, 14, true);
        
        // Timbrado Nº
        if (ticketData.timbrado) {
            textoCentrado('Timbrado N° ' + ticketData.timbrado, 10, true);
        }
        
        y += 3;
        lineaSeparadoraDoble();
        
        // ========== DATOS DE LA FACTURA ==========
        lineaDosColumnas('Nro:', ticketData.numero, 8);
        lineaDosColumnas('Fecha:', ticketData.fecha, 8);
        
        if (ticketData.vigenciaDel && ticketData.vigenciaAl) {
            lineaDosColumnas('Vigencia:', ticketData.vigenciaDel + ' al ' + ticketData.vigenciaAl, 8);
        }
        
        // ========== DATOS DEL CLIENTE ==========
        if (ticketData.cliente) {
            const lineasCliente = dividirTexto(ticketData.cliente, anchoUtil - 50);
            lineaDosColumnas('Cliente:', lineasCliente[0], 8);
            for (let i = 1; i < lineasCliente.length; i++) {
                doc.setFontSize(8);
                doc.setFont('courier', 'normal');
                doc.text('         ' + lineasCliente[i], margen, y);
                y += 9;
            }
        }
        
        if (ticketData.clienteRuc) {
            lineaDosColumnas('RUC/CI:', ticketData.clienteRuc, 8);
        }
        
        lineaDosColumnas('Condicion:', ticketData.condicion, 8);
        
        y += 2;
        lineaSeparadoraDoble();
        
        // ========== DETALLE DE PRODUCTOS ==========
        lineaSeparadora();
        
        doc.setFontSize(7);
        doc.setFont('courier', 'bold');
        
        const colDesc = margen;
        const colCant = margen + anchoUtil * 0.48;
        const colPrecio = margen + anchoUtil * 0.62;
        const colTotal = margen + anchoUtil * 0.80;
        
        doc.text('DESCRIPCION', colDesc, y);
        doc.text('CANT', colCant, y);
        doc.text('P.UNIT', colPrecio, y);
        doc.text('TOTAL', colTotal, y);
        y += 10;
        
        doc.setFont('courier', 'normal');
        
        ticketData.items.forEach(item => {
            const cantStr = item.cantidad.toString();
            const precioStr = item.precio || '0';
            const totalStr = item.total || '0';
            
            const maxDescChars = 20;
            const lineasDesc = dividirTextoChars(item.descripcion, maxDescChars);
            
            doc.text(lineasDesc[0], colDesc, y);
            doc.text(cantStr, colCant, y);
            doc.text(precioStr, colPrecio, y);
            doc.text(totalStr, colTotal, y);
            y += 9;
            
            for (let i = 1; i < lineasDesc.length; i++) {
                doc.text(lineasDesc[i], colDesc, y);
                y += 9;
            }
        });
        
        lineaSeparadoraDoble();
        
        // ========== DETALLE TOTALES (BASE IMPONIBLE) ==========
        if (ticketData.subtotalGravadas5 || ticketData.subtotalGravadas10 || ticketData.subtotalExentas) {
            lineaDosColumnas('Gravadas 5%:', ticketData.subtotalGravadas5 || '0', 8);
            lineaDosColumnas('Subtotal:', ticketData.subtotal || '0', 8);
        }
        
        // ========== DETALLE DEL IMPUESTO ==========
        if (ticketData.liquidacionIva5 || ticketData.liquidacionIva10) {
            lineaDosColumnas('Liq. IVA 5%:', ticketData.liquidacionIva5 || '0', 8);
            lineaDosColumnas('Total IVA:', ticketData.totalIva || '0', 8);
        }
        
        y += 3;
        lineaSeparadora();
        
        // ========== TOTAL EN RECUADRO ==========
        y += 3;
        doc.setFontSize(11);
        doc.setFont('courier', 'bold');
        const textoTotal = 'TOTAL: ' + ticketData.total;
        const anchoTextoTotal = calcularAncho(textoTotal, 11);
        const xTotal = (anchoPuntos - anchoTextoTotal) / 2;
        
        // Dibujar recuadro
        doc.setDrawColor(0);
        doc.setLineWidth(1);
        doc.rect(xTotal - 8, y - 10, anchoTextoTotal + 16, 16);
        doc.text(textoTotal, xTotal, y);
        y += 20;
        
        // ========== PIE ==========
        y += 8;
        textoCentrado('GRACIAS POR SU COMPRA!', 10, true);
        textoCentrado('SistemIA', 9);
        
        y += 10;
        
        // Ajustar altura del documento
        const alturaFinal = Math.max(y + 20, 400);
        doc.internal.pageSize.height = alturaFinal;
        
        // Generar PDF
        const pdfBlob = doc.output('blob');
        const pdfUrl = URL.createObjectURL(pdfBlob);
        
        if (autoPrint) {
            const ventanaImpresion = window.open(pdfUrl, '_blank');
            if (ventanaImpresion) {
                setTimeout(function() {
                    ventanaImpresion.focus();
                    ventanaImpresion.print();
                }, 800);
            } else {
                alert('Por favor, permita ventanas emergentes para imprimir automáticamente.');
            }
        } else {
            window.open(pdfUrl, '_blank');
        }
        
    } catch (error) {
        console.error('Error al generar PDF del ticket:', error);
        alert('Error al generar PDF: ' + error.message);
    }
};

// Función para generar PDF e imprimir sin previsualización
window.generarPdfTicketDirecto = function (ticketData) {
    return window.generarPdfTicket(ticketData, true);
};

// Función para generar PDF solo para previsualización
window.generarPdfTicketPreview = function (ticketData) {
    return window.generarPdfTicket(ticketData, false);
};

// Función para generar PDF e incrustarlo en iframe (evita bloqueador de pop-ups)
window.generarPdfEnIframe = function (ticketData) {
    try {
        if (typeof jspdf === 'undefined') {
            console.error('jsPDF no está cargado. Reintentando en 1 segundo...');
            setTimeout(() => window.generarPdfEnIframe(ticketData), 1000);
            return;
        }
        
        if (!jspdf.jsPDF) {
            console.error('jsPDF.jsPDF no está disponible');
            alert('Error: jsPDF no está cargado. Por favor recargue la página.');
            return;
        }

        const { jsPDF } = jspdf;
        
        const anchoPuntos = 227; // 80mm
        const alturaPuntos = 500;
        
        const doc = new jsPDF({
            unit: 'pt',
            format: [anchoPuntos, alturaPuntos],
            orientation: 'portrait'
        });
        
        let y = 10;
        const margen = 6;
        const anchoUtil = anchoPuntos - (margen * 2);
        
        function calcularAncho(texto, fontSize) {
            return doc.getTextWidth(texto);
        }
        
        function textoCentrado(texto, fontSize, negrita = false) {
            doc.setFontSize(fontSize);
            doc.setFont('courier', negrita ? 'bold' : 'normal');
            const ancho = calcularAncho(texto, fontSize);
            const x = (anchoPuntos - ancho) / 2;
            doc.text(texto, x, y);
            y += fontSize * 1.2;
        }
        
        function textoIzquierda(texto, fontSize, negrita = false) {
            doc.setFontSize(fontSize);
            doc.setFont('courier', negrita ? 'bold' : 'normal');
            doc.text(texto, margen, y);
            y += fontSize * 1.2;
        }
        
        function lineaDosColumnas(label, valor, fontSize = 8) {
            doc.setFontSize(fontSize);
            doc.setFont('courier', 'bold');
            doc.text(label + ' ', margen, y);
            doc.setFont('courier', 'normal');
            const anchoLabel = calcularAncho(label + ' ', fontSize);
            doc.text(valor || '', margen + anchoLabel, y);
            y += fontSize * 1.2;
        }
        
        function lineaSeparadora() {
            doc.setFontSize(7);
            doc.setFont('courier', 'normal');
            doc.text('-'.repeat(46), margen, y);
            y += 8;
        }
        
        function lineaSeparadoraDoble() {
            doc.setFontSize(7);
            doc.setFont('courier', 'normal');
            doc.text('='.repeat(46), margen, y);
            y += 8;
        }
        
        function dividirTexto(texto, maxAncho) {
            if (!texto) return [''];
            const palabras = texto.split(' ');
            const lineas = [];
            let lineaActual = '';
            
            for (const palabra of palabras) {
                const test = lineaActual ? lineaActual + ' ' + palabra : palabra;
                const ancho = calcularAncho(test, 8);
                
                if (ancho > maxAncho && lineaActual) {
                    lineas.push(lineaActual);
                    lineaActual = palabra;
                } else {
                    lineaActual = test;
                }
            }
            if (lineaActual) lineas.push(lineaActual);
            return lineas.length > 0 ? lineas : [''];
        }
        
        function dividirTextoChars(texto, maxChars) {
            if (!texto) return [''];
            const palabras = texto.split(' ');
            const lineas = [];
            let lineaActual = '';
            
            for (const palabra of palabras) {
                const test = lineaActual ? lineaActual + ' ' + palabra : palabra;
                if (test.length > maxChars && lineaActual) {
                    lineas.push(lineaActual);
                    lineaActual = palabra.length > maxChars ? palabra.substring(0, maxChars) : palabra;
                } else if (test.length > maxChars) {
                    lineas.push(test.substring(0, maxChars));
                    lineaActual = '';
                } else {
                    lineaActual = test;
                }
            }
            if (lineaActual) lineas.push(lineaActual);
            return lineas.length > 0 ? lineas : [''];
        }
        
        // ========== ENCABEZADO ==========
        
        // LOGO (si está habilitado)
        if (ticketData.mostrarLogo && ticketData.logoUrl) {
            try {
                const maxLogoWidth = anchoUtil * 0.75;
                const maxLogoHeight = 50;
                const logoX = (anchoPuntos - maxLogoWidth) / 2;
                doc.addImage(ticketData.logoUrl, 'PNG', logoX, y, maxLogoWidth, maxLogoHeight);
                y += maxLogoHeight + 5;
            } catch (e) {
                console.warn('No se pudo cargar el logo:', e);
            }
        }
        
        // Actividad económica / Rubro de la empresa
        if (ticketData.rubroEmpresa) {
            doc.setFontSize(7);
            const lineasRubro = dividirTexto(ticketData.rubroEmpresa, anchoUtil);
            lineasRubro.forEach(linea => textoCentrado(linea, 7));
        }
        
        // Dirección de la empresa
        if (ticketData.direccion) {
            doc.setFontSize(7);
            const lineasDir = dividirTexto(ticketData.direccion, anchoUtil);
            lineasDir.forEach(linea => textoCentrado(linea, 7));
        }
        
        // RUC y Sucursal en una línea
        doc.setFontSize(8);
        let lineaRucSuc = 'Ruc: ' + ticketData.ruc;
        if (ticketData.sucursal) {
            lineaRucSuc += '       ' + ticketData.sucursal;
        }
        textoCentrado(lineaRucSuc, 8);
        
        y += 6;
        
        // FACTURA (tipo de documento grande y negrita)
        textoCentrado(ticketData.tipoDoc, 14, true);
        
        // Timbrado Nº
        if (ticketData.timbrado) {
            textoCentrado('Timbrado N° ' + ticketData.timbrado, 10, true);
        }
        
        y += 3;
        lineaSeparadoraDoble();
        
        // ========== DATOS DE LA FACTURA ==========
        lineaDosColumnas('Nro:', ticketData.numero, 8);
        lineaDosColumnas('Fecha:', ticketData.fecha, 8);
        
        if (ticketData.vigenciaDel && ticketData.vigenciaAl) {
            lineaDosColumnas('Vigencia:', ticketData.vigenciaDel + ' al ' + ticketData.vigenciaAl, 8);
        }
        
        // ========== DATOS DEL CLIENTE ==========
        if (ticketData.cliente) {
            const lineasCliente = dividirTexto(ticketData.cliente, anchoUtil - 50);
            lineaDosColumnas('Cliente:', lineasCliente[0], 8);
            for (let i = 1; i < lineasCliente.length; i++) {
                doc.setFontSize(8);
                doc.setFont('courier', 'normal');
                doc.text('         ' + lineasCliente[i], margen, y);
                y += 9;
            }
        }
        
        if (ticketData.clienteRuc) {
            lineaDosColumnas('RUC/CI:', ticketData.clienteRuc, 8);
        }
        
        lineaDosColumnas('Condicion:', ticketData.condicion, 8);
        
        y += 2;
        lineaSeparadoraDoble();
        
        // ========== DETALLE DE PRODUCTOS ==========
        lineaSeparadora();
        
        doc.setFontSize(7);
        doc.setFont('courier', 'bold');
        
        const colDesc = margen;
        const colCant = margen + anchoUtil * 0.48;
        const colPrecio = margen + anchoUtil * 0.62;
        const colTotal = margen + anchoUtil * 0.80;
        
        doc.text('DESCRIPCION', colDesc, y);
        doc.text('CANT', colCant, y);
        doc.text('P.UNIT', colPrecio, y);
        doc.text('TOTAL', colTotal, y);
        y += 10;
        
        doc.setFont('courier', 'normal');
        
        ticketData.items.forEach(item => {
            const cantStr = item.cantidad.toString();
            const precioStr = item.precio || '0';
            const totalStr = item.total || '0';
            
            const maxDescChars = 20;
            const lineasDesc = dividirTextoChars(item.descripcion, maxDescChars);
            
            doc.text(lineasDesc[0], colDesc, y);
            doc.text(cantStr, colCant, y);
            doc.text(precioStr, colPrecio, y);
            doc.text(totalStr, colTotal, y);
            y += 9;
            
            for (let i = 1; i < lineasDesc.length; i++) {
                doc.text(lineasDesc[i], colDesc, y);
                y += 9;
            }
        });
        
        lineaSeparadoraDoble();
        
        // ========== DETALLE TOTALES (BASE IMPONIBLE) ==========
        if (ticketData.subtotalGravadas5 || ticketData.subtotalGravadas10 || ticketData.subtotalExentas) {
            lineaDosColumnas('Gravadas 5%:', ticketData.subtotalGravadas5 || '0', 8);
            lineaDosColumnas('Subtotal:', ticketData.subtotal || '0', 8);
        }
        
        // ========== DETALLE DEL IMPUESTO ==========
        if (ticketData.liquidacionIva5 || ticketData.liquidacionIva10) {
            lineaDosColumnas('Liq. IVA 5%:', ticketData.liquidacionIva5 || '0', 8);
            lineaDosColumnas('Total IVA:', ticketData.totalIva || '0', 8);
        }
        
        y += 3;
        lineaSeparadora();
        
        // ========== TOTAL EN RECUADRO ==========
        y += 3;
        doc.setFontSize(11);
        doc.setFont('courier', 'bold');
        const textoTotal = 'TOTAL: ' + ticketData.total;
        const anchoTextoTotal = calcularAncho(textoTotal, 11);
        const xTotal = (anchoPuntos - anchoTextoTotal) / 2;
        
        // Dibujar recuadro
        doc.setDrawColor(0);
        doc.setLineWidth(1);
        doc.rect(xTotal - 8, y - 10, anchoTextoTotal + 16, 16);
        doc.text(textoTotal, xTotal, y);
        y += 20;
        
        // ========== PIE ==========
        y += 8;
        textoCentrado('GRACIAS POR SU COMPRA!', 10, true);
        textoCentrado('SistemIA', 9);
        
        y += 10;
        
        const alturaFinal = Math.max(y + 20, 400);
        doc.internal.pageSize.height = alturaFinal;
        
        // Convertir PDF a Data URL
        const pdfDataUri = doc.output('dataurlstring');
        
        console.log('PDF generado exitosamente');
        
        // Ocultar el overlay de carga
        const overlay = document.getElementById('loadingOverlay');
        if (overlay) {
            overlay.style.display = 'none';
        }
        
        // Mostrar la barra de acciones
        const actionBar = document.getElementById('actionBar');
        if (actionBar) {
            actionBar.style.display = 'block';
        }
        
        // Incrustar PDF en el iframe
        const iframe = document.getElementById('pdfFrame');
        if (iframe) {
            iframe.src = pdfDataUri;
            
            // Esperar a que cargue el PDF y luego imprimir
            iframe.onload = function() {
                console.log('PDF cargado en iframe, esperando renderizado...');
                setTimeout(function() {
                    try {
                        iframe.contentWindow.focus();
                        setTimeout(function() {
                            iframe.contentWindow.print();
                            console.log('Diálogo de impresión abierto');
                        }, 800);
                    } catch (e) {
                        console.error('Error al imprimir desde iframe:', e);
                        window.print();
                    }
                }, 1000);
            };
        } else {
            console.error('No se encontró el iframe pdfFrame');
            alert('Error: No se pudo mostrar el PDF');
        }
        
    } catch (error) {
        console.error('Error al generar PDF del ticket:', error);
        alert('Error al generar PDF: ' + error.message);
    }
};

// Función para generar PDF, imprimir y cerrar ventana automáticamente
window.generarPdfTicketYCerrar = function (ticketData) {
    try {
        if (typeof jspdf === 'undefined') {
            console.error('jsPDF no está cargado. Reintentando en 1 segundo...');
            setTimeout(() => window.generarPdfTicketYCerrar(ticketData), 1000);
            return;
        }
        
        if (!jspdf.jsPDF) {
            console.error('jsPDF.jsPDF no está disponible');
            alert('jsPDF no está cargado correctamente. Por favor, recargue la página.');
            return;
        }

        const { jsPDF } = jspdf;
        
        const anchoPuntos = 227; // 80mm
        const alturaPuntos = 500;
        
        const doc = new jsPDF({
            unit: 'pt',
            format: [anchoPuntos, alturaPuntos],
            orientation: 'portrait'
        });
        
        let y = 10;
        const margen = 6;
        const anchoUtil = anchoPuntos - (margen * 2);
        
        function calcularAncho(texto, fontSize) {
            return doc.getTextWidth(texto);
        }
        
        function textoCentrado(texto, fontSize, negrita = false) {
            doc.setFontSize(fontSize);
            doc.setFont('courier', negrita ? 'bold' : 'normal');
            const ancho = calcularAncho(texto, fontSize);
            const x = (anchoPuntos - ancho) / 2;
            doc.text(texto, x, y);
            y += fontSize * 1.2;
        }
        
        function lineaDosColumnas(label, valor, fontSize = 8) {
            doc.setFontSize(fontSize);
            doc.setFont('courier', 'bold');
            doc.text(label + ' ', margen, y);
            doc.setFont('courier', 'normal');
            const anchoLabel = calcularAncho(label + ' ', fontSize);
            doc.text(valor || '', margen + anchoLabel, y);
            y += fontSize * 1.2;
        }
        
        function lineaSeparadora() {
            doc.setFontSize(7);
            doc.setFont('courier', 'normal');
            doc.text('-'.repeat(46), margen, y);
            y += 8;
        }
        
        function lineaSeparadoraDoble() {
            doc.setFontSize(7);
            doc.setFont('courier', 'normal');
            doc.text('='.repeat(46), margen, y);
            y += 8;
        }
        
        function dividirTexto(texto, maxAncho) {
            if (!texto) return [''];
            const palabras = texto.split(' ');
            const lineas = [];
            let lineaActual = '';
            
            for (const palabra of palabras) {
                const test = lineaActual ? lineaActual + ' ' + palabra : palabra;
                const ancho = calcularAncho(test, 8);
                
                if (ancho > maxAncho && lineaActual) {
                    lineas.push(lineaActual);
                    lineaActual = palabra;
                } else {
                    lineaActual = test;
                }
            }
            if (lineaActual) lineas.push(lineaActual);
            return lineas.length > 0 ? lineas : [''];
        }
        
        function dividirTextoChars(texto, maxChars) {
            if (!texto) return [''];
            const palabras = texto.split(' ');
            const lineas = [];
            let lineaActual = '';
            
            for (const palabra of palabras) {
                const test = lineaActual ? lineaActual + ' ' + palabra : palabra;
                if (test.length > maxChars && lineaActual) {
                    lineas.push(lineaActual);
                    lineaActual = palabra.length > maxChars ? palabra.substring(0, maxChars) : palabra;
                } else if (test.length > maxChars) {
                    lineas.push(test.substring(0, maxChars));
                    lineaActual = '';
                } else {
                    lineaActual = test;
                }
            }
            if (lineaActual) lineas.push(lineaActual);
            return lineas.length > 0 ? lineas : [''];
        }
        
        // ========== ENCABEZADO ==========
        
        if (ticketData.mostrarLogo && ticketData.logoUrl) {
            try {
                const maxLogoWidth = anchoUtil * 0.75;
                const maxLogoHeight = 50;
                const logoX = (anchoPuntos - maxLogoWidth) / 2;
                doc.addImage(ticketData.logoUrl, 'PNG', logoX, y, maxLogoWidth, maxLogoHeight);
                y += maxLogoHeight + 5;
            } catch (e) {
                console.warn('No se pudo cargar el logo:', e);
            }
        }
        
        // Actividad económica / Rubro
        if (ticketData.rubroEmpresa) {
            doc.setFontSize(7);
            const lineasRubro = dividirTexto(ticketData.rubroEmpresa, anchoUtil);
            lineasRubro.forEach(linea => textoCentrado(linea, 7));
        }
        
        // Dirección
        if (ticketData.direccion) {
            doc.setFontSize(7);
            const lineasDir = dividirTexto(ticketData.direccion, anchoUtil);
            lineasDir.forEach(linea => textoCentrado(linea, 7));
        }
        
        // RUC y Sucursal
        doc.setFontSize(8);
        let lineaRucSuc = 'Ruc: ' + ticketData.ruc;
        if (ticketData.sucursal) {
            lineaRucSuc += '       ' + ticketData.sucursal;
        }
        textoCentrado(lineaRucSuc, 8);
        
        y += 6;
        
        // FACTURA
        textoCentrado(ticketData.tipoDoc, 14, true);
        
        // Timbrado
        if (ticketData.timbrado) {
            textoCentrado('Timbrado N° ' + ticketData.timbrado, 10, true);
        }
        
        y += 3;
        lineaSeparadoraDoble();
        
        // ========== DATOS DE LA FACTURA ==========
        lineaDosColumnas('Nro:', ticketData.numero, 8);
        lineaDosColumnas('Fecha:', ticketData.fecha, 8);
        
        if (ticketData.vigenciaDel && ticketData.vigenciaAl) {
            lineaDosColumnas('Vigencia:', ticketData.vigenciaDel + ' al ' + ticketData.vigenciaAl, 8);
        }
        
        // ========== DATOS DEL CLIENTE ==========
        if (ticketData.cliente) {
            const lineasCliente = dividirTexto(ticketData.cliente, anchoUtil - 50);
            lineaDosColumnas('Cliente:', lineasCliente[0], 8);
            for (let i = 1; i < lineasCliente.length; i++) {
                doc.setFontSize(8);
                doc.setFont('courier', 'normal');
                doc.text('         ' + lineasCliente[i], margen, y);
                y += 9;
            }
        }
        
        if (ticketData.clienteRuc) {
            lineaDosColumnas('RUC/CI:', ticketData.clienteRuc, 8);
        }
        
        lineaDosColumnas('Condicion:', ticketData.condicion, 8);
        
        y += 2;
        lineaSeparadoraDoble();
        
        // ========== DETALLE DE PRODUCTOS ==========
        lineaSeparadora();
        
        doc.setFontSize(7);
        doc.setFont('courier', 'bold');
        
        const colDesc = margen;
        const colCant = margen + anchoUtil * 0.48;
        const colPrecio = margen + anchoUtil * 0.62;
        const colTotal = margen + anchoUtil * 0.80;
        
        doc.text('DESCRIPCION', colDesc, y);
        doc.text('CANT', colCant, y);
        doc.text('P.UNIT', colPrecio, y);
        doc.text('TOTAL', colTotal, y);
        y += 10;
        
        doc.setFont('courier', 'normal');
        
        ticketData.items.forEach(item => {
            const cantStr = item.cantidad.toString();
            const precioStr = item.precio || '0';
            const totalStr = item.total || '0';
            
            const maxDescChars = 20;
            const lineasDesc = dividirTextoChars(item.descripcion, maxDescChars);
            
            doc.text(lineasDesc[0], colDesc, y);
            doc.text(cantStr, colCant, y);
            doc.text(precioStr, colPrecio, y);
            doc.text(totalStr, colTotal, y);
            y += 9;
            
            for (let i = 1; i < lineasDesc.length; i++) {
                doc.text(lineasDesc[i], colDesc, y);
                y += 9;
            }
        });
        
        lineaSeparadoraDoble();
        
        // ========== TOTALES ==========
        if (ticketData.subtotalGravadas5 || ticketData.subtotalGravadas10 || ticketData.subtotalExentas) {
            lineaDosColumnas('Gravadas 5%:', ticketData.subtotalGravadas5 || '0', 8);
            lineaDosColumnas('Subtotal:', ticketData.subtotal || '0', 8);
        }
        
        if (ticketData.liquidacionIva5 || ticketData.liquidacionIva10) {
            lineaDosColumnas('Liq. IVA 5%:', ticketData.liquidacionIva5 || '0', 8);
            lineaDosColumnas('Total IVA:', ticketData.totalIva || '0', 8);
        }
        
        y += 3;
        lineaSeparadora();
        
        // ========== TOTAL EN RECUADRO ==========
        y += 3;
        doc.setFontSize(11);
        doc.setFont('courier', 'bold');
        const textoTotal = 'TOTAL: ' + ticketData.total;
        const anchoTextoTotal = calcularAncho(textoTotal, 11);
        const xTotal = (anchoPuntos - anchoTextoTotal) / 2;
        
        doc.setDrawColor(0);
        doc.setLineWidth(1);
        doc.rect(xTotal - 8, y - 10, anchoTextoTotal + 16, 16);
        doc.text(textoTotal, xTotal, y);
        y += 20;
        
        // ========== PIE ==========
        y += 8;
        textoCentrado('GRACIAS POR SU COMPRA!', 10, true);
        textoCentrado('SistemIA', 9);
        
        y += 10;
        
        const alturaFinal = Math.max(y + 20, 400);
        doc.internal.pageSize.height = alturaFinal;
        
        // Convertir PDF a Data URL
        const pdfDataUri = doc.output('dataurlstring');
        
        console.log('PDF generado exitosamente, abriendo en nueva ventana...');
        
        const ventanaImpresion = window.open('', '_blank', 'width=800,height=900,toolbar=0,menubar=0,location=0');
        
        if (ventanaImpresion) {
            ventanaImpresion.document.write(`
                <!DOCTYPE html>
                <html>
                <head>
                    <title>Ticket - Impresión</title>
                    <style>
                        body { margin: 0; padding: 0; }
                        iframe { border: none; width: 100%; height: 100vh; }
                    </style>
                </head>
                <body>
                    <iframe src="${pdfDataUri}" type="application/pdf"></iframe>
                </body>
                </html>
            `);
            ventanaImpresion.document.close();
            
            setTimeout(function() {
                try {
                    ventanaImpresion.focus();
                    ventanaImpresion.print();
                    console.log('Diálogo de impresión abierto exitosamente');
                } catch (e) {
                    console.error('Error al ejecutar print():', e);
                }
            }, 1500);
            
            setTimeout(function() {
                if (window.opener) {
                    window.close();
                }
            }, 2500);
            
            console.log('PDF abierto exitosamente');
        } else {
            console.error('No se pudo abrir ventana - Bloqueador de pop-ups activo');
            alert('Por favor, permita ventanas emergentes para imprimir automáticamente.');
        }
        
    } catch (error) {
        console.error('Error al generar PDF del ticket:', error);
        alert('Error al generar PDF: ' + error.message);
    }
};

// =====================================================
// FUNCIONES DE DESCARGA DE ARCHIVOS
// =====================================================

// Descargar archivo desde bytes en Base64
window.downloadFileFromBytes = function (fileName, base64Content, contentType) {
    try {
        // Convertir Base64 a Blob
        const byteCharacters = atob(base64Content);
        const byteNumbers = new Array(byteCharacters.length);
        for (let i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
        }
        const byteArray = new Uint8Array(byteNumbers);
        const blob = new Blob([byteArray], { type: contentType || 'application/octet-stream' });
        
        // Crear link de descarga
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        
        // Disparar descarga
        document.body.appendChild(link);
        link.click();
        
        // Limpiar
        setTimeout(function() {
            document.body.removeChild(link);
            window.URL.revokeObjectURL(url);
        }, 100);
        
        console.log('Archivo descargado:', fileName);
    } catch (error) {
        console.error('Error al descargar archivo:', error);
        alert('Error al descargar archivo: ' + error.message);
    }
};

// Descargar archivo de texto directamente
window.downloadTextFile = function (fileName, content, contentType) {
    try {
        const blob = new Blob([content], { type: contentType || 'text/plain' });
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        
        document.body.appendChild(link);
        link.click();
        
        setTimeout(function() {
            document.body.removeChild(link);
            window.URL.revokeObjectURL(url);
        }, 100);
        
        console.log('Archivo de texto descargado:', fileName);
    } catch (error) {
        console.error('Error al descargar archivo:', error);
    }
};

// =================== FUNCIONES PARA AUDITORÍA ===================
// Descargar archivo desde base64
window.descargarArchivoBase64 = (fileName, base64, contentType) => {
    try {
        const byteCharacters = atob(base64);
        const byteNumbers = new Array(byteCharacters.length);
        for (let i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
        }
        const byteArray = new Uint8Array(byteNumbers);
        const blob = new Blob([byteArray], { type: contentType });
        
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        link.style.display = 'none';
        
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        
        URL.revokeObjectURL(url);
        console.log('Archivo descargado:', fileName);
    } catch (error) {
        console.error('Error al descargar archivo:', error);
    }
};

// Imprimir contenido HTML
window.imprimirHtml = (htmlContent) => {
    const printWindow = window.open('', '_blank');
    if (printWindow) {
        printWindow.document.write(htmlContent);
        printWindow.document.close();
        printWindow.focus();
        setTimeout(() => {
            printWindow.print();
            printWindow.close();
        }, 250);
    } else {
        alert('Por favor habilite las ventanas emergentes para imprimir.');
    }
};

// =================== TOAST AUTO-CIERRE CON BARRA DE PROGRESO ===================
window.mostrarToastProgreso = function (mensaje, duracionMs, tipo) {
    duracionMs = duracionMs || 3000;
    tipo = tipo || 'info'; // 'info', 'success', 'error'

    // Remover toast anterior si existe
    var prev = document.getElementById('toast-progreso-overlay');
    if (prev) prev.remove();

    var iconos = { info: 'ℹ️', success: '✅', error: '❌' };
    var colores = { info: '#0d6efd', success: '#198754', error: '#dc3545' };
    var icono = iconos[tipo] || iconos.info;
    var color = colores[tipo] || colores.info;

    // Crear overlay
    var overlay = document.createElement('div');
    overlay.id = 'toast-progreso-overlay';
    overlay.style.cssText = 'position:fixed;top:0;left:0;width:100%;height:100%;z-index:99999;display:flex;align-items:center;justify-content:center;background:rgba(0,0,0,0.3);';

    overlay.innerHTML =
        '<div style="background:var(--bg-surface,#fff);border-radius:12px;padding:24px 32px;max-width:420px;width:90%;box-shadow:0 8px 32px rgba(0,0,0,0.25);text-align:center;">' +
            '<div style="font-size:2.5rem;margin-bottom:12px;">' + icono + '</div>' +
            '<div style="font-size:1rem;color:var(--text-primary,#333);margin-bottom:18px;line-height:1.5;white-space:pre-line;">' + mensaje + '</div>' +
            '<div style="background:#e9ecef;border-radius:6px;height:6px;overflow:hidden;">' +
                '<div id="toast-progreso-barra" style="height:100%;width:100%;background:' + color + ';border-radius:6px;transition:width ' + duracionMs + 'ms linear;"></div>' +
            '</div>' +
            '<div style="font-size:0.8rem;color:var(--text-muted,#888);margin-top:8px;">Cerrando automáticamente...</div>' +
        '</div>';

    document.body.appendChild(overlay);

    // Iniciar animación de barra (de 100% a 0%)
    requestAnimationFrame(function () {
        var barra = document.getElementById('toast-progreso-barra');
        if (barra) barra.style.width = '0%';
    });

    // Auto-cerrar después de la duración
    setTimeout(function () {
        var el = document.getElementById('toast-progreso-overlay');
        if (el) {
            el.style.transition = 'opacity 0.3s';
            el.style.opacity = '0';
            setTimeout(function () { if (el.parentNode) el.parentNode.removeChild(el); }, 300);
        }
    }, duracionMs);
};

// ========== SUSCRIPCIONES - Listener PostMessage desde iframe ==========
window.registrarSuscripcionesListener = function (dotNetRef) {
    // Remover listener anterior si existe
    if (window._suscripcionesMessageHandler) {
        window.removeEventListener('message', window._suscripcionesMessageHandler);
    }
    window._suscripcionesDotNetRef = dotNetRef;
    window._suscripcionesMessageHandler = function (e) {
        if (e.data && e.data.tipo === 'ventaSuscripcionCreada' && e.data.idVenta) {
            console.log('[Suscripciones] PostMessage recibido, idVenta:', e.data.idVenta);
            if (window._suscripcionesDotNetRef) {
                window._suscripcionesDotNetRef.invokeMethodAsync('ManejarVentaDesdeJS', e.data.idVenta)
                    .then(function () { console.log('[Suscripciones] Modal cerrado exitosamente'); })
                    .catch(function (err) { console.error('[Suscripciones] Error:', err); });
            }
        }
    };
    window.addEventListener('message', window._suscripcionesMessageHandler);
    console.log('[Suscripciones] Listener postMessage registrado (instancia)');
};
