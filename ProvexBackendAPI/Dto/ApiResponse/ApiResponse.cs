namespace ProvexBackendAPI.Dto.ApiResponse
{
    public class ApiResponse<T>
    {
        public T? Data { get; set; }

        public int StatusCode { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; }


        public ApiResponse(T data, int statusCode, bool success = true, string? message = null)
        {
            Data = data;
            StatusCode = statusCode;
            Success = success;
            Message = message;
        }
    }
}
