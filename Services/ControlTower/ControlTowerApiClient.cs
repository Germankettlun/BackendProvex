using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProvexApi.Models.ControlTower;
using ProvexApi.Services.ControlTower;
using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ProvexApi.Services.ControlTower
{
    public sealed class ControlTowerApiClient : IControlTowerApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ControlTowerApiOptions _options;
        private readonly ILogger<ControlTowerApiClient> _logger;
        private readonly JsonSerializerOptions _serializerOptions;

        public ControlTowerApiClient(
            HttpClient httpClient,
            IOptions<ControlTowerApiOptions> options,
            ILogger<ControlTowerApiClient> logger)
        {
            _options = options.Value;
            _httpClient = httpClient;
            _logger = logger;

            _httpClient.BaseAddress ??= _options.BaseUri;
            _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);

            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
        }

        public Task<ControlTowerApiResult<ControlTowerLoginResponse>> AuthenticateAsync(
            ControlTowerLoginRequest request,
            CancellationToken ct = default)
        {
            return PostAsync<ControlTowerLoginRequest, ControlTowerLoginResponse>(
                "/user/authorization",
                request,
                bearerToken: null,
                ct);
        }

        public Task<ControlTowerApiResult<ControlTowerLoginResponse>> AuthenticateWithDefaultsAsync(
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(_options.DefaultUsername) ||
                string.IsNullOrWhiteSpace(_options.DefaultPassword))
            {
                throw new InvalidOperationException("Credenciales por defecto de Control Tower no configuradas.");
            }

            var request = new ControlTowerLoginRequest
            {
                Username = _options.DefaultUsername!,
                Password = _options.DefaultPassword!
            };

            return AuthenticateAsync(request, ct);
        }

        public async Task<ControlTowerApiResult<TResponse>> GetAsync<TResponse>(
            string path,
            string? bearerToken,
            IDictionary<string, string?>? query = null,
            CancellationToken ct = default)
        {
            var request = BuildRequest(HttpMethod.Get, path, bearerToken, query);
            return await SendAsync<TResponse>(request, ct).ConfigureAwait(false);
        }

        public async Task<ControlTowerApiResult<TResponse>> PostAsync<TRequest, TResponse>(
            string path,
            TRequest payload,
            string? bearerToken,
            CancellationToken ct = default)
        {
            var request = BuildRequest(HttpMethod.Post, path, bearerToken);
            request.Content = BuildJsonContent(payload);
            return await SendAsync<TResponse>(request, ct).ConfigureAwait(false);
        }

        public async Task<ControlTowerApiResult<TResponse>> PutAsync<TRequest, TResponse>(
            string path,
            TRequest payload,
            string? bearerToken,
            CancellationToken ct = default)
        {
            var request = BuildRequest(HttpMethod.Put, path, bearerToken);
            request.Content = BuildJsonContent(payload);
            return await SendAsync<TResponse>(request, ct).ConfigureAwait(false);
        }

        public async Task<ControlTowerApiResult<bool>> DeleteAsync(
            string path,
            string? bearerToken,
            IDictionary<string, string?>? query = null,
            CancellationToken ct = default)
        {
            var request = BuildRequest(HttpMethod.Delete, path, bearerToken, query);
            var result = await SendAsync<JsonElement?>(request, ct).ConfigureAwait(false);
            return result.Success
                ? ControlTowerApiResult<bool>.Ok(true, result.StatusCode)
                : ControlTowerApiResult<bool>.Fail(result.Error, result.StatusCode);
        }

        private HttpRequestMessage BuildRequest(
            HttpMethod method,
            string path,
            string? bearerToken,
            IDictionary<string, string?>? query = null)
        {
            var cleanPath = path.StartsWith('/') ? path[1..] : path;
            var uri = cleanPath;

            if (query != null)
            {
                var filtered = query
                    .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
                    .ToDictionary(kv => kv.Key, kv => kv.Value!);

                if (filtered.Count > 0)
                {
                    uri = QueryHelpers.AddQueryString(cleanPath, filtered);
                }
            }

            var request = new HttpRequestMessage(method, uri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (!string.IsNullOrWhiteSpace(bearerToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            }

            return request;
        }

        private StringContent BuildJsonContent<T>(T payload)
        {
            var json = JsonSerializer.Serialize(payload, _serializerOptions);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        private async Task<ControlTowerApiResult<TResponse>> SendAsync<TResponse>(
            HttpRequestMessage request,
            CancellationToken ct)
        {
            try
            {
                _logger.LogInformation(
                    "[ControlTower] {Method} {Url}",
                    request.Method,
                    request.RequestUri);

                using var response = await _httpClient.SendAsync(request, ct).ConfigureAwait(false);
                var content = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    if (typeof(TResponse) == typeof(JsonElement?) && string.IsNullOrWhiteSpace(content))
                    {
                        return ControlTowerApiResult<TResponse>.Ok((TResponse)(object?)null!, response.StatusCode);
                    }

                    var data = JsonSerializer.Deserialize<TResponse>(content, _serializerOptions);
                    return ControlTowerApiResult<TResponse>.Ok(data!, response.StatusCode);
                }

                _logger.LogWarning(
                    "[ControlTower] Solicitud fallida. Status {Status} Body {Body}",
                    (int)response.StatusCode,
                    content);

                return ControlTowerApiResult<TResponse>.Fail(content, response.StatusCode);
            }
            catch (TaskCanceledException) when (!ct.IsCancellationRequested)
            {
                _logger.LogError("[ControlTower] Tiempo de espera agotado para {Url}", request.RequestUri);
                return ControlTowerApiResult<TResponse>.Fail("Timeout", System.Net.HttpStatusCode.RequestTimeout);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ControlTower] Error al consumir API externa");
                return ControlTowerApiResult<TResponse>.Fail("UnhandledException", System.Net.HttpStatusCode.InternalServerError);
            }
        }
    }
}