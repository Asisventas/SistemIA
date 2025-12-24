// Función para hablar usando la API de síntesis de voz
export function speak(text) {
    return new Promise((resolve) => {
        // Crear una nueva instancia de SpeechSynthesisUtterance
        const utterance = new SpeechSynthesisUtterance(text);
        
        // Configurar el idioma en español
        utterance.lang = 'es-ES';
        
        // Ajustar la velocidad del habla
        utterance.rate = 1.0;
        
        // Ajustar el tono
        utterance.pitch = 1.0;
        
        // Obtener todas las voces disponibles
        const voices = window.speechSynthesis.getVoices();
        
        // Intentar encontrar una voz en español
        const spanishVoice = voices.find(voice => voice.lang.startsWith('es'));
        if (spanishVoice) {
            utterance.voice = spanishVoice;
        }
        
        // Evento cuando termina de hablar
        utterance.onend = () => {
            resolve();
        };
        
        // Comenzar a hablar
        window.speechSynthesis.speak(utterance);
    });
}
