using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetForecastIncomeRows;
using MoneyTracker.BusinessLogic.Tests.TestUtilities;
using MoneyTracker.Data;

namespace MoneyTracker.BusinessLogic.Tests.Features.ForecastIncomes.GetForecastIncomeRows;

public class GetForecastIncomeRowsQueryHandlerTests
{
    [Fact]
    public async Task Handle_EndBeforeStart_ShouldThrowBadRequestException()
    {
        using var db = InMemoryDbContextFactory.Create();
        var sut = new GetForecastIncomeRowsQueryHandler(db);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            sut.Handle(new GetForecastIncomeRowsQuery(new DateOnly(2026, 2, 1), new DateOnly(2026, 1, 1)), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Handle_RangeOver366Days_ShouldThrowBadRequestException()
    {
        using var db = InMemoryDbContextFactory.Create();
        var sut = new GetForecastIncomeRowsQueryHandler(db);

        var start = new DateOnly(2026, 1, 1);
        await Assert.ThrowsAsync<BadRequestException>(() =>
            sut.Handle(new GetForecastIncomeRowsQuery(start, start.AddDays(367)), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Handle_ValidRange_ShouldReturnOnlyPendingIncomeOccurrences()
    {
        using var db = InMemoryDbContextFactory.Create();
        var date = new DateOnly(2026, 1, 10);
        db.ForecastOccurrences.AddRange(
            new ForecastOccurrenceBuilder()
                .WithForecastDefinitionId(Guid.NewGuid())
                .WithIsIncome(true)
                .WithDescription("salary")
                .WithAmount(3000)
                .WithExpectedDate(date)
                .WithStatus(ForecastOccurrenceStatus.PendingId)
                .Build(),
            new ForecastOccurrenceBuilder()
                .WithForecastDefinitionId(Guid.NewGuid())
                .WithIsIncome(true)
                .WithDescription("confirmed salary")
                .WithAmount(2000)
                .WithExpectedDate(date)
                .WithStatus(ForecastOccurrenceStatus.ConfirmedId)
                .Build(),
            new ForecastOccurrenceBuilder()
                .WithForecastDefinitionId(Guid.NewGuid())
                .WithIsIncome(false)
                .WithDescription("expense")
                .WithAmount(100)
                .WithExpectedDate(date)
                .WithStatus(ForecastOccurrenceStatus.PendingId)
                .Build());

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var sut = new GetForecastIncomeRowsQueryHandler(db);
        var result = await sut.Handle(new GetForecastIncomeRowsQuery(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31)), TestContext.Current.CancellationToken);

        var row = Assert.Single(result);
        Assert.Equal("salary", row.Description);
        Assert.Equal(3000, row.Amount);
        Assert.Equal(date, row.Date);
    }
}
