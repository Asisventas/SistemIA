namespace SistemIA.Models
{
    // DTOs para pruebas con XML externo
    public class EnvioExternoRequest
    {
        public string? Xml { get; set; }
        public string? Modo { get; set; } // "lote" | "sync"
        public string? Ambiente { get; set; } // "test" | "prod"
    }

    public class ConsultaExternaRequest
    {
        public string? Tipo { get; set; } // "cdc" | "lote"
        public string? Id { get; set; }
        public string? Ambiente { get; set; } // "test" | "prod"
    }
}
