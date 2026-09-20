namespace EmployeeAnalyticsAPI.DTOs
{
    public class SalaryExtremesDto
    {
        public string Department { get; set; } = null!;
        public decimal HighestSalary { get; set; }
        public decimal LowestSalary { get; set; }
        public int EmployeeCount { get; set; }
    }
}
