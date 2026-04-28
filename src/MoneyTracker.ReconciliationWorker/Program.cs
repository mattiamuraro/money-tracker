using MoneyTracker.BusinessLogic.Common.Extensions;
using MoneyTracker.Data.EntityFramework;
using MoneyTracker.ReconciliationWorker;
using MoneyTracker.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.AddSqlServerDbContext<MoneyTrackerDbContext>("moneytacker-db");
builder.Services.AddBusinessLogicServices();
builder.Services.AddHostedService<ForecastOccurrenceReconciliationService>();

var host = builder.Build();
host.Run();
