using Application.Folders.Commands;
using Application.Lockers.Commands;
using Domain.Entities;
using Domain.Entities.Physical;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Folders.Commands;

public class RemoveFolderTests : BaseClassFixture
{
    public RemoveFolderTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }
    
    [Fact]
    public async Task ShouldRemoveFolder_WhenFolderHasNoDocuments()
    {
        // Arrange
        var department = CreateDepartment();
        var folder = CreateFolder();
        var locker = CreateLocker(folder);
        var room = CreateRoom(department, locker);
        await Add(room);

        var command = new RemoveFolder.Command()
        {
            FolderId = folder.Id,
        };
        
        // Act
        var result = await SendAsync(command);

        // Assert
        result.Id.Should().Be(folder.Id);
        var removedFolder = await FindAsync<Folder>(folder.Id);
        removedFolder.Should().BeNull();
        
        // Cleanup
        Remove(folder);
        Remove(locker);
        Remove(room);
        Remove(await FindAsync<Department>(department.Id));
    }

    [Fact]
    public async Task ShouldThrowConflictException_WhenFolderStillHasDocuments()
    {
        // Arrange
        var department = CreateDepartment();
        var documents = CreateNDocuments(1);
        var folder = CreateFolder(documents);
        var locker = CreateLocker(folder);
        var room = CreateRoom(department, locker);
        await AddAsync(room);

        var command = new RemoveFolder.Command()
        {
            FolderId = folder.Id
        };
        
        // Act
        var action = async () => await SendAsync(command);
        
        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Folder cannot be removed because it contains documents.");
        
        // Cleanup
        Remove(documents.First());
        Remove(folder);
        Remove(locker);
        Remove(room);
        Remove(await FindAsync<Department>(department.Id));
    }

    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenFolderDoesNotExist()
    {
        // Arrange
        var command = new RemoveFolder.Command()
        {
            FolderId = Guid.NewGuid(),
        };
        
        // Act
        var action = async () => await SendAsync(command);
        
        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Folder does not exist.");
    }
}