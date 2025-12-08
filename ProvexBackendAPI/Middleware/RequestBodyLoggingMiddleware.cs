using Microsoft.AspNetCore.Http;
using Serilog;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ProvexBackendAPI.Middleware
{
    public class RequestBodyLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestBodyLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var request = context.Request;

            var start = DateTimeOffset.UtcNow;
            Log.Information("[REQUEST_IN] {Method} {Path} | Query: {QueryString}", request.Method, request.Path, request.QueryString.ToString());
            // Log básicos de headers relevantes (sin Authorization)
            var contentType = request.Headers["Content-Type"].ToString();
            var userAgent = request.Headers["User-Agent"].ToString();
            var xApiHeaders = string.Join(", ", request.Headers.Where(h => h.Key.StartsWith("X-Api-", StringComparison.OrdinalIgnoreCase)).Select(h => $"{h.Key}:{h.Value}"));            
            Log.Information("[REQUEST_HEADERS] {Method} {Path} | Content-Type: {ContentType} | User-Agent: {UserAgent} | X-Api: {XApi}", request.Method, request.Path, contentType, userAgent, string.IsNullOrEmpty(xApiHeaders) ? "<none>" : xApiHeaders);

            if (request.Method == HttpMethods.Post || request.Method == HttpMethods.Put || request.Method == HttpMethods.Patch)
            {
                request.EnableBuffering();
                using var reader = new StreamReader(request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                request.Body.Position = 0;

                if (!string.IsNullOrWhiteSpace(body))
                {
                    // Enmascarar credenciales en rutas de login
                    var masked = request.Path.HasValue && request.Path.Value!.Contains("/Authentication/Login", StringComparison.OrdinalIgnoreCase)
                        ? "{ \"username\": ****, \"password\": **** }"
                        : body;
                    Log.Information("[REQUEST_BODY] {Method} {Path} | Body: {Body}", request.Method, request.Path, masked);
                }
            }

            await _next(context);

            var elapsed = DateTimeOffset.UtcNow - start;
            Log.Information("[API_SUMMARY] {Method} {Path} | Status: {StatusCode} | Duration: {DurationMs} ms", request.Method, request.Path, context.Response.StatusCode, elapsed.TotalMilliseconds);
        }
    }
}