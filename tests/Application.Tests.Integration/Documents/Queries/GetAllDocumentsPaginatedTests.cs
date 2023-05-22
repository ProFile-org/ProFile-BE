using Application.Documents.Queries.GetAllDocumentsPaginated;
using Bogus;
using Domain.Entities.Physical;
using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Application.Tests.Integration.Documents.Queries;

public class GetAllDocumentsPaginatedTests : BaseClassFixture
{
    
    public GetAllDocumentsPaginatedTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Fact]
    public async Task ShouldReturnAllDocuments_WhenNoContainersAreDefined()
    {
        // Arrange
        var room = await SetupTestEntities();

        var query = new GetAllDocumentsPaginatedQuery()
        {
            Page = 1,
            Size = 5
        };

        // Act

        // Assert

    }
    
    private async Task<Room> SetupTestEntities()
    {
        var room1 = new Room()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Person.FirstName,
            Capacity = 3,
            IsAvailable = true,
            NumberOfLockers = 2,
        };
        
        var room2 = new Room()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Person.FirstName,
            Capacity = 3,
            IsAvailable = true,
            NumberOfLockers = 2,
        };

        var locker1 = new Locker()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Person.LastName,
            Room = room1,
            Capacity = 4,
            IsAvailable = true,
            NumberOfFolders = 2
        };

        var locker2 = new Locker()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Person.UserName,
            Room = room1,
            Capacity = 4,
            NumberOfFolders = 1
        };
        
        var locker3 = new Locker()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Person.LastName,
            Room = room2,
            Capacity = 4,
            IsAvailable = true,
            NumberOfFolders = 2
        };

        var locker4 = new Locker()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Person.UserName,
            Room = room2,
            Capacity = 4,
            NumberOfFolders = 1
        };

        var folder1 = new Folder()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Person.FirstName,
            Locker = locker1,
            IsAvailable = true,
            Capacity = 3,
            NumberOfDocuments = 0,
        };

        var folder2 = new Folder()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Person.LastName,
            Locker = locker1,
            IsAvailable = true,
            Capacity = 2,
            NumberOfDocuments = 3
        };
        
        var folder3 = new Folder()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Person.FirstName,
            Locker = locker1,
            IsAvailable = true,
            Capacity = 3,
            NumberOfDocuments = 0,
        };

        var folder4 = new Folder()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Person.LastName,
            Locker = locker2,
            IsAvailable = true,
            Capacity = 2,
            NumberOfDocuments = 3
        };

        var folder5 = new Folder()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Person.UserName,
            Locker = locker4,
            IsAvailable = false,
            Capacity = 3,
            NumberOfDocuments = 1
        };

        var document1 = new Document()
        {

        };

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Rooms.AddAsync(room1);
        await context.Lockers.AddAsync(locker1);
        await context.Lockers.AddAsync(locker2);
        await context.Folders.AddAsync(folder1);
        await context.Folders.AddAsync(folder2);
        await context.Folders.AddAsync(folder3);

        await context.SaveChangesAsync();

        return room1;
    }

    private async Task CleanupTestEntities(Room room)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.RemoveRange(room.Lockers.SelectMany(l => l.Folders));
        context.RemoveRange(room.Lockers);
        context.Remove(room);

        await context.SaveChangesAsync();
    }
}