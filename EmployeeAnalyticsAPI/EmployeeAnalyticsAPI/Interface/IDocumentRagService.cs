using EmployeeAnalyticsAPI.DTOs;

namespace EmployeeAnalyticsAPI.Interface
{
    public interface IDocumentRagService
    {
        Task<ApiResponse<DocumentQueryDto>> IngestDocumentAsync(
            IFormFile file,
            CancellationToken ct = default);

        Task<ApiResponse<DocumentQueryDto>> AskDocumentAsync(
            string question,
            CancellationToken ct = default);
    }
}
