using System;

namespace ProvexApi.Services.ControlTower
{
    public sealed class ControlTowerApiOptions
    {
        public string BaseUrl { get; set; } = "https://pre-back.hispatectrack.com";
        public int TimeoutSeconds { get; set; } = 30;
        public string? DefaultUsername { get; set; }
        public string? DefaultPassword { get; set; }

        public Uri BaseUri => new(BaseUrl.EndsWith('/') ? BaseUrl : BaseUrl + "/");
    }
}