using Application.Helpers;
using Application.Identity;
using Domain.Entities;
using Infrastructure.Shared;
using Microsoft.Extensions.Configuration;
using NodaTime;
using Serilog;

namespace Infrastructure.Persistence;

public class ApplicationDbContextSeed
{
    public static async Task Seed(ApplicationDbContext context, IConfiguration configuration, ILogger logger)
    {
        if (!configuration.GetValue<bool>("Seed")) return;
        
        var securitySettings = configuration.GetSection(nameof(SecuritySettings)).Get<SecuritySettings>();
        try
        {
            await TrySeedAsync(context, securitySettings!.Pepper);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private static async Task TrySeedAsync(ApplicationDbContext context, string pepper)
    {
        var department = new Department()
        {
            Name = "Admin"
        };

        var salt = StringUtil.RandomSalt();
        
        // Default users
        var admin = new User
        {
            Username = "admin", 
            Email = "admin@profile.dev", 
            PasswordHash = "admin".HashPasswordWith(salt, pepper),
            PasswordSalt = salt,
            IsActive = true,
            IsActivated = true,
            Created = LocalDateTime.FromDateTime(DateTime.UtcNow),
            Role = IdentityData.Roles.Admin,
        };
        
        salt = StringUtil.RandomSalt();
        var staff = new User()
        {
            Username = "staff", 
            Email = "staff@profile.dev", 
            PasswordHash = "staff".HashPasswordWith(salt, pepper),
            PasswordSalt = salt,
            IsActive = true,
            IsActivated = true,
            Created = LocalDateTime.FromDateTime(DateTime.UtcNow),
            Role = IdentityData.Roles.Staff,
        };
        
        var itDepartment = new Department()
        {
            Name = "IT"
        };
        salt = StringUtil.RandomSalt();
        var employee = new User()
        {
            Username = "employee", 
            Email = "employee@profile.dev", 
            PasswordHash = "employee".HashPasswordWith(salt, pepper),
            PasswordSalt = salt,
            IsActive = true,
            IsActivated = true,
            Created = LocalDateTime.FromDateTime(DateTime.UtcNow),
            Role = IdentityData.Roles.Employee,
        };
        
        if (context.Departments.All(u => u.Name != department.Name))
        {
            await context.Departments.AddAsync(department);
            if (context.Users.All(u => u.Username != admin.Username))
            {
                admin.Department = department;
                await context.Users.AddAsync(admin);
            }
            if (context.Users.All(u => u.Username != staff.Username))
            {
                staff.Department = department;
                await context.Users.AddAsync(staff);
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
            if (context.Users.All(u => u.Username != staff.Username))
            {
                staff.Department = departmentEntity;
                await context.Users.AddAsync(staff);
            }
        }
        
        if (context.Departments.All(u => u.Name != itDepartment.Name))
        {
            await context.Departments.AddAsync(itDepartment);
            if (context.Users.All(u => u.Username != employee.Username))
            {
                employee.Department = itDepartment;
                await context.Users.AddAsync(employee);
            }
        }
        else
        {
            var departmentEntity = context.Departments.Single(x => x.Name.Equals(itDepartment.Name));
            if (context.Users.All(u => u.Username != employee.Username))
            {
                employee.Department = departmentEntity;
                await context.Users.AddAsync(employee);
            }
        }

        await context.SaveChangesAsync();
    }
}