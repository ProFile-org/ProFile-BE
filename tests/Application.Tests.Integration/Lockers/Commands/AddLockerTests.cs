using Application.Common.Exceptions;
using Application.Lockers.Commands.AddLocker;
using Bogus;
using Domain.Entities.Physical;
using Domain.Exceptions;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Application.Tests.Integration.Lockers.Commands;

public class AddLockerTests : BaseClassFixture
{
    public AddLockerTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    
    }

    [Fact]
    public async Task ShouldReturnLocker_WhenCreateDetailsAreValid()
    {
        // Arrange
        var room = new Room()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Name.JobArea(),
            Description = new Faker().Lorem.Sentence(),
            Capacity = 3,
            IsAvailable = true,
            NumberOfLockers = 0,
        };
        
        await AddAsync(room);
        
        var addLockerCommand = new AddLockerCommand()
        {
            Name = new Faker().Name.JobTitle(),
            Description = new Faker().Lorem.Sentence(),
            Capacity = 1,
            RoomId = room.Id,
        };

        room.NumberOfLockers += 1;
        
        // Act
        
        var locker = await SendAsync(addLockerCommand);
        
        // Assert
        locker.Name.Should().Be(addLockerCommand.Name);
        locker.Description.Should().Be(addLockerCommand.Description);
        locker.Capacity.Should().Be(addLockerCommand.Capacity);
        locker.NumberOfFolders.Should().Be(0);
        locker.IsAvailable.Should().BeTrue();
        locker.Room.Id.Should().Be(room.Id);
        locker.Room.NumberOfLockers.Should().Be(room.NumberOfLockers);
        
        // Cleanup
        var roomEntity = await FindAsync<Room>(room.Id);
        Remove(roomEntity);
    }

    [Fact]
    public async Task ShouldThrowConflictException_WhenLockerAlreadyExistsInTheSameRoom()
    {
        // Arrange
        var room = new Room()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Name.JobArea(),
            Description = new Faker().Lorem.Sentence(),
            Capacity = 3,
            IsAvailable = true,
            NumberOfLockers = 0,
        };
        
        await AddAsync(room);

        var addLockerCommand = new AddLockerCommand()
        {
            Name = new Faker().Name.JobTitle(),
            Description = new Faker().Lorem.Sentence(),
            Capacity = 1,
            RoomId = room.Id,
        };
        
        await SendAsync(addLockerCommand);
        
        // Act
        var action = async () => await SendAsync(addLockerCommand);
        
        // Assert
        await action.Should().ThrowAsync<ConflictException>().WithMessage("Locker name already exists.");
        
        // Cleanup
        var roomEntity = await FindAsync<Room>(room.Id);
        Remove(roomEntity);
    }

    [Fact]
    public async Task ShouldReturnLocker_WhenLockersHasSameNameButInDifferentRooms()
    {
        // Arrange
        var room1 = new Room()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Name.JobArea(),
            Description = new Faker().Lorem.Sentence(),
            Capacity = 3,
            IsAvailable = true,
            NumberOfLockers = 0,
        };
        
        await AddAsync(room1);
        
        var room2 = new Room()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Name.JobArea(),
            Description = new Faker().Lorem.Sentence(),
            Capacity = 3,
            IsAvailable = true,
            NumberOfLockers = 0,
        };
        
        await AddAsync(room2);

        var addLockerCommand = new AddLockerCommand()
        {
            Name = new Faker().Name.JobTitle(),
            Description = new Faker().Lorem.Sentence(),
            Capacity = 1,
            RoomId = room1.Id,
        };
        
        var addLockerCommand2 = new AddLockerCommand()
        {
            Name = addLockerCommand.Name,
            Description = new Faker().Lorem.Sentence(),
            Capacity = 1,
            RoomId = room2.Id,
        };

        room2.NumberOfLockers += 1;
        
        var locker1 = await SendAsync(addLockerCommand);
        
        // Act
        var locker2 = await SendAsync(addLockerCommand2);
        
        // Assert
        locker2.Name.Should().Be(addLockerCommand2.Name);
        locker2.Description.Should().Be(addLockerCommand2.Description);
        locker2.Capacity.Should().Be(addLockerCommand2.Capacity);
        locker2.NumberOfFolders.Should().Be(0);
        locker2.IsAvailable.Should().BeTrue();
        locker2.Room.Id.Should().Be(room2.Id);
        locker2.Room.NumberOfLockers.Should().Be(room2.NumberOfLockers);
        
        // Cleanup
        var room1Entity = await FindAsync<Room>(room1.Id);
        var room2Entity = await FindAsync<Room>(room2.Id);
        Remove(room1Entity);
        Remove(room2Entity);
    }

    [Fact]
    public async Task ShouldThrowLimitExceededException_WhenGoingOverCapacity()
    {
        // Arrange
        var room = new Room()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Name.JobArea(),
            Description = new Faker().Lorem.Sentence(),
            Capacity = 1,
            IsAvailable = true,
            NumberOfLockers = 0,
        };
        
        await AddAsync(room);

        var addLockerCommand = new AddLockerCommand()
        {
            Name = new Faker().Name.JobTitle(),
            Description = new Faker().Lorem.Sentence(),
            Capacity = 1,
            RoomId = room.Id,
        };
        
        var addLockerCommand2 = new AddLockerCommand()
        {
            Name = new Faker().Name.JobTitle(),
            Description = new Faker().Lorem.Sentence(),
            Capacity = 1,
            RoomId = room.Id,
        };
        
        var locker = await SendAsync(addLockerCommand);
        
        // Act
        var action = async () => await SendAsync(addLockerCommand2);
        
        // Assert
        await action.Should().ThrowAsync<LimitExceededException>().WithMessage("This room cannot accept more lockers.");
        
        // Cleanup
        var roomEntity = await FindAsync<Room>(room.Id);
        Remove(roomEntity);
    }
}