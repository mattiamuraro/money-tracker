using System.Reflection;
using Microsoft.Extensions.Logging;
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

var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
var serviceVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "unknown";
logger.LogInformation(
    new EventId(3201, "WorkerStartup"),
    "Starting {ServiceName} v{ServiceVersion} in {EnvironmentName} environment.",
    builder.Environment.ApplicationName,
    serviceVersion,
    builder.Environment.EnvironmentName);

host.Run();
