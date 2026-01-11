using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProvexApi.Models.ControlTower
{
    public sealed class ControlTowerLoginResponse
    {
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;

        [JsonPropertyName("refresh")]
        public string Refresh { get; set; } = string.Empty;

        [JsonPropertyName("contact")]
        public JsonElement? Contact { get; set; }

        [JsonPropertyName("settings")]
        public JsonElement? Settings { get; set; }
    }
}
