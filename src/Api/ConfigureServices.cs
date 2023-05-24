using Api.Middlewares;
using Api.Policies;
using Api.Services;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Api;

public static class ConfigureServices
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        // Register services
        services.AddServices();
        
        services.AddControllers(opt =>
            opt.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer())));

        services.AddCors(options =>
        {
            options.AddPolicy("AllowAllOrigins", builder =>
            {
                builder.AllowAnyOrigin();
                builder.AllowAnyHeader();
                builder.AllowAnyMethod();
            });
        });

        services.AddHttpContextAccessor();
        
        // For swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        
        return services;
    }
    
    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        // In order for ExceptionMiddleware to work
        services.AddScoped<ExceptionMiddleware>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        return services;
    }
}