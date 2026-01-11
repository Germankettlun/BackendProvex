namespace ProvexApi.Entities.Login
{
    public class SessionToken
    {
        public int Id { get; set; }
        public string Jti { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public DateTime IssuedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;
        public string RefreshToken { get; set; } = null!;
        public DateTime RefreshTokenExpiresAt { get; set; }
    }
}
