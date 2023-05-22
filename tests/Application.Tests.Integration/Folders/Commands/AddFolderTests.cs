using Application.Folders.Commands.AddFolder;
using Application.Lockers.Commands.AddLocker;
using Application.Rooms.Commands.CreateRoom;
using Bogus;
using Domain.Entities.Physical;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Folders.Commands;

public class AddFolderTests : BaseClassFixture
{
    public AddFolderTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Fact]
    public async Task ShouldAddFolder_WhenAddDetailsAreValid()
    {
        // Arrange
        var folderGenerator = new Faker<AddFolderCommand>()
            .RuleFor(f => f.Name, faker => faker.Commerce.ProductName())
            .RuleFor(f => f.Description, faker => faker.Commerce.ProductDescription())
            .RuleFor(f => f.Capacity, faker => faker.Random.Int(1,9999));
        
        var roomGenerator = new Faker<CreateRoomCommand>()
            .RuleFor(r => r.Name, faker => faker.Commerce.ProductName())
            .RuleFor(r => r.Description, faker => faker.Commerce.ProductDescription())
            .RuleFor(r => r.Capacity, 1);

        var lockerGenerator = new Faker<AddLockerCommand>()
            .RuleFor(l => l.Name, faker => faker.Commerce.ProductName())
            .RuleFor(l => l.Description, faker => faker.Commerce.ProductDescription())
            .RuleFor(l => l.Capacity, 1);

        var addRoomCommand = roomGenerator.Generate();
        var room = await SendAsync(addRoomCommand);

        var addLockerCommand = lockerGenerator.Generate();
        addLockerCommand = addLockerCommand with
        {
            RoomId = room.Id
        };
        var locker = await SendAsync(addLockerCommand);
        
        var addFolderCommand = folderGenerator.Generate();
        addFolderCommand =  addFolderCommand with 
        {
            LockerId = locker.Id
        };

        // Act
        var folder = await SendAsync(addFolderCommand);
        // Assert
        folder.Name.Should().Be(addFolderCommand.Name);
        folder.Description.Should().Be(addFolderCommand.Description);
        folder.Capacity.Should().Be(addFolderCommand.Capacity);
        folder.Locker.Id.Should().Be(locker.Id);
        folder.Locker.NumberOfFolders.Should().Be(locker.NumberOfFolders + 1);
        folder.NumberOfDocuments.Should().Be(0);
        folder.IsAvailable.Should().BeTrue();
        
        // Clean up
        var folderEntity = await FindAsync<Folder>(folder.Id);
        var lockerEntity = await FindAsync<Locker>(locker.Id);
        var roomEntity = await FindAsync<Room>(room.Id);
        Remove(folderEntity);
        Remove(lockerEntity);
        Remove(roomEntity);
    }
}