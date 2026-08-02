// Controllers/EmployeeReportController.cs
using EmployeeAnalyticsAPI.Interface;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAnalyticsAPI.Controllers
{
    /// <summary>
    /// employee report controller for generating JSON and PDF reports for employees.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeReportController : ControllerBase
    {
        private readonly IEmployeeReportService _report;

        public EmployeeReportController(
            IEmployeeReportService report,
            ILogger<EmployeeReportController> logger)
        {
            _report = report;
        }
        /// <summary>
        /// endpoint to get JSON report for a single employee by their ID.
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("{employeeId:int}")]
        public async Task<IActionResult> GetReport(
            int employeeId, CancellationToken ct)
        {
            var data = await _report.GenerateEmployeeReportByIdAsync(employeeId, ct);

            if (data == null)
                return NotFound(new
                {
                    Status = false,
                    Message = $"Employee {employeeId} not found."
                });

            return Ok(new { Status = true, Data = data });
        }
        /// <summary>
        /// an endpoint to get JSON reports for all employees.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllReports(
            CancellationToken ct)
        {
            var data = await _report.GenerateEmployeeReportsAsync(ct);

            return Ok(new
            {
                Status = true,
                Count = data.Count,
                Data = data
            });
        }

        /// <summary>
        /// Specific endpoint to download a PDF report for a single employee by their ID.
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("{employeeId:int}/pdf")]
        public async Task<IActionResult> GetPdf(
            int employeeId, CancellationToken ct)
        {

            var bytes = await _report
                .GenerateEmployeeReportPdfByIdAsync(employeeId, ct);

            return File(bytes, "application/pdf",
                $"Employee_{employeeId}_Report.pdf");
        }

        /// <summary>
        /// An endpoint to download a PDF report for all employees.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("all/pdf")]
        public async Task<IActionResult> GetAllPdf(
            CancellationToken ct)
        {

            var bytes = await _report.GenerateEmployeeReportsPdfAsync(ct);

            return File(bytes, "application/pdf",
                $"AllEmployees_{DateTime.UtcNow:yyyyMMdd}.pdf");
        }
    }
}