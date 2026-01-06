// Chat Voz - Funciones de reconocimiento y síntesis de voz para el Asistente IA
// Usa Web Speech API (nativo del navegador, sin dependencias externas)

window.chatVoz = {
    recognition: null,
    synthesis: window.speechSynthesis,
    dotNetRef: null,

    // Verificar si la API de voz está disponible
    verificarDisponibilidad: function() {
        const tieneReconocimiento = 'webkitSpeechRecognition' in window || 'SpeechRecognition' in window;
        const tieneSintesis = 'speechSynthesis' in window;
        return tieneReconocimiento && tieneSintesis;
    },

    // Iniciar reconocimiento de voz (Speech-to-Text)
    iniciarReconocimiento: function(dotNetRef) {
        this.dotNetRef = dotNetRef;
        
        const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
        if (!SpeechRecognition) {
            dotNetRef.invokeMethodAsync('ErrorVoz', 'Reconocimiento de voz no disponible');
            return;
        }

        this.recognition = new SpeechRecognition();
        this.recognition.lang = 'es-PY'; // Español de Paraguay
        this.recognition.continuous = false;
        this.recognition.interimResults = false;
        this.recognition.maxAlternatives = 1;

        this.recognition.onresult = (event) => {
            const texto = event.results[0][0].transcript;
            console.log('Texto reconocido:', texto);
            dotNetRef.invokeMethodAsync('RecibirTextoVoz', texto);
        };

        this.recognition.onerror = (event) => {
            console.error('Error de reconocimiento:', event.error);
            let mensaje = 'Error de reconocimiento';
            switch(event.error) {
                case 'no-speech':
                    mensaje = 'No se detectó voz';
                    break;
                case 'audio-capture':
                    mensaje = 'No se puede acceder al micrófono';
                    break;
                case 'not-allowed':
                    mensaje = 'Permiso de micrófono denegado';
                    break;
            }
            dotNetRef.invokeMethodAsync('ErrorVoz', mensaje);
        };

        this.recognition.onend = () => {
            // Si terminó sin resultado, notificar
            if (this.dotNetRef) {
                // El resultado ya fue manejado en onresult si hubo éxito
            }
        };

        try {
            this.recognition.start();
            console.log('Reconocimiento de voz iniciado');
        } catch (e) {
            console.error('Error al iniciar reconocimiento:', e);
            dotNetRef.invokeMethodAsync('ErrorVoz', 'Error al iniciar reconocimiento');
        }
    },

    // Detener reconocimiento de voz
    detenerReconocimiento: function() {
        if (this.recognition) {
            try {
                this.recognition.stop();
            } catch (e) {
                console.log('Reconocimiento ya detenido');
            }
            this.recognition = null;
        }
    },

    // Síntesis de voz (Text-to-Speech)
    hablar: function(texto) {
        if (!this.synthesis) return;

        // Cancelar cualquier habla anterior
        this.synthesis.cancel();

        const utterance = new SpeechSynthesisUtterance(texto);
        utterance.lang = 'es-ES'; // Español
        utterance.rate = 1.0; // Velocidad normal
        utterance.pitch = 1.0; // Tono normal
        utterance.volume = 1.0; // Volumen máximo

        // Buscar una voz en español si está disponible
        const voces = this.synthesis.getVoices();
        const vozEspanol = voces.find(v => v.lang.startsWith('es-')) || voces[0];
        if (vozEspanol) {
            utterance.voice = vozEspanol;
        }

        this.synthesis.speak(utterance);
    },

    // Detener síntesis de voz
    detenerHabla: function() {
        if (this.synthesis) {
            this.synthesis.cancel();
        }
    },

    // Scroll al final del contenedor de mensajes
    scrollAlFinal: function(elemento) {
        if (elemento) {
            elemento.scrollTop = elemento.scrollHeight;
        }
    }
};

// Precargar voces cuando estén disponibles
if (window.speechSynthesis) {
    window.speechSynthesis.onvoiceschanged = function() {
        window.speechSynthesis.getVoices();
    };
}
