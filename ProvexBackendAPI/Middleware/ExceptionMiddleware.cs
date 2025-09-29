using Microsoft.IdentityModel.Tokens;
using ProvexBackendAPI.Dto.ApiResponse;
using ProvexBackendAPI.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Authentication;
using System.Text.Json;

namespace ProvexBackendAPI.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception ocurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        public static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";

            // Default
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
            string message = "Error inesperado";

            switch (ex)
            {
                // ---- TUS EXCEPCIONES CUSTOM ----
                case NotFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    message = ex.Message;
                    break;

                case BadRequestException:
                    statusCode = HttpStatusCode.BadRequest;
                    message = ex.Message;
                    break;

                case UnauthorizedException:
                    statusCode = HttpStatusCode.Unauthorized;
                    message = ex.Message;
                    break;

                case ConflictException:
                    statusCode = HttpStatusCode.Conflict;
                    message = ex.Message;
                    break;

                // ---- EXCEPCIONES USADAS EN LOGIN / AUTH ----
                case ValidationException vex:
                    statusCode = HttpStatusCode.BadRequest;             // 400
                    message = string.IsNullOrWhiteSpace(vex.Message) ? "Datos inválidos." : vex.Message;
                    break;

                case InvalidCredentialException:
                    statusCode = HttpStatusCode.Unauthorized;           // 401
                    message = "Usuario o contraseña inválidos.";
                    break;

                // Usas UnauthorizedAccessException con mensaje: "locked_out" | "not_allowed" | "requires_2fa"
                case UnauthorizedAccessException uae when uae.Message == "locked_out":
                    statusCode = HttpStatusCode.Unauthorized;
                    message = "La cuenta está bloqueada temporalmente.";
                    break;

                case UnauthorizedAccessException uae when uae.Message == "not_allowed":
                    statusCode = HttpStatusCode.Unauthorized;
                    message = "Inicio de sesión no permitido.";
                    break;

                case UnauthorizedAccessException uae when uae.Message == "requires_2fa":
                    statusCode = HttpStatusCode.Unauthorized;
                    message = "Se requiere segundo factor de autenticación.";
                    break;

                case SecurityTokenException:
                    statusCode = HttpStatusCode.Unauthorized;           // 401
                    message = "Token inválido o expirado.";
                    break;

                   
            }
            
            context.Response.StatusCode = (int)statusCode;

            if (statusCode == HttpStatusCode.Unauthorized && !context.Response.Headers.ContainsKey("WWW-Authenticate"))
            {
                context.Response.Headers["WWW-Authenticate"] = "Bearer";
            }

            var apiResponse = new ApiResponse<string>(null, (int)statusCode, false, message);
            var result = JsonSerializer.Serialize(apiResponse);

            return context.Response.WriteAsync(result);
        }
    }
}
