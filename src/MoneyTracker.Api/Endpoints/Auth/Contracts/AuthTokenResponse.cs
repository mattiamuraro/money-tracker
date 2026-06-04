namespace MoneyTracker.Api.Endpoints.Auth.Contracts;

public class AuthTokenResponse
{
    public required string Token { get; set; }
    public required string RefreshToken { get; set; }
}
