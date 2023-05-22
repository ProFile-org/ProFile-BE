using Application.Common.Exceptions;
using Application.Lockers.Commands.AddLocker;
using Bogus;
using Domain.Entities.Physical;
using Domain.Exceptions;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Application.Tests.Integration.Lockers.Commands;

public class CreateLockerTests : BaseClassFixture
{
    private static IServiceScopeFactory _scopeFactory = null!;
    public CreateLockerTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
        _scopeFactory = apiFactory.Services.GetRequiredService<IServiceScopeFactory>();
    }

    [Fact]
    public async Task ShouldReturnLocker_WhenLockerDetailsAreValid()
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

        var createLockerCommand = new CreateLockerCommand()
        {
            Name = new Faker().Name.JobTitle(),
            Description = new Faker().Lorem.Sentence(),
            Capacity = 1,
            RoomId = room.Id,
        };

        room.NumberOfLockers += 1;
        
        // Act
        
        var locker = await SendAsync(createLockerCommand);
        
        // Assert
        locker.Name.Should().Be(createLockerCommand.Name);
        locker.Description.Should().Be(createLockerCommand.Description);
        locker.Capacity.Should().Be(createLockerCommand.Capacity);
        locker.Room.Id.Should().Be(room.Id);
        
        //Cleanup
        var lockerEntity = await FindAsync<Locker>(locker.Id);
        var roomEntity = await FindAsync<Room>(room.Id);
        Remove(lockerEntity);
        Remove(roomEntity);
    }

    [Fact]
    public async Task ShouldThrowConflictException_WhenLockerAlreadyExistsInTheSameRoom()
    {
        //Arrange
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

        var createLockerCommand = new CreateLockerCommand()
        {
            Name = new Faker().Name.JobTitle(),
            Description = new Faker().Lorem.Sentence(),
            Capacity = 1,
            RoomId = room.Id,
        };
        
        room.NumberOfLockers += 1;
        
        var createLockerCommand2 = new CreateLockerCommand()
        {
            Name = createLockerCommand.Name,
            Description = new Faker().Lorem.Sentence(),
            Capacity = 1,
            RoomId = room.Id,
        };

        room.NumberOfLockers += 1;
        
        // Act
        
        var locker = await SendAsync(createLockerCommand);
        var action = async () => await SendAsync(createLockerCommand2);
        
        //Assert
        await action.Should().ThrowAsync<ConflictException>().WithMessage("Locker's name already exists.");
        
        //Cleanup
        var lockerEntity = await FindAsync<Locker>(locker.Id);
        var roomEntity = await FindAsync<Room>(room.Id);
        Remove(lockerEntity);
        Remove(roomEntity);
    }

    [Fact]
    public async Task ShouldReturnLocker_WhenLockersHasSameNameButInDifferentRooms()
    {
        //Arrange
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

        var createLockerCommand = new CreateLockerCommand()
        {
            Name = new Faker().Name.JobTitle(),
            Description = new Faker().Lorem.Sentence(),
            Capacity = 1,
            RoomId = room1.Id,
        };
        
        room1.NumberOfLockers += 1;
        
        var createLockerCommand2 = new CreateLockerCommand()
        {
            Name = createLockerCommand.Name,
            Description = new Faker().Lorem.Sentence(),
            Capacity = 1,
            RoomId = room2.Id,
        };

        room2.NumberOfLockers += 1;
        
        // Act
        
        var locker1 = await SendAsync(createLockerCommand);
        var locker2 = await SendAsync(createLockerCommand2);
        
        //Assert
        locker2.Name.Should().Be(createLockerCommand2.Name);
        locker2.Description.Should().Be(createLockerCommand2.Description);
        locker2.Capacity.Should().Be(createLockerCommand2.Capacity);
        locker2.Room.Id.Should().Be(room2.Id);
        
        //Cleanup
        var locker1Entity = await FindAsync<Locker>(locker1.Id);
        var locker2Entity = await FindAsync<Locker>(locker2.Id);
        var room1Entity = await FindAsync<Room>(room1.Id);
        var room2Entity = await FindAsync<Room>(room2.Id);
        Remove(locker1Entity);
        Remove(locker2Entity);
        Remove(room1Entity);
        Remove(room2Entity);
    }

    [Fact]
    public async Task ShouldThrowLimitExceededException_WhenGoingOverCapacity()
    {
        //Arrange
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

        var createLockerCommand = new CreateLockerCommand()
        {
            Name = new Faker().Name.JobTitle(),
            Description = new Faker().Lorem.Sentence(),
            Capacity = 1,
            RoomId = room.Id,
        };
        
        room.NumberOfLockers += 1;
        
        var createLockerCommand2 = new CreateLockerCommand()
        {
            Name = createLockerCommand.Name,
            Description = new Faker().Lorem.Sentence(),
            Capacity = 1,
            RoomId = room.Id,
        };

        room.NumberOfLockers += 1;
        
        // Act
        var locker = await SendAsync(createLockerCommand);
        var action = async () => await SendAsync(createLockerCommand2);
        
        //Assert
        await action.Should().ThrowAsync<LimitExceededException>().WithMessage("This room cannot accept more lockers.");
        
        //Cleanup
        var lockerEntity = await FindAsync<Locker>(locker.Id);
        var roomEntity = await FindAsync<Room>(room.Id);
        Remove(lockerEntity);
        Remove(roomEntity);
    }
}