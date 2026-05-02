using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.PaymentCategories.CreateCategory;

/// <summary>
/// Handler for creating a new payment category
/// </summary>
public class CreateCategoryCommandHandler(
    IValidator<CreateCategoryCommand> validator,
    MoneyTrackerDbContext dbContext)
    : IHandler<CreateCategoryCommand, Guid>
{
    public async Task<Guid> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var categoryExists = await dbContext.PaymentCategories
            .AnyAsync(c => c.Code == command.Code, cancellationToken);
        if (categoryExists)
            throw new ConflictException($"Category with code '{command.Code}' already exists.");

        var category = new PaymentCategory
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Code = command.Code
        };

        dbContext.PaymentCategories.Add(category);
        await dbContext.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}
