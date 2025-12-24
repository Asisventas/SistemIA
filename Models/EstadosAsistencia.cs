using System;

namespace SistemIA.Models
{
    /// <summary>
    /// Estados posibles de puntualidad para el control de asistencia
    /// </summary>
    public enum EstadoPuntualidad
    {
        /// <summary>
        /// Llegó dentro del horario establecido y tolerancia
        /// </summary>
        Puntual = 1,

        /// <summary>
        /// Llegó después del horario + tolerancia
        /// </summary>
        Tardanza = 2,

        /// <summary>
        /// Llegó antes del horario establecido
        /// </summary>
        Adelanto = 3,

        /// <summary>
        /// No se presentó en el horario establecido
        /// </summary>
        Ausencia = 4,

        /// <summary>
        /// Salió antes del horario establecido
        /// </summary>
        SalidaTemprana = 5,

        /// <summary>
        /// Trabajó tiempo extra
        /// </summary>
        TiempoExtra = 6,

        /// <summary>
        /// Día no laborable
        /// </summary>
        NoLaborable = 7
    }

    /// <summary>
    /// Estados de justificación para registros que requieren explicación
    /// </summary>
    public enum EstadoJustificacion
    {
        /// <summary>
        /// No requiere justificación
        /// </summary>
        NoRequiere = 0,

        /// <summary>
        /// Esperando justificación del empleado
        /// </summary>
        Pendiente = 1,

        /// <summary>
        /// Justificación aprobada por supervisor
        /// </summary>
        Aprobada = 2,

        /// <summary>
        /// Justificación rechazada
        /// </summary>
        Rechazada = 3,

        /// <summary>
        /// En revisión por recursos humanos
        /// </summary>
        EnRevision = 4
    }

    /// <summary>
    /// Métodos de registro de asistencia disponibles
    /// </summary>
    public enum MetodoRegistro
    {
        /// <summary>
        /// Reconocimiento facial
        /// </summary>
        Facial = 1,

        /// <summary>
        /// Entrada manual por supervisor
        /// </summary>
        Manual = 2,

        /// <summary>
        /// Tarjeta de proximidad o magnética
        /// </summary>
        Tarjeta = 3,

        /// <summary>
        /// Código QR
        /// </summary>
        QR = 4,

        /// <summary>
        /// Huella digital
        /// </summary>
        Huella = 5,

        /// <summary>
        /// Aplicación móvil
        /// </summary>
        Movil = 6,

        /// <summary>
        /// Portal web
        /// </summary>
        Web = 7,

        /// <summary>
        /// Sistema automático
        /// </summary>
        Automatico = 8
    }
}
