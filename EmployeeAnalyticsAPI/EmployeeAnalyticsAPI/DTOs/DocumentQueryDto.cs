namespace EmployeeAnalyticsAPI.DTOs
{
    public class DocumentQueryDto
    {
        public string? Question { get; set; }
        public string? FileName { get; set; }
        public int PageCount { get; set; }
        public int ChunkCount { get; set; }
    }
}
