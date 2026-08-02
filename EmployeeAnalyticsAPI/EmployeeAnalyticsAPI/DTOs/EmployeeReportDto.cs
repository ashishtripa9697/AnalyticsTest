// DTOs/EmployeeReportDto.cs
namespace EmployeeAnalyticsAPI.DTOs
{
    public class EmployeeReportDto
    {
        // ── Basic Info ───────────────────────────────────────────
        public int EmployeeId { get; set; }
        public string? FullName { get; set; }
        public string? Department { get; set; }
        public DateTime HireDate { get; set; }

        // ── Salary Breakdown ─────────────────────────────────────
        public decimal CurrentSalary { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal HRA { get; set; }
        public decimal PFDeduction { get; set; }
        public decimal NetSalary { get; set; }

        // ── Bank Details ─────────────────────────────────────────
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }

        // ── PF & UAN ─────────────────────────────────────────────
        public string? UANNumber { get; set; }
        public string? PFNumber { get; set; }

        // ── Personal Details ─────────────────────────────────────
        public string? PanNumber { get; set; }
        public string? AadharNumber { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }

        // ── Report Meta ──────────────────────────────────────────
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}