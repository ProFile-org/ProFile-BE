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

public class DisableRoomTest : BaseClassFixture
{
    public DisableRoomTest(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }
    
    [Fact]
    public async Task ShouldDisableRoom_WhenRoomHaveNoDocument()
    {
        //Arrange 
        var folder = new Folder()
        {
            Id = new Guid("1A1D4BB8-5303-4DED-BC4A-04A1C16E84FE"),
            Name = "Folder",
            NumberOfDocuments = 0,
            Capacity = 20,
            Description = new Faker().Lorem.Sentence(),
            Locker = new Locker()
            {
                Id = new Guid("81BC4A24-FD35-424E-BAEA-907F0D574F4D"),
                Name = "Locker",
                Capacity = 20,
                NumberOfFolders = 1,
                IsAvailable = true,
                Room = new Room()
                {
                    Id = new Guid("DF666431-C8DD-438C-941A-BD1FA15D67B9"),
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
            RoomId = new Guid("DF666431-C8DD-438C-941A-BD1FA15D67B9")
        };
        //Act
        var result = await SendAsync(disableRoomCommand);
        //Assert
        result.IsAvailable.Should().BeFalse();
        //Cleanup
        var lockerEntity = await FindAsync<Locker>(new Guid("81BC4A24-FD35-424E-BAEA-907F0D574F4D"));
        var roomEntity = await FindAsync<Room>(new Guid("DF666431-C8DD-438C-941A-BD1FA15D67B9"));

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
    public async Task ShouldThrowConfictException_WhenRoomIsNotEmptyOfDocuments()
    {
        //Arrange
        var document = new Document()
        {
            Id = new Guid("50251F22-2F3A-49C1-9259-37CBC6991AAC"),
            Title = "doc something",
            DocumentType = "something something type",
            Importer = new User()
            {
                Id = new Guid("D92411EA-496C-4025-87D9-2535DEF50ED2"),
                Username = new Faker().Internet.UserName(),
                PasswordHash = SecurityUtil.Hash(new Faker().Internet.Password()),
                Department = new Department()
                {
                    Id = new Guid("E7059201-9199-4330-B574-EDE06670DCF0"),
                    Name = "Something Something Department"
                },
                Role = "Admin",
                IsActivated = true,
                IsActive = true,
                FirstName = new Faker().Person.FirstName,
                LastName = new Faker().Person.LastName,
                Created = LocalDateTime.FromDateTime(DateTime.Now),
                Email = new Faker().Internet.Email(),
                Position = new Faker().Name.JobDescriptor(),
            },
            Department = new Department()
            {   
                Id = new Guid("DF51AFF1-CA79-4FA9-9977-ECF1184F8A0B"),
                Name = "Something Department"
            },
            Folder = new Folder()
            {
                Id = new Guid("1A1D4BB8-5303-4DED-BC4A-04A1C16E84FE"),
                Name = "Folder01",
                NumberOfDocuments = 1,
                Capacity = 20,
                Description = new Faker().Lorem.Sentence(),
                Locker = new Locker()
                {
                    Id = new Guid("81BC4A24-FD35-424E-BAEA-907F0D574F4D"),
                    Name = "Locker01",
                    Capacity = 20,
                    NumberOfFolders = 1,
                    IsAvailable = true,
                    Room = new Room()
                    {
                        Id = new Guid("DF666431-C8DD-438C-941A-BD1FA15D67B9"),
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
            RoomId = new Guid("DF666431-C8DD-438C-941A-BD1FA15D67B9")
        };
        //Act
        var action = async () => await SendAsync(disableRoomCommand);
        //Assert
        await action.Should().ThrowAsync<ConflictException>()
            .WithMessage("Room cannot be disabled because it contains documents");
        //Cleanup
        var lockerEntity = await FindAsync<Locker>(new Guid("81BC4A24-FD35-424E-BAEA-907F0D574F4D"));
        var roomEntity = await FindAsync<Room>(new Guid("DF666431-C8DD-438C-941A-BD1FA15D67B9"));
        var folderEntity = await FindAsync<Folder>(new Guid("1A1D4BB8-5303-4DED-BC4A-04A1C16E84FE"));
        var department01Entity = await FindAsync<Department>(new Guid("DF51AFF1-CA79-4FA9-9977-ECF1184F8A0B"));
        var department02Entity = await FindAsync<Department>(new Guid("E7059201-9199-4330-B574-EDE06670DCF0"));
        var userEntity = await FindAsync<User>(new Guid("D92411EA-496C-4025-87D9-2535DEF50ED2"));
        
        Remove(document);
        Remove(folderEntity);
        Remove(lockerEntity);
        Remove(roomEntity);
        Remove(userEntity);
        Remove(department01Entity);
        Remove(department02Entity);
    }
}