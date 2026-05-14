using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MoneyTracker.Data.EntityFramework;
using MoneyTracker.Data.MigrationService;
using MoneyTracker.ServiceDefaults;


var builder = Host.CreateApplicationBuilder(args);
builder.AddEnvironmentSecretProviders();

// Register the DbContext the same way as in the API
builder.Services.AddDbContext<MoneyTrackerDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("moneytacker-db"));
});

builder.AddServiceDefaults();
builder.Services.AddOptions<AdminCredentialsOptions>()
    .Bind(builder.Configuration.GetSection(AdminCredentialsOptions.SectionName))
    .Validate(options => builder.Environment.IsDevelopment() || !string.IsNullOrWhiteSpace(options.Username),
        "Auth:Username must be configured outside Development.")
    .Validate(options => builder.Environment.IsDevelopment() || !string.IsNullOrWhiteSpace(options.Password),
        "Auth:Password must be configured outside Development.")
    .ValidateOnStart();
builder.Services.AddHostedService<MigrationWorker>();

var host = builder.Build();

var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
var serviceVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "unknown";
logger.LogInformation(
    new EventId(4201, "MigrationServiceStartup"),
    "Starting {ServiceName} v{ServiceVersion} in {EnvironmentName} environment.",
    builder.Environment.ApplicationName,
    serviceVersion,
    builder.Environment.EnvironmentName);

host.Run();