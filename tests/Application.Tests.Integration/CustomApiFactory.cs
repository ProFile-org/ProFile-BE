using Api;
using Application.Common.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Shared;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Application.Tests.Integration;

public class CustomApiFactory : WebApplicationFactory<IApiMarker>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {        
        builder.ConfigureServices((builderContext, services) =>
        {
            Remove<DbContextOptions<ApplicationDbContext>>(services);

            var databaseSettings = GetConfiguration().GetSection(nameof(DatabaseSettings)).Get<DatabaseSettings>();
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(databaseSettings!.ConnectionString, optionsBuilder => optionsBuilder.UseNodaTime());
            });
            
            Remove<IMailService>(services);
            services.AddTransient<IMailService, CustomMailService>();
        });
    }

    private static void Remove<T>(IServiceCollection services) 
        where T : class
    {
        var descriptor = services.SingleOrDefault(
            d => d.ServiceType ==
                 typeof(T));

        if (descriptor != null)
        {
            services.Remove(descriptor);
        }
    }

    private IConfiguration GetConfiguration()
    {
        return new ConfigurationBuilder()
            .AddEnvironmentVariables("PROFILE_").Build();
    }
}