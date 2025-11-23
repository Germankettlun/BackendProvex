using System;
using System.Net; 
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using ProvexApi.Models.SDT;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks; 
using System.Collections.Generic;
using Microsoft.Graph.Models.ExternalConnectors;
using ProvexApi.Helper;
using static Microsoft.Graph.Constants;
using System.Net.Http; // HttpClientHandler
using System.Security.Cryptography.X509Certificates; // X509Certificate2
using System.Net.Security; // SslPolicyErrors

public static class SdtApiHelper
{
    public class SdtApiConfig
    {
        public string ConBDProvex { get; set; }
        public string ConBDExtranet { get; set; }
        public string ApiAuthUser { get; set; }
        public string ApiAuthPassword { get; set; }
        public int ApiTimeOut { get; set; }
        public string Url { get; set; }
        public string Tabla { get; set; }
        // Nuevo: permitir ignorar SSL o pin del certificado desde config si se desea propagar
        public bool IgnoreSslErrors { get; set; }
        public string? TrustedThumbprint { get; set; }
    }

    public static SdtApiConfig GetSdtApiConfig(IConfiguration configuration, string apiName)
    {
        try
        {
            // Conexion BD
            string? provexConn = configuration["ConnectionProvexStrings:DatabaseConnection"];
            string? extranetConn = configuration["ConnectionExtranetStrings:DatabaseConnection"];
            string? apiAuthUser = configuration["SDT:ApiAuth:User"];
            string? apiAuthPassword = configuration["SDT:ApiAuth:Password"];
            string? url = configuration[$"SDT:Apis:{apiName}:url"];
            string? tabla = configuration[$"SDT:Apis:{apiName}:tabla"];
            string? timeOut = configuration[$"SDT:Apis:{apiName}:timeOutMin"];

            // Flags opcionales para SSL
            bool ignoreSsl = false;
            bool.TryParse(configuration[$"SDT:Apis:{apiName}:ignoreSslErrors"], out ignoreSsl);
            bool globalIgnore = false;
            bool.TryParse(configuration["SDT:IgnoreSslErrors"], out globalIgnore);
            string? trustedThumb = configuration[$"SDT:Apis:{apiName}:trustedThumbprint"] ?? configuration["SDT:TrustedThumbprint"];

            //LogHelper.Log($"Consulta conf BD : {ApiDbCon} | Url {url}  | Tabla {tabla} | TimeOutMin {timeOut}", "Configuracion");

            // Validación básica (puedes ampliar según sea necesario)
            if (string.IsNullOrWhiteSpace(apiAuthUser) ||
                string.IsNullOrWhiteSpace(apiAuthPassword) ||
                string.IsNullOrWhiteSpace(url))
            {
                LogHelper.Log("Falta informacion de configuracion", "Configuracion");
                throw new InvalidOperationException("Falta información en la configuración para SDT.");
            }

            return new SdtApiConfig
            {
                ConBDProvex = provexConn ?? throw new InvalidOperationException("DatabaseConnection no puede ser nulo - Provex."),
                ConBDExtranet = extranetConn ?? throw new InvalidOperationException("DatabaseConnection no puede ser nulo - Extranet."),
                ApiAuthUser = apiAuthUser ?? throw new InvalidOperationException("apiAuthUser no puede ser nulo."),
                ApiAuthPassword = apiAuthPassword ?? throw new InvalidOperationException("ApiAuthPassword no puede ser nulo."),
                ApiTimeOut =  int.TryParse(timeOut, out var t) && t >= 0 ? t : 5,  //Si no existe o es menor a 0, asigna 5 minutos
                Url = url ?? throw new InvalidOperationException("Url no puede ser nulo."),
                Tabla = tabla ?? throw new InvalidOperationException("tabla no puede ser nulo."),
                IgnoreSslErrors = ignoreSsl || globalIgnore,
                TrustedThumbprint = string.IsNullOrWhiteSpace(trustedThumb) ? null : trustedThumb
            };
        }
        catch (Exception ex)
        {
            LogHelper.Log($"Error : {ex.Message}", "Configuracion");
            throw new InvalidOperationException("Error : " + ex.Message);
        }
    }

    public static async Task<T> GetDataAsync<T>(
    string url,
    string user,
    string password,
    int timeoutMinutes,
    bool ignoreSslErrors = false,
    string? trustedThumbprint = null)
    {
        string logPrefix = $"url: {url} | timeout {timeoutMinutes} | user: {user}";
        try
        {
            // 0) Configurar handler con validación de certificado opcional
            HttpMessageHandler CreateHandler()
            {
                // Permitir override por variable de entorno si no viene parámetro
                if (!ignoreSslErrors)
                {
                    var envIgnore = Environment.GetEnvironmentVariable("SDT__IgnoreSslErrors");
                    if (!string.IsNullOrWhiteSpace(envIgnore) && bool.TryParse(envIgnore, out var b) && b)
                        ignoreSslErrors = true;
                }
                if (string.IsNullOrWhiteSpace(trustedThumbprint))
                {
                    trustedThumbprint = Environment.GetEnvironmentVariable("SDT__TrustedThumbprint") ?? trustedThumbprint;
                }

                var handler = new HttpClientHandler();

                if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    if (ignoreSslErrors)
                    {
                        LogHelper.Log($"'{logPrefix}' | SSL Validation deshabilitado por configuración (uso sólo en Dev).", "SSL");
                        handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                    }
                    else if (!string.IsNullOrWhiteSpace(trustedThumbprint))
                    {
                        var pin = trustedThumbprint.Replace(" ", string.Empty).ToUpperInvariant();
                        handler.ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) =>
                        {
                            try
                            {
                                if (cert is X509Certificate2 c2)
                                {
                                    var thumb = c2.Thumbprint?.Replace(" ", string.Empty).ToUpperInvariant();
                                    var ok = string.Equals(thumb, pin, StringComparison.OrdinalIgnoreCase);
                                    if (!ok)
                                    {
                                        LogHelper.Log($"Cert pinning falló. Esperado={pin}, Recibido={thumb}", "SSL");
                                    }
                                    return ok;
                                }
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Log($"Error en validación de certificado: {ex.Message}", "SSL");
                            }
                            return false;
                        };
                    }
                    // else: usar validación por defecto del SO
                }
                return handler;
            }

            using (var httpClient = new HttpClient(CreateHandler()))
            {
                httpClient.Timeout = TimeSpan.FromMinutes(timeoutMinutes);

                using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    request.Headers.Add("X-From", "Excel");
                    var creds = Encoding.ASCII.GetBytes($"{user}:{password}");
                    request.Headers.Authorization =
                        new AuthenticationHeaderValue("Basic", Convert.ToBase64String(creds));

                    // 3) Resolver DNS (solo logging)
                    try
                    {
                        var host = new Uri(url).Host;
                        var ipList = Dns.GetHostAddresses(host);
                        foreach (var ip in ipList)
                            LogHelper.Log($"'{logPrefix}' | DNS Resuelto '{host}' -> {ip}", "DNS");
                    }
                    catch (Exception exDns)
                    {
                        LogHelper.Log($"'{logPrefix}' | ❌ Error al resolver DNS: {exDns.Message}", "DNS");
                    }

                    using (var response = await httpClient.SendAsync(request))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            var err = await response.Content.ReadAsStringAsync();
                            throw new Exception($"'{logPrefix}' | Error: {response.StatusCode} - {err}");
                        }

                        var content = await response.Content.ReadAsStringAsync();
                        var data = JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                        });

                        if (data == null)
                        {
                            LogHelper.Log($"'{logPrefix}' | La deserialización retornó null", "Configuracion");
                            throw new Exception($"'{logPrefix}' | Deserialización nula.");
                        }

                        return data;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogHelper.Log($"'{logPrefix}' | Error : {ex.Message}", "Configuracion");
            throw new InvalidOperationException($"'{logPrefix}' | Error : {ex.Message}");
        }
    }

    // Overload para usar directamente la configuración, propagando flags SSL
    public static Task<T> GetDataAsync<T>(SdtApiConfig cfg, string url)
        => GetDataAsync<T>(url, cfg.ApiAuthUser, cfg.ApiAuthPassword, cfg.ApiTimeOut, cfg.IgnoreSslErrors, cfg.TrustedThumbprint);
}


