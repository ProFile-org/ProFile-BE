using Application.Common.Exceptions;
using Application.Folders.Commands;
using Domain.Entities;
using Domain.Entities.Physical;
using Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Folders.Commands;

public class AddFolderTests : BaseClassFixture
{
    public AddFolderTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Fact]
    public async Task ShouldAddFolder_WhenAddDetailsAreValid()
    {
        // Arrange
        var department = CreateDepartment();
        var locker = CreateLocker();
        var room = CreateRoom(department, locker);
        await AddAsync(room);
        
        var command =  new AddFolder.Command() 
        {
            LockerId = locker.Id,
            Capacity = 1,
            Name = "something"
        };

        // Act
        var folder = await SendAsync(command);
        
        // Assert
        folder.Name.Should().Be(command.Name);
        folder.Description.Should().Be(command.Description);
        folder.Capacity.Should().Be(command.Capacity);
        folder.Locker.Id.Should().Be(locker.Id);
        folder.Locker.NumberOfFolders.Should().Be(locker.NumberOfFolders + 1);
        folder.NumberOfDocuments.Should().Be(0);
        folder.IsAvailable.Should().BeTrue();
        
        // Clean up
        var folderEntity = await FindAsync<Folder>(folder.Id);
        Remove(folderEntity);
        Remove(locker);
        Remove(room);
        Remove(await FindAsync<Department>(department.Id));
    }

    [Fact]
    public async Task ShouldAddFolder_WhenFoldersHasSameNameButInDifferentLockers()
    {
        // Arrange
        var department = CreateDepartment();
        var folder1 = CreateFolder();
        var locker1 = CreateLocker(folder1);
        var locker2 = CreateLocker();
        var room = CreateRoom(department, locker1, locker2);
        await AddAsync(room);
        
        var command = new AddFolder.Command()
        {
            LockerId = locker2.Id,
            Name = folder1.Name,
            Capacity = 3,
        };
        
        // Act
        var folder2 = await SendAsync(command);
        
        // Assert
        folder1.Name.Should().Be(folder2.Name);
        
        // Cleanup
        Remove(folder1);
        Remove(await FindAsync<Folder>(folder2.Id));
        Remove(locker1);
        Remove(locker2);
        Remove(room);
        Remove(await FindAsync<Department>(department.Id));
    }

    [Fact]
    public async Task ShouldThrowConflictException_WhenFolderAlreadyExistsInTheSameLocker()
    {
        // Arrange
        var department = CreateDepartment();
        var folder = CreateFolder();
        var locker = CreateLocker(folder);
        var room = CreateRoom(department, locker);
        await AddAsync(room);

        var command = new AddFolder.Command() 
        {
            Name = folder.Name,
            LockerId = locker.Id,
            Capacity = 3
        };
        
        // Act
        var action = async () => await SendAsync(command);
        
        // Assert
        await action.Should().ThrowAsync<ConflictException>()
            .WithMessage("Folder name already exists.");
        
        // Cleanup
        Remove(folder);
        Remove(locker);
        Remove(room);
        Remove(await FindAsync<Department>(department.Id));
    }

    [Fact]
    public async Task ShouldThrowLimitExceededException_WhenGoingOverCapacity()
    {
        // Arrange
        var department = CreateDepartment();
        var folder1 = CreateFolder();
        var folder2 = CreateFolder();
        var folder3 = CreateFolder();
        var locker = CreateLocker(folder1, folder2, folder3);
        var room = CreateRoom(department, locker);
        await AddAsync(room);

        var command = new AddFolder.Command()
        {
            Name = "something",
            Capacity = 3,
            LockerId = locker.Id,
            Description = "something else",
        };
        
        // Act
        var action = async () => await SendAsync(command);
        
        // Assert
        await action.Should().ThrowAsync<LimitExceededException>()
            .WithMessage("This locker cannot accept more folders.");
        
        // Cleanup
        Remove(folder1);
        Remove(folder2);
        Remove(folder3);
        Remove(locker);
        Remove(room);
        Remove(await FindAsync<Department>(department.Id));
    }

    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenLockerIdNotExists()
    {
        // Arrange
        var command =  new AddFolder.Command()
        {
            LockerId = Guid.NewGuid(),
            Name = "something",
            Capacity = 3
        };
        
        // Act
        var action = async () => await SendAsync(command);
        
        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Locker does not exist.");
    }
}