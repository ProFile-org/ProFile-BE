using System.Reflection;
using Api.Common;
using Api.Middlewares;
using Api.Policies;
using Api.Services;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.OpenApi.Models;

namespace Api;

public static class ConfigureServices
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: Move this away in the future
        var frontendBaseUrl = configuration.GetValue<string>("BASE_FRONTEND_URL") ?? "http://localhost";
        
        // Register services
        services.AddServices();
        
        services.AddControllers(opt =>
            opt.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer())));

        services.AddHttpContextAccessor();
        
        services.AddCors(options =>
        {
            options.AddPolicy(CORSPolicy.Development, builder =>
            {
                builder.SetIsOriginAllowed(_ => true);
                builder.AllowAnyHeader();
                builder.AllowAnyMethod();
                builder.AllowCredentials();
            });
            
            options.AddPolicy(CORSPolicy.Production, builder =>
            {
                builder.WithOrigins(frontendBaseUrl);
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
}