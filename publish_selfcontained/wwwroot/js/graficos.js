// graficos.js - Funciones para Chart.js
console.log('📊 Graficos.js cargado');

// Verificar que Chart.js esté disponible (silencioso si no está)
if (typeof Chart === 'undefined') {
    console.debug('Chart.js no está disponible aún');
} else {
    console.debug('Chart.js disponible - versión:', Chart.version);
}

// Función principal para crear gráfico de tipos de cambio
window.crearGraficoTiposCambio = function(canvasId, datos, opciones) {
    try {
        console.log('=== INICIANDO CREACIÓN DE GRÁFICO ===');
        console.log('📋 Canvas ID:', canvasId);
        console.log('📊 Datos recibidos:', datos);
        console.log('⚙️ Opciones recibidas:', opciones);
        
        const canvas = document.getElementById(canvasId);
        if (!canvas) {
            console.debug('Canvas no encontrado, omitiendo creación:', canvasId);
            return false;
        }
        console.log('✅ Canvas encontrado:', canvas);

        // Destruir gráfico anterior si existe
        const chartKey = canvasId + '_chart';
        if (window[chartKey]) {
            console.log('🗑️ Destruyendo gráfico anterior');
            window[chartKey].destroy();
            window[chartKey] = null;
        }

        // Verificar que Chart.js esté disponible
        if (typeof Chart === 'undefined') {
            console.debug('Chart.js no está disponible');
            return false;
        }
        console.debug('Chart.js confirmado');

        // Configuración del gráfico
        const config = {
            type: 'line',
            data: datos,
            options: opciones || {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: {
                        beginAtZero: false
                    }
                }
            }
        };

    console.debug('Configuración preparada');

        // Crear el gráfico
        const chart = new Chart(canvas, config);
        window[chartKey] = chart;
        
    console.debug('Gráfico creado exitosamente');
        return true;
        
    } catch (error) {
        console.error('Error creando gráfico:', error);
        return false;
    }
};

// Función para limpiar el gráfico principal
window.limpiarGraficoPrincipal = function() {
    try {
        console.debug('Limpiando gráfico principal...');
        
        if (window.graficoTiposCambio_chart) {
            window.graficoTiposCambio_chart.destroy();
            window.graficoTiposCambio_chart = null;
            console.debug('Gráfico principal limpiado');
            return true;
        } else {
            console.debug('No hay gráfico principal para limpiar');
            return true;
        }
    } catch (error) {
        console.error('Error limpiando gráfico principal:', error);
        return false;
    }
};

// Función específica para el gráfico principal de tipos de cambio
window.crearGraficoPrincipal = function(datasetsJson, opcionesJson) {
    try {
        console.log('=== CREANDO GRÁFICO PRINCIPAL ===');
        console.log('📊 Datasets JSON recibidos:', typeof datasetsJson, datasetsJson?.substring(0, 100) + '...');
        console.log('⚙️ Opciones JSON recibidas:', typeof opcionesJson, opcionesJson?.substring(0, 100) + '...');
        
    // Buscar el canvas (sin reintentos ruidosos)
    const buscarYCrear = () => {
            const canvas = document.getElementById('graficoTiposCambio');
            if (!canvas) {
        console.debug('Canvas graficoTiposCambio no encontrado; se omite creación');
        return false;
            }
        console.debug('Canvas encontrado:', canvas);

            // Destruir gráfico anterior si existe
            if (window.graficoTiposCambio_chart) {
                console.debug('Destruyendo gráfico anterior');
                window.graficoTiposCambio_chart.destroy();
                window.graficoTiposCambio_chart = null;
            }

            // Verificar que Chart.js esté disponible
            if (typeof Chart === 'undefined') {
                console.debug('Chart.js no está disponible');
                return false;
            }
            console.debug('Chart.js confirmado versión:', Chart.version || 'desconocida');

            // Parsear los datos si vienen como string
            let datasets = datasetsJson;
            let opciones = opcionesJson;
            
            try {
                if (typeof datasetsJson === 'string') {
                    console.debug('Parseando datasets JSON...');
                    datasets = JSON.parse(datasetsJson);
                    console.debug('Datasets parseados:', datasets.length, 'conjuntos');
                }
                if (typeof opcionesJson === 'string') {
                    console.debug('Parseando opciones JSON...');
                    opciones = JSON.parse(opcionesJson);
                    console.debug('Opciones parseadas');
                }
            } catch (parseError) {
                console.error('Error parseando JSON:', parseError);
                return false;
            }

            console.debug('Datasets procesados:', datasets?.length, 'conjuntos');
            console.debug('Opciones procesadas:', opciones);

            // Configuración del gráfico
            const config = {
                type: 'line',
                data: { datasets: datasets },
                options: opciones || {
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: {
                        y: {
                            beginAtZero: false
                        }
                    }
                }
            };

            console.debug('Configuración final preparada');
            
            // Crear el gráfico
            console.debug('Creando instancia de Chart...');
            const chart = new Chart(canvas, config);
            window.graficoTiposCambio_chart = chart;
            
            console.debug('Gráfico principal creado exitosamente');
            return true;
        };

        // Intentar inmediatamente sin reintentos
        return buscarYCrear();
        
    } catch (error) {
    console.error('Error creando gráfico principal:', error);
        return false;
    }
};

// Función de prueba simple
window.testChart = function() {
    console.log('🧪 Ejecutando prueba de Chart.js...');
    
    const testData = {
        labels: ['Lun', 'Mar', 'Mié', 'Jue', 'Vie'],
        datasets: [{
            label: 'Prueba USD/PYG',
            data: [7200, 7300, 7250, 7400, 7350],
            borderColor: 'rgb(75, 192, 192)',
            backgroundColor: 'rgba(75, 192, 192, 0.2)',
            tension: 0.3
        }]
    };
    
    const testOptions = {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            title: {
                display: true,
                text: 'Gráfico de Prueba'
            }
        },
        scales: {
            y: {
                beginAtZero: false
            }
        }
    };
    
    return window.crearGraficoTiposCambio('basicTestChart', testData, testOptions);
};

// Función para crear gráfico en simpleChart específicamente
window.crearGraficoSimple = function() {
    try {
        const canvas = document.getElementById('simpleChart');
        if (!canvas) {
            console.debug('Canvas simpleChart no encontrado; se omite');
            return false;
        }

        // Limpiar gráfico anterior
        if (window.simpleChartInstance) {
            window.simpleChartInstance.destroy();
            window.simpleChartInstance = null;
        }

        if (typeof Chart === 'undefined') {
            console.debug('Chart.js no está disponible');
            return false;
        }

        // Datos de prueba
        const data = {
            labels: ['Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb', 'Dom'],
            datasets: [{
                label: 'USD/PYG Compra',
                data: [7250, 7280, 7260, 7300, 7290, 7310, 7295],
                borderColor: '#0066cc',
                backgroundColor: 'rgba(0, 102, 204, 0.1)',
                tension: 0.3,
                fill: true
            }, {
                label: 'USD/PYG Venta',
                data: [7300, 7330, 7310, 7350, 7340, 7360, 7345],
                borderColor: '#ff6600',
                backgroundColor: 'rgba(255, 102, 0, 0.1)',
                tension: 0.3,
                fill: true
            }]
        };

        const config = {
            type: 'line',
            data: data,
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    title: {
                        display: true,
                        text: 'Tipos de Cambio - Prueba',
                        font: { size: 16 }
                    },
                    legend: {
                        display: true,
                        position: 'top'
                    }
                },
                scales: {
                    y: {
                        beginAtZero: false,
                        title: {
                            display: true,
                            text: 'Guaraníes (Gs.)'
                        }
                    },
                    x: {
                        title: {
                            display: true,
                            text: 'Días'
                        }
                    }
                },
                interaction: {
                    intersect: false,
                    mode: 'index'
                }
            }
        };

        // Crear gráfico
        window.simpleChartInstance = new Chart(canvas, config);
    console.debug('Gráfico simple creado exitosamente');
        return true;

    } catch (error) {
    console.error('Error creando gráfico simple:', error);
        return false;
    }
};

// Función para limpiar gráfico simple
window.limpiarGraficoSimple = function() {
    try {
        if (window.simpleChartInstance) {
            window.simpleChartInstance.destroy();
            window.simpleChartInstance = null;
            console.log('🗑️ Gráfico simple eliminado');
            return true;
        }
        return false;
    } catch (error) {
        console.error('❌ Error eliminando gráfico simple:', error);
        return false;
    }
};

// Función de verificación de librerías
window.verificarLibrerias = function() {
    try {
        let info = [];
        if (typeof Chart !== 'undefined') {
            info.push('✅ Chart.js v' + Chart.version);
        } else {
            info.push('❌ Chart.js no encontrado');
        }
        
        if (typeof Chart !== 'undefined' && Chart.adapters && Chart.adapters._date) {
            info.push('✅ Adaptador de fecha disponible');
        } else {
            info.push('⚠️ Adaptador de fecha no disponible');
        }
        
        return info.join(' | ');
    } catch (e) {
        return '❌ Error: ' + e.message;
    }
};

console.debug('Graficos.js - funciones cargadas');