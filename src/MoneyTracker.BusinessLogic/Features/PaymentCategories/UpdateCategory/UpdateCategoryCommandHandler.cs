using FluentValidation;
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

        var existingCategory = dbContext.PaymentCategories.FirstOrDefault(c => c.Code == command.Code && c.Id != command.Id);
        if (existingCategory != null)
            throw new InvalidOperationException($"Category with code '{command.Code}' already exists.");

        category.Name = command.Name;
        category.Code = command.Code;

        dbContext.PaymentCategories.Update(category);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
