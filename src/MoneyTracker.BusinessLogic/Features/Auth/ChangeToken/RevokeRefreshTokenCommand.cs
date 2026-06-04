namespace MoneyTracker.BusinessLogic.Features.Auth.ChangeToken;

public class RevokeRefreshTokenCommand
{
    public string RefreshToken { get; set; } = string.Empty;
}
