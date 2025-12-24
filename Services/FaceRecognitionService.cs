using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace SistemIA.Services
{
    public class FaceRecognitionService : IFaceRecognitionService
    {
        private readonly IJSRuntime _jsRuntime;

        public FaceRecognitionService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<byte[]?> GetFaceEmbedding(byte[] imageData)
        {
            try
            {
                // Convertir la imagen a base64
                string base64Image = Convert.ToBase64String(imageData);
                string dataUrl = $"data:image/jpeg;base64,{base64Image}";

                // Llamar a la función JavaScript
                byte[]? result = await _jsRuntime.InvokeAsync<byte[]>("getFaceEmbedding", dataUrl);
                
                if (result == null || result.Length == 0)
                {
                    Console.WriteLine("No se obtuvo un embedding facial válido");
                    return null;
                }

                Console.WriteLine($"Embedding facial obtenido: {result.Length} bytes");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting face embedding: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return null;
            }
        }
    }
}
