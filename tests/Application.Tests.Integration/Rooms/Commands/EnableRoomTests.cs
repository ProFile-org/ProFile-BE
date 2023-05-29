using Application.Common.Exceptions;
using Application.Rooms.Commands;
using Domain.Entities;
using Domain.Entities.Physical;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Rooms.Commands;

public class EnableRoomTests : BaseClassFixture
{
    public EnableRoomTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }
    
    [Fact]
    public async Task ShouldEnableRoom_WhenRoomExistsAndIsDisabled()
    {
        // Arrange 
        var department = CreateDepartment();
        var folder = CreateFolder();
        var locker = CreateLocker(folder);
        var room = CreateRoom(department, locker);
        folder.IsAvailable = false;
        locker.IsAvailable = false;
        room.IsAvailable = false;
        await AddAsync(room);
       
        var command = new EnableRoom.Command()
        {
            RoomId = room.Id
        };
        
        // Act
        var result = await SendAsync(command);
        
        // Assert
        var folderResult = await FindAsync<Folder>(folder.Id);
        var lockerResult = await FindAsync<Locker>(locker.Id);
        
        result.IsAvailable.Should().BeTrue();
        folderResult!.IsAvailable.Should().BeFalse();
        lockerResult!.IsAvailable.Should().BeFalse();
        
        // Cleanup
        Remove(folder);
        Remove(locker);
        Remove(room);
        Remove(await FindAsync<Department>(department.Id));
    }

    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenRoomDoesNotExist()
    {
        // Arrange
        var command = new EnableRoom.Command()
        {
             RoomId = Guid.NewGuid()
        };
        
        // Act
        var action = async () => await SendAsync(command);
        
        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Room does not exist.");
    }

    [Fact]
    public async Task ShouldThrowConflictException_WhenRoomIsAlreadyAvailable()
    {
        // Arrange
        var department = CreateDepartment();
        var room = CreateRoom(department);
        room.IsAvailable = true;
        await AddAsync(room);

        var command = new EnableRoom.Command()
        {
            RoomId = room.Id
        };

        // Act
        var action = async () => await SendAsync(command);

        // Assert
        await action.Should().ThrowAsync<ConflictException>()
            .WithMessage("Room have already been enabled.");

        // Cleanup
        Remove(room);
        Remove(await FindAsync<Department>(department.Id));
    }
}