using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;

namespace MoneyTracker.BusinessLogic.Tests.Common.Extensions;

public class FeatureRegistrationCoverageTests
{
    [Fact]
    public void RegisterServices_Methods_For_All_Feature_Registrations_Should_Be_Invokable()
    {
        var assembly = typeof(BusinessLogicServiceCollectionExtensions).Assembly;

        var registrationTypeNames = new[]
        {
            "MoneyTracker.BusinessLogic.Features.Auth.Login.LoginRegistration",
            "MoneyTracker.BusinessLogic.Features.Auth.Register.RegisterRegistration",
            "MoneyTracker.BusinessLogic.Features.Payments.CreatePayment.CreatePaymentRegistration",
            "MoneyTracker.BusinessLogic.Features.Payments.UpdatePayment.UpdatePaymentRegistration",
            "MoneyTracker.BusinessLogic.Features.Payments.DeletePayment.DeletePaymentRegistration",
            "MoneyTracker.BusinessLogic.Features.Payments.GetPayment.GetPaymentRegistration",
            "MoneyTracker.BusinessLogic.Features.Payments.GetPaymentById.GetPaymentByIdRegistration",
            "MoneyTracker.BusinessLogic.Features.Incomes.CreateIncome.CreateIncomeRegistration",
            "MoneyTracker.BusinessLogic.Features.Incomes.UpdateIncome.UpdateIncomeRegistration",
            "MoneyTracker.BusinessLogic.Features.Incomes.DeleteIncome.DeleteIncomeRegistration",
            "MoneyTracker.BusinessLogic.Features.Incomes.GetIncome.GetIncomeRegistration",
            "MoneyTracker.BusinessLogic.Features.Incomes.GetIncomeById.GetIncomeByIdRegistration",
            "MoneyTracker.BusinessLogic.Features.PaymentCategories.CreateCategory.CreateCategoryRegistration",
            "MoneyTracker.BusinessLogic.Features.PaymentCategories.UpdateCategory.UpdateCategoryRegistration",
            "MoneyTracker.BusinessLogic.Features.PaymentCategories.DeleteCategory.DeleteCategoryRegistration",
            "MoneyTracker.BusinessLogic.Features.PaymentCategories.GetAllCategories.GetAllCategoriesRegistration",
            "MoneyTracker.BusinessLogic.Features.PaymentCategories.GetCategoryById.GetCategoryByIdRegistration",
            "MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastRecurrenceRuleTypes.GetForecastRecurrenceRuleTypesRegistration",
            "MoneyTracker.BusinessLogic.Features.Forecasts.SynchronizeForecastOccurrences.SynchronizeForecastOccurrencesRegistration",
            "MoneyTracker.BusinessLogic.Features.ForecastExpenses.CreateForecastExpenseDefinition.CreateForecastExpenseDefinitionRegistration",
            "MoneyTracker.BusinessLogic.Features.ForecastExpenses.UpdateForecastExpenseDefinition.UpdateForecastExpenseDefinitionRegistration",
            "MoneyTracker.BusinessLogic.Features.ForecastExpenses.DeleteForecastExpenseDefinition.DeleteForecastExpenseDefinitionRegistration",
            "MoneyTracker.BusinessLogic.Features.ForecastExpenses.DiscardForecastExpenseOccurrence.DiscardForecastExpenseOccurrenceRegistration",
            "MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetForecastExpenseDefinitions.GetForecastExpenseDefinitionsRegistration",
            "MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetForecastExpenseRows.GetForecastExpenseRowsRegistration",
            "MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetPendingForecastExpenseOccurrences.GetPendingForecastExpenseOccurrencesRegistration",
            "MoneyTracker.BusinessLogic.Features.ForecastExpenses.CreateForecastExpenseDefinitionAndSynchronize.CreateForecastExpenseDefinitionAndSynchronizeRegistration",
            "MoneyTracker.BusinessLogic.Features.ForecastExpenses.UpdateForecastExpenseDefinitionAndSynchronize.UpdateForecastExpenseDefinitionAndSynchronizeRegistration",
            "MoneyTracker.BusinessLogic.Features.ForecastExpenses.DeleteForecastExpenseDefinitionAndSynchronize.DeleteForecastExpenseDefinitionAndSynchronizeRegistration",
            "MoneyTracker.BusinessLogic.Features.ForecastIncomes.CreateForecastIncomeDefinition.CreateForecastIncomeDefinitionRegistration",
            "MoneyTracker.BusinessLogic.Features.ForecastIncomes.UpdateForecastIncomeDefinition.UpdateForecastIncomeDefinitionRegistration",
            "MoneyTracker.BusinessLogic.Features.ForecastIncomes.DeleteForecastIncomeDefinition.DeleteForecastIncomeDefinitionRegistration",
            "MoneyTracker.BusinessLogic.Features.ForecastIncomes.DiscardForecastIncomeOccurrence.DiscardForecastIncomeOccurrenceRegistration",
            "MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetForecastIncomeDefinitions.GetForecastIncomeDefinitionsRegistration",
            "MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetForecastIncomeRows.GetForecastIncomeRowsRegistration",
            "MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetPendingForecastIncomeOccurrences.GetPendingForecastIncomeOccurrencesRegistration",
            "MoneyTracker.BusinessLogic.Features.ForecastIncomes.CreateForecastIncomeDefinitionAndSynchronize.CreateForecastIncomeDefinitionAndSynchronizeRegistration",
            "MoneyTracker.BusinessLogic.Features.ForecastIncomes.UpdateForecastIncomeDefinitionAndSynchronize.UpdateForecastIncomeDefinitionAndSynchronizeRegistration",
            "MoneyTracker.BusinessLogic.Features.ForecastIncomes.DeleteForecastIncomeDefinitionAndSynchronize.DeleteForecastIncomeDefinitionAndSynchronizeRegistration"
        };

        foreach (var registrationTypeName in registrationTypeNames)
        {
            var type = assembly.GetType(registrationTypeName);
            Assert.NotNull(type);

            var method = type!.GetMethod(
                "RegisterServices",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic,
                binder: null,
                types: [typeof(IServiceCollection)],
                modifiers: null);

            Assert.NotNull(method);

            var services = new ServiceCollection();
            var result = method!.Invoke(null, [services]);

            Assert.Same(services, result);
        }
    }
}
