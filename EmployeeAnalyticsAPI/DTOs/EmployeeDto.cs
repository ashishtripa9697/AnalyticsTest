namespace EmployeeAnalyticsAPI.DTOs
{
    public class EmployeeDto
    {
        public int EmployeeId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public decimal Salary { get; set; }
        public string? ContactNumber { get; set; } 
        public string DepartmentName { get; set; } = null!;
        public DateTime HireDate { get; set; }
        public string? AadharNumber { get; set; }
        public string? PanNumber { get; set; }
    }
}
