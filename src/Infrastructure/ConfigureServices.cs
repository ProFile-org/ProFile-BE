using System.Data;
using Application.Common.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
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
        services.AddScoped<DbContext>(s => s.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IDbConnection>(s =>
        {
            var dbContext = s.GetRequiredService<ApplicationDbContext>();
            return dbContext.Database.GetDbConnection();
        });

        services.AddScoped<IDbTransaction>(s =>
        {
            var connection = s.GetRequiredService<IDbConnection>();
            connection.Open();
            return connection.BeginTransaction();
        });

        services.RegisterRepositories();
        
        return services;
    }

    private static IServiceCollection AddApplicationDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetSection(
                $"{nameof(DatabaseSettings)}:{nameof(DatabaseSettings.ConnectionString)}")
            .Value;
        
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
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}