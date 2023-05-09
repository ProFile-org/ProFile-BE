using Api.Common;
using Serilog;

namespace Api.Extensions;

public static class HostExtensions
{
    public static IHostBuilder AddAppConfigurations(this IHostBuilder host)
    {
        return host.UseSerilog(Serilogger.Configure);
    }
}