using Application.Common.Exceptions;
using Application.Rooms.Commands;
using Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Rooms.Commands;

public class UpdateRoomTests : BaseClassFixture
{
    public UpdateRoomTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Fact]
    public async Task ShouldUpdateRoom_WhenUpdateDetailsAreValid()
    {
        // Act
        var department = CreateDepartment();
        var room = CreateRoom(department);
        await AddAsync(room);

        var command = new UpdateRoom.Command()
        {
            RoomId = room.Id,
            Name = "Something else",
            Description = "Description else",
            Capacity = 6,
        };
        
        // Act
        var result = await SendAsync(command);
        
        // Assert
        result.Name.Should().Be(command.Name);
        result.Description.Should().Be(command.Description);
        result.Capacity.Should().Be(command.Capacity);
        
        // Cleanup
        Remove(room);
        Remove(await FindAsync<Department>(department.Id));
    }
    
    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenRoomDoesNotExist()
    {
        // Act
        var command = new UpdateRoom.Command()
        {
            RoomId = Guid.NewGuid(),
            Name = "Something else",
            Description = "Description else",
            Capacity = 6,
        };
        
        // Act
        var action = async () => await SendAsync(command);
        
        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Room does not exist.");
    }
    
    [Fact]
    public async Task ShouldThrowConflictException_WhenNewCapacityIsLowerThanNumberOfCurrentLockers()
    {
        // Act
        var department = CreateDepartment();
        var locker1 = CreateLocker();
        var locker2 = CreateLocker();
        var room = CreateRoom(department, locker1, locker2);
        await AddAsync(room);

        var command = new UpdateRoom.Command()
        {
            RoomId = room.Id,
            Name = "Something else",
            Description = "Description else",
            Capacity = 1,
        };
        
        // Act
        var action = async () => await SendAsync(command);
        
        // Assert
        await action.Should().ThrowAsync<ConflictException>()
            .WithMessage("New capacity cannot be less than current number of lockers");
        
        // Cleanup
        Remove(locker1);
        Remove(locker2);
        Remove(room);
        Remove(await FindAsync<Department>(department.Id));
    }
    
    [Fact]
    public async Task ShouldThrowConflictException_WhenNewNameHasAlreadyExisted()
    {
        // Act
        var department1 = CreateDepartment();
        var department2 = CreateDepartment();
        var existedNameRoom = CreateRoom(department1);
        var room = CreateRoom(department2);
        await AddAsync(room);

        var command = new UpdateRoom.Command()
        {
            RoomId = room.Id,
            Name = existedNameRoom.Name,
            Description = "Description else",
            Capacity = 1,
        };
        
        // Act
        var action = async () => await SendAsync(command);
        
        // Assert
        await action.Should().ThrowAsync<ConflictException>()
            .WithMessage("New name has already exists.");
        
        // Cleanup
        Remove(existedNameRoom);
        Remove(room);
        Remove(await FindAsync<Department>(department1.Id));
        Remove(await FindAsync<Department>(department2.Id));
    }
}