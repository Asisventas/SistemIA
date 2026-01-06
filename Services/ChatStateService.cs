using System.Text.Json;

namespace SistemIA.Services;

/// <summary>
/// Servicio singleton para mantener el estado del chat entre navegaciones
/// </summary>
public class ChatStateService
{
    private List<MensajeChatState> _mensajes = new();
    private bool _estaAbierto = false;
    private bool _estaExpandido = false;
    private int _mensajesSinLeer = 0;
    private string? _ultimoNombreUsuario;
    
    public List<MensajeChatState> Mensajes => _mensajes;
    public bool EstaAbierto 
    { 
        get => _estaAbierto; 
        set => _estaAbierto = value; 
    }
    public bool EstaExpandido 
    { 
        get => _estaExpandido; 
        set => _estaExpandido = value; 
    }
    public int MensajesSinLeer 
    { 
        get => _mensajesSinLeer; 
        set => _mensajesSinLeer = value; 
    }
    
    public void AgregarMensaje(MensajeChatState mensaje)
    {
        _mensajes.Add(mensaje);
    }
    
    public void LimpiarMensajes()
    {
        _mensajes.Clear();
    }
    
    public bool TieneMensajes => _mensajes.Count > 0;
    
    public void InicializarSiVacio(string nombreUsuario, string saludoInicial, List<string> sugerencias)
    {
        // Solo inicializar si no hay mensajes o si cambió el usuario
        if (_mensajes.Count == 0 || _ultimoNombreUsuario != nombreUsuario)
        {
            _ultimoNombreUsuario = nombreUsuario;
            if (_mensajes.Count == 0)
            {
                _mensajes.Add(new MensajeChatState
                {
                    Texto = saludoInicial,
                    EsUsuario = false,
                    Sugerencias = sugerencias,
                    Hora = DateTime.Now
                });
            }
            else if (_mensajes.Count > 0 && !_mensajes[0].EsUsuario)
            {
                // Actualizar saludo existente con nuevo nombre
                _mensajes[0].Texto = saludoInicial;
            }
        }
    }
}

public class MensajeChatState
{
    public string Texto { get; set; } = "";
    public bool EsUsuario { get; set; }
    public DateTime Hora { get; set; } = DateTime.Now;
    public string? RutaNavegacion { get; set; }
    public string? Icono { get; set; }
    public List<string>? Sugerencias { get; set; }
    public bool MostrarOpcionSoporte { get; set; } = false;
    public string? PreguntaOriginal { get; set; }
}
