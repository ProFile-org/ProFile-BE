using System.Reflection;
using Api.Middlewares;
using Api.Policies;
using Api.Services;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.OpenApi.Models;

namespace Api;

public static class ConfigureServices
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        // Register services
        services.AddServices();
        services.AddBackgroundServices();
        services.AddControllers(opt =>
            opt.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer())));

        services.AddHttpContextAccessor();
        
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAllOrigins", builder =>
            {
                builder.WithOrigins("http://localhost:3000");
                builder.AllowAnyHeader();
                builder.AllowAnyMethod();
                builder.AllowCredentials();
            });
        });

        services.AddHttpContextAccessor();
        
        // For swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "ProFile API",
                Description = "An ASP.NET Core Web API for managing documents",
            });
            
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        });
        
        return services;
    }
    
    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        // In order for ExceptionMiddleware to work
        services.AddScoped<ExceptionMiddleware>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }

    private static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<BorrowRequestService>();

        return services;
    }
}