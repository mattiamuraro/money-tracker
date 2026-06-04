namespace MoneyTracker.BusinessLogic.Features.Auth.Login;

public class LoginAuthTokenDto
{
    public required string Token { get; set; }
    public required string RefreshToken { get; set; }
}
