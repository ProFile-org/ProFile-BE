using Application.Common.Exceptions;
using Application.Rooms.Commands;
using Domain.Entities;
using Domain.Entities.Physical;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Rooms.Commands;

public class DisableRoomTests : BaseClassFixture
{
    public DisableRoomTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }
    
    [Fact]
    public async Task ShouldDisableRoom_WhenRoomHaveNoDocument()
    {
        // Arrange 
        var department = CreateDepartment();
        var folder = CreateFolder();
        var locker = CreateLocker(folder);
        var room = CreateRoom(department, locker);
        await AddAsync(room);
       
        var disableRoomCommand = new DisableRoom.Command()
        {
            RoomId = room.Id
        };
        
        // Act
        var result = await SendAsync(disableRoomCommand);
        
        // Assert
        var folderResult = await FindAsync<Folder>(folder.Id);
        var lockerResult = await FindAsync<Locker>(locker.Id);
        
        result.IsAvailable.Should().BeFalse();
        folderResult.IsAvailable.Should().BeFalse();
        lockerResult.IsAvailable.Should().BeFalse();
        
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
        var disableRoomCommand = new DisableRoom.Command()
        {
             RoomId = Guid.NewGuid()
        };
        
        // Act
        var action = async () => await SendAsync(disableRoomCommand);
        
        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Room does not exist.");
    }

    [Fact]
    public async Task ShouldThrowInvalidOperationException_WhenRoomIsNotEmptyOfDocuments()
    {
        // Arrange
        var department = CreateDepartment();
        var documents = CreateNDocuments(1);
        var folder = CreateFolder(documents);
        var locker = CreateLocker(folder);
        var room = CreateRoom(department, locker);

        await AddAsync(room);
            
        var disableRoomCommand = new DisableRoom.Command()
        {
            RoomId = room.Id
        };
        
        // Act
        var action = async () => await SendAsync(disableRoomCommand);
        
        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Room cannot be disabled because it contains documents.");
        
        // Cleanup
        Remove(documents.First());
        Remove(folder);
        Remove(locker);
        Remove(room);
        Remove(await FindAsync<Department>(department.Id));
    }

    [Fact]
    public async Task ShouldThrowInvalidOperationException_WhenRoomIsNotAvailable()
    {
        // Arrange
        var department = CreateDepartment();
        var room = CreateRoom(department);
        room.IsAvailable = false;
        await AddAsync(room);

        var command = new DisableRoom.Command()
        {
            RoomId = room.Id
        };

        // Act
        var action = async () => await SendAsync(command);

        // Assert
        await action.Should().ThrowAsync<ConflictException>()
            .WithMessage("Room have already been disabled.");

        // Cleanup
        Remove(room);
        Remove(await FindAsync<Department>(department.Id));
    }
}