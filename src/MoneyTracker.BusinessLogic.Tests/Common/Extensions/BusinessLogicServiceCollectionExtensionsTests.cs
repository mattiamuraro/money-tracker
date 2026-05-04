using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MoneyTracker.BusinessLogic.Common.Extensions;
using MoneyTracker.BusinessLogic.Features.Auth.Login;
using MoneyTracker.BusinessLogic.Features.Auth.Register;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.CreateForecastExpenseDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.DeleteForecastExpenseDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.DiscardForecastExpenseOccurrence;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetForecastExpenseDefinitions;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetForecastExpenseRows;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.GetPendingForecastExpenseOccurrences;
using MoneyTracker.BusinessLogic.Features.ForecastExpenses.UpdateForecastExpenseDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.CreateForecastIncomeDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.DeleteForecastIncomeDefinition;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.DiscardForecastIncomeOccurrence;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetForecastIncomeDefinitions;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetForecastIncomeRows;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.GetPendingForecastIncomeOccurrences;
using MoneyTracker.BusinessLogic.Features.ForecastIncomes.UpdateForecastIncomeDefinition;
using MoneyTracker.BusinessLogic.Features.Forecasts.GetForecastRecurrenceRuleTypes;
using MoneyTracker.BusinessLogic.Features.Forecasts.SynchronizeForecastOccurrences;
using MoneyTracker.BusinessLogic.Features.Incomes.CreateIncome;
using MoneyTracker.BusinessLogic.Features.Incomes.DeleteIncome;
using MoneyTracker.BusinessLogic.Features.Incomes.GetIncome;
using MoneyTracker.BusinessLogic.Features.Incomes.GetIncomeById;
using MoneyTracker.BusinessLogic.Features.Incomes.UpdateIncome;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.CreateCategory;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.DeleteCategory;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.GetAllCategories;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.GetCategoryById;
using MoneyTracker.BusinessLogic.Features.PaymentCategories.UpdateCategory;
using MoneyTracker.BusinessLogic.Features.Payments.CreatePayment;
using MoneyTracker.BusinessLogic.Features.Payments.DeletePayment;
using MoneyTracker.BusinessLogic.Features.Payments.GetPayment;
using MoneyTracker.BusinessLogic.Features.Payments.GetPaymentById;
using MoneyTracker.BusinessLogic.Features.Payments.UpdatePayment;
using MoneyTracker.Data.EntityFramework;
using Xunit;

namespace MoneyTracker.BusinessLogic.Tests.Common.Extensions;

/// <summary>
/// Unit tests for BusinessLogicServiceCollectionExtensions
/// </summary>
public class BusinessLogicServiceCollectionExtensionsTests
{
    [Fact]
    public void AddBusinessLogicServices_Should_ReturnServiceCollection_ForChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddBusinessLogicServices();

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_LoginCommandHandler_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(LoginCommandHandler) &&
            x.ImplementationType == typeof(LoginCommandHandler));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_RegisterCommandHandler_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(RegisterCommandHandler) &&
            x.ImplementationType == typeof(RegisterCommandHandler));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_CreatePaymentCommandHandler_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(CreatePaymentCommandHandler) &&
            x.ImplementationType == typeof(CreatePaymentCommandHandler));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_UpdatePaymentCommandHandler_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(UpdatePaymentCommandHandler) &&
            x.ImplementationType == typeof(UpdatePaymentCommandHandler));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_DeletePaymentCommandHandler_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(DeletePaymentCommandHandler) &&
            x.ImplementationType == typeof(DeletePaymentCommandHandler));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_GetPaymentQueryHandler_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(GetPaymentQueryHandler) &&
            x.ImplementationType == typeof(GetPaymentQueryHandler));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_GetPaymentByIdQueryHandler_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(GetPaymentByIdQueryHandler) &&
            x.ImplementationType == typeof(GetPaymentByIdQueryHandler));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_CreateIncomeCommandHandler_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(CreateIncomeCommandHandler) &&
            x.ImplementationType == typeof(CreateIncomeCommandHandler));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_UpdateIncomeCommandHandler_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(UpdateIncomeCommandHandler) &&
            x.ImplementationType == typeof(UpdateIncomeCommandHandler));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_DeleteIncomeCommandHandler_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(DeleteIncomeCommandHandler) &&
            x.ImplementationType == typeof(DeleteIncomeCommandHandler));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_GetIncomeQueryHandler_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(GetIncomeQueryHandler) &&
            x.ImplementationType == typeof(GetIncomeQueryHandler));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_GetIncomeByIdQueryHandler_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(GetIncomeByIdQueryHandler) &&
            x.ImplementationType == typeof(GetIncomeByIdQueryHandler));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_GetForecastRecurrenceRuleTypesQueryHandler_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(GetForecastRecurrenceRuleTypesQueryHandler) &&
            x.ImplementationType == typeof(GetForecastRecurrenceRuleTypesQueryHandler));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_SynchronizeForecastOccurrencesCommandHandler_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(SynchronizeForecastOccurrencesCommandHandler) &&
            x.ImplementationType == typeof(SynchronizeForecastOccurrencesCommandHandler));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_GetForecastIncomeRowsQueryHandler_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(GetForecastIncomeRowsQueryHandler) &&
            x.ImplementationType == typeof(GetForecastIncomeRowsQueryHandler));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_GetForecastExpenseRowsQueryHandler_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(GetForecastExpenseRowsQueryHandler) &&
            x.ImplementationType == typeof(GetForecastExpenseRowsQueryHandler));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_GetPendingForecastIncomeOccurrencesQueryHandler_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(GetPendingForecastIncomeOccurrencesQueryHandler) &&
            x.ImplementationType == typeof(GetPendingForecastIncomeOccurrencesQueryHandler));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_GetPendingForecastExpenseOccurrencesQueryHandler_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(GetPendingForecastExpenseOccurrencesQueryHandler) &&
            x.ImplementationType == typeof(GetPendingForecastExpenseOccurrencesQueryHandler));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_GetForecastIncomeDefinitionsQueryHandler_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(GetForecastIncomeDefinitionsQueryHandler) &&
            x.ImplementationType == typeof(GetForecastIncomeDefinitionsQueryHandler));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_GetForecastExpenseDefinitionsQueryHandler_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(GetForecastExpenseDefinitionsQueryHandler) &&
            x.ImplementationType == typeof(GetForecastExpenseDefinitionsQueryHandler));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_CreateForecastIncomeDefinitionCommandHandler_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(CreateForecastIncomeDefinitionCommandHandler) &&
            x.ImplementationType == typeof(CreateForecastIncomeDefinitionCommandHandler));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_UpdateForecastIncomeDefinitionCommandHandler_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(UpdateForecastIncomeDefinitionCommandHandler) &&
            x.ImplementationType == typeof(UpdateForecastIncomeDefinitionCommandHandler));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_DeleteForecastIncomeDefinitionCommandHandler_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(DeleteForecastIncomeDefinitionCommandHandler) &&
            x.ImplementationType == typeof(DeleteForecastIncomeDefinitionCommandHandler));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_DiscardForecastIncomeOccurrenceCommandHandler_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(DiscardForecastIncomeOccurrenceCommandHandler) &&
            x.ImplementationType == typeof(DiscardForecastIncomeOccurrenceCommandHandler));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_CreateForecastExpenseDefinitionCommandHandler_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(CreateForecastExpenseDefinitionCommandHandler) &&
            x.ImplementationType == typeof(CreateForecastExpenseDefinitionCommandHandler));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_UpdateForecastExpenseDefinitionCommandHandler_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(UpdateForecastExpenseDefinitionCommandHandler) &&
            x.ImplementationType == typeof(UpdateForecastExpenseDefinitionCommandHandler));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_DeleteForecastExpenseDefinitionCommandHandler_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(DeleteForecastExpenseDefinitionCommandHandler) &&
            x.ImplementationType == typeof(DeleteForecastExpenseDefinitionCommandHandler));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_DiscardForecastExpenseOccurrenceCommandHandler_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(DiscardForecastExpenseOccurrenceCommandHandler) &&
            x.ImplementationType == typeof(DiscardForecastExpenseOccurrenceCommandHandler));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_CreateCategoryCommandHandler_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(CreateCategoryCommandHandler) &&
            x.ImplementationType == typeof(CreateCategoryCommandHandler));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_UpdateCategoryCommandHandler_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(UpdateCategoryCommandHandler) &&
            x.ImplementationType == typeof(UpdateCategoryCommandHandler));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_DeleteCategoryCommandHandler_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(DeleteCategoryCommandHandler) &&
            x.ImplementationType == typeof(DeleteCategoryCommandHandler));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_GetAllCategoriesQueryHandler_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(GetAllCategoriesQueryHandler) &&
            x.ImplementationType == typeof(GetAllCategoriesQueryHandler));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_GetCategoryByIdQueryHandler_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(GetCategoryByIdQueryHandler) &&
            x.ImplementationType == typeof(GetCategoryByIdQueryHandler));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_CreatePaymentCommandValidator_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(IValidator<CreatePaymentCommand>) &&
            x.ImplementationType == typeof(CreatePaymentCommandValidator));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_UpdatePaymentCommandValidator_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(IValidator<UpdatePaymentCommand>) &&
            x.ImplementationType == typeof(UpdatePaymentCommandValidator));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_CreateIncomeCommandValidator_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(IValidator<CreateIncomeCommand>) &&
            x.ImplementationType == typeof(CreateIncomeCommandValidator));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_UpdateIncomeCommandValidator_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(IValidator<UpdateIncomeCommand>) &&
            x.ImplementationType == typeof(UpdateIncomeCommandValidator));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_CreateCategoryCommandValidator_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(IValidator<CreateCategoryCommand>) &&
            x.ImplementationType == typeof(CreateCategoryCommandValidator));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_UpdateCategoryCommandValidator_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(IValidator<UpdateCategoryCommand>) &&
            x.ImplementationType == typeof(UpdateCategoryCommandValidator));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_LoginCommandValidator_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(IValidator<LoginCommand>) &&
            x.ImplementationType == typeof(LoginCommandValidator));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_RegisterCommandValidator_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(IValidator<RegisterCommand>) &&
            x.ImplementationType == typeof(RegisterCommandValidator));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_CreateForecastIncomeDefinitionCommandValidator_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(IValidator<CreateForecastIncomeDefinitionCommand>) &&
            x.ImplementationType == typeof(CreateForecastIncomeDefinitionCommandValidator));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_UpdateForecastIncomeDefinitionCommandValidator_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(IValidator<UpdateForecastIncomeDefinitionCommand>) &&
            x.ImplementationType == typeof(UpdateForecastIncomeDefinitionCommandValidator));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_CreateForecastExpenseDefinitionCommandValidator_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(IValidator<CreateForecastExpenseDefinitionCommand>) &&
            x.ImplementationType == typeof(CreateForecastExpenseDefinitionCommandValidator));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_UpdateForecastExpenseDefinitionCommandValidator_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(IValidator<UpdateForecastExpenseDefinitionCommand>) &&
            x.ImplementationType == typeof(UpdateForecastExpenseDefinitionCommandValidator));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddBusinessLogicServices_Should_Register_AllServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBusinessLogicServices();

        // Register required dependencies that handlers depend on
        services.AddDbContext<MoneyTrackerDbContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.Configure<MoneyTracker.BusinessLogic.Common.Options.JwtOptions>(o =>
        {
            o.Key = "test-key-with-at-least-32-characters-long";
            o.Issuer = "test";
            o.Audience = "test";
        });
        services.Configure<MoneyTracker.BusinessLogic.Common.Options.AuthOptions>(o => o.AllowRegistration = true);
        services.AddSingleton<Microsoft.AspNetCore.Identity.IPasswordHasher<MoneyTracker.Data.User>,
            Microsoft.AspNetCore.Identity.PasswordHasher<MoneyTracker.Data.User>>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Verify all handlers can be resolved
        Assert.NotNull(serviceProvider.GetService<LoginCommandHandler>());
        Assert.NotNull(serviceProvider.GetService<RegisterCommandHandler>());
        Assert.NotNull(serviceProvider.GetService<CreatePaymentCommandHandler>());
        Assert.NotNull(serviceProvider.GetService<UpdatePaymentCommandHandler>());
        Assert.NotNull(serviceProvider.GetService<DeletePaymentCommandHandler>());
        Assert.NotNull(serviceProvider.GetService<GetPaymentQueryHandler>());
        Assert.NotNull(serviceProvider.GetService<GetPaymentByIdQueryHandler>());
        Assert.NotNull(serviceProvider.GetService<CreateIncomeCommandHandler>());
        Assert.NotNull(serviceProvider.GetService<UpdateIncomeCommandHandler>());
        Assert.NotNull(serviceProvider.GetService<DeleteIncomeCommandHandler>());
        Assert.NotNull(serviceProvider.GetService<GetIncomeQueryHandler>());
        Assert.NotNull(serviceProvider.GetService<GetIncomeByIdQueryHandler>());
        Assert.NotNull(serviceProvider.GetService<GetForecastRecurrenceRuleTypesQueryHandler>());
        Assert.NotNull(serviceProvider.GetService<SynchronizeForecastOccurrencesCommandHandler>());
        Assert.NotNull(serviceProvider.GetService<GetForecastIncomeRowsQueryHandler>());
        Assert.NotNull(serviceProvider.GetService<GetForecastExpenseRowsQueryHandler>());
        Assert.NotNull(serviceProvider.GetService<GetPendingForecastIncomeOccurrencesQueryHandler>());
        Assert.NotNull(serviceProvider.GetService<GetPendingForecastExpenseOccurrencesQueryHandler>());
        Assert.NotNull(serviceProvider.GetService<GetForecastIncomeDefinitionsQueryHandler>());
        Assert.NotNull(serviceProvider.GetService<GetForecastExpenseDefinitionsQueryHandler>());
        Assert.NotNull(serviceProvider.GetService<CreateForecastIncomeDefinitionCommandHandler>());
        Assert.NotNull(serviceProvider.GetService<UpdateForecastIncomeDefinitionCommandHandler>());
        Assert.NotNull(serviceProvider.GetService<DeleteForecastIncomeDefinitionCommandHandler>());
        Assert.NotNull(serviceProvider.GetService<DiscardForecastIncomeOccurrenceCommandHandler>());
        Assert.NotNull(serviceProvider.GetService<CreateForecastExpenseDefinitionCommandHandler>());
        Assert.NotNull(serviceProvider.GetService<UpdateForecastExpenseDefinitionCommandHandler>());
        Assert.NotNull(serviceProvider.GetService<DeleteForecastExpenseDefinitionCommandHandler>());
        Assert.NotNull(serviceProvider.GetService<DiscardForecastExpenseOccurrenceCommandHandler>());
        Assert.NotNull(serviceProvider.GetService<CreateCategoryCommandHandler>());
        Assert.NotNull(serviceProvider.GetService<UpdateCategoryCommandHandler>());
        Assert.NotNull(serviceProvider.GetService<DeleteCategoryCommandHandler>());
        Assert.NotNull(serviceProvider.GetService<GetAllCategoriesQueryHandler>());
        Assert.NotNull(serviceProvider.GetService<GetCategoryByIdQueryHandler>());

        // Verify all validators can be resolved
        Assert.NotNull(serviceProvider.GetService<IValidator<CreatePaymentCommand>>());
        Assert.NotNull(serviceProvider.GetService<IValidator<UpdatePaymentCommand>>());
        Assert.NotNull(serviceProvider.GetService<IValidator<CreateIncomeCommand>>());
        Assert.NotNull(serviceProvider.GetService<IValidator<UpdateIncomeCommand>>());
        Assert.NotNull(serviceProvider.GetService<IValidator<CreateCategoryCommand>>());
        Assert.NotNull(serviceProvider.GetService<IValidator<UpdateCategoryCommand>>());
        Assert.NotNull(serviceProvider.GetService<IValidator<LoginCommand>>());
        Assert.NotNull(serviceProvider.GetService<IValidator<RegisterCommand>>());
        Assert.NotNull(serviceProvider.GetService<IValidator<CreateForecastIncomeDefinitionCommand>>());
        Assert.NotNull(serviceProvider.GetService<IValidator<UpdateForecastIncomeDefinitionCommand>>());
        Assert.NotNull(serviceProvider.GetService<IValidator<CreateForecastExpenseDefinitionCommand>>());
        Assert.NotNull(serviceProvider.GetService<IValidator<UpdateForecastExpenseDefinitionCommand>>());
    }
}
