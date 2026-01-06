// ========== SOPORTE - CAPTURA DE PANTALLA Y VIDEO ==========
// Este módulo maneja la captura de screenshots y grabación de video para soporte

window.soporteCaptura = {
    mediaRecorder: null,
    recordedChunks: [],
    stream: null,

    // Capturar pantalla usando html2canvas
    capturarPantalla: async function () {
        try {
            // Verificar si html2canvas está disponible
            if (typeof html2canvas === 'undefined') {
                // Cargar html2canvas dinámicamente
                await this.cargarHtml2Canvas();
            }

            // Capturar toda la ventana visible
            const canvas = await html2canvas(document.body, {
                logging: false,
                useCORS: true,
                allowTaint: true,
                backgroundColor: null,
                scale: 0.75, // Reducir tamaño para no generar archivos muy grandes
                ignoreElements: (element) => {
                    // Ignorar el panel de chat para evitar recursividad
                    return element.classList.contains('chat-panel');
                }
            });

            // Convertir a base64
            const dataUrl = canvas.toDataURL('image/png', 0.8);
            return dataUrl;
        } catch (error) {
            console.error('Error al capturar pantalla:', error);
            return null;
        }
    },

    // Cargar html2canvas desde CDN si no está disponible
    cargarHtml2Canvas: function () {
        return new Promise((resolve, reject) => {
            const script = document.createElement('script');
            script.src = 'https://cdnjs.cloudflare.com/ajax/libs/html2canvas/1.4.1/html2canvas.min.js';
            script.onload = resolve;
            script.onerror = reject;
            document.head.appendChild(script);
        });
    },

    // Iniciar grabación de video de pantalla
    iniciarGrabacion: async function () {
        try {
            // Solicitar permisos de pantalla
            this.stream = await navigator.mediaDevices.getDisplayMedia({
                video: {
                    cursor: 'always',
                    displaySurface: 'browser'
                },
                audio: false
            });

            this.recordedChunks = [];

            // Configurar el MediaRecorder
            const options = {
                mimeType: 'video/webm;codecs=vp9'
            };

            // Fallback si vp9 no está soportado
            if (!MediaRecorder.isTypeSupported(options.mimeType)) {
                options.mimeType = 'video/webm;codecs=vp8';
            }
            if (!MediaRecorder.isTypeSupported(options.mimeType)) {
                options.mimeType = 'video/webm';
            }

            this.mediaRecorder = new MediaRecorder(this.stream, options);

            this.mediaRecorder.ondataavailable = (event) => {
                if (event.data.size > 0) {
                    this.recordedChunks.push(event.data);
                }
            };

            this.mediaRecorder.onstop = () => {
                // Detener todos los tracks cuando la grabación termina
                if (this.stream) {
                    this.stream.getTracks().forEach(track => track.stop());
                }
            };

            // Escuchar cuando el usuario cierra el compartir pantalla
            this.stream.getVideoTracks()[0].onended = () => {
                if (this.mediaRecorder && this.mediaRecorder.state !== 'inactive') {
                    this.mediaRecorder.stop();
                }
            };

            this.mediaRecorder.start(100); // Grabar en chunks de 100ms
            return true;
        } catch (error) {
            console.error('Error al iniciar grabación:', error);
            return false;
        }
    },

    // Detener grabación y obtener video en base64
    detenerGrabacion: async function () {
        return new Promise((resolve) => {
            if (!this.mediaRecorder || this.mediaRecorder.state === 'inactive') {
                resolve(null);
                return;
            }

            this.mediaRecorder.onstop = async () => {
                // Detener tracks
                if (this.stream) {
                    this.stream.getTracks().forEach(track => track.stop());
                }

                // Crear blob
                const blob = new Blob(this.recordedChunks, { type: 'video/webm' });

                // Convertir a base64
                const reader = new FileReader();
                reader.onloadend = () => {
                    resolve(reader.result);
                };
                reader.onerror = () => {
                    resolve(null);
                };
                reader.readAsDataURL(blob);

                this.recordedChunks = [];
            };

            this.mediaRecorder.stop();
        });
    },

    // Cancelar grabación sin guardar
    cancelarGrabacion: function () {
        if (this.mediaRecorder && this.mediaRecorder.state !== 'inactive') {
            this.mediaRecorder.stop();
        }
        if (this.stream) {
            this.stream.getTracks().forEach(track => track.stop());
        }
        this.recordedChunks = [];
    },

    // Obtener información del navegador
    obtenerInfoNavegador: function () {
        const info = {
            userAgent: navigator.userAgent,
            idioma: navigator.language,
            plataforma: navigator.platform,
            pantalla: `${window.screen.width}x${window.screen.height}`,
            ventana: `${window.innerWidth}x${window.innerHeight}`,
            colorDepth: window.screen.colorDepth,
            pixelRatio: window.devicePixelRatio,
            cookiesHabilitadas: navigator.cookieEnabled,
            conexion: navigator.onLine ? 'Online' : 'Offline',
            horaLocal: new Date().toLocaleString()
        };

        // Intentar detectar el navegador
        const ua = navigator.userAgent;
        let navegador = 'Desconocido';
        if (ua.includes('Firefox')) navegador = 'Firefox';
        else if (ua.includes('Edg')) navegador = 'Edge';
        else if (ua.includes('Chrome')) navegador = 'Chrome';
        else if (ua.includes('Safari')) navegador = 'Safari';
        else if (ua.includes('Opera') || ua.includes('OPR')) navegador = 'Opera';

        info.navegadorDetectado = navegador;

        return JSON.stringify(info, null, 2);
    }
};
