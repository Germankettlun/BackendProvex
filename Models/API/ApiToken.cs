namespace ProvexApi.Models.API
{
	public class ApiToken
	{
		public int Id { get; set; }
		public string Token { get; set; } = null!;
        public string ClientId { get; set; } = null!; // Identificador del cliente al que se asigna el token
        public DateTime CreatedAt { get; set; } =	DateTime.UtcNow;
		// Si deseas que no expire a menos que se revoque, puedes dejar ExpiresAt nulo o asignar una fecha muy lejana.
		public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;
        public string? RefreshToken { get; set; } = null!;
        public DateTime? RefreshTokenExpiresAt { get; set; } = DateTime.UtcNow;

    }
}