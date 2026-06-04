namespace MoneyTracker.Data;

public class UserPasswordHistory
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
