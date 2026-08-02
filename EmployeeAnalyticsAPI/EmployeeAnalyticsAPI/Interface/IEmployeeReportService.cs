using EmployeeAnalyticsAPI.DTOs;

namespace EmployeeAnalyticsAPI.Interface
{
    public interface IEmployeeReportService
    {
        Task<EmployeeReportDto?> GenerateEmployeeReportByIdAsync(
            int employeeId,CancellationToken ct= default);
        Task<List<EmployeeReportDto>> GenerateEmployeeReportsAsync(
            CancellationToken ct= default);
        Task<Byte[]> GenerateEmployeeReportPdfByIdAsync(
            int employeeId, CancellationToken ct = default);
        Task<Byte[]> GenerateEmployeeReportsPdfAsync(
            CancellationToken ct = default);
    }
}
