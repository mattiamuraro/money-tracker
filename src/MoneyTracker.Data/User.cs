using MoneyTracker.Data.Base;
using System.ComponentModel.DataAnnotations;

namespace MoneyTracker.Data;

public class User : BaseEntity
{
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public ICollection<UserRefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<UserPasswordHistory> PasswordHistories { get; set; } = [];
}
