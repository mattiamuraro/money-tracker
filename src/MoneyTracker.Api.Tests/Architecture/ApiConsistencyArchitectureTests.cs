using System.Reflection;
using Xunit;

namespace MoneyTracker.Api.Tests.Architecture;

public class ApiConsistencyArchitectureTests
{
    private static readonly string ApiProjectRoot = ResolveApiProjectRoot();

    [Fact]
    public void AuthEndpoints_Should_Apply_RequestSizeLimit_Filter_On_All_Auth_Post_Endpoints()
    {
        var content = ReadApiFile("Endpoints/Auth/AuthEndpoints.cs");

        var count = CountOccurrences(content, ".RequireAuthRequestSizeLimit()");

        Assert.Equal(5, count);
    }

    [Fact]
    public void AuthEndpoints_Should_Apply_NoStore_Filter_On_All_Auth_Post_Endpoints()
    {
        var content = ReadApiFile("Endpoints/Auth/AuthEndpoints.cs");

        var count = CountOccurrences(content, ".AddNoStoreResponseHeaders()");

        Assert.Equal(5, count);
    }

    [Fact]
    public void IncomeEndpoints_Should_Not_Read_Idempotency_Header_Directly()
    {
        var content = ReadApiFile("Endpoints/Incomes/IncomeEndpoints.cs");

        Assert.DoesNotContain("Request.Headers[\"X-Idempotency-Key\"]", content, StringComparison.Ordinal);
    }

    [Fact]
    public void PaymentEndpoints_Should_Not_Read_Idempotency_Header_Directly()
    {
        var content = ReadApiFile("Endpoints/Payments/PaymentEndpoints.cs");

        Assert.DoesNotContain("Request.Headers[\"X-Idempotency-Key\"]", content, StringComparison.Ordinal);
    }

    [Fact]
    public void PaymentAndIncomeEndpoints_Should_Use_Shared_CreatedResource_Convention()
    {
        Assert.Contains("ApiEndpointConventions.CreatedResource(ApiRoutes.Payments, id)", ReadApiFile("Endpoints/Payments/PaymentEndpoints.cs"), StringComparison.Ordinal);
        Assert.Contains("ApiEndpointConventions.CreatedResource(ApiRoutes.Incomes, id)", ReadApiFile("Endpoints/Incomes/IncomeEndpoints.cs"), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Endpoints/PaymentCategories/PaymentCategoryEndpoints.cs")]
    [InlineData("Endpoints/Payments/PaymentEndpoints.cs")]
    [InlineData("Endpoints/Incomes/IncomeEndpoints.cs")]
    [InlineData("Endpoints/ForecastRecurrenceRuleTypes/ForecastRecurrenceRuleTypeEndpoints.cs")]
    [InlineData("Endpoints/ForecastIncomes/ForecastIncomeEndpoints.cs")]
    [InlineData("Endpoints/ForecastExpenses/ForecastExpenseEndpoints.cs")]
    [InlineData("Endpoints/Auth/AuthEndpoints.cs")]
    public void EndpointSlices_Should_Use_Shared_MapApiGroup_Convention(string relativePath)
    {
        var content = ReadApiFile(relativePath);

        Assert.Contains("MapApiGroup(", content, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Endpoints/PaymentCategories/PaymentCategoryEndpoints.cs")]
    [InlineData("Endpoints/Payments/PaymentEndpoints.cs")]
    [InlineData("Endpoints/Incomes/IncomeEndpoints.cs")]
    [InlineData("Endpoints/ForecastRecurrenceRuleTypes/ForecastRecurrenceRuleTypeEndpoints.cs")]
    [InlineData("Endpoints/ForecastIncomes/ForecastIncomeEndpoints.cs")]
    [InlineData("Endpoints/ForecastExpenses/ForecastExpenseEndpoints.cs")]
    public void Protected_EndpointSlices_Should_Use_Shared_ReadAccess_Convention(string relativePath)
    {
        var content = ReadApiFile(relativePath);

        Assert.Contains(".RequireReadAccess()", content, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Endpoints/PaymentCategories/PaymentCategoryEndpoints.cs")]
    [InlineData("Endpoints/Payments/PaymentEndpoints.cs")]
    [InlineData("Endpoints/Incomes/IncomeEndpoints.cs")]
    [InlineData("Endpoints/ForecastIncomes/ForecastIncomeEndpoints.cs")]
    [InlineData("Endpoints/ForecastExpenses/ForecastExpenseEndpoints.cs")]
    public void Mutable_EndpointSlices_Should_Use_Shared_WriteAccess_Convention(string relativePath)
    {
        var content = ReadApiFile(relativePath);

        Assert.Contains(".RequireWriteAccess()", content, StringComparison.Ordinal);
        Assert.DoesNotContain("RequireAuthorization(AuthAuthorization.Policies.WriteAccess)", content, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Endpoints/ForecastExpenses/Contracts/ForecastExpenseOccurrencesQuery.cs")]
    [InlineData("Endpoints/ForecastIncomes/Contracts/ForecastIncomeOccurrencesQuery.cs")]
    [InlineData("Endpoints/Incomes/Contracts/IncomeFilterQuery.cs")]
    [InlineData("Endpoints/Payments/Contracts/PaymentFilterQuery.cs")]
    public void MonthQueryContracts_Should_Use_Shared_Month_Parser(string relativePath)
    {
        var content = ReadApiFile(relativePath);

        Assert.Contains("=> Month.GetRequiredYearMonth(MonthValidationMessage);", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Only_EndpointHelpers_Should_Read_Idempotency_Header_Directly()
    {
        var apiFiles = Directory.GetFiles(ApiProjectRoot, "*.cs", SearchOption.AllDirectories);

        var violatingFiles = apiFiles
            .Where(file => !file.EndsWith("ExtensionMethods\\EndpointHelpers.cs", StringComparison.OrdinalIgnoreCase))
            .Where(file => File.ReadAllText(file).Contains("Request.Headers[\"X-Idempotency-Key\"]", StringComparison.Ordinal))
            .Select(Path.GetFileName)
            .ToArray();

        Assert.Empty(violatingFiles);
    }

    [Fact]
    public void BuilderExtensionMethods_Should_Be_Removable_After_ApiServiceCollection_Refactor()
    {
        var builderExtensionsPath = Path.Combine(ApiProjectRoot, "ExtensionMethods", "BuilderExtensionMethods.cs");
        Assert.False(File.Exists(builderExtensionsPath));
    }

    private static string ReadApiFile(string relativePath)
    {
        var fullPath = Path.Combine(ApiProjectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        return File.ReadAllText(fullPath);
    }

    private static int CountOccurrences(string content, string marker)
    {
        var count = 0;
        var index = 0;

        while (true)
        {
            index = content.IndexOf(marker, index, StringComparison.Ordinal);
            if (index < 0)
                return count;

            count++;
            index += marker.Length;
        }
    }

    private static string ResolveApiProjectRoot()
    {
        var testAssemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            ?? throw new InvalidOperationException("Unable to resolve test assembly directory.");

        var srcRoot = Path.GetFullPath(Path.Combine(testAssemblyDirectory, "../../../../"));
        var apiRoot = Path.Combine(srcRoot, "MoneyTracker.Api");

        if (!Directory.Exists(apiRoot))
            throw new DirectoryNotFoundException($"MoneyTracker.Api source folder not found at '{apiRoot}'.");

        return apiRoot;
    }
}
