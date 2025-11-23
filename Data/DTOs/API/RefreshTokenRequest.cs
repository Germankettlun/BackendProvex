namespace ProvexApi.Data.DTOs.API
{
    /// <summary>
    /// DTO para recibir el refresh token en las llamadas a /refresh y /logout
    /// </summary>
    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = null!;
    }
}
