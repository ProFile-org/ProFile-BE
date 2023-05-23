using Application.Common.Exceptions;
using Application.Lockers.Commands.AddLocker;
using Application.Lockers.Commands.DisableLocker;
using Application.Lockers.Commands.EnableLocker;
using Bogus;
using Domain.Entities.Physical;
using Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Lockers.Commands;

public class EnableLockerTests : BaseClassFixture
{
    public EnableLockerTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
        
    }

    [Fact]
    public async Task ShouldEnableLocker_WhenLockerExistsAndIsNotAvailable()
    {
        //Arrange
        var room = new Room()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Description = new Faker().Lorem.Sentence(),
            Capacity = 1,
            IsAvailable = true,
            NumberOfLockers = 0,
        };

        await AddAsync(room);

        var createLockerCommand = new AddLockerCommand()
        {
            Name = new Faker().Commerce.ProductName(),
            Description = new Faker().Lorem.Sentence(),
            Capacity = 2,
            RoomId = room.Id,
        };

        var locker = await SendAsync(createLockerCommand);
        room.NumberOfLockers += 1;

        var disableLockerCommand = new DisableLockerCommand()
        {
            LockerId = locker.Id,
        };
        
        await SendAsync(disableLockerCommand);
        room.NumberOfLockers -= 1;
        
        //Act
        
        var enableLockerCommand = new EnableLockerCommand()
        {
            LockerId = locker.Id,
        };
        
        var result = await SendAsync(enableLockerCommand);
        room.NumberOfLockers += 1;

        //Assert
        result.Name.Should().Be(locker.Name);
        result.Description.Should().Be(locker.Description);
        result.Capacity.Should().Be(locker.Capacity);
        result.IsAvailable.Should().BeTrue();
        result.NumberOfFolders.Should().Be(locker.NumberOfFolders);
        result.Room.Id.Should().Be(room.Id);
        result.Room.NumberOfLockers.Should().Be(room.NumberOfLockers);
        
        //Cleanup
        var roomEntity = await FindAsync<Room>(room.Id);
        Remove(roomEntity);
    }
    
    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenLockerDoesNotExist()
    {
        //Arrange
        var enableLockerCommand = new EnableLockerCommand()
        {
            LockerId = Guid.NewGuid(),
        };
        
        //Act
        var action = async () => await SendAsync(enableLockerCommand);
        
        //Assert
        await action.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Locker does not exist.");
    }

    [Fact]
    public async Task ShouldThrowAvailableEntityException_WhenLockerIsAlreadyEnabled()
    {
        //Arrange 
        var room = new Room()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Description = new Faker().Lorem.Sentence(),
            Capacity = 1,
            IsAvailable = true,
            NumberOfLockers = 0,
        };
        
        await AddAsync(room);

        var createLockerCommand = new AddLockerCommand()
        {
            Name = new Faker().Commerce.ProductName(),
            Description = new Faker().Lorem.Sentence(),
            Capacity = 2,
            RoomId = room.Id,
        };
        
        var locker = await SendAsync(createLockerCommand);
        var enableLockerCommand = new EnableLockerCommand()
        {
            LockerId = locker.Id,
        };
        
        //Act 
        var action = async () => await SendAsync(enableLockerCommand);
        
        //Assert
        await action.Should()
            .ThrowAsync<AvailableEntityException>()
            .WithMessage("Locker has already been enabled.");
        
        //Cleanup
        var roomEntity = await FindAsync<Room>(room.Id);
        Remove(roomEntity);
    }

    [Fact]
    public async Task ShouldThrowLimitExceededException_WhenEnableLockerButRoomIsFull()
    {
        //Arrange 
        var room = new Room()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Description = new Faker().Lorem.Sentence(),
            Capacity = 1,
            IsAvailable = true,
            NumberOfLockers = 0,
        };
        
        await AddAsync(room);

        var createLocker1Command = new AddLockerCommand()
        {
            Name = new Faker().Commerce.ProductName(),
            Description = new Faker().Lorem.Sentence(),
            Capacity = 2,
            RoomId = room.Id,
        };
        
        var locker1 = await SendAsync(createLocker1Command);
        var disableLocker1Command = new DisableLockerCommand()
        {
            LockerId = locker1.Id,
        };

        await SendAsync(disableLocker1Command);

        var createLocker2Command = new AddLockerCommand()
        {
            Name = new Faker().Commerce.ProductName(),
            Description = new Faker().Lorem.Sentence(),
            Capacity = 2,
            RoomId = room.Id,
        };

        await SendAsync(createLocker2Command);

        var enableLocker1Command = new EnableLockerCommand()
        {
            LockerId = locker1.Id,
        };
        
        //Act 
        var action = async () => await SendAsync(enableLocker1Command);
        
        //Assert
        await action.Should()
            .ThrowAsync<LimitExceededException>()
            .WithMessage("This room cannot accept more lockers.");
        
        //Cleanup
        var roomEntity = await FindAsync<Room>(room.Id);
        Remove(roomEntity);
    }
}