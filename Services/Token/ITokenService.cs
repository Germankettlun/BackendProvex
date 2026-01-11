using System.Security.Claims;

namespace ProvexApi.Services.Token
{
    public interface ITokenService
    {
        /// <summary>
        /// Genera y persiste AccessToken + RefreshToken.
        /// </summary>
        /// <param name="clientId">Identificador de cliente/usuario</param>
        /// <param name="requestType">Tipo de login (“login_web”, etc.)</param>
        /// <param name="expirationMinutes">Duración en minutos del access token</param>
        /// <returns>Tuple con nuevo AccessToken y RefreshToken</returns>
        (string AccessToken, string RefreshToken) CreateToken(string clientId, string requestType, out int expirationMinutes);

        /// <summary>
        /// Valida un JWT y extrae sus Claims.</summary>
        bool ValidateToken(string token, out ClaimsPrincipal claimsPrincipal);

        /// <summary>
        /// Refresca un token: genera y guarda nuevos Access+Refresh.</summary>
        (string AccessToken, string RefreshToken) RefreshToken(string refreshToken);

        /// <summary>
        /// Revoca (desactiva) un refresh token existente.</summary>
        void RevokeRefreshToken(string refreshToken);
    }
}
