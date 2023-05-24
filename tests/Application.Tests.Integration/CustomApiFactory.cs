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
using NSubstitute;

namespace Application.Tests.Integration;

public class CustomApiFactory : WebApplicationFactory<IApiMarker>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {        
        builder.ConfigureServices((builderContext, services) =>
        {

            var databaseSettings = GetConfiguration().GetSection(nameof(DatabaseSettings)).Get<DatabaseSettings>();
            services
                .Remove<DbContextOptions<ApplicationDbContext>>()
                .AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(databaseSettings.ConnectionString, optionsBuilder => optionsBuilder.UseNodaTime());
            });
        });
    }

    private IConfiguration GetConfiguration()
    {
        return new ConfigurationBuilder()
            .AddEnvironmentVariables("PROFILE_").Build();
    }
}