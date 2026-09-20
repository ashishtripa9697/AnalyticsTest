using EmployeeAnalyticsAPI.DTOs;
using EmployeeAnalyticsAPI.GlobleService.StaticLogic;
using EmployeeAnalyticsAPI.Interface;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAnalyticsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentQueryController : ControllerBase
    {
        private readonly IDocumentRagService _docRag;
        private readonly ILogger<DocumentQueryController> _logger;

        public DocumentQueryController(
            IDocumentRagService docRag,
            ILogger<DocumentQueryController> logger)
        {
            _docRag = docRag;
            _logger = logger;
        }

        /// <summary>
        /// Upload a PDF and ingest it into the vector store.
        /// </summary>
        [HttpPost("ingest-document")]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB max
        public async Task<IActionResult> IngestDocument(
            IFormFile file,
            CancellationToken ct)
        {
            _logger.LogInformation(
                "Ingesting document: {FileName}", file?.FileName);

            var result = await _docRag.IngestDocumentAsync(file!, ct);

            return result.Status
                ? Ok(result)
                : BadRequest(result);
        }

        /// <summary>
        /// Ask a question about the uploaded policy document.
        /// Example: "What is the notice period for senior employees?"
        /// </summary>
        [HttpPost("ask-document")]
        public async Task<IActionResult> AskDocument(
            [FromForm] EmployeeQueryDto dto,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(dto.Question))
                return BadRequest(new ApiResponse<DocumentQueryDto>
                {
                    Status = false,
                    Message = MessageLogic.Question
                });

            _logger.LogInformation(
                "Document question: {Question}", dto.Question);

            var result = await _docRag.AskDocumentAsync(dto.Question, ct);

            return result.Status
                ? Ok(result)
                : StatusCode(500, result);
        }
    }
}