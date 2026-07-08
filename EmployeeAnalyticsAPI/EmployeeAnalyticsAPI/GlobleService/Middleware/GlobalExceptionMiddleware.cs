using EmployeeAnalyticsAPI.DataL;
using EmployeeAnalyticsAPI.DTOs;
using EmployeeAnalyticsAPI.Models;
using System.Net;
using System.Text.Json;

namespace EmployeeAnalyticsAPI.GlobleService.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IServiceScopeFactory _scopFactory;
        private readonly FileLogger _fileLogger;

        public GlobalExceptionMiddleware(
            RequestDelegate request,
            ILogger<GlobalExceptionMiddleware> logger,
            IServiceScopeFactory scopFactory)   // removed FileLogger param
        {
            _next = request;
            _logger = logger;
            _scopFactory = scopFactory;
            _fileLogger = FileLogger.Instance; //  use static singleton
        }

        //  Issue 1 fixed — now calls HandleExceptionAsync
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex); // ✅ fixed
            }
        }

        private async Task HandleExceptionAsync(
            HttpContext context, Exception ex)
        {
            var traceId = context.TraceIdentifier;
            var path = context.Request.Path.ToString();
            var method = context.Request.Method;

            var statusCode = ex switch
            {
                ArgumentNullException => HttpStatusCode.BadRequest,
                ArgumentException => HttpStatusCode.BadRequest,
                KeyNotFoundException => HttpStatusCode.NotFound,
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                NotImplementedException => HttpStatusCode.NotImplemented,
                TimeoutException => HttpStatusCode.GatewayTimeout,
                _ => HttpStatusCode.InternalServerError
            };

            // 1. Write to log file
            _fileLogger.Error(
                $"UnhandledException | {method} {path}", ex, traceId);

            // 2. Write to database
            await LogToDatabaseAsync(context, ex, (int)statusCode, traceId);

            // 3. Write to console logger
            _logger.LogError(ex,
                "UnhandledException | TraceId: {TraceId} | " +
                "Path: {Path} | Method: {Method}",
                traceId, path, method);

            // 4. Return JSON response
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new ApiResponse<object>
            {
                Status = false,
                Message = GetFriendlyMessage(ex, statusCode)
            };

            var json = JsonSerializer.Serialize(response,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

            await context.Response.WriteAsync(json);
        }

        //  Issue 3 fixed — HttpStatusCode not object
        private static string GetFriendlyMessage(
            Exception ex, HttpStatusCode code) => code switch
            {
                HttpStatusCode.BadRequest
                    => $"Invalid request: {ex.Message}",
                HttpStatusCode.NotFound
                    => "The requested resource was not found.",
                HttpStatusCode.Unauthorized
                    => "You are not authorised to perform this action.",
                HttpStatusCode.NotImplemented
                    => "This feature is not yet implemented.",
                HttpStatusCode.GatewayTimeout
                    => "The request timed out. Please try again.",
                _ => "An unexpected error occurred. " +
                       "Please contact support if this persists."
            };

        private async Task LogToDatabaseAsync(
            HttpContext context, Exception ex,
            int statusCode, string traceId)
        {
            try
            {
                using var scope = _scopFactory.CreateScope();
                var db = scope.ServiceProvider
                              .GetRequiredService<AppDbContext>();

                db.ErrorsTble.Add(new ErrorLog
                {
                    TraceId = traceId,
                    RequestPath = context.Request.Path,
                    RequestMethod = context.Request.Method,
                    StatusCode = statusCode,
                    ExceptionType = ex.GetType().FullName,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    Source = ex.Source,
                    UserId = context.User?.Identity?.Name
                                    ?? "Anonymous",
                    CreatedAt = DateTime.UtcNow
                });

                await db.SaveChangesAsync();
            }
            catch (Exception dbEx)
            {
                _fileLogger.Error(
                    "CRITICAL: Could not write to ErrorLogs table",
                    dbEx, traceId);
            }
        }
    }
}