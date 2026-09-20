namespace EmployeeAnalyticsAPI.Models
{
    public partial class ErrorLog
    {
        public int Id { get; set; }

        public string? TraceId { get; set; }

        public string? RequestPath { get; set; }

        public string? RequestMethod { get; set; }

        public int? StatusCode { get; set; }

        public string? ExceptionType { get; set; }

        public string? Message { get; set; }

        public string? StackTrace { get; set; }

        public string? Source { get; set; }

        public string? UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
