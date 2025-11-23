using System.Text.Json.Serialization;

namespace ProvexApi.Models.SensiWatch
{
    /// <summary>
    /// Timestamp con información del dispositivo y servidor
    /// </summary>
    public class SensiWatchTimestamp
    {
        [JsonPropertyName("deviceTime")]
        public long DeviceTime { get; set; }

        [JsonPropertyName("receiveTime")]
        public long ReceiveTime { get; set; }

        /// <summary>
        /// Convierte el timestamp Unix (ms) del dispositivo a DateTime UTC
        /// </summary>
        public DateTime DeviceDateTime => DateTimeOffset.FromUnixTimeMilliseconds(DeviceTime).UtcDateTime;

        /// <summary>
        /// Convierte el timestamp Unix (ms) de recepción a DateTime UTC
        /// </summary>
        public DateTime ReceiveDateTime => DateTimeOffset.FromUnixTimeMilliseconds(ReceiveTime).UtcDateTime;
    }
}