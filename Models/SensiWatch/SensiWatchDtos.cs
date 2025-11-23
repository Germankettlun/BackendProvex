namespace ProvexApi.Models.SensiWatch
{
    /// <summary>
    /// DTO para lecturas de temperatura
    /// </summary>
    public class TemperatureReadingDto
    {
        public long ReadingId { get; set; }
        public string DeviceSerial { get; set; } = string.Empty;
        public string? InternalTripId { get; set; }
        public double Temperature { get; set; }
        public DateTime DeviceTime { get; set; }
        public DateTime ReceiveTime { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? LocationAddress { get; set; }
    }

    /// <summary>
    /// DTO para resumen de trip
    /// </summary>
    public class TripSummaryDto
    {
        public string InternalTripId { get; set; } = string.Empty;
        public string? TrailerId { get; set; }
        public string? DeviceSerial { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int TotalReadings { get; set; }
        public double? MinTemperature { get; set; }
        public double? MaxTemperature { get; set; }
        public double? AvgTemperature { get; set; }
        public List<LocationPointDto> Route { get; set; } = new();
    }

    /// <summary>
    /// DTO para puntos de ubicación
    /// </summary>
    public class LocationPointDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Address { get; set; }
    }

    /// <summary>
    /// DTO para estado de dispositivos
    /// </summary>
    public class DeviceStatusDto
    {
        public string SerialNumber { get; set; } = string.Empty;
        public string? DeviceName { get; set; }
        public DateTime? LastSeen { get; set; }
        public string Status { get; set; } = "Unknown";
        public string? CurrentTripId { get; set; }
        public double? LastTemperature { get; set; }
        public double? BatteryLevel { get; set; }
    }
}