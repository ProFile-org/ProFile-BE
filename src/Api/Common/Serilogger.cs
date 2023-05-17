using Serilog;

namespace Api.Common;

public static class Serilogger
{
    public static Action<HostBuilderContext, LoggerConfiguration> Configure =>
        (context, configuration) =>
    {
        // Fetch appsettings based on environment
        var applicationName = context.HostingEnvironment.ApplicationName?.ToLower().Replace(".", "-");
        var environmentName = context.HostingEnvironment.EnvironmentName ?? "Development";

        // Configure structured logging
        configuration
            .WriteTo.Debug()
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithProperty("Environment", environmentName)
            .Enrich.WithProperty("Application", applicationName)
            .ReadFrom.Configuration(context.Configuration);
    };
}