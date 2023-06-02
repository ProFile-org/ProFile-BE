using Application.Common.Exceptions;
using Application.Folders.Commands;
using Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Folders.Commands;

public class EnableFolderTests : BaseClassFixture
{
    public EnableFolderTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }
    
    [Fact]
    public async Task ShouldEnableFolder_WhenThatFolderExistsAndIsDisabled()
    {
        // Arrange 
        var department = CreateDepartment();
        var folder = CreateFolder();
        var locker = CreateLocker(folder);
        var room = CreateRoom(department, locker);
        folder.IsAvailable = false;
        await AddAsync(room);
       
        var command = new EnableFolder.Command()
        {
            FolderId = folder.Id,
        };
        
        // Act
        var result = await SendAsync(command);
        
        // Assert
        result.IsAvailable.Should().BeTrue();
        
        // Cleanup
        Remove(folder);
        Remove(locker);
        Remove(room);
        Remove(await FindAsync<Department>(department.Id));
    }

    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenThatFolderDoesNotExist()
    {
        // Arrange
        var command = new EnableFolder.Command()
        {
             FolderId = Guid.NewGuid(),
        };
        
        // Act
        var action = async () => await SendAsync(command);
        
        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Folder does not exist.");
    }

    [Fact]
    public async Task ShouldThrowConflictException_WhenFolderIsAlreadyAvailable()
    {
        // Arrange
        var department = CreateDepartment();
        var folder = CreateFolder();
        folder.IsAvailable = true;
        var locker = CreateLocker(folder);
        var room = CreateRoom(department, locker);
        await AddAsync(room);

        var command = new EnableFolder.Command()
        {
            FolderId = folder.Id,
        };

        // Act
        var action = async () => await SendAsync(command);

        // Assert
        await action.Should().ThrowAsync<ConflictException>()
            .WithMessage("Folder has already been enabled.");

        // Cleanup
        Remove(folder);
        Remove(locker);
        Remove(room);
        Remove(await FindAsync<Department>(department.Id));
    }
}