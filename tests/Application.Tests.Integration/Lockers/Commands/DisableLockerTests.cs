using Application.Common.Exceptions;
using Application.Lockers.Commands.AddLocker;
using Application.Lockers.Commands.DisableLocker;
using Bogus;
using Domain.Entities.Physical;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Lockers.Commands;

public class DisableLockerTests : BaseClassFixture
{
    public DisableLockerTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
        
    }

    [Fact]
    public async Task ShouldDisableLocker_WhenLockerExistsAndIsAvailable()
    {
        // Arrange
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

        var removeLockerCommand = new DisableLockerCommand()
        {
            LockerId = locker.Id,
        };
        
        // Act
        var result = await SendAsync(removeLockerCommand);
        
        // Assert
        result.IsAvailable.Should().BeFalse();
        
        //Cleanup
        var roomEntity = await FindAsync<Room>(room.Id);
        var lockerEntity = await FindAsync<Locker>(locker.Id);
        Remove(lockerEntity);
        Remove(roomEntity);
    }
    
    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenLockerDoesNotExist()
    {
        // Arrange
        var removeLockerCommand = new DisableLockerCommand()
        {
            LockerId = Guid.NewGuid(),
        };
        
        // Act
        var action = async () => await SendAsync(removeLockerCommand);
        
        // Assert
        await action.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Locker does not exist.");
    }

    [Fact]
    public async Task ShouldThrowConflictException_WhenLockerIsAlreadyDisabled()
    {
        // Arrange 
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
        var removeLockerCommand = new DisableLockerCommand()
        {
            LockerId = locker.Id,
        };
        
        await SendAsync(removeLockerCommand);
        
        // Act 
        var action = async () => await SendAsync(removeLockerCommand);
        
        // Assert
        await action.Should().ThrowAsync<ConflictException>().WithMessage("Locker has already been disabled.");
        
        // Cleanup
        var roomEntity = await FindAsync<Room>(room.Id);
        var lockerEntity = await FindAsync<Locker>(locker.Id);
        Remove(lockerEntity);
        Remove(roomEntity);
    }
}