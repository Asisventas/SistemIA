window.initFaceDetection = async function() {
    if (!window.faceapi) {
        const script = document.createElement('script');
        script.src = '/face-api.min.js';
        await new Promise((resolve, reject) => {
            script.onload = resolve;
            script.onerror = reject;
            document.head.appendChild(script);
        });
    }

    await Promise.all([
        window.faceapi.nets.faceRecognitionNet.loadFromUri('/face_recognition_models'),
        window.faceapi.nets.faceLandmark68Net.loadFromUri('/face_recognition_models'),
        window.faceapi.nets.ssdMobilenetv1.loadFromUri('/face_recognition_models')
    ]).catch(err => console.error('Error loading models:', err));
};
