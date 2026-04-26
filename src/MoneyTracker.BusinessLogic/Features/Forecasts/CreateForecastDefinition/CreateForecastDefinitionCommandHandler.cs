using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MoneyTracker.BusinessLogic.Common.Services.ExtensionMethods;
using MoneyTracker.BusinessLogic.Features.Forecasts.CreateForecastDefinition.ExtensionMethods;
using MoneyTracker.Data;
using MoneyTracker.Data.Base;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Forecasts.CreateForecastDefinition;

public class CreateForecastDefinitionCommandHandler
{
    private readonly IValidator<CreateForecastDefinitionCommand> _validator;
    private readonly MoneyTrackerDbContext _dbContext;

    public CreateForecastDefinitionCommandHandler(IValidator<CreateForecastDefinitionCommand> validator, MoneyTrackerDbContext dbContext)
    {
        _validator = validator;
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateForecastDefinitionCommand command, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        if (command.IsIncome)
        {
            var forecastIncome = await CreateForecastIncomeAsnyc(command);
            await CreateNewForecastIncomeDefinitionsAsync(forecastIncome, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return forecastIncome.Id;
        }
        else
        {
            await ValidatePaymentCategoryExistsForExpenseAsync(command.PaymentCategoryId!.Value, cancellationToken);

            var forecastExpense = await CreateForecastExpenseAsnyc(command);
            await CreateForecastExpenseDefinitionsAsync(forecastExpense, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return forecastExpense.Id;
        }
    }

    private async Task ValidatePaymentCategoryExistsForExpenseAsync(Guid paymentCategoryId, CancellationToken cancellationToken)
    {
        var categoryExists = await _dbContext.PaymentCategories.AnyAsync(x => x.Id == paymentCategoryId, cancellationToken);
        if (!categoryExists)
            throw new InvalidOperationException("The requested payment category does not exist.");
    }

    private async Task<ForecastIncome> CreateForecastIncomeAsnyc(CreateForecastDefinitionCommand command)
    {
        var forecastIncome = command.ToNewForecastIncome();
        await _dbContext.ForecastIncomes.AddAsync(forecastIncome);

        return forecastIncome;
    }

    private async Task<ForecastExpense> CreateForecastExpenseAsnyc(CreateForecastDefinitionCommand command)
    {
        var forecastExpense = command.ToNewForecastExpense();
        _dbContext.ForecastExpenses.Add(forecastExpense);

        return forecastExpense;
    }

    private async Task CreateNewForecastIncomeDefinitionsAsync(ForecastIncome forecastIncome, CancellationToken cancellationToken)
    {
        var (startDate, endDate) = ForecastOccurrencesHelper.GetSynchronizationWindow();

        await RehydrateEntityAsync(forecastIncome, cancellationToken);
        var recurrences = forecastIncome.GetRecurrences(startDate, endDate);
        var forecastOccurrences = recurrences.Select(s => forecastIncome.ToNewForecastOccurrence(s));

        _dbContext.AddRange(forecastOccurrences);
    }

    private async Task CreateForecastExpenseDefinitionsAsync(ForecastExpense forecastExpense, CancellationToken cancellationToken)
    {
        var (startDate, endDate) = ForecastOccurrencesHelper.GetSynchronizationWindow();

        await RehydrateEntityAsync(forecastExpense, cancellationToken);
        var recurrences = forecastExpense.GetRecurrences(startDate, endDate);
        var forecastOccurrences = recurrences.Select(s => forecastExpense.ToNewForecastOccurrence(s));

        _dbContext.AddRange(forecastOccurrences);
    }


    private async Task RehydrateEntityAsync(BaseForecast baseForecast, CancellationToken cancellationToken)
    {
        if (baseForecast.ForecastRecurrenceRuleType is null)
        {
            var forecastRecurrenceRuleType = await _dbContext.ForecastRecurrenceRuleTypes.FirstOrDefaultAsync(f => f.Id == baseForecast.ForecastRecurrenceRuleTypeId, cancellationToken);
            if (forecastRecurrenceRuleType is not null)
                baseForecast.ForecastRecurrenceRuleType = forecastRecurrenceRuleType;
        }
    }
}
