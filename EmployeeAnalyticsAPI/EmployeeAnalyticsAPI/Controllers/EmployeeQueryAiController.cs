// Controllers/EmployeeQueryAiController.cs
using EmployeeAnalyticsAPI.DTOs;
using EmployeeAnalyticsAPI.Interface;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAnalyticsAPI.Controllers
{
    /// <summary>
    /// EmployeeQueryAiController provides endpoints for ingesting employee data into a RAG system and querying it using natural language questions.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeQueryAiController : ControllerBase
    {
        private readonly IEmployeeRagService _ragService;

        public EmployeeQueryAiController(IEmployeeRagService ragService)
            => _ragService = ragService;

/// <summary>
/// Initiates the ingestion of employee data asynchronously.
/// </summary>
/// <remarks>This method triggers the ingestion process by delegating the operation to the underlying  service.
/// Ensure that the provided <see cref="CancellationToken"/> is monitored for cancellation  scenarios.</remarks>
/// <param name="ct">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
/// <returns>An <see cref="IActionResult"/> indicating the result of the ingestion operation.  Returns an HTTP 200 OK response
/// with a status message upon successful completion.</returns>
       
        [HttpPost("ingest")]
        public async Task<IActionResult> Ingest(CancellationToken ct)
        {
            await _ragService.IngestEmployeeDataAsync(ct);
            return Ok(new { Status = true, Message = "Ingestion completed." });
        }

/// <summary>
/// Processes a query related to employees and returns the result.
/// </summary>
/// <remarks>This method validates the input question and delegates the query processing to an internal service.
/// Ensure that the <paramref name="dto"/> contains a valid question before calling this method.</remarks>
/// <param name="dto">The data transfer object containing the query details, including the question to be processed.</param>
/// <param name="ct">A cancellation token that can be used to cancel the operation.</param>
/// <returns>An <see cref="IActionResult"/> containing the query result. Returns a <see cref="BadRequestObjectResult"/> if the
/// question is empty or whitespace, an <see cref="OkObjectResult"/> if the query is successful, or a <see
/// cref="StatusCodeResult"/> with status code 500 if an error occurs.</returns>
        
        [HttpPost("query")]
        public async Task<IActionResult> Query([FromBody] EmployeeQueryDto dto,CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(dto.Question))
                return BadRequest(new ApiResponse<EmployeeQueryDto>
                {
                    Status = false,
                    Message = "Question cannot be empty."
                });

            var result = await _ragService.AnswerAsync(dto.Question, ct);
            return result.Status ? Ok(result) : StatusCode(500, result);
        }
    }
}