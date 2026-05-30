using EmployeeAnalyticsAPI.DTOs;
using EmployeeAnalyticsAPI.Interface;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAnalyticsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly ISalary _salaryService;
        public AnalyticsController(ISalary salaryService)
        {
            _salaryService = salaryService;
        }

        // GET: /api/Analytics/salary-extremes
        [HttpGet("salary-extremes")]
        public async Task<ActionResult> GetSalaryExtremes()
        {
            // current SalariesTble only
            var result = await _salaryService.GetSalaryExtremes();
            return Ok(new ApiResponse<SalaryExtremesDto> { Data = result, Status = true, Message = "Success" });
        }

        // GET: /api/Analytics/average-salary
        [HttpGet("average-salary")]
        public async Task<ActionResult> GetAverageSalary()
        {
            var result = await _salaryService.GetAverageSalary();
            return Ok(new ApiResponse<AverageSalaryDto>
            {
                Data = result,
                Status = true,
                Message="Success"
            });
        }

        // GET: /api/Analytics/top-department
        [HttpGet("top-department")]
        public async Task<ActionResult<TopDepartmentDto>> GetTopDepartment()
        {
            var result = await _salaryService.GetTopDepartment();
            return Ok(result);
        }
    }
}
