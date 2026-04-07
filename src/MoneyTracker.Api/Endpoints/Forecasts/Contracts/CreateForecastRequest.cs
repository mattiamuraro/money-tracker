using System.ComponentModel.DataAnnotations;

namespace MoneyTracker.Api.Endpoints.Forecasts.Contracts;

public class CreateForecastRequest
{
    [Required]
    [MaxLength(100)]
    public required string Description { get; set; }

    [Range(typeof(decimal), "0.01", "9999999999999999")]
    public decimal Amount { get; set; }

    public DateOnly RecurrenceStart { get; set; }

    public DateOnly? RecurrenceEnd { get; set; }

    [Range(1, 365)]
    public int DayInterval { get; set; }

    public bool IsIncome { get; set; }
}
