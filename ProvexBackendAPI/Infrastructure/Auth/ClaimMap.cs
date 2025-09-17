namespace ProvexBackendAPI.Infrastructure.Auth
{
    public record ClaimMap<T>(string Type, Func<T, string?> Selector, bool SkipIfNullOrEmpty = true);
    public class ClaimMap
    {
        public static ClaimMap<T> For<T>(string claimType, Func<T, string?> selector, bool skipIfNullOrEmpty = true)
       => new(claimType, selector, skipIfNullOrEmpty);
    }
}
