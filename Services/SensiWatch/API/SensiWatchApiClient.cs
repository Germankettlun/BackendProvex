using Microsoft.Extensions.Options;
using ProvexApi.Models.SensiWatch.API;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ProvexApi.Services.SensiWatch.API
{
    /// <summary>
    /// Cliente para realizar llamadas a la API de SensiWatch (llamadas salientes)
    /// </summary>
    public class SensiWatchApiClient : ISensiWatchApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly SensiWatchApiOptions _options;
        private readonly ILogger<SensiWatchApiClient> _logger;
        private string? _cachedToken;
        private DateTime _tokenExpiry;

        public SensiWatchApiClient(
            HttpClient httpClient,
            IOptions<SensiWatchApiOptions> options,
            ILogger<SensiWatchApiClient> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
            
            _httpClient.BaseAddress = new Uri(_options.BaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
        }

        public async Task<string> GetAccessTokenAsync()
        {
            // Verificar si tenemos un token válido en cache
            if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry)
            {
                return _cachedToken;
            }

            try
            {
                _logger.LogInformation("[SensiWatch API] Obteniendo nuevo token de acceso");

                var request = new HttpRequestMessage(HttpMethod.Post, _options.TokenEndpoint);
                request.Headers.Add("Ocp-Apim-Subscription-Key", _options.SubscriptionKey);

                var formData = new Dictionary<string, string>
                {
                    ["username"] = _options.Username,
                    ["password"] = _options.Password,
                    ["client_id"] = _options.ClientId,
                    ["client_secret"] = _options.ClientSecret,
                    ["grant_type"] = "password",
                    ["scope"] = _options.Scope
                };

                request.Content = new FormUrlEncodedContent(formData);

                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("[SensiWatch API] Error obteniendo token: {StatusCode} - {Content}", 
                        response.StatusCode, errorContent);
                    throw new HttpRequestException($"Error getting token: {response.StatusCode}");
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.access_token))
                {
                    throw new InvalidOperationException("Invalid token response");
                }

                // Cachear el token (expira 5 minutos antes del tiempo real para ser seguro)
                _cachedToken = tokenResponse.access_token;
                _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.expires_in - 300);

                _logger.LogInformation("[SensiWatch API] Token obtenido exitosamente, expira en {ExpiresIn} segundos", 
                    tokenResponse.expires_in);

                return _cachedToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SensiWatch API] Error obteniendo token de acceso");
                throw;
            }
        }

        public async Task<CreateTripResponse> CreateTripAsync(CreateTripRequest tripRequest)
        {
            try
            {
                _logger.LogInformation("[SensiWatch API] Creando trip: {InternalTripId}", 
                    tripRequest.InternalTripID);

                var token = await GetAccessTokenAsync();
                var url = $"{_options.TripsEndpoint}/{_options.ProgramId}/Trips";

                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Ocp-Apim-Subscription-Key", _options.SubscriptionKey);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var jsonContent = JsonSerializer.Serialize(tripRequest, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("[SensiWatch API] Error creando trip: {StatusCode} - {Content}", 
                        response.StatusCode, errorContent);
                    throw new HttpRequestException($"Error creating trip: {response.StatusCode} - {errorContent}");
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var createTripResponse = JsonSerializer.Deserialize<CreateTripResponse>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (createTripResponse == null)
                {
                    throw new InvalidOperationException("Invalid create trip response");
                }

                _logger.LogInformation("[SensiWatch API] Trip creado exitosamente: {TripId}", 
                    createTripResponse.TripId);

                return createTripResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SensiWatch API] Error creando trip: {InternalTripId}", 
                    tripRequest.InternalTripID);
                throw;
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                _logger.LogInformation("[SensiWatch API] Probando conectividad");

                var token = await GetAccessTokenAsync();
                
                // Si llegamos aquí sin excepción, la conexión es exitosa
                _logger.LogInformation("[SensiWatch API] Conectividad exitosa");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[SensiWatch API] Error de conectividad");
                return false;
            }
        }
    }
}