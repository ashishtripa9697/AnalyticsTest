namespace EmployeeAnalyticsAPI.DTOs
{
    public class ApiResponse<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public bool Status { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
