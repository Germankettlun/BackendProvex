using System.Text.Json.Serialization;

namespace ProvexApi.Models.SensiWatch
{
    /// <summary>
    /// Ubicación geográfica reportada por el dispositivo
    /// </summary>
    public class DeviceLocation
    {
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("timestamp")]
        public SensiWatchTimestamp? Timestamp { get; set; }
    }
}