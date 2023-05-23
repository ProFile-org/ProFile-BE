using Application.Common.Exceptions;
using Application.Helpers;
using Application.Lockers.Commands.AddLocker;
using Application.Rooms.Commands.DisableRoom;
using Bogus;
using Domain.Entities;
using Domain.Entities.Physical;
using FluentAssertions;
using NodaTime;
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
        var room = CreateRoom();
        await AddAsync(room);
       
        var disableRoomCommand = new DisableRoomCommand()
        {
            RoomId = room.Id
        };
        
        // Act
        var result = await SendAsync(disableRoomCommand);
        
        // Assert
        result.IsAvailable.Should().BeFalse();
        
        // Cleanup
        Remove(room);
    }

    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenRoomDoesNotExist()
    {
        // Arrange
        var disableRoomCommand = new DisableRoomCommand()
        {
             RoomId = Guid.NewGuid()
        };
        
        // Act
        var action = async () => await SendAsync(disableRoomCommand);
        
        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Room does not exist");
    }

    [Fact]
    public async Task ShouldThrowInvalidOperationException_WhenRoomIsNotEmptyOfDocuments()
    {
        // Arrange
        var documents = CreateNDocuments(1);
        var folder = CreateFolder(documents);
        var locker = CreateLocker(folder);
        var room = CreateRoom(locker);

        await AddAsync(room);
            
        var disableRoomCommand = new DisableRoomCommand()
        {
            RoomId = room.Id
        };
        
        // Act
        var action = async () => await SendAsync(disableRoomCommand);
        
        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Room cannot be disabled because it contains documents");
        
        // Cleanup
        Remove(documents.First());
        Remove(folder);
        Remove(locker);
        Remove(room);
    }

    [Fact]
    public async Task ShouldThrowInvalidOperationException_WhenRoomIsNotAvailable()
    {
        // Arrange
        var room = CreateRoom();
        room.IsAvailable = false;
        await AddAsync(room);

        var command = new DisableRoomCommand()
        {
            RoomId = room.Id
        };

        // Act
        var action = async () => await SendAsync(command);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Room have already been disabled");

        // Cleanup
        Remove(room);
    }
    
    
}