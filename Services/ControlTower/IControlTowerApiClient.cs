using ProvexApi.Models.ControlTower;
using ProvexApi.Services.ControlTower;

namespace ProvexApi.Services.ControlTower
{
    public interface IControlTowerApiClient
    {
        Task<ControlTowerApiResult<ControlTowerLoginResponse>> AuthenticateAsync(
            ControlTowerLoginRequest request,
            CancellationToken ct = default);

        Task<ControlTowerApiResult<ControlTowerLoginResponse>> AuthenticateWithDefaultsAsync(
            CancellationToken ct = default);

        Task<ControlTowerApiResult<TResponse>> GetAsync<TResponse>(
            string path,
            string? bearerToken,
            IDictionary<string, string?>? query = null,
            CancellationToken ct = default);

        Task<ControlTowerApiResult<TResponse>> PostAsync<TRequest, TResponse>(
            string path,
            TRequest payload,
            string? bearerToken,
            CancellationToken ct = default);

        Task<ControlTowerApiResult<TResponse>> PutAsync<TRequest, TResponse>(
            string path,
            TRequest payload,
            string? bearerToken,
            CancellationToken ct = default);

        Task<ControlTowerApiResult<bool>> DeleteAsync(
            string path,
            string? bearerToken,
            IDictionary<string, string?>? query = null,
            CancellationToken ct = default);
    }
}