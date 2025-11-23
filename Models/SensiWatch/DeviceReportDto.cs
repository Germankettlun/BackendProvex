using System.Text.Json.Serialization;

namespace ProvexApi.Models.SensiWatch
{
    /// <summary>
    /// DTO para recibir reportes de datos desde SensiWatch
    /// </summary>
    public class DeviceReportDto
    {
        [JsonPropertyName("deviceIdentity")]
        public DeviceIdentity? DeviceIdentity { get; set; }

        [JsonPropertyName("locations")]
        public List<DeviceLocation>? Locations { get; set; }

        [JsonPropertyName("sensors")]
        public List<SensorReading>? Sensors { get; set; }

        [JsonPropertyName("swpTrip")]
        public SwpTripInfo? SwpTrip { get; set; }

        /// <summary>
        /// Obtiene las lecturas de temperatura
        /// </summary>
        public IEnumerable<SensorReading> TemperatureReadings =>
            Sensors?.Where(s => s.IsTemperature) ?? Enumerable.Empty<SensorReading>();

        /// <summary>
        /// Obtiene las lecturas de humedad
        /// </summary>
        public IEnumerable<SensorReading> HumidityReadings =>
            Sensors?.Where(s => s.IsHumidity) ?? Enumerable.Empty<SensorReading>();

        /// <summary>
        /// Obtiene la última ubicación reportada
        /// </summary>
        public DeviceLocation? LastLocation =>
            Locations?.OrderByDescending(l => l.Timestamp?.DeviceTime).FirstOrDefault();

        /// <summary>
        /// Obtiene el serial del dispositivo
        /// </summary>
        public string? DeviceSerial => DeviceIdentity?.SensitechSerialNumber;

        /// <summary>
        /// Obtiene el ID interno del trip
        /// </summary>
        public string? InternalTripId => SwpTrip?.InternalTripId;
    }
}