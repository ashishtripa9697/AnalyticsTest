namespace EmployeeAnalyticsAPI.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateTime HireDate { get; set; }
        public int DepartmentId { get; set; }
        public int? EmployeeDetailsId { get; set; }

        public virtual Department? Department { get; set; }
        public virtual List<Salary> SalariesTble { get; set; } = new();
        public virtual List<EmployeeChunk> EmployeeChunksTble { get; set; } = new();
        public virtual List<EmployeeDetail> EmployeeDetails { get; set; } = new();
    }
}