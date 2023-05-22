using Application.Common.Exceptions;
using Application.Common.Models.Dtos.Physical;
using Application.Folders.Commands.AddFolder;
using Application.Lockers.Commands.AddLocker;
using Application.Rooms.Commands.CreateRoom;
using Bogus;
using Domain.Entities.Physical;
using Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Folders.Commands;

public class AddFolderTests : BaseClassFixture
{
    private readonly Faker<AddFolderCommand> _folderGenerator = new Faker<AddFolderCommand>()
            .RuleFor(f => f.Name, faker => faker.Commerce.ProductName())
            .RuleFor(f => f.Description, faker => faker.Commerce.ProductDescription())
            .RuleFor(f => f.Capacity, faker => faker.Random.Int(1,9999));
    
    private readonly Faker<CreateRoomCommand> _roomGenerator = new Faker<CreateRoomCommand>()
        .RuleFor(r => r.Name, faker => faker.Commerce.ProductName())
        .RuleFor(r => r.Description, faker => faker.Commerce.ProductDescription())
        .RuleFor(r => r.Capacity, faker => faker.Random.Int(1,9999));

    private readonly Faker<AddLockerCommand> _lockerGenerator = new Faker<AddLockerCommand>()
        .RuleFor(l => l.Name, faker => faker.Commerce.ProductName())
        .RuleFor(l => l.Description, faker => faker.Commerce.ProductDescription())
        .RuleFor(l => l.Capacity, faker => faker.Random.Int(1,9999));
    public AddFolderTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Fact]
    public async Task ShouldAddFolder_WhenAddDetailsAreValid()
    {
        // Arrange
        var addRoomCommand = _roomGenerator.Generate();
        var room = await SendAsync(addRoomCommand);

        var addLockerCommand = _lockerGenerator.Generate();
        addLockerCommand = addLockerCommand with
        {
            RoomId = room.Id,
            Capacity = 1
        };
        var locker = await SendAsync(addLockerCommand);
        
        var addFolderCommand = _folderGenerator.Generate();
        addFolderCommand =  addFolderCommand with 
        {
            LockerId = locker.Id,
            Capacity = 1
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

    [Fact]
    public async Task ShouldAddFolder_WhenFoldersHasSameNameButInDifferentLockers()
    {
        // Arrange
        var sameFolderName = new Faker().Commerce.ProductName();
        var addRoomCommand = _roomGenerator.Generate();
        addRoomCommand = addRoomCommand with
        {
            Capacity = 2
        };
        
        var room = await SendAsync(addRoomCommand);
        
        var addLockerCommand = _lockerGenerator.Generate();
        
        var addLockerACommand = addLockerCommand with
        {
            RoomId = room.Id,
            Capacity = 1
        };
        var addLockerBCommand = addLockerCommand with
        {
            RoomId = room.Id,
            Name = new Faker().Commerce.ProductName(),
            Capacity = 1
        };
        
        var lockerA = await SendAsync(addLockerACommand);
        var lockerB = await SendAsync(addLockerBCommand);
        
        var addFolderCommand = _folderGenerator.Generate();
        var addFolderCommandForLockerA =  addFolderCommand with 
        {
            Name = sameFolderName,
            LockerId = lockerA.Id
        };
        var addFolderCommandForLockerB =  addFolderCommand with 
        {
            Name = sameFolderName,
            LockerId = lockerB.Id
        };
        var folderA = await SendAsync(addFolderCommandForLockerA);
        
        // Act
        var folderB = await SendAsync(addFolderCommandForLockerB);
        
        // Assert
        folderA.Locker.Id.Should().NotBe(folderB.Locker.Id);
        folderA.Name.Should().Be(folderB.Name);
        
        // Clean up
        var folderAEntity = await FindAsync<Folder>(folderA.Id);
        var folderBEntity = await FindAsync<Folder>(folderB.Id);
        var lockerAEntity = await FindAsync<Locker>(lockerA.Id);
        var lockerBEntity = await FindAsync<Locker>(lockerB.Id);
        var roomEntity = await FindAsync<Room>(room.Id);
        Remove(folderAEntity);
        Remove(folderBEntity);
        Remove(lockerAEntity);
        Remove(lockerBEntity);
        Remove(roomEntity);
    }

    [Fact]
    public async Task ShouldThrowConflictException_WhenFolderAlreadyExistsInTheSameLocker()
    {
        // Arrange
        var sameFolderName = new Faker().Commerce.ProductName();
        var addRoomCommand = _roomGenerator.Generate();
        var room = await SendAsync(addRoomCommand);
        
        var addLockerCommand = _lockerGenerator.Generate();
        addLockerCommand = addLockerCommand with
        {
            RoomId = room.Id,
            Capacity = 2
        };
        var locker = await SendAsync(addLockerCommand);
        
        var addFolderCommand = _folderGenerator.Generate();
        var addFolderCommandForLockerA =  addFolderCommand with 
        {
            Name = sameFolderName,
            LockerId = locker.Id
        };
        var addFolderCommandForLockerB =  addFolderCommand with 
        {
            Name = sameFolderName,
            LockerId = locker.Id
        };
        var folder = await SendAsync(addFolderCommandForLockerA);
        
        // Act
        var action = async () => await SendAsync(addFolderCommandForLockerB);
        // Assert
        await action.Should().ThrowAsync<ConflictException>().WithMessage("Folder's name already exists.");
        
        // Clean up
        var folderEntity = await FindAsync<Folder>(folder.Id);
        var lockerEntity = await FindAsync<Locker>(locker.Id);
        var roomEntity = await FindAsync<Room>(room.Id);
        Remove(folderEntity);
        Remove(lockerEntity);
        Remove(roomEntity);
    }

    [Fact]
    public async Task ShouldThrowLimitExceededException_WhenGoingOverCapacity()
    {
        // Arrange
        var addRoomCommand = _roomGenerator.Generate();
        var room = await SendAsync(addRoomCommand);
        
        var addLockerCommand = _lockerGenerator.Generate();
        addLockerCommand = addLockerCommand with
        {
            RoomId = room.Id,
            Capacity = new Faker().Random.Int(1,10)
        };
        var locker = await SendAsync(addLockerCommand);
        

        var list = new List<FolderDto>();
        // Act
        
        var action = async () =>
        {
            
            for (var i = 0; i < room.Capacity; i++)
            {
                var addFolderCommand = _folderGenerator.Generate();
                addFolderCommand = addFolderCommand with
                {
                    LockerId = locker.Id
                };
                list.Add(await SendAsync(addFolderCommand));    
            }
        };
        // Assert
        await action.Should().ThrowAsync<LimitExceededException>()
            .WithMessage("This locker cannot accept more folders.");
        
        // Clean up
        foreach (var f in list)
        {
            var folderEntity = await FindAsync<Folder>(f.Id);
            Remove(folderEntity);
        }
        var lockerEntity = await FindAsync<Locker>(locker.Id);
        var roomEntity = await FindAsync<Room>(room.Id);
        Remove(lockerEntity);
        Remove(roomEntity);
    }

    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenLockerIdNotExists()
    {
        // Arrange
        var addFolderCommand = _folderGenerator.Generate();
        addFolderCommand =  addFolderCommand with 
        {
            LockerId = Guid.NewGuid()
        };
        
        // Act
        var folder = async () => await SendAsync(addFolderCommand);
        // Assert
        await folder.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Locker does not exist.");
    }
}