namespace MoneyTracker.BusinessLogic.Features.Auth.Login;

public class LoginCommand
{
    internal const string AuthFailedMessage = "Authentication failed.";

    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
