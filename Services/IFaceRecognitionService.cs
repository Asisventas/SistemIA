namespace SistemIA.Services
{
    public interface IFaceRecognitionService
    {
        Task<byte[]?> GetFaceEmbedding(byte[] imageData);
    }
}
