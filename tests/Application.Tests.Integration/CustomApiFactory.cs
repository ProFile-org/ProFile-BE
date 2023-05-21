using Api;
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
        // builder.ConfigureAppConfiguration(configurationBuilder =>
        // {
        //     var integrationConfig = new ConfigurationBuilder()
        //         .AddJsonFile("appsettings.Test.json")
        //         .AddEnvironmentVariables()
        //         .Build();
        //
        //     configurationBuilder.AddConfiguration(integrationConfig);
        // });
        
        builder.ConfigureServices((builderContext, services) =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                     typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            var connectionStringBuilder = new NpgsqlConnectionStringBuilder();
            connectionStringBuilder.Host = "database";
            connectionStringBuilder.Database = "mytestdb";
            connectionStringBuilder.Port = 5432;
            connectionStringBuilder.Username = "profiletester";
            connectionStringBuilder.Password = "supasupasecured";
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql("Server=database;Port=5432;Database=mytestdb;User ID=profiletester;Password=supasupasecured" , optionsBuilder => optionsBuilder.UseNodaTime());
            });
        });
    }
}