namespace MoneyTracker.Api.Options;

public sealed class ExceptionDetailOptions
{
    public const string SectionName = "Security:ExceptionDetails";

    public bool IncludeExceptionDetails { get; set; }
}
