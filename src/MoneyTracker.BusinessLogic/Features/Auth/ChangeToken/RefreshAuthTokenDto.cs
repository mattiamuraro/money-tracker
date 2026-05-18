namespace MoneyTracker.BusinessLogic.Features.Auth.ChangeToken;

public class RefreshAuthTokenDto
{
    public required string Token { get; set; }
    public required string RefreshToken { get; set; }
}
