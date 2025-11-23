namespace ProvexApi.Models.SensiWatch.API
{
    /// <summary>
    /// Opciones de configuración para la API de SensiWatch
    /// </summary>
    public class SensiWatchApiOptions
    {
        public string BaseUrl { get; set; } = "https://developer.api.sensiwatch.com";
        public string TokenEndpoint { get; set; } = "/external-auth/connect/token";
        public string TripsEndpoint { get; set; } = "/external/trip-api-manage/v1/Programs";
        public string SubscriptionKey { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ClientId { get; set; } = "SWPExternal";
        public string ClientSecret { get; set; } = "SensitechExternal!";
        public string Scope { get; set; } = "apiExternal";
        public int TimeoutSeconds { get; set; } = 30;
        public int ProgramId { get; set; } = 0;
    }

    /// <summary>
    /// Respuesta del token de autenticación de SensiWatch
    /// </summary>
    public class TokenResponse
    {
        public string access_token { get; set; } = string.Empty;
        public string token_type { get; set; } = string.Empty;
        public int expires_in { get; set; }
        public string scope { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request para crear un trip en SensiWatch
    /// </summary>
    public class CreateTripRequest
    {
        public string InternalTripID { get; set; } = string.Empty;
        public int TripTemplateId { get; set; } = 0;
        public bool IsDraft { get; set; } = false;
        public string SerialNumber { get; set; } = string.Empty;
        public TripOrigin Origin { get; set; } = new();
        public List<TripDestinationRequest> Destinations { get; set; } = new();
        public string? TrailerId { get; set; }
        public string? PurchaseOrderNumber { get; set; }
        public string? OrderNumber { get; set; }
    }

    public class TripOrigin
    {
        public int LocationId { get; set; }
    }

    public class TripDestinationRequest
    {
        public int LocationId { get; set; }
        public List<TripProduct> Products { get; set; } = new();
        public string? PoNumber { get; set; }
        public string? OrderNumber { get; set; }
    }

    public class TripProduct
    {
        public int ProductId { get; set; }
    }

    /// <summary>
    /// Respuesta de creación de trip
    /// </summary>
    public class CreateTripResponse
    {
        public int TripId { get; set; }
        public Guid TripGuid { get; set; }
        public string InternalTripId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}