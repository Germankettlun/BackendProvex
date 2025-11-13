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

            HttpStatusCode statusCode = ex switch
            {
                NotFoundException => HttpStatusCode.NotFound,
                BadRequestException => HttpStatusCode.BadRequest,
                UnauthorizedException => HttpStatusCode.Unauthorized,
                ConflictException => HttpStatusCode.Conflict,
                _ => HttpStatusCode.InternalServerError,
            };

            string message = ex.Message ?? "Error inesperado";

            context.Response.StatusCode = (int)statusCode;

            var apiResponse = new ApiResponse<string>(null, (int)statusCode, false, message);
            var result = JsonSerializer.Serialize(apiResponse);

            return context.Response.WriteAsync(result);
        }

  }
}
