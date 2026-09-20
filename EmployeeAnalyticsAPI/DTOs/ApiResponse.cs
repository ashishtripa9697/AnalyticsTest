namespace EmployeeAnalyticsAPI.DTOs
{
    public class ApiResponse<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public bool Status { get; set; }
        public string Message { get; set; } = string.Empty;

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
    }
}
