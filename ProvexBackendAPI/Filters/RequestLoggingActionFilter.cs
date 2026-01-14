using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using System.Diagnostics;
using System.Text.Json;

namespace ProvexBackendAPI.Filters
{
    public class RequestLoggingActionFilter : IActionFilter
    {
        private readonly JsonSerializerOptions _jsonOpts = new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IgnoreReadOnlyProperties = true
        };

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var httpMethod = context.HttpContext.Request.Method;
            var controller = context.Controller?.GetType().Name ?? "UnknownController";
            var action = context.ActionDescriptor.DisplayName ?? "UnknownAction";

            var args = context.ActionArguments.ToDictionary(
                kv => kv.Key,
                kv => kv.Value
            );

            string argsJson;
            try
            {
                argsJson = JsonSerializer.Serialize(args, _jsonOpts);
            }
            catch
            {
                argsJson = "<unserializable arguments>";
            }

            var sw = Stopwatch.StartNew();
            context.HttpContext.Items["__ActionSW__"] = sw;

            Log.Information("[API_IN] {Method} {Controller} {Action} | Args: {Args}", httpMethod, controller, action, argsJson);
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            var httpMethod = context.HttpContext.Request.Method;
            var controller = context.Controller?.GetType().Name ?? "UnknownController";
            var action = context.ActionDescriptor.DisplayName ?? "UnknownAction";
            var status = context.HttpContext.Response?.StatusCode;

            var sw = context.HttpContext.Items.TryGetValue("__ActionSW__", out var o) && o is Stopwatch s ? s : null;
            var elapsed = sw != null ? sw.Elapsed.TotalMilliseconds : (double?)null;

            if (context.Exception is null)
            {
                Log.Information("[API_OUT] {Method} {Controller} {Action} | Status: {Status} | Duration: {DurationMs} ms", httpMethod, controller, action, status, elapsed);
            }
            else
            {
                Log.Error(context.Exception, "[API_ERR] {Method} {Controller} {Action} | Status: {Status} | Duration: {DurationMs} ms", httpMethod, controller, action, status, elapsed);
            }
        }
    }
}