using Application.Rooms.Commands.Remove;
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
        var room = CreateRoom();
        await Add(room);

        var command = new Command()
        {
            RoomId = room.Id
        };
        // Act
        var result = await SendAsync(command);

        // Assert
        var deletedRoom = await FindAsync<Room>(room.Id);
        result.IsAvailable.Should().BeFalse();
        deletedRoom.Should().BeNull();
    }

    [Fact]
    public async Task ShouldThrowInvalidOperationException_WhenRoomHaveDocuments()
    {
        // Arrange
        var documents = CreateNDocuments(1);
        var folder = CreateFolder(documents);
        var locker = CreateLocker(folder);
        var room = CreateRoom(locker);
        await AddAsync(room);

        var command = new Command()
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
    }

    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenRoomDoesNotExist()
    {
        // Arrange
        var command = new Command()
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