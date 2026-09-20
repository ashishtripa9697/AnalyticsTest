namespace EmployeeAnalyticsAPI.Models
{
    public class Department
    {
        public int DId { get; set; }
        public string DName { get; set; } = null!;
        public List<Employee> EmployeesTble { get; set; } = new();
    }
}
