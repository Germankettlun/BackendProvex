using System;

namespace ProvexApi.Models.BackEnd.Logs
{
    public class ServiceApiCallLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string ApiName { get; set; } = null!;
        public string HttpMethod { get; set; } = null!;
        public string RequestUrl { get; set; } = null!;
        public int StatusCode { get; set; }
        public bool IsSuccess { get; set; }
        public bool AuthSuccess { get; set; }
        public string? UserName { get; set; }           // ← nuevo
        public string? ClientIp { get; set; }           // ← nuevo
        public int DurationMs { get; set; }
        public int ResponseSize { get; set; }
        public string? Snippet { get; set; }           // fragmento de la respuesta
        public string? token { get; set; }           // fragmento de la respuesta
    }
}
