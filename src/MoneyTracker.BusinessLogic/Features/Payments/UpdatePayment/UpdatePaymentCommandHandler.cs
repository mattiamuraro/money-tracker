using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Payments.UpdatePayment;

/// <summary>
/// Handler for the UpdatePaymentCommand
/// </summary>
public class UpdatePaymentCommandHandler(
    IValidator<UpdatePaymentCommand> validator,
    MoneyTrackerDbContext dbContext)
    : IHandler<UpdatePaymentCommand>
{
    public async Task Handle(UpdatePaymentCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var payment = await dbContext.Payments.FindAsync(
            new object[] { request.PaymentId }, cancellationToken: cancellationToken);
        if (payment == null)
            throw new EntityNotFoundException($"Payment with id {request.PaymentId} not found");

        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            var normalizedDescription = request.Description.Trim();
            payment.Description = normalizedDescription;
            payment.DescriptionNormalized = normalizedDescription.ToUpperInvariant();
        }

        if (request.PaymentCategoryId.HasValue)
        {
            var categoryExists = await dbContext.PaymentCategories.AnyAsync(
                category => category.Id == request.PaymentCategoryId.Value, cancellationToken);
            if (!categoryExists)
                throw new EntityNotFoundException($"PaymentCategory with id {request.PaymentCategoryId.Value} not found");
            payment.PaymentCategoryId = request.PaymentCategoryId.Value;
        }

        if (request.Amount.HasValue)
            payment.Amount = request.Amount.Value;
        if (request.Date.HasValue)
            payment.Date = request.Date.Value;
        if (request.IsOneShot.HasValue)
            payment.IsOneShot = request.IsOneShot.Value;

        dbContext.Payments.Update(payment);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
