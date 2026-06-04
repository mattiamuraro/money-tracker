using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.PaymentCategories.UpdateCategory;

/// <summary>
/// Handler for updating an existing payment category
/// </summary>
public class UpdateCategoryCommandHandler(
    IValidator<UpdateCategoryCommand> validator,
    MoneyTrackerDbContext dbContext)
    : IHandler<UpdateCategoryCommand>
{
    public async Task Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var category = await dbContext.PaymentCategories.FindAsync(
            new object[] { command.Id }, cancellationToken: cancellationToken);
        if (category == null)
            throw new EntityNotFoundException($"Payment category with id {command.Id} not found");

        var categoryExists = await dbContext.PaymentCategories
            .AnyAsync(c => c.Code == command.Code && c.Id != command.Id, cancellationToken);
        if (categoryExists)
            throw new ConflictException($"Category with code '{command.Code}' already exists.");

        category.Name = command.Name;
        category.NameNormalized = command.Name.Trim().ToUpperInvariant();
        category.Code = command.Code;

        dbContext.PaymentCategories.Update(category);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
