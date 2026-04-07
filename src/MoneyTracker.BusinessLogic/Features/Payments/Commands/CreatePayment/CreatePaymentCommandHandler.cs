using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;
using MediatR;

namespace MoneyTracker.BusinessLogic.Features.Payments.Commands.CreatePayment;

/// <summary>
/// Handler per il command CreatePaymentCommand
/// </summary>
public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, Guid>
{
    private readonly MoneyTrackerDbContext _dbContext;

    public CreatePaymentCommandHandler(MoneyTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        // Verificare che la categoria esista
        var category = await _dbContext.PaymentCategories.FindAsync(
            new object[] { request.PaymentCategoryId },
            cancellationToken: cancellationToken);

        if (category == null)
            throw new InvalidOperationException($"PaymentCategory with id {request.PaymentCategoryId} not found");

        var payment = new Data.Payment
        {
            Id = Guid.NewGuid(),
            Description = request.Description,
            PaymentCategoryId = request.PaymentCategoryId,
            Amount = request.Amount,
            Date = request.Date,
            IsOneShot = request.IsOneShot,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy,
            ModifiedAt = DateTime.UtcNow,
            ModifiedBy = request.CreatedBy
        };

        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return payment.Id;
    }
}
