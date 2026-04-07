using FluentValidation;
using MoneyTracker.Api.Features.Payments.Create;
using Xunit;

namespace MoneyTracker.Api.Tests.Features.Payments.Create;

/// <summary>
/// Test suite per CreatePaymentCommandValidator
/// Assicura che la validazione sia coerente e corretta
/// </summary>
public class CreatePaymentCommandValidatorTests
{
    private readonly CreatePaymentCommandValidator _validator;

    public CreatePaymentCommandValidatorTests()
    {
        _validator = new CreatePaymentCommandValidator();
    }

    [Fact]
    public async Task Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new CreatePaymentCommand(
            "Test Payment",
            Guid.NewGuid(),
            100.50m,
            DateTime.UtcNow.AddDays(-1),
            "TestUser"
        );

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithEmptyDescription_ShouldFail()
    {
        // Arrange
        var command = new CreatePaymentCommand(
            string.Empty,
            Guid.NewGuid(),
            100.50m,
            DateTime.UtcNow.AddDays(-1),
            "TestUser"
        );

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description");
    }

    [Fact]
    public async Task Validate_WithZeroAmount_ShouldFail()
    {
        // Arrange
        var command = new CreatePaymentCommand(
            "Test Payment",
            Guid.NewGuid(),
            0m,
            DateTime.UtcNow.AddDays(-1),
            "TestUser"
        );

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Amount");
    }

    [Fact]
    public async Task Validate_WithFutureDate_ShouldFail()
    {
        // Arrange
        var command = new CreatePaymentCommand(
            "Test Payment",
            Guid.NewGuid(),
            100.50m,
            DateTime.UtcNow.AddDays(1),
            "TestUser"
        );

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Date");
    }
}
