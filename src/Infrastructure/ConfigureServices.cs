using System.Data;
using System.Security.Cryptography;
using System.Text;
using Application.Common.Interfaces;
using Infrastructure.Identity;
using Infrastructure.Identity.Authentication;
using Infrastructure.Persistence;
using Infrastructure.Shared;
using Microsoft.AspNetCore.Authentication.Cookies;
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
        services.AddScoped<IIdentityService, IdentityService>();

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
            options.EncryptionKeyId = jweSettings!.EncryptionKeyId;
            options.SigningKeyId = jweSettings.SigningKeyId;
            options.TokenLifetime = jweSettings.TokenLifetime;
            options.RefreshTokenLifetimeInDays = jweSettings.RefreshTokenLifetimeInDays;
        });

        var encryptionKey = RSA.Create(3072);
        var signingKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        var privateEncryptionKey = new RsaSecurityKey(encryptionKey) {KeyId = jweSettings!.EncryptionKeyId};
        var publicSigningKey = new ECDsaSecurityKey(ECDsa.Create(signingKey.ExportParameters(false))) {KeyId = jweSettings.SigningKeyId};

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,
            // public key for signing
            IssuerSigningKey = publicSigningKey,

            // private key for encryption
            TokenDecryptionKey = privateEncryptionKey,
        };
        
        services.AddSingleton(encryptionKey);
        services.AddSingleton(signingKey);
        services.AddSingleton(tokenValidationParameters);

        services.AddAuthentication(JweAuthenticationOptions.DefaultScheme)
            .AddScheme<JweAuthenticationOptions, JweAuthenticationHandler>(JweAuthenticationOptions.DefaultScheme,
                options =>
                {
                    options.TokenValidationParameters = tokenValidationParameters;
                });
        
        return services;    
    }
}