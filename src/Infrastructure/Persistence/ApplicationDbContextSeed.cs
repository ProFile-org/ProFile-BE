using Microsoft.Extensions.Configuration;
using Serilog;

namespace Infrastructure.Persistence;

public class ApplicationDbContextSeed
{
    public static async Task Seed(ApplicationDbContext context, IConfiguration configuration, ILogger logger)
    {
        try
        {
            // Note: For later uses when I actually have a working hash function
            // await TrySeedAsync(context, configuration);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task TrySeedAsync(ApplicationDbContext context, IConfiguration configuration)
    {
        // Note: uncomment this
    //     // Default roles
    //     var administratorRole = "Administrator";
    //
    //     // Default users
    //     var administrator = new User { Username = "admin", Email = "administrator@localhost", PasswordHash = };
    //
    //     if (context.Users.All(u => u.Username != administrator.Username))
    //     {
    //         administrator.Role = administratorRole;
    //         await context.Users.AddAsync(administrator);
    //     }
    }
}