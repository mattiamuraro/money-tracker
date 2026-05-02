using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Common.Extensions;
using MoneyTracker.BusinessLogic.Common.Handlers;
using MoneyTracker.BusinessLogic.Features.Forecasts.CreateForecastDefinition.ExtensionMethods;
using MoneyTracker.Data;
using MoneyTracker.Data.Base;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Forecasts.CreateForecastDefinition;

public class CreateForecastDefinitionCommandHandler(
    IValidator<CreateForecastDefinitionCommand> validator,
    MoneyTrackerDbContext dbContext)
    : IHandler<CreateForecastDefinitionCommand, Guid>
{
    public async Task<Guid> Handle(CreateForecastDefinitionCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (command.IsIncome)
        {
            var forecastIncome = await CreateForecastIncomeAsync(command);
            await CreateNewForecastIncomeDefinitionsAsync(forecastIncome, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return forecastIncome.Id;
        }
        else
        {
            await ValidatePaymentCategoryExistsAsync(command.PaymentCategoryId!.Value, cancellationToken);
            var forecastExpense = await CreateForecastExpenseAsync(command);
            await CreateForecastExpenseDefinitionsAsync(forecastExpense, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return forecastExpense.Id;
        }
    }

    private async Task ValidatePaymentCategoryExistsAsync(Guid paymentCategoryId, CancellationToken cancellationToken)
    {
        var categoryExists = await dbContext.PaymentCategories.AnyAsync(x => x.Id == paymentCategoryId, cancellationToken);
        if (!categoryExists)
            throw new EntityNotFoundException("The requested payment category does not exist.");
    }

    private async Task<ForecastIncome> CreateForecastIncomeAsync(CreateForecastDefinitionCommand command)
    {
        var forecastIncome = command.ToNewForecastIncome();
        await dbContext.ForecastIncomes.AddAsync(forecastIncome);
        return forecastIncome;
    }

    private Task<ForecastExpense> CreateForecastExpenseAsync(CreateForecastDefinitionCommand command)
    {
        var forecastExpense = command.ToNewForecastExpense();
        dbContext.ForecastExpenses.Add(forecastExpense);
        return Task.FromResult(forecastExpense);
    }

    private async Task CreateNewForecastIncomeDefinitionsAsync(ForecastIncome forecastIncome, CancellationToken cancellationToken)
    {
        var (startDate, endDate) = ForecastOccurrencesHelper.GetSynchronizationWindow();
        await RehydrateEntityAsync(forecastIncome, cancellationToken);
        var forecastOccurrences = forecastIncome.GetRecurrences(startDate, endDate)
            .Select(s => forecastIncome.ToNewForecastOccurrence(s));
        dbContext.AddRange(forecastOccurrences);
    }

    private async Task CreateForecastExpenseDefinitionsAsync(ForecastExpense forecastExpense, CancellationToken cancellationToken)
    {
        var (startDate, endDate) = ForecastOccurrencesHelper.GetSynchronizationWindow();
        await RehydrateEntityAsync(forecastExpense, cancellationToken);
        var forecastOccurrences = forecastExpense.GetRecurrences(startDate, endDate)
            .Select(s => forecastExpense.ToNewForecastOccurrence(s));
        dbContext.AddRange(forecastOccurrences);
    }

    private async Task RehydrateEntityAsync(BaseForecast baseForecast, CancellationToken cancellationToken)
    {
        if (baseForecast.ForecastRecurrenceRuleType is null)
        {
            var forecastRecurrenceRuleType = await dbContext.ForecastRecurrenceRuleTypes
                .FirstOrDefaultAsync(f => f.Id == baseForecast.ForecastRecurrenceRuleTypeId, cancellationToken);
            if (forecastRecurrenceRuleType is not null)
                baseForecast.ForecastRecurrenceRuleType = forecastRecurrenceRuleType;
        }
    }
}
