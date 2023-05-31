using Application.Rooms.Commands;
using Domain.Entities;
using Domain.Entities.Physical;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Rooms.Commands;

public class RemoveRoomTests : BaseClassFixture
{
    public RemoveRoomTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Fact]
    public async Task ShouldRemoveRoom_WhenRoomHasNoDocuments()
    {
        // Arrange
        var department = CreateDepartment();
        var room = CreateRoom(department);
        await Add(room);

        var command = new RemoveRoom.Command()
        {
            RoomId = room.Id
        };
        
        // Act
        await SendAsync(command);

        // Assert
        var deletedRoom = await FindAsync<Room>(room.Id);
        deletedRoom.Should().BeNull();
        
        // Cleanup
        Remove(await FindAsync<Department>(department.Id));
    }

    [Fact]
    public async Task ShouldThrowInvalidOperationException_WhenRoomHaveDocuments()
    {
        // Arrange
        var department = CreateDepartment();
        var documents = CreateNDocuments(1);
        var folder = CreateFolder(documents);
        var locker = CreateLocker(folder);
        var room = CreateRoom(department, locker);
        await AddAsync(room);

        var command = new RemoveRoom.Command()
        {
            RoomId = room.Id
        };
        
        // Act
        var action = async () => await SendAsync(command);
        
        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Room cannot be removed because it contains documents.");
        
        // Cleanup
        Remove(documents.First());
        Remove(folder);
        Remove(locker);
        Remove(room);
        Remove(await FindAsync<Department>(department.Id));
    }

    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenRoomDoesNotExist()
    {
        // Arrange
        var command = new RemoveRoom.Command()
        {
            RoomId = Guid.NewGuid()
        };
        
        // Act
        var action = async () => await SendAsync(command);
        
        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Room does not exist.");
    }
}