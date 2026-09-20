using EmployeeAnalyticsAPI.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAnalyticsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController(IEmployeeService employeeService) : ControllerBase
    {
        private readonly IEmployeeService _employeeService = employeeService;

        [HttpGet]
        public async Task<IActionResult> GetAllEmployees()
        {
            var response = await _employeeService.GetAllEmployeeAsync();
            return Ok(response); // Returns ApiResponse<List<EmployeeDto>> directly
        }

        [HttpGet("{employeeId:int}")]
        public async Task<IActionResult> GetEmployeeById(int employeeId)
        {
            var response = await _employeeService.GetEmployeeByIdAsync(employeeId);

            return Ok(response); // Returns ApiResponse<EmployeeDto> directly
        }
    }
}