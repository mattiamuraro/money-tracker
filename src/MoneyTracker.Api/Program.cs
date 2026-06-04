using MoneyTracker.Api.ExtensionMethods;

var builder = WebApplication.CreateBuilder(args);
builder.AddApiApplicationServices();

var app = builder.Build();
app.LogApiStartup();
app.UseApiMiddlewarePipeline();
app.MapApiEndpoints();

app.Run();

// Expose Program to integration tests via WebApplicationFactory<Program>
public partial class Program { }
