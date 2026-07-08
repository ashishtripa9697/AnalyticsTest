using EmployeeAnalyticsAPI.DataL;
using EmployeeAnalyticsAPI.DTOs;
using EmployeeAnalyticsAPI.GlobleService.StaticLogic;
using EmployeeAnalyticsAPI.Interface;
using EmployeeAnalyticsAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EmployeeAnalyticsAPI.Services
{
    public class EmployeeRagService(
        AppDbContext db,
        EmbeddingService embedder,
        IConfiguration config,
        IHttpClientFactory httpClientFactory) : IEmployeeRagService
    {
        private readonly AppDbContext _db = db;
        private readonly EmbeddingService _embedder = embedder;
        private readonly HttpClient _http = httpClientFactory.CreateClient();
        private readonly string _apiKey = config["OpenAI:ApiKey"]!;
        private readonly string _chatModel = config["OpenAI:ChatModel"] ?? "llama3-70b-8192";
        private readonly string _baseUrl = config["OpenAI:BaseUrl"] ?? "https://api.groq.com/openai/v1";

        public async Task IngestEmployeeDataAsync(CancellationToken ct = default)
        {
            var existing = await _db.EmployeeChunksTble.ToListAsync(ct);
            _db.EmployeeChunksTble.RemoveRange(existing);
            await _db.SaveChangesAsync(ct);

            var employees = await _db.EmployeesTble
                .Include(e => e.Department)
                .Include(e => e.SalariesTble)
                .ToListAsync(ct);

            foreach (var emp in employees)
            {
                var latestSalary = emp.SalariesTble
                    .OrderByDescending(s => s.EffectiveFrom)
                    .FirstOrDefault();

                var text = $"{emp.FirstName} {emp.LastName} works in " +
                           $"{emp.Department?.DName} department. " +
                           $"Current salary: {latestSalary?.Amount:C}. " +
                           $"Hired on: {emp.HireDate:yyyy-MM-dd}. " +
                           $"Employee ID: {emp.Id}.";

                var vector = await _embedder.EmbedAsync(text, ct);

                var chunk = new EmployeeChunk
                {
                    Content = text,
                    Department = emp.Department?.DName,
                    EmployeeId = emp.Id,
                    CreatedAt = DateTime.UtcNow
                };
                chunk.EmbeddingVector = vector;
                _db.EmployeeChunksTble.Add(chunk);
            }

            await _db.SaveChangesAsync(ct);
        }

        public async Task<ApiResponse<EmployeeQueryDto>> AnswerAsync(string question, CancellationToken ct = default)
        {
            var result = new ApiResponse<EmployeeQueryDto>();

            var qVector = await _embedder.EmbedAsync(question, ct);

            var allChunks = await _db.EmployeeChunksTble.ToListAsync(ct);

            if (!allChunks.Any())
            {
                result.Status = false;
                result.Message = MessageLogic.NoEmployeeData;
                return result;
            }

            var topChunks = allChunks
                .Select(c => new
                {
                    Chunk = c,
                    Similarity = EmbeddingService.CosineSimilarity(c.EmbeddingVector, qVector)
                })
                .OrderByDescending(x => x.Similarity)
                .Take(5)
                .Select(x => x.Chunk)
                .ToList();

            var context = string.Join("\n", topChunks.Select(c => c.Content));

            var requestBody = new
            {
                model = _chatModel,
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = MessageLogic.AiAnswerIfNoData
                    },
                    new
                    {
                        role = "user",
                        content = $"Employee data:\n{context}\n\nQuestion: {question}"
                    }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/chat/completions");

            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            request.Content = JsonContent.Create(requestBody);

            var response = await _http.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                result.Status = false;
                result.Message = $"Groq API error {(int)response.StatusCode}: {errorBody}";
                return result;
            }

            var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);

            string? answer = null;
            try
            {
                answer = json
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();
            }
            catch
            {
                if (json.TryGetProperty("choices", out var choices) && choices.ValueKind == JsonValueKind.Array && choices.GetArrayLength() > 0)
                {
                    var first = choices[0];
                    if (first.TryGetProperty("text", out var textProp) && textProp.ValueKind == JsonValueKind.String)
                    {
                        answer = textProp.GetString();
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(answer))
            {
                result.Status = false;
                result.Message = MessageLogic.ModelAnswer;
                return result;
            }

            result.Status = true;
            result.Message = answer;
            result.Data.Add(new EmployeeQueryDto { Question = question });

            return result;
        }
    }
}