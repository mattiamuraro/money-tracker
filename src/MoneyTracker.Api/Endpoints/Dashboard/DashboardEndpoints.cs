using Microsoft.AspNetCore.Mvc;
using MoneyTracker.Api.Endpoints.Dashboard.Contracts;
using MoneyTracker.Api.ExtensionMethods;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.Dashboard.GetDashboardSummary;

namespace MoneyTracker.Api.Endpoints.Dashboard;

public static class DashboardEndpoints
{
    internal static WebApplication AddDashboardApis(this WebApplication app)
    {
        var group = app.MapApiGroup(ApiRoutes.Dashboard, "Dashboard")
            .RequireReadAccess();

        group.MapGet("/summary", static async (
                [FromServices] IHandler<GetDashboardSummaryQuery, DashboardSummaryDto> handler,
                [AsParameters] DashboardSummaryQuery query,
                CancellationToken cancellationToken) =>
            {
                var (year, month) = query.GetRequiredYearMonth();
                var result = await handler.Handle(new GetDashboardSummaryQuery
                {
                    Year = year,
                    Month = month,
                }, cancellationToken);

                var response = new DashboardSummaryResponse
                {
                    PaymentsCount = result.PaymentsCount,
                    PaymentTotal = result.PaymentTotal,
                    LatestPaymentDescription = result.LatestPaymentDescription,
                    LatestPaymentDate = result.LatestPaymentDate,
                    ForecastIncomeTotal = result.ForecastIncomeTotal,
                    ForecastExpenseTotal = result.ForecastExpenseTotal,
                    ForecastBalance = result.ForecastBalance,
                    NextUpcomingExpenseDescription = result.NextUpcomingExpenseDescription,
                };

                return Results.Ok(response);
            })
            .WithName("GetDashboardSummary")
            .WithDescription("Retrieves aggregated dashboard metrics for the specified month")
            .Produces<DashboardSummaryResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return app;
    }
}
