namespace ProvexApi.Models.API
{
    public class ApiResponse<T>
    {
        //minusculas
        public ResultEnum status { get; init; }
        public T? data { get; init; }
        public string? message { get; init; }
    }

}
