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
        //Arrange 
        var folder = new Folder()
        {
            Id = Guid.NewGuid(),
            Name = "Folder",
            NumberOfDocuments = 0,
            Capacity = 20,
            Description = new Faker().Lorem.Sentence(),
            Locker = new Locker()
            {
                Id = Guid.NewGuid(),
                Name = "Locker",
                Capacity = 20,
                NumberOfFolders = 1,
                IsAvailable = true,
                Room = new Room()
                {
                    Id = Guid.NewGuid(),
                    Capacity = 20,
                    Name = "Room",
                    IsAvailable = true,
                    NumberOfLockers = 1
                }
            },
            IsAvailable = true
        };

        await AddAsync(folder);
        var disableRoomCommand = new DisableRoomCommand()
        {
            RoomId = folder.Locker.Room.Id
        };
        //Act
        var result = await SendAsync(disableRoomCommand);
        //Assert
        result.IsAvailable.Should().BeFalse();
        //Cleanup
        var lockerEntity = await FindAsync<Locker>(folder.Locker.Id);
        var roomEntity = await FindAsync<Room>(folder.Locker.Room.Id);

        Remove(folder);
        Remove(lockerEntity);
        Remove(roomEntity);
    }

    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenRoomDoesNotExist()
    {
        //Arrange
        var disableRoomCommand = new DisableRoomCommand()
        {
             RoomId = Guid.NewGuid()
        };
        //Act
        var action = async () => await SendAsync(disableRoomCommand);
        //Assert
        await action.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Room does not exist");
    }

    [Fact]
    public async Task ShouldThrowInvalidOperationException_WhenRoomIsNotEmptyOfDocuments()
    {
        //Arrange
        var document = new Document()
        {
            Id = Guid.NewGuid(),
            Title = "doc something",
            DocumentType = "something something type",
            
            Folder = new Folder()
            {
                Id = Guid.NewGuid(),
                Name = "Folder01",
                NumberOfDocuments = 1,
                Capacity = 20,
                Description = new Faker().Lorem.Sentence(),
                Locker = new Locker()
                {
                    Id = Guid.NewGuid(),
                    Name = "Locker01",
                    Capacity = 20,
                    NumberOfFolders = 1,
                    IsAvailable = true,
                    Room = new Room()
                    {
                        Id = Guid.NewGuid(),
                        Capacity = 20,
                        Name = "Room01",
                        IsAvailable = true,
                        NumberOfLockers = 1
                    }
                },
                IsAvailable = true
            }
        };
        await AddAsync(document);
        var disableRoomCommand = new DisableRoomCommand()
        {
            RoomId = document.Folder.Locker.Room.Id
        };
        //Act
        var action = async () => await SendAsync(disableRoomCommand);
        //Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Room cannot be disabled because it contains documents");
        //Cleanup
        var lockerEntity = await FindAsync<Locker>(document.Folder.Locker.Id);
        var roomEntity = await FindAsync<Room>(document.Folder.Locker.Room.Id);
        var folderEntity = await FindAsync<Folder>(document.Folder.Id);
        
        Remove(document);
        Remove(folderEntity);
        Remove(lockerEntity);
        Remove(roomEntity);
    }
}