var builder = DistributedApplication.CreateBuilder(args);

var sqlDatabse = builder.AddSqlServer("moneytracker-sqlserver")
                     .WithDataVolume("moneyTrackerSqlDataVolume")
                     .AddDatabase("moneytacker-db");

var migrationService = builder.AddProject<Projects.MoneyTracker_Data_MigrationService>("moneytracker-data-migrationservice")
                            .WithReference(sqlDatabse)
                            .WaitFor(sqlDatabse);

var api = builder.AddProject<Projects.MoneyTracker_Api>("moneytracker-api")
                 .WithReference(sqlDatabse)
                 .WithHttpHealthCheck("/health")
                 .WaitFor(migrationService);

var frontend = builder.AddExecutable("moneytracker-frontend", "node", "../MoneyTracker.Frontend", "./start.mjs")
                      .WithReference(api)
                      .WithEnvironment("MONEYTRACKER_API_URL", api.GetEndpoint("http"))
                      .WithHttpEndpoint(env: "PORT")
                      .WithExternalHttpEndpoints()
                      .WaitFor(api);

builder.Build().Run();
