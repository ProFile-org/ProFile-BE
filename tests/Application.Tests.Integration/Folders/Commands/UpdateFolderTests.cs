using Application.Common.Exceptions;
using Application.Folders.Commands;
using Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Folders.Commands;

public class UpdateFolderTests : BaseClassFixture
{
    public UpdateFolderTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }
    
    [Fact]
    public async Task ShouldUpdateFolder_WhenUpdateDetailsAreValid()
    {
        // Arrange
        var department = CreateDepartment();
        var folder = CreateFolder();
        var locker = CreateLocker(folder);
        var room = CreateRoom(department, locker);
        await AddAsync(room);

        var command = new UpdateFolder.Command()
        {
            FolderId = folder.Id,
            Name = "Something else",
            Capacity = 6,
            Description = "ehehe",
        };
        
        // Act
        var result = await SendAsync(command);
        
        // Assert
        result.Id.Should().Be(folder.Id);
        result.Name.Should().Be(command.Name);
        result.Capacity.Should().Be(command.Capacity);
        result.Description.Should().Be(command.Description);
        
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
        var command = new UpdateFolder.Command()
        {
            FolderId = Guid.NewGuid(),
            Name = "Something else",
            Capacity = 6,
            Description = "ehehe",
        };
        
        // Act
        var result = async () => await SendAsync(command);
        
        // Assert
        await result.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Folder does not exist.");
    }
    
    [Fact]
    public async Task ShouldThrowConflictException_WhenNewFolderNameHasAlreadyExistedInThatLocker()
    {
        // Arrange
        var department = CreateDepartment();
        var duplicateNameFolder = CreateFolder();
        var folder = CreateFolder();
        var locker = CreateLocker(duplicateNameFolder, folder);
        var room = CreateRoom(department, locker);
        await AddAsync(room);
        
        var command = new UpdateFolder.Command()
        {
            FolderId = folder.Id,
            Name = duplicateNameFolder.Name,
            Capacity = 6,
            Description = "ehehe",
        };
        
        // Act
        var result = async () => await SendAsync(command);
        
        // Assert
        await result.Should().ThrowAsync<ConflictException>()
            .WithMessage("Folder name already exists.");
        
        // Cleanup
        Remove(folder);
        Remove(duplicateNameFolder);
        Remove(locker);
        Remove(room);
        Remove(await FindAsync<Department>(department.Id));
    }
    
    [Fact]
    public async Task ShouldThrowConflictException_WhenNewCapacityIsLessThanCurrentNumberOfDocuments()
    {
        // Arrange
        var department = CreateDepartment();
        var documents = CreateNDocuments(2);
        var folder = CreateFolder(documents);
        folder.Capacity = 3;
        var locker = CreateLocker(folder);
        var room = CreateRoom(department, locker);
        await AddAsync(room);
        
        var command = new UpdateFolder.Command()
        {
            FolderId = folder.Id,
            Name = "Something else",
            Capacity = 1,
            Description = "ehehe",
        };
        
        // Act
        var result = async () => await SendAsync(command);
        
        // Assert
        await result.Should().ThrowAsync<ConflictException>()
            .WithMessage("New capacity cannot be less than current number of documents.");
        
        // Cleanup
        Remove(documents[0]);
        Remove(documents[1]);
        Remove(folder);
        Remove(locker);
        Remove(room);
        Remove(await FindAsync<Department>(department.Id));
    }
}