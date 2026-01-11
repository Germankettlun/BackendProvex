using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ProvexApi.Data;
using ProvexApi.Helper;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace ProvexApi.Services.Token
{
    public class TokenService : ITokenService
    {
        private readonly ProvexDbContext _db;
        private readonly IConfiguration _configuration;

        public TokenService(ProvexDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public (string AccessToken, string RefreshToken) CreateToken(string clientId, string requestType, out int expirationMinutes)
        {
            // 1) Leer expiración del JWT
            expirationMinutes = _configuration.GetValue<int>(
                $"Jwt:TimeByType:{requestType.ToLower()}", 30);

            // 2) Generar JWT
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var secretKey = _configuration["Jwt:Key"];

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, clientId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var accessToken = JwtHelper.GenerateToken(
                issuer, audience, secretKey, claims, expirationMinutes)
                ?? throw new InvalidOperationException("No se pudo generar el AccessToken.");

            // 3) Generar RefreshToken
            var refreshToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            var refreshExpireDays = _configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays", 7);
            var refreshExpiry = DateTime.UtcNow.AddDays(refreshExpireDays);

            // 4) Persistir en BD
            var dbEntry = new Models.API.ApiToken
            {
                ClientId = clientId,
                Token = accessToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
                RefreshToken = refreshToken,
                RefreshTokenExpiresAt = refreshExpiry,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _db.ApiTokens.Add(dbEntry);
            _db.SaveChanges();

            // 5) Devolver ambos tokens
            return (accessToken, refreshToken);
        }

        public bool ValidateToken(string token, out ClaimsPrincipal claimsPrincipal)
        {
            claimsPrincipal = null!;
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var secretKey = _configuration["Jwt:Key"];
            var handler = new JwtSecurityTokenHandler();

            try
            {
                var keyBytes = Encoding.UTF8.GetBytes(secretKey);
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                claimsPrincipal = handler.ValidateToken(token, validationParameters, out _);
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"ServiceToken.ValidateToken error: {ex.Message}", nameof(TokenService));
                return false;
            }
        }

        public (string AccessToken, string RefreshToken) RefreshToken(string refreshToken)
        {
            // 1) Buscar el refresh activo
            var entry = _db.ApiTokens
                .FirstOrDefault(x => x.RefreshToken == refreshToken && x.IsActive);

            if (entry == null || entry.RefreshTokenExpiresAt < DateTime.UtcNow)
                throw new SecurityTokenException("Refresh token inválido o expirado.");

            // 2) Inactivar el antiguo
            entry.IsActive = false;
            _db.ApiTokens.Update(entry);
            _db.SaveChanges();

            // 3) Generar nuevos tokens (reusa CreateToken)
            var (newAccess, newRefresh) = CreateToken(entry.ClientId, "login_web", out _);
            return (newAccess, newRefresh);
        }

        public void RevokeRefreshToken(string refreshToken)
        {
            var entry = _db.ApiTokens
                .FirstOrDefault(x => x.RefreshToken == refreshToken && x.IsActive);
            if (entry == null) return;

            entry.IsActive = false;
            _db.ApiTokens.Update(entry);
            _db.SaveChanges();
        }
    }
}
