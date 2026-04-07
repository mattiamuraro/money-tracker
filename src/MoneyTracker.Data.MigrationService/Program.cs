using Microsoft.EntityFrameworkCore;
using MoneyTracker.Data.EntityFramework;
using MoneyTracker.Data.MigrationService;


var builder = Host.CreateApplicationBuilder(args);


// Register the DbContext the same way as in the API
builder.Services.AddDbContext<MoneyTrackerDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("moneytacker-db"));
});

builder.AddServiceDefaults();
builder.Services.AddHostedService<MigrationWorker>();

var host = builder.Build();
host.Run();