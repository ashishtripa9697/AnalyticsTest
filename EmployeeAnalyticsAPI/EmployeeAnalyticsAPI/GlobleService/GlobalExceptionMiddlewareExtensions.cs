using EmployeeAnalyticsAPI.GlobleService.Middleware;

namespace EmployeeAnalyticsAPI.GlobleService
{
    public static class GlobalExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(
            this IApplicationBuilder app)
         =>app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
