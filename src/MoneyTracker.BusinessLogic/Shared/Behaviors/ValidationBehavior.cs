using MediatR;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;

namespace MoneyTracker.BusinessLogic.Shared.Behaviors;

/// <summary>
/// Pipeline behavior for validating CQRS requests using FluentValidation
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationBehavior(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Get validator for this request type
        var validatorType = typeof(IValidator<>).MakeGenericType(typeof(TRequest));
        var validator = _serviceProvider.GetService(validatorType) as IValidator<TRequest>;

        if (validator == null)
        {
            // No validator registered, proceed
            return await next();
        }

        // Run validation
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            // Convert FluentValidation errors to validation exception
            var errors = validationResult.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.ErrorMessage).ToArray());

            throw new ValidationException(validationResult.Errors);
        }

        return await next();
    }
}
