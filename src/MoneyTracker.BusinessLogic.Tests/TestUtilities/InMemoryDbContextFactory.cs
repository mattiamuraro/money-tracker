using Microsoft.EntityFrameworkCore;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Tests.TestUtilities;

internal static class InMemoryDbContextFactory
{
    internal static MoneyTrackerDbContext Create()
    {
        return new MoneyTrackerDbContext(
            new DbContextOptionsBuilder<MoneyTrackerDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options,
            null);
    }
}
