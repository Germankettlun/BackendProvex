using System.Text.Json.Serialization;

namespace ProvexApi.Models.SensiWatch
{
    /// <summary>
    /// Información del trip/viaje en SensiWatch
    /// </summary>
    public class SwpTripInfo
    {
        [JsonPropertyName("tripId")]
        public int TripId { get; set; }

        [JsonPropertyName("tripGuid")]
        public Guid TripGuid { get; set; }

        [JsonPropertyName("internalTripId")]
        public string? InternalTripId { get; set; }

        [JsonPropertyName("trailerId")]
        public string? TrailerId { get; set; }

        [JsonPropertyName("destinations")]
        public List<TripDestination>? Destinations { get; set; }
    }
}