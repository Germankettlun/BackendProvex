using ProvexApi.Models.SensiWatch.API;

namespace ProvexApi.Services.SensiWatch.API
{
    /// <summary>
    /// Interfaz para el cliente de la API de SensiWatch (llamadas salientes)
    /// </summary>
    public interface ISensiWatchApiClient
    {
        /// <summary>
        /// Obtiene un token de acceso de SensiWatch
        /// </summary>
        /// <returns>Token de acceso</returns>
        Task<string> GetAccessTokenAsync();

        /// <summary>
        /// Crea un nuevo trip en SensiWatch
        /// </summary>
        /// <param name="tripRequest">Datos del trip a crear</param>
        /// <returns>Respuesta de creación del trip</returns>
        Task<CreateTripResponse> CreateTripAsync(CreateTripRequest tripRequest);

        /// <summary>
        /// Verifica la conectividad con SensiWatch
        /// </summary>
        /// <returns>True si la conexión es exitosa</returns>
        Task<bool> TestConnectionAsync();
    }
}