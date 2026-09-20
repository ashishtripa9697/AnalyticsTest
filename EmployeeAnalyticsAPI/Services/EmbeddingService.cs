// Services/EmbeddingService.cs
using System.Security.Cryptography;
using System.Text;

namespace EmployeeAnalyticsAPI.Services
{
    public class EmbeddingService
    {
        private const int VectorSize = 384;

        public EmbeddingService(IConfiguration configuration,
                                 IHttpClientFactory httpClientFactory)
        {
            // no external API needed
        }

        public Task<float[]> EmbedAsync(
            string text,
            CancellationToken ct = default)
        {
            var vector = GenerateLocalEmbedding(text);
            return Task.FromResult(vector);
        }

        // Generates a deterministic vector from text using word hashing
        private static float[] GenerateLocalEmbedding(string text)
        {
            var vector = new float[VectorSize];
            var words = text.ToLower()
                             .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                // hash each word into vector positions
                var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(word));

                for (int i = 0; i < bytes.Length && i * 2 + 1 < VectorSize; i++)
                {
                    int pos = (bytes[i] * 3 + i) % VectorSize;
                    float value = (bytes[i] / 255f) * 2 - 1;  // normalize to -1..1
                    vector[pos] += value;
                }
            }

            // Normalize the vector to unit length
            float magnitude = MathF.Sqrt(vector.Sum(v => v * v)) + 1e-10f;
            for (int i = 0; i < vector.Length; i++)
                vector[i] /= magnitude;

            return vector;
        }

        public static float CosineSimilarity(float[] a, float[] b)
        {
            float dot = 0, magA = 0, magB = 0;

            for (int i = 0; i < a.Length; i++)
            {
                dot += a[i] * b[i];
                magA += a[i] * a[i];
                magB += b[i] * b[i];
            }

            return dot / (MathF.Sqrt(magA) * MathF.Sqrt(magB) + 1e-10f);
        }
    }
}