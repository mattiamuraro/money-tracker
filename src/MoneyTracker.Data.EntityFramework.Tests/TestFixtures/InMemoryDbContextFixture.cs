using Microsoft.EntityFrameworkCore;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.Data.EntityFramework.Tests.TestFixtures;

public class InMemoryDbContextFixture
{
    private readonly DbContextOptions<MoneyTrackerDbContext> _dbContextOptions;

    public InMemoryDbContextFixture()
    {
        _dbContextOptions = new DbContextOptionsBuilder<MoneyTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    public MoneyTrackerDbContext CreateDbContext()
    {
        return new MoneyTrackerDbContext(_dbContextOptions);
    }

    public void Dispose()
    {
        using var context = CreateDbContext();
        context.Database.EnsureDeleted();
    }
}
