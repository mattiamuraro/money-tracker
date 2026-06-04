namespace MoneyTracker.BusinessLogic.Features.Auth.Register;

public class RegisterAuthTokenDto
{
    public required string Token { get; set; }
    public required string RefreshToken { get; set; }
}
