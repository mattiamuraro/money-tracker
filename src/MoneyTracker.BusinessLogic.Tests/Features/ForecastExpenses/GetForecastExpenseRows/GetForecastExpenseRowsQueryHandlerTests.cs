using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetForecastExpenseRows;
using MoneyTracker.BusinessLogic.Tests.TestUtilities;
using MoneyTracker.Data;

namespace MoneyTracker.BusinessLogic.Tests.Features.ForecastExpenses.GetForecastExpenseRows;

public class GetForecastExpenseRowsQueryHandlerTests
{
    [Fact]
    public async Task Handle_EndBeforeStart_ShouldThrowBadRequestException()
    {
        using var db = InMemoryDbContextFactory.Create();
        var sut = new GetForecastExpenseRowsQueryHandler(db);

        var start = new DateOnly(2026, 2, 1);
        var end = new DateOnly(2026, 1, 1);

        await Assert.ThrowsAsync<BadRequestException>(() => sut.Handle(new GetForecastExpenseRowsQuery(start, end), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Handle_RangeOver366Days_ShouldThrowBadRequestException()
    {
        using var db = InMemoryDbContextFactory.Create();
        var sut = new GetForecastExpenseRowsQueryHandler(db);

        var start = new DateOnly(2026, 1, 1);
        var end = start.AddDays(367);

        await Assert.ThrowsAsync<BadRequestException>(() => sut.Handle(new GetForecastExpenseRowsQuery(start, end), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Handle_ValidRange_ShouldReturnOnlyPendingExpenses_WithCategoryMapping()
    {
        using var db = InMemoryDbContextFactory.Create();
        var categoryId = Guid.NewGuid();
        db.PaymentCategories.Add(new PaymentCategoryBuilder().WithId(categoryId).WithName("Home").WithCode("HOME").Build());

        var inRangeDate = new DateOnly(2026, 1, 15);
        db.ForecastOccurrences.AddRange(
            new ForecastOccurrenceBuilder()
                .WithForecastDefinitionId(Guid.NewGuid())
                .WithIsIncome(false)
                .WithDescription("rent")
                .WithAmount(800)
                .WithExpectedDate(inRangeDate)
                .WithPaymentCategoryId(categoryId)
                .WithStatus(ForecastOccurrenceStatus.PendingId)
                .Build(),
            new ForecastOccurrenceBuilder()
                .WithForecastDefinitionId(Guid.NewGuid())
                .WithIsIncome(false)
                .WithDescription("no category")
                .WithAmount(20)
                .WithExpectedDate(inRangeDate.AddDays(1))
                .WithPaymentCategoryId(null)
                .WithStatus(ForecastOccurrenceStatus.PendingId)
                .Build(),
            new ForecastOccurrenceBuilder()
                .WithForecastDefinitionId(Guid.NewGuid())
                .WithIsIncome(true)
                .WithDescription("salary")
                .WithAmount(3000)
                .WithExpectedDate(inRangeDate)
                .WithStatus(ForecastOccurrenceStatus.PendingId)
                .Build(),
            new ForecastOccurrenceBuilder()
                .WithForecastDefinitionId(Guid.NewGuid())
                .WithIsIncome(false)
                .WithDescription("confirmed")
                .WithAmount(10)
                .WithExpectedDate(inRangeDate)
                .WithStatus(ForecastOccurrenceStatus.ConfirmedId)
                .Build());

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var sut = new GetForecastExpenseRowsQueryHandler(db);
        var result = await sut.Handle(new GetForecastExpenseRowsQuery(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31)), TestContext.Current.CancellationToken);

        Assert.Equal(2, result.Count);

        var rent = Assert.Single(result.Where(x => x.Description == "rent"));
        Assert.Equal(800, rent.Amount);
        Assert.Equal("Home", rent.Category);
        Assert.Equal(inRangeDate, rent.Date);

        var withoutCategory = Assert.Single(result.Where(x => x.Description == "no category"));
        Assert.Equal(20, withoutCategory.Amount);
        Assert.Null(withoutCategory.Category);
        Assert.Equal(inRangeDate.AddDays(1), withoutCategory.Date);
    }
}
