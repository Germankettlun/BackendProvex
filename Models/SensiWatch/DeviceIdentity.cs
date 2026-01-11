using System.Text.Json.Serialization;

namespace ProvexApi.Models.SensiWatch
{
    /// <summary>
    /// Información de identidad del dispositivo termógrafo
    /// </summary>
    public class DeviceIdentity
    {
        [JsonPropertyName("imei")]
        public string? Imei { get; set; }

        [JsonPropertyName("platformId")]
        public string? PlatformId { get; set; }

        [JsonPropertyName("deviceName")]
        public string? DeviceName { get; set; }

        [JsonPropertyName("sensitechSerialNumber")]
        public string? SensitechSerialNumber { get; set; }

        [JsonPropertyName("orgUnit")]
        public string? OrgUnit { get; set; }

        [JsonPropertyName("customerShipmentId")]
        public string? CustomerShipmentId { get; set; }

        [JsonPropertyName("deviceReportId")]
        public string? DeviceReportId { get; set; }
    }
}