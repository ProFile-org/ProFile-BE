using Application.Common.Exceptions;
using Application.Lockers.Commands.Add;
using Application.Lockers.Commands.Disable;
using Application.Lockers.Commands.Enable;
using Bogus;
using Domain.Entities.Physical;
using Domain.Exceptions;
using FluentAssertions;
using Xunit;
using Command = Application.Lockers.Commands.Disable.Command;

namespace Application.Tests.Integration.Lockers.Commands;

public class EnableLockerTests : BaseClassFixture
{
    public EnableLockerTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
        
    }

    [Fact]
    public async Task ShouldEnableLocker_WhenLockerExistsAndIsNotAvailable()
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

        var createLockerCommand = new Application.Lockers.Commands.Add.Command()
        {
            Name = new Faker().Commerce.ProductName(),
            Description = new Faker().Lorem.Sentence(),
            Capacity = 2,
            RoomId = room.Id,
        };

        var locker = await SendAsync(createLockerCommand);

        var disableLockerCommand = new Command()
        {
            LockerId = locker.Id,
        };
        
        await SendAsync(disableLockerCommand);
        
        // Act
        
        var enableLockerCommand = new Application.Lockers.Commands.Enable.Command()
        {
            LockerId = locker.Id,
        };
        
        var result = await SendAsync(enableLockerCommand);

        // Assert
        result.IsAvailable.Should().BeTrue();
        
        // Cleanup
        var roomEntity = await FindAsync<Room>(room.Id);
        Remove(roomEntity);
    }
    
    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenLockerDoesNotExist()
    {
        // Arrange
        var enableLockerCommand = new Application.Lockers.Commands.Enable.Command()
        {
            LockerId = Guid.NewGuid(),
        };
        
        // Act
        var action = async () => await SendAsync(enableLockerCommand);
        
        // Assert
        await action.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Locker does not exist.");
    }

    [Fact]
    public async Task ShouldThrowConflictException_WhenLockerIsAlreadyEnabled()
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

        var createLockerCommand = new Application.Lockers.Commands.Add.Command()
        {
            Name = new Faker().Commerce.ProductName(),
            Description = new Faker().Lorem.Sentence(),
            Capacity = 2,
            RoomId = room.Id,
        };
        
        var locker = await SendAsync(createLockerCommand);
        var enableLockerCommand = new Application.Lockers.Commands.Enable.Command()
        {
            LockerId = locker.Id,
        };
        
        // Act 
        var action = async () => await SendAsync(enableLockerCommand);
        
        // Assert
        await action.Should()
            .ThrowAsync<ConflictException>()
            .WithMessage("Locker has already been enabled.");
        
        // Cleanup
        var roomEntity = await FindAsync<Room>(room.Id);
        Remove(roomEntity);
    }
}
