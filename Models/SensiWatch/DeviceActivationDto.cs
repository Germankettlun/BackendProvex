using System.Text.Json.Serialization;

namespace ProvexApi.Models.SensiWatch
{
    /// <summary>
    /// DTO para recibir mensajes de activación de dispositivos desde SensiWatch
    /// </summary>
    public class DeviceActivationDto
    {
        [JsonPropertyName("deviceIdentity")]
        public DeviceIdentity? DeviceIdentity { get; set; }

        [JsonPropertyName("activationTime")]
        public long ActivationTime { get; set; }

        /// <summary>
        /// Convierte el timestamp Unix (ms) de activación a DateTime UTC
        /// </summary>
        public DateTime ActivationDateTime => DateTimeOffset.FromUnixTimeMilliseconds(ActivationTime).UtcDateTime;
    }
}