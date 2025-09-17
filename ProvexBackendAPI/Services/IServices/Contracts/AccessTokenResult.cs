namespace ProvexBackendAPI.Services.IServices.Contracts
{
    public record AccessTokenResult(
    string Token,
    DateTime ExpiresAtUtc,
    long ExpiresAtUnix
);
}
