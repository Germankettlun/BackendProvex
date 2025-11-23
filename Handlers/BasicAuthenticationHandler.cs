using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ProvexApi.Handlers
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _cfg;

        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IHttpClientFactory httpFactory,
            IConfiguration configuration
        ) : base(options, logger, encoder)
        {
            // Si existe un HttpClient nombrado "aad-ropc" lo usa; si no, usa el default.
            _http = httpFactory.CreateClient("aad-ropc");
            _cfg = configuration;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var path = Request.Path.ToString();

            if (!Request.Headers.TryGetValue("Authorization", out var hdr))
                return AuthenticateResult.Fail("Missing Authorization Header");

            var header = hdr.ToString();
            if (!header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                return AuthenticateResult.Fail("Invalid Authorization Header");

            // ==============================
            // 1) Decodificar credenciales
            // ==============================
            string user, pass;
            try
            {
                var b64 = header.Substring("Basic ".Length).Trim();
                var raw = Encoding.UTF8.GetString(Convert.FromBase64String(b64));
                var i = raw.IndexOf(':');
                if (i <= 0) throw new FormatException();
                user = raw[..i];
                pass = raw[(i + 1)..];
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Basic: credenciales mal formadas. Path={Path}", path);
                return AuthenticateResult.Fail("Invalid Basic credentials");
            }

            // ==============================
            // 2) Config Azure AD
            // ==============================
            var tenant = _cfg["AzureAd:TenantId"];
            var clientId = _cfg["AzureAd:ClientIdNative"];
            var audience = _cfg["AzureAd:Audience"]; // api://... o https://graph.microsoft.com
            var instance = (_cfg["AzureAd:Instance"] ?? "https://login.microsoftonline.com").TrimEnd('/');
            var scopesCfg = _cfg["AzureAd:Scopes"];   // opcional
            var allowFallbackToGraph = _cfg.GetValue("AzureAd:FallbackToGraph", true);

            if (string.IsNullOrWhiteSpace(tenant)) return AuthenticateResult.Fail("CONFIG_MISSING_TENANT");
            if (string.IsNullOrWhiteSpace(clientId)) return AuthenticateResult.Fail("CONFIG_MISSING_CLIENTID");
            if (string.IsNullOrWhiteSpace(audience)) return AuthenticateResult.Fail("CONFIG_MISSING_AUDIENCE");

            var tokenUrl = $"{instance}/{tenant}/oauth2/v2.0/token";

            // Scopes por defecto si no vienen en config
            string scopes = string.IsNullOrWhiteSpace(scopesCfg)
                ? (audience.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                    ? "openid profile offline_access https://graph.microsoft.com/User.Read"
                    : $"{audience.TrimEnd('/')}/access_as_user openid profile offline_access")
                : scopesCfg;

            Logger.LogInformation(
                "Basic: ROPC AAD. User={User} Tenant={Tenant} Audience={Audience} Path={Path} Scopes={Scopes}",
                user, tenant, audience, path, scopes);

            // ==============================
            // 3) Llamar a token endpoint (sin cancelar por RequestAborted)
            // ==============================
            var (ok, body, status, err, desc, codes) =
                await RopcAsync(tokenUrl, clientId, user, pass, scopes, CancellationToken.None);

            // Fallback automático a Graph si 500011 invalid_resource
            if (!ok && allowFallbackToGraph && codes.Contains("500011") &&
                !audience.StartsWith("https://graph.microsoft.com", StringComparison.OrdinalIgnoreCase))
            {
                Logger.LogWarning("Basic: 500011 invalid_resource. Probando fallback a Graph...");
                scopes = "openid profile offline_access https://graph.microsoft.com/User.Read";

                (ok, body, status, err, desc, codes) =
                    await RopcAsync(tokenUrl, clientId, user, pass, scopes, CancellationToken.None);
            }

            if (!ok)
            {
                Logger.LogWarning(
                    "Basic: ROPC falló. Status={Status} Error={Error} Codes={Codes}. Desc={Desc}",
                    (int)status, err, codes, Truncate(desc, 500));

                if (status == HttpStatusCode.Unauthorized)
                    return AuthenticateResult.Fail(MapAadError(err, codes));

                return AuthenticateResult.Fail($"AAD_{(int)status}");
            }

            // ==============================
            // 4) Parsear token de respuesta
            // ==============================
            string accessToken, tokenType = "Bearer";
            DateTimeOffset? expiresOn = null;

            try
            {
                using var j = JsonDocument.Parse(body);
                accessToken = j.RootElement.GetProperty("access_token").GetString()!;
                if (j.RootElement.TryGetProperty("token_type", out var tt))
                    tokenType = tt.GetString() ?? "Bearer";
                if (j.RootElement.TryGetProperty("expires_in", out var ei))
                    expiresOn = DateTimeOffset.UtcNow.AddSeconds(ei.GetInt32());
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Basic: token response inválida. Body={Body}", Truncate(body, 800));
                return AuthenticateResult.Fail("AAD_INVALID_TOKEN_RESPONSE");
            }

            Logger.LogInformation(
                "Basic: token OK. Type={Type} ExpiresUtc={Exp}",
                tokenType, expiresOn?.UtcDateTime.ToString("o") ?? "n/a");

            // ==============================
            // 5) Construir identidad
            // ==============================
            var id = new ClaimsIdentity(Scheme.Name);
            id.AddClaim(new Claim(ClaimTypes.Name, user));
            id.AddClaim(new Claim("auth_kind", "basic-aad"));
            id.AddClaim(new Claim("azure_token_type", tokenType));
            id.AddClaim(new Claim("azure_at", accessToken));
            if (expiresOn.HasValue)
                id.AddClaim(new Claim("azure_expires_on_utc", expiresOn.Value.UtcDateTime.ToString("o")));

            Context.Items["MsAccessToken"] = accessToken;
            Context.Items["MsTokenType"] = tokenType;
            if (expiresOn.HasValue)
                Context.Items["MsTokenExpiresOnUtc"] = expiresOn.Value.UtcDateTime;

            return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(id), Scheme.Name));
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.Headers["WWW-Authenticate"] = "Basic realm=\"provex\", charset=\"UTF-8\"";
            Logger.LogDebug("Basic: challenge. Path={Path}", Request.Path.ToString());
            return base.HandleChallengeAsync(properties);
        }

        protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            Logger.LogWarning("Basic: forbidden. Path={Path}", Request.Path.ToString());
            return base.HandleForbiddenAsync(properties);
        }

        // ======================================================
        // Helpers
        // ======================================================
        private async Task<(bool ok, string body, HttpStatusCode status, string err, string desc, string codes)>
            RopcAsync(string tokenUrl, string clientId, string user, string pass, string scopes, CancellationToken ct)
        {
            using var form = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = clientId,
                ["username"] = user,
                ["password"] = pass,
                ["scope"] = scopes
            });

            var sw = System.Diagnostics.Stopwatch.StartNew();

            HttpResponseMessage resp;
            try
            {
                resp = await _http.PostAsync(tokenUrl, form, ct);
            }
            catch (OperationCanceledException ex)
            {
                sw.Stop();
                Logger.LogWarning(ex,
                    "Basic: ROPC timeout/cancelled tras {ElapsedMs} ms. Url={Url}",
                    sw.ElapsedMilliseconds, tokenUrl);

                return (false, "", HttpStatusCode.RequestTimeout, "request_cancelled", "Cancelled", "");
            }
            catch (HttpRequestException ex)
            {
                sw.Stop();
                Logger.LogError(ex,
                    "Basic: error de red AAD tras {ElapsedMs} ms. Url={Url}",
                    sw.ElapsedMilliseconds, tokenUrl);
                return (false, "", HttpStatusCode.BadGateway, "network_error", ex.Message, "");
            }
            catch (Exception ex)
            {
                sw.Stop();
                Logger.LogError(ex,
                    "Basic: excepción llamando AAD tras {ElapsedMs} ms. Url={Url}",
                    sw.ElapsedMilliseconds, tokenUrl);
                return (false, "", HttpStatusCode.BadGateway, "unhandled", ex.Message, "");
            }

            sw.Stop();

            var body = await SafeReadAsync(resp);
            Logger.LogDebug(
                "Basic: ROPC AAD respondió Status={Status} en {ElapsedMs} ms",
                (int)resp.StatusCode, sw.ElapsedMilliseconds);

            if (resp.IsSuccessStatusCode)
                return (true, body, resp.StatusCode, "", "", "");

            var (err, desc, codes) = ParseAadError(body);
            return (false, body, resp.StatusCode, err, desc, codes);
        }

        private static (string error, string description, string codes) ParseAadError(string json)
        {
            try
            {
                using var j = JsonDocument.Parse(json);
                var err = j.RootElement.TryGetProperty("error", out var e) ? e.GetString() ?? "" : "";
                var desc = j.RootElement.TryGetProperty("error_description", out var d) ? d.GetString() ?? "" : "";
                var codes = j.RootElement.TryGetProperty("error_codes", out var c) && c.ValueKind == JsonValueKind.Array
                    ? string.Join(",", c.EnumerateArray().Select(x => x.GetInt32()))
                    : "";
                return (err, desc, codes);
            }
            catch
            {
                return ("", "", "");
            }
        }

        private static async Task<string> SafeReadAsync(HttpResponseMessage r)
        {
            try { return await r.Content.ReadAsStringAsync(); }
            catch { return string.Empty; }
        }

        private static string MapAadError(string err, string codes)
        {
            if (codes.Contains("50076")) return "AAD_MFA_REQUIRED";
            if (codes.Contains("50057")) return "AAD_ACCOUNT_DISABLED";
            if (codes.Contains("50053")) return "AAD_ACCOUNT_LOCKED";
            if (codes.Contains("700016")) return "AAD_APP_NOT_FOUND";
            if (codes.Contains("7000218")) return "AAD_INVALID_CREDENTIALS";
            if (codes.Contains("500011")) return "AAD_INVALID_RESOURCE";
            return string.IsNullOrEmpty(err) ? "AAD_UNAUTHORIZED" : $"AAD_{err.ToUpperInvariant()}";
        }

        private static string Truncate(string s, int max) =>
            string.IsNullOrEmpty(s) ? "" : (s.Length <= max ? s : s[..max] + "...");
    }
}
