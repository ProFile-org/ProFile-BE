using System.Data;
using System.Security.Cryptography;
using System.Text;
using Application.Common.Interfaces;
using Infrastructure.Identity.Authentication;
using Infrastructure.Persistence;
using Infrastructure.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplicationDbContext(configuration);
        services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
        
        services.AddJweAuthentication(configuration);

        services.AddAuthorization();
        
        return services;
    }

    private static IServiceCollection AddApplicationDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseSettings = configuration.GetSection(nameof(DatabaseSettings)).Get<DatabaseSettings>();
        
        services.AddDbContext<ApplicationDbContext>(builder =>
        {
            builder.UseNpgsql(databaseSettings.ConnectionString, options =>
            {
                options.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                options.UseNodaTime();
            });
        });

        return services;
    }

    private static IServiceCollection AddJweAuthentication(this IServiceCollection services,
        IConfiguration configuration)
    {
        var jweSettings = configuration.GetSection(nameof(JweSettings)).Get<JweSettings>();
        
        services.Configure<JweSettings>(options =>
        {
            options.EncryptionKeyId = jweSettings.EncryptionKeyId;
            options.SigningKeyId = jweSettings.SigningKeyId;
        });

        services.AddSingleton<RSA>(_ => RSA.Create(3072));
        services.AddSingleton<ECDsa>(_ => ECDsa.Create(ECCurve.NamedCurves.nistP256));
        
        services.AddAuthentication(JweAuthenticationOptions.DefaultScheme)
            .AddScheme<JweAuthenticationOptions, JweAuthenticationHandler>(JweAuthenticationOptions.DefaultScheme,
                _ => { });
        
        return services;    
    }
}