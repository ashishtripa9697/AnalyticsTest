// Models/EmployeeDetail.cs
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeAnalyticsAPI.Models
{
    public class EmployeeDetail
    {
        public int Id { get; set; }
        public int? EmployeeId { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string? BankName { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string? AccountNumber { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string? UANNumber { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string? PFNumber { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string? PanNumber { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string? AadharNumber { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string? Address { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string? City { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string? State { get; set; }

        public DateTime? CreatedAt { get; set; }

        // Navigation
        public virtual Employee? Employee { get; set; }
    }
}