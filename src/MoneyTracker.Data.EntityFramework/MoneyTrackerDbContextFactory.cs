using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MoneyTracker.Data.EntityFramework;

/// <summary>
/// Design-time factory used by EF Core tools (dotnet ef migrations add, etc.)
/// when running outside of the Aspire host where no real connection string is available.
/// </summary>
internal sealed class MoneyTrackerDbContextFactory : IDesignTimeDbContextFactory<MoneyTrackerDbContext>
{
    public MoneyTrackerDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseSqlServer("Server=localhost;Database=MoneyTrackerDesignTime;Trusted_Connection=True;")
            .Options;

        return new MoneyTrackerDbContext(options);
    }
}
