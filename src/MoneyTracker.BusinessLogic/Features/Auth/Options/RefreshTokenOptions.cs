namespace MoneyTracker.BusinessLogic.Features.Auth.Options;

public class RefreshTokenOptions
{
    public const string SectionName = "Auth:RefreshToken";

    public int ExpiryDays { get; set; } = 14;
}
