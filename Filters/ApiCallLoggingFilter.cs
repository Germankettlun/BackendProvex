// Filters/ApiCallLoggingFilter.cs
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ProvexApi.Helper;
using ProvexApi.Services.Logs;

namespace ProvexApi.Filters
{
    public class ApiCallLoggingFilter : IAsyncActionFilter
    {
        private readonly ApiCallLogger _logger;

        public ApiCallLoggingFilter(ApiCallLogger logger)
        {
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                var http = context.HttpContext;
                var start = DateTime.UtcNow;
                var apiName = context.ActionDescriptor.DisplayName!;
                var method = http.Request.Method;
                var url = http.Request.Path + http.Request.QueryString;
                var clientIp = http.Connection.RemoteIpAddress?.ToString();

                // 1) extraemos esquema y credencial del Authorization header
                var authHeader = http.Request.Headers["Authorization"].FirstOrDefault() ?? "";
                var parts = authHeader.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                var scheme = parts.Length == 2 ? parts[0] : "";
                var param = parts.Length == 2 ? parts[1] : "";

                // 2) determinamos userName y token a loggear
                string? userName;
                string? token;

                if (scheme.Equals("Basic", StringComparison.OrdinalIgnoreCase))
                {
                    // Basic → User ya autenticado por tu BasicAuthenticationHandler
                    userName = http.User.Identity?.Name ?? "anonymous";

                    // Azure token que tu handler puso como claim "azure_at"
                    token = http.User.FindFirst("azure_at")?.Value;
                }
                else if (scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase))
                {
                    // Bearer → tomamos literalmente el token de la cabecera
                    token = param;
                    userName = http.User.Identity?.IsAuthenticated == true
                                     ? http.User.Identity.Name
                                     : "anonymous";
                }
                else
                {
                    // sin auth → anónimo
                    token = null;
                    userName = "anonymous";
                }

                // Ejecuta la acción
                var resultContext = await next();

                // 3) después de la acción, medimos resultado
                var statusCode = resultContext.HttpContext.Response.StatusCode;
                var isSuccess = statusCode >= 200 && statusCode < 300;
                var authSuccess = http.User.Identity?.IsAuthenticated ?? false;
                var durationMs = (int)(DateTime.UtcNow - start).TotalMilliseconds;

                // 4) snippet de la respuesta
                string snippet = "";
                if (resultContext.Result is ObjectResult orResult && orResult.Value != null)
                {
                    var json = JsonSerializer.Serialize(orResult.Value);
                    snippet = json.Length > 1000 ? json.Substring(0, 1000) : json;
                }
                else if (resultContext.Result is ContentResult cr && cr.Content != null)
                {
                    snippet = cr.Content.Length > 1000 ? cr.Content.Substring(0, 1000) : cr.Content;
                }

                // 5) llamamos al logger pasando siempre userName y token correctos
                await _logger.LogAsync(
                    apiName,
                    method!,
                    url!,
                    statusCode,
                    isSuccess,
                    authSuccess,
                    durationMs,
                    snippet,
                    userName,
                    clientIp,
                    token
                );
            }
            catch (Exception ex)
            {
                LogHelper.Log("Existencia | Error : " + ex.Message, "Controller");
            }           
        }
    }
}
