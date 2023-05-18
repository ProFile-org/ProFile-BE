using System.Data;
using Application.Common.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplicationDbContext(configuration);
        services.AddScoped<IApplicationDbContext, ApplicationDbContext>();

        return services;
    }

    private static IServiceCollection AddApplicationDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetSection(
                $"{nameof(DatabaseSettings)}:{nameof(DatabaseSettings.ConnectionString)}")
            .Value;
        
        services.AddDbContext<ApplicationDbContext>(builder =>
        {
            builder.UseNpgsql(connectionString, options =>
            {
                options.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                options.UseNodaTime();
            });
        });

        return services;
    }
}