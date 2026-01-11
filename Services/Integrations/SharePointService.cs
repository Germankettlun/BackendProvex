using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace ProvexApi.Services.Integrations
{
    public sealed class UploadResult
    {
        public bool Success { get; init; }
        public string Message { get; init; } = "";
        public string SiteId { get; init; } = "";
        public string DriveId { get; init; } = "";
        public string Folder { get; init; } = "";
        public string FileName { get; init; } = "";
        public long? Size { get; init; }
        public string WebUrl { get; init; } = "";
        public string? LocalPath { get; set; }
        public DateTimeOffset UploadedAt { get; init; } = DateTimeOffset.UtcNow;

        public static UploadResult Ok(string siteId, string driveId, string folder, string fileName, long? size, string webUrl)
            => new() { Success = true, Message = "OK", SiteId = siteId, DriveId = driveId, Folder = folder, FileName = fileName, Size = size, WebUrl = webUrl };
        public static UploadResult Fail(string message) => new() { Success = false, Message = message };
    }

    public interface ISharePointService
    {
        Task<UploadResult> UploadAsync(System.IO.Stream file, string fileName, string empresa, string temporada, CancellationToken ct);
    }

    public sealed class SharePointService : ISharePointService
    {
        private readonly IConfiguration _cfg;
        private readonly HttpClient _http;
        private readonly ILogger<SharePointService> _log;

        public SharePointService(IConfiguration cfg, IHttpClientFactory httpFactory, ILogger<SharePointService> log)
        {
            _cfg = cfg;
            _http = httpFactory.CreateClient("graph");
            _http.BaseAddress = new Uri(_cfg["SharePoint:Url"] ?? "https://graph.microsoft.com/v1.0/");
            _http.Timeout = TimeSpan.FromMinutes(5);
            _log = log;
        }

        public async Task<UploadResult> UploadAsync(System.IO.Stream file, string fileName, string empresa, string temporada, CancellationToken ct)
        {
            try
            {
                if (!_cfg.GetValue<bool>("SharePoint:Enabled"))
                    return UploadResult.Fail("SharePoint deshabilitado.");

                var tenant = _cfg["SharePoint:TenantId"]!;
                var client = _cfg["SharePoint:ClientId"]!;
                var secret = _cfg["SharePoint:ClientSecret"]!;
                var scope = _cfg["SharePoint:getToken"] ?? "https://graph.microsoft.com/.default";
                var siteId = _cfg["SharePoint:SiteId"]!;
                var driveId = _cfg["SharePoint:DriveId"]!;
                var baseFolder = (_cfg["SharePoint:FolderPath"] ?? "").Replace("\\", "/").Trim('/');

                _log.LogInformation("SP: token app-only. Tenant={Tenant} Client={Client} Scope={Scope}", tenant, client, scope);
                var app = ConfidentialClientApplicationBuilder.Create(client).WithClientSecret(secret).WithTenantId(tenant).Build();
                var token = await app.AcquireTokenForClient(new[] { scope }).ExecuteAsync(ct);
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

                var subFolder = $"Temporada {temporada}";
                var finalFolder = Sanitize(string.IsNullOrEmpty(baseFolder) ? subFolder : $"{baseFolder}/{subFolder}");

                _log.LogInformation("SP: destino Site={Site} Drive={Drive} Folder={Folder} File={File} Size={Size}",
                    siteId, driveId, finalFolder, fileName, file.Length);

                if (!string.IsNullOrEmpty(finalFolder))
                    await EnsureFolderPathAsync(siteId, driveId, finalFolder, ct);

                // 1) intenta subir con REPLACE
                var attempt = await UploadOnceAsync(siteId, driveId, finalFolder, fileName, file, replace: true, ct);
                if (attempt.Success) return attempt;

                // 2) si el fallo fue por recurso bloqueado o conflicto, versiona nombre: _V2, _V3, ...
                if (attempt.Message.Contains("423") || attempt.Message.Contains("resourceLocked", StringComparison.OrdinalIgnoreCase) ||
                    attempt.Message.Contains("409"))
                {
                    var alt = await GetAvailableVersionNameAsync(siteId, driveId, finalFolder, fileName, 99, ct);
                    _log.LogWarning("SP: recurso bloqueado o en conflicto. Reintento con nombre alternativo {Alt}", alt);
                    file.Position = 0;
                    return await UploadOnceAsync(siteId, driveId, finalFolder, alt, file, replace: false, ct);
                }

                return attempt;
            }
            catch (MsalServiceException ex)
            {
                _log.LogError(ex, "SP: error MSAL servicio");
                return UploadResult.Fail($"MSAL_SERVICE_ERROR: {ex.ErrorCode}");
            }
            catch (MsalClientException ex)
            {
                _log.LogError(ex, "SP: error MSAL cliente");
                return UploadResult.Fail($"MSAL_CLIENT_ERROR: {ex.ErrorCode}");
            }
            catch (OperationCanceledException)
            {
                _log.LogWarning("SP: operación cancelada");
                return UploadResult.Fail("CANCELLED");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "SP: excepción no controlada");
                return UploadResult.Fail("UNHANDLED");
            }
        }

        // ---------- core upload with replace/fallback ----------
        private async Task<UploadResult> UploadOnceAsync(string siteId, string driveId, string folder, string name, System.IO.Stream file, bool replace, CancellationToken ct)
        {
            var encFolder = string.Join('/', folder.Split('/', StringSplitOptions.RemoveEmptyEntries).Select(Uri.EscapeDataString));
            var encName = Uri.EscapeDataString(name);

            // Small file
            if (file.Length <= 4 * 1024 * 1024)
            {
                file.Position = 0;
                var url = string.IsNullOrEmpty(encFolder)
                    ? $"sites/{siteId}/drives/{driveId}/root:/{encName}:/content"
                    : $"sites/{siteId}/drives/{driveId}/root:/{encFolder}/{encName}:/content";

                using var req = new HttpRequestMessage(HttpMethod.Put, url) { Content = new StreamContent(file) };
                req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                if (!replace) req.Headers.TryAddWithoutValidation("If-None-Match", "*"); // 412 si existe

                var r = await _http.SendAsync(req, ct);
                var body = await SafeReadAsync(r, ct);
                var code = (int)r.StatusCode;

                if (r.IsSuccessStatusCode)
                {
                    var root = JsonDocument.Parse(body).RootElement;
                    var webUrl = root.TryGetProperty("webUrl", out var w) ? w.GetString() ?? "" : "";
                    var size = root.TryGetProperty("size", out var sz) ? sz.GetInt64() : (long?)null;
                    _log.LogInformation("SP: upload simple OK. WebUrl={Url}", webUrl);
                    return UploadResult.Ok(siteId, driveId, folder, name, size, webUrl);
                }

                _log.LogError("SP: upload simple falló. Status={Status} Body={Body}", code, Truncate(body, 800));
                return UploadResult.Fail($"simple:{code}:{body}");
            }

            // Large file with upload session
            var sessionUrl = await CreateUploadSessionAsync(siteId, driveId, folder, name, replace, ct);
            const int chunk = 5 * 1024 * 1024;
            long sent = 0, total = file.Length;
            file.Position = 0;

            while (sent < total)
            {
                var size = (int)Math.Min(chunk, total - sent);
                var buffer = new byte[size];
                var read = await file.ReadAsync(buffer, 0, size, ct);

                using var part = new ByteArrayContent(buffer, 0, read);
                part.Headers.ContentRange = new ContentRangeHeaderValue(sent, sent + read - 1, total);
                part.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                var req = new HttpRequestMessage(HttpMethod.Put, sessionUrl) { Content = part };
                var resp = await _http.SendAsync(req, ct);
                var code = (int)resp.StatusCode;

                if (code is 200 or 201)
                {
                    var finish = await SafeReadAsync(resp, ct);
                    var root = JsonDocument.Parse(finish).RootElement;
                    var webUrl = root.TryGetProperty("webUrl", out var w) ? w.GetString() ?? "" : "";
                    var fsize = root.TryGetProperty("size", out var sz) ? sz.GetInt64() : (long?)null;
                    _log.LogInformation("SP: upload resumible OK. WebUrl={Url}", webUrl);
                    return UploadResult.Ok(siteId, driveId, folder, name, fsize, webUrl);
                }

                if (code == 202)
                {
                    sent += read;
                    _log.LogDebug("SP: progreso {Sent}/{Total}", sent, total);
                    continue;
                }

                // 423 locked, 409 conflict, otros errores
                var err = await SafeReadAsync(resp, ct);
                _log.LogError("SP: chunk falló. Range={From}-{To}/{Total} Status={Status} Body={Body}",
                    sent, sent + read - 1, total, code, Truncate(err, 800));
                return UploadResult.Fail($"chunk:{code}:{err}");
            }

            _log.LogWarning("SP: upload inconcluso sin 200/201 final");
            return UploadResult.Fail("incomplete");
        }

        // ---------- helpers ----------
        private async Task<bool> ExistsAsync(string siteId, string driveId, string folder, string name, CancellationToken ct)
        {
            var encFolder = string.Join('/', folder.Split('/', StringSplitOptions.RemoveEmptyEntries).Select(Uri.EscapeDataString));
            var encName = Uri.EscapeDataString(name);
            var url = string.IsNullOrEmpty(encFolder)
                ? $"sites/{siteId}/drives/{driveId}/root:/{encName}"
                : $"sites/{siteId}/drives/{driveId}/root:/{encFolder}/{encName}";
            var res = await _http.GetAsync(url, ct);
            return res.IsSuccessStatusCode;
        }

        private async Task<string> GetAvailableVersionNameAsync(string siteId, string driveId, string folder, string originalName, int maxN, CancellationToken ct)
        {
            var dot = originalName.LastIndexOf('.');
            var baseName = dot > 0 ? originalName[..dot] : originalName;
            var ext = dot > 0 ? originalName[dot..] : "";

            // si ya viene con _Vn, calcula el siguiente
            var m = Regex.Match(baseName, @"^(?<stem>.+)_V(?<n>\d{1,3})$", RegexOptions.IgnoreCase);
            var stem = m.Success ? m.Groups["stem"].Value : baseName;
            var start = m.Success ? int.Parse(m.Groups["n"].Value) + 1 : 2;

            for (int n = start; n <= maxN; n++)
            {
                var candidate = $"{stem}_V{n}{ext}";
                if (!await ExistsAsync(siteId, driveId, folder, candidate, ct))
                    return candidate;
            }
            // último recurso: timestamp
            return $"{stem}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";
        }

        private async Task<string> CreateUploadSessionAsync(string siteId, string driveId, string folder, string name, bool replace, CancellationToken ct)
        {
            var encFolder = string.Join('/', folder.Split('/', StringSplitOptions.RemoveEmptyEntries).Select(Uri.EscapeDataString));
            var encName = Uri.EscapeDataString(name);
            var path = string.IsNullOrEmpty(encFolder)
                ? $"sites/{siteId}/drives/{driveId}/root:/{encName}:/createUploadSession"
                : $"sites/{siteId}/drives/{driveId}/root:/{encFolder}/{encName}:/createUploadSession";

            var bodyObj = new Dictionary<string, object?>
            {
                ["item"] = new Dictionary<string, object?>
                {
                    ["@microsoft.graph.conflictBehavior"] = replace ? "replace" : "rename",
                    ["name"] = name
                }
            };
            var body = JsonSerializer.Serialize(bodyObj);

            using var req = new HttpRequestMessage(HttpMethod.Post, path)
            { Content = new StringContent(body, Encoding.UTF8, "application/json") };

            var r = await _http.SendAsync(req, ct);
            var txt = await SafeReadAsync(r, ct);
            r.EnsureSuccessStatusCode();

            var uploadUrl = JsonDocument.Parse(txt).RootElement.GetProperty("uploadUrl").GetString()!;
            _log.LogInformation("SP: sesión creada");
            return uploadUrl;
        }

        private static string Sanitize(string path)
        {
            var p = path.Replace('\\', '/').Trim('/');
            p = Regex.Replace(p, @"[:*?""<>|#%&{}~\\]", " ");
            p = Regex.Replace(p, @"\s+", " ").Trim();
            return p.Replace(" /", "/").Replace("/ ", "/");
        }

        private async Task EnsureFolderPathAsync(string siteId, string driveId, string path, CancellationToken ct)
        {
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var current = "";

            for (int i = 0; i < segments.Length; i++)
            {
                var seg = segments[i];
                var encSeg = Uri.EscapeDataString(seg);

                var probeUrl = string.IsNullOrEmpty(current)
                    ? $"sites/{siteId}/drives/{driveId}/root:/{encSeg}"
                    : $"sites/{siteId}/drives/{driveId}/root:/{current}/{encSeg}";

                var probe = await _http.GetAsync(probeUrl, ct);
                if (probe.IsSuccessStatusCode)
                {
                    current = string.IsNullOrEmpty(current) ? encSeg : $"{current}/{encSeg}";
                    continue;
                }

                var parentChildren = string.IsNullOrEmpty(current)
                    ? $"sites/{siteId}/drives/{driveId}/root/children"
                    : $"sites/{siteId}/drives/{driveId}/root:/{current}:/children";

                var payloadObj = new Dictionary<string, object?>
                {
                    ["name"] = seg,
                    ["folder"] = new { },
                    ["@microsoft.graph.conflictBehavior"] = "replace"
                };
                var payload = JsonSerializer.Serialize(payloadObj);

                using var req = new HttpRequestMessage(HttpMethod.Post, parentChildren)
                { Content = new StringContent(payload, Encoding.UTF8, "application/json") };

                var res = await _http.SendAsync(req, ct);
                var body = await SafeReadAsync(res, ct);
                var code = (int)res.StatusCode;

                if (code is 200 or 201 or 409)
                {
                    current = string.IsNullOrEmpty(current) ? encSeg : $"{current}/{encSeg}";
                    continue;
                }

                _log.LogError("SP: crear carpeta falló. Path={Path} Status={Status} Body={Body}",
                              string.Join('/', segments.Take(i + 1)), code, Truncate(body, 800));
                throw new HttpRequestException($"Create folder '{string.Join('/', segments.Take(i + 1))}' failed: {code} {res.ReasonPhrase}. {body}");
            }
        }

        private static async Task<string> SafeReadAsync(HttpResponseMessage r, CancellationToken ct)
        {
            try { return await r.Content.ReadAsStringAsync(ct); }
            catch { return string.Empty; }
        }

        private static string Truncate(string s, int max) =>
            string.IsNullOrEmpty(s) ? "" : (s.Length <= max ? s : s[..max] + "...");
    }
}
