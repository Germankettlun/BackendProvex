using System.Text.Json.Serialization;

namespace ProvexApi.Models.SensiWatch
{
    /// <summary>
    /// Lectura individual de un sensor (temperatura, humedad, batería, etc.)
    /// </summary>
    public class SensorReading
    {
        [JsonPropertyName("sensorId")]
        public string? SensorId { get; set; }

        [JsonPropertyName("value")]
        public string? Value { get; set; }

        [JsonPropertyName("timestamp")]
        public SensiWatchTimestamp? Timestamp { get; set; }

        /// <summary>
        /// Convierte el valor del sensor a double si es posible
        /// </summary>
        public double? ValueAsDouble => double.TryParse(Value, out var result) ? result : null;

        /// <summary>
        /// Indica si es una lectura de temperatura
        /// </summary>
        public bool IsTemperature => SensorId?.ToLower() == "temperature";

        /// <summary>
        /// Indica si es una lectura de humedad
        /// </summary>
        public bool IsHumidity => SensorId?.ToLower() == "humidity";

        /// <summary>
        /// Indica si es una lectura de batería
        /// </summary>
        public bool IsBattery => SensorId?.ToLower() == "battery";

        /// <summary>
        /// Indica si es una lectura de luz
        /// </summary>
        public bool IsLight => SensorId?.ToLower() == "light";
    }
}