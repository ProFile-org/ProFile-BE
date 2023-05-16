using System.Data;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplicationDbContext(configuration);

        services.AddScoped<IDbConnection>(s =>
        {
            var dbContext = s.GetRequiredService<DbContext>();
            return dbContext.Database.GetDbConnection();
        });

        services.AddScoped<IDbTransaction>(s =>
        {
            var connection = s.GetRequiredService<IDbConnection>();
            return connection.BeginTransaction();
        });

        services.RegisterRepositories();
        
        return services;
    }

    private static IServiceCollection AddApplicationDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseSettings = configuration.GetSection(nameof(DatabaseSettings));
        var connectionString = databaseSettings.GetConnectionString(nameof(DatabaseSettings.ConnectionString));
        
        services.AddDbContext<ApplicationDbContext>(builder =>
        {
            builder.UseNpgsql(connectionString, builder =>
            {
                builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
            });
        });

        return services;
    }

    private static IServiceCollection RegisterRepositories(this IServiceCollection services)
    {
        services.AddScoped<ISampleRepository, SampleRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}