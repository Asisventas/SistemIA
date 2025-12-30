// Escáner de códigos de barras con cámara usando html5-qrcode
window.BarcodeScanner = {
    scanner: null,
    dotNetRef: null,
    isScanning: false,

    // Inicializa el escáner
    init: async function (elementId, dotNetReference) {
        try {
            this.dotNetRef = dotNetReference;
            
            if (typeof Html5Qrcode === 'undefined') {
                console.error('Html5Qrcode library not loaded');
                return { success: false, error: 'Librería de escaneo no cargada' };
            }

            this.scanner = new Html5Qrcode(elementId, {
                verbose: false,
                formatsToSupport: [
                    Html5QrcodeSupportedFormats.EAN_13,
                    Html5QrcodeSupportedFormats.EAN_8,
                    Html5QrcodeSupportedFormats.UPC_A,
                    Html5QrcodeSupportedFormats.UPC_E,
                    Html5QrcodeSupportedFormats.CODE_128,
                    Html5QrcodeSupportedFormats.CODE_39,
                    Html5QrcodeSupportedFormats.CODE_93,
                    Html5QrcodeSupportedFormats.ITF,
                    Html5QrcodeSupportedFormats.QR_CODE
                ]
            });

            return { success: true };
        } catch (err) {
            console.error('Error initializing scanner:', err);
            return { success: false, error: err.message };
        }
    },

    // Obtiene las cámaras disponibles
    getCameras: async function () {
        try {
            const cameras = await Html5Qrcode.getCameras();
            return cameras.map(c => ({ id: c.id, label: c.label }));
        } catch (err) {
            console.error('Error getting cameras:', err);
            return [];
        }
    },

    // Inicia el escaneo
    start: async function (cameraId, preferBackCamera) {
        try {
            if (!this.scanner) {
                return { success: false, error: 'Escáner no inicializado' };
            }

            if (this.isScanning) {
                await this.stop();
            }

            const config = {
                fps: 10,
                qrbox: { width: 280, height: 150 },
                aspectRatio: 1.5,
                disableFlip: false
            };

            // Callback cuando se detecta un código
            const onScanSuccess = async (decodedText, decodedResult) => {
                // Reproducir sonido de beep
                this.playBeep();
                
                // Enviar el código a Blazor
                if (this.dotNetRef) {
                    await this.dotNetRef.invokeMethodAsync('OnBarcodeScanned', decodedText);
                }
            };

            const onScanFailure = (error) => {
                // Silenciar errores de escaneo continuo (son normales)
            };

            // Determinar qué cámara usar
            let cameraConfig;
            if (cameraId) {
                cameraConfig = cameraId;
            } else if (preferBackCamera) {
                cameraConfig = { facingMode: "environment" };
            } else {
                cameraConfig = { facingMode: "user" };
            }

            await this.scanner.start(cameraConfig, config, onScanSuccess, onScanFailure);
            this.isScanning = true;

            return { success: true };
        } catch (err) {
            console.error('Error starting scanner:', err);
            return { success: false, error: err.message };
        }
    },

    // Detiene el escaneo
    stop: async function () {
        try {
            if (this.scanner && this.isScanning) {
                await this.scanner.stop();
                this.isScanning = false;
            }
            return { success: true };
        } catch (err) {
            console.error('Error stopping scanner:', err);
            return { success: false, error: err.message };
        }
    },

    // Destruye el escáner
    dispose: async function () {
        try {
            await this.stop();
            if (this.scanner) {
                this.scanner = null;
            }
            this.dotNetRef = null;
            return { success: true };
        } catch (err) {
            console.error('Error disposing scanner:', err);
            return { success: false, error: err.message };
        }
    },

    // Reproduce un sonido de beep
    playBeep: function () {
        try {
            const audioContext = new (window.AudioContext || window.webkitAudioContext)();
            const oscillator = audioContext.createOscillator();
            const gainNode = audioContext.createGain();

            oscillator.connect(gainNode);
            gainNode.connect(audioContext.destination);

            oscillator.frequency.value = 1200;
            oscillator.type = 'sine';
            gainNode.gain.setValueAtTime(0.3, audioContext.currentTime);
            gainNode.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 0.15);

            oscillator.start(audioContext.currentTime);
            oscillator.stop(audioContext.currentTime + 0.15);
        } catch (e) {
            // Silenciar errores de audio
        }
    },

    // Verifica si la librería está disponible
    isAvailable: function () {
        return typeof Html5Qrcode !== 'undefined';
    }
};
