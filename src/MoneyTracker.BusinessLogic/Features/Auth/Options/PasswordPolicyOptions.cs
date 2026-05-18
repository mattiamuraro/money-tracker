namespace MoneyTracker.BusinessLogic.Features.Auth.Options;

public class PasswordPolicyOptions
{
    public const string SectionName = "Auth:PasswordPolicy";

    public int MinimumLength { get; set; } = 12;
    public int MaximumLength { get; set; } = 128;
    public int PasswordHistoryCount { get; set; } = 5;
    public string[] BlockedPasswords { get; set; } =
    [
        "password",
        "password123",
        "qwerty",
        "12345678",
        "admin123",
        "letmein"
    ];
}
