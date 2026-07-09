using EmployeeAnalyticsAPI.DataL;
using EmployeeAnalyticsAPI.DTOs;
using EmployeeAnalyticsAPI.GlobleService.StaticLogic;
using EmployeeAnalyticsAPI.Interface;
using EmployeeAnalyticsAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using UglyToad.PdfPig;

namespace EmployeeAnalyticsAPI.Services
{
    public class DocumentRagService : IDocumentRagService
    {
        private readonly AppDbContext _db;
        private readonly EmbeddingService _embedder;
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private readonly string _chatModel;
        private readonly string _baseUrl;

        // chunk size in characters
        private const int ChunkSize = 500;
        private const int ChunkOverlap = 100;

        public DocumentRagService(
            AppDbContext db,
            EmbeddingService embedder,
            IConfiguration config,
            IHttpClientFactory httpClientFactory)
        {
            _db = db;
            _embedder = embedder;
            _http = httpClientFactory.CreateClient();
            _apiKey = config["OpenAI:ApiKey"]!;
            _chatModel = config["OpenAI:ChatModel"] ?? "llama-3.3-70b-versatile";
            _baseUrl = config["OpenAI:BaseUrl"]
                         ?? "https://api.groq.com/openai/v1";
        }

        // ── INGEST PDF ───────────────────────────────────────────
        public async Task<ApiResponse<DocumentQueryDto>> IngestDocumentAsync(
            IFormFile file,
            CancellationToken ct = default)
        {
            var result = new ApiResponse<DocumentQueryDto>();

            // 1. Validate file
            if (file == null || file.Length == 0)
            {
                result.Status = false;
                result.Message = MessageLogic.NoFile;
                return result;
            }

            if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                result.Status = false;
                result.Message = MessageLogic.Supported;
                return result;
            }

            try
            {
                // 2. Remove old chunks for this file
                var existing = await _db.DocumentChunksTble
                    .Where(d => d.FileName == file.FileName)
                    .ToListAsync(ct);
                _db.DocumentChunksTble.RemoveRange(existing);
                await _db.SaveChangesAsync(ct);

                // 3. Read PDF using PdfPig
                using var stream = file.OpenReadStream();
                using var memStream = new MemoryStream();
                await stream.CopyToAsync(memStream, ct);
                memStream.Position = 0;

                var allChunks = new List<(int page, string text)>();
                int pageCount = 0;

                using (var pdf = PdfDocument.Open(memStream.ToArray()))
                {
                    pageCount = pdf.NumberOfPages;

                    foreach (var page in pdf.GetPages())
                    {
                        // Extract text from page
                        var pageText = string.Join(" ",
                            page.GetWords().Select(w => w.Text));

                        if (string.IsNullOrWhiteSpace(pageText))
                            continue;

                        // 4. Split page text into overlapping chunks
                        var chunks = SplitIntoChunks(pageText, page.Number);
                        allChunks.AddRange(chunks);
                    }
                }

                if (!allChunks.Any())
                {
                    result.Status = false;
                    result.Message = MessageLogic.NoFileUpload;
                    return result;
                }

                // 5. Embed each chunk and save to DB
                foreach (var (pageNum, text) in allChunks)
                {
                    var vector = await _embedder.EmbedAsync(text, ct);

                    var chunk = new DocumentChunk
                    {
                        FileName = file.FileName,
                        PageNumber = pageNum,
                        Content = text,
                        CreatedAt = DateTime.UtcNow
                    };
                    chunk.EmbeddingVector = vector;

                    _db.DocumentChunksTble.Add(chunk);
                }

                await _db.SaveChangesAsync(ct);


                result.Status = true;
                result.Message = $"Successfully ingested '{file.FileName}' " +
                                 $"({pageCount} pages, {allChunks.Count} chunks).";
                result.Data.Add(new DocumentQueryDto
                {
                    FileName = file.FileName,
                    PageCount = pageCount,
                    ChunkCount = allChunks.Count
                });
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = $"Ingestion failed: {ex.Message}";
            }

            return result;
        }

        // ── QUERY DOCUMENT ───────────────────────────────────────
        public async Task<ApiResponse<DocumentQueryDto>> AskDocumentAsync(
            string question,
            CancellationToken ct = default)
        {
            var result = new ApiResponse<DocumentQueryDto>();

            try
            {
                // 1. Embed question
                var qVector = await _embedder.EmbedAsync(question, ct);

                // 2. Load all document chunks
                var allChunks = await _db.DocumentChunksTble.ToListAsync(ct);

                if (!allChunks.Any())
                {
                    result.Status = false;
                    result.Message = MessageLogic.NoFileUpload;
                    return result;
                }

                // 3. Find top 5 most similar chunks
                var topChunks = allChunks
                    .Select(c => new
                    {
                        Chunk = c,
                        Similarity = EmbeddingService.CosineSimilarity(
                                         c.EmbeddingVector, qVector)
                    })
                    .OrderByDescending(x => x.Similarity)
                    .Take(5)
                    .Select(x => x.Chunk)
                    .ToList();

                // 4. Build prompt with page references
                var context = string.Join("\n\n", topChunks.Select(c =>
                    $"[{c.FileName} — Page {c.PageNumber}]\n{c.Content}"));

                // 5. Call Groq
                var requestBody = new
                {
                    model = _chatModel,
                    messages = new[]
                    {
                        new { role = "system",
                              content = MessageLogic.CompanyPolicyAssisstent },
                        new { role = "user",
                              content = $"Policy content:\n{context}" +
                                        $"\n\nQuestion: {question}" }
                    }
                };

                var request = new HttpRequestMessage(
                    HttpMethod.Post, $"{_baseUrl}/chat/completions");

                request.Headers.Add("Authorization", $"Bearer {_apiKey}");
                request.Content = JsonContent.Create(requestBody);

                var response = await _http.SendAsync(request, ct);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync(ct);
                    result.Status = false;
                    result.Message = $"LLM error: {error}";
                    return result;
                }

                var json = await response.Content
                    .ReadFromJsonAsync<JsonElement>(cancellationToken: ct);

                var answer = json
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();
                
                result.Status = true;
                result.Message = answer ?? MessageLogic.NoAnswer;
                result.Data.Add(new DocumentQueryDto { Question = question });
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = $"Query failed: {ex.Message}";
            }

            return result;
        }

        // ── HELPER — Split text into overlapping chunks ──────────
        private static List<(int page, string text)> SplitIntoChunks(
            string text, int pageNumber)
        {
            var chunks = new List<(int, string)>();
            int start = 0;

            while (start < text.Length)
            {
                int end = Math.Min(start + ChunkSize, text.Length);
                var chunk = text[start..end].Trim();

                if (!string.IsNullOrWhiteSpace(chunk))
                    chunks.Add((pageNumber, chunk));

                start += ChunkSize - ChunkOverlap;
            }

            return chunks;
        }
    }
}
