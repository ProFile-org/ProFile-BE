using System.Text;
using Application.Helpers;
using Bogus;
using Domain.Common;
using Domain.Entities;
using Domain.Entities.Digital;
using Domain.Entities.Physical;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Xunit;

namespace Application.Tests.Integration;

[Collection(nameof(BaseCollectionFixture))]
public class BaseClassFixture
{
    protected static IServiceScopeFactory ScopeFactory = null!;

    protected BaseClassFixture(CustomApiFactory apiFactory)
    {
        ScopeFactory = apiFactory.Services.GetRequiredService<IServiceScopeFactory>();
    }

    protected static async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        using var scope = ScopeFactory.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        return await mediator.Send(request);
    }

    protected void Remove<TEntity>(TEntity entity) where TEntity : BaseEntity?
    {
        using var scope = ScopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbContext.Set<TEntity>().Remove(entity);
        dbContext.SaveChanges();
    }

    protected static async Task<TEntity?> FindAsync<TEntity>(params object[] keyValues)
        where TEntity : class
    {
        using var scope = ScopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.FindAsync<TEntity>(keyValues);
    }

    protected static async Task AddAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        using var scope = ScopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.AddAsync(entity);

        await context.SaveChangesAsync();
    }

    protected static async Task Add<TEntity>(TEntity entity)
        where TEntity : class
    {
        using var scope = ScopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Add(entity);

        await context.SaveChangesAsync();
    }

    protected static async Task<int> CountAsync<TEntity>() where TEntity : class
    {
        using var scope = ScopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.Set<TEntity>().CountAsync();
    }

    protected Document[] CreateNDocuments(int n)
    {
        var list = new List<Document>();
        for (var i = 0; i < n; i++)
        {
            var document = new Document()
            {
                Id = Guid.NewGuid(),
                Title = new Faker().Commerce.ProductName(),
                DocumentType = "Something Department",
            };
            list.Add(document);
        }

        return list.ToArray();
    }

    protected Folder CreateFolder(params Document[] documents)
    {
        var folder = new Folder()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfDocuments = documents.Length,
            IsAvailable = true
        };

        foreach (var document in documents)
        {
            folder.Documents.Add(document);
        }

        return folder;
    }

    protected Locker CreateLocker(params Folder[] folders)
    {
        var locker = new Locker()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfFolders = folders.Length,
            IsAvailable = true
        };

        foreach (var folder in folders)
        {
            locker.Folders.Add(folder);
        }

        return locker;
    }

    protected Room CreateRoom(Department department, params Locker[] lockers)
    {
        var room = new Room()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfLockers = lockers.Length,
            IsAvailable = true,
            Department = department,
            DepartmentId = department.Id,
        };

        foreach (var locker in lockers)
        {
            room.Lockers.Add(locker);
        }

        return room;
    }

    protected static Department CreateDepartment()
    {
        return new Department()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Random.Word()
        };
    }

    protected static User CreateUser(string role, string password)
    {
        return new User()
        {
            Id = Guid.NewGuid(),
            Username = new Faker().Person.UserName,
            Email = new Faker().Person.Email,
            FirstName = new Faker().Person.FirstName,
            LastName = new Faker().Person.LastName,
            Role = role,
            Position = new Faker().Random.Word(),
            IsActivated = true,
            IsActive = true,
            Created = LocalDateTime.FromDateTime(DateTime.Now),
            PasswordHash = SecurityUtil.Hash(password)
        };
    }

    protected static Staff CreateStaff(User user, Room? room)
    {
        return new Staff()
        {
            Id = user.Id,
            User = user,
            Room = room,
        };
    }
    
    protected static UserGroup CreateUserGroup(User[] users)
    {
        return new UserGroup()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Users = users,
        };
    }
    
    protected static FileEntity CreateFile()
    {
        return new FileEntity()
        {
            Id = Guid.NewGuid(),
            FileType = new Faker().Database.Type(),
            FileData = Encoding.ASCII.GetBytes(new Faker().Lorem.Random.Words())
        };
    }
    
    protected static Entry CreateEntry(FileEntity file)
    {
        return new Entry()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            File = file,
            Path = new Faker().Commerce.ProductDescription(),
            FileId = file.Id,
        };
    }
}