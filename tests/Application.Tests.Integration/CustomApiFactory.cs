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
            connectionStringBuilder.Host = "localhost";
            connectionStringBuilder.Database = "mytestdb";
            connectionStringBuilder.Port = 5423;
            connectionStringBuilder.Username = "profiletester";
            connectionStringBuilder.Password = "supasupasecured";
            Console.WriteLine(connectionStringBuilder.ConnectionString);
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(connectionStringBuilder.ConnectionString , optionsBuilder => optionsBuilder.UseNodaTime());
            });
        });
    }
}