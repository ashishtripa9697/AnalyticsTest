using EmployeeAnalyticsAPI.DTOs;

namespace EmployeeAnalyticsAPI.Interface
{
    public interface IEmployeeRagService
    {
        Task<ApiResponse<EmployeeQueryDto>> AnswerAsync(string question, CancellationToken ct = default);
        Task IngestEmployeeDataAsync(CancellationToken ct = default);
    }
}
