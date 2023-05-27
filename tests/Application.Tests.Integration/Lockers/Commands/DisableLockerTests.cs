using Application.Common.Exceptions;
using Bogus;
using Domain.Entities.Physical;
using FluentAssertions;
using Xunit;
using Command = Application.Lockers.Commands.Disable.Command;

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

        var createLockerCommand = new Application.Lockers.Commands.Add.Command()
        {
            Name = new Faker().Commerce.ProductName(),
            Description = new Faker().Lorem.Sentence(),
            Capacity = 2,
            RoomId = room.Id,
        };

        var locker = await SendAsync(createLockerCommand);
        room.NumberOfLockers += 1;

        var disableLockerCommand = new Command()
        {
            LockerId = locker.Id,
        };
        
        // Act
        var result = await SendAsync(disableLockerCommand);
        
        // Assert
        result.Name.Should().Be(locker.Name);
        result.Description.Should().Be(locker.Description);
        result.Capacity.Should().Be(locker.Capacity);
        result.IsAvailable.Should().BeFalse();
        result.NumberOfFolders.Should().Be(locker.NumberOfFolders);
        
        // Cleanup
        var roomEntity = await FindAsync<Room>(room.Id);
        Remove(roomEntity);
    }
    
    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenLockerDoesNotExist()
    {
        // Arrange
        var disableLockerCommand = new Command()
        {
            LockerId = Guid.NewGuid(),
        };
        
        // Act
        var action = async () => await SendAsync(disableLockerCommand);
        
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
        
        // Act 
        await SendAsync(disableLockerCommand);
        var action = async () => await SendAsync(disableLockerCommand);
        
        // Assert
        await action.Should().ThrowAsync<ConflictException>().WithMessage("Locker has already been disabled.");
        
        // Cleanup
        var roomEntity = await FindAsync<Room>(room.Id);
        Remove(roomEntity);
    }
}