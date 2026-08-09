// Controllers/EmployeeReportController.cs
using EmployeeAnalyticsAPI.DTOs;
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
            IEmployeeReportService report)
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
    int pageNumber = 1,
    int pageSize = 20,
    CancellationToken ct = default)
        {
            var data = await _report.GenerateEmployeeReportsAsync(
                pageNumber,
                pageSize,
                ct);

            return Ok(data);
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
            if (bytes == null || bytes.Length == 0)
                return NotFound(new { Status = false, Message = $"Employee {employeeId} not found." });
            return File(bytes, "application/pdf",
                $"Employee_{employeeId}_Report.pdf");
        }

        /// <summary>
        /// An endpoint to download a PDF report for employees.
        /// </summary>
        [HttpGet("all/pdf")]
        public async Task<IActionResult> GetAllPdf(
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken ct = default)
        {
            var response = await _report.GenerateEmployeeReportsPdfAsync(
                pageNumber,
                pageSize,
                ct);

            if (!response.Status || response.Data == null || response.Data.Count == 0)
            {
                return NotFound(new
                {
                    Status = false,
                    Message = "No employee reports found."
                });
            }

            return File(
                response.Data[0],
                "application/pdf",
                $"Employees_Page_{pageNumber}_{DateTime.UtcNow:yyyyMMdd}.pdf");
        }
    }
}