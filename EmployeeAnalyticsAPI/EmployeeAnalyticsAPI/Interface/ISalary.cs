using EmployeeAnalyticsAPI.DTOs;

namespace EmployeeAnalyticsAPI.Interface
{
    public interface ISalary
    {
        Task<List<SalaryExtremesDto>> GetSalaryExtremes();
        Task<List<AverageSalaryDto>> GetAverageSalary();
        Task<TopDepartmentDto> GetTopDepartment();
    }
}
