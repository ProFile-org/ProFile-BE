using Api.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

try
{
    builder.Host.AddAppConfigurations();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
    }
    else
    {
    }

    app.MapControllers();

    app.MapGet("/", () => "Hello from ProFile!");

    app.Run();
}
catch (Exception ex)
{
    var error = ex.GetType().Name;
    if (error.Equals("StopTheHostException", StringComparison.Ordinal))
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