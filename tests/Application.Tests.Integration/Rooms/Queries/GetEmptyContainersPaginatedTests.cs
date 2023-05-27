using Application.Rooms.Queries.GetEmptyContainersPaginated;
using Bogus;
using Domain.Entities.Physical;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Application.Tests.Integration.Rooms.Queries;

public class GetEmptyContainersPaginatedTests : BaseClassFixture
{
    public GetEmptyContainersPaginatedTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Fact]
    public async Task ShouldReturnLockersWithEmptyFolders()
    {
        // Arrange
        var room = await SetupTestEntities();
        var query = new Query()
        {
            Page = 1,
            Size = 2,
            RoomId = room.Id
        };

        // Act
        var result = await SendAsync(query);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items.First().Id.Should().Be(room.Lockers.ElementAt(0).Id);
        result.Items.First().Name.Should().Be(room.Lockers.ElementAt(0).Name);
        result.Items.First().Description.Should().Be(room.Lockers.ElementAt(0).Description);
        result.Items.First().NumberOfFreeFolders.Should().Be(1);
        result.Items.First().Capacity.Should().Be(4);
        result.Items.First().NumberOfFolders.Should().Be(2);
        result.Items.First().Folders.First().Id.Should().Be(room.Lockers.ElementAt(0).Folders.First().Id);
        result.Items.First().Folders.First().Name.Should().Be(room.Lockers.ElementAt(0).Folders.First().Name);
        result.Items.First().Folders.First().Description.Should().Be(room.Lockers.ElementAt(0).Folders.First().Description);
        result.Items.First().Folders.First().Slot.Should().Be(3);

        // Cleanup
        await CleanupTestEntities(room);
    }

    [Fact]
    public async Task ShouldThrowNotFound_WhenRoomDoesNotExist()
    {
        // Arrange
        var query = new Query()
        {
            Page = 1,
            Size = 2,
            RoomId = Guid.NewGuid()
        };

        // Act
        var action = async () => await SendAsync(query);

        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>("Room does not exist");
    }

    private async Task<Room> SetupTestEntities()
    {
        var room = new Room()
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
            Room = room,
            Capacity = 4,
            IsAvailable = true,
            NumberOfFolders = 2
        };

        var locker2 = new Locker()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Person.UserName,
            Room = room,
            Capacity = 4,
            IsAvailable = false,
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
            Name = new Faker().Person.UserName,
            Locker = locker2,
            IsAvailable = false,
            Capacity = 3,
            NumberOfDocuments = 1
        };

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Rooms.AddAsync(room);
        await context.Lockers.AddAsync(locker1);
        await context.Lockers.AddAsync(locker2);
        await context.Folders.AddAsync(folder1);
        await context.Folders.AddAsync(folder2);
        await context.Folders.AddAsync(folder3);

        await context.SaveChangesAsync();

        return room;
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