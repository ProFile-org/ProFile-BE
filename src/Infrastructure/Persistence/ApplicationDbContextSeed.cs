using Application.Helpers;
using Application.Identity;
using Domain.Entities;
using Infrastructure.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NodaTime;
using Serilog;

namespace Infrastructure.Persistence;

public class ApplicationDbContextSeed
{
    public static async Task Seed(ApplicationDbContext context, IConfiguration configuration, ILogger logger)
    {
        if (!configuration.GetValue<bool>("Seed")) return;

        try
        {
            await TrySeedAsync(context);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private static async Task TrySeedAsync(ApplicationDbContext context)
    {
        var department = new Department()
        {
            Name = "Admin"
        };
        
        // Default users
        var admin = new User
        {
            Username = "admin", 
            Email = "admin@profile.dev", 
            PasswordHash = SecurityUtil.Hash("admin"),
            IsActive = true,
            IsActivated = true,
            Created = LocalDateTime.FromDateTime(DateTime.UtcNow),
            Role = IdentityData.Roles.Admin,
        };
        
        if (context.Departments.All(u => u.Name != department.Name))
        {
            await context.Departments.AddAsync(department);
            if (context.Users.All(u => u.Username != admin.Username))
            {
                admin.Department = department;
                await context.Users.AddAsync(admin);
            }
        }
        else
        {
            var departmentEntity = context.Departments.Single(x => x.Name.Equals(department.Name));
            if (context.Users.All(u => u.Username != admin.Username))
            {
                admin.Department = departmentEntity;
                await context.Users.AddAsync(admin);
            }
        }

        await context.SaveChangesAsync();
    }
}