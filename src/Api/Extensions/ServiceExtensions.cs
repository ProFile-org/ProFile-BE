using Api.Middlewares;
using Application;
using Infrastructure;

namespace Api.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add infrastructure project services
        services.AddApplicationServices();
        services.AddInfrastructureServices(configuration);
        
        // Register services
        services.AddServices();
        
        services.AddControllers();

        // For swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        
        return services;
    }
    
    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        // In order for ExceptionMiddleware to work
        services.AddScoped<ExceptionMiddleware>();

        return services;
    }
}