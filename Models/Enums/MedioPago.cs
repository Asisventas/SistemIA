namespace SistemIA.Models.Enums
{
    /// <summary>
    /// Medios de pago admitidos. Mapeables a SIFEN iTiPago (E7.1)
    /// </summary>
    public enum MedioPago
    {
        Efectivo = 1,
        Cheque = 2,
        Tarjeta = 3, // Crédito, Débito o Prepaga
        Transferencia = 4,
        Vale = 5,
        Retencion = 6,
        // Extensiones operativas del POS
        DevolucionCliente = 10,
        CobroFlota = 11,
        ChequeDia = 12,
        ChequeDiferido = 13,
        QR = 14,
        Billetera = 15,
        Otros = 99
    }
}
