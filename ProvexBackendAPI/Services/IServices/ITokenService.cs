using ProvexBackendAPI.Services.IServices.Contracts;
using System.Security.Claims;

namespace ProvexBackendAPI.Services.IServices
{
    public interface ITokenService
    {
        Task<AccessTokenResult> GenerateTokenRobustoAsync<T>(
         T subject,
         Func<T, Task<IEnumerable<string>>>? rolesProvider = null,
         IEnumerable<ProvexBackendAPI.Infrastructure.Auth.ClaimMap<T>>? maps = null,
         IEnumerable<Claim>? extraClaims = null);

        Task<AccessTokenResult> GenerateTokenAsync(string username, List<string> roles);
    }
}
