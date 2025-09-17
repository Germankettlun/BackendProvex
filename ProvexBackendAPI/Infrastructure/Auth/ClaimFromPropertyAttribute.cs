namespace ProvexBackendAPI.Infrastructure.Auth
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ClaimFromPropertyAttribute : Attribute
    {
        public string ClaimType { get; }
        public bool Required { get; init; } = false;

        public ClaimFromPropertyAttribute(string claimType) => ClaimType = claimType;
    }
}
