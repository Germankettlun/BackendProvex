using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;


// Reemplaza estos namespaces con las ubicaciones correctas.
using ProvexApi.Data.DTOs.ASOEX; // <-- IMPORTANTE: Usamos las clases DTO originales.

// Reemplaza este namespace con uno apropiado para tus helpers
namespace ProvexApi.Helpers
{
    // *** CORRECCIÓN: Se han ELIMINADO las definiciones duplicadas de las clases DTO Paged de este archivo. ***

    public static class AsoexApiHelper
    {
        // Clase interna para mantener la configuración de una API específica
        public class AsoexApiConfig
        {
            public string User { get; set; }
            public string Password { get; set; }
            public int TimeOutMinutes { get; set; }
        }

        // Método para leer la configuración desde appsettings.json
        private static AsoexApiConfig GetConfig(IConfiguration configuration, string apiName)
        {
            string? apiAuthUser = configuration["Asoex:User"];
            string? apiAuthPassword = configuration["Asoex:Pass"];
            string? timeOut = configuration[$"Asoex:Apis:{apiName}:timeOutMin"];

            if (string.IsNullOrWhiteSpace(apiAuthUser) || string.IsNullOrWhiteSpace(apiAuthPassword))
            {
                throw new InvalidOperationException("Falta información de autenticación (User/Pass) para ASOEX en la configuración.");
            }

            return new AsoexApiConfig
            {
                User = apiAuthUser,
                Password = apiAuthPassword,
                TimeOutMinutes = int.TryParse(timeOut, out var t) && t > 0 ? t : 5, // Default de 5 minutos si no se especifica o es inválido
            };
        }

        /// <summary>
        /// Realiza una llamada GET a un endpoint de ASOEX, manejando la paginación y aplicando un timeout específico.
        /// </summary>
        public static async Task<List<T>> ObtenerDatosPaginadosAsync<T, TPage>(
         IConfiguration configuration,
         string apiName,
         string urlBase,
         ILogger logger,
         // *** MEJORA: Se añade un parámetro opcional para reportar el progreso. ***
         Action<string>? progressReporter = null) where TPage : IApiPagedResponse<T>
        {
            var config = GetConfig(configuration, apiName);
            var listaCompleta = new List<T>();
            int page = 1;
            bool hayMasPaginas;

            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromMinutes(config.TimeOutMinutes);

                // *** CORRECCIÓN: Se usa Encoding.UTF8 para la autenticación Bearer/Base64. ***
                var creds = Encoding.UTF8.GetBytes($"{config.User}:{config.Password}");
                // La documentación de la API especifica "Bearer", no "Basic".
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Convert.ToBase64String(creds));

                do
                {
                    var url = $"{urlBase}{(urlBase.Contains("?") ? "&" : "?")}page={page}";
                    try
                    {
                        var response = await httpClient.GetAsync(url);
                        if (!response.IsSuccessStatusCode)
                        {
                            var errorContent = await response.Content.ReadAsStringAsync();
                            logger.LogError("Error en la API de ASOEX. Status: {StatusCode}, URL: {Url}, Respuesta: {ErrorContent}", response.StatusCode, url, errorContent);
                            throw new HttpRequestException($"Error en la API de ASOEX: {response.StatusCode}");
                        }

                        var json = await response.Content.ReadAsStringAsync();
                        var resultadoPaginado = JsonSerializer.Deserialize<TPage>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        int recordCount = 0;
                        if (resultadoPaginado?.data != null && resultadoPaginado.data.Any())
                        {
                            listaCompleta.AddRange(resultadoPaginado.data);
                            recordCount = resultadoPaginado.data.Count;
                        }

                        progressReporter?.Invoke($"[PAGINATION] Contexto '{apiName}': Página {page} de {resultadoPaginado?.last_page ?? 1} obtenida con {recordCount} registros.");

                        hayMasPaginas = resultadoPaginado != null && resultadoPaginado.current_page < resultadoPaginado.last_page;
                        if (hayMasPaginas) { page++; }
                    }
                    catch (TaskCanceledException ex) // Captura específicamente los timeouts
                    {
                        logger.LogError(ex, "[TIMEOUT-ERROR] La petición para la API '{ApiName}' superó el timeout configurado de {TimeoutMinutes} minutos. URL: {Url}", apiName, config.TimeOutMinutes, url);
                        throw new TimeoutException($"La descarga de '{apiName}' excedió el límite de tiempo.", ex);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "[HTTP-ERROR] Falló la descarga para la API '{ApiName}' en la página {Page}. URL: {Url}", apiName, page, url);
                        throw;
                    }

                } while (hayMasPaginas);
            }

            return listaCompleta;
        }

        public static async Task<List<T>> ObtenerDatosPaginadosConNumeroPaginaAsync<T, TPage>(
    IConfiguration configuration,
    string apiName,
    string urlBase,
    ILogger logger,
    Action<string>? progressReporter = null)
    where T : class
    where TPage : IApiPagedResponse<T>
        {
            var config = GetConfig(configuration, apiName);
            var listaCompleta = new List<T>();
            int page = 1;
            bool hayMasPaginas;

            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromMinutes(config.TimeOutMinutes);

                var creds = Encoding.UTF8.GetBytes($"{config.User}:{config.Password}");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Convert.ToBase64String(creds));

                do
                {
                    var url = $"{urlBase}{(urlBase.Contains("?") ? "&" : "?")}page={page}";
                    try
                    {
                        var response = await httpClient.GetAsync(url);
                        if (!response.IsSuccessStatusCode)
                        {
                            var errorContent = await response.Content.ReadAsStringAsync();
                            logger.LogError("Error en la API de ASOEX. Status: {StatusCode}, URL: {Url}, Respuesta: {ErrorContent}", response.StatusCode, url, errorContent);
                            throw new HttpRequestException($"Error en la API de ASOEX: {response.StatusCode}");
                        }

                        var json = await response.Content.ReadAsStringAsync();
                        var resultadoPaginado = JsonSerializer.Deserialize<TPage>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        int recordCount = 0;
                        if (resultadoPaginado?.data != null && resultadoPaginado.data.Any())
                        {
                            foreach (var item in resultadoPaginado.data)
                            {
                                var prop = item.GetType().GetProperty("pagina_api");
                                if (prop != null)
                                    prop.SetValue(item, page);
                                listaCompleta.Add(item);
                            }
                            recordCount = resultadoPaginado.data.Count;
                        }

                        progressReporter?.Invoke($"[PAGINATION] Contexto '{apiName}': Página {page} de {resultadoPaginado?.last_page ?? 1} obtenida con {recordCount} registros.");

                        hayMasPaginas = resultadoPaginado != null && resultadoPaginado.current_page < resultadoPaginado.last_page;
                        if (hayMasPaginas) { page++; }
                    }
                    catch (TaskCanceledException ex)
                    {
                        logger.LogError(ex, "[TIMEOUT-ERROR] La petición para la API '{ApiName}' superó el timeout configurado de {TimeoutMinutes} minutos. URL: {Url}", apiName, config.TimeOutMinutes, url);
                        throw new TimeoutException($"La descarga de '{apiName}' excedió el límite de tiempo.", ex);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "[HTTP-ERROR] Falló la descarga para la API '{ApiName}' en la página {Page}. URL: {Url}", apiName, page, url);
                        throw;
                    }

                } while (hayMasPaginas);
            }

            return listaCompleta;
        }

    }
}