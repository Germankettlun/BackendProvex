using System.Text.Json.Serialization;

namespace ProvexApi.Models.ControlTower
{
    public sealed class ControlTowerLoginRequest
    {
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
    }
}