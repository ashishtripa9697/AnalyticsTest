using EmployeeAnalyticsAPI.DTOs;
using EmployeeAnalyticsAPI.GlobleService.StaticLogic;
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
    
        [HttpPost("ingest")]
        public async Task<IActionResult> Ingest(CancellationToken ct)
        {
            await _ragService.IngestEmployeeDataAsync(ct);
            return Ok(new { Status = true, Message = MessageLogic.Ingest });
        }

       /// <summary>
      /// Processes a query related to employees and returns the result.
     /// </summary>  
        [HttpPost("query")]
        public async Task<IActionResult> Query([FromForm] EmployeeQueryDto dto,CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(dto.Question))
                return BadRequest(new ApiResponse<EmployeeQueryDto>
                {
                    Status = false,
                    Message = MessageLogic.Question
                });

            var result = await _ragService.AnswerAsync(dto.Question, ct);
            return result.Status ? Ok(result) : StatusCode(500, result);
        }
    }
}