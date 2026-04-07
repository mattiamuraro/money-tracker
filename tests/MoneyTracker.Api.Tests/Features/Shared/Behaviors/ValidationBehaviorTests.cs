using MoneyTracker.Api.Features.Shared.Behaviors;
using MediatR;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using FluentValidation;

namespace MoneyTracker.Api.Tests.Features.Shared.Behaviors;

/// <summary>
/// Test suite per ValidationBehavior
/// Verifica che il pipeline comportamento di validazione funzioni correttamente
/// </summary>
public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WithValidRequest_ShouldProceedToNextHandler()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ValidationBehavior<TestRequest, TestResponse>>>();
        var validatorMock = new Mock<IValidator<TestRequest>>();
        
        var validationContext = new ValidationContext<TestRequest>(new TestRequest());
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var behavior = new ValidationBehavior<TestRequest, TestResponse>(
            new[] { validatorMock.Object },
            loggerMock.Object
        );

        var nextHandlerCalled = false;
        var expectedResponse = new TestResponse { Value = "Success" };

        // Act
        var result = await behavior.Handle(
            new TestRequest(),
            async () =>
            {
                nextHandlerCalled = true;
                return expectedResponse;
            },
            CancellationToken.None
        );

        // Assert
        Assert.True(nextHandlerCalled);
        Assert.Equal(expectedResponse.Value, result.Value);
    }

    [Fact]
    public async Task Handle_WithInvalidRequest_ShouldThrowValidationException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ValidationBehavior<TestRequest, TestResponse>>>();
        var validatorMock = new Mock<IValidator<TestRequest>>();
        
        var validationFailure = new FluentValidation.Results.ValidationFailure("Field", "Error message");
        var validationResult = new FluentValidation.Results.ValidationResult(new[] { validationFailure });
        
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var behavior = new ValidationBehavior<TestRequest, TestResponse>(
            new[] { validatorMock.Object },
            loggerMock.Object
        );

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            behavior.Handle(
                new TestRequest(),
                async () => new TestResponse { Value = "Should not reach here" },
                CancellationToken.None
            )
        );
    }

    // Helper test classes
    private class TestRequest { }
    private class TestResponse { public string Value { get; set; } = string.Empty; }
}
