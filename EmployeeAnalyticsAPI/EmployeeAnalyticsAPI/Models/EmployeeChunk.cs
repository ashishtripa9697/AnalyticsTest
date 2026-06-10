using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace EmployeeAnalyticsAPI.Models
{
    public partial class EmployeeChunk
    {
        public int Id { get; set; }
        public string Content { get; set; } = null!;
        public string? Department { get; set; }
        public int? EmployeeId { get; set; }
        public string Embedding { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public virtual Employee? Employee { get; set; }

        [NotMapped]
        public float[] EmbeddingVector
        {
            get => JsonSerializer.Deserialize<float[]>(Embedding)
                   ?? Array.Empty<float>();
            set => Embedding = JsonSerializer.Serialize(value);
        }
    }
}
