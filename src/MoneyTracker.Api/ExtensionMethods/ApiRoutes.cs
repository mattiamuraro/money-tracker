namespace MoneyTracker.Api.ExtensionMethods;

internal static class ApiRoutes
{
    internal const string VersionPrefix = "/api/v1";
    internal const string Auth = "auth";
    internal const string Payments = "payments";
    internal const string Incomes = "incomes";
    internal const string PaymentCategories = "categories";
    internal const string ForecastRecurrenceRuleTypes = "forecast-recurrence-rule-types";
    internal const string ForecastIncomes = "forecast-incomes";
    internal const string ForecastIncomeDefinitions = "forecast-incomes/definitions";
    internal const string ForecastExpenses = "forecast-expenses";
    internal const string ForecastExpenseDefinitions = "forecast-expenses/definitions";

    internal static string CreateGroupPath(string routeSegment)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(routeSegment);
        return $"{VersionPrefix}/{routeSegment.Trim('/')}";
    }

    internal static string CreateResourcePath(string routeSegment, Guid id)
        => $"{CreateGroupPath(routeSegment).TrimEnd('/')}/{id}";
}
