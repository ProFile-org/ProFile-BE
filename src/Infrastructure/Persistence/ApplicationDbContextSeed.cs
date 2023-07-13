using Application.Helpers;
using Application.Identity;
using Bogus;
using Domain.Entities;
using Domain.Entities.Physical;
using Domain.Statuses;
using Infrastructure.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NodaTime;
using Org.BouncyCastle.Crypto.Macs;
using Serilog;
using Serilog.Context;

namespace Infrastructure.Persistence;

public static class ApplicationDbContextSeed
{
    public static async Task Seed(ApplicationDbContext context, IConfiguration configuration, ILogger logger)
    {
        if (!configuration.GetValue<bool>("Seed")) return;

        var securitySettings = configuration.GetSection(nameof(SecuritySettings)).Get<SecuritySettings>();
        try
        {
            await TrySeedAsync(context, securitySettings!.Pepper, logger);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private static async Task TrySeedAsync(ApplicationDbContext context, string pepper, ILogger logger)
    {
        await context.SeedAdminDepartment(pepper);
        await context.SeedItDepartment(pepper);
        await context.SeedHrDepartment(pepper);
        await context.SeedAccountingDepartment(pepper);

        await context.SaveChangesAsync();
    }

    private static async Task SeedAdminDepartment(this ApplicationDbContext context, string pepper)
    {
        var salt = StringUtil.RandomSalt();

        var adminDepartment = new Department()
        {
            Name = "Admin"
        };

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

        await context.TryAddDepartmentWith(adminDepartment,
            new List<User>()
            {
                admin
            },
            Enumerable.Empty<User>());
    }

    private static async Task SeedItDepartment(this ApplicationDbContext context, string pepper)
    {
        var itDepartment = new Department()
        {
            Name = "IT"
        };
        
        var employee1 = CreateEmployee("employee", "employee@profile.dev", "employee", pepper);
        var employee2 = CreateRandomEmployee("johndoe", pepper);
        var employee3 = CreateRandomEmployee("johnnysins", pepper);

        var staffUser = CreateStaff("staff", "staff@profile.dev", "staff", pepper);
        var staffUser2 = CreateStaff("staff2", "staff2@profile.dev", "staff", pepper);
        
        await context.TryAddDepartmentWith(itDepartment,
            new List<User>()
            {
                employee1,
                employee2,
                employee3,
            },
            new List<User>()
            {
                staffUser,
                staffUser2,
            });

        var d1 = CreateNDocument(employee1, itDepartment, 5);
        d1.AddRange(CreateNDocument(employee2, itDepartment, 4));
        var folder1 = CreateFolder("Short-term contracts 1", "Short term contracts", 20,d1);
        var d2 = CreateNDocument(employee3, itDepartment, 8);
        d2.AddRange(CreateNDocument(employee2, itDepartment, 7));
        var folder2 = CreateFolder("Short-term contracts 2", "Short-term contracts 2", 15, 
            CreateNDocument(employee2, itDepartment, 7));
        var d3 = CreateNDocument(employee1, itDepartment, 3);
        d3.AddRange(CreateNDocument(employee2, itDepartment, 7));
        d3.AddRange(CreateNDocument(employee3, itDepartment, 4));
        var folder3 = CreateFolder("Long-term contracts 1", "Long-term contracts 1", 30, d3);
        var d4 = CreateNDocument(employee1, itDepartment, 2);
        d4.AddRange(CreateNDocument(employee2, itDepartment, 9));
        d4.AddRange(CreateNDocument(employee3, itDepartment, 4));
        var folder4 = CreateFolder("Long-term contracts 2", "Long-term contracts 2", 20, d4);
        var d5 = CreateNDocument(employee1, itDepartment, 5);
        d5.AddRange(CreateNDocument(employee2, itDepartment, 1));
        d5.AddRange(CreateNDocument(employee3, itDepartment, 8));
        var folder5 = CreateFolder("Long-term contracts 3", "Long-term contracts 3", 20, d5);
        var d6 = CreateNDocument(employee1, itDepartment, 5);
        d6.AddRange(CreateNDocument(employee2, itDepartment, 5));
        d6.AddRange(CreateNDocument(employee3, itDepartment, 5));
        var folder6 = CreateFolder("SRS", "Software requirement specification", 20, d6);
        var d7 = CreateNDocument(employee1, itDepartment, 1);
        d7.AddRange(CreateNDocument(employee2, itDepartment, 1));
        d7.AddRange(CreateNDocument(employee3, itDepartment, 12));
        var folder7 = CreateFolder("SDS", "Software design specification", 20, d7);
        var d8 = CreateNDocument(employee1, itDepartment, 7);
        d8.AddRange(CreateNDocument(employee2, itDepartment, 5));
        d8.AddRange(CreateNDocument(employee3, itDepartment, 1));
        var folder8 = CreateFolder("Private invoices", "Private invoices", 20, d8);
        var d9 = CreateNDocument(employee1, itDepartment, 5);
        d9.AddRange(CreateNDocument(employee2, itDepartment, 8));
        d9.AddRange(CreateNDocument(employee3, itDepartment, 1));
        var folder9 = CreateFolder("Public invoices", "Public invoices", 20, d9);

        var locker1 = CreateLocker("Contracts", "Contracts", 20, new List<Folder>()
        {
            folder1,
            folder2,
            folder3,
            folder4,
            folder5,
        });
        var locker2 = CreateLocker("Specifications", "All types of specifications", 20, new List<Folder>()
        {
            folder6,
            folder7,
        });
        var locker3 = CreateLocker("Invoices", "Invoices related to products", 20, new List<Folder>()
        {
            folder8,
            folder9,
        });
        
        var locker4 = CreateLocker("Papers", "All research papers related", 20, new List<Folder>()
        {
            
        });
        var locker5 = CreateLocker("Reports", "Annually, Monthly, Weekly reports", 20, new List<Folder>());
        await context.TryAddRoom("IT Storage", "Storage for IT Department", 40, itDepartment, staffUser, new List<Locker>()
        {
            locker1,
            locker2,
            locker3,
        });
        await context.TryAddRoom("IT Storage 2", "Storage 2 for IT Department", 30, itDepartment, staffUser2, new List<Locker>()
        {
            locker4,
            locker5,
        });
    }

    private static async Task SeedHrDepartment(this ApplicationDbContext context, string pepper)
    {
        var hrDepartment = new Department
        {
            Name = "Human Resource"
        };

        var employee1 = CreateRandomEmployee("cuonglt2", pepper);
        var employee2 = CreateRandomEmployee("thanht", pepper);
        var employee3 = CreateRandomEmployee("chaum", pepper);
        var employee4 = CreateRandomEmployee("tuantt", pepper);

        var staff = CreateStaff("maint", "maint@profile.dev", "employee", pepper);
        
        await context.TryAddDepartmentWith(hrDepartment,
            new List<User>()
            {
                employee1,
                employee2,
                employee3,
                employee4,
            },
            new List<User>()
            {
                staff,
            });
        
        var d1 = CreateNDocument(employee1, hrDepartment, 5);
        d1.AddRange(CreateNDocument(employee2, hrDepartment, 4));
        var folder1 = CreateFolder("Short-term contracts 1", "Short term contracts", 20,d1);
        var d2 = CreateNDocument(employee3, hrDepartment, 8);
        d2.AddRange(CreateNDocument(employee2, hrDepartment, 7));
        var folder2 = CreateFolder("Short-term contracts 2", "Short-term contracts 2", 15, 
            CreateNDocument(employee2, hrDepartment, 7));
        var d3 = CreateNDocument(employee1, hrDepartment, 3);
        d3.AddRange(CreateNDocument(employee2, hrDepartment, 7));
        d3.AddRange(CreateNDocument(employee3, hrDepartment, 4));
        var folder3 = CreateFolder("Long-term contracts 1", "Long-term contracts 1", 30, d3);
        var d4 = CreateNDocument(employee1, hrDepartment, 2);
        d4.AddRange(CreateNDocument(employee2, hrDepartment, 9));
        d4.AddRange(CreateNDocument(employee3, hrDepartment, 4));
        var folder4 = CreateFolder("Long-term contracts 2", "Long-term contracts 2", 20, d4);
        var d5 = CreateNDocument(employee1, hrDepartment, 5);
        d5.AddRange(CreateNDocument(employee2, hrDepartment, 1));
        d5.AddRange(CreateNDocument(employee3, hrDepartment, 8));
        var folder5 = CreateFolder("Long-term contracts 3", "Long-term contracts 3", 20, d5);
        var d6 = CreateNDocument(employee1, hrDepartment, 5);
        d6.AddRange(CreateNDocument(employee2, hrDepartment, 5));
        d6.AddRange(CreateNDocument(employee3, hrDepartment, 5));
        var folder6 = CreateFolder("SRS", "Software requirement specification", 20, d6);
        var d7 = CreateNDocument(employee1, hrDepartment, 1);
        d7.AddRange(CreateNDocument(employee2, hrDepartment, 1));
        d7.AddRange(CreateNDocument(employee3, hrDepartment, 12));
        var folder7 = CreateFolder("SDS", "Software design specification", 20, d7);
        var d8 = CreateNDocument(employee1, hrDepartment, 7);
        d8.AddRange(CreateNDocument(employee2, hrDepartment, 5));
        d8.AddRange(CreateNDocument(employee3, hrDepartment, 1));
        var folder8 = CreateFolder("Private invoices", "Private invoices", 20, d8);
        var d9 = CreateNDocument(employee1, hrDepartment, 5);
        d9.AddRange(CreateNDocument(employee2, hrDepartment, 8));
        d9.AddRange(CreateNDocument(employee3, hrDepartment, 1));
        var folder9 = CreateFolder("Public invoices", "Public invoices", 20, d9);

        var locker1 = CreateLocker("Contracts", "Contracts", 20, new List<Folder>()
        {
            folder1,
            folder2,
            folder3,
            folder4,
            folder5,
        });
        var locker2 = CreateLocker("Specifications", "All types of specifications", 20, new List<Folder>()
        {
            folder6,
            folder7,
        });
        var locker3 = CreateLocker("Invoices", "Invoices related to products", 20, new List<Folder>()
        {
            folder8,
            folder9,
        });
        
        var locker4 = CreateLocker("Papers", "All research papers related", 20, new List<Folder>()
        {
            
        });
        var locker5 = CreateLocker("Reports", "Annually, Monthly, Weekly reports", 20, new List<Folder>());
        await context.TryAddRoom("HR Storage", "Storage for IT Department", 40, hrDepartment, staff, new List<Locker>()
        {
            locker1,
            locker2,
            locker3,
            locker4,
            locker5,
        });
    }
    
    private static async Task SeedAccountingDepartment(this ApplicationDbContext context, string pepper)
    {
        var accountingDepartment = new Department
        {
            Name = "Accounting"
        };

        var employee1 = CreateRandomEmployee("peterparker", pepper);
        var employee2 = CreateRandomEmployee("maryjane", pepper);
        var employee3 = CreateRandomEmployee("gwenstacy", pepper);
        var employee4 = CreateRandomEmployee("miles", pepper);

        var staff = CreateStaff("eddie", "eddie@profile.dev", "employee", pepper);
        
        await context.TryAddDepartmentWith(accountingDepartment,
            new List<User>()
            {
                employee1,
                employee2,
                employee3,
                employee4,
            },
            new List<User>()
            {
                staff,
            });
        
        
        var d1 = CreateNDocument(employee1, accountingDepartment, 5);
        d1.AddRange(CreateNDocument(employee2, accountingDepartment, 4));
        var folder1 = CreateFolder("Short-term contracts 1", "Short term contracts", 20,d1);
        var d2 = CreateNDocument(employee3, accountingDepartment, 8);
        d2.AddRange(CreateNDocument(employee2, accountingDepartment, 7));
        var folder2 = CreateFolder("Short-term contracts 2", "Short-term contracts 2", 15, 
            CreateNDocument(employee2, accountingDepartment, 7));
        var d3 = CreateNDocument(employee1, accountingDepartment, 3);
        d3.AddRange(CreateNDocument(employee2, accountingDepartment, 7));
        d3.AddRange(CreateNDocument(employee3, accountingDepartment, 4));
        var folder3 = CreateFolder("Long-term contracts 1", "Long-term contracts 1", 30, d3);
        var d4 = CreateNDocument(employee1, accountingDepartment, 2);
        d4.AddRange(CreateNDocument(employee2, accountingDepartment, 9));
        d4.AddRange(CreateNDocument(employee3, accountingDepartment, 4));
        var folder4 = CreateFolder("Long-term contracts 2", "Long-term contracts 2", 20, d4);
        var d5 = CreateNDocument(employee1, accountingDepartment, 5);
        d5.AddRange(CreateNDocument(employee2, accountingDepartment, 1));
        d5.AddRange(CreateNDocument(employee3, accountingDepartment, 8));
        var folder5 = CreateFolder("Long-term contracts 3", "Long-term contracts 3", 20, d5);
        var d6 = CreateNDocument(employee1, accountingDepartment, 5);
        d6.AddRange(CreateNDocument(employee2, accountingDepartment, 5));
        d6.AddRange(CreateNDocument(employee3, accountingDepartment, 5));
        var folder6 = CreateFolder("SRS", "Software requirement specification", 20, d6);
        var d7 = CreateNDocument(employee1, accountingDepartment, 1);
        d7.AddRange(CreateNDocument(employee2, accountingDepartment, 1));
        d7.AddRange(CreateNDocument(employee3, accountingDepartment, 12));
        var folder7 = CreateFolder("SDS", "Software design specification", 20, d7);
        var d8 = CreateNDocument(employee1, accountingDepartment, 7);
        d8.AddRange(CreateNDocument(employee2, accountingDepartment, 5));
        d8.AddRange(CreateNDocument(employee3, accountingDepartment, 1));
        var folder8 = CreateFolder("Private invoices", "Private invoices", 20, d8);
        var d9 = CreateNDocument(employee1, accountingDepartment, 5);
        d9.AddRange(CreateNDocument(employee2, accountingDepartment, 8));
        d9.AddRange(CreateNDocument(employee3, accountingDepartment, 1));
        var folder9 = CreateFolder("Public invoices", "Public invoices", 20, d9);

        var locker1 = CreateLocker("Contracts", "Contracts", 20, new List<Folder>()
        {
            folder1,
            folder2,
            folder3,
            folder4,
            folder5,
        });
        var locker2 = CreateLocker("Specifications", "All types of specifications", 20, new List<Folder>()
        {
            folder6,
            folder7,
        });
        var locker3 = CreateLocker("Invoices", "Invoices related to products", 20, new List<Folder>()
        {
            folder8,
            folder9,
        });
        
        var locker4 = CreateLocker("Papers", "All research papers related", 20, new List<Folder>()
        {
            
        });
        var locker5 = CreateLocker("Reports", "Annually, Monthly, Weekly reports", 20, new List<Folder>());
        await context.TryAddRoom("Accounting Storage", "Storage for Accounting Department", 40, accountingDepartment, staff, new List<Locker>()
        {
            locker1,
            locker2,
            locker3,
            locker4,
            locker5,
        });
    }

    private static User CreateRandomEmployee(string username, string pepper)
    {
        var salt = StringUtil.RandomSalt();
        return new User()
        {
            Username = username,
            Email = new Faker().Person.Email,
            FirstName = new Faker().Person.FirstName,
            LastName = new Faker().Person.LastName,
            PasswordHash = new Faker().Random.String().HashPasswordWith(salt, pepper),
            PasswordSalt = salt,
            IsActive = true,
            IsActivated = true,
            Created = LocalDateTime.FromDateTime(DateTime.UtcNow),
            Role = IdentityData.Roles.Employee,
        };
    }
    
    private static User CreateEmployee(string username, string email, string password, string pepper)
    {
        var salt = StringUtil.RandomSalt();
        return new User()
        {
            Username = username,
            Email = email,
            FirstName = new Faker().Person.FirstName,
            LastName = new Faker().Person.LastName,
            PasswordHash = password.HashPasswordWith(salt, pepper),
            PasswordSalt = salt,
            IsActive = true,
            IsActivated = true,
            Created = LocalDateTime.FromDateTime(DateTime.UtcNow),
            Role = IdentityData.Roles.Employee,
        };
    }
    
    private static User CreateStaff(string username, string email, string password, string pepper)
    {
        var salt = StringUtil.RandomSalt();
        return new User()
        {
            Username = username,
            Email = email,
            FirstName = new Faker().Person.FirstName,
            LastName = new Faker().Person.LastName,
            PasswordHash = password.HashPasswordWith(salt, pepper),
            PasswordSalt = salt,
            IsActive = true,
            IsActivated = true,
            Created = LocalDateTime.FromDateTime(DateTime.UtcNow),
            Role = IdentityData.Roles.Staff,
        };
    }

    private static async Task TryAddDepartmentWith(this ApplicationDbContext context, Department department,
        IEnumerable<User> users, IEnumerable<User> staffs)
    {
        if (context.Departments.All(u => u.Name != department.Name))
        {
            await context.Departments.AddAsync(department);
            await context.AddUsers(department, users);
            await context.AddStaffs(department, staffs);
        }
        else
        {
            var departmentEntity = context.Departments.Single(x => x.Name.Equals(department.Name));
            await context.AddUsers(departmentEntity, users);
            await context.AddStaffs(departmentEntity, staffs);
        }
    }
    
    private static async Task TryAddRoom(this ApplicationDbContext context, string name,
        string description, int capacity, Department department, User user, ICollection<Locker> lockers)
    {
        var staff = await context.Staffs.FirstOrDefaultAsync(x => x.User.Id == user.Id);
        if (context.Rooms.All(x => x.Name != name))
        {
            var room = new Room
            {
                Name = name,
                Department = department,
                DepartmentId = department.Id,
                Created = LocalDateTime.FromDateTime(DateTime.UtcNow),
                Staff = staff,
                Lockers = lockers,
                Capacity = capacity,
                NumberOfLockers = lockers.Count,
                IsAvailable = true,
                Description = description,
            };
            await context.Rooms.AddAsync(room);
        }
    }

    private static Locker CreateLocker(string name, string description, int capacity, ICollection<Folder> folders)
    {
        return new Locker
        {
            Name = name,
            Created = LocalDateTime.FromDateTime(DateTime.UtcNow),
            Capacity = capacity,
            IsAvailable = true,
            Description = description,
            Folders = folders,
            NumberOfFolders = folders.Count,
        };
    }
    
    private static Folder CreateFolder(string name, string description, int capacity, ICollection<Document> documents)
    {
        return new Folder
        {
            Name = name,
            Created = LocalDateTime.FromDateTime(DateTime.UtcNow),
            Capacity = capacity,
            IsAvailable = true,
            Description = description,
            Documents = documents,
            NumberOfDocuments = documents.Count,
        };
    }
    
    private static Document CreateDocument(User user, Department department)
    {
        return new Document
        {
            Title = new Faker().Name.JobArea(),
            Created = LocalDateTime.FromDateTime(DateTime.UtcNow),
            Description = new Faker().Commerce.ProductDescription(),
            DocumentType = new Faker().Commerce.Categories(1)[0],
            Department = department,
            Importer = user,
            ImporterId = user.Id,
            IsPrivate = new Faker().Random.Bool(),
            Status = Random(),
            CreatedBy = user.Id,
        };
    }

    private static DocumentStatus Random()
    {
        var r = new Random().Next(0, 4);

        return r switch
        {
            0 => DocumentStatus.Available,
            1 => DocumentStatus.Borrowed,
            2 => DocumentStatus.Issued,
            3 => DocumentStatus.Lost,
            _ => DocumentStatus.Available
        };
    }

    private static List<Document> CreateNDocument(User user, Department department, int n)
    {
        var list = new List<Document>();
        for (var i = 0; i < n; i++)
        {
            list.Add(CreateDocument(user,department));
        }

        return list;
    }

    private static async Task AddUsers(this ApplicationDbContext context, Department department,
        IEnumerable<User> users)
    {
        foreach (var user in users)
        {
            if (!context.Users.All(u => u.Username != user.Username)) continue;
            user.Department = department;
            await context.Users.AddAsync(user);
        }
    }
    
    private static async Task AddStaffs(this ApplicationDbContext context, Department department,
        IEnumerable<User> staffs)
    {
        foreach (var staff in staffs)
        {
            if (context.Users.All(u => u.Username != staff.Username))
            {
                staff.Department = department;
                await context.Users.AddAsync(staff);
            }

            var staffEntity = new Staff
            {
                User = staff,
                Room = null,
            };
            if (context.Staffs.All(s => s.User.Username != staff.Username))
            {
                await context.Staffs.AddAsync(staffEntity);
            }
        }
    }
}