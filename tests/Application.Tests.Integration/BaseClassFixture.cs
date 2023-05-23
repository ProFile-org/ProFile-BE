using Application.Departments.Commands.CreateDepartment;
using Application.Helpers;
using Bogus;
using Domain.Common;
using Domain.Entities;
using Domain.Entities.Physical;
using FluentAssertions;
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
    protected readonly Faker<CreateDepartmentCommand> _departmentGenerator = new Faker<CreateDepartmentCommand>()
        .RuleFor(x => x.Name, faker => faker.Commerce.Department());
    protected static IServiceScopeFactory _scopeFactory = null!;

    protected BaseClassFixture(CustomApiFactory apiFactory)
    {
        _scopeFactory = apiFactory.Services.GetRequiredService<IServiceScopeFactory>();
    }
    
    protected static async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        using var scope = _scopeFactory.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        return await mediator.Send(request);
    }

    protected void Remove<TEntity>(TEntity entity) where TEntity : BaseEntity
    {
        using var scope = _scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbContext.Set<TEntity>().Remove(entity);
        dbContext.SaveChanges();
    }

    protected static async Task<TEntity?> FindAsync<TEntity>(params object[] keyValues)
        where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.FindAsync<TEntity>(keyValues);
    }

    protected static async Task AddAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.AddAsync(entity);

        await context.SaveChangesAsync();
    }
    
    protected static async Task Add<TEntity>(TEntity entity)
        where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Add(entity);

        await context.SaveChangesAsync();
    }

    protected static async Task<int> CountAsync<TEntity>() where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.Set<TEntity>().CountAsync();
    }

    protected Department CreateDepartment()
    {
        
        var department = new Department()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.Department()
        };

        return department;
    }

    protected User CreateUser(Department department)
    {
        var user = new User()
        {
            Id = Guid.NewGuid(),
            Department = department,
            Role = new Faker().Name.JobType(),
            Username = new Faker().Internet.UserName(),
            Created = LocalDateTime.FromDateTime(DateTime.Now),
            IsActivated = true,
            IsActive = true,
            PasswordHash = SecurityUtil.Hash(new Faker().Internet.Password())
        };

        return user;
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
                DocumentType = "Something Something Type",
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

    protected Room CreateRoom(params Locker[] lockers)
    {
        var room = new Room()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfLockers = lockers.Length,
            IsAvailable = true
        };

        foreach (var locker in lockers)
        {
            room.Lockers.Add(locker);
        }

        return room;
    }
}