using EmployeeAnalyticsAPI.DTOs;

namespace EmployeeAnalyticsAPI.Interface
{
    public interface IEmployeeReportService
    {
        Task<EmployeeReportDto?> GenerateEmployeeReportByIdAsync(
            int employeeId,CancellationToken ct= default);
        Task<ApiResponse<EmployeeReportDto>> GenerateEmployeeReportsAsync(int pageNumber=1,int pageSize=20,
            CancellationToken ct= default);
        Task<Byte[]> GenerateEmployeeReportPdfByIdAsync(
            int employeeId, CancellationToken ct = default);
        Task<ApiResponse<byte[]>> GenerateEmployeeReportsPdfAsync(
    int pageNumber = 1,
    int pageSize = 20,
    CancellationToken ct = default);
    }
}
