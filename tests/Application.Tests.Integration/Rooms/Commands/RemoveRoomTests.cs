using Application.Rooms.Commands.RemoveRoom;
using Bogus;
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
        //Arrange
        var room = CreateRoom();
        await Add(room);

        var command = new RemoveRoomCommand()
        {
            RoomId = room.Id
        };
        //Act
        var result = await SendAsync(command);
        var deletedRoom = await FindAsync<Room>(room.Id);

        //Assert
        result.IsAvailable.Should().BeFalse();
        deletedRoom.Should().BeNull();
    }

    [Fact]
    public async Task ShouldThrowInvalidOperationException_WhenRoomHaveDocuments()
    {
        //Arrange
        var documents = CreateNDocuments(1);
        var folder = CreateFolder(documents);
        var locker = CreateLocker(folder);
        var room = CreateRoom(locker);
        await AddAsync(room);

        var command = new RemoveRoomCommand()
        {
            RoomId = room.Id
        };
        
        //Act
        var action = async () => await SendAsync(command);
        //Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Can not remove room when room still have documents");

    }

    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenRoomDoesNotExist()
    {
        //Arrange
        var command = new RemoveRoomCommand()
        {
            RoomId = Guid.NewGuid()
        };
        //Act
        var action = async () => await SendAsync(command);
        //Assert
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Room does not exist");
    }


    private Document[] CreateNDocuments(int n)
    {
        var list = new List<Document>();
        for (var i = 0; i < n; i++)
        {
            var document = new Document()
            {
                Id = Guid.NewGuid(),
                Title = new Faker().Commerce.ProductName(),
                DocumentType = "Something Department",
            };
            list.Add(document);
        }

        return list.ToArray();
    }
    
    private Folder CreateFolder(params Document[] documents)
    {
        var folder = new Folder()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfDocuments = documents.Length,
            IsAvailable = true
        };

        foreach (var document in documents)
        {
            folder.Documents.Add(document);
        }

        return folder;
    }
    
    private Locker CreateLocker(params Folder[] folders)
    {
        var locker = new Locker()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfFolders = folders.Length,
            IsAvailable = true
        };

        foreach (var folder in folders)
        {
            locker.Folders.Add(folder);
        }
            
        return locker;
    }
    
    private Room CreateRoom(params Locker[] lockers)
    {
        var room = new Room()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.ProductName(),
            Capacity = 3,
            NumberOfLockers = lockers.Length,
            IsAvailable = true
        };

        foreach (var locker in lockers)
        {
            room.Lockers.Add(locker);
        }
            
        return room;
    }

}