using Application.Common.Exceptions;
using Application.Folders.Commands;
using Domain.Entities;
using Domain.Entities.Physical;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Folders.Commands;

public class DisableFolderTests : BaseClassFixture
{
    public DisableFolderTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Fact]
    public async Task ShouldDisableFolder_WhenFolderHaveNoDocument()
    {
        // Arrange
        var department = CreateDepartment();
        var folder = CreateFolder();
        var locker = CreateLocker(folder);
        var room = CreateRoom(department, locker);
        await AddAsync(room);

        var disableFolderCommand = new DisableFolder.Command()
        {
            FolderId = folder.Id
        };
        
        // Act
        var disabledFolder = await SendAsync(disableFolderCommand);

        // Assert
        disabledFolder.IsAvailable.Should().BeFalse();
        
        // Cleanup
        Remove(folder);
        Remove(locker);
        Remove(room);
        Remove(await FindAsync<Department>(department.Id));
    }

    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenFolderDoesNotExist()
    {
        // Arrange
        var disableFolderCommand = new DisableFolder.Command()
        {
            FolderId = Guid.NewGuid()
        };
        
        // Act
        var result = async () => await SendAsync(disableFolderCommand);
        
        // Assert
        await result.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Folder does not exist.");
    }

    [Fact]
    public async Task ShouldThrowInvalidOperationException_WhenFolderIsAlreadyDisabled()
    {
        // Arrange
        var department = CreateDepartment();
        var folder = CreateFolder();
        folder.IsAvailable = false;
        var locker = CreateLocker(folder);
        var room = CreateRoom(department, locker);
        await AddAsync(room);
        var disableFolderCommand = new DisableFolder.Command()
        {
            FolderId = folder.Id
        };
        
        // Act
        var result = async () => await SendAsync(disableFolderCommand);

        // Assert
        await result.Should().ThrowAsync<ConflictException>()
            .WithMessage("Folder has already been disabled.");
        
        // Cleanup
        Remove(folder);
        Remove(locker);
        Remove(room);
        Remove(await FindAsync<Department>(department.Id));
    }
    
    [Fact]
    public async Task ShouldThrowInvalidOperationException_WhenFolderHasDocuments()
    {
        // Arrange
        var department = CreateDepartment();
        var document = CreateNDocuments(1).First();
        var folder = CreateFolder(document);
        var locker = CreateLocker(folder);
        var room = CreateRoom(department, locker);
        await AddAsync(room);
        var disableFolderCommand = new DisableFolder.Command()
        {
            FolderId = folder.Id
        };
        
        // Act
        var result = async () => await SendAsync(disableFolderCommand);

        // Assert
        await result.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Folder cannot be disabled because it contains documents.");
        
        // Cleanup
        Remove(document);
        Remove(folder);
        Remove(locker);
        Remove(room);
        Remove(await FindAsync<Department>(department.Id));
    }
}