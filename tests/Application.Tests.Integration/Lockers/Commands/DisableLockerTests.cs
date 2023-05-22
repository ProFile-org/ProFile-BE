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
    public async Task ShouldRemoveLocker_WhenLockerExists()
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

        var removeLockerCommand = new DisableLockerCommand()
        {
            LockerId = locker.Id,
        };
        
        //Act
        var result = await SendAsync(removeLockerCommand);
        room.NumberOfLockers -= 1;
        
        //Assert
        result.Name.Should().Be(locker.Name);
        result.Description.Should().Be(locker.Description);
        result.Capacity.Should().Be(locker.Capacity);
        result.IsAvailable.Should().BeFalse();
        result.NumberOfFolders.Should().Be(locker.NumberOfFolders);
        result.Room.Id.Should().Be(room.Id);
        result.Room.NumberOfLockers.Should().Be(room.NumberOfLockers);
        
        //Cleanup
        var roomEntity = await FindAsync<Room>(room.Id);
        var lockerEntity = await FindAsync<Locker>(locker.Id);
        Remove(lockerEntity);
        Remove(roomEntity);
    }
    
    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenLockerDoesNotExist()
    {
        //Arrange
        var removeLockerCommand = new DisableLockerCommand()
        {
            LockerId = Guid.NewGuid(),
        };
        
        //Act
        var action = async () => await SendAsync(removeLockerCommand);
        
        //Assert
        await action.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Locker does not exist.");
    }
}