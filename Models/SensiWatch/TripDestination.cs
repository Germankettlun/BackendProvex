using System.Text.Json.Serialization;

namespace ProvexApi.Models.SensiWatch
{
    /// <summary>
    /// Información del destino en un trip de SensiWatch
    /// </summary>
    public class TripDestination
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("poNumber")]
        public string? PoNumber { get; set; }

        [JsonPropertyName("orderNumber")]
        public string? OrderNumber { get; set; }
    }
}