using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace EmployeeAnalyticsAPI.Models
{
    public class DocumentChunk
    {
        public int Id { get; set; }
        public string FileName { get; set; } = null!;
        public int PageNumber { get; set; }
        public string Content { get; set; } = null!;
        public string Embedding { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        [NotMapped]
        public float[] EmbeddingVector
        {
            get => JsonSerializer.Deserialize<float[]>(Embedding)
                   ?? Array.Empty<float>();
            set => Embedding = JsonSerializer.Serialize(value);
        }
    }
}
