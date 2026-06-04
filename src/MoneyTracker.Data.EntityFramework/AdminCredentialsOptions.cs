namespace MoneyTracker.Data.EntityFramework;

public sealed class AdminCredentialsOptions
{
    public const string SectionName = "Auth";

    public string? Username { get; set; }
    public string? Password { get; set; }
}
