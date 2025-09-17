namespace ProvexBackendAPI.Infrastructure.Auth
{
    public class JwtSettings
    {
        public string Key { get; set; } = default!;
        public string Issuer { get; set; } = default!;
        public string Audience { get; set; } = default!;
        
        //Vendrá de configuración
        public int AccessTokenMinutes { get; set; } = 120;
    }
}
