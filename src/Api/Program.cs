using Api.Extensions;
using Infrastructure.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

try
{
    builder.Host.AddAppConfigurations(builder.Configuration);
    
    // Add services related infrastructure
    builder.Services.AddInfrastructure(builder.Configuration);
    
    var app = builder.Build();
    
    app.UseInfrastructure();

    app.MigrateDatabase<ApplicationDbContext>((context, _) =>
        {
            ApplicationDbContextSeed.Seed(context, builder.Configuration, Log.Logger).Wait();
        })
        .Run();
}
catch (Exception ex)
{
    // Handle an error related to .NET 6
    // https://github.com/dotnet/runtime/issues/60600
    var error = ex.GetType().Name;
    if (error.Equals("HostAbortedException", StringComparison.Ordinal))
    {
        throw;
    }
    
    Log.Fatal($"Unhandled exception: {ex}");
}
finally
{
    Log.Information($"Shut down {builder.Environment.ApplicationName} complete");
    Log.CloseAndFlush();
}