
using System.Net;

namespace ProvexApi.Services.ControlTower
{
    public sealed class ControlTowerApiResult<T>
    {
        public bool Success { get; init; }
        public HttpStatusCode StatusCode { get; init; }
        public T? Data { get; init; }
        public string? Error { get; init; }

        public static ControlTowerApiResult<T> Ok(T data, HttpStatusCode statusCode = HttpStatusCode.OK)
            => new() { Success = true, StatusCode = statusCode, Data = data };

        public static ControlTowerApiResult<T> Fail(string? error, HttpStatusCode statusCode)
            => new() { Success = false, StatusCode = statusCode, Error = error };
    }
}