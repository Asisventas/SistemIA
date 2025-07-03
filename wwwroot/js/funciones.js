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
