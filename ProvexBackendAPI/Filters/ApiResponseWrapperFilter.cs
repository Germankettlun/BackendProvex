using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using ProvexBackendAPI.Dto.ApiResponse;

namespace ProvexBackendAPI.Filters
{
    public class ApiResponseWrapperFilter : IActionFilter
    {
        private readonly IConfiguration _configuration;

        public ApiResponseWrapperFilter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnActionExecuting(ActionExecutingContext context) { }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Leer información de build desde configuración (inyectada por web.config)
            var buildNumber = _configuration["BuildInfo:BuildNumber"] ?? "dev-local";
            var buildId = _configuration["BuildInfo:BuildId"] ?? "local";
            var commitHash = _configuration["BuildInfo:CommitHash"] ?? "unknown";
            var branch = _configuration["BuildInfo:Branch"] ?? "local";
            var buildDate = _configuration["BuildInfo:BuildDate"] ?? DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC");
            var environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Unknown";

            // Agregar headers de versión a TODAS las respuestas
            context.HttpContext.Response.Headers.TryAdd("X-API-Build-Number", buildNumber);
            context.HttpContext.Response.Headers.TryAdd("X-API-Build-Id", buildId);
            context.HttpContext.Response.Headers.TryAdd("X-API-Commit", commitHash);
            context.HttpContext.Response.Headers.TryAdd("X-API-Branch", branch);
            context.HttpContext.Response.Headers.TryAdd("X-API-Build-Date", buildDate);
            context.HttpContext.Response.Headers.TryAdd("X-API-Environment", environment);

            if (context.Result is ObjectResult objectResult)
            {
                if (objectResult.Value is not ApiResponse<object>)
                {
                    var wrapped = new ApiResponse<object>(objectResult.Value, objectResult.StatusCode ?? 200);
                    context.Result = new ObjectResult(wrapped)
                    {
                        StatusCode = objectResult.StatusCode,
                    };
                }
            }
            else if (context.Result is EmptyResult)
            {
                context.Result = new ObjectResult(new ApiResponse<object>(null, 204, true, "Sin contenido"))
                {
                    StatusCode = 204
                };
            }
        }
    }
}
