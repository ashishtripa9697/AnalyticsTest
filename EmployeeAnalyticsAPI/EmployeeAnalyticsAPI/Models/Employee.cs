namespace EmployeeAnalyticsAPI.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateTime HireDate { get; set; }
        public int DepartmentId { get; set; }
        public Department? Department { get; set; }
        public List<Salary> SalariesTble { get; set; } = new();
        public List<EmployeeChunk> EmployeeChunksTble { get; set; } = new();
    }
}
