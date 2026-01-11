using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ProvexApi.Filters
{
    public class ApiResponseWrapperFilter : IActionFilter
    {
        private readonly IConfiguration _configuration;

        public ApiResponseWrapperFilter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Nada antes de ejecutar la acción por ahora
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Información de build inyectada por variables de entorno / appsettings
            var buildNumber = _configuration["BuildInfo:BuildNumber"] ?? "dev-local";
            var buildId = _configuration["BuildInfo:BuildId"] ?? "local";
            var commitHash = _configuration["BuildInfo:CommitHash"] ?? "unknown";
            var branch = _configuration["BuildInfo:Branch"] ?? "local";
            var buildDate = _configuration["BuildInfo:BuildDate"] ?? DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC");
            var environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Unknown";

            var headers = context.HttpContext.Response.Headers;

            headers.TryAdd("X-API-Build-Number", buildNumber);
            headers.TryAdd("X-API-Build-Id", buildId);
            headers.TryAdd("X-API-Commit", commitHash);
            headers.TryAdd("X-API-Branch", branch);
            headers.TryAdd("X-API-Build-Date", buildDate);
            headers.TryAdd("X-API-Environment", environment);

            // Si más adelante quieres envolver el cuerpo con ApiResponse<T>,
            // aquí puedes portar la misma lógica que usas en ProvexBackendAPI.
        }
    }
}
