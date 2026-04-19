using MoneyTracker.BusinessLogic.Shared.Models;

namespace MoneyTracker.BusinessLogic.Tests.Shared.Models;

public class ErrorResponseTests
{
    [Fact]
    public void Should_Have_Expected_Default_Values()
    {
        var before = DateTime.UtcNow;
        var response = new ErrorResponse();
        var after = DateTime.UtcNow;

        Xunit.Assert.Equal(string.Empty, response.Code);
        Xunit.Assert.Equal(string.Empty, response.Message);
        Xunit.Assert.Equal(string.Empty, response.TraceId);
        Xunit.Assert.Null(response.Details);
        Xunit.Assert.Null(response.Errors);
        Xunit.Assert.Equal(0, response.StatusCode);
        Xunit.Assert.InRange(response.Timestamp, before, after);
    }

    [Fact]
    public void Should_Allow_Setting_All_Properties()
    {
        var timestamp = new DateTime(2026, 4, 1, 8, 30, 0, DateTimeKind.Utc);
        var errors = new Dictionary<string, string[]>
        {
            ["Amount"] = ["Amount must be greater than zero"]
        };

        var response = new ErrorResponse
        {
            Code = "INVALID_AMOUNT",
            Message = "Validation failed",
            Details = "Amount cannot be <= 0",
            TraceId = "trace-123",
            Errors = errors,
            StatusCode = 400,
            Timestamp = timestamp
        };

        Xunit.Assert.Equal("INVALID_AMOUNT", response.Code);
        Xunit.Assert.Equal("Validation failed", response.Message);
        Xunit.Assert.Equal("Amount cannot be <= 0", response.Details);
        Xunit.Assert.Equal("trace-123", response.TraceId);
        Xunit.Assert.Equal(400, response.StatusCode);
        Xunit.Assert.Equal(timestamp, response.Timestamp);
        Xunit.Assert.NotNull(response.Errors);
        Xunit.Assert.Equal("Amount must be greater than zero", response.Errors!["Amount"][0]);
    }
}
